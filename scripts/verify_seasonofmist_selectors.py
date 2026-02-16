#!/usr/bin/env python3
"""
Verification script for SeasonOfMist parser selectors.
Tests listing page and detail page extraction against the live site.

Usage: pip install requests beautifulsoup4 lxml && python verify_seasonofmist_selectors.py
"""

import re
import sys

import requests
from bs4 import BeautifulSoup

HEADERS = {
    "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 "
                  "(KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
}

CATEGORY_URLS = [
    "https://shop.season-of-mist.com/music?cat=3",
    "https://shop.season-of-mist.com/music?cat=5",
    "https://shop.season-of-mist.com/music?cat=23",
]

OK = "\033[92mOK\033[0m"
FAIL = "\033[91mFAIL\033[0m"
WARN = "\033[93mWARN\033[0m"


def check(label, value, required=True):
    if value and str(value).strip():
        print(f"  [{OK}] {label}: {str(value).strip()[:120]}")
        return True
    elif required:
        print(f"  [{FAIL}] {label}: NOT FOUND")
        return False
    else:
        print(f"  [{WARN}] {label}: not found (optional)")
        return True


def parse_product_name(raw_title):
    parts = raw_title.split(" - ")
    if len(parts) >= 3:
        band = parts[0].strip()
        album = " - ".join(parts[1:-1]).strip()
        fmt = parts[-1].strip()
        return band, album, fmt
    elif len(parts) == 2:
        return parts[0].strip(), parts[1].strip(), ""
    return "", raw_title.strip(), ""


def verify_listing_page(url):
    print(f"\n{'=' * 70}")
    print(f"LISTING PAGE: {url}")
    print(f"{'=' * 70}")

    response = requests.get(url, headers=HEADERS, timeout=30)
    response.raise_for_status()
    soup = BeautifulSoup(response.text, "lxml")

    all_ok = True

    # Product items: div.products-grid > div.item
    grids = soup.select("div.products-grid")
    product_items = []
    for grid in grids:
        items = grid.select("div.item")
        product_items.extend(items)

    all_ok &= check("Product items (div.products-grid div.item)", f"{len(product_items)} found")

    # Show first 5 products
    detail_url = None
    print(f"\n  First 5 products:")
    seen = set()
    count = 0
    for item in product_items:
        name_link = item.select_one("h2.product-name > a")
        if not name_link:
            continue

        href = name_link.get("href", "").strip()
        if not href or href in seen:
            continue
        seen.add(href)

        raw_title = name_link.get_text(strip=True)
        band, album, fmt = parse_product_name(raw_title)

        if detail_url is None:
            detail_url = href

        print(f"    [{count}] Band: {band} | Album: {album} | Format: {fmt}")
        print(f"         URL: {href[:80]}")
        count += 1
        if count >= 5:
            break

    all_ok &= check("Products with links", f"{count} shown" if count > 0 else "")

    # Pagination: a.next or a[title='Next']
    next_link = soup.select_one("a.next") or soup.select_one("a[title='Next']")
    if next_link:
        check("Next page (a.next / a[title='Next'])", next_link.get("href", ""))
    else:
        print(f"  [{WARN}] Next page: not found (may be last page)")

    return detail_url, all_ok


def verify_detail_page(url):
    print(f"\n{'=' * 70}")
    print(f"DETAIL PAGE: {url}")
    print(f"{'=' * 70}")

    response = requests.get(url, headers=HEADERS, timeout=30)
    response.raise_for_status()
    soup = BeautifulSoup(response.text, "lxml")

    all_ok = True

    # Attributes table
    print(f"\n  Attributes table (#product-attribute-specs-table):")
    attrs_table = soup.select_one("#product-attribute-specs-table")
    if attrs_table:
        rows = attrs_table.select("tr")
        for row in rows:
            th = row.select_one("th")
            td = row.select_one("td")
            if th and td:
                attr_name = th.get_text(strip=True)
                attr_val = td.get_text(strip=True)
                print(f"    {attr_name}: {attr_val}")
    else:
        print(f"  [{FAIL}] Attributes table NOT FOUND")
        all_ok = False

    # Band
    band = get_attr(soup, "Band")
    all_ok &= check("Band", band)

    # Title
    title = get_attr(soup, "Title")
    all_ok &= check("Title", title)

    # Catalog #
    catalog = get_attr(soup, "Catalog #")
    all_ok &= check("Catalog # (SKU)", catalog)

    # Price
    price_el = soup.select_one("span.price")
    price = ""
    if price_el:
        price_text = price_el.get_text(strip=True).replace("\u20ac", "").replace(",", ".").strip()
        price = price_text
    all_ok &= check("Price (span.price)", price)

    # Photo URL - product-img-box or a.product-image
    photo_url = ""
    img_box = soup.select_one("div.product-img-box img")
    if img_box:
        photo_url = img_box.get("src", "")
    if not photo_url:
        img_link = soup.select_one("a.product-image img")
        if img_link:
            photo_url = img_link.get("src", "")
    all_ok &= check("Photo URL", photo_url)

    # Genre
    genre = get_attr(soup, "Detailed musical style")
    if not genre:
        genre = get_attr(soup, "Generic musical style")
    check("Genre (Detailed/Generic musical style)", genre, required=False)

    # Release Date
    release_date = get_attr(soup, "Release Date")
    all_ok &= check("Release Date", release_date)

    # Label
    label = get_attr(soup, "Label")
    check("Label", label, required=False)

    # Media type inference
    media = infer_media(url)
    check("Media type (inferred from URL)", media, required=False)

    # Status (Pre-Order button)
    btn = soup.select_one("button.btn-cart")
    btn_text = btn.get_text(strip=True) if btn else ""
    status = "PreOrder" if "pre-order" in btn_text.lower() or "preorder" in btn_text.lower() else "null"
    check("Status (button text)", f"'{btn_text}' -> {status}", required=False)

    return all_ok


def get_attr(soup, attr_name):
    table = soup.select_one("#product-attribute-specs-table")
    if not table:
        return ""
    for row in table.select("tr"):
        th = row.select_one("th")
        td = row.select_one("td")
        if th and td and th.get_text(strip=True) == attr_name:
            return td.get_text(strip=True)
    return ""


def infer_media(url):
    slug = url.upper()
    if "-LP" in slug or "-VINYL" in slug:
        return "LP"
    if "-CASSETTE" in slug or "-TAPE" in slug:
        return "Tape"
    if "-CD" in slug:
        return "CD"
    return "Unknown"


def main():
    print("SeasonOfMist Selector Verification Script")
    print("=" * 70)

    all_ok = True
    detail_url = None

    # Verify each category listing page
    for category_url in CATEGORY_URLS:
        url, ok = verify_listing_page(category_url)
        all_ok &= ok
        if detail_url is None and url:
            detail_url = url

    if not detail_url:
        print(f"\n[{FAIL}] Could not find any detail URL from listing pages!")
        sys.exit(1)

    # Verify detail page
    all_ok &= verify_detail_page(detail_url)

    # Summary
    print(f"\n{'=' * 70}")
    if all_ok:
        print(f"[{OK}] All selectors verified successfully!")
    else:
        print(f"[{WARN}] Some selectors had issues. Review output above.")

    return 0 if all_ok else 1


if __name__ == "__main__":
    sys.exit(main())

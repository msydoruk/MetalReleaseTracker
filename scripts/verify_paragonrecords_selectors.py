#!/usr/bin/env python3
"""
Verification script for ParagonRecords parser selectors.
Tests listing page and detail page extraction against the live site.

Usage: pip install requests beautifulsoup4 lxml && python verify_paragonrecords_selectors.py
"""

import os
import re
import sys

os.environ.setdefault("PYTHONIOENCODING", "utf-8")

import requests
from bs4 import BeautifulSoup

BASE_URL = "https://www.paragonrecords.org"

HEADERS = {
    "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 "
                  "(KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
}

CATEGORY_URLS = [
    "https://www.paragonrecords.org/collections/cd",
    "https://www.paragonrecords.org/collections/vinyl",
    "https://www.paragonrecords.org/collections/cassette",
]

FORMAT_TOKENS = [
    "CASSETTE", "DIGIPAK", "DIGISLEEVE", "GATEFOLD", "DOUBLE", "DIGI",
    "COLOURED", "COLORED", "DLP", "LP", "DCD", "CD", "EP", "TAPE",
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


def strip_format_suffix(text):
    result = text.rstrip()
    changed = True
    while changed and result:
        changed = False
        for token in FORMAT_TOKENS:
            if result.upper().endswith(f" {token}"):
                result = result[:-(len(token) + 1)].rstrip()
                changed = True
                break
    return result if result else text.rstrip()


def parse_product_name(raw_title):
    parts = raw_title.split(" - ", 1)
    if len(parts) == 2:
        band = parts[0].strip()
        album = strip_format_suffix(parts[1].strip())
        return band, album
    return "", raw_title.strip()


def to_absolute_url(href):
    if href.startswith("http"):
        return href
    return BASE_URL + href


def verify_listing_page(url):
    print(f"\n{'=' * 70}")
    print(f"LISTING PAGE: {url}")
    print(f"{'=' * 70}")

    response = requests.get(url, headers=HEADERS, timeout=30)
    response.raise_for_status()
    soup = BeautifulSoup(response.text, "lxml")

    all_ok = True

    # Product grid
    grid = soup.select_one("div.grid--view-items")
    if not grid:
        print(f"  [{FAIL}] Product grid (div.grid--view-items) NOT FOUND")
        return None, False

    product_items = grid.select("div.grid__item")
    all_ok &= check("Product items (div.grid__item)", f"{len(product_items)} found")

    # Show first 5 products
    detail_url = None
    print(f"\n  First 5 products:")
    seen = set()
    count = 0
    for item in product_items:
        link = item.select_one("a.grid-view-item__link")
        if not link:
            continue

        href = link.get("href", "").strip()
        if not href:
            continue

        abs_url = to_absolute_url(href)
        if abs_url in seen:
            continue
        seen.add(abs_url)

        title_el = item.select_one("div.grid-view-item__title")
        raw_title = title_el.get_text(strip=True) if title_el else ""
        vendor_el = item.select_one("div.grid-view-item__vendor")
        vendor = vendor_el.get_text(strip=True) if vendor_el else ""
        band, album = parse_product_name(raw_title)

        if detail_url is None:
            detail_url = abs_url

        print(f"    [{count}] Band: {band} | Album: {album} | Label: {vendor}")
        print(f"         URL: {abs_url[:80]}")
        count += 1
        if count >= 5:
            break

    all_ok &= check("Products with links", f"{count} shown" if count > 0 else "")

    # Pagination
    next_link = soup.select_one("ul.pagination a")
    # Find the one with icon-arrow-right (Next)
    next_url = None
    for a_tag in soup.select("ul.pagination a"):
        if a_tag.select_one("svg.icon-arrow-right") or a_tag.select_one("span.icon__fallback-text"):
            text = a_tag.get_text(strip=True)
            if "Next" in text:
                next_url = to_absolute_url(a_tag.get("href", ""))
                break

    if next_url:
        check("Next page (ul.pagination a with Next)", next_url)
    else:
        print(f"  [{WARN}] Next page: not found (may be last page)")

    # Page count
    page_text = soup.select_one("li.pagination__text")
    if page_text:
        check("Pagination text", page_text.get_text(strip=True))

    return detail_url, all_ok


def verify_detail_page(url):
    print(f"\n{'=' * 70}")
    print(f"DETAIL PAGE: {url}")
    print(f"{'=' * 70}")

    response = requests.get(url, headers=HEADERS, timeout=30)
    response.raise_for_status()
    soup = BeautifulSoup(response.text, "lxml")
    page_source = response.text

    all_ok = True

    # Title (H1)
    h1 = soup.select_one("h1.product-single__title")
    title_text = h1.get_text(strip=True) if h1 else ""
    all_ok &= check("Title (h1.product-single__title)", title_text)

    # Parse band / album
    band, album = parse_product_name(title_text)
    all_ok &= check("Band (from title)", band)
    all_ok &= check("Album (from title, format stripped)", album)

    # Price from og:price:amount
    og_price = soup.select_one("meta[property='og:price:amount']")
    price = og_price.get("content", "") if og_price else ""
    all_ok &= check("Price (og:price:amount)", price)

    # Photo from og:image:secure_url
    og_image = soup.select_one("meta[property='og:image:secure_url']")
    photo = og_image.get("content", "") if og_image else ""
    all_ok &= check("Photo (og:image:secure_url)", photo)

    # Genre from description
    desc_el = soup.select_one("div.product-single__description")
    genre = desc_el.get_text(strip=True) if desc_el else ""
    check("Genre (product-single__description)", genre, required=False)

    # Label from product JSON in script
    label = ""
    match = re.search(r'"product":\{[^}]*"vendor":"([^"]+)"', page_source)
    if match:
        label = match.group(1)
    all_ok &= check("Label (product JSON vendor)", label)

    # Media type inference from URL
    media = infer_media(url, title_text)
    check("Media type (inferred)", media, required=False)

    # Button text
    btn_span = soup.select_one("#AddToCartText-product-template")
    if not btn_span:
        btn_span = soup.select_one("button.product-form__cart-submit")
    btn_text = btn_span.get_text(strip=True) if btn_span else ""
    check("Button text", btn_text, required=False)

    return all_ok


def infer_media(url, title):
    slug = url.upper()
    if "-LP" in slug or "-VINYL" in slug or "-DLP" in slug:
        return "LP"
    if "-CASSETTE" in slug or "-TAPE" in slug:
        return "Tape"
    if "-CD" in slug or "-DCD" in slug:
        return "CD"

    upper = title.upper()
    if upper.endswith(" LP") or " LP " in upper or " VINYL" in upper or " DLP" in upper:
        return "LP"
    if upper.endswith(" CASSETTE") or " CASSETTE " in upper or " TAPE" in upper:
        return "Tape"
    if upper.endswith(" CD") or " CD " in upper or " DCD" in upper:
        return "CD"

    return "Unknown"


def main():
    print("ParagonRecords Selector Verification Script")
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

    # Verify last page detection
    print(f"\n{'=' * 70}")
    print("PAGINATION TEST: Last page of cassettes")
    print(f"{'=' * 70}")

    response = requests.get(CATEGORY_URLS[2], headers=HEADERS, timeout=30)
    soup = BeautifulSoup(response.text, "lxml")

    page_text = soup.select_one("li.pagination__text")
    if page_text:
        m = re.search(r"Page \d+ of (\d+)", page_text.get_text())
        if m:
            last_page = int(m.group(1))
            print(f"  Cassettes total pages: {last_page}")

            last_page_url = f"{CATEGORY_URLS[2]}?page={last_page}"
            response = requests.get(last_page_url, headers=HEADERS, timeout=30)
            soup = BeautifulSoup(response.text, "lxml")

            next_on_last = None
            for a_tag in soup.select("ul.pagination a"):
                if a_tag.select_one("svg.icon-arrow-right"):
                    next_on_last = a_tag
                    break

            if next_on_last:
                print(f"  [{FAIL}] Next link found on last page (unexpected)")
                all_ok = False
            else:
                print(f"  [{OK}] No next link on last page (correct)")
    else:
        print(f"  [{WARN}] No pagination text found")

    # Summary
    print(f"\n{'=' * 70}")
    if all_ok:
        print(f"[{OK}] All selectors verified successfully!")
    else:
        print(f"[{WARN}] Some selectors had issues. Review output above.")

    return 0 if all_ok else 1


if __name__ == "__main__":
    sys.exit(main())

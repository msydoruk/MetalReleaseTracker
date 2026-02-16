#!/usr/bin/env python3
"""
Verification script for BlackMetalStore parser selectors.
Tests listing page and detail page extraction against the live site.

Usage: pip install requests beautifulsoup4 lxml && python verify_blackmetalstore_selectors.py
"""

import json
import re
import sys

import requests
from bs4 import BeautifulSoup

HEADERS = {
    "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 "
                  "(KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
}

CATEGORY_URLS = [
    "https://blackmetalstore.com/categoria-produto/cds/",
    "https://blackmetalstore.com/categoria-produto/cassettes/",
    "https://blackmetalstore.com/categoria-produto/vinyl/",
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


def verify_listing_page(url):
    print(f"\n{'=' * 70}")
    print(f"LISTING PAGE: {url}")
    print(f"{'=' * 70}")

    response = requests.get(url, headers=HEADERS, timeout=30)
    response.raise_for_status()
    soup = BeautifulSoup(response.text, "lxml")

    all_ok = True

    # Product links
    product_links = soup.select('a[href*="/produto/"]')
    seen = set()
    unique_links = []
    for link in product_links:
        href = link.get("href", "")
        if href and href not in seen:
            seen.add(href)
            unique_links.append(link)

    all_ok &= check("Product links (a[href*='/produto/'])", f"{len(unique_links)} found")

    # Show first 5 products with titles
    detail_url = None
    print("\n  First 5 products:")
    for i, link in enumerate(unique_links[:5]):
        href = link.get("href", "")
        # Find title - walk up to parent div.product, then find h2.product-title
        parent = link
        title_text = ""
        while parent:
            parent = parent.parent
            if parent and parent.name == "div" and "product" in parent.get("class", []):
                h2 = parent.find("h2", class_="product-title")
                if not h2:
                    h2 = parent.find("h2")
                if h2:
                    title_text = h2.get_text(strip=True)
                break

        if detail_url is None and href:
            detail_url = href

        # Try title splitting
        band, album, media = split_title(title_text)
        print(f"    [{i}] {title_text}")
        print(f"         Band: {band} | Album: {album} | Media: {media}")

    # Pagination
    next_page = soup.select_one("a.next.page-numbers")
    if next_page:
        check("Next page (a.next.page-numbers)", next_page.get("href", ""))
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

    # 1. Title (h1)
    h1 = soup.select_one("h1")
    title_text = h1.get_text(strip=True) if h1 else ""
    all_ok &= check("Title (h1)", title_text)

    band, album, media_raw = split_title(title_text)
    print(f"    -> Band: {band}")
    print(f"    -> Album: {album}")
    print(f"    -> Media raw: {media_raw}")

    # 2. SKU
    sku_el = soup.select_one("span.sku")
    sku = sku_el.get_text(strip=True) if sku_el else ""
    all_ok &= check("SKU (span.sku)", sku)

    # 3. Price from HTML
    price_bdi = soup.select_one("p.price bdi")
    price_text = price_bdi.get_text(strip=True) if price_bdi else ""
    all_ok &= check("Price (p.price bdi)", price_text)

    # 4. Image
    img = soup.select_one("img.wp-post-image")
    img_src = img.get("src", "") if img else ""
    all_ok &= check("Image (img.wp-post-image)", img_src)

    # 5. Description
    desc_el = soup.select_one("div.woocommerce-product-details__short-description")
    desc = desc_el.get_text(strip=True)[:100] if desc_el else ""
    check("Description (short-description)", desc, required=False)

    # 6. Label (categories - 2nd link in posted_in)
    posted_in_links = soup.select("span.posted_in a")
    print(f"\n  Categories (span.posted_in a): {len(posted_in_links)} found")
    format_cats = {"cds", "cassettes", "vinyl"}
    label = ""
    for i, cat in enumerate(posted_in_links):
        cat_text = cat.get_text(strip=True)
        is_format = cat_text.lower() in format_cats
        marker = " <- format" if is_format else " <- LABEL" if not label and not is_format else ""
        if not is_format and not label:
            label = cat_text
        print(f"    [{i}] {cat_text}{marker}")

    check("Label (non-format category)", label, required=False)

    # 7. Genre (brand)
    brand_el = soup.select_one("div.product-brand a")
    genre = brand_el.get_text(strip=True) if brand_el else ""
    check("Genre (div.product-brand a)", genre, required=False)

    # 8. JSON-LD
    print(f"\n  JSON-LD extraction:")
    scripts = soup.select('script[type="application/ld+json"]')
    json_ld_found = False

    for script in scripts:
        try:
            data = json.loads(script.string)
            product = None

            if data.get("@type") == "Product":
                product = data
            elif "@graph" in data:
                for item in data["@graph"]:
                    if item.get("@type") == "Product":
                        product = item
                        break

            if product:
                json_ld_found = True
                check("JSON-LD @type=Product", "FOUND")
                check("JSON-LD name", product.get("name"))
                check("JSON-LD sku", product.get("sku"))
                check("JSON-LD image", str(product.get("image", ""))[:80])

                offers = product.get("offers", {})
                if isinstance(offers, list):
                    offers = offers[0] if offers else {}

                # Price can be direct or inside priceSpecification
                price = offers.get("price")
                currency = offers.get("priceCurrency")
                if not price and "priceSpecification" in offers:
                    spec = offers["priceSpecification"]
                    if isinstance(spec, list):
                        spec = spec[0] if spec else {}
                    price = spec.get("price")
                    currency = currency or spec.get("priceCurrency")

                check("JSON-LD price", price)
                check("JSON-LD priceCurrency", currency)
                check("JSON-LD offers.availability", offers.get("availability"), required=False)

                # Brand -> genre
                brand = product.get("brand", [])
                if isinstance(brand, list) and brand:
                    brand_name = brand[0].get("name", "")
                elif isinstance(brand, dict):
                    brand_name = brand.get("name", "")
                else:
                    brand_name = ""
                check("JSON-LD brand (genre)", brand_name, required=False)
                break
        except (json.JSONDecodeError, TypeError):
            continue

    if not json_ld_found:
        print(f"  [{FAIL}] JSON-LD Product: NOT FOUND")
        all_ok = False

    return all_ok


def split_title(title):
    if not title:
        return ("", "", "")

    media_raw = ""
    media_match = re.search(r'\(([^)]+)\)\s*$', title)
    if media_match:
        media_raw = media_match.group(1).strip()
        title = title[:media_match.start()].strip()

    for sep in [' \u2013 ', ' \u2014 ', ' - ']:
        parts = title.split(sep, 1)
        if len(parts) == 2:
            return (parts[0].strip(), parts[1].strip(), media_raw)

    return (title, title, media_raw)


def main():
    print("BlackMetalStore Selector Verification Script")
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

    response = requests.get(
        "https://blackmetalstore.com/categoria-produto/cassettes/",
        headers=HEADERS, timeout=30
    )
    soup = BeautifulSoup(response.text, "lxml")
    page_numbers = soup.select("ul.page-numbers a.page-numbers:not(.next):not(.prev)")
    last_page_num = 1
    for pn in page_numbers:
        try:
            num = int(pn.get_text(strip=True))
            last_page_num = max(last_page_num, num)
        except ValueError:
            pass

    print(f"  Cassettes total pages: {last_page_num}")
    last_page_url = f"https://blackmetalstore.com/categoria-produto/cassettes/page/{last_page_num}/"
    response = requests.get(last_page_url, headers=HEADERS, timeout=30)
    soup = BeautifulSoup(response.text, "lxml")
    next_on_last = soup.select_one("a.next.page-numbers")
    if next_on_last:
        print(f"  [{FAIL}] Next page link found on last page (unexpected)")
        all_ok = False
    else:
        print(f"  [{OK}] No next page link on last page (correct)")

    # Summary
    print(f"\n{'=' * 70}")
    if all_ok:
        print(f"[{OK}] All selectors verified successfully!")
    else:
        print(f"[{WARN}] Some selectors had issues. Review output above.")

    return 0 if all_ok else 1


if __name__ == "__main__":
    sys.exit(main())

#!/usr/bin/env python3
"""
Verification script for NapalmRecords parser selectors.
Tests listing page and detail page extraction against the live site.

Usage: pip install requests beautifulsoup4 lxml && python verify_napalmrecords_selectors.py
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
    "https://napalmrecords.com/english/music/cds?product_list_dir=desc&product_list_order=release_date",
    "https://napalmrecords.com/english/music/lps?product_list_dir=desc&product_list_order=release_date",
    "https://napalmrecords.com/english/music/tapes?product_list_dir=desc&product_list_order=release_date",
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

    # Product items
    product_items = soup.select("li.product-item")
    all_ok &= check("Product items (li.product-item)", f"{len(product_items)} found")

    # Show first 5 products with band names and titles
    detail_url = None
    print("\n  First 5 products:")
    seen = set()
    count = 0
    for item in product_items:
        link = item.select_one("a.product-item-link")
        if not link:
            continue

        href = link.get("href", "").strip()
        if not href or href in seen:
            continue
        seen.add(href)

        album_title = link.get_text(strip=True)
        band_name_el = item.select_one("div.custom-band-name")
        band_name = band_name_el.get_text(strip=True) if band_name_el else ""

        if detail_url is None:
            detail_url = href

        print(f"    [{count}] Band: {band_name} | Album: {album_title}")
        print(f"         URL: {href[:80]}")
        count += 1
        if count >= 5:
            break

    all_ok &= check("Products with links", f"{count} shown" if count > 0 else "")

    # Band names
    band_names = soup.select("div.custom-band-name")
    all_ok &= check("Band names (div.custom-band-name)", f"{len(band_names)} found")

    # Pagination
    next_page = soup.select_one("a.action.next")
    if next_page:
        check("Next page (a.action.next)", next_page.get("href", ""))
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
    page_source = response.text

    all_ok = True

    # 1. Album name (h1.page-title)
    h1 = soup.select_one("h1.page-title")
    if not h1:
        h1 = soup.select_one("h1")
    album_name = h1.get_text(strip=True) if h1 else ""
    all_ok &= check("Album name (h1.page-title)", album_name)

    # 2. Attributes table
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

    # 3. Band name from table
    band_td = soup.select_one("#product-attribute-specs-table td[data-th='Band']")
    band = band_td.get_text(strip=True) if band_td else ""
    all_ok &= check("Band (td[data-th='Band'])", band)

    # 4. Genre from table
    genre_td = soup.select_one("#product-attribute-specs-table td[data-th='Genre']")
    genre = genre_td.get_text(strip=True) if genre_td else ""
    check("Genre (td[data-th='Genre'])", genre, required=False)

    # 5. Release date from table
    date_td = soup.select_one("#product-attribute-specs-table td[data-th='Release Date']")
    release_date = date_td.get_text(strip=True) if date_td else ""
    all_ok &= check("Release Date (td[data-th='Release Date'])", release_date)

    # 6. SKU - Art. Nr.
    sku = ""
    strong_tags = soup.select("strong")
    for strong in strong_tags:
        text = strong.get_text(strip=True)
        if "Art" in text and "Nr" in text:
            parent_text = strong.parent.get_text(strip=True) if strong.parent else ""
            match = re.search(r'(\d+)', parent_text)
            if match:
                sku = match.group(1)
                break

    if not sku:
        form = soup.select_one("form[data-product-sku]")
        if form:
            sku = form.get("data-product-sku", "")

    all_ok &= check("SKU (Art. Nr. or data-product-sku)", sku)

    # 7. Price from JS
    js_match = re.search(r'"final_price"[:\s]*"?([\d.]+)"?', page_source)
    if js_match:
        all_ok &= check("Price (JS final_price)", js_match.group(1))
    else:
        price_wrapper = soup.select_one("span.price-wrapper[data-price-amount]")
        if price_wrapper:
            all_ok &= check("Price (data-price-amount)", price_wrapper.get("data-price-amount"))
        else:
            print(f"  [{FAIL}] Price: NOT FOUND (neither JS nor HTML)")
            all_ok = False

    # 8. Photo URL (og:image)
    og_image = soup.select_one("meta[property='og:image']")
    photo_url = og_image.get("content", "") if og_image else ""
    all_ok &= check("Photo URL (og:image)", photo_url)

    # 9. Description
    desc_el = soup.select_one("div.description div.value")
    desc = desc_el.get_text(strip=True)[:100] if desc_el else ""
    check("Description (div.description div.value)", desc, required=False)

    # 10. Media type inference
    media = infer_media(url, album_name)
    check("Media type (inferred)", media, required=False)

    return all_ok


def infer_media(url, album_name):
    combined = f"{url} {album_name}".upper()
    if "/LPS" in combined or "- LP" in combined or "VINYL" in combined:
        return "LP"
    if "/TAPES" in combined or "TAPE" in combined or "CASSETTE" in combined:
        return "Tape"
    if "/CDS" in combined or "- CD" in combined or "DIGIPAK" in combined:
        return "CD"
    return "Unknown"


def main():
    print("NapalmRecords Selector Verification Script")
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
    print("PAGINATION TEST: Last page of tapes")
    print(f"{'=' * 70}")

    response = requests.get(
        CATEGORY_URLS[2],
        headers=HEADERS, timeout=30
    )
    soup = BeautifulSoup(response.text, "lxml")

    # Find last page number from pagination
    pages = soup.select("ul.items.pages-items a.page")
    last_page_num = 1
    for page in pages:
        try:
            num = int(page.get_text(strip=True))
            last_page_num = max(last_page_num, num)
        except ValueError:
            pass

    print(f"  Tapes total pages: {last_page_num}")

    if last_page_num > 1:
        last_page_url = CATEGORY_URLS[2] + f"&p={last_page_num}"
        response = requests.get(last_page_url, headers=HEADERS, timeout=30)
        soup = BeautifulSoup(response.text, "lxml")
        next_on_last = soup.select_one("a.action.next")
        if next_on_last:
            print(f"  [{FAIL}] Next page link found on last page (unexpected)")
            all_ok = False
        else:
            print(f"  [{OK}] No next page link on last page (correct)")
    else:
        print(f"  [{OK}] Only 1 page, no pagination to test")

    # Summary
    print(f"\n{'=' * 70}")
    if all_ok:
        print(f"[{OK}] All selectors verified successfully!")
    else:
        print(f"[{WARN}] Some selectors had issues. Review output above.")

    return 0 if all_ok else 1


if __name__ == "__main__":
    sys.exit(main())

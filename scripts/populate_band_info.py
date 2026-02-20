"""
Populate band descriptions, photos, and genres from Metal Archives.

Reads band names from CoreDataService DB, matches them to MetalArchivesId
from ParserService DB, fetches band info + photo from Metal Archives
(via FlareSolverr to bypass Cloudflare), uploads photo to MinIO,
and updates CoreDataService DB.

Dependencies: pip install psycopg2-binary minio requests beautifulsoup4
"""

import os
import time
import random
import logging
from io import BytesIO

import requests
import psycopg2
from bs4 import BeautifulSoup
from minio import Minio

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(message)s",
)
logger = logging.getLogger(__name__)

# --- Configuration (overridable via env vars) ---

CORE_DB_HOST = os.getenv("CORE_DB_HOST", "localhost")
CORE_DB_PORT = int(os.getenv("CORE_DB_PORT", "5436"))
CORE_DB_NAME = os.getenv("CORE_DB_NAME", "core_data_service_db")
CORE_DB_USER = os.getenv("CORE_DB_USER", "coredata_user")
CORE_DB_PASS = os.getenv("CORE_DB_PASS", "CoreDataP@ssw0rd123")

PARSER_DB_HOST = os.getenv("PARSER_DB_HOST", "localhost")
PARSER_DB_PORT = int(os.getenv("PARSER_DB_PORT", "5434"))
PARSER_DB_NAME = os.getenv("PARSER_DB_NAME", "ParserServiceDb")
PARSER_DB_USER = os.getenv("PARSER_DB_USER", "parser_admin")
PARSER_DB_PASS = os.getenv("PARSER_DB_PASS", "P@rserDbP@ss123!")

MINIO_ENDPOINT = os.getenv("MINIO_ENDPOINT", "localhost:9001")
MINIO_ACCESS_KEY = os.getenv("MINIO_ACCESS_KEY", "admin")
MINIO_SECRET_KEY = os.getenv("MINIO_SECRET_KEY", "M1n10S3cur3P@ss!")
MINIO_BUCKET = os.getenv("MINIO_BUCKET", "metal-release-tracker")
MINIO_SECURE = os.getenv("MINIO_SECURE", "false").lower() == "true"

FLARESOLVERR_URL = os.getenv("FLARESOLVERR_URL", "http://localhost:8191/v1")

REQUEST_DELAY_MIN = float(os.getenv("REQUEST_DELAY_MIN", "2"))
REQUEST_DELAY_MAX = float(os.getenv("REQUEST_DELAY_MAX", "3"))


def connect_core_db():
    return psycopg2.connect(
        host=CORE_DB_HOST,
        port=CORE_DB_PORT,
        dbname=CORE_DB_NAME,
        user=CORE_DB_USER,
        password=CORE_DB_PASS,
    )


def connect_parser_db():
    return psycopg2.connect(
        host=PARSER_DB_HOST,
        port=PARSER_DB_PORT,
        dbname=PARSER_DB_NAME,
        user=PARSER_DB_USER,
        password=PARSER_DB_PASS,
    )


def get_minio_client():
    return Minio(
        MINIO_ENDPOINT,
        access_key=MINIO_ACCESS_KEY,
        secret_key=MINIO_SECRET_KEY,
        secure=MINIO_SECURE,
    )


def fetch_core_bands(core_conn):
    with core_conn.cursor() as cursor:
        cursor.execute('SELECT "Id", "Name" FROM "Bands"')
        return cursor.fetchall()


def fetch_band_references(parser_conn):
    with parser_conn.cursor() as cursor:
        cursor.execute('SELECT "BandName", "MetalArchivesId", "Genre" FROM "BandReferences"')
        return cursor.fetchall()


def build_reference_lookup(references):
    lookup = {}
    for band_name, ma_id, genre in references:
        lookup[band_name.strip().lower()] = {
            "ma_id": ma_id,
            "genre": genre,
        }
    return lookup


def fetch_via_flaresolverr(url):
    payload = {
        "cmd": "request.get",
        "url": url,
        "maxTimeout": 60000,
    }
    response = requests.post(FLARESOLVERR_URL, json=payload, timeout=90)
    data = response.json()

    if data.get("status") != "ok":
        raise RuntimeError(f"FlareSolverr error: {data.get('message', 'unknown')}")

    solution = data.get("solution", {})
    if solution.get("status") != 200:
        raise RuntimeError(f"FlareSolverr got HTTP {solution.get('status')} for {url}")

    return solution.get("response", "")


def fetch_band_page(ma_id):
    url = f"https://www.metal-archives.com/bands/_/{ma_id}"
    return fetch_via_flaresolverr(url)


def parse_band_info(html):
    soup = BeautifulSoup(html, "html.parser")

    photo_url = None
    photo_link = soup.find("a", id="photo")
    if photo_link:
        img = photo_link.find("img")
        if img and img.get("src"):
            photo_url = img["src"]

    stats = {}
    stats_div = soup.find("div", id="band_stats")
    if stats_div:
        for dt in stats_div.find_all("dt"):
            key = dt.get_text(strip=True).rstrip(":")
            dd = dt.find_next_sibling("dd")
            if dd:
                stats[key] = dd.get_text(strip=True)

    parts = []
    if stats.get("Country of origin"):
        parts.append(stats["Country of origin"])
    if stats.get("Status"):
        parts.append(f"Status: {stats['Status']}")
    if stats.get("Formed in"):
        parts.append(f"Formed: {stats['Formed in']}")
    if stats.get("Themes"):
        parts.append(f"Themes: {stats['Themes']}")

    description = " | ".join(parts) if parts else None

    return photo_url, description


def download_photo(photo_url):
    return fetch_via_flaresolverr(photo_url)


def download_photo_direct(photo_url, cookies):
    response = requests.get(photo_url, cookies=cookies, timeout=30)
    response.raise_for_status()
    return response.content


def upload_to_minio(minio_client, band_id, photo_data):
    object_name = f"images/bands/{band_id}.jpg"
    minio_client.put_object(
        MINIO_BUCKET,
        object_name,
        BytesIO(photo_data),
        length=len(photo_data),
        content_type="image/jpeg",
    )
    return object_name


def update_band(core_conn, band_id, description, photo_path, genre):
    with core_conn.cursor() as cursor:
        cursor.execute(
            'UPDATE "Bands" SET "Description" = %s, "PhotoUrl" = %s, "Genre" = %s WHERE "Id" = %s',
            (description, photo_path, genre, str(band_id)),
        )
    core_conn.commit()


def main():
    logger.info("Connecting to databases and MinIO...")
    core_conn = connect_core_db()
    parser_conn = connect_parser_db()
    minio_client = get_minio_client()

    try:
        bands = fetch_core_bands(core_conn)
        logger.info("Found %d bands in CoreDataService DB", len(bands))

        references = fetch_band_references(parser_conn)
        logger.info("Found %d band references in ParserService DB", len(references))

        lookup = build_reference_lookup(references)

        updated = 0
        skipped = 0
        failed = 0

        for band_id, band_name in bands:
            ref = lookup.get(band_name.strip().lower())
            if not ref:
                logger.warning("No Metal Archives match for: %s — skipping", band_name)
                skipped += 1
                continue

            ma_id = ref["ma_id"]
            genre = ref["genre"]

            try:
                logger.info("Fetching MA page for %s (MA ID: %d)...", band_name, ma_id)
                html = fetch_band_page(ma_id)
                photo_url, description = parse_band_info(html)

                photo_path = None
                if photo_url:
                    logger.info("  Downloading photo from %s...", photo_url)
                    try:
                        photo_data = download_photo_direct(photo_url, {})
                    except Exception:
                        logger.info("  Direct download failed, trying via FlareSolverr...")
                        raw = fetch_via_flaresolverr(photo_url)
                        photo_data = raw.encode("latin-1") if isinstance(raw, str) else raw

                    photo_path = upload_to_minio(minio_client, band_id, photo_data)
                    logger.info("  Uploaded to MinIO: %s", photo_path)

                update_band(core_conn, band_id, description, photo_path, genre)
                logger.info("  Updated band: %s", band_name)
                updated += 1

            except Exception as exc:
                logger.error("  Failed for %s: %s", band_name, exc)
                failed += 1

            delay = random.uniform(REQUEST_DELAY_MIN, REQUEST_DELAY_MAX)
            time.sleep(delay)

        logger.info("Done. Updated: %d, Skipped: %d, Failed: %d", updated, skipped, failed)

    finally:
        core_conn.close()
        parser_conn.close()


if __name__ == "__main__":
    main()

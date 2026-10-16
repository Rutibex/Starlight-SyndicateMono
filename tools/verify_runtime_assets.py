#!/usr/bin/env python3
"""Verify essential shipped data without rebuilding it or changing the working tree.

Requires Python 3 standard library only. Use after clone and before publishing.
"""
from __future__ import annotations

import gzip
import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT / 'Assets'
ERRORS: list[str] = []


def check(condition: bool, problem: str) -> None:
    if not condition:
        ERRORS.append(problem)


def load(relative: str):
    path = ASSETS / relative
    try:
        return json.loads(path.read_text(encoding='utf-8'))
    except (OSError, json.JSONDecodeError) as exc:
        ERRORS.append(f'{relative}: {exc}')
        return None


# Mandatory catalogs explicitly loaded by gameplay, plus major subsystem payloads.
mandatory = [
    'Fleet.json', 'Factions.json', 'Weapons.json', 'Drones.json',
    'Defense.json', 'Support.json', 'Rigs.json', 'Utility.json',
    'Industry37.json', 'Skills36.json', 'Missions38.json',
    'SystemsQ/catalog.json', 'SystemsQ/planets.json',
    'Sde40/index.json', 'Sde40/attributes.json',
    'Retheme41/registry.json', 'Galaxy42/galaxy.json',
    'Stations44/stations.json', 'Stations44/agents.json',
    'ShipModelManifest.json'
]
for name in mandatory:
    check((ASSETS / name).is_file(), 'Required data missing: Assets/' + name)

for name in ['SpaceSky.png', 'FontAtlas.png', 'UiFont.png']:
    path = ASSETS / name
    check(path.is_file(), 'Required texture missing: Assets/' + name)
    if path.is_file():
        with path.open('rb') as f:
            check(f.read(8) == b'\x89PNG\r\n\x1a\n', f'Invalid PNG signature: {name}')

spatial = sorted((ASSETS / 'Spatial43').glob('*.json.gz'))
check(len(spatial) >= 8000, 'Expected the complete spatial data shard set (8,000+ files)')
if spatial:
    try:
        with gzip.open(spatial[0], 'rt', encoding='utf-8') as f:
            json.load(f)
    except (OSError, EOFError, json.JSONDecodeError) as exc:
        ERRORS.append(f'Corrupt first spatial shard: {exc}')

models = sorted((ASSETS / 'FleetModels').glob('*.gltf'))
check(len(models) >= 1000, 'Runtime ship LOD models incomplete (expected at least 1,000)')
check(len(list((ASSETS / 'ShipThumbnails').glob('*.png'))) >= 350, 'Ship thumbnails incomplete')
check(len(list((ASSETS / 'Sde40').glob('*.json'))) >= 100, 'SDE 40 item data shards incomplete')

manifest = load('ShipModelManifest.json') if (ASSETS / 'ShipModelManifest.json').is_file() else None
if isinstance(manifest, dict):
    entries = manifest.get('Entries', [])
    check(isinstance(entries, list) and bool(entries), 'Empty ship model manifest')
    checked = 0
    for entry in entries:
        for lod in entry.get('Lods', []):
            rel = lod.get('File', '')
            p = (ASSETS / rel).resolve()
            check(p.is_relative_to((ASSETS / 'FleetModels').resolve()) and p.is_file(),
                  f'Missing/unsafe model LOD: {rel}')
            checked += 1
    check(checked > 1000, 'Model manifest references too few LODs')

for image in (ROOT / 'docs/images').glob('*.png'):
    check(image.stat().st_size > 1000, 'Documentation screenshot is too small: ' + image.name)

print(f'Runtime data: {len(spatial)} spatial shards, {len(models)} ship models, '
      f'{len(list((ASSETS / "ShipThumbnails").glob("*.png")))} thumbnails')
if ERRORS:
    for issue in ERRORS:
        print('ERROR:', issue, file=sys.stderr)
    raise SystemExit(f'Asset verification failed: {len(ERRORS)} issue(s).')
print('Runtime asset presence and manifest references verified.')

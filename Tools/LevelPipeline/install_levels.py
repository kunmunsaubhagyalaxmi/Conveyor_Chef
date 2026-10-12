"""Install the levels built by build_levels.py into the Unity project.

    python install_levels.py

* copies build/<continent>/<country>/*.asset to
  Assets/Project Data/Content/LevelSystem/World Levels/<continent>/<country>/
* keeps the Unity GUID of a level that is already installed at the same path (so rebuilding
  never breaks references); new levels and folders get new GUIDs
* removes .asset files in World Levels that are not part of the build
* rewrites the levels list of "Levels Database.asset" in game order (manifest.json)

Close nothing in Unity; it re-imports the changed files when its window gets focus.
"""
import json
import os
import re
import shutil
import sys
import uuid

HERE = os.path.dirname(os.path.abspath(__file__))
PROJECT = os.path.normpath(os.path.join(HERE, '..', '..'))
ASSETS = os.path.join(PROJECT, 'Assets')
LEVEL_SYSTEM = os.path.join(ASSETS, 'Project Data', 'Content', 'LevelSystem')
TARGET = os.path.join(LEVEL_SYSTEM, 'World Levels')
DATABASE = os.path.join(LEVEL_SYSTEM, 'Levels Database.asset')
BUILD = os.path.join(HERE, 'build')

ASSET_META = ('fileFormatVersion: 2\nguid: %s\nNativeFormatImporter:\n  externalObjects: {}\n'
              '  mainObjectFileID: 11400000\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n')
FOLDER_META = ('fileFormatVersion: 2\nguid: %s\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n'
               '  userData: \n  assetBundleName: \n  assetBundleVariant: \n')


def all_guids():
    used = set()
    for d, _, files in os.walk(ASSETS):
        for f in files:
            if f.endswith('.meta'):
                m = re.search(r'^guid: (\w+)', open(os.path.join(d, f), encoding='utf-8', errors='ignore').read(), re.M)
                if m:
                    used.add(m.group(1))
    return used


def main():
    manifest = json.load(open(os.path.join(BUILD, 'manifest.json')))
    bad = [m['n'] for m in manifest if m.get('failed')]
    if bad:
        sys.exit('build has failed levels %s - rebuild them before installing' % bad)
    used = all_guids()

    def new_guid():
        g = uuid.uuid4().hex
        while g in used:
            g = uuid.uuid4().hex
        used.add(g)
        return g

    def ensure_folder(path):
        if not os.path.isdir(path):
            os.makedirs(path)
        parent_meta = path.rstrip('/\\') + '.meta'
        if not os.path.exists(parent_meta):
            open(parent_meta, 'w', encoding='utf-8', newline='\n').write(FOLDER_META % new_guid())

    ensure_folder(TARGET)
    keep, order = set(), []
    for m in manifest:
        rel = m['path']
        src = os.path.join(BUILD, rel)
        dst = os.path.join(TARGET, rel)
        folder = os.path.dirname(dst)
        # every folder level needs a .meta
        parts = os.path.relpath(folder, TARGET).split(os.sep)
        for i in range(1, len(parts) + 1):
            ensure_folder(os.path.join(TARGET, *parts[:i]))
        shutil.copyfile(src, dst)
        meta = dst + '.meta'
        if os.path.exists(meta):
            guid = re.search(r'^guid: (\w+)', open(meta, encoding='utf-8').read(), re.M).group(1)
        else:
            guid = new_guid()
            open(meta, 'w', encoding='utf-8', newline='\n').write(ASSET_META % guid)
        keep.add(os.path.normcase(os.path.abspath(dst)))
        order.append(guid)

    removed = 0
    for d, _, files in os.walk(TARGET):
        for f in files:
            if f.endswith('.asset'):
                p = os.path.join(d, f)
                if os.path.normcase(os.path.abspath(p)) not in keep:
                    os.remove(p)
                    if os.path.exists(p + '.meta'):
                        os.remove(p + '.meta')
                    removed += 1

    db = open(DATABASE, encoding='utf-8', newline='').read()
    nl = '\r\n' if '\r\n' in db else '\n'
    block = re.search(r'^  levels:\r?\n((?:  - .*\r?\n)+)', db, re.M)
    new_block = ''.join('  - {fileID: 11400000, guid: %s, type: 2}' % g + nl for g in order)
    open(DATABASE, 'w', encoding='utf-8', newline='').write(db[:block.start(1)] + new_block + db[block.end(1):])
    print('installed %d levels into %s (removed %d old files); database now lists %d levels'
          % (len(order), os.path.relpath(TARGET, PROJECT), removed, len(order)))


if __name__ == '__main__':
    main()

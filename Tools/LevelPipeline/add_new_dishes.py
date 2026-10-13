"""Brings the newer dish colours (orange 17, brown 18, white 19) into the existing levels.

Every dish colour plays the same, so swapping which colours a level uses (e.g. red -> orange,
teal -> white everywhere in that level) keeps its layout, customers and difficulty exactly as
they are; only which dishes appear changes. Each level gets a fixed (seeded) random choice of
colours from the 10, so across a country's 30 levels all 10 of its dishes show up.

Only colours whose picture exists for that country are used:
  Assets/Project Data/Game/Resources/World/<continent>/<country>/dish_orange.png (brown, white)
so it is safe to run while the pictures are still being made; run it again when more arrive.
Levels that already use a newer colour are left alone (use --force to choose again).

usage:
  python add_new_dishes.py            # dry run: shows what would change
  python add_new_dishes.py --apply    # writes the level files
  python add_new_dishes.py --apply --force
"""
import glob
import os
import random
import re
import struct
import sys
from collections import Counter

PR = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
LEVELS = os.path.join(PR, 'Assets', 'Project Data', 'Content', 'LevelSystem', 'World Levels')
WORLD = os.path.join(PR, 'Assets', 'Project Data', 'Game', 'Resources', 'World')

BASE = list(range(10, 17))                 # red green pink blue yellow teal purple
NEW = {17: 'orange', 18: 'brown', 19: 'white'}
ALL = BASE + list(NEW)

TYPE_LINE = re.compile(r'^(\s*- elementType: )(\d+)\s*$', re.M)
HEX_FIELD = re.compile(r'^(\s*(?:busSpawnQueue|spawnQueue): )([0-9a-f]*)\s*$', re.M)


def decode(hexstr):
    data = bytes.fromhex(hexstr)
    return [struct.unpack('<i', data[i:i + 4])[0] for i in range(0, len(data), 4)]


def encode(values):
    return b''.join(struct.pack('<i', v) for v in values).hex()


def is_dish(t):
    return 10 <= t <= 19


def available_new(continent, country):
    folder = os.path.join(WORLD, continent, country)
    return [t for t, name in NEW.items() if os.path.exists(os.path.join(folder, 'dish_%s.png' % name))]


def used_colours(text):
    used = Counter()
    for m in TYPE_LINE.finditer(text):
        t = int(m.group(2))
        if is_dish(t):
            used[t] += 1
    for m in HEX_FIELD.finditer(text):
        for t in decode(m.group(2)):
            if is_dish(t):
                used[t] += 1
    return used


def recolour(text, mapping):
    text = TYPE_LINE.sub(lambda m: m.group(1) + str(mapping.get(int(m.group(2)), int(m.group(2)))), text)
    text = HEX_FIELD.sub(lambda m: m.group(1) + encode([mapping.get(v, v) for v in decode(m.group(2))]), text)
    return text


def main():
    apply = '--apply' in sys.argv
    force = '--force' in sys.argv
    changed = skipped = no_art = 0
    shown = Counter()
    for path in sorted(glob.glob(os.path.join(LEVELS, '*', '*', '*.asset'))):
        continent, country = path.replace('\\', '/').split('/')[-3:-1]
        extra = available_new(continent, country)
        if not extra:
            no_art += 1
            continue
        text = open(path, encoding='utf-8').read()
        used = used_colours(text)
        if not used:
            continue
        if any(t in NEW for t in used) and not force:
            skipped += 1
            continue

        # back to the original 7 first when choosing again
        palette = BASE + extra
        rng = random.Random(os.path.basename(path))
        current = sorted(used)
        targets = rng.sample(palette, len(current))
        mapping = {c: t for c, t in zip(current, targets)}
        new_text = recolour(text, mapping)

        # the level must keep exactly the same shape: same count of every (renamed) colour
        before = sorted(used.values())
        after = sorted(used_colours(new_text).values())
        assert before == after, path
        for t in targets:
            shown[t] += 1
        if apply and new_text != text:
            with open(path, 'w', encoding='utf-8', newline='\n') as f:
                f.write(new_text)
        changed += 1

    print('%s %d levels; %d already use the new colours (skipped); %d in countries without new pictures yet'
          % ('Recoloured' if apply else 'Would recolour', changed, skipped, no_art))
    if shown:
        names = dict(zip(BASE, ['red', 'green', 'pink', 'blue', 'yellow', 'teal', 'purple']))
        names.update(NEW)
        print('levels using each colour:', ', '.join('%s %d' % (names[t], shown[t]) for t in ALL))
    if not apply:
        print('dry run: nothing written (add --apply)')


if __name__ == '__main__':
    main()

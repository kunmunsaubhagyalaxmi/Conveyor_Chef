"""Conveyor Chef level pipeline: builds every level of the world, 30 per country.

    python build_levels.py                  # build all 1050 levels into ./build
    python build_levels.py --only china     # rebuild one country (by id) into ./build
    python build_levels.py --plan           # print the difficulty plan, build nothing
    python install_levels.py                # copy ./build into the Unity project + database

Layout: 7 continents x 5 countries x 30 levels (WorldCatalog.cs). Level n of the game is
country (n-1) // 30, level (n-1) % 30 + 1 of that country.

Every level is checked with the game's real rules (levelgen.py):
  * the perfect-play solver must win it with at least 2 dock slots to spare;
  * a "casual player" bot must win it about as often as the difficulty plan says.

Output: build/<continent>/<country>/<Country>_NN.asset, plus build/manifest.json (game order).
"""
import json
import multiprocessing as mp
import os
import random
import re
import shutil
import sys
import time

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import levelgen as gen  # noqa: E402

PROJECT = os.path.normpath(os.path.join(HERE, '..', '..'))
CATALOG = os.path.join(PROJECT, 'Assets', 'Project Data', 'Game', 'Scripts', 'World', 'WorldCatalog.cs')
HANDMADE = os.path.join(PROJECT, 'Assets', 'Project Data', 'Content', 'LevelSystem', 'Levels')
BUILD = os.path.join(HERE, 'build')

LEVELS_PER_COUNTRY = 30
RUNS = 200


# ------------------------------------------------------------------ world layout
def read_catalog():
    src = open(CATALOG, encoding='utf-8').read()
    continents = re.findall(r'new Continent\((\d+), "(\w+)", "([^"]+)"', src)
    countries = re.findall(r'new Country\((\d+), "(\w+)", "([^"]+)"', src)
    cont = {int(i): (cid, name) for i, cid, name in continents}
    out = []
    for i, cid, name in countries:
        i = int(i)
        out.append(dict(index=i, id=cid, name=name, continent=cont[i // 5][0], continent_name=cont[i // 5][1]))
    return out


def file_name(country, k):
    return '%s_%02d' % (country['name'].replace(' ', '').replace("'", ''), k + 1)


# ------------------------------------------------------------------ difficulty plan
def target(n, total):
    """Casual-bot win rate wanted for global level n (1-based). Lower = harder."""
    c, k = divmod(n - 1, LEVELS_PER_COUNTRY)
    countries = total // LEVELS_PER_COUNTRY
    if n <= 5:
        return 100.0                                   # tutorial levels
    base = 92.0 - 60.0 * (c / max(1, countries - 1)) ** 0.8   # each country harder than the last
    t = base - 14.0 * k / (LEVELS_PER_COUNTRY - 1)        # and harder towards its end
    if k % 10 == 9:
        t -= 28.0                                      # boss: levels 10, 20, 30 of a country
    elif k % 5 == 4:
        t -= 16.0                                      # hard: levels 5, 15, 25
    elif k % 5 == 0 and k > 0:
        t += 8.0                                       # breather after a hard level
    if c == 0 and k < 12:
        t = max(t, 74.0)                               # gentle start in China
    return max(6.0, min(98.0, t))


def size_plan(n, total, rng):
    c, k = divmod(n - 1, LEVELS_PER_COUNTRY)
    progress = (n - 1) / max(1, total - 1)
    hard = k % 10 == 9 or k % 5 == 4
    sets = 4 + 11 * progress ** 0.7 + 2.0 * k / (LEVELS_PER_COUNTRY - 1) + (1.5 if hard else 0) + rng.uniform(-1, 1)
    sets = max(3, min(16, int(round(sets))))
    colours = max(3, min(7, int(round(3.2 + 4.0 * progress ** 0.6 + rng.uniform(-0.5, 0.5)))))
    colours = min(colours, sets)
    hidden = 0.0 if n < 15 else min(0.22, 0.04 + 0.2 * progress) * rng.choice((0, 0.5, 1, 1))
    spawners = 0 if n < 8 else rng.choice((0, 0, 1, 1, 2)) if k % 4 != 0 else 0
    return dict(target=target(n, total), sets=sets, colours=colours, hidden=hidden, spawners=spawners)


# ------------------------------------------------------------------ hand-made levels
def handmade_spec(path):
    t = open(path, encoding='utf-8').read()

    def hexints(h):
        return [int.from_bytes(bytes.fromhex(h[i:i + 8]), 'little') for i in range(0, len(h), 8)]

    w = int(re.search(r'^  width: (\d+)', t, re.M).group(1))
    h = int(re.search(r'^  height: (\d+)', t, re.M).group(1))
    cell, hidden = {}, set()
    for a, x, y, s in re.findall(r'- elementType: (\d+)\n\s+elementPosition:\n\s+x: (\d+)\n\s+y: (\d+)\n\s+specialEffectType: (\d+)', t):
        cell[(int(x), int(y))] = int(a)
        if int(s) == 1 and 10 <= int(a) <= 16:
            hidden.add((int(x), int(y)))
    spawners = {}
    sp = re.search(r'^  spawnerData:(.*?)(?=^  tutorialSteps)', t, re.S | re.M)
    if sp:
        for s in re.split(r'\n  - elementPosition:', sp.group(1))[1:]:
            pos = re.search(r'x: (\d+)\s+y: (\d+)', s)
            key = (int(pos.group(1)), int(pos.group(2)))
            if key not in spawners:
                q = re.search(r'spawnQueue: ?([0-9a-f]*)', s)
                spawners[key] = (int(re.search(r'direction: (\d+)', s).group(1)), hexints(q.group(1)) if q else [])
    bus = hexints(re.search(r'^  busSpawnQueue: ?([0-9a-f]*)$', t, re.M).group(1))
    return dict(w=w, h=h, cell=cell, hidden=hidden, spawners=spawners, bus=bus)


def measure_handmade():
    cache = os.path.join(HERE, 'handmade_rates.json')
    if not os.path.exists(cache):
        rng = random.Random(11)
        rates = {}
        for i in range(1, 51):
            p = os.path.join(HANDMADE, 'Level_%03d.asset' % i)
            rates[i] = gen.casual(handmade_spec(p), rng, 240)
        json.dump(rates, open(cache, 'w'), indent=1)
    return {int(k): v for k, v in json.load(open(cache)).items()}


def assign_handmade(total, rates):
    """Levels 1-5 open the game; other hand-made boards go where their difficulty fits
    (only in the first 10 countries: they are small boards)."""
    assign = {n: n for n in range(1, 6)}
    free = [n for n in range(6, min(total, 300) + 1)]
    pool = [i for i in range(6, 51) if rates[i] >= 5]

    def rate(i):
        return rates[i]

    for i in sorted(pool, key=lambda i: min(abs(target(n, total) - rate(i)) for n in free)):
        best = min(free, key=lambda n: abs(target(n, total) - rate(i)))
        if abs(target(best, total) - rate(i)) <= 6:
            assign[best] = i
            free.remove(best)
    return assign


def drop_duplicate_spawners(yaml_text):
    """Some hand-made levels kept a stale second spawner entry for the same cell (the game
    only reads the first one). Remove the extra entries."""
    m = re.search(r'^  spawnerData:\n(.*?)(?=^  tutorialSteps)', yaml_text, re.S | re.M)
    if not m:
        return yaml_text
    entries = re.split(r'(?=^  - elementPosition:)', m.group(1), flags=re.M)
    seen, kept = set(), []
    for e in entries:
        pos = re.search(r'x: (\d+)\s+y: (\d+)', e)
        if pos:
            key = (pos.group(1), pos.group(2))
            if key in seen:
                continue
            seen.add(key)
        kept.append(e)
    return yaml_text[:m.start(1)] + ''.join(kept) + yaml_text[m.end(1):]


# ------------------------------------------------------------------ building one level
def build_one(job):
    n, total, seed = job
    rng = random.Random(seed)
    tgt = target(n, total)
    last = None
    for attempt in range(6):
        def plan(_n, t=tgt):
            p = size_plan(n, total, rng)
            p['target'] = t
            return p
        gen.plan = plan
        spec = gen.generate(n, random.Random(rng.random()))
        if spec is None:
            continue
        # fair: perfect play keeps 2 dock slots free, and the bot wins at least sometimes
        if spec['peak'] <= 5 and spec['rate'] >= max(3.0, tgt - 14):
            return n, spec
        if last is None or (spec['peak'] <= 5 and abs(spec['rate'] - tgt) < abs(last['rate'] - tgt)):
            last = spec
        if tgt < 15:
            tgt += 4                                   # a finale that came out brutal: a touch easier
    return n, (last if last is not None and last['peak'] <= 5 else None)


def main():
    args = sys.argv[1:]
    countries = read_catalog()
    total = len(countries) * LEVELS_PER_COUNTRY
    only = args[args.index('--only') + 1] if '--only' in args else None

    if '--plan' in args:
        for c in countries:
            ts = [target(c['index'] * 30 + k + 1, total) for k in range(30)]
            print('%-14s %-12s levels %4d-%4d  easiest %3.0f%%  average %3.0f%%  hardest %3.0f%%' % (
                c['continent_name'], c['name'], c['index'] * 30 + 1, c['index'] * 30 + 30, max(ts), sum(ts) / 30, min(ts)))
        return

    rates = measure_handmade()
    assign = assign_handmade(total, rates)
    if only is None and os.path.exists(BUILD):
        shutil.rmtree(BUILD)
    os.makedirs(BUILD, exist_ok=True)

    jobs, manifest = [], []
    for c in countries:
        if only and c['id'] != only:
            continue
        folder = os.path.join(BUILD, c['continent'], c['id'])
        os.makedirs(folder, exist_ok=True)
        for k in range(LEVELS_PER_COUNTRY):
            n = c['index'] * LEVELS_PER_COUNTRY + k + 1
            name = file_name(c, k)
            path = os.path.join(folder, name + '.asset')
            manifest.append(dict(n=n, name=name, path=os.path.relpath(path, BUILD).replace('\\', '/'),
                                 handmade=assign.get(n), target=round(target(n, total), 1)))
            if n in assign:
                src = open(os.path.join(HANDMADE, 'Level_%03d.asset' % assign[n]), encoding='utf-8').read()
                src = re.sub(r'm_Name: .*', 'm_Name: ' + name, src, count=1)
                open(path, 'w', encoding='utf-8', newline='\n').write(drop_duplicate_spawners(src))
            else:
                jobs.append((n, total, 20261011 * 7 + n))

    by_n = {m['n']: m for m in manifest}
    start = time.time()
    done = 0
    with mp.Pool(max(1, os.cpu_count() - 2)) as pool:
        for n, spec in pool.imap_unordered(build_one, jobs, chunksize=1):
            m = by_n[n]
            done += 1
            if spec is None:
                print('FAILED level %d' % n, flush=True)
                m['failed'] = True
                continue
            with open(os.path.join(BUILD, m['path']), 'w', encoding='utf-8', newline='\n') as f:
                f.write(gen.to_yaml(spec, m['name']))
            m.update(rate=round(spec['rate'], 1), dock=spec['peak'], customers=len(spec['bus']))
            if done % 25 == 0 or done == len(jobs):
                print('%4d/%d built  (%.0f s)' % (done, len(jobs), time.time() - start), flush=True)

    manifest.sort(key=lambda m: m['n'])
    name = 'manifest.json' if only is None else 'manifest_%s.json' % only
    json.dump(manifest, open(os.path.join(BUILD, name), 'w'), indent=1)
    failed = [m['n'] for m in manifest if m.get('failed')]
    print('done: %d levels, %d hand-made, %d failed %s' % (len(manifest), sum(1 for m in manifest if m['handmade']),
                                                        len(failed), failed))


if __name__ == '__main__':
    main()

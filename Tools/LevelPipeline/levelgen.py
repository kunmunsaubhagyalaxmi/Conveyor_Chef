"""Conveyor Chef level generator.

Builds new LevelData assets (same YAML format as the hand-made levels), each one
checked with the game's real rules before it is saved:
  * a perfect-play solver must find a win;
  * a "casual player" bot (serves the current customer, then the waiting one, else taps
    a reachable dish) must win a target share of 200 games, so difficulty follows a plan.

usage: python gen.py <first level> <last level> <out dir> [--seed N] [--dry]
writes <out dir>/Level_###.asset (+ .meta) and prints a summary line per level.
"""
import json
import math
import os
import random
import sys
import uuid

DOCK, SEATS = 7, 3
COLOURS = [10, 11, 12, 13, 14, 15, 16, 17, 18, 19]   # red green pink blue yellow teal purple orange brown white
MAX_SETS_PER_COLOUR = 2                        # a colour fills at most 2 customers per level (variety)
OFF = {0: (0, -1), 1: (1, 0), 2: (0, 1), 3: (-1, 0)}   # Forward Right Back Left
N4 = ((1, 0), (-1, 0), (0, 1), (0, -1))


# ---------------------------------------------------------------- simulation (game rules)
class Game:
    def __init__(self, spec):
        self.w, self.h = spec['w'], spec['h']
        self.cell = dict(spec['cell'])               # (x,y) -> 0 empty, 1 wall, 10-16 dish, 100 spawner
        self.hidden = set(spec['hidden'])
        self.spawners = {p: [((p[0] + OFF[d][0]), (p[1] + OFF[d][1])), list(q)] for p, (d, q) in spec['spawners'].items()}
        self.bus = spec['bus']
        self.bi = self.seats = self.peak = 0
        self.dock = {}
        self.spawn()

    def copy(self):
        g = Game.__new__(Game)
        g.w, g.h, g.bus, g.bi, g.seats, g.peak = self.w, self.h, self.bus, self.bi, self.seats, self.peak
        g.cell, g.hidden, g.dock = dict(self.cell), set(self.hidden), dict(self.dock)
        g.spawners = {k: [v[0], list(v[1])] for k, v in self.spawners.items()}
        return g

    def empty(self, p):
        return self.cell.get(p, 1) == 0

    def spawn(self):
        moved = True
        while moved:
            moved = False
            for target, q in self.spawners.values():
                if q and self.empty(target):
                    self.cell[target] = q.pop()          # the game spawns from the end of the queue
                    moved = True

    def pickable(self):
        reach = {(x, 0) for x in range(self.w) if self.empty((x, 0))}
        stack = list(reach)
        while stack:
            x, y = stack.pop()
            for dx, dy in N4:
                n = (x + dx, y + dy)
                if n not in reach and 0 <= n[0] < self.w and 0 <= n[1] < self.h and self.empty(n):
                    reach.add(n)
                    stack.append(n)
        return [p for p, a in self.cell.items() if 10 <= a <= 19 and
                (p[1] == 0 or any((p[0] + dx, p[1] + dy) in reach for dx, dy in N4))]

    def collecting(self):
        return self.bus[self.bi] if self.bi < len(self.bus) else None

    def waiting(self):
        return self.bus[self.bi + 1] if self.bi + 1 < len(self.bus) else None

    def tap(self, p):
        c = self.cell[p]
        self.cell[p] = 0
        self.hidden.discard(p)
        self.spawn()
        self.dock[c] = self.dock.get(c, 0) + 1
        while self.bi < len(self.bus):
            b = self.bus[self.bi]
            take = min(self.dock.get(b, 0), SEATS - self.seats)
            self.dock[b] = self.dock.get(b, 0) - take
            self.seats += take
            if self.seats == SEATS:
                self.bi += 1
                self.seats = 0
                continue
            break
        used = sum(self.dock.values())
        self.peak = max(self.peak, used)
        if self.bi >= len(self.bus):
            return 'win'
        return 'lose' if used >= DOCK else None

    def key(self):
        return (tuple(sorted(self.cell.items())), tuple(sorted((k, v) for k, v in self.dock.items() if v)),
                self.bi, self.seats)


def solve(spec, limit=150000):
    best, seen, nodes = [None], {}, [0]

    def dfs(g):
        nodes[0] += 1
        if nodes[0] > limit or (best[0] is not None and best[0] <= 1):
            return
        k = g.key()
        if seen.get(k, 99) <= g.peak:
            return
        seen[k] = g.peak
        if best[0] is not None and g.peak >= best[0]:
            return
        cb, wb = g.collecting(), g.waiting()
        for p in sorted(g.pickable(), key=lambda p: (g.cell[p] != cb, g.cell[p] != wb)):
            n = g.copy()
            res = n.tap(p)
            if res == 'win':
                if best[0] is None or n.peak < best[0]:
                    best[0] = n.peak
            elif res is None:
                dfs(n)

    dfs(Game(spec))
    return best[0]


def casual(spec, rng, runs):
    start = Game(spec)
    wins = 0
    for _ in range(runs):
        g = start.copy()
        while True:
            moves = g.pickable()
            if not moves:
                break
            cb, wb = g.collecting(), g.waiting()
            vis = [p for p in moves if p not in g.hidden]
            pick = [p for p in vis if g.cell[p] == cb] or [p for p in vis if g.cell[p] == wb]
            if not pick:
                stack = [p for p in vis if g.dock.get(g.cell[p], 0) > 0]
                pick = stack if stack and rng.random() < 0.6 else moves
            res = g.tap(rng.choice(pick))
            if res:
                wins += res == 'win'
                break
    return 100.0 * wins / runs


# ---------------------------------------------------------------- level construction
def plan(n):
    """Target bot win rate and size for global level number n (1-based)."""
    continent = (n - 1) // 15          # 3 Europe, 4 Africa, 5 Oceania, 6 Antarctica
    p = (n - 1) % 15
    slot = p % 3                       # 1st / 2nd / 3rd level of a country
    base = {3: 72, 4: 64, 5: 56, 6: 48}.get(continent, 70)
    target = base - 2.5 * (p // 3) + (14, 0, -16)[slot]
    target = max(12.0, min(95.0, target))
    progress = (n - 46) / 59.0          # 0 at the start of Europe, 1 at the end
    sets = int(round(8 + 7 * progress + (-1, 0, 1)[slot] + random.uniform(-1, 1)))
    sets = max(6, min(16, sets))
    colours = max(4, min(7, int(round(4.5 + 2.5 * progress + random.uniform(-0.6, 0.6)))))
    colours = min(colours, sets)
    hidden = 0.0 if slot == 0 and p < 3 else min(0.22, 0.05 + 0.2 * progress) * random.choice((0, 0.5, 1, 1))
    spawners = random.choice((0, 0, 1, 1, 2)) if n % 4 != 1 else 0
    return dict(target=target, sets=sets, colours=colours, hidden=hidden, spawners=spawners)


def layout(cells_needed, rng):
    """Symmetric walls; every open cell connected to the front row. Returns (w, h, open cells)."""
    for _ in range(400):
        w = rng.choice((5, 6, 7, 7, 8, 9)) if cells_needed > 24 else rng.choice((4, 5, 5, 6, 7))
        h = max(3, min(10, math.ceil((cells_needed + w + rng.randint(0, w * 2)) / w)))
        wall_p = rng.choice((0.0, 0.08, 0.14, 0.2, 0.26))
        cell = {}
        half = (w + 1) // 2
        for y in range(h):
            for x in range(half):
                is_wall = y > 0 and rng.random() < wall_p
                if y == 0 and (x == 0) and rng.random() < 0.3:
                    is_wall = True                    # corner walls like the hand-made levels
                cell[(x, y)] = cell[(w - 1 - x, y)] = 1 if is_wall else 0
        # connectivity through non-wall cells from the front row
        seen = {(x, 0) for x in range(w) if cell[(x, 0)] == 0}
        stack = list(seen)
        while stack:
            x, y = stack.pop()
            for dx, dy in N4:
                q = (x + dx, y + dy)
                if q in cell and q not in seen and cell[q] == 0:
                    seen.add(q)
                    stack.append(q)
        for p in cell:
            if cell[p] == 0 and p not in seen:
                cell[p] = 1                           # unreachable pockets become walls
        open_cells = [p for p in cell if cell[p] == 0]
        if cells_needed <= len(open_cells) - 1 <= cells_needed + w + 2 and h <= 10:
            return w, h, cell
    return None


def depth_map(w, h, cell):
    """Steps from the front row through non-wall cells (how deep a dish sits)."""
    d = {p: 0 for p in cell if p[1] == 0 and cell[p] != 1}
    frontier = list(d)
    while frontier:
        nxt = []
        for x, y in frontier:
            for dx, dy in N4:
                q = (x + dx, y + dy)
                if q in cell and cell[q] != 1 and q not in d:
                    d[q] = d[(x, y)] + 1
                    nxt.append(q)
        frontier = nxt
    return d


def build(n, pl, mess, rng):
    total = pl['sets'] * 3
    spawn_n = pl['spawners']
    spawn_queue_len = [rng.randint(3, 5) for _ in range(spawn_n)]
    grid_dishes = total - sum(spawn_queue_len)
    lay = layout(grid_dishes + spawn_n, rng)
    if lay is None:
        return None
    w, h, cell = lay
    dmap = depth_map(w, h, cell)
    open_cells = sorted((p for p in cell if cell[p] == 0), key=lambda p: (dmap[p], p[1], abs(p[0] - (w - 1) / 2)))
    # keep the front-most cells empty as the walking space, fill the rest
    spare = len(open_cells) - (grid_dishes + spawn_n)
    keep_empty = [p for p in open_cells if p[1] == 0][:max(1, spare)]
    keep_empty += [p for p in open_cells if p not in keep_empty][:max(0, spare - len(keep_empty))]
    fill = [p for p in open_cells if p not in keep_empty]
    if len(fill) != grid_dishes + spawn_n or not any(p[1] == 0 for p in keep_empty):
        return None

    # spawners: deep cells, facing an adjacent filled cell
    spawners = {}
    for i in range(spawn_n):
        cands = [p for p in fill if p not in spawners and dmap[p] >= 2 and
                 any((p[0] + OFF[d][0], p[1] + OFF[d][1]) in fill and (p[0] + OFF[d][0], p[1] + OFF[d][1]) not in spawners
                     for d in OFF)]
        if not cands:
            return None
        p = rng.choice(cands)
        dirs = [d for d in OFF if (p[0] + OFF[d][0], p[1] + OFF[d][1]) in fill and
                (p[0] + OFF[d][0], p[1] + OFF[d][1]) not in spawners]
        spawners[p] = [rng.choice(dirs), spawn_queue_len[i]]
    dish_cells = [p for p in fill if p not in spawners]

    # every dish slot with a "when does it surface" key: depth (+ queue position for spawners)
    slots = [(dmap[p], ('cell', p)) for p in dish_cells]
    for p, (d, ln) in spawners.items():
        for k in range(ln):
            slots.append((dmap[p] + 1 + k, ('spawn', p, k)))
    keyed = sorted(slots, key=lambda s: s[0] + rng.gauss(0, mess))

    # colours per set, then the customer queue in surfacing order of each set
    # every colour once, then some colours a second time - never more than MAX_SETS_PER_COLOUR
    palette = rng.sample(COLOURS, pl['colours'])
    extra = pl['sets'] - len(palette)
    if extra > len(palette) * (MAX_SETS_PER_COLOUR - 1):
        return None
    sets = palette + rng.sample(palette * (MAX_SETS_PER_COLOUR - 1), extra)
    rng.shuffle(sets)
    assign = {}
    for i, (_, slot) in enumerate(keyed):
        assign[slot] = sets[i // 3]
    bus = list(sets)
    # a little extra disorder in the queue on harder levels
    for _ in range(int(mess * 1.5)):
        i = rng.randrange(len(bus) - 1)
        bus[i], bus[i + 1] = bus[i + 1], bus[i]

    final = {p: (1 if v == 1 else 0) for p, v in cell.items()}
    for p in dish_cells:
        final[p] = assign[('cell', p)]
    spawner_spec = {}
    for p, (d, ln) in spawners.items():
        final[p] = 100
        # spawned from the end: the first dish out is queue[-1]
        spawner_spec[p] = (d, [assign[('spawn', p, k)] for k in reversed(range(ln))])
    hidden = set()
    hid_pool = [p for p in dish_cells if p[1] > 0 and dmap[p] >= 2]
    rng.shuffle(hid_pool)
    hidden = set(hid_pool[:int(len(dish_cells) * pl['hidden'])])
    return dict(n=n, w=w, h=h, cell=final, hidden=hidden, spawners=spawner_spec, bus=bus)


def generate(n, rng, runs=200):
    random.seed(rng.random())
    pl = plan(n)
    lo, hi = pl['target'] - 12, pl['target'] + 12
    mess = 0.8 + (100 - pl['target']) / 22.0
    best = None
    for attempt in range(120):
        spec = build(n, pl, mess, rng)
        if spec is None:
            continue
        rate = casual(spec, rng, 60)               # quick screen
        if not (lo - 15 <= rate <= hi + 15):
            mess = max(0.2, mess + (0.5 if rate > hi else -0.5))
            continue
        rate = casual(spec, rng, runs)
        if lo <= rate <= hi:
            peak = solve(spec)
            if peak is not None:
                spec.update(rate=rate, peak=peak, target=pl['target'], attempts=attempt + 1)
                return spec
        else:
            mess = max(0.2, mess + (0.35 if rate > hi else -0.35))
            if rate < lo and pl['hidden'] > 0:
                pl['hidden'] *= 0.8              # too hard: fewer hidden dishes
        if best is None or abs(rate - pl['target']) < abs(best['rate'] - pl['target']):
            peak = solve(spec)
            if peak is not None:
                spec.update(rate=rate, peak=peak, target=pl['target'], attempts=attempt + 1)
                best = spec
        if attempt % 30 == 29:                    # widen the band if the plan is too strict
            lo, hi = lo - 5, hi + 5
    return best


# ---------------------------------------------------------------- writing assets
def hexq(values):
    return ''.join(int(v).to_bytes(4, 'little').hex() for v in values)


def to_yaml(spec, name):
    w, h = spec['w'], spec['h']
    if w >= h:
        order = [(x, y) for y in range(h) for x in range(w)]
    else:
        order = [(x, y) for x in range(w) for y in range(h)]
    out = ['%YAML 1.1', '%TAG !u! tag:unity3d.com,2011:', '--- !u!114 &11400000', 'MonoBehaviour:',
           '  m_ObjectHideFlags: 0', '  m_CorrespondingSourceObject: {fileID: 0}', '  m_PrefabInstance: {fileID: 0}',
           '  m_PrefabAsset: {fileID: 0}', '  m_GameObject: {fileID: 0}', '  m_Enabled: 1', '  m_EditorHideFlags: 0',
           '  m_Script: {fileID: 11500000, guid: 86d931e0e510c5941b73798608b73d81, type: 3}',
           '  m_Name: ' + name, '  m_EditorClassIdentifier: ', '  elementsData:']
    for p in order:
        out += ['  - elementType: %d' % spec['cell'][p], '    elementPosition:', '      x: %d' % p[0],
                '      y: %d' % p[1], '    specialEffectType: %d' % (1 if p in spec['hidden'] else 0), '    temp: 0']
    if spec['spawners']:
        out.append('  spawnerData:')
        for p, (d, q) in sorted(spec['spawners'].items()):
            out += ['  - elementPosition:', '      x: %d' % p[0], '      y: %d' % p[1],
                    '    direction: %d' % d, '    spawnQueue: ' + hexq(q)]
    else:
        out.append('  spawnerData: []')
    note = str(len(spec['bus']))
    if spec['spawners']:
        note += '  SPA x%d' % len(spec['spawners'])
    if spec['hidden']:
        note += '  CRA x%d' % len(spec['hidden'])
    out += ['  tutorialSteps: []', '  busSpawnQueue: ' + hexq(spec['bus']), '  width: %d' % w, '  height: %d' % h,
            '  coinsReward: 20', '  useInRandomizer: 1', '  note: ' + note]
    return '\n'.join(out) + '\n'


def meta(guid):
    return ('fileFormatVersion: 2\nguid: %s\nNativeFormatImporter:\n  externalObjects: {}\n'
            '  mainObjectFileID: 11400000\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n' % guid)


if __name__ == '__main__':
    first, last, out_dir = int(sys.argv[1]), int(sys.argv[2]), sys.argv[3]
    seed = int(sys.argv[sys.argv.index('--seed') + 1]) if '--seed' in sys.argv else 2026
    dry = '--dry' in sys.argv
    os.makedirs(out_dir, exist_ok=True)
    summary = []
    for n in range(first, last + 1):
        rng = random.Random(seed * 1000 + n)
        spec = generate(n, rng)
        if spec is None:
            print('Level %d: FAILED' % n, flush=True)
            continue
        name = 'Level_%03d' % n
        guid = uuid.uuid4().hex
        if not dry:
            with open(os.path.join(out_dir, name + '.asset'), 'w', encoding='utf-8', newline='\n') as f:
                f.write(to_yaml(spec, name))
            with open(os.path.join(out_dir, name + '.asset.meta'), 'w', encoding='utf-8', newline='\n') as f:
                f.write(meta(guid))
        cols = len(set(spec['bus']))
        print('Level %3d  %dx%d  dishes %2d  customers %2d  colours %d  walls %2d  hidden %2d  spawners %d  '
              'target %3.0f%%  casual %3.0f%%  best dock %d/7  (try %d)' % (
                  n, spec['w'], spec['h'], len(spec['bus']) * 3, len(spec['bus']), cols,
                  sum(1 for v in spec['cell'].values() if v == 1), len(spec['hidden']), len(spec['spawners']),
                  spec['target'], spec['rate'], spec['peak'], spec['attempts']), flush=True)
        summary.append(dict(n=n, guid=guid, w=spec['w'], h=spec['h'], rate=spec['rate'], target=spec['target'],
                            cell={'%d,%d' % k: v for k, v in spec['cell'].items()},
                            hidden=['%d,%d' % p for p in spec['hidden']],
                            spawners={'%d,%d' % k: v for k, v in spec['spawners'].items()}, bus=spec['bus']))
    with open(os.path.join(out_dir, 'summary.json'), 'w') as f:
        json.dump(summary, f)

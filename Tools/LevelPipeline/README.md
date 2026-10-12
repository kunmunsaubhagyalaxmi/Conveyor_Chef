# Conveyor Chef level pipeline

Builds every level of the game: 7 continents x 5 countries x **30 levels** = 1050 levels,
each one checked with the game's real rules before it is installed.

## Run it

```
cd "Tools/LevelPipeline"
python build_levels.py          # builds all 1050 levels into ./build (about 5 minutes)
python install_levels.py        # copies them into Unity and rewrites the level database
```

Then switch to Unity and let it import.

Other options:

```
python build_levels.py --plan           # print the difficulty plan per country, build nothing
python build_levels.py --only china     # rebuild one country (country id from WorldCatalog.cs)
```

Needs only Python 3 (standard library, no extra packages).

## Where things go

| What | Where |
| --- | --- |
| Level files | `Assets/Project Data/Content/LevelSystem/World Levels/<continent>/<country>/<Country>_NN.asset` |
| Game order | `Levels Database.asset` (level *n* of the game = entry *n*) |
| Countries, missions, dishes | `Assets/Project Data/Game/Scripts/World/WorldCatalog.cs` |

Level *n* belongs to country `(n-1) / 30` and is level `(n-1) % 30 + 1` there.
Each country's 30 levels are 3 missions of 10 (`WorldCatalog.Country.Missions`).

## How a level is checked

`levelgen.py` simulates the game exactly (7-slot dock, customers taking 3 dishes, the waiting
customer, spawners, hidden dishes):

* **Solver** - perfect play must win, keeping at least 2 dock slots free.
* **Casual player** - a simple bot plays 200 times; its win rate is the level's difficulty.

## Difficulty plan (`target()` in build_levels.py)

* Levels 1-5 of China are the hand-made tutorial levels.
* Every country is a little harder than the one before (China ~84% average, South Pole ~23%).
* Inside a country levels get harder towards the end; levels 5/15/25 are hard and 10/20/30
  are boss levels; the level after a hard one is a breather.
* Hand-made levels from `LevelSystem/Levels` are reused where their difficulty fits.

Change `target()` or `size_plan()` and rebuild to retune the whole game. `install_levels.py`
keeps each level's Unity GUID, so rebuilding never breaks references.

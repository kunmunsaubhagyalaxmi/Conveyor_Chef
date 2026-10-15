# Level Map: assets to generate

This is the asset list for the chapter-map Level Selection, the style with the factory map, the
"1-1" level buttons and the chapter tabs.

**One chapter = one mission = 10 levels on one map.** Each country has 3 chapters.

**Style for every asset:** glossy, bright, cartoon casual mobile-game style, the same as the rest of
Conveyor Chef. Use a transparent background (PNG) and **no text or numbers in any image**. The game
adds all text.

Generate each UI asset at **2× the listed size** if you can; the game scales them down.

---

## A. UI kit (generated once, used for every country)

| # | File name | Size (px) | What it is | Prompt idea |
|---|---|---|---|---|
| 1 | `node_open.png` | 256 × 256 | Open level button: round, glossy blue with a white/gold rim, empty centre | "glossy round blue game level button, gold rim, empty centre, top view, transparent background, no text" |
| 2 | `node_done.png` | 256 × 256 | Finished level button (optional, e.g. green or gold) | same as 1 but green/gold |
| 3 | `node_locked.png` | 256 × 256 | Locked level: dark grey/navy circle with a silver padlock | "dark navy round game button with a silver padlock in the middle, glossy, transparent background" |
| 4 | `node_current_glow.png` | 384 × 384 | Glow ring behind the next level to play (yellow/white sparkle ring, empty centre) | "glowing golden sparkle ring, empty centre, transparent background" |
| 5 | `node_hard.png` | 256 × 256 | Hard level button (red version of 1) | same as 1 but red with a gold rim |
| 6 | `node_boss.png` | 320 × 320 | Boss level button, the last level of a chapter (bigger, gold, a crown or chef hat on top, empty centre) | "big gold round boss level button with a small crown on top, empty centre, transparent background" |
| 7 | `star_full.png` | 128 × 128 | Gold star | "glossy gold cartoon star, thick dark outline, transparent background" |
| 8 | `star_empty.png` | 128 × 128 | Empty / grey star | same as 7, grey and slightly transparent |
| 9 | `path_dot.png` | 48 × 48 | One dot of the dotted path (dark brown/black, soft edge) | "single round dark brown dot, soft shadow, transparent background" |
| 10 | `header_bar.png` | 1000 × 220 | Top chapter title bar, empty, blue with a gold/white border (stretchable: plain middle, decorated ends) | "glossy blue game title banner, gold border, empty, wide, transparent background, no text" |
| 11 | `arrow_button.png` | 160 × 160 | Round arrow button pointing RIGHT (the game flips it for left) | "glossy round blue button with a white arrow pointing right, transparent background" |
| 12 | `tab_selected.png` | 400 × 180 | Selected chapter tab (cream with a gold border, rounded) | "rounded cream game tab button, gold border, empty, transparent background" |
| 13 | `tab_normal.png` | 400 × 180 | Unselected chapter tab (dark blue, rounded) | same as 12, dark blue with a light-blue border |
| 14 | `tab_bar_back.png` | 1080 × 220 | Background strip behind the tabs (dark navy with a gold top edge) | "dark navy game bottom bar, gold top edge, transparent background" |
| 15 | `lock_small.png` | 96 × 96 | Small padlock for locked tabs | "small silver cartoon padlock, transparent background" |
| 16 | `chef_pin.png` | 200 × 240 | Map pin showing the chef above the current level (optional; the game has a chef avatar) | "round map pin with a cute chef face inside, gold border, pointer at the bottom, transparent background" |
| 17 | `play_button.png` | 260 × 110 | Small green PLAY pill (optional, no text) | "glossy green rounded pill button, white border, empty, transparent background" |

**Total: 13 required (1, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 15) + 4 optional (2, 14, 16, 17).**

---

## B. Map backgrounds (one per chapter)

| What | Value |
|---|---|
| Size | **1080 × 1920** (vertical 9:16), PNG, no transparency needed |
| Count | **105** (35 countries × 3 missions). To save space, you can start with 1 per country (35). |
| File name | `levelmap_1.png`, `levelmap_2.png`, `levelmap_3.png` |
| Folder | `Assets/Project Data/Game/Resources/World/<continent>/<country>/` |
| Prompts and exact paths | `levelmap_prompts.csv` (one row per chapter, with the theme) |
| Layout guide | `levelmap_guide.png`: attach it to every map prompt |

**Rules for every map:**
- Paint one **empty, plain path** winding from the bottom to the top, as in the guide. Draw **no**
  dots, buttons, numbers, stars or locks on it; the game adds those.
- Put the **big landmark building** in spot A (like the "Golden Spoon" factory in the reference) and
  houses, food stalls, bridges and rivers in spots B–F.
- Keep the **top 20%** (title bar) and **bottom 10%** (tabs) simple: sea, sky or treetops.
- Keep important things away from the **left and right 10%**, which get cut off on tall phones.
- No text, signs with words, or characters.

**Map prompt template:**
> Bright glossy cartoon mobile-game chapter map, top-down 3/4 view, vertical 1080x1920.
> Theme: "<mission name>" in <country>. One big landmark building (<landmark>) in spot A, small
> themed houses, food stalls selling <dishes>, rivers, bridges and trees around. One empty winding
> dirt path exactly as in the attached guide, with nothing on it. Simple sea/sky at the top and bottom
> edges. No text, no numbers, no UI, no characters.

---

## C. Checklist

- [ ] UI kit: 13 required images (+ 4 optional)
- [ ] Map backgrounds: start with China 1–3 to test the look, then the rest of Asia, then the
      other continents
- [ ] Send me the UI kit and the first 3 China maps. I'll build the screen and line it up before
      you make the other 102.

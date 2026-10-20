# Panel tops with the title painted in (baked text)

Every panel's top section is regenerated with its **title painted into the picture, perfectly
centred**. The game uses each picture as it is (no text drawn on top). Add them one by one -
until a picture exists, the panel keeps its current look.

How to use:

1. Paste the **STYLE** block, then the item's prompt.
2. Attach the reference picture named in the item (same shape and layout are essential: the
   game places the content and the close button on fixed spots of that layout).
3. Save with the exact **file name** and **folder**, keep the exact **size**.
4. Tell Claude - it sets the import settings and checks the fit.

## STYLE (paste first)

```
Casual mobile cooking game UI, premium polished glossy cartoon style, exactly matching the
attached reference picture: same shape, same size, same proportions, same colours, same
lighting, same position of every part. Transparent background PNG. The title text is bold,
rounded, chunky cartoon lettering (like the Fredoka One font), all capital letters, cream-white
letters with a thick dark outline and a soft shadow, PERFECTLY CENTRED horizontally and
vertically inside its band, evenly spaced, not touching the band's edges, spelled exactly as
given. No other text anywhere.
```

---

## 1. Big menu panels (7 pictures)

- Size: **941 x 1672** - Folder: `Assets/Project Data/Game/Resources/MenuPanelFrames/`
- Reference for all 7: `Resources/MenuPanelFrames/blank.png`

| File | Title painted on the red ribbon |
|---|---|
| `shop.png` | SHOP |
| `collection.png` | COLLECTION |
| `achievements.png` | ACHIEVEMENTS |
| `challenges.png` | CHALLENGES |
| `customize.png` | CUSTOMIZE |
| `leaderboard.png` | LEADERBOARD |
| `story.png` | STORY |

```
The exact same panel as the reference (941x1672): glossy blue frame with gold trim, the red
ribbon with golden scroll ends across the top, the cream content area in the same place, and
the round orange-red close button at the top-right corner - now with a bold white X on it.
On the middle of the red ribbon, the word "[TITLE]" in cream-white letters with a thick dark
red outline, perfectly centred on the ribbon band, sized to fill about 70% of the band's width
(shorter words smaller so every title has the same letter height). Nothing else changes.
```

---

## 2. Popup title ribbons (3 pictures)

- Size: **2172 x 724** - Folder: `Assets/Project Data/Game/Resources/UI/Panels/`
- Reference for all 3: `Resources/UI/Panels/title_ribbon_blue.png`

| File | Title |
|---|---|
| `title_menu.png` | MENU |
| `title_settings.png` | SETTINGS |
| `title_language.png` | LANGUAGE |

```
The exact same arched blue banner with golden border and scroll curls as the reference
(2172x724). On the middle of the blue band, the word "[TITLE]" in cream-white letters with a
thick dark navy outline, perfectly centred on the band (follow the band's gentle arch),
filling about 65% of the band's width. Nothing else changes.
```

---

## 3. Level popups (2 pictures)

| File | Size | Reference | Title |
|---|---|---|---|
| `level_completed.png` | 1086 x 1448 | `Resources/UI/Panels/level_completed_blank.png` | LEVEL COMPLETED |
| `level_failed.png` | 819 x 1024 | `Resources/UI/Panels/level_failed_blank.png` | LEVEL FAILED |

- Folder: `Assets/Project Data/Game/Resources/UI/Panels/`

```
The exact same popup as the reference ([SIZE]): same frame, stars / chef hat on top, same
cream content area. On the blue banner under the top decoration, the words "[TITLE]" in
cream-white letters with a thick dark navy outline, perfectly centred on the banner, on one
line, filling about 75% of the banner's width. Nothing else changes.
```

---

## 4. Country Completed (2 pictures)

| File | Size | Reference | Title |
|---|---|---|---|
| `title_country_completed.png` | 2172 x 724 | `Resources/UI/Panels/country_title_blank.png` | COUNTRY COMPLETED! |
| `title_next_country.png` | 2172 x 724 | `Resources/UI/Panels/next_country_blank.png` | NEXT COUNTRY |

- Folder: `Assets/Project Data/Game/Resources/UI/Panels/`

```
The exact same banner as the reference (2172x724). On the middle of the band, the words
"[TITLE]" in cream-white letters with a thick dark outline (dark navy on blue, dark brown on
gold), perfectly centred on the band, on one line, filling about 70% of the band's width.
Nothing else changes.
```

---

Note: painted titles are English in every language (the words are part of the picture).

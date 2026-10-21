# Panel headers and Settings buttons, with the words painted in

Everything here has its text **baked into the picture**. The game draws no text on top.
The look copies the Settings panel you liked: glossy orange pills with a golden rim, and chunky
**yellow-gold letters with a thick dark-brown outline**.

How to use:

1. Paste the **STYLE** block for the group, then the item's prompt (replace `[WORD]`).
2. Attach the **reference picture** named in the group. The look must match it closely.
3. Save the picture as PNG with a transparent background, using the exact file name and folder.
4. Check the spelling of every letter before saving. Image generators often misspell words.
5. Tell Claude when a group is in. Claude sets the import settings and puts each picture in the game.

---

## A. Panel headers (only the top part of each panel)

The 7 big panels keep their frame. Only the red ribbon at the top is replaced. Claude paints each
new header onto the top of its panel picture, so the panel stays one single picture.

- **Size:** 2048 x 720, transparent background
- **Folder:** `Docs/UIArt/headers/`
- **Reference to attach:** `Docs/UIArt/references/panel_top_reference.png` (the current ribbon,
  for the shape), plus `Assets/Project Data/Game/Images/GameUI/Menu/settings_title.png` (for the
  lettering style)

### STYLE (paste first)

```
Casual mobile cooking game UI header, premium glossy cartoon style, matching the attached
references exactly. A single wide festive RED satin ribbon banner (#E0262B to #A3121A) with
thick shiny GOLDEN trim and golden curled scroll ends on both sides, gently arched upward in
the middle, folded ribbon tails tucked behind each end, like the attached panel top. Soft top
highlight, subtle inner shadow, lighting from above, straight-on symmetrical view.
On the ribbon, the title in big chunky rounded bold cartoon letters (like the Lilita One /
Luckiest Guy font), ALL CAPITAL LETTERS, bright YELLOW-GOLD gradient fill (light yellow at the
top to warm orange-gold at the bottom), a THICK DARK-BROWN outline (#4A2408) and a small soft
drop shadow, exactly like the lettering of the attached SETTINGS title. The text is PERFECTLY
CENTRED horizontally and vertically on the red band, follows the band's gentle arch, sits fully
inside the band with clear space before the golden scrolls on both sides, evenly spaced letters,
spelled exactly as given. No other text, no close button, no panel, no background.
Transparent background PNG, the banner fills the canvas width with a small margin.
```

### Prompt

```
The red and gold ribbon header described above, with the word "[WORD]" on it. Make the letters
as tall as the band allows; for a long word make the ribbon's red band wider so the word still
fits comfortably with space at both ends. Nothing else.
```

| File name | [WORD] |
|---|---|
| `header_shop.png` | SHOP |
| `header_collection.png` | COLLECTION |
| `header_achievements.png` | ACHIEVEMENTS |
| `header_challenges.png` | CHALLENGES |
| `header_customize.png` | CUSTOMIZE |
| `header_leaderboard.png` | LEADERBOARD |
| `header_story.png` | STORY |

### Popup title for the new Language panel

Make this one in the same style as the SETTINGS and MENU titles (blue banner, not red).

- **File:** `title_language.png`
- **Size:** 2172 x 724, transparent background
- **Folder:** `Assets/Project Data/Game/Images/GameUI/Menu/`
- **Reference to attach:** `Assets/Project Data/Game/Images/GameUI/Menu/settings_title.png`

```
The exact same arched glossy BLUE banner with thick golden border and golden scroll curls as the
attached SETTINGS title: same shape, same size, same colours, same lighting. Only the word
changes: "LANGUAGE" in the same chunky rounded bold yellow-gold letters with the same thick
dark-brown outline and soft shadow, PERFECTLY CENTRED on the blue band, same letter height as
the reference, evenly spaced, spelled exactly L-A-N-G-U-A-G-E. No other text. Transparent
background PNG, 2172x724.
```

---

## B. Settings buttons (same family as SOUND ON / VIBRATION ON)

- **Size:** 1800 x 610, transparent background
- **Folder:** `Assets/Project Data/Game/Images/GameUI/Menu/`
- **Reference to attach:** `Assets/Project Data/Game/Images/GameUI/Menu/sound_on_button.png`
  (for the OFF buttons, attach `sound_off_button.png` as well)

### STYLE (paste first)

```
Casual mobile cooking game UI button, premium glossy cartoon style, exactly matching the attached
reference button: the same long rounded pill shape, the same proportions, the same glossy ORANGE
body (#FF9A1F to #E0520F) with the same thick shiny GOLDEN rim, the same white highlight streak
along the top, the same soft inner shadow and lighting from above. Straight-on view, no
perspective.
On the left, a chunky cartoon icon in golden yellow with a thick dark-brown outline, the same
size and position as the speaker icon in the reference. To its right, the words in the SAME big
chunky rounded bold letters as the reference, ALL CAPITAL LETTERS, bright yellow-gold fill with a
THICK DARK-BROWN outline (#4A2408) and a soft shadow, vertically centred, the icon and words
together centred on the button, spelled exactly as given. No other text.
Transparent background PNG, the button fills the canvas with a small margin, 1800x610.
```

### Prompts

| File name | Icon | Words |
|---|---|---|
| `music_on_button.png` | a pair of music notes (beamed eighth notes) | MUSIC ON |
| `music_off_button.png` | the same music notes with a thick diagonal slash across them, like the slashed speaker on the SOUND OFF button | MUSIC OFF |
| `language_button.png` | a round globe with meridian lines | LANGUAGE |

```
The button described above, with [ICON] on the left and the words "[WORDS]". Nothing else.
```

### Language list buttons (inside the Language panel)

These are the same orange pill, but with **no icon**: only the language name, centred.

- **Size:** 1800 x 610, transparent background
- **Folder:** `Assets/Project Data/Game/Images/GameUI/Language/`
- **Reference to attach:** `Assets/Project Data/Game/Images/GameUI/Menu/sound_on_button.png`

```
The same orange pill button as the attached reference, same rim, gloss and lighting, but with NO
icon. Only the word "[NAME]" in the same chunky rounded bold yellow-gold letters with the thick
dark-brown outline, PERFECTLY CENTRED on the button, as large as the reference's words, spelled
exactly as given, including the accent marks. No other text. Transparent background PNG,
1800x610.
```

| File name | [NAME] |
|---|---|
| `lang_english.png` | ENGLISH |
| `lang_hinglish.png` | HINGLISH |
| `lang_spanish.png` | ESPAÑOL |
| `lang_portuguese.png` | PORTUGUÊS |
| `lang_indonesian.png` | INDONESIA |
| `lang_french.png` | FRANÇAIS |
| `lang_german.png` | DEUTSCH |
| `lang_turkish.png` | TÜRKÇE |

Check the accent marks (Ñ, Ê, Ç, Ü) carefully. They are the letters generators get wrong most often.

### Tick for the chosen language

A small badge on the chosen language's button, so you don't need a second set of 8 buttons.

- **File:** `lang_selected_tick.png`
- **Size:** 512 x 512, transparent background
- **Folder:** `Assets/Project Data/Game/Images/GameUI/Language/`

```
Casual mobile game UI badge, premium glossy cartoon style matching the attached orange button:
a round glossy GREEN badge (#4CD137 to #1E8E16) with a thick shiny golden rim and a white
highlight on top, with a bold white check mark in the middle that has a thick dark-green outline.
Straight-on view, centred. No text. Transparent background PNG, 512x512.
```

---

## Order

1. **Group A headers first.** They fix the most visible problem.
2. **Then the Settings buttons:** MUSIC ON/OFF and LANGUAGE.
3. **Then the Language list and the tick.**

The language option itself (switching the game's words) comes back with these pictures. Until
then, the panels keep their current look, so nothing breaks in between.

---

## C. Popup titles: OUT OF POWER-UPS and RESTART LEVEL?

Same style as the SETTINGS title (blue banner, gold scrolls, yellow-gold letters). The game uses
them as soon as they are in the folder; until then it shows the blank blue ribbon with written text.

- **Size:** 2172 x 724, transparent background
- **Folder:** `Assets/Project Data/Game/Resources/UI/Panels/`
- **Reference to attach:** `Assets/Project Data/Game/Images/GameUI/Menu/settings_title.png`

```
The exact same arched glossy BLUE banner with thick golden border and golden scroll curls as the
attached SETTINGS title: same shape, same size, same colours, same lighting. Only the words change:
"[WORDS]" in the same chunky rounded bold yellow-gold letters with the same thick dark-brown
outline and soft shadow, PERFECTLY CENTRED on the blue band, one line, evenly spaced, as tall as
the band allows while keeping clear space before the golden scrolls on both sides (make the blue
band a little wider if needed), spelled exactly as given. No other text, no icons. Transparent
background PNG, 2172x724.
```

| File name | [WORDS] | Spelling check |
|---|---|---|
| `title_out_of_powerups.png` | OUT OF POWER-UPS | O-U-T  O-F  P-O-W-E-R-hyphen-U-P-S |
| `title_restart_level.png` | RESTART LEVEL? | R-E-S-T-A-R-T  L-E-V-E-L-? |

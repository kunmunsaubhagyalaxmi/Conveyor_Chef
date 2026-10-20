# Conveyor Chef - new UI art (master prompts)

Every word in the game is now written by the game itself (centred, and in the player's
language: English, Hinglish, Spanish, Portuguese, Indonesian, French, German, Turkish).
So **no picture may contain any letters or numbers** - the game puts the words on top.

How to use this file:

1. Paste the **MASTER STYLE** block first, then the prompt of one item.
2. Attach the reference picture named in the item, so the style matches the current game.
3. Save the PNG with the exact **file name**, **size** and **folder** given.
4. Tell Claude when a group is in: it sets the import settings, measures where the words
   go and switches the game over. Until then the old pictures stay, so nothing breaks.

Make them in this order: **A (panels) -> B (buttons) -> C (icons) -> D (main menu)**.

---

## MASTER STYLE (paste before every prompt)

```
Casual mobile cooking game UI asset, premium polished style matching the attached reference:
glossy cartoon rendering, thick rounded shapes, saturated colours, soft top highlight and a
subtle inner shadow, clean dark outline. Palette: royal blue frames (#1F6FD8 to #0E4FA8) with
bright golden-yellow trim (#FFC93A to #E89A10), warm cream insides (#FFF4DC), festive red
ribbons (#E0262B to #A3121A), glossy green (#4CD137 to #1E8E16), glossy orange (#FF9A1F to
#E0520F). Lighting from the top. Centered, symmetrical, straight-on view (no perspective).
ABSOLUTELY NO TEXT: no letters, no numbers, no words, no symbols that look like letters.
Transparent background (PNG with alpha), nothing outside the asset, no drop shadow on the
background, no watermark. Exact canvas size as given.
```

---

## A. Panels (most important - fixes the off-centre titles)

### A1. Big menu panel - ONE frame for all 7 panels
Shop, Collection, Achievements, Challenges, Customize, Leaderboard and Story all use it; the
game writes SHOP / COLLECTION / ... on the ribbon.

- File: `blank.png` - **941 x 1672**
- Folder: `Assets/Project Data/Game/Resources/MenuPanelFrames/`
- Reference: `Resources/MenuPanelFrames/shop.png`

```
A tall portrait game panel, 941x1672, exactly like the reference panel: glossy royal-blue
outer frame with golden trim and golden corner flourishes, a large warm-cream content area
with rounded corners and a thin gold inner line. Across the top centre, a festive red ribbon
banner with golden scroll ends and small golden curls, sitting DIRECTLY on the top edge of the
frame - no blue arch or empty blue area under the ribbon. The ribbon's middle is a clean,
flat, evenly lit red band, wide and tall enough for one word, completely EMPTY. A round
glossy orange-red close button with a white X at the top-right corner, overlapping the frame.
The cream content area starts right under the ribbon and fills the panel down to the bottom
frame. No text anywhere.
```

### A2. Popup title ribbon (MENU, SETTINGS, LANGUAGE, PAUSED)
Sits on top of the small popup panel (`Images/GameUI/Popups/pause_popup_panel.png`, which has
no words and stays).

- File: `title_ribbon_blue.png` - **2172 x 724**
- Folder: `Assets/Project Data/Game/Images/GameUI/Menu/`
- Reference: `Images/GameUI/Menu/menu_title.png`

```
A wide arched title banner, 2172x724, like the reference "MENU" banner: glossy royal-blue
banner with thick golden-yellow border and golden scroll curls at both ends, gently arched.
The middle of the banner is a clean, flat, evenly lit blue band for one word, completely
EMPTY. No text.
```

### A3. Level Completed popup
- File: `level_completed_blank.png` - **1086 x 1448**
- Folder: `Assets/Project Data/Game/Images/GameUI/Popups/`
- Reference: `Images/GameUI/Popups/level_completed_banner.png`

```
The same popup as the reference: royal-blue frame with gold trim, cream content area, three
golden stars on top and the blue arched banner under them. The banner is completely EMPTY
(no words) - a clean blue band with golden border, wide enough for two words. Keep the
sparkles. No text.
```

### A4. Level Failed popup
- File: `level_failed_blank.png` - **819 x 1024**
- Folder: `Assets/Project Data/Game/Images/GameUI/Popups/`
- Reference: `Images/GameUI/Popups/level_failed_popup.png`

```
The same popup as the reference: royal-blue frame with gold trim, cream content area, the
chef hat with a cracked heart and the crossed spatula and whisk on top. The blue banner is
completely EMPTY (no words) - a clean blue band with golden border. No text.
```

### A5. Country Completed title
- File: `country_title_blank.png` - **2172 x 724**
- Folder: `Assets/Project Data/Game/Images/CountryComplete/`
- Reference: `Images/CountryComplete/country_completed_title.png`

```
The same arched blue-and-gold title banner as the reference, with the small golden stars at
the ends, but the banner middle is completely EMPTY - a clean blue band for two words. No text.
```

### A6. Next Country label
- File: `next_country_blank.png` - **2172 x 724**
- Folder: `Assets/Project Data/Game/Images/CountryComplete/`
- Reference: `Images/CountryComplete/next_country_label.png`

```
The same golden plaque with red ribbon tails as the reference, but the plaque middle is
completely EMPTY - a clean golden band for two words. No text.
```

---

## B. Buttons (all without words - one matching family)

All buttons share one shape: a wide rounded pill with a thick golden-yellow outline, a glossy
gradient body, a soft white highlight across the top third and a darker bottom lip.
Words (and the icon, see C) are put on by the game.

- Folder for all: `Assets/Project Data/Game/Images/GameUI/Buttons/`
- Reference for all: `Images/GameUI/Menu/sound_on_button.png`

| File | Size | Colour | Used for |
|---|---|---|---|
| `pill_green.png` | 2172 x 724 | glossy green | PLAY, CONTINUE, CLAIM |
| `pill_blue.png` | 2172 x 724 | glossy royal blue | STORY, CHALLENGES, CUSTOMIZE, SETTINGS |
| `pill_orange.png` | 2172 x 724 | glossy orange-to-red | SOUND, VIBRATION, MUSIC, LANGUAGE |
| `pill_red.png` | 2172 x 724 | glossy red | RESTART, QUIT |
| `pill_pink.png` | 2172 x 724 | glossy pink-magenta with dark navy inner band | GET X3 (watch ad) |
| `pill_gray.png` | 2172 x 724 | soft grey (disabled / OFF state) | locked or OFF |
| `small_orange.png` | 1024 x 430 | glossy orange-to-red | language list, tabs |
| `small_green.png` | 1024 x 430 | glossy green | chosen language, chosen tab |

Prompt (change the colour and size per row):

```
A wide rounded pill-shaped game button, [SIZE], glossy [COLOUR] body with a thick
golden-yellow outline, a soft white highlight across the top third and a slightly darker
bottom lip giving depth, exactly the same shape, outline thickness and lighting as the
reference button. Completely EMPTY face - no icon, no text. Transparent background.
```

---

## C. Icons (drawn on the left of a button by the game)

Square, glossy golden-yellow with a dark brown outline (like the speaker icon in the
reference SOUND ON button), readable at small size.

- Size: **512 x 512** each - Folder: `Assets/Project Data/Game/Images/GameUI/Icons/`
- Reference: `Images/GameUI/Menu/sound_on_button.png` (the speaker icon)

| File | Icon |
|---|---|
| `icon_sound_on.png` | speaker with sound waves |
| `icon_sound_off.png` | speaker with a red diagonal line |
| `icon_vibration_on.png` | phone with vibration lines |
| `icon_vibration_off.png` | grey phone, no lines |
| `icon_music_on.png` | two musical notes |
| `icon_music_off.png` | musical notes with a red diagonal line |
| `icon_language.png` | globe with meridian lines |
| `icon_play.png` | play triangle |
| `icon_restart.png` | circular arrow |
| `icon_story.png` | open book |
| `icon_challenges.png` | crossed fork and spoon with a star |
| `icon_customize.png` | chef hat with a small brush |
| `icon_settings.png` | gear |
| `icon_video.png` | film strip / video play (for GET X3) |

```
A single game icon, 512x512, [ICON], glossy golden-yellow with a thick dark brown outline and
a soft highlight, same style as the speaker icon in the reference button. Centered, no
background, no text.
```

---

## D. Main menu bottom bar (labels written by the game)

The four bottom buttons keep their pictures, but without the word plaque; the game writes
the label on a separate plaque.

- Size: **512 x 512** each - Folder: `Assets/Project Data/Game/Resources/MainMenu/`
- References: `Resources/MainMenu/shop.png`, `collection.png`, `achievements.png`, `leaderboard.png`

| File | Picture |
|---|---|
| `nav_shop.png` | the little shop with red-white awning (same as reference) |
| `nav_collection.png` | the brown recipe book with chef hat (same as reference) |
| `nav_achievements.png` | the golden trophy with laurels (same as reference) |
| `nav_leaderboard.png` | the podium 1-2-3 with crown (same as reference) - numbers allowed here |
| `nav_plaque.png` (1024 x 300) | the empty brown-and-gold label plaque from the references |

```
The same game icon as the reference, 512x512, identical style, but WITHOUT the label plaque
and without any text under it - only the picture, centered. Transparent background.
```
```
The brown rounded label plaque with golden border from the reference, 1024x300, completely
EMPTY - no text. Transparent background.
```

---

## Not needed (already without words)

`pause_popup_panel.png`, `level_complete_popup.png`, `country_complete_frame.png`,
`ribbon_red_wide.png`, `ribbon_red_thin.png`, `home_button.png`, `next_country_card.png`,
stars, flags, landmarks and the level card pictures.

## Note on numbers

Pictures may show numbers only where they are part of the drawing (the 1-2-3 podium).
Coin, diamond and level numbers are always written by the game.

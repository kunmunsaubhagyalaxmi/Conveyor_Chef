"""Writes Docs/LevelCardArt/level_card_prompts.csv and .md: one image prompt per level card.

Every prompt uses the same style as the hand-made India cards (Mumbai Docks, Varanasi Ghats):
a bright, painterly mobile-game scene of the real place the level is named after.
The mood (day / sunset / night / snow) and focus (landmark / market / eatery) follow the title.

Run from the project root:  python Tools/make_level_card_prompts.py
"""
import csv
import os
import re

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
TITLES = os.path.join(ROOT, "Assets/Project Data/Game/Resources/World/card_titles.txt")
OUT = os.path.join(ROOT, "Docs/LevelCardArt")

CONTINENTS = [("asia", "Asia"), ("north_america", "North America"), ("south_america", "South America"),
              ("europe", "Europe"), ("africa", "Africa"), ("oceania", "Oceania"), ("antarctica", "Antarctica")]
COUNTRIES = [  # (folder, name, continent index, flag) in WorldCatalog order, 30 levels each
    ("china", "China", 0, "Chinese"), ("japan", "Japan", 0, "Japanese"), ("india", "India", 0, "Indian"),
    ("south_korea", "South Korea", 0, "South Korean"), ("thailand", "Thailand", 0, "Thai"),
    ("usa", "United States", 1, "American"), ("mexico", "Mexico", 1, "Mexican"), ("canada", "Canada", 1, "Canadian"),
    ("cuba", "Cuba", 1, "Cuban"), ("jamaica", "Jamaica", 1, "Jamaican"),
    ("brazil", "Brazil", 2, "Brazilian"), ("argentina", "Argentina", 2, "Argentine"), ("peru", "Peru", 2, "Peruvian"),
    ("colombia", "Colombia", 2, "Colombian"), ("chile", "Chile", 2, "Chilean"),
    ("italy", "Italy", 3, "Italian"), ("france", "France", 3, "French"), ("spain", "Spain", 3, "Spanish"),
    ("greece", "Greece", 3, "Greek"), ("germany", "Germany", 3, "German"),
    ("morocco", "Morocco", 4, "Moroccan"), ("egypt", "Egypt", 4, "Egyptian"), ("nigeria", "Nigeria", 4, "Nigerian"),
    ("ethiopia", "Ethiopia", 4, "Ethiopian"), ("south_africa", "South Africa", 4, "South African"),
    ("australia", "Australia", 5, "Australian"), ("new_zealand", "New Zealand", 5, "New Zealand"),
    ("fiji", "Fiji", 5, "Fijian"), ("samoa", "Samoa", 5, "Samoan"), ("tonga", "Tonga", 5, "Tongan"),
    ("peninsula", "the Antarctic Peninsula", 6, None), ("weddell_sea", "the Weddell Sea, Antarctica", 6, None),
    ("ross_shelf", "the Ross Ice Shelf, Antarctica", 6, None), ("mcmurdo", "McMurdo Sound, Antarctica", 6, None),
    ("south_pole", "the South Pole, Antarctica", 6, None),
]

STYLE = ("Bright, cheerful, highly detailed digital painting in a polished casual mobile-game style "
         "(soft painterly rendering, clean readable shapes, glossy highlights, rich saturated colours), "
         "wide 4:3 landscape, 1448x1086.")
RULES = ("The real place must be instantly recognisable: accurate, famous architecture or nature of this exact location as the big clear centrepiece "
         "in the middle of the picture, seen from a classic postcard viewpoint. Lively but uncluttered: a few tiny people far away (no close-up faces), "
         "local details that belong there. Keep the edges calm (the card crops a little on every side). "
         "No text, no letters, no signs with words, no logos, no watermark, no border, no UI.")

NIGHT = re.compile(r"night|nights|lights|after dark|fiafia|aurora|stars|polar night|jazz|pub|bar\b|carnival|festival|fest\b|fair\b|parade|party|rumba|tanabata|neon|akihabara|strip|times square|dinner cruise|day of the dead", re.I)
SUNSET = re.compile(r"sunset|sunrise|golden|ghats|dusk|evening", re.I)
SNOW = re.compile(r"snow|ice\b|ice festival|winter|christmas|alps|glacier|chalet|polar|zugspitze|whistler|banff|lake louise|jasper|churchill|yukon|sapporo|harbin|mount cook|tekapo|seoraksan|chamonix|dolomites|aconcagua|ushuaia|perito|calafate|tierra del fuego|torres del paine|marble caves", re.I)
MARKET = re.compile(r"market|bazaar|souk|medina|merkato|square\b|plaza|street|streets|lane|laneways|alleys|khao san|mall road|chinatown|quarter|old town|old city|shibuya crossing|food hall|spice", re.I)
FOOD = re.compile(r"bites|treats|kitchen|cafe|caf[eé]|bistro|diner|grill|bbq|barbie|braai|tapas|pizza|pizzeria|tacos|ceviche|bakery|patisserie|dim sum|hot pot|okonomiyaki|yatai|feast|snack|curry|jerk|currywurst|chocolate|cheese|lobster|seafood|fish fry|sushi|sweets|mustard|marzipan|bagels|crepes|cannoli|pesto|kebab|coffee|tea house|paella|pintxos|asado|hangi|lovo|umu|clam|cheesesteak|ribs|chicken|coney|deep dish|beef|petha|darshini|dakgalbi|suya|mess hall|snack bar|cart|kiosk|shack|stand|luau|wine|vineyard|wineries|winery|bodega|brewery|cellars|cider|cocoa|salmon|okonomiyaki|eats|rush|food", re.I)
WATER = re.compile(r"harbour|harbor|port|pier|docks|bay|beach|coast|shore|lake|river|canal|lagoon|marina|falls|sound|cove|island|islands|reef|waterfront|seafront|corniche|malecon|riverside|riverboat|boats|backwaters|felucca|sea\b|ocean|surf|fjord|blue lagoon|cay\b", re.I)


def mood(title, antarctic, level):
    if re.search(r"aurora", title, re.I):
        return "Night scene under a glowing green aurora in a starry sky, warm lit windows."
    if re.search(r"stars", title, re.I):
        return "Clear night under a breathtaking starry sky and the Milky Way, warm glowing lights below."
    if NIGHT.search(title):
        return "Evening scene: deep blue twilight sky, warm glowing lanterns and string lights, cosy golden light spilling from windows and stalls."
    if SUNSET.search(title):
        return "Golden sunset: a big warm sun low in an orange-pink-purple sky, everything bathed in warm light with long reflections."
    if antarctic:
        return "Crisp polar daylight: dazzling white snow and blue ice under a clear pale-blue sky, soft pink-gold low sun on the horizon."
    if SNOW.search(title):
        return "Sunny winter day: fresh white snow on rooftops and trees, clear blue sky, warm light glowing from windows."
    if level % 3 == 2:
        return "Warm golden late-afternoon light, blue sky with soft pink-gold fluffy clouds, gentle long shadows."
    return "Sunny day: vivid blue sky with big fluffy white clouds, bright sunlight, soft shadows."


def focus(title, country, flag, antarctic):
    parts = []
    if antarctic:
        parts.append(f"The scene is \"{title}\" in {country}: show this real Antarctic place (its ice, mountains, coast or research station) "
                     "as it really looks, with cute penguins or seals where they belong and a small cosy red expedition hut or tent glowing warmly, "
                     "suggesting a friendly polar kitchen.")
    elif FOOD.search(title) and not MARKET.search(title):
        parts.append(f"The scene is \"{title}\" in {country}: the city or place in the name with its most famous landmark clearly visible behind, "
                     "and in the mid-ground a charming, inviting little eatery, stall or terrace serving the local signature food this name suggests, "
                     "with colourful awnings and a little steam rising.")
    elif MARKET.search(title):
        parts.append(f"The scene is \"{title}\" in {country}: this exact famous street, square or market as it really looks, "
                     "bustling with colourful stalls, awnings, lanterns and local produce, its signature buildings or landmark rising behind.")
    else:
        parts.append(f"The scene is \"{title}\" in {country}: this exact real landmark or landscape as it really looks, "
                     "shown big and proud as the centrepiece.")
    if WATER.search(title):
        parts.append("Icy blue water in the foreground with floating ice and one small red expedition boat." if antarctic
                     else "Sparkling blue water in the foreground with beautiful reflections and a few local boats.")
    if flag:
        parts.append(f"One or two small {flag} flags placed naturally (on a boat, stall or pole).")
    return " ".join(parts)


def main():
    titles = [t.strip() for t in open(TITLES, encoding="utf-8").read().splitlines() if t.strip()]
    os.makedirs(OUT, exist_ok=True)
    rows = []
    for i, title in enumerate(titles):
        c = min(i // 30, len(COUNTRIES) - 1)
        folder, country, cont, flag = COUNTRIES[c]
        level = i % 30
        antarctic = cont == 6
        prompt = " ".join([STYLE, focus(title, country, flag, antarctic), mood(title, antarctic, level), RULES])
        path = f"Assets/Project Data/Game/Resources/World/{CONTINENTS[cont][0]}/{folder}/level_{level + 1}.png"
        rows.append(dict(level=i + 1, continent=CONTINENTS[cont][1], country=COUNTRIES[c][1].replace("the ", "").split(",")[0],
                         card=level + 1, title=title, save_as=path, prompt=prompt))

    with open(os.path.join(OUT, "level_card_prompts.csv"), "w", encoding="utf-8-sig", newline="") as f:
        w = csv.DictWriter(f, fieldnames=list(rows[0].keys()))
        w.writeheader()
        w.writerows(rows)

    with open(os.path.join(OUT, "level_card_prompts.md"), "w", encoding="utf-8") as f:
        f.write("# Level card image prompts\n\n")
        f.write("One prompt per level, in the style of the India cards (Mumbai Docks, Varanasi Ghats).\n\n")
        f.write("* Generate at 4:3 landscape (1448x1086 or 1024x768), PNG.\n")
        f.write("* Save it exactly at the `save as` path (replaces the placeholder). The game shows it on the card automatically.\n")
        f.write("* Same list as a spreadsheet: `level_card_prompts.csv`.\n")
        f.write("* Regenerate this file after changing card titles: `python Tools/make_level_card_prompts.py`.\n\n")
        current = None
        for r in rows:
            if r["country"] != current:
                current = r["country"]
                f.write(f"\n## {r['continent']} - {current}\n\n")
            f.write(f"### {r['card']}. {r['title']}\n\nSave as: `{r['save_as']}`\n\n```\n{r['prompt']}\n```\n\n")
    print(len(rows), "prompts written to", OUT)


if __name__ == "__main__":
    main()

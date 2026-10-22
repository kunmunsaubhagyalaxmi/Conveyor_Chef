# Country map icons (the round emblem by COUNTRY PROGRESS on Level Selection)

India already has one: `Images/LevelSelection/glossy_tricolour_india_map_icon.png`, the country's outline
filled with its flag, glossy, on a transparent background. Every other country gets the same.

- **Size:** 1024 x 1024, transparent background (PNG)
- **Reference to attach:** `Assets/Project Data/Game/Images/LevelSelection/glossy_tricolour_india_map_icon.png`
- **File name:** `levelselect_icon.png`, one per country, in that country's folder (table below)
- The game picks it up by itself (the import settings are set automatically). Until a country has
  one, the emblem shows the country's flag.

## STYLE (paste first)

```
The exact same style as the attached India icon: the country's outline as one chunky, glossy,
slightly puffy 3D shape, filled with the country's flag (the flag's colours and symbols laid across
the shape, the main symbol large and centred), soft highlights on top, a thin darker rim, gentle
inner shading, cartoon mobile-game look. The shape is centred and fills about 90% of the square,
with the same margin as the reference. No text, no border, no background, no shadow outside the
shape. Transparent background PNG, 1024x1024.
```

## Prompt (one per country)

```
[STYLE above]
Country: [COUNTRY]. Shape: [SHAPE]. Flag: [FLAG].
```

| Folder (`Assets/Project Data/Game/Resources/World/...`) | [COUNTRY] | [SHAPE] | [FLAG] |
|---|---|---|---|
| `asia/china/` | China | mainland China outline (Hainan and Taiwan left out) | red, with the large gold star and four small gold stars at the upper left |
| `asia/japan/` | Japan | the four main islands as one arched group, enlarged | white with a red circle in the middle |
| `asia/south_korea/` | South Korea | the southern half of the Korean peninsula | white with the red-and-blue taegeuk circle in the middle and the four black trigrams |
| `asia/thailand/` | Thailand | Thailand outline with its long southern tail | five horizontal stripes: red, white, wide blue, white, red |
| `north_america/usa/` | United States | the 48 mainland states | red and white stripes with the blue canton of white stars at the upper left |
| `north_america/mexico/` | Mexico | Mexico outline with Baja California | green, white, red vertical bands with the eagle emblem in the middle |
| `north_america/canada/` | Canada | Canada outline, the Arctic islands simplified | red, white, red vertical bands with the red maple leaf in the middle |
| `north_america/cuba/` | Cuba | the long island of Cuba, enlarged | blue and white stripes with the red triangle and white star on the left |
| `north_america/jamaica/` | Jamaica | the island of Jamaica, enlarged | the gold diagonal cross with green top and bottom and black left and right triangles |
| `south_america/brazil/` | Brazil | Brazil outline | green with the yellow diamond and the blue starry globe with its white band |
| `south_america/argentina/` | Argentina | Argentina outline down to Tierra del Fuego | light blue, white, light blue stripes with the golden Sun of May |
| `south_america/peru/` | Peru | Peru outline | red, white, red vertical bands |
| `south_america/colombia/` | Colombia | Colombia outline | yellow (top half), blue, red horizontal bands |
| `south_america/chile/` | Chile | Chile's long thin outline, drawn a little wider so it reads | white over red with the blue square and white star at the upper left |
| `europe/italy/` | Italy | the Italian boot with Sicily | green, white, red vertical bands |
| `europe/france/` | France | the French hexagon with Corsica | blue, white, red vertical bands |
| `europe/spain/` | Spain | the Spanish mainland | red, wide yellow, red horizontal bands with the coat of arms on the left |
| `europe/greece/` | Greece | the Greek mainland with Crete and a few islands, grouped | blue and white stripes with the white cross on blue at the upper left |
| `europe/germany/` | Germany | Germany outline | black, red, gold horizontal bands |
| `africa/morocco/` | Morocco | Morocco outline | red with the green five-pointed star outline in the middle |
| `africa/egypt/` | Egypt | Egypt's near-square outline with Sinai | red, white, black horizontal bands with the golden eagle in the middle |
| `africa/nigeria/` | Nigeria | Nigeria outline | green, white, green vertical bands |
| `africa/ethiopia/` | Ethiopia | Ethiopia outline | green, yellow, red horizontal bands with the blue disc and yellow star in the middle |
| `africa/south_africa/` | South Africa | South Africa outline | the green Y shape with red, blue, black, gold and white bands |
| `oceania/australia/` | Australia | Australia with Tasmania | blue with the Union Jack at the upper left, the large white star and the Southern Cross |
| `oceania/new_zealand/` | New Zealand | the North and South Islands side by side | blue with the Union Jack at the upper left and four red stars with white edges |
| `oceania/fiji/` | Fiji | the two main islands grouped, enlarged | light blue with the Union Jack at the upper left and the Fiji shield on the right |
| `oceania/samoa/` | Samoa | the two main islands side by side, enlarged | red with the blue square at the upper left holding five white stars |
| `oceania/tonga/` | Tonga | the main island group, gathered and enlarged | red with the white square at the upper left holding a red cross |

Antarctica has no national flags, so its five stops share one look: the Antarctica outline in glossy
icy white and pale blue, with one red map pin on the stop's place.

```
[STYLE above, with "filled with the country's flag" replaced by "in glossy icy white and pale blue"]
The whole Antarctic continent outline, with one shiny red map pin standing on [PLACE].
```

| Folder | [PLACE] |
|---|---|
| `antarctica/peninsula/` | the tip of the Antarctic Peninsula (the arm reaching towards South America) |
| `antarctica/weddell_sea/` | the deep bay on the Atlantic side, next to the peninsula |
| `antarctica/ross_shelf/` | the great ice shelf in the big bay on the Pacific side |
| `antarctica/mcmurdo/` | the edge of the Ross ice shelf, with a tiny smoking volcano beside the pin |
| `antarctica/south_pole/` | the exact centre of the continent, with a tiny red-and-white striped pole |

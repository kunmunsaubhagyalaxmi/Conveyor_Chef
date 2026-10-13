# New dishes: orange, brown and white

Each country is getting 3 more dish types, bringing it from 7 to 10. That makes **105 new pictures**
(35 countries × 3). Each picture's dish name, exact file path and a ready prompt are in
**`new_dishes.csv`**.

## Picture rules

- **Size and format:** a square transparent PNG, 512 × 512 or larger. The game scales it to 512.
- **Content:** one dish on its own. Use the same style as the existing dishes, and attach two or
  three of that country's existing dish pictures to the prompt as a style reference, for example
  `Resources/World/europe/italy/dish_red.png`.
- **Colour:** the food's main colour must clearly be **orange**, **brown** or **white**, because
  players match dishes by colour.
- **No extras:** no text, hands, table or background.

## Where they go

Save every picture to the path in the CSV, including the Asian countries:

```
Assets/Project Data/Game/Resources/World/<continent>/<country>/dish_orange.png
Assets/Project Data/Game/Resources/World/<continent>/<country>/dish_brown.png
Assets/Project Data/Game/Resources/World/<continent>/<country>/dish_white.png
```

They import as dish sprites by themselves (`DishArtImporter`).

## Putting them into the levels

When pictures are in, run:

```
python Tools/LevelPipeline/add_new_dishes.py --apply
```

This swaps which colours each level uses, so the new dishes appear across every country's
30 levels. The layout, customers and difficulty of each level stay exactly the same. It only
uses colours whose picture exists for that country, so you can add pictures one country at a
time and run it again. Without `--apply` it only shows what it would change.

**Important:** don't run it before the pictures for a country are in. A dish without a picture
would show as the plain coloured character instead of food.

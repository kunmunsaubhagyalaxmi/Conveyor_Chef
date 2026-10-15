# Chef outfit art

Each outfit needs 2 transparent PNGs in `Assets/Project Data/Game/Resources/Chef/`:

- `<id>_avatar.png`: a square head-and-shoulders portrait, 512×512.
- `<id>_full.png`: the whole standing chef, 512×1024 (portrait).

Use the same chef as the current Classic art, with the same face, cartoon style and line weight, so only the outfit changes. Classic already uses the existing art. Until an outfit's PNGs are added, it shows "COMING SOON" in Customize > CHEF.

Start every prompt with this shared style line:
> Cute cartoon mobile-game chef character, same friendly round face as the reference, bold clean outlines, soft cel shading, bright saturated colours, transparent background, centred, no text.

| id | Name | Unlocks at | Prompt (add after the style line) |
|---|---|---|---|
| street_food | Street Food Chef | Lv 3 | orange bandana, rolled-up sleeves, grease-spotted striped apron, holding a wok with flames |
| pastry | Pastry Chef | Lv 6 | tall white toque, pastel pink apron dusted with flour, holding a piping bag and a cupcake |
| sushi | Sushi Master | Lv 10 | white hachimaki headband with red circle, navy happi jacket, holding a sushi knife and a nigiri |
| pizza | Pizza Maestro | Lv 14 | red neck scarf, white jacket with Italian-flag trim, holding a wooden pizza peel with a pizza |
| bbq | BBQ Pitmaster | Lv 18 | cowboy-style hat, brown leather apron, plaid shirt, holding big tongs and a sizzling steak |
| royal | Royal Chef | Lv 24 | white double-breasted jacket with gold trim and buttons, small golden crown on a toque, purple sash |
| golden | Golden Chef | Lv 30 | shining gold jacket and gold toque, sparkles around, holding a golden ladle, legendary look |

For the avatar, add "head and shoulders portrait, square". For the full body, add "full body standing pose, feet visible".

## Chef poses (win jump, loading screen)

Put these in `Assets/Project Data/Game/Resources/Chef/Poses/` as transparent portrait PNGs (about 1024×1536). Without them, the standing chef is used.

- `chef_jump.png`: shown on the win screen as the chef jumps up.
- `chef_cheer.png` (optional): shown after he lands.
- `chef_wave.png`: shown on the loading screen.

Start each prompt with this shared line, and attach `Images/MenuPanels/chef_full.png` as the reference image:
> The exact same chef character as the reference image: young cartoon chef with messy brown hair, big brown eyes, rosy cheeks, tall white chef hat, white double-breasted chef jacket with black buttons, red neckerchief, navy blue apron with a red cloth tucked at the side, navy trousers, white sneakers. Same 3D-cartoon mobile game style, soft lighting, clean edges, full body, centred, transparent background, no text, no shadow on the ground. Portrait image 1024×1536.

| File | Add after the shared line |
|---|---|
| chef_jump.png | jumping high in the air with joy, both fists raised above his head, legs bent mid-jump, huge open-mouth happy smile, eyes closed in delight, chef hat slightly tilted from the jump, apron flapping, dynamic energetic pose |
| chef_wave.png | standing and waving hello with his right hand raised high, left hand on his hip, friendly wide smile, slight head tilt, relaxed cheerful pose |
| chef_cheer.png | landing from a jump with knees slightly bent, one arm pumping a fist, winking with a big grin, confident happy pose |

Each outfit can also have its own poses, for example `pizza_jump.png`.

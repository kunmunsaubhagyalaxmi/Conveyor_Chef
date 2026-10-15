"""Build the chapter-map UI sprites from the game's existing glossy art."""

from pathlib import Path
from PIL import Image, ImageDraw, ImageFilter
import colorsys
import math

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "Assets/Project Data/Game/Images/CountryMap"
OUT = ROOT / "Assets/Project Data/Game/Resources/PathMapUI"
OUT.mkdir(parents=True, exist_ok=True)


def source(name):
    return Image.open(SOURCE / name).convert("RGBA")


def fit(im, size, margin=0.06):
    bbox = im.getchannel("A").getbbox()
    im = im.crop(bbox)
    w, h = size
    limit = (int(w * (1 - 2 * margin)), int(h * (1 - 2 * margin)))
    im.thumbnail(limit, Image.Resampling.LANCZOS)
    result = Image.new("RGBA", size)
    result.alpha_composite(im, ((w - im.width) // 2, (h - im.height) // 2))
    return result


def save(im, name):
    im.save(OUT / name, optimize=True)


def recolor_blue(im, hue, sat=0.85, value=1.0):
    pixels = im.load()
    for y in range(im.height):
        for x in range(im.width):
            r, g, b, a = pixels[x, y]
            if a < 4:
                continue
            h, s, v = colorsys.rgb_to_hsv(r / 255, g / 255, b / 255)
            if 0.52 < h < 0.73 and s > 0.35:
                rr, gg, bb = colorsys.hsv_to_rgb(hue, min(1, s * sat), min(1, v * value))
                pixels[x, y] = (int(rr * 255), int(gg * 255), int(bb * 255), a)
    return im


def tint_white_rim_gold(im):
    p = im.load()
    cx, cy = im.width / 2, im.height / 2
    for y in range(im.height):
        for x in range(im.width):
            r, g, b, a = p[x, y]
            if a < 10:
                continue
            radius = math.hypot((x - cx) / cx, (y - cy) / cy)
            h, s, v = colorsys.rgb_to_hsv(r / 255, g / 255, b / 255)
            if 0.42 < radius < 0.87 and s < 0.27 and v > 0.48:
                light = min(1, 0.65 + 0.35 * v)
                p[x, y] = (int(255 * light), int((164 + 65 * v) * light), int((32 + 76 * v) * light), a)
    return im


def canvas(size):
    return Image.new("RGBA", size)


def gradient_plate(size, box, colors, radius, outline=None, outline_width=0, shadow=True):
    im = canvas(size)
    if shadow:
        layer = canvas(size)
        d = ImageDraw.Draw(layer)
        d.rounded_rectangle((box[0] + 6, box[1] + 12, box[2] + 6, box[3] + 12), radius, fill=(12, 33, 67, 130))
        im.alpha_composite(layer.filter(ImageFilter.GaussianBlur(14)))
    d = ImageDraw.Draw(im)
    if outline:
        d.rounded_rectangle(box, radius, fill=outline)
        box = (box[0] + outline_width, box[1] + outline_width, box[2] - outline_width, box[3] - outline_width)
    mask = Image.new("L", size)
    ImageDraw.Draw(mask).rounded_rectangle(box, max(1, radius - outline_width), fill=255)
    grad = canvas(size)
    gp = grad.load()
    top, bottom = colors
    for y in range(size[1]):
        t = max(0, min(1, (y - box[1]) / max(1, box[3] - box[1])))
        col = tuple(int(top[i] * (1 - t) + bottom[i] * t) for i in range(3))
        for x in range(box[0], box[2] + 1):
            gp[x, y] = (*col, 255)
    grad.putalpha(mask)
    im.alpha_composite(grad)
    return im


def glossy_line(im, box, radius, inset=12):
    d = ImageDraw.Draw(im)
    x0, y0, x1, y1 = box
    d.arc((x0 + inset, y0 + inset, x1 - inset, y1 - inset), 185, 345, fill=(255, 255, 255, 175), width=max(3, inset // 2))


node = source("glossy_blue_game_map_node.png")
open_node = fit(tint_white_rim_gold(node.copy()), (512, 512), .07)
save(open_node, "node_open.png")
save(fit(recolor_blue(tint_white_rim_gold(node.copy()), .0, .90), (512, 512), .07), "node_hard.png")

locked = source("glossy_blue_locked_level_icon.png")
lp = locked.load()
for y in range(locked.height):
    for x in range(locked.width):
        r, g, b, a = lp[x, y]
        h, s, v = colorsys.rgb_to_hsv(r / 255, g / 255, b / 255)
        if a > 5 and 0.52 < h < 0.73 and s > .35:
            lp[x, y] = (int(r * .32), int(g * .40), int(b * .48), a)
save(fit(locked, (512, 512), .04), "node_locked.png")

glow = source("magical_golden_blue_selection_halo.png")
save(fit(glow, (768, 768), .005), "node_current_glow.png")
star = source("glossy_golden_game_star_icon.png")
save(fit(star, (256, 256), .02), "star_full.png")
grey = star.copy()
pix = grey.load()
for y in range(grey.height):
    for x in range(grey.width):
        r, g, b, a = pix[x, y]
        lum = int(.24 * r + .66 * g + .10 * b)
        pix[x, y] = (int(lum * .83), int(lum * .87), int(lum * .96), int(a * .78))
save(fit(grey, (256, 256), .02), "star_empty.png")

boss = fit(recolor_blue(tint_white_rim_gold(node.copy()), .12, .90), (640, 640), .11)
d = ImageDraw.Draw(boss)
d.polygon([(246, 178), (258, 111), (306, 154), (320, 95), (334, 154), (382, 111), (394, 178)], fill=(126, 57, 12), outline=(69, 35, 10))
d.polygon([(252, 172), (263, 121), (309, 167), (320, 108), (331, 167), (377, 121), (388, 172)], fill=(255, 199, 43))
d.rounded_rectangle((252, 171, 388, 198), 10, fill=(242, 148, 20), outline=(114, 55, 12), width=5)
for px, py in [(262, 126), (320, 113), (378, 126)]:
    d.ellipse((px - 7, py - 7, px + 7, py + 7), fill=(255, 252, 177))
save(boss, "node_boss.png")

dot = canvas((96, 96))
d = ImageDraw.Draw(dot)
d.ellipse((21, 26, 79, 84), fill=(45, 28, 17, 75))
dot = dot.filter(ImageFilter.GaussianBlur(3))
d = ImageDraw.Draw(dot)
d.ellipse((19, 16, 77, 74), fill=(58, 35, 23, 255), outline=(22, 23, 34, 255), width=5)
d.ellipse((30, 24, 53, 34), fill=(148, 116, 82, 125))
save(dot, "path_dot.png")

header = gradient_plate((2000, 440), (26, 56, 1974, 383), ((60, 163, 249), (14, 63, 155)), 105,
                        outline=(109, 54, 19), outline_width=20)
d = ImageDraw.Draw(header)
d.rounded_rectangle((50, 75, 1950, 350), 83, outline=(255, 207, 78), width=18)
d.rounded_rectangle((74, 97, 1926, 326), 67, outline=(212, 240, 255, 180), width=7)
d.arc((105, 107, 1895, 253), 190, 345, fill=(255, 255, 255, 110), width=13)
for x in (96, 1904):
    d.ellipse((x - 16, 205, x + 16, 237), fill=(255, 228, 105), outline=(135, 73, 18), width=4)
save(header, "header_bar.png")

arrow = gradient_plate((320, 320), (25, 25, 294, 294), ((72, 177, 255), (10, 65, 157)), 135,
                       outline=(235, 181, 62), outline_width=15)
d = ImageDraw.Draw(arrow)
d.ellipse((43, 43, 276, 276), outline=(224, 244, 255), width=9)
d.polygon([(123, 93), (211, 160), (123, 227), (123, 190), (87, 190), (87, 131), (123, 131)], fill=(18, 66, 121))
d.polygon([(126, 82), (221, 154), (126, 226), (126, 183), (91, 183), (91, 124), (126, 124)], fill=(255, 255, 244), outline=(171, 223, 250))
save(arrow, "arrow_button.png")

selected = gradient_plate((800, 360), (18, 20, 781, 340), ((255, 255, 229), (255, 223, 171)), 55,
                          outline=(108, 55, 20), outline_width=16)
d = ImageDraw.Draw(selected)
d.rounded_rectangle((36, 37, 762, 318), 40, outline=(255, 194, 56), width=17)
d.arc((65, 58, 735, 190), 190, 350, fill=(255, 255, 255, 180), width=8)
save(selected, "tab_selected.png")

normal = gradient_plate((800, 360), (18, 20, 781, 340), ((65, 115, 169), (24, 48, 93)), 55,
                        outline=(8, 31, 61), outline_width=16)
d = ImageDraw.Draw(normal)
d.rounded_rectangle((36, 37, 762, 318), 40, outline=(100, 158, 203), width=13)
d.arc((65, 58, 735, 190), 190, 350, fill=(188, 229, 255, 115), width=8)
save(normal, "tab_normal.png")

small_lock = canvas((192, 192))
d = ImageDraw.Draw(small_lock)
d.arc((48, 19, 144, 130), 178, 362, fill=(29, 43, 57), width=33)
d.arc((48, 19, 144, 130), 178, 362, fill=(208, 220, 234), width=23)
d.arc((62, 29, 130, 120), 182, 358, fill=(255, 255, 255), width=6)
d.rounded_rectangle((37, 87, 155, 171), 17, fill=(40, 51, 68), outline=(22, 30, 45), width=6)
d.rounded_rectangle((44, 91, 148, 161), 13, fill=(186, 197, 212), outline=(246, 250, 255), width=6)
d.ellipse((85, 111, 107, 133), fill=(45, 58, 75))
d.polygon([(91, 125), (101, 125), (107, 146), (85, 146)], fill=(45, 58, 75))
save(small_lock, "lock_small.png")

# Optional pieces for the chapter screen.
save(fit(recolor_blue(tint_white_rim_gold(node.copy()), .32, .86), (512, 512), .07), "node_done.png")

tab_back = gradient_plate((2160, 440), (0, 32, 2159, 439), ((25, 57, 110), (9, 27, 65)), 10,
                          outline=(13, 32, 65), outline_width=4, shadow=False)
td = ImageDraw.Draw(tab_back)
td.rectangle((0, 32, 2159, 53), fill=(116, 57, 15))
td.rectangle((0, 38, 2159, 47), fill=(255, 197, 61))
save(tab_back, "tab_bar_back.png")

pin = canvas((400, 480))
pd = ImageDraw.Draw(pin)
pd.ellipse((57, 25, 343, 310), fill=(72, 35, 14))
pd.polygon([(130, 260), (270, 260), (200, 444)], fill=(72, 35, 14))
pd.ellipse((68, 37, 332, 300), fill=(255, 201, 55), outline=(255, 239, 158), width=9)
pd.polygon([(139, 260), (261, 260), (200, 425)], fill=(255, 188, 42))
pd.ellipse((89, 58, 311, 280), fill=(42, 112, 207), outline=(250, 238, 199), width=10)
pd.ellipse((145, 116, 255, 247), fill=(255, 216, 174))
pd.ellipse((155, 192, 180, 202), fill=(74, 49, 42))
pd.ellipse((220, 192, 245, 202), fill=(74, 49, 42))
pd.arc((175, 203, 225, 240), 10, 170, fill=(160, 65, 53), width=5)
pd.rounded_rectangle((139, 135, 261, 161), 12, fill=(244, 250, 255), outline=(169, 196, 218), width=4)
for cx, cy, rad in [(163, 121, 32), (201, 109, 38), (238, 121, 32)]:
    pd.ellipse((cx-rad, cy-rad, cx+rad, cy+rad), fill=(248, 252, 255), outline=(180, 206, 227), width=3)
save(pin, "chef_pin.png")

play = gradient_plate((520, 220), (18, 20, 501, 201), ((90, 228, 86), (22, 135, 49)), 90,
                      outline=(29, 81, 30), outline_width=10)
pd = ImageDraw.Draw(play)
pd.rounded_rectangle((36, 37, 483, 183), 70, outline=(245, 255, 227), width=11)
pd.arc((61, 49, 459, 137), 190, 350, fill=(255, 255, 255, 145), width=7)
save(play, "play_button.png")

print(f"Saved {len(list(OUT.glob('*.png')))} UI sprites to {OUT}")

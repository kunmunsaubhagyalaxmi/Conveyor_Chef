"""Builds Resources/Chef/<outfit>_hud.png: each outfit's avatar inside the classic gold square.

The frame comes from the classic HUD picture (Resources/MainMenu/avatar.png, 443x512): its gold
square is whole on the left, right and bottom, so the top edge (hidden behind the classic hat) is
the bottom edge mirrored, and the blue rays behind the chef are rebuilt from the blue that shows
around him. Like the classic one, the face fills the square and the hat pops out over the top.

Run from the project root:  python Tools/make_chef_hud_avatars.py
"""
import math
import os
from PIL import Image, ImageDraw, ImageFilter

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
RES = os.path.join(ROOT, "Assets/Project Data/Game/Resources")
OUTFITS = ["street_food", "pastry", "sushi", "pizza", "bbq", "royal", "golden"]

# the classic square, measured on avatar.png (outer edge, ring thickness, corner radii)
OUTER = (6, 95, 437, 508)
RING = 33
R_OUT, R_IN = 84, 52
FACE_X, FACE_Y = 222, 300   # centre of the classic face
SCALE = 1.15                # the outfit avatars are drawn at the classic face size


def is_gold(p):
    r, g, b, a = p
    return a > 160 and r > 170 and g > 105 and b < 150 and r - b > 80


def rounded_mask(size, box, radius, ss=4):
    W, H = size
    m = Image.new("L", (W * ss, H * ss), 0)
    ImageDraw.Draw(m).rounded_rectangle([c * ss for c in box], radius * ss, fill=255)
    return m.resize((W, H), Image.LANCZOS)


def build_frame(src):
    W, H = src.size
    px = src.load()
    outer = rounded_mask((W, H), OUTER, R_OUT)
    x0, y0, x1, y1 = OUTER
    inner_box = (x0 + RING, y0 + RING, x1 - RING, y1 - RING)
    inner = rounded_mask((W, H), inner_box, R_IN)
    ring_mask = Image.composite(Image.new("L", (W, H), 0), outer, inner.point(lambda v: 255 if v > 127 else 0))

    ring = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    rp = ring.load()
    mirror = lambda y: y0 + y1 - y
    for y in range(H):
        for x in range(W):
            if ring_mask.getpixel((x, y)) == 0:
                continue
            p = px[x, y]
            if not is_gold(p):
                my = mirror(y)
                if 0 <= my < H and is_gold(px[x, my]):
                    p = px[x, my]
                else:  # look a little further along the ring for a visible gold pixel
                    for d in range(1, 12):
                        for xx in (x - d, x + d):
                            if 0 <= xx < W and 0 <= my < H and is_gold(px[xx, my]):
                                p = px[xx, my]
                                break
                        else:
                            continue
                        break
            rp[x, y] = (p[0], p[1], p[2], 255)
    ring.putalpha(ring_mask)
    return ring, inner, outer


def build_rays(src, inner):
    """Blue rays behind the chef, rebuilt from the blue that shows around the classic chef."""
    W, H = src.size
    px = src.load()
    cx, cy = W * 0.62, H * 0.55
    A, R = 72, 16
    maxr = 330.0
    sums = [[[0, 0, 0, 0] for _ in range(R)] for _ in range(A)]
    rad = [[0, 0, 0, 0] for _ in range(R)]
    for y in range(H):
        for x in range(W):
            r_, g_, b_, a_ = px[x, y]
            if not (a_ > 200 and b_ > 150 and b_ - r_ > 50):
                continue
            a = int((math.atan2(y - cy, x - cx) + math.pi) / (2 * math.pi) * A) % A
            r = min(R - 1, int(math.hypot(x - cx, y - cy) / maxr * R))
            for s in (sums[a][r], rad[r]):
                s[0] += r_; s[1] += g_; s[2] += b_; s[3] += 1

    def radial(r):
        for d in range(R):
            for rr in (r - d, r + d):
                if 0 <= rr < R and rad[rr][3]:
                    s = rad[rr]
                    return (s[0] / s[3], s[1] / s[3], s[2] / s[3])
        return (40, 140, 240)

    out = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    op = out.load()
    for y in range(H):
        for x in range(W):
            ang = math.atan2(y - cy, x - cx)
            r = min(R - 1, int(math.hypot(x - cx, y - cy) / maxr * R))
            col = radial(r)
            ray = 1.0 + 0.07 * math.cos(ang * 18)
            op[x, y] = tuple(max(0, min(255, int(c * ray))) for c in col) + (255,)
    out = out.filter(ImageFilter.GaussianBlur(1.5))
    out.putalpha(inner)
    return out


def face_centre(chef):
    """Centre of the face: median of the skin pixels in the middle of the picture
    (props such as flames or a gold jacket stay out of this area)."""
    W, H = chef.size
    px = chef.load()
    xs, ys = [], []
    for y in range(int(H * 0.2), int(H * 0.6)):
        for x in range(int(W * 0.25), int(W * 0.75)):
            r, g, b, a = px[x, y]
            if a > 200 and r > 190 and 120 < g < 215 and 90 < b < 185 and r > g > b and r - b > 45:
                xs.append(x); ys.append(y)
    xs.sort(); ys.sort()
    return xs[len(xs) // 2], ys[len(ys) // 2]


def main():
    src = Image.open(os.path.join(RES, "MainMenu/avatar.png")).convert("RGBA")
    W, H = src.size
    ring, inner, outer = build_frame(src)
    rays = build_rays(src, inner)
    x0, y0, x1, y1 = OUTER
    for oid in OUTFITS:
        chef = Image.open(os.path.join(RES, f"Chef/{oid}_avatar.png")).convert("RGBA")
        fx, fy = face_centre(chef)
        s = SCALE
        chef = chef.resize((round(chef.width * s), round(chef.height * s)), Image.LANCZOS)
        layer = Image.new("RGBA", (W, H), (0, 0, 0, 0))
        layer.alpha_composite(chef, (round(FACE_X - fx * s), round(FACE_Y - fy * s)))
        # the chef stays inside the square at the sides and bottom; above it the hat is free
        clip = outer.copy()
        ImageDraw.Draw(clip).rectangle([0, 0, W, y0 + RING + 10], fill=255)
        clip = Image.composite(clip, Image.new("L", (W, H), 0), clip)
        layer.putalpha(Image.composite(layer.getchannel("A"), Image.new("L", (W, H), 0), clip))

        out = Image.new("RGBA", (W, H), (0, 0, 0, 0))
        out.alpha_composite(rays)
        top = ring.copy()
        ImageDraw.Draw(top).rectangle([0, y0 + R_OUT, W, H], fill=(0, 0, 0, 0))
        out.alpha_composite(top)            # top edge behind the chef: the hat overlaps it
        out.alpha_composite(layer)
        sides = ring.copy()
        ImageDraw.Draw(sides).rectangle([0, 0, W, y0 + R_OUT - 1], fill=(0, 0, 0, 0))
        out.alpha_composite(sides)          # sides and bottom in front of him
        out.save(os.path.join(RES, f"Chef/{oid}_hud.png"), optimize=True)
        print("wrote", oid + "_hud.png", "scale %.2f" % s)


if __name__ == "__main__":
    main()

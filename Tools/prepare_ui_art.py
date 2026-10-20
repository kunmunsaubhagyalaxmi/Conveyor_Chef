"""Prepares the text-free UI art (see Docs/UIArt/ui_art_master_prompts.md) for the game:

* trims empty transparent borders from buttons, ribbons and the bottom-bar plaque (a few px
  of margin kept) so every piece lines up exactly where the game puts it;
* puts each icon centred on a square canvas, so all icons sit the same way on buttons;
* writes Unity import settings: Sprite, no mipmaps, and a max size that fits how big the
  picture is shown (keeps the build small).

Run from the project root after adding or replacing UI art:  python Tools/prepare_ui_art.py
Existing .meta files keep their GUID (references stay), only the settings are updated.
"""
import glob
import os
import re
import uuid

from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
GAME = os.path.join(ROOT, "Assets", "Project Data", "Game")

# (pattern, trim mode, max texture size)
ART = [
    ("Resources/UI/Buttons/pill_*.png", "trim", 1024),
    ("Resources/UI/Buttons/small_*.png", "trim", 512),
    ("Resources/UI/Icons/icon_*.png", "square", 256),
    ("Resources/UI/Panels/title_ribbon_blue.png", "trim", 1024),
    ("Resources/UI/Panels/country_title_blank.png", "trim", 1024),
    ("Resources/UI/Panels/next_country_blank.png", "trim", 1024),
    ("Resources/UI/Panels/level_completed_blank.png", None, 2048),
    ("Resources/UI/Panels/level_failed_blank.png", None, 2048),
    ("Resources/MenuPanelFrames/blank.png", None, 2048),
    ("Resources/MainMenu/nav_*.png", None, 512),
    ("Resources/MainMenu/nav_plaque.png", "trim", 512),
]

META = """fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: {size}
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 0
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: 100
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 0
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: {size}
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 2
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    customData:
    physicsShape: []
    bones: []
    spriteID: {spriteid}
    internalID: 0
    vertices: []
    indices:
    edges: []
    weights: []
    secondaryTextures: []
    spriteCustomMetadata:
      entries: []
    nameFileIdTable: {{}}
  mipmapLimitGroupName:
  pSDRemoveMatte: 0
  userData:
  assetBundleName:
  assetBundleVariant:
"""


def trim(path, pad=6):
    im = Image.open(path).convert("RGBA")
    box = im.getchannel("A").point(lambda v: 255 if v > 8 else 0).getbbox()
    if not box:
        return
    w, h = im.size
    box = (max(0, box[0] - pad), max(0, box[1] - pad), min(w, box[2] + pad), min(h, box[3] + pad))
    if box != (0, 0, w, h):
        im.crop(box).save(path, optimize=True)


def square(path, pad=0.06):
    im = Image.open(path).convert("RGBA")
    box = im.getchannel("A").point(lambda v: 255 if v > 8 else 0).getbbox()
    if not box:
        return
    c = im.crop(box)
    side = int(max(c.size) * (1 + 2 * pad))
    out = Image.new("RGBA", (side, side), (0, 0, 0, 0))
    out.alpha_composite(c, ((side - c.width) // 2, (side - c.height) // 2))
    if out.size != im.size or box != (0, 0) + im.size:
        out.save(path, optimize=True)


def write_meta(path, size):
    meta = path + ".meta"
    if os.path.exists(meta):
        t = open(meta, encoding="utf-8").read()
        t = re.sub(r"(\n  maxTextureSize: )\d+", r"\g<1>%d" % size, t, count=1)
        t = re.sub(r"(buildTarget: DefaultTexturePlatform\n\s+maxTextureSize: )\d+", r"\g<1>%d" % size, t)
        t = re.sub(r"textureType: \d+", "textureType: 8", t, count=1)
        t = re.sub(r"enableMipMap: \d", "enableMipMap: 0", t, count=1)
        open(meta, "w", encoding="utf-8", newline="\n").write(t)
        return
    open(meta, "w", encoding="utf-8", newline="\n").write(
        META.format(guid=uuid.uuid4().hex, size=size, spriteid=uuid.uuid4().hex))


def folder_meta(folder):
    meta = folder.rstrip("/\\") + ".meta"
    if not os.path.exists(meta):
        open(meta, "w", encoding="utf-8", newline="\n").write(
            "fileFormatVersion: 2\nguid: %s\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n"
            "  userData: \n  assetBundleName: \n  assetBundleVariant: \n" % uuid.uuid4().hex)


def main():
    for folder in ("Resources/UI/Buttons", "Resources/UI/Icons", "Resources/UI/Panels"):
        folder_meta(os.path.join(GAME, folder))
    done = set()
    for pattern, mode, size in ART:
        for path in sorted(glob.glob(os.path.join(GAME, pattern))):
            if mode == "trim":
                trim(path)
            elif mode == "square":
                square(path)
            write_meta(path, size)
            done.add(path)
            print("%-55s %s" % (os.path.relpath(path, GAME).replace(os.sep, "/"), Image.open(path).size))
    print(len(done), "pictures ready")


if __name__ == "__main__":
    main()

using UnityEditor;

namespace Watermelon
{
    /// <summary>
    /// Dish pictures dropped into Resources/World/&lt;continent&gt;/&lt;country&gt;/ as dish_&lt;colour&gt;.png
    /// import like the existing dishes: sprite, no mipmaps, at most 512 px, high quality
    /// compression (ASTC 5x5 on Android), transparent background. Without this a new picture would import as a plain
    /// texture and the game could not load it as a dish.
    /// </summary>
    public sealed class DishArtImporter : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            string path = assetPath.Replace("\\", "/");
            if (!path.Contains("/Resources/World/"))
                return;
            string file = System.IO.Path.GetFileNameWithoutExtension(path);
            if (file == "levelselect_icon")
            {
                ImportCountryIcon((TextureImporter)assetImporter);
                return;
            }
            if (!file.StartsWith("dish_"))
                return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.maxTextureSize = 512;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;

            // Android: ASTC 5x5 like the other dishes (they are drawn smaller than 384 px, so
            // the 4x4 block of high quality only made the APK bigger)
            TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings("Android");
            android.overridden = true;
            android.maxTextureSize = 512;
            android.format = TextureImporterFormat.ASTC_5x5;
            importer.SetPlatformTextureSettings(android);
        }

        // The country's map icon in the round emblem by COUNTRY PROGRESS on Level Selection
        // (levelselect_icon.png): a sprite drawn about 120 px, so 256 px is plenty.
        private static void ImportCountryIcon(TextureImporter importer)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.maxTextureSize = 256;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;

            TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings("Android");
            android.overridden = true;
            android.maxTextureSize = 256;
            android.format = TextureImporterFormat.ASTC_5x5;
            importer.SetPlatformTextureSettings(android);
        }
    }
}

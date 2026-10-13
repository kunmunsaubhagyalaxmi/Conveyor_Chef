using UnityEditor;

namespace Watermelon
{
    /// <summary>
    /// Dish pictures dropped into Resources/World/&lt;continent&gt;/&lt;country&gt;/ as dish_&lt;colour&gt;.png
    /// import like the existing dishes: sprite, no mipmaps, at most 512 px, high quality
    /// compression, transparent background. Without this a new picture would import as a plain
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
            if (!file.StartsWith("dish_"))
                return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.maxTextureSize = 512;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
        }
    }
}

using System.IO;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools.Import
{
    /// <summary>
    /// Applies the project-wide pixel-art import settings to every PNG under Assets/_Project/Art
    /// and slices sheets per <see cref="SlicePlanner"/>. Re-import an asset to re-apply.
    /// </summary>
    public sealed class ArtPostprocessor : AssetPostprocessor
    {
        public const string ArtRoot = "Assets/_Project/Art/";
        public const int PixelsPerUnit = 32;
        private const int MaxTextureSize = 4096;
        private const int PngHeaderBytes = 24;

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(ArtRoot) || !assetPath.EndsWith(".png")) return;
            var importer = (TextureImporter)assetImporter;
            if (!TryReadPngSize(assetPath, out int w, out int h))
            {
                Debug.LogError($"ArtPostprocessor: cannot read PNG size of {assetPath}; settings not applied.");
                return;
            }
            ApplyPixelSettings(importer);
            ApplySlicing(importer, w, h);
        }

        private static void ApplyPixelSettings(TextureImporter importer)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.maxTextureSize = MaxTextureSize;
        }

        private void ApplySlicing(TextureImporter importer, int w, int h)
        {
            string file = Path.GetFileNameWithoutExtension(assetPath);
            SlicePlan plan = SlicePlanner.Plan(assetPath, w, h);
            bool bottom = SlicePlanner.UsesBottomPivot(assetPath) || plan.Mode == SliceMode.Auto;
            Vector2 pivot = bottom ? new Vector2(0.5f, 0f) : new Vector2(0.5f, 0.5f);
            if (plan.Mode == SliceMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                SetSinglePivot(importer, pivot);
                return;
            }
            importer.spriteImportMode = SpriteImportMode.Multiple;
#pragma warning disable CS0618
            importer.spritesheet = plan.Mode == SliceMode.Grid
                ? SpriteSheetBuilder.Grid(file, w, h, plan.CellWidth, plan.CellHeight, pivot)
                : AutoSlice(file, pivot);
#pragma warning restore CS0618
        }

        private SpriteMetaData[] AutoSlice(string file, Vector2 pivot)
        {
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.LoadImage(File.ReadAllBytes(assetPath));
            SpriteMetaData[] result = SpriteSheetBuilder.Auto(file, tex, pivot);
            Object.DestroyImmediate(tex);
            return result;
        }

        private static void SetSinglePivot(TextureImporter importer, Vector2 pivot)
        {
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = pivot;
            importer.SetTextureSettings(settings);
        }

        private static bool TryReadPngSize(string path, out int w, out int h)
        {
            w = 0;
            h = 0;
            var head = new byte[PngHeaderBytes];
            using (FileStream s = File.OpenRead(path))
            {
                if (s.Read(head, 0, PngHeaderBytes) < PngHeaderBytes) return false;
            }
            w = (head[16] << 24) | (head[17] << 16) | (head[18] << 8) | head[19];
            h = (head[20] << 24) | (head[21] << 16) | (head[22] << 8) | head[23];
            return w > 0 && h > 0;
        }
    }
}

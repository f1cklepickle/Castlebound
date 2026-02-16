using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Castlebound.Tests.Scale
{
    public class PpuImportBaselineTests
    {
        private static readonly string[] ScopedFolders =
        {
            "Assets/_Project/Art/Castle_Assets",
            "Assets/_Project/Art/Barrier_Assets",
            "Assets/_Project/Art/Goblin_Assets",
            "Assets/_Project/Art/Knight_Assets",
            "Assets/_Project/Art/UI",
        };

        [Test]
        public void AppliesPpu32_AcrossArtScope()
        {
            var violations = new List<string>();

            foreach (var importer in EnumerateTextureImporters())
            {
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    continue;
                }

                if (!Mathf.Approximately(importer.spritePixelsPerUnit, 32f))
                {
                    violations.Add($"{importer.assetPath} (PPU={importer.spritePixelsPerUnit})");
                }
            }

            AssertNoViolations("Expected sprite PPU=32 across scoped art folders.", violations);
        }

        [Test]
        public void UsesPointFilter_AcrossArtScope()
        {
            var violations = new List<string>();

            foreach (var importer in EnumerateTextureImporters())
            {
                if (importer.filterMode != FilterMode.Point)
                {
                    violations.Add($"{importer.assetPath} (Filter={importer.filterMode})");
                }
            }

            AssertNoViolations("Expected Point filter across scoped art folders.", violations);
        }

        [Test]
        public void DisablesCompression_AcrossArtScope()
        {
            var violations = new List<string>();

            foreach (var importer in EnumerateTextureImporters())
            {
                if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                {
                    violations.Add($"{importer.assetPath} (Compression={importer.textureCompression})");
                }
            }

            AssertNoViolations("Expected uncompressed textures across scoped art folders.", violations);
        }

        [Test]
        public void DisablesMipMaps_ForSpriteArtScope()
        {
            var violations = new List<string>();

            foreach (var importer in EnumerateTextureImporters())
            {
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    continue;
                }

                if (importer.mipmapEnabled)
                {
                    violations.Add($"{importer.assetPath} (MipMaps enabled)");
                }
            }

            AssertNoViolations("Expected mipmaps disabled for sprite textures across scoped art folders.", violations);
        }

        private static IEnumerable<TextureImporter> EnumerateTextureImporters()
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D", ScopedFolders);
            var paths = guids.Select(AssetDatabase.GUIDToAssetPath).Distinct();

            foreach (var path in paths)
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                {
                    continue;
                }

                yield return importer;
            }
        }

        private static void AssertNoViolations(string header, List<string> violations)
        {
            if (violations.Count == 0)
            {
                return;
            }

            var message = header + "\n" + string.Join("\n", violations);
            Assert.Fail(message);
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Castlebound.Tests.Scale
{
    public class CastleTilesetImportBaselineTests
    {
        private static readonly string[] ScopedFolders =
        {
            "Assets/_Project/Art/Castle_Assets"
        };

        [Test]
        public void CastleSprites_UsePpu32()
        {
            var violations = new List<string>();

            foreach (var importer in EnumerateTextureImporters())
            {
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    violations.Add($"{importer.assetPath} (TextureType={importer.textureType}, expected Sprite)");
                    continue;
                }

                if (!Mathf.Approximately(importer.spritePixelsPerUnit, 32f))
                {
                    violations.Add($"{importer.assetPath} (PPU={importer.spritePixelsPerUnit})");
                }
            }

            AssertNoViolations("Expected castle tileset sprites to use PPU=32.", violations);
        }

        [Test]
        public void CastleSprites_UsePointFilter()
        {
            var violations = new List<string>();

            foreach (var importer in EnumerateTextureImporters())
            {
                if (importer.filterMode != FilterMode.Point)
                {
                    violations.Add($"{importer.assetPath} (Filter={importer.filterMode})");
                }
            }

            AssertNoViolations("Expected Point filter for castle tileset textures.", violations);
        }

        [Test]
        public void CastleSprites_AreUncompressed()
        {
            var violations = new List<string>();

            foreach (var importer in EnumerateTextureImporters())
            {
                if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                {
                    violations.Add($"{importer.assetPath} (Compression={importer.textureCompression})");
                }
            }

            AssertNoViolations("Expected uncompressed castle tileset textures.", violations);
        }

        [Test]
        public void CastleSprites_DisableMipMaps()
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

            AssertNoViolations("Expected mipmaps disabled for castle tileset sprites.", violations);
        }

        [Test]
        public void CastleSprites_UseCenterPivotConsistency()
        {
            var violations = new List<string>();

            foreach (var importer in EnumerateTextureImporters())
            {
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    continue;
                }

                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                var alignment = (SpriteAlignment)settings.spriteAlignment;
                if (alignment == SpriteAlignment.Center)
                {
                    continue;
                }

                if (alignment == SpriteAlignment.Custom)
                {
                    var p = importer.spritePivot;
                    if (Mathf.Approximately(p.x, 0.5f) && Mathf.Approximately(p.y, 0.5f))
                    {
                        continue;
                    }
                }

                violations.Add($"{importer.assetPath} (Alignment={alignment}, Pivot={importer.spritePivot})");
            }

            AssertNoViolations("Expected center pivot consistency for castle tileset sprites.", violations);
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

            Assert.Fail(header + "\n" + string.Join("\n", violations));
        }
    }
}

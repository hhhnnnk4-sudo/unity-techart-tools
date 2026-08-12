using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Hhnnnk4.TechArtTools.Editor
{
    /// <summary>
    /// One-click batch operations that apply import changes to the current selection.
    /// Every operation shows a progress bar and works non-destructively.
    /// </summary>
    public static class TechArtBatchTools
    {
        private const int BatchMaxTextureSize = 2048;

        [MenuItem("Tools/TechArt Tools/Batch/Textures/Disable Read-Write (Selection)")]
        public static void TexturesDisableReadWrite()
        {
            RunBatchImporters("Disable Read-Write", GetSelectedImporters<TextureImporter>(),
                importer => importer.isReadable = false);
        }

        [MenuItem("Tools/TechArt Tools/Batch/Textures/Disable Read-Write (Selection)", validate = true)]
        private static bool ValidateTextures() => GetSelectedImporters<TextureImporter>().Count > 0;

        [MenuItem("Tools/TechArt Tools/Batch/Textures/Enable Mipmaps (Selection)")]
        public static void TexturesEnableMipmaps()
        {
            RunBatchImporters("Enable Mipmaps", GetSelectedImporters<TextureImporter>(),
                importer => importer.mipmapEnabled = true);
        }

        [MenuItem("Tools/TechArt Tools/Batch/Textures/Cap Max Size to 2048 (Selection)")]
        public static void TexturesCapMaxSize()
        {
            RunBatchImporters("Cap Max Size to 2048", GetSelectedImporters<TextureImporter>(),
                importer => importer.maxTextureSize = Mathf.Min(importer.maxTextureSize, BatchMaxTextureSize));
        }

        [MenuItem("Tools/TechArt Tools/Batch/Textures/Set Android ASTC 6x6 (Selection)")]
        public static void TexturesSetAndroidAstc()
        {
            RunBatchImporters("Set Android ASTC 6x6", GetSelectedImporters<TextureImporter>(), importer =>
            {
                var settings = new TextureImporterPlatformSettings
                {
                    name = "Android",
                    overridden = true,
                    maxTextureSize = importer.maxTextureSize,
                    format = TextureImporterFormat.ASTC_6x6,
                    compressionQuality = (int)TextureCompressionQuality.Normal
                };
                importer.SetPlatformTextureSettings(settings);
            });
        }

        [MenuItem("Tools/TechArt Tools/Batch/Meshes/Disable Read-Write (Selection)")]
        public static void MeshesDisableReadWrite()
        {
            RunBatchImporters("Disable Mesh Read-Write", GetSelectedImporters<ModelImporter>(),
                importer => importer.isReadable = false);
        }

        [MenuItem("Tools/TechArt Tools/Batch/Meshes/Disable Read-Write (Selection)", validate = true)]
        private static bool ValidateMeshes() => GetSelectedImporters<ModelImporter>().Count > 0;

        [MenuItem("Tools/TechArt Tools/Batch/Materials/Clear Stale Keywords (Selection)")]
        public static void MaterialsClearStaleKeywords()
        {
            var materials = Selection.GetFiltered<Material>(SelectionMode.Assets);
            RunBatch("Clear Stale Keywords", materials, material =>
            {
                if (material.shader == null) return;
                var stale = TechArtMaterialAuditor.GetStaleKeywords(material, material.shader);
                if (stale.Count == 0) return;

                material.shaderKeywords = material.shaderKeywords
                    .Where(keyword => !stale.Contains(keyword))
                    .ToArray();
                EditorUtility.SetDirty(material);
            });
        }

        [MenuItem("Tools/TechArt Tools/Batch/Materials/Clear Stale Keywords (Selection)", validate = true)]
        private static bool ValidateMaterials() => Selection.GetFiltered<Material>(SelectionMode.Assets).Length > 0;

        private static List<TImporter> GetSelectedImporters<TImporter>()
            where TImporter : AssetImporter
        {
            var result = new List<TImporter>();
            foreach (var obj in Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Assets))
            {
                var path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path)) continue;

                if (AssetImporter.GetAtPath(path) is TImporter importer)
                {
                    result.Add(importer);
                }
            }

            return result;
        }

        private static void RunBatchImporters<TImporter>(string title, List<TImporter> importers, Action<TImporter> action)
            where TImporter : AssetImporter
        {
            RunBatch(title, importers, action);
            AssetDatabase.SaveAssets();
        }

        private static void RunBatch<T>(string title, IEnumerable<T> items, Action<T> action)
        {
            var list = items as IReadOnlyList<T> ?? items.ToList();
            if (list.Count == 0)
            {
                EditorUtility.DisplayDialog("TechArt Batch", "Nothing selected.", "OK");
                return;
            }

            AssetDatabase.StartAssetEditing();
            try
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (EditorUtility.DisplayCancelableProgressBar(
                            $"TechArt Batch: {title}",
                            $"{i + 1}/{list.Count}",
                            (float)i / list.Count))
                    {
                        break;
                    }

                    try
                    {
                        action(list[i]);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
            }

            EditorUtility.DisplayDialog("TechArt Batch", $"Done: {title} ({list.Count} item(s)).", "OK");
        }
    }
}

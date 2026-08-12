using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hhnnnk4.TechArtTools.Editor
{
    /// <summary>
    /// Orchestrates asset and scene audits and collects the resulting issues.
    /// </summary>
    public static class TechArtAudit
    {
        public static event Action<string> OnProgress;

        public static List<TechArtIssue> RunAssets(TechArtAuditConfig config, IEnumerable<string> paths)
        {
            var issues = new List<TechArtIssue>();
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var path in paths)
                {
                    if (string.IsNullOrEmpty(path)) continue;
                    OnProgress?.Invoke(path);

                    if (!IsAuditableExtension(path)) continue;

                    var mainType = AssetDatabase.GetMainAssetTypeAtPath(path);
                    if (mainType == typeof(Texture2D))
                    {
                        TechArtTextureAuditor.Audit(path, config, issues);
                    }
                    else if (mainType == typeof(Material))
                    {
                        TechArtMaterialAuditor.Audit(path, config, issues);
                    }
                    else if (AssetDatabase.LoadAssetAtPath<Mesh>(path) != null)
                    {
                        TechArtMeshAuditor.Audit(path, config, issues);
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }

            return issues;
        }

        public static List<TechArtIssue> RunScenes(TechArtAuditConfig config)
        {
            var issues = new List<TechArtIssue>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                TechArtSceneAuditor.Audit(scene, config, issues);
            }

            return issues;
        }

        private static readonly HashSet<string> AuditableExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg", ".tga", ".psd", ".tif", ".tiff", ".exr", ".hdr", ".gif", ".bmp",
            ".mat", ".fbx", ".obj", ".dae", ".blend", ".asset"
        };

        private static bool IsAuditableExtension(string path)
        {
            return AuditableExtensions.Contains(Path.GetExtension(path));
        }
    }
}

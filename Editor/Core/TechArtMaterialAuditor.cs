using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Hhnnnk4.TechArtTools.Editor
{
    public static class TechArtMaterialAuditor
    {
        public static void Audit(string path, TechArtAuditConfig config, List<TechArtIssue> issues)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) return;

            var name = Path.GetFileName(path);

            if (mat.shader == null)
            {
                issues.Add(new TechArtIssue(
                    TechArtIssueCategory.Material,
                    TechArtIssueSeverity.Error,
                    "Missing shader",
                    $"'{name}' has no shader assigned.",
                    mat));
                return;
            }

            var shader = mat.shader;

            if (config.CheckShaderKeywords && mat.shaderKeywords != null && mat.shaderKeywords.Length > config.MaxShaderKeywords)
            {
                issues.Add(new TechArtIssue(
                    TechArtIssueCategory.Material,
                    TechArtIssueSeverity.Warning,
                    "Too many shader keywords",
                    $"'{name}' enables {mat.shaderKeywords.Length} keywords (threshold: {config.MaxShaderKeywords}). Keyword bloat increases shader variants and memory.",
                    mat));
            }

            if (config.CheckStaleKeywords)
            {
                var stale = GetStaleKeywords(mat, shader);
                if (stale.Count > 0)
                {
                    issues.Add(new TechArtIssue(
                        TechArtIssueCategory.Material,
                        TechArtIssueSeverity.Warning,
                        "Stale shader keywords",
                        $"'{name}' enables keywords that do not exist in '{shader.name}': {string.Join(", ", stale)}.",
                        mat,
                        () =>
                        {
                            var allowed = GetAllowedKeywords(shader);
                            mat.shaderKeywords = Array.FindAll(mat.shaderKeywords, allowed.Contains);
                            EditorUtility.SetDirty(mat);
                            AssetDatabase.SaveAssetIfDirty(mat);
                        }));
                }
            }

            if (config.WarnOnMissingTextures)
            {
                int count = ShaderUtil.GetPropertyCount(shader);
                for (int i = 0; i < count; i++)
                {
                    if (ShaderUtil.GetPropertyType(shader, i) != ShaderUtil.ShaderPropertyType.Texture) continue;
                    var propertyName = ShaderUtil.GetPropertyName(shader, i);
                    if (!IsMeaningfulTextureProperty(propertyName)) continue;
                    if (mat.GetTexture(propertyName) == null)
                    {
                        issues.Add(new TechArtIssue(
                            TechArtIssueCategory.Material,
                            TechArtIssueSeverity.Info,
                            "Unassigned texture",
                            $"Texture property '{propertyName}' is not assigned on '{name}'.",
                            mat));
                    }
                }
            }
        }

        public static List<string> GetStaleKeywords(Material mat, Shader shader)
        {
            var result = new List<string>();
            if (shader == null) return result;

            var allowed = GetAllowedKeywords(shader);
            foreach (var keyword in mat.shaderKeywords)
            {
                if (!allowed.Contains(keyword)) result.Add(keyword);
            }

            return result;
        }

        private static HashSet<string> GetAllowedKeywords(Shader shader)
        {
            var allowed = new HashSet<string>(StringComparer.Ordinal);
            foreach (var k in ShaderUtil.GetShaderGlobalKeywords(shader)) allowed.Add(k);
            foreach (var k in ShaderUtil.GetShaderLocalKeywords(shader)) allowed.Add(k);
            return allowed;
        }

        private static bool IsMeaningfulTextureProperty(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            var lower = name.ToLowerInvariant();
            if (lower.Contains("none")) return false;
            return lower.Contains("map") || lower.Contains("tex") || lower.Contains("normal") || lower.Contains("bump");
        }
    }
}

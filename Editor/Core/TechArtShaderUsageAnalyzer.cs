using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace Hhnnnk4.TechArtTools.Editor
{
    public class TechArtShaderUsageInfo
    {
        public string ShaderName;
        public List<string> MaterialPaths = new List<string>();
        public bool IsBuiltIn;

        public int MaterialCount => MaterialPaths.Count;
    }

    /// <summary>
    /// Aggregates material usage per shader, and flags built-in pipeline shaders
    /// that are typically candidates for migration to URP/HDRP.
    /// </summary>
    public static class TechArtShaderUsageAnalyzer
    {
        public static List<TechArtShaderUsageInfo> Analyze(IEnumerable<string> materialPaths)
        {
            var map = new Dictionary<string, TechArtShaderUsageInfo>(StringComparer.Ordinal);

            foreach (var path in materialPaths)
            {
                if (string.IsNullOrEmpty(path)) continue;
                if (!string.Equals(Path.GetExtension(path), ".mat", StringComparison.OrdinalIgnoreCase)) continue;

                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null || material.shader == null) continue;

                var shaderName = material.shader.name;
                if (!map.TryGetValue(shaderName, out var info))
                {
                    info = new TechArtShaderUsageInfo
                    {
                        ShaderName = shaderName,
                        IsBuiltIn = IsBuiltInShader(shaderName)
                    };
                    map[shaderName] = info;
                }

                info.MaterialPaths.Add(path);
            }

            var result = new List<TechArtShaderUsageInfo>(map.Values);
            result.Sort((a, b) => b.MaterialCount.CompareTo(a.MaterialCount));
            return result;
        }

        public static bool IsBuiltInShader(string shaderName)
        {
            if (string.IsNullOrEmpty(shaderName)) return false;

            if (shaderName.StartsWith("Universal Render Pipeline/", StringComparison.Ordinal)) return false;
            if (shaderName.StartsWith("HDRP/", StringComparison.Ordinal)) return false;
            if (shaderName.StartsWith("Shader Graphs/", StringComparison.Ordinal)) return false;
            if (shaderName.StartsWith("Hidden/", StringComparison.Ordinal)) return false;

            return shaderName == "Standard"
                   || shaderName.StartsWith("Legacy Shaders/", StringComparison.Ordinal)
                   || shaderName.StartsWith("Sprites/", StringComparison.Ordinal)
                   || shaderName.StartsWith("UI/", StringComparison.Ordinal)
                   || shaderName.StartsWith("Particles/", StringComparison.Ordinal)
                   || shaderName.StartsWith("FX/", StringComparison.Ordinal)
                   || shaderName.StartsWith("Skybox/", StringComparison.Ordinal)
                   || shaderName.StartsWith("Nature/", StringComparison.Ordinal);
        }
    }
}

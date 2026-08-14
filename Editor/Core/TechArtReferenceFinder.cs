using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Hhnnnk4.TechArtTools.Editor
{
    /// <summary>
    /// Finds which project files reference a given asset by scanning serialized
    /// files for the asset's GUID. Fast text-based scan, no dependency graph required.
    /// </summary>
    public static class TechArtReferenceFinder
    {
        private static readonly HashSet<string> SearchableExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".unity", ".prefab", ".mat", ".asset", ".controller", ".overridecontroller",
            ".playable", ".mixer", ".timeline", ".spriteatlas", ".scenetemplate",
            ".shadergraph", ".shadersubgraph", ".physicmaterial", ".physicmaterial2d",
            ".rendertexture", ".signal", ".inputactions", ".flare", ".guiskin",
            ".fontsettings", ".anim", ".asset", ".mask", ".cubemap"
        };

        public static string GetGuid(string assetPath)
        {
            return AssetDatabase.AssetPathToGUID(assetPath);
        }

        public static List<string> FindReferences(string assetPath)
        {
            var result = new List<string>();
            var guid = GetGuid(assetPath);
            if (string.IsNullOrEmpty(guid)) return result;

            foreach (var file in GetAllSearchableFiles())
            {
                if (FileContainsGuid(file, guid)) result.Add(file);
            }

            return result;
        }

        public static bool FileContainsGuid(string path, string guid)
        {
            if (!File.Exists(path)) return false;

            try
            {
                using (var reader = new StreamReader(path))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.IndexOf(guid, StringComparison.Ordinal) >= 0) return true;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"TechArt Reference Finder: could not read '{path}': {e.Message}");
            }

            return false;
        }

        public static List<string> GetAllSearchableFiles()
        {
            var files = new List<string>();
            foreach (var root in new[] { "Assets", "Packages" })
            {
                if (!Directory.Exists(root)) continue;

                foreach (var file in Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories))
                {
                    if (SearchableExtensions.Contains(Path.GetExtension(file))) files.Add(file);
                }
            }

            return files;
        }
    }
}

using System.IO;
using UnityEditor;
using UnityEngine;

namespace Hhnnnk4.TechArtTools.Editor
{
    public static class TechArtAuditConfigLoader
    {
        private const string DefaultFolder = "Assets/TechArtTools";
        private const string DefaultPath = "Assets/TechArtTools/TechArtAuditConfig.asset";

        /// <summary>
        /// Returns the first TechArtAuditConfig found in the project, or creates
        /// a default one under Assets/TechArtTools/.
        /// </summary>
        public static TechArtAuditConfig GetOrCreate()
        {
            var guids = AssetDatabase.FindAssets("t:TechArtAuditConfig");
            if (guids != null && guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                var existing = AssetDatabase.LoadAssetAtPath<TechArtAuditConfig>(path);
                if (existing != null) return existing;
            }

            var config = ScriptableObject.CreateInstance<TechArtAuditConfig>();
            if (!AssetDatabase.IsValidFolder(DefaultFolder))
            {
                AssetDatabase.CreateFolder("Assets", "TechArtTools");
            }

            AssetDatabase.CreateAsset(config, DefaultPath);
            AssetDatabase.SaveAssets();
            return config;
        }

        public static void DeleteDefault()
        {
            if (AssetDatabase.LoadAssetAtPath<TechArtAuditConfig>(DefaultPath) != null)
            {
                AssetDatabase.DeleteAsset(DefaultPath);
            }
        }
    }
}

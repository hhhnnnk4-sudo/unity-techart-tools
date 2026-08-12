using UnityEditor;
using UnityEngine;

namespace Hhnnnk4.TechArtTools.Editor
{
    public static class TechArtMenu
    {
        [MenuItem("Tools/TechArt Tools/Create Audit Config")]
        public static void CreateAuditConfig()
        {
            var config = TechArtAuditConfigLoader.GetOrCreate();
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }

        [MenuItem("Tools/TechArt Tools/Documentation", priority = 100)]
        public static void OpenDocumentation()
        {
            Application.OpenURL("https://github.com/hhhnnnk4-sudo/unity-techart-tools/blob/main/README.md");
        }
    }
}

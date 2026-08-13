using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Hhnnnk4.TechArtTools.Editor
{
    public static class TechArtPrefabAuditor
    {
        public static void Audit(string path, TechArtAuditConfig config, List<TechArtIssue> issues)
        {
            if (!config.CheckPrefabMissingScripts) return;

            GameObject contents = null;
            try
            {
                contents = PrefabUtility.LoadPrefabContents(path);
                var missing = CountMissingScripts(contents);
                if (missing > 0)
                {
                    issues.Add(new TechArtIssue(
                        TechArtIssueCategory.Prefab,
                        TechArtIssueSeverity.Warning,
                        "Missing scripts in prefab",
                        $"'{Path.GetFileName(path)}' has {missing} missing script component(s).",
                        AssetDatabase.LoadAssetAtPath<GameObject>(path)));
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                if (contents != null)
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                }
            }
        }

        public static int CountMissingScripts(GameObject root)
        {
            int missing = 0;
            foreach (var component in root.GetComponentsInChildren<Component>(true))
            {
                if (component == null) missing++;
            }

            return missing;
        }
    }
}

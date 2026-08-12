using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hhnnnk4.TechArtTools.Editor
{
    public static class TechArtSceneAuditor
    {
        public static void Audit(Scene scene, TechArtAuditConfig config, List<TechArtIssue> issues)
        {
            var rootObjects = scene.GetRootGameObjects();

            foreach (var root in rootObjects)
            {
                if (config.CheckMissingScripts)
                {
                    int missing = CountMissingScripts(root);
                    if (missing > 0)
                    {
                        issues.Add(new TechArtIssue(
                            TechArtIssueCategory.Scene,
                            TechArtIssueSeverity.Warning,
                            "Missing scripts",
                            $"'{root.name}' (scene: {scene.name}) has {missing} missing script component(s).",
                            null));
                    }
                }

                if (config.CheckLights)
                {
                    int realtime = 0;
                    int shadowed = 0;
                    foreach (var light in root.GetComponentsInChildren<Light>(true))
                    {
                        if (light.lightmapBakeType == LightmapBakeType.Realtime) realtime++;
                        if (light.shadows != LightShadows.None) shadowed++;
                    }

                    if (config.WarnRealtimeLights > 0 && realtime > config.WarnRealtimeLights)
                    {
                        issues.Add(new TechArtIssue(
                            TechArtIssueCategory.Scene,
                            TechArtIssueSeverity.Warning,
                            "Too many realtime lights",
                            $"'{root.name}' (scene: {scene.name}) has {realtime} realtime lights (threshold: {config.WarnRealtimeLights}).",
                            null));
                    }

                    if (shadowed > 0 && config.WarnRendererCount > 0)
                    {
                        int renderers = root.GetComponentsInChildren<Renderer>(true).Length;
                        if (renderers > config.WarnRendererCount)
                        {
                            issues.Add(new TechArtIssue(
                                TechArtIssueCategory.Scene,
                                TechArtIssueSeverity.Info,
                                "High renderer count",
                                $"'{root.name}' (scene: {scene.name}) has {renderers} renderers (threshold: {config.WarnRendererCount}).",
                                null));
                        }
                    }
                }
            }
        }

        private static int CountMissingScripts(GameObject root)
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

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Hhnnnk4.TechArtTools.Editor
{
    /// <summary>
    /// Shows which shaders are used by how many materials, flagging built-in
    /// pipeline shaders as migration candidates.
    /// </summary>
    public class TechArtShaderUsageWindow : EditorWindow
    {
        private TechArtAuditScope _scope = TechArtAuditScope.Assets;

        private readonly List<TechArtShaderUsageInfo> _results = new List<TechArtShaderUsageInfo>();
        private Vector2 _scroll;
        private bool _busy;
        private string _status = string.Empty;

        private GUIStyle _rowStyle;

        [MenuItem("Tools/TechArt Tools/Shader Usage")]
        public static void Open()
        {
            GetWindow<TechArtShaderUsageWindow>("Shader Usage");
        }

        private void OnGUI()
        {
            DrawToolbar();
            if (_results.Count == 0 && !_busy)
            {
                EditorGUILayout.Space(20);
                EditorGUILayout.HelpBox(
                    "Analyzes material usage per shader across your selection or the whole project.\n\n" +
                    "Built-in pipeline shaders (Standard, Legacy, Sprites, UI, Particles, etc.) are flagged\n" +
                    "as candidates for URP/HDRP migration.\n\nClick Scan to begin.",
                    MessageType.Info);
            }
            else
            {
                DrawResults();
            }
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            _scope = (TechArtAuditScope)EditorGUILayout.EnumPopup(_scope, EditorStyles.toolbarPopup, GUILayout.Width(140));
            if (GUILayout.Button("Scan", EditorStyles.toolbarButton)) Scan();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Shaders: {_results.Count}", EditorStyles.boldLabel, GUILayout.Width(120));
            if (!string.IsNullOrEmpty(_status))
            {
                EditorGUILayout.LabelField(_status, EditorStyles.miniLabel);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawResults()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            int builtInCount = 0;
            int materialCount = 0;
            foreach (var info in _results)
            {
                if (info.IsBuiltIn) builtInCount += info.MaterialCount;
                materialCount += info.MaterialCount;
                DrawRow(info);
            }

            if (builtInCount > 0)
            {
                EditorGUILayout.HelpBox(
                    $"{builtInCount} material(s) use built-in pipeline shaders — review for URP/HDRP migration.",
                    MessageType.Warning);
            }

            EditorGUILayout.LabelField(
                $"Total materials: {materialCount}   Materials on built-in shaders: {builtInCount}",
                EditorStyles.miniLabel);
            EditorGUILayout.EndScrollView();
        }

        private void DrawRow(TechArtShaderUsageInfo info)
        {
            if (_rowStyle == null)
            {
                _rowStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(8, 8, 6, 6),
                    margin = new RectOffset(4, 4, 2, 2)
                };
            }

            EditorGUILayout.BeginVertical(_rowStyle);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(info.ShaderName, EditorStyles.boldLabel);
            if (info.IsBuiltIn)
            {
                EditorGUILayout.LabelField("built-in", EditorStyles.miniBoldLabel, GUILayout.Width(60));
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField($"{info.MaterialCount} material(s)", GUILayout.Width(110));
            if (GUILayout.Button("Select", GUILayout.Width(60))) SelectMaterials(info);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            GUILayout.Space(2);
        }

        private static void SelectMaterials(TechArtShaderUsageInfo info)
        {
            var assets = new List<UnityEngine.Object>(info.MaterialCount);
            foreach (var path in info.MaterialPaths)
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material != null) assets.Add(material);
            }

            Selection.objects = assets.ToArray();
        }

        private void Scan()
        {
            _busy = true;
            _results.Clear();
            try
            {
                var paths = _scope == TechArtAuditScope.Selection ? GetSelectedPaths() : GetAllMaterialPaths();
                _results.AddRange(TechArtShaderUsageAnalyzer.Analyze(paths));
                _status = $"Scanned {paths.Count} material(s).";
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                _status = "Scan failed.";
            }
            finally
            {
                _busy = false;
                Repaint();
            }
        }

        private static List<string> GetSelectedPaths()
        {
            var paths = new List<string>();
            foreach (var obj in Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Assets))
            {
                var path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path)) paths.Add(path);
            }

            return paths;
        }

        private static List<string> GetAllMaterialPaths()
        {
            var all = AssetDatabase.GetAllAssetPaths();
            var paths = new List<string>(all.Length);
            foreach (var path in all)
            {
                if (!path.EndsWith(".mat", StringComparison.OrdinalIgnoreCase)) continue;
                if (path.StartsWith("Assets/", StringComparison.Ordinal) ||
                    path.StartsWith("Packages/", StringComparison.Ordinal))
                {
                    paths.Add(path);
                }
            }

            return paths;
        }
    }
}

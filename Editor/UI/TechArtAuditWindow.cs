using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Hhnnnk4.TechArtTools.Editor
{
    public enum TechArtAuditScope { Selection, Assets, OpenScenes }

    public class TechArtAuditWindow : EditorWindow
    {
        private TechArtAuditConfig _config;
        private TechArtAuditScope _scope = TechArtAuditScope.Selection;

        private readonly List<TechArtIssue> _issues = new List<TechArtIssue>();
        private Vector2 _scroll;
        private bool _busy;

        private int _errorCount;
        private int _warningCount;
        private int _infoCount;
        private string _status = string.Empty;

        private GUIStyle _rowStyle;

        [MenuItem("Tools/TechArt Tools/Audit Window")]
        public static void Open()
        {
            GetWindow<TechArtAuditWindow>("TechArt Audit");
        }

        private void OnEnable()
        {
            _config = TechArtAuditConfigLoader.GetOrCreate();
            RefreshCounts();
        }

        private void OnGUI()
        {
            DrawToolbar();
            if (_issues.Count == 0 && !_busy)
            {
                DrawEmptyState();
            }
            else
            {
                DrawIssueList();
            }
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            _scope = (TechArtAuditScope)EditorGUILayout.EnumPopup(_scope, EditorStyles.toolbarPopup, GUILayout.Width(140));

            if (GUILayout.Button("Audit", EditorStyles.toolbarButton)) RunAudit();
            if (GUILayout.Button("Fix All", EditorStyles.toolbarButton)) FixAll();
            if (GUILayout.Button("Clear", EditorStyles.toolbarButton)) Clear();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Config", EditorStyles.toolbarButton))
            {
                Selection.activeObject = _config;
                EditorGUIUtility.PingObject(_config);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField(
                $"Errors: {_errorCount}   Warnings: {_warningCount}   Info: {_infoCount}",
                EditorStyles.boldLabel,
                GUILayout.Width(200));
            if (!string.IsNullOrEmpty(_status))
            {
                EditorGUILayout.LabelField(_status, EditorStyles.miniLabel);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawEmptyState()
        {
            EditorGUILayout.Space(20);
            EditorGUILayout.HelpBox(
                "Choose a scope and click Audit.\n\n" +
                "  - Selection: audit the assets selected in the Project window\n" +
                "  - Assets:    audit all importable textures / meshes / materials\n" +
                "  - OpenScenes: audit the currently open scenes",
                MessageType.Info);
        }

        private void DrawIssueList()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            for (int i = 0; i < _issues.Count; i++)
            {
                DrawIssue(_issues[i]);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawIssue(TechArtIssue issue)
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
            var icon = GetSeverityIcon(issue.Severity);
            if (icon != null)
            {
                EditorGUILayout.LabelField(icon, GUILayout.Width(20));
            }

            EditorGUILayout.LabelField(issue.Title, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            if (issue.Asset != null && GUILayout.Button("Ping", GUILayout.Width(50)))
            {
                Selection.activeObject = issue.Asset;
                EditorGUIUtility.PingObject(issue.Asset);
            }

            if (issue.IsFixable && GUILayout.Button("Fix", GUILayout.Width(50)))
            {
                TryFix(issue);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(issue.Message, (MessageType)issue.Severity);

            if (!string.IsNullOrEmpty(issue.AssetPath))
            {
                EditorGUILayout.LabelField(issue.AssetPath, EditorStyles.miniLabel);
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(2);
        }

        private static GUIContent GetSeverityIcon(TechArtIssueSeverity severity)
        {
            switch (severity)
            {
                case TechArtIssueSeverity.Error:
                    return EditorGUIUtility.IconContent("console.erroricon");
                case TechArtIssueSeverity.Warning:
                    return EditorGUIUtility.IconContent("console.warnicon");
                default:
                    return EditorGUIUtility.IconContent("console.infoicon");
            }
        }

        private void RunAudit()
        {
            _busy = true;
            _issues.Clear();
            try
            {
                switch (_scope)
                {
                    case TechArtAuditScope.Selection:
                        _issues.AddRange(TechArtAudit.RunAssets(_config, GetSelectedAssetPaths()));
                        _status = "Selected assets audited.";
                        break;
                    case TechArtAuditScope.Assets:
                        _issues.AddRange(TechArtAudit.RunAssets(_config, GetAllAuditableAssetPaths()));
                        _status = "All project assets audited.";
                        break;
                    case TechArtAuditScope.OpenScenes:
                        _issues.AddRange(TechArtAudit.RunScenes(_config));
                        _status = "Open scenes audited.";
                        break;
                }

                if (!_config.ShowInfoLevel)
                {
                    _issues.RemoveAll(issue => issue.Severity == TechArtIssueSeverity.Info);
                }
            }
            finally
            {
                _busy = false;
                RefreshCounts();
                Repaint();
            }
        }

        private void FixAll()
        {
            for (int i = 0; i < _issues.Count; i++)
            {
                var issue = _issues[i];
                if (!issue.IsFixable) continue;

                if (EditorUtility.DisplayCancelableProgressBar(
                        "TechArt Fix All",
                        issue.Title,
                        (float)i / Mathf.Max(1, _issues.Count)))
                {
                    break;
                }

                TryFix(issue);
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();

            _status = "Fixes applied. Re-auditing...";
            RunAudit();
        }

        private void TryFix(TechArtIssue issue)
        {
            try
            {
                issue.FixAction?.Invoke();
                _status = "Fix applied: " + issue.Title;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                _status = "Fix failed: " + issue.Title;
            }

            RefreshCounts();
            Repaint();
        }

        private void Clear()
        {
            _issues.Clear();
            _status = string.Empty;
            RefreshCounts();
            Repaint();
        }

        private void RefreshCounts()
        {
            _errorCount = 0;
            _warningCount = 0;
            _infoCount = 0;
            foreach (var issue in _issues)
            {
                switch (issue.Severity)
                {
                    case TechArtIssueSeverity.Error: _errorCount++; break;
                    case TechArtIssueSeverity.Warning: _warningCount++; break;
                    case TechArtIssueSeverity.Info: _infoCount++; break;
                }
            }
        }

        private static List<string> GetSelectedAssetPaths()
        {
            var paths = new List<string>();
            foreach (var obj in Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Assets))
            {
                var path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path)) paths.Add(path);
            }

            return paths;
        }

        private static List<string> GetAllAuditableAssetPaths()
        {
            var all = AssetDatabase.GetAllAssetPaths();
            var auditable = new List<string>(all.Length);
            foreach (var path in all)
            {
                if (!path.StartsWith("Assets/", StringComparison.Ordinal) &&
                    !path.StartsWith("Packages/", StringComparison.Ordinal))
                {
                    continue;
                }

                var extension = Path.GetExtension(path);
                switch (extension.ToLowerInvariant())
                {
                    case ".png":
                    case ".jpg":
                    case ".jpeg":
                    case ".tga":
                    case ".psd":
                    case ".tif":
                    case ".tiff":
                    case ".exr":
                    case ".hdr":
                    case ".gif":
                    case ".bmp":
                    case ".mat":
                    case ".fbx":
                    case ".obj":
                    case ".dae":
                    case ".blend":
                    case ".asset":
                        auditable.Add(path);
                        break;
                }
            }

            return auditable;
        }
    }
}

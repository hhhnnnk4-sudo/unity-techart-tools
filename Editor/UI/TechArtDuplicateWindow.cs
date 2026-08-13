using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Hhnnnk4.TechArtTools.Editor
{
    public enum TechArtDuplicateScope { Selection, Assets }

    public class TechArtDuplicateWindow : EditorWindow
    {
        private TechArtDuplicateScope _scope = TechArtDuplicateScope.Assets;
        private bool _includeTextures = true;
        private bool _includeMaterials = true;

        private readonly List<TechArtDuplicateInfo> _results = new List<TechArtDuplicateInfo>();
        private Vector2 _scroll;
        private bool _busy;
        private string _status = string.Empty;

        private GUIStyle _rowStyle;

        [MenuItem("Tools/TechArt Tools/Duplicate Finder")]
        public static void Open()
        {
            GetWindow<TechArtDuplicateWindow>("Duplicate Finder");
        }

        private void OnGUI()
        {
            DrawToolbar();
            if (_results.Count == 0 && !_busy)
            {
                DrawEmptyState();
            }
            else
            {
                DrawResults();
            }
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            _scope = (TechArtDuplicateScope)EditorGUILayout.EnumPopup(_scope, EditorStyles.toolbarPopup, GUILayout.Width(110));
            _includeTextures = GUILayout.Toggle(_includeTextures, "Textures", EditorStyles.toolbarButton, GUILayout.Width(80));
            _includeMaterials = GUILayout.Toggle(_includeMaterials, "Materials", EditorStyles.toolbarButton, GUILayout.Width(85));

            if (GUILayout.Button("Scan", EditorStyles.toolbarButton)) Scan();

            GUILayout.FlexibleSpace();

            if (_results.Count > 0 && GUILayout.Button("Delete All Duplicates", EditorStyles.toolbarButton))
            {
                DeleteAllDuplicates();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            var groupCount = _results.Count;
            long totalWasted = 0;
            foreach (var group in _results) totalWasted += group.WastedBytes;

            EditorGUILayout.LabelField(
                $"Duplicate groups: {groupCount}   Wasted: {EditorUtility.FormatBytes(totalWasted)}",
                EditorStyles.boldLabel,
                GUILayout.Width(320));
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
                "Finds byte-identical duplicate textures and materials by content hash.\n\n" +
                "  - Selection: scan the assets selected in the Project window\n" +
                "  - Assets:    scan the whole project\n\n" +
                "Click Scan to begin.",
                MessageType.Info);
        }

        private void DrawResults()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            for (int i = 0; i < _results.Count; i++)
            {
                DrawGroup(_results[i]);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawGroup(TechArtDuplicateInfo group)
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
            EditorGUILayout.LabelField(
                $"{group.Count} duplicates · {group.Type} · wastes {group.WastedSize}",
                EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Delete", GUILayout.Width(60))) DeleteGroup(group);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Hash: " + group.Hash, EditorStyles.miniLabel);

            for (int i = 0; i < group.Paths.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(i == 0 ? "keep" : "dup ", EditorStyles.miniLabel, GUILayout.Width(36));
                var path = group.Paths[i];
                var isAsset = path.StartsWith("Assets/", StringComparison.Ordinal);
                if (GUILayout.Button(path, EditorStyles.miniButtonLeft))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                    if (asset != null)
                    {
                        Selection.activeObject = asset;
                        EditorGUIUtility.PingObject(asset);
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(2);
        }

        private void Scan()
        {
            _busy = true;
            _results.Clear();
            try
            {
                var paths = _scope == TechArtDuplicateScope.Selection ? GetSelectedPaths() : GetAllPaths();
                var total = paths.Count;
                var processed = 0;

                var found = TechArtDuplicateFinder.FindDuplicates(
                    paths,
                    _includeTextures,
                    _includeMaterials,
                    path =>
                    {
                        if (EditorUtility.DisplayCancelableProgressBar(
                                "Scanning for duplicates",
                                path,
                                total > 0 ? (float)processed / total : 0f))
                        {
                            throw new OperationCanceledException();
                        }

                        processed++;
                    });

                _results.AddRange(found);
                _status = $"Scanned {processed} asset(s), found {_results.Count} duplicate group(s).";
            }
            catch (OperationCanceledException)
            {
                _status = "Scan cancelled.";
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                _status = "Scan failed.";
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                _busy = false;
                Repaint();
            }
        }

        private void DeleteGroup(TechArtDuplicateInfo group, bool confirm = true)
        {
            if (group.Paths.Count < 2) return;

            if (confirm)
            {
                var confirmed = EditorUtility.DisplayDialog(
                    "Delete Duplicates",
                    $"Delete {group.Paths.Count - 1} duplicate(s) (keep '{group.KeeperPath}')?\n\n" +
                    "Assets are moved to the OS trash. Check the list before confirming.",
                    "Delete",
                    "Cancel");
                if (!confirmed) return;
            }

            var deleted = 0;
            for (int i = 1; i < group.Paths.Count; i++)
            {
                if (AssetDatabase.DeleteAsset(group.Paths[i])) deleted++;
            }

            group.Paths.RemoveRange(1, group.Paths.Count - 1);
            _results.RemoveAll(g => g.Paths.Count < 2);
            AssetDatabase.Refresh();
            _status = $"Deleted {deleted} duplicate(s).";
            Repaint();
        }

        private void DeleteAllDuplicates()
        {
            if (_results.Count == 0) return;

            int total = 0;
            foreach (var group in _results) total += group.Paths.Count - 1;

            var confirmed = EditorUtility.DisplayDialog(
                "Delete All Duplicates",
                $"Delete {total} duplicate(s) across {_results.Count} group(s)?\n\n" +
                "Assets are moved to the OS trash. Check the list before confirming.",
                "Delete All",
                "Cancel");
            if (!confirmed) return;

            foreach (var group in _results.ToArray())
            {
                DeleteGroup(group, confirm: false);
            }

            AssetDatabase.SaveAssets();
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

        private static List<string> GetAllPaths()
        {
            var all = AssetDatabase.GetAllAssetPaths();
            var paths = new List<string>(all.Length);
            foreach (var path in all)
            {
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

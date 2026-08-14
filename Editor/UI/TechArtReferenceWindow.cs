using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Hhnnnk4.TechArtTools.Editor
{
    /// <summary>
    /// Shows every scene / prefab / material / asset file that references the
    /// selected asset (matched by GUID).
    /// </summary>
    public class TechArtReferenceWindow : EditorWindow
    {
        private UnityEngine.Object _target;
        private string _guid = string.Empty;

        private readonly List<string> _results = new List<string>();
        private Vector2 _scroll;
        private bool _busy;

        [MenuItem("Tools/TechArt Tools/Find References")]
        public static void Open()
        {
            var window = GetWindow<TechArtReferenceWindow>("Find References");
            window.SetTarget(Selection.activeObject);
        }

        [MenuItem("Assets/TechArt Tools/Find References", priority = 1000)]
        public static void OpenForSelection()
        {
            var window = GetWindow<TechArtReferenceWindow>("Find References");
            window.SetTarget(Selection.activeObject);
        }

        [MenuItem("Assets/TechArt Tools/Find References", validate = true)]
        private static bool ValidateForSelection()
        {
            var obj = Selection.activeObject;
            return obj != null && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(obj));
        }

        private void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged()
        {
            if (_target == null)
            {
                SetTarget(Selection.activeObject);
            }
        }

        private void SetTarget(UnityEngine.Object target)
        {
            _target = target;
            _guid = target != null ? TechArtReferenceFinder.GetGuid(AssetDatabase.GetAssetPath(target)) : string.Empty;
            _results.Clear();
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (_target == null)
            {
                DrawEmptyState();
                return;
            }

            DrawTargetInfo();
            DrawResults();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            var newTarget = EditorGUILayout.ObjectField(
                _target,
                typeof(UnityEngine.Object),
                false) as UnityEngine.Object;
            if (newTarget != _target)
            {
                SetTarget(newTarget);
            }

            if (GUILayout.Button("Scan", EditorStyles.toolbarButton)) Scan();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawEmptyState()
        {
            EditorGUILayout.Space(20);
            EditorGUILayout.HelpBox(
                "Select an asset (texture, material, mesh, prefab, ...) to find which files reference it.\n\n" +
                "Useful before deleting or moving an asset, or to trace who uses a texture/mesh.",
                MessageType.Info);
        }

        private void DrawTargetInfo()
        {
            var path = AssetDatabase.GetAssetPath(_target);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(_target.name, EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Path: " + path, EditorStyles.miniLabel);
            EditorGUILayout.LabelField("GUID: " + _guid, EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawResults()
        {
            if (_busy)
            {
                EditorGUILayout.HelpBox("Scanning...", MessageType.Info);
                return;
            }

            if (_results.Count == 0)
            {
                EditorGUILayout.HelpBox("No references found.", MessageType.Info);
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            for (int i = 0; i < _results.Count; i++)
            {
                var file = _results[i];
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button(file, EditorStyles.miniButtonLeft))
                {
                    var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(file);
                    if (obj != null)
                    {
                        Selection.activeObject = obj;
                        EditorGUIUtility.PingObject(obj);
                    }
                }

                if (file.EndsWith(".unity", StringComparison.OrdinalIgnoreCase) &&
                    GUILayout.Button("Open", GUILayout.Width(50)))
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        EditorSceneManager.OpenScene(file, OpenSceneMode.Single);
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.LabelField($"{_results.Count} file(s) reference this asset.", EditorStyles.miniLabel);
        }

        private void Scan()
        {
            if (string.IsNullOrEmpty(_guid)) return;

            _busy = true;
            _results.Clear();
            try
            {
                var files = TechArtReferenceFinder.GetAllSearchableFiles();
                for (int i = 0; i < files.Count; i++)
                {
                    if (EditorUtility.DisplayCancelableProgressBar(
                            "Finding references",
                            files[i],
                            (float)i / Mathf.Max(1, files.Count)))
                    {
                        break;
                    }

                    if (TechArtReferenceFinder.FileContainsGuid(files[i], _guid))
                    {
                        _results.Add(files[i]);
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                _busy = false;
                Repaint();
            }
        }
    }
}

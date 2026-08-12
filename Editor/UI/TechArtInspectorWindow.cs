using UnityEditor;
using UnityEngine;

namespace Hhnnnk4.TechArtTools.Editor
{
    /// <summary>
    /// A compact statistics panel that follows the Project window selection.
    /// Shows runtime memory and import settings for meshes, textures and materials.
    /// </summary>
    public class TechArtInspectorWindow : EditorWindow
    {
        private Vector2 _scroll;

        [MenuItem("Tools/TechArt Tools/Inspector")]
        public static void Open()
        {
            GetWindow<TechArtInspectorWindow>("TechArt Inspector");
        }

        private void OnEnable()
        {
            Selection.selectionChanged += Repaint;
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= Repaint;
        }

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            var target = Selection.activeObject;
            if (target == null)
            {
                EditorGUILayout.HelpBox(
                    "Select a mesh, texture or material in the Project window to inspect it.",
                    MessageType.Info);
            }
            else
            {
                DrawAssetHeader(target);

                var mesh = target as Mesh;
                if (mesh != null)
                {
                    DrawMesh(mesh);
                    return;
                }

                var texture = target as Texture2D;
                if (texture != null)
                {
                    DrawTexture(texture);
                    return;
                }

                var material = target as Material;
                if (material != null)
                {
                    DrawMaterial(material);
                    return;
                }

                DrawUnsupported(target);
            }

            EditorGUILayout.EndScrollView();
        }

        private static void DrawAssetHeader(Object target)
        {
            EditorGUILayout.LabelField(target.name, EditorStyles.boldLabel);
            var path = AssetDatabase.GetAssetPath(target);
            if (!string.IsNullOrEmpty(path))
            {
                EditorGUILayout.LabelField(path, EditorStyles.miniLabel);
            }

            EditorGUILayout.Space(6);
        }

        private void DrawMesh(Mesh mesh)
        {
            EditorGUILayout.LabelField("Type", "Mesh");
            EditorGUILayout.LabelField("Vertices", mesh.vertexCount.ToString());
            if (mesh.isReadable)
            {
                EditorGUILayout.LabelField("Triangles", (mesh.triangles.Length / 3).ToString());
            }
            else
            {
                EditorGUILayout.LabelField("Triangles", "(not readable)");
            }

            EditorGUILayout.LabelField("Index Format", mesh.indexFormat.ToString());
            EditorGUILayout.LabelField("Read/Write", mesh.isReadable ? "On (warning)" : "Off");
            var bounds = mesh.bounds;
            EditorGUILayout.LabelField("Bounds Size", $"{bounds.size.x:F2} x {bounds.size.y:F2} x {bounds.size.z:F2}");
            EditorGUILayout.LabelField("Runtime Memory", EditorUtility.FormatBytes(Profiler.GetRuntimeMemorySizeLong(mesh)));
        }

        private void DrawTexture(Texture2D texture)
        {
            EditorGUILayout.LabelField("Type", "Texture2D");
            EditorGUILayout.LabelField("Dimensions", $"{texture.width} x {texture.height}");
            EditorGUILayout.LabelField("Texture Format", texture.format.ToString());
            EditorGUILayout.LabelField("Runtime Memory", EditorUtility.FormatBytes(Profiler.GetRuntimeMemorySizeLong(texture)));

            var path = AssetDatabase.GetAssetPath(texture);
            if (string.IsNullOrEmpty(path)) return;

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Import Settings", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Type", importer.textureType.ToString());
            EditorGUILayout.LabelField("Max Size", importer.maxTextureSize.ToString());
            EditorGUILayout.LabelField("Mipmaps", importer.mipmapEnabled ? "On" : "Off");
            EditorGUILayout.LabelField("Read/Write", importer.isReadable ? "On" : "Off");
            EditorGUILayout.LabelField("sRGB", importer.sRGBTexture ? "Yes" : "No");
            EditorGUILayout.LabelField("Compression", importer.textureCompression.ToString());

            if (importer.textureType == TextureImporterType.NormalMap && importer.sRGBTexture)
            {
                EditorGUILayout.HelpBox("This normal map has sRGB enabled. Normal maps must be non-sRGB.", MessageType.Warning);
            }
        }

        private void DrawMaterial(Material material)
        {
            EditorGUILayout.LabelField("Type", "Material");
            EditorGUILayout.LabelField("Shader", material.shader != null ? material.shader.name : "(missing)");
            EditorGUILayout.LabelField("Shader Keywords", material.shaderKeywords != null ? material.shaderKeywords.Length.ToString() : "0");
            EditorGUILayout.LabelField("Render Queue", material.renderQueue.ToString());
            EditorGUILayout.LabelField("Is URP", material.HasProperty("_BaseMap") ? "Yes (likely)" : "No / Built-in");

            if (material.shader != null)
            {
                var stale = TechArtMaterialAuditor.GetStaleKeywords(material, material.shader);
                if (stale.Count > 0)
                {
                    EditorGUILayout.HelpBox(
                        $"Stale keywords: {string.Join(", ", stale)}",
                        MessageType.Warning);
                }
            }
        }

        private static void DrawUnsupported(Object asset)
        {
            EditorGUILayout.LabelField("Type", asset.GetType().Name);
            EditorGUILayout.HelpBox("This asset type is not covered by TechArt Inspector.", MessageType.Info);
        }
    }
}

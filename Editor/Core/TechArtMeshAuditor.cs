using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Hhnnnk4.TechArtTools.Editor
{
    public static class TechArtMeshAuditor
    {
        public static void Audit(string path, TechArtAuditConfig config, List<TechArtIssue> issues)
        {
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null) return;

            var name = Path.GetFileName(path);
            var modelImporter = AssetImporter.GetAtPath(path) as ModelImporter;

            if (config.CheckReadWriteEnabled && modelImporter != null && modelImporter.isReadable)
            {
                issues.Add(new TechArtIssue(
                    TechArtIssueCategory.Mesh,
                    TechArtIssueSeverity.Warning,
                    "Read/Write enabled",
                    $"'{name}' has Read/Write enabled. This keeps a CPU copy of the mesh in memory.",
                    mesh,
                    () =>
                    {
                        modelImporter.isReadable = false;
                        modelImporter.SaveAndReimport();
                    }));
            }

            if (config.WarnHighVertexCount > 0 && mesh.vertexCount >= config.WarnHighVertexCount)
            {
                issues.Add(new TechArtIssue(
                    TechArtIssueCategory.Mesh,
                    TechArtIssueSeverity.Info,
                    "High vertex count",
                    $"'{name}' has {mesh.vertexCount} vertices (threshold: {config.WarnHighVertexCount}).",
                    mesh));
            }

            if (config.WarnHighTriangleCount > 0 && mesh.isReadable && mesh.triangles.Length / 3 >= config.WarnHighTriangleCount)
            {
                issues.Add(new TechArtIssue(
                    TechArtIssueCategory.Mesh,
                    TechArtIssueSeverity.Info,
                    "High triangle count",
                    $"'{name}' has {mesh.triangles.Length / 3} triangles (threshold: {config.WarnHighTriangleCount}).",
                    mesh));
            }
        }
    }
}

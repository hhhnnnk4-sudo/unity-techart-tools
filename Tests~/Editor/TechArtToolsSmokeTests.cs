using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Hhnnnk4.TechArtTools.Editor;

namespace Hhnnnk4.TechArtTools.Tests
{
    public class TechArtToolsSmokeTests
    {
        [Test]
        public void ConfigDefaultsAreSane()
        {
            var config = ScriptableObject.CreateInstance<TechArtAuditConfig>();
            try
            {
                Assert.Greater(config.MaxTextureSize, 0);
                Assert.Greater(config.MaxShaderKeywords, 0);
                Assert.Greater(config.WarnHighVertexCount, 0);
                Assert.LessOrEqual(config.MaxTextureSize, 8192);
                Assert.Greater(config.MobileCompressionFormat, 0);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void AuditEmptyPathListReturnsNoIssues()
        {
            var config = ScriptableObject.CreateInstance<TechArtAuditConfig>();
            try
            {
                var issues = TechArtAudit.RunAssets(config, new List<string>());
                Assert.IsNotNull(issues);
                Assert.AreEqual(0, issues.Count);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void MobilePlatformMappingIsValid()
        {
            Assert.AreEqual("Android", TechArtTextureAuditor.GetPlatformString(TechArtMobilePlatform.Android));
            Assert.AreEqual("iPhone", TechArtTextureAuditor.GetPlatformString(TechArtMobilePlatform.iPhone));
            Assert.AreEqual("WebGL", TechArtTextureAuditor.GetPlatformString(TechArtMobilePlatform.WebGL));
        }

        [Test]
        public void CreateDefaultTextureInMemoryAndAuditWorks()
        {
            var config = ScriptableObject.CreateInstance<TechArtAuditConfig>();
            try
            {
                var texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
                texture.name = "SmokeTest";
                var issue = new TechArtIssue(
                    TechArtIssueCategory.Texture,
                    TechArtIssueSeverity.Warning,
                    "Smoke",
                    "Smoke test issue",
                    texture,
                    () => { });
                Assert.IsTrue(issue.IsFixable);
                Assert.AreEqual("Smoke", issue.Title);
                UnityEngine.Object.DestroyImmediate(texture);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void ExportMarkdownWritesReportFile()
        {
            var issues = new List<TechArtIssue>
            {
                new TechArtIssue(TechArtIssueCategory.Material, TechArtIssueSeverity.Warning, "Stale keywords", "Some message"),
                new TechArtIssue(TechArtIssueCategory.Texture, TechArtIssueSeverity.Error, "sRGB normal", "Another message")
            };

            var path = Path.Combine(Path.GetTempPath(), "techart_report_test.md");
            try
            {
                TechArtReportExporter.ExportMarkdown(issues, path);
                Assert.IsTrue(File.Exists(path));
                var content = File.ReadAllText(path);
                Assert.IsTrue(content.Contains("Stale keywords"));
                Assert.IsTrue(content.Contains("| Warning | Material |"));
                Assert.IsTrue(content.Contains("sRGB normal"));
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Test]
        public void ExportJsonProducesValidStructure()
        {
            var issues = new List<TechArtIssue>
            {
                new TechArtIssue(TechArtIssueCategory.Mesh, TechArtIssueSeverity.Info, "High vertex count", "100k vertices")
            };

            var path = Path.Combine(Path.GetTempPath(), "techart_report_test.json");
            try
            {
                TechArtReportExporter.ExportJson(issues, path);
                Assert.IsTrue(File.Exists(path));

                var root = JsonUtility.FromJson<TechArtReportRoot>(File.ReadAllText(path));
                Assert.IsNotNull(root);
                Assert.AreEqual(1, root.items.Count);
                Assert.AreEqual("Mesh", root.items[0].category);
                Assert.AreEqual("Info", root.items[0].severity);
                Assert.AreEqual("High vertex count", root.items[0].title);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Test]
        public void ConfigHasBaseCompressionToggle()
        {
            var config = ScriptableObject.CreateInstance<TechArtAuditConfig>();
            try
            {
                Assert.IsTrue(config.CheckBaseCompression);
                Assert.IsTrue(config.WarnOnHiddenShaders);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void Md5HashIsStableAndCorrect()
        {
            // MD5("abc") = 900150983cd24fb0d6963f7d28e17f72
            var hash = TechArtDuplicateFinder.ComputeMd5(System.Text.Encoding.UTF8.GetBytes("abc"));
            Assert.AreEqual("900150983cd24fb0d6963f7d28e17f72", hash);
        }

        [Test]
        public void FindDuplicatesGroupsIdenticalFiles()
        {
            var dir = Path.Combine(Path.GetTempPath(), "techart_dup_test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                var a = Path.Combine(dir, "a.png");
                var b = Path.Combine(dir, "b.png");
                var c = Path.Combine(dir, "c.png");
                var bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
                File.WriteAllBytes(a, bytes);
                File.WriteAllBytes(b, bytes);
                File.WriteAllBytes(c, new byte[] { 9, 9, 9 });

                var groups = TechArtDuplicateFinder.FindDuplicates(
                    new[] { a, b, c },
                    includeTextures: true,
                    includeMaterials: false);

                Assert.AreEqual(1, groups.Count);
                Assert.AreEqual(2, groups[0].Paths.Count);
                Assert.AreEqual(TechArtDuplicateType.Texture, groups[0].Type);
                Assert.AreEqual(bytes.Length, groups[0].WastedBytes);
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        [Test]
        public void BuiltInShaderClassifierIsCorrect()
        {
            Assert.IsTrue(TechArtShaderUsageAnalyzer.IsBuiltInShader("Standard"));
            Assert.IsTrue(TechArtShaderUsageAnalyzer.IsBuiltInShader("Legacy Shaders/Diffuse"));
            Assert.IsTrue(TechArtShaderUsageAnalyzer.IsBuiltInShader("Sprites/Default"));
            Assert.IsTrue(TechArtShaderUsageAnalyzer.IsBuiltInShader("UI/Default"));
            Assert.IsFalse(TechArtShaderUsageAnalyzer.IsBuiltInShader("Universal Render Pipeline/Lit"));
            Assert.IsFalse(TechArtShaderUsageAnalyzer.IsBuiltInShader("HDRP/Lit"));
            Assert.IsFalse(TechArtShaderUsageAnalyzer.IsBuiltInShader("Shader Graphs/MyGraph"));
            Assert.IsFalse(TechArtShaderUsageAnalyzer.IsBuiltInShader("Hidden/InternalErrorShader"));
        }

        [Test]
        public void PrefabMissingScriptCountIsZeroOnCleanObject()
        {
            var go = new GameObject("Clean");
            try
            {
                go.AddComponent<Transform>();
                Assert.AreEqual(0, TechArtPrefabAuditor.CountMissingScripts(go));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }
    }
}

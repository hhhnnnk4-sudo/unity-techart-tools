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
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(config);
            }
        }
    }
}

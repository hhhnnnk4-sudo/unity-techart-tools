using System;
using System.Collections.Generic;
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
    }
}

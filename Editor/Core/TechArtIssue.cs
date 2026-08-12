using System;
using UnityEditor;
using UnityEngine;

namespace Hhnnnk4.TechArtTools.Editor
{
    public enum TechArtIssueSeverity { Info, Warning, Error }

    public enum TechArtIssueCategory { Texture, Mesh, Material, Scene, General }

    /// <summary>
    /// A single finding produced by an audit. Carries an optional one-click fix.
    /// </summary>
    public sealed class TechArtIssue
    {
        public TechArtIssueCategory Category;
        public TechArtIssueSeverity Severity;
        public string Title;
        public string Message;
        public string AssetPath;
        public UnityEngine.Object Asset;
        public Action FixAction;

        public TechArtIssue(
            TechArtIssueCategory category,
            TechArtIssueSeverity severity,
            string title,
            string message,
            UnityEngine.Object asset = null,
            Action fixAction = null)
        {
            Category = category;
            Severity = severity;
            Title = title;
            Message = message;
            Asset = asset;
            AssetPath = asset != null ? AssetDatabase.GetAssetPath(asset) : string.Empty;
            FixAction = fixAction;
        }

        public bool IsFixable => FixAction != null;
    }
}

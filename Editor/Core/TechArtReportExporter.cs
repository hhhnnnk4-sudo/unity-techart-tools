using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Hhnnnk4.TechArtTools.Editor
{
    [Serializable]
    public class TechArtReportItem
    {
        public string category;
        public string severity;
        public string title;
        public string message;
        public string assetPath;
        public bool fixable;
    }

    [Serializable]
    public class TechArtReportRoot
    {
        public string generated;
        public List<TechArtReportItem> items = new List<TechArtReportItem>();
    }

    /// <summary>
    /// Serializes audit results to JSON or Markdown for sharing / archiving.
    /// </summary>
    public static class TechArtReportExporter
    {
        public static void ExportJson(IReadOnlyList<TechArtIssue> issues, string path)
        {
            var root = BuildRoot(issues);
            File.WriteAllText(path, JsonUtility.ToJson(root, true), Encoding.UTF8);
        }

        public static void ExportMarkdown(IReadOnlyList<TechArtIssue> issues, string path)
        {
            var builder = new StringBuilder();
            builder.AppendLine("# TechArt Tools - Audit Report");
            builder.AppendLine();
            builder.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            builder.AppendLine($"Issues: {issues.Count}");
            builder.AppendLine();
            builder.AppendLine("| Severity | Category | Title | Asset | Fixable |");
            builder.AppendLine("| --- | --- | --- | --- | --- |");

            foreach (var issue in issues)
            {
                var asset = string.IsNullOrEmpty(issue.AssetPath) ? "-" : issue.AssetPath;
                var title = Escape(issue.Title);
                builder.AppendLine($"| {issue.Severity} | {issue.Category} | {title} | `{asset}` | {(issue.IsFixable ? "yes" : "no")} |");
                builder.AppendLine();
                builder.AppendLine($"  - {Escape(issue.Message)}");
            }

            File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
        }

        private static TechArtReportRoot BuildRoot(IReadOnlyList<TechArtIssue> issues)
        {
            var root = new TechArtReportRoot
            {
                generated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            foreach (var issue in issues)
            {
                root.items.Add(new TechArtReportItem
                {
                    category = issue.Category.ToString(),
                    severity = issue.Severity.ToString(),
                    title = issue.Title,
                    message = issue.Message,
                    assetPath = issue.AssetPath,
                    fixable = issue.IsFixable
                });
            }

            return root;
        }

        private static string Escape(string text)
        {
            return text == null ? string.Empty : text.Replace("|", "\\|").Replace("\n", " ");
        }
    }
}

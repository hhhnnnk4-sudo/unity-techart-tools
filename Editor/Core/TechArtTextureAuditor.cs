using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Hhnnnk4.TechArtTools.Editor
{
    public static class TechArtTextureAuditor
    {
        public static void Audit(string path, TechArtAuditConfig config, List<TechArtIssue> issues)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;

            var asset = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            var name = Path.GetFileName(path);
            var isNormal = importer.textureType == TextureImporterType.NormalMap;

            if (importer.maxTextureSize > config.MaxTextureSize)
            {
                issues.Add(new TechArtIssue(
                    TechArtIssueCategory.Texture,
                    TechArtIssueSeverity.Warning,
                    "Texture size exceeds project limit",
                    $"'{name}' max size is {importer.maxTextureSize}px, project limit is {config.MaxTextureSize}px.",
                    asset,
                    () =>
                    {
                        importer.maxTextureSize = config.MaxTextureSize;
                        importer.SaveAndReimport();
                    }));
            }

            if (config.CheckReadWrite && importer.isReadable)
            {
                issues.Add(new TechArtIssue(
                    TechArtIssueCategory.Texture,
                    TechArtIssueSeverity.Warning,
                    "Read/Write enabled",
                    $"'{name}' has Read/Write enabled. This keeps a CPU copy in memory at runtime.",
                    asset,
                    () =>
                    {
                        importer.isReadable = false;
                        importer.SaveAndReimport();
                    }));
            }

            if (config.CheckSrgbOnNormalMaps && isNormal && importer.sRGBTexture)
            {
                issues.Add(new TechArtIssue(
                    TechArtIssueCategory.Texture,
                    TechArtIssueSeverity.Error,
                    "Normal map is sRGB",
                    $"'{name}' is a normal map but sRGB is enabled. Normal maps must be non-sRGB.",
                    asset,
                    () =>
                    {
                        importer.sRGBTexture = false;
                        importer.SaveAndReimport();
                    }));
            }

            if (config.CheckMipmaps && !isNormal && importer.textureType == TextureImporterType.Default && !importer.mipmapEnabled)
            {
                issues.Add(new TechArtIssue(
                    TechArtIssueCategory.Texture,
                    TechArtIssueSeverity.Warning,
                    "Mipmaps disabled",
                    $"'{name}' has no mipmaps. 3D textures should enable mipmaps to avoid aliasing and save bandwidth.",
                    asset,
                    () =>
                    {
                        importer.mipmapEnabled = true;
                        importer.SaveAndReimport();
                    }));
            }

            if (config.CheckBaseCompression &&
                importer.maxTextureSize >= 512 &&
                importer.textureCompression == TextureImporterCompression.Uncompressed)
            {
                issues.Add(new TechArtIssue(
                    TechArtIssueCategory.Texture,
                    TechArtIssueSeverity.Warning,
                    "Base platform texture is uncompressed",
                    $"'{name}' is imported uncompressed on the base platform. Compress to save memory and bandwidth.",
                    asset,
                    () =>
                    {
                        importer.textureCompression = TextureImporterCompression.Compressed;
                        importer.SaveAndReimport();
                    }));
            }

            if (config.CheckMobileCompression)
            {
                var platform = GetPlatformString(config.MobilePlatform);
                importer.GetPlatformTextureSettings(platform, out _, out var format, out _);
                if (IsUncompressed(format))
                {
                    issues.Add(new TechArtIssue(
                        TechArtIssueCategory.Texture,
                        TechArtIssueSeverity.Warning,
                        "Mobile platform has no compression",
                        $"'{name}' is not compressed on {platform} (format: {format}). Uncompressed textures waste GPU memory and bandwidth.",
                        asset,
                        () => ApplyMobileCompression(importer, config)));
                }
            }

            if (config.CheckNonPowerOfTwo && asset != null && !Mathf.IsPowerOfTwo(asset.width) && !Mathf.IsPowerOfTwo(asset.height))
            {
                issues.Add(new TechArtIssue(
                    TechArtIssueCategory.Texture,
                    TechArtIssueSeverity.Info,
                    "Non-power-of-two texture",
                    $"'{name}' is {asset.width} x {asset.height}. Some platforms rescale or reject NPOT textures.",
                    asset));
            }
        }

        private static bool IsUncompressed(TextureImporterFormat format)
        {
            switch (format)
            {
                case TextureImporterFormat.Automatic:
                case TextureImporterFormat.Automatic16bit:
                case TextureImporterFormat.AutomaticCompressed:
                case TextureImporterFormat.AutomaticCrunched:
                case TextureImporterFormat.AutomaticHDR:
                case TextureImporterFormat.RGBA32:
                case TextureImporterFormat.RGBA16:
                case TextureImporterFormat.RGB24:
                case TextureImporterFormat.ARGB32:
                case TextureImporterFormat.R16:
                case TextureImporterFormat.RGBA4444:
                case TextureImporterFormat.RGBA5551:
                case TextureImporterFormat.Alpha8:
                    return true;
                default:
                    return false;
            }
        }

        public static string GetPlatformString(TechArtMobilePlatform platform)
        {
            switch (platform)
            {
                case TechArtMobilePlatform.Android: return "Android";
                case TechArtMobilePlatform.iPhone: return "iPhone";
                case TechArtMobilePlatform.WebGL: return "WebGL";
                default: return "Android";
            }
        }

        public static void ApplyMobileCompression(TextureImporter importer, TechArtAuditConfig config)
        {
            var settings = new TextureImporterPlatformSettings
            {
                name = GetPlatformString(config.MobilePlatform),
                overridden = true,
                maxTextureSize = Mathf.Min(importer.maxTextureSize, config.MaxTextureSize),
                format = config.MobileCompressionFormat,
                compressionQuality = (int)TextureCompressionQuality.Normal
            };
            importer.SetPlatformTextureSettings(settings);
            importer.SaveAndReimport();
        }
    }
}

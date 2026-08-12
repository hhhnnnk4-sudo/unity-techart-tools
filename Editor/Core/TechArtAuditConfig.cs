using System;
using UnityEditor;
using UnityEngine;

namespace Hhnnnk4.TechArtTools.Editor
{
    public enum TechArtMobilePlatform { Android, iPhone, WebGL }

    /// <summary>
    /// Thresholds and toggles that drive every audit. Saved as an asset
    /// (Assets/TechArtTools/TechArtAuditConfig.asset) and auto-created on first use.
    /// </summary>
    [Serializable]
    public class TechArtAuditConfig : ScriptableObject
    {
        [Header("General")]
        [Tooltip("Show informational (Info) issues in the report.")]
        public bool ShowInfoLevel = true;

        [Header("Texture")]
        [Tooltip("Textures larger than this (in pixels, per axis) are reported.")]
        public int MaxTextureSize = 2048;
        public bool CheckReadWrite = true;
        public bool CheckMipmaps = true;
        public bool CheckSrgbOnNormalMaps = true;
        public bool CheckMobileCompression = true;
        [Tooltip("Mobile platform the compression check and one-click fix target.")]
        public TechArtMobilePlatform MobilePlatform = TechArtMobilePlatform.Android;
        [Tooltip("Compression format applied by the one-click fix on the configured mobile platform.")]
        public TextureImporterFormat MobileCompressionFormat = TextureImporterFormat.ASTC_6x6;
        [Tooltip("Warn when large textures are imported uncompressed on the base platform.")]
        public bool CheckBaseCompression = true;
        public bool CheckNonPowerOfTwo = true;

        [Header("Mesh")]
        public bool CheckReadWriteEnabled = true;
        [Tooltip("Meshes with more vertices than this are reported (Info).")]
        public int WarnHighVertexCount = 50000;
        [Tooltip("Meshes with more triangles than this are reported (Info).")]
        public int WarnHighTriangleCount = 200000;

        [Header("Material")]
        public bool CheckShaderKeywords = true;
        [Tooltip("Materials enabling more keywords than this are reported (keyword bloat).")]
        public int MaxShaderKeywords = 24;
        public bool CheckStaleKeywords = true;
        public bool WarnOnMissingTextures = true;

        [Header("Scene")]
        public bool CheckMissingScripts = true;
        public bool CheckLights = true;
        [Tooltip("Scenes with more realtime lights than this are reported.")]
        public int WarnRealtimeLights = 8;
        [Tooltip("Root objects with more renderers than this are reported (Info).")]
        public int WarnRendererCount = 4000;
    }
}

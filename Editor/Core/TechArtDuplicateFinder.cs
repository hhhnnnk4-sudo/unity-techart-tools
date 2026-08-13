using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;

namespace Hhnnnk4.TechArtTools.Editor
{
    public enum TechArtDuplicateType { Texture, Material }

    public class TechArtDuplicateInfo
    {
        public string Hash;
        public TechArtDuplicateType Type;
        public List<string> Paths = new List<string>();
        public long WastedBytes;

        public int Count => Paths.Count;
        public string KeeperPath => Paths.Count > 0 ? Paths[0] : string.Empty;
        public string WastedSize => EditorUtility.FormatBytes(WastedBytes);
    }

    /// <summary>
    /// Finds byte-identical duplicate assets (textures and/or materials) by content hash.
    /// </summary>
    public static class TechArtDuplicateFinder
    {
        private static readonly HashSet<string> TextureExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg", ".tga", ".psd", ".tif", ".tiff", ".exr", ".hdr", ".gif", ".bmp", ".webp"
        };

        public static List<TechArtDuplicateInfo> FindDuplicates(
            IEnumerable<string> paths,
            bool includeTextures,
            bool includeMaterials,
            Action<string> onProgress = null)
        {
            var byHash = new Dictionary<string, List<string>>(StringComparer.Ordinal);

            foreach (var path in paths)
            {
                if (string.IsNullOrEmpty(path)) continue;
                onProgress?.Invoke(path);

                if (includeTextures && IsTexture(path)) AddToHash(byHash, path, TechArtDuplicateType.Texture);
                else if (includeMaterials && IsMaterial(path)) AddToHash(byHash, path, TechArtDuplicateType.Material);
            }

            var result = new List<TechArtDuplicateInfo>();
            foreach (var kv in byHash)
            {
                if (kv.Value.Count < 2) continue;

                var info = new TechArtDuplicateInfo
                {
                    Hash = kv.Key,
                    Type = kv.Value.Count > 0 && IsMaterial(kv.Value[0])
                        ? TechArtDuplicateType.Material
                        : TechArtDuplicateType.Texture,
                    Paths = kv.Value
                };
                info.Paths.Sort(StringComparer.Ordinal);

                foreach (var p in info.Paths)
                {
                    var file = new FileInfo(p);
                    if (file.Exists) info.WastedBytes += file.Length;
                }

                result.Add(info);
            }

            result.Sort((a, b) => b.WastedBytes.CompareTo(a.WastedBytes));
            return result;
        }

        public static string ComputeMd5(byte[] data)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(data);
                return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
            }
        }

        private static void AddToHash(Dictionary<string, List<string>> map, string path, TechArtDuplicateType type)
        {
            if (!File.Exists(path)) return;

            var hash = ComputeMd5(File.ReadAllBytes(path));
            if (!map.TryGetValue(hash, out var list))
            {
                map[hash] = list = new List<string>();
            }

            list.Add(path);
        }

        private static bool IsTexture(string path) => TextureExtensions.Contains(Path.GetExtension(path));
        private static bool IsMaterial(string path) => string.Equals(Path.GetExtension(path), ".mat", StringComparison.OrdinalIgnoreCase);
    }
}

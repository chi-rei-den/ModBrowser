using Chireiden.ModBrowser.Models;
using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.IO;

namespace Chireiden.ModBrowser.ModLoader
{
    public static class ModInfo
    {
        public static bool ExtractInfo(this Mod mod, byte[] file, bool readInfo = false)
        {
            try
            {
                mod.Size = file.Length;

                using var ms = new MemoryStream(file);
                using var br = new BinaryReader(ms);
                br.ReadBytes(4);
                var tmlVersion = br.ReadString();
                mod.ModLoaderVersion = "tModLoader v" + tmlVersion;
                br.ReadBytes(280);

                if (new Version(tmlVersion) > new Version(0, 11))
                {
                    mod.Name = br.ReadString();
                    mod.Version = "v" + br.ReadString();

                    if (!readInfo)
                    {
                        return true;
                    }

                    var fileCount = br.ReadInt32();
                    var files = new List<ModFile>();
                    for (var i = 0; i < fileCount; i++)
                    {
                        files.Add(new ModFile(br));
                    }

                    for (var i = 0; i < fileCount; i++)
                    {
                        if (files[i].FileName == "Info")
                        {
                            br.ReadModInfo(mod);
                        }
                        else
                        {
                            br.ReadBytes(files[i].CompressedLength);
                        }
                    }
                }
                else
                {
                    using var deflateStream = new DeflateStream(ms, CompressionMode.Decompress);
                    using var content = new BinaryReader(deflateStream);

                    mod.Name = br.ReadString();
                    mod.Version = "v" + br.ReadString();

                    if (!readInfo)
                    {
                        return true;
                    }

                    var fileCount = br.ReadInt32();
                    for (var i = 0; i < fileCount; i++)
                    {
                        var fileName = content.ReadString();
                        var size = content.ReadInt32();
                        if (fileName == "Info")
                        {
                            content.ReadModInfo(mod);
                        }
                        else
                        {
                            content.ReadBytes(size);
                        }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void ReadModInfo(this BinaryReader br, Mod mod)
        {
            var tag = br.ReadString();
            while (tag.Length > 0)
            {
                _ = tag switch
                {
                    "modReferences" => mod.ModReferences ??= string.Join(", ", ReadList(br)),
                    "author" => mod.Author ??= br.ReadString(),
                    "version" => mod.Version ??= "v" + br.ReadString(),
                    "displayName" => mod.DisplayName ??= br.ReadString(),
                    "homepage" => mod.Homepage ??= br.ReadString(),
                    "description" => mod.Description ??= br.ReadString(),
                    "side" => mod.ModSide ??= ((ModSide)br.ReadByte()).ToString(),

                    #region Unused

                    "dllReferences" => string.Join(", ", ReadList(br)),
                    "weakReferences" => string.Join(", ", ReadList(br)),
                    "sortAfter" => string.Join(", ", ReadList(br)),
                    "sortBefore" => string.Join(", ", ReadList(br)),
                    "noCompile" => "",
                    "!hideCode" => "",
                    "!hideResources" => "",
                    "includeSource" => "",
                    "includePDB" => "",
                    "beta" => "",
                    "eacPath" => br.ReadString(),
                    "buildVersion" => br.ReadString(),

                    #endregion

                    _ => ""
                };

                tag = br.ReadString();
            }
        }

        private static IEnumerable<string> ReadList(BinaryReader reader)
        {
            var item = reader.ReadString();
            while (item.Length > 0)
            {
                yield return item;
                item = reader.ReadString();
            }
        }
    }

    public enum ModSide
    {
        Both,
        Client,
        Server,
        NoSync
    }
}

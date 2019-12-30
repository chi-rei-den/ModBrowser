﻿using Chireiden.ModBrowser.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Chireiden.ModBrowser.ModLoader
{
    public static class ModInfo
    {
        private static IEnumerable<string> ReadList(BinaryReader reader)
        {
            var item = reader.ReadString();
            while (item.Length > 0)
            {
                yield return item;
                item = reader.ReadString();
            }
        }

        public static void FromStream(this Mod mod, Stream stream)
        {
            using (var reader = new BinaryReader(stream))
            {
                var tag = reader.ReadString();
                while (tag.Length > 0)
                {
                    _ = tag switch
                    {
                        "modReferences" => mod.ModReferences ??= string.Join(", ", ReadList(reader)),
                        "author" => mod.Author ??= reader.ReadString(),
                        "version" => mod.Version ??= "v" + reader.ReadString(),
                        "displayName" => mod.DisplayName ??= reader.ReadString(),
                        "homepage" => mod.Homepage ??= reader.ReadString(),
                        "description" => mod.Description ??= reader.ReadString(),
                        "side" => mod.ModSide ??= ((ModSide)reader.ReadByte()).ToString(),

                        #region Unused

                        "dllReferences" => string.Join(", ", ReadList(reader)),
                        "weakReferences" => string.Join(", ", ReadList(reader)),
                        "sortAfter" => string.Join(", ", ReadList(reader)),
                        "sortBefore" => string.Join(", ", ReadList(reader)),
                        "noCompile" => "",
                        "!hideCode" => "",
                        "!hideResources" => "",
                        "includeSource" => "",
                        "includePDB" => "",
                        "beta" => "",
                        "eacPath" => reader.ReadString(),
                        "buildVersion" => reader.ReadString(),

                        #endregion

                        _ => ""
                    };

                    tag = reader.ReadString();
                }
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
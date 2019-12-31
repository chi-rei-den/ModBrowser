using System.IO;

namespace Chireiden.ModBrowser.ModLoader
{
    public class ModFile
    {
        public string FileName;
        public int CompressedLength;
        public int Length;

        public ModFile(BinaryReader br)
        {
            this.FileName = br.ReadString();
            this.Length = br.ReadInt32();
            this.CompressedLength = br.ReadInt32();
        }
    }
}

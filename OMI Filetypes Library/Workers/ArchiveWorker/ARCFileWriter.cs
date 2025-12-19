using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OMI.Formats.Archive;
using OMI.Workers;

namespace OMI.Workers.Archive
{
    public class ARCFileWriter : IDataFormatWriter
    {
        private ConsoleArchive _archive;

        public ARCFileWriter(ConsoleArchive archive)
        {
            _archive = archive;
        }

        public void WriteToFile(string filename)
        {
            using (FileStream fs = File.OpenWrite(filename))
            {
                WriteToStream(fs);
            }
        }

        public void WriteToStream(Stream stream)
        {
            using (var writer = new EndiannessAwareBinaryWriter(stream, ByteOrder.BigEndian))
            {
                writer.Write(_archive.Count);
                int currentOffset = 4 + _archive.Keys.ToArray().Sum(key => 10 + key.Length);
                foreach (KeyValuePair<string, byte[]> pair in _archive)
                {
                    int size = pair.Value.Length;
                    writer.Write((short)pair.Key.Length);
                    writer.WriteString(pair.Key, Encoding.ASCII);
                    writer.Write(currentOffset);
                    writer.Write(size);
                    currentOffset += size;
                }
                foreach (byte[] data in _archive.Values)
                {
                    writer.Write(data);
                }
            }
        }
    }
}

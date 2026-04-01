using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OMI.Formats.Archive;

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
                foreach (KeyValuePair<string, ConsoleArchiveEntry> entry in _archive)
                {
                    int size = entry.Value.Data.Length;
                    writer.Write((short)entry.Key.Length);
                    writer.WriteString(entry.Key, Encoding.ASCII);
                    writer.Write(currentOffset);
                    writer.Write(size);
                    currentOffset += size;
                }
                foreach (ConsoleArchiveEntry entry in _archive.Values)
                {
                    writer.Write(entry.Data);
                }
            }
        }
    }
}

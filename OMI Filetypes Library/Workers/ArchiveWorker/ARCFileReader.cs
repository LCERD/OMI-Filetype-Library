using System;
using System.IO;
using System.Text;
using OMI.Formats.Archive;
using OMI.Workers;

namespace OMI.Workers.Archive
{
    public class ARCFileReader : IDataFormatReader<ConsoleArchive>, IDataFormatReader
    {
        public ConsoleArchive FromFile(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException(filename);
            using (FileStream fs = File.OpenRead(filename))
            {
                return FromStream(fs);
            }
        }

        public ConsoleArchive FromStream(Stream stream)
        {
            ConsoleArchive archive = new ConsoleArchive();
            using (EndiannessAwareBinaryReader reader = new EndiannessAwareBinaryReader(stream, ByteOrder.BigEndian))
            {
                int numberOfFiles = reader.ReadInt32();
                for (int i = 0; i < numberOfFiles; i++)
                {
                    string name = ReadString(reader);
                    int pos = reader.ReadInt32();
                    int size = reader.ReadInt32();
                    archive[name] = ReadBytesFromPosition(stream, pos, size);
                }
            }
            return archive;
        }

        private string ReadString(EndiannessAwareBinaryReader reader)
        {
            short length = reader.ReadInt16();
            return reader.ReadString(length, Encoding.ASCII);
        }

        private byte[] ReadBytesFromPosition(Stream stream, int position, int size)
        {
            long origin = stream.Position;
            if (stream.Seek(position, SeekOrigin.Begin) != position)
                throw new Exception();
            byte[] bytes = new byte[size];
            stream.Read(bytes, 0, size);
            if (stream.Seek(origin, SeekOrigin.Begin) != origin)
                throw new Exception();
            return bytes;
        }

        object IDataFormatReader.FromStream(Stream stream) => FromStream(stream);

        object IDataFormatReader.FromFile(string filename) => FromFile(filename);
    }
}

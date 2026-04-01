using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OMI.Formats.Languages;

namespace OMI.Workers.Language
{
    public class LOCFileReader : IDataFormatReader<LOCFile>, IDataFormatReader
    {

        public LOCFile FromFile(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException(filename);
            
            LOCFile locFile = null;
            using (FileStream fs = File.OpenRead(filename))
            {
                locFile = FromStream(fs);
            }
            return locFile;
        }

        public LOCFile FromStream(Stream stream)
        {
            LOCFile locFile = new LOCFile();
            using (var reader = new EndiannessAwareBinaryReader(stream, ByteOrder.BigEndian))
            {
                int loc_type = reader.ReadInt32();
                int language_count = reader.ReadInt32();
                bool lookUpKey = loc_type == 2;

                if (lookUpKey) 
                {
                    locFile.hasUids = reader.ReadBoolean();
                    reader.BaseStream.Position -= 1;
                }

                List<string> keys = lookUpKey ? ReadKeys(reader) : null;
                int[] languageEntryBufferSizes = new int[language_count];
                for (int i = 0; i < language_count; i++)
                {
                    string language = ReadString(reader);
                    languageEntryBufferSizes[i] = reader.ReadInt32();
                    locFile.Languages.Add(language);
                }
                for (int i = 0; i < language_count; i++)
                {
                    Stream languageEntryStream = new MemoryStream(reader.ReadBytes(languageEntryBufferSizes[i]));
                    using (var entryReader = new EndiannessAwareBinaryReader(languageEntryStream, ByteOrder.BigEndian))
                    {
                        if (0 < entryReader.ReadInt32())
                            entryReader.ReadByte();
                        string language = ReadString(entryReader);
                        if (!locFile.Languages.Contains(language))
                            throw new KeyNotFoundException(nameof(language));
                        int count = entryReader.ReadInt32();
                        for (int j = 0; j < count; j++)
                        {
                            string key = lookUpKey ? keys[j] : ReadString(entryReader);
                            string value = ReadString(entryReader);
                            locFile.SetLocEntry(key, language, value);
                        }
                    }
                }
            }
            return locFile;
        }

        object IDataFormatReader.FromFile(string filename) => FromFile(filename);

        object IDataFormatReader.FromStream(Stream stream) => FromStream(stream);

        private List<string> ReadKeys(EndiannessAwareBinaryReader reader)
        {
            bool useUniqueIds = reader.ReadBoolean();
            int keyCount = reader.ReadInt32();
            List<string> keys = new List<string>(keyCount);
            for (int i = 0; i < keyCount; i++)
            {
                string key = useUniqueIds ? reader.ReadInt32().ToString("X08") : ReadString(reader);
                keys.Add(key);
            }
            return keys;
        }

        private string ReadString(EndiannessAwareBinaryReader reader)
        {
            int length = reader.ReadUInt16();
            return reader.ReadString(length, Encoding.UTF8);
        }
    }
}
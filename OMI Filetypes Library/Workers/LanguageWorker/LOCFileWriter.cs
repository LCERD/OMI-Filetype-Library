using OMI.Formats.Languages;
using OMI.Workers;
using System;
using System.IO;
using System.Text;

namespace OMI.Workers.Language
{
    public class LOCFileWriter : IDataFormatWriter
    {
        private LOCFile _locfile;
        private int _version;

        public LOCFileWriter(LOCFile file, int version)
        {
            _ = file ?? throw new ArgumentNullException(nameof(file));
            _locfile = file;
            _version = version;
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
            using (var writer = new EndiannessAwareBinaryWriter(stream, Encoding.UTF8, leaveOpen: true, ByteOrder.BigEndian))
            {
                writer.Write(_version);
                writer.Write(_locfile.Languages.Count);
                if (_version == 2)
                {
                    writer.Write(_locfile.hasUids);
                    writer.Write(_locfile.LocKeys.Count);
                    foreach (var key in _locfile.LocKeys.Keys)
                    {
                        if (_locfile.hasUids)
                            WriteUid(writer, key);
                        else
                            WriteString(writer, key);
                    }
                }
                WriteLanguages(writer);
                WriteLanguageEntries(writer);
            }
        }

        private void WriteLanguages(EndiannessAwareBinaryWriter writer)
        {
            foreach (var language in _locfile.Languages)
            {
                WriteString(writer, language);
                
                // Calculate the size of the language entry

                int size = 0;
                size += sizeof(int); // version
                if (_version > 0)
                    size += sizeof(byte); // bool
                size += sizeof(short) + writer.EncodingScheme.GetByteCount(language); // language name
                size += sizeof(int); // key count

                foreach (var locKey in _locfile.LocKeys.Keys)
                {
                    if (_version == 0)
                        size += sizeof(short) + writer.EncodingScheme.GetByteCount(locKey); // loc key string
                    size += sizeof(short) + writer.EncodingScheme.GetByteCount(_locfile.LocKeys[locKey][language]); // loc value string
                }
                writer.Write(size);
            }
        }

        private void WriteLanguageEntries(EndiannessAwareBinaryWriter writer)
        {
            foreach(var language in _locfile.Languages)
            {
                writer.Write(_version);
                if (_version > 0)
                    writer.Write(_locfile.hasUids);

                WriteString(writer, language);
                writer.Write(_locfile.LocKeys.Keys.Count);
                foreach(var locKey in _locfile.LocKeys.Keys)
                {
                    if (_version == 0)
                        WriteString(writer, locKey);
                    WriteString(writer, _locfile.LocKeys[locKey][language]);
                }
            }
        }

        private void WriteString(EndiannessAwareBinaryWriter writer, string s)
        {
            var length = Convert.ToInt16(writer.EncodingScheme.GetByteCount(s));
            writer.Write(length);
            writer.WriteString(s);
        }

        private void WriteUid(EndiannessAwareBinaryWriter writer, string s)
        {
            uint uid = uint.Parse(s, System.Globalization.NumberStyles.HexNumber, null);   
            writer.Write(uid);
        }
    }
}

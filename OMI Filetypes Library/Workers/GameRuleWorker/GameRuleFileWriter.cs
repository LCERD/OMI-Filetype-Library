using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System.IO.Compression;
using OMI.Formats.GameRule;
using OMI.Workers;

namespace OMI.Workers.GameRule
{
    public class GameRuleFileWriter : IDataFormatWriter
    {
        private readonly GameRuleFile _grfFile;
        private List<string> StringLookUpTable;

        public GameRuleFileWriter(GameRuleFile grf)
        {
            if (grf.Header.unknownData[3] != 0)
                throw new NotImplementedException("World grf saving is currently unsupported");
            _grfFile = grf;
            StringLookUpTable = new List<string>();
            PrepareLookUpTable(_grfFile.Root, ref StringLookUpTable);
        }

        private void PrepareLookUpTable(GameRuleFile.GameRule rule, ref List<string> lut)
        {
            if (!lut.Contains(rule.Name))
                lut.Add(rule.Name);
            foreach (GameRuleFile.GameRule subRule in rule.ChildRules)
                PrepareLookUpTable(subRule, ref lut);
            foreach (KeyValuePair<string, string> parameter in rule.Parameters)
                if (!lut.Contains(parameter.Key))
                    lut.Add(parameter.Key);
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
            var writer = new EndiannessAwareBinaryWriter(stream, Encoding.ASCII, leaveOpen: true, ByteOrder.BigEndian);
            WriteHeader(writer);
            using (var uncompressed_stream = new MemoryStream())
            {
                var decompressed_writer = new EndiannessAwareBinaryWriter(uncompressed_stream, Encoding.ASCII, leaveOpen: true, ByteOrder.BigEndian);
                WriteBody(decompressed_writer);
                HandleCompression(writer, uncompressed_stream);
            }
        }

        private void HandleCompression(EndiannessAwareBinaryWriter destination, MemoryStream sourceStream)
        {
            byte[] buffer = sourceStream.ToArray();
            int original_length = buffer.Length;

            if (_grfFile.Header.CompressionLevel >= GameRuleFile.CompressionLevel.CompressedRle)
                buffer = CompressRLE(buffer);
            if (_grfFile.Header.CompressionLevel >= GameRuleFile.CompressionLevel.Compressed)
            {
                buffer = Compress(buffer);
                destination.Write(original_length);
                destination.Write(buffer.Length);
            }
            if (_grfFile.Header.CompressionLevel >= GameRuleFile.CompressionLevel.CompressedRleCrc)
                MakeAndWriteCrc(destination, buffer);
            destination.Write(buffer);
        }

        private byte[] Compress(byte[] data)
        {
            var outputStream = new MemoryStream(); // Stream gets Disposed in DeflaterOutputStream

            using (Stream deflateStream = _grfFile.Header.CompressionType switch
            {
                GameRuleFile.CompressionType.Zlib => new DeflaterOutputStream(outputStream),
                GameRuleFile.CompressionType.Deflate => new DeflateStream(outputStream, CompressionLevel.Optimal),
                GameRuleFile.CompressionType.XMem => throw new NotImplementedException(),
                _ => throw new NotImplementedException(),
            })
            {
                deflateStream.Write(data, 0, data.Length);
            }
            return outputStream.ToArray();
        }

        private byte[] CompressRLE(byte[] data) => RLE.Encode(data).ToArray();

        private void MakeAndWriteCrc(EndiannessAwareBinaryWriter writer, byte[] data)
        {
            uint crc = CRC32.CRC(data);
            if (crc != _grfFile.Header.Crc)
            {
                writer.BaseStream.Position = 3;
                writer.Write(crc);
                writer.BaseStream.Seek(0, SeekOrigin.End);
            }
        }

        private void WriteHeader(EndiannessAwareBinaryWriter writer)
        {
            writer.Write((short)1);
            if (_grfFile.Header.CompressionLevel < GameRuleFile.CompressionLevel.None ||
                _grfFile.Header.CompressionLevel > GameRuleFile.CompressionLevel.CompressedRleCrc)
                throw new ArgumentOutOfRangeException(nameof(_grfFile.Header.CompressionLevel));
            writer.Write((byte)_grfFile.Header.CompressionLevel);
            writer.Write(_grfFile.Header.Crc);
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write((byte)0); // <- used in world grf
        }

        private void WriteBody(EndiannessAwareBinaryWriter writer)
        {
            WriteTagLookUpTable(writer);
            WriteFiles(writer);
            WriteGameRuleHierarchy(writer, _grfFile.Root);
        }

        private void WriteFiles(EndiannessAwareBinaryWriter writer)
        {
            writer.Write(_grfFile.Files.Count);
            foreach (GameRuleFile.FileEntry file in _grfFile.Files)
            {
                WriteString(writer, file.Name);
                writer.Write(file.Data.Length);
                writer.Write(file.Data);
            }
        }

        private void WriteTagLookUpTable(EndiannessAwareBinaryWriter writer)
        {
            writer.Write(StringLookUpTable.Count);
            StringLookUpTable.ForEach( s => WriteString(writer, s) );
        }

        private void WriteGameRuleHierarchy(EndiannessAwareBinaryWriter writer, GameRuleFile.GameRule rule)
        {
            writer.Write(rule.ChildRules.Count);
            foreach (GameRuleFile.GameRule subRule in rule.ChildRules)
            {
                SetString(writer, subRule.Name);
                writer.Write(subRule.Parameters.Count);
                foreach (KeyValuePair<string, string> parameter in subRule.Parameters)
                    WriteParameter(writer, parameter);
                WriteGameRuleHierarchy(writer, subRule);
            }
        }

        private void WriteParameter(EndiannessAwareBinaryWriter writer, KeyValuePair<string, string> param)
        {
            SetString(writer, param.Key);
            WriteString(writer, param.Value);
        }

        private void SetString(EndiannessAwareBinaryWriter writer, string s)
        {
            int i = StringLookUpTable.IndexOf(s);
            if (i == -1)
                throw new Exception(nameof(s));
            writer.Write(i);
        }

        private void WriteString(EndiannessAwareBinaryWriter writer, string s)
        {
            writer.Write((short)s.Length);
            writer.WriteString(s);
        }
    }
}

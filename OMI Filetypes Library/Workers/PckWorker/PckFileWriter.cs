using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OMI.Formats.Pck;

namespace OMI.Workers.Pck
{
    public class PckFileWriter : IDataFormatWriter
    {
        private readonly PckFile _pckFile;
        private readonly ByteOrder _byteOrder;
        private readonly IList<string> _parameterList;

        public PckFileWriter(PckFile pckFile, ByteOrder byteOrder, int xmlVersion = -1)
        {
            _pckFile = pckFile;
            if(xmlVersion > -1 && xmlVersion <= 3)
            {
                _pckFile.xmlVersion = xmlVersion;
            }
            _byteOrder = byteOrder;
            _parameterList = pckFile.GetParameterList();
        }

        public void WriteToFile(string filename)
        {
            using (FileStream fs = File.Create(filename))
            {
                WriteToStream(fs);
            }
        }

        public void WriteToStream(Stream stream)
        {
            using (var writer = new EndiannessAwareBinaryWriter(stream,
                _byteOrder == ByteOrder.LittleEndian ? Encoding.Unicode : Encoding.BigEndianUnicode, true, _byteOrder))
            {
                writer.Write(_pckFile.Type);

                bool hasXMLVersion = _pckFile.xmlVersion > 0;

                writer.Write(_parameterList.Count + Convert.ToInt32(hasXMLVersion));
                if(hasXMLVersion)
                    _parameterList.Add(PckFile.XML_VERSION_STRING);
                foreach (var entry in _parameterList)
                {
                        writer.Write(_parameterList.IndexOf(entry));
                        WriteString(writer, entry);
                }
                if (hasXMLVersion)
                {
                    writer.Write(_pckFile.xmlVersion);
                }

                writer.Write(_pckFile.AssetCount);
                IReadOnlyCollection<PckAsset> assets = _pckFile.GetAssets();
                foreach (PckAsset asset in assets)
                {
                    writer.Write(asset.Size);
                    writer.Write((int)asset.Type);
                    WriteString(writer, asset.Filename);
                }

                foreach (PckAsset asset in assets)
                {
                    writer.Write(asset.Parameters.Count);
                    foreach (KeyValuePair<string, string> parameter in asset.Parameters)
                    {
                        if (!_parameterList.Contains(parameter.Key))
                            throw new KeyNotFoundException("Parameter not found in Look Up Table: " + parameter.Key);
                        writer.Write(_parameterList.IndexOf(parameter.Key));
                        WriteString(writer, parameter.Value);
                    }
                    writer.Write(asset.Data);
                }
            }
        }

        private void WriteString(EndiannessAwareBinaryWriter writer, string s)
        {
            writer.Write(s.Length);
            writer.WriteString(s);
            writer.Write(0);
        }
    }
}

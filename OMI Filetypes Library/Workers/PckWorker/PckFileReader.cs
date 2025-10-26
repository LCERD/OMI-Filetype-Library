/* Copyright (c) 2022-present miku-666
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would be
 *    appreciated but is not required.
 * 2. Altered source versions must be plainly marked as such, and must not be
 *    misrepresented as being the original software.
 * 3. This notice may not be removed or altered from any source distribution.
**/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using OMI.Formats.Pck;

namespace OMI.Workers.Pck
{
    public class PckFileReader : IDataFormatReader<PckFile>, IDataFormatReader
    {
        private readonly ByteOrder _byteOrder;

        private IList<PckAsset> _assets;

        /// <summary>
        /// Constructs a new Instance of PckFileReader with the default byte order (<see cref="ByteOrder.BigEndian"/>)
        /// </summary>
        public PckFileReader()
            : this(ByteOrder.BigEndian)
        { }

        public PckFileReader(ByteOrder byteOrder)
        {
            _byteOrder = byteOrder;
        }

        public PckFile FromFile(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException(filename);

            using FileStream fs = File.OpenRead(filename);
            return FromStream(fs);
        }

        public PckFile FromStream(Stream stream)
        {
            PckFile pckFile = null;
            using (var reader = new EndiannessAwareBinaryReader(stream,
                _byteOrder == ByteOrder.LittleEndian ? Encoding.Unicode : Encoding.BigEndianUnicode,
                leaveOpen: true,
                byteOrder: _byteOrder))
            {
                int pckType = reader.ReadInt32();
                if (pckType > 0x00_F0_00_00)
                    throw new OverflowException(nameof(pckType));
                else if (pckType < 3)
                    throw new Exception(pckType.ToString());

                IList<string> propertyList = ReadLookUpTable(reader, out bool hasVersionStr);
                pckFile = new PckFile(pckType, hasVersionStr);
                ReadAssetEntries(reader);
                ReadAssetContents(pckFile, propertyList, reader);
            }
            return pckFile;
        }

        private IList<string> ReadLookUpTable(EndiannessAwareBinaryReader reader, out bool hasVerStr)
        {
            int count = reader.ReadInt32();
            var propertyLookUp = new List<string>(count);
            for (int i = 0; i < count; i++)
            {
                int index = reader.ReadInt32();
                string value = ReadString(reader);
                propertyLookUp.Insert(index, value);
            }
            if (hasVerStr = propertyLookUp.Contains(PckFile.XMLVersionString))
            {
                int xmlVersion = reader.ReadInt32();
                Console.WriteLine($"XML Version num: {xmlVersion}");
            }
            return propertyLookUp;
        }

        private void ReadAssetEntries(EndiannessAwareBinaryReader reader)
        {
            int assetCount = reader.ReadInt32();
            _assets = new List<PckAsset>(assetCount);
            for (; 0 < assetCount; assetCount--)
            {
                int assetSize = reader.ReadInt32();
                var assetType = (PckAssetType)reader.ReadInt32();
                string filename = ReadString(reader).Replace('\\', '/');
                var entry = new PckAsset(filename, assetType, assetSize);
                _assets.Add(entry);
            }
        }

        private void ReadAssetContents(PckFile pckFile, IList<string> propertyList, EndiannessAwareBinaryReader reader)
        {
            foreach (PckAsset asset in _assets)
            {
                int propertyCount = reader.ReadInt32();
                for (; 0 < propertyCount; propertyCount--)
                {
                    string key = propertyList[reader.ReadInt32()];
                    string value = ReadString(reader);
                    asset.Properties.Add(key, value);
                }
                reader.Read(asset.Data, 0, asset.Size);
                pckFile.AddAsset(asset);
            }
            _assets.Clear();
        }

        private string ReadString(EndiannessAwareBinaryReader reader)
        {
            int len = reader.ReadInt32();
            string s = reader.ReadString(len);
            reader.ReadInt32(); // padding
            return s;
        }

        object IDataFormatReader.FromStream(Stream stream) => FromStream(stream);

        object IDataFormatReader.FromFile(string filename) => FromFile(filename);
    }
}
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
using System.IO;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;
using OMI.Extentions;
using OMI.Formats.Ghost;

namespace OMI.Workers.Ghost
{
    public class GhostRecordReader : IDataFormatReader<GhostRecord>, IDataFormatReader
    {
        public GhostRecordReader()
        {
        }

        public GhostRecord FromFile(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException(filename);
            GhostRecord RecordFile;
            using (var fs = File.OpenRead(filename))
            {
                RecordFile = FromStream(fs);
            }
            return RecordFile;
        }

        public GhostRecord FromStream(Stream stream)
        {
            GhostRecord Record = new GhostRecord();

            using (var reader = new EndiannessAwareBinaryReader(stream, Encoding.ASCII, Endianness.BigEndian))
            {
                reader.ReadUInt16();
                Record.startX = reader.ReadSingle();
                Record.startY = reader.ReadSingle();
                Record.startZ = reader.ReadSingle();
                Record.TimeLength = reader.ReadInt64();
                Record.NumSamples = reader.ReadUInt32();
                if(Record.NumSamples != 0)
                {
                    for (uint uVar5 = Record.NumSamples >> 3; uVar5 != 0; uVar5 = uVar5 - 1)
                    {
                        Record.Add(ReadSample(reader));
                        Record.Add(ReadSample(reader));
                        Record.Add(ReadSample(reader));
                        Record.Add(ReadSample(reader));
                        Record.Add(ReadSample(reader));
                        Record.Add(ReadSample(reader));
                        Record.Add(ReadSample(reader));
                        Record.Add(ReadSample(reader));
                    }
                    uint TempSamples = Record.NumSamples;
                    for (TempSamples = TempSamples & 7; TempSamples != 0; TempSamples = TempSamples - 1)
                    {
                        Record.Add(ReadSample(reader));
                    }
                }
            }
            return Record;
        }

        private GhostRecord.RecordSample ReadSample(EndiannessAwareBinaryReader reader)
        {
            GhostRecord.RecordSample _sample = new GhostRecord.RecordSample();
            _sample.Timestamp = reader.ReadInt64();
            _sample.TimestampAsTime = TimeSpan.FromMilliseconds(_sample.Timestamp);
            _sample.Position[0] = reader.ReadInt16();
            _sample.Position[1] = reader.ReadInt16();
            _sample.Position[2] = reader.ReadInt16();
            _sample.Rotation[0] = reader.ReadInt16();
            _sample.Rotation[1] = reader.ReadInt16();
            _sample.Rotation[2] = reader.ReadInt16();
            return _sample;
        }

        object IDataFormatReader.FromStream(Stream stream) => FromStream(stream);

        object IDataFormatReader.FromFile(string filename) => FromFile(filename);
    }
}

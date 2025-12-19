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
            GhostRecord recordFile;
            using (FileStream fs = File.OpenRead(filename))
            {
                recordFile = FromStream(fs);
            }
            return recordFile;
        }

        public GhostRecord FromStream(Stream stream)
        {
            GhostRecord record = new GhostRecord();

            using (var reader = new EndiannessAwareBinaryReader(stream, Encoding.ASCII, ByteOrder.BigEndian))
            {
                reader.ReadUInt16();
                record.startX = reader.ReadSingle();
                record.startY = reader.ReadSingle();
                record.startZ = reader.ReadSingle();
                record.TimeLength = reader.ReadInt64();
                record.NumSamples = reader.ReadUInt32();
                if(record.NumSamples != 0)
                {
                    for (uint uVar5 = record.NumSamples >> 3; uVar5 != 0; uVar5 = uVar5 - 1)
                    {
                        record.Add(ReadSample(reader));
                        record.Add(ReadSample(reader));
                        record.Add(ReadSample(reader));
                        record.Add(ReadSample(reader));
                        record.Add(ReadSample(reader));
                        record.Add(ReadSample(reader));
                        record.Add(ReadSample(reader));
                        record.Add(ReadSample(reader));
                    }
                    uint tempSamples = record.NumSamples;
                    for (tempSamples = tempSamples & 7; tempSamples != 0; tempSamples = tempSamples - 1)
                    {
                        record.Add(ReadSample(reader));
                    }
                }
            }
            return record;
        }

        private GhostRecord.RecordSample ReadSample(EndiannessAwareBinaryReader reader)
        {
            GhostRecord.RecordSample sample = new GhostRecord.RecordSample();
            sample.Timestamp = reader.ReadInt64();
            sample.TimestampAsTime = TimeSpan.FromMilliseconds(sample.Timestamp);
            sample.Position[0] = reader.ReadInt16();
            sample.Position[1] = reader.ReadInt16();
            sample.Position[2] = reader.ReadInt16();
            sample.Rotation[0] = reader.ReadInt16();
            sample.Rotation[1] = reader.ReadInt16();
            sample.Rotation[2] = reader.ReadInt16();
            return sample;
        }

        object IDataFormatReader.FromStream(Stream stream) => FromStream(stream);

        object IDataFormatReader.FromFile(string filename) => FromFile(filename);
    }
}

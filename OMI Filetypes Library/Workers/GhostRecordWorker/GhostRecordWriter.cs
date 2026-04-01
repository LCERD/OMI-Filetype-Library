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
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using OMI.Formats.Ghost;

namespace OMI.Workers.Ghost
{
    public class GhostRecordWriter : IDataFormatWriter
    {
        private GhostRecord _record;
        public GhostRecordWriter(GhostRecord record)
        {
            _record = record;
        }

        public void WriteToFile(string fileName)
        {
            using (FileStream fs = File.OpenWrite(fileName))
            {
                WriteToStream(fs);
            }
        }

        public void WriteToStream(Stream stream)
        {
            byte[] buffer = new byte[0x5DCC];
            using (var writer = new EndiannessAwareBinaryWriter(stream, Encoding.ASCII, leaveOpen: true, ByteOrder.BigEndian))
            {
                writer.Write(buffer);
                writer.BaseStream.Position = 0;
                writer.Write((short)4);
                writer.Write(_record.startX);
                writer.Write(_record.startY);
                writer.Write(_record.startZ);
                writer.Write(_record.TimeLength);
                writer.Write((UInt32)_record.Count);

                foreach(GhostRecord.RecordSample sample in _record)
                {
                    writer.Write(sample.Timestamp);
                    writer.Write(sample.Position[0]);
                    writer.Write(sample.Position[1]);
                    writer.Write(sample.Position[2]);
                    writer.Write(sample.Rotation[0]);
                    writer.Write(sample.Rotation[1]);
                    writer.Write(sample.Rotation[2]);
                }


            }
        }
    }
}

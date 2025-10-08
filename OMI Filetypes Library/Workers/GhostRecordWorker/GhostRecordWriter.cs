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
        private GhostRecord _Record;
        public GhostRecordWriter(GhostRecord _record)
        {
            _Record = _record;
        }

        public void WriteToFile(string fileName)
        {
            using (var fs = File.OpenWrite(fileName))
            {
                WriteToStream(fs);
            }
        }

        public void WriteToStream(Stream stream)
        {
            byte[] Buff = new byte[0x5DCC];
            using (var writer = new EndiannessAwareBinaryWriter(stream, Encoding.ASCII, leaveOpen: true, Endianness.BigEndian))
            {
                writer.Write(Buff);
                writer.BaseStream.Position = 0;
                writer.Write((short)4);
                writer.Write(_Record.startX);
                writer.Write(_Record.startY);
                writer.Write(_Record.startZ);
                writer.Write(_Record.TimeLength);
                writer.Write((UInt32)_Record.Count);

                foreach(GhostRecord.RecordSample sample in _Record)
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

        private static int GetRGBAFromColor(System.Drawing.Color color)
        {
            int argb = color.ToArgb();
            return (argb & 0xffffff) << 8 | argb >> 24 & 0xff;
        }

        private void WriteByte(Stream stream, byte b)
        {
            stream.WriteByte(b);
        }
    }
}

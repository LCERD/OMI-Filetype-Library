/* Copyright (c) 2023-present miku-666
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

namespace OMI
{
    public sealed class EndiannessAwareBinaryWriter : BinaryWriter
    {
        private readonly ByteOrder _byteOrder = ByteOrder.LittleEndian;
        private readonly Encoding _encoding = Encoding.UTF8;
        public Encoding EncodingScheme => _encoding;

        public EndiannessAwareBinaryWriter(Stream output) : base(output)
        {
        }

        public EndiannessAwareBinaryWriter(Stream output, Encoding encoding) : base(output, encoding)
        {
            _encoding = encoding;
        }

        public EndiannessAwareBinaryWriter(Stream output, Encoding encoding, bool leaveOpen) : base(output, encoding, leaveOpen)
        {
            _encoding = encoding;
        }

        public EndiannessAwareBinaryWriter(Stream output, ByteOrder byteOrder) : base(output)
        {
            _byteOrder = byteOrder;
        }

        public EndiannessAwareBinaryWriter(Stream output, Encoding encoding, ByteOrder byteOrder) : base(output, encoding)
        {
            _byteOrder = byteOrder;
            _encoding = encoding;
        }

        public EndiannessAwareBinaryWriter(Stream output, Encoding encoding, bool leaveOpen, ByteOrder byteOrder) : base(output, encoding, leaveOpen)
        {
            _byteOrder = byteOrder;
            _encoding = encoding;
        }

        private static void CheckEndiannessAndSwapBuffer(ref byte[] buffer, ByteOrder byteOrder)
        {
            if (!BitConverter.IsLittleEndian ||
                byteOrder == ByteOrder.BigEndian)
                Array.Reverse(buffer);
        }

        public override void Write(short value) => Write(value, _byteOrder);
        public override void Write(ushort value) => Write((short)value, _byteOrder);
        public override void Write(int value) => Write(value, _byteOrder);
        public override void Write(uint value) => Write((int)value, _byteOrder);
        public override void Write(long value) => Write(value, _byteOrder);
        public override void Write(ulong value) => Write((long)value, _byteOrder);
        public override void Write(float value) => Write(value, _byteOrder);

        public void Write(short value, ByteOrder byteOrder)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            CheckEndiannessAndSwapBuffer(ref buffer, byteOrder);
            Write(buffer);
        }

        public void Write(int value, ByteOrder byteOrder)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            CheckEndiannessAndSwapBuffer(ref buffer, byteOrder);
            Write(buffer);
        }

        public void Write(long value, ByteOrder byteOrder)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            CheckEndiannessAndSwapBuffer(ref buffer, byteOrder);
            Write(buffer);
        }

        public void Write(float value, ByteOrder byteOrder)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            CheckEndiannessAndSwapBuffer(ref buffer, byteOrder);
            Write(buffer);
        }

        /// <summary>
        /// Writes a string to the given <see cref="BinaryWriter.BaseStream"/> using the provided <see cref="EncodingScheme"/>
        /// </summary>
        /// <param name="s">String to write</param>
        /// <param name="maxCapacity">Maximum capacity the string can use</param>
        public void WriteString(string s, int maxCapacity) => WriteString(s, maxCapacity, _encoding);

        /// <summary>
        /// Writes a string to the given <see cref="BinaryWriter.BaseStream"/> using the provided <see cref="EncodingScheme"/>
        /// </summary>
        /// <param name="s">String to write</param>
        public void WriteString(string s) => WriteString(s, _encoding);

        public void WriteString(string s, int maxCapacity, Encoding encoding)
        {
            if (s.Length > maxCapacity)
            {
                throw new ArgumentException($"String cannot be longer than the max capacity specified({maxCapacity})!");
            }
            byte[] buffer = new byte[maxCapacity];
            encoding.GetBytes(s, 0, s.Length, buffer, 0);
            Write(buffer);
        }

        public void WriteString(string s, Encoding encoding)
        {
            byte[] buffer = encoding.GetBytes(s);
            Write(buffer);
        }
    }
}

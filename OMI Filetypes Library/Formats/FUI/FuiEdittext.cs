/*
 * all known FourJUserInterface information is the direct product of Miku-666(NessieHax)'s work! check em out! 
 * https://github.com/NessieHax
 * See License usage at the bottom of file!
*/
using System.Drawing;

namespace OMI.Formats.FUI
{
    public class FuiEdittext
    {
        public enum TextAlignment : byte
        {
            Left = 0,
            Right = 1,
            Center = 2,
        }

        internal int Unknown0; // name..
        public RectangleF Rectangle;
        public int FontId;
        public float FontScale;
        public System.Drawing.Color Color;
        public StringAlignment Alignment; // 0 - 2
        public byte _a;
        public byte _b;
        public byte _c;
        public int Unknown3; // left margin
        public int Unknown4; // right margin
        public int Unknown5; // indent
        public float Unknown6; // leading ?
        public int Unknown7;
        /// <summary>
        /// Max size: 0x100
        /// </summary>
        public string htmlSource;
    }
}
/* Copyright (c) 2026-present miku-666
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
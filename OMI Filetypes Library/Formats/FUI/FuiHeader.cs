/*
 * all known FourJUserInterface information is the direct product of Miku-666(NessieHax)'s work! check em out! 
 * https://github.com/NessieHax
 * See License usage at the bottom of file!
*/
using System.Drawing;

namespace OMI.Formats.FUI
{
    public sealed class FuiHeader
    {
        public static readonly byte DefaultVersion = 1;
        public static readonly string DefaultSignature = "\x01IUF";

        public byte Version => (byte)(Signature[0] & 0xff);

        public readonly string Signature;
        public readonly string SwfFileName;
        public readonly RectangleF FrameSize;

        public FuiHeader(string swfFileName, RectangleF frameSize) : this(DefaultSignature, swfFileName, frameSize)
        {

        }

        internal FuiHeader(string signature, string swfFileName, RectangleF frameSize)
        {
            Signature = signature;
            SwfFileName = swfFileName;
            FrameSize = frameSize;
        }

        public override string ToString()
        {
            return $"SwfFileName: {SwfFileName} | Frame Size: {FrameSize} | Signature: {Signature} | Version: {Version}";
        }
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
/*
 * all known FourJUserInterface information is the direct product of Miku-666(NessieHax)'s work! check em out! 
 * https://github.com/NessieHax
 * See License usage at the bottom of file!
*/
using System.Drawing;

namespace OMI.Formats.FUI
{
    public class FuiBitmap
    {
        public enum FuiImageFormat
        {
            PNG_WITH_ALPHA_DATA = 1,
            PNG_NO_ALPHA_DATA = 3,
            JPEG_NO_ALPHA_DATA = 6,
            /// <summary>
            /// <see cref="ZlibDataOffset"/> has to be set!
            /// </summary>
            JPEG_WITH_ALPHA_DATA = 8
        }

        public int SymbolIndex { get; }
        public FuiImageFormat ImageFormat { get; }
        public Image Image { get; }

        public FuiBitmap(Image image, FuiImageFormat imageFormat, int symbolIndex = -1)
        {
            SymbolIndex = symbolIndex;
            ImageFormat = imageFormat;
            Image = image;
        }

        public static implicit operator Image(FuiBitmap fuiBitmap) => fuiBitmap.Image;
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
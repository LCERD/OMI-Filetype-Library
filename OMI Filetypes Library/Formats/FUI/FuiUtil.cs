/*
 * all known FourJUserInterface information is the direct product of Miku-666(NessieHax)'s work! check em out! 
 * https://github.com/NessieHax
 * See License usage at the bottom of file!
*/
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace OMI.Formats.FUI
{
    internal static class FuiUtil
    {
        static ColorMatrix r2b_mat = new ColorMatrix(
        [
        //src:R   G   B   A   W
            [0f, 0f, 1f, 0f, 0f], // R
            [0f, 1f, 0f, 0f, 0f], // G
            [1f, 0f, 0f, 0f, 0f], // B
            [0f, 0f, 0f, 1f, 0f], // A
            [0f, 0f, 0f, 0f, 1f], // W <= Brightness
                                  // dest
        ]);

        public static Image ReverseColorRB(this Image img)
        {
            Size s = img.Size;
            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(r2b_mat, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            if (img.PixelFormat != PixelFormat.Format32bppArgb)
            {
                // Source - https://stackoverflow.com/a/2016509
                // Posted by Hans Passant, modified by community. See post 'Timeline' for change history
                // Retrieved 2026-01-10, License - CC BY-SA 3.0

                Bitmap clone = new Bitmap(img.Width, img.Height, PixelFormat.Format32bppArgb);
                using (Graphics gr = Graphics.FromImage(clone))
                {
                    gr.DrawImage(img, 0, 0);
                }
                img = clone;
            }

            using (Graphics g = Graphics.FromImage(img))
            {
                g.DrawImage(img, new Rectangle(Point.Empty, s), 0, 0, s.Width, s.Height, GraphicsUnit.Pixel, imageAttributes);
            }
            imageAttributes.Dispose();
            return img;
        }

        public static Image SetAlphaData(this Image img, byte[] alphaData)
        {
            var result = new Bitmap(img);

            BitmapData pixelData = result.LockBits(new Rectangle(Point.Empty, img.Size), ImageLockMode.ReadWrite, PixelFormat.Format32bppPArgb);

            int stride = pixelData.Stride;
            IntPtr pixelBufferStart = pixelData.Scan0;
            const int COLOR_LENGTH = 4;
            int[] color = new int[1];
            for (int i = 0; i < img.Width * img.Height; i++)
            {
                int y = Math.DivRem(i, img.Width, out int x);
                IntPtr pixelOffset = pixelBufferStart + (COLOR_LENGTH * x) + (stride * y);

                Marshal.Copy(pixelOffset, color, 0, 1);
                byte alpha = alphaData[i];

                if (alpha == 0)
                {
                    Marshal.Copy(new int[1], 0, pixelOffset, 1);
                    continue;
                }
                color[0] = (alpha << 24) | (color[0] & 0xffffff);
                Marshal.Copy(color, 0, pixelOffset, 1);
            }
            result.UnlockBits(pixelData);
            return result;
        }

        public static byte[] GetAlphaData(this Image img)
        {
            var result = new Bitmap(img);

            BitmapData pixelData = result.LockBits(new Rectangle(Point.Empty, img.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);

            int stride = pixelData.Stride;
            IntPtr pixelBufferStart = pixelData.Scan0;
            const int COLOR_LENGTH = 4;
            int[] color = new int[1];
            byte[] data = new byte[img.Width * img.Height];
            for (int i = 0; i < img.Width * img.Height; i++)
            {
                int y = Math.DivRem(i, img.Width, out int x);
                IntPtr pixelOffset = pixelBufferStart + (COLOR_LENGTH * x) + (stride * y);

                Marshal.Copy(pixelOffset, color, 0, 1);
                data[i] = (byte)((color[0] >> 24) & 0xff);
            }
            result.UnlockBits(pixelData);
            return data;
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
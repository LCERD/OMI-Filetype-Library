/*
 * all known FourJUserInterface information is the direct product of Miku-666(NessieHax)'s work! check em out! 
 * https://github.com/NessieHax
 * See License usage at the bottom of file!
*/
using System.Diagnostics;
using System.Drawing.Imaging;

namespace OMI.Formats.FUI
{
    public struct ColorF(float r, float g, float b, float a)
    {
        public float R = r;
        public float G = g;
        public float B = b;
        public float A = a;

        public override string ToString() => $"[R: {R}; G: {G}; B: {B}; A: {A}]";
    }

    [DebuggerDisplay("Mult = ({Multiplier}); Offset = ({Offset})")]
    public struct FuiColorTransform
    {
        public ColorF Offset;

        public ColorF Multiplier;

        public FuiColorTransform() : this(new ColorF(1f, 1f, 1f, 1f), new ColorF(0f, 0f, 0f, 0f))
        {
        }

        public FuiColorTransform(ColorF multiplier, ColorF offset)
        {
            Multiplier = multiplier;
            Offset = offset;
        }

        public bool IsIdentity
        {
            get
            {
                return (Multiplier.R == 1f && Multiplier.G == 1f && Multiplier.B == 1f && Multiplier.A == 1f);
            }
        }

        public bool IsEmpty
        {
            get
            {
                return (Multiplier.R == 0f && Multiplier.G == 0f && Multiplier.B == 0f && Multiplier.A == 0f) &&
                        (Offset.R == 0f && Offset.G == 0f && Offset.B == 0f && Offset.A == 0f);
            }
        }

        public static implicit operator ColorMatrix(FuiColorTransform ct)
            => new ColorMatrix(new float[][]
            {
                [ct.Multiplier.R, 0f, 0f, 0f, 0f],
                [0f, ct.Multiplier.G, 0f, 0f, 0f],
                [0f, 0f, ct.Multiplier.B, 0f, 0f],
                [0f, 0f, 0f, ct.Multiplier.A, 0f],
                [ct.Offset.R, ct.Offset.G, ct.Offset.B, ct.Offset.A, 1f]
            });
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

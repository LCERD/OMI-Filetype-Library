/*
 * all known FourJUserInterface information is the direct product of Miku-666(NessieHax)'s work! check em out! 
 * https://github.com/NessieHax
 * See License usage at the bottom of file!
*/
using System.Drawing.Imaging;

namespace OMI.Formats.FUI
{
    public struct FuiColorTransform
    {
        public float RedMultTerm;
        public float GreenMultTerm;
        public float BlueMultTerm;
        public float AlphaMultTerm;

        public float RedAddTerm;
        public float GreenAddTerm;
        public float BlueAddTerm;
        public float AlphaAddTerm;

        public FuiColorTransform()
        {
            RedMultTerm = 1f;
            GreenMultTerm = 1f;
            BlueMultTerm = 1f;
            AlphaMultTerm = 1f;
        }

        public bool IsIdentity
        {
            get
            {
                return (RedMultTerm == 1f && GreenMultTerm == 1f && BlueMultTerm == 1f && AlphaMultTerm == 1f);
            }
        }

        public bool IsEmpty
        {
            get
            {
                return (RedMultTerm == 0f && GreenMultTerm == 0f && BlueMultTerm == 0f && AlphaMultTerm == 0f) &&
                        (RedAddTerm == 0f && GreenAddTerm == 0f && BlueAddTerm == 0f && AlphaAddTerm == 0f);
            }
        }

        public static implicit operator ColorMatrix(FuiColorTransform ct)
            => new ColorMatrix(new float[][]
            {
                [ct.RedMultTerm, 0f, 0f, 0f, 0f],
                [0f, ct.GreenMultTerm, 0f, 0f, 0f],
                [0f, 0f, ct.BlueMultTerm, 0f, 0f],
                [0f, 0f, 0f, ct.AlphaMultTerm, 0f],
                [ct.RedAddTerm, ct.GreenAddTerm, ct.BlueAddTerm, ct.AlphaAddTerm, 1f]
            });

        public override string ToString()
        {
            return
                $"(RGBA) Mult = ({RedMultTerm}, {GreenMultTerm}, {BlueMultTerm}, {AlphaMultTerm}); (RGBA) Add = ({RedAddTerm}, {GreenAddTerm}, {BlueAddTerm}, {AlphaAddTerm})";
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

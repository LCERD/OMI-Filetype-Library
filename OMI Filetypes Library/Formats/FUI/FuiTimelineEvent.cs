/*
 * all known FourJUserInterface information is the direct product of Miku-666(NessieHax)'s work! check em out! 
 * https://github.com/NessieHax
 * See License usage at the bottom of file!s
*/
using System;
using System.Numerics;


namespace OMI.Formats.FUI
{
    public class FuiTimelineEvent
    {
        public FuiTimelineEvent(
            string name, EventFlags eventType,
            short depth, fuiObjectType objectType,
            short index, short unknown1,
            Matrix3x2 matrix, FuiColorTransform colorTransform, System.Drawing.Color color)
        {
            EventType = eventType;
            ObjectType = objectType;
            Depth = depth;
            Index = index;
            Unknown1 = unknown1;
            Name = name;
            Matrix = matrix;
            ColorTransform = colorTransform;
            Color = color;
        }

        [Flags]
        public enum EventFlags : ushort
        {
            None = 0x00,
            PlaceObj = 0x01,
            RemoveObj = 0x02,
            UpdateTransform = 0x04,
            UpdateColor = 0x08,
        }
        public EventFlags EventType { get; }
        public fuiObjectType ObjectType { get; }
        public short Depth { get; }
        public short Index { get; }
        public short Unknown1 { get; }
        public string Name { get; }
        public Matrix3x2 Matrix { get; }
        public FuiColorTransform ColorTransform { get; }
        public System.Drawing.Color Color { get; }

        public override string ToString()
        {
            return $"EventType: {EventType} | ObjectType: {ObjectType} | Depth: {Depth} | Index: {Index} | Unknown1: {Unknown1} | Name: {Name} | Matrix: {Matrix} | ColorTransform: {ColorTransform} | Color: {Color}";
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
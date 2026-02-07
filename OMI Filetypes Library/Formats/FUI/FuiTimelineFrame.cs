/*
 * all known FourJUserInterface information is the direct product of Miku-666(NessieHax)'s work! check em out! 
 * https://github.com/NessieHax
 * See License usage at the bottom of file!
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace OMI.Formats.FUI
{
    [DebuggerDisplay("Frame Name: '{FrameName}' Event count: {Events.Count}")]
    public class FuiTimelineFrame
    {
        public FuiTimelineFrame(string frameName, IEnumerable<FuiTimelineEvent> events = default)
        {
            FrameName = frameName ?? string.Empty;
            if (FrameName.Length > 0x40)
                Debug.Fail("Frame name to long");
            Events = new (events ?? Enumerable.Empty<FuiTimelineEvent>());
        }

        public string FrameName { get; }
        public List<FuiTimelineEvent> Events { get; }

        public FuiTimelineEvent GetNamedEvent(string name) => Events.FirstOrDefault(e => e.Name == name);

        public void PlaceNewObject(short depth, fuiObjectType objectType, short index, Matrix3x2 matrix, FuiColorTransform colorTransform = default)
        {
            Events.Add(new FuiTimelineEvent("", FuiTimelineEvent.EventFlags.PlaceObj | FuiTimelineEvent.EventFlags.UpdateTransform, depth, objectType, index, 0, matrix, colorTransform, System.Drawing.Color.Black));
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
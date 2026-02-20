/*
 * all known FourJUserInterface information is the direct product of Miku-666(NessieHax)'s work! check em out! 
 * https://github.com/NessieHax
 * See License usage at the bottom of file!
*/
using System.Diagnostics;

namespace OMI.Formats.FUI
{
    [DebuggerDisplay("[Frame:{FrameIndex}]: {Type} {Arg0} = {Arg1}")]
    public class FuiTimelineAction
    {
        public enum ActionType : ushort
        {
            HandleEvent = 0, // handleAnimationStep, UpdateLabel, InitHud, AnimationEnd, ShowTimerAnimation, SlideComplete, ShowAnimatedLogoText
            Pause = 1,
            SetFrame = 2,
            SetFrameAndStart = 3,

            SetTabIndex = 4, //! Set StringArg1 to a valid number (e.g. 1-9)
            SetLabelTextAlignment = 5,
            SetLabelProperty0 = 6,
            SetLabelProperty1 = 7,
            SetLabelProperty2 = 8,
            DoActionOn = 9,  //! Set StringArg1 specific to object used (e.g. List, Label, etc.)

            SetValue = 16,
            SetVisible = 17,



            DoListAction = 29, //! Calls 'FJ_List::setAction' or 'FJ_List2D::setAction'
            SelectFrame = 30, //! ?
            AnimationEnd = 31, // Add named animation ?
        }

        public ActionType Type;
        public short FrameIndex;
        public string Arg0;
        public string Arg1;
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

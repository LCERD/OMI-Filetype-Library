/* Copyright (c) 2022-present miku-666
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
using System.Drawing;
using System.IO;
using System.Text;
using OMI.Formats.FUI;
using OMI.Formats.FUI.Components;
/*
* all known FourJUserInterface information is the direct product of Miku-666(NessieHax)'s work! check em out! 
* https://github.com/NessieHax
*/
namespace OMI.Workers.FUI
{
    public class FourjUIWriter : IDataFormatWriter
    {
        private FourjUserInterface _UIContainer;
        public FourjUIWriter(FourjUserInterface container)
        {
            _UIContainer = container;
        }

        public void WriteToFile(string fileName)
        {
            using (FileStream fs = File.OpenWrite(fileName))
            {
                WriteToStream(fs);
            }
        }

        public void WriteToStream(Stream stream)
        {
            using (var writer = new EndiannessAwareBinaryWriter(stream, Encoding.ASCII, leaveOpen: true, ByteOrder.LittleEndian))
            {
                writer.Write(_UIContainer.Header.Signature);

                writer.Write(_UIContainer.Header.ContentSize);
                writer.WriteString(_UIContainer.Header.SwfFileName, 0x40);
                writer.Write(_UIContainer.Timelines.Count);
                writer.Write(_UIContainer.TimelineEventNames.Count);
                writer.Write(_UIContainer.TimelineActions.Count);
                writer.Write(_UIContainer.Shapes.Count);
                writer.Write(_UIContainer.ShapeComponents.Count);
                writer.Write(_UIContainer.Verts.Count);
                writer.Write(_UIContainer.TimelineFrames.Count);
                writer.Write(_UIContainer.TimelineEvents.Count);
                writer.Write(_UIContainer.References.Count);
                writer.Write(_UIContainer.Edittexts.Count);
                writer.Write(_UIContainer.Symbols.Count);
                writer.Write(_UIContainer.Bitmaps.Count);
                writer.Write(0);
                writer.Write(_UIContainer.FontNames.Count);
                writer.Write(_UIContainer.ImportAssets.Count);
                writer.Write(_UIContainer.Header.FrameSize.X);
                writer.Write(_UIContainer.Header.FrameSize.Y);
                writer.Write(_UIContainer.Header.FrameSize.X + _UIContainer.Header.FrameSize.Width);
                writer.Write(_UIContainer.Header.FrameSize.Y + _UIContainer.Header.FrameSize.Height);

                foreach (FuiTimeline timeline in _UIContainer.Timelines)
                {
                    writer.Write(timeline.SymbolIndex);
                    writer.Write(timeline.FrameIndex);
                    writer.Write(timeline.FrameCount);
                    writer.Write(timeline.ActionIndex);
                    writer.Write(timeline.ActionCount);
                    writer.Write(timeline.Area.X);
                    writer.Write(timeline.Area.Y);
                    writer.Write(timeline.Area.X + timeline.Area.Width);
                    writer.Write(timeline.Area.Y + timeline.Area.Height);
                }
                foreach (FuiTimelineAction timelineAction in _UIContainer.TimelineActions)
                {
                    writer.Write((ushort)timelineAction.Type);
                    writer.Write(timelineAction.FrameIndex);
                    writer.WriteString(timelineAction.StringArg0, 0x40);
                    writer.WriteString(timelineAction.StringArg1, 0x40);
                }
                foreach (FuiShape shape in _UIContainer.Shapes)
                {
                    writer.Write(shape.Unknown);
                    writer.Write(shape.ShapeComponentIndex);
                    writer.Write(shape.ShapeComponentCount);
                    writer.Write(shape.Area.X);
                    writer.Write(shape.Area.Y);
                    writer.Write(shape.Area.X + shape.Area.Width);
                    writer.Write(shape.Area.Y + shape.Area.Height);
                }
                foreach (FuiShapeComponent shapeComponent in _UIContainer.ShapeComponents)
                {
                    writer.Write((int)shapeComponent.FillInfo.Type);
                    writer.Write(GetRGBAFromColor(shapeComponent.FillInfo.Color));
                    writer.Write(shapeComponent.FillInfo.BitmapIndex);
                    writer.Write(shapeComponent.FillInfo.Matrix.M11);
                    writer.Write(shapeComponent.FillInfo.Matrix.M22);
                    writer.Write(shapeComponent.FillInfo.Matrix.M12);
                    writer.Write(shapeComponent.FillInfo.Matrix.M21);
                    writer.Write(shapeComponent.FillInfo.Matrix.Translation.X);
                    writer.Write(shapeComponent.FillInfo.Matrix.Translation.Y);
                    writer.Write(shapeComponent.VertIndex);
                    writer.Write(shapeComponent.VertCount);
                }
                foreach (PointF vert in _UIContainer.Verts)
                {
                    writer.Write(vert.X);
                    writer.Write(vert.Y);
                }
                foreach (FuiTimelineFrame timelineFrame in _UIContainer.TimelineFrames)
                {
                    writer.WriteString(timelineFrame.FrameName, 0x40);
                    writer.Write(timelineFrame.EventIndex);
                    writer.Write(timelineFrame.EventCount);
                }
                foreach (FuiTimelineEvent timelineEvent in _UIContainer.TimelineEvents)
                {
                    writer.Write((ushort)timelineEvent.EventType);
                    writer.Write((byte)timelineEvent.ObjectType);
                    writer.Write((byte)0);
                    writer.Write(timelineEvent.Unknown0);
                    writer.Write(timelineEvent.Index);
                    writer.Write(timelineEvent.Unknown1);
                    writer.Write(timelineEvent.NameIndex);
                    writer.Write(timelineEvent.Matrix.M11);
                    writer.Write(timelineEvent.Matrix.M22);
                    writer.Write(timelineEvent.Matrix.M12);
                    writer.Write(timelineEvent.Matrix.M21);
                    writer.Write(timelineEvent.Matrix.Translation.X);
                    writer.Write(timelineEvent.Matrix.Translation.Y);
                    writer.Write(timelineEvent.ColorTransform.RedMultTerm);
                    writer.Write(timelineEvent.ColorTransform.GreenMultTerm);
                    writer.Write(timelineEvent.ColorTransform.BlueMultTerm);
                    writer.Write(timelineEvent.ColorTransform.AlphaMultTerm);
                    writer.Write(timelineEvent.ColorTransform.RedAddTerm);
                    writer.Write(timelineEvent.ColorTransform.GreenAddTerm);
                    writer.Write(timelineEvent.ColorTransform.BlueAddTerm);
                    writer.Write(timelineEvent.ColorTransform.AlphaAddTerm);
                    writer.Write(GetRGBAFromColor(timelineEvent.Color));
                }
                foreach (var eventName in _UIContainer.TimelineEventNames)
                {
                    writer.WriteString(eventName, 0x40);
                }
                foreach (FuiReference reference in _UIContainer.References)
                {
                    writer.Write(reference.SymbolIndex);
                    writer.WriteString(reference.Name, 0x40);
                    writer.Write(reference.Index);
                }
                foreach (FuiEdittext edittext in _UIContainer.Edittexts)
                {
                    writer.Write(edittext.Unknown0);
                    writer.Write(edittext.Rectangle.X);
                    writer.Write(edittext.Rectangle.Y);
                    writer.Write(edittext.Rectangle.X + edittext.Rectangle.Width);
                    writer.Write(edittext.Rectangle.Y + edittext.Rectangle.Height);
                    writer.Write(edittext.FontId);
                    writer.Write(edittext.FontScale);
                    writer.Write(GetRGBAFromColor(edittext.Color));
                    writer.Write((int)edittext.Alignment);
                    writer.Write(edittext.Unknown3);
                    writer.Write(edittext.Unknown4);
                    writer.Write(edittext.Unknown5);
                    writer.Write(edittext.Unknown6);
                    writer.Write(edittext.Unknown7);
                    writer.WriteString(edittext.htmlSource, 0x100);
                }
                foreach (FuiFontName fontName in _UIContainer.FontNames)
                {
                    writer.Write(fontName.ID);
                    writer.WriteString(fontName.Name, 0x40);
                    writer.Write(new byte[0xC0]);
                }
                foreach (FuiSymbol symbol in _UIContainer.Symbols)
                {
                    writer.WriteString(symbol.Name, 0x40);
                    writer.Write((int)symbol.ObjectType);
                    writer.Write(symbol.Index);
                }
                foreach (var importAssetName in _UIContainer.ImportAssets)
                {
                    writer.WriteString(importAssetName, 0x40);
                }
                foreach (FuiBitmap bitmap in _UIContainer.Bitmaps)
                {
                    writer.Write(bitmap.SymbolIndex);
                    writer.Write((int)bitmap.ImageFormat);
                    writer.Write(bitmap.ImageSize.Width);
                    writer.Write(bitmap.ImageSize.Height);
                    writer.Write(bitmap.Offset);
                    writer.Write(bitmap.Size);
                    writer.Write(bitmap.ZlibDataOffset);
                    writer.Write(bitmap.BindHandle);
                }
                foreach (var imgData in _UIContainer.ImagesData)
                {
                    writer.Write(imgData);
                }
            }
        }

        private static int GetRGBAFromColor(System.Drawing.Color color)
        {
            int argb = color.ToArgb();
            return (argb & 0xffffff) << 8 | argb >> 24 & 0xff;
        }
    }
}

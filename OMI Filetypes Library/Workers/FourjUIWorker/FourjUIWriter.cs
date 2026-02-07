/*
 * all known FourJUserInterface information is the direct product of Miku-666(NessieHax)'s work! check em out! 
 * https://github.com/NessieHax
 * See License details at the bottom of file!
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using OMI.Formats.FUI;

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
                writer.WriteString(_UIContainer.Header.Signature, 8);

                int timelineEventNamesCount = _UIContainer.Timelines
                    .SelectMany(tl => tl.Frames
                        .SelectMany(tlf => tlf.Events
                            .Where(tle => !string.IsNullOrWhiteSpace(tle.Name))
                            .Select(tle => tle.Name)
                        )
                    )
                    .Count();

                int timelineEventsCount = _UIContainer.Timelines
                    .SelectMany(tl => tl.Frames.SelectMany(tle => tle.Events)
                    )
                    .Count();

                int timelineFramesCount = _UIContainer.Timelines.SelectMany(tl => tl.Frames).Count();
                int timelineActionsCount = _UIContainer.Timelines.SelectMany(tl => tl.Actions).Count();
                int shapeComponentsCount = _UIContainer.Shapes.SelectMany(tl => tl.Components).Count();
                int vertsCount = _UIContainer.Shapes.SelectMany(tl => tl.Components.SelectMany(c => c.Verts)).Count();

                long contentSizeOffset = writer.BaseStream.Position;
                writer.Write(0);
                writer.WriteString(_UIContainer.Header.SwfFileName, 0x40);
                writer.Write((int)_UIContainer.Timelines.Count);
                writer.Write((int)timelineEventNamesCount);
                writer.Write((int)timelineActionsCount);
                writer.Write((int)_UIContainer.Shapes.Count);
                writer.Write((int)shapeComponentsCount);
                writer.Write((int)vertsCount);
                writer.Write((int)timelineFramesCount);
                writer.Write((int)timelineEventsCount);
                writer.Write((int)_UIContainer.References.Count);
                writer.Write((int)_UIContainer.Edittexts.Count);
                writer.Write((int)_UIContainer.Symbols.Count);
                writer.Write((int)_UIContainer.Bitmaps.Count);
                long imageSizeOffset = writer.BaseStream.Position;
                writer.Write((int)0);
                writer.Write((int)_UIContainer.FontNames.Count);
                writer.Write((int)_UIContainer.ImportAssets.Count);
                WriteRectangleF(writer, rect: _UIContainer.Header.FrameSize);

                List<FuiTimelineAction> actions = new List<FuiTimelineAction>(timelineActionsCount);
                List<FuiTimelineFrame> frames = new List<FuiTimelineFrame>(timelineFramesCount);
                List<FuiTimelineEvent> frameEvents = new List<FuiTimelineEvent>(timelineEventsCount);
                List<string> frameEventNames = new List<string>(timelineEventNamesCount);
                
                List<FuiShapeComponent> shapeComponents = new List<FuiShapeComponent>(shapeComponentsCount);
                List<PointF> verts = new List<PointF>(shapeComponentsCount);

                foreach (FuiTimeline timeline in _UIContainer.Timelines)
                {
                    writer.Write((int)timeline.SymbolIndex);
                    writer.Write((short)frames.Count);
                    writer.Write((short)timeline.Frames.Count);
                    writer.Write((short)actions.Count);
                    writer.Write((short)timeline.Actions.Count);
                    WriteRectangleF(writer, timeline.Area);
                    frames.AddRange(timeline.Frames);
                    actions.AddRange(timeline.Actions);
                }
                foreach (FuiTimelineAction timelineAction in actions)
                {
                    writer.Write((ushort)timelineAction.Type);
                    writer.Write((short)timelineAction.FrameIndex);
                    writer.WriteString(timelineAction.Arg0, 0x40);
                    writer.WriteString(timelineAction.Arg1, 0x40);
                }
                foreach (FuiShape shape in _UIContainer.Shapes)
                {
                    writer.Write((int)shape.Unknown);
                    writer.Write((int)shapeComponents.Count);
                    writer.Write((int)shape.Components.Count);
                    WriteRectangleF(writer, shape.Area);
                    shapeComponents.AddRange(shape.Components);
                }
                foreach (FuiShapeComponent shapeComponent in shapeComponents)
                {
                    writer.Write((int)shapeComponent.FillInfo.Type);
                    writer.Write(GetRGBAFromColor(shapeComponent.FillInfo.Color));
                    writer.Write(shapeComponent.FillInfo.BitmapIndex);
                    WriteMatrix(writer, shapeComponent.FillInfo.Matrix);
                    writer.Write((int)verts.Count);
                    writer.Write((int)shapeComponent.Verts.Length);
                    verts.AddRange(shapeComponent.Verts);
                }
                foreach (PointF vert in verts)
                {
                    writer.Write((float)vert.X);
                    writer.Write((float)vert.Y);
                }
                foreach (FuiTimelineFrame timelineFrame in frames)
                {
                    writer.WriteString(timelineFrame.FrameName, 0x40);
                    writer.Write((int)frameEvents.Count);
                    writer.Write((int)timelineFrame.Events.Count);
                    frameEvents.AddRange(timelineFrame.Events);
                }
                foreach (FuiTimelineEvent timelineEvent in frameEvents)
                {
                    writer.Write((ushort)timelineEvent.EventType);
                    writer.Write((byte)timelineEvent.ObjectType);
                    writer.Write((byte)0);
                    writer.Write((short)timelineEvent.Depth);
                    writer.Write((short)timelineEvent.Index);
                    writer.Write((short)timelineEvent.Unknown1);
                    writer.Write((short)(string.IsNullOrWhiteSpace(timelineEvent.Name) ? -1 : frameEventNames.Count));
                    WriteMatrix(writer, matrix: timelineEvent.Matrix);
                    WriteColorTransform(writer, colorTransform: timelineEvent.ColorTransform);
                    writer.Write(GetRGBAFromColor(timelineEvent.Color));
                    if (!string.IsNullOrWhiteSpace(timelineEvent.Name))
                        frameEventNames.Add(timelineEvent.Name);
                }
                foreach (string eventName in frameEventNames)
                {
                    writer.WriteString(eventName, 0x40);
                }
                foreach (FuiReference reference in _UIContainer.References)
                {
                    writer.Write((int)reference.SymbolIndex);
                    writer.WriteString(reference.Name, 0x40);
                    writer.Write((int)reference.Index);
                }
                foreach (FuiEdittext edittext in _UIContainer.Edittexts)
                {
                    writer.Write((int)edittext.Unknown0);
                    WriteRectangleF(writer, edittext.Rectangle);
                    writer.Write((int)edittext.FontId);
                    writer.Write((float)edittext.FontScale);
                    writer.Write(GetRGBAFromColor(edittext.Color));
                    // TODO: fix aligment (swap StringAlignment.Far and StringAlignment.Center) -null
                    writer.Write((byte)edittext.Alignment);
                    writer.Write((byte)edittext._a);
                    writer.Write((byte)edittext._b);
                    writer.Write((byte)edittext._c);
                    writer.Write((int)edittext.Unknown3);
                    writer.Write((int)edittext.Unknown4);
                    writer.Write((int)edittext.Unknown5);
                    writer.Write((float)edittext.Unknown6);
                    writer.Write((int)edittext.Unknown7);
                    writer.WriteString(edittext.htmlSource, 0x100);
                }
                foreach (FuiFontName fontName in _UIContainer.FontNames)
                {
                    writer.Write(fontName.ID);
                    writer.WriteString(fontName.Name, 0x100);
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

                List<byte[]> imgData = new List<byte[]>(0x1000);
                Debug.WriteLine($"bitmap count: {_UIContainer.Bitmaps.Count}");
                Debug.WriteLine($"stream pos before: 0x{writer.BaseStream.Position:X}");
                foreach (FuiBitmap bitmap in _UIContainer.Bitmaps)
                {
                    writer.Write((int)bitmap.SymbolIndex);
                    writer.Write((int)bitmap.ImageFormat);
                    writer.Write((int)bitmap.Image.Width);
                    writer.Write((int)bitmap.Image.Height);
                    int offset = imgData.Select(b => b.Length).Sum();
                    //Debug.WriteLine($"offset: 0x{offset:X}");
                    writer.Write((int)offset); // offset

                    int zlibOffset = -1;
                    byte[] imgBuffer = MakeImage(bitmap.Image, bitmap.ImageFormat, out zlibOffset);

                    imgData.Add(imgBuffer);
                    int size = imgBuffer.Length;
                    //Debug.WriteLine($"size: 0x{size:X}");
                    writer.Write((int)size); // size
                    //Debug.WriteLine($"zlibOffset: 0x{zlibOffset:X}");
                    writer.Write((int)zlibOffset); // ZlibDataOffset
                    //! Bind handle need to be set to zero!
                    writer.Write((int)0);
                }
                Debug.WriteLine($"stream pos after: 0x{writer.BaseStream.Position:X}");
                
                writer.Write(imgData.SelectMany(b => b).ToArray());

                long eof = writer.BaseStream.Position;

                int imgBufferSize = imgData.Select(b => b.Length).Sum();
                writer.BaseStream.Seek(imageSizeOffset, SeekOrigin.Begin);
                writer.Write((int)imgBufferSize);

                int contentSize = (int)eof - 0x98; // FUI_HEADER_BYTE_SIZE
                writer.BaseStream.Seek(contentSizeOffset, SeekOrigin.Begin);
                writer.Write(contentSize);
                Debug.WriteLine($"eof: 0x{eof:X}");
            }
        }

        private byte[] MakeImage(Image image, FuiBitmap.FuiImageFormat imageFormat, out int zlibOffset)
        {
            MemoryStream imgStream = new MemoryStream();
            zlibOffset = -1;
            switch (imageFormat)
            {
                case (FuiBitmap.FuiImageFormat)4:
                case FuiBitmap.FuiImageFormat.PNG_WITH_ALPHA_DATA:
                case FuiBitmap.FuiImageFormat.PNG_NO_ALPHA_DATA:
                    image.ReverseColorRB().Save(imgStream, ImageFormat.Png);
                    return imgStream.ToArray();
                case FuiBitmap.FuiImageFormat.JPEG_NO_ALPHA_DATA:
                    image.Save(imgStream, ImageFormat.Jpeg);
                    return imgStream.ToArray();
                case FuiBitmap.FuiImageFormat.JPEG_WITH_ALPHA_DATA:
                    image.Save(imgStream, ImageFormat.Jpeg);

                    byte[] alphaData = image.GetAlphaData();

                    var resCompressedStream = new MemoryStream();

                    DeflaterOutputStream compressedStream = new DeflaterOutputStream(resCompressedStream);

                    compressedStream.Write(alphaData, 0, alphaData.Length);
                    compressedStream.Finish();

                    byte[] compressedAlphaData = resCompressedStream.GetBuffer();

                    imgStream.Write(compressedAlphaData, 0, compressedAlphaData.Length);

                    compressedStream.Dispose();

                    zlibOffset = compressedAlphaData.Length;

                    return imgStream.ToArray();
                default:
                    throw new Exception(nameof(imageFormat));
            }
        }

        private static void WriteColorTransform(EndiannessAwareBinaryWriter writer, FuiColorTransform colorTransform)
        {
            writer.Write((float)colorTransform.Offset.R);
            writer.Write((float)colorTransform.Offset.G);
            writer.Write((float)colorTransform.Offset.B);
            writer.Write((float)colorTransform.Offset.A);

            writer.Write((float)colorTransform.Multiplier.R);
            writer.Write((float)colorTransform.Multiplier.G);
            writer.Write((float)colorTransform.Multiplier.B);
            writer.Write((float)colorTransform.Multiplier.A);
        }

        private static void WriteMatrix(EndiannessAwareBinaryWriter writer, Matrix3x2 matrix)
        {
            writer.Write((float)matrix.M11);
            writer.Write((float)matrix.M12);
            writer.Write((float)matrix.M21);
            writer.Write((float)matrix.M22);
            writer.Write((float)matrix.M31);
            writer.Write((float)matrix.M32);
        }

        private static void WriteRectangleF(EndiannessAwareBinaryWriter writer, RectangleF rect)
        {
            writer.Write((float)(rect.X));
            writer.Write((float)(rect.X + rect.Width));
            writer.Write((float)(rect.Y));
            writer.Write((float)(rect.Y + rect.Height));
        }

        private static int GetRGBAFromColor(System.Drawing.Color color)
        {
            int argb = color.ToArgb();
            return (argb & 0xffffff) << 8 | argb >> 24 & 0xff;
        }
    }
}
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
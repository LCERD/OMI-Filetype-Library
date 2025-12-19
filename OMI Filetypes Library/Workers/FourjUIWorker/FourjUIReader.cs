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
using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;
using OMI.Extentions;
using OMI.Formats.FUI;
using OMI.Formats.FUI.Components;
using System.Numerics;
/*
* all known FourJUserInterface information is the direct product of Miku-666(NessieHax)'s work! check em out! 
* https://github.com/NessieHax
*/
namespace OMI.Workers.FUI
{
    public class FourjUIReader : IDataFormatReader<FourjUserInterface>, IDataFormatReader
    {
        public FourjUIReader()
        {
        }

        public FourjUserInterface FromFile(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException(filename);
            FourjUserInterface userInterfaceContainer;
            using (FileStream fs = File.OpenRead(filename))
            {
                userInterfaceContainer = FromStream(fs);
            }
            return userInterfaceContainer;
        }

        public FourjUserInterface FromStream(Stream stream)
        {
            FourjUserInterface uiContainer = new FourjUserInterface();
            using (var reader = new EndiannessAwareBinaryReader(stream, Encoding.ASCII, Endianness.LittleEndian))
            {
                var signature = reader.ReadInt64(Endianness.BigEndian);
                var contentSize = reader.ReadInt32();
                var swfFileName = reader.ReadString(0x40);

                uiContainer.Timelines = new List<FuiTimeline>(reader.ReadInt32());
                uiContainer.TimelineEventNames = new List<string>(reader.ReadInt32());
                uiContainer.TimelineActions = new List<FuiTimelineAction>(reader.ReadInt32());
                uiContainer.Shapes = new List<FuiShape>(reader.ReadInt32());
                uiContainer.ShapeComponents = new List<FuiShapeComponent>(reader.ReadInt32());
                uiContainer.Verts = new List<PointF>(reader.ReadInt32());
                uiContainer.TimelineFrames = new List<FuiTimelineFrame>(reader.ReadInt32());
                uiContainer.TimelineEvents = new List<FuiTimelineEvent>(reader.ReadInt32());
                uiContainer.References = new List<FuiReference>(reader.ReadInt32());
                uiContainer.Edittexts = new List<FuiEdittext>(reader.ReadInt32());
                uiContainer.Symbols = new List<FuiSymbol>(reader.ReadInt32());
                uiContainer.Bitmaps = new List<FuiBitmap>(reader.ReadInt32());

                int imagesSize = reader.ReadInt32();

                uiContainer.FontNames = new List<FuiFontName>(reader.ReadInt32());
                uiContainer.ImportAssets = new List<string>(reader.ReadInt32());


                RectangleF rect = ReadRect(reader);
                uiContainer.Header = new FuiHeader(signature, contentSize, swfFileName, rect);

                reader.Fill(uiContainer.Timelines, ReadTimeline);
                reader.Fill(uiContainer.TimelineActions, ReadTimelineAction);
                reader.Fill(uiContainer.Shapes, ReadShape);
                reader.Fill(uiContainer.ShapeComponents, ReadShapeComponent);
                reader.Fill(uiContainer.Verts, ReadVert);
                reader.Fill(uiContainer.TimelineFrames, ReadTimelineFrame);
                reader.Fill(uiContainer.TimelineEvents, ReadTimelineEvent);
                reader.Fill(uiContainer.TimelineEventNames, ReadString);
                reader.Fill(uiContainer.References, ReadReference);
                reader.Fill(uiContainer.Edittexts, ReadEdittext);
                reader.Fill(uiContainer.FontNames, ReadFontName);
                reader.Fill(uiContainer.Symbols, ReadSymbol);
                reader.Fill(uiContainer.ImportAssets, ReadString);
                reader.Fill(uiContainer.Bitmaps, ReadBitmap);

                using (var ms = new MemoryStream(reader.ReadBytes(imagesSize)))
                {
                    foreach (FuiBitmap bitmap in uiContainer.Bitmaps)
                    {
                        long origin = ms.Position;
                        ms.Seek(bitmap.Offset, SeekOrigin.Begin);
                        byte[] buffer = new byte[bitmap.Size];
                        ms.Read(buffer, 0, bitmap.Size);
                        ms.Seek(origin, SeekOrigin.Begin);
                        uiContainer.ImagesData.Add(buffer);
                    }
                }
            }
            return uiContainer;
        }

        private static RectangleF ReadRect(EndiannessAwareBinaryReader reader)
        {
            float minX = reader.ReadSingle();
            float maxX = reader.ReadSingle();
            float minY = reader.ReadSingle();
            float maxY = reader.ReadSingle();
            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }

        private FuiBitmap ReadBitmap(EndiannessAwareBinaryReader reader)
        {
            FuiBitmap tline = new FuiBitmap();
            tline.SymbolIndex = reader.ReadInt32();
            tline.ImageFormat = (FuiBitmap.FuiImageFormat)reader.ReadInt32();
            tline.ImageSize.Width = reader.ReadInt32();
            tline.ImageSize.Height = reader.ReadInt32();
            tline.Offset = reader.ReadInt32();
            tline.Size = reader.ReadInt32();
            tline.ZlibDataOffset = reader.ReadInt32();
            reader.ReadInt32();
            return tline;
        }

        private FuiSymbol ReadSymbol(EndiannessAwareBinaryReader reader)
        {
            FuiSymbol symbol = new FuiSymbol();
            symbol.Name = reader.ReadString(0x40);
            symbol.ObjectType = (fuiObject_eFuiObjectType)reader.ReadInt32();
            symbol.Index = reader.ReadInt32();
            return symbol;
        }

        private FuiFontName ReadFontName(EndiannessAwareBinaryReader reader)
        {
            FuiFontName fontName = new FuiFontName();
            fontName.ID = reader.ReadInt32();
            fontName.Name = reader.ReadString(0x40);
            reader.ReadBytes(0xc0); // unknown values
            return fontName;
        }

        private FuiEdittext ReadEdittext(EndiannessAwareBinaryReader reader)
        {
            FuiEdittext edittext = new FuiEdittext();
            edittext.Unknown0 = reader.ReadInt32();
            edittext.Rectangle = ReadRect(reader);
            edittext.FontId = reader.ReadInt32();
            edittext.FontScale = reader.ReadSingle();
            edittext.Color = GetColorFromRGBA(reader.ReadInt32());
            edittext.Alignment = reader.ReadInt32();
            edittext.Unknown3 = reader.ReadInt32();
            edittext.Unknown4 = reader.ReadInt32();
            edittext.Unknown5 = reader.ReadInt32();
            edittext.Unknown6 = reader.ReadInt32();
            edittext.Unknown7 = reader.ReadInt32();
            edittext.htmlSource = reader.ReadString(0x100);
            return edittext;
        }

        private static System.Drawing.Color GetColorFromRGBA(int rgba)
        {
            return System.Drawing.Color.FromArgb(rgba & 0xff | rgba >> 8 & 0xffffff);
        }

        private FuiReference ReadReference(EndiannessAwareBinaryReader reader)
        {
            FuiReference reference = new FuiReference();
            reference.SymbolIndex = reader.ReadInt32();
            reference.Name = reader.ReadString(0x40);
            reference.Index = reader.ReadInt32();
            return reference;
        }

        private string ReadString(EndiannessAwareBinaryReader reader)
        {
            return reader.ReadString(0x40);
        }

        private FuiTimelineEvent ReadTimelineEvent(EndiannessAwareBinaryReader reader)
        {
            FuiTimelineEvent timelineEvent = new FuiTimelineEvent();
            timelineEvent.EventType = (FuiTimelineEvent.EventFlags)reader.ReadInt16();
            timelineEvent.ObjectType = (fuiObject_eFuiObjectType)reader.ReadByte();
            _ = reader.ReadByte();
            timelineEvent.Unknown0 = reader.ReadInt16();
            timelineEvent.Index = reader.ReadInt16();
            timelineEvent.Unknown1 = reader.ReadInt16();
            timelineEvent.NameIndex = reader.ReadInt16();
            timelineEvent.Matrix = ReadMatrix(reader);
            timelineEvent.ColorTransform.RedMultTerm = reader.ReadSingle();
            timelineEvent.ColorTransform.GreenMultTerm = reader.ReadSingle();
            timelineEvent.ColorTransform.BlueMultTerm = reader.ReadSingle();
            timelineEvent.ColorTransform.AlphaMultTerm = reader.ReadSingle();
            timelineEvent.ColorTransform.RedAddTerm = reader.ReadSingle();
            timelineEvent.ColorTransform.GreenAddTerm = reader.ReadSingle();
            timelineEvent.ColorTransform.BlueAddTerm = reader.ReadSingle();
            timelineEvent.ColorTransform.AlphaAddTerm = reader.ReadSingle();
            timelineEvent.Color = GetColorFromRGBA(reader.ReadInt32());
            return timelineEvent;
        }

        private FuiTimelineFrame ReadTimelineFrame(EndiannessAwareBinaryReader reader)
        {
            FuiTimelineFrame timelineFrame = new FuiTimelineFrame();
            timelineFrame.FrameName = reader.ReadString(0x40);
            timelineFrame.EventIndex = reader.ReadInt32();
            timelineFrame.EventCount = reader.ReadInt32();
            return timelineFrame;
        }

        private PointF ReadVert(EndiannessAwareBinaryReader reader)
        {
            PointF vert = new PointF();
            vert.X = reader.ReadSingle();
            vert.Y = reader.ReadSingle();
            return vert;
        }

        private static Matrix3x2 ReadMatrix(EndiannessAwareBinaryReader reader)
        {
            float scaleX = reader.ReadSingle();
            float scaleY = reader.ReadSingle();
            Matrix3x2 result = Matrix3x2.CreateScale(scaleX, scaleY);
            float rotateSkewX = reader.ReadSingle();
            float rotateSkewY = reader.ReadSingle();
            result += Matrix3x2.CreateSkew(rotateSkewX * (float)Math.PI / 180f, rotateSkewY * (float)Math.PI / 180f);
            float trX = reader.ReadSingle();
            float trY = reader.ReadSingle();
            result.Translation = new Vector2(trX, trY);
            return result;
        }

        private FuiShapeComponent ReadShapeComponent(EndiannessAwareBinaryReader reader)
        {
            FuiShapeComponent shapeComponent = new FuiShapeComponent();
            shapeComponent.FillInfo.Type = (FuiFillStyle.FillType)reader.ReadInt32();
            shapeComponent.FillInfo.Color = GetColorFromRGBA(reader.ReadInt32());
            shapeComponent.FillInfo.BitmapIndex = reader.ReadInt32();
            shapeComponent.FillInfo.Matrix = ReadMatrix(reader);
            shapeComponent.VertIndex = reader.ReadInt32();
            shapeComponent.VertCount = reader.ReadInt32();
            return shapeComponent;
        }

        private FuiShape ReadShape(EndiannessAwareBinaryReader reader)
        {
            FuiShape shape = new FuiShape();
            shape.Unknown = reader.ReadInt32();
            shape.ShapeComponentIndex = reader.ReadInt32();
            shape.ShapeComponentCount = reader.ReadInt32();
            shape.Area = ReadRect(reader);
            return shape;
        }

        private FuiTimeline ReadTimeline(EndiannessAwareBinaryReader reader)
        {
            FuiTimeline timeline = new FuiTimeline();
            timeline.SymbolIndex = reader.ReadInt32();
            timeline.FrameIndex = reader.ReadInt16();
            timeline.FrameCount = reader.ReadInt16();
            timeline.ActionIndex = reader.ReadInt16();
            timeline.ActionCount = reader.ReadInt16();
            timeline.Area = ReadRect(reader);
            return timeline;
        }

        private FuiTimelineAction ReadTimelineAction(EndiannessAwareBinaryReader reader)
        {
            FuiTimelineAction timelineAction = new FuiTimelineAction();
            timelineAction.Type = (FuiTimelineAction.ActionType)reader.ReadInt16();
            timelineAction.FrameIndex = reader.ReadInt16();
            timelineAction.StringArg0 = reader.ReadString(0x40);
            timelineAction.StringArg1 = reader.ReadString(0x40);
            return timelineAction;
        }

        object IDataFormatReader.FromStream(Stream stream) => FromStream(stream);

        object IDataFormatReader.FromFile(string filename) => FromFile(filename);
    }
}

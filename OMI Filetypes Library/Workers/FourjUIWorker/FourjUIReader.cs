/*
 * all known FourJUserInterface information is the direct product of Miku-666(NessieHax)'s work! check em out! 
 * https://github.com/NessieHax
 * See License usage at the bottom of file!
*/

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using OMI.Extentions;
using OMI.Formats.FUI;
/*
* all known FourJUserInterface information is the direct product of Miku-666(NessieHax)'s work! check em out! 
* https://github.com/NessieHax
*/
namespace OMI.Workers.FUI
{
    public class FourjUIReader : IDataFormatReader<FourjUserInterface>, IDataFormatReader
    {
        const int FUI_HEADER_BYTE_SIZE = 0x98;

        const int FUI_TIMELINE_BYTE_SIZE = 0x1c;
        const int FUI_TIMELINE_ACTION_BYTE_SIZE = 0x84;

        const int FUI_SHAPE_BYTE_SIZE = 0x1c;
        const int FUI_SHAPE_COMPONENT_BYTE_SIZE = 0x2c;
        const int FUI_VERT_BYTE_SIZE = 0x8;

        const int FUI_TIMELINE_FRAME_BYTE_SIZE = 0x48;
        const int FUI_TIMELINE_EVENT_BYTE_SIZE = 0x48;
        const int FUI_TIMELINE_EVENT_NAME_BYTE_SIZE = 0x40;

        //const int FUI_BITMAP_BYTE_SIZE = 0x20;
        private long _timelinesStartOffset;
        private long _timelineActionsStartOffset;
        private long _shapesStartOffset;
        private long _shapeComponentsStartOffset;
        private long _vertsStartOffset;
        private long _timelineFramesStartOffset;
        private long _timelineEventsStartOffset;
        private long _timelineEventNamesStartOffset;
        private long _imageDataStartOffset;

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
            using var reader = new EndiannessAwareBinaryReader(stream, Encoding.ASCII, ByteOrder.LittleEndian);

            string signature = reader.ReadString(8);
            Debug.Assert(signature == FuiHeader.DefaultSignature, "Invalid signature");

            var contentSize = reader.ReadInt32();
            var swfFileName = reader.ReadString(0x40);

            int timelinesCount = reader.ReadInt32();
            int timelineEventNamesCount = reader.ReadInt32();
            int timelineActionsCount = reader.ReadInt32();
            int shapesCount = reader.ReadInt32();
            int shapeComponentsCount = reader.ReadInt32();
            int vertsCount = reader.ReadInt32();
            int timelineFramesCount = reader.ReadInt32();
            int timelineEventsCount = reader.ReadInt32();
            int referencesCount = reader.ReadInt32();
            int edittextsCount = reader.ReadInt32();
            int symbolsCount = reader.ReadInt32();
            int bitmapsCount = reader.ReadInt32();

            int imagesSize = reader.ReadInt32();

            int fontNamesCount = reader.ReadInt32();
            int importAssetsCount = reader.ReadInt32();

            RectangleF rect = ReadRect(reader);
            FourjUserInterface uiContainer = new FourjUserInterface(swfFileName, rect,
                importAssetsCount,
                timelinesCount,
                shapesCount,
                referencesCount,
                edittextsCount,
                fontNamesCount,
                symbolsCount,
                bitmapsCount
                );

            _timelinesStartOffset = FUI_HEADER_BYTE_SIZE;
            long timelinesSize = FUI_TIMELINE_BYTE_SIZE * timelinesCount;

            _timelineActionsStartOffset = _timelinesStartOffset + timelinesSize;
            long timelineActionsSize = FUI_TIMELINE_ACTION_BYTE_SIZE * timelineActionsCount;

            _shapesStartOffset = _timelineActionsStartOffset + timelineActionsSize;
            long shapesSize = FUI_SHAPE_BYTE_SIZE * shapesCount;

            _shapeComponentsStartOffset = _shapesStartOffset + shapesSize;
            long shapeComponentsSize = FUI_SHAPE_COMPONENT_BYTE_SIZE * shapeComponentsCount;

            _vertsStartOffset = _shapeComponentsStartOffset + shapeComponentsSize;
            long vertsSize = FUI_VERT_BYTE_SIZE * vertsCount;

            _timelineFramesStartOffset = _vertsStartOffset + vertsSize;
            long timelineFramesSize = FUI_TIMELINE_FRAME_BYTE_SIZE * timelineFramesCount;

            _timelineEventsStartOffset = _timelineFramesStartOffset + timelineFramesSize;
            long timelineEventsSize = FUI_TIMELINE_EVENT_BYTE_SIZE * timelineEventsCount;

            _timelineEventNamesStartOffset = _timelineEventsStartOffset + timelineEventsSize;
            long timelineEventNamesSize = FUI_TIMELINE_EVENT_NAME_BYTE_SIZE * timelineEventNamesCount;

            long origin = reader.BaseStream.Position;
            _imageDataStartOffset = reader.BaseStream.Seek(-imagesSize, SeekOrigin.End);
            reader.BaseStream.Seek(origin, SeekOrigin.Begin);

            Debug.Assert(reader.BaseStream.Position == FUI_HEADER_BYTE_SIZE, "Invalid Header size");

            reader.Fill(uiContainer.Timelines, timelinesCount, ReadTimeline);

            reader.BaseStream.Position = _shapesStartOffset;
            reader.Fill(uiContainer.Shapes, shapesCount, ReadShape);

            reader.BaseStream.Position = _timelineEventNamesStartOffset + timelineEventNamesSize;
            reader.Fill(uiContainer.References, referencesCount, ReadReference);
            reader.Fill(uiContainer.Edittexts, edittextsCount, ReadEdittext);
            reader.Fill(uiContainer.FontNames, fontNamesCount, ReadFontName);
            reader.Fill(uiContainer.Symbols, symbolsCount, ReadSymbol);
            reader.Fill(uiContainer.ImportAssets, importAssetsCount, r => r.ReadString(0x40));
            reader.Fill(uiContainer.Bitmaps, bitmapsCount, ReadBitmap);


            Debug.Assert(reader.BaseStream.Seek(0, SeekOrigin.End) == contentSize + FUI_HEADER_BYTE_SIZE, "Contentsize missmatch");
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
            var symbolIndex = reader.ReadInt32();
            var imageFormat = (FuiBitmap.FuiImageFormat)reader.ReadInt32();
            var width = reader.ReadInt32();
            var height = reader.ReadInt32();
            var offset = reader.ReadInt32();
            var size = reader.ReadInt32();
            var zlibDataOffset = reader.ReadInt32();
            
            // "BindHandle" can be ignored anything set will crash the game or lead to undefined behaviour. -null
            _ = reader.ReadInt32();

            long origin = reader.BaseStream.Position;
            reader.BaseStream.Seek(_imageDataStartOffset + offset, SeekOrigin.Begin);

            byte[] buffer = reader.ReadBytes(size);

            Stream imgBufferStream = new MemoryStream(buffer);

            Image img = Image.FromStream(imgBufferStream);

            if (imageFormat <= FuiBitmap.FuiImageFormat.PNG_NO_ALPHA_DATA)
                img = img.ReverseColorRB();
            if (imageFormat == FuiBitmap.FuiImageFormat.JPEG_WITH_ALPHA_DATA && zlibDataOffset > -1)
            {
                int bufferSize = size - zlibDataOffset;

                reader.BaseStream.Seek(_imageDataStartOffset + offset + zlibDataOffset, SeekOrigin.Begin);

                Stream decompressedStream = new InflaterInputStream(reader.BaseStream, new Inflater(), bufferSize);
                var outputStream = new MemoryStream();
                decompressedStream.CopyTo(outputStream);
                img = img.SetAlphaData(outputStream.ToArray());
            }
            reader.BaseStream.Seek(origin, SeekOrigin.Begin);

            FuiBitmap fuiBitmap = new FuiBitmap(img, imageFormat, symbolIndex);
            return fuiBitmap;
        }

        private FuiSymbol ReadSymbol(EndiannessAwareBinaryReader reader)
        {
            string name = reader.ReadString(0x40);
            fuiObjectType objectType = (fuiObjectType)reader.ReadInt32();
            int index = reader.ReadInt32();
            return new FuiSymbol(name, objectType, index);
        }

        private FuiFontName ReadFontName(EndiannessAwareBinaryReader reader)
        {
            FuiFontName fontName = new FuiFontName();
            fontName.ID = reader.ReadInt32();
            fontName.Name = reader.ReadString(0x100);
            return fontName;
        }

        private FuiEdittext ReadEdittext(EndiannessAwareBinaryReader reader)
        {
            FuiEdittext edittext = new FuiEdittext();
            edittext.Unknown0 = reader.ReadInt32();
            edittext.Rectangle = ReadRect(reader);
            edittext.FontId = reader.ReadInt32();
            edittext.FontScale = reader.ReadSingle();
            edittext.Color = ReadColor(reader);
            edittext.Alignment = (StringAlignment)reader.ReadByte();
            edittext._a = reader.ReadByte();
            edittext._b = reader.ReadByte();
            edittext._c = reader.ReadByte();
            edittext.Unknown3 = reader.ReadInt32();
            edittext.Unknown4 = reader.ReadInt32();
            edittext.Unknown5 = reader.ReadInt32();
            edittext.Unknown6 = reader.ReadSingle();
            edittext.Unknown7 = reader.ReadInt32();
            edittext.htmlSource = reader.ReadString(0x100);
            return edittext;
        }


        private FuiReference ReadReference(EndiannessAwareBinaryReader reader)
        {
            FuiReference reference = new FuiReference();
            reference.SymbolIndex = reader.ReadInt32();
            reference.Name = reader.ReadString(0x40);
            reference.Index = reader.ReadInt32();
            return reference;
        }

        private FuiTimelineEvent ReadTimelineEvent(EndiannessAwareBinaryReader reader)
        {
            FuiTimelineEvent.EventFlags eventType = (FuiTimelineEvent.EventFlags)reader.ReadInt16();
            var objectType = (fuiObjectType)reader.ReadByte();
            _ = reader.ReadByte();
            var depth = reader.ReadInt16();
            var index = reader.ReadInt16();
            var unknown1 = reader.ReadInt16();
            var nameIndex = reader.ReadInt16();
            Matrix3x2 matrix = ReadMatrix(reader);
            FuiColorTransform colorTransform = ReadColorTransform(reader);
            System.Drawing.Color color = ReadColor(reader);

            string name = string.Empty;
            if (nameIndex > -1)
            {
                long origin = reader.BaseStream.Position;
                reader.BaseStream.Position = _timelineEventNamesStartOffset + nameIndex * FUI_TIMELINE_EVENT_NAME_BYTE_SIZE;
                name = reader.ReadString(0x40);
                reader.BaseStream.Position = origin;    
            }
            FuiTimelineEvent timelineEvent = new FuiTimelineEvent(name, eventType, depth, objectType, index, unknown1, matrix, colorTransform, color);

            return timelineEvent;
        }

        private FuiColorTransform ReadColorTransform(EndiannessAwareBinaryReader reader)
        {
            var colorTransform = new FuiColorTransform();
            colorTransform.RedAddTerm = reader.ReadSingle();
            colorTransform.GreenAddTerm = reader.ReadSingle();
            colorTransform.BlueAddTerm = reader.ReadSingle();
            colorTransform.AlphaAddTerm = reader.ReadSingle();

            colorTransform.RedMultTerm = reader.ReadSingle();
            colorTransform.GreenMultTerm = reader.ReadSingle();
            colorTransform.BlueMultTerm = reader.ReadSingle();
            colorTransform.AlphaMultTerm = reader.ReadSingle();
            return colorTransform;
        }

        private FuiTimelineFrame ReadTimelineFrame(EndiannessAwareBinaryReader reader)
        {
            string frameName = reader.ReadString(0x40);
            int eventIndex = reader.ReadInt32();
            int eventCount = reader.ReadInt32();
            FuiTimelineFrame timelineFrame = new FuiTimelineFrame(frameName, Enumerable.Empty<FuiTimelineEvent>());
            
            long offset = _timelineEventsStartOffset + eventIndex * FUI_TIMELINE_EVENT_BYTE_SIZE;

            reader.FillAtOffset(timelineFrame.Events, eventCount, offset, ReadTimelineEvent);
            return timelineFrame;
        }

        private PointF ReadVert(EndiannessAwareBinaryReader reader)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            return new PointF(x, y);
        }

        private static Matrix3x2 ReadMatrix(EndiannessAwareBinaryReader reader)
        {
            Matrix3x2 result = Matrix3x2.Identity;
            result.M11 = reader.ReadSingle();
            result.M12 = reader.ReadSingle();
            result.M21 = reader.ReadSingle();
            result.M22 = reader.ReadSingle();
            result.M31 = reader.ReadSingle();
            result.M32 = reader.ReadSingle();
            return result;
        }

        private System.Drawing.Color ReadColor(EndiannessAwareBinaryReader reader)
        {
            uint rgba = reader.ReadUInt32();
            return System.Drawing.Color.FromArgb((int)(((rgba & 0xff) << 24) | ((rgba & 0xffffff00) >> 8)));
        }

        private FuiFillStyle ReadFillStyle(EndiannessAwareBinaryReader reader)
        {
            FuiFillStyle fillInfo = new FuiFillStyle();
            fillInfo.Type = (FuiFillStyle.FillType)reader.ReadInt32();
            fillInfo.Color = ReadColor(reader);
            fillInfo.BitmapIndex = reader.ReadInt32();
            fillInfo.Matrix = ReadMatrix(reader);
            return fillInfo;
        }

        private FuiShapeComponent ReadShapeComponent(EndiannessAwareBinaryReader reader)
        {
            FuiFillStyle fillInfo = ReadFillStyle(reader);
            int vertIndex = reader.ReadInt32();
            int vertCount = reader.ReadInt32();
            FuiShapeComponent shapeComponent = new FuiShapeComponent(fillInfo, new PointF[vertCount]);

            long offset = _vertsStartOffset + vertIndex * FUI_VERT_BYTE_SIZE;
            reader.FillAtOffset(shapeComponent.Verts, vertCount, offset, ReadVert);
            return shapeComponent;
        }

        private FuiShape ReadShape(EndiannessAwareBinaryReader reader)
        {
            _ = reader.ReadInt32();
            int shapeComponentIndex = reader.ReadInt32();
            int shapeComponentCount = reader.ReadInt32();
            RectangleF area = ReadRect(reader);
            FuiShape shape = new FuiShape(area, Enumerable.Empty<FuiShapeComponent>());

            long offset = _shapeComponentsStartOffset + shapeComponentIndex * FUI_SHAPE_COMPONENT_BYTE_SIZE;
            reader.FillAtOffset(shape.Components, shapeComponentCount, offset, ReadShapeComponent);
            return shape;
        }

        private FuiTimeline ReadTimeline(EndiannessAwareBinaryReader reader)
        {
            int symbolIndex = reader.ReadInt32();
            short frameIndex = reader.ReadInt16();
            short frameCount = reader.ReadInt16();
            short actionIndex = reader.ReadInt16();
            short actionCount = reader.ReadInt16();
            RectangleF area = ReadRect(reader);
            FuiTimeline timeline = new FuiTimeline(area, Enumerable.Empty<FuiTimelineFrame>(), Enumerable.Empty<FuiTimelineAction>(), symbolIndex);
            
            long framesOffset = _timelineFramesStartOffset + frameIndex * FUI_TIMELINE_FRAME_BYTE_SIZE;
            reader.FillAtOffset(timeline.Frames, frameCount, framesOffset, ReadTimelineFrame);
            
            long actionOffset = _timelineActionsStartOffset + actionIndex * FUI_TIMELINE_ACTION_BYTE_SIZE;
            reader.FillAtOffset(timeline.Actions, actionCount, actionOffset, ReadTimelineAction);
            return timeline;
        }

        private FuiTimelineAction ReadTimelineAction(EndiannessAwareBinaryReader reader)
        {
            FuiTimelineAction timelineAction = new FuiTimelineAction();
            timelineAction.Type = (FuiTimelineAction.ActionType)reader.ReadInt16();
            timelineAction.FrameIndex = reader.ReadInt16();
            timelineAction.Arg0 = reader.ReadString(0x40);
            timelineAction.Arg1 = reader.ReadString(0x40);
            return timelineAction;
        }

        object IDataFormatReader.FromStream(Stream stream) => FromStream(stream);

        object IDataFormatReader.FromFile(string filename) => FromFile(filename);
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
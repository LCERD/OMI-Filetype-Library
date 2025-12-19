using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;


/*
 * all known FourJUserInterface information is the direct product of Miku-666(NessieHax)'s work! check em out! 
 * https://github.com/NessieHax
*/
namespace OMI.Formats.FUI
{
    public sealed class FourjUserInterface
    {
        public Components.FuiHeader Header;
        public List<Components.FuiTimeline> Timelines;
        public List<Components.FuiTimelineAction> TimelineActions;
        public List<Components.FuiShape> Shapes;
        public List<Components.FuiShapeComponent> ShapeComponents;
        public List<PointF> Verts;
        public List<Components.FuiTimelineFrame> TimelineFrames;
        public List<Components.FuiTimelineEvent> TimelineEvents;
        public List<string> TimelineEventNames;
        public List<Components.FuiReference> References;
        public List<Components.FuiEdittext> Edittexts;
        public List<Components.FuiFontName> FontNames;
        public List<Components.FuiSymbol> Symbols;
        public List<string> ImportAssets;
        public List<Components.FuiBitmap> Bitmaps;
        public List<byte[]> ImagesData = new List<byte[]>();
    }

    namespace Components
    {
        public sealed class FuiHeader
        {
            public static readonly byte DefaultVersion = 1;
            public static readonly long DefaultSignature = DefaultVersion << 56 | 0x495546 << 32;

            public byte Version => (byte)(Signature >> 56 & 0xff);

            public readonly long Signature;
            public readonly int ContentSize;
            public readonly string SwfFileName;
            public readonly RectangleF FrameSize;

            public FuiHeader(long signature, int contentSize, string swfFileName, RectangleF frameSize)
            {
                Signature = signature;
                ContentSize = contentSize;
                SwfFileName = swfFileName;
                FrameSize = frameSize;
            }

            public override string ToString()
            {
                return $"Signature: 0x{Signature.ToString("X16")}\n" +
                    $"Version: {Version}\n" +
                    $"Content Size: {ContentSize}\n" +
                    $"Frame Size: {FrameSize}";
            }
        }

        public sealed class FuiTimeline
        {
            public int SymbolIndex;
            public short FrameIndex;
            public short FrameCount;
            public short ActionIndex;
            public short ActionCount;
            public RectangleF Area;
        }

        public class FuiTimelineAction
        {
            public enum ActionType : ushort
            {
                HandleEvent = 0,
                Pause = 1,
                SetFrame = 2,
                SetFrameAndStart = 3,
                SetTabIndex = 4, //! Set StringArg1 to a valid number (e.g. 1-9)
                DoActionOn = 9,  //! Set StringArg1 specific to object used (e.g. List, Label, etc.)

                SetValue = 16,
                SetVisible = 17,
                DoListAction = 29, //! Calls 'FJ_List::setAction' or 'FJ_List2D::setAction'
                SelectFrame = 30, //! ?
            }

            public ActionType Type;
            public short FrameIndex;
            public string StringArg0;
            public string StringArg1;
        }

        public class FuiShape
        {
            internal int Unknown;
            public int ShapeComponentIndex;
            public int ShapeComponentCount;
            public RectangleF Area;
        }

        public class FuiShapeComponent
        {
            public FuiFillStyle FillInfo = new FuiFillStyle();
            public int VertIndex;
            public int VertCount;
        }

        public class FuiTimelineFrame
        {
            /// <summary>
            /// Max size: 0x40
            /// </summary>
            public string FrameName;
            public int EventIndex;
            public int EventCount;
        }


        public class FuiTimelineEvent
        {
            // TODO: add missing event flags
            [Flags]
            public enum EventFlags : ushort
            {
                None      = 0x00,
                Start     = 0x01,
                Stop      = 0x02,
                PlayFrame = 0x04,
                unk_0x08  = 0x08,
                
                unk_0x10  = 0x10,
                unk_0x20  = 0x20,
                unk_0x40  = 0x40,
                unk_0x80  = 0x80,
                _special = 0x8005,
            }
            public EventFlags EventType;
            public fuiObjectType ObjectType;
            public short Unknown0;
            public short Index;
            public short Unknown1;
            public short NameIndex;
            public Matrix3x2 Matrix;
            public FuiColorTransform ColorTransform = new FuiColorTransform();
            public System.Drawing.Color Color;
        }

        public class FuiReference
        {
            internal int SymbolIndex;
            /// <summary>
            /// Max size: 0x40
            /// </summary>
            public string Name;
            public int Index;
        }

        public class FuiEdittext
        {
            internal int Unknown0;
            public RectangleF Rectangle;
            public int FontId;
            public float FontScale;
            public System.Drawing.Color Color;
            public int Alignment; // 0 - 3
            public int Unknown3;
            public int Unknown4;
            public int Unknown5;
            public int Unknown6;
            public int Unknown7;
            /// <summary>
            /// Max size: 0x100
            /// </summary>
            public string htmlSource;
        }
        
        public class FuiFontName
        {
            public int ID;
            /// <summary>
            /// Max size: 0x40
            /// </summary>
            public string Name;

            public byte[] UnknownData;
        }
        
        public class FuiSymbol
        {
            /// <summary>
            /// Max size: 0x40
            /// </summary>
            public string Name;
            public fuiObjectType ObjectType;
            public int Index;
        }

        public class FuiBitmap
        {
            public enum FuiImageFormat
            {
                PNG_WITH_ALPHA_DATA = 1,
                PNG_NO_ALPHA_DATA = 3,
                JPEG_NO_ALPHA_DATA = 6,
                JPEG_UNKNOWN = 7,
                /// <summary>
                /// <see cref="ZlibDataOffset"/> has to be set!
                /// </summary>
                JPEG_WITH_ALPHA_DATA = 8
            }

            public int SymbolIndex;
            public FuiImageFormat ImageFormat;
            public Size ImageSize;
            public int Offset;
            public int Size;
            public int ZlibDataOffset;
            /// <summary>
            /// Preserved
            /// </summary>
            public readonly int BindHandle = 0;
        }
        
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
        }
        
        public struct FuiFillStyle
        {
            public enum FillType
            {
                Color = 1,
                //Unknown = 3,
                Image = 5,
            }

            public FillType Type;
            public System.Drawing.Color Color;
            public int BitmapIndex;
            public Matrix3x2 Matrix;
        }
        
        public enum fuiObjectType
        {
            STAGE = 0,
            SHAPE = 1,
            TIMELINE = 2,
            BITMAP = 3,
            REFERENCE = 4,
            EDITTEXT = 5,
            CODEGENRECT = 6,
        }

    }
}

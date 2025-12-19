using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OMI.Formats.Pck;
using OMI;
using OMI.Workers;
using OMI.Workers.Color;
using OMI.Workers.FUI;
using OMI.Workers.Language;
using OMI.Workers.Material;
using OMI.Workers.Model;
using OMI.Workers.Pck;
using OMI.Formats.FUI;
using OMI.Formats.Color;
using OMI.Formats.Languages;
using OMI.Formats.Model;
using OMI.Formats.Material;
using OMI.Formats.Ghost;
using System.Runtime.InteropServices;
using OMI.Workers.Ghost;

namespace OMI_Filetype_Library_Test
{
    internal class Program
    {
        private static PckFile _pckFile;
        private static Endianness _endianness;
        private static FourjUserInterface _UIContainer;
        private static ColorContainer _colContainer;
        private static LOCFile _locfile;
        private static int _type;
        private static ModelContainer modelContainer;
        private static MaterialContainer materialContainer;
        static void Main(string[] args)
        {
            if (args.Length >= 2 && args[0].Equals("--load") && File.Exists(args[1]))
            {
                IDataFormatReader reader = GetFormatReaderFromFilename(args[1]);
                //IDataFormatWriter writer = GetFormatWriterFromFilename(args[1]);


                Console.ForegroundColor = ConsoleColor.White;
                switch (Path.GetExtension(args[1]))
                {
                    case ".fui":
                        FourjUserInterface fuiFile = new FourjUIReader().FromFile(args[1]);
                        new FourjUIWriter(fuiFile).WriteToFile(args[1] + ".fui");
                        break;
                    case ".gst":
                        GhostRecord record = new GhostRecordReader().FromFile(args[1]);
                        new GhostRecordWriter(record).WriteToFile(args[1] + ".new");
                        break;
                    case ".col":
                        break;
                    case ".loc":
                        LOCFile locfile = new LOCFileReader().FromFile(args[1]);
                        int type = 2;
                        new LOCFileWriter(locfile, type).WriteToFile(args[1] + ".loc");
                        break;
                    case ".pck":
                        break;
                    case ".bin" when Path.GetFileNameWithoutExtension(args[1]).StartsWith("models"):
                        break;
                    case ".bin" when Path.GetFileNameWithoutExtension(args[1]).StartsWith("materials"):
                        break;
                }

                object data = reader.FromFile(args[1]);
                Console.WriteLine(data);
            }
        }

        private static IDataFormatReader GetFormatReaderFromFilename(string filename)
        {
            switch (Path.GetExtension(filename))
            {
                case ".gst":
                    return new GhostRecordReader();
                case ".fui":
                    return new FourjUIReader();
                case ".col":
                    return new COLFileReader();
                case ".loc":
                    return new LOCFileReader();
                case ".pck":
                    return new PckFileReader();
                case ".bin" when Path.GetFileNameWithoutExtension(filename).StartsWith("models"):
                    return new ModelFileReader();
                case ".bin" when Path.GetFileNameWithoutExtension(filename).StartsWith("materials"):
                    return new MaterialFileReader();
                default:
                    throw new NotSupportedException(Path.GetFileName(filename));
            }
        }

        private static IDataFormatWriter GetFormatWriterFromFilename(string filename) 
        {
            switch (Path.GetExtension(filename))
            {
                case ".fui":
                    return new FourjUIWriter(_UIContainer);
                case ".col":
                    return new COLFileWriter(_colContainer);
                case ".loc":
                    return new LOCFileWriter(_locfile, _type);
                case ".pck":
                    return new PckFileWriter(_pckFile, _endianness);
                case ".bin" when Path.GetFileNameWithoutExtension(filename).StartsWith("models"):
                    return new ModelFileWriter(modelContainer, 0);
                case ".bin" when Path.GetFileNameWithoutExtension(filename).StartsWith("materials"):
                    return new MaterialFileWriter(materialContainer);
                default:
                    throw new NotSupportedException(Path.GetFileName(filename));
            }
        }
    }
}
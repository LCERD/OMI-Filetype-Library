using System;
using System.Text;
using System.IO;
using OMI.Formats.Color;
using OMI.Workers;
/*
* most known Color information is the direct product of May/MattNL's work! check em out! 
* https://github.com/MattN-L
*/
namespace OMI.Workers.Color
{
    public class COLFileWriter : IDataFormatWriter
    {
        private ColorContainer _container;

        public COLFileWriter(ColorContainer container)
        {
            _container = container;
        }

        public void WriteToFile(string filename)
        {
            using (FileStream fs = File.OpenWrite(filename))
            {
                WriteToStream(fs);
            }
        }

        public void WriteToStream(Stream stream)
        {
            using (var writer = new EndiannessAwareBinaryWriter(stream, ByteOrder.BigEndian))
            {
                writer.Write(_container.Version);
                writer.Write(_container.Colors.Count);
                foreach (ColorContainer.Color col in _container.Colors)
                {
                    writer.Write((short)col.Name.Length);
                    writer.WriteString(col.Name, Encoding.ASCII);
                    writer.Write(col.ColorPallette.ToArgb());
                }

                if (_container.Version > 0)
                {
                    writer.Write(_container.WaterColors.Count);
                    foreach (ColorContainer.WaterColor col in _container.WaterColors)
                    {
                        writer.Write((short)col.Name.Length);
                        writer.WriteString(col.Name, Encoding.ASCII);
                        writer.Write(col.SurfaceColor.ToArgb());
                        writer.Write(col.UnderwaterColor.ToArgb());
                        writer.Write(col.FogColor.ToArgb());
                    }
                }
            }
        }
    }
}

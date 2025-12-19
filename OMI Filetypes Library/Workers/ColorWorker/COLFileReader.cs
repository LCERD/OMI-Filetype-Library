using System.IO;
using System.Text;
using OMI.Formats.Color;
using System.Diagnostics;
using OMI.Workers;
/*
* most known Color information is the direct product of May/MattNL's work! check em out! 
* https://github.com/MattN-L
*/
namespace OMI.Workers.Color
{
    public class COLFileReader : IDataFormatReader<ColorContainer>, IDataFormatReader
    {
        public ColorContainer FromFile(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException(filename);
            using (FileStream fs = File.OpenRead(filename))
            {
                return FromStream(fs);
            }
        }

        public ColorContainer FromStream(Stream s)
        {
            var colorContainer = new ColorContainer();
            using (var reader = new EndiannessAwareBinaryReader(s, Encoding.ASCII, leaveOpen: true, ByteOrder.BigEndian))
            {
                colorContainer.Version = reader.ReadInt32();
                int numberOfColors = reader.ReadInt32();
                for (int i = 0; i < numberOfColors; i++)
                {
                    var col = new ColorContainer.Color();
                    short length = reader.ReadInt16();
                    col.Name = reader.ReadString(length);
                    col.ColorPallette = System.Drawing.Color.FromArgb(reader.ReadInt32());
                    colorContainer.Colors.Add(col);
                }

                if (colorContainer.Version > 0)
                {
                    int numberOfWaterColors = reader.ReadInt32();
                    for (int i = 0; i < numberOfWaterColors; i++)
                    {
                        var col = new ColorContainer.WaterColor();
                        short length = reader.ReadInt16();
                        col.Name = reader.ReadString(length);
                        col.SurfaceColor = System.Drawing.Color.FromArgb(reader.ReadInt32());
                        col.UnderwaterColor = System.Drawing.Color.FromArgb(reader.ReadInt32());
                        col.FogColor = System.Drawing.Color.FromArgb(reader.ReadInt32());
                        colorContainer.WaterColors.Add(col);
                    }
                }
            }
            return colorContainer;
        }

        object IDataFormatReader.FromFile(string filename) => FromFile(filename);

        object IDataFormatReader.FromStream(Stream stream) => FromStream(stream);
    }
}

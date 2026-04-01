using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OMI.Formats.Material;
using OMI.Workers;
/*
* all known Model/Material information is the direct product of May/MattNL's work! check em out! 
* https://github.com/MattN-L
*/
namespace OMI.Workers.Material
{
    public class MaterialFileWriter : IDataFormatWriter
    {
        private readonly MaterialContainer container;

        public MaterialFileWriter(MaterialContainer container)
        {
            this.container = container;
        }

        public void WriteToStream(Stream stream)
        {
            using (var writer = new EndiannessAwareBinaryWriter(stream, Encoding.ASCII, leaveOpen: true, ByteOrder.BigEndian))
            {
                writer.Write(container.Version);
                writer.Write(container.Count);
                foreach (MaterialContainer.Material material in container)
                {
                    writer.Write(Convert.ToInt16(material.Name.Length));
                    writer.WriteString(material.Name);
                    writer.Write(Convert.ToInt16(material.Type.Length));
                    writer.WriteString(material.Type);
                }
            }
        }

        public void WriteToFile(string filename)
        {
            using (FileStream fs = File.OpenWrite(filename))
            {
                WriteToStream(fs);
            }
        }
    }
}

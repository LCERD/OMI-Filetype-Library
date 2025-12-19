using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using OMI.Formats.Material;
using OMI.Workers;
/*
* all known Model/Material information is the direct product of May/MattNL's work! check em out! 
* https://github.com/MattN-L
*/
namespace OMI.Workers.Material
{
    public class MaterialFileReader : IDataFormatReader<MaterialContainer>, IDataFormatReader
    {
        public MaterialContainer FromFile(string filename)
        {
            if (File.Exists(filename))
            {
                MaterialContainer modelContainer = null;
                using (FileStream fs = File.OpenRead(filename))
                {
                    modelContainer = FromStream(fs);
                }
                return modelContainer;
            }
            throw new FileNotFoundException(filename);
        }

        public MaterialContainer FromStream(Stream stream)
        {
            var container = new MaterialContainer();
            using (var reader = new EndiannessAwareBinaryReader(stream, Encoding.ASCII, leaveOpen: true, ByteOrder.BigEndian))
            {
                container.Version = reader.ReadInt32();
                int numberOfMaterials = reader.ReadInt32();
                for (int i = 0; i < numberOfMaterials; i++)
                {
                    string name = reader.ReadString(reader.ReadInt16());
                    string type = reader.ReadString(reader.ReadInt16());
                    container.Add(new MaterialContainer.Material(name, type));
                }
            }
            return container;
        }

        object IDataFormatReader.FromFile(string filename) => FromFile(filename);

        object IDataFormatReader.FromStream(Stream stream) => FromStream(stream);
    }
}

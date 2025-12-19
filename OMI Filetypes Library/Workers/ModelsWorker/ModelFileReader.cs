using System;
using System.Text;
using System.IO;
using OMI.Formats.Model;
using System.Diagnostics;
using System.Numerics;

/*
* all known Model/Material information is the direct product of MattNL's work! check em out! 
* https://github.com/MattN-L
*/
namespace OMI.Workers.Model
{
    static class BinaryReaderExtensions
    {
        public static Vector2 ReadVector2(this BinaryReader reader)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            return new Vector2(x, y);
        }
        
        public static Vector2 ReadVector2I(this BinaryReader reader)
        {
            float x = reader.ReadInt32();
            float y = reader.ReadInt32();
            return new Vector2(x, y);
        }

        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            Vector2 xy = reader.ReadVector2();
            float z = reader.ReadSingle();
            return new Vector3(xy, z);
        }

        public static Vector3 ReadVector3I(this BinaryReader reader)
        {
            Vector2 xy = reader.ReadVector2I();
            int z = reader.ReadInt32();
            return new Vector3(xy, z);
        }
    }

    public class ModelFileReader : IDataFormatReader<ModelContainer>, IDataFormatReader
    {
        public ModelContainer Read(byte[] data)
        {
            MemoryStream s = new MemoryStream(data);
            return FromStream(s);
        }

        public ModelContainer FromFile(string filename)
        {
            if (File.Exists(filename))
            {
                ModelContainer modelContainer = null;
                using (FileStream fs = File.OpenRead(filename))
                {
                    modelContainer = FromStream(fs);
                }
                return modelContainer;
            }
            throw new FileNotFoundException(filename);
        }

        public ModelContainer FromStream(Stream stream)
        {
            var container = new ModelContainer();
            using (var reader = new EndiannessAwareBinaryReader(stream, Encoding.ASCII, true, ByteOrder.BigEndian))
            {
                container.Version = reader.ReadInt32();

                int numOfModels = reader.ReadInt32();

                for (int i = 0; i < numOfModels; i++)
                {
                    string modelName = ReadString(reader);
                    int textureWidth = reader.ReadInt32();
                    int textureHeight = reader.ReadInt32();
                    Formats.Model.Model model = new Formats.Model.Model(modelName, new System.Drawing.Size(textureWidth, textureHeight));
                    int numOfParts = reader.ReadInt32();

                    for (int j = 0; j < numOfParts; j++)
                    {
                        string partName = ReadString(reader);
                        string partParentName = string.Empty;

                        if(container.Version > 1)
                        {
                            partParentName = ReadString(reader);
                        }

                        Vector3 translation = reader.ReadVector3();
                        Vector3 rotation = reader.ReadVector3();
                        Vector3 additionalRotation = new Vector3();

                        if (container.Version > 0)
                        {
                            additionalRotation = reader.ReadVector3();
                        }

                        ModelPart part = new ModelPart(partName, partParentName, translation, rotation, additionalRotation);

                        int numOfBoxes = reader.ReadInt32();

                        for (int x = 0; x < numOfBoxes; x++)
                        {
                            Vector3 position = reader.ReadVector3();
                            Vector3 size = reader.ReadVector3I();
                            Vector2 uv = reader.ReadVector2();
                            float inflate = reader.ReadSingle();
                            bool mirror = reader.ReadBoolean();
                            ModelBox box = new ModelBox(position, size, uv, inflate, mirror);
                            part.AddBox(box);
                        }
                        model.AddPart(part);
                    }
                    container.Add(model);
                }
            }
            return container;
        }

        private string ReadString(EndiannessAwareBinaryReader reader)
        {
            short length = reader.ReadInt16();
            return reader.ReadString(length);
        }

        object IDataFormatReader.FromStream(Stream stream) => FromStream(stream);

        object IDataFormatReader.FromFile(string filename) => FromFile(filename);
    }
}

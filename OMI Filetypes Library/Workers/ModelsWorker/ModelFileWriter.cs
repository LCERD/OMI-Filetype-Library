using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.IO;
using OMI.Formats.Model;
using OMI.Workers;
using System.Numerics;
/*
* all known Model/Material information is the direct product of May/MattNL's work! check em out! 
* https://github.com/MattN-L
*/

namespace OMI.Workers.Model
{
    static class BinaryWriterExtensions
    {
        public static void Write(this BinaryWriter writer, Vector2 vector2)
        {
            writer.Write(vector2.X);
            writer.Write(vector2.Y);
        }

        public static void Write(this BinaryWriter writer, Vector3 vector3)
        {
            writer.Write(vector3.X);
            writer.Write(vector3.Y);
            writer.Write(vector3.Z);
        }

        public static void WriteVector3I(this BinaryWriter writer, Vector3 vector3)
        {
            writer.Write((int)vector3.X);
            writer.Write((int)vector3.Y);
            writer.Write((int)vector3.Z);
        }
    }

    public class ModelFileWriter : IDataFormatWriter
    {
        private int fileVersion;
        private readonly ModelContainer container;

        public ModelFileWriter(ModelContainer container, int version)
        {
            fileVersion = version;
            this.container = container;
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
            using (var writer = new EndiannessAwareBinaryWriter(stream, Encoding.ASCII, ByteOrder.BigEndian))
            {
                writer.Write(fileVersion);
                writer.Write(container.ModelCount);

                foreach (Formats.Model.Model model in container)
                {
                    WriteString(writer, model.Name);
                    writer.Write(model.TextureSize.Width);
                    writer.Write(model.TextureSize.Height);
                    writer.Write(model.PartCount);
                    foreach (ModelPart part in model.GetParts())
                    {
                        WriteString(writer, part.Name);
                        if (fileVersion > 1)
                        {
                            // in case part doesn't have parent
                            WriteString(writer, part.ParentName ?? string.Empty);
                        }
                        writer.Write(part.Translation);
                        writer.Write(part.Rotation);

                        if (fileVersion > 0)
                        {
                            writer.Write(part.AdditionalRotation);
                        }
                        writer.Write(part.BoxCount);
                        foreach (ModelBox box in part.GetBoxes())
                        {
                            writer.Write(box.Position);
                            writer.WriteVector3I(box.Size);
                            writer.Write(box.Uv);
                            writer.Write(box.Inflate);
                            writer.Write(box.Mirror);

                        }
                    }
                }
            }
        }

        private void WriteString(EndiannessAwareBinaryWriter writer, string s)
        {
            writer.Write((short)s.Length);
            writer.WriteString(s);
        }
    }
}
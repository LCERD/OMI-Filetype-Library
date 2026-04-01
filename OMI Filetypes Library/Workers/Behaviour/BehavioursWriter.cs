using OMI.Formats.Behaviour;
using System;
using System.IO;
using System.Text;

namespace OMI.Workers.Behaviour
{
    public class BehavioursWriter : IDataFormatWriter
    {
        private BehaviourFile behaviourFile;

        public static void Write(Stream stream, BehaviourFile file)
        {
            new BehavioursWriter(file).WriteToStream(stream);
        }

        public BehavioursWriter(BehaviourFile file)
        {
            behaviourFile = file;
        }

        public void WriteToFile(string filename)
        {
            using (FileStream fs = File.OpenWrite(filename))
            {
                WriteToStream(fs);
            }
        }

        void IDataFormatWriter.WriteToStream(Stream stream) => WriteToStream(stream);
        public void WriteToStream(Stream stream)
        {
            var writer = new EndiannessAwareBinaryWriter(stream, Encoding.ASCII, leaveOpen: true, ByteOrder.BigEndian);

            writer.Write(0);
            writer.Write(behaviourFile.entries.Count);
            foreach (BehaviourFile.RiderPositionOverride entry in behaviourFile.entries)
            {
                writer.Write((short)entry.name.Length);
                writer.WriteString(entry.name);
                writer.Write(entry.overrides.Count);
                foreach (BehaviourFile.RiderPositionOverride.PositionOverride posOverride in entry.overrides)
                {
                    writer.Write(posOverride.EntityIsTamed);
                    writer.Write(posOverride.EntityHasSaddle);
                    writer.Write(posOverride.x);
                    writer.Write(posOverride.y);
                    writer.Write(posOverride.z);
                }
            }
        }
    }
}
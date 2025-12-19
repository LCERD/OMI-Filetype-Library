using OMI;
using OMI.Formats.Behaviour;
using OMI.Workers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMI.Workers.Behaviour
{
    public class BehavioursReader : IDataFormatReader<BehaviourFile>, IDataFormatReader
    {
        public BehaviourFile FromFile(string filename)
        {
            if (File.Exists(filename))
            {
                using (FileStream fs = File.OpenRead(filename))
                {
                    return FromStream(fs);
                }
            }
            throw new FileNotFoundException(filename);
        }

        public BehaviourFile FromStream(Stream stream)
        {
            BehaviourFile behaviourFile = new BehaviourFile();

            var reader = new EndiannessAwareBinaryReader(stream, Encoding.ASCII, leaveOpen: true, ByteOrder.BigEndian);

            _ = reader.ReadInt32();
            int riderPosOverrideCount = reader.ReadInt32();
            for (int i = 0; i < riderPosOverrideCount; i++)
            {
                short strlen = reader.ReadInt16();
                string name = reader.ReadString(strlen);
                var riderPositionOverride = new BehaviourFile.RiderPositionOverride(name);
                int posOverideCount = reader.ReadInt32();
                for (; 0 < posOverideCount; posOverideCount--)
                {
                    var posOverride = new BehaviourFile.RiderPositionOverride.PositionOverride();
                    posOverride.EntityIsTamed = reader.ReadBoolean();
                    posOverride.EntityHasSaddle = reader.ReadBoolean();
                    posOverride.x = reader.ReadSingle();
                    posOverride.y = reader.ReadSingle();
                    posOverride.z = reader.ReadSingle();
                    riderPositionOverride.overrides.Add(posOverride);
                }
                behaviourFile.entries.Add(riderPositionOverride);
            }
            return behaviourFile;
        }

        object IDataFormatReader.FromStream(Stream stream) => FromStream(stream);

        object IDataFormatReader.FromFile(string filename) => FromFile(filename);
    }
}

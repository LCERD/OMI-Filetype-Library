using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMI.Formats.Ghost
{
    public class GhostRecord : List<GhostRecord.RecordSample>
    {

        public GhostRecord()
        {

        }

        public float startX { get; set; }
        public float startY { get; set; }
        public float startZ { get; set; }
        public long TimeLength { get; set; }
        public UInt32 NumSamples { get; set; }


        public class RecordSample 
        {
            public RecordSample()
            {
                Position = new short[] { 0,0,0 };
                Rotation = new short[] { 0,0,0 };
            }
            public long Timestamp { get; set; }
            public TimeSpan TimestampAsTime { get; set; }

            public short[] Position { get; set; }
            public short[] Rotation { get; set; }
        }

    }
}

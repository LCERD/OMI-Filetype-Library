using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMI.Extentions
{
    internal static class EndiannessAwareBinaryExtention
    {
        internal static void WriteItemCollection<T>(this EndiannessAwareBinaryWriter writer, ICollection<T> collection, Action<EndiannessAwareBinaryWriter, T> writeItem)
        {
            foreach (T item in collection)
            {
                writeItem.Invoke(writer, item);
            }
        }

        internal static void Fill<T>(this EndiannessAwareBinaryReader reader, ICollection<T> list, int count, Func<EndiannessAwareBinaryReader, T> readItemFunc)
        {
            for (int i = 0; i < count; i++)
            {
                if (list.GetType().IsArray)
                {
                    ((T[])list)[i] = readItemFunc(reader);
                    continue;
                }
                list.Add(readItemFunc(reader));
            }
        }

        internal static void FillAtOffset<T>(this EndiannessAwareBinaryReader reader, ICollection<T> list, int count, long offset, Func<EndiannessAwareBinaryReader, T> readItemFunc)
        {
            long origin = reader.BaseStream.Position;
            reader.BaseStream.Position = offset;
            reader.Fill(list, count, readItemFunc);
            reader.BaseStream.Position = origin;
        }
    }
}

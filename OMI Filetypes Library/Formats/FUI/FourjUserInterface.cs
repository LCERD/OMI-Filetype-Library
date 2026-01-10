/*
 * all known FourJUserInterface information is the direct product of Miku-666(NessieHax)'s work! check em out! 
 * https://github.com/NessieHax
 * See License usage at the bottom of file!
*/
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace OMI.Formats.FUI
{
    public sealed class FourjUserInterface
    {
        public FuiHeader Header { get; }

        public List<string> ImportAssets { get; }
        public List<FuiTimeline> Timelines { get; }
        public List<FuiShape> Shapes { get; }
        public List<FuiReference> References { get; }
        public List<FuiEdittext> Edittexts { get; }
        public List<FuiFontName> FontNames { get; }
        public List<FuiSymbol> Symbols { get; }
        public List<FuiBitmap> Bitmaps { get; }

        public FourjUserInterface(string swfFileName, RectangleF frameSize)
            : this(swfFileName, frameSize, 2, 5, 5, 5, 5, 5, 5, 5)
        {
        }

        internal FourjUserInterface(
            string swfFileName, RectangleF frameSize,
            int importAssetsCount,
            int timelinesCount,
            int shapesCount,
            int referencesCount,
            int edittextsCount,
            int fontNamesCount,
            int symbolsCount,
            int bitmapsCount)
        {
            Header = new FuiHeader(swfFileName, frameSize);
            ImportAssets = new(importAssetsCount);
            Timelines = new(timelinesCount);
            Shapes = new(shapesCount);
            References = new(referencesCount);
            Edittexts = new(edittextsCount);
            FontNames = new(fontNamesCount);
            Symbols = new(symbolsCount);
            Bitmaps = new(bitmapsCount);
        }

        public bool AddSymbol(string name, Image image, FuiBitmap.FuiImageFormat fuiImageFormat = FuiBitmap.FuiImageFormat.PNG_WITH_ALPHA_DATA)
        {
            if (image is null)
                return false;
            if (Symbols.Any(sym => sym.Name == name))
                return false;
            Symbols.Add(new FuiSymbol(name, fuiObjectType.BITMAP, Bitmaps.Count));
            Bitmaps.Add(new FuiBitmap(image, fuiImageFormat, Symbols.Count - 1));
            return true;
        }

        public bool AddSymbol(string name, FuiTimeline timeline)
        {
            return false;
        }

        public bool SetSymbol(string name, Image image, FuiBitmap.FuiImageFormat fuiImageFormat = FuiBitmap.FuiImageFormat.PNG_WITH_ALPHA_DATA)
        {
            if (image is null)
                return false;
            if (Symbols.Find(sym => sym.Name == name) is not FuiSymbol symbol)
                return false;
            switch (symbol.ObjectType)
            {
                case fuiObjectType.BITMAP:
                    Bitmaps[symbol.Index] = new FuiBitmap(image, fuiImageFormat, symbol.Index);
                    return true;
                case fuiObjectType.TIMELINE:
                    Timelines.RemoveAt(symbol.Index);
                    symbol.Index = Bitmaps.Count;
                    Bitmaps.Add(new FuiBitmap(image, fuiImageFormat, symbol.Index));
                    return true;
                default:
                    return false;
            }
        }

        public bool SetSymbol(string name, FuiTimeline timeline)
        {
            return false;
        }

        public bool TryGetSymbol(string name, out FuiSymbol symbol)
        {
            symbol = Symbols.Find(sym => sym.Name == name);
            return symbol != null;
        }

        public override string ToString() => Header.ToString();

    }

    public enum fuiObjectType
    {
        STAGE = 0,
        SHAPE = 1,
        TIMELINE = 2,
        BITMAP = 3,
        REFERENCE = 4,
        EDITTEXT = 5,
        CODEGENRECT = 6,
    }

}
/* Copyright (c) 2026-present miku-666
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would be
 *    appreciated but is not required.
 * 2. Altered source versions must be plainly marked as such, and must not be
 *    misrepresented as being the original software.
 * 3. This notice may not be removed or altered from any source distribution.
**/
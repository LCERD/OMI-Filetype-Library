/* Copyright (c) 2022-present miku-666
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
using System;
using System.Collections.Generic;
using System.Linq;

namespace OMI.Formats.Languages
{
    public static class AvailableLanguages
    {
        public const string CzechCzechia = "cs-CS";
        public const string Czechia = "cs-CZ";
        public const string Danish = "da-DA";
        public const string DenmarkDanish = "da-DK";
        public const string GermanAustria = "de-AT";
        public const string German = "de-DE";
        public const string GreekGreece = "el-EL";
        public const string Greece = "el-GR";
        public const string EnglishAustralia = "en-AU";
        public const string EnglishCanada = "en-CA";
        public const string English = "en-EN";
        public const string EnglishUnitedKingdom = "en-GB";
        public const string EnglishIreland = "en-IE";
        public const string EnglishNewZealand = "en-NZ";
        public const string EnglishUnitedStatesOfAmerica = "en-US";
        public const string SpanishSpain = "es-ES";
        public const string SpanishMexico = "es-MX";
        public const string FinnishFinland = "fi-FI";
        public const string FrenchFrance = "fr-FR";
        public const string FrenchCanada = "fr-CA";
        public const string ItalianItaly = "it-IT";
        public const string JapaneseJapan = "ja-JP";
        public const string KoreanSouthKorea = "ko-KR";
        public const string Latin = "la-LAS";
        public const string NorwegianNorway = "no-NO";
        public const string NorwegianBokmålNorway = "nb-NO";
        public const string DutchNetherlands = "nl-NL";
        public const string DutchBelgium = "nl-BE";
        public const string PolishPoland = "pl-PL";
        public const string PortugueseBrazil = "pt-BR";
        public const string PortuguesePortugal = "pt-PT";
        public const string RussianRussia = "ru-RU";
        public const string SlovakSlovakia = "sk-SK";
        public const string SwedishSweden = "sv-SE";
        public const string TurkishTurkey = "tr-TR";
        public const string ChineseChina = "zh-CN"; // Chinese (simplified)
        public const string ChineseHongKong = "zh-HK"; // Chinese (traditional)
        public const string ChineseSingapore = "zh-SG";
        public const string ChineseTaiwan = "zh-TW";
    }

    public class LOCFile
    {
        public class InvalidLanguageException : Exception
        {
            public string Language { get; }
            public InvalidLanguageException(string message, string language) : base(message)
            {
                Language = language;
            }
        }

        public static readonly string[] ValidLanguages = new string[]
        {
            AvailableLanguages.CzechCzechia,
            AvailableLanguages.Czechia,

            "da-CH",
            AvailableLanguages.Danish,
            AvailableLanguages.DenmarkDanish,

            AvailableLanguages.GermanAustria,
            AvailableLanguages.German,

            AvailableLanguages.GreekGreece,
            AvailableLanguages.Greece,

            AvailableLanguages.EnglishAustralia,
            AvailableLanguages.EnglishCanada,
            AvailableLanguages.English,
            AvailableLanguages.EnglishUnitedKingdom,
            "en-GR",
            AvailableLanguages.EnglishIreland,
            AvailableLanguages.EnglishNewZealand,
            AvailableLanguages.EnglishUnitedStatesOfAmerica,

            AvailableLanguages.SpanishSpain,
            AvailableLanguages.SpanishMexico,

            "fi-BE",
            "fi-CH",
            AvailableLanguages.FinnishFinland,

            AvailableLanguages.FrenchFrance,
            AvailableLanguages.FrenchCanada,

            AvailableLanguages.ItalianItaly,

            AvailableLanguages.JapaneseJapan,

            AvailableLanguages.KoreanSouthKorea,

            AvailableLanguages.Latin,

            AvailableLanguages.NorwegianNorway,

            AvailableLanguages.NorwegianBokmålNorway,

            AvailableLanguages.DutchNetherlands,
            AvailableLanguages.DutchBelgium,

            AvailableLanguages.PolishPoland,

            AvailableLanguages.PortugueseBrazil,
            AvailableLanguages.PortuguesePortugal,

            AvailableLanguages.RussianRussia,

            AvailableLanguages.SlovakSlovakia,

            AvailableLanguages.SwedishSweden,

            AvailableLanguages.TurkishTurkey,

            AvailableLanguages.ChineseChina,
            AvailableLanguages.ChineseHongKong,
            AvailableLanguages.ChineseSingapore,
            AvailableLanguages.ChineseTaiwan,
            "zh-CHT",
            "zh-HanS",
            "zh-HanT",
        };

        private Dictionary<string, Dictionary<string, string>> _lockeys = new Dictionary<string, Dictionary<string, string>>();
        private List<string> _languages = new List<string>(ValidLanguages.Length);
        
        internal bool hasUids = false;

        public Dictionary<string, Dictionary<string, string>> LocKeys => _lockeys;
        public List<string> Languages => _languages;

        private Dictionary<string, string> GetTranslation(string locKey)
        {
            if (!LocKeys.ContainsKey(locKey))
                LocKeys.Add(locKey, new Dictionary<string, string>());
            return LocKeys[locKey];
        }

        public Dictionary<string, string> GetLocEntries(string locKey)
        {
            if (!LocKeys.ContainsKey(locKey))
                throw new KeyNotFoundException("Loc key not found");
            return LocKeys[locKey];
        }

        public bool HasLocEntry(string locKey)
            => LocKeys.ContainsKey(locKey);

        public string GetLocEntry(string locKey, string language)
        {
            if (!LocKeys.ContainsKey(locKey))
                throw new KeyNotFoundException(nameof(locKey));
            if (!Languages.Contains(language)) throw new KeyNotFoundException("Language Entry not found");
            return GetTranslation(locKey)[language]?? string.Empty;
        }

        public void SetLocEntry(string locKey, string value)
        {
            foreach (var language in Languages)
            {
                GetTranslation(locKey)[language] = value;
            }
        }

        public void SetLocEntry(string locKey, string language, string value)
        {
            if (!Languages.Contains(language))
                throw new KeyNotFoundException(nameof(language));
            GetTranslation(locKey)[language] = value;
        }

        public bool AddLocKey(string locKey, string value)
        {
            if (LocKeys.ContainsKey(locKey))
                return false;
            Languages.ForEach( language => SetLocEntry(locKey, language, value) );
            return true;
        }

        public bool RemoveLocKey(string locKey)
        {
            if (!LocKeys.ContainsKey(locKey))
                return false;
            return LocKeys.Remove(locKey);
        }

        public void AddLanguage(string language)
        {
            if (!ValidLanguages.Contains(language))
                throw new InvalidLanguageException("Invalid language", language);
            if (Languages.Contains(language))
                throw new InvalidLanguageException("Language already exists", language);
            Languages.Add(language);
            foreach(var key in LocKeys.Keys)
                SetLocEntry(key, language, "");
        }

        public void RemoveLanguage(string language)
        {
            if (!ValidLanguages.Contains(language))
                throw new InvalidLanguageException("Invalid language", language);
            if (!Languages.Contains(language))
                throw new InvalidLanguageException("Language doesn't exist", language);
            if (Languages.Remove(language))
                foreach (var translation in LocKeys.Values)
                    translation.Remove(language);
        }
    }
}

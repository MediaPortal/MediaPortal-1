#region Copyright (C) 2005-2016 Team MediaPortal

// Copyright (C) 2005-2016 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System.Collections.Generic;

namespace MediaPortal.Player.MediaInfo
{
    public abstract class LanguageMediaStream : MediaStream
    {
        private static readonly Dictionary<string, LanguageInfo> _lcids = new Dictionary<string, LanguageInfo>
        {
           { "aa", new LanguageInfo { Name = "Afar", Lcid = 0 } },
           { "ab", new LanguageInfo { Name = "Abkhazian", Lcid = 0 } },
           { "ae", new LanguageInfo { Name = "Avestan", Lcid = 0 } },
           { "af", new LanguageInfo { Name = "Afrikaans", Lcid = 0x0436 } },
           { "ak", new LanguageInfo { Name = "Akan", Lcid = 0 } },
           { "am", new LanguageInfo { Name = "Amharic", Lcid = 0x045e } },
           { "an", new LanguageInfo { Name = "Aragonese", Lcid = 0 } },
           { "ar", new LanguageInfo { Name = "Arabic", Lcid = 0x0401 } },
           { "as", new LanguageInfo { Name = "Assamese", Lcid = 0x044d } },
           { "av", new LanguageInfo { Name = "Avaric", Lcid = 0 } },
           { "ay", new LanguageInfo { Name = "Aymara", Lcid = 0 } },
           { "az", new LanguageInfo { Name = "Azerbaijani", Lcid = 0 } },
           { "ba", new LanguageInfo { Name = "Bashkir", Lcid = 0x046d } },
           { "be", new LanguageInfo { Name = "Belarusian", Lcid = 0x0423 } },
           { "bg", new LanguageInfo { Name = "Bulgarian", Lcid = 0x0402 } },
           { "bh", new LanguageInfo { Name = "Bihari", Lcid = 0 } },
           { "bi", new LanguageInfo { Name = "Bislama", Lcid = 0 } },
           { "bm", new LanguageInfo { Name = "Bambara", Lcid = 0 } },
           { "bn", new LanguageInfo { Name = "Bengali", Lcid = 0x0445 } },
           { "bo", new LanguageInfo { Name = "Tibetan", Lcid = 0x0451 } },
           { "br", new LanguageInfo { Name = "Breton", Lcid = 0x047e } },
           { "bs", new LanguageInfo { Name = "Bosnian", Lcid = 0x141A } },
           { "ca", new LanguageInfo { Name = "Catalan", Lcid = 0x0403 } },
           { "ce", new LanguageInfo { Name = "Chechen", Lcid = 0 } },
           { "ch", new LanguageInfo { Name = "Chamorro", Lcid = 0 } },
           { "co", new LanguageInfo { Name = "Corsican", Lcid = 0x0483 } },
           { "cr", new LanguageInfo { Name = "Cree", Lcid = 0 } },
           { "cs", new LanguageInfo { Name = "Czech", Lcid = 0x0405 } },
           { "cu", new LanguageInfo { Name = "Slave", Lcid = 0 } },
           { "cv", new LanguageInfo { Name = "Chuvash", Lcid = 0 } },
           { "cy", new LanguageInfo { Name = "Welsh", Lcid = 0x0452 } },
           { "da", new LanguageInfo { Name = "Danish", Lcid = 0x0406 } },
           { "de", new LanguageInfo { Name = "German", Lcid = 0x0407 } },
           { "dv", new LanguageInfo { Name = "Divehi", Lcid = 0x0465 } },
           { "dz", new LanguageInfo { Name = "Dzongkha", Lcid = 0 } },
           { "ee", new LanguageInfo { Name = "Ewe", Lcid = 0 } },
           { "el", new LanguageInfo { Name = "Greek", Lcid = 0x0408 } },
           { "en", new LanguageInfo { Name = "English", Lcid = 0x0409 } },
           { "en-gb", new LanguageInfo { Name = "English (Great Britain)", Lcid = 0x0809 } },
           { "en-us", new LanguageInfo { Name = "English (United States)", Lcid = 0x0409 } },
           { "eo", new LanguageInfo { Name = "Esperanto", Lcid = 0 } },
           { "es", new LanguageInfo { Name = "Spanish", Lcid = 0x040a } },
           { "et", new LanguageInfo { Name = "Estonian", Lcid = 0x0425 } },
           { "eu", new LanguageInfo { Name = "Basque", Lcid = 0x042d } },
           { "fa", new LanguageInfo { Name = "Persian", Lcid = 0 } },
           { "ff", new LanguageInfo { Name = "Fulah", Lcid = 0 } },
           { "fi", new LanguageInfo { Name = "Finnish", Lcid = 0x040b } },
           { "fj", new LanguageInfo { Name = "Fijian", Lcid = 0 } },
           { "fo", new LanguageInfo { Name = "Faroese", Lcid = 0x0438 } },
           { "fr", new LanguageInfo { Name = "French", Lcid = 0x040c } },
           { "fy", new LanguageInfo { Name = "Frisian", Lcid = 0x0462 } },
           { "ga", new LanguageInfo { Name = "Irish", Lcid = 0x083c } },
           { "gd", new LanguageInfo { Name = "Gaelic", Lcid = 0x043c } },
           { "gl", new LanguageInfo { Name = "Galician", Lcid = 0x0456 } },
           { "gn", new LanguageInfo { Name = "Guarani", Lcid = 0x0474 } },
           { "gu", new LanguageInfo { Name = "Gujarati", Lcid = 0x0447 } },
           { "gv", new LanguageInfo { Name = "Manx", Lcid = 0 } },
           { "ha", new LanguageInfo { Name = "Hausa", Lcid = 0x0468 } },
           { "he", new LanguageInfo { Name = "Hebrew", Lcid = 0x040d } },
           { "hi", new LanguageInfo { Name = "Hindi", Lcid = 0x0439 } },
           { "ho", new LanguageInfo { Name = "Hiri Motu", Lcid = 0 } },
           { "hr", new LanguageInfo { Name = "Croatian", Lcid = 0x041a } },
           { "ht", new LanguageInfo { Name = "Haitian", Lcid = 0 } },
           { "hu", new LanguageInfo { Name = "Hungarian", Lcid = 0x040e } },
           { "hy", new LanguageInfo { Name = "Armenian", Lcid = 0x042b } },
           { "hz", new LanguageInfo { Name = "Herero", Lcid = 0 } },
           { "ia", new LanguageInfo { Name = "Auxiliary Language Association", Lcid = 0 } },
           { "id", new LanguageInfo { Name = "Indonesian", Lcid = 0x0421 } },
           { "ie", new LanguageInfo { Name = "Interlingue", Lcid = 0 } },
           { "ig", new LanguageInfo { Name = "Igbo", Lcid = 0x0470 } },
           { "ii", new LanguageInfo { Name = "Sichuan Yi", Lcid = 0x0478 } },
           { "ik", new LanguageInfo { Name = "Inupiaq", Lcid = 0 } },
           { "info", new LanguageInfo { Name = "Language info", Lcid = 0 } },
           { "io", new LanguageInfo { Name = "Ido", Lcid = 0 } },
           { "is", new LanguageInfo { Name = "Icelandic", Lcid = 0x040f } },
           { "it", new LanguageInfo { Name = "Italian", Lcid = 0x0410 } },
           { "iu", new LanguageInfo { Name = "Inuktitut", Lcid = 0x045d } },
           { "ja", new LanguageInfo { Name = "Japanese", Lcid = 0x0411 } },
           { "jv", new LanguageInfo { Name = "Javanese", Lcid = 0x0411 } },
           { "ka", new LanguageInfo { Name = "Georgian", Lcid = 0x0437 } },
           { "kg", new LanguageInfo { Name = "Kongo", Lcid = 0 } },
           { "ki", new LanguageInfo { Name = "Kikuyu", Lcid = 0 } },
           { "kj", new LanguageInfo { Name = "Kuanyama", Lcid = 0 } },
           { "kk", new LanguageInfo { Name = "Kazakh", Lcid = 0x043f } },
           { "kl", new LanguageInfo { Name = "Kalaallisut", Lcid = 0 } },
           { "km", new LanguageInfo { Name = "Khmer", Lcid = 0x0453 } },
           { "kn", new LanguageInfo { Name = "Kannada", Lcid = 0x044b } },
           { "ko", new LanguageInfo { Name = "Korean", Lcid = 0x0412 } },
           { "kr", new LanguageInfo { Name = "Kanuri", Lcid = 0x0471 } },
           { "ks", new LanguageInfo { Name = "Kashmiri", Lcid = 0x0860 } },
           { "ku", new LanguageInfo { Name = "Kurdish", Lcid = 0 } },
           { "kv", new LanguageInfo { Name = "Komi", Lcid = 0 } },
           { "kw", new LanguageInfo { Name = "Cornish", Lcid = 0 } },
           { "ky", new LanguageInfo { Name = "Kyrgyz", Lcid = 0x0440 } },
           { "la", new LanguageInfo { Name = "Latin", Lcid = 0x0476 } },
           { "lb", new LanguageInfo { Name = "Luxembourgish", Lcid = 0x046e } },
           { "lg", new LanguageInfo { Name = "Ganda", Lcid = 0 } },
           { "li", new LanguageInfo { Name = "Limburgish", Lcid = 0 } },
           { "ln", new LanguageInfo { Name = "Lingala", Lcid = 0 } },
           { "lo", new LanguageInfo { Name = "Lao", Lcid = 0x0454 } },
           { "lt", new LanguageInfo { Name = "Lithuanian", Lcid = 0x0427 } },
           { "lu", new LanguageInfo { Name = "Luba-Katanga", Lcid = 0 } },
           { "lv", new LanguageInfo { Name = "Latvian", Lcid = 0x0426 } },
           { "mg", new LanguageInfo { Name = "Malagasy", Lcid = 0 } },
           { "mh", new LanguageInfo { Name = "Marshallese", Lcid = 0 } },
           { "mi", new LanguageInfo { Name = "Maori", Lcid = 0x0481 } },
           { "mk", new LanguageInfo { Name = "Macedonian", Lcid = 0x042f } },
           { "ml", new LanguageInfo { Name = "Malayalam", Lcid = 0x044c } },
           { "mn", new LanguageInfo { Name = "Mongolian", Lcid = 0x0850 } },
           { "mo", new LanguageInfo { Name = "Moldavian", Lcid = 0 } },
           { "more", new LanguageInfo { Name = "Language, more info", Lcid = 0 } },
           { "mr", new LanguageInfo { Name = "Marathi", Lcid = 0x044e } },
           { "ms", new LanguageInfo { Name = "Malay", Lcid = 0x043e } },
           { "mt", new LanguageInfo { Name = "Maltese", Lcid = 0x043a } },
           { "mul", new LanguageInfo { Name = "Multiple languages", Lcid = 0 } },
           { "my", new LanguageInfo { Name = "Burmese", Lcid = 0x0455 } },
           { "na", new LanguageInfo { Name = "Nauru", Lcid = 0 } },
           { "nb", new LanguageInfo { Name = "Norwegian Bokmal", Lcid = 0x0414 } },
           { "nd", new LanguageInfo { Name = "Ndebele", Lcid = 0 } },
           { "ne", new LanguageInfo { Name = "Nepali", Lcid = 0x0461 } },
           { "ng", new LanguageInfo { Name = "Ndonga", Lcid = 0 } },
           { "nl", new LanguageInfo { Name = "Dutch", Lcid = 0x0413 } },
           { "nn", new LanguageInfo { Name = "Norwegian Nynorsk", Lcid = 0x0814 } },
           { "no", new LanguageInfo { Name = "Norwegian", Lcid = 0x0414 } },
           { "nr", new LanguageInfo { Name = "Ndebele", Lcid = 0 } },
           { "nv", new LanguageInfo { Name = "Navaho", Lcid = 0 } },
           { "ny", new LanguageInfo { Name = "Nyanja", Lcid = 0 } },
           { "oc", new LanguageInfo { Name = "Occitan", Lcid = 0x0482 } },
           { "oj", new LanguageInfo { Name = "Ojibwa", Lcid = 0 } },
           { "om", new LanguageInfo { Name = "Oromo", Lcid = 0x0472 } },
           { "or", new LanguageInfo { Name = "Oriya", Lcid = 0x0448 } },
           { "os", new LanguageInfo { Name = "Ossetic", Lcid = 0 } },
           { "pa", new LanguageInfo { Name = "Punjabi", Lcid = 0x0446 } },
           { "pi", new LanguageInfo { Name = "Pali", Lcid = 0 } },
           { "pl", new LanguageInfo { Name = "Polish", Lcid = 0x0415 } },
           { "ps", new LanguageInfo { Name = "Pashto", Lcid = 0x0463 } },
           { "pt", new LanguageInfo { Name = "Portuguese", Lcid = 0x0816 } },
           { "pt-br", new LanguageInfo { Name = "Portuguese (Brazil)", Lcid = 0x0416 } },
           { "qu", new LanguageInfo { Name = "Quechua", Lcid = 0 } },
           { "rm", new LanguageInfo { Name = "Raeto-Romance", Lcid = 0 } },
           { "rn", new LanguageInfo { Name = "Rundi", Lcid = 0 } },
           { "ro", new LanguageInfo { Name = "Romanian", Lcid = 0x0418 } },
           { "ru", new LanguageInfo { Name = "Russian", Lcid = 0x0419 } },
           { "rw", new LanguageInfo { Name = "Kinyarwanda", Lcid = 0x0487 } },
           { "sa", new LanguageInfo { Name = "Sanskrit", Lcid = 0x044f } },
           { "sc", new LanguageInfo { Name = "Sardinian", Lcid = 0 } },
           { "sd", new LanguageInfo { Name = "Sindhi", Lcid = 0x0459 } },
           { "se", new LanguageInfo { Name = "Northern Sami", Lcid = 0x043b } },
           { "sg", new LanguageInfo { Name = "Sango", Lcid = 0 } },
           { "si", new LanguageInfo { Name = "Sinhala", Lcid = 0 } },
           { "sk", new LanguageInfo { Name = "Slovak", Lcid = 0x041b } },
           { "sl", new LanguageInfo { Name = "Slovenian", Lcid = 0x0424 } },
           { "sm", new LanguageInfo { Name = "Samoan", Lcid = 0 } },
           { "sn", new LanguageInfo { Name = "Shona", Lcid = 0 } },
           { "so", new LanguageInfo { Name = "Somali", Lcid = 0x0477 } },
           { "sq", new LanguageInfo { Name = "Albanian", Lcid = 0x041c } },
           { "sr", new LanguageInfo { Name = "Serbian", Lcid = 0x0c1a } },
           { "ss", new LanguageInfo { Name = "Swati", Lcid = 0 } },
           { "st", new LanguageInfo { Name = "Sotho", Lcid = 0 } },
           { "su", new LanguageInfo { Name = "Sundanese", Lcid = 0 } },
           { "sv", new LanguageInfo { Name = "Swedish", Lcid = 0x041d } },
           { "sw", new LanguageInfo { Name = "Swahili", Lcid = 0x0441 } },
           { "ta", new LanguageInfo { Name = "Tamil", Lcid = 0x0449 } },
           { "te", new LanguageInfo { Name = "Telugu", Lcid = 0x044a } },
           { "tg", new LanguageInfo { Name = "Tajik", Lcid = 0x0428 } },
           { "th", new LanguageInfo { Name = "Thai", Lcid = 0x041e } },
           { "ti", new LanguageInfo { Name = "Tigrinya", Lcid = 0 } },
           { "tk", new LanguageInfo { Name = "Turkmen", Lcid = 0x0442 } },
           { "tl", new LanguageInfo { Name = "Tagalog", Lcid = 0 } },
           { "tn", new LanguageInfo { Name = "Tswana", Lcid = 0x0432 } },
           { "to", new LanguageInfo { Name = "Tonga", Lcid = 0 } },
           { "tr", new LanguageInfo { Name = "Turkish", Lcid = 0x041f } },
           { "ts", new LanguageInfo { Name = "Tsonga", Lcid = 0x0431 } },
           { "tt", new LanguageInfo { Name = "Tatar", Lcid = 0x0444 } },
           { "tw", new LanguageInfo { Name = "Twi", Lcid = 0 } },
           { "ty", new LanguageInfo { Name = "Tahitian", Lcid = 0 } },
           { "ug", new LanguageInfo { Name = "Uighur", Lcid = 0x0480 } },
           { "uk", new LanguageInfo { Name = "Ukrainian", Lcid = 0x0422 } },
           { "ur", new LanguageInfo { Name = "Urdu", Lcid = 0x0420 } },
           { "uz", new LanguageInfo { Name = "Uzbek", Lcid = 0x0843 } },
           { "ve", new LanguageInfo { Name = "Venda", Lcid = 0x0433 } },
           { "vi", new LanguageInfo { Name = "Vietnamese", Lcid = 0x042a } },
           { "vo", new LanguageInfo { Name = "Volapuk", Lcid = 0 } },
           { "wa", new LanguageInfo { Name = "Walloon", Lcid = 0 } },
           { "wo", new LanguageInfo { Name = "Wolof", Lcid = 0x0488 } },
           { "xh", new LanguageInfo { Name = "Xhosa", Lcid = 0x0434 } },
           { "yi", new LanguageInfo { Name = "Yiddish", Lcid = 0x043d } },
           { "yo", new LanguageInfo { Name = "Yoruba", Lcid = 0x046a } },
           { "za", new LanguageInfo { Name = "Zhuang", Lcid = 0 } },
           { "zh", new LanguageInfo { Name = "Chinese", Lcid = 0x0804 } },
           { "zh-cn", new LanguageInfo { Name = "Chinese (China)", Lcid = 0x0804 } },
           { "zh-tw", new LanguageInfo { Name = "Chinese (Taiwan)", Lcid = 0x0404 } },
           { "zu", new LanguageInfo { Name = "Zulu", Lcid = 0x0435 } },
        };

        protected LanguageMediaStream(MediaInfo info, int number)
            : base(info, number)
        {
        }

        public string Language { get; set; }

        public int Lcid { get; set; }

        public bool Default { get; set; }

        public bool Forced { get; set; }

        protected override void AnalyzeStreamInternal(MediaInfo info)
        {
            base.AnalyzeStreamInternal(info);
            var language = GetString(info, "Language").ToLower();
            Default = GetBool(info, "Default");
            Forced = GetBool(info, "Forced");
            Language = GetLanguage(language);
            Lcid = GetLcid(language);
        }

        public string GetLanguage(string source)
        {
            LanguageInfo info;
            return _lcids.TryGetValue(source, out info) ? info.Name : "Unknown";
        }

        public int GetLcid(string source)
        {
            LanguageInfo info;
            return _lcids.TryGetValue(source, out info) ? info.Lcid : 0;
        }

        private sealed class LanguageInfo
        {
            public string Name { get; set; }

            public int Lcid { get; set; }
        }
    }
}
#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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

using System;
using System.Collections.Generic;
using System.Globalization;

namespace TvLibrary.Epg
{
  /// <summary>
  /// Class which holds all dvb epg languages
  /// </summary>
  public class Languages
  {
    #region Singleton
    private static Languages instance = null;
    private List<KeyValuePair<String, String>> langs = new List<KeyValuePair<String, String>>();

    public Languages()
    {
      CultureInfo[] cinfos = CultureInfo.GetCultures(CultureTypes.AllCultures);
      foreach (CultureInfo ci in cinfos)
      {
        langs.Add(new KeyValuePair<String, String>(ci.ThreeLetterISOLanguageName, ci.EnglishName));
      }
      // Exceptions with a double ISO639-2 code (B and T). Adding code B
      foreach (string[] languageException in LanguageExceptions)
        langs.Add(new KeyValuePair<String, String>(languageException[1], languageException[0]));
    }

    public static Languages Instance
    {
      get
      {
        if (instance == null)
          instance = new Languages();
        return instance;
      }
    }
    #endregion
    //
    // Created needed expections lists for all cultures with two ISO-639-2 ( B and T )
    // (data taken from http://en.wikipedia.org/wiki/List_of_ISO_639-2_codes)
    // 
    // There were 22 B codes; scc and scr are now deprecated but still used by some EPG providers :(
    //
    private string[][] LanguageExceptions = { new[] {"Albanian","alb"},       new[] {"Armenian","arm"},
                                              new[] {"Basque","baq"},         new[] {"Burmese","bur"},
                                              new[] {"Chinese", "chi" },      new[] {"Czech","cze"},
                                              new[] {"Dutch","dut"},          new[] {"French","fre"},
                                              new[] {"Georgian","geo"},       new[] {"German","ger"},
                                              new[] {"Greek (Modern)", "gre"},new[] {"Icelandic","ice"},
                                              new[] {"Macedonian","mac"},     new[] {"Malay","may"},
                                              new[] {"Maori","mao"},          new[] {"Persian","per"},
                                              new[] {"Romanian","rum"},       new[] {"Tibetan","tib"},
                                              new[] {"Welsh","wel"},          new[] {"Serbo-Croatian (Cyrillic)","scc"},
                                              new[] {"Serbo-Croatian (Roman)","scr"}
                                            };

    /// <summary>
    /// Gets the languages.
    /// </summary>
    /// <returns>list of all pairs of languagecode/languagename</returns>
    public List<KeyValuePair<String, String>> GetLanguagePairs()
    {
      return langs;
    }

    /// <summary>
    /// Gets the languages.
    /// </summary>
    /// <returns>list of all languages</returns>
    public List<String> GetLanguages()
    {
      List<String> result = new List<string>();
      foreach (KeyValuePair<String, String> kv in langs)
      {
        result.Add(kv.Value);
      }
      return result;
    }

    /// <summary>
    /// Gets the language codes.
    /// </summary>
    /// <returns>list of all language codes</returns>
    public List<String> GetLanguageCodes()
    {
      List<String> result = new List<string>();
      foreach (KeyValuePair<String, String> kv in langs)
      {
        result.Add(kv.Key);
      }
      return result;
    }

    /// <summary>
    /// Gets the language from a language code.
    /// </summary>
    /// <param name="code">The code.</param>
    /// <returns>language</returns>
    public string GetLanguageFromCode(string code)
    {

      if (String.IsNullOrEmpty(code))
      {
        return "";
      }
      if (code.Length > 3)
      {
        return code;
      }

      foreach (KeyValuePair<String, String> kv in langs)
      {
        if (kv.Key == code)
        {
          return kv.Value;
        }
      }

      return code;
    }
  }
}
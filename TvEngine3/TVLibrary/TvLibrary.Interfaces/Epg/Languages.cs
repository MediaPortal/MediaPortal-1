#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
    //
    // Created needed expections lists for all cultures with two ISO-639-2 ( B and T )
    // (data taken from http://en.wikipedia.org/wiki/List_of_ISO_639-2_codes)
    // 
    private string[] LanguageNameExceptions = {
                                                "Albanian", "Armenian",
                                                "Basque", "Burmese",
                                                "Chinese", "Czech",
                                                "Dutch",
                                                "French",
                                                "Georgian", "German", "Greek (Modern)",
                                                "Icelandic",
                                                "Macedonian", "Malay", "Maori",
                                                "Persian",
                                                "Romanian",
                                                "Tibetan",
                                                "Welsh"
                                              };

    private string[] LanguageCodeExceptions = {
                                                "alb", "arm",
                                                "baq", "bur",
                                                "chi", "cze",
                                                "dut",
                                                "fre",
                                                "geo", "ger", "gre",
                                                "ice",
                                                "mac", "may", "mao",
                                                "per",
                                                "rum",
                                                "tib",
                                                "wel"
                                              };

    /// <summary>
    /// Gets the languages.
    /// </summary>
    /// <returns>list of all languages</returns>
    public List<String> GetLanguages()
    {
      List<String> langs = new List<String>();

      CultureInfo[] cinfos = CultureInfo.GetCultures(CultureTypes.AllCultures);
      foreach (CultureInfo ci in cinfos)
      {
        langs.Add(ci.EnglishName);
      }
      // Exceptions with a double ISO639-2 code (B and T). Adding code B
      langs.AddRange(LanguageNameExceptions);
      return langs;
    }

    /// <summary>
    /// Gets the language codes.
    /// </summary>
    /// <returns>list of all language codes</returns>
    public List<String> GetLanguageCodes()
    {
      List<String> langs = new List<String>();

      CultureInfo[] cinfos = CultureInfo.GetCultures(CultureTypes.AllCultures);
      foreach (CultureInfo ci in cinfos)
      {
        langs.Add(ci.ThreeLetterISOLanguageName);
      }
      // Exceptions with a double ISO639-2 code (B and T). Adding code B
      langs.AddRange(LanguageCodeExceptions);
      return langs;
    }

    /// <summary>
    /// Gets the language from a language code.
    /// </summary>
    /// <param name="code">The code.</param>
    /// <returns>language</returns>
    public static string GetLanguageFromCode(string code)
    {
      if (String.IsNullOrEmpty(code))
      {
        return "";
      }
      if (code.Length > 3)
      {
        return code;
      }
      CultureInfo[] cinfos = CultureInfo.GetCultures(CultureTypes.AllCultures);
      foreach (CultureInfo ci in cinfos)
      {
        if (ci.ThreeLetterISOLanguageName == code)
        {
          return ci.EnglishName;
        }
      }
      return code;
    }
  }
}
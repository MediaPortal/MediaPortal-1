/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
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

    /// <summary>
    /// Gets the languages.
    /// </summary>
    /// <returns>list of all languages</returns>
    public List<String> GetLanguages()
    {
      List<String> langs = new List<String>();

      CultureInfo[] cinfos = CultureInfo.GetCultures(CultureTypes.AllCultures);
      foreach (CultureInfo ci in cinfos)
        langs.Add(ci.EnglishName);
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
        langs.Add(ci.ThreeLetterISOLanguageName);
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

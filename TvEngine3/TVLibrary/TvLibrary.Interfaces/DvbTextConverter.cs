#region Copyright (C) 2005-2018 Team MediaPortal

// Copyright (C) 2005-2018 Team MediaPortal
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
using System.Runtime.InteropServices;

namespace TvLibrary.Interfaces
{
  /// <summary>
  /// DVB text strings to Unicode converter
  /// </summary>
  public class DvbTextConverter
  {
    /// <summary>
    /// Convert DVB string to Unicode according to provided language
    /// </summary>
    public static string Convert(IntPtr ptr, string lang)
    {
      int len = 0;
      int pos = 0;
      //string aLang = lang;
      int encoding = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ANSICodePage;
      try
      {
        byte c = Marshal.ReadByte(ptr, 0);
        if (c >= 0x20)
        {
          encoding = 20269;
        }
        else
        {
          pos = 1;
          switch (c)
          {
            case 0x00: return "";
            case 0x01: encoding = 28595; /* ISO-8859-5  */  break;
            case 0x02: encoding = 28596; /* ISO-8859-6  */  break;
            case 0x03: encoding = 28597; /* ISO-8859-7  */  break;
            case 0x04: encoding = 28598; /* ISO-8859-8  */  break;
            case 0x05: encoding = 28599; /* ISO-8859-9  */  break;
            case 0x06: encoding = 28600; /* ISO-8859-10 */  break;
            case 0x07: encoding = 874;   /* ISO-8859-11 */  break;
            //case 0x08: encoding = 28602; /* ISO-8859-12 doesn't exist */  break;
            case 0x09: encoding = 28603; /* ISO-8859-13 */  break;
            case 0x0A: encoding = 28604; /* ISO-8859-14 */  break;
            case 0x0B: encoding = 28605; /* ISO-8859-15 */  break;
            case 0x10:
              {
                pos = 3;
                c = Marshal.ReadByte(ptr, 2);
                switch (c)
                {
                  case 0x01: encoding = 28591; /* ISO-8859-1  */  break;
                  case 0x02: encoding = 28592; /* ISO-8859-2  */  break;
                  case 0x03: encoding = 28593; /* ISO-8859-3  */  break;
                  case 0x04: encoding = 28594; /* ISO-8859-4  */  break;
                  case 0x05: encoding = 28595; /* ISO-8859-5  */  break;
                  case 0x06: encoding = 28596; /* ISO-8859-6  */  break;
                  case 0x07: encoding = 28597; /* ISO-8859-7  */  break;
                  case 0x08: encoding = 28598; /* ISO-8859-8  */  break;
                  case 0x09: encoding = 28599; /* ISO-8859-9  */  break;
                  case 0x0A: encoding = 28600; /* ISO-8859-10 */  break;
                  case 0x0B: encoding = 874;   /* ISO-8859-11 */  break;
                  //case 0x0C: encoding = 28602; /* ISO-8859-12 doesn't exist */  break;
                  case 0x0D: encoding = 28603; /* ISO-8859-13 */  break;
                  case 0x0E: encoding = 28604; /* ISO-8859-14 */  break;
                  case 0x0F: encoding = 28605; /* ISO-8859-15 */  break;
                }
                break;
              }
            case 0x11: encoding = 1200;  /* ISO/IEC 10646-1 */ break;
            case 0x12: encoding = 949;   /* KSC5601-1987   */  break;
            case 0x13: encoding = 20936; /* GB-2312-1980   */  break;
            case 0x14: encoding = 950;   /* Big5 */            break;
            case 0x15: encoding = 65001; /* UTF-8 */           break;
          }
        }
        len = pos;
        while (Marshal.ReadByte(ptr, len) != 0)
          len++;
      }
      catch (Exception ex)
      {
        Log.Log.WriteFile("Error while converting dvb text", ex);
      }

      byte[] text = new byte[len - pos];
      for (int i = 0; i < len - pos; i++)
        text[i] = Marshal.ReadByte(ptr, i + pos);
      
      if (encoding == 20269) //Only for default-encoded DVB text strings
      {
        //This is an old hack/workaround that is probably no longer needed..
        if (string.IsNullOrEmpty(lang))
        {
          lang = System.Globalization.CultureInfo.CurrentCulture.ThreeLetterISOLanguageName;
        }
        lang = lang.ToLowerInvariant();
        if (lang == "ukr" || lang == "bel" || lang == "rus")
        {
          encoding = 28595; //ISO-8859-5
        }
        //if (lang == "cze" || lang == "ces")
        //{
        //  encoding = 20269; //ISO-6937
        //}
      }

      string result;
      if (encoding == 20269)
        result = ISOTextDecoder.from_ISO_6937_EU(text);
      else if (encoding == 20600)
        result = ISOTextDecoder.from_ISO_8859_10(text);
      else if (encoding == 20604)
        result = ISOTextDecoder.from_ISO_8859_14(text);
      else
        result = System.Text.Encoding.GetEncoding(encoding).GetString(text);
      // if (aLang != string.Empty)
      //   Log.Log.Debug("DVBTextConverter: converted string with given lang {0}(->{1}), encoding detected: {2}. resulting string: \"{3}\"",
      //     aLang, lang, encoding, result);
      return result;
    }
  }
}
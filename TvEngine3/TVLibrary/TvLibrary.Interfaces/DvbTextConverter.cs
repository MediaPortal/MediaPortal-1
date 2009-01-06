#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

#endregion

using System;
using System.Text;
using System.Globalization;
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
      int encoding = CultureInfo.CurrentCulture.TextInfo.ANSICodePage;
      try
      {
        if (string.IsNullOrEmpty(lang))
          lang = CultureInfo.CurrentCulture.ThreeLetterISOLanguageName;
        lang = lang.ToLowerInvariant();
        if (lang == "cze" || lang == "ces")
          encoding = 20269; //ISO-6937
        else if (lang == "ukr" || lang == "bel" || lang == "rus")
          encoding = 28595; //ISO-8859-5

        byte c = Marshal.ReadByte(ptr, 0);
        if (c < 0x20)
        {
          pos = 1;
          switch (c)
          {
            case 0x00:
              return "";
            case 0x01:
              encoding = 28595; //ISO-8859-5
              break;
            case 0x02:
              encoding = 28596; //ISO-8859-6
              break;
            case 0x03:
              encoding = 28597; //ISO-8859-7
              break;
            case 0x04:
              encoding = 28598; //ISO-8859-8
              break;
            case 0x05:
              encoding = 28599; //ISO-8859-9
              break;
            //case 0x06: encoding = ; //ISO-8859-10
            //	break;
            case 0x07:
              encoding = 874; //ISO-8859-11
              break;
            //case 0x08: encoding = ; //ISO-8859-12
            //	break;
            case 0x09:
              encoding = 28603; //ISO-8859-13
              break;
            //case 0x0A: encoding = ; //ISO-8859-14
            //	break;
            case 0x0B:
              encoding = 28605; //ISO-8859-15
              break;
            case 0x10:
              {
                pos = 3;
                c = Marshal.ReadByte(ptr, 2);
                switch (c)
                {
                  case 0x01:
                    encoding = 28591; //ISO-8859-1
                    break;
                  case 0x02:
                    encoding = 28592; //ISO-8859-2
                    break;
                  case 0x03:
                    encoding = 28593; //ISO-8859-3
                    break;
                  case 0x04:
                    encoding = 28594; //ISO-8859-4
                    break;
                  case 0x05:
                    encoding = 28595; //ISO-8859-5
                    break;
                  case 0x06:
                    encoding = 28596; //ISO-8859-6
                    break;
                  case 0x07:
                    encoding = 28597; //ISO-8859-7
                    break;
                  case 0x08:
                    encoding = 28598; //ISO-8859-8
                    break;
                  case 0x09:
                    encoding = 28599; //ISO-8859-9
                    break;
                  //case 0x0A: encoding = ; //ISO-8859-10
                  //	break;
                  case 0x0B:
                    encoding = 874; //ISO-8859-11
                    break;
                  //case 0x0C: encoding = ; //ISO-8859-12
                  //	break;
                  case 0x0D:
                    encoding = 28591; //ISO-8859-13
                    break;
                  //case 0x0E: encoding = ; //ISO-8859-14
                  //	break;
                  case 0x0F:
                    encoding = 28591; //ISO-8859-15
                    break;
                }
                break;
              }
            case 0x11:
              encoding = 1200; //ISO/IEC 10646-1
              break;
            case 0x12:
              encoding = 949; //KSC5601-1987
              break;
            case 0x13:
              encoding = 936; //GB-2312-1980
              break;
            case 0x14:
              encoding = 950; //Big5
              break;
            case 0x15:
              encoding = 65001; //UTF-8
              break;
          }
        }
        len = pos;
        while (Marshal.ReadByte(ptr, len) != 0)
          len++;
      } catch (Exception ex)
      {
        Log.Log.WriteFile("Error while converting dvb text", ex);
      }
      byte[] text = new byte[len - pos];
      for (int i = 0; i < len - pos; i++)
      {
        text[i] = Marshal.ReadByte(ptr, i + pos);
      }
      return Encoding.GetEncoding(encoding).GetString(text);
    }
  }
}

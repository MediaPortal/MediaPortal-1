#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.Web;
using System.Text;
using System.Collections;

namespace MediaPortal.Utils.Web
{
  public class HtmlString
  {
    #region Constructors/Destructors
    private HtmlString()
    {
    }
    #endregion

    #region Public Static Methods

    public static MatchTagCollection TagList(string source)
    {
      MatchTagCollection list = new MatchTagCollection();

      for (int i = 0; i < source.Length; i++)
      {
        if (source[i] == '<')
        {
          int length = TagLength(source, i);

          // check if tag is too short or comment
          // min lenght for a tag is 3 <x>
          // comments start <!-- 
          if (length >= 3 &&
            (source[i + 1] == '/' || source[i + 1] == '!' ||
            (source[i + 1] >= 'A' && source[i + 1] <= 'Z') ||
            (source[i + 1] >= 'a' && source[i + 1] <= 'z')))
          {
            MatchTag tag = new MatchTag(source, i, length);
            list.Add(tag);
            i = i + length - 1;
          }
        }
      }

      return list;
    }

    public static string ToAscii(string source)
    {
      string stripped;

      stripped = NewLines(source);
      stripped = Decode(stripped);

      return stripped;
    }

    public static string Decode(string source)
    {
      string stripped = source;

      stripped = HttpUtility.HtmlDecode(stripped);

      // replace unicode characters
      stripped = stripped.Replace((char)145, '’');
      stripped = stripped.Replace((char)150, '-');
      stripped = stripped.Replace((char)160, ' ');

      return stripped;
    }

    public static string NewLines(string source)
    {
      string stripped = source;

      stripped = stripped.Replace("\n", "");
      stripped = stripped.Replace("\r", "");
      stripped = stripped.Replace("\t", "");
      stripped = stripped.Replace("<br>", "\n");

      return stripped;
    }
    #endregion

    #region Private Static Methods

    private static int TagLength(string strSource, int StartPos)
    {
      int index = 0;
      int nesting = 0;

      if (strSource[StartPos] == '<')
        index++;

      while (StartPos + index < strSource.Length)
      {
        if (strSource[StartPos + index] == '<')
          nesting++;
        if (strSource[StartPos + index] == '>')
        {
          if (nesting > 0)
          {
            nesting--;
          }
          else
          {
            index++;
            break;
          }
        }
        index++;
      }

      if (nesting > 0)
        return 0;

      return index;

    }
    #endregion
  }
}

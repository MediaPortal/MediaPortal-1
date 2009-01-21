#region Copyright (C) 2005-2009 Team MediaPortal

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

#endregion

using System.Collections;
using System.Text.RegularExpressions;
using MediaPortal.Utils.Web;

namespace MediaPortal.WebEPG.Parser
{
  /// <summary>
  /// Parses a single data row
  /// </summary>
  public class DataRowParser
  {
    #region Variables

    private ArrayList _templateData;
    private string _fieldDelimiter;

    #endregion

    #region Constructors/Destructors

    public DataRowParser(string template, string delimiter)
    {
      _fieldDelimiter = delimiter;
      _templateData = GetElements(template);
    }

    #endregion

    #region Public Methods

    public bool ParseRow(string source, ref IParserData data)
    {
      ArrayList sourceData = GetElements(source);

      for (int i = 0; i < _templateData.Count; i++)
      {
        string template = (string) _templateData[i];

        if (template.IndexOf("#") != -1)
        {
          string rowSource = (string) sourceData[i];
          data.SetElement(template, rowSource);
        }
      }
      return true;
    }

    #endregion

    #region Private Methods

    private ArrayList GetElements(string source)
    {
      ArrayList elements = new ArrayList();

      //source = HtmlString.ToAscii(source);

      Regex elementTag = new Regex(_fieldDelimiter);
      MatchCollection elementTags = elementTag.Matches(source);
      for (int i = 0; i < elementTags.Count; i++)
      {
        Match delim = elementTags[i];

        if (i == 0 && delim.Index == 0)
        {
          continue;
        }

        int start = 0;
        int end = delim.Index;
        if (i - 1 >= 0)
        {
          Match last = elementTags[i - 1];

          start = last.Index + last.Length;
        }

        string field = source.Substring(start, end - start);
        elements.Add(field);
      }

      return elements;
    }

    #endregion
  }
}
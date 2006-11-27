#region Copyright (C) 2006 Team MediaPortal
/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Text.RegularExpressions;

namespace MediaPortal.Utils.Web
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class HtmlParser
  {
    #region Variables
    HtmlParserTemplate _template;
    HtmlProfiler _profiler;
    HtmlSectionParser _sectionParser;
    string _sectionSource;
    Type _dataType;
    object[] _dataArgs;
    #endregion

    #region Constructors/Destructors
    public HtmlParser(HtmlParserTemplate template, Type dataType, params object[] dataArgs)
    {
      _template = template;
      _dataType = dataType;
      _dataArgs = dataArgs;
      _sectionSource = string.Empty;
      _profiler = new HtmlProfiler(_template.SectionTemplate);
      _sectionParser = new HtmlSectionParser(_template.SectionTemplate);
    }
    #endregion

    #region Public Methods
    public int ParseUrl(HTTPRequest site)
    {
      HTMLPage webPage = new HTMLPage(site);
      string pageSource = webPage.GetPage();

      int startIndex = 0;
      if (_template.Start != null && _template.Start != string.Empty)
      {
        startIndex = pageSource.IndexOf(_template.Start, 0);
        if (startIndex == -1)
          startIndex = 0;
        //return -1;
      }


      int endIndex = pageSource.Length;

      if (_template.End != null && _template.End != string.Empty)
      {
        endIndex = pageSource.ToLower().IndexOf(_template.End, startIndex);
        if (endIndex == -1)
          endIndex = pageSource.Length;
        //return -1;
      }

      int count = 0;
      if (pageSource != null)
      {
        count = _profiler.MatchCount(pageSource.Substring(startIndex, endIndex - startIndex));
      }

      return count;
    }

    public IParserData GetData(int index)
    {
      string sectionSource;
      if (_sectionSource != string.Empty)
      {
        sectionSource = _sectionSource;
        _sectionSource = string.Empty;
      }
      else
      {
        sectionSource = _profiler.GetSource(index);
      }

      IParserData sectionData = (IParserData)Activator.CreateInstance(_dataType, _dataArgs);
      if (_sectionParser.ParseSection(sectionSource, ref sectionData))
        return sectionData;
      return null;
    }

    public string SearchRegex(int index, string regex, bool remove)
    {
      string sectionSource;
      if (_sectionSource != string.Empty)
      {
        sectionSource = _sectionSource;
      }
      else
      {
        sectionSource = _profiler.GetSource(index);
      }


      Match result = null;
      try
      {
        Regex searchRegex = new Regex(regex);
        result = searchRegex.Match(sectionSource);
      }
      catch (System.ArgumentException)// ex)
      {
        //_log.Error("Html Parser: Regex error: {0} {1}", regex, ex.ToString());
        return string.Empty;
      }

      string found = string.Empty;
      if (result.Success)
      {
        found = sectionSource.Substring(result.Index, result.Length);
        if (remove)
        {
          _sectionSource = sectionSource.Substring(0, result.Index);

          int end = result.Index + result.Length;
          _sectionSource += sectionSource.Substring(end, sectionSource.Length - end);
        }

      }

      return found;
    }

    public bool GetHyperLink(int index, string match, ref HTTPRequest linkURL)
    {

      string regex = "<a href=[^>]*" + match.ToLower() + "[^>]*>";

      string result = SearchRegex(index, regex, false);

      bool linkFound = false;
      string strLinkURL = string.Empty;

      if (result != "")
      {
        int start = -1;
        char delim = '>';

        if ((start = result.IndexOf("=")) != -1)
        {
          for (int i = 0; i < result.Length - start; i++)
          {
            if (result[start + i] == '\"' || result[start + i] == '\'')
            {
              delim = result[start + i];
              break;
            }
          }
        }

        int end = -1;
        if (delim != '>')
        {
          start = -1;
          start = result.IndexOf(delim);
        }
        if (start != -1)
          end = result.IndexOf(delim, ++start);
        if (end != -1)
        {
          strLinkURL = result.Substring(start, end - start);
          linkFound = true;
        }
      }

      //if(strLinkURL.ToLower().IndexOf("http") == -1)
      //{
      //if (strLinkURL.ToLower().IndexOf("javascript") != -1)
      //{
      string[] param = GetJavaSubLinkParams(result); //strLinkURL);
      if (param != null)
      {
        if (param.Length == 1 && !linkURL.HasTag("[1]"))
        {
          linkURL = linkURL.Add(param[0]);
        }
        else
        {
          for (int i = 0; i < param.Length; i++)
            linkURL.ReplaceTag("[" + (i + 1).ToString() + "]", param[i]);
        }
      }
      else
      {
        linkURL = linkURL.Add(strLinkURL.Trim());
      }
      //}

      return linkFound;
    }

    //public ParserDataCollection ParseUrl(HTTPRequest site)
    //{
    //  ParserDataCollection sectionList = new ParserDataCollection();

    //  HTMLPage webPage = new HTMLPage(site);
    //  string pageSource = webPage.GetPage();

    //  if (pageSource != null)
    //  {
    //    int count = _profiler.MatchCount(pageSource);

    //    for (int i = 0; i < count; i++)
    //    {
    //      string sectionSource = _profiler.GetSource(i);

    //      IParserData sectionData = (IParserData ) Activator.CreateInstance(_dataType);
    //      if (_sectionParser.ParseSection(sectionSource, ref sectionData))
    //        sectionList.Add(sectionData);
    //    }
    //  }

    //  return sectionList;
    //}
    #endregion

    #region Private Methods
    private string[] GetJavaSubLinkParams(string link)
    {

      int args = -1;
      int[,] param = null;
      int start = -1;

      if ((start = link.IndexOf("(")) != -1)
      {
        args = 0;
        param = new int[link.Length - start, 2];
        param[0, 0] = start + 1;
        for (int i = 0; i < link.Length - start; i++)
        {
          if (link[start + i] == ',')
          {
            param[args, 1] = start + i;
            args++;
            param[args, 0] = start + i + 1;
          }
          if (link[start + i] == ')')
          {
            param[args, 1] = start + i;
            break;
          }
        }
      }

      string[] array = null;
      if (args != -1 && param != null)
      {
        args++;
        array = new string[args];
        for (int i = 0; i < args; i++)
          array[i] = link.Substring(param[i, 0], param[i, 1] - param[i, 0]).Trim('\"', '\'');
      }

      return array;
    }
    #endregion
  }
}

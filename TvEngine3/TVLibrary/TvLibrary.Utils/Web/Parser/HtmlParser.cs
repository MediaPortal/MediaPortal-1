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
using System.Globalization;
using System.Text.RegularExpressions;

namespace MediaPortal.Utils.Web
{
  /// <summary>
  /// Parses HTML site from given Template
  /// </summary>
  public class HtmlParser
  {
    #region Variables

    private HtmlParserTemplate _template;
    private HtmlProfiler _profiler;
    private HtmlSectionParser _sectionParser;
    private string _sectionSource;
    private Type _dataType;
    private object[] _dataArgs;

    #endregion

    #region Constructors/Destructors

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlParser"/> class.
    /// </summary>
    /// <param name="template">The template.</param>
    /// <param name="parserDataType">Type of the parser data.</param>
    /// <param name="parserDataArgs">The parser data args.</param>
    public HtmlParser(HtmlParserTemplate template, Type parserDataType, params object[] parserDataArgs)
    {
      _template = template;
      _dataType = parserDataType;
      _dataArgs = parserDataArgs;
      _sectionSource = string.Empty;
      _profiler = new HtmlProfiler(_template.SectionTemplate);
      _sectionParser = new HtmlSectionParser(_template.SectionTemplate);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Parses the URL and returns the number of instances of the template found on this site
    /// </summary>
    /// <param name="site">The site.</param>
    /// <returns>count</returns>
    public int ParseUrl(HTTPRequest site)
    {
      HTMLPage webPage = new HTMLPage(site);
      string pageSource = webPage.GetPage();

      int startIndex = 0;
      if (_template.Start != null && _template.Start != string.Empty)
      {
        startIndex = pageSource.IndexOf(_template.Start, 0, StringComparison.OrdinalIgnoreCase);
        if (startIndex == -1)
        {
          startIndex = 0;
        }
        //log error ?
      }


      int endIndex = pageSource.Length;

      if (_template.End != null && _template.End != string.Empty)
      {
        endIndex = pageSource.IndexOf(_template.End, startIndex, StringComparison.OrdinalIgnoreCase);
        if (endIndex == -1)
        {
          endIndex = pageSource.Length;
        }
        //log error?
      }

      int count = 0;
      if (pageSource != null)
      {
        count = _profiler.MatchCount(pageSource.Substring(startIndex, endIndex - startIndex));
      }

      return count;
    }

    /// <summary>
    /// Gets the data.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>Parser Data</returns>
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

      // create a new IParserData object from the type and arguments given to the constructor
      IParserData sectionData = (IParserData)Activator.CreateInstance(_dataType, _dataArgs);
      if (_sectionParser.ParseSection(sectionSource, ref sectionData))
      {
        return sectionData;
      }
      return null;
    }

    /// <summary>
    /// Searches the regex.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="regex">The regex.</param>
    /// <param name="remove">if set to <c>true</c> [remove].</param>
    /// <returns>string found</returns>
    public string SearchRegex(int index, string regex, bool remove)
    {
      return SearchRegex(index, regex, false, remove);
    }

    /// <summary>
    /// Searches the regex.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="regex">The regex.</param>
    /// <param name="caseinsensitive">if set to <c>true</c> [caseinsensitive].</param>
    /// <param name="remove">if set to <c>true</c> [remove].</param>
    /// <returns>string found</returns>
    public string SearchRegex(int index, string regex, bool caseinsensitive, bool remove)
    {
      return SearchRegex(index, regex, caseinsensitive, remove, string.Empty);
    }

    /// <summary>
    /// Searches the regex.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="regex">The regex.</param>
    /// <param name="caseinsensitive">if set to <c>true</c> [caseinsensitive].</param>
    /// <param name="remove">if set to <c>true</c> [remove].</param>
    /// <param name="replace">the string to [replace] found string if set.</param>
    /// <returns>string found</returns>
    public string SearchRegex(int index, string regex, bool caseinsensitive, bool remove, string replace)
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
      Regex searchRegex = null;
      try
      {
        if (caseinsensitive)
        {
          searchRegex = new Regex(regex.ToLower(CultureInfo.CurrentCulture));
          result = searchRegex.Match(sectionSource.ToLower(CultureInfo.CurrentCulture));
        }
        else
        {
          searchRegex = new Regex(regex);
          result = searchRegex.Match(sectionSource);
        }
      }
      catch (ArgumentException) // ex)
      {
        //_log.Error("Html Parser: Regex error: {0} {1}", regex, ex.ToString());
        return string.Empty;
      }

      string found = null;
      if (result.Success)
      {
        found = sectionSource.Substring(result.Index, result.Length);
        if (!string.IsNullOrEmpty(replace))
        {
          _sectionSource = searchRegex.Replace(sectionSource, replace);
        }
        else if (remove)
        {
          _sectionSource = sectionSource.Substring(0, result.Index);

          int end = result.Index + result.Length;
          _sectionSource += sectionSource.Substring(end, sectionSource.Length - end);
        }
      }

      return found;
    }

    /// <summary>
    /// Gets the hyper link.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="match">The match.</param>
    /// <param name="linkURL">The link URL.</param>
    /// <returns>bool - success/fail</returns>
    public bool GetHyperLink(int index, string match, ref HTTPRequest linkURL)
    {
      string regex = "<(a |[^>]*onclick)[^>]*" + match + "[^>]*>"; //"<a .*? href=[^>]*" .ToLowerInvariant()

      string result = SearchRegex(index, regex, true, false);

      if (result == null)
      {
        return false;
      }

      bool linkFound = false;
      string strLinkURL = string.Empty;

      int start = -1;
      char delim = '>';

      if (result.ToLowerInvariant().IndexOf("href=") != -1)
      {
        start += result.ToLowerInvariant().IndexOf("href=") + 5;
      }
      if (result.ToLowerInvariant().IndexOf("onclick=") != -1)
      {
        start += result.ToLowerInvariant().IndexOf("onclick=") + 8;
      }
      if (result[start + 1] == '\"' || result[start + 1] == '\'')
      {
        start++;
        delim = result[start];
      }

      int end = -1;
      //if (delim != '>')
      //{
      //  start = -1;
      //  start = result.IndexOf(delim);
      //}
      if (start != -1)
      {
        end = result.IndexOf(delim, ++start);
      }

      if (end != -1)
      {
        strLinkURL = result.Substring(start, end - start);
        linkFound = true;
      }

      if ((start = strLinkURL.IndexOf("=")) != -1)
      {
        for (int i = 0; i < strLinkURL.Length - start; i++)
        {
          if (strLinkURL[start + i] == '\"' || strLinkURL[start + i] == '\'')
          {
            delim = strLinkURL[start + i];
            start = start + i;
            break;
          }
        }

        end = -1;

        if (start != -1)
        {
          end = strLinkURL.IndexOf(delim, ++start);
        }

        if (end != -1)
        {
          strLinkURL = strLinkURL.Substring(start, end - start);
        }
      }

      string[] param = GetJavaSubLinkParams(result); //strLinkURL);
      if (param != null)
      {
        if (!linkURL.HasTag("[1]"))
        {
          linkURL = linkURL.Add(HtmlString.ToAscii(param[0]));
        }
        else
        {
          for (int i = 0; i < param.Length; i++)
          {
            linkURL.ReplaceTag("[" + (i + 1).ToString() + "]", HtmlString.ToAscii(param[i]));
          }
        }
      }
      else
      {
        linkURL = linkURL.Add(HtmlString.ToAscii(strLinkURL.Trim()));
      }
      //}

      return linkFound;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Gets the java sub link params.
    /// </summary>
    /// <param name="link">The link.</param>
    /// <returns>params</returns>
    private string[] GetJavaSubLinkParams(string link)
    {
      int args = -1;
      int[,] param = null;
      int start = -1;

      if ((start = link.IndexOf("(")) != -1)
      {
        args = 0;
        param = new int[link.Length - start,2];
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
        {
          array[i] = link.Substring(param[i, 0], param[i, 1] - param[i, 0]).Trim('\"', '\'');
        }
      }

      return array;
    }

    #endregion
  }
}
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
using System.Collections.Generic;
using MediaPortal.Utils.Web;
using MediaPortal.WebEPG.Config.Grabber;

namespace MediaPortal.WebEPG.Parser
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class WebParser : IParser
  {
    #region Variables
    HtmlParser _listingParser;
    HtmlParser _sublinkParser;
    string _sublinkMatch;
    HTTPRequest _sublinkRequest;
    WebParserTemplate _template;
    #endregion

    #region Constructors/Destructors
    public WebParser(WebParserTemplate webTemplate)
    {
      _template = webTemplate;
      HtmlParserTemplate listingTemplate = _template.GetTemplate("default");
      Dictionary<string, int> monthsDict = null;
      if (_template.months != null)
      {
        monthsDict = new Dictionary<string, int>();
        for (int i = 0; i < _template.months.Length; i++)
        {
          monthsDict.Add(_template.months[i], i + 1); ;
        }
      }
      _listingParser = new HtmlParser(listingTemplate, typeof(ProgramData), monthsDict); //_template.months);
      _sublinkParser = null;

      if (_template.sublinks != null && _template.sublinks.Count > 0)
      {
        SublinkInfo sublink = _template.sublinks[0];
        HtmlParserTemplate sublinkTemplate = _template.GetTemplate(sublink.template);
        if (sublinkTemplate != null)
        {
          _sublinkParser = new HtmlParser(sublinkTemplate, typeof(ProgramData));
          _sublinkMatch = sublink.search;
          _sublinkRequest = sublink.url;
        }
      }
    }
    #endregion

    #region Public Methods
    public bool GetLinkedData(ref ProgramData data)
    {
      if (data.SublinkRequest != null)
      {
        int count = _sublinkParser.ParseUrl(data.SublinkRequest);

        if (count > 0)
        {
          // get first match
          ProgramData subdata = (ProgramData)_sublinkParser.GetData(0);

          data.Merge(subdata);

          return true;
        }
      }
      return false;
    }
    #endregion

    #region IParser Implementations
    public int ParseUrl(HTTPRequest site)
    {
      if (_sublinkParser != null && _sublinkRequest == null)
        _sublinkRequest = new HTTPRequest(site);
      return _listingParser.ParseUrl(site);
    }

    public IParserData GetData(int index)
    {
      if (_template.searchList != null)
      {
        for (int i = 0; i < _template.searchList.Count; i++)
        {
          WebSearchData search = _template.searchList[i];
          string result = _listingParser.SearchRegex(index, search.Match, search.Remove);
        }
      }

      ProgramData data = (ProgramData)_listingParser.GetData(index);
      if (_sublinkParser != null)
      {
        HTTPRequest sublinkRequest = new HTTPRequest(_sublinkRequest);
        if (sublinkRequest.Delay < 500)
          sublinkRequest.Delay = 500;
        if (_listingParser.GetHyperLink(index, _sublinkMatch, ref sublinkRequest))
          data.SublinkRequest = sublinkRequest;
      }
      return data;
    }
    #endregion
  }
}

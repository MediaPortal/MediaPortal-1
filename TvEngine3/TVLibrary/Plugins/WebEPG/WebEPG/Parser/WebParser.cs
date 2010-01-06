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

using System.Collections.Generic;
using MediaPortal.Utils.Web;
using MediaPortal.WebEPG.Config.Grabber;

namespace MediaPortal.WebEPG.Parser
{
  /// <summary>
  /// Parser for EPG data on the web
  /// </summary>
  public class WebParser : IParser
  {
    #region Variables

    private HtmlParser _listingParser;
    private HtmlParser _sublinkParser;
    private string _sublinkMatch;
    private HTTPRequest _sublinkRequest;
    private WebParserTemplate _template;
    private DataPreference _listingPreference;
    private DataPreference _sublinkPreference;

    #endregion

    #region Constructors/Destructors

    /// <summary>
    /// Initializes a new instance of the <see cref="WebParser"/> class.
    /// </summary>
    /// <param name="webTemplate">The web template.</param>
    public WebParser(WebParserTemplate webTemplate)
    {
      // Store the template
      _template = webTemplate;

      // Get default template -> currently only a default is supported
      // In the future template per channel ID can be added.
      HtmlParserTemplate listingTemplate = _template.GetTemplate("default");
      _listingPreference = _template.GetPreference("default");

      // Create dictionary for month strings
      Dictionary<string, int> monthsDict = null;
      if (_template.months != null)
      {
        // template contains months list -> load into dictionary
        monthsDict = new Dictionary<string, int>();
        for (int i = 0; i < _template.months.Length; i++)
        {
          monthsDict.Add(_template.months[i], i + 1);
          ;
        }
      }


      // create new Html Parser using default template
      _listingParser = new HtmlParser(listingTemplate, typeof (ProgramData), monthsDict);

      // setup sublink parser if template and config exists
      _sublinkParser = null;
      if (_template.sublinks != null && _template.sublinks.Count > 0)
      {
        // Load first sublink template -> only one supported now
        // future support for multiple sublinks possible
        SublinkInfo sublink = _template.sublinks[0];
        HtmlParserTemplate sublinkTemplate = _template.GetTemplate(sublink.template);
        _sublinkPreference = _template.GetPreference(sublinkTemplate.Name);

        if (sublinkTemplate != null)
        {
          // create sublink parser using template
          _sublinkParser = new HtmlParser(sublinkTemplate, typeof (ProgramData));
          _sublinkMatch = sublink.search;
          _sublinkRequest = sublink.url;
        }
      }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Gets the data from a linked page
    /// </summary>
    /// <param name="data">ref to ProgramData</param>
    /// <returns>true if data was correctly parsed otherwise false</returns>
    public bool GetLinkedData(ref ProgramData data)
    {
      // check if the Data has a sublink request set
      if (data.SublinkRequest != null)
      {
        // find template matches
        int count = _sublinkParser.ParseUrl(data.SublinkRequest);

        if (count > 0)
        {
          // get first match -> only the first match is supported for sublink templates
          ProgramData subdata = (ProgramData)_sublinkParser.GetData(0);
          if (subdata != null)
          {
            subdata.Preference = _sublinkPreference;

            data.Merge(subdata);

            return true;
          }
        }
      }
      return false;
    }

    #endregion

    #region IParser Implementations

    /// <summary>
    /// Parses a site for a given URL.
    /// </summary>
    /// <param name="site">The request for the site to be parsed</param>
    /// <returns>number of instances of the tempate found</returns>
    public int ParseUrl(HTTPRequest site)
    {
      if (_sublinkParser != null && _sublinkRequest == null)
      {
        _sublinkRequest = new HTTPRequest(site);
      }
      return _listingParser.ParseUrl(site);
    }

    /// <summary>
    /// Gets the data.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns></returns>
    public IParserData GetData(int index)
    {
      // Perform any search and remove requests
      // Searches can search over the whole data 
      // optionally removing text so it will not be parsed with the main data
      ProgramData searchData = null;
      if (_template.searchList != null && _template.searchList.Count > 0)
      {
        searchData = new ProgramData();
        for (int i = 0; i < _template.searchList.Count; i++)
        {
          WebSearchData search = _template.searchList[i];
          string result = _listingParser.SearchRegex(index, search.Match, search.Remove);
          if (result != null)
          {
            searchData.SetElement(search.Field, result);
          }
        }
      }

      // Get the parsed data at index
      ProgramData data = ((ProgramData)_listingParser.GetData(index));
      if (data != null)
      {
        // Set the data preference -> important for merging data (eg data from sublink page)
        data.Preference = new DataPreference(_listingPreference);

        // If search data exists merge.
        if (searchData != null)
        {
          data.Merge(searchData);
        }

        // If there is a sublink parser, check for a matching sublink
        // sublink is not parsed here, because that may not be required 
        // the URL for the sublink will be built and stored for future use see GetLinkedData()
        if (_sublinkParser != null)
        {
          HTTPRequest sublinkRequest = new HTTPRequest(_sublinkRequest);
          if (sublinkRequest.Delay < 500)
          {
            sublinkRequest.Delay = 500;
          }
          if (_listingParser.GetHyperLink(index, _sublinkMatch, ref sublinkRequest))
          {
            data.SublinkRequest = sublinkRequest;
          }
        }
      }
      return data;
    }

    #endregion
  }
}
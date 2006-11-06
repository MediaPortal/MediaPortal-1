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
using System.Collections;
using System.Collections.Generic;
using MediaPortal.WebEPG;
using MediaPortal.WebEPG.Parser;
using MediaPortal.WebEPG.Config.Grabber;
using MediaPortal.Utils.Web;

namespace MediaPortal.EPG.config
{
  /// <summary>
  /// GrabberConfig
  /// </summary>
  public class GrabberConfig
  {
    //#region Enums
    //public enum Type
    //{
    //  XML,
    //  DATA,
    //  HTML
    //};
    //#endregion

    //#region Variables
    //MediaPortal.Webepg.Profile.Xml _xmlreader;
    //HTTPRequest _listingRequest;
    //RequestData _reqData;
    //string _baseUrl;
    //string _timeZone;
    //Type _grabberType;
    //int _maxListingCount;
    //int _pageStart;
    //int _pageEnd;
    //int _grabDelay;
    //int _guideDays;
    //int _siteGuideDays;
    //bool _monthLookup;
    //#endregion

    //#region Constructors/Destructors
    ///// <summary>
    ///// Initializes a new instance of the <see cref="GrabberConfig"/> class.
    ///// </summary>
    ///// <param name="file">The filename</param>
    //public GrabberConfig(string file)
    //{
    //  _xmlreader = new MediaPortal.Webepg.Profile.Xml(file);
    //  _baseUrl = _xmlreader.GetValueAsString("Listing", "BaseURL", string.Empty);
    //  if (_baseUrl == string.Empty)
    //  {
    //    throw new ArgumentException("No BaseURL defined in");
    //  }

    //  string getQuery = _xmlreader.GetValueAsString("Listing", "SearchURL", string.Empty);
    //  string postQuery = _xmlreader.GetValueAsString("Listing", "PostQuery", string.Empty);
    //  string encoding = _xmlreader.GetValueAsString("Listing", "Encoding", string.Empty);
    //  _listingRequest = new HTTPRequest(_baseUrl, getQuery, postQuery, encoding);
    //  _listingRequest.External = _xmlreader.GetValueAsBool("Listing", "External", false);

    //  string listingType = _xmlreader.GetValueAsString("Listing", "ListingType", "HTML");

    //  switch (listingType)
    //  {
    //    case "XML":
    //      _grabberType = Type.XML;
    //      break;
    //    case "DATA":
    //      _grabberType = Type.DATA;
    //      break;
    //    case "HTML":
    //      _grabberType = Type.HTML;
    //      break;
    //    default:
    //      throw new ArgumentException("Unknown Grabber Type");
    //  }

    //  _grabDelay = _xmlreader.GetValueAsInt("Listing", "GrabDelay", 500);
    //  _maxListingCount = _xmlreader.GetValueAsInt("Listing", "MaxCount", 0);
    //  _pageStart = _xmlreader.GetValueAsInt("Listing", "PageStart", 0);
    //  _pageEnd = _xmlreader.GetValueAsInt("Listing", "PageEnd", 0);
    //  _guideDays = _xmlreader.GetValueAsInt("Info", "GuideDays", 0);

    //  _timeZone = _xmlreader.GetValueAsString("Info", "TimeZone", string.Empty);
    //  _monthLookup = _xmlreader.GetValueAsBool("DateTime", "Months", false);


    //  _reqData = new RequestData();
    //  _reqData.OffsetStart = _xmlreader.GetValueAsInt("Listing", "OffsetStart", 0);
    //  _reqData.MaxListingCount = _xmlreader.GetValueAsInt("Listing", "MaxCount", 0);
    //  string firstDay = _xmlreader.GetValueAsString("DayNames", "0", string.Empty);
    //  if (firstDay != string.Empty && _guideDays != 0)
    //  {
    //    _reqData.DayNames = new string[_guideDays];
    //    _reqData.DayNames[0] = firstDay;
    //    for (int i = 1; i < _guideDays; i++)
    //      _reqData.DayNames[i] = _xmlreader.GetValueAsString("DayNames", i.ToString(), string.Empty);
    //  }

    //  _reqData.SearchLang = _xmlreader.GetValueAsString("Listing", "SearchLanguage", "en-US");
    //  _reqData.WeekDay = _xmlreader.GetValueAsString("Listing", "WeekdayString", "dddd");
    //}
    //#endregion

    //#region Properties
    //public HTTPRequest ListingRequest
    //{
    //  get { return _listingRequest; }
    //}

    //public RequestData ReqData
    //{
    //  get { return _reqData; }
    //}

    //public string BaseUrl
    //{
    //  get { return _baseUrl; }
    //}

    //public string TimeZone
    //{
    //  get { return _timeZone; }
    //}

    //public Type GrabberType
    //{
    //  get { return _grabberType; }
    //}

    //public int MaxListingCount
    //{
    //  get { return _maxListingCount; }
    //}

    //public int PageStart
    //{
    //  get { return _pageStart; }
    //}

    //public int PageEnd
    //{
    //  get { return _pageEnd; }
    //}

    //public int GrabDelay
    //{
    //  get { return _grabDelay; }
    //}

    //public int GuideDays
    //{
    //  get { return _guideDays; }
    //}

    //public int SiteGuideDays
    //{
    //  get { return _siteGuideDays; }
    //}

    ////public string ListingDelimitor
    ////{
    ////  get { return _strListingDelimitor; }
    ////}

    ////public string DataDelimitor
    ////{
    ////  get { return _strDataDelimitor; }
    ////}

    ////public string ListingTemplate
    ////{
    ////  get { return _listingTemplate; }
    ////}

    //public bool MonthLookup
    //{
    //  get { return _monthLookup; }
    //}
    //#endregion

    //#region Public Methods
    //public IParser XmlGrabber()
    //{
    //  XmlParserTemplate data = new XmlParserTemplate();

    //  data.XPath = _xmlreader.GetValueAsString("Listing", "XPath", string.Empty);
    //  data.Channel = _xmlreader.GetValueAsString("Listing", "ChannelEntry", string.Empty);
    //  data.Fields = new XmlFieldCollection();

    //  XmlField element = new XmlField();
    //  element.FieldName = "#STARTXMLTV";
    //  element.XmlName = _xmlreader.GetValueAsString("Listing", "StartEntry", string.Empty);
    //  if (element.XmlName != string.Empty)
    //    data.Fields.Add(element);

    //  element = new XmlField();
    //  element.FieldName = "#ENDXMLTV";
    //  element.XmlName = _xmlreader.GetValueAsString("Listing", "EndEntry", string.Empty);
    //  if (element.XmlName != string.Empty)
    //    data.Fields.Add(element);

    //  element = new XmlField();
    //  element.FieldName = "#TITLE";
    //  element.XmlName = _xmlreader.GetValueAsString("Listing", "TitleEntry", string.Empty);
    //  if (element.XmlName != string.Empty)
    //    data.Fields.Add(element);

    //  element = new XmlField();
    //  element.FieldName = "#SUBTITLE";
    //  element.XmlName = _xmlreader.GetValueAsString("Listing", "SubtitleEntry", string.Empty);
    //  if (element.XmlName != string.Empty)
    //    data.Fields.Add(element);

    //  element = new XmlField();
    //  element.FieldName = "#DESCRIPTION";
    //  element.XmlName = _xmlreader.GetValueAsString("Listing", "DescEntry", string.Empty);
    //  if (element.XmlName != string.Empty)
    //    data.Fields.Add(element);

    //  element = new XmlField();
    //  element.FieldName = "#GENRE";
    //  element.XmlName = _xmlreader.GetValueAsString("Listing", "GenreEntry", string.Empty);
    //  if (element.XmlName != string.Empty)
    //    data.Fields.Add(element);

    //  XmlParser parser = new XmlParser(data);

    //  return parser;
    //}

    //public IParser DataGrabber()
    //{
    //  DataParser dataParser = null;
    //  string rowDelimitor = _xmlreader.GetValueAsString("Listing", "ListingDelimitor", "\n");
    //  string dataDelimitor = _xmlreader.GetValueAsString("Listing", "DataDelimitor", "\t");
    //  string listingTemplate = _xmlreader.GetValueAsString("Listing", "Template", string.Empty);
    //  if (listingTemplate != string.Empty)
    //    dataParser = new DataParser(listingTemplate, dataDelimitor, rowDelimitor);

    //  return dataParser;
    //}

    //public IParser HtmlGrabber()
    //{
    //  string listingTemplate = _xmlreader.GetValueAsString("Listing", "Template", string.Empty);
    //  if (listingTemplate == string.Empty)
    //    return null;

    //  _siteGuideDays = _xmlreader.GetValueAsInt("Info", "GuideDays", 0);

    //  string strGuideStart = _xmlreader.GetValueAsString("Listing", "Start", "<body");
    //  string strGuideEnd = _xmlreader.GetValueAsString("Listing", "End", "</body");
    //  string tags = _xmlreader.GetValueAsString("Listing", "Tags", "T");

    //  //_templateProfile = new HTMLProfiler(listingTemplate, bAhrefs, strGuideStart, strGuideEnd);
    //  HtmlParserTemplate template = new HtmlParserTemplate();
    //  template.Start = strGuideStart;
    //  template.End = strGuideEnd;
    //  template.SectionTemplate = new HtmlSectionTemplate();
    //  template.SectionTemplate.Template = listingTemplate;
    //  template.SectionTemplate.Template = tags;

    //  WebParserTemplate optional = new WebParserTemplate();
    //  if (_xmlreader.GetValueAsBool("DateTime", "Months", false))
    //  {
    //    optional.months = new Dictionary<string, int>();

    //    for (int i = 1; i <= 12; i++)
    //    {
    //      optional.months.Add(_xmlreader.GetValueAsString("DateTime", i.ToString(), string.Empty), i);
    //    }
    //  }


    //  if (_xmlreader.GetValueAsBool("Listing", "SearchRegex", false))
    //  {
    //    optional.searchList = new List<WebSearchData>();
    //    bool searchRemove = _xmlreader.GetValueAsBool("Listing", "SearchRemove", false);

    //    WebSearchData searchItem;
    //    searchItem = GetSearchData("#REPEAT", searchRemove, _xmlreader.GetValueAsString("Listing", "SearchRepeat", string.Empty));
    //    if (searchItem != null)
    //      optional.searchList.Add(searchItem);

    //    searchItem = GetSearchData("#SUBTITLES", searchRemove, _xmlreader.GetValueAsString("Listing", "SearchSubtitles", string.Empty));
    //    if (searchItem != null)
    //      optional.searchList.Add(searchItem);

    //    searchItem = GetSearchData("#EPNUM", searchRemove, _xmlreader.GetValueAsString("Listing", "SearchEpNum", string.Empty));
    //    if (searchItem != null)
    //      optional.searchList.Add(searchItem);

    //    searchItem = GetSearchData("#EPTOTAL", searchRemove, _xmlreader.GetValueAsString("Listing", "SearchEpTotal", string.Empty));
    //    if (searchItem != null)
    //      optional.searchList.Add(searchItem);
    //  }

    //  string subListingLink = _xmlreader.GetValueAsString("Listing", "SubListingLink", string.Empty);
    //  if (subListingLink != string.Empty)
    //  {
    //    string strSubStart = _xmlreader.GetValueAsString("SubListing", "Start", "<body");
    //    string strSubEnd = _xmlreader.GetValueAsString("SubListing", "End", "</body");
    //    string subencoding = _xmlreader.GetValueAsString("SubListing", "Encoding", string.Empty);
    //    string subUrl = _xmlreader.GetValueAsString("SubListing", "URL", string.Empty);
    //    HTTPRequest sublinkRequest;
    //    if (subUrl != string.Empty)
    //    {
    //      sublinkRequest = new HTTPRequest(subUrl);
    //      sublinkRequest.PostQuery = _xmlreader.GetValueAsString("SubListing", "PostQuery", string.Empty);
    //      sublinkRequest.Encoding = subencoding;
    //    }
    //    else
    //    {
    //      sublinkRequest = new HTTPRequest(_listingRequest);
    //    }
    //    string subTags = _xmlreader.GetValueAsString("SubListing", "Tags", "T");
    //    string sublistingTemplate = _xmlreader.GetValueAsString("SubListing", "Template", string.Empty);
    //    if (sublistingTemplate != string.Empty)
    //    {
    //      HtmlParserTemplate subTemplate = new HtmlParserTemplate();
    //      subTemplate.Start = strSubStart;
    //      subTemplate.End = strSubEnd;
    //      subTemplate.SectionTemplate = new HtmlSectionTemplate();
    //      subTemplate.SectionTemplate.Template = sublistingTemplate;
    //      subTemplate.SectionTemplate.Template = subTags;
    //      return new WebParser(template, subTemplate, subListingLink, sublinkRequest, optional);
    //    }
    //  }
    //  return new WebParser(template, optional);
    //}

    ///// <summary>
    ///// Gets the genre.
    ///// </summary>
    ///// <param name="genre">The genre.</param>
    ///// <returns></returns>
    //public string GetGenre(string genre)
    //{
    //  return _xmlreader.GetValueAsString("GenreMap", genre, genre);
    //}

    //public string GetChannel(string strChannelId)
    //{
    //  return _xmlreader.GetValueAsString("ChannelList", strChannelId, string.Empty);
    //}

    //public string GetRemoveProgramList(string strChannelId)
    //{
    //  string removeProgramsList = _xmlreader.GetValueAsString("RemovePrograms", "*", string.Empty);
    //  if (removeProgramsList != string.Empty)
    //    removeProgramsList += ";";
    //  string chanRemovePrograms = _xmlreader.GetValueAsString("RemovePrograms", strChannelId, string.Empty);
    //  if (chanRemovePrograms != string.Empty)
    //  {
    //    removeProgramsList += chanRemovePrograms;
    //    removeProgramsList += ";";
    //  }

    //  return removeProgramsList;
    //}
    //#endregion

    //#region Private Methods
    //private WebSearchData GetSearchData(string fieldname, bool remove, string match)
    //{
    //  WebSearchData search = null;

    //  if (match != string.Empty)
    //  {
    //    search = new WebSearchData();
    //    search.Field = fieldname;
    //    search.Remove = remove;
    //    search.Match = match;
    //  }

    //  return search;
    //}
    //#endregion
  }
}

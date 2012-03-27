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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using CSScriptLibrary;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Util;

namespace MediaPortal.Video.Database
{
  /// <summary>
  /// supporting classes to fetch movie information out of different databases
  /// currently supported: IMDB http://us.imdb.com and additional database by using external csscripts
  /// </summary>
  public class IMDB : IEnumerable
  {
    public static string ScriptDirectory = Config.GetSubFolder(Config.Dir.Config, "scripts\\MovieInfo");
    public const string DEFAULT_DATABASE = "IMDB";
    public const int DEFAULT_SEARCH_LIMIT = 25;

    #region interfaces and classes

    public interface IProgress
    {
      void OnProgress(string line1, string line2, string line3, int percent);
      bool OnDisableCancel(IMDBFetcher fetcher);
      bool OnSearchStarting(IMDBFetcher fetcher);
      bool OnSearchStarted(IMDBFetcher fetcher);
      bool OnSearchEnd(IMDBFetcher fetcher);
      bool OnMovieNotFound(IMDBFetcher fetcher);
      bool OnDetailsStarting(IMDBFetcher fetcher);
      bool OnDetailsStarted(IMDBFetcher fetcher);
      bool OnDetailsEnd(IMDBFetcher fetcher);
      bool OnDetailsNotFound(IMDBFetcher fetcher);
      bool OnActorsStarting(IMDBFetcher fetcher);
      bool OnActorInfoStarting(IMDBFetcher fetcher);
      bool OnActorsStarted(IMDBFetcher fetcher);
      bool OnActorsEnd(IMDBFetcher fetcher);
      bool OnRequestMovieTitle(IMDBFetcher fetcher, out string movieName);
      bool OnSelectMovie(IMDBFetcher fetcher, out int selected);
      bool OnSelectActor(IMDBFetcher fetcher, out int selected);
      bool OnScanStart(int total);
      bool OnScanEnd();
      bool OnScanIterating(int count);
      bool OnScanIterated(int count);
    }

    /// <summary>
    /// class that represents URL and Title of a search result
    /// </summary>
    public class IMDBUrl
    {
      private string m_strURL = "";
      private string m_strTitle = "";
      private string m_strDatabase = "";
      private string m_strIMDBURL = "";

      public IMDBUrl(string strURL, string strTitle, string strDB)
      {
        URL = strURL;
        Title = strTitle;
        Database = strDB;
      }

      public string URL
      {
        get { return m_strURL; }
        set { m_strURL = value; }
      }

      public string Title
      {
        get { return m_strTitle; }
        set { m_strTitle = value; }
      }

      public string Database
      {
        get { return m_strDatabase; }
        set { m_strDatabase = value; }
      }

      public string IMDBURL
      {
        get { return m_strIMDBURL; }
        set { m_strIMDBURL = value; }
      }
    }

    public class IMDBEnumerator : IEnumerator
    {
      private int _position = -1;
      private IMDB _t;

      public IMDBEnumerator(IMDB t)
      {
        _t = t;
      }

      public bool MoveNext()
      {
        if (_position < _t._elements.Count - 1)
        {
          _position++;
          return true;
        }
        return false;
      }

      public void Reset()
      {
        _position = -1;
      }

      public IMDBUrl Current // non-IEnumerator version: type-safe
      {
        get
        {
          if (_t._elements.Count == 0)
          {
            return null;
          }
          return (IMDBUrl)_t._elements[_position];
        }
      }

      object IEnumerator.Current // IEnumerator version: returns object
      {
        get
        {
          if (_t._elements.Count == 0)
          {
            return null;
          }
          return _t._elements[_position];
        }
      }
    }

    public class MovieInfoDatabase
    {
      private string _id;
      private int _limit = DEFAULT_SEARCH_LIMIT;
      private IIMDBScriptGrabber _grabber;
      private bool _grabberLoaded;

      public string ID
      {
        get { return _id; }
        set { _id = value; }
      }

      public int Limit
      {
        get { return _limit; }
        set { _limit = value; }
      }

      public IIMDBScriptGrabber Grabber
      {
        get
        {
          if (!_grabberLoaded)
          {
            if (!LoadScript())
              Grabber = null;
            _grabberLoaded = true; // only try to load it once
          }
          return _grabber;
        }
        set { _grabber = value; }
      }

      public MovieInfoDatabase(string id, int limit)
      {
        ID = id;
        Limit = limit;
      }

      public bool LoadScript()
      {
        string scriptFileName = ScriptDirectory + @"\" + ID + ".csscript";

        // Script support script.csscript
        if (!File.Exists(scriptFileName))
        {
          Log.Error("InfoGrabber LoadScript() - grabber script not found: {0}", scriptFileName);
          return false;
        }

        try
        {
          Environment.CurrentDirectory = Config.GetFolder(Config.Dir.Base);
          AsmHelper script = new AsmHelper(CSScript.Load(scriptFileName, null, false));
          Grabber = (IIMDBScriptGrabber)script.CreateObject("Grabber");
        }
        catch (Exception ex)
        {
          Log.Error("InfoGrabber LoadScript() - file: {0}, message : {1}", scriptFileName, ex.Message);
          return false;
        }

        return true;
      }
    }

    #endregion

    #region internal vars

    // list of the search results, containts objects of IMDBUrl
    private ArrayList _elements = new ArrayList();

    private List<MovieInfoDatabase> _databaseList = new List<MovieInfoDatabase>();

    private IProgress m_progress;

    #endregion

    #region ctor

    public IMDB()
      : this(null) {}

    public IMDB(IProgress progress)
    {
      m_progress = progress;
      // load the settings
      LoadSettings();
    }

    #endregion

    /// <summary>
    /// load settings from mediaportal.xml
    /// </summary>
    private void LoadSettings()
    {
      // getting available databases and limits
      using (Settings xmlreader = new MPSettings())
      {
        int iNumber = xmlreader.GetValueAsInt("moviedatabase", "number", 0);

        _databaseList.Clear();
        if (iNumber <= 0)
        {
          // no given databases in XML - setting to IMDB
          _databaseList.Add(new MovieInfoDatabase(DEFAULT_DATABASE, DEFAULT_SEARCH_LIMIT));
        }
        else
        {
          string strDatabase;
          int iLimit;

          // get the databases
          for (int i = 0; i < iNumber; i++)
          {
            strDatabase = xmlreader.GetValueAsString("moviedatabase", "database" + i, "IMDB");
            iLimit = xmlreader.GetValueAsInt("moviedatabase", "limit" + i, DEFAULT_SEARCH_LIMIT);

            foreach (MovieInfoDatabase db in _databaseList)
            {
              if (db.ID == strDatabase)
              {
                goto DoubleEntry;
              }
            }

            _databaseList.Add(new MovieInfoDatabase(strDatabase, iLimit));

            DoubleEntry:
            continue;
          }
        }
      }
    }

    /// <summary>
    /// count the elements
    /// </summary>
    public int Count
    {
      get { return _elements.Count; }
    }

    public IMDBUrl this[int index]
    {
      get { return (IMDBUrl)_elements[index]; }
    }

    public IMDBEnumerator GetEnumerator() // non-IEnumerable version
    {
      return new IMDBEnumerator(this);
    }

    #region IEnumerable Member

    IEnumerator IEnumerable.GetEnumerator()
    {
      return new IMDBEnumerator(this);
    }

    #endregion

    #region helper methods to get infos

    /// <summary>
    /// trys to get a webpage from the specified url and returns the content as string
    /// </summary>
    private string GetPage(string strURL, string strEncode, out string absoluteUri)
    {
      string strBody = "";
      absoluteUri = string.Empty;
      Stream receiveStream = null;
      StreamReader sr = null;
      WebResponse result = null;
      try
      {
        // Make the Webrequest
        //Log.Info("IMDB: get page:{0}", strURL);
        WebRequest req = WebRequest.Create(strURL);
        req.Headers.Add("Accept-Language", "en-US");
        try
        {
          // Use the current user in case an NTLM Proxy or similar is used.
          // wr.Proxy = WebProxy.GetDefaultProxy();
          req.Proxy.Credentials = CredentialCache.DefaultCredentials;
        }
        catch (Exception) {}
        result = req.GetResponse();
        receiveStream = result.GetResponseStream();

        // Encoding: depends on selected page
        Encoding encode = Encoding.GetEncoding(strEncode);
        using (sr = new StreamReader(receiveStream, encode))
        {
          strBody = sr.ReadToEnd();
        }


        absoluteUri = result.ResponseUri.AbsoluteUri;
      }
      catch (Exception)
      {
        //Log.Error("Error retreiving WebPage: {0} Encoding:{1} err:{2} stack:{3}", strURL, strEncode, ex.Message, ex.StackTrace);
      }
      finally
      {
        if (sr != null)
        {
          try
          {
            sr.Close();
          }
          catch (Exception) {}
        }
        if (receiveStream != null)
        {
          try
          {
            receiveStream.Close();
          }
          catch (Exception) {}
        }
        if (result != null)
        {
          try
          {
            result.Close();
          }
          catch (Exception) {}
        }
      }
      return strBody;
    }
    
    /// <summary>
    /// make a searchstring out of the filename
    /// </summary>
    public string GetSearchString(string strMovie)
    {
      string strUrl = strMovie;
      strUrl = strUrl.Trim();
      Regex rx = new Regex(@"(([\(\{\[]|\b)((576|720|1080)[pi]|dir(ectors )?cut|dvd([r59]|rip|scr(eener)?)|(avc)?hd|wmv|ntsc|pal|mpeg|dsr|r[1-5]|bd[59]|dts|ac3|blu(-)?ray|[hp]dtv|stv|hddvd|xvid|divx|x264|dxva|(?-i)FEST[Ii]VAL|L[iI]M[iI]TED|[WF]S|PROPER|REPACK|RER[Ii]P|REAL|RETA[Ii]L|EXTENDED|REMASTERED|UNRATED|CHRONO|THEATR[Ii]CAL|DC|SE|UNCUT|[Ii]NTERNAL|V\d{1}|BR[Rr]ip|[DS]UBBED)([\]\)\}]|\b)(-[^\s]+$)?)", RegexOptions.IgnoreCase);
      strUrl = rx.Replace(strUrl, "")
                 .Replace(".", " ")
                 .Replace("_", " ").Trim();
      return strUrl;
    }

    #endregion

    #region methods to get movie infos from different databases

    /// <summary>
    /// this method switches between the different databases to get the search results
    /// </summary>
    /// 
    // Changed
    public void Find(string strMovie)
    {
      try
      {
        // getting searchstring
        string strSearch = HttpUtility.UrlEncode(GetSearchString(strMovie));

        // be aware of german special chars äöüß Ă¤Ă¶ĂĽĂź %E4%F6%FC%DF %c3%a4%c3%b6%c3%bc%c3%9f
        strSearch = strSearch.Replace("%c3%a4", "%E4");
        strSearch = strSearch.Replace("%c3%b6", "%F6");
        strSearch = strSearch.Replace("%c3%bc", "%FC");
        strSearch = strSearch.Replace("%c3%9f", "%DF");
        // be aware of spanish special chars ńáéíóúÁÉÍÓÚ %E4%F6%FC%DF %c3%a4%c3%b6%c3%bc%c3%9f
        strSearch = strSearch.Replace("%c3%b1", "%F1");
        strSearch = strSearch.Replace("%c3%a0", "%E0");
        strSearch = strSearch.Replace("%c3%a1", "%E1");
        strSearch = strSearch.Replace("%c3%a8", "%E8");
        strSearch = strSearch.Replace("%c3%a9", "%E9");
        strSearch = strSearch.Replace("%c3%ac", "%EC");
        strSearch = strSearch.Replace("%c3%ad", "%ED");
        strSearch = strSearch.Replace("%c3%b2", "%F2");
        strSearch = strSearch.Replace("%c3%b3", "%F3");
        strSearch = strSearch.Replace("%c3%b9", "%F9");
        strSearch = strSearch.Replace("%c3%ba", "%FA");
        // Extra Codes
        strSearch = strSearch.Replace("%c3%b8", "%F8"); //ø
        strSearch = strSearch.Replace("%c3%98", "%D8"); //ø
        strSearch = strSearch.Replace("%c3%86", "%C6"); //Æ
        strSearch = strSearch.Replace("%c3%a6", "%E6"); //æ
        strSearch = strSearch.Replace("%c2%bd", "%BD"); //½
        // CRO
        strSearch = strSearch.Replace("%c4%86", "%0106"); //Č
        strSearch = strSearch.Replace("%c4%87", "%0107"); //č
        strSearch = strSearch.Replace("%c4%8c", "%010C"); //Ć
        strSearch = strSearch.Replace("%c4%8d", "%010D"); //ć
        strSearch = strSearch.Replace("%c4%90", "%0110"); //Đ
        strSearch = strSearch.Replace("%c4%91", "%0111"); //đ
        strSearch = strSearch.Replace("%c5%a0", "%0160"); //Š
        strSearch = strSearch.Replace("%c5%a1", "%0161"); //š
        strSearch = strSearch.Replace("%c5%bc", "%017c"); //Ž
        strSearch = strSearch.Replace("%c5%bd", "%017d"); //ž

        _elements.Clear();

        string line1 = GUILocalizeStrings.Get(984);
        string line2 = GetSearchString(strMovie).Replace("+", " ");
        string line3 = "";
        int percent = 0;

        if (m_progress != null)
        {
          m_progress.OnProgress(line1, line2, line3, percent);
        }
        // search the desired databases
        foreach (MovieInfoDatabase db in _databaseList)
        {
          // only do a search if requested
          if (db.Limit <= 0)
          {
            continue;
          }
          if (db.Grabber == null)
          {
            continue;
          }

          // load the script file as an assembly
          // if something went wrong it returns false

          line1 = GUILocalizeStrings.Get(984) + ": Script " + db.ID;

          if (m_progress != null)
          {
            m_progress.OnProgress(line1, line2, line3, percent);
          }

          try
          {
            db.Grabber.FindFilm(strSearch, db.Limit, _elements);
            percent += 100 / _databaseList.Count;
            if (m_progress != null)
            {
              m_progress.OnProgress(line1, line2, line3, percent);
            }
          }
          catch (Exception ex)
          {
            Log.Error("Movie database lookup Find() - grabber: {0}, message : {1}", db.ID, ex.Message);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Info("Movie database lookup Find() - Exception: {0}", ex.Message);
      }
    }

    /// <summary>
    /// this method switches between the different databases to fetche the search result into movieDetails
    /// </summary>
    public bool GetDetails(IMDBUrl url, ref IMDBMovie movieDetails)
    {
      try
      {
        MovieInfoDatabase currentDB = null;
        
        foreach (MovieInfoDatabase db in _databaseList)
        {
          if (db.ID == url.Database)
          {
            currentDB = db;
          }
        }
        
        if (currentDB == null)
        {
          return false;
        }
        
        if (currentDB.Grabber == null)
        {
          return false;
        }


        currentDB.Grabber.GetDetails(url, ref movieDetails);
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("Movie database lookup GetDetails() - grabber: {0}, message : {1}", url.Database, ex.Message);
        return false;
      }
    }

    #endregion

    #region methods to get actor infos

    public void FindActor(string strActor)
    {
      // getting searchstring
      string strSearch = HttpUtility.UrlEncode(GetSearchString(strActor));

      // be aware of german special chars äöüß Ă¤Ă¶ĂĽĂź %E4%F6%FC%DF %c3%a4%c3%b6%c3%bc%c3%9f
      strSearch = strSearch.Replace("%c3%a4", "%E4");
      strSearch = strSearch.Replace("%c3%b6", "%F6");
      strSearch = strSearch.Replace("%c3%bc", "%FC");
      strSearch = strSearch.Replace("%c3%9f", "%DF");
      // be aware of spanish special chars ńáéíóúÁÉÍÓÚ %E4%F6%FC%DF %c3%a4%c3%b6%c3%bc%c3%9f
      strSearch = strSearch.Replace("%c3%b1", "%F1");
      strSearch = strSearch.Replace("%c3%a0", "%E0");
      strSearch = strSearch.Replace("%c3%a1", "%E1");
      strSearch = strSearch.Replace("%c3%a8", "%E8");
      strSearch = strSearch.Replace("%c3%a9", "%E9");
      strSearch = strSearch.Replace("%c3%ac", "%EC");
      strSearch = strSearch.Replace("%c3%ad", "%ED");
      strSearch = strSearch.Replace("%c3%b2", "%F2");
      strSearch = strSearch.Replace("%c3%b3", "%F3");
      strSearch = strSearch.Replace("%c3%b9", "%F9");
      strSearch = strSearch.Replace("%c3%ba", "%FA");
      strSearch = strSearch.Replace("%c3%b8", "%F8"); //ø
      strSearch = strSearch.Replace("%c3%98", "%D8"); //ø
      strSearch = strSearch.Replace("%c3%86", "%C6"); //Æ
      strSearch = strSearch.Replace("%c3%a6", "%E6"); //æ

      _elements.Clear();

      string strURL = String.Format("http://www.imdb.com/find?s=nm&q=" + strSearch, strSearch);
      FindIMDBActor(strURL);
    }

    public void SetIMDBActor(string strURL, string strName)
    {
      IMDBUrl oneUrl = new IMDBUrl(strURL, strName, "IMDB");
      _elements.Add(oneUrl);
    }

    private void FindIMDBActor(string strURL)
    {
      string[] vdbParserStr = VdbParserStringActor();

      if (vdbParserStr == null || vdbParserStr.Length != 29)
      {
        return;
      }

      try
      {
        string absoluteUri;
        // UTF-8 have problem with special country chars, default IMDB enc is used
        string strBody = GetPage(strURL, "utf-8", out absoluteUri);
        string value = string.Empty;
        HTMLParser parser = new HTMLParser(strBody);
        
        //if ((parser.skipToEndOf("<title>")) &&
        //    (parser.extractTo("</title>", ref value)) && !value.ToLower().Equals("imdb name search"))
        if ((parser.skipToEndOf(vdbParserStr[0])) &&
            (parser.extractTo(vdbParserStr[1], ref value)) && !value.ToLower().Equals(vdbParserStr[2]))
        {
          value = new HTMLUtil().ConvertHTMLToAnsi(value);
          value = Util.Utils.RemoveParenthesis(value).Trim();
          IMDBUrl oneUrl = new IMDBUrl(absoluteUri, value, "IMDB");
          _elements.Add(oneUrl);
          return;
        }

        parser.resetPosition();

        string popularBody = string.Empty;
        string exactBody = string.Empty;
        string url = string.Empty;
        string name = string.Empty;
        string role = string.Empty;

        //if (parser.skipToStartOfNoCase("Popular names"))
        if (parser.skipToStartOfNoCase(vdbParserStr[3]))
        {
          //parser.skipToEndOf("<table>");
          parser.skipToEndOf(vdbParserStr[4]);
          //parser.extractTo("</table>", ref popularBody);
          parser.extractTo(vdbParserStr[5], ref popularBody);

          parser = new HTMLParser(popularBody);
          
          //while (parser.skipToStartOf("href=\"/name/"))
          while (parser.skipToStartOf(vdbParserStr[6]))
          {
            //parser.skipToEndOf("href=\"");
            parser.skipToEndOf(vdbParserStr[7]);
            //parser.extractTo("\"", ref url);
            parser.extractTo(vdbParserStr[8], ref url);
            //parser.skipToEndOf("Image()).src='/rg/find-name-");
            parser.skipToEndOf(vdbParserStr[9]);
            //parser.skipToEndOf("';\">");
            parser.skipToEndOf(vdbParserStr[10]);
            //parser.extractTo("</a>", ref name);
            parser.extractTo(vdbParserStr[11], ref name);
            //parser.skipToEndOf("<small>(");
            parser.skipToEndOf(vdbParserStr[12]);
            //parser.extractTo(",", ref role);
            parser.extractTo(vdbParserStr[13], ref role);
            
            if (role != string.Empty)
            {
              name += " - " + role;
            }

            name = new HTMLUtil().ConvertHTMLToAnsi(name);
            name = Util.Utils.RemoveParenthesis(name).Trim();
            IMDBUrl newUrl = new IMDBUrl("http://www.imdb.com" + url, name, "IMDB");
            _elements.Add(newUrl);
            //parser.skipToEndOf("</tr>");
            parser.skipToEndOf(vdbParserStr[14]);
          }
        }
        parser = new HTMLParser(strBody);
        
        //if (parser.skipToStartOfNoCase("Exact Matches"))
        if (parser.skipToStartOfNoCase(vdbParserStr[15]))
        {
          //parser.skipToEndOf("<table>");
          parser.skipToEndOf(vdbParserStr[16]);
          //parser.extractTo("</table>", ref exactBody);
          parser.extractTo(vdbParserStr[17], ref exactBody);
        }
        //else if (parser.skipToStartOfNoCase("Approx Matches"))
        else if (parser.skipToStartOfNoCase(vdbParserStr[18]))
        {
          //parser.skipToEndOf("<table>");
          parser.skipToEndOf(vdbParserStr[19]);
          //parser.extractTo("</table>", ref exactBody);
          parser.extractTo(vdbParserStr[20], ref exactBody);
        }
        else
        {
          return;
        }

        parser = new HTMLParser(exactBody);
        url = string.Empty;
        name = string.Empty;
        role = string.Empty;
        
        //<a href="/name/nm0000246/" onclick="set_args('nm0000246', 1)">Bruce Willis</a>
        //while (parser.skipToStartOf("href=\"/name/"))
        while (parser.skipToStartOf(vdbParserStr[21]))
        {
          //parser.skipToEndOf("href=\"");
          parser.skipToEndOf(vdbParserStr[22]);
          //parser.extractTo("\"", ref url);
          parser.extractTo(vdbParserStr[23], ref url);
          //parser.skipToEndOf("Image()).src='/rg/find-name-");
          parser.skipToEndOf(vdbParserStr[24]);
          //parser.skipToEndOf("';\">");
          parser.skipToEndOf(vdbParserStr[25]);
          //parser.extractTo("</a>", ref name);
          parser.extractTo(vdbParserStr[26], ref name);
          //parser.skipToEndOf("<small>(");
          parser.skipToEndOf(vdbParserStr[27]);
          //parser.extractTo(",", ref role);
          parser.extractTo(vdbParserStr[28], ref role);

          if (role != string.Empty)
          {
            name += " - " + role;
          }

          name = new HTMLUtil().ConvertHTMLToAnsi(name);
          name = Util.Utils.RemoveParenthesis(name).Trim();
          IMDBUrl newUrl = new IMDBUrl("http://www.imdb.com" + url, name, "IMDB");
          _elements.Add(newUrl);
          //parser.skipToEndOf("</tr>");
          parser.skipToEndOf(vdbParserStr[29]);
        }
      }
      catch (Exception ex)
      {
        Log.Error("exception for imdb lookup of {0} err:{1} stack:{2}", strURL, ex.Message, ex.StackTrace);
      }
    }
    
    // Filmograpy and bio
    public bool GetActorDetails(IMDBUrl url, out IMDBActor actor)
    {
      actor = new IMDBActor();

      string[] vdbParserStr = VdbParserStringActorDetails();

      if (vdbParserStr == null || vdbParserStr.Length != 45)
      {
        return false;
      }

      try
      {
        string absoluteUri;
        string strBody = GetPage(url.URL, "utf-8", out absoluteUri);
        
        if (strBody == null)
        {
          return false;
        }
        
        if (strBody.Length == 0)
        {
          return false;
        }
        
        // IMDBActorID
        try
        {
          int pos = url.URL.LastIndexOf("nm");
          string id = url.URL.Substring(pos, 9).Replace("/", string.Empty);
          actor.IMDBActorID = id;
        }
        catch (Exception) {}

        HTMLParser parser = new HTMLParser(strBody);
        string strThumb = string.Empty;
        string value = string.Empty;
        string value2 = string.Empty;
        
        // Actor name
        //if ((parser.skipToEndOf("<title>")) &&
        //    (parser.extractTo("- IMDb</title>", ref value)))
        if ((parser.skipToEndOf(vdbParserStr[0])) &&
            (parser.extractTo(vdbParserStr[1], ref value)))
        {
          value = new HTMLUtil().ConvertHTMLToAnsi(value);
          value = Util.Utils.RemoveParenthesis(value).Trim();
          actor.Name = HttpUtility.HtmlDecode(value.Trim());
        }
        
        if (actor.Name == string.Empty)
        {
          actor.Name = url.Title;
        }
        
        // Photo
        string parserTxt = parser.Content;
        string photoBlock = string.Empty;
        
        //if (parser.skipToStartOf("<td id=\"img_primary\"") &&
        //    (parser.extractTo("</td>", ref photoBlock)))
        if (parser.skipToStartOf(vdbParserStr[2]) &&
            (parser.extractTo(vdbParserStr[3], ref photoBlock)))
        {
          parser.Content = photoBlock;
        
          //if ((parser.skipToEndOf("<img src=\"")) &&
          //    (parser.extractTo("\"", ref strThumb)))
          if ((parser.skipToEndOf(vdbParserStr[4])) &&
              (parser.extractTo(vdbParserStr[5], ref strThumb)))
          {
            actor.ThumbnailUrl = strThumb;
          }
          parser.Content = parserTxt;
        }
        
        // Birth date
        //if ((parser.skipToEndOf("Born:")) &&
        //    (parser.skipToEndOf("birth_monthday=")) &&
        //    (parser.skipToEndOf(">")) &&
        //    (parser.extractTo("<", ref value)) &&
        //    (parser.skipToEndOf("year=")) &&
        //    (parser.extractTo("\"", ref value2)))
        if ((parser.skipToEndOf(vdbParserStr[6])) &&
            (parser.skipToEndOf(vdbParserStr[7])) &&
            (parser.skipToEndOf(vdbParserStr[8])) &&
            (parser.extractTo(vdbParserStr[9], ref value)) &&
            (parser.skipToEndOf(vdbParserStr[10])) &&
            (parser.extractTo(vdbParserStr[11], ref value2)))

        {
          actor.DateOfBirth = value + " " + value2;
        }
        
        // Death date
        //if ((parser.skipToEndOf(">Died:</h4>")) &&
        //    (parser.skipToEndOf("deaths\"")) &&
        //    (parser.skipToEndOf(">")) &&
        //    (parser.extractTo("<", ref value)) &&
        //    (parser.skipToEndOf("death_date=")) &&
        //    (parser.extractTo("\"", ref value2)))
        if ((parser.skipToEndOf(vdbParserStr[12])) &&
            (parser.skipToEndOf(vdbParserStr[13])) &&
            (parser.skipToEndOf(vdbParserStr[14])) &&
            (parser.extractTo(vdbParserStr[15], ref value)) &&
            (parser.skipToEndOf(vdbParserStr[16])) &&
            (parser.extractTo(vdbParserStr[17], ref value2)))
        {
          actor.DateOfDeath = value + " " + value2;
        }

        parser.resetPosition();
        
        // Birth place
        //if ((parser.skipToEndOf("birth_place=")) &&
        //    (parser.skipToEndOf(">")) &&
        //    (parser.extractTo("<", ref value)))
        if ((parser.skipToEndOf(vdbParserStr[18])) &&
            (parser.skipToEndOf(vdbParserStr[19])) &&
            (parser.extractTo(vdbParserStr[20], ref value)))
        {
          actor.PlaceOfBirth = HttpUtility.HtmlDecode(value);
        }

        // Death place
        //if ((parser.skipToEndOf("death_place=")) &&
        //    (parser.skipToEndOf(">")) &&
        //    (parser.extractTo("<", ref value)))
        if ((parser.skipToEndOf(vdbParserStr[21])) &&
            (parser.skipToEndOf(vdbParserStr[22])) &&
            (parser.extractTo(vdbParserStr[23], ref value)))
        {
          actor.PlaceOfDeath = HttpUtility.HtmlDecode(value);
        }

        //Mini Biography
        parser.resetPosition();
        
        //if ((parser.skipToEndOf("<td id=\"overview-top\">")) &&
        //    (parser.skipToEndOf("<p>")) &&
        //    (parser.extractTo("See full bio</a>", ref value)))
        if ((parser.skipToEndOf(vdbParserStr[24])) &&
            (parser.skipToEndOf(vdbParserStr[25])) &&
            (parser.extractTo(vdbParserStr[26], ref value)))
        {
          value = new HTMLUtil().ConvertHTMLToAnsi(value);
          actor.MiniBiography = Util.Utils.stripHTMLtags(value);
          actor.MiniBiography = actor.MiniBiography.Replace("See full bio »", string.Empty).Trim();
          actor.MiniBiography = HttpUtility.HtmlDecode(actor.MiniBiography); // Remove HTML entities like &#189;
          
          if (actor.MiniBiography != string.Empty)
          {
            // get complete biography
            string bioURL = absoluteUri;
            
            //if (!bioURL.EndsWith("/"))
            if (!bioURL.EndsWith(vdbParserStr[27]))
            {
              //bioURL += "/bio";
              bioURL += vdbParserStr[28];
            }
            else
            {
              //bioURL += "bio";
              bioURL += vdbParserStr[29];
            }

            string strBioBody = GetPage(bioURL, "utf-8", out absoluteUri);
            
            if (!string.IsNullOrEmpty(strBioBody))
            {
              HTMLParser parser1 = new HTMLParser(strBioBody);

              //if (parser1.skipToEndOf("<h5>Mini Biography</h5>") &&
              //    parser1.skipToEndOf("<div class=\"wikipedia_bio\">") &&
              //    parser1.extractTo("</div>", ref value))
              if (parser1.skipToEndOf(vdbParserStr[30]) &&
                  parser1.skipToEndOf(vdbParserStr[31]) &&
                  parser1.extractTo(vdbParserStr[32], ref value))
              {
                value = new HTMLUtil().ConvertHTMLToAnsi(value);
                value = Regex.Replace(value, @"</h5>\s<h5>", "\n\r");
                value = Regex.Replace(value, @"<h5>", "\n\r\n\r");
                value = Regex.Replace(value, @"</h5>", ":\n\r");
                actor.Biography = Util.Utils.stripHTMLtags(value).Trim();
                actor.Biography = HttpUtility.HtmlDecode(actor.Biography); // Remove HTML entities like &#189;
              }
              else
              {
                parser1.resetPosition();
                
                //if (parser1.skipToEndOf("<h5>Mini Biography</h5>") &&
                //  parser1.extractTo("</p>", ref value))
                if (parser1.skipToEndOf(vdbParserStr[33]) &&
                  parser1.extractTo(vdbParserStr[34], ref value))
                {
                  value = new HTMLUtil().ConvertHTMLToAnsi(value);
                  actor.Biography = Util.Utils.stripHTMLtags(value).Trim();
                  actor.Biography = HttpUtility.HtmlDecode(actor.Biography); // Remove HTML entities like &#189;
                }
              }
            }
          }
        }
        // Person is movie director or an actor/actress
        bool isActorPass = false;
        bool isDirectorPass = false;
        bool isWriterPass = false;
        
        parser.resetPosition();

        HTMLParser dirParser = new HTMLParser(); // HTML body for Director
        HTMLParser wriParser = new HTMLParser(); // HTML body for Writers

        //if ((parser.skipToEndOf("name=\"Director\">Director</a>")) &&
        //    (parser.skipToEndOf("</div>")))
        if ((parser.skipToEndOf(vdbParserStr[35])) &&
            (parser.skipToEndOf(vdbParserStr[36])))
        {
          isDirectorPass = true;
          dirParser.Content = parser.Content;
        }
        
        parser.resetPosition();

        //if ((parser.skipToEndOf("name=\"Writer\">Writer</a>")) &&
        //    (parser.skipToEndOf("</div>")))
        if ((parser.skipToEndOf(vdbParserStr[37])) &&
            (parser.skipToEndOf(vdbParserStr[38])))
        {
          isWriterPass = true;
          wriParser.Content = parser.Content;
        }

        parser.resetPosition();

        //if (parser.skipToEndOf("name=\"Actress\">Actress</a>") || 
        //    parser.skipToEndOf("name=\"Actor\">Actor</a>"))
        if (parser.skipToEndOf(vdbParserStr[39]) ||
          parser.skipToEndOf(vdbParserStr[40]))
        {
          isActorPass = true;
        }
        
        // Get filmography Actor
        if (isActorPass)
        {
          GetActorMovies(actor, parser, false, false);
        }
        
        // Get filmography for writers
        if (isWriterPass)
        {
          parser = wriParser;
          parser.resetPosition();

          //if ((parser.skipToEndOf("name=\"Writer\">Writer</a>")) &&
          //  (parser.skipToEndOf("</div>")))
          if ((parser.skipToEndOf(vdbParserStr[41])) &&
            (parser.skipToEndOf(vdbParserStr[42])))
          {
            GetActorMovies(actor, parser, false, true);
          }
        }

        // Get filmography Director
        if (isDirectorPass)
        {
          parser = dirParser;
          parser.resetPosition();
          
          //if (parser.skipToEndOf("name=\"Director\">Director</a>") &&
          //    parser.skipToEndOf("</div>"))
          if (parser.skipToEndOf(vdbParserStr[43]) &&
              parser.skipToEndOf(vdbParserStr[44]))
          {
            GetActorMovies(actor, parser, true, false);
          }
        }

        // Add filmography
        if (actor.Count > 0)
        {
          actor.SortActorMoviesByYear();
        }

        return true;
      }
      catch (Exception ex)
      {
        Log.Error("IMDB.GetActorDetails({0} exception:{1} {2} {3}", url.URL, ex.Message, ex.Source, ex.StackTrace);
      }
      return false;
    }

    private void GetActorMovies(IMDBActor actor, HTMLParser parser, bool director, bool writer)
    {
      string[] vdbParserStr = VdbParserStringActorMovies();

      if (vdbParserStr == null || vdbParserStr.Length != 19)
      {
        return;
      }

      string movies = string.Empty;
      
      // Get films and roles block
      //if (parser.extractTo("<div id", ref movies))
      if (parser.extractTo(vdbParserStr[0], ref movies))
      {
        parser.Content = movies;
      }
      
      // Parse block for evey film and get year, title and it's imdbID and role
      //while (parser.skipToStartOf("<span class=\"year_column\""))
      while (parser.skipToStartOf(vdbParserStr[1]))
      {
        string movie = string.Empty;
        
        //if (parser.extractTo("<div class", ref movie))
        if (parser.extractTo(vdbParserStr[2], ref movie))
        {
          //movie += "</li>";
          movie += vdbParserStr[3];
          HTMLParser movieParser = new HTMLParser(movie);
          string title = string.Empty;
          string strYear = string.Empty;
          string role = string.Empty;
          string imdbID = string.Empty;
          // IMDBid
          //movieParser.skipToEndOf("title/");
          movieParser.skipToEndOf(vdbParserStr[4]);
          //movieParser.extractTo("/", ref imdbID);
          movieParser.extractTo(vdbParserStr[5], ref imdbID);
          // Title
          movieParser.resetPosition();
          //movieParser.skipToEndOf("<a");
          movieParser.skipToEndOf(vdbParserStr[6]);
          //movieParser.skipToEndOf(">");
          movieParser.skipToEndOf(vdbParserStr[7]);
          //movieParser.extractTo("<br/>", ref title);
          movieParser.extractTo(vdbParserStr[8], ref title);
          title = Util.Utils.stripHTMLtags(title);
          title = title.Replace("\n", " ").Replace("\r", string.Empty);
          title = HttpUtility.HtmlDecode(title.Trim()); // Remove HTML entities like &#189;
          // Year
          movieParser.resetPosition();
          
          //if (movieParser.skipToStartOf(">20") &&
          //    movieParser.skipToEndOf(">"))
          if (movieParser.skipToStartOf(vdbParserStr[9]) &&
              movieParser.skipToEndOf(vdbParserStr[10]))
          {
            //movieParser.extractTo("<", ref strYear);
            movieParser.extractTo(vdbParserStr[11], ref strYear);
          }
          //else if (movieParser.skipToStartOf(">19") &&
          //         movieParser.skipToEndOf(">"))
          else if (movieParser.skipToStartOf(vdbParserStr[12]) &&
                   movieParser.skipToEndOf(vdbParserStr[13]))
          {
            //movieParser.extractTo("<", ref strYear);
            movieParser.extractTo(vdbParserStr[14], ref strYear);
          }
          
          // Roles
          if (!director && !writer)
          {
            //if (movieParser.skipToEndOf("<br/>")) // Role case 1, no character link
            if (movieParser.skipToEndOf(vdbParserStr[15])) // Role case 1, no character link
            {
              //movieParser.extractTo("<", ref role);
              movieParser.extractTo(vdbParserStr[16], ref role);
              role = Util.Utils.stripHTMLtags(role).Trim();
              role = HttpUtility.HtmlDecode(role.Replace("\n", " ")
                                              .Replace("\r", string.Empty).Trim());
              
              if (role != null && role.EndsWith("/"))
              {
                role = role.Remove(role.LastIndexOf("/"));
              }

              if (role == string.Empty) // Role case 2, with character link
              {
                movieParser.resetPosition();
                //movieParser.skipToEndOf("<br/>");
                movieParser.skipToEndOf(vdbParserStr[17]);
                //movieParser.extractTo("</a>", ref role);
                movieParser.extractTo(vdbParserStr[18], ref role);
                role = Util.Utils.stripHTMLtags(role).Trim();
                role = HttpUtility.HtmlDecode(role.Replace("\n", " ")
                                                  .Replace("\r", string.Empty).Trim());
                
                if (role != null && role.EndsWith("/"))
                {
                  role = role.Remove(role.LastIndexOf("/"));
                }
              }
            }
          }
          else if (director)
          {
            role = GUILocalizeStrings.Get(199).Replace(":", string.Empty);
          }
          else // Writer
          {
            role = GUILocalizeStrings.Get(200).Replace(":", string.Empty);
          }
          
          int year = 0;
          
          try
          {
            year = Int32.Parse(strYear.Substring(0, 4));
          }
          catch (Exception)
          {
            year = 1900;
          }

          // SKip trash titles
          if (!SkipNoMovies(title))
          {
            string regex = "(\\(.*\\))";

            if (title != null && title.IndexOf("(") > 0)
            {
              title = Regex.Replace(title, regex, "").Trim();
            }

            IMDBActor.IMDBActorMovie actorMovie = new IMDBActor.IMDBActorMovie();
            actorMovie.MovieTitle = title;

            if (role != null) role = Regex.Replace(role, regex, "").Trim();
            actorMovie.Role = role;

            actorMovie.Year = year;
            actorMovie.MovieImdbID = imdbID;
            // Check if director/writer movie exists in actors movies, concatenate role
            // to already fetched actor movie (no duplicate movie entries)
            bool skipAdd = false;

            if (writer)
            {
              for (int i = 0; i < actor.Count; i++)
              {
                if (actor[i].MovieImdbID == imdbID)
                {
                  actor[i].Role = GUILocalizeStrings.Get(200).Replace(":", string.Empty) + ", " + actor[i].Role;
                  skipAdd = true;
                  break;
                }
              }
            }

            if (director)
            {
              for (int i = 0; i < actor.Count; i++)
              {
                if (actor[i].MovieImdbID == imdbID)
                {
                  actor[i].Role = GUILocalizeStrings.Get(199).Replace(":", string.Empty) + ", " + actor[i].Role;
                  skipAdd = true;
                  break;
                }
              }
            }

            if (!skipAdd)
            {
              actor.Add(actorMovie);
            }
          }
        }
      }
    }

    private bool SkipNoMovies(string title)
    {
      if (title.ToLower().Contains("(short)") ||
          title.ToLower().Contains("video short") ||
          title.ToLower().Contains("video game") ||
          title.ToLower().Contains("tv series") ||
          title.ToLower().Contains("tv mini-series") ||
          title.ToLower().Contains("tv short") ||
          title.ToLower().Contains("(documentary)") ||
          title.ToLower().Contains("tv documentary") ||
          title.ToLower().Contains("tv special") ||
          title.ToLower().Contains("documentary short"))
      {
        return true;
      }
      else
      {
        return false;
      }
    }

    // Find actor parser strings
    private string[] VdbParserStringActor()
    {
      string[] vdbParserStr = VideoDatabaseParserStrings.GetParserStrings("ActorInfoMain");
      return vdbParserStr;
    }

    // Actor detals parser strings
    private string[] VdbParserStringActorDetails()
    {
      string[] vdbParserStr = VideoDatabaseParserStrings.GetParserStrings("ActorInfoDetails");
      return vdbParserStr;
    }

    // Actor movies & roles parser strings
    private string[] VdbParserStringActorMovies()
    {
      string[] vdbParserStr = VideoDatabaseParserStrings.GetParserStrings("ActorInfoMovies");
      return vdbParserStr;
    }

    #endregion
  }

  /// <summary>
  /// Interface used for script support
  /// </summary>
  public interface IIMDBScriptGrabber
  {
    void FindFilm(string title, int limit, ArrayList elements);
    bool GetDetails(IMDB.IMDBUrl url, ref IMDBMovie movieDetails);
    string GetName();
    string GetLanguage();
  }
}
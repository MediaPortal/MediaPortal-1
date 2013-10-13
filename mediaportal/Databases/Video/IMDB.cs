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
using System.Text.RegularExpressions;
using System.Web;
using CSScriptLibrary;
using MediaPortal.Configuration;
using MediaPortal.ExtensionMethods;
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
    public static string InternalScriptDirectory = Config.GetSubFolder(Config.Dir.Config, "scripts");
    public const string DEFAULT_DATABASE = "IMDB_MP13x";
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
      private static IIMDBScriptGrabber _grabber;
      private static AsmHelper _asmHelper;
      private static Dictionary<string, IIMDBScriptGrabber> _grabbers = new Dictionary<string, IIMDBScriptGrabber>();

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

      public MovieInfoDatabase(string id, int limit)
      {
        ID = id;
        Limit = limit;
      }

      public IIMDBScriptGrabber Grabber
      {
        get
        {
          if (!_grabbers.ContainsKey(ID))
          {
            if (!LoadScript(ID))
            {
              Grabber = null;
            }
            else
            {
              _grabbers[ID] = _grabber;
            }
            
            return _grabber;
          }
          return _grabbers[ID];
        }
        set { _grabber = value; }
      }

      public static void ResetGrabber()
      {
        if (_asmHelper != null)
        {
          _asmHelper.Dispose();
          _asmHelper = null;
        }

        if (_grabber != null)
        {
          _grabber.SafeDispose();
          _grabber = null;
        }

        _grabbers.Clear();
      }

      private static bool LoadScript(string dbId)
      {
        string scriptFileName = ScriptDirectory + @"\" + dbId + ".csscript";

        // Script support script.csscript
        if (!File.Exists(scriptFileName))
        {
          Log.Error("InfoGrabber LoadScript() - grabber script not found: {0}", scriptFileName);
          return false;
        }

        try
        {
          Environment.CurrentDirectory = Config.GetFolder(Config.Dir.Base);
          _asmHelper = new AsmHelper(CSScript.Load(scriptFileName, null, false));
          _grabber = (IIMDBScriptGrabber)_asmHelper.CreateObject("Grabber");
        }
        catch (Exception ex)
        {
          Log.Error("InfoGrabber LoadScript() - file: {0}, message : {1}", scriptFileName, ex.Message);
          return false;
        }

        return true;
      }
    }

    public class InternalActorsScriptGrabber
    {
      private static IIMDBInternalActorsScriptGrabber _internalActorsGrabber;
      private static bool _internalActorsGrabberLoaded;
      private static AsmHelper _asmHelper;

      public static void ResetGrabber()
      {
        if (_asmHelper != null)
        {
          _asmHelper.Dispose();
          _asmHelper = null;
        }

        if (_internalActorsGrabber != null)
        {
          _internalActorsGrabber.SafeDispose();
          _internalActorsGrabber = null;
        }

        _internalActorsGrabberLoaded = false;
      }

      public static IIMDBInternalActorsScriptGrabber InternalActorsGrabber
      {
        get
        {
          if (!_internalActorsGrabberLoaded)
          {
            if (!LoadScript())
              InternalActorsGrabber = null;
            _internalActorsGrabberLoaded = true; // only try to load it once
          }
          return _internalActorsGrabber;
        }
        set { _internalActorsGrabber = value; }
      }

      private static bool LoadScript()
      {
        string scriptFileName = InternalScriptDirectory + @"\InternalActorMoviesGrabber.csscript";

        // Script support script.csscript
        if (!File.Exists(scriptFileName))
        {
          Log.Error("InternalActorMoviesGrabber LoadScript() - grabber script not found: {0}", scriptFileName);
          return false;
        }

        try
        {
          Environment.CurrentDirectory = Config.GetFolder(Config.Dir.Base);
          _asmHelper = new AsmHelper(CSScript.Load(scriptFileName, null, false));
          InternalActorsGrabber = (IIMDBInternalActorsScriptGrabber) _asmHelper.CreateObject("InternalActorsGrabber");
        }
        catch (Exception ex)
        {
          Log.Error("InternalActorMoviesGrabber LoadScript() - file: {0}, message : {1}", scriptFileName, ex.Message);
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
    /// make a searchstring out of the filename
    /// </summary>
    public string GetSearchString(string strMovie)
    {
      string strUrl = strMovie;
      strUrl = strUrl.Trim();
      string[] vdbParserStr = VdbParserStringCleaner();

      if (vdbParserStr == null || vdbParserStr.Length != 1)
      {
        return strUrl;
      }

      Regex rx = new Regex(vdbParserStr[0], RegexOptions.IgnoreCase);
      strUrl = rx.Replace(strUrl, "");
      strUrl = strUrl.Replace(".", " ");
      strUrl = strUrl.Replace("_", " ").Trim();
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
      _elements.Clear();
      
      try
      {
        _elements = InternalActorsScriptGrabber.InternalActorsGrabber.FindIMDBActor(strURL);
      }
      catch (Exception ex)
      {
        Log.Error("IMDB FindIMDBActor error: {0}", ex.Message);
      }
    }
    
    // Filmograpy and bio
    public bool GetActorDetails(IMDBUrl url, out IMDBActor actor)
    {
      actor = new IMDBActor();

      try
      {
        if (InternalActorsScriptGrabber.InternalActorsGrabber.GetActorDetails(url, out actor))
        {
          // Add filmography
          if (actor.Count > 0)
          {
            actor.SortActorMoviesByYear();
          }

          return true;
        }
      }
      catch (Exception ex)
      {
        Log.Error("IMDB GetActorDetails Error: {0}", ex.Message);
      }
      return false;
    }

    #endregion

    /// <summary>
    /// Helper function for fetching actorsID from IMDB movie page using IMDBmovieID.
    /// Parameter imdbMovieID must be in IMDB format (ie. tt0123456 including leading zeros)
    /// </summary>
    /// <param name="imdbMovieID"></param>
    /// <param name="actorList"></param>
    public void GetIMDBMovieActorsList(string imdbMovieID, ref ArrayList actorList)
    {
      if (!Win32API.IsConnectedToInternet())
      {
        return;
      }
      if (imdbMovieID == null) return;
      if (imdbMovieID == string.Empty | !imdbMovieID.StartsWith("tt")) return;

      bool shortActorsListSize = true;

      using (Settings xmlreader = new MPSettings())
      {
        if (xmlreader.GetValueAsString("moviedatabase", "actorslistsize", "Short") != "Short")
        {
          shortActorsListSize = false;
        }
      }

      try
      {
        actorList = InternalActorsScriptGrabber.InternalActorsGrabber.GetIMDBMovieActorsList(imdbMovieID,
        	shortActorsListSize);
      }
      catch (Exception ex)
      {
        Log.Error("IMDB GetIMDBMovieActorsList error: {0}", ex.Message);
      }
    }
    
    // Get actor search parser strings
    private string[] VdbParserStringCleaner()
    {
      string[] vdbParserStr = VideoDatabaseParserStrings.GetParserStrings("CleanFilter");
      return vdbParserStr;
    }
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

  public interface IIMDBInternalActorsScriptGrabber
  {
    bool GetPlotImdb(ref IMDBMovie movie);
    string GetThumbImdb(string imdbId);
    ArrayList FindIMDBActor(string strURL);
    bool GetActorDetails(IMDB.IMDBUrl url, out IMDBActor actor);
    ArrayList GetIMDBMovieActorsList(string imdbMovieID, bool shrtActorsList);
  }
}
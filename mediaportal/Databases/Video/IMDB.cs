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
using System.Collections;
using System.IO;
using System.Net;
using System.Web;
using System.Text;
using System.Text.RegularExpressions;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Configuration;
using CSScriptLibrary;

namespace MediaPortal.Video.Database
{
  /// <summary>
  /// supporting classes to fetch movie information out of different databases
  /// currently supported: IMDB http://us.imdb.com and additional database by using external csscripts
  /// </summary>
  public class IMDB : IEnumerable
  {
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
      bool OnActorsStarted(IMDBFetcher fetcher);
      bool OnActorsEnd(IMDBFetcher fetcher);
      bool OnRequestMovieTitle(IMDBFetcher fetcher, out string movieName);
      bool OnSelectMovie(IMDBFetcher fetcher, out int selected);
      bool OnScanStart(int total);
      bool OnScanEnd();
      bool OnScanIterating(int count);
      bool OnScanIterated(int count);
    }
    // class that represents URL and Title of a search result
    public class IMDBUrl
    {
      string m_strURL = "";
      string m_strTitle = "";
      string m_strDatabase = "";
      string m_strIMDBURL = "";

      public IMDBUrl(string strURL, string strTitle, string strDB)
      {
        m_strURL = strURL;
        m_strTitle = strTitle;
        m_strDatabase = strDB;
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
    }; // END class IMDBUrl


    // do not know, what this class does ;-)
    public class IMDBEnumerator : IEnumerator
    {
      private int position = -1;
      private IMDB t;

      public IMDBEnumerator(IMDB t)
      {
        this.t = t;
      }

      public bool MoveNext()
      {
        if (position < t.elements.Count - 1)
        {
          position++;
          return true;
        }
        else
        {
          return false;
        }
      }

      public void Reset()
      {
        position = -1;
      }

      public IMDB.IMDBUrl Current // non-IEnumerator version: type-safe
      {
        get
        {
          if (t.elements.Count == 0)
            return null;
          return (IMDB.IMDBUrl)t.elements[position];
        }
      }

      object IEnumerator.Current // IEnumerator version: returns object
      {
        get
        {
          if (t.elements.Count == 0)
            return null;
          return t.elements[position];
        }
      }
    } // END class IMDBEnumerator

    // internal vars
    // list of the search results, containts objects of IMDBUrl
    ArrayList elements = new ArrayList();

    // Arrays for multiple database support
    int[] aLimits;		// contains the limit for searchresults
    string[] aDatabases;		// contains the name of the database, e.g. IMDB

    IProgress m_progress;
    // constructor
    public IMDB()
      : this(null)
    {
    }

    public IMDB(IProgress progress)
    {

      m_progress = progress;
      // load the settings
      LoadSettings();
    } // END constructor

    // load settings from mediaportal.xml
    private void LoadSettings()
    {
      // getting available databases and limits
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        int iNumber = xmlreader.GetValueAsInt("moviedatabase", "number", 0);
        if (iNumber <= 0)
        {
          // no given databases in XML - setting to IMDB
          aLimits = new int[1];
          aDatabases = new string[1];
          aLimits[0] = 25;
          aDatabases[0] = "IMDB";
        }
        else
        {
          // initialise arrays
          aLimits = new int[iNumber];
          aDatabases = new string[iNumber];
          string strDatabase;
          int iLimit;
          bool bDouble = false;
          // get the databases
          for (int i = 0; i < iNumber; i++)
          {
            bDouble = false;
            iLimit = xmlreader.GetValueAsInt("moviedatabase", "limit" + i.ToString(), 25);
            strDatabase = xmlreader.GetValueAsString("moviedatabase", "database" + i.ToString(), "IMDB");
            // be aware of double entries!
            for (int j = 0; j < i; j++)
            {
              if (aDatabases[j] == strDatabase)
              {
                // double entry found, exit search
                bDouble = true;
                j = i;
              }
            }
            // valid entry?
            if (!bDouble)
            {
              // entry does not exist yet
              aLimits[i] = iLimit;
              aDatabases[i] = strDatabase;
            }
            else
            {
              // skip this entry
              aLimits[i] = 0;
              aDatabases[i] = "";
            }
          }
        }
      }
    } // END LoadSettings()

    // count the elements
    public int Count
    {
      get { return elements.Count; }
    } // END Count

    //??
    public IMDB.IMDBUrl this[int index]
    {
      get { return (IMDB.IMDBUrl)elements[index]; }
    } // END IMDB.IMDBUrl this[int index]

    //??
    public IMDBEnumerator GetEnumerator() // non-IEnumerable version
    {
      return new IMDBEnumerator(this);
    } // END IMDBEnumerator GetEnumerator()

    //??
    IEnumerator IEnumerable.GetEnumerator() // IEnumerable version
    {
      return (IEnumerator)new IMDBEnumerator(this);
    } // END IEnumerable.GetEnumerator()

    // trys to get a webpage from the specified url and returns the content as string
    private string GetPage(string strURL, string strEncode, out string absoluteUri)
    {
      string strBody = "";
      absoluteUri = string.Empty;
      Stream ReceiveStream = null;
      StreamReader sr = null;
      WebResponse result = null;
      try
      {
        // Make the Webrequest
        //Log.Info("IMDB: get page:{0}", strURL);
        WebRequest req = WebRequest.Create(strURL);

        result = req.GetResponse();
        ReceiveStream = result.GetResponseStream();

        // Encoding: depends on selected page
        Encoding encode = System.Text.Encoding.GetEncoding(strEncode);
        sr = new StreamReader(ReceiveStream, encode);
        strBody = sr.ReadToEnd();

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
          catch (Exception)
          {
          }
        }
        if (ReceiveStream != null)
        {
          try
          {
            ReceiveStream.Close();
          }
          catch (Exception)
          {
          }
        }
        if (result != null)
        {
          try
          {
            result.Close();
          }
          catch (Exception)
          {
          }
        }
      }
      return strBody;
    } // END GetPage()

    // cuts end of sting after strWord
    void RemoveAllAfter(ref string strLine, string strWord)
    {
      int iPos = strLine.IndexOf(strWord);
      if (iPos > 0)
      {
        strLine = strLine.Substring(0, iPos);
      }
    } // END RemoveAllAfter()

    // make a searchstring out of the filename
    string GetSearchString(string strMovie)
    {
      string strURL = strMovie;
      strURL = strURL.ToLower();
      strURL = strURL.Trim();

      // @ 23.09.2004 by FlipGer
      if (strURL.Length <= 7)
      {
        return strURL;
      }
      // END @ | i think it does not make much sense to parse such a short string
      // and i have no problems with x-men on OFDB ;-) and a better result on IMDB with x-men (try out "x men" :-)

      string strTmp = "";
      int ipos = 0;
      int iBracket = 0;
      //bool	bSkip = false;
      for (int i = 0; i < strURL.Length; ++i)
      {
        /* Why are numbers bigger than 999 skipped?
        for (int c=0;i+c < strURL.Length&&Char.IsDigit(strURL[i+c]);c++)
        {
          Log.Info("c: {0}",c);
          if (c==3)
          {
            i+=4;
            break;
          }
        }*/
        //if (i >=strURL.Length) break;
        char kar = strURL[i];
        if (kar == '[' || kar == '(')
          iBracket++;			//skip everthing between () and []
        else if (kar == ']' || kar == ')')
          iBracket--;
        else if (iBracket <= 0)
        {
          // change all non cahrs or digits into ' '
          if (!Char.IsLetterOrDigit(kar))
          {
            kar = ' ';

          }
          // skip whitespace at the beginning, only necessary if the "number skipping" is used
          //if ((kar==' ') && (ipos==0)) continue;

          // Mars Warrior @ 03-sep-2004.
          // Check for ' ' and '+' to avoid double or more ' ' and '+' which
          // mess up the search to the IMDB...
          if (strTmp.Length == 0)
          {
            strTmp += kar;
            ipos++;
          }
          else
          {
            if (
              Char.IsLetterOrDigit(kar) ||
              (kar == ' ' && strTmp[strTmp.Length - 1] != ' ')
              //|| (kar == '+' && strTmp[strTmp.Length -1] != '+')
              )
            {
              strTmp += kar;
              ipos++;
            }
          }
        }
      }

      strTmp = strTmp.Trim();

      // Mars Warrior @ 03-sep-2004.
      // The simple line "strTmp.ToLower()" does NOT work. As a result the wrong string
      // (still includes the " dvd" etc. strings) is send to the IMDB causing wrong lookups
      // By changing the line, everything is working MUCH better now ;-)

      RemoveAllAfter(ref strTmp, "divx");
      RemoveAllAfter(ref strTmp, "xvid");
      RemoveAllAfter(ref strTmp, "dvd");
      RemoveAllAfter(ref strTmp, " dvdrip");
      RemoveAllAfter(ref strTmp, "svcd");
      RemoveAllAfter(ref strTmp, "mvcd");
      RemoveAllAfter(ref strTmp, "vcd");
      RemoveAllAfter(ref strTmp, "cd");
      RemoveAllAfter(ref strTmp, "ac3");
      RemoveAllAfter(ref strTmp, "ogg");
      RemoveAllAfter(ref strTmp, "ogm");
      RemoveAllAfter(ref strTmp, "internal");
      RemoveAllAfter(ref strTmp, "fragment");
      RemoveAllAfter(ref strTmp, "proper");
      RemoveAllAfter(ref strTmp, "limited");
      RemoveAllAfter(ref strTmp, "rerip");

      RemoveAllAfter(ref strTmp, "+divx");
      RemoveAllAfter(ref strTmp, "+xvid");
      RemoveAllAfter(ref strTmp, "+dvd");
      RemoveAllAfter(ref strTmp, "+dvdrip");
      RemoveAllAfter(ref strTmp, "+svcd");
      RemoveAllAfter(ref strTmp, "+mvcd");
      RemoveAllAfter(ref strTmp, "+vcd");
      RemoveAllAfter(ref strTmp, "+cd");
      RemoveAllAfter(ref strTmp, "+ac3");
      RemoveAllAfter(ref strTmp, "+ogg");
      RemoveAllAfter(ref strTmp, "+ogm");
      RemoveAllAfter(ref strTmp, "+internal");
      RemoveAllAfter(ref strTmp, "+fragment");
      RemoveAllAfter(ref strTmp, "+proper");
      RemoveAllAfter(ref strTmp, "+limited");
      RemoveAllAfter(ref strTmp, "+rerip");

      // return the new formatted string
      return strTmp;
    } // END GetSearchString()

    // this method switches between the different databases to get the search results
    public void Find(string strMovie)
    {
      try
      {
        string strURL;
        // getting searchstring
        string strSearch = HttpUtility.UrlEncode(GetSearchString(strMovie));

        // be aware of german special chars ‰ˆ¸ﬂ √§√∂√º√ü %E4%F6%FC%DF %c3%a4%c3%b6%c3%bc%c3%9f
        strSearch = strSearch.Replace("%c3%a4", "%E4");
        strSearch = strSearch.Replace("%c3%b6", "%F6");
        strSearch = strSearch.Replace("%c3%bc", "%FC");
        strSearch = strSearch.Replace("%c3%9f", "%DF");
        // be aware of spanish special chars Ò·ÈÌÛ˙¡…Õ”⁄ %E4%F6%FC%DF %c3%a4%c3%b6%c3%bc%c3%9f
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
        elements.Clear();

        string line1, line2, line3;
        line1 = GUILocalizeStrings.Get(984);
        line2 = GetSearchString(strMovie).Replace("+", " ");
        line3 = "";
        int percent = 0;

        if (m_progress != null)
          m_progress.OnProgress(line1, line2, line3, percent);
        // search the desired databases
        for (int i = 0; i < aDatabases.Length; i++)
        {
          // only do a search if requested
          if (aLimits[i] > 0)
          {
            switch (aDatabases[i].ToUpper())
            {
              case "IMDB":
                // IMDB support
                line1 = GUILocalizeStrings.Get(984) + ":IMDB";
                if (m_progress != null)
                  m_progress.OnProgress(line1, line2, line3, percent);
                strURL = "http://us.imdb.com/Tsearch?title=" + strSearch;
                FindIMDB(strURL, aLimits[i]);
                percent += 100 / aDatabases.Length;
                if (m_progress != null)
                  m_progress.OnProgress(line1, line2, line3, percent);
                // END IMDB support
                break;

              default:
                // Script support script.csscript
                string grabberFileName = Config.GetSubFolder(Config.Dir.Base, "scripts\\imdb") + @"\" + aDatabases[i] + ".csscript";
                line1 = GUILocalizeStrings.Get(984) + ": Script " + aDatabases[i];
                if (File.Exists(grabberFileName))
                {
                  if (m_progress != null)
                    m_progress.OnProgress(line1, line2, line3, percent);
                  try
                  {
                    Environment.CurrentDirectory = Config.GetFolder(Config.Dir.Base);
                    AsmHelper script = new AsmHelper(CSScriptLibrary.CSScript.Load(grabberFileName, null, false));
                    IIMDBScriptGrabber grabber = (IIMDBScriptGrabber)script.CreateObject("Grabber");
                    grabber.FindFilm(strSearch, aLimits[i], elements);
                    percent += 100 / aDatabases.Length;
                    if (m_progress != null)
                      m_progress.OnProgress(line1, line2, line3, percent);
                  }
                  catch (Exception ex)
                  {
                    Log.Info("Script garbber error file: {0}, message : {1}", grabberFileName, ex.Message);
                  }
                }
                else
                {
                  // unsupported database?
                  Log.Error("Movie database lookup - database not supported: {0}", aDatabases[i].ToUpper());
                }
                break;
            }
          }
        }
      }
      catch (Exception)
      {
      }
    } // END Find()

    #region actors
    public void FindActor(string strActor)
    {
      string strURL;
      // getting searchstring
      string strSearch = HttpUtility.UrlEncode(GetSearchString(strActor));

      // be aware of german special chars ‰ˆ¸ﬂ √§√∂√º√ü %E4%F6%FC%DF %c3%a4%c3%b6%c3%bc%c3%9f
      strSearch = strSearch.Replace("%c3%a4", "%E4");
      strSearch = strSearch.Replace("%c3%b6", "%F6");
      strSearch = strSearch.Replace("%c3%bc", "%FC");
      strSearch = strSearch.Replace("%c3%9f", "%DF");
      // be aware of spanish special chars Ò·ÈÌÛ˙¡…Õ”⁄ %E4%F6%FC%DF %c3%a4%c3%b6%c3%bc%c3%9f
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

      elements.Clear();

      string line1, line2, line3;
      line1 = GUILocalizeStrings.Get(986);
      line2 = strActor;
      line3 = "";
      int percent = -1;
      if (m_progress != null)
        m_progress.OnProgress(line1, line2, line3, percent);
      strURL = String.Format("http://us.imdb.com/find?q={0};nm=on;mx=20", strSearch);
      FindIMDBActor(strURL, strActor);

    } // END FindActor()

    private void FindIMDBActor(string strURL, string strActor)
    {
      try
      {
        HTMLUtil htmlUtil = new HTMLUtil();
        string absoluteUri;
        string strBody = GetPage(strURL, "utf-8", out absoluteUri);
        string value = string.Empty;
        HTMLParser parser = new HTMLParser(strBody);
        if ((parser.skipToEndOf("<title>")) &&
            (parser.extractTo("</title>", ref value)) && !value.Equals("IMDb Name  Search"))
        {
          value = new HTMLUtil().ConvertHTMLToAnsi(value);
          value = MediaPortal.Util.Utils.RemoveParenthesis(value).Trim();
          IMDBUrl oneUrl = new IMDBUrl(absoluteUri, value, "IMDB");
          elements.Add(oneUrl);
          return;
        }
        parser.resetPosition();
        while (parser.skipToEndOfNoCase("found the following results"))
        {
          string url = string.Empty;
          string name = string.Empty;
          //<a href="/name/nm0000246/" onclick="set_args('nm0000246', 1)">Bruce Willis</a>
          if (parser.skipToStartOf("href=\"/name/"))
          {
            parser.skipToEndOf("href=\"");
            parser.extractTo("\"", ref url);
            parser.skipToEndOf(">");
            parser.extractTo("</a>", ref name);
            name = new HTMLUtil().ConvertHTMLToAnsi(name);
            name = MediaPortal.Util.Utils.RemoveParenthesis(name).Trim();
            IMDBUrl newUrl = new IMDBUrl("http://us.imdb.com" + url, name, "IMDB");
            elements.Add(newUrl);
          }
          else
          {
            parser.skipToEndOfNoCase("</a>");
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("exception for imdb lookup of {0} err:{1} stack:{2}", strURL, ex.Message, ex.StackTrace);
      }
    }
    public bool GetActorDetails(IMDB.IMDBUrl url, out IMDBActor actor)
    {
      actor = new IMDBActor();
      try
      {
        //<a name="headshot" href="photogallery"><img border="0" src="http://ia.imdb.com/media/imdb/01/I/84/36/12m.jpg" width="100" height="140" alt="Bruce Willis (I)"></a>
        string absoluteUri;
        string strBody = GetPage(url.URL, "utf-8", out absoluteUri);
        if (strBody == null)
          return false;
        if (strBody.Length == 0)
          return false;
        HTMLParser parser = new HTMLParser(strBody);
        string strThumb = string.Empty;
        string value = string.Empty;
        string value2 = string.Empty;
        if ((parser.skipToEndOf("<title>")) &&
            (parser.extractTo("</title>", ref value)))
        {
          value = new HTMLUtil().ConvertHTMLToAnsi(value);
          value = MediaPortal.Util.Utils.RemoveParenthesis(value).Trim();
          //Log.Info("Actor Name:{0}", value);
          actor.Name = value;
        }
        if (actor.Name == string.Empty)
        {
          actor.Name = url.Title;
        }
        //get picture
        if ((parser.skipToEndOf("<a name=\"headshot")) &&
            (parser.skipToEndOf("<img")) &&
            (parser.skipToEndOf("src=\"")) &&
            (parser.extractTo("\"", ref strThumb)))
        {
          //Log.Info("Actor Thumb:{0}", strThumb);
          actor.ThumbnailUrl = strThumb;
        }
        if ((parser.skipToEndOf("/OnThisDay?")) &&
            (parser.skipToEndOf(">")) &&
            (parser.extractTo("<", ref value)) &&
            (parser.skipToEndOf("/BornInYear?")) &&
            (parser.extractTo("\"", ref value2)))
        {
          //Log.Info("Actor Birth:{0} {1}", value, value2);
          actor.DateOfBirth = value + " " + value2;
        }

        if ((parser.skipToEndOf("/BornWhere?")) &&
            (parser.skipToEndOf(">")) &&
            (parser.extractTo("<", ref value)))
        {
          //Log.Info("Actor Place:{0}", value);
          actor.PlaceOfBirth = value;
        }
        //find Mini Biography
        //<dt><div class="ch">Mini biography</div></dt>
        //<dd><a href="/name/nm0000193/">Demi Moore</a> was born 1962 in Roswell, New Mexico. Her father left her mother... <a href="bio">(show more)</a></dd>
        //</dl>
        if ((parser.skipToEndOf("Mini biography")) &&
            (parser.skipToEndOf("</h5>")) &&
            (parser.extractTo("<a", ref value)) &&
            (parser.skipToEndOf("href=\"")) &&
            (parser.extractTo("\"", ref value2)))
        {
          //Log.Info("Actor Mini:{0}", value);
          //Log.Info("Actor BIO URL:{0}", value2);
          actor.MiniBiography = MediaPortal.Util.Utils.stripHTMLtags(value).Trim();
          actor.MiniBiography = HttpUtility.HtmlDecode(actor.MiniBiography);  // Remove HTML entities like &#189;

          //get complete biography
          string bioURL = absoluteUri;
          int pos = bioURL.IndexOf("?");
          if (pos > 0)
            bioURL = bioURL.Substring(0, pos);
          if (!bioURL.EndsWith("/"))
          {
            bioURL += "/";
          }
          bioURL += value2;
          //Log.Info("Bio Url:{0}", bioURL);
          string strBioBody = GetPage(bioURL, "utf-8", out absoluteUri);
          if (strBioBody != null && strBioBody.Length > 0)
          {
            HTMLParser parser1 = new HTMLParser(strBioBody);
            if (parser1.skipToEndOf("<h5>Mini biography</h5>") &&
                parser1.extractTo("</p>", ref value))
            {
              //Log.Info("Actor Bio:{0}", value);
              actor.Biography = MediaPortal.Util.Utils.stripHTMLtags(value).Trim();
              actor.Biography = HttpUtility.HtmlDecode(actor.Biography);  // Remove HTML entities like &#189;
            }
          }
        }
        if (parser.skipToEndOf("<ol>"))
        {
          string movies = string.Empty;
          if (parser.extractTo("</ol>", ref movies))
          {
            //Log.Info("Actor Movies:{0}", movies);
            parser.Content = movies;
          }
          while (parser.skipToStartOf("<li>"))
          {
            string movie = string.Empty;
            if (parser.extractTo("</li>", ref movie))
            {
              movie += "</li>";
              int start = movie.IndexOf("<i>");
              int end = movie.IndexOf("</i>");
              if ((start >= 0) && (end >= 0))
              {
                movie = movie.Substring(0, start) + movie.Substring(end + 4);
              }
              //Log.Info("Actor Movie:{0}", movie);
              HTMLParser movieParser = new HTMLParser(movie);
              string title = string.Empty;
              string episode = string.Empty;
              string strYear = string.Empty;
              string role = string.Empty;
              movieParser.skipToEndOf("<a");
              movieParser.skipToEndOf(">");
              movieParser.extractTo("</a>", ref title);
              title = HttpUtility.HtmlDecode(title);  // Remove HTML entities like &#189;
              //Log.Info("Actor Movie title:{0}", title);
              bool isTvSeries = false;
              while (movieParser.skipToEndOf("- <a"))
              {
                isTvSeries = true;
                if (movieParser.skipToEndOf(">"))
                {
                  movieParser.extractTo("</a>", ref episode);
                  episode = HttpUtility.HtmlDecode(episode);  // Remove HTML entities like &#189;
                  //Log.Info("Actor Movie episode:{0}", episode);
                }
                if (movieParser.skipToStartOf("(20") &&
                    movieParser.skipToEndOf("("))
                {
                  movieParser.extractTo(")", ref strYear);
                  //Log.Info("Actor Episode year:{0}", strYear);
                }
                else if (movieParser.skipToStartOf("(19") &&
                         movieParser.skipToEndOf("("))
                {
                  movieParser.extractTo(")", ref strYear);
                  //Log.Info("Actor Episode year:{0}", strYear);
                }
                if (movieParser.skipToEndOf(".... "))
                {
                  movieParser.extractTo("<", ref role);
                  //Log.Info("Actor Episode role:{0}", role);
                  role = role.Trim();
                  role = HttpUtility.HtmlDecode(role);  // Remove HTML entities like &#189;
                }

                int year = 0;
                try
                {
                  year = Int32.Parse(strYear);
                }
                catch (Exception)
                {
                  year = 1900;
                }
                IMDBActor.IMDBActorMovie actorMovie = new IMDBActor.IMDBActorMovie();
                actorMovie.MovieTitle = title + "-" + episode;
                actorMovie.Role = role;
                actorMovie.Year = year;
                actor.Add(actorMovie);
                //Log.Info("Actor Movie {0} as {1},{2}", actorMovie.MovieTitle, actorMovie.Role, actorMovie.Year);
              }
              if (!isTvSeries)
              {
                if (movieParser.skipToStartOf("(20") &&
                    movieParser.skipToEndOf("("))
                {
                  movieParser.extractTo(")", ref strYear);
                  //Log.Info("Actor Movie year:{0}", strYear);
                }
                else if (movieParser.skipToStartOf("(19") &&
                    movieParser.skipToEndOf("("))
                {
                  movieParser.extractTo(")", ref strYear);
                  //Log.Info("Actor Movie year:{0}", strYear);
                }
                if (movieParser.skipToEndOf(".... "))
                {
                  movieParser.extractTo("<", ref role);
                  //Log.Info("Actor Movie role:{0}", role);
                  role = role.Trim();
                }

                int year = 0;
                try
                {
                  year = Int32.Parse(strYear);
                }
                catch (Exception)
                {
                  year = 1900;
                }
                IMDBActor.IMDBActorMovie actorMovie = new IMDBActor.IMDBActorMovie();
                actorMovie.MovieTitle = title;
                actorMovie.Role = role;
                actorMovie.Year = year;
                actor.Add(actorMovie);
              }
            }

          }
        }
        return true;
      }
      catch (Exception)
      {
        //Log.Info("IMDB.GetActorDetails({0} exception:{1} {2} {3}", url.URL,ex.Message,ex.Source,ex.StackTrace);
      }
      return false;
    }


    #endregion
    // this method switches between the different databases to fetche the search result into movieDetails
    public bool GetDetails(IMDB.IMDBUrl url, ref IMDBMovie movieDetails)
    {
      try
      {
        /*
        // extract host from url, to find out which mezhod should be called
        int		iStart = url.URL.IndexOf(".")+1;
        int		iEnd = url.URL.IndexOf(".",iStart);
        if ((iStart<0) || (iEnd<0))
        {
          // could not extract hostname!
          Log.Info("Movie DB lookup GetDetails(): could not extract hostname from {0}",url.URL);
          return false;
        }
        string	strHost = url.URL.Substring(iStart,iEnd-iStart).ToUpper();*/
        switch (url.Database)
        {
          case "IMDB":
            return GetDetailsIMDB(url, ref movieDetails);
          default:
            // Script support script.csscript
            string grabberFileName = Config.GetSubFolder(Config.Dir.Base, "scripts\\imdb") + @"\" + url.Database + ".csscript";
            if (File.Exists(grabberFileName))
            {
              try
              {
                AsmHelper script = new AsmHelper(CSScriptLibrary.CSScript.Load(grabberFileName, null, false));
                IIMDBScriptGrabber grabber = (IIMDBScriptGrabber)script.CreateObject("Grabber");
                grabber.GetDetails(url, ref movieDetails);
              }
              catch (Exception ex)
              {
                Log.Info("Script garbber error GetDetails() file: {0}, message : {1}", grabberFileName, ex.Message);
                return false;
              }
              return true;
            }
            else
            {
              // unsupported database?
              Log.Error("Movie database lookup GetDetails()- database not supported: {0}", url.Database);
              return false;
            }
        }
      }
      catch (Exception ex)
      {
        Log.Error("Movie database lookup GetDetails()- exception: {0}", url.Database);
        return false;
      }
      return false;
    } // END GetDetails()


    #region IMDB
    private void FindIMDB(string strURL, int iLimit)
    {
      int iCount = 0;
      string strTitle;
      try
      {
        string absoluteUri;
        string strBody = GetPage(strURL, "utf-8", out absoluteUri);

        // Mars Warrior @ 03-sep-2004.
        // First try to find an Exact Match. If no exact match found, just look
        // for any match and add all those to the list. This narrows it down more easily...
        int iStartOfMovieList = strBody.IndexOf("Popular Titles");
        if (iStartOfMovieList < 0) iStartOfMovieList = strBody.IndexOf("Exact Matches");
        if (iStartOfMovieList < 0) iStartOfMovieList = strBody.IndexOf("Partial Matches");
        if (iStartOfMovieList < 0) iStartOfMovieList = strBody.IndexOf("Approx Matches");

        int endOfTitleList = strBody.IndexOf("Suggestions For Improving Your Results");
        if (iStartOfMovieList < 0)
        {
          int iMovieTitle = strBody.IndexOf("<title>");
          int iOverview = strBody.IndexOf("Overview");
          int iMovieGenre = strBody.IndexOf("Genre:");
          int iMoviePlot = strBody.IndexOf("Plot");

          if (iMovieTitle >= 0 && iOverview >= 0 && iMoviePlot >= 0)
          {
            int iEnd = strBody.IndexOf("<", iMovieTitle + 7);
            if (iEnd > 0)
            {
              iMovieTitle += "<title>".Length;
              strTitle = strBody.Substring(iMovieTitle, iEnd - iMovieTitle);
              strTitle = MediaPortal.Util.Utils.stripHTMLtags(strTitle);
              HTMLUtil htmlUtil = new HTMLUtil();
              htmlUtil.ConvertHTMLToAnsi(strTitle, out strTitle);
              IMDBUrl url = new IMDBUrl(strURL, strTitle + " (imdb)", "IMDB");
              elements.Add(url);
            }
          }
          return;
        }

        iStartOfMovieList += "<table>".Length;
        int iEndOfMovieList = strBody.IndexOf("Suggestions For Improving Your Results"); //strBody.IndexOf("</table>", iStartOfMovieList);

        if (iEndOfMovieList < 0)
        {
          iEndOfMovieList = strBody.Length;
        }
        if (endOfTitleList < iEndOfMovieList && endOfTitleList > iStartOfMovieList)
        {
          iEndOfMovieList = endOfTitleList;
        }
        strBody = strBody.Substring(iStartOfMovieList, iEndOfMovieList - iStartOfMovieList);
        while ((true) && (iCount < iLimit))
        {
          ////<A HREF="/Title?0167261">Lord of the Rings: The Two Towers, The (2002)</A>
          int iAHREF = strBody.IndexOf("<a href=");
          if (iAHREF >= 0)
          {
            int iEndAHREF = strBody.IndexOf("</a>");
            if (iEndAHREF >= 0)
            {
              iAHREF += "<a href=.".Length;
              string strAHRef = strBody.Substring(iAHREF, iEndAHREF - iAHREF);
              int iURL = strAHRef.IndexOf(">");
              if (iURL > 0)
              {
                strTitle = "";
                strURL = strAHRef.Substring(0, iURL);
                if (strURL[strURL.Length - 1] == '\"')
                  strURL = strURL.Substring(0, strURL.Length - 1);
                iURL++;
                int iURLEnd = strAHRef.IndexOf("<", iURL);
                if (iURLEnd > 0)
                {
                  strTitle = strAHRef.Substring(iURL, iURLEnd - iURL);
                }
                else
                  strTitle = strAHRef.Substring(iURL);

                int onclick = strURL.IndexOf(" onclick");
                if (onclick >= 0)
                  strURL = strURL.Substring(0, onclick - 1);
                strURL = String.Format("http://us.imdb.com{0}", strURL);
                HTMLUtil htmlUtil = new HTMLUtil();
                htmlUtil.ConvertHTMLToAnsi(strTitle, out strTitle);


                int endTagLength = "</a>".Length;
                int posNextTag = strBody.IndexOf("<", iEndAHREF + endTagLength);
                if (posNextTag > 0)
                {
                  string strSub = strBody.Substring(iEndAHREF + endTagLength, posNextTag - (iEndAHREF + endTagLength));
                  strTitle += strSub;
                }
                // to avoid including of &nbsp; 
                if ((strTitle.IndexOf("\n") < 0) && (strTitle.IndexOf("&nbsp;") < 0))
                {
                  IMDBUrl url = new IMDBUrl(strURL, strTitle + " (imdb)", "IMDB");
                  elements.Add(url);
                }
                iCount++;
              }
              if (iEndAHREF + 1 >= strBody.Length)
                break;
              iStartOfMovieList = iEndAHREF + 1;
              strBody = strBody.Substring(iEndAHREF + 1);
            }
            else
            {
              break;
            }
          }
          else
          {
            break;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("exception for imdb lookup of {0} err:{1} stack:{2}", strURL, ex.Message, ex.StackTrace);
      }
    }

    private bool GetDetailsIMDB(IMDB.IMDBUrl url, ref IMDBMovie movieDetails)
    {
      try
      {

        int iStart = 0;
        int iEnd = 0;
        movieDetails.Reset();
        // add databaseinfo
        movieDetails.Database = "IMDB";

        string strAbsURL;
        string strBody = GetPage(url.URL, "utf-8", out strAbsURL);
        if (strBody == null || strBody.Length == 0)
          return false;

        int iPos = strAbsURL.IndexOf("/title/");
        if (iPos > 0)
        {
          iPos += "/title/".Length;
          movieDetails.IMDBNumber = strAbsURL.Substring(iPos);
          int pos = movieDetails.IMDBNumber.IndexOf("/");
          if (pos > 0)
            movieDetails.IMDBNumber = movieDetails.IMDBNumber.Substring(0, pos);
        }

        url.Title = url.Title.Trim();
        // cut of " (imdb)"
        iEnd = url.Title.IndexOf("(");
        if (iEnd >= 0)
          movieDetails.Title = url.Title.Substring(0, iEnd);
        else
          movieDetails.Title = url.Title;
        movieDetails.Title = movieDetails.Title.Trim();
        string movieTitle = System.Web.HttpUtility.HtmlEncode(movieDetails.Title);
        int iDirectedBy = strBody.IndexOf("Director");
        int iCredits = strBody.IndexOf("Writer");
        int iGenre = strBody.IndexOf("Genre:");
        int iTagLine = strBody.IndexOf("Tagline:</h5>");
        int iPlotOutline = strBody.IndexOf("Plot Outline:</h5>");
        int iPlotSummary = strBody.IndexOf("Plot Summary:</h5>");
        int iPlot = strBody.IndexOf("<a href=\"plotsummary");
        int iImage = strBody.IndexOf("<img border=\"0\" alt=\"" + movieTitle + "\" title=\"" + movieTitle + "\" src=\"");
        if (iImage >= 0)
        {
          iImage += ("<img border=\"0\" alt=\"" + movieTitle + "\" title=\"" + movieTitle + "\" src=\"").Length;
        }
        int iRating = strBody.IndexOf("User Rating:</b>");
        int iCred = strBody.IndexOf("<table class=\"cast\">");
        int iTop = strBody.IndexOf("Top 250:");
        int iYear = strBody.IndexOf("/Sections/Years/");
        if (iYear >= 0)
        {
          iYear += "/Sections/Years/".Length;
          string strYear = strBody.Substring(iYear, 4);
          movieDetails.Year = System.Int32.Parse(strYear);
        }

        if (iDirectedBy >= 0)
          movieDetails.Director = ParseAHREFIMDB(strBody, iDirectedBy, url.URL).Trim();

        if (iCredits >= 0)
          movieDetails.WritingCredits = ParseAHREFIMDB(strBody, iCredits, url.URL).Trim();

        if (iGenre >= 0)
          movieDetails.Genre = ParseGenresIMDB(strBody, iGenre, url.URL).Trim();

        if (iRating >= 0) // and votes
        {
          iRating += "User Rating:</b>".Length;
          iStart = strBody.IndexOf("<b>", iRating);
          if (iStart >= 0)
          {
            iStart += "<b>".Length;
            iEnd = strBody.IndexOf("/", iStart);

            // set rating
            string strRating = strBody.Substring(iStart, iEnd - iStart);
            if (strRating != string.Empty)
              strRating = strRating.Replace('.', ',');
            try
            {
              movieDetails.Rating = (float)System.Double.Parse(strRating);
              if (movieDetails.Rating > 10.0f)
                movieDetails.Rating /= 10.0f;
            }
            catch (Exception)
            {
            }

            if (movieDetails.Rating != 0.0f)
            {
              // now, votes
              movieDetails.Votes = "0";
              iStart = strBody.IndexOf("(", iEnd + 2);
              if (iStart > 0)
              {
                iEnd = strBody.IndexOf(" votes</a>)", iStart);
                if (iEnd > 0)
                {
                  iStart += "(<a href=\"ratings\">".Length; // skip the parantese and link before votes
                  movieDetails.Votes = strBody.Substring(iStart, iEnd - iStart).Trim();
                }
              }
            }
          }
        }

        if (iTop >= 0) // top rated movie :)
        {
          iTop += "top 250:".Length + 2; // jump space and #
          iEnd = strBody.IndexOf("</a>", iTop);
          string strTop = strBody.Substring(iTop, iEnd - iTop);
          movieDetails.Top250 = System.Int32.Parse(strTop);
        }
        if (iTagLine >= 0)
        {
          iTagLine += "Tagline:</h5>".Length;
          iEnd = strBody.IndexOf("<", iTagLine);
          movieDetails.TagLine = strBody.Substring(iTagLine, iEnd - iTagLine).Trim();
          movieDetails.TagLine = MediaPortal.Util.Utils.stripHTMLtags(movieDetails.TagLine);
          movieDetails.TagLine = HttpUtility.HtmlDecode(movieDetails.TagLine);  // Remove HTML entities like &#189;
        }

        if (iPlotOutline < 0)
        {
          if (iPlotSummary > 0)
          {
            iPlotSummary += "Plot Summary:</h5>".Length;
            iEnd = strBody.IndexOf("<", iPlotSummary);
            movieDetails.PlotOutline = strBody.Substring(iPlotSummary, iEnd - iPlotSummary).Trim();
            movieDetails.PlotOutline = MediaPortal.Util.Utils.stripHTMLtags(movieDetails.PlotOutline);
            movieDetails.PlotOutline = HttpUtility.HtmlDecode(movieDetails.PlotOutline);  // remove HTML entities
          }
        }
        else
        {
          iPlotOutline += "Plot Outline:</h5>".Length;
          iEnd = strBody.IndexOf("<", iPlotOutline);
          movieDetails.PlotOutline = strBody.Substring(iPlotOutline, iEnd - iPlotOutline).Trim();
          movieDetails.PlotOutline = MediaPortal.Util.Utils.stripHTMLtags(movieDetails.PlotOutline);
          movieDetails.PlotOutline = HttpUtility.HtmlDecode(movieDetails.PlotOutline);  // remove HTML entities
          movieDetails.Plot = movieDetails.PlotOutline.Trim();
          movieDetails.Plot = HttpUtility.HtmlDecode(movieDetails.Plot);  // remove HTML entities
        }

        if (iImage >= 0)
        {
          iEnd = strBody.IndexOf("\"", iImage);
          //movieDetails.ThumbURL = strBody.Substring(iImage, iEnd - iImage).Trim();
        }

        //plot
        if (iPlot >= 0)
        {
          string strPlotURL = url.URL + "plotsummary";
          try
          {
            string absoluteUri;
            string strPlotHTML = GetPage(strPlotURL, "utf-8", out absoluteUri);

            if (0 != strPlotHTML.Length)
            {

              int iPlotStart = strPlotHTML.IndexOf("<p class=\"plotpar\">");
              if (iPlotStart >= 0)
              {
                iPlotStart += "<p class=\"plotpar\">".Length;

                int iPlotEnd = strPlotHTML.IndexOf("<i>", iPlotStart); // ends with <i> for person who wrote it or
                if (iPlotEnd < 0) iPlotEnd = strPlotHTML.IndexOf("</p>", iPlotStart); // </p> for end of paragraph

                if (iPlotEnd >= 0)
                {
                  movieDetails.Plot = strPlotHTML.Substring(iPlotStart, iPlotEnd - iPlotStart);
                  movieDetails.Plot = MediaPortal.Util.Utils.stripHTMLtags(movieDetails.Plot);
                  movieDetails.Plot = HttpUtility.HtmlDecode(movieDetails.Plot);  // remove HTML entities
                }
              }
            }
          }
          catch (Exception ex)
          {
            Log.Error("exception for imdb lookup of {0} err:{1} stack:{2}", strPlotURL, ex.Message, ex.StackTrace);
          }
        }

        //cast
        string RegCastBlock = "<table class=\"cast\">.*?</table>";
        string RegActorAndRole = "td class=\"nm\"><a href=./name.*?>(?<actor>.*?)</a><.*?<td class=\"char\">(?<role>.*?)<";

        Match castBlock = Regex.Match(strBody, RegCastBlock);

        // These are some fallback methods to find the block with the cast, in case something changes on IMDB, these may work reasonably well anyway...
        if (!castBlock.Success)
          castBlock = Regex.Match(strBody, @"redited\scast.*?</table>");
        if (!castBlock.Success)
          castBlock = Regex.Match(strBody, @"first\sbilled\sonly.*?</table>");
        if (!castBlock.Success)
          castBlock = Regex.Match(strBody, @"redited\scast.*?more");
        if (!castBlock.Success)
          castBlock = Regex.Match(strBody, @"first\sbilled\sonly.*?more");

        string strCastBlock = castBlock.Value;

        MatchCollection mc = Regex.Matches(strCastBlock, RegActorAndRole);
        string strActor = string.Empty;
        string strRole = string.Empty;

        foreach (Match m in mc)
        {
          strActor = string.Empty;
          strActor = m.Groups["actor"].Value;
          strActor = MediaPortal.Util.Utils.stripHTMLtags(strActor).Trim();
          strActor = HttpUtility.HtmlDecode(strActor);

          strRole = string.Empty;
          strRole = m.Groups["role"].Value;
          strRole = MediaPortal.Util.Utils.stripHTMLtags(strRole).Trim();
          strRole = HttpUtility.HtmlDecode(strRole);

          movieDetails.Cast += strActor;
          if (strRole != string.Empty)
            movieDetails.Cast += " as " + strRole;

          movieDetails.Cast += "\n";
        }

        int iRunTime = strBody.IndexOf("Runtime:");
        if (iRunTime > 0)
        {
          iRunTime += "Runtime:</h5>".Length;
          string runtime = "";
          while (!Char.IsDigit(strBody[iRunTime]) && iRunTime + 1 < strBody.Length)
            iRunTime++;
          if (iRunTime < strBody.Length)
          {
            while (Char.IsDigit(strBody[iRunTime]) && iRunTime + 1 < strBody.Length)
            {
              runtime += strBody[iRunTime];
              iRunTime++;
            }
            try
            {
              movieDetails.RunTime = Int32.Parse(runtime);
            }
            catch (Exception) { }
          }
        }

        int mpaa = strBody.IndexOf("MPAA</a>:</h5>");
        if (mpaa > 0)
        {
          mpaa += "MPAA</a>:</h5>".Length;
          int mpaaEnd = strBody.IndexOf("</div>", mpaa);
          if (mpaaEnd > 0)
          {
            movieDetails.MPARating = strBody.Substring(mpaa, mpaaEnd - mpaa);
          }
        }


        return true;
      }
      catch (Exception ex)
      {
        Log.Error("exception for imdb lookup of {0} err:{1} stack:{2}", url.URL, ex.Message, ex.StackTrace);
      }
      return false;
    }

    string ParseAHREFIMDB(string strBody, int iahref, string strURL)
    {
      int iStart = strBody.IndexOf("<a href=\"", iahref);
      if (iStart < 0)
        iStart = strBody.IndexOf("<A HREF=\"", iahref);
      if (iStart < 0)
        return "";

      int iEnd = strBody.IndexOf("</a>", iStart);
      if (iEnd < 0)
        iEnd = strBody.IndexOf("</A>", iStart);
      if (iEnd < 0)
        return "";

      iStart += "<a href=\"".Length;
      int iSep = strBody.IndexOf(">", iStart);
      string strurl = strBody.Substring(iStart, (iSep - iStart) - 1);
      iSep++;
      string strTitle = strBody.Substring(iSep, iEnd - iSep);
      strTitle = MediaPortal.Util.Utils.stripHTMLtags(strTitle);
      HTMLUtil htmlUtil = new HTMLUtil();
      htmlUtil.ConvertHTMLToAnsi(strTitle, out strTitle);
      strTitle = strTitle.Trim();
      return strTitle.Trim();

    }
    string ParseGenresIMDB(string strBody, int iGenre, string url)
    {
      string strTmp;
      string strTitle = "";
      string strHRef = strBody.Substring(iGenre);
      int iSlash = strHRef.IndexOf(" / ");
      int iEnd = 0;
      int iStart = 0;
      if (iSlash >= 0)
      {
        int iRealEnd = strHRef.IndexOf(">more<");
        if (iRealEnd < 0)
          iRealEnd = strHRef.IndexOf("</div>");
        while (iSlash < iRealEnd)
        {
          iStart = iEnd + 2;
          iEnd = iSlash;
          int iLen = iEnd - iStart;
          if (iLen < 0)
            break;
          strTmp = strHRef.Substring(iStart, iLen);
          strTitle = strTitle + ParseAHREFIMDB(strTmp, 0, "") + " / ";

          iSlash = strHRef.IndexOf(" / ", iEnd + 2);
          if (iSlash < 0)
            iSlash = iRealEnd;
        }
      }
      // last genre
      iEnd += 2;
      strTmp = strHRef.Substring(iEnd);
      strTitle = strTitle + ParseAHREFIMDB(strTmp, 0, "");
      HTMLUtil htmlUtil = new HTMLUtil();
      htmlUtil.ConvertHTMLToAnsi(strTitle, out strTitle);

      return strTitle;
    }

    #endregion

    #region CSPV
    private void FindCspv(string strURL, int iLimit)
    {
      int iCount = 0;
      string strTitle = string.Empty;
      string strOTitle = string.Empty;
      try
      {
        string absoluteUri;
        string strBody = GetPage(strURL, "ISO-8859-1", out absoluteUri);

        HTMLParser parser = new HTMLParser(strBody);
        parser.skipToEndOf("Tal·latok:");
        while ((parser.Position < strBody.Length) && (iCount < iLimit))
        {
          if (parser.skipToEndOfNoCase("href=\"") &&
              parser.extractTo("\"", ref strURL))
          {
            if (strURL.Contains("Adatlap_film"))
            {
              if (parser.skipToEndOfNoCase(">") &&
                  parser.extractTo("</a>", ref strTitle))
              {
                if (parser.skipToEndOfNoCase("<span class=\"date\">") &&
                    parser.skipToEndOfNoCase(",") &&
                    parser.extractTo(")", ref strOTitle))
                {
                }
                HTMLUtil htmlUtil = new HTMLUtil();
                htmlUtil.ConvertHTMLToAnsi(strTitle, out strTitle);
                htmlUtil.ConvertHTMLToAnsi(strOTitle, out strOTitle);
                IMDBUrl url = new IMDBUrl(strURL, strTitle + " (cspv)", "CSPV");
                url.IMDBURL = "http://us.imdb.com/Tsearch?title=" + strOTitle.Replace(" ", "+");
                elements.Add(url);
                iCount++;
              }
            }
          }
          else
          {
            break;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("exception for cspv lookup of {0} err:{1} stack:{2}", strURL, ex.Message, ex.StackTrace);
      }
    }

    private bool GetDetailsCspv(IMDB.IMDBUrl url, ref IMDBMovie movieDetails)
    {
      try
      {
        movieDetails.Reset();
        // add databaseinfo
        movieDetails.Database = "CSPV";
        HTMLUtil htmlUtil = new HTMLUtil();
        string strAbsURL;
        url.URL = url.URL.Replace("&amp;", "&");
        string strBody = GetPage(url.URL, "ISO-8859-1", out strAbsURL);
        if (strBody == null || strBody.Length == 0)
          return false;

        HTMLParser parser = new HTMLParser(strBody);
        if (parser.skipToEndOfNoCase("middle-left\">"))
        {
          string strTitle = string.Empty;
          string strYear = string.Empty;
          string runtime = string.Empty;
          string strDirector = string.Empty;
          string strWriting = string.Empty;
          string strCast = string.Empty;
          string strGenre = string.Empty;
          string strPlot = string.Empty;
          string strNumber = string.Empty;
          string strThumb = string.Empty;
          string strRating = string.Empty;
          string strVotes = string.Empty;
          string strMpaa = string.Empty;
          parser.extractTo("</h1>", ref strTitle);
          strTitle = MediaPortal.Util.Utils.stripHTMLtags(strTitle);
          movieDetails.Title = strTitle;

          if (parser.skipToEndOfNoCase("<p>") &&
              parser.extractTo(",", ref strGenre))
          {
            movieDetails.Genre = strGenre;
          }

          if (parser.extractTo("-", ref strYear))
          {
            try
            {
              movieDetails.Year = System.Int32.Parse(strYear.Trim());
            }
            catch (Exception)
            {
              movieDetails.Year = 1970;
            }
          }
          if (parser.skipToEndOfNoCase("<p class=\"directors\">") &&
              parser.skipToEndOfNoCase(">") &&
              parser.extractTo("</a>", ref strDirector))
          {
            movieDetails.Director = strDirector;
          }
          if (parser.skipToEndOfNoCase("<p class=\"actors\">") &&
              parser.extractTo("</p>", ref strCast))
          {
            strCast = HTMLParser.removeHtml(strCast.Replace("</a><br />", Environment.NewLine));
            movieDetails.Cast = strCast;
          }
          if (parser.skipToEndOfNoCase("VIDEN:</h2>") &&
           parser.skipToEndOfNoCase("<p>") &&
           parser.extractTo("</p>", ref strPlot))
          {
            strPlot = HTMLParser.removeHtml(strPlot);
            movieDetails.Plot = strPlot;
          }

        }
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("exception for CSPV lookup of {0} err:{1} stack:{2}", url.URL, ex.Message, ex.StackTrace);
      }
      return false;
    }
    #endregion
  } // END class IMDB


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

}// END namespace
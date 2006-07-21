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
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;
using MediaPortal.GUI.Library;
using System.Net;
using System.Web;
using System.Threading;
using System.ComponentModel;
using MediaPortal.Music.Database;


namespace MediaPortal.MusicVideos.Database
{
  public class YahooUtil
  {

    private static YahooUtil instance;
    private Hashtable loEscapes = new Hashtable();
    private static bool _workerCompleted = true;
    private string HTMLdownload;										   // Hold HTML for parsing or misc


    public Regex moSongRegex = new Regex(":play\\w+\\((\\d+)\\S+ class=\"listheader\" title=\"([^\"]*)");
    public Regex moSearchSongRegex = new Regex(":play\\w+\\((\\d+)\\S+ title=\"([^\"]*)");
    //private Regex moArtistRegex = new Regex("/ar-(\\d+)-+\\S+\" title=\"([^>\"]*)");
    public Regex moArtistRegex = new Regex("<td class=\"listitem\"><\\ba href=\"http://[\\.\\w]+/ar-(\\d*)-+[^\"]*\" title=\"([^\"]*)\\b*|\\bfont title=\"([^\"]*)\\b*");
    private YahooUtil()
    {
      init();
    }
    public static YahooUtil getInstance()
    {
      if (instance == null)
      {
        instance = new YahooUtil();
      }
      return instance;
    }
    //public void setYahooSites(List<YahooSite> foYahooSiteList)
    //{
    //    moYahooSiteTable = new Dictionary<string,YahooSite>();
    //    foreach (YahooSite loSite in foYahooSiteList)
    //    {
    //        moYahooSiteTable.Add(loSite.countryName, loSite);
    //if(loSite.)
    //  }
    //moYahooSiteTable.
    //}

    public YahooSite getYahooSiteById(string fsCountryId)
    {
      YahooSite loSite = new YahooSite();
      YahooSettings loSettings = YahooSettings.getInstance();
      foreach (YahooSite loTempSite in loSettings.moYahooSiteTable.Values)
      {
        if (loTempSite.countryId == fsCountryId)
        {
          loSite = loTempSite;
          break;
        }
      }
      return loSite;
    }
    public YahooSite getYahooSite(string fsCountryName)
    {
      YahooSettings loSettings = YahooSettings.getInstance();
      return (YahooSite)loSettings.moYahooSiteTable[fsCountryName];
    }
    private string getVideoUrl(YahooVideo foVideo, string fsBitRate)
    {
      YahooSite loSite = getYahooSite(foVideo.countryId);
      string lsVideoUrl;
      //get the video hash here
      string lsVideoHash = getMediaHash(foVideo.songId, loSite.countryId);
      Log.Write("Hash ={0}", lsVideoHash);
      if (loSite.countryId == "us")
      {
        lsVideoUrl = "http://launchtoday.launch.yahoo.com/player/medialog.asp?vid=" + foVideo.songId;
      }
      else
      {
        lsVideoUrl = "http://launchtoday." + loSite.countryId + ".launch.yahoo.com/player/medialog.asp?vid=" + foVideo.songId;
      }
      //lsVideoUrl += "&bw=" + CURRENT_BR + "&mf=1&pid=505&ps=0&p1=2&p2=21&p3=2&rpid=35&pv=10&bp=Windows%2520NT&csid=791020104&uid=1886812234&pguid=einsJELEt4VkQCKMov00bg&etid=0&uguid=3e5u3891dirro&fcv=";
      lsVideoUrl += "&bw=" + fsBitRate + "&mf=1&pid=505&ps=0&p1=2&p2=21&p3=2&rpid=35&pv=10&bp=Windows%2520NT&csid=791020104&uid=1886812234&pguid=einsJELEt4VkQCKMov00bg&etid=0&uguid=3e5u3891dirro&fcv=";
      lsVideoUrl += "&mh=" + lsVideoHash;
      lsVideoUrl += "&z=ms.asx";
      Log.Write("Using BitRate:{0}", fsBitRate);
      Log.Write("url={0}", lsVideoUrl);

      string HTMLdownload = getHTMLData(lsVideoUrl);

      if (HTMLdownload.IndexOf("makeplaylist.dll") > -1)
      {
        //get the mms url
        HTMLdownload = getHTMLData(HTMLdownload);
      }
      else
      {
        Console.WriteLine("MMS url lnot found");
      }
      return HTMLdownload;

    }
    public string getHTMLData(string fsURL)
    {
      BackgroundWorker worker = new BackgroundWorker();
      _workerCompleted = false;
      worker.DoWork += new DoWorkEventHandler(DownloadWorker);
      worker.RunWorkerAsync(fsURL);

      using (WaitCursor cursor = new WaitCursor())
      {
        while (_workerCompleted == false)
          GUIWindowManager.Process();
      }
      return HTMLdownload;
    }
    private string downloadHtml(String fsUrl)
    {
      byte[] HTMLbuffer;
      String lsURL = fsUrl;
      WebClient loWebClient = new WebClient();
      loWebClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
      HTMLbuffer = loWebClient.DownloadData(lsURL);

      HTMLdownload = Encoding.UTF8.GetString(HTMLbuffer);
      HTMLdownload = convertHtmlToText(HTMLdownload);
      return HTMLdownload;

    }
    public void DownloadWorker(object sender, DoWorkEventArgs e)
    {

      HTMLdownload = downloadHtml((String)e.Argument);
      _workerCompleted = true;
      //return HTMLdownload;
    }
    public string getMediaHash(string fsVideoId, string fsCountryId)
    {
      string lsHashUrl;
      if (fsCountryId == "us")
      {
        lsHashUrl = "http://launchtoday.launch.yahoo.com/player/default.asp?cid=613&vid=" + fsVideoId + "&sx=default.xml&ps=&bw=&fs=&tw=calmv&vo=";

      }
      else
      {
        lsHashUrl = "http://launchtoday." + fsCountryId + ".launch.yahoo.com/player/default.asp?cid=613&vid=" + fsVideoId + "&sx=default.xml&ps=&bw=&fs=&tw=calmv&vo=";
      }

      //HttpWebResponse webresponse = null;
      string lsVideoHash = "";
      try
      {

        WebClient loClient1 = new WebClient();

        string lsCookie = "LYC=l_v=2&l_lv=9&l_l=p2e34hp&l_s=zq40yquvwq3vu3w30suqwsszryzzwwwu&l_lid=1ba4l1m&l_r=8s&l_lc=0_1_64_0_-1&l_gv=ngupt&l_um=0_1_0_0_0&l_mv=310_100; B=9e0q77l1c33ef&b=2; Q=q1=Q0qGuBUAoKBwcA--&q2=Q0pEsg--; F=a=0BzJn5QsvTO3rLVlvWVAcfM4_SKGNXbMaDwbYSG4XHl490MT9VDJwQA9vY9D&b=vXKL; U=mt=YuEA0Z2MhYpEVeygH0pyuUy5Txv4.1ZC7ytO&ux=T/vXDB&un=8h0v1t0p1r28v; todayIntro=1%3B; YMRAD=1201477343*0_0_102_1_0_58_1_0_559_1_0_487_1_0_571_1; C=mg=1; FPB=clud6jt40118pe6c; MVUserInfo3=355_10; l%5FPD3=124; PH=phl=5JD_YwTPsMe2QtHozdftiUKjT7aZpmR9JD0twYWWLV_hdADnCA--; I=ir=f7&in=61d66adc&i1=BhABqH; LYS=l_fh=0&l_vo=myla; HP=1; Y=v=1&n=dg0jitu5o5o3l&l=p2e34hp/o&p=m2h0d1n413000300&r=8s&lg=us&intl=us&np=1; T=z=2wiYDB223YDBfUzqmbHT.GqNDM2BjY3MzBPMjFOTjU-&a=QAE&sk=DAAfFyVJKSVY.I&d=c2wBTXpReEFURXdORGM0TlRZNU9USS0BYQFRQUUBdGlwAU9xVFp1QQF6egEyd2lZREJnV0E-; playerFullVersion=10.0.0.3646; PVL=2157574_2155908_2164174_16215423_22669575_22920551_15726467_2157084_24193964_23759637_2160653_8659755_24566123_2171468_8676023_2144758_2167821_22725021_20835316_23355378_2157673_17042678_2155265_22420117|26_34_34_26_26_26_34_26_26_26_26_33_26_26_33_34_26_26_26_26_26_33_26_26; pmmczSC9D78OQONK1GM2B78CVG78KJ4=2; pmmcBSC9D78OQONK1GM2B78CVG78KJ4=0; PVL=";
        loClient1.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
        loClient1.Headers.Add("Cookie", lsCookie);
        byte[] buffer1 = loClient1.DownloadData(lsHashUrl);
        string lsHtml = Encoding.ASCII.GetString(buffer1);


        Regex loHashRegex = new Regex("var mediaHash = '(.+?)';");
        MatchCollection loMc = loHashRegex.Matches(lsHtml);

        if (loMc.Count > 0)
        {
          lsVideoHash = loMc[0].Groups[1].Value;
          //Console.WriteLine();
        }
        else
        {
          lsVideoHash = "";

        }
      }
      catch (Exception e)
      {
        Log.Write(e);
      }

      return lsVideoHash;
    }
    public List<YahooVideo> getVideoList(string fsHtml, string fsCountryId, Regex foArtistRegex, Regex foSongRegex)
    //public ArrayList getVideoList(string fsHtml, string fsCountryId)
    {
      List<YahooVideo> loVideoList = new List<YahooVideo>();

      Log.Write("in");
      YahooVideo loVideoInfo;
      GroupCollection loSongGrpCol = null;
      GroupCollection loArtistGrpCol = null;
      MatchCollection loSongMatches = null;
      MatchCollection loArtistMatches = null;
      // YahooSite loSite = (YahooSite)moYahooSiteTable[btncountry.SelectedLabel];
      // string countryId = loSite.countryId;
      loArtistMatches = foArtistRegex.Matches(fsHtml);
      loSongMatches = foSongRegex.Matches(fsHtml);


      if (loArtistMatches.Count == loSongMatches.Count)
      {
        // Loop through the match collection to retrieve all 
        // matches and positions
        for (int i = 0; i < loArtistMatches.Count; i++)
        {
          loVideoInfo = new YahooVideo();
          loSongGrpCol = loSongMatches[i].Groups;
          loArtistGrpCol = loArtistMatches[i].Groups;
          loVideoInfo.songId = loSongGrpCol[1].Value;
          loVideoInfo.songName = loSongGrpCol[2].Value;
          loVideoInfo.artistId = loArtistGrpCol[1].Value;
          loVideoInfo.countryId = fsCountryId;
          if (loArtistGrpCol[2].Value != "")
          {
            loVideoInfo.artistName = loArtistGrpCol[2].Value;
          }
          else if (loArtistGrpCol[3].Value != "")
          {
            loVideoInfo.artistName = loArtistGrpCol[3].Value;
          }
          else
          {
            loVideoInfo.artistName = " ";
          }


          //reformat the song name
          loVideoInfo.songName = loVideoInfo.songName.Replace("Live@LAUNCH Exclusive Performance", "LIVE");
          loVideoInfo.songName = loVideoInfo.songName.Replace("@LAUNCH Exclusive Performance", "LIVE");
          loVideoInfo.songName = loVideoInfo.songName.Replace("LAUNCH Exclusive Performance", "LIVE");
          loVideoInfo.songName = loVideoInfo.songName.Replace("LAUNCH Exclusive Interview", "Interview");

          loVideoList.Add(loVideoInfo);
        }
      }
      else
      {
        Log.Write("Number of artist found doesn't equal number of songs.Artist count={0} song count={1}", loArtistMatches.Count, loSongMatches.Count);
      }

      return loVideoList;
    }
    public string getVideoMMSUrl(YahooVideo foVideo, string fsBitRate)
    {

      YahooSite loSite = getYahooSiteById(foVideo.countryId);
      string lsVideoUrl;
      //get the video hash here
      string lsVideoHash = getMediaHash(foVideo.songId, loSite.countryId);
      Log.Write("Hash ={0}", lsVideoHash);
      if (loSite.countryId == "us")
      {
        lsVideoUrl = "http://launchtoday.launch.yahoo.com/player/medialog.asp?vid=" + foVideo.songId;
      }
      else
      {
        lsVideoUrl = "http://launchtoday." + loSite.countryId + ".launch.yahoo.com/player/medialog.asp?vid=" + foVideo.songId;
      }
      //lsVideoUrl += "&bw=" + CURRENT_BR + "&mf=1&pid=505&ps=0&p1=2&p2=21&p3=2&rpid=35&pv=10&bp=Windows%2520NT&csid=791020104&uid=1886812234&pguid=einsJELEt4VkQCKMov00bg&etid=0&uguid=3e5u3891dirro&fcv=";
      lsVideoUrl += "&bw=" + fsBitRate + "&mf=1&pid=505&ps=0&p1=2&p2=21&p3=2&rpid=35&pv=10&bp=Windows%2520NT&csid=791020104&uid=1886812234&pguid=einsJELEt4VkQCKMov00bg&etid=0&uguid=3e5u3891dirro&fcv=";
      lsVideoUrl += "&mh=" + lsVideoHash;
      lsVideoUrl += "&z=ms.asx";
      Log.Write("Using BitRate:{0}", fsBitRate);
      Log.Write("url={0}", lsVideoUrl);

      HTMLdownload = downloadHtml(lsVideoUrl);
      //HTMLdownload = this.HTMLdownload;

      if (HTMLdownload.IndexOf("makeplaylist.dll") > -1)
      {
        //get the mms url
        //HTMLdownload = getHTMLData(HTMLdownload);
        HTMLdownload = downloadHtml(HTMLdownload);
        //HTMLdownload = this.HTMLdownload;
      }
      else
      {
        Console.WriteLine("MMS url lnot found");
      }
      return HTMLdownload;


    }
    public void getArtistImage(String fsArtistId, YahooSite foSite)
    {
      String lsArtistUrl = "http://music.yahoo.com/ar-" + fsArtistId + "-bio";

      String lsHtml = getHTMLData(lsArtistUrl);
      Regex loRegex = new Regex("<td width=\"300\"><img src=\"([^\"]*)");
      Match loMatch = loRegex.Match(lsHtml);
      String lsImageUrl = loMatch.Groups[1].Value;
      WebClient wc = new WebClient();
      //moviename = moviename.Replace(":", "-");
      wc.DownloadFile(lsImageUrl, @"thumbs\MPTemp -" + fsArtistId + ".jpg");
    }
    private string convertHtmlToText(String fsHtml)
    {
      foreach (string lsKey in loEscapes.Keys)
      {

        fsHtml = fsHtml.Replace(lsKey, loEscapes[lsKey].ToString());

      }
      return fsHtml;

    }
    private void init()
    {
      //loEsc"&quot;",       "\"");    //# quote
      loEscapes.Add("&amp;", "&");    //# ampersand
      loEscapes.Add("&lt;", "<");    //# less than
      loEscapes.Add("&gt;", ">");    //# greater than
      loEscapes.Add("&emsp;", "\x80");//# em space (HTML 2.0)
      loEscapes.Add("&sbquo;", "\x82");//# single low-9 (bottom) quotation mark (U+201A)
      loEscapes.Add("&fnof;", "\x83");//# Florin or Guilder (currency) (U+0192)
      loEscapes.Add("&bdquo;", "\x84");//# double low-9 (bottom) quotation mark (U+201E)
      loEscapes.Add("&hellip;", "\x85");//# horizontal ellipsis (U+2026)
      loEscapes.Add("&dagger;", "\x86");//# dagger (U+2020)
      loEscapes.Add("&Dagger;", "\x87");//# double dagger (U+2021)
      loEscapes.Add("&circ;", "\x88");//# modifier letter circumflex accent
      loEscapes.Add("&permil;", "\x89");//# per mill sign (U+2030)
      loEscapes.Add("&Scaron;", "\x8A");//# latin capital letter S with caron (U+0160)
      loEscapes.Add("&lsaquo;", "\x8B");//# left single angle quotation mark (U+2039)
      loEscapes.Add("&OElig;", "\x8C");//# latin capital ligature OE (U+0152)
      loEscapes.Add("&diams;", "\x8D");//# diamond suit (U+2666)
      loEscapes.Add("&clubs;", "\x8E");//# club suit (U+2663)
      loEscapes.Add("&hearts;", "\x8F");//# heart suit (U+2665)
      loEscapes.Add("&spades;", "\x90");//# spade suit (U+2660)
      loEscapes.Add("&lsquo;", "\x91");//# left single quotation mark (U+2018)
      loEscapes.Add("&rsquo;", "\x92");//# right single quotation mark (U+2019)
      loEscapes.Add("&ldquo;", "\x93");//# left double quotation mark (U+201C)
      loEscapes.Add("&rdquo;", "\x94");//# right double quotation mark (U+201D)
      loEscapes.Add("&endash;", "\x96");//# dash the width of ensp (Lynx)
      loEscapes.Add("&ndash;", "\x96");//# dash the width of ensp (HTML 2.0)
      loEscapes.Add("&emdash;", "\x97");//# dash the width of emsp (Lynx)
      loEscapes.Add("&mdash;", "\x97");//# dash the width of emsp (HTML 2.0)
      loEscapes.Add("&tilde;", "\x98");//# small tilde
      loEscapes.Add("&trade;", "\x99");//# trademark sign (HTML 2.0)
      loEscapes.Add("&scaron;", "\x9A");//# latin small letter s with caron (U+0161)
      loEscapes.Add("&rsaquo;", "\x9B");//# right single angle quotation mark (U+203A)
      loEscapes.Add("&oelig;", "\x9C");//# latin small ligature oe (U+0153)
      loEscapes.Add("&Yuml;", "\x9F");//# latin capital letter Y with diaeresis (U+0178)
      loEscapes.Add("&ensp;", "\xA0");//# en space (HTML 2.0)
      loEscapes.Add("&thinsp;", "\xA0");//# thin space (Lynx)
      loEscapes.Add("&nbsp;", "\xA0");//# non breaking space
      loEscapes.Add("&iexcl;", "\xA1");//# inverted exclamation mark
      loEscapes.Add("&cent;", "\xA2");//# cent (currency)
      loEscapes.Add("&pound;", "\xA3");//# pound sterling (currency)
      loEscapes.Add("&curren;", "\xA4");//# general currency sign (currency)
      loEscapes.Add("&yen;", "\xA5");//# yen (currency)
      loEscapes.Add("&brkbar;", "\xA6");//# broken vertical bar (Lynx)
      loEscapes.Add("&brvbar;", "\xA6");//# broken vertical bar
      loEscapes.Add("&sect;", "\xA7");//# section sign
      loEscapes.Add("&die;", "\xA8");//# spacing dieresis (Lynx)
      loEscapes.Add("&uml;", "\xA8");//# spacing dieresis
      loEscapes.Add("&copy;", "\xA9");//# copyright sign
      loEscapes.Add("&ordf;", "\xAA");//# feminine ordinal indicator
      loEscapes.Add("&laquo;", "\xAB");//# angle quotation mark, left
      loEscapes.Add("&not;", "\xAC");//# negation sign
      loEscapes.Add("&shy;", "\xAD");//# soft hyphen
      loEscapes.Add("&reg;", "\xAE");//# circled R registered sign
      loEscapes.Add("&hibar;", "\xAF");//# spacing macron (Lynx)
      loEscapes.Add("&macr;", "\xAF");//# spacing macron
      loEscapes.Add("&deg;", "\xB0");//# degree sign
      loEscapes.Add("&plusmn;", "\xB1");//# plus-or-minus sign
      loEscapes.Add("&sup2;", "\xB2");//# superscript 2
      loEscapes.Add("&sup3;", "\xB3");//# superscript 3
      loEscapes.Add("&acute;", "\xB4");//# spacing acute
      loEscapes.Add("&micro;", "\xB5");//# micro sign
      loEscapes.Add("&para;", "\xB6");//# paragraph sign
      loEscapes.Add("&middot;", "\xB7");//# middle dot
      loEscapes.Add("&cedil;", "\xB8");//# spacing cedilla
      loEscapes.Add("&sup1;", "\xB9");//# superscript 1
      loEscapes.Add("&ordm;", "\xBA");//# masculine ordinal indicator
      loEscapes.Add("&raquo;", "\xBB");//# angle quotation mark, right
      loEscapes.Add("&frac14;", "\xBC");//# fraction 1/4
      loEscapes.Add("&frac12;", "\xBD");//# fraction 1/2
      loEscapes.Add("&frac34;", "\xBE");//# fraction 3/4
      loEscapes.Add("&iquest;", "\xBF");//# inverted question mark
      loEscapes.Add("&Agrave;", "\xC0");//# capital A, grave accent
      loEscapes.Add("&Aacute;", "\xC1");//# capital A, acute accent
      loEscapes.Add("&Acirc;", "\xC2");//# capital A, circumflex accent
      loEscapes.Add("&Atilde;", "\xC3");//# capital A, tilde
      loEscapes.Add("&Auml;", "\xC4");//# capital A, dieresis or umlaut mark
      loEscapes.Add("&Aring;", "\xC5");//# capital A, ring
      loEscapes.Add("&AElig;", "\xC6");//# capital AE diphthong (ligature)
      loEscapes.Add("&Ccedil;", "\xC7");//# capital C, cedilla
      loEscapes.Add("&Egrave;", "\xC8");//# capital E, grave accent
      loEscapes.Add("&Eacute;", "\xC9");//# capital E, acute accent
      loEscapes.Add("&Ecirc;", "\xCA");//# capital E, circumflex accent
      loEscapes.Add("&Euml;", "\xCB");//# capital E, dieresis or umlaut mark
      loEscapes.Add("&Igrave;", "\xCC");//# capital I, grave accent
      loEscapes.Add("&Iacute;", "\xCD");//# capital I, acute accent
      loEscapes.Add("&Icirc;", "\xCE");//# capital I, circumflex accent
      loEscapes.Add("&Iuml;", "\xCF");//# capital I, dieresis or umlaut mark
      loEscapes.Add("&Dstrok;", "\xD0");//# capital Eth, Icelandic (Lynx)
      loEscapes.Add("&ETH;", "\xD0");//# capital Eth, Icelandic
      loEscapes.Add("&Ntilde;", "\xD1");//# capital N, tilde
      loEscapes.Add("&Ograve;", "\xD2");//# capital O, grave accent
      loEscapes.Add("&Oacute;", "\xD3");//# capital O, acute accent
      loEscapes.Add("&Ocirc;", "\xD4");//# capital O, circumflex accent
      loEscapes.Add("&Otilde;", "\xD5");//# capital O, tilde
      loEscapes.Add("&Ouml;", "\xD6");//# capital O, dieresis or umlaut mark
      loEscapes.Add("&times;", "\xD7");//# multiplication sign
      loEscapes.Add("&Oslash;", "\xD8");//# capital O, slash
      loEscapes.Add("&Ugrave;", "\xD9");//# capital U, grave accent
      loEscapes.Add("&Uacute;", "\xDA");//# capital U, acute accent
      loEscapes.Add("&Ucirc;", "\xDB");//# capital U, circumflex accent
      loEscapes.Add("&Uuml;", "\xDC");//# capital U, dieresis or umlaut mark
      loEscapes.Add("&Yacute;", "\xDD");//# capital Y, acute accent
      loEscapes.Add("&THORN;", "\xDE");//# capital THORN, Icelandic
      loEscapes.Add("&szlig;", "\xDF");//# small sharp s, German (sz ligature)
      loEscapes.Add("&agrave;", "\xE0");//# small a, grave accent
      loEscapes.Add("&aacute;", "\xE1");//# small a, acute accent
      loEscapes.Add("&acirc;", "\xE2");//# small a, circumflex accent
      loEscapes.Add("&atilde;", "\xE3");//# small a, tilde
      loEscapes.Add("&auml;", "\xE4");//# small a, dieresis or umlaut mark
      loEscapes.Add("&aring;", "\xE5");//# small a, ring
      loEscapes.Add("&aelig;", "\xE6");//# small ae diphthong (ligature)
      loEscapes.Add("&ccedil;", "\xE7");//# small c, cedilla
      loEscapes.Add("&egrave;", "\xE8");//# small e, grave accent
      loEscapes.Add("&eacute;", "\xE9");//# small e, acute accent
      loEscapes.Add("&ecirc;", "\xEA");//# small e, circumflex accent
      loEscapes.Add("&euml;", "\xEB");//# small e, dieresis or umlaut mark
      loEscapes.Add("&igrave;", "\xEC");//# small i, grave accent
      loEscapes.Add("&iacute;", "\xED");//# small i, acute accent
      loEscapes.Add("&icirc;", "\xEE");//# small i, circumflex accent
      loEscapes.Add("&iuml;", "\xEF");//# small i, dieresis or umlaut mark
      loEscapes.Add("&dstrok;", "\xF0");//# small eth, Icelandic (Lynx)
      loEscapes.Add("&eth;", "\xF0");//# small eth, Icelandic
      loEscapes.Add("&ntilde;", "\xF1");//# small n, tilde
      loEscapes.Add("&ograve;", "\xF2");//# small o, grave accent
      loEscapes.Add("&oacute;", "\xF3");//# small o, acute accent
      loEscapes.Add("&ocirc;", "\xF4");//# small o, circumflex accent
      loEscapes.Add("&otilde;", "\xF5");//# small o, tilde
      loEscapes.Add("&ouml;", "\xF6");//# small o, dieresis or umlaut mark
      loEscapes.Add("&divide;", "\xF7");//# division sign
      loEscapes.Add("&oslash;", "\xF8");//# small o, slash
      loEscapes.Add("&ugrave;", "\xF9");//# small u, grave accent
      loEscapes.Add("&uacute;", "\xFA");//# small u, acute accent
      loEscapes.Add("&ucirc;", "\xFB");//# small u, circumflex accent
      loEscapes.Add("&uuml;", "\xFC");//# small u, dieresis or umlaut mark
      loEscapes.Add("&yacute;", "\xFD");//# small y, acute accent
      loEscapes.Add("&thorn;", "\xFE");//# small thorn, Icelandic
      loEscapes.Add("&yuml;", "\xFF");//# small y, dieresis or umlaut mark
    }
  }
}

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
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using MediaPortal.GUI.Library;
using System.Text.RegularExpressions;

namespace MediaPortal.MusicVideos.Database
{
  public class YahooGenres
  {
    private Dictionary<string, string> _yahooGenreList;
    public ArrayList _yahooSortedGenreList;
    public List<YahooVideo> _yahooGenreVideoList = new List<YahooVideo>();
    private int _yahooCurrentPage = 0;
    private String _yahooCountryName;
    private String _yahooCurrentGenreId;
    private bool _yahooNextPageFlag = false;
    private bool _yahooPreviousPageFlag = false;
    public Regex _yahooRegex = new Regex("href=\"genrehub.asp\\?genreID=[^=]*=(\\d+)");

    public bool hasNext()
    {
      return _yahooNextPageFlag;
    }

    public bool hasPrevious()
    {
      return _yahooPreviousPageFlag;
    }

    public YahooGenres()
    {
      loadGenres();
    }

    private void loadGenres()
    {
      _yahooGenreList = new Dictionary<string, string>();
      YahooUtil loUtil = YahooUtil.getInstance();
      YahooSettings loSettings = YahooSettings.getInstance();
      _yahooCountryName = loSettings._defaultCountryName;
      YahooSite loSite = loSettings._yahooSiteTable[loSettings._defaultCountryName];
      string lsCntryId = loSite._yahooSiteCountryId;
      string lsGenreListUrl = loSite._yahooSiteGenreListURL;
      //byte[] HTMLbuffer;
      string lsHtml = loUtil.getHTMLData(lsGenreListUrl);
      //string lsHtml = Encoding.UTF8.GetString(HTMLbuffer);
      //Log.Write("{0}", lsHtml);
      //Log.Write("{0}",lsHtml);
      //Regex loGenreRegex = new Regex("http://" + loSite._yahooSiteCountryId + ".rd.yahoo.com/launch/mv/hp/genre/(\\w+)/\\*http://[\\.\\w]+/genrehub.asp?genreID=(\\d+)");
      Regex loGenreRegex = new Regex("([\\w,&,;,\\-,/]*)/\\*http://[\\.,\\w]*music.yahoo.com[\\s.]+/musicvideos/genrehub.asp\\?genreID=(\\d+)");
      MatchCollection loGenreMatches = loGenreRegex.Matches(lsHtml);
      //Genre loGenre;       
      string lsGenreId;
      string lsGenreName;
      GroupCollection loGenreGrpCol;
      Log.Write("Found {0} genres", loGenreMatches.Count);
      // Loop through the match collection to retrieve all 
      // matches and positions
      for (int i = 0; i < loGenreMatches.Count; i++)
      {

        loGenreGrpCol = loGenreMatches[i].Groups;
        lsGenreName = loGenreGrpCol[1].Value;
        lsGenreId = loGenreGrpCol[2].Value;
        try
        {
          Log.Write("{0}", loGenreGrpCol[0].Value);
          Log.Write("{0}", loGenreGrpCol[1].Value);
          Log.Write("{0}", loGenreGrpCol[2].Value);
          Log.Write("{0}", loGenreGrpCol[3].Value);
          Log.Write("{0}", loGenreGrpCol[4].Value);
        }
        catch (Exception e)
        {
          Log.Write(e);
        }

        if (!_yahooGenreList.ContainsValue(lsGenreId))
        {
          Log.Write("Adding genre id:{0}", lsGenreId);
          Log.Write("Adding genre name:{0}", lsGenreName);
          _yahooGenreList.Add(lsGenreName, lsGenreId);
          Log.Write("Genre found with name:{0} and id:{0}", lsGenreName, lsGenreId);
        }
        else
        {
          Log.Write("found a duplicate genre:{0}", lsGenreName);
        }
        _yahooSortedGenreList = new ArrayList(_yahooGenreList.Keys);
        _yahooSortedGenreList.Sort();
      }
    }

    public void loadNextVideos()
    {
      if (hasNext())
      {
        _yahooCurrentPage++;
        loadGenreVideos();
      }
    }

    public void loadPreviousVideos()
    {
      if (hasPrevious())
      {
        _yahooCurrentPage--;
        loadGenreVideos();
      }
    }

    private void loadGenreVideos()
    {
      YahooUtil loUtil = YahooUtil.getInstance();
      YahooSite loSite = loUtil.getYahooSite(_yahooCountryName); ;
      string lsGenreUrl = loSite._yahooSiteGenreURL + _yahooCurrentGenreId + "&p=" + _yahooCurrentPage;
      //string lsGenreUrl = "http://music.yahoo.com/musicvideos/genrehub.asp?genreID=" + _yahooCurrentGenreId; //+ "&p=" + _yahooCurrentPage;
      string lsHtml = loUtil.getHTMLData(lsGenreUrl);
      Log.Write("genre url={0}", lsGenreUrl);
      _yahooGenreVideoList = loUtil.getVideoList(lsHtml, loSite._yahooSiteCountryId, loUtil._yahooArtistRegex, loUtil._yahooSongRegex);
      Log.Write("video count ={0}", _yahooGenreVideoList.Count);
      setNavigationFlags(lsHtml);
      //Log.Write("{0}",lsHtml);
    }

    public void loadFirstGenreVideos(string fsGenreName)
    {
      _yahooCurrentPage = 1;
      //List<YahooVideo> loGenreVideos = null;
      if (_yahooGenreList[fsGenreName] == null)
      {
        Log.Write("Genre not found in list.");
        return; ;
      }
      _yahooCurrentGenreId = _yahooGenreList[fsGenreName];
      loadGenreVideos();


    }

    private void setNavigationFlags(String fsHtml)
    {
      GroupCollection loGrpCol = null;
      MatchCollection loMatches = null;
      int liPageIndex = 0;
      loMatches = _yahooRegex.Matches(fsHtml);

      bool lbNextSet = false;
      bool lbPrevSet = false;
      // Loop through the match collection to retrieve all 
      // matches and positions
      for (int i = 0; i < loMatches.Count; i++)
      {

        loGrpCol = loMatches[i].Groups;
        liPageIndex = Convert.ToInt32(loGrpCol[1].Value);
        if (liPageIndex > _yahooCurrentPage)
        {
          _yahooNextPageFlag = true;
          lbNextSet = true;
        }
        else
        {
          _yahooPreviousPageFlag = true;
          lbPrevSet = true;

        }

      }
      if (lbNextSet == false)
      {
        _yahooNextPageFlag = false;
      }
      if (lbPrevSet == false)
      {
        _yahooPreviousPageFlag = false;
      }
    }

    public int getCurrentPageNumber()
    {
      return _yahooCurrentPage;
    }
  }

}
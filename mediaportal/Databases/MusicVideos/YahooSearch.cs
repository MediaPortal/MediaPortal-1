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
using System.Text.RegularExpressions;
using MediaPortal.GUI.Library;

namespace MediaPortal.MusicVideos.Database
{

  public class YahooSearch
  {
    public List<YahooVideo> _yahooLastSearchResult;
    private int _yahooCurrentPage = 0;
    private bool _yahooNextPageFlag = false;
    private bool _yahooPreviousPageFlag = false;
    public Regex _yahooRegex = new Regex("/search/\\?m=video&p=[^&]*&b=(\\d+)");
    private String _yahooSearchUrl = "";
    private String _yahooCountryName = "";
    private String _yahooLastSearchText = "";

    public YahooSearch(String fsCountry)
    {
      _yahooCountryName = fsCountry;
    }

    public bool hasNext()
    {
      return _yahooNextPageFlag;
    }

    public bool hasPrevious()
    {
      return _yahooPreviousPageFlag;
    }

    public void loadNextVideos()
    {
      if (hasNext())
      {
        _yahooCurrentPage += 25;
        loadVideos();
      }

    }

    public void loadPreviousVideos()
    {
      if (hasPrevious())
      {
        _yahooCurrentPage -= 25;
        loadVideos();
      }

    }

    public void searchVideos(string fsSearchText)
    {
      _yahooLastSearchText = fsSearchText;
      _yahooLastSearchResult = new List<YahooVideo>();
      if (fsSearchText == "" || fsSearchText == null) return;//return _yahooLastSearchResult;
      fsSearchText = fsSearchText.Replace(" ", "%20");

      YahooSettings loSettings = YahooSettings.getInstance();
      YahooSite loSite = loSettings._yahooSiteTable[_yahooCountryName];

      _yahooSearchUrl = loSite._yahooSiteSearchURL + fsSearchText + "&b=";
      _yahooCurrentPage = 1;
      loadVideos();

    }

    private void loadVideos()
    {
      YahooSettings loSettings = YahooSettings.getInstance();
      YahooSite loSite = loSettings._yahooSiteTable[_yahooCountryName];
      YahooUtil loUtil = YahooUtil.getInstance();
      string lsHtml = loUtil.getHTMLData(_yahooSearchUrl + _yahooCurrentPage);
      List<YahooVideo> loTempResultList = loUtil.getVideoList(lsHtml, loSite._yahooSiteCountryId, loUtil._yahooArtistRegex, loUtil._yahooSearchSongRegex);
      Log.Write("search returned {0} videos", loTempResultList.Count);

      _yahooLastSearchResult = loTempResultList;
      setNavigationFlags(lsHtml);
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

    public string getLastSearchText()
    {
      return _yahooLastSearchText;
    }

  }
}
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
    public List<YahooVideo> moLastSearchResult;
    private int miCurrentPage = 0;
    private bool mbNextPageFlag = false;
    private bool mbPreviousPageFlag = false;
    public Regex moRegex = new Regex("/search/\\?m=video&p=[^&]*&b=(\\d+)");
    private String msSearchUrl = "";
    private String msCountryName = "";
    private String msLastSearchText = "";
    public YahooSearch(String fsCountry)
    {
      msCountryName = fsCountry;
    }
    public bool hasNext()
    {
      return mbNextPageFlag;
    }
    public bool hasPrevious()
    {
      return mbPreviousPageFlag;
    }
    public void loadNextVideos()
    {
      if (hasNext())
      {
        miCurrentPage += 25;
        loadVideos();
      }

    }
    public void loadPreviousVideos()
    {
      if (hasPrevious())
      {
        miCurrentPage -= 25;
        loadVideos();
      }

    }
    public void searchVideos(string fsSearchText)
    {
      msLastSearchText = fsSearchText;
      moLastSearchResult = new List<YahooVideo>();
      if (fsSearchText == "" || fsSearchText == null) return;//return moLastSearchResult;
      fsSearchText = fsSearchText.Replace(" ", "%20");

      YahooSettings loSettings = YahooSettings.getInstance();
      YahooSite loSite = loSettings.moYahooSiteTable[msCountryName];

      msSearchUrl = loSite.SearchURL + fsSearchText + "&b=";
      miCurrentPage = 1;
      loadVideos();

    }
    private void loadVideos()
    {
      YahooSettings loSettings = YahooSettings.getInstance();
      YahooSite loSite = loSettings.moYahooSiteTable[msCountryName];
      YahooUtil loUtil = YahooUtil.getInstance();
      string lsHtml = loUtil.getHTMLData(msSearchUrl + miCurrentPage);
      List<YahooVideo> loTempResultList = loUtil.getVideoList(lsHtml, loSite.countryId, loUtil.moArtistRegex, loUtil.moSearchSongRegex);
      Log.Write("search returned {0} videos", loTempResultList.Count);

      moLastSearchResult = loTempResultList;
      setNavigationFlags(lsHtml);
    }
    private void setNavigationFlags(String fsHtml)
    {
      GroupCollection loGrpCol = null;
      MatchCollection loMatches = null;
      int liPageIndex = 0;
      loMatches = moRegex.Matches(fsHtml);

      bool lbNextSet = false;
      bool lbPrevSet = false;
      // Loop through the match collection to retrieve all 
      // matches and positions
      for (int i = 0; i < loMatches.Count; i++)
      {

        loGrpCol = loMatches[i].Groups;
        liPageIndex = Convert.ToInt32(loGrpCol[1].Value);
        if (liPageIndex > miCurrentPage)
        {
          mbNextPageFlag = true;
          lbNextSet = true;
        }
        else
        {
          mbPreviousPageFlag = true;
          lbPrevSet = true;

        }

      }
      if (lbNextSet == false)
      {
        mbNextPageFlag = false;
      }
      if (lbPrevSet == false)
      {
        mbPreviousPageFlag = false;
      }
    }
    public int getCurrentPageNumber()
    {
      return miCurrentPage;
    }
    public string getLastSearchText()
    {
      return msLastSearchText;
    }
  }
}
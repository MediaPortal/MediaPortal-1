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
using MediaPortal;
using MediaPortal.GUI.Library;
//using MediaPortal.Dialogs;
using System.Collections;
using System.Text.RegularExpressions;

namespace MediaPortal.MusicVideos.Database
{

  public class YahooNewVideos//:GUIWindow
  {

    public List<YahooVideo> moNewVideoList = new List<YahooVideo>();
    //public List<YahooVideo> moNextVideoList = new List<YahooVideo>();
    //private int WINDOW_ID = 473555;
    //private DateTime moLastImportNewVideoDt;
    //private Boolean mbNewVideoLoaded = false;
    //private string msLastNewVideoRunCountry;
    private int miCurrentPage = 0;
    private bool mbNextPageFlag = false;
    private bool mbPreviousPageFlag = false;
    public Regex moRegex = new Regex("href=\"\\w+.asp\\?p=(\\d+)");
    public bool hasNext()
    {
      return mbNextPageFlag;
    }
    public bool hasPrevious()
    {
      return mbPreviousPageFlag;
    }

    private void loadVideos(String fsCountryName)
    {
      YahooUtil loUtil = YahooUtil.getInstance();

      Log.Write("in loadNewVideos");

      if (moNewVideoList == null)
      {
        moNewVideoList = new List<YahooVideo>();
      }
      else
      {
        moNewVideoList.Clear();
      }
      YahooSite loSite = loUtil.getYahooSite(fsCountryName);
      //msLastNewVideoRunCountry = loSite.countryName;

      string lsHtml;

      lsHtml = loUtil.getHTMLData(loSite.NewURL + "?p=" + miCurrentPage);
      moNewVideoList.AddRange(loUtil.getVideoList(lsHtml, loSite.countryId, loUtil.moArtistRegex, loUtil.moSongRegex));
      //moLastImportNewVideoDt = DateTime.Now;
      //mbNewVideoLoaded = true;
      setNavigationFlags(lsHtml);
    }
    public void loadNextVideos(String fsCountryName)
    {
      if (hasNext())
      {
        miCurrentPage++;
        loadVideos(fsCountryName);
      }

    }
    public void loadPreviousVideos(String fsCountryName)
    {
      if (hasPrevious())
      {
        miCurrentPage--;
        loadVideos(fsCountryName);
      }

    }
    public void loadNewVideos(string fsCountryName)
    {
      miCurrentPage = 1;
      loadVideos(fsCountryName);
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

  }
}
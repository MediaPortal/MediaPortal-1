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

namespace MediaPortal.MusicVideos.Database
{

  public class YahooTopVideos//:GUIWindow
  {

    private List<YahooVideo> moTopVideoList = new List<YahooVideo>();
    //private int WINDOW_ID = 473555;
    //private DateTime moLastImportTopVideoDt;
    //private Boolean mbTopVideoLoaded = false;
    //private string msLastTopVideoRunCountry;
    private int miLastPageNoLoaded = 0;
    private string msCountry;
    public YahooTopVideos(string fsCountry)
    {
      msCountry = fsCountry;
    }
    public List<YahooVideo> getLastLoadedList()
    {
      return moTopVideoList;
    }
    public List<YahooVideo> loadFirstPage()
    {
      loadTopVideos(1);
      miLastPageNoLoaded = 1;
      return moTopVideoList;

    }
    public bool hasMorePages()
    {
      return miLastPageNoLoaded < 5;
    }
    public bool hasPreviousPage()
    {
      return miLastPageNoLoaded > 1;
    }
    public List<YahooVideo> loadNextPage()
    {
      if (hasMorePages())
      {
        loadTopVideos(miLastPageNoLoaded + 1);
        miLastPageNoLoaded++;
      }

      return moTopVideoList;

    }
    public List<YahooVideo> loadPreviousPage()
    {
      if (miLastPageNoLoaded > 1)
      {
        loadTopVideos(miLastPageNoLoaded - 1);
        miLastPageNoLoaded--;
      }

      return moTopVideoList;

    }

    //#region GUIWindow Overrides

    //public override int GetID
    //{
    //    get { return WINDOW_ID; }
    //    set { base.GetID = value; }
    //}

    //public override bool Init()
    //{
    //    return Load(GUIGraphicsContext.Skin + @"\mytopmusicvideos.xml");
    //}

    //protected override void OnPageDestroy(int new_windowId)
    //{
    //    base.OnPageDestroy(new_windowId);
    //}
    //protected override void OnPageLoad()
    //{
    //    Log.Write("YahooTopVideos onPageLoad.");
    //    //load the top videos
    //    YahooSettings loSettings= YahooSettings.getInstance();
    //    loadTopVideos(loSettings.msDefaultCountryName);
    //}



    //#endregion
    private void loadTopVideos(int fiPageNo)
    {
      YahooUtil loUtil = YahooUtil.getInstance();
      Log.Write("in loadTopVideos");
      //if (mbTopVideoLoaded && fsCountryName == msLastTopVideoRunCountry)
      //{
      //    TimeSpan loDataDiff = DateTime.Now - moLastImportTopVideoDt;
      //    Log.Write("date difference={0}", loDataDiff.Minutes);
      //    if (loDataDiff.Minutes < 60)
      //    {
      //        return;
      //    }
      //}
      if (moTopVideoList == null)
      {
        moTopVideoList = new List<YahooVideo>();
      }
      else
      {
        moTopVideoList.Clear();
      }

      YahooSite loSite = loUtil.getYahooSite(msCountry);
      //msLastTopVideoRunCountry = loSite.countryName;

      string HTMLdownload;
      //for (int i = 1; i <= 5; i++)
      //{

      HTMLdownload = loUtil.getHTMLData(loSite.TopURL + "?p=" + fiPageNo);
      moTopVideoList.AddRange(loUtil.getVideoList(HTMLdownload, loSite.countryId, loUtil.moArtistRegex, loUtil.moSongRegex));

    }
    public void loadAllTopVideos()
    {
      YahooUtil loUtil = YahooUtil.getInstance();
      Log.Write("in loadTopVideos");
      //if (mbTopVideoLoaded && msCountry == msLastTopVideoRunCountry)
      ///if(mbTopVideoLoaded)
      ///{
      ///    TimeSpan loDataDiff = DateTime.Now - moLastImportTopVideoDt;
      ///    Log.Write("date difference={0}", loDataDiff.Minutes);
      ///    if (loDataDiff.Minutes < 60)
      ///    {
      ///         return;
      ///     }
      /// }
      if (moTopVideoList == null)
      {
        moTopVideoList = new List<YahooVideo>();
      }
      else
      {
        moTopVideoList.Clear();
      }
      //GUIDialogProgress loProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      //loProgress.SetHeading("Yahoo Download");
      //loProgress.SetLine(1, "Importing Top Videos.");
      //loProgress.SetPercentage(0);
      //loProgress.StartModal(4734);
      //loProgress.Progress();
      //loProgress.ShowProgressBar(true);
      YahooSite loSite = loUtil.getYahooSite(msCountry);
      //msLastTopVideoRunCountry = loSite.countryName;

      string HTMLdownload;
      for (int i = 1; i <= 5; i++)
      {

        HTMLdownload = loUtil.getHTMLData(loSite.TopURL + "?p=" + i);
        moTopVideoList.AddRange(loUtil.getVideoList(HTMLdownload, loSite.countryId, loUtil.moArtistRegex, loUtil.moSongRegex));
        //loProgress.SetPercentage(i * 20);
        //loProgress.Progress();
      }
      //loProgress.SetPercentage(100);
      //loProgress.Close();
      ///moLastImportTopVideoDt = DateTime.Now;
      ///mbTopVideoLoaded = true;

    }
    public int getFirstVideoRank()
    {
      return ((miLastPageNoLoaded - 1) * moTopVideoList.Count) + 1;
    }
    public int getLastVideoRank()
    {
      return miLastPageNoLoaded * moTopVideoList.Count;
    }

  }
}

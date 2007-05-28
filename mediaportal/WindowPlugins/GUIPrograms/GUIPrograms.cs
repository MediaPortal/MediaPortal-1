#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.IO;
using System.Collections;
using System.Xml.Serialization;
using MediaPortal.Database;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.View;
using MediaPortal.Profile;
using MediaPortal.Util;
using MediaPortal.Configuration;
using Microsoft.DirectX.Direct3D;
using Programs.Utils;
using ProgramsDatabase;
using FileInfo = ProgramsDatabase.FileInfo;

namespace WindowPlugins.GUIPrograms
{
  /// <summary>
  /// The GUIPrograms plugin is used to list a collection of arbitrary files
  /// and use them as arguments when launching external applications.
  /// </summary>
  /// 
  public class GUIPrograms : GUIWindow
  {
    #region enums

    enum View
    {
      List = 0,
      Icons = 1,
      LargeIcons = 2,
      FilmStrip = 3,
    }

    #endregion

    #region Serialisation

    [Serializable]
    public class MapSettings
    {
      protected int _SortBy;
      protected int _ViewAs;
      protected bool _SortAscending;
      protected int _LastAppID;
      protected int _LastFileID;
      protected int _LastViewLevel;
      protected bool _OverviewVisible;

      public MapSettings()
      {
        _SortBy = 0; //name
        _ViewAs = 0; //list
        _SortAscending = true;
        _OverviewVisible = true;
        _LastAppID = - 1;
        _LastFileID = - 1;
        _LastViewLevel = 0;
      }


      [XmlElement("SortBy")]
      public int SortBy
      {
        get { return _SortBy; }
        set { _SortBy = value; }
      }

      [XmlElement("ViewAs")]
      public int ViewAs
      {
        get { return _ViewAs; }
        set { _ViewAs = value; }
      }

      [XmlElement("SortAscending")]
      public bool SortAscending
      {
        get { return _SortAscending; }
        set { _SortAscending = value; }
      }

      [XmlElement("OverviewVisible")]
      public bool OverviewVisible
      {
        get { return _OverviewVisible; }
        set { _OverviewVisible = value; }
      }

      [XmlElement("LastAppID")]
      public int LastAppID
      {
        get { return _LastAppID; }
        set { _LastAppID = value; }
      }

      [XmlElement("LastFileID")]
      public int LastFileID
      {
        get { return _LastFileID; }
        set { _LastFileID = value; }
      }

      [XmlElement("LastViewLevelID")]
      public int LastViewLevel
      {
        get { return _LastViewLevel; }
        set { _LastViewLevel = value; }
      }


      public string ViewAsText
      {
        get { return GetViewAsText(); }
      }

      public void SwitchToNextView()
      {
        switch ((View) ViewAs)
        {
          case View.List:
            ViewAs = (int) View.Icons;
            break;
          case View.Icons:
            ViewAs = (int) View.LargeIcons;
            break;
          case View.LargeIcons:
            ViewAs = (int) View.FilmStrip;
            break;
          case View.FilmStrip:
            ViewAs = (int) View.List;
            break;
        }
      }

      string GetViewAsText()
      {
        string result = "";
        switch ((View) ViewAs)
        {
          case View.List:
            result = GUILocalizeStrings.Get(101);
            break;
          case View.Icons:
            result = GUILocalizeStrings.Get(100);
            break;
          case View.LargeIcons:
            result = GUILocalizeStrings.Get(417);
            break;
          case View.FilmStrip:
            result = GUILocalizeStrings.Get(733);
            break;
        }
        return result;
      }
    }

    void SaveSettings()
    {
      using (Settings xmlwriter = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        switch ((View) mapSettings.ViewAs)
        {
          case View.List:
            xmlwriter.SetValue("myprograms", "viewby", "list");
            break;
          case View.Icons:
            xmlwriter.SetValue("myprograms", "viewby", "icons");
            break;
          case View.LargeIcons:
            xmlwriter.SetValue("myprograms", "viewby", "largeicons");
            break;
          case View.FilmStrip:
            xmlwriter.SetValue("myprograms", "viewby", "filmstrip");
            break;
        }
        xmlwriter.SetValue("myprograms", "lastAppID", mapSettings.LastAppID.ToString());
        //        xmlwriter.SetValue("myprograms", "lastViewLevel", mapSettings.LastViewLevel.ToString());
        xmlwriter.SetValue("myprograms", "lastViewLevel", ProgramSettings.viewHandler.CurrentLevel);
        xmlwriter.SetValue("myprograms", "sortby", mapSettings.SortBy);
        xmlwriter.SetValueAsBool("myprograms", "sortasc", mapSettings.SortAscending);
        xmlwriter.SetValueAsBool("myprograms", "overviewvisible", mapSettings.OverviewVisible);

        xmlwriter.SetValue("myprograms", "startWindow", ProgramState.StartWindow.ToString());
        xmlwriter.SetValue("myprograms", "startview", ProgramState.View);
      }
    }

    void LoadSettings()
    {
      string _slideSpeed = ProgramSettings.ReadSetting(ProgramUtils.cSLIDESPEED);
      if ((_slideSpeed != "") && (_slideSpeed != null))
      {
        slideSpeed = int.Parse(_slideSpeed);
      }

      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        switch (xmlreader.GetValueAsString("myprograms", "viewby", "list"))
        {
          case "list":
            mapSettings.ViewAs = (int)View.List;
            break;
          case "icons":
            mapSettings.ViewAs = (int)View.Icons;
            break;
          case "largeicons":
            mapSettings.ViewAs = (int)View.LargeIcons;
            break;
          case "filmstrip":
            mapSettings.ViewAs = (int)View.FilmStrip;
            break;
        }

        mapSettings.LastAppID = xmlreader.GetValueAsInt("myprograms", "lastAppID", - 1);
        mapSettings.LastViewLevel = xmlreader.GetValueAsInt("myprograms", "lastViewLevel", - 1);
        mapSettings.SortBy = xmlreader.GetValueAsInt("myprograms", "sortby", 0);
        mapSettings.SortAscending = xmlreader.GetValueAsBool("myprograms", "sortasc", true);
        mapSettings.OverviewVisible = xmlreader.GetValueAsBool("myprograms", "overviewvisible", true);

        ProgramState.StartWindow = xmlreader.GetValueAsInt("myprograms", "startWindow", GetID);
        ProgramState.View = xmlreader.GetValueAsString("myprograms", "startview", String.Empty);
      }
    }

    void LoadLastAppIDFromSettings()
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        mapSettings.LastAppID = xmlreader.GetValueAsInt("myprograms", "lastAppID", - 1);
        mapSettings.LastViewLevel = xmlreader.GetValueAsInt("myprograms", "lastViewLevel", - 1);
        mapSettings.SortBy = xmlreader.GetValueAsInt("myprograms", "sortby", 0);
        mapSettings.SortAscending = xmlreader.GetValueAsBool("myprograms", "sortasc", true);
        mapSettings.OverviewVisible = xmlreader.GetValueAsBool("myprograms", "overviewvisible", true);
      }
    }

    void LoadFolderSettings(string directoryName)
    {
      if (directoryName == "")
        directoryName = "root";
      object o;
      FolderSettings.GetFolderSetting(directoryName, "Programs", typeof (MapSettings), out o);
      if (o != null)
        mapSettings = o as MapSettings;
      if (mapSettings == null)
        mapSettings = new MapSettings();
    }

    void SaveFolderSettings(string directoryName)
    {
      if (directoryName == "")
        directoryName = "root";
      FolderSettings.AddFolderSetting(directoryName, "Programs", typeof (MapSettings), mapSettings);
    }

    #endregion 

    #region SkinControls

    // Buttons
    [SkinControl(2)] protected GUIButtonControl btnViewAs = null;
    [SkinControl(3)] protected GUIButtonControl btnRefresh = null;
    [SkinControl(4)] protected GUIButtonControl btnViews = null;

    // Images                     
    [SkinControl(6)] protected GUIImage screenShotImage = null;

    // FacadeView
    [SkinControl(50)] protected GUIFacadeControl facadeView = null;

    #endregion 

    #region Constructor / Destructor

    public GUIPrograms()
    {
      GetID = (int) Window.WINDOW_FILES;
      apps = ProgramDatabase.AppList;
      LoadSettings();
      skipInit = true;
    }

    ~GUIPrograms()
    {
      SaveSettings();
      //FolderSettings.DeleteFolderSetting("root", "Programs");
    }

    #endregion 

    #region Init / DeInit

    void DeInitMyPrograms()
    {
      SaveSettings();
      // make sure the selected index wasn't reseted already
      // and save the index only if it's non-zero
      // otherwise: DXDevice.Reset clears selection 
      int itemIndex = GetSelectedItemNo();
      if (itemIndex > 0)
      {
        selectedItemIndex = GetSelectedItemNo();
      }
    }

    void InitMyPrograms()
    {
      LoadFolderSettings("");
      if (skipInit)
      {
        mapSettings.LastAppID = -1;
      }
      else
      {
        LoadLastAppIDFromSettings(); // hacky load back the last app id, otherwise this can get lost from dx resets....
      }
      lastApp = apps.GetAppByID(mapSettings.LastAppID);
      if (lastApp != null)
      {
        lastFilepath = lastApp.DefaultFilepath();
        lastApp.CurrentSortIndex = mapSettings.SortBy;
        lastApp.CurrentSortIsAscending = mapSettings.SortAscending;
        ProgramSettings.viewHandler.CurrentLevel = mapSettings.LastViewLevel;
      }
      else
      {
        lastFilepath = "";
      }
      UpdateListControl();
      SwitchView();
      skipInit = false;
    }

    #endregion 

    #region Base & Content Variables

    static Applist apps = ProgramDatabase.AppList;
    MapSettings mapSettings = new MapSettings();
    DirectoryHistory itemHistory = new DirectoryHistory();
    AppItem lastApp = null;
    string lastFilepath = "";
    int selectedItemIndex = - 1;
    int slideSpeed = 3000; // speed in milliseconds between two slides
    long slideTime = 0;
    bool skipInit = false;

    View[,] views;
    bool[,] sortasc;
    ProgramSort.SortMethod[,] sortby;

    static string _thumbnailPath = string.Empty;
    static string _lastThumbnailPath = string.Empty;

    #endregion 

    #region Properties / Helper Routines

    public static string ThumbnailPath
    {
      get
      {
        return _thumbnailPath;
      }
      set
      {
        if (value == "")
          _thumbnailPath = "";
        else if (File.Exists(value))
          _thumbnailPath = value;
      }
    }

    GUIListItem GetSelectedItem()
    {
      return facadeView.SelectedListItem;
    }

    int GetSelectedItemNo()
    {
      return facadeView.SelectedListItemIndex;
    }

    int GetCurrentFatherID()
    {
      if (lastApp != null)
      {
        return lastApp.AppID;
      }
      else
      {
        return - 1; // root
      }
    }

    ProgramViewHandler ViewHandler
    {
      get { return ProgramSettings.viewHandler; }
    }

    View CurrentView
    {
      get
      {
        if (ViewHandler.View == null)
          return View.List;

        if (views == null)
        {
          views = new View[ViewHandler.Views.Count, 50];

          ArrayList viewStrings = new ArrayList();
          viewStrings.Add("List");
          viewStrings.Add("Icons");
          viewStrings.Add("Big Icons");
          viewStrings.Add("Filmstrip");

          for (int i = 0; i < ViewHandler.Views.Count; ++i)
          {
            for (int j = 0; j < ViewHandler.Views[i].Filters.Count; ++j)
            {
              FilterDefinition def = (FilterDefinition)ViewHandler.Views[i].Filters[j];
              int defaultView = viewStrings.IndexOf(def.DefaultView);

              if (defaultView != -1)
                views[i, j] = (View)defaultView;
              else
                views[i, j] = View.List;
            }
          }
        }

        return views[ViewHandler.Views.IndexOf(ViewHandler.View), ViewHandler.CurrentLevel];
      }
      set
      {
        views[ViewHandler.Views.IndexOf(ViewHandler.View), ViewHandler.CurrentLevel] = value;
      }
    }

    bool CurrentSortAsc
    {
      get
      {
        if (ViewHandler.View == null)
          return true;

        if (sortasc == null)
        {
          sortasc = new bool[ViewHandler.Views.Count, 50];

          for (int i = 0; i < ViewHandler.Views.Count; ++i)
          {
            for (int j = 0; j < ViewHandler.Views[i].Filters.Count; ++j)
            {
              FilterDefinition def = (FilterDefinition)ViewHandler.Views[i].Filters[j];
              sortasc[i, j] = def.SortAscending;
            }
          }
        }

        return sortasc[ViewHandler.Views.IndexOf(ViewHandler.View), ViewHandler.CurrentLevel];
      }
      set
      {
        sortasc[ViewHandler.Views.IndexOf(ViewHandler.View), ViewHandler.CurrentLevel] = value;
      }
    }

    ProgramSort.SortMethod CurrentSortMethod
    {
      get
      {
        if (ViewHandler.View == null)
          return ProgramSort.SortMethod.Name;

        if (sortby == null)
        {
          sortby = new ProgramSort.SortMethod[ViewHandler.Views.Count, 50];

          ArrayList sortStrings = new ArrayList();
          sortStrings.Add("Name");
          sortStrings.Add("Title");
          sortStrings.Add("Filename");
          sortStrings.Add("Rating");
          sortStrings.Add("LaunchCount");
          sortStrings.Add("LastTimeLaunched");

          for (int i = 0; i < ViewHandler.Views.Count; ++i)
          {
            for (int j = 0; j < ViewHandler.Views[i].Filters.Count; ++j)
            {
              FilterDefinition def = (FilterDefinition)ViewHandler.Views[i].Filters[j];
              int defaultSort = sortStrings.IndexOf(def.DefaultSort);

              if (defaultSort != -1)
                sortby[i, j] = (ProgramSort.SortMethod)defaultSort;
              else
                sortby[i, j] = ProgramSort.SortMethod.Name;
            }
          }
        }

        return sortby[ViewHandler.Views.IndexOf(ViewHandler.View), ViewHandler.CurrentLevel];
      }
      set
      {
        sortby[ViewHandler.Views.IndexOf(ViewHandler.View), ViewHandler.CurrentLevel] = value;
      }
    }

    #endregion 

    #region Overrides

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\myprograms.xml");
    }

    protected override void OnPageLoad()
    {
      string view = ProgramState.View;
      ProgramSettings.viewHandler.CurrentView = view;
      base.OnPageLoad();
      InitMyPrograms();
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      DeInitMyPrograms();
      base.OnPageDestroy(newWindowId);
    }
    
    public override void Render(float timePassed)
    {
      base.Render(timePassed);

      if (ThumbnailPath != _lastThumbnailPath)
      {
        screenShotImage.FileName = ThumbnailPath;
        facadeView.FilmstripView.InfoImageFileName = ThumbnailPath;
        facadeView.FilmstripView.NeedRefresh();

        _lastThumbnailPath = ThumbnailPath;
      }

      RenderThumbnail(timePassed);
   }

    void ScrapeFileInfo(FileItem curFile)
    {
      int minRelevance = 30;
      bool bSuccess = false;
      ScraperSaveType saveType = ScraperSaveType.DataAndImages;
      GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_OK);
      GUIDialogProgress dlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_PROGRESS);
      dlgProgress.ShowWaitCursor = false;
      dlgProgress.ShowProgressBar(false);
      dlgProgress.SetHeading("Lookup Gameinfo");
      dlgProgress.SetLine(1, curFile.Title);
      dlgProgress.SetLine(2, curFile.System_);
      dlgProgress.SetLine(3, "");
      dlgProgress.StartModal(GetID);
//      dlgProgress.SetPercentage(60);
      dlgProgress.Progress();
      bSuccess = curFile.FindFileInfo(myProgScraperType.ALLGAME);
      if ((bSuccess && curFile.FileInfoList.Count > 0) && ((FileInfo)(curFile.FileInfoList[0])).RelevanceNorm >= minRelevance)
      {
        GUIDialogSelect pDlg = (GUIDialogSelect)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_SELECT);
        if (null != pDlg)
        {
          pDlg.Reset();
          pDlg.SetHeading("Select Title");
          foreach (FileInfo item in curFile.FileInfoList)
          {
            if (item.RelevanceNorm >= minRelevance)
            {
              pDlg.Add(String.Format("{0} ({1})", item.Title, item.Platform));
            }
          }
          pDlg.DoModal(GetID);

          // and wait till user selects one
          int iSelectedGame = pDlg.SelectedLabel;
          if (iSelectedGame < 0) return;

          dlgProgress.StartModal(GetID);
          dlgProgress.Progress();
          dlgProgress.ShowProgressBar(false);
          curFile.FileInfoFavourite = (FileInfo)curFile.FileInfoList[iSelectedGame];

          curFile.FindFileInfoDetail(lastApp, curFile.FileInfoFavourite, myProgScraperType.ALLGAME, saveType);
          if ((saveType == ScraperSaveType.DataAndImages) || (saveType == ScraperSaveType.Data))
          {
//            dlgProgress.SetPercentage(60);
            dlgProgress.Progress();
            curFile.SaveFromFileInfoFavourite();
          }
//          dlgProgress.SetPercentage(100);
          dlgProgress.Progress();
          dlgProgress.Close();
          dlgProgress=null;
        }
        OnInfo();
      }
      else
      {
        string strMsg = "";
        if (!bSuccess)
        {
          strMsg = "Connection failed";
          Log.Info("myPrograms: RefreshData failed");
        }
        else
        {
          strMsg = String.Format("No match for '{0}'", curFile.Title);
          Log.Info("myPrograms: No data found for '{0}'", curFile.Title);
        }
        if (null != dlgOk)
        {
          dlgOk.SetHeading(187);
          dlgOk.SetLine(1, strMsg);
          dlgOk.SetLine(2, String.Empty);
          dlgOk.DoModal(GetID);
        }
      }
    }

    protected void OnShowViews()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
        return;
      dlg.Reset();
      dlg.SetHeading(499); // Actions
      dlg.Add(GUILocalizeStrings.Get(100000 + GetID)); // Files
      foreach (ViewDefinition view in ProgramSettings.viewHandler.Views)
      {
        dlg.Add(view.LocalizedName);
      }
      dlg.DoModal(GetID);

      if (dlg.SelectedLabel == -1)
        return;

      if (dlg.SelectedLabel == 0)
      {
        int nNewWindow = (int) Window.WINDOW_FILES;
        ProgramState.StartWindow = nNewWindow;
        ProgramState.View = "";
        ProgramSettings.viewHandler.CurrentView = null;
        if (nNewWindow != GetID)
        {
          GUIWindowManager.ReplaceWindow(nNewWindow);
        }
      }
      else
      {
        ViewDefinition selectedView = ProgramSettings.viewHandler.Views[dlg.SelectedLabel - 1];
        ProgramSettings.viewHandler.CurrentView = selectedView.Name;
        ProgramState.View = selectedView.Name;
        int nNewWindow = (int)GUIWindow.Window.WINDOW_FILES;
        if (GetID != nNewWindow)
        {
          ProgramState.StartWindow = nNewWindow;
          if (nNewWindow != GetID)
          {
            GUIWindowManager.ReplaceWindow(nNewWindow);
          }
        }
        else
        {
          if (facadeView.Count <= 0)
          {
            GUIControl.FocusControl(GetID, btnViewAs.GetID);
          }
        }
      }
      if (lastApp != null)
      {
        lastApp.LoadFiles();
      }
      UpdateButtonStates();
      UpdateListControl();
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);

      if (control == btnViewAs)
      {
        mapSettings.SwitchToNextView();
        SwitchView();
      }

      if (control == btnRefresh)
      {
        if (lastApp != null)
        {
          lastApp.Refresh(true);
          lastFilepath = lastApp.DefaultFilepath();
          // todo: reset viewHandler
          UpdateButtonStates();
          UpdateListControl();
        }
      }

      if (control == btnViews)
      {
        OnShowViews();
      }

      if (control == facadeView)
      {
        if (actionType == Action.ActionType.ACTION_SELECT_ITEM)
        {
          OnClick();
        }
      }
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_ITEM_FOCUS_CHANGED:
          {
            int iControl = message.SenderControlId;
            if (iControl == facadeView.GetID)
            {
              if (lastApp != null)
              {
                lastApp.ResetThumbs();
              }
            }
          }
          break;
      }
      return base.OnMessage(message);
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PARENT_DIR  || action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        // <U> keypress
        BackItemClicked();
        UpdateButtonStates();
        return;
      }

      //      if (action.wID == Action.ActionType.ACTION_CLOSE_DIALOG || action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      if (action.wID == Action.ActionType.ACTION_CLOSE_DIALOG)
      {
        SaveFolderSettings("");
        GUIWindowManager.ShowPreviousWindow();
        return;
      }

      if (action.wID == Action.ActionType.ACTION_SHOW_INFO)
      {
        OnInfo();
        return;
      }
      base.OnAction(action);
    }

    protected override void OnShowContextMenu()
    {
      GUIListItem item = facadeView.SelectedListItem;
      int itemNo = facadeView.SelectedListItemIndex;
      if (item == null)
        return;

      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg == null)
        return;
      dlg.Reset();
      dlg.SetHeading(498); // menu

      if (!item.IsFolder)
      {
        dlg.AddLocalizedString(13041);    //Show File Info
        //dlg.AddLocalizedString(930);    //Add to favorites
        //dlg.AddLocalizedString(931);    //Rating
      }

      dlg.DoModal(GetID);
      if (dlg.SelectedId == -1)
        return;
      switch (dlg.SelectedId)
      {
        case 13041: // Show album info
          OnInfo();
          break;

        case 930: // add to favorites
          //AddSongToFavorites(item);
          break;

        case 931: // Rating
          OnSetRating(facadeView.SelectedListItemIndex);
          break;
      }
    }

    void OnInfo()
    {
      if (null != lastApp)
      {
        selectedItemIndex = GetSelectedItemNo();
        GUIListItem item = GetSelectedItem();
        FileItem curFile = null;
        if (!item.Label.Equals(ProgramUtils.cBackLabel) && (!item.IsFolder))
        {
          if ((item.MusicTag != null) && (item.MusicTag is FileItem))
          {
            curFile = (FileItem)item.MusicTag;
          }
          // show file info but only if the selected item is not the back button
          bool ovVisible = mapSettings.OverviewVisible;
          ProgramInfoAction modalResult = ProgramInfoAction.LookupFileInfo;
          int selectedFileID = -1;
          lastApp.OnInfo(item, ref ovVisible, ref modalResult, ref selectedFileID);
          if ((null != curFile) && (modalResult == ProgramInfoAction.LookupFileInfo))
          {
            FileItem scrapeFile = lastApp.Files.GetFileByID(selectedFileID);
            if (null != scrapeFile)
            {
              int scrapeIndex = lastApp.Files.IndexOf(scrapeFile);
              if (-1 != scrapeIndex)
              {
                GUIControl.SelectItemControl(GetID, facadeView.GetID, scrapeIndex + 1);
              }
              ScrapeFileInfo(scrapeFile);
            }
          }
          mapSettings.OverviewVisible = ovVisible;
          UpdateListControl();
        }
      }
    }

    protected void OnSetRating(int itemNumber)
    {
      GUIListItem item = facadeView[itemNumber];
      if (item == null)
        return;
      FileItem curFile = item.MusicTag as FileItem;
      if (curFile == null)
        return;

      GUIDialogSetRating dialog = (GUIDialogSetRating)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_RATING);

      dialog.Rating = curFile.Rating;
      dialog.SetTitle(String.Format("{0}", curFile.Title));

      dialog.DoModal(GetID);

      facadeView[itemNumber].MusicTag = curFile;
      curFile.Rating = dialog.Rating;
      curFile.Write();

      if (dialog.Result == GUIDialogSetRating.ResultCode.Previous)
      {
        while (itemNumber > 0)
        {
          itemNumber--;
          item = facadeView[itemNumber];
          if (!item.IsFolder && !item.IsRemote)
          {
            OnSetRating(itemNumber);
            return;
          }
        }
      }

      if (dialog.Result == GUIDialogSetRating.ResultCode.Next)
      {
        while (itemNumber + 1 < facadeView.Count)
        {
          itemNumber++;
          item = facadeView[itemNumber];
          if (!item.IsFolder && !item.IsRemote)
          {
            OnSetRating(itemNumber);
            return;
          }
        }
      }
    }

    #endregion

    #region Display

    void UpdateButtonStates()
    {
      GUIPropertyManager.SetProperty("#view", ProgramSettings.viewHandler.LocalizedCurrentView);
      btnRefresh.IsVisible = RefreshButtonVisible();

      // display apptitle if available.....
      if (lastApp != null)
      {
        if ((ProgramSettings.viewHandler.CurrentView != null) && (ProgramSettings.viewHandler.MaxLevels > 0))
        {
          GUIPropertyManager.SetProperty("#curheader", ProgramSettings.viewHandler.LocalizedCurrentView);
        }
        else
        {
          GUIPropertyManager.SetProperty("#curheader", lastApp.Title);
        }
      }
      else
      {
        string strText = ProgramSettings.ReadSetting(ProgramUtils.cPLUGINTITLE);
        if ((strText != "") && (strText != null))
        {
          GUIPropertyManager.SetProperty("#curheader", strText);
        }
        else
        {
          GUIPropertyManager.SetProperty("#curheader", GUILocalizeStrings.Get(0));
        }
      }

      btnViewAs.Label = mapSettings.ViewAsText;
    }

    void SwitchView()
    {
      int itemIndex = facadeView.SelectedListItemIndex;

      switch ((View)mapSettings.ViewAs)
      {
        case View.List:
          facadeView.View = GUIFacadeControl.ViewMode.List;
          screenShotImage.Visible = true;
          break;
        case View.Icons:
          facadeView.View = GUIFacadeControl.ViewMode.SmallIcons;
          screenShotImage.Visible = false;
          break;
        case View.LargeIcons:
          facadeView.View = GUIFacadeControl.ViewMode.LargeIcons;
          screenShotImage.Visible = false;
          break;
        case View.FilmStrip:
          facadeView.View = GUIFacadeControl.ViewMode.Filmstrip;
          screenShotImage.Visible = false;
          break;
      }

      facadeView.SelectedListItemIndex = itemIndex;
      UpdateButtonStates();
    }

    void RenderThumbnail(float timePassed)
    {
      // does the thumb needs replacing??
      long now = (DateTime.Now.Ticks/10000);
      long timeElapsed = now - slideTime;
      if (timeElapsed >= (slideSpeed))
      {
        RefreshThumbnail();
        // only refresh the picture, don't refresh the other data otherwise scrolling of labels is interrupted!
      }
    }

    void RefreshThumbnail()
    {
      AppItem appWithImg = lastApp;
      GUIListItem item = GetSelectedItem();

      // some preconditions...
      if (appWithImg == null)
      {
        if ((item != null) && (item.MusicTag != null) && (item.MusicTag is AppItem))
        {
          appWithImg = (AppItem)(item.MusicTag);
        }
        else
        {
          return;
        }
      }

      ThumbnailPath = "";
      if (item.ThumbnailImage != ""
        && item.ThumbnailImage != GUIGraphicsContext.Skin + @"\media\DefaultFolderBig.png"
        && item.ThumbnailImage != GUIGraphicsContext.Skin + @"\media\DefaultAlbum.png"
        )
      {
        // only show big thumb if there is really one....
        ThumbnailPath = appWithImg.GetCurThumb(item); // some modes look for thumbs differently
      }

      appWithImg.NextThumb(); // try to find a next thumbnail
      slideTime = (DateTime.Now.Ticks/10000); // reset timer!
    }

    bool RefreshButtonVisible()
    {
      if (lastApp == null)
      {
        return false;
      }
      else
      {
        return (lastApp.RefreshButtonVisible() && lastApp.GUIRefreshPossible && lastApp.EnableGUIRefresh);
      }
    }

    bool ThereAreAppsToDisplay()
    {
      if (lastApp == null)
      {
        return true; // root has apps
      }
      else
      {
        return lastApp.SubItemsAllowed(); // grouper items for example
      }
    }

    bool ThereAreFilesOrLinksToDisplay()
    {
      return (lastApp != null); // all apps can have files except the root
    }

    bool IsBackButtonNecessary()
    {
      return (lastApp != null); // always show back button except for root
    }

    void UpdateListControl()
    {
      int TotalItems = 0;
      GUIControl.ClearControl(GetID, facadeView.GetID);

      if (IsBackButtonNecessary())
      {
        ProgramUtils.AddBackButton(facadeView);
      }

      if (ThereAreAppsToDisplay())
      {
        TotalItems = TotalItems + DisplayApps();
      }

      if (ThereAreFilesOrLinksToDisplay())
      {
        TotalItems = TotalItems + DisplayFiles();
      }

      if (lastApp != null)
      {
        facadeView.Sort(new ProgramSort(CurrentSortMethod, CurrentSortAsc));
      }


      string itemCountText = String.Format("{0} {1}", TotalItems, GUILocalizeStrings.Get(632));
      GUIPropertyManager.SetProperty("#itemcount", itemCountText);

      if (selectedItemIndex >= 0)
      {
        GUIControl.SelectItemControl(GetID, facadeView.GetID, selectedItemIndex);
      }
    }

    int DisplayFiles()
    {
      int totalFiles = 0;
      if (lastApp == null)
      {
        return totalFiles;
      }
      totalFiles = lastApp.DisplayFiles(this.lastFilepath, facadeView);
      return (totalFiles);
    }

    int DisplayApps()
    {
      int totalApps = 0;
      foreach (AppItem app in apps.appsOfFatherID(GetCurrentFatherID()))
      {
        if (app.Enabled)
        {
          totalApps = totalApps + 1;
          GUIListItem item = new GUIListItem(app.Title);
          if (app.Imagefile != "")
          {
            item.ThumbnailImage = app.Imagefile;
            item.IconImageBig = app.Imagefile;
            item.IconImage = app.Imagefile;
          }
          else
          {
            if (app.SourceType == myProgSourceType.APPEXEC)
            {
              item.ThumbnailImage = GUIGraphicsContext.Skin + @"\media\DefaultAlbum.png";
              item.IconImageBig = GUIGraphicsContext.Skin + @"\media\DefaultAlbum.png";
              item.IconImage = GUIGraphicsContext.Skin + @"\media\DefaultDVDEmpty.png";
            }
            else
            {
              item.ThumbnailImage = GUIGraphicsContext.Skin + @"\media\DefaultFolderBig.png";
              item.IconImageBig = GUIGraphicsContext.Skin + @"\media\DefaultFolderBig.png";
              item.IconImage = GUIGraphicsContext.Skin + @"\media\DefaultFolderNF.png";
            }
          }
          item.MusicTag = app;
          item.IsFolder = (app.SourceType != myProgSourceType.APPEXEC); // pseudo-folder for all but appexec
          item.OnItemSelected += new GUIListItem.ItemSelectedHandler(OnAppItemSelected);
          facadeView.Add(item);
        }
      }
      return (totalApps);
    }

    string BuildHistoryKey(AppItem app, int viewLevel, string pathSub)
    {
      int appID;
      if (app != null)
      {
        appID = app.AppID;
      }
      else
      {
        appID = 1; // root
      }
      return String.Format("app{0}#level{1}#sub_{2}", appID, viewLevel, pathSub);
    }

    void SaveItemIndex(string value, AppItem app, string pathSub)
    {
      string key = BuildHistoryKey(app, ProgramSettings.viewHandler.CurrentLevel, pathSub);
      itemHistory.Set(value, key);
    }

    void RestoreItemIndex(AppItem app, string pathSub)
    {
      string key = BuildHistoryKey(app, ProgramSettings.viewHandler.CurrentLevel, pathSub);
      string itemHist = itemHistory.Get(key);
      if (itemHist != "")
      {
        int itemIndex = ProgramUtils.StrToIntDef(itemHist, -1);
        if ((itemIndex >= 0) && (itemIndex <= facadeView.Count - 1))
        {
          GUIControl.SelectItemControl(GetID, facadeView.GetID, itemIndex);
        }
      }
    }

    #endregion 

    #region EventHandlers

    void FileItemClicked(GUIListItem item)
    {
      // file item was clicked => launch it!
      if (lastApp != null)
      {
        mapSettings.LastAppID = lastApp.AppID;
        lastFilepath = lastApp.DefaultFilepath();
        lastApp.LaunchFile(item);
      }
      else
      {
        // Hmmm... This must be APPEXEC
        if (item.MusicTag != null)
        {
          if (item.MusicTag is AppItem)
          {
            bool bPinOk = true;
            AppItem candidate = (AppItem)item.MusicTag;
            if (candidate.Pincode > 0)
            {
              bPinOk = candidate.CheckPincode();
            }
            if (bPinOk)
            {
              ((AppItem)item.MusicTag).LaunchFile(item);
            }
          }
        }
      }
    }

    void FolderItemClicked(GUIListItem item)
    {
      if (item.MusicTag != null)
      {
        if (item.MusicTag is AppItem)
        {
          bool bPinOk = true;
          AppItem candidate = (AppItem) item.MusicTag;
          if (candidate.Pincode > 0)
          {
            bPinOk = candidate.CheckPincode();
          }
          if (bPinOk)
          {
            SaveItemIndex(GetSelectedItemNo().ToString(), lastApp, lastFilepath);
            lastApp = candidate;
            mapSettings.LastAppID = lastApp.AppID;
            lastFilepath = lastApp.DefaultFilepath();
            ProgramSettings.viewHandler.CurrentLevel = 0;
          }
        }
        else if (item.MusicTag is FileItem)
        {
          SaveItemIndex(GetSelectedItemNo().ToString(), lastApp, lastFilepath);
          // example: subfolder in directory-cache mode
          // => set filepath which will be a search criteria for sql / browse
          if (lastFilepath == "")
          {
            // first subfolder
            lastFilepath = lastApp.FileDirectory + "\\" + item.Label;
          }
          else
          {
            // subsequent subfolder
            lastFilepath = lastFilepath + "\\" + item.Label;
          }
        }
        else if (item.MusicTag is ProgramFilterItem)
        {
          AddNextFilter(item);
        }
        UpdateListControl();
      }
      else
      {
        // tag is null
        // example: subfolder in directory-browse mode
        SaveItemIndex(GetSelectedItemNo().ToString(), lastApp, lastFilepath);
        lastFilepath = item.Path;
        UpdateListControl();
      }
    }

    void BackItemClicked()
    {
      if (lastApp != null)
      {
        if ((lastFilepath != null) && (lastFilepath != "") && (lastFilepath != lastApp.FileDirectory))
        {
          // back item in filelist clicked
          string newFilepath = Path.GetDirectoryName(lastFilepath);
          lastFilepath = newFilepath;
        }
        else
        {
          if (ProgramSettings.viewHandler.RemoveFilterItem())
          {
            ProgramFilterItem curFilter;
            // force reload, this will load the next filter-level.....
            lastApp.LoadFiles();
            // auto-remove filters if there is only ONE EMPTY Filteritem
            // displaying
            bool doAutoRemove = ((lastApp.Files.Count == 1) && (ProgramSettings.viewHandler.IsFilterQuery));
            while (doAutoRemove)
            {
              doAutoRemove = false;
              if (lastApp.Files[0] is ProgramFilterItem)
              {
                curFilter = lastApp.Files[0] as ProgramFilterItem;
                if ((curFilter.Title == "") && (curFilter.Title2 == ""))
                {
                  if (ProgramSettings.viewHandler.RemoveFilterItem())
                  {
                    lastApp.LoadFiles();
                    doAutoRemove = ((lastApp.Files.Count == 1) && (ProgramSettings.viewHandler.IsFilterQuery));
                  }
                }
              }
            }
          }
          else
          {
            // back item in application list clicked
            // go to father item
            lastApp = apps.GetAppByID(lastApp.FatherID);
            if (lastApp != null)
            {
              mapSettings.LastAppID = lastApp.AppID;
              lastFilepath = lastApp.DefaultFilepath();
            }
            else
            {
              // back to home screen.....
              mapSettings.LastAppID = - 1;
              lastFilepath = "";
            }
          }
        }
        UpdateButtonStates();
        UpdateListControl();
        RestoreItemIndex(lastApp, lastFilepath);
      }
      else
      {
        // from root.... go back to main menu
        GUIWindowManager.ShowPreviousWindow();
      }


    }

    void AddNextFilter(GUIListItem item)
    {
      ProgramFilterItem curFilter;
      SaveItemIndex(GetSelectedItemNo().ToString(), lastApp, lastFilepath);
      ProgramSettings.viewHandler.AddFilterItem(item.MusicTag as ProgramFilterItem);
      if (lastApp != null)
      {
        // force reload, this will load the next filter-level.....
        lastApp.LoadFiles();
        // check if the next filter is only displaying ONE EMPTY item
        // if yes, autoselect this filteritem
        bool doAutoSelect = ((lastApp.Files.Count == 1) && (ProgramSettings.viewHandler.IsFilterQuery));
        while (doAutoSelect)
        {
          doAutoSelect = false;
          if (lastApp.Files[0] is ProgramFilterItem)
          {
            curFilter = lastApp.Files[0] as ProgramFilterItem;
            if ((curFilter.Title == "") && (curFilter.Title2 == ""))
            {
              ProgramSettings.viewHandler.AddFilterItem(curFilter);
              lastApp.LoadFiles();
              doAutoSelect = ((lastApp.Files.Count == 1) && (ProgramSettings.viewHandler.IsFilterQuery));
            }
          }
        }
      }
    }

    void OnAppItemSelected(GUIListItem item, GUIControl parent)
    {
      ThumbnailPath = "";
      if (item.ThumbnailImage != ""
        && item.ThumbnailImage != GUIGraphicsContext.Skin + @"\media\DefaultFolderBig.png"
        && item.ThumbnailImage != GUIGraphicsContext.Skin + @"\media\DefaultAlbum.png")
      {
        // only show big thumb if there is really one....
        ThumbnailPath = item.ThumbnailImage;
      }

      GUIFilmstripControl filmstrip = parent as GUIFilmstripControl;
      if (filmstrip != null)
      {
        filmstrip.InfoImageFileName = ThumbnailPath;
      }
    }

    void OnClick()
    {
      GUIListItem item = GetSelectedItem();
      if (!item.IsFolder)
      {
        selectedItemIndex = GetSelectedItemNo();
        // non-folder item clicked => always a fileitem!
        FileItemClicked(item);
      }
      else
      {
        // folder-item clicked.... 
        selectedItemIndex = - 1;
        if (item.Label.Equals(ProgramUtils.cBackLabel))
        {
          BackItemClicked();
          UpdateButtonStates();
        }
        else
        {
          // application-item or subfolder
          FolderItemClicked(item);
          UpdateButtonStates();
        }
      }
    }

    #endregion 
  }
}
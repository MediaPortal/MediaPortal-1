/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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

using System;
using MediaPortal.Dialogs;
using Microsoft.DirectX.Direct3D;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using ProgramsDatabase;
using Programs.Utils;

namespace WindowPlugins.GUIPrograms
{
  /// <summary>
  /// 
  /// </summary>
  public class GUIFileInfo: GUIWindow
  {
    #region SkinControls

    // Labels
    [SkinControlAttribute(20)]
    protected GUILabelControl lblTitle = null;
    [SkinControlAttribute(31)]
    protected GUILabelControl lblSystemCaption = null;
    [SkinControlAttribute(32)]
    protected GUILabelControl lblYearManuCaption = null;
    [SkinControlAttribute(33)]
    protected GUILabelControl lblRatingCaption = null;
    [SkinControlAttribute(34)]
    protected GUILabelControl lblGenreCaption = null;

    // Fadelabels
    [SkinControlAttribute(21)]
    protected GUIFadeLabel lblSystemData = null;
    [SkinControlAttribute(22)]
    protected GUIFadeLabel lblYearManuData = null;
    [SkinControlAttribute(23)]
    protected GUIFadeLabel lblRatingData = null;
    [SkinControlAttribute(24)]
    protected GUIFadeLabel lblGenreData = null;


    // Textbox                   
    [SkinControlAttribute(4)]
      //    protected GUITextScrollUpControl tbOverviewData = null;
    protected GUITextControl tbOverviewData = null;

    //Images                     
    [SkinControlAttribute(3)]
    protected GUIImage imgSmall = null;
    [SkinControlAttribute(10)]
    protected GUIImage imgBig = null;

    // Buttons                   
    [SkinControlAttribute(7)]
    protected GUIButtonControl btnPrev = null;
    [SkinControlAttribute(8)]
    protected GUIButtonControl btnLaunch = null;
    [SkinControlAttribute(9)]
    protected GUIButtonControl btnNext = null;
    [SkinControlAttribute(11)]
    protected GUIButtonControl btnToggleOverview = null;
    [SkinControlAttribute(12)]
    protected GUIButtonControl btnRefreshData = null;

    #endregion 

    #region Base & Content Variables

    bool isRunning = false;
    int parentWindowID = 0;
    GUIWindow parentWindow = null;
    Texture curTexture = null;
    FileItem curFile = null;
    AppItem curApp = null;
    int textureWidth = 0;
    int textureHeight = 0;
    bool isOverlay = false;
    bool isOverviewVisible = true;
    int slideSpeed = 3;
    long slideTime = 0;

    string programSystemLabel = "";
    string programManufacturerLabel = "";
    string programRatingLabel = "";
    string programGenreLabel = "";

    string programSystem = "";
    string programManufacturer = "";
    string programRating = "";
    string programGenre = "";

    string programOverview = "";

    ProgramInfoAction modalResult = ProgramInfoAction.None;
    int selectedFileID = -1;

    #endregion 

    #region Properties / Helper Routines
    public ProgramInfoAction ModalResult
    {
      get {return modalResult;}
      set {modalResult = value;}
    }

    public int SelectedFileID
    {
      get 
      { return selectedFileID;}
    }

    void SyncFileID()
    {
      if (null != curFile)
      {
        selectedFileID = curFile.FileID;
      }
      else
      {
        selectedFileID = -1;
      }
    }

    #endregion

    #region Constructor / Destructor

    public GUIFileInfo()
    {
      GetID = ProgramUtils.ProgramInfoID;
    }

    #endregion 

    #region Properties

    public FileItem File
    {
      set
      {
        curFile = value;
        SyncFileID();
      }
    }

    public AppItem App
    {
      set
      {
        curApp = value;
        if (curApp != null)
          curApp.ResetThumbs();
      }
    }

    public bool IsOverviewVisible
    {
      get
      {
        return isOverviewVisible;
      }
      set
      {
        isOverviewVisible = value;
      }
    }

    #endregion 

    #region Overrides

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\DialogFileInfo.xml");
    }


    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      isOverlay = GUIGraphicsContext.Overlay;
      curTexture = null;

      if (curApp != null)
      {
        curApp.ResetThumbs();
      }
      // if there is no overview text, default to bigger pictures
      if (curFile != null)
      {
        if (curFile.Overview == "")
        {
          this.isOverviewVisible = false;
        }
      }
      Refresh();
    }


    protected override void OnPageDestroy(int newWindowId)
    {
      curFile = null;
      if (curTexture != null)
      {
        curTexture.Dispose();
        curTexture = null;
      }
      GUIGraphicsContext.Overlay = isOverlay;
      base.OnPageDestroy(newWindowId);
    }


    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      if (control == btnPrev)
      {
        curFile = curApp.PrevFile(curFile);
        SyncFileID();
        curApp.ResetThumbs();
        Refresh();
      }
      else if (control == btnNext)
      {
        curFile = curApp.NextFile(curFile);
        SyncFileID();
        curApp.ResetThumbs();
        Refresh();
      }
      else if (control == btnLaunch)
      {
        if (curApp != null)
        {
          curApp.LaunchFile(curFile, true);
          Refresh();
        }

      }
      else if (control == btnToggleOverview)
      {
        isOverviewVisible = !isOverviewVisible;
        Refresh();
      }
      else if (control == this.btnRefreshData)
      {
        Close(ProgramInfoAction.LookupFileInfo);
//        RefreshData();
//        Refresh();
      }
    }


    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_CLOSE_DIALOG || action.wID == Action.ActionType.ACTION_PARENT_DIR  || action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        Close(ProgramInfoAction.None);
        return ;
      }
      base.OnAction(action);
    }

    public override void Render(float timePassed)
    {
      RenderDlg(timePassed);

      if (null == curTexture)
        return ;

      // does the thumb needs replacing??
      long timeElapsed = (DateTime.Now.Ticks / 10000) - slideTime;
      if (timeElapsed >= (slideSpeed * 1000))
      {
        RefreshPicture(); // only refresh the picture, don't refresh the other data otherwise scrolling of labels is interrupted!
      }


      GUIControl curImg = null;
      if (this.isOverviewVisible)
      {
        curImg = imgSmall;
      }
      else
      {
        curImg = imgBig;
      }
      if (curImg != null)
      {
        float x = (float)curImg.XPosition;
        float y = (float)curImg.YPosition;
        int curWidth;
        int curHeight;
        GUIGraphicsContext.Correct(ref x, ref y);

        int maxWidth = curImg.Width;
        int maxHeight = curImg.Height;
        GUIGraphicsContext.GetOutputRect(textureWidth, textureHeight, maxWidth, maxHeight, out curWidth, out curHeight);
        GUIFontManager.Present();
        int deltaX = ((curImg.Width - curWidth) / 2);
        if (deltaX < 0)
        {
          deltaX = 0;
        }
        int deltaY = ((curImg.Height - curHeight) / 2);
        if (deltaY < 0)
        {
          deltaY = 0;
        }
        x = x + deltaX;
        y = y + deltaY;
        Picture.RenderImage(ref curTexture, (int)x, (int)y, curWidth, curHeight, textureWidth, textureHeight, 0, 0, true);
      }
    }

    #endregion 

    #region Display

    void Close(ProgramInfoAction res)
    {
      modalResult = res;
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, GetID, 0, 0, 0, 0, null);
      OnMessage(msg);

      GUIWindowManager.UnRoute();
      parentWindow = null;
      isRunning = false;
    }


    public void DoModal(int parentId)
    {
      parentWindowID = parentId;
      parentWindow = GUIWindowManager.GetWindow(parentWindowID);
      if (null == parentWindow)
      {
        parentWindowID = 0;
        return ;
      }

      GUIWindowManager.RouteToWindow(GetID);

      // activate this window...
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, GetID, 0, 0, 0, 0, null);
      OnMessage(msg);

      isRunning = true;
      while (isRunning && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
      {
        GUIWindowManager.Process();
      }
    }

    void RefreshPicture()
    {
      if (curTexture != null)
      {
        curTexture.Dispose();
        curTexture = null;
      }

      if (curFile != null)
      {
        string thumbFile = curApp.GetCurThumb(curFile);
        // load the found thumbnail picture
        if (System.IO.File.Exists(thumbFile))
        {
          curTexture = Picture.Load(thumbFile, 0, 512, 512, true, false, out textureWidth, out textureHeight);
        }
        curApp.NextThumb(); // try to find a next thumbnail
      }
      slideTime = (DateTime.Now.Ticks / 10000); // reset timer!
    }


    void Refresh()
    {
      RefreshPicture();
      Update();
    }


    void Update()
    {
      if (null == curFile)
        return ;

      ReadContent();

      if (isOverviewVisible)
      {
        imgBig.IsVisible = false;
        tbOverviewData.IsVisible = true;
        tbOverviewData.Clear();
        tbOverviewData.Label = programOverview; // ... and set text next!
        btnToggleOverview.Label = GUILocalizeStrings.Get(13006);
      }
      else
      {
        imgBig.IsVisible = true;
        tbOverviewData.IsVisible = false;
        tbOverviewData.Clear();
        btnToggleOverview.Label = GUILocalizeStrings.Get(13007);
      }

      lblTitle.Label = curFile.Title;

      // if any title is overwritten, re-set the fresh text
      if (programSystemLabel != "")
      {
        lblSystemCaption.Label = programSystemLabel;
      }
      else
      {
        lblSystemCaption.Label = GUILocalizeStrings.Get(13000);
      }
      if (programManufacturerLabel != "")
      {
        lblYearManuCaption.Label = programManufacturerLabel;
      }
      else
      {
        lblYearManuCaption.Label = GUILocalizeStrings.Get(13001);
      }

      if (programRatingLabel != "")
      {
        lblRatingCaption.Label = programRatingLabel;
      }
      else
      {
        lblRatingCaption.Label = GUILocalizeStrings.Get(173);
      }
      if (programGenreLabel != "")
      {
        lblGenreCaption.Label = programGenreLabel;
      }
      else
      {
        lblGenreCaption.Label = GUILocalizeStrings.Get(174);
      }

      lblSystemData.Label = programSystem;
      lblYearManuData.Label = programManufacturer;
      lblRatingData.Label = programRating;
      lblGenreData.Label = programGenre;

      if (curFile.Filename != "")
      {
        btnLaunch.Disabled = false;
      }
      else
      {
        btnLaunch.Disabled = true;
      }

    }


    void ReadContent()
    {
      string notAvailableText = GUILocalizeStrings.Get(416);
      // read fields out of the content profile
      // fields can contain texts and / or references to fields
      programSystemLabel = ProgramContentManager.GetFieldValue(curApp, curFile, "Line1Label", "");
      programManufacturerLabel = ProgramContentManager.GetFieldValue(curApp, curFile, "Line2Label", "");
      programRatingLabel = ProgramContentManager.GetFieldValue(curApp, curFile, "Line3Label", "");
      programGenreLabel = ProgramContentManager.GetFieldValue(curApp, curFile, "Line4Label", "");

      programSystem = ProgramContentManager.GetFieldValue(curApp, curFile, "Line1Data", notAvailableText);
      programManufacturer = ProgramContentManager.GetFieldValue(curApp, curFile, "Line2Data", notAvailableText);
      programRating = ProgramContentManager.GetFieldValue(curApp, curFile, "Line3Data", notAvailableText);
      programGenre = ProgramContentManager.GetFieldValue(curApp, curFile, "Line4Data", notAvailableText);
      programOverview = ProgramContentManager.GetFieldValue(curApp, curFile, "OverviewData", "");
    }

    public void RenderDlg(float timePassed)
    {
      // render the parent window
      if (null != parentWindow)
        parentWindow.Render(timePassed);
      GUIFontManager.Present();
      // render this dialog box
      base.Render(timePassed);
    }

    #endregion 
  }
}

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

using MediaPortal.Video.Database;
using MediaPortal.GUI.Library;
using MediaPortal.Util;

namespace MediaPortal.GUI.Video
{
  /// <summary>
  /// 
  /// </summary>
  public class GUIVideoArtistInfo : GUIWindow, IRenderLayer
  {
    [SkinControlAttribute(3)]     protected GUIToggleButtonControl btnBiography = null;
    [SkinControlAttribute(4)]     protected GUIToggleButtonControl btnMovies = null;
    [SkinControlAttribute(20)]    protected GUITextScrollUpControl tbPlotArea = null;
    [SkinControlAttribute(21)]    protected GUIImage imgCoverArt = null;
    [SkinControlAttribute(22)]    protected GUITextControl tbTextArea = null;

    enum ViewMode
    {
      Biography,
      Movies,
    }

    #region Base Dialog Variables
    bool m_bRunning = false;
    int m_dwParentWindowID = 0;
    GUIWindow m_pParentWindow = null;

    #endregion


    ViewMode viewmode = ViewMode.Biography;

    IMDBActor currentActor = null;
    bool _prevOverlay = false;
    string imdbCoverArtUrl = string.Empty;

    public GUIVideoArtistInfo()
    {
      GetID = (int)GUIWindow.Window.WINDOW_VIDEO_ARTIST_INFO;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\DialogVideoArtistInfo.xml");
    }

    public override void PreInit()
    {
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {
        Close();
        return;
      }
      base.OnAction(action);
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          _prevOverlay = GUIGraphicsContext.Overlay;
          base.OnMessage(message);
          return true;
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
          base.OnMessage(message);
          GUIGraphicsContext.Overlay = _prevOverlay;
          return true;
      }
      return base.OnMessage(message);
    }

    #region Base Dialog Members 

    void Close()
    {
      GUIWindowManager.IsSwitchingToNewWindow = true;
      lock (this)
      {
        m_bRunning = false;
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, GetID, 0, 0, 0, 0, null);
        OnMessage(msg);

        GUIWindowManager.UnRoute();
        m_pParentWindow = null;
      }
      GUIWindowManager.IsSwitchingToNewWindow = false;
    }

    public void DoModal(int dwParentId)
    {
      m_dwParentWindowID = dwParentId;
      m_pParentWindow = GUIWindowManager.GetWindow(m_dwParentWindowID);
      if (null == m_pParentWindow)
      {
        m_dwParentWindowID = 0;
        return;
      }

      GUIWindowManager.IsSwitchingToNewWindow = true;
      GUIWindowManager.RouteToWindow(GetID);

      // active this window...
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, GetID, 0, 0, 0, 0, null);
      OnMessage(msg);

      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Dialog);

      GUIWindowManager.IsSwitchingToNewWindow = false;
      m_bRunning = true;
      while (m_bRunning && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
      {
        GUIWindowManager.Process();
      }
      GUILayerManager.UnRegisterLayer(this);
    }
    #endregion

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      Update();
    }
    protected override void OnPageDestroy(int newWindowId)
    {
      if (m_bRunning)
      {
        // User probably pressed H (SWITCH_HOME)
        m_bRunning = false;
        GUIWindowManager.UnRoute();
        m_pParentWindow = null;
      }

      base.OnPageDestroy(newWindowId);
      currentActor = null;
    }


    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);

      if (control == btnMovies)
      {
        viewmode = ViewMode.Movies;
        Update();
      }
      if (control == btnBiography)
      {
        viewmode = ViewMode.Biography;
        Update();
      }
    }
    
    public IMDBActor Actor
    {
      get { return currentActor; }
      set { currentActor = value; }
    }

    void Update()
    {
      if (currentActor == null) return;

      //cast->image
      if (viewmode == ViewMode.Movies)
      {
        tbPlotArea.IsVisible = false;
        tbTextArea.IsVisible = true;
        imgCoverArt.IsVisible = true;
        btnBiography.Selected = false;
        btnMovies.Selected = true;
      }
      //cast->plot
      if (viewmode == ViewMode.Biography)
      {
        tbPlotArea.IsVisible = true;
        tbTextArea.IsVisible = false;
        imgCoverArt.IsVisible = true;
        btnBiography.Selected = true;
        btnMovies.Selected = false;

      }
      GUIPropertyManager.SetProperty("#Actor.Name", currentActor.Name);
      GUIPropertyManager.SetProperty("#Actor.DateOfBirth", currentActor.DateOfBirth);
      GUIPropertyManager.SetProperty("#Actor.PlaceOfBirth", currentActor.PlaceOfBirth);
      string biography = currentActor.Biography;
      if ((biography == string.Empty) || ( biography == Strings.Unknown))
      {
        biography = currentActor.MiniBiography;
        if (biography == Strings.Unknown)
        {
          biography = "";
        }
      }
      GUIPropertyManager.SetProperty("#Actor.Biography", biography);
      string movies = "";
      for (int i = 0; i < currentActor.Count; ++i)
      {
        string line = String.Format("{0}. {1} ({2})\n            {3}\n", i + 1, currentActor[i].MovieTitle, currentActor[i].Year, currentActor[i].Role);
        movies += line;
      }
      GUIPropertyManager.SetProperty("#Actor.Movies", movies);

      string largeCoverArtImage = MediaPortal.Util.Utils.GetLargeCoverArtName(Thumbs.MovieActors, currentActor.Name);
      if (imgCoverArt != null)
      {
        imgCoverArt.FreeResources();
        imgCoverArt.SetFileName(largeCoverArtImage);
        imgCoverArt.AllocResources();
      }

    }

 
    #region IRenderLayer
    public bool ShouldRenderLayer()
    {
      return true;
    }

    public void RenderLayer(float timePassed)
    {
      Render(timePassed);
    }
    #endregion
  }
}

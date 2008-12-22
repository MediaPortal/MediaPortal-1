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

using Microsoft.DirectX.Direct3D;

using MediaPortal.GUI.Library;
using MediaPortal.Music.Database;
using MediaPortal.TagReader;

namespace MediaPortal.GUI.Music
{
  /// <summary>
  /// 
  /// </summary> 
  public class GUIMusicInfo : GUIWindow, IRenderLayer
  {
    [SkinControlAttribute(20)]
    protected GUILabelControl lblAlbum = null;
    [SkinControlAttribute(21)]
    protected GUILabelControl lblArtist = null;
    [SkinControlAttribute(22)]
    protected GUILabelControl lblDate = null;
    [SkinControlAttribute(23)]
    protected GUILabelControl lblRating = null;
    [SkinControlAttribute(24)]
    protected GUILabelControl lblGenre = null;
    [SkinControlAttribute(25)]
    protected GUIFadeLabel lblTone = null;
    [SkinControlAttribute(26)]
    protected GUIFadeLabel lblStyles = null;
    [SkinControlAttribute(3)]
    protected GUIImage imgCoverArt = null;
    [SkinControlAttribute(4)]
    protected GUITextControl tbTextArea = null;
    [SkinControlAttribute(5)]
    protected GUIButtonControl btnTracks = null;
    [SkinControlAttribute(6)]
    protected GUIButtonControl btnRefresh = null;

    #region Base Dialog Variables
    bool m_bRunning = false;
    bool needsRefresh = false;
    int m_dwParentWindowID = 0;
    GUIWindow m_pParentWindow = null;

    #endregion

    Texture coverArtTexture = null;
    bool showReview = false;
    MusicAlbumInfo albumInfo = null;
    MusicTag m_tag = null;
    int coverArtTextureWidth = 0;
    int coverArtTextureHeight = 0;
    bool _prevOverlay = false;

    public GUIMusicInfo()
    {
      GetID = (int)GUIWindow.Window.WINDOW_MUSIC_INFO;
    }
    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\DialogAlbumInfo.xml");
    }
    public override void PreInit()
    {
    }

    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU || action.wID == Action.ActionType.ACTION_CONTEXT_MENU)
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
    public void RenderDlg(float timePassed)
    {
      base.Render(timePassed);
    }

    void Close()
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, GetID, 0, 0, 0, 0, null);
      OnMessage(msg);

      GUIWindowManager.UnRoute();
      m_pParentWindow = null;
      m_bRunning = false;
    }

    public void DoModal(int dwParentId)
    {
      needsRefresh = false;
      m_dwParentWindowID = dwParentId;
      m_pParentWindow = GUIWindowManager.GetWindow(m_dwParentWindowID);
      if (null == m_pParentWindow)
      {
        m_dwParentWindowID = 0;
        return;
      }

      GUIWindowManager.RouteToWindow(GetID);

      // active this window...
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT, GetID, 0, 0, 0, 0, null);
      OnMessage(msg);

      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Dialog);
      m_bRunning = true;
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.Dialog);
      while (m_bRunning && GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
      {
        GUIWindowManager.Process();
      }
      GUILayerManager.UnRegisterLayer(this);
    }
    #endregion


    protected override void OnPageDestroy(int newWindowId)
    {
      if (m_bRunning)
      {
        // Probably user pressed H (SWITCH_HOME)
        GUIWindowManager.UnRoute();
        m_pParentWindow = null;
        m_bRunning = false;
      }
      albumInfo = null;
      if (coverArtTexture != null)
      {
        coverArtTexture.Dispose();
        coverArtTexture = null;
      }
      base.OnPageDestroy(newWindowId);
    }
    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      coverArtTexture = null;
      showReview = true;
      Refresh();
    }

    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      if (control == btnRefresh)
      {
        string imageFileName = albumInfo.ImageURL;
        string thumbNailFileName = Util.Utils.GetAlbumThumbName(m_tag.Artist, m_tag.Album);
        MediaPortal.Util.Utils.FileDelete(thumbNailFileName);
        needsRefresh = true;
        Close();
        return;
      }

      if (control == btnTracks)
      {
        showReview = !showReview;
        Update();
      }
    }


    public MusicAlbumInfo Album
    {
      set { albumInfo = value; }
    }

    void Update()
    {
      if (null == albumInfo) return;
      string strTmp;
      lblAlbum.Label = albumInfo.Title;
      lblArtist.Label = albumInfo.Artist;
      lblDate.Label = albumInfo.DateOfRelease;

      string rating = string.Empty;
      if (albumInfo.Rating > 0)
        rating = String.Format("{0}/9", albumInfo.Rating);
      lblRating.Label = rating;
      lblGenre.Label = albumInfo.Genre;

      lblTone.Clear();
      lblTone.Add(albumInfo.Tones.Trim());
      strTmp = albumInfo.Tones; strTmp.Trim();

      lblStyles.Clear();
      lblStyles.Add(albumInfo.Styles.Trim());

      if (showReview)
      {
        tbTextArea.Clear();
        tbTextArea.Label = albumInfo.Review;
        btnTracks.Label = GUILocalizeStrings.Get(182);
      }
      else
      {
        string line = string.Empty;
        for (int i = 0; i < albumInfo.NumberOfSongs; ++i)
        {
          MusicSong song = albumInfo.GetSong(i);
          string track = String.Format("{0}. {1}\n",
                  song.Track,
                  song.SongName);
          line += track;
        };

        tbTextArea.Label = line;

        for (int i = 0; i < albumInfo.NumberOfSongs; ++i)
        {
          MusicSong song = albumInfo.GetSong(i);
          line = MediaPortal.Util.Utils.SecondsToHMSString(song.Duration);
          GUIMessage msg1 = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL2_SET, GetID, 0, tbTextArea.GetID, i, 0, null);
          msg1.Label = (line);
          OnMessage(msg1);
        }

        btnTracks.Label = GUILocalizeStrings.Get(183);
      }
    }

    public override void Render(float timePassed)
    {
      RenderDlg(timePassed);

      if (null == coverArtTexture) return;

      if (null != imgCoverArt)
      {
        float x = (float)imgCoverArt.XPosition;
        float y = (float)imgCoverArt.YPosition;
        int width;
        int height;
        GUIGraphicsContext.Correct(ref x, ref y);

        int maxWidth = imgCoverArt.Width;
        int maxHeight = imgCoverArt.Height;
        GUIGraphicsContext.GetOutputRect(coverArtTextureWidth, coverArtTextureHeight, maxWidth, maxHeight, out width, out height);

        GUIFontManager.Present();
        MediaPortal.Util.Picture.RenderImage(coverArtTexture, (int)x, (int)y, width, height, coverArtTextureWidth, coverArtTextureHeight, 0, 0, true);
      }
    }


    void Refresh()
    {
      if (coverArtTexture != null)
      {
        coverArtTexture.Dispose();
        coverArtTexture = null;
      }

      string thumbNailFileName;
      string imageFileName = albumInfo.ImageURL;
      if (m_tag == null)
      {
        m_tag = new MusicTag();
        m_tag.Artist = albumInfo.Artist;
        m_tag.Album = albumInfo.Title;
      }
      thumbNailFileName = Util.Utils.GetAlbumThumbName(m_tag.Artist, m_tag.Album);
      if (!System.IO.File.Exists(thumbNailFileName))
      {
        //	Download image and save as 
        //	permanent thumb
        MediaPortal.Util.Utils.DownLoadImage(imageFileName, thumbNailFileName);
      }

      if (System.IO.File.Exists(thumbNailFileName))
      {
        coverArtTexture = MediaPortal.Util.Picture.Load(thumbNailFileName, 0, 128, 128, true, false, out coverArtTextureWidth, out coverArtTextureHeight);
        //imgCoverArt.FreeResources();
        //imgCoverArt.AllocResources();
      }
      Update();
    }


    public MusicTag Tag
    {
      get { return m_tag; }
      set { m_tag = value; }
    }

    public bool NeedsRefresh
    {
      get { return needsRefresh; }
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

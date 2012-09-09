#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Music.Database;
using MediaPortal.TagReader;
using Microsoft.DirectX.Direct3D;
using Action = MediaPortal.GUI.Library.Action;

namespace MediaPortal.GUI.Music
{
  /// <summary>
  /// 
  /// </summary> 
  public class GUIMusicInfo : GUIDialogWindow
  {
    [SkinControl(20)] protected GUILabelControl lblAlbum = null;
    [SkinControl(21)] protected GUILabelControl lblArtist = null;
    [SkinControl(22)] protected GUILabelControl lblDate = null;
    [SkinControl(23)] protected GUILabelControl lblRating = null;
    [SkinControl(24)] protected GUILabelControl lblGenre = null;
    [SkinControl(25)] protected GUIFadeLabel lblTone = null;
    [SkinControl(26)] protected GUIFadeLabel lblStyles = null;
    [SkinControl(3)] protected GUIImage imgCoverArt = null;
    [SkinControl(4)] protected GUITextControl tbTextArea = null;
    [SkinControl(5)] protected GUIButtonControl btnTracks = null;
    [SkinControl(6)] protected GUIButtonControl btnRefresh = null;

    private bool needsRefresh = false;
    private Texture coverArtTexture = null;
    private bool showReview = false;
    private MusicAlbumInfo albumInfo = null;
    private MusicTag m_tag = null;
    private int coverArtTextureWidth = 0;
    private int coverArtTextureHeight = 0;

    public GUIMusicInfo()
    {
      GetID = (int)Window.WINDOW_MUSIC_INFO;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\DialogAlbumInfo.xml"));
    }

    public override void DoModal(int ParentID)
    {
      needsRefresh = false;
      AllocResources();
      InitControls();

      base.DoModal(ParentID);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
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

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      if (control == btnRefresh)
      {
        string imageFileName = albumInfo.ImageURL;
        string thumbNailFileName = Util.Utils.GetAlbumThumbName(m_tag.Artist, m_tag.Album);
        Util.Utils.FileDelete(thumbNailFileName);
        needsRefresh = true;
        PageDestroy();
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

    private void Update()
    {
      if (null == albumInfo)
      {
        return;
      }
      lblAlbum.Label = albumInfo.Title;
      lblArtist.Label = albumInfo.Artist;
      lblDate.Label = albumInfo.DateOfRelease;

      string rating = string.Empty;
      if (albumInfo.Rating > 0)
      {
        rating = String.Format("{0}/9", albumInfo.Rating);
      }
      lblRating.Label = rating;
      lblGenre.Label = albumInfo.Genre;
      lblTone.Label = albumInfo.Tones.Trim();
      lblStyles.Label = albumInfo.Styles.Trim();

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
        }
        ;

        tbTextArea.Label = line;

        for (int i = 0; i < albumInfo.NumberOfSongs; ++i)
        {
          MusicSong song = albumInfo.GetSong(i);
          line = Util.Utils.SecondsToHMSString(song.Duration);
          GUIMessage msg1 = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL2_SET, GetID, 0, tbTextArea.GetID, i, 0,
                                           null);
          msg1.Label = (line);
          OnMessage(msg1);
        }

        btnTracks.Label = GUILocalizeStrings.Get(183);
      }
    }

    public override void Render(float timePassed)
    {
      base.Render(timePassed);

      if (null == coverArtTexture)
      {
        return;
      }

      if (null != imgCoverArt)
      {
        float x = (float)imgCoverArt.XPosition;
        float y = (float)imgCoverArt.YPosition;
        int width;
        int height;
        GUIGraphicsContext.Correct(ref x, ref y);

        int maxWidth = imgCoverArt.Width;
        int maxHeight = imgCoverArt.Height;
        GUIGraphicsContext.GetOutputRect(coverArtTextureWidth, coverArtTextureHeight, maxWidth, maxHeight, out width,
                                         out height);

        GUIFontManager.Present();
        Util.Picture.RenderImage(coverArtTexture, (int)x, (int)y, width, height, coverArtTextureWidth,
                                 coverArtTextureHeight, 0, 0, true);
      }
    }


    private void Refresh()
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
      if (!Util.Utils.FileExistsInCache(thumbNailFileName))
      {
        //	Download image and save as 
        //	permanent thumb
        Util.Utils.DownLoadImage(imageFileName, thumbNailFileName);
      }

      if (Util.Utils.FileExistsInCache(thumbNailFileName))
      {
        coverArtTexture = Util.Picture.Load(thumbNailFileName, 0, 128, 128, true, false, out coverArtTextureWidth,
                                            out coverArtTextureHeight);
        //imgCoverArt.Dispose();
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

  }
}
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
using System.ComponentModel;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Music.Database;
using Action = MediaPortal.GUI.Library.Action;

namespace MediaPortal.GUI.Music
{
  /// <summary>
  /// 
  /// </summary> 
  public class GUIMusicInfo : GUIDialogWindow
  {
    [SkinControl(6)] protected GUIButtonControl btnRefresh = null;

    private bool needsRefresh = false;
    private MusicAlbumInfo albumInfo = null;
    private BackgroundWorker bw;

    public GUIMusicInfo()
    {
      GetID = (int)Window.WINDOW_MUSIC_INFO;
      bw = new BackgroundWorker();
      bw.DoWork += bw_DoWork;
      bw.RunWorkerCompleted += bw_RunWorkerCompleted;
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
      base.OnPageDestroy(newWindowId);
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();

      GUIPropertyManager.SetProperty("#AlbumInfo.Thumb", string.Empty);
      GUIPropertyManager.SetProperty("#AlbumInfo.Title", string.Empty);
      GUIPropertyManager.SetProperty("#AlbumInfo.Artist", string.Empty);
      GUIPropertyManager.SetProperty("#AlbumInfo.Year", string.Empty);
      GUIPropertyManager.SetProperty("#AlbumInfo.Rating", string.Empty);
      GUIPropertyManager.SetProperty("#AlbumInfo.Genre", string.Empty);
      GUIPropertyManager.SetProperty("#AlbumInfo.Tones", string.Empty);
      GUIPropertyManager.SetProperty("#AlbumInfo.Styles", string.Empty);
      GUIPropertyManager.SetProperty("#AlbumInfo.Review", string.Empty);
      GUIPropertyManager.SetProperty("#AlbumInfo.Tracks", string.Empty);

      Update();
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      if (control == btnRefresh)
      {
        needsRefresh = true;
        PageDestroy();
        return;
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

      var rating = string.Empty;
      if (albumInfo.Rating > 0)
      {
        rating = String.Format("{0}", albumInfo.Rating);
      }

      string thumbNailFileName = Util.Utils.GetAlbumThumbName(albumInfo.Artist, albumInfo.Title);
      if (Util.Utils.FileExistsInCache(thumbNailFileName))
      {
        string strLarge = Util.Utils.ConvertToLargeCoverArt(thumbNailFileName);
        if (Util.Utils.FileExistsInCache(strLarge))
        {
          thumbNailFileName = strLarge;
        }
        GUIPropertyManager.SetProperty("#AlbumInfo.Thumb", thumbNailFileName);
      }
      else if (!string.IsNullOrEmpty(albumInfo.ImageURL))
      {
        bw.RunWorkerAsync();
      }
      
      GUIPropertyManager.SetProperty("#AlbumInfo.Title", albumInfo.Title);
      GUIPropertyManager.SetProperty("#AlbumInfo.Artist", albumInfo.Artist);
      GUIPropertyManager.SetProperty("#AlbumInfo.Year", albumInfo.DateOfRelease);
      GUIPropertyManager.SetProperty("#AlbumInfo.Rating", rating);
      GUIPropertyManager.SetProperty("#AlbumInfo.Genre", albumInfo.Genre);
      GUIPropertyManager.SetProperty("#AlbumInfo.Tones", albumInfo.Tones.Trim());
      GUIPropertyManager.SetProperty("#AlbumInfo.Styles", albumInfo.Styles.Trim());
      GUIPropertyManager.SetProperty("#AlbumInfo.Review", albumInfo.Review);
      GUIPropertyManager.SetProperty("#AlbumInfo.Tracks", albumInfo.Tracks);
    }

    public bool NeedsRefresh
    {
      get { return needsRefresh; }
    }

    #region bw methods

    private void bw_DoWork(object sender, DoWorkEventArgs e)
    {
      string thumbNailFileName = Util.Utils.GetAlbumThumbName(albumInfo.Artist, albumInfo.Title);
      Log.Debug("downloading album image for: {0} - {1}", albumInfo.Artist, albumInfo.Title);
      Util.Utils.DownLoadImage(albumInfo.ImageURL, thumbNailFileName);
    }

    private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      if (e.Error != null) return;
      string thumbNailFileName = Util.Utils.GetAlbumThumbName(albumInfo.Artist, albumInfo.Title);
      if (Util.Utils.FileExistsInCache(thumbNailFileName))
      {
        GUIPropertyManager.SetProperty("#AlbumInfo.Thumb", thumbNailFileName);
      }
    }

    #endregion

  }
}
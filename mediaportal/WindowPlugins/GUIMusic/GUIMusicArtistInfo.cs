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
using System.Collections;
using System.IO;
using System.Text;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Music.Database;
using Microsoft.DirectX.Direct3D;
using Action = MediaPortal.GUI.Library.Action;

namespace MediaPortal.GUI.Music
{
  /// <summary>
  /// 
  /// </summary>
  public class GUIMusicArtistInfo : GUIDialogWindow
  {
    [SkinControl(20)] protected GUILabelControl lblArtist = null;
    [SkinControl(21)] protected GUILabelControl lblArtistName = null;
    [SkinControl(22)] protected GUILabelControl lblBorn = null;
    [SkinControl(23)] protected GUILabelControl lblYearsActive = null;
    [SkinControl(24)] protected GUILabelControl lblGenre = null;
    [SkinControl(25)] protected GUIFadeLabel lblTones = null;
    [SkinControl(26)] protected GUIFadeLabel lblStyles = null;
    [SkinControl(27)] protected GUILabelControl lblInstruments = null;
    [SkinControl(3)] protected GUIImage imgCoverArt = null;
    [SkinControl(4)] protected GUITextControl tbReview = null;
    [SkinControl(5)] protected GUIButtonControl btnBio = null;
    [SkinControl(6)] protected GUIButtonControl btnRefresh = null;

    private bool m_bRefresh = false;
    private Texture coverArtTexture = null;
    private bool viewBio = false;
    private MusicArtistInfo artistInfo = null;
    private int coverArtTextureWidth = 0;
    private int coverArtTextureHeight = 0;

    public GUIMusicArtistInfo()
    {
      GetID = (int)Window.WINDOW_ARTIST_INFO;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\DialogArtistInfo.xml"));
    }

    public override void DoModal(int ParentID)
    {
        m_bRefresh = false;
        AllocResources();
        InitControls();
        
        base.DoModal(ParentID);
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      artistInfo = null;
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
      viewBio = true;
      Refresh();
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      base.OnClicked(controlId, control, actionType);
      if (control == btnRefresh)
      {
        string coverArtUrl = artistInfo.ImageURL;
        string coverArtFileName = GUIMusicFiles.GetArtistCoverArtName(artistInfo.Artist);
        if (coverArtFileName != string.Empty)
        {
          Util.Utils.FileDelete(coverArtFileName);
        }
        m_bRefresh = true;
        PageDestroy();
        return;
      }

      if (control == btnBio)
      {
        viewBio = !viewBio;
        Update();
      }
    }

    public MusicArtistInfo Artist
    {
      set { artistInfo = value; }
    }

    private void Update()
    {
      if (null == artistInfo)
      {
        return;
      }
      string tmpLine;
      string nameAKA = artistInfo.Artist;
      if (artistInfo.Aka != null && artistInfo.Aka.Length > 0)
      {
        nameAKA += "(" + artistInfo.Aka + ")";
      }
      lblArtist.Label = artistInfo.Artist;
      lblArtistName.Label = nameAKA;
      lblBorn.Label = artistInfo.Born;
      lblYearsActive.Label = artistInfo.YearsActive;
      lblGenre.Label = artistInfo.Genres;
      lblInstruments.Label = artistInfo.Instruments;
      lblTones.Label = artistInfo.Tones;
      lblStyles.Label = artistInfo.Styles;

      if (viewBio)
      {
        tbReview.Label = artistInfo.AMGBiography;
        btnBio.Label = GUILocalizeStrings.Get(132);
      }
      else
      {
        // translate the diff. discographys
        string textAlbums = GUILocalizeStrings.Get(690);
        string textCompilations = GUILocalizeStrings.Get(691);
        string textSingles = GUILocalizeStrings.Get(700);
        string textMisc = GUILocalizeStrings.Get(701);


        StringBuilder strLine = new StringBuilder(2048);
        ArrayList list = null;
        string discography = null;

        // get the Discography Album
        list = artistInfo.DiscographyAlbums;
        strLine.Append('\t');
        strLine.Append(textAlbums);
        strLine.Append('\n');

        discography = artistInfo.Albums;
        if (discography != null && discography.Length > 0)
        {
          strLine.Append(discography);
          strLine.Append('\n');
        }
        else
        {
          StringBuilder strLine2 = new StringBuilder(512);
          for (int i = 0; i < list.Count; ++i)
          {
            string[] listInfo = (string[])list[i];
            tmpLine = String.Format("{0} - {1} ({2})\n",
                                    listInfo[0], // year 
                                    listInfo[1], // title
                                    listInfo[2]); // label
            strLine.Append(tmpLine);
            strLine2.Append(tmpLine);
          }
          ;
          strLine.Append('\n');
          artistInfo.Albums = strLine2.ToString();
        }

        // get the Discography Compilations
        list = artistInfo.DiscographyCompilations;
        strLine.Append('\t');
        strLine.Append(textCompilations);
        strLine.Append('\n');
        discography = artistInfo.Compilations;
        if (discography != null && discography.Length > 0)
        {
          strLine.Append(discography);
          strLine.Append('\n');
        }
        else
        {
          StringBuilder strLine2 = new StringBuilder(512);
          for (int i = 0; i < list.Count; ++i)
          {
            string[] listInfo = (string[])list[i];
            tmpLine = String.Format("{0} - {1} ({2})\n",
                                    listInfo[0], // year 
                                    listInfo[1], // title
                                    listInfo[2]); // label
            strLine.Append(tmpLine);
            strLine2.Append(tmpLine);
          }
          ;
          strLine.Append('\n');
          artistInfo.Compilations = strLine2.ToString();
        }

        // get the Discography Singles
        list = artistInfo.DiscographySingles;
        strLine.Append('\t');
        strLine.Append(textSingles);
        strLine.Append('\n');
        discography = artistInfo.Singles;
        if (discography != null && discography.Length > 0)
        {
          strLine.Append(discography);
          strLine.Append('\n');
        }
        else
        {
          StringBuilder strLine2 = new StringBuilder(512);
          for (int i = 0; i < list.Count; ++i)
          {
            string[] listInfo = (string[])list[i];
            tmpLine = String.Format("{0} - {1} ({2})\n",
                                    listInfo[0], // year 
                                    listInfo[1], // title
                                    listInfo[2]); // label
            strLine.Append(tmpLine);
            strLine2.Append(tmpLine);
          }
          ;
          strLine.Append('\n');
          artistInfo.Singles = strLine2.ToString();
        }

        // get the Discography Misc
        list = artistInfo.DiscographyMisc;
        strLine.Append('\t');
        strLine.Append(textMisc);
        strLine.Append('\n');
        discography = artistInfo.Misc;
        if (discography != null && discography.Length > 0)
        {
          strLine.Append(discography);
          strLine.Append('\n');
        }
        else
        {
          StringBuilder strLine2 = new StringBuilder(512);
          for (int i = 0; i < list.Count; ++i)
          {
            string[] listInfo = (string[])list[i];
            tmpLine = String.Format("{0} - {1} ({2})\n",
                                    listInfo[0], // year 
                                    listInfo[1], // title
                                    listInfo[2]); // label
            strLine.Append(tmpLine);
            strLine2.Append(tmpLine);
          }
          ;
          strLine.Append('\n');
          artistInfo.Misc = strLine2.ToString();
        }

        tbReview.Label = strLine.ToString();
        btnBio.Label = GUILocalizeStrings.Get(689);
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

      string coverArtFileName;
      string coverArtUrl = artistInfo.ImageURL;
      coverArtFileName = GUIMusicFiles.GetArtistCoverArtName(artistInfo.Artist);
      // do not overwrite existing artist thumb
      if (coverArtFileName != string.Empty && !Util.Utils.FileExistsInCache(coverArtFileName))
      {
        //	Download image and save as 
        //	permanent thumb
        Util.Utils.DownLoadImage(coverArtUrl, coverArtFileName);
      }

      try
      {
        coverArtTexture = Util.Picture.Load(coverArtFileName, 0, 128, 128, true, false, out coverArtTextureWidth,
                                            out coverArtTextureHeight);
      }
      catch (FileNotFoundException)
      {
        //ignore          
      }

      Update();
    }

    public bool NeedsRefresh
    {
      get { return m_bRefresh; }
    }

  }
}
#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using MediaPortal.Database;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Music.Database;
using SQLite.NET;

namespace MediaPortal.GUI.Settings
{
  /// <summary>
  /// 
  /// </summary>
  /// 
  public class MusicDatabaseReorg
  {
    #region Enums

    //	return codes of ReorgDatabase
    //	numbers are strings from strings.xml
    private enum Errors
    {
      ERROR_OK = 317,
      ERROR_CANCEL = 0,
      ERROR_DATABASE = 315,
      ERROR_REORG_SONGS = 319,
      ERROR_REORG_ARTIST = 321,
      ERROR_REORG_ALBUMARTIST = 322,
      ERROR_REORG_GENRE = 323,
      ERROR_REORG_PATH = 325,
      ERROR_REORG_ALBUM = 327,
      ERROR_WRITING_CHANGES = 329,
      ERROR_COMPRESSING = 332
    }

    #endregion

    #region Variables

    private MusicDatabase m_dbs = MusicDatabase.Instance;
    private ArrayList m_pathids = new ArrayList();
    private ArrayList m_shares = new ArrayList();

    private int _parentWindowID = 0;

    #endregion

    #region CTOR

    public MusicDatabaseReorg() {}

    public MusicDatabaseReorg(int Id)
    {
      _parentWindowID = Id;
    }

    #endregion

    public class AlbumInfoCache
    {
      public int idAlbumInfo = 0;
      public string Artist;
      public string Album;
    } ;

    #region Implementation

    private void SetPercentDonebyEvent(object sender, DatabaseReorgEventArgs e)
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      if (null == pDlgProgress)
      {
        return;
      }
      pDlgProgress.SetPercentage(e.progress);
      pDlgProgress.SetLine(1, e.phase);
      pDlgProgress.Progress();
    }

    private bool IsCanceled()
    {
      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      if (null == pDlgProgress)
      {
        return false;
      }

      pDlgProgress.ProgressKeys();
      if (pDlgProgress.IsCanceled)
      {
        try
        {
          MusicDatabase.DirectExecute("rollback");
        }
        catch (Exception) {}
        return true;
      }
      return false;
    }

    public int DoReorg()
    {
      /// Todo: move this statement to the GUI.
      /// Database Reorg now fully in music.database
      /// 

      GUIDialogProgress pDlgProgress =
        (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
      if (null == pDlgProgress)
      {
        return (int)Errors.ERROR_REORG_SONGS;
      }

      pDlgProgress.SetHeading(313);
      pDlgProgress.SetLine(2, "");
      pDlgProgress.SetLine(3, "");
      pDlgProgress.SetPercentage(0);
      pDlgProgress.Progress();
      pDlgProgress.SetLine(1, 316);
      pDlgProgress.ShowProgressBar(true);

      ///TFRO71 4 june 2005
      ///Connect the event to a method that knows what to do with the event.
      MusicDatabase.DatabaseReorgChanged += new MusicDBReorgEventHandler(SetPercentDonebyEvent);
      ///Execute the reorganisation
      int appel = m_dbs.MusicDatabaseReorg(null);
      ///Tfro Disconnect the event from the method.
      MusicDatabase.DatabaseReorgChanged -= new MusicDBReorgEventHandler(SetPercentDonebyEvent);

      pDlgProgress.SetLine(2, "Klaar");

      return (int)Errors.ERROR_OK;
    }

    /// <summary>
    /// Thread, which runs the reorg in background
    /// </summary>
    public void ReorgAsync()
    {
      m_dbs.MusicDatabaseReorg(null);
      GUIDialogNotify dlgNotify =
        (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
      if (null != dlgNotify)
      {
        dlgNotify.SetHeading(GUILocalizeStrings.Get(313));
        dlgNotify.SetText(GUILocalizeStrings.Get(317));
        dlgNotify.DoModal(_parentWindowID);
      }
    }

    public void DeleteAlbumInfo()
    {
      // CMusicDatabaseReorg is friend of CMusicDatabase

      // use the same databaseobject as CMusicDatabase
      // to rollback transactions even if CMusicDatabase
      // memberfunctions are called; create our working dataset

      SQLiteResultSet results;
      string strSQL;
      strSQL = String.Format("select * from albuminfo order by strAlbum");
      results = MusicDatabase.DirectExecute(strSQL);
      int iRowsFound = results.Rows.Count;
      if (iRowsFound == 0)
      {
        GUIDialogOK pDlg = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
        if (pDlg != null)
        {
          pDlg.SetHeading(313);
          pDlg.SetLine(1, 425);
          pDlg.SetLine(2, "");
          pDlg.SetLine(3, "");
          pDlg.DoModal(GUIWindowManager.ActiveWindow);
        }
        return;
      }
      ArrayList vecAlbums = new ArrayList();
      for (int i = 0; i < results.Rows.Count; ++i)
      {
        AlbumInfoCache album = new AlbumInfoCache();
        album.idAlbumInfo = DatabaseUtility.GetAsInt(results, i, "idAlbumInfo");
        album.Album = DatabaseUtility.Get(results, i, "strAlbum");
        album.Artist = DatabaseUtility.Get(results, i, "strArtist");
        vecAlbums.Add(album);
      }

      //	Show a selectdialog that the user can select the albuminfo to delete 
      string szText = GUILocalizeStrings.Get(181);
      GUIDialogSelect pDlgSelect =
        (GUIDialogSelect)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT);
      if (pDlgSelect != null)
      {
        pDlgSelect.SetHeading(szText);
        pDlgSelect.Reset();
        foreach (AlbumInfoCache album in vecAlbums)
        {
          pDlgSelect.Add(album.Album + " - " + album.Artist);
        }
        pDlgSelect.DoModal(GUIWindowManager.ActiveWindow);

        // and wait till user selects one
        int iSelectedAlbum = pDlgSelect.SelectedLabel;
        if (iSelectedAlbum < 0)
        {
          vecAlbums.Clear();
          return;
        }

        AlbumInfoCache albumDel = (AlbumInfoCache)vecAlbums[iSelectedAlbum];
        strSQL = String.Format("delete from albuminfo where idAlbumInfo={0}", albumDel.idAlbumInfo);
        MusicDatabase.DirectExecute(strSQL);


        vecAlbums.Clear();
      }
    }


    public void DeleteSingleAlbum()
    {
      // CMusicDatabaseReorg is friend of CMusicDatabase

      // use the same databaseobject as CMusicDatabase
      // to rollback transactions even if CMusicDatabase
      // memberfunctions are called; create our working dataset

      string strSQL;
      SQLiteResultSet results;
      strSQL = String.Format("select distinct strAlbum, strAlbumArtist from tracks order by strAlbum");
      results = MusicDatabase.DirectExecute(strSQL);
      int iRowsFound = results.Rows.Count;
      if (iRowsFound == 0)
      {
        GUIDialogOK pDlg = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
        if (null != pDlg)
        {
          pDlg.SetHeading(313);
          pDlg.SetLine(1, 425);
          pDlg.SetLine(2, "");
          pDlg.SetLine(3, "");
          pDlg.DoModal(GUIWindowManager.ActiveWindow);
        }
        return;
      }
      ArrayList vecAlbums = new ArrayList();
      for (int i = 0; i < results.Rows.Count; ++i)
      {
        AlbumInfoCache album = new AlbumInfoCache();
        album.Album = DatabaseUtility.Get(results, i, "strAlbum");
        album.Artist = DatabaseUtility.Get(results, i, "strAlbumArtist");
        vecAlbums.Add(album);
      }

      //	Show a selectdialog that the user can select the album to delete 
      string szText = GUILocalizeStrings.Get(181);
      GUIDialogSelect pDlgSelect =
        (GUIDialogSelect)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT);
      if (null != pDlgSelect)
      {
        pDlgSelect.SetHeading(szText);
        pDlgSelect.Reset();
        foreach (AlbumInfoCache album in vecAlbums)
        {
          pDlgSelect.Add(album.Album + " - " + album.Artist);
        }
        pDlgSelect.DoModal(GUIWindowManager.ActiveWindow);

        // and wait till user selects one
        int iSelectedAlbum = pDlgSelect.SelectedLabel;
        if (iSelectedAlbum < 0)
        {
          vecAlbums.Clear();
          return;
        }

        AlbumInfoCache albumDel = (AlbumInfoCache)vecAlbums[iSelectedAlbum];
        //	Delete album
        strSQL = String.Format("delete from tracks where strAlbum='{0}' and strAlbumArtist like '%{1}'", albumDel.Album,
                               albumDel.Artist);
        MusicDatabase.DirectExecute(strSQL);

        //	Delete album info
        strSQL = String.Format("delete from albuminfo where strAlbum='{0}' and strAlbumArtist like '%{1}'",
                               albumDel.Album, albumDel.Artist);
        MusicDatabase.DirectExecute(strSQL);
      }
    }

    #endregion
  }
}
using System;
using System.Collections;
using System.Threading;
using SQLite.NET;

using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Database;
using MediaPortal.Music.Database;


///TODO list for the reorganisation
///24-5-2005	New files in the shares must be found too (now only through configuration
///24-5-2005	test how fast this works with 2000 songs, then we can fire this one always when starting up. Maybe as background process
///24-5-2005	Move reorg to music/database.cs. Then we can also use it in the configuration.exe
///24-5-2005	Delete all unnecessary coding for remembering relationships, this is now automatic
///24-5-2005	Solve the "first time reorg" bug
///24-5-2005	Adding extra indexes on the musictables for performance ?
///24-5-2005	recode the delete song thing for songs that don't have corresponding files
///24-5-2005	put code to delete files in non-existing MusicFolders in seperate procedure
///24-5-2005	make the body of the mainprocedure easier and put more in seperate procedures
///24-5-2005	Delete any thumbs going with a deleted song or album

namespace MediaPortal.GUI.Settings
{
	/// <summary>
	/// 
	/// </summary>
	/// 


	public class MusicDatabaseReorg
	{
		//	return codes of ReorgDatabase
		//	numbers are strings from strings.xml
		enum Errors
		{

			  ERROR_OK					=	317
			, ERROR_CANCEL				=	0
			, ERROR_DATABASE			=	315
			, ERROR_REORG_SONGS			=	319			
			, ERROR_REORG_ARTIST		=	321
			, ERROR_REORG_GENRE			=	323
			, ERROR_REORG_PATH			=	325
			, ERROR_REORG_ALBUM			=	327
			, ERROR_WRITING_CHANGES		=	329	
			, ERROR_COMPRESSING			=	332
		}

		MusicDatabase m_dbs=new MusicDatabase();
		ArrayList m_pathids = new ArrayList();
		ArrayList m_shares = new ArrayList ();

		public MusicDatabaseReorg()
		{
		 
		}


		/// This code seems obsolete. Comment it out and we'll see later
		/// tfro71 4 june 2005
		//void SetPercentDone(int nPercent)
		//{
		//	GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
		//	if (null==pDlgProgress) return;
		//	pDlgProgress.SetPercentage(nPercent);
		//	pDlgProgress.Progress();
		//}

		void SetPercentDonebyEvent(object sender, DatabaseReorgEventArgs e)
		{
			GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
			if (null==pDlgProgress) return;
			pDlgProgress.SetPercentage(e.progress);
			pDlgProgress.SetLine(1, e.phase );
			pDlgProgress.Progress();
		}

		bool IsCanceled()
		{
			GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
			if (null==pDlgProgress) return false;

			pDlgProgress.ProgressKeys();
			if (pDlgProgress.IsCanceled)
			{
				try
				{
					MusicDatabase.DBHandle.Execute("rollback"); 
				}
				catch (Exception)
				{
				}
				return true;
			}
			return false;
		}

    	public int DoReorg()
		{
			/// Todo: move this statement to the GUI.
			/// Database Reorg now fully in music.database
			/// 

			GUIDialogProgress 	pDlgProgress= (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
			if (null==pDlgProgress) return (int)Errors.ERROR_REORG_SONGS;

			pDlgProgress.SetHeading(313);
			pDlgProgress.SetLine(2, "");
			pDlgProgress.SetLine(3, "");
			pDlgProgress.SetPercentage(0);
			pDlgProgress.Progress();
			pDlgProgress.SetLine(1, 316);
			pDlgProgress.ShowProgressBar(true);

			///TFRO71 4 june 2005
			///Connect the event to a method that knows what to do with the event.
			m_dbs.DatabaseReorgChanged += new MusicDBReorgEventHandler(SetPercentDonebyEvent); 
			///Execute the reorganisation
			int appel = m_dbs.MusicDatabaseReorg(null);
			///Tfro Disconnect the event from the method.
			m_dbs.DatabaseReorgChanged -= new MusicDBReorgEventHandler(SetPercentDonebyEvent); 

			pDlgProgress.SetLine(2, "Klaar" );

			return (int)Errors.ERROR_OK;
		}

		public void DeleteAlbumInfo()
		{
			// CMusicDatabaseReorg is friend of CMusicDatabase
		
			// use the same databaseobject as CMusicDatabase
			// to rollback transactions even if CMusicDatabase
			// memberfunctions are called; create our working dataset

			SQLiteResultSet results;
			string strSQL;
			strSQL=String.Format("select * from albuminfo,album,genre,artist where albuminfo.idAlbum=album.idAlbum and albuminfo.idGenre=genre.idGenre and albuminfo.idArtist=artist.idArtist order by album.strAlbum");
			results = MusicDatabase.DBHandle.Execute(strSQL);
			int iRowsFound = results.Rows.Count;
			if (iRowsFound== 0) 
			{
				GUIDialogOK pDlg= (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
				if (pDlg!=null)
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
			for (int i=0; i < results.Rows.Count;++i)
			{
				MusicDatabase.AlbumInfoCache album = new MusicDatabase.AlbumInfoCache();
				album.idAlbum   = Int32.Parse(DatabaseUtility.Get(results,i,"album.idAlbum") ) ;
				album.Album	= DatabaseUtility.Get(results,i,"album.strAlbum") ;
				album.Artist	= DatabaseUtility.Get(results,i,"artist.strArtist") ;
				vecAlbums.Add(album);
			}

			//	Show a selectdialog that the user can select the albuminfo to delete 
			string  szText=GUILocalizeStrings.Get(181);
			GUIDialogSelect pDlgSelect= (GUIDialogSelect)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT);
			if (pDlgSelect!=null)
			{
				pDlgSelect.SetHeading(szText);
				pDlgSelect.Reset();
				foreach (MusicDatabase.AlbumInfoCache album in vecAlbums)
				{
					pDlgSelect.Add(album.Album + " - " + album.Artist);
				}
				pDlgSelect.DoModal(GUIWindowManager.ActiveWindow);

				// and wait till user selects one
				int iSelectedAlbum= pDlgSelect.SelectedLabel;
				if (iSelectedAlbum<0)
				{
					vecAlbums.Clear();
					return;	
				}

				MusicDatabase.AlbumInfoCache albumDel = (MusicDatabase.AlbumInfoCache)vecAlbums[iSelectedAlbum];
				strSQL=String.Format("delete from albuminfo where albuminfo.idAlbum={0}", albumDel.idAlbum);
				MusicDatabase.DBHandle.Execute(strSQL);

        
				vecAlbums.Clear();
			}
		}


		
		public void DeleteSingleAlbum()
		{
			// CMusicDatabaseReorg is friend of CMusicDatabase
		
			// use the same databaseobject as CMusicDatabase
			// to rollback transactions even if CMusicDatabase
			// memberfunctions are called; create our working dataset

			ArrayList m_songids = new ArrayList();
			ArrayList m_albumids = new ArrayList();
			ArrayList m_artistids = new ArrayList();
			ArrayList m_genreids = new ArrayList();
			ArrayList m_albumnames = new ArrayList();

			string strSQL;
			SQLiteResultSet results;
			strSQL=String.Format("select * from album,artist where album.idArtist=artist.idArtist order by album.strAlbum");
			results = MusicDatabase.DBHandle.Execute(strSQL);
			int iRowsFound = results.Rows.Count;
			if (iRowsFound== 0) 
			{
				GUIDialogOK pDlg= (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
				if (null!=pDlg)
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
			for (int i=0; i < results.Rows.Count;++i)
			{
				MusicDatabase.AlbumInfoCache album = new MusicDatabase.AlbumInfoCache();
				album.idAlbum   = Int32.Parse(DatabaseUtility.Get(results,i,"album.idAlbum") ) ;
				album.Album	= DatabaseUtility.Get(results,i,"album.strAlbum") ;
				album.Artist	= DatabaseUtility.Get(results,i,"artist.strArtist") ;
				vecAlbums.Add(album);
			}

			//	Show a selectdialog that the user can select the album to delete 
			string szText=GUILocalizeStrings.Get(181);
			GUIDialogSelect pDlgSelect= (GUIDialogSelect)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT);
			if (null!=pDlgSelect)
			{
				pDlgSelect.SetHeading(szText);
				pDlgSelect.Reset();
				foreach (MusicDatabase.AlbumInfoCache album in vecAlbums)
				{
					pDlgSelect.Add(album.Album + " - " + album.Artist);
				}
				pDlgSelect.DoModal(GUIWindowManager.ActiveWindow);

				// and wait till user selects one
				int iSelectedAlbum= pDlgSelect.SelectedLabel;
				if (iSelectedAlbum<0)
				{
					vecAlbums.Clear();
					return;	
				}

				MusicDatabase.AlbumInfoCache albumDel = (MusicDatabase.AlbumInfoCache)vecAlbums[iSelectedAlbum];
				//	Delete album
				strSQL=String.Format("delete from album where idAlbum={0}", albumDel.idAlbum);
				MusicDatabase.DBHandle.Execute(strSQL);

				//	Delete album info
				strSQL=String.Format("delete from albuminfo where idAlbum={0}", albumDel.idAlbum);
				MusicDatabase.DBHandle.Execute(strSQL);

				//	Get the songs of the album
				strSQL=String.Format("select * from song where idAlbum={0}", albumDel.idAlbum);
				results = MusicDatabase.DBHandle.Execute(strSQL);
				 iRowsFound = results.Rows.Count;
				if (iRowsFound!= 0) 
				{
					//	Get all artists of this album
					m_artistids.Clear();
					for (int i=0; i < results.Rows.Count;++i)
					{	
						m_artistids.Add( Int32.Parse(DatabaseUtility.Get(results,i,"idArtist") ) );
					}

					//	Do we have another song of this artist?
					foreach (int iID in m_artistids)
					{
						strSQL=String.Format("select * from song where idArtist={0} and idAlbum<>{1}", iID, albumDel.idAlbum);
						results = MusicDatabase.DBHandle.Execute(strSQL);
						 iRowsFound = results.Rows.Count;
						if (iRowsFound==0) 
						{
							//	No, delete the artist
							strSQL=String.Format("delete from artist where idArtist={0}", iID);
							MusicDatabase.DBHandle.Execute(strSQL);
						}
					}
					m_artistids.Clear();
				}

				//	Delete the albums songs
				strSQL=String.Format("delete from song where idAlbum={0}", albumDel.idAlbum);
				MusicDatabase.DBHandle.Execute(strSQL);

        // Delete album thumb
        
			}

		}
	
	}
}

using System;
using System.Collections;
using SQLite.NET;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.TagReader;
using MediaPortal.Music.Database;

namespace MediaPortal.GUI.Settings
{
	/// <summary>
	/// 
	/// </summary>
	public class MusicDatabaseReorg
	{
		//	return codes of ReorgDatabase
		//	numbers are strings from strings.xml
		enum Errors
		{

			ERROR_OK							=317
			, ERROR_CANCEL				=		0
			, ERROR_DATABASE			=	315
			, ERROR_REORG_SONGS		=	319			
			, ERROR_REORG_ARTIST	=	321
			, ERROR_REORG_GENRE		=	323
			, ERROR_REORG_PATH		=	325
			, ERROR_REORG_ALBUM		=	327
			, ERROR_WRITING_CHANGES=	329	
			, ERROR_COMPRESSING		=	332
		}

		Database m_dbs=new Database();
		ArrayList m_songids = new ArrayList();
		ArrayList m_albumids = new ArrayList();
		ArrayList m_artistids = new ArrayList();
		ArrayList m_pathids = new ArrayList();
		ArrayList m_genreids = new ArrayList();
    ArrayList m_albumnames = new ArrayList();

		public MusicDatabaseReorg()
		{
			m_dbs.Open();
		}

		void SetPercentDone(int nPercent)
		{
			GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
			if (null==pDlgProgress) return;
			pDlgProgress.SetPercentage(nPercent);
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
					Database.DBHandle.Execute("rollback"); 
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
			string strSQL;

			try
			{
				Database.DBHandle.Execute("begin"); 
			}
			catch (Exception )
			{
				return (int)Errors.ERROR_DATABASE;
			}

			SQLiteResultSet results;
			strSQL=String.Format("select * from song, path where song.idPath=path.idPath");
			try
			{
				results = Database.DBHandle.Execute(strSQL);
				if (results==null) return (int)Errors.ERROR_REORG_SONGS;
			}
			catch (Exception)
			{
				Database.DBHandle.Execute("rollback");
				return (int)Errors.ERROR_REORG_SONGS;
			}

			//	songs cleanup
			GUIDialogProgress 	pDlgProgress= (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
			if (null==pDlgProgress) return (int)Errors.ERROR_REORG_SONGS;

			pDlgProgress.SetHeading(313);
			pDlgProgress.SetLine(0, 314);
			pDlgProgress.SetLine(1, "");
			pDlgProgress.SetLine(2, "");
			pDlgProgress.SetPercentage(0);
			pDlgProgress.Progress();


			if (results.Rows.Count==0)
			{
				Database.DBHandle.Execute("rollback");
				return (int)Errors.ERROR_OK;
			}
			pDlgProgress.SetLine(0, 316);
			pDlgProgress.ShowProgressBar(true);
			if (IsCanceled())
				return (int)Errors.ERROR_CANCEL;
			//	test every song in database, if its file still exists
			for (int i=0; i < results.Rows.Count;++i)
			{
				string strFileName = m_dbs.Get(results,i,"path.strPath") ;
				strFileName += m_dbs.Get(results,i,"song.strFileName") ;
				pDlgProgress.SetLine(2, System.IO.Path.GetFileName(strFileName) );
	
				if (! System.IO.File.Exists(strFileName))
				{
					// song doesn't exist anymore, we have cleanup 
					// candidates, remember foreign keys to remove 
					// entries later if no relation exists
					m_songids.Add( Int32.Parse( m_dbs.Get(results,i,"song.idSong") ) );
					m_albumids.Add( Int32.Parse( m_dbs.Get(results,i,"song.idAlbum") ) );
          m_albumnames.Add(m_dbs.Get(results,i,"album.strAlbum"));
					m_artistids.Add( Int32.Parse( m_dbs.Get(results,i,"song.idArtist") ) );
					m_pathids.Add( Int32.Parse( m_dbs.Get(results,i,"song.idPath") ) );
					m_genreids.Add( Int32.Parse( m_dbs.Get(results,i,"song.idGenre") ) );
				}
				else
				{
					int idAlbumNew=0, idArtistNew=0, idPathNew=0, idGenreNew=0;
					int idSong=Int32.Parse( m_dbs.Get(results,i,"song.idSong"));
					int idAlbum=idAlbumNew=Int32.Parse( m_dbs.Get(results,i,"song.idAlbum"));
					int idArtist=idArtistNew=Int32.Parse( m_dbs.Get(results,i,"song.idArtist"));
					int idPath=idPathNew=Int32.Parse( m_dbs.Get(results,i,"song.idPath"));
					int idGenre=idGenreNew=Int32.Parse( m_dbs.Get(results,i,"song.idGenre"));

					if (!UpdateSong(strFileName, idSong, ref idAlbumNew, ref idArtistNew, ref idGenreNew, ref idPathNew))
					{
						Database.DBHandle.Execute("rollback"); 
						return (int)Errors.ERROR_REORG_SONGS;
					}

					// if we have cleanup candidates, remember foreign 
					// keys to remove entries later if no relation exists
					if (idAlbumNew!=idAlbum)
						m_albumids.Add(idAlbum);
					if (idArtistNew!=idArtist)
						m_artistids.Add(idArtist);
					if (idPathNew!=idPath)
						m_pathids.Add(idPath);
					if (idGenreNew!=idGenre)
						m_genreids.Add(idGenre);
				}
				SetPercentDone(i*100/results.Rows.Count);
				if (IsCanceled())
				{
					return (int)Errors.ERROR_CANCEL;
				}
			}//for (int i=0; i < results.Rows.Count;++i)

			// nothing to do for song cleanup?
			if (m_songids.Count>0)
			{
				pDlgProgress.SetLine(0, 318);
				pDlgProgress.SetLine(2, "");
				pDlgProgress.Progress();

				int nRet=DeleteSongs();
				if (nRet!=(int)Errors.ERROR_OK)
					return nRet;
			}

			// artist cleanup
			if (m_artistids.Count>0)
			{
				pDlgProgress.SetLine(0, 320);
				pDlgProgress.SetLine(2, "");
				pDlgProgress.SetPercentage(0);
				pDlgProgress.Progress();

				// Do the collected artistids have a
				// relation to song table?
				int nRet=ExamineAndDeleteArtistids();
				if (nRet!=(int)Errors.ERROR_OK)
					return nRet;
			}

			// genres cleanup
			if (m_genreids.Count>0)
			{
				pDlgProgress.SetLine(0, 322);
				pDlgProgress.SetLine(2, "");
				pDlgProgress.SetPercentage(0);
				pDlgProgress.Progress();

				// Do the collected genreids have a
				// relation to song table?
				int nRet=ExamineAndDeleteGenreids();
				if (nRet!=(int)Errors.ERROR_OK)
					return nRet;
			}
			// path cleanup
			if (m_pathids.Count>0)
			{
				pDlgProgress.SetLine(0, 324);
				pDlgProgress.SetLine(2, "");
				pDlgProgress.SetPercentage(0);
				pDlgProgress.Progress();
		
				// Do the collected pathids have a
				// relation to song table?
				int nRet=ExamineAndDeletePathids();
				if (nRet!=(int)Errors.ERROR_OK)
					return nRet;
			}
			// album cleanup
			if (m_albumids.Count>0)
			{
				pDlgProgress.SetLine(0, 326);
				pDlgProgress.SetLine(2, "");
				pDlgProgress.SetPercentage(0);
				pDlgProgress.Progress();

				// Do the collected albumids have a
				// relation to song table?
				int nRet=ExamineAndDeleteAlbumids();
				if (nRet!=(int)Errors.ERROR_OK)
					return nRet;
			}
      pDlgProgress.SetLine(0, 328);
			pDlgProgress.SetLine(1, "");
			pDlgProgress.SetLine(2, 330);
			pDlgProgress.ShowProgressBar(false);
			pDlgProgress.Progress();
			// commit changes of our transaction
			try
			{
				Database.DBHandle.Execute("end");
			}
			catch (Exception)
			{
				return (int)Errors.ERROR_WRITING_CHANGES;
			}


      pDlgProgress.SetLine(0, 331);
			pDlgProgress.SetLine(1, "");
			pDlgProgress.SetLine(2, 330);
			pDlgProgress.Progress();

			m_dbs.EmptyCache();
			// compress database file
			return Compress();
		}

		int DeleteSongs()
		{
			// build a propper where clause out of the
			// songids, whose files do not exist anymore 
			// and delete the songs from database
			string strWhere="song.idSong in ( ";
			foreach (int lSongId in m_songids)
			{
				string strSongId;
				strSongId=String.Format("{0}, ", lSongId);
				strWhere+=strSongId;
			}
			strWhere=strWhere.TrimEnd( new char[] {',',' '} );
			strWhere+=" )";

			// delete the songs
			string strSql;
			strSql = "delete from song where " + strWhere;
			try 
			{
				Database.DBHandle.Execute(strSql); 
			}
			catch (Exception)
			{
				Database.DBHandle.Execute("rollback"); 
				return (int)Errors.ERROR_REORG_SONGS;
			}
			return (int)Errors.ERROR_OK;
		}

		int ExamineAndDeleteArtistids()
		{
			int i=0;
			string strSql;
			SQLiteResultSet results;
			// check the collected artist keys, if they have a relation to song
			foreach (int iArtistId in m_artistids)
			{
				// do we get songs of that artistException
				strSql=String.Format("select * from song where song.idArtist={0}", iArtistId );
				try
				{
					results = Database.DBHandle.Execute(strSql);
				}
				catch (Exception)
				{
					Database.DBHandle.Execute("rollback"); 
					return (int)Errors.ERROR_REORG_ARTIST;
				}
				int iRowsFound = results.Rows.Count;
				if (iRowsFound== 0)
				{
					// Exception no, delete him
					strSql=String.Format("delete from artist where artist.idArtist={0}", iArtistId  );
					try
					{
						Database.DBHandle.Execute(strSql); 
					}
					catch (Exception)
					{
						Database.DBHandle.Execute("rollback");
						return (int)Errors.ERROR_REORG_ARTIST;
					}
				}
				SetPercentDone(i*100/m_artistids.Count);
				if (IsCanceled())
					return (int)Errors.ERROR_CANCEL;
			}

			return (int)Errors.ERROR_OK;
		}

		int ExamineAndDeleteGenreids()
		{
			int i=0;
			string strSql;
			SQLiteResultSet results;
			// check the collected genre keys, if they have a relation to song
			foreach (int iGenreId in m_genreids)
			{
				// do we get songs of that genreException
				strSql=String.Format("select * from song where song.idGenre={0}", iGenreId );
				try
				{
					results = Database.DBHandle.Execute(strSql);
				}
				catch (Exception)
				{
					Database.DBHandle.Execute("rollback"); 
					return (int)Errors.ERROR_REORG_GENRE;
				}
				int iRowsFound = results.Rows.Count;
				if (iRowsFound== 0)
				{
					// Exception no, delete him
					strSql=String.Format("delete from genre where genre.idGenre={0}", iGenreId  );
					try
					{
						Database.DBHandle.Execute(strSql); 
					}
					catch (Exception)
					{
						Database.DBHandle.Execute("rollback");
						return (int)Errors.ERROR_REORG_GENRE;
					}
				}
				SetPercentDone(i*100/m_genreids.Count);
				if (IsCanceled())
					return (int)Errors.ERROR_CANCEL;
			}

			return (int)Errors.ERROR_OK;
		}

		int ExamineAndDeletePathids()
		{
			int i=0;
			string strSql;
			SQLiteResultSet results;
			// check the collected path keys, if they have a relation to song
			foreach (int iPathId in m_pathids)
			{
				// do we get songs of that pathException
				strSql=String.Format("select * from song where song.idPath={0}", iPathId );
				try
				{
					results = Database.DBHandle.Execute(strSql);
				}
				catch (Exception)
				{
					Database.DBHandle.Execute("rollback"); 
					return (int)Errors.ERROR_REORG_PATH;
				}
				int iRowsFound = results.Rows.Count;
				if (iRowsFound== 0)
				{
					// Exception no, delete him
					strSql=String.Format("delete from path where path.idPath={0}", iPathId  );
					try
					{
						Database.DBHandle.Execute(strSql); 
					}
					catch (Exception)
					{
						Database.DBHandle.Execute("rollback");
						return (int)Errors.ERROR_REORG_PATH;
					}
				}
				SetPercentDone(i*100/m_pathids.Count);
				if (IsCanceled())
					return (int)Errors.ERROR_CANCEL;
			}

			return (int)Errors.ERROR_OK;
		}

		int ExamineAndDeleteAlbumids()
		{
			int i=0;
			string strSql;
			SQLiteResultSet results;
			// check the collected album keys, if they have a relation to song
			foreach (int iAlbumId in m_albumids)
			{
				// do we get songs of that albumException
				strSql=String.Format("select * from song where song.idAlbum={0}", iAlbumId );
				try
				{
					results = Database.DBHandle.Execute(strSql);
				}
				catch (Exception)
				{
					Database.DBHandle.Execute("rollback"); 
					return (int)Errors.ERROR_REORG_ALBUM;
				}
				int iRowsFound = results.Rows.Count;
				if (iRowsFound== 0)
				{
					// Exception no, delete him
					strSql=String.Format("delete from album where album.idAlbum={0}", iAlbumId  );
					try
					{
						Database.DBHandle.Execute(strSql); 
					}
					catch (Exception)
					{
						Database.DBHandle.Execute("rollback");
						return (int)Errors.ERROR_REORG_ALBUM;
					}
				}
				SetPercentDone(i*100/m_albumids.Count);
				if (IsCanceled())
					return (int)Errors.ERROR_CANCEL;
			}

			return (int)Errors.ERROR_OK;
		}


		int Compress()
		{
			//	compress database
			try
			{
				Database.DBHandle.Execute("vacuum");
			}
			catch(Exception)
			{
				return (int)Errors.ERROR_COMPRESSING;
			}
			return (int)Errors.ERROR_OK;
		}

		bool UpdateSong(string  strPathSong, int idSong, ref int idAlbum, ref int idArtist, ref int idGenre, ref int idPath)
		{
			MusicTag tag;
			tag=TagReader.TagReader.ReadTag(strPathSong);									
			if (tag!=null)
			{
				Song song = new Song();
				song.Title		= tag.Title;
				song.Genre		= tag.Genre;
				song.FileName= strPathSong;
				song.Artist	= tag.Artist;
				song.Album		= tag.Album;
				song.Year			=	tag.Year;
				song.Track			= tag.Track;
				song.Duration	= tag.Duration;

				string strPath, strFileName;
				m_dbs.Split(song.FileName, out strPath, out strFileName); 

				string strTmp;
				strTmp=song.Album;m_dbs.RemoveInvalidChars(ref strTmp);song.Album=strTmp;
				strTmp=song.Genre;m_dbs.RemoveInvalidChars(ref strTmp);song.Genre=strTmp;
				strTmp=song.Artist;m_dbs.RemoveInvalidChars(ref strTmp);song.Artist=strTmp;
				strTmp=song.Title;m_dbs.RemoveInvalidChars(ref strTmp);song.Title=strTmp;

				m_dbs.RemoveInvalidChars(ref strFileName);

				idGenre  = m_dbs.AddGenre(tag.Genre);
				idArtist = m_dbs.AddArtist(tag.Artist);
				idPath   = m_dbs.AddPath(strPath);
				idAlbum  = m_dbs.AddAlbum(tag.Album,idArtist);
				ulong dwCRC=0;
				CRCTool crc= new CRCTool();
				crc.Init(CRCTool.CRCCode.CRC32);
				dwCRC=crc.calc(song.FileName);
				//SQLiteResultSet results;

				string strSQL;
				strSQL=String.Format("update song set idArtist={0},idAlbum={1},idGenre={2},idPath={3},strTitle='{4}',iTrack={5},iDuration={6},iYear={7},dwFileNameCRC='{8}',strFileName='{9}' where idSong={10}",
															idArtist,idAlbum,idGenre,idPath,
															song.Title,
															song.Track,song.Duration,song.Year,
															dwCRC,
															strFileName, idSong);
				try
				{
					Database.DBHandle.Execute(strSQL);
				}
				catch(Exception)
				{
					return false;
				}
			}
			return true;
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
			results = Database.DBHandle.Execute(strSQL);
			int iRowsFound = results.Rows.Count;
			if (iRowsFound== 0) 
			{
				GUIDialogOK pDlg= (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
				if (pDlg!=null)
				{
					pDlg.SetHeading(313);
					pDlg.SetLine(0, 425);
					pDlg.SetLine(1, "");
					pDlg.SetLine(2, "");
					pDlg.DoModal(GUIWindowManager.ActiveWindow);
				}
				return;
			}
			ArrayList vecAlbums = new ArrayList();
			for (int i=0; i < results.Rows.Count;++i)
			{
				Database.AlbumInfoCache album = new Database.AlbumInfoCache();
				album.idAlbum   = Int32.Parse(m_dbs.Get(results,i,"album.idAlbum") ) ;
				album.Album	= m_dbs.Get(results,i,"album.strAlbum") ;
				album.Artist	= m_dbs.Get(results,i,"artist.strArtist") ;
				vecAlbums.Add(album);
			}

			//	Show a selectdialog that the user can select the albuminfo to delete 
			string  szText=GUILocalizeStrings.Get(181);
			GUIDialogSelect pDlgSelect= (GUIDialogSelect)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT);
			if (pDlgSelect!=null)
			{
				pDlgSelect.SetHeading(szText);
				pDlgSelect.Reset();
				foreach (Database.AlbumInfoCache album in vecAlbums)
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

				Database.AlbumInfoCache albumDel = (Database.AlbumInfoCache)vecAlbums[iSelectedAlbum];
				strSQL=String.Format("delete from albuminfo where albuminfo.idAlbum={0}", albumDel.idAlbum);
				Database.DBHandle.Execute(strSQL);

        
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
			strSQL=String.Format("select * from album,artist where album.idArtist=artist.idArtist order by album.strAlbum");
			results = Database.DBHandle.Execute(strSQL);
			int iRowsFound = results.Rows.Count;
			if (iRowsFound== 0) 
			{
				GUIDialogOK pDlg= (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
				if (null!=pDlg)
				{
					pDlg.SetHeading(313);
					pDlg.SetLine(0, 425);
					pDlg.SetLine(1, "");
					pDlg.SetLine(2, "");
					pDlg.DoModal(GUIWindowManager.ActiveWindow);
				}
				return;
			}
			ArrayList vecAlbums = new ArrayList();
			for (int i=0; i < results.Rows.Count;++i)
			{
				Database.AlbumInfoCache album = new Database.AlbumInfoCache();
				album.idAlbum   = Int32.Parse(m_dbs.Get(results,i,"album.idAlbum") ) ;
				album.Album	= m_dbs.Get(results,i,"album.strAlbum") ;
				album.Artist	= m_dbs.Get(results,i,"artist.strArtist") ;
				vecAlbums.Add(album);
			}

			//	Show a selectdialog that the user can select the album to delete 
			string szText=GUILocalizeStrings.Get(181);
			GUIDialogSelect pDlgSelect= (GUIDialogSelect)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT);
			if (null!=pDlgSelect)
			{
				pDlgSelect.SetHeading(szText);
				pDlgSelect.Reset();
				foreach (Database.AlbumInfoCache album in vecAlbums)
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

				Database.AlbumInfoCache albumDel = (Database.AlbumInfoCache)vecAlbums[iSelectedAlbum];
				//	Delete album
				strSQL=String.Format("delete from album where idAlbum={0}", albumDel.idAlbum);
				Database.DBHandle.Execute(strSQL);

				//	Delete album info
				strSQL=String.Format("delete from albuminfo where idAlbum={0}", albumDel.idAlbum);
				Database.DBHandle.Execute(strSQL);

				//	Get the songs of the album
				strSQL=String.Format("select * from song where idAlbum={0}", albumDel.idAlbum);
				results = Database.DBHandle.Execute(strSQL);
				 iRowsFound = results.Rows.Count;
				if (iRowsFound!= 0) 
				{
					//	Get all artists of this album
					m_artistids.Clear();
					for (int i=0; i < results.Rows.Count;++i)
					{	
						m_artistids.Add( Int32.Parse(m_dbs.Get(results,i,"idArtist") ) );
					}

					//	Do we have another song of this artist?
					foreach (int iID in m_artistids)
					{
						strSQL=String.Format("select * from song where idArtist={0} and idAlbum<>{1}", iID, albumDel.idAlbum);
						results = Database.DBHandle.Execute(strSQL);
						 iRowsFound = results.Rows.Count;
						if (iRowsFound==0) 
						{
							//	No, delete the artist
							strSQL=String.Format("delete from artist where idArtist={0}", iID);
							Database.DBHandle.Execute(strSQL);
						}
					}
					m_artistids.Clear();
				}

				//	Delete the albums songs
				strSQL=String.Format("delete from song where idAlbum={0}", albumDel.idAlbum);
				Database.DBHandle.Execute(strSQL);

        // Delete album thumb
        
			}

		}

	}
}

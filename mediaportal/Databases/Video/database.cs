using System;
using System.Collections;
using SQLite.NET;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
namespace MediaPortal.Video.Database
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class VideoDatabase
	{
		static SQLiteClient m_db=null;
    // singleton. Dont allow any instance of this class
    private VideoDatabase()
    {
    }
    static VideoDatabase()
    {
      
      Log.Write("opening video database");
      try 
      {
        // Open database
        System.IO.Directory.CreateDirectory("database");
        m_db = new SQLiteClient(@"database\videodatabase2.db");
        CreateTables();

      } 
      catch (Exception ex) 
      {
        Log.Write("videodatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
      }
      
      Log.Write("video database opened");
    }
		static void AddTable( string strTable, string strSQL)
		{
			if (m_db==null) return;
			SQLiteResultSet results;
			results = m_db.Execute("SELECT name FROM sqlite_master WHERE name='"+strTable+"' and type='table' UNION ALL SELECT name FROM sqlite_temp_master WHERE type='table' ORDER BY name");
			if (results!=null&& results.Rows.Count>0) 
			{
				
				if (results.Rows.Count==1) 
				{
					ArrayList arr = (ArrayList)results.Rows[0];
					if (arr.Count==1)
					{
						if ( (string)arr[0] == strTable) 
						{
							return;
						}
					}
				}
			}

			try 
			{
				m_db.Execute(strSQL);
			}
			catch (SQLiteException ex) 
			{
				Log.Write("videodatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
			return ;
		}
		static bool CreateTables()
		{
			if (m_db==null) return false;
			AddTable("bookmark","CREATE TABLE bookmark ( idBookmark integer primary key, idFile integer, fPercentage text)\n");
			AddTable("genre","CREATE TABLE genre ( idGenre integer primary key, strGenre text)\n");
			AddTable("genrelinkmovie","CREATE TABLE genrelinkmovie ( idGenre integer, idMovie integer)\n");
			AddTable("movie","CREATE TABLE movie ( idMovie integer primary key, idPath integer, hasSubtitles integer, discid text)\n");
			AddTable("movieinfo","CREATE TABLE movieinfo ( idMovie integer, idDirector integer, strPlotOutline text, strPlot text, strTagLine text, strVotes text, fRating text,strCast text,strCredits text, iYear integer, strGenre text, strPictureURL text, strTitle text, IMDBID text)\n");
			AddTable("actorlinkmovie","CREATE TABLE actorlinkmovie ( idActor integer, idMovie integer )\n");
			AddTable("actors","CREATE TABLE actors ( idActor integer primary key, strActor text )\n");
			AddTable("path","CREATE TABLE path ( idPath integer primary key, strPath text, cdlabel text)\n");
			AddTable("files","CREATE TABLE files ( idFile integer primary key, idPath integer, idMovie integer,strFilename text)\n");
      AddTable("resume","CREATE TABLE resume ( idResume integer primary key, idMovie integer, stoptime integer)\n");
      return true;
		}

		static string Get(SQLiteResultSet results,int iRecord,string strColum)
		{
			if (null==results) return "";
			if (results.Rows.Count<iRecord) return "";
			ArrayList arr=(ArrayList)results.Rows[iRecord];
			int iCol=0;
			foreach (string columnName in results.ColumnNames)
			{
				if (strColum==columnName)
				{
						return ((string)arr[iCol]).Trim();
				}
				iCol++;
			}
			return "";
		}

		static int AddFile(int lMovieId, int lPathId,  string strFileName)
		{
			if (m_db==null) return -1;
			string strSQL="";
			try
			{
				int lFileId=-1;
				SQLiteResultSet results;
        strFileName=strFileName.Trim();

				strSQL=String.Format("select * from files where idmovie={0} and idpath={1} and strFileName like '{2}'",lMovieId,lPathId,strFileName);
				results=m_db.Execute(strSQL);
				if (results!=null && results.Rows.Count>0) 
				{
					lFileId=System.Int32.Parse( Get(results,0,"idFile"));
					return lFileId;
				}
				strSQL=String.Format ("insert into files (idFile, idMovie,idPath, strFileName) values(null, {0},{1},'{2}')", lMovieId,lPathId, strFileName );
				results=m_db.Execute(strSQL);
				lFileId=m_db.LastInsertID();
				return lFileId;
			}
			catch (SQLiteException ex) 
			{
				Log.Write("videodatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
			return -1;
		}


		static int GetFile( string strFilenameAndPath,out int lPathId,out int lMovieId, bool bExact)
		{
			lPathId=-1;
			lMovieId=-1;
			try
			{
				if (null==m_db) return -1;
				string strPath, strFileName ;
        string cdlabel=GetDVDLabel(strFilenameAndPath);
        RemoveInvalidChars(ref cdlabel);
        strFilenameAndPath=strFilenameAndPath.Trim();
				Split(strFilenameAndPath, out strPath, out strFileName); 
				RemoveInvalidChars(ref strPath);
    
				lPathId = GetPath(strPath);
				if (lPathId < 0) return -1;

				string strSQL;
				SQLiteResultSet results;
				strSQL=String.Format("select * from files where idpath={0}",lPathId);
				results=m_db.Execute(strSQL);
				if (results.Rows.Count > 0) 
				{
					for (int iRow=0; iRow < results.Rows.Count;++iRow)
					{
						string strFname= Get(results,iRow,"strFilename") ;
						if (bExact)
						{
							if (strFname==strFileName)
							{
								// was just returning 'true' here, but this caused problems with
								// the bookmarks as these are stored by fileid. forza.
								int lFileId=System.Int32.Parse( Get(results,iRow,"idFile") );
								lMovieId=System.Int32.Parse( Get(results,iRow,"idMovie") ) ;
								return lFileId;
							}
						}
						else
						{
              if (Utils.ShouldStack(strFname,strFileName))
              {
                int lFileId=System.Int32.Parse( Get(results,iRow,"idFile") );
								lMovieId=System.Int32.Parse( Get(results,iRow,"idMovie") ) ;
								return lFileId;
							}
							if (strFname==strFileName)
							{
								// was just returning 'true' here, but this caused problems with
								// the bookmarks as these are stored by fileid. forza.
								int lFileId=System.Int32.Parse( Get(results,iRow,"idFile") );
								lMovieId=System.Int32.Parse( Get(results,iRow,"idMovie") ) ;
								return lFileId;
							}
						}
					}
				}
			}
			catch (SQLiteException ex) 
			{
				Log.Write("videodatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
			return -1;
		}

		static void Split( string strFileNameAndPath, out string strPath, out string strFileName)
		{
      strFileNameAndPath=strFileNameAndPath.Trim();
			strFileName="";
			strPath="";
			int i=strFileNameAndPath.Length-1;
			while (i > 0)
			{
				char ch=strFileNameAndPath[i];
				if (ch==':' || ch=='/' || ch=='\\') break;
				else i--;
      }
      strPath     = strFileNameAndPath.Substring(0,i).Trim();
			strFileName = strFileNameAndPath.Substring(i,strFileNameAndPath.Length - i).Trim();
		}

		
    static public void RemoveInvalidChars(ref string strTxt)
    {
      string strReturn="";
      for (int i=0; i < (int)strTxt.Length; ++i)
      {
        char k=strTxt[i];
        if (k=='\'') 
        {
          strReturn += "'";
        }
        strReturn += k;
      }
      if (strReturn=="") 
        strReturn="unknown";
      strTxt=strReturn.Trim();
    }


		static int AddPath( string strPath)
		{
			try
			{
				if (null==m_db) return -1;
				string strSQL;
        
        string cdlabel=GetDVDLabel(strPath);
        RemoveInvalidChars(ref cdlabel);

        strPath=strPath.Trim();
				SQLiteResultSet results;
				strSQL = String.Format("select * from path where strPath like '{0}' and cdlabel like '{1}'",strPath,cdlabel);
				results=m_db.Execute(strSQL);
				if (results.Rows.Count == 0) 
				{
					// doesnt exists, add it
					strSQL = String.Format("insert into Path (idPath, strPath, cdlabel) values( NULL, '{0}', '{1}')", strPath,cdlabel);
					m_db.Execute(strSQL);
					int lPathId=m_db.LastInsertID();
					return lPathId;
				}
				else
				{
					int lPathId=System.Int32.Parse( Get(results,0,"idPath") );
					return lPathId;
				}

			}
			catch (SQLiteException ex) 
			{
				Log.Write("videodatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
			return -1;
		}

    static string GetDVDLabel(string strFile)
    {
      string cdlabel=String.Empty;
      if (Utils.IsDVD(strFile)) 
      {
        cdlabel=Utils.GetDriveSerial(strFile);        
      }
      return cdlabel;
    }

    static public void AddMovieFile( string strFile)
    {
      bool bHassubtitles=false;
      if (strFile.ToLower().IndexOf(".ifo")>=0) bHassubtitles=true;
      if (strFile.ToLower().IndexOf(".vob")>=0) bHassubtitles=true;
      string strCDLabel = "";
      if (Utils.IsDVD(strFile)) 
      {
        Utils.GetDVDLabel(strFile, out strCDLabel);        
      }
      string[] sub_exts = {  ".utf", ".utf8", ".utf-8", ".sub", ".srt", ".smi", ".rt", ".txt", ".ssa", ".aqt", ".jss", ".ass", ".idx",".ifo" };
      // check if movie has subtitles
      for (int i = 0; i < sub_exts.Length; i++)
      {
        string strSubTitleFile = strFile;
        strSubTitleFile = System.IO.Path.ChangeExtension(strFile, sub_exts[i]);
        if (System.IO.File.Exists(strSubTitleFile))
        {
          bHassubtitles = true;
          break;
        }
      }  
      VideoDatabase.AddMovie(strFile, bHassubtitles);
    }

    static public int AddMovie( string strFilenameAndPath, bool bHassubtitles)
		{
			if (m_db==null) return -1;
			try
			{
				if (null==m_db) return -1;
				string strPath, strFileName;

				Split(strFilenameAndPath,out strPath, out strFileName); 
				RemoveInvalidChars(ref strPath);
				RemoveInvalidChars(ref strFileName);
		    
				int lMovieId = GetMovie(strFilenameAndPath, false);
				if (lMovieId < 0)
				{
					string strSQL;

					int lPathId = AddPath(strPath);
					if (lPathId < 0) return -1;
          int iHasSubs=0;
          if (bHassubtitles) iHasSubs=1;
					strSQL=String.Format("insert into movie (idMovie, idPath, hasSubtitles, discid) values( NULL, {0}, {1},'')",lPathId,iHasSubs);
					m_db.Execute(strSQL);
					lMovieId=m_db.LastInsertID();
					AddFile(lMovieId,lPathId,strFileName);
				}
				else
				{
					int lPathId = GetPath(strPath);
					if (lPathId < 0) return -1;
					AddFile(lMovieId,lPathId,strFileName);
				}
				return lMovieId;
			}
			catch (SQLiteException ex) 
			{
				Log.Write("videodatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
			return -1;
		}

		static int AddGenre( string strGenre1)
		{
			try
			{
				string strGenre=strGenre1;
				RemoveInvalidChars(ref strGenre);

				if (null==m_db) return -1;
				SQLiteResultSet results;
				string strSQL="select * from genre where strGenre like '";
				strSQL += strGenre;
				strSQL += "'";
				results=m_db.Execute(strSQL);
				if (results.Rows.Count == 0) 
				{
					// doesnt exists, add it
					strSQL = "insert into genre (idGenre, strGenre) values( NULL, '" ;
					strSQL += strGenre;
					strSQL += "')";
					m_db.Execute(strSQL);
					int lGenreId=m_db.LastInsertID();
					return lGenreId;
				}
				else
				{
					int lGenreId=System.Int32.Parse( Get(results,0,"idGenre") );
					return lGenreId;
				}
			}
			catch (SQLiteException ex) 
			{
				Log.Write("videodatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
			return -1;
		}

    static public void GetGenres(ArrayList genres)
		{
			if (m_db==null) return ;
			try
			{
				genres.Clear();
				if (null==m_db) return ;
				SQLiteResultSet results;
				results=m_db.Execute("select * from genre order by strGenre");
				if (results.Rows.Count == 0)  return;
				for (int iRow=0; iRow < results.Rows.Count;iRow++)
				{
					genres.Add(Get(results,iRow, "strGenre" ) );
				}
			}
			catch (SQLiteException ex) 
			{
				Log.Write("videodatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}

    }

    static public void GetActors(ArrayList actors)
		{
			if (m_db==null) return ;
			try
			{
				actors.Clear();
				if (null==m_db) return ;
				SQLiteResultSet results;
				results=m_db.Execute("select * from actors");
				if (results.Rows.Count == 0)  return;
				for (int iRow=0; iRow < results.Rows.Count;iRow++)
				{
					actors.Add(Get(results,iRow, "strActor" ) );
				}
			}
			catch (SQLiteException ex) 
			{
				Log.Write("videodatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
		}

    static public void GetYears(ArrayList years)
		{
			if (m_db==null) return;
			try
			{
				years.Clear();
				if (null==m_db) return ;
				SQLiteResultSet results;
				results=m_db.Execute("select * from movieinfo");
				if (results.Rows.Count == 0)  return;
				for (int iRow=0; iRow < results.Rows.Count;iRow++)
				{
					string strYear=Get(results,iRow,"iYear");
					bool bAdd=true;
					for (int i=0; i < years.Count;++i)
					{
						if (strYear == (string)years[i]) bAdd=false;
					}
					if (bAdd) years.Add( strYear);
				}
			}
			catch (SQLiteException ex) 
			{
				Log.Write("videodatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
		}

		static int GetPath( string strPath)
		{
			try
			{
				if (null==m_db) return -1;
				string strSQL;
        string cdlabel=GetDVDLabel(strPath);
        RemoveInvalidChars(ref cdlabel);

        strPath=strPath.Trim();
				strSQL =String.Format("select * from path where strPath like '{0}' and cdlabel like '{1}'",strPath,cdlabel);
				SQLiteResultSet results;
				results=m_db.Execute(strSQL);
				if (results.Rows.Count > 0)
				{
					int lPathId = System.Int32.Parse ( Get(results,0,"idPath") );
					return lPathId;
				}
			}
			catch (SQLiteException ex) 
			{
				Log.Write("videodatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
			return -1;
		}

		static int AddActor( string strActor1)
		{
			try
			{
				string strActor=strActor1;
				RemoveInvalidChars(ref strActor);

				if (null==m_db) return -1;
				string strSQL="select * from Actors where strActor like '";
				strSQL += strActor;
				strSQL += "'";
				SQLiteResultSet results;
				results=m_db.Execute(strSQL);
				if (results.Rows.Count == 0) 
				{
					// doesnt exists, add it
					strSQL = "insert into Actors (idActor, strActor) values( NULL, '" ;
					strSQL += strActor;
					strSQL += "')";
					m_db.Execute(strSQL);
					int lActorId=m_db.LastInsertID();
					return lActorId;
				}
				else
				{
					int lActorId=System.Int32.Parse( Get(results,0,"idActor") );
					return lActorId;
				}

			}
			catch (SQLiteException ex) 
			{
				Log.Write("videodatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
			return -1;
		}

		static int GetMovie( string strFilenameAndPath, bool bExact)
		{
			int lPathId;
			int lMovieId;
			if (GetFile(strFilenameAndPath, out lPathId,out  lMovieId,bExact) < 0)
			{
				return -1;
			}
			return lMovieId;
		}

    static public void DeleteMovieInfo( string strFileNameAndPath)
    {
		    
      int lMovieId=GetMovie(strFileNameAndPath,false);
      if ( lMovieId < 0) return ;
      DeleteMovieInfoById(lMovieId);
    }
    static public void DeleteMovieInfoById( long lMovieId)
    {
      try
      {
        if (null==m_db) return ;
		    
				string strSQL;
				strSQL = String.Format("delete from genrelinkmovie where idmovie={0}", lMovieId);
				m_db.Execute(strSQL);

				strSQL = String.Format("delete from actorlinkmovie where idmovie={0}", lMovieId);
				m_db.Execute(strSQL);

				strSQL = String.Format("delete from movieinfo where idmovie={0}", lMovieId);
				m_db.Execute(strSQL);  
			}
			catch (SQLiteException ex) 
			{
				Log.Write("videodatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
		}

    static public bool HasMovieInfo( string strFilenameAndPath)
    {
      if (strFilenameAndPath==null || strFilenameAndPath.Length==0 )return false;
			try
			{
				if (null==m_db) return false;
				int lMovieId=GetMovie(strFilenameAndPath,false);
				if ( lMovieId < 0) return false;
				string strSQL;
				strSQL=String.Format("select * from movieinfo where movieinfo.idmovie={0}",lMovieId);

				SQLiteResultSet results;
				results=m_db.Execute(strSQL);
				if (results.Rows.Count == 0) 
				{
					return false;
				}
				return true;
			}
			catch (SQLiteException ex) 
			{
				Log.Write("videodatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
			return false;
    }

    static public bool HasSubtitle( string strFilenameAndPath)
    {
			try
			{
				if (null==m_db) return false;
				int lMovieId=GetMovie(strFilenameAndPath,false);
				if ( lMovieId < 0) return false;
				string strSQL;
				strSQL=String.Format("select * from movie where movie.idMovie={0}",lMovieId);

				SQLiteResultSet results;
				results=m_db.Execute(strSQL);
				if (results.Rows.Count == 0) 
				{
					return false;
				}
				int lHasSubs = System.Int32.Parse( Get(results,0,"hasSubtitles" ) );
				if (lHasSubs!=0) return true;
				return false;
			}
			catch (SQLiteException ex) 
			{
				Log.Write("videodatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
			return false;
    }

    static public void	GetFiles(int lMovieId, ref ArrayList movies)
    {
			try
			{
				movies.Clear();
				if (null==m_db) return ;
				if (lMovieId < 0) return;
		  	
				string strSQL;
				strSQL = String.Format("select * from path,files where path.idPath=files.idPath and files.idmovie={0} order by strFilename", lMovieId );

				SQLiteResultSet results;
				results=m_db.Execute(strSQL);
				if (results.Rows.Count == 0)  return;
				for (int i=0; i < results.Rows.Count; ++i)
				{
					string strPath,strFile;
					strFile= Get(results,i, "files.strFilename") ;
					strPath= Get(results,i, "path.strPath");
					strFile=strPath+strFile;
					movies.Add(strFile);
				}
			}
			catch (SQLiteException ex) 
			{
				Log.Write("videodatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
		}

		static public void DeleteMovie( string strFilenameAndPath)
		{
			try
			{
				int lPathId;
				int lMovieId;
				if (null==m_db) return ;
				if (GetFile(strFilenameAndPath, out lPathId, out lMovieId,false) < 0)
				{
					return ;
				}

				ClearBookMarksOfMovie(strFilenameAndPath);

				string strSQL;
				strSQL=String.Format("delete from files where idmovie={0}", lMovieId);
				m_db.Execute(strSQL);

				strSQL=String.Format("delete from genrelinkmovie where idmovie={0}", lMovieId);
				m_db.Execute(strSQL);

				strSQL=String.Format("delete from actorlinkmovie where idmovie={0}", lMovieId);
				m_db.Execute(strSQL);

				strSQL=String.Format("delete from movieinfo where idmovie={0}", lMovieId);
				m_db.Execute(strSQL);

				strSQL=String.Format("delete from movie where idmovie={0}", lMovieId);
				m_db.Execute(strSQL);
			}
			catch (SQLiteException ex) 
			{
				Log.Write("videodatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}

		}
		static void AddGenreToMovie(int lMovieId, int lGenreId)
		{
			try
			{
				if (null==m_db) return ;
				string strSQL;
				strSQL=String.Format("select * from genrelinkmovie where idGenre={0} and idMovie={1}",lGenreId,lMovieId);

				SQLiteResultSet results;
				results=m_db.Execute(strSQL);
				if (results.Rows.Count == 0) 
				{
					// doesnt exists, add it
					strSQL=String.Format ("insert into genrelinkmovie (idGenre, idMovie) values( {0},{1})",lGenreId,lMovieId);
					m_db.Execute(strSQL);
				}
			}
			catch (SQLiteException ex) 
			{
				Log.Write("videodatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
		}

		static void AddActorToMovie(int lMovieId, int lActorId)
		{
			try
			{
				if (null==m_db) return ;
				string strSQL;
				strSQL=String.Format("select * from actorlinkmovie where idActor={0} and idMovie={1}",lActorId,lMovieId);
				
				SQLiteResultSet results;
				results=m_db.Execute(strSQL);
				if (results.Rows.Count == 0) 
				{
					// doesnt exists, add it
					strSQL=String.Format ("insert into actorlinkmovie (idActor, idMovie) values( {0},{1})",lActorId,lMovieId);
					m_db.Execute(strSQL);
				}
			}
			catch (SQLiteException ex) 
			{
				Log.Write("videodatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
		}

    static public void SetThumbURL(int lMovieId, string thumbURL)
    {
      RemoveInvalidChars(ref thumbURL);
      try
      {
        if (null==m_db) return ;
        string strSQL;

        strSQL=String.Format("update movieinfo set strPictureURL='{0}' where idMovie={1}", thumbURL, lMovieId );
        m_db.Execute(strSQL);
      }
      catch (SQLiteException ex) 
      {
        Log.Write("videodatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
      }
    }

		static public void SetDVDLabel(int lMovieId, string strDVDLabel1)
		{
			string strDVDLabel=strDVDLabel1;
			RemoveInvalidChars(ref strDVDLabel);
			try
			{
				if (null==m_db) return ;
				string strSQL;

				strSQL=String.Format("update movie set discid='{0}' where idMovie={1}", strDVDLabel1, lMovieId );
				m_db.Execute(strSQL);
			}
			catch (SQLiteException ex) 
			{
				Log.Write("videodatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
		}

		static public void ClearBookMarksOfMovie( string strFilenameAndPath)
		{
			try
			{
				if (null==m_db) return ;
				int lPathId, lMovieId;
				int lFileId=GetFile(strFilenameAndPath, out lPathId, out lMovieId, true);
				if (lFileId < 0) return;
				string strSQL;
				strSQL=String.Format("delete from bookmark where idFile={0}",lFileId);
				m_db.Execute(strSQL);
			}
			catch (SQLiteException ex) 
			{
				Log.Write("videodatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
		}
		static public void AddBookMarkToMovie( string strFilenameAndPath, float fTime)
		{
			try
			{
				int lPathId, lMovieId;
				int lFileId=GetFile(strFilenameAndPath,out lPathId, out lMovieId, true);
				if (lFileId < 0) return;
				if (null==m_db) return ;
				string strSQL;
				strSQL=String.Format("select * from bookmark where idFile={0} and fPercentage='{1}'",lFileId,fTime);
				SQLiteResultSet results;
				results=m_db.Execute(strSQL);
				if (results.Rows.Count != 0)  return;

				strSQL=String.Format ("insert into bookmark (idBookmark, idFile, fPercentage) values(NULL,{0},'{1}')", lFileId,fTime);
				m_db.Execute(strSQL);
			}
			catch (SQLiteException ex) 
			{
				Log.Write("videodatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
		}

		static public void GetBookMarksForMovie( string strFilenameAndPath, ref ArrayList bookmarks)
		{
      bookmarks.Clear();
			try
			{
				int lPathId, lMovieId;
				int lFileId=GetFile(strFilenameAndPath, out lPathId, out lMovieId, true);
				if (lFileId < 0) return;
				bookmarks.Clear();
				if (null==m_db) return ;
				
				string strSQL;
				strSQL=String.Format("select * from bookmark where idFile={0} order by fPercentage", lFileId);
				SQLiteResultSet results;
				results=m_db.Execute(strSQL);
				if (results.Rows.Count == 0)  return;
				for (int iRow=0; iRow <results.Rows.Count;iRow++)
				{
					double fTime=Convert.ToDouble ( Get(results,iRow,"fPercentage") );
					bookmarks.Add(fTime);
				}
			}
			catch (SQLiteException ex) 
			{
				Log.Write("videodatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
		}


		static public void GetMovies(ref ArrayList movies)
		{
			try
			{
				movies.Clear();
				if (null==m_db) return ;
				string strSQL;
				strSQL=String.Format("select * from movie,movieinfo,actors,path where movieinfo.idmovie=movie.idmovie and movieinfo.iddirector=actors.idActor and movie.idpath=path.idpath");
	
				SQLiteResultSet results;
				results=m_db.Execute(strSQL);
				if (results.Rows.Count == 0)  return;
				for (int iRow=0; iRow <results.Rows.Count;iRow++)
				{
					IMDBMovie details=new IMDBMovie();
					details.Rating=(float)System.Double.Parse(Get(results,iRow,"movieinfo.fRating") );
					if (details.Rating>10.0f) details.Rating /=10.0f;
					details.Director=Get(results,iRow,"actors.strActor");
					details.WritingCredits=Get(results,iRow,"movieinfo.strCredits");
					details.TagLine=Get(results,iRow,"movieinfo.strTagLine");
					details.PlotOutline=Get(results,iRow,"movieinfo.strPlotOutline");
					details.Plot=Get(results,iRow,"movieinfo.strPlot");
					details.Votes=Get(results,iRow,"movieinfo.strVotes");
					details.Cast=Get(results,iRow,"movieinfo.strCast");
					details.Year=System.Int32.Parse(Get(results,iRow,"movieinfo.iYear"));
					details.Genre=Get(results,iRow,"movieinfo.strGenre");
					details.ThumbURL=Get(results,iRow,"movieinfo.strPictureURL");
					details.Title=Get(results,iRow,"movieinfo.strTitle");
					details.Path=Get(results,iRow,"path.strPath");
					details.DVDLabel=Get(results,iRow,"movie.discid") ;
					details.IMDBNumber=Get(results,iRow,"movieinfo.IMDBID") ;
					long lMovieId=System.Int32.Parse( Get(results,iRow,"movieinfo.idMovie") );
					details.SearchString= String.Format("{0}", lMovieId);
          details.CDLabel=Get(results,0,"path.cdlabel") ;
					movies.Add(details);
				}
			}
			catch (SQLiteException ex) 
			{
				Log.Write("videodatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
		}

    static public int GetMovieInfo( string strFilenameAndPath, ref IMDBMovie details)
    {
      int lMovieId=GetMovie(strFilenameAndPath,false);
      if (lMovieId<0) return -1;

      GetMovieInfoById(lMovieId, ref details);
      return lMovieId;
    }

    static public void GetMovieInfoById( int lMovieId, ref IMDBMovie details)
    {
      try
      {
        string strSQL;
				strSQL=String.Format("select * from movieinfo,actors,movie,path where path.idpath=movie.idpath and movie.idMovie=movieinfo.idMovie and movieinfo.idmovie={0} and idDirector=idActor", lMovieId);
		    
				SQLiteResultSet results;
				results=m_db.Execute(strSQL);
				if (results.Rows.Count == 0)  return;
				details.Rating=(float)System.Double.Parse(Get(results,0,"movieinfo.fRating") );
				if (details.Rating>10.0f) details.Rating /=10.0f;
				details.Director=Get(results,0,"actors.strActor");
				details.WritingCredits=Get(results,0,"movieinfo.strCredits");
				details.TagLine=Get(results,0,"movieinfo.strTagLine");
				details.PlotOutline=Get(results,0,"movieinfo.strPlotOutline");
				details.Plot=Get(results,0,"movieinfo.strPlot");
				details.Votes=Get(results,0,"movieinfo.strVotes");
				details.Cast=Get(results,0,"movieinfo.strCast");
				details.Year=System.Int32.Parse(Get(results,0,"movieinfo.iYear"));
				details.Genre=Get(results,0,"movieinfo.strGenre");
				details.ThumbURL=Get(results,0,"movieinfo.strPictureURL");
				details.Title=Get(results,0,"movieinfo.strTitle");
				details.Path=Get(results,0,"path.strPath");
				details.DVDLabel=Get(results,0,"movie.discid") ;
				details.IMDBNumber=Get(results,0,"movieinfo.IMDBID") ;
				 lMovieId=System.Int32.Parse( Get(results,0,"movieinfo.idMovie") );
				details.SearchString= String.Format("{0}", lMovieId);
        details.CDLabel=Get(results,0,"path.cdlabel") ;
			}
			catch (SQLiteException ex) 
			{
				Log.Write("videodatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
		}

    static public void GetMoviesByGenre(string strGenre1, ref ArrayList movies)
    {
			try
			{
				string strGenre=strGenre1;
				RemoveInvalidChars(ref strGenre);

				movies.Clear();
				if (null==m_db) return ;
				string strSQL;
				strSQL=String.Format("select * from genrelinkmovie,genre,movie,movieinfo,actors,path where path.idpath=movie.idpath and genrelinkmovie.idGenre=genre.idGenre and genrelinkmovie.idmovie=movie.idmovie and movieinfo.idmovie=movie.idmovie and genre.strGenre='{0}' and movieinfo.iddirector=actors.idActor", strGenre);

				SQLiteResultSet results;
				results=m_db.Execute(strSQL);
				if (results.Rows.Count == 0)  return;
				for (int iRow=0; iRow <results.Rows.Count;iRow++)
				{
					IMDBMovie details=new IMDBMovie();
					details.Rating=(float)System.Double.Parse(Get(results,iRow,"movieinfo.fRating") );
					if (details.Rating>10.0f) details.Rating /=10.0f;
					details.Director=Get(results,iRow,"actors.strActor");
					details.WritingCredits=Get(results,iRow,"movieinfo.strCredits");
					details.TagLine=Get(results,iRow,"movieinfo.strTagLine");
					details.PlotOutline=Get(results,iRow,"movieinfo.strPlotOutline");
					details.Plot=Get(results,iRow,"movieinfo.strPlot");
					details.Votes=Get(results,iRow,"movieinfo.strVotes");
					details.Cast=Get(results,iRow,"movieinfo.strCast");
					details.Year=System.Int32.Parse(Get(results,iRow,"movieinfo.iYear"));
					details.Genre=Get(results,iRow,"movieinfo.strGenre");
					details.ThumbURL=Get(results,iRow,"movieinfo.strPictureURL");
					details.Title=Get(results,iRow,"movieinfo.strTitle");
					details.Path=Get(results,iRow,"path.strPath");
					details.DVDLabel=Get(results,iRow,"movie.discid") ;
					details.IMDBNumber=Get(results,iRow,"movieinfo.IMDBID") ;
					long lMovieId=System.Int32.Parse( Get(results,iRow,"movieinfo.idMovie") );
					details.SearchString= String.Format("{0}", lMovieId);
          details.CDLabel=Get(results,0,"path.cdlabel") ;
					movies.Add(details);
				}
			}
			catch (SQLiteException ex) 
			{
				Log.Write("videodatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
		}

    static public void GetMoviesByActor(string strActor1, ref ArrayList movies)
		{
			try
			{
				string strActor=strActor1;
				RemoveInvalidChars(ref strActor);

				movies.Clear();
				if (null==m_db) return ;
				string strSQL;
				strSQL=String.Format("select * from actorlinkmovie,actors,movie,movieinfo,path where path.idpath=movie.idpath and actors.idActor=actorlinkmovie.idActor and actorlinkmovie.idmovie=movie.idmovie and movieinfo.idmovie=movie.idmovie and actors.stractor='{0}'", strActor);

				SQLiteResultSet results;
				results=m_db.Execute(strSQL);
				if (results.Rows.Count == 0)  return;
				for (int iRow=0; iRow <results.Rows.Count;iRow++)
				{
					IMDBMovie details=new IMDBMovie();
					details.Rating=(float)System.Double.Parse(Get(results,iRow,"movieinfo.fRating") );
					if (details.Rating>10.0f) details.Rating /=10.0f;
					details.Director=Get(results,iRow,"actors.strActor");
					details.WritingCredits=Get(results,iRow,"movieinfo.strCredits");
					details.TagLine=Get(results,iRow,"movieinfo.strTagLine");
					details.PlotOutline=Get(results,iRow,"movieinfo.strPlotOutline");
					details.Plot=Get(results,iRow,"movieinfo.strPlot");
					details.Votes=Get(results,iRow,"movieinfo.strVotes");
					details.Cast=Get(results,iRow,"movieinfo.strCast");
					details.Year=System.Int32.Parse(Get(results,iRow,"movieinfo.iYear"));
					details.Genre=Get(results,iRow,"movieinfo.strGenre");
					details.ThumbURL=Get(results,iRow,"movieinfo.strPictureURL");
					details.Title=Get(results,iRow,"movieinfo.strTitle");
					details.Path=Get(results,iRow,"path.strPath");
					details.DVDLabel=Get(results,iRow,"movie.discid") ;
					details.IMDBNumber=Get(results,iRow,"movieinfo.IMDBID") ;
					long lMovieId=System.Int32.Parse( Get(results,iRow,"movieinfo.idMovie") );
					details.SearchString= String.Format("{0}", lMovieId);
          details.CDLabel=Get(results,0,"path.cdlabel") ;
					movies.Add(details);
				}
			}
			catch (SQLiteException ex) 
			{
				Log.Write("videodatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
		}

    static public void GetMoviesByYear(string strYear, ref ArrayList movies)
		{
			try
			{
				int iYear=System.Int32.Parse(strYear);

				movies.Clear();
				if (null==m_db) return ;
				string strSQL;
				strSQL=String.Format("select * from movie,movieinfo,actors,path where path.idpath=movie.idpath and movieinfo.idmovie=movie.idmovie and movieinfo.iddirector=actors.idActor and movieinfo.iYear={0}",iYear);

				SQLiteResultSet results;
				results=m_db.Execute(strSQL);
				if (results.Rows.Count == 0)  return;
				for (int iRow=0; iRow <results.Rows.Count;iRow++)
				{
					IMDBMovie details=new IMDBMovie();
					details.Rating=(float)System.Double.Parse(Get(results,iRow,"movieinfo.fRating") );
					if (details.Rating>10.0f) details.Rating /=10.0f;
					details.Director=Get(results,iRow,"actors.strActor");
					details.WritingCredits=Get(results,iRow,"movieinfo.strCredits");
					details.TagLine=Get(results,iRow,"movieinfo.strTagLine");
					details.PlotOutline=Get(results,iRow,"movieinfo.strPlotOutline");
					details.Plot=Get(results,iRow,"movieinfo.strPlot");
					details.Votes=Get(results,iRow,"movieinfo.strVotes");
					details.Cast=Get(results,iRow,"movieinfo.strCast");
					details.Year=System.Int32.Parse(Get(results,iRow,"movieinfo.iYear"));
					details.Genre=Get(results,iRow,"movieinfo.strGenre");
					details.ThumbURL=Get(results,iRow,"movieinfo.strPictureURL");
					details.Title=Get(results,iRow,"movieinfo.strTitle");
					details.Path=Get(results,iRow,"path.strPath");
					details.DVDLabel=Get(results,iRow,"movie.discid") ;
					details.IMDBNumber=Get(results,iRow,"movieinfo.IMDBID") ;
					long lMovieId=System.Int32.Parse( Get(results,iRow,"movieinfo.idMovie") );
					details.SearchString= String.Format("{0}", lMovieId);
          details.CDLabel=Get(results,0,"path.cdlabel") ;
					movies.Add(details);
				}
			}
			catch (SQLiteException ex) 
			{
				Log.Write("videodatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}

    }

    static public void GetMoviesByPath(string strPath1, ref ArrayList movies)
		{
			try
			{
				string strPath=strPath1;
        if (strPath.Length>0)
        {
          if ( strPath[strPath.Length-1]=='/' || strPath[strPath.Length-1]=='\\')
            strPath = strPath.Substring(0,strPath.Length-1);
        }

				RemoveInvalidChars(ref strPath);



				movies.Clear();
				if (null==m_db) return ;
				int lPathId = GetPath(strPath);
				if (lPathId< 0) return;
				string strSQL;
				strSQL=String.Format("select * from files,movieinfo where files.idpath={0} and files.idMovie=movieinfo.idMovie", lPathId);

				SQLiteResultSet results;
				results=m_db.Execute(strSQL);
				if (results.Rows.Count == 0)  return;
				for (int iRow=0; iRow <results.Rows.Count;iRow++)
				{
					IMDBMovie details=new IMDBMovie();
					long lMovieId=System.Int32.Parse( Get(results,iRow,"files.idMovie") );
					details.SearchString= String.Format("{0}", lMovieId);
					details.IMDBNumber=Get(results,iRow,"movieinfo.IMDBID") ;
          details.Title=Get(results,iRow,"movieinfo.strTitle");
					details.File=Get(results,iRow,"files.strFilename");

					movies.Add(details);
				}
			}
			catch (SQLiteException ex) 
			{
				Log.Write("videodatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
		}

    static public void SetMovieInfo( string strFilenameAndPath, ref IMDBMovie details)
    {
      if (strFilenameAndPath.Length==0) return ;
      int lMovieId=GetMovie(strFilenameAndPath,true);
      if (lMovieId< 0) return;
      SetMovieInfoById(lMovieId,ref details);

      string strPath, strFileName ;
      Split(strFilenameAndPath, out strPath, out strFileName); 
      details.Path=strPath;
      details.File=strFileName;
    }
    static public void SetMovieInfoById( int lMovieId, ref IMDBMovie details)
    {
      try
      {
        details.SearchString=String.Format("{0}", lMovieId);

				IMDBMovie details1=details;
				string strLine;
				strLine=details1.Cast;RemoveInvalidChars(ref strLine); details1.Cast=strLine;
				strLine=details1.Director;RemoveInvalidChars(ref strLine);  details1.Director=strLine;  
				strLine=details1.Plot;RemoveInvalidChars(ref strLine);  details1.Plot=strLine;  
				strLine=details1.PlotOutline;RemoveInvalidChars(ref strLine);  details1.PlotOutline=strLine;  
				strLine=details1.TagLine;RemoveInvalidChars(ref strLine);  details1.TagLine=strLine;  
				strLine=details1.ThumbURL;RemoveInvalidChars(ref strLine);  details1.ThumbURL=strLine;  
				strLine=details1.SearchString;RemoveInvalidChars(ref strLine);  details1.SearchString=strLine; 
				strLine=details1.Title;RemoveInvalidChars(ref strLine);  details1.Title=strLine;  
				strLine=details1.Votes;RemoveInvalidChars(ref strLine);  details1.Votes=strLine; 
				strLine=details1.WritingCredits;RemoveInvalidChars(ref strLine);  details1.WritingCredits=strLine;  
				strLine=details1.Genre;RemoveInvalidChars(ref strLine);  details1.Genre=strLine;
				strLine=details1.IMDBNumber;RemoveInvalidChars(ref strLine);  details1.IMDBNumber=strLine;  

				// add director
				int lDirector=AddActor(details.Director);
    
				// add all genres
				string szGenres=details.Genre;
				ArrayList vecGenres=new ArrayList();
				if ( szGenres.IndexOf("/")>=0 )
				{
					Tokens f = new Tokens(szGenres, new char[] {'/'} );
					foreach (string strGenre in f)
					{ 
						strGenre.Trim();
						int lGenreId=AddGenre(strGenre);
						vecGenres.Add(lGenreId);
					}
				}
				else
				{
					string strGenre=details.Genre; 
					strGenre.Trim();
					int lGenreId=AddGenre(strGenre);
					vecGenres.Add(lGenreId);
				}

				// add cast...
				ArrayList vecActors=new ArrayList();
				string strCast;
				int ipos=0;
				strCast=details.Cast;
				ipos=strCast.IndexOf(" as ");
				while (ipos > 0)
				{
					string strActor;
					int x=ipos;
					while (x > 0)
					{
						if (strCast[x] != '\r'&& strCast[x]!='\n') x--;
						else 
						{
							x++;
							break;
						}
					}
					strActor=strCast.Substring(x,ipos-x);          
					int lActorId=AddActor(strActor);
					vecActors.Add(lActorId);
					ipos=strCast.IndexOf(" as ",ipos+3);
				}

				for (int i=0; i < vecGenres.Count; ++i)
				{
					AddGenreToMovie(lMovieId,(int)vecGenres[i]);
				}
    
				for (int i=0; i < vecActors.Count; i++)
				{
					AddActorToMovie(lMovieId,(int)vecActors[i]);
				}

				string strSQL;
				string strRating;
				strRating=String.Format("{0}", details1.Rating);
				if (strRating=="") strRating="0.0";
				strSQL=String.Format("select * from movieinfo where idmovie={0}", lMovieId);
				
				SQLiteResultSet results;
				results=m_db.Execute(strSQL);
				if (results.Rows.Count == 0) 
				{
					strSQL=String.Format("insert into movieinfo ( idMovie,idDirector,strPlotOutline,strPlot,strTagLine,strVotes,fRating,strCast,strCredits , iYear  ,strGenre, strPictureURL, strTitle,IMDBID) values({0},{1},'{2}','{3}','{4}','{5}','{6}','{7}','{8}',{9},'{10}','{11}','{12}','{13}')",
															lMovieId,lDirector, details1.PlotOutline,
															details1.Plot,details1.TagLine,
															details1.Votes,strRating,
															details1.Cast,details1.WritingCredits,
										            
															details1.Year, details1.Genre,
															details1.ThumbURL,details1.Title,
															details1.IMDBNumber );

					m_db.Execute(strSQL);
                
				}
				else
				{
					strSQL=String.Format("update movieinfo set idDirector={0}, strPlotOutline='{1}', strPlot='{2}', strTagLine='{3}', strVotes='{4}', fRating='{5}', strCast='{6}',strCredits='{7}', iYear={8}, strGenre='{9}' strPictureURL='{10}', strTitle='{11}' IMDBID='{12}' where idMovie={13}",
																		lDirector,details1.PlotOutline,
																		details1.Plot,details1.TagLine,
																		details1.Votes,strRating,
																		details1.Cast,details1.WritingCredits,
																		details1.Year,details1.Genre,
																		details1.ThumbURL,details1.Title,
																		details1.IMDBNumber,
																		lMovieId);
					m_db.Execute(strSQL);
				}
			}
			catch (SQLiteException ex) 
			{
				Log.Write("videodatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
		}


    static public void DeleteMovieStopTime(int iMovieId)
    {
      try
      {
        string sql = String.Format("delete from resume where idMovie={0}", iMovieId);
        m_db.Execute(sql);
      }
      catch (SQLiteException ex) 
      {
        Log.Write("videodatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
      }
    }

    static public int GetMovieStopTime(int iMovieId)
    {
      try
      {
        string sql = string.Format("select * from resume where idMovie={0}", iMovieId);
        SQLiteResultSet results;
        results=m_db.Execute(sql);
        if (results.Rows.Count == 0) return 0;
        int stoptime=Int32.Parse(Get(results,0,"stoptime")) ;
        return stoptime;

      }
      catch (SQLiteException ex) 
      {
        Log.Write("videodatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
      }
      return 0;
    }

    static public void SetMovieStopTime(int iMovieId, int stoptime)
    {
      try
      {
        string sql = String.Format("select * from resume where idMovie={0}", iMovieId);
        SQLiteResultSet results;
        results=m_db.Execute(sql);
        if (results.Rows.Count == 0) 
        {
					  sql=String.Format("insert into resume ( idResume,idMovie,stoptime) values(NULL,{0},{1})",
                iMovieId,stoptime);
        }
        else
        {
					  sql=String.Format("update resume set stoptime={0} where idMovie={1}",
                iMovieId,stoptime);
        }
        m_db.Execute(sql);
      }
      catch (SQLiteException ex) 
      {
        Log.Write("videodatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
      }
    }

	}
}

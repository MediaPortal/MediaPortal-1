using System;
using System.Collections;
using SQLite.NET;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using MediaPortal.Database;

namespace MediaPortal.Picture.Database
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class PictureDatabase : IDisposable
	{
    bool disposed=false;
		SQLiteClient m_db=null;

    

    public PictureDatabase()
		{
      lock (typeof(PictureDatabase))
      {
        Log.Write("opening picture database");
        try 
        {
          // Open database
					try
					{
          System.IO.Directory.CreateDirectory("database");
					}
					catch(Exception){}
					m_db = new SQLiteClient(@"database\PictureDatabase.db");
          CreateTables();

        } 
        catch (Exception ex) 
        {
          Log.Write("picture database exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
        }
        Log.Write("picture database opened");
      }
		}
		bool CreateTables()
		{
      lock (typeof(PictureDatabase))
      {
        if (m_db==null) return false;
        DatabaseUtility.AddTable(m_db,"picture","CREATE TABLE picture ( idPicture integer primary key, strFile text, iRotation integer)\n");
        return true;
      }
		}

		public int AddPicture(string strPicture, int iRotation)
		{
      lock (typeof(PictureDatabase))
      {
        if (m_db==null) return -1;
        string strSQL="";
        try
        {
          int lPicId=-1;
          SQLiteResultSet results;
          string strPic=strPicture;
          DatabaseUtility.RemoveInvalidChars(ref strPic);

          strSQL=String.Format("select * from picture where strFile like '{0}'",strPic);
          results=m_db.Execute(strSQL);
          if (results!=null && results.Rows.Count>0) 
          {
            lPicId=System.Int32.Parse( DatabaseUtility.Get(results,0,"idPicture"));
            return lPicId;
          }
          strSQL=String.Format ("insert into picture (idPicture, strFile, iRotation) values(null, '{0}',{1})", strPic, iRotation);
          results=m_db.Execute(strSQL);
          lPicId=m_db.LastInsertID();
          return lPicId;
        }
        catch (SQLiteException ex) 
        {
          Log.Write("MediaPortal.Picture.Database exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
        }
        return -1;
      }
		}


		public int GetRotation(string strPicture)
		{
      lock (typeof(PictureDatabase))
      {
        if (m_db==null) return -1;
        string strSQL="";
        try
        {
          SQLiteResultSet results;
          string strPic=strPicture;
          DatabaseUtility.RemoveInvalidChars(ref strPic);

          strSQL=String.Format("select * from picture where strFile like '{0}'",strPic);
          results=m_db.Execute(strSQL);
          if (results!=null && results.Rows.Count>0) 
          {
            int iRotation=System.Int32.Parse( DatabaseUtility.Get(results,0,"iRotation"));
            return iRotation;
          }
          AddPicture(strPicture,0);
          return 0;
        }
        catch (SQLiteException ex) 
        {
          Log.Write("MediaPortal.Picture.Database exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
        }
        return 0;
      }
		}

		public void SetRotation(string strPicture, int iRotation)
		{
      lock (typeof(PictureDatabase))
      {
        if (m_db==null) return ;
        string strSQL="";
        try
        {
          SQLiteResultSet results;
          string strPic=strPicture;
          DatabaseUtility.RemoveInvalidChars(ref strPic);

          long lPicId=AddPicture(strPicture,iRotation);
          if (lPicId>=0)
          {
            strSQL=String.Format("update picture set iRotation={0} where strFile like '{1}'",iRotation,strPic);
            results=m_db.Execute(strSQL);
          }
        }
        catch (SQLiteException ex) 
        {
          Log.Write("MediaPortal.Picture.Database exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
        }
      }
    }
    #region IDisposable Members

    public void Dispose()
    {
      if (!disposed)
      {
        disposed=true;
        if (m_db!=null)
        {
          try
          {
            m_db.Close();
          }
          catch (Exception){}
          m_db=null;
        }
      }
    }

    #endregion
  }
}

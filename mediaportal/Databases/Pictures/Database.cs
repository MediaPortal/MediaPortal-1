using System;
using System.Collections;
using SQLite.NET;
using MediaPortal.Util;
using MediaPortal.GUI.Library;

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
          System.IO.Directory.CreateDirectory("database");
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

		
		void AddTable( string strTable, string strSQL)
		{
      lock (typeof(PictureDatabase))
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
          Log.Write("picture database exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
        }
        return ;
      }
		}
		bool CreateTables()
		{
      lock (typeof(PictureDatabase))
      {
        if (m_db==null) return false;
        AddTable("picture","CREATE TABLE picture ( idPicture integer primary key, strFile text, iRotation integer)\n");
        return true;
      }
		}

		public string Get(SQLiteResultSet results,int iRecord,string strColum)
		{
      lock (typeof(PictureDatabase))
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
		}

		
    public void RemoveInvalidChars(ref string strTxt)
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
          RemoveInvalidChars(ref strPic);

          strSQL=String.Format("select * from picture where strFile like '{0}'",strPic);
          results=m_db.Execute(strSQL);
          if (results!=null && results.Rows.Count>0) 
          {
            lPicId=System.Int32.Parse( Get(results,0,"idPicture"));
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
          RemoveInvalidChars(ref strPic);

          strSQL=String.Format("select * from picture where strFile like '{0}'",strPic);
          results=m_db.Execute(strSQL);
          if (results!=null && results.Rows.Count>0) 
          {
            int iRotation=System.Int32.Parse( Get(results,0,"iRotation"));
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
          RemoveInvalidChars(ref strPic);

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

using System;
using System.Collections;
using SQLite.NET;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using MediaPortal.GUI.Library;

namespace MediaPortal.Util
{
	/// <summary>
	/// 
	/// </summary>
	public class FolderSettings
	{
    static public SQLiteClient m_db=null;

    private FolderSettings()
    {
    }

		static FolderSettings()
		{
      try 
      {
        // Open database
        Log.Write("open folderdatabase");
        m_db = new SQLiteClient(@"database\FolderDatabase2.db3");

				m_db.Execute("PRAGMA cache_size=2000;\n");
				m_db.Execute("PRAGMA synchronous='OFF';\n");
				m_db.Execute("PRAGMA count_changes=1;\n");
				m_db.Execute("PRAGMA full_column_names=0;\n");
				m_db.Execute("PRAGMA short_column_names=0;\n");
				AddTable("path","CREATE TABLE path ( idPath integer primary key, strPath text);\n");
				AddTable("setting","CREATE TABLE setting ( idSetting integer primary key, idPath integer , key text, value text);\n");

      } 
      catch (Exception ex) 
      {
        Log.Write("folderdatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
      }
    }
  
		static public bool AddTable( string strTable, string strSQL)
		{
			//	lock (typeof(DatabaseUtility))
		
			Log.Write("AddTable:[{0}]",strTable);
			if (m_db==null) 
			{
				Log.Write("AddTable: database not opened");
				return false;
			}
			if (strSQL==null) 
			{
				Log.Write("AddTable: no sql?");
				return false;
			}
			if (strTable==null) 
			{
				Log.Write("AddTable: No table?");
				return false;
			}
			if (strTable.Length==0) 
			{
				Log.Write("AddTable: empty table?");
				return false;
			}
			if (strSQL.Length==0) 
			{
				Log.Write("AddTable: empty sql?");
				return false;
			}

			//Log.Write("check for  table:{0}", strTable);
			SQLiteResultSet results;
			results = m_db.Execute("SELECT name FROM sqlite_master WHERE name='"+strTable+"' and type='table' UNION ALL SELECT name FROM sqlite_temp_master WHERE type='table' ORDER BY name");
			if (results!=null)
			{
				if (results.Rows.Count>0) return false;
			}

			try 
			{
				//Log.Write("create table:{0}", strSQL);
				m_db.Execute(strSQL);
				//Log.Write("table created");
			}
			catch (SQLiteException ex) 
			{
				Log.WriteFile(Log.LogType.Log,true,"DatabaseUtility exception err:{0} stack:{1} sql:{2}", ex.Message,ex.StackTrace,strSQL);
			}
			return true;
		}



    static string Get(SQLiteResultSet results, int iRecord, string strColum)
    {
      if (null == results) return "";
      if (results.Rows.Count < iRecord) return "";
      ArrayList arr = (ArrayList)results.Rows[iRecord];
      int iCol = 0;
      foreach (string columnName in results.ColumnNames)
      {
        if (strColum == columnName)
        {
          string strLine = ((string)arr[iCol]).Trim();
          strLine = strLine.Replace("''","'");
          return strLine;
        }
        iCol++;
      }
      return "";
    }

    static void RemoveInvalidChars(ref string strTxt)
    {
			if (strTxt==null) return;
      string strReturn = "";
      for (int i = 0; i < (int)strTxt.Length; ++i)
      {
        char k = strTxt[i];
        if (k == '\'') 
        {
          strReturn += "'";
        }
        strReturn += k;
      }
      if (strReturn == "") 
        strReturn = Strings.Unknown;
      strTxt = strReturn.Trim();
    }

    static int AddPath(string FilteredPath)
    {
			if (FilteredPath==null) return -1;
			if (FilteredPath==String.Empty) return -1;
      if (null == m_db) return -1;
      try
      {
        SQLiteResultSet results;
        string strSQL = String.Format("select * from path where strPath like '{0}'", FilteredPath);
        results = m_db.Execute(strSQL);
        if (results.Rows.Count == 0) 
        {
          // doesnt exists, add it
          strSQL = String.Format("insert into path (idPath, strPath) values ( NULL, '{0}' )", FilteredPath);
          m_db.Execute(strSQL);
          return m_db.LastInsertID();
        }
        else
        {
          return Int32.Parse(Get(results, 0, "idPath"));
        }
      } 
      catch (Exception ex) 
      {
        Log.Write("folderdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
      return -1;
    }

    static public void DeleteFolderSetting(string strPath, string Key)
		{
			if (strPath==null) return;
			if (strPath==String.Empty) return;
			if (Key==null) return;
			if (Key==String.Empty) return;
      if (null == m_db) return ;
      try
      {
        string strPathFiltered = Utils.RemoveTrailingSlash(strPath);
        string KeyFiltered=Key;
        RemoveInvalidChars(ref strPathFiltered );
        RemoveInvalidChars(ref KeyFiltered );

        int PathId=AddPath(strPathFiltered);
        if (PathId<0) return ;
        string strSQL = String.Format("delete from setting where idPath={0} and key ='{1}'", PathId, KeyFiltered);
        m_db.Execute(strSQL);
      }
      catch (Exception ex) 
      {
        Log.Write("folderdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
    }

    static public void AddFolderSetting(string strPath, string Key, Type type, object Value)
		{
			if (strPath==null) return;
			if (strPath==String.Empty) return;
			if (Key==null) return;
			if (Key==String.Empty) return;

      if (null == m_db) return ;
      try
      {
        string strPathFiltered = Utils.RemoveTrailingSlash(strPath);
        string KeyFiltered=Key;
        RemoveInvalidChars(ref strPathFiltered );
        RemoveInvalidChars(ref KeyFiltered );

        int PathId=AddPath(strPathFiltered);
        if (PathId<0) return ;

        DeleteFolderSetting(strPath,Key);


        XmlSerializer serializer = new XmlSerializer(type);
        //serialize...
        using (MemoryStream strm = new MemoryStream())
        {
          using (TextWriter w = new StreamWriter(strm))
          {
            serializer.Serialize(w,Value);
            w.Flush();
            strm.Seek(0,SeekOrigin.Begin);

            using (TextReader reader = new StreamReader(strm))
            {
              string ValueText=reader.ReadToEnd();
              string ValueTextFiltered = ValueText;
              RemoveInvalidChars(ref ValueTextFiltered);

              string strSQL = String.Format("insert into setting (idSetting,idPath, key,value) values(NULL, {0}, '{1}', '{2}') ", PathId, KeyFiltered,ValueTextFiltered);
              m_db.Execute(strSQL);
            }
          }
        }
      }
      catch (Exception ex) 
      {
        Log.Write("folderdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);

      }
    }

    static public void GetFolderSetting(string strPath, string Key, Type type, out object Value)
    {
      Value=null;
			if (strPath==null) return;
			if (strPath==String.Empty) return;
			if (Key==null) return;
			if (Key==String.Empty) return;

      if (null == m_db) return ;
      try
      {
        string strPathFiltered = Utils.RemoveTrailingSlash(strPath);
        string KeyFiltered=Key;
        RemoveInvalidChars(ref strPathFiltered );
        RemoveInvalidChars(ref KeyFiltered );

        int PathId=AddPath(strPathFiltered);
        if (PathId<0) return ;

        SQLiteResultSet results;
        string strSQL = String.Format("select * from setting where idPath={0} and key='{1}'", PathId, KeyFiltered);
        results=m_db.Execute(strSQL);
        if (results.Rows.Count == 0) 
        {
          return;
        }
        string strValue=Get(results,0,"value");

        //deserialize...

        using (MemoryStream strm = new MemoryStream())
        {
          using (StreamWriter writer = new StreamWriter(strm))
          {
            writer.Write(strValue);
            writer.Flush();
            strm.Seek(0,SeekOrigin.Begin);
            TextReader r = new StreamReader( strm );
            try
            {
              XmlSerializer serializer= new XmlSerializer( type );
              Value=serializer.Deserialize(r);
            }
            catch(Exception )
            {
            }
          }
        }
      }
      catch (Exception ex) 
      {
        Log.Write("folderdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
    }
	}
}

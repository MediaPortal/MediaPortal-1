using System;
using System.Collections;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using SQLite.NET;

namespace MediaPortal.Radio.Database
{
  public class RadioDatabase
  {
    static SQLiteClient m_db=null;

    // singleton. Dont allow any instance of this class
    private RadioDatabase()
    {
    }

    static RadioDatabase()
    {
      try 
      {
        // Open database
        Log.Write("open radiodatabase");
        System.IO.Directory.CreateDirectory("database");
        m_db = new SQLiteClient(@"database\RadioDatabase.db");
        CreateTables();

        if (m_db!=null)
        {
          m_db.Execute("PRAGMA cache_size=8192\n");
          m_db.Execute("PRAGMA synchronous='OFF'\n");
          m_db.Execute("PRAGMA count_changes='OFF'\n");
        }

      } 
      catch (Exception ex) 
      {
        Log.Write("RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
      }
      Log.Write("Radio database opened");
    }
  
    static bool AddTable( string strTable, string strSQL)
    {
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
        //Log.Write("  results:{0}", results.Rows.Count);
        if (results.Rows.Count==1) 
        {
          //Log.Write(" check result:0");
          ArrayList arr = (ArrayList)results.Rows[0];
          if (arr.Count==1)
          {

            if ( (string)arr[0] == strTable) 
            {
              //Log.Write(" table exists");
              return false;
            }
            //Log.Write(" table has different name:{0}", (string)arr[0]);
          }
          //else Log.Write(" array contains:{0} items?", arr.Count);
        }
      }

      try 
      {
        //Log.Write("create table:{0}", strSQL);
        m_db.Execute(strSQL);
        //Log.Write("table created");
      }
      catch (SQLiteException ex) 
      {
        Log.Write("RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
      }
      return true;
    }

    static bool CreateTables()
    {
      if (m_db==null) return false;
      if ( AddTable("station","CREATE TABLE station ( idStation integer primary key, strName text, iChannelNr integer, frequency text, URL text, genre text, bitrate int)\n"))
      {
        m_db.Execute("CREATE INDEX idxStation ON station(idStation)");
      }
      

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
          if (arr[iCol]!=null)
            return ((string)arr[iCol]).Trim();
          else return "";
        }
        iCol++;
      }
      return "";
    }

    
    static public void RemoveInvalidChars(ref string strTxt)
    {
      if (strTxt==null) return ;
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

    static public void GetStations(ref ArrayList stations)
    {
      stations.Clear();
      if (m_db==null) return ;
      lock (m_db)
      {
        try
        {
          if (null==m_db) return ;
          string strSQL;
          strSQL=String.Format("select * from station order by iChannelNr");
          SQLiteResultSet results;
          results=m_db.Execute(strSQL);
          if (results.Rows.Count== 0) return ;
          for (int i=0; i < results.Rows.Count;++i)
          {
            RadioStation chan=new RadioStation();
            try
            {
              chan.ID=Int32.Parse(Get(results,i,"idStation"));
            }catch(Exception){}
            try
            {
              chan.Channel = Int32.Parse(Get(results,i,"iChannelNr"));
            }catch(Exception){}
            try
            {
              chan.Frequency = Int64.Parse(Get(results,i,"frequency"));
            }catch(Exception)
            {}
            chan.Name = Get(results,i,"strName");
            chan.URL = Get(results,i,"URL");
            if (chan.URL.Equals("unknown")) chan.URL ="";
            try
            {
            chan.BitRate=Int32.Parse( Get(results,i,"bitrate") );
            }
            catch(Exception){}

            chan.Genre=Get(results,i,"genre") ;
            stations.Add(chan);
          }

          return ;
        }
        catch(SQLiteException ex)
        {
          Log.Write("RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
        }
        return ;
      }
    }

    
    static public int AddStation(ref RadioStation channel)
    {
        lock (m_db)
        {
          string strSQL;
          try
          {
            string strChannel=channel.Name;
            string strURL    =channel.URL;
            string strGenre    =channel.Genre;
            RemoveInvalidChars(ref strChannel);
            RemoveInvalidChars(ref strURL);
            RemoveInvalidChars(ref strGenre);

            if (null==m_db) return -1;
            SQLiteResultSet results;
            strSQL=String.Format( "select * from station where strName like '{0}'", strChannel);
            results=m_db.Execute(strSQL);
            if (results.Rows.Count==0) 
            {
              // doesnt exists, add it
              strSQL=String.Format("insert into station (idStation, strName,iChannelNr ,frequency,URL,bitrate,genre) values ( NULL, '{0}', {1}, {2}, '{3}',{4},'{5}' )", 
                                    strChannel,channel.Channel,channel.Frequency.ToString(),strURL, channel.BitRate,strGenre);
              m_db.Execute(strSQL);
              int iNewID=m_db.LastInsertID();
              channel.ID=iNewID;
              return iNewID;
            }
            else
            {
              int iNewID=Int32.Parse(Get(results,0,"idStation"));
              channel.ID=iNewID;
              return iNewID;
            }
          } 
          catch (SQLiteException ex) 
          {
            Log.Write("RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
          }

          return -1;
        }
    }

    static public int GetStationId(string strStation)
    {
      lock (m_db)
      {
        string strSQL;
        try
        {
          if (null==m_db) return -1;
          SQLiteResultSet results;
          strSQL=String.Format( "select * from station where strName like '{0}'", strStation);
          results=m_db.Execute(strSQL);
          if (results.Rows.Count==0) return -1;
          int iNewID=Int32.Parse(Get(results,0,"idStation"));
          return iNewID;
        } 
        catch (SQLiteException ex) 
        {
          Log.Write("RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
        }

        return -1;
      }
    }


    static public void RemoveStation(string strStationName)
    {
      lock (m_db)
      {
        if (null==m_db) return ;

        int iChannelId=GetStationId(strStationName);
        if (iChannelId<0) return ;
        
        try
        {
          if (null==m_db) return ;
          string strSQL=String.Format("delete from station where idStation={0}", iChannelId);
          m_db.Execute(strSQL);
        }
        catch(SQLiteException ex)
        {
          Log.Write("RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
        }
      }
    }   
    
    static public void RemoveStations()
    {
      lock (m_db)
      {
        if (null==m_db) return ;
        
        try
        {
          if (null==m_db) return ;
          string strSQL=String.Format("delete from station ");
          m_db.Execute(strSQL);
        }
        catch(SQLiteException ex)
        {
          Log.Write("RadioDatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
        }
      }
    }
  }
}

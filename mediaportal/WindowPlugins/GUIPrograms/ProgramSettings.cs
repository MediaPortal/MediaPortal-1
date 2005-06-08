using System;
using System.Collections;
using SQLite.NET;
using WindowPlugins.GUIPrograms;

namespace ProgramsDatabase
{
  /// <summary>
  /// Summary description for ProgramSettings.
  /// </summary>
  public class ProgramSettings
  {
    static public SQLiteClient sqlDB = null;
    static public ProgramViewHandler viewHandler = null;

    // singleton. Dont allow any instance of this class
    private ProgramSettings(){}

    static ProgramSettings(){}


    static public string ReadSetting(string Key)
    {
      SQLiteResultSet results;
      string res = null;
      string SQL = "SELECT value FROM setting WHERE key ='" + Key + "'";
      results = sqlDB.Execute(SQL);
      if (results != null && results.Rows.Count > 0)
      {
        ArrayList arr = (ArrayList)results.Rows[0];
        res = (string)arr[0];
      }
      //Log.Write("dw read setting key:{0}\nvalue:{1}", Key, res);
      return res;
    }

    static int CountKey(string Key)
    {
      SQLiteResultSet results;
      int res = 0;
      results = sqlDB.Execute("SELECT COUNT(*) FROM setting WHERE key ='" + Key + "'");
      if (results != null && results.Rows.Count > 0)
      {
        ArrayList arr = (ArrayList)results.Rows[0];
        res = Int32.Parse((string)arr[0]);
      }
      return res;
    }

    static public bool KeyExists(string Key)
    {
      return (CountKey(Key) > 0);
    }

    static public void WriteSetting(string Key, string Value)
    {
      if (KeyExists(Key))
      {
        sqlDB.Execute("update setting set value = '" + Value + "' where key = '" + Key + "'");
      }
      else
      {
        sqlDB.Execute("insert into setting (key, value) values ('" + Key + "', '" + Value + "');");
      }
    }

    static public void DeleteSetting(string Key)
    {
      sqlDB.Execute("delete from setting where key = '" + Key + "'");
    }



  }
}

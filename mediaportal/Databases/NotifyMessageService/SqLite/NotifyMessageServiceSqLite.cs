using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SQLite.NET;
using MediaPortal.GUI.Library;
using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.Services;

namespace MediaPortal.NotifyMessageService.Database
{
  public class NotifyMessageServiceSqLite
  {
    private SQLiteClient _db;
    private bool _dbHealth = false;

    public NotifyMessageServiceSqLite()
    {
      this.open();
    }

    public void Clear()
    {
      lock (typeof(NotifyMessageServiceSqLite))
      {
        if (this._db == null)
          return;

        try
        {
          this._db.Execute("DELETE FROM messages");
        }
        catch (Exception ex)
        {
          Log.Error("[NotifyMessageServiceSqLite][Clear] Exception:{0} stack:{1}", ex.Message, ex.StackTrace);
        }
      }
    }

    internal List<NotifyMessage> GetMessages()
    {
      lock (typeof(NotifyMessageServiceSqLite))
      {
        try
        {
          List<NotifyMessage> list = new List<NotifyMessage>();

          string strSQL;
          SQLiteResultSet result;

          strSQL = string.Format("SELECT * FROM messages");

          result = this._db.Execute(strSQL);
          if (result != null && result.Rows.Count > 0)
          {
            for (int i = 0; i < result.Rows.Count; i++)
            {
              list.Add(new NotifyMessage(
                DatabaseUtility.GetAsInt(result, i, "id"),
                DatabaseUtility.Get(result, i, "idMessage"),
                DatabaseUtility.GetAsInt(result, i, "idPlugin"),
                DatabaseUtility.Get(result, i, "origin"),
                DatabaseUtility.Get(result, i, "title"),
                DatabaseUtility.Get(result, i, "description"),
                DatabaseUtility.Get(result, i, "author"),
                (NotifyMessageDialogModeEnum)DatabaseUtility.GetAsInt(result, i, "dialog"),
                DatabaseUtility.GetAsInt(result, i, "pluginActivate") > 0,
                DatabaseUtility.Get(result, i, "pluginArgs"),
                DatabaseUtility.GetAsInt(result, i, "del") > 0,
                DatabaseUtility.Get(result, i, "logo"),
                DatabaseUtility.Get(result, i, "thumb"),
                new DateTime(DatabaseUtility.GetAsInt64(result, i, "tsPublished")),
                DatabaseUtility.GetAsInt(result, i, "ttl"),
                DatabaseUtility.Get(result, i, "tag"),
                (NotifyMessageStatusEnum)DatabaseUtility.GetAsInt(result, i, "status"),
                new DateTime(DatabaseUtility.GetAsInt64(result, i, "ts")),
                (NotifyMessageClassEnum)DatabaseUtility.GetAsInt64(result, i, "class"),
                (NotifyMessageLevelEnum)DatabaseUtility.GetAsInt(result, i, "level"),
                DatabaseUtility.Get(result, i, "mediaLink")
                ));
            }
          }

          return list;
        }

        catch (Exception ex)
        {
          Log.Error("[NotifyMessageServiceSqLite][GetMessages] Exception:{0} stack:{1}", ex.Message, ex.StackTrace);
        }

        return null;
      }
    }

    public int MessageCreate(string strMessageId, INotifyMessage msg)
    {
      if (msg == null)
        return -1;

      Log.Debug("[NotifyMessageServiceSqLite][MessageCreate] ID:" + strMessageId);
      lock (typeof(NotifyMessageServiceSqLite))
      {
        try
        {
          string strSQL = String.Format("INSERT INTO messages (id, idMessage, ttl, status, dialog, del, logo, ts, idPlugin, pluginArgs, origin, title, description, tag, pluginActivate, tsPublished, thumb, author, class, level, mediaLink)" +
                   " VALUES(null, '{0}',{1},{2},{3},{4},'{5}',{6},{7},'{8}','{9}','{10}','{11}','{12}',{13},{14},'{15}','{16}',{17},{18},'{19}')",
                  sanitySqlValue(strMessageId),
                  msg.MessageTTL,
                  (int)msg.Status,
                  (int)msg.DialogMode,
                  msg.DeleteMessageAfterPresentation ? 1 : 0,
                  sanitySqlValue(msg.OriginLogo),
                  msg.TimeStamp.Ticks,
                  msg.PluginId,
                  sanitySqlValue(msg.PluginArguments),
                  sanitySqlValue(msg.Origin),
                  sanitySqlValue(msg.Title),
                  sanitySqlValue(msg.Description),
                  sanitySqlValue(msg.Tag),
                  msg.ActivatePluginWindow ? 1 : 0,
                  msg.PublishDate.ToUniversalTime().Ticks,
                  sanitySqlValue(msg.Thumb),
                  sanitySqlValue(msg.Author),
                  (long)msg.Class,
                  (int)msg.Level,
                  sanitySqlValue(msg.MediaLink)
                  );
          SQLiteResultSet result = this._db.Execute(strSQL);
          return this._db.LastInsertID();

        }
        catch (Exception ex)
        {
          Log.Error("[NotifyMessageServiceSqLite][MessageCreate] Exception:{0} stack:{1}", ex.Message, ex.StackTrace);
        }

        return -1;
      }
    }

    public bool MessageDelete(string strID)
    {
      if (string.IsNullOrWhiteSpace(strID))
        return false;

      Log.Debug("[NotifyMessageServiceSqLite][MessageDelete] ID:" + strID);
      lock (typeof(NotifyMessageServiceSqLite))
      {
        try
        {
          this._db.Execute("DELETE FROM messages WHERE idMessage='" + strID + '\'');
          return true;
        }
        catch (Exception ex)
        {
          Log.Error("[NotifyMessageServiceSqLite][MessageDelete] Exception:{0} stack:{1}", ex.Message, ex.StackTrace);
        }
      }

      return false;
    }

    public bool MessageSetStatus(string strID, NotifyMessageStatusEnum status)
    {
      if (string.IsNullOrWhiteSpace(strID))
        return false;

      Log.Debug("[NotifyMessageServiceSqLite][MessageSetStatus] ID:" + strID);
      lock (typeof(NotifyMessageServiceSqLite))
      {
        try
        {
          this._db.Execute("UPDATE messages SET status=" + (int)status + " WHERE idMessage='" + strID + '\'');
          return true;
        }
        catch (Exception ex)
        {
          Log.Error("[NotifyMessageServiceSqLite][MessageSetStatus] Exception:{0} stack:{1}", ex.Message, ex.StackTrace);
        }
      }

      return false;
    }

    public bool MessageClearDialogMode(string strID)
    {
      if (string.IsNullOrWhiteSpace(strID))
        return false;

      Log.Debug("[NotifyMessageServiceSqLite][MessageClearDialogMode] ID:" + strID);
      lock (typeof(NotifyMessageServiceSqLite))
      {
        try
        {
          this._db.Execute("UPDATE messages SET dialog=0 WHERE idMessage='" + strID + '\'');
          return true;
        }
        catch (Exception ex)
        {
          Log.Error("[NotifyMessageServiceSqLite][MessageClearDialogMode] Exception:{0} stack:{1}", ex.Message, ex.StackTrace);
        }
      }

      return false;
    }


    private void open()
    {
      Log.Info("[NotifyMessageServiceSqLite][open] Opening the database...");
      lock (typeof(NotifyMessageServiceSqLite))
      {
        try
        {
          if (this._db != null)
          {
            Log.Info("[NotifyMessageServiceSqLite][open] Already opened.");
            return;
          }

          // Open database
          string strPath = Config.GetFolder(Config.Dir.Database);
          try
          {
            Directory.CreateDirectory(strPath);
          }
          catch (Exception ex)
          {
            Log.Error("[NotifyMessageServiceSqLite][open] Excetion: {0}", ex.Message);
          }

          //this._db = new SQLiteClient(Config.GetFile(Config.Dir.Database, @"NotifyMessageDatabaseV1.db3"));
          this._db = new SQLiteClient(Directory.Exists("m:\\") ? @"m:\Team MediaPortal\MediaPortal\database\NotifyMessageDatabaseV1.db3" : Config.GetFile(Config.Dir.Database, @"NotifyMessageDatabaseV1.db3"));

          this._dbHealth = DatabaseUtility.IntegrityCheck(this._db);

          DatabaseUtility.SetPragmas(this._db);

          this.createTables();
          this.upgradeDatabase();

          Log.Info("[NotifyMessageServiceSqLite][open] Database opened.");
        }
        catch (Exception ex)
        {
          Log.Error("[NotifyMessageServiceSqLite][open] Exception:{0} stack:{1}", ex.Message, ex.StackTrace);
          this._db = null;
        }
      }
    }

    private void createTables()
    {
      if (this._db == null)
        return;

      #region Tables
      DatabaseUtility.AddTable(this._db, "messages",
        "CREATE TABLE messages (id INTEGER primary key, idMessage TEXT, ttl INTEGER, status INT, dialog INT, del BOOL, logo TEXT, ts DATE_TIME, idPlugin INTEGER, pluginArgs TEXT, origin TEXT, title TEXT, description TEXT, tag TEXT, pluginActivate BOOL, tsPublished DATE_TIME, thumb TEXT, author TEXT, class LONG, level INTEGER, mediaLink TEXT)");

      #endregion

      #region Indexes
      DatabaseUtility.AddIndex(this._db, "idxMessages_id", "CREATE INDEX idxMessages_id ON messages (idMessage ASC)");
      #endregion
    }

    private void upgradeDatabase()
    {
      try
      {
        if (this._db == null)
          return;
      }
      catch (Exception ex)
      {
        Log.Error("[NotifyMessageServiceSqLite][upgradeDatabase] Exception:{0} stack:{1}", ex.Message, ex.StackTrace);
      }
    }

    private static string sanitySqlValue(string strValue)
    {
      if (string.IsNullOrWhiteSpace(strValue))
        return string.Empty;

      if (strValue.Contains('\''))
        return strValue.Replace("'", "''");
      else
        return strValue;
    }
  }
}

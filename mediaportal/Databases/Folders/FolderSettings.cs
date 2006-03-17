using System;
using System.Collections.Generic;
using System.Text;
using Databases.Folders;
using Databases.Folders.SqlServer;
namespace MediaPortal.Database
{
  public class FolderSettings
  {
    static IFolderSettings _database = DatabaseFactory.GetFolderDatabase();

    static public void Dispose()
    {
      _database.Dispose();
      _database = null;
    }
    static public void DeleteFolderSetting(string path, string Key)
    {
      _database.DeleteFolderSetting(path, Key);
    }
    static public void AddFolderSetting(string path, string Key, Type type, object Value)
    {
      _database.AddFolderSetting( path,  Key,  type,  Value);
    }
    static public void GetFolderSetting(string path, string Key, Type type, out object Value)
    {
      _database.GetFolderSetting(path, Key, type, out Value);
    }
  }
}

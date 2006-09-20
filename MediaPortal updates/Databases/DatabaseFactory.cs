using System;
using System.Collections.Generic;
using System.Text;
using Databases.Folders;
using Databases.Folders.SqlServer;
using MediaPortal.Picture.Database;
using MediaPortal.Radio.Database;
using MediaPortal.Video.Database;

namespace MediaPortal.Database
{
  public class DatabaseFactory
  {
    static bool UseADO = false;

    static public IFolderSettings GetFolderDatabase()
    {
      if (UseADO)
        return new FolderSettingAdo();
      else
        return new FolderSettingsSqlLite();
    }

    static public IPictureDatabase GetPictureDatabase()
    {
      if (UseADO)
        return new PictureDatabaseADO();
      else
        return new PictureDatabaseSqlLite();
    }

    static public IRadioDatabase GetRadioDatabase()
    {
      if (UseADO)
        return new RadioDatabaseADO();
      else
        return new RadioDatabaseSqlLite();
    }

    static public IVideoDatabase GetVideoDatabase()
    {
      if (UseADO)
        return null;
      else
        return new VideoDatabaseSqlLite();
    }
  }
}

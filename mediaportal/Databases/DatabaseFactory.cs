#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using Databases.Folders;
using Databases.Folders.SqlServer;
using MediaPortal.Picture.Database;
using MediaPortal.Video.Database;
using MediaPortal.Video.Database.SqlServer;
using MediaPortal.Profile;
using MediaPortal.GUI.Library;

namespace MediaPortal.Database
{
  public class DatabaseFactory
  {
    public static IFolderSettings GetFolderDatabase()
    {
      bool FolderDBUseADO = false;

      using (Profile.Settings xmlreader = new MPSettings())
      {
        FolderDBUseADO = xmlreader.GetValueAsBool("folderdatabase", "UseADO", false);
      }
      Log.Debug("DatabaseFactory FolderDBUseADO: {0}", FolderDBUseADO);

      if (FolderDBUseADO)
      {
        return new FolderSettingAdo();
      }
      else
      {
        return new FolderSettingsSqlLite();
      }
    }

    public static IPictureDatabase GetPictureDatabase()
    {
      bool PictureDBUseADO = false;

      using (Profile.Settings xmlreader = new MPSettings())
      {
        PictureDBUseADO = xmlreader.GetValueAsBool("picturedatabase", "UseADO", false);
      }
      Log.Debug("DatabaseFactory PictureDBUseADO: {0}", PictureDBUseADO);

      if (PictureDBUseADO)
      {
        return new PictureDatabaseADO();
      }
      else
      {
        return new PictureDatabaseSqlLite();
      }
    }

    public static IVideoDatabase GetVideoDatabase()
    {
      bool MovieDBUseADO = false;

      using (Profile.Settings xmlreader = new MPSettings())
      {
        MovieDBUseADO = xmlreader.GetValueAsBool("moviedatabase", "UseADO", false);
      }
      Log.Debug("DatabaseFactory MovieDBUseADO: {0}", MovieDBUseADO);

      if (MovieDBUseADO)
      {
        return new VideoDatabaseADO();
      }
      else
      {
        return new VideoDatabaseSqlLite();
      }
    }
  }
}
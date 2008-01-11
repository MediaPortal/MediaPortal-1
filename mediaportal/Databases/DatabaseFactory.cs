#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

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

    public static IFolderSettings GetFolderDatabase()
    {
      if (UseADO)
        return new FolderSettingAdo();
      else
        return new FolderSettingsSqlLite();
    }

    public static IPictureDatabase GetPictureDatabase()
    {
      if (UseADO)
        return new PictureDatabaseADO();
      else
        return new PictureDatabaseSqlLite();
    }

    public static IRadioDatabase GetRadioDatabase()
    {
      if (UseADO)
        return new RadioDatabaseADO();
      else
        return new RadioDatabaseSqlLite();
    }

    public static IVideoDatabase GetVideoDatabase()
    {
      if (UseADO)
        return null;
      else
        return new VideoDatabaseSqlLite();
    }
  }
}

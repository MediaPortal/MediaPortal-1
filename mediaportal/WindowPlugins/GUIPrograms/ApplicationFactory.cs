/* 
 *	Copyright (C) 2005 Team MediaPortal
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

using Programs.Utils;
using SQLite.NET;
using WindowPlugins.GUIPrograms;

namespace ProgramsDatabase
{
  /// <summary>
  /// Factory object that creates the matchin AppItem descendant class
  /// depending on the sourceType parameter
  /// Descendant classes differ in LOADING and REFRESHING filelists
  /// </summary>
  public class ApplicationFactory
  {
    static public ApplicationFactory AppFactory = new ApplicationFactory();

    // singleton. Dont allow any instance of this class
    private ApplicationFactory(){}

    static ApplicationFactory()
    {
      // nothing to create......
    }

    public AppItem GetAppItem(SQLiteClient sqlDB, myProgSourceType sourceType)
    {
      AppItem res = null;
      switch (sourceType)
      {
        case myProgSourceType.DIRBROWSE:
          res = new appItemDirBrowse(sqlDB);
          break;
        case myProgSourceType.DIRCACHE:
          res = new appItemDirCache(sqlDB);
          break;
        case myProgSourceType.MYFILEINI:
          res = new appItemMyFileINI(sqlDB);
          break;
        case myProgSourceType.MYFILEMEEDIO:
          res = new appItemMyFileMLF(sqlDB);
          break;
        case myProgSourceType.MAMEDIRECT:
          res = new appItemMameDirect(sqlDB);
          break;
        case myProgSourceType.FILELAUNCHER:
          res = new appFilesEdit(sqlDB);
          break;
        case myProgSourceType.GROUPER:
          res = new appGrouper(sqlDB);
          break;
        case myProgSourceType.GAMEBASE:
          res = new AppItemGamebase(sqlDB);
          break;
      }
      return res;
    }

  }
}

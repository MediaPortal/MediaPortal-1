/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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

using MediaPortal.GUI.Library;
using SQLite.NET;
using MediaPortal.Utils.Services;

namespace ProgramsDatabase
{
  /// <summary>
  /// Summary description for appFilesEdit.
  /// </summary>
  public class appGrouper: AppItem
  {

    public appGrouper(SQLiteClient initSqlDB): base(initSqlDB){}

    public override bool SubItemsAllowed()
    {
      return true;
    }

    public override bool FileEditorAllowed()
    {
      return true; // files are in fact FILTERITEMS => links to files
    }

    override public bool FileAddAllowed()
    {
      return false; // no file adding allowed! links can only be added indirectly from another filelist
    }

    override public bool FilesCanBeFavourites()
    {
      return false; // links cannot be links again..... :)
    }

    public override void LaunchFile(FileItem curFile, bool MPGUIMode)
    {
      if (curFile is FilelinkItem)
      {
        base.LaunchFilelink((FilelinkItem)curFile, MPGUIMode);
      }
      else
      {
        ServiceProvider services = GlobalServiceProvider.Instance;
        ILog log = services.Get<ILog>();
        log.Info("myPrograms: appGrouper: internal error: Filelinkitem expected in LaunchFile");
      }
    }


  }

}

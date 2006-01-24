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
using System;
using MediaPortal.GUI.Library;
namespace MediaPortal.Util
{
	/// <summary>
	/// 
	/// </summary>
	public class DaemonTools
	{
    static string _Path;
    static string _Drive;
    static bool   _Enabled;
    static int    _DriveNo;
    static string _MountedIsoFile=String.Empty;

    static DaemonTools()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        _Enabled= xmlreader.GetValueAsBool("daemon", "enabled", false);
        _Path= xmlreader.GetValueAsString("daemon", "path", "");
        _Drive=xmlreader.GetValueAsString("daemon", "drive", "E:");
        _DriveNo=xmlreader.GetValueAsInt("daemon", "driveNo", 0);
      }
    }

    static public bool IsEnabled
    {
      get { return _Enabled;}
    }

    static public bool IsMounted(string IsoFile)
    {
			if (IsoFile==null) return false;
			if (IsoFile==String.Empty) return false;
      IsoFile=Utils.RemoveTrailingSlash(IsoFile);
      if (_MountedIsoFile.Equals(IsoFile)) return true;
      return false;
    }

    static public bool Mount(string IsoFile, out string VirtualDrive)
		{
			VirtualDrive=String.Empty;
			if (IsoFile==null) return false;
			if (IsoFile==String.Empty) return false;
      if (!_Enabled) return false;
      if (!System.IO.File.Exists(_Path)) return false;

      UnMount();
  
      IsoFile=Utils.RemoveTrailingSlash(IsoFile);
      string strParams=String.Format("-mount {0},\"{1}\"",_DriveNo,IsoFile);
      Utils.StartProcess(_Path, strParams, true , true);
      VirtualDrive=_Drive;
      _MountedIsoFile=IsoFile;
      return true;
    }

    static public void UnMount()
    {
      if (!_Enabled) return ;
      if (!System.IO.File.Exists(_Path)) return ;
      
      string strParams=String.Format("-unmount {0}",_DriveNo);
      Utils.StartProcess(_Path, strParams, true , true);
      _MountedIsoFile=String.Empty;
    }

    static public string GetVirtualDrive()
    {
      if (_MountedIsoFile!=String.Empty) return _Drive;
      return String.Empty;
    }
	}
}

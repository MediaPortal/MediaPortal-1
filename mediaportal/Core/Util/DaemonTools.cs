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
      using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
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

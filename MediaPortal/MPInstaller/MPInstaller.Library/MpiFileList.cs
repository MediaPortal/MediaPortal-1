using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.Configuration;

namespace MediaPortal.MPInstaller
{
  /// <summary>
  /// Const for enumerat files used by the mpi system
  /// </summary>
  public static class MpiFileList
  {
    public static string LOCAL_LISTING = Config.GetFolder(Config.Dir.Installer) + @"\config.xml";
    public static string ONLINE_LISTING = Config.GetFolder(Config.Dir.Installer) +  @"\online.xml";
    public static string QUEUE_LISTING = Config.GetFolder(Config.Dir.Installer) + @"\queue.xml";
  }
}

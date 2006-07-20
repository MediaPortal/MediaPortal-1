using System;
using System.Collections.Generic;
using System.Text;
using System.Security.AccessControl;

namespace TvLibrary.Helper
{
  public class FileAccessHelper
  {
    public static void GrantFullControll(string fileName)
    {
      if (!System.IO.File.Exists(fileName)) return;
      try
      {
        FileSecurity security = System.IO.File.GetAccessControl(fileName);
        FileSystemAccessRule newRule = new FileSystemAccessRule("EveryOne", FileSystemRights.FullControl, AccessControlType.Allow);
        security.AddAccessRule(newRule);
        System.IO.File.SetAccessControl(fileName, security);
      }
      catch (Exception)
      {
      }
    }
  }
}

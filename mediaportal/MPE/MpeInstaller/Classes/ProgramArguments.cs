using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MpeInstaller.Classes
{
  public class ProgramArguments
  {
    public ProgramArguments(string[] args)
    {
      Silent = false;
      Update = false;
      foreach (string s in args)
      {
        if (File.Exists(s))
          PackageFile = s;
        if (s.StartsWith("/S"))
          Silent = true;
        if (s.StartsWith("/U"))
          Update = true;
        if (s.StartsWith("/MPQUEUE"))
          MpQueue = true;
        if (s.StartsWith("/BK="))
        {
          BackGround = s.Substring(4).Replace("\"", "");
          if (File.Exists(BackGround))
            Splash = true;
        }
      }
    }

    public bool MpQueue { get; set; }
    public string PackageFile { get; set; }
    public bool Silent { get; set; }
    public bool Update { get; set; }
    public string BackGround { get; set; }
    public bool Splash { get; set; }
  }
}
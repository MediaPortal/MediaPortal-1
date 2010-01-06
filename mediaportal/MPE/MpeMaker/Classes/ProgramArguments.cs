using System;
using System.Collections.Generic;
using System.Text;
using MpeCore.Classes;

namespace MpeMaker.Classes
{
  public class ProgramArguments
  {
    public ProgramArguments(string[] args)
    {
      Build = false;
      SetVersion = false;
      if (args.Length > 0)
      {
        ProjectFile = args[0];
        foreach (string s in args)
        {
          if (s.StartsWith("/B"))
            Build = true;

          if (s.StartsWith("/V="))
          {
            string ver = s.Remove(0, 3); // remove /?= from the argument          
            Version = VersionInfo.Pharse(ver);
            SetVersion = true;
          }
        }
      }
    }

    public string ProjectFile { get; set; }
    public VersionInfo Version { get; set; }
    public bool Build { get; set; }
    public bool SetVersion { get; set; }
  }
}
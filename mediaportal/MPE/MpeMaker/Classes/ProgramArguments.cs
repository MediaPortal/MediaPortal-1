#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
      UpdateXML = false;

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

          if (s.StartsWith("/UpdateXML"))
            UpdateXML = true;
        }
      }
    }

    public string ProjectFile { get; set; }
    public VersionInfo Version { get; set; }
    public bool Build { get; set; }
    public bool SetVersion { get; set; }
    public bool UpdateXML { get; set; }
  }
}
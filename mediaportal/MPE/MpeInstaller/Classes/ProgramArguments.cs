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
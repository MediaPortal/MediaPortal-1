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
using MpeCore.Interfaces;

namespace MpeCore.Classes.PathProvider
{
  public class WindowsPaths : IPathProvider
  {
    private Dictionary<string, string> _paths;

    public WindowsPaths()
    {
      _paths = new Dictionary<string, string>();
      //_paths.Add("%ProgramFiles", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));

      foreach (string options in Enum.GetNames(typeof (Environment.SpecialFolder)))
      {
        _paths.Add(string.Format("%{0}%", options),
                   Environment.GetFolderPath(
                     (Environment.SpecialFolder)Enum.Parse(typeof (Environment.SpecialFolder), options)));
      }
      string fontsDir = Environment.GetEnvironmentVariable("windir");

      if (fontsDir == null)
        fontsDir = "c:\\windows\\fonts";
      else
        fontsDir = Path.Combine(fontsDir, "fonts");
      _paths.Add("%Fonts%", fontsDir);
    }

    #region IPathProvider Members

    public string Name
    {
      get { return "WindowsPaths"; }
    }

    public Dictionary<string, string> Paths
    {
      get { return _paths; }
    }

    public string Colapse(string fileName)
    {
      foreach (KeyValuePair<string, string> path in Paths)
      {
        if (!string.IsNullOrEmpty(path.Key) && !string.IsNullOrEmpty(path.Value))
        {
          fileName = fileName.Replace(path.Key, path.Value);
        }
      }
      return fileName;
    }

    public string Expand(string filenameTemplate)
    {
      foreach (KeyValuePair<string, string> path in Paths)
      {
        if (!string.IsNullOrEmpty(path.Key) && !string.IsNullOrEmpty(path.Value))
        {
          filenameTemplate = filenameTemplate.Replace(path.Key, path.Value);
        }
      }
      return filenameTemplate;
    }

    #endregion
  }
}
#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Xml.Serialization;

namespace MpeCore.Classes
{
  public class ApplicationSettings
  {
    protected static ApplicationSettings _instance = null;
    public static ApplicationSettings Instance
    {
      get 
      {
        if (_instance == null)
#pragma warning disable 612,618
          return _instance = Load();
#pragma warning restore 612,618
        return _instance;
      }
    }

    [Obsolete("Use the static member Instance instead - class is now a singleton.")]
    public ApplicationSettings()
    {
      LastUpdate = DateTime.MinValue;
      UpdateDays = 7;
      UpdateAll = false;
      DoUpdateInStartUp = true;
      ShowOnlyStable = true;
      ShowOnlyCompatible = true;
      IgnoredUpdates = new List<string>();
    }

    public DateTime LastUpdate { get; set; }
    public int UpdateDays { get; set; }
    public bool UpdateAll { get; set; }
    public bool DoUpdateInStartUp { get; set; }
    public bool ShowOnlyStable { get; set; }
    public bool ShowOnlyCompatible { get; set; }
    public List<string> IgnoredUpdates { get; set; }

    public void Save()
    {
      if (!Directory.Exists(MpeInstaller.BaseFolder))
        Directory.CreateDirectory(MpeInstaller.BaseFolder);
      string filename = string.Format("{0}\\InstallerSettings.xml", MpeInstaller.BaseFolder);
      var serializer = new XmlSerializer(typeof (ApplicationSettings));
      using (TextWriter writer = new StreamWriter(filename))
      {
        serializer.Serialize(writer, this);
        writer.Close();
      }
    }

    [Obsolete("Use the static member Instance instead - class is now a singleton.")]
    public static ApplicationSettings Load()
    {
      string filename = string.Format("{0}\\InstallerSettings.xml", MpeInstaller.BaseFolder);
      if (File.Exists(filename))
      {
        try
        {
          using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
          {
            var serializer = new XmlSerializer(typeof(ApplicationSettings));
            return (ApplicationSettings)serializer.Deserialize(fs);
          }
        }
        catch { }
      }
      return new ApplicationSettings();
    }
  }
}
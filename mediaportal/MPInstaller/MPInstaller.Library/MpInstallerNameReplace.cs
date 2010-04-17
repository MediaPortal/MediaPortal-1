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
using System.Windows.Forms;
using System.Xml;

namespace MediaPortal.MPInstaller
{
  public class MpInstallerNameReplace
  {
    private Dictionary<string, string> _groupSubstitution;

    public Dictionary<string, string> GroupSubstitutions
    {
      get { return _groupSubstitution; }
      set { _groupSubstitution = value; }
    }

    private Dictionary<string, string> _nameCleanups;

    public Dictionary<string, string> NameCleanups
    {
      get { return _nameCleanups; }
      set { _nameCleanups = value; }
    }

    public MpInstallerNameReplace()
    {
      GroupSubstitutions = new Dictionary<string, string>();
      NameCleanups = new Dictionary<string, string>();
    }

    public void Load(string filename)
    {
      XmlDocument doc = new XmlDocument();
      if (File.Exists(filename))
      {
        try
        {
          doc.Load(filename);
          XmlNode ver = doc.DocumentElement.SelectSingleNode("/MPinstallerCleenup");
          XmlNodeList NameCleanupsList = ver.SelectNodes("GroupSubstitution/Substitution");
          foreach (XmlNode nodeSubstitution in NameCleanupsList)
          {
            GroupSubstitutions.Add(nodeSubstitution.Attributes["key"].Value, nodeSubstitution.Attributes["value"].Value);
          }
          NameCleanupsList = ver.SelectNodes("NameCleanups/Substitution");
          foreach (XmlNode nodeNameCleanups in NameCleanupsList)
          {
            NameCleanups.Add(nodeNameCleanups.Attributes["key"].Value, nodeNameCleanups.Attributes["value"].Value);
          }
        }
        catch (Exception ex)
        {
          MessageBox.Show(ex.Message);
        }
      }
    }
  }
}
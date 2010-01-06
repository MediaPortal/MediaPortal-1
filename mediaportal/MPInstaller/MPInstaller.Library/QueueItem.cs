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
using System.Xml;
using System.Xml.Serialization;
using System.Text;

namespace MediaPortal.MPInstaller
{
  public enum QueueAction
  {
    Install,
    Uninstall,
    Unknow,
  }

  public class QueueItem
  {
    private string name;

    [System.Xml.Serialization.XmlAttribute]
    public string Name
    {
      get { return name; }
      set { name = value; }
    }


    private string version;

    [System.Xml.Serialization.XmlAttribute]
    public string Version
    {
      get { return version; }
      set { version = value; }
    }

    private QueueAction action;

    [System.Xml.Serialization.XmlAttribute]
    public QueueAction Action
    {
      get { return action; }
      set { action = value; }
    }

    private string localFile;

    [System.Xml.Serialization.XmlAttribute]
    public string LocalFile
    {
      get { return localFile; }
      set { localFile = value; }
    }

    private string downloadUrl;

    [System.Xml.Serialization.XmlAttribute]
    public string DownloadUrl
    {
      get { return downloadUrl; }
      set { downloadUrl = value; }
    }

    private List<GroupString> setupgroups;

    public List<GroupString> SetupGroups
    {
      get { return setupgroups; }
      set { setupgroups = value; }
    }


    public QueueItem()
    {
      this.Action = QueueAction.Unknow;
      this.LocalFile = string.Empty;
      this.Name = string.Empty;
      this.Version = string.Empty;
      this.DownloadUrl = string.Empty;
      SetupGroups = new List<GroupString>();
    }
  }
}
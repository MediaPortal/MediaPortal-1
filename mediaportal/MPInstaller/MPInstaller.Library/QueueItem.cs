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
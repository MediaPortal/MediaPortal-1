#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System.Collections;
using System.IO;
using TvLibrary.Log;
using MediaPortal.WebEPG.Profile;

namespace MediaPortal.EPG.config
{
  /// <summary>
  /// Config has methods to save and load the config file.
  /// </summary>
  public class EPGConfig
  {
    #region Variables

    private ArrayList _ConfigList;
    private int _MaxGrab;
    private string _strPath = "";

    #endregion

    #region Constructors/Destructors

    public EPGConfig(string path)
    {
      _strPath = path;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Property to get/set the maximum grab days
    /// </summary>
    public int MaxGrab
    {
      get { return _MaxGrab; }
      set { _MaxGrab = value; }
    }

    public ArrayList GetAll()
    {
      return _ConfigList;
    }

    public void SetAll(ArrayList configList)
    {
      _ConfigList = new ArrayList();

      for (int i = 0; i < configList.Count; i++)
      {
        EPGConfigData channel = (EPGConfigData)configList[i];
        if (channel.ChannelID != null && channel.DisplayName != null && channel.PrimaryGrabberID != null)
        {
          _ConfigList.Add(channel);
        }
      }
    }

    public EPGConfigData GetAt(int index)
    {
      if (index < _ConfigList.Count)
      {
        return (EPGConfigData)_ConfigList[index];
      }
      return null;
    }

    public int Add(EPGConfigData channel)
    {
      if (channel.ChannelID != null && channel.DisplayName != null && channel.PrimaryGrabberID != null)
      {
        if (_ConfigList == null)
        {
          _ConfigList = new ArrayList();
        }
        return _ConfigList.Add(channel);
      }
      return -1;
    }

    public int UpdateAt(int index, EPGConfigData channel)
    {
      if (index < _ConfigList.Count && channel.ChannelID != null && channel.DisplayName != null &&
          channel.PrimaryGrabberID != null)
      {
        _ConfigList.RemoveAt(index);
        return _ConfigList.Add(channel);
      }
      return -1;
    }

    public void RemoveAt(int index)
    {
      if (index < _ConfigList.Count)
      {
        _ConfigList.RemoveAt(index);
      }
    }

    public int IndexOf(string name)
    {
      for (int i = 0; i < _ConfigList.Count; i++)
      {
        EPGConfigData channel = (EPGConfigData)_ConfigList[i];
        if (channel.DisplayName == name)
        {
          return i;
        }
      }
      return -1;
    }

    public void Load()
    {
      _ConfigList = new ArrayList();

      string configFile = _strPath + "\\WebEPG.xml";
      if (!File.Exists(configFile))
      {
        return;
      }

      Log.Info("WebEPG Config: Loading Existing WebEPG.xml");
      Xml xmlreader = new Xml(configFile);
      _MaxGrab = xmlreader.GetValueAsInt("General", "MaxDays", 1);
      int channelCount = xmlreader.GetValueAsInt("ChannelMap", "Count", 0);
      for (int i = 1; i <= channelCount; i++)
      {
        EPGConfigData channel = new EPGConfigData();
        channel.ChannelID = xmlreader.GetValueAsString(i.ToString(), "ChannelID", "");
        channel.DisplayName = xmlreader.GetValueAsString(i.ToString(), "DisplayName", "");

        string GrabberID = xmlreader.GetValueAsString(i.ToString(), "Grabber1", "");
        if (GrabberID != "")
        {
          int start = GrabberID.IndexOf("\\") + 1;
          int end = GrabberID.LastIndexOf(".");

          string GrabberSite = GrabberID.Substring(start, end - start);
          GrabberSite = GrabberSite.Replace("_", ".");
          channel.PrimaryGrabberName = GrabberSite;
          channel.PrimaryGrabberID = GrabberID;
          channel.Linked = xmlreader.GetValueAsBool(i.ToString(), "Grabber1-Linked", false);
          channel.linkStart = xmlreader.GetValueAsInt(i.ToString(), "Grabber1-Start", 18);
          channel.linkEnd = xmlreader.GetValueAsInt(i.ToString(), "Grabber1-End", 23);

          _ConfigList.Add(channel);
        }
      }
    }

    public void Save()
    {
      if (_ConfigList != null)
      {
        string confFile = _strPath + "\\WebEPG.xml";
        if (File.Exists(confFile))
        {
          File.Delete(confFile.Replace(".xml", ".bak"));
          File.Move(confFile, confFile.Replace(".xml", ".bak"));
        }
        Xml xmlwriter = new Xml(confFile);

        xmlwriter.SetValue("General", "MaxDays", _MaxGrab.ToString());
        xmlwriter.SetValue("ChannelMap", "Count", _ConfigList.Count.ToString());

        for (int i = 0; i < _ConfigList.Count; i++)
        {
          EPGConfigData channel = (EPGConfigData)_ConfigList[i];
          xmlwriter.SetValue((i + 1).ToString(), "ChannelID", channel.ChannelID);
          xmlwriter.SetValue((i + 1).ToString(), "DisplayName", channel.DisplayName);
          xmlwriter.SetValue((i + 1).ToString(), "Grabber1", channel.PrimaryGrabberID);
          if (channel.Linked)
          {
            xmlwriter.SetValueAsBool((i + 1).ToString(), "Grabber1-Linked", channel.Linked);
            xmlwriter.SetValue((i + 1).ToString(), "Grabber1-Start", channel.linkStart);
            xmlwriter.SetValue((i + 1).ToString(), "Grabber1-End", channel.linkEnd);
          }
        }
        xmlwriter.Save();
      }
    }

    #endregion
  }
}
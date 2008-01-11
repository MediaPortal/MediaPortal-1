#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.Visualization
{
  public class VisualizationInfo
  {
    public enum PluginType
    {
      Unknown = -1,
      None = 0,
      GForce,
      WhiteCap,
      SoftSkies,
      Winamp,
      Sonique,
      WMP,
    };

    private string[] BlackList = new string[]
            {
                "{BFA29983-66E4-11d7-A75D-0000B4908923}",   // The WMP version the the G-Force plugin.  Since we support G-Force natively, we don't need this one
                "{3DC95765-154D-11d8-A75D-0000B4908923}",   // The WMP version the the WhiteCap plugin.  Since we support WhiteCap natively, we don't need this one
                "{4EC05565-154D-11d8-A75D-0000B4908923}",   // The WMP version the the SoftSkies plugin.  Since we support SoftSkies natively, we don't need this one 
                "{BDEEAAAB-15DC-4e7d-802D-10115C069AD8}",   // The WMP version of TwistedPixel. We support the Winamp Version
                "Milkdrop 1.04d",                           // Winamp Milkdrop 1.04d
            };

    private VisualizationInfo.PluginType _VisualizationType = VisualizationInfo.PluginType.None;
    private string _FilePath = string.Empty;
    private string _Name = string.Empty;
    private string _CLSID = string.Empty;
    private List<string> _PresetNames = new List<string>();
    private int _PresetIndex = 0;
    private bool _IsCOMPlugin = false;
    private bool _IsDummyPlugin = false;
    private bool _IsBlackListed = false;

    public PluginType VisualizationType
    {
      get { return _VisualizationType; }
      set { _VisualizationType = value; }
    }

    public string FilePath
    {
      get { return _FilePath; }
      set { _FilePath = value; }
    }

    public string Name
    {
      get { return _Name; }
      set { _Name = value; }
    }

    public string CLSID
    {
      get { return _CLSID; }
      set { _CLSID = value; }
    }

    public List<string> PresetNames
    {
      get
      {
        if (_PresetNames == null)
          _PresetNames = new List<string>();

        return _PresetNames;
      }

      set { _PresetNames = value; }
    }

    public int PresetIndex
    {
      get { return _PresetIndex; }
      set { _PresetIndex = value; }
    }

    public bool HasPresets
    {
      get { return _PresetNames != null && _PresetNames.Count > 0; }
    }

    public int PresetCount
    {
      get
      {
        if (_PresetNames != null)
          return _PresetNames.Count;

        else
          return 0;
      }
    }

    public bool IsCOMPlugin
    {
      get { return _IsCOMPlugin; }
      set { _IsCOMPlugin = value; }
    }

    public bool IsDummyPlugin
    {
      get { return _IsDummyPlugin; }
      set { _IsDummyPlugin = value; }
    }

    public bool IsBlackListed
    {
      get { return _IsBlackListed; }
      set { _IsBlackListed = value; }
    }

    public VisualizationInfo(VisualizationInfo.PluginType vizType, string path, string name, string clsid, List<string> presets)
    {
      _IsBlackListed = IsBlackListedVisualization(clsid) || IsBlackListedVisualization(name);
      _VisualizationType = vizType;
      _FilePath = path;
      _Name = name;
      _CLSID = clsid;
      _PresetNames = presets;
    }

    public VisualizationInfo(VisualizationInfo.PluginType vizType, string path, string name, string clsid, int presetIndex)
    {
      _IsBlackListed = IsBlackListedVisualization(clsid) || IsBlackListedVisualization(name);
      _VisualizationType = vizType;
      _FilePath = path;
      _Name = name;
      _CLSID = clsid;
      _PresetIndex = presetIndex;
    }

    public VisualizationInfo(string name, bool isDummy)
    {
      _VisualizationType = VisualizationInfo.PluginType.None;
      _Name = name;
      _IsDummyPlugin = isDummy;
    }

    private bool IsBlackListedVisualization(string clsid)
    {
      if (clsid.Length == 0)
        return false;

      string pluginClisd = clsid.ToLower();

      for (int i = 0; i < BlackList.Length; i++)
      {
        string curClsid = BlackList[i].ToLower();

        if (pluginClisd == curClsid)
          return true;
      }

      return false;
    }

    /// <summary>
    /// Determines whether the member values of the specified VisualizationInfo object
    /// are the same as the current VisualizationInfo object
    /// <param name="vizInfo"></param>
    /// <returns>true if data is identical</returns>
    /// </summary>
    public bool IsIdenticalTo(VisualizationInfo vizInfo, bool doSimpleCheck)
    {
      if (this._Name != vizInfo._Name)
        return false;

      if (this._CLSID != vizInfo.CLSID)
        return false;

      if (this.FilePath != vizInfo.FilePath)
        return false;

      if (this.IsCOMPlugin != vizInfo.IsCOMPlugin)
        return false;

      if (this.IsDummyPlugin != vizInfo.IsDummyPlugin)
        return false;

      if (this.PresetIndex != vizInfo.PresetIndex)
        return false;

      if (this.VisualizationType != vizInfo.VisualizationType)
        return false;

      if (!doSimpleCheck)
      {
        if (this.HasPresets != vizInfo.HasPresets)
          return false;


        if (this.PresetNames.Count != vizInfo.PresetNames.Count)
          return false;

        for (int i = 0; i < this.PresetNames.Count; i++)
        {
          if (this.PresetNames[i] != vizInfo.PresetNames[i])
            return false;
        }
      }

      return true;
    }

    public override string ToString()
    {
      string sVizType = string.Empty; ;

      switch (_VisualizationType)
      {
        case VisualizationInfo.PluginType.GForce:
        case VisualizationInfo.PluginType.WhiteCap:
        case VisualizationInfo.PluginType.SoftSkies:
          sVizType = "";
          break;

        case VisualizationInfo.PluginType.Sonique:
          sVizType = " (Sonique)";
          break;

        case VisualizationInfo.PluginType.Winamp:
          sVizType = " (Winamp)";
          break;

        case VisualizationInfo.PluginType.WMP:
          sVizType = " (Windows Media Player)";
          break;
      }

      return string.Format("{0}{1}", _Name, sVizType);
    }
  }
}

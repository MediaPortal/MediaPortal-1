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

using System.Collections.Generic;

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
      Bassbox,
      WMP,
    } ;

    private string[] BlackList = new string[]
                                   {                                     
                                     "{6870D1D8-5018-454f-8DBE-4EE920743B8B}",
                                     // Windows Media 9 Series (Rhythm and Wave) not work without IWMPEffects2 interface, we don't need this one
                                     "{B0D32936-2E7A-4a69-8AB8-40FB4E83A0D0}",
                                     // Windows Media Player 10 (Energy Bliss) not work without IWMPEffects2 interface, we don't need this one
                                     "{BFA29983-66E4-11d7-A75D-0000B4908923}",
                                     // The WMP version the the G-Force plugin.  Since we support G-Force natively, we don't need this one
                                     "{3DC95765-154D-11d8-A75D-0000B4908923}",
                                     // The WMP version the the WhiteCap plugin.  Since we support WhiteCap natively, we don't need this one
                                     "{4EC05565-154D-11d8-A75D-0000B4908923}",
                                     // The WMP version the the SoftSkies plugin.  Since we support SoftSkies natively, we don't need this one 
                                     "{BDEEAAAB-15DC-4e7d-802D-10115C069AD8}",
                                     // The WMP version of TwistedPixel. We support the Winamp Version
                                     "Milkdrop 1.04d", // Winamp Milkdrop 1.04d
                                   };

    private PluginType _VisualizationType = PluginType.None;
    private string _FilePath = string.Empty;
    private string _Name = string.Empty;
    private string _CLSID = string.Empty;
    private List<string> _PresetNames = new List<string>();
    private int _PresetIndex = 0;
    private bool _IsCOMPlugin = false;
    private bool _IsDummyPlugin = false;
    private bool _IsBlackListed = false;
    private bool _useOpenGL = false;
    private bool _useCover = false;
    private int _fftSensitivity = 0;
    private int _renderTiming = 0;
    private int _viewPortSize = 0;

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
        {
          _PresetNames = new List<string>();
        }

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
        {
          return _PresetNames.Count;
        }

        else
        {
          return 0;
        }
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

    public bool UseOpenGL
    {
      get { return _useOpenGL; }
      set { _useOpenGL = value; }
    }

    public bool UseCover
    {
      get { return _useCover; }
      set { _useCover = value; }
    }

    public int FFTSensitivity
    {
      get { return _fftSensitivity; }
      set { _fftSensitivity = value; }
    }

    public int RenderTiming
    {
      get { return _renderTiming; }
      set { _renderTiming = value; }
    }

    public int ViewPortSize
    {
      get { return _viewPortSize; }
      set { _viewPortSize = value; }
    }

    public int ViewPortSizeX
    {
      get
      {
        switch (_viewPortSize)
        {
          case 0:
            return 512;

          case 1:
            return 640;

          case 2:
            return 800;

          case 3:
            return 1024;
        }

        return 512;
      }
    }

    public int ViewPortSizeY
    {
      get
      {
        switch (_viewPortSize)
        {
          case 0:
            return 384;

          case 1:
            return 480;

          case 2:
            return 600;

          case 3:
            return 786;
        }

        return 384;
      }
    }

    public VisualizationInfo(PluginType vizType, string path, string name, string clsid, List<string> presets)
    {
      _IsBlackListed = IsBlackListedVisualization(clsid) || IsBlackListedVisualization(name);
      _VisualizationType = vizType;
      _FilePath = path;
      _Name = name;
      _CLSID = clsid;
      _PresetNames = presets;
    }

    public VisualizationInfo(PluginType vizType, string path, string name, string clsid, int presetIndex)
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
      _VisualizationType = PluginType.None;
      _Name = name;
      _IsDummyPlugin = isDummy;
    }

    private bool IsBlackListedVisualization(string clsid)
    {
      if (clsid.Length == 0)
      {
        return false;
      }

      string pluginClisd = clsid.ToLowerInvariant();

      for (int i = 0; i < BlackList.Length; i++)
      {
        string curClsid = BlackList[i].ToLowerInvariant();

        if (pluginClisd == curClsid)
        {
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Determines whether the member values of the specified VisualizationInfo object
    /// are the same as the current VisualizationInfo object
    /// <param name="vizInfo"></param>
    /// <returns>true if data is identical</returns>
    /// </summary>
    public bool IsIdenticalTo(VisualizationInfo vizInfo)
    {
      if (this._Name != vizInfo._Name)
      {
        return false;
      }

      if (this._CLSID != vizInfo.CLSID)
      {
        return false;
      }

      if (this.FilePath != vizInfo.FilePath)
      {
        return false;
      }

      if (this.IsCOMPlugin != vizInfo.IsCOMPlugin)
      {
        return false;
      }

      if (this.IsDummyPlugin != vizInfo.IsDummyPlugin)
      {
        return false;
      }

      if (this.VisualizationType != vizInfo.VisualizationType)
      {
        return false;
      }

      return true;
    }

    public override string ToString()
    {
      string sVizType = string.Empty;
      ;

      switch (_VisualizationType)
      {
        case PluginType.GForce:
        case PluginType.WhiteCap:
        case PluginType.SoftSkies:
          sVizType = "";
          break;

        case PluginType.Sonique:
          sVizType = " (Sonique)";
          break;

        case PluginType.Winamp:
          sVizType = " (Winamp)";
          break;

        case PluginType.Bassbox:
          sVizType = " (Bassbox)";
          break;

        case PluginType.WMP:
          sVizType = " (Windows Media Player)";
          break;
      }

      return string.Format("{0}{1}", _Name, sVizType);
    }
  }
}
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
      Winamp,
      Sonique,
      Bassbox,
      WMP,
    } ;

    private PluginType _VisualizationType = PluginType.None;
    private string _FilePath = string.Empty;
    private string _Name = string.Empty;
    private List<string> _PresetNames = new List<string>();
    private int _PresetIndex = 0;
    private int _PlgIndex = -1;
    private bool _IsCOMPlugin = false;
    private bool _IsDummyPlugin = false;
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

    public int PlgIndex
    {
      get { return _PlgIndex; }
      set { _PlgIndex = value; }
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

    public VisualizationInfo(PluginType vizType, string path, string name, List<string> presets)
    {
      _VisualizationType = vizType;
      _FilePath = path;
      _Name = name;
      _PresetNames = presets;
    }

    public VisualizationInfo(PluginType vizType, string path, string name, int presetIndex, int plgIndex)
    {
      _VisualizationType = vizType;
      _FilePath = path;
      _Name = name;
      _PresetIndex = presetIndex;
      _PlgIndex = plgIndex;
    }

    public VisualizationInfo(string name, bool isDummy)
    {
      _VisualizationType = PluginType.None;
      _Name = name;
      _IsDummyPlugin = isDummy;
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

      if (this.FilePath != vizInfo.FilePath)
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
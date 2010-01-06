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

namespace MediaPortal.Player.DSP
{
  public class DSPPluginInfo
  {
    #region Enums

    public enum PluginType
    {
      Unknown = -1,
      VST = 0,
      Winamp,
    } ;

    #endregion

    #region Variables

    private string[] BlackList = new string[]
                                   {
                                     "EQ 1.dll", // VST plugin. Known that it diesn't work
                                   };

    private PluginType _DSPPluginType = PluginType.Unknown;
    private string _FilePath = string.Empty;
    private string _Name = string.Empty;
    private bool _IsBlackListed = false;

    #endregion

    #region Constructors/Destructors

    public DSPPluginInfo(PluginType pluginType, string path, string name)
    {
      _IsBlackListed = IsBlackListedPlugin(name);
      _DSPPluginType = pluginType;
      _FilePath = path;
      _Name = name;
    }

    #endregion

    #region Properties

    public PluginType DSPPluginType
    {
      get { return _DSPPluginType; }
      set { _DSPPluginType = value; }
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

    public bool IsBlackListed
    {
      get { return _IsBlackListed; }
      set { _IsBlackListed = value; }
    }

    #endregion

    #region Methods

    private bool IsBlackListedPlugin(string name)
    {
      if (name.Length == 0)
      {
        return false;
      }

      string pluginName = name.ToLower();

      for (int i = 0; i < BlackList.Length; i++)
      {
        string curName = BlackList[i].ToLower();

        if (pluginName == curName)
        {
          return true;
        }
      }

      return false;
    }


    public override string ToString()
    {
      string sPluginType = string.Empty;
      ;

      switch (_DSPPluginType)
      {
        case PluginType.VST:
          sPluginType = " (VST)";
          break;

        case PluginType.Winamp:
          sPluginType = " (Winamp)";
          break;
      }

      return string.Format("{0}{1}", _Name, sPluginType);
    }

    #endregion
  }
}
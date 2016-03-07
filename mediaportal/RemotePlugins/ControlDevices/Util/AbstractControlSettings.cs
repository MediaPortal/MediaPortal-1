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

using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.Profile;

namespace MediaPortal.ControlDevices
{
  public abstract class AbstractControlSettings : IControlSettings
  {
    private static string CONFIG_SECTION = "controldevice";
    protected IControlPlugin _plugin;
    protected string _prefix;
    protected bool _enabled;
    protected bool _enableInput;
    protected bool _enableOutput;

    /// <summary>
    /// 
    /// </summary>
    private bool _verbose;

    public AbstractControlSettings(IControlPlugin plugin, string prefix)
    {
      _plugin = plugin;
      _prefix = prefix;
    }

    public bool Enabled
    {
      set { _enabled = value; }
      get { return _enabled; }
    }

    public bool EnableInput
    {
      set { _enableInput = value; }
      get { return _enableInput; }
    }

    public bool EnableOutput
    {
      set { _enableOutput = value; }
      get { return _enableOutput; }
    }

    public bool Verbose
    {
      set { _enabled = value; }
      get { return _enabled; }
    }

    public string Prefix
    {
      get { return _prefix; }
    }

    public void Load()
    {
      _enabled = GetValueAsBool("enable", _enabled);
      _enableInput = GetValueAsBool("enable_input", _enableInput);
      _enableOutput = GetValueAsBool("enable_output", _enableOutput);
      _verbose = GetValueAsBool("verbose", _verbose);
    }

    public void Save()
    {
      SetValueAsBool("enable", _enabled);
      SetValueAsBool("verbose", _verbose);
    }

    public abstract UserControl SettingsPanel { get; }

    public abstract void ShowAdvancedSettings();

    protected void SetValueAsBool(string name, bool val)
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValueAsBool(CONFIG_SECTION, _prefix + "_" + name, val);
      }
    }

    protected void SetValue(string name, object val)
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue(CONFIG_SECTION, _prefix + "_" + name, val);
      }
    }


    protected object GetValue(string name)
    {
      using (Settings xmlreader = new MPSettings())
      {
        return xmlreader.GetValue(CONFIG_SECTION, _prefix + "_" + name);
      }
    }

    protected bool GetValueAsBool(string name, bool def)
    {
      using (Settings xmlreader = new MPSettings())
      {
        return xmlreader.GetValueAsBool(CONFIG_SECTION, _prefix + "_" + name, def);
      }
    }
  }
}
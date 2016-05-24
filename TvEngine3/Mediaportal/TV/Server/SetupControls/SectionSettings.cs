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

namespace Mediaportal.TV.Server.SetupControls
{
  public delegate void ServerConfigurationChangedEventHandler(object sender, bool reloadConfigController, HashSet<int> reloadConfigTuners);
  public delegate void PluginEnabledOrDisabledEventHandler(object sender, object plugin, bool isEnabled);

  public partial class SectionSettings : System.Windows.Forms.UserControl
  {
    public const string MESSAGE_CAPTION = "MediaPortal TV Server";
    public const string SENTENCE_CHECK_LOG_FILES = "Please check the log files for more details.";
    private event ServerConfigurationChangedEventHandler _onServerConfigurationChanged = null;

    public SectionSettings()
    {
      // For design view. Do not delete.
    }

    public SectionSettings(string name)
    {
      Init(name);
    }

    public SectionSettings(string name, ServerConfigurationChangedEventHandler handler)
    {
      Init(name);
      if (handler != null)
      {
        _onServerConfigurationChanged += handler;
      }
    }

    private void Init(string text)
    {
      AutoScroll = true;
      Text = text;
    }

    protected virtual void OnServerConfigurationChanged(object sender, bool reloadConfigController, HashSet<int> reloadConfigTuners)
    {
      if (_onServerConfigurationChanged != null)
      {
        _onServerConfigurationChanged(sender, reloadConfigController, reloadConfigTuners);
      }
    }

    public virtual void SaveSettings() {}

    public virtual void LoadSettings() {}

    public virtual void OnSectionActivated() {}

    public virtual void OnSectionDeActivated() {}
  }
}
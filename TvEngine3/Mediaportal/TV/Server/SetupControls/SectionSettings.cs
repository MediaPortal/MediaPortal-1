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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace Mediaportal.TV.Server.SetupControls
{
  public delegate void ServerConfigurationChangedEventHandler(object sender, bool reloadConfigController, HashSet<int> reloadConfigTuners);

  public partial class SectionSettings : System.Windows.Forms.UserControl
  {
    private event ServerConfigurationChangedEventHandler _onServerConfigurationChanged;

    public SectionSettings()
    {
      Init();
      InitializeComponent();
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

    private void Init()
    {
      AutoScroll = true;
    }

    private void Init(string text)
    {
      Init();
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

    public virtual void LoadWizardSettings(XmlNode node) {}


    /// <summary>
    /// Returns the current setting for the given setting name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public virtual object GetSetting(string name)
    {
      return null;
    }

    public virtual void OnSectionActivated() {}

    public virtual void OnSectionDeActivated() {}

    public virtual bool CanActivate
    {
      get { return true; }
    }
  }
}
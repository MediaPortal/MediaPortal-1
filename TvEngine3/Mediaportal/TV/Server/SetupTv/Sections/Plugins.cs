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
using System.Windows.Forms;
using Mediaportal.TV.Server.Plugins.Base.Interfaces;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class Plugins : SectionSettings
  {
    private readonly PluginEnabledOrDisabledEventHandler _handler = null;
    private readonly PluginLoaderSetupTv _pluginLoader;
    private bool _ignoreEvents;

    public Plugins(PluginEnabledOrDisabledEventHandler handler, PluginLoaderSetupTv pluginLoader)
      : base("Plugins")
    {
      _handler = handler;
      _pluginLoader = pluginLoader;
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      this.LogDebug("plugins: activating");

      _ignoreEvents = true;
      listViewPlugins.BeginUpdate();
      try
      {
        listViewPlugins.AutoResizeColumns(ColumnHeaderAutoResizeStyle.None);
        listViewPlugins.Items.Clear();

        this.LogDebug("plugins: available plugins...");
        ListViewGroup listGroup = listViewPlugins.Groups["listViewGroupAvailable"];
        foreach (ITvServerPlugin plugin in _pluginLoader.Plugins)
        {
          bool isEnabled = ServiceAgents.Instance.SettingServiceAgent.GetValue(GetPluginEnabledSettingName(plugin), false);
          this.LogDebug("  name = {0, -40}, is enabled = {1, -5}, version = {2, -10}, author = {3}", plugin.Name, isEnabled, plugin.Version, plugin.Author);

          ListViewItem item = listViewPlugins.Items.Add(string.Empty);
          item.Group = listGroup;
          item.SubItems.Add(plugin.Name);
          item.SubItems.Add(plugin.Version);
          item.SubItems.Add(plugin.Author);
          item.Tag = plugin;
          item.Checked = ServiceAgents.Instance.SettingServiceAgent.GetValue(GetPluginEnabledSettingName(plugin), false);
        }

        this.LogDebug("plugins: incompatible plugins...");
        listGroup = listViewPlugins.Groups["listViewGroupIncompatible"];
        foreach (Type plugin in _pluginLoader.IncompatiblePlugins)
        {
          string version = plugin.Assembly.GetName().Version.ToString();
          this.LogDebug("  name = {0, -40}, version = {2, -10}", plugin.Name, version);

          ListViewItem item = listViewPlugins.Items.Add(string.Empty);
          item.Group = listGroup;
          item.SubItems.Add(plugin.Name);
          item.SubItems.Add(version);
          item.SubItems.Add("Unknown");
          item.Checked = false;
        }

        listViewPlugins.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
      }
      finally
      {
        listViewPlugins.EndUpdate();
        _ignoreEvents = false;
      }

      base.OnSectionActivated();
    }

    private void listViewPlugins_ItemChecked(object sender, ItemCheckedEventArgs e)
    {
      if (_ignoreEvents)
      {
        return;
      }
      ITvServerPlugin plugin = e.Item.Tag as ITvServerPlugin;
      if (plugin == null)
      {
        return;
      }

      if (e.Item.Checked)
      {
        this.LogInfo("plugins: enable {0}", plugin.Name);
      }
      else
      {
        this.LogInfo("plugins: disable {0}", plugin.Name);
      }
      ServiceAgents.Instance.SettingServiceAgent.SaveValue(GetPluginEnabledSettingName(plugin), e.Item.Checked);

      if (_handler != null)
      {
        _handler(this, plugin, e.Item.Checked);
      }
    }

    public static string GetPluginEnabledSettingName(ITvServerPlugin plugin)
    {
      return string.Format("pluginEnabled{0}", plugin.Name);
    }
  }
}
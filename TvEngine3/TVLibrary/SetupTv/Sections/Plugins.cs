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
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TvControl;
using DirectShowLib;


using Gentle.Common;
using Gentle.Framework;
using TvDatabase;
using TvLibrary;
using TvLibrary.Interfaces;
using TvLibrary.Implementations;
using TvEngine;

namespace SetupTv.Sections
{
  public partial class Plugins : SetupTv.SectionSettings
  {
    PluginLoader _loader;
    bool _needRestart = false;
    bool _ignoreEvents = false;
    public delegate void ChangedEventHandler(object sender, EventArgs e);

    public event ChangedEventHandler ChangedActivePlugins;



    public Plugins(string name, PluginLoader loader)
      : base(name)
    {
      _loader=loader;
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      _needRestart = false;
      _ignoreEvents = true;
      TvBusinessLayer layer = new TvBusinessLayer();
      base.OnSectionActivated();
      listView1.Items.Clear();
      foreach (ITvServerPlugin plugin in _loader.Plugins)
      {
        
        ListViewItem item = listView1.Items.Add("");
        item.SubItems.Add(plugin.Name);
        item.SubItems.Add(plugin.Author);
        item.SubItems.Add(plugin.Version);
        Setting setting = layer.GetSetting(String.Format("plugin{0}", plugin.Name), "false");
        item.Checked = setting.Value == "true";
        item.Tag = setting;

      }
			listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);				
      _ignoreEvents = false;
    }

    public override void OnSectionDeActivated()
    {
      if (_needRestart)
      {
       // RemoteControl.Instance.Restart();
      }
      base.OnSectionDeActivated();
    }

    private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
    {
      if (_ignoreEvents) return;
      Setting setting = e.Item.Tag as Setting;
      if (setting == null) return;
      if (e.Item.Checked) setting.Value = "true";
      else setting.Value = "false";
      setting.Persist();
      _needRestart = true;

      OnChanged(setting, EventArgs.Empty);
    }

    //Pass on the information for the plugin that was changed
    protected virtual void OnChanged(object sender, EventArgs e)
    {
        if (ChangedActivePlugins != null)
            ChangedActivePlugins(sender, e);
    }
  }
}

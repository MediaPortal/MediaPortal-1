/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Globalization;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using TvControl;
using TvDatabase;

namespace SetupTv.Sections
{
  public partial class XmlSetup : SectionSettings
  {
    public XmlSetup()
      : this("XmlTv")
    {
    }

    public XmlSetup(string name)
      : base(name)
    {
      InitializeComponent();

    }

    public override void OnSectionDeActivated()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("xmlTv");
      setting.Value = textBoxFolder.Text;
      setting.Persist();
      setting = layer.GetSetting("xmlTvUseTimeZone", "true");
      if (checkBox1.Checked)
        setting.Value = "true";
      else
        setting.Value = "false";
      setting.Persist();
      setting = layer.GetSetting("xmlTvTimeZoneHours", "0");
      setting.Value = textBoxHours.Text;
      setting.Persist();

      setting = layer.GetSetting("xmlTvTimeZoneMins", "0");
      setting.Value = textBoxMinutes.Text;
      setting.Persist();

      
      setting = layer.GetSetting("xmlTvLastUpdate", "");
      setting.Value = "";
      setting.Persist();

      base.OnSectionDeActivated();
    }

    public override void OnSectionActivated()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      textBoxFolder.Text = layer.GetSetting("xmlTv", System.IO.Directory.GetCurrentDirectory()).Value;
      checkBox1.Checked = layer.GetSetting("xmlTvUseTimeZone", "true").Value == "true";
      textBoxHours.Text = layer.GetSetting("xmlTvTimeZoneHours", "0").Value;
      textBoxMinutes.Text = layer.GetSetting("xmlTvTimeZoneMins", "0").Value;
    }

    private void XmlSetup_Load(object sender, EventArgs e)
    {

    }

    private void buttonBrowse_Click(object sender, EventArgs e)
    {
      FolderBrowserDialog dlg = new FolderBrowserDialog();
      dlg.SelectedPath = textBoxFolder.Text;
      dlg.Description = "Specify xmltv folder";
      dlg.ShowNewFolderButton = true;
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        textBoxFolder.Text = dlg.SelectedPath;
      }
    }

    private void textBoxFolder_TextChanged(object sender, EventArgs e)
    {

    }

    private void textBox2_TextChanged(object sender, EventArgs e)
    {

    }

  }
}
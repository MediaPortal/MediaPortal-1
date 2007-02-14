/* 
 *	Copyright (C) 2007 Team MediaPortal
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

#region Usings
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
#endregion

using SetupTv;
using TvDatabase;
namespace TvEngine.PowerScheduler
{
  public partial class PowerSchedulerMasterSetup : SectionSettings
  {

    TvBusinessLayer _layer;

    public PowerSchedulerMasterSetup() : base("PowerSchedulerPlugin")
    {
      InitializeComponent();
      _layer = new TvBusinessLayer();
    }

    public override void LoadSettings()
    {
      Setting setting;
      setting = _layer.GetSetting("PowerSchedulerShutdownActive", "false");
      checkBox1.Checked = Convert.ToBoolean(setting.Value);

      numericUpDown1.Enabled = checkBox1.Checked;
      checkBox3.Enabled = checkBox1.Checked;
      comboBox1.Enabled = checkBox1.Checked;

      setting = _layer.GetSetting("PowerSchedulerIdleTimeout", "5");
      numericUpDown1.Value = Convert.ToDecimal(setting.Value);

      setting = _layer.GetSetting("PowerSchedulerWakeupActive", "false");
      checkBox2.Checked = Convert.ToBoolean(setting.Value);

      numericUpDown2.Enabled = checkBox2.Checked;

      setting = _layer.GetSetting("PowerSchedulerForceShutdown", "false");
      checkBox3.Checked = Convert.ToBoolean(setting.Value);

      setting = _layer.GetSetting("PowerSchedulerShutdownMode", "2");
      comboBox1.SelectedIndex = Convert.ToInt32(setting.Value);

      setting = _layer.GetSetting("PowerSchedulerPreWakeupTime", "60");
      numericUpDown2.Value = Convert.ToDecimal(setting.Value);
    }

    private void checkBox1_CheckedChanged(object sender, EventArgs e)
    {
      Setting setting = _layer.GetSetting("PowerSchedulerShutdownActive", "false");
      if (checkBox1.Checked)
        setting.Value = "true";
      else
        setting.Value = "false";
      setting.Persist();

      numericUpDown1.Enabled = checkBox1.Checked;
      checkBox3.Enabled = checkBox1.Checked;
      comboBox1.Enabled = checkBox1.Checked;
    }

    private void numericUpDown1_ValueChanged(object sender, EventArgs e)
    {
      Setting setting = _layer.GetSetting("PowerSchedulerIdleTimeout", "5");
      setting.Value = numericUpDown1.Value.ToString();
      setting.Persist();
    }

    private void checkBox2_CheckedChanged(object sender, EventArgs e)
    {
      Setting setting = _layer.GetSetting("PowerSchedulerWakeupActive", "false");
      if (checkBox2.Checked)
        setting.Value = "true";
      else
        setting.Value = "false";
      setting.Persist();

      numericUpDown2.Enabled = checkBox2.Checked;
    }

    private void checkBox3_CheckedChanged(object sender, EventArgs e)
    {
      Setting setting = _layer.GetSetting("PowerSchedulerForceShutdown", "false");
      if (checkBox3.Checked)
        setting.Value = "true";
      else
        setting.Value = "false";
      setting.Persist();
    }

    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
      Setting setting = _layer.GetSetting("PowerSchedulerShutdownMode", "2");
      setting.Value = comboBox1.SelectedIndex.ToString();
      setting.Persist();
    }

    private void numericUpDown2_ValueChanged(object sender, EventArgs e)
    {
      Setting setting = _layer.GetSetting("PowerSchedulerPreWakeupTime", "60");
      setting.Value = numericUpDown2.Value.ToString();
      setting.Persist();
    }
  }
}

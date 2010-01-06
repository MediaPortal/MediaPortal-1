#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

#endregion

using System;
using System.Windows.Forms;

namespace MediaPortal.ControlDevices
{
  public partial class ControlDevicePanel : UserControl
  {
    private IControlPlugin _plugin;

    public ControlDevicePanel()
    {
      InitializeComponent();
      _plugin = null;
    }

    public ControlDevicePanel(IControlPlugin plugin)
    {
      InitializeComponent();

      _plugin = plugin;
      ctrlInput.Enabled = _plugin.Capability(EControlCapabilities.CAP_INPUT);
      ctrlOutput.Enabled = _plugin.Capability(EControlCapabilities.CAP_OUTPUT);
      ctrlVerbose.Enabled = _plugin.Capability(EControlCapabilities.CAP_VERBOSELOG);
      ctrlMapping.Enabled = _plugin.Capability(EControlCapabilities.CAP_INPUTMAPPING);
      ctrlAdvanced.Enabled = _plugin.Capability(EControlCapabilities.CAP_SETUP_ADVANCED);
      ctrlDefaults.Enabled = _plugin.Capability(EControlCapabilities.CAP_SETUP_DEFAULT);
    }

    protected void OnSelectionChanged()
    {
      if (null == _plugin)
      {
        return;
      }

      // ...
      if (true == ctrlInput.Checked)
      {
        ctrlMapping.Enabled = _plugin.Capability(EControlCapabilities.CAP_INPUTMAPPING);
        ctrlVerbose.Enabled = _plugin.Capability(EControlCapabilities.CAP_VERBOSELOG);
      }
      else
      {
        ctrlMapping.Enabled = false;
        ctrlVerbose.Enabled = false;
      }

      // ...
      if ((true == ctrlInput.Checked) || (true == ctrlOutput.Checked))
      {
        ctrlAdvanced.Enabled = _plugin.Capability(EControlCapabilities.CAP_SETUP_ADVANCED);
        ctrlVerbose.Enabled = _plugin.Capability(EControlCapabilities.CAP_VERBOSELOG);
      }
      else
      {
        ctrlAdvanced.Enabled = false;
        ctrlVerbose.Enabled = false;
      }
    }

    private void ctrlInput_CheckedChanged(object sender, EventArgs e)
    {
      OnSelectionChanged();
    }

    private void ctrlOutpu_CheckedChanged(object sender, EventArgs e)
    {
      OnSelectionChanged();
    }

    private void ctrlVerbose_CheckedChanged(object sender, EventArgs e) {}

    private void ctrlMapping_Click(object sender, EventArgs e) {}

    private void ctrlAdvanced_Click(object sender, EventArgs e)
    {
      if (null == _plugin)
      {
        return;
      }

      IControlSettings settings = _plugin.Settings;
      settings.ShowAdvancedSettings();
    }

    private void ctrlDefaults_Click(object sender, EventArgs e) {}
  }
}
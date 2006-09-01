using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using MediaPortal.ControlDevices;

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
        return;

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

    private void ctrlVerbose_CheckedChanged(object sender, EventArgs e)
    {

    }

    private void ctrlMapping_Click(object sender, EventArgs e)
    {

    }

    private void ctrlAdvanced_Click(object sender, EventArgs e)
    {
      if (null == _plugin)
        return;

      IControlSettings settings = _plugin.Settings;
      settings.ShowAdvancedSettings();
    }

    private void ctrlDefaults_Click(object sender, EventArgs e)
    {

    }
  }
}

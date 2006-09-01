using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using MediaPortal.ControlDevices;

namespace MediaPortal.ControlDevices.USBUIRT
{
  public class USBUIRTSettings : AbstractControlSettings
  {
    public USBUIRTSettings(IControlPlugin plugin) : base(plugin,"usbuirt") { }

    public override UserControl SettingsPanel { get { return (UserControl)new ControlDevicePanel(_plugin); } }

    public override void ShowAdvancedSettings()
    {
    }
  }
}

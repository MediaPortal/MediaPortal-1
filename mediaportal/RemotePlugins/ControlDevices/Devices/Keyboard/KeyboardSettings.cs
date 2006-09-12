using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using MediaPortal.ControlDevices;

namespace MediaPortal.ControlDevices.Keyboard
{
  class KeyboardSettings : AbstractControlSettings
  {
    public KeyboardSettings(IControlPlugin plugin) : base(plugin, "keyboard") { }

    public override UserControl SettingsPanel { get { return (UserControl)new ControlDevicePanel(_plugin); } }

    public override void ShowAdvancedSettings()
    {
    }
  }
}

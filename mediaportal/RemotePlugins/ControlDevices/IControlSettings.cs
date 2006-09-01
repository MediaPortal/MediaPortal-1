using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.ControlDevices
{
  public interface IControlSettings
  {
    bool Enabled
    {
      set;
      get;
    }

    bool EnableInput
    {
      set;
      get;
    }

    bool EnableOutput
    {
      set;
      get;
    }

    bool Verbose
    {
      set;
      get;
    }

    string Prefix
    {
      get;
    }

    System.Windows.Forms.UserControl SettingsPanel { get; }

    void Load();

    void Save();

    void ShowAdvancedSettings();
  }
}

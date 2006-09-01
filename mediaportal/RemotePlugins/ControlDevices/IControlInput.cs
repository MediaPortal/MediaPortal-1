using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.ControlDevices
{
  public interface IControlInput
  {
    bool Enabled
    {
      set;
      get;
    }

    bool EnabledInput
    {
      set;
      get;
    }

    bool EnabledOutput
    {
      set;
      get;
    }

    bool Verbose
    {
      set;
      get;
    }

    void Default();
    void Load();
    void Save();
  }
}

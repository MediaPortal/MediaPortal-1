using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using MediaPortal.GUI.Library;

namespace MediaPortal.ControlDevices
{
  public interface IControlInput
  {
    bool UseWndProc
    {
      get;
    }

    bool WndProc(ref Message msg, out Action action, out char key, out Keys keyCode);
  }
}

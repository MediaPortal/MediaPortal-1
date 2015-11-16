using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Action = MediaPortal.GUI.Library.Action;

namespace MediaPortal.InputDevices
{
  /// <summary>
  /// Interface for an input device
  /// </summary>
  public interface IInputDevice
  {
    void Init(IntPtr hwnd);
    void DeInit();
    bool WndProc(ref Message msg, out Action action, out char key, out Keys keyCode);

    /// <summary>
    /// Map a WndProc event to an mapping regardless of whether input devices are stopped or not
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    MediaPortal.InputDevices.InputHandler.Mapping GetMapping(Message msg);

    
  }
}

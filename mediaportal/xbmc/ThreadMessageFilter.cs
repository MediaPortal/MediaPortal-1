using System;
using System.Security.Permissions;
using System.Windows.Forms;
using MediaPortal;
using MediaPortal.Util;

namespace MediaPortal
{
  /// <summary>
  /// Provides a thread message filter and handle messages.
  /// </summary>
  public class ThreadMessageFilter : IMessageFilter
  {
    private D3DApp owner;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThreadMessageFilter"/> class.
    /// </summary>
    /// <param name="owner">The owner.</param>
    public ThreadMessageFilter(D3DApp owner)
    {
      this.owner = owner;
    }

    /// <exclude/>
    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    bool IMessageFilter.PreFilterMessage(ref Message m)
    {
      if (m.HWnd != IntPtr.Zero) // Get rid of message if it's sent to a window...
        return false;

      if (m.Msg == Win32API.WM_SHOWWINDOW)
      {
        // Shows the window
        try
        {
          owner.Restore();
          return true;
        }
        catch { } // return false;
      }

      return false;
    }
  }
}
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
using System.Security.Permissions;
using System.Windows.Forms;
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
      {
        return false;
      }

      if (m.Msg == Win32API.WM_SHOWWINDOW)
      {
        // Shows the window
        try
        {
          owner.Restore();
          return true;
        }
        catch {} // return false;
      }

      return false;
    }
  }
}
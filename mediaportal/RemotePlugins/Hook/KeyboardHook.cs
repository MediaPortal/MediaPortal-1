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

using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MediaPortal.Hooks
{
  public class KeyboardHook : Hook
  {
    #region Constructors

    public KeyboardHook()
      : base(HookType.WH_KEYBOARD_LL)
    {
      base.HookInvoked += new HookEventHandler(OnHookInvoked);
    }

    #endregion Constructors

    #region Events

    public event KeyEventHandler KeyDown;
    public event KeyEventHandler KeyUp;

    #endregion Events

    #region Methods

    private void OnHookInvoked(object sender, HookEventArgs e)
    {
      if (e.WParam == 256 && KeyDown != null)
      {
        KeyboardHookStruct khs = new KeyboardHookStruct(e);
        KeyDown(sender, new KeyEventArgs((Keys) khs.virtualKey | Control.ModifierKeys));
      }
      else if (e.WParam == 257 && KeyUp != null)
      {
        KeyboardHookStruct khs = new KeyboardHookStruct(e);
        KeyUp(sender, new KeyEventArgs((Keys) khs.virtualKey | Control.ModifierKeys));
      }
    }

    #endregion Methods

    #region Structures

    private struct KeyboardHookStruct
    {
      public KeyboardHookStruct(HookEventArgs e)
      {
        KeyboardHookStruct khs = (KeyboardHookStruct) Marshal.PtrToStructure(e.LParam, typeof (KeyboardHookStruct));

        virtualKey = khs.virtualKey;
        scanCode = khs.scanCode;
        flags = khs.flags;
        time = khs.time;
        dwExtraInfo = khs.dwExtraInfo;
      }

      public int virtualKey;
      public int scanCode;
      public int flags;
      public int time;
      public int dwExtraInfo;
    }

    #endregion Structures
  }
}
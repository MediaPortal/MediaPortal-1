#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Reflection;
using System.Runtime.InteropServices;

namespace MediaPortal.Hooks
{
  public class Hook
  {
    #region Constructors

    public Hook()
    {
    }

    public Hook(HookType hookType)
    {
      _hookType = hookType;
      _hookDelegate = new HookDelegate(this.InternalHookDelegate);
    }

    public Hook(HookType hookType, HookDelegate hookDelegate)
    {
      _hookType = hookType;
      _hookDelegate = hookDelegate;
    }

    #endregion Constructors

    #region Events

    public event HookEventHandler HookInvoked;

    #endregion Events

    #region Methods

    int InternalHookDelegate(int code, int wParam, IntPtr lParam)
    {
      if (code == 0)
      {
        HookEventArgs e = new HookEventArgs(code, wParam, lParam);

        OnHookInvoked(e);

        if (e.Handled)
          return 1;
      }

      return NativeMethods.CallNextHookEx(_hookHandle, code, wParam, lParam);
    }

    void OnHookInvoked(HookEventArgs e)
    {
      if (HookInvoked != null)
        HookInvoked(this, e);
    }

    #endregion Methods

    #region Properties

    public bool IsEnabled
    {
      get { return _hookHandle != IntPtr.Zero; }
      set
      {
        if (value && _hookHandle == IntPtr.Zero)
        {
          _hookHandle = NativeMethods.SetWindowsHookEx(_hookType, _hookDelegate, Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]), 0 /* AppDomain.GetCurrentThreadId() */);
        }
        else if (value == false && _hookHandle != IntPtr.Zero)
        {
          NativeMethods.UnhookWindowsHookEx(_hookHandle);

          _hookHandle = IntPtr.Zero;
        }
      }
    }

    #endregion Properties

    #region Fields

    HookDelegate _hookDelegate = null;
    IntPtr _hookHandle = IntPtr.Zero;
    HookType _hookType;

    #endregion Fields
  }
}

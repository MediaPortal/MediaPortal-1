#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MediaPortal.Hooks
{
  public class Hook
  {
    #region Constructors

    public Hook() {}

    public Hook(Util.Win32API.HookType hookType)
    {
      _hookType = hookType;
      _hookDelegate = new Util.Win32API.HookDelegate(this.InternalHookDelegate);
    }

    public Hook(Util.Win32API.HookType hookType, Util.Win32API.HookDelegate hookDelegate)
    {
      _hookType = hookType;
      _hookDelegate = hookDelegate;
    }

    #endregion Constructors

    #region Events

    public event HookEventHandler HookInvoked;

    #endregion Events

    #region Methods

    private int InternalHookDelegate(int code, int wParam, IntPtr lParam)
    {
      if (code == 0)
      {
        HookEventArgs e = new HookEventArgs(code, wParam, lParam);

        OnHookInvoked(e);

        if (e.Handled)
        {
          return 1;
        }
      }

      return Util.Win32API.CallNextHookEx(_hookHandle, code, wParam, lParam);
    }

    private void OnHookInvoked(HookEventArgs e)
    {
      if (HookInvoked != null)
      {
        HookInvoked(this, e);
      }
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
          _hookHandle = Util.Win32API.SetWindowsHookEx(_hookType, _hookDelegate,
                                                       Marshal.GetHINSTANCE(
                                                         Assembly.GetExecutingAssembly().GetModules()[0]), 0
            /* AppDomain.GetCurrentThreadId() */);
        }
        else if (value == false && _hookHandle != IntPtr.Zero)
        {
          Util.Win32API.UnhookWindowsHookEx(_hookHandle);

          _hookHandle = IntPtr.Zero;
        }
      }
    }

    #endregion Properties

    #region Fields

    private Util.Win32API.HookDelegate _hookDelegate = null;
    private IntPtr _hookHandle = IntPtr.Zero;
    private Util.Win32API.HookType _hookType;

    #endregion Fields
  }
}
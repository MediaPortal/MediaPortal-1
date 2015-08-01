#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Runtime.InteropServices;
using Mediaportal.TV.Server.Plugins.TunerExtension.Turbosight.RemoteControl.Enum;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.Turbosight.RemoteControl
{
  /// <summary>
  /// A call back interface for receiving remote control key press events from
  /// the TBS NXP IR RC receiver wrapper.
  /// </summary>
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface INxpRemoteControlKeyPressCallBack
  {
    /// <summary>
    /// Invoked by the wrapper when a remote control key press event is fired
    /// by the underlying driver.
    /// </summary>
    /// <param name="keyCode">The remote control key's unique code.</param>
    /// <param name="protocol">The protocol/format used to detect and process the code.</param>
    /// <param name="context">The optional context passed to the wrapper when the interface was initialised.</param>
    /// <returns>an HRESULT indicating whether the event was handled successfully</returns>
    [PreserveSig]
    int OnKeyPress(uint keyCode, NxpRemoteControlProtocol protocol, IntPtr context);
  }
}
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

using System.Runtime.InteropServices;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Multiplexer
{
  /// <summary>
  /// The MediaPortal transport stream multiplexer filter class.
  /// </summary>
  [Guid("511d13f0-8a56-42fa-b151-b72a325cf71a")]
  internal class MediaPortalTsMultiplexer
  {
  }

  /// <summary>
  /// The main interface on the MediaPortal transport stream multiplexer.
  /// </summary>
  [Guid("8533d2d1-1be1-4262-b70a-432df592b903"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  internal interface ITsMultiplexer
  {
    /// <summary>
    /// Configure the logging subsystem for the multiplexer.
    /// </summary>
    /// <remarks>
    /// This function must be called immediately after the multiplexer is instanciated.
    /// </remarks>
    /// <param name="path">The path to the log and debug files.</param>
    /// <returns>an HRESULT indicating whether the function succeeded</returns>
    [PreserveSig]
    int ConfigureLogging([MarshalAs(UnmanagedType.LPWStr)] string path);

    /// <summary>
    /// Enable or disable input stream dumping for one or more input pins.
    /// </summary>
    /// <remarks>
    /// The multiplexer will copy the raw input stream received from enabled input pins to file.
    /// One file per enabled pin. The files will be overwritten/recreated each time streaming is
    /// started.
    /// </remarks>
    /// <param name="pinMask">A bit mask specifying the pins to enable/disable.</param>
    [PreserveSig]
    void DumpInput(int pinMask);

    /// <summary>
    /// Enable or disable output stream dumping.
    /// </summary>
    /// <param name="enable"><c>True</c> to enable dumping.</param>
    [PreserveSig]
    void DumpOutput([MarshalAs(UnmanagedType.I1)] bool enable);

    /// <summary>
    /// Set the components for the multiplexer to operate on.
    /// </summary>
    /// <param name="video"><c>True</c> if video streams should be multiplexed into the output transport stream.</param>
    /// <param name="audio"><c>True</c> if audio streams should be multiplexed into the output transport stream.</param>
    /// <param name="rds"><c>True</c> if information from RDS (radio data system) streams should be included in the output transport stream.</param>
    /// <param name="teletext"><c>True</c> if teletext streams should be multiplexed into the output transport stream.</param>
    /// <param name="vps"><c>True</c> if VPS (video programming system) streams should be multiplexed into the output transport stream.</param>
    /// <param name="wss"><c>True</c> if WSS (wide screen signalling) streams should be multiplexed into the output transport stream.</param>
    /// <returns>an HRESULT indicating whether the function succeeded</returns>
    [PreserveSig]
    int SetActiveComponents([MarshalAs(UnmanagedType.I1)] bool video,
                            [MarshalAs(UnmanagedType.I1)] bool audio,
                            [MarshalAs(UnmanagedType.I1)] bool rds,
                            [MarshalAs(UnmanagedType.I1)] bool teletext,
                            [MarshalAs(UnmanagedType.I1)] bool vps,
                            [MarshalAs(UnmanagedType.I1)] bool wss);
  }
}
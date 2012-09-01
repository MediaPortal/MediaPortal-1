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

namespace TvLibrary.Interfaces.Analyzer
{
  /// <summary>
  /// TsWriter PMT grabber callback interface.
  /// </summary>
  [ComVisible(true), ComImport,
    Guid("37a1c1e3-4760-49fe-ab59-6688ada54923"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IPmtCallBack
  {
    /// <summary>
    /// Called by an ITsPmtGrabber instance when it receives a new PMT section for a service.
    /// </summary>
    /// <param name="pmtPid">The PID of the elementary stream from which the PMT section received.</param>
    /// <param name="serviceId">The ID associated with the service which the PMT section is associated with.</param>
    /// <param name="isServiceRunning">Indicates whether the service that the grabber is monitoring is active.
    ///   The grabber will not wait for PMT to be received if it thinks the service is not running.</param>
    /// <returns>an HRESULT indicating whether the PMT section was successfully handled</returns>
    [PreserveSig]
    int OnPmtReceived(int pmtPid, int serviceId, bool isServiceRunning);
  }

  /// <summary>
  /// TsWriter PMT grabber interface.
  /// </summary>
  [ComVisible(true), ComImport,
    Guid("6e714740-803d-4175-bef6-67246bdf1855"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITsPmtGrabber
  {
    /// <summary>
    /// Set the PID and service for the grabber to monitor.
    /// </summary>
    /// <remarks>
    /// The service ID must also be set because a PID may carry PMT for more than one service.
    /// The PMT PID may be zero, in which case the grabber will use the service ID to determine
    /// the PID which is currently carrying the service's PMT, using the PAT.
    /// The service ID may be zero, in which case the grabber will grab PMT for the first service
    /// that it finds in the PAT.
    /// </remarks>
    /// <param name="pmtPid">The PID that the grabber should monitor.</param>
    /// <param name="serviceId">The ID of the service that the grabber should monitor.</param>
    /// <returns>an HRESULT indicating whether the grabber parameters were successfully registered</returns>
    [PreserveSig]
    int SetPmtPid(int pmtPid, int serviceId);

    /// <summary>
    /// Set the delegate for the grabber to notify when a new PMT section is received.
    /// </summary>
    /// <param name="callBack">The delegate callback interface.</param>
    /// <returns>an HRESULT indicating whether the delegate was successfully registered</returns>
    [PreserveSig]
    int SetCallBack(IPmtCallBack callBack);

    /// <summary>
    /// Used by the delegate to retrieve a PMT section from the grabber.
    /// </summary>
    /// <param name="pmtData">A pointer to a buffer that will be populated with the most recently received PMT section.</param>
    /// <returns>the length of the PMT section in bytes</returns>
    [PreserveSig]
    int GetPmtData(IntPtr pmtData);
  }
}
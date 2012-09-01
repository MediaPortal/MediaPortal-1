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

namespace TvLibrary.Interfaces.Analyzer
{
  /// <summary>
  /// Possible elementary stream or service encryption states.
  /// </summary>
  public enum EncryptionState
  {
    /// <summary>
    /// Encryption state not yet determined.
    /// </summary>
    NotSet = -1,
    /// <summary>
    /// Not encrypted (free-to-air).
    /// </summary>
    Clear,
    /// <summary>
    /// Encrypted.
    /// </summary>
    Encrypted
  }

  /// <summary>
  /// TsWriter encryption analyser callback interface.
  /// </summary>
  [ComVisible(true), ComImport,
    Guid("7b42a7b1-0f93-44f4-9f0f-57b3a424d882"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IEncryptionStateChangeCallBack
  {
    /// <summary>
    /// Called by an ITsEncryptionAnalyser instance when the encryption state of any of the elementary streams it is monitoring changes.
    /// </summary>
    /// <param name="pid">The PID associated with the elementary stream that changed state.</param>
    /// <param name="encryptionState">The current encryption state of the elementary stream.</param>
    /// <returns>an HRESULT indicating whether the notification was successfully handled</returns>
    [PreserveSig]
    int OnEncryptionStateChange(int pid, EncryptionState encryptionState);
  }

  /// <summary>
  /// TsWriter encryption analyser interface.
  /// </summary>
  [ComVisible(true), ComImport,
    Guid("59f8d617-92fd-48d5-8f6d-a97bfd95c448"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITsEncryptionAnalyser
  {
    /// <summary>
    /// Add an elementary stream to the set of streams that the analyser should monitor.
    /// </summary>
    /// <param name="pid">The PID associated with the elementary stream.</param>
    /// <returns>an HRESULT indicating whether the elementary stream was successfully registered</returns>
    [PreserveSig]
    int AddPid(int pid);

    /// <summary>
    /// Remove an elementary stream from the set of streams that the analyser is monitoring.
    /// </summary>
    /// <param name="pid">The PID associated with the elementary stream.</param>
    /// <returns>an HRESULT indicating whether the elementary stream was successfully deregistered</returns>
    [PreserveSig]
    int RemovePid(int pid);

    /// <summary>
    /// Get a count of the elementary streams that the analyser is currently monitoring.
    /// </summary>
    /// <param name="pidCount">The number of elementary streams that the analyser is currently monitoring.</param>
    /// <returns>an HRESULT indicating whether the elementary stream count was successfully retrieved</returns>
    [PreserveSig]
    int GetPidCount(out int pidCount);

    /// <summary>
    /// Get the encryption state for a specific elementary stream that the analyser is monitoring.
    /// </summary>
    /// <param name="pidIndex">The PID index. The value of this parameter should be in the range 0..[GetPidCount() - 1] (inclusive).</param>
    /// <param name="pid">The PID associated with the stream.</param>
    /// <param name="encryptionState">The current encryption state of the elementary stream.</param>
    /// <returns>an HRESULT indicating whether the elementary stream state was successfully retrieved</returns>
    [PreserveSig]
    int GetPid(int pidIndex, out int pid, out EncryptionState encryptionState);

    /// <summary>
    /// Set the delegate for the analyser to notify when the encryption state of one of the monitored elementary streams changes.
    /// </summary>
    /// <param name="callBack">The delegate callback interface.</param>
    /// <returns>an HRESULT indicating whether the delegate was successfully registered</returns>
    [PreserveSig]
    int SetCallBack(IEncryptionStateChangeCallBack callBack);

    /// <summary>
    /// Reset the encryption analyser.
    /// </summary>
    /// <remarks>
    /// This clears the list of elementary streams that the analyser is monitoring.
    /// </remarks>
    /// <returns>an HRESULT indicating whether the analyser was successfully reset</returns>
    [PreserveSig]
    int Reset();
  }
}
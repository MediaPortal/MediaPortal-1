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
  /// TsWriter CAT grabber callback interface.
  /// </summary>
  [ComVisible(true), ComImport,
    Guid("38536ab6-7a41-404f-917F-c47dd8b639c7"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ICaCallBack
  {
    /// <summary>
    /// Called by an ITsCaGrabber instance when it receives a new CAT section.
    /// </summary>
    /// <returns>an HRESULT indicating whether the CAT section was successfully handled</returns>
    [PreserveSig]
    int OnCaReceived();
  }

  /// <summary>
  /// TsWriter CAT grabber interface.
  /// </summary>
  [ComVisible(true), ComImport,
    Guid("f9aa3910-7818-452a-94d1-72e039df50ef"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITsCaGrabber
  {
    /// <summary>
    /// Reset the grabber, causing it to forget about previously seen CAT sections.
    /// </summary>
    /// <returns>an HRESULT indicating whether the grabber was successfully reset</returns>
    [PreserveSig]
    int Reset();

    /// <summary>
    /// Set the delegate for the grabber to notify when a new CAT section is received.
    /// </summary>
    /// <param name="callBack">The delegate callback interface.</param>
    /// <returns>an HRESULT indicating whether the delegate was successfully registered</returns>
    [PreserveSig]
    int SetCallBack(ICaCallBack callBack);

    /// <summary>
    /// Used by the delegate to retrieve a CAT section from the grabber.
    /// </summary>
    /// <param name="caData">A pointer to a buffer that will be populated with the most recently received CAT section.</param>
    /// <returns>the length of the CAT section in bytes</returns>
    [PreserveSig]
    int GetCaData(IntPtr caData);
  }
}
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
  public class MediaPortalTsMultiplexer
  {
  }

  /// <summary>
  /// The main interface on the MediaPortal transport stream multiplexer.
  /// </summary>
  [Guid("8533d2d1-1be1-4262-b70a-432df592b903"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITsMultiplexer
  {
    /// <summary>
    /// Set the components for the multiplexer to operate on.
    /// </summary>
    /// <param name="video">Should video streams be multiplexed into the output transport stream.</param>
    /// <param name="audio">Should audio streams be multiplexed into the output transport stream.</param>
    /// <param name="teletext">Should teletext streams be multiplexed into the output transport stream.</param>
    /// <returns>an HRESULT indicating whether the function succeeded</returns>
    [PreserveSig]
    int SetActiveComponents([MarshalAs(UnmanagedType.I1)] bool video, [MarshalAs(UnmanagedType.I1)] bool audio, [MarshalAs(UnmanagedType.I1)] bool teletext);
  }
}
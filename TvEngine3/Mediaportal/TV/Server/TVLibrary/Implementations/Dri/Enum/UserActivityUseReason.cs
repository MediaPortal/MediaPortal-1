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

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Dri.Enum
{
  /// <remarks>
  /// http://msdn.microsoft.com/en-us/library/windows/desktop/dd797938%28v=vs.85%29.aspx
  /// </remarks>
  [Flags]
  internal enum UserActivityUseReason : uint
  {
    None = 0,
    SetupOrScanning = 1,
    Playback = 2,
    PlaybackPictureInPicture = 4,
    RecordingUserDirected = 8,
    RecordingSpeculative = 16
  }
}
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

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Enum
{
  /// <summary>
  /// Supported stream formats.
  /// </summary>
  /// <remarks>
  /// Currently all supported formats are based on the MPEG 2 transport stream.
  /// </remarks>
  [Flags]
  internal enum StreamFormat
  {
    Default = 0,
    Mpeg2Ts = 1,
    Dvb = 2,
    Atsc = 4,
    Scte = 8,
    Analog = 16,    // analog TV or FM radio encoded and wrapped into an MPEG 2 TS
    Freesat = 32
  }
}
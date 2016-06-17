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

namespace Mediaportal.TV.Server.Common.Types.Enum
{
  /// <summary>
  /// QualityType's for setting the desired quality
  /// </summary>
  public enum QualityType
  {
    // Seven least significant bits represent average bit rate; next seven bits represent peak bit rate.

    /// <summary>default quality</summary>
    Default = 0,
    /// <summary>portable quality setting for those recordings that dont need to be close to perfect</summary>
    Portable = (45 << 7) | 20,
    /// <summary>low quality setting for those recordings that dont need to be close to perfect</summary>
    Low = (55 << 7) | 33,
    /// <summary>medium quality but still quite a bit less diskspace needed than high</summary>
    Medium = (88 << 7) | 66,
    /// <summary>high quality setting will create larger files then the other options</summary>
    High = (100 << 7) | 100,
    /// <summary>custom quality setting, defined in SetupTv</summary>
    Custom = 0x3fff
  }
}
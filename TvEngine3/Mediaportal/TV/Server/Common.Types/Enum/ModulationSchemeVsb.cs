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

using System.ComponentModel;

namespace Mediaportal.TV.Server.Common.Types.Enum
{
  /// <summary>
  /// Vestigial side band modulation schemes (ATSC).
  /// </summary>
  public enum ModulationSchemeVsb
  {
    /// <summary>
    /// Auto.
    /// </summary>
    /// <remarks>
    /// The Auto value is set to separate the value range from the BDA
    /// ModulationType range. Plugins may need to translate to specific
    /// ModulationType values before the tuning translation.
    /// </remarks>
    Automatic = 100,
    /// <summary>
    /// 8 VSB.
    /// </summary>
    [Description("8 VSB")]
    Vsb8,
    /// <summary>
    /// 16 VSB.
    /// </summary>
    [Description("16 VSB")]
    Vsb16
  }
}
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
  /// Logical DiSEqC switch commands for DiSEqC 1.0 and 1.1 compatible switches.
  /// </summary>
  public enum DiseqcPort
  {
    /// <summary>
    /// DiSEqC not used.
    /// </summary>
    None = 0,
    /// <summary>
    /// DiSEqC 1.0 port A (option A, position A)
    /// </summary>
    [Description("1.0 Port A (Option A Position A)")]
    PortA,
    /// <summary>
    /// DiSEqC 1.0 port B (option A, position B)
    /// </summary>
    [Description("1.0 Port B (Option A Position B)")]
    PortB,
    /// <summary>
    /// DiSEqC 1.0 port C (option B, position A)
    /// </summary>
    [Description("1.0 Port C (Option B Position A)")]
    PortC,
    /// <summary>
    /// DiSEqC 1.0 port D (option B, position B)
    /// </summary>
    [Description("1.0 Port D (Option B Position B)")]
    PortD,
    /// <summary>
    /// DiSEqC 1.1 port 1
    /// </summary>
    [Description("1.1 Port 1")]
    Port1,
    /// <summary>
    /// DiSEqC 1.1 port 2
    /// </summary>
    [Description("1.1 Port 2")]
    Port2,
    /// <summary>
    /// DiSEqC 1.1 port 3
    /// </summary>
    [Description("1.1 Port 3")]
    Port3,
    /// <summary>
    /// DiSEqC 1.1 port 4
    /// </summary>
    [Description("1.1 Port 4")]
    Port4,
    /// <summary>
    /// DiSEqC 1.1 port 5
    /// </summary>
    [Description("1.1 Port 5")]
    Port5,
    /// <summary>
    /// DiSEqC 1.1 port 6
    /// </summary>
    [Description("1.1 Port 6")]
    Port6,
    /// <summary>
    /// DiSEqC 1.1 port 7
    /// </summary>
    [Description("1.1 Port 7")]
    Port7,
    /// <summary>
    /// DiSEqC 1.1 port 8
    /// </summary>
    [Description("1.1 Port 8")]
    Port8,
    /// <summary>
    /// DiSEqC 1.1 port 9
    /// </summary>
    [Description("1.1 Port 9")]
    Port9,
    /// <summary>
    /// DiSEqC 1.1 port 10
    /// </summary>
    [Description("1.1 Port 10")]
    Port10,
    /// <summary>
    /// DiSEqC 1.1 port 11
    /// </summary>
    [Description("1.1 Port 11")]
    Port11,
    /// <summary>
    /// DiSEqC 1.1 port 12
    /// </summary>
    [Description("1.1 Port 12")]
    Port12,
    /// <summary>
    /// DiSEqC 1.1 port 13
    /// </summary>
    [Description("1.1 Port 13")]
    Port13,
    /// <summary>
    /// DiSEqC 1.1 port 14
    /// </summary>
    [Description("1.1 Port 14")]
    Port14,
    /// <summary>
    /// DiSEqC 1.1 port 15
    /// </summary>
    [Description("1.1 Port 15")]
    Port15,
    /// <summary>
    /// DiSEqC 1.1 port 16
    /// </summary>
    [Description("1.1 Port 16")]
    Port16
  }
}
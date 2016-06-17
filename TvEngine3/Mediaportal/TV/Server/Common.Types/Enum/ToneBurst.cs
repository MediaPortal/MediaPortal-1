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
  /// Tone burst (simple DiSEqC) commands.
  /// </summary>
  public enum ToneBurst
  {
    /// <summary>
    /// Tone/data burst not used.
    /// </summary>
    None = 0,
    /// <summary>
    /// Tone burst, also known as "simple A" or "unmodulated".
    /// </summary>
    [Description("Tone Burst (Simple A, Unmodulated)")]
    ToneBurst,
    /// <summary>
    /// Data burst, also known as "simple B" or "modulated".
    /// </summary>
    [Description("Data Burst (Simple B, Modulated)")]
    DataBurst
  }
}
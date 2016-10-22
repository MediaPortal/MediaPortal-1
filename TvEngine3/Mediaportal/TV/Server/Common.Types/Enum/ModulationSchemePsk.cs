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
  /// Phase shift keying modulation schemes (DC II, DSS, DVB-DSNG, DVB-S,
  /// DVB-S2 and ISDB-S).
  /// </summary>
  public enum ModulationSchemePsk
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
    /// BPSK.
    /// </summary>
    [Description("BPSK")]
    Psk2,
    /// <summary>
    /// QPSK.
    /// </summary>
    [Description("QPSK")]
    Psk4,
    /// <summary>
    /// Offset QPSK.
    /// </summary>
    [Description("Offset QPSK")]
    Psk4Offset,
    /// <summary>
    /// Split-mode QPSK, I phase.
    /// </summary>
    [Description("Split-mode QPSK, I Phase")]
    Psk4SplitI,
    /// <summary>
    /// Split-mode QPSK, Q phase.
    /// </summary>
    [Description("Split-mode QPSK, Q Phase")]
    Psk4SplitQ,
    /// <summary>
    /// 8 PSK.
    /// </summary>
    [Description("8 PSK")]
    Psk8,
    /// <summary>
    /// 16 [A]PSK.
    /// </summary>
    [Description("16 APSK")]
    Psk16,
    /// <summary>
    /// 32 [A]PSK.
    /// </summary>
    [Description("32 APSK")]
    Psk32,
    /// <summary>
    /// 64 [A]PSK.
    /// </summary>
    [Description("64 APSK")]
    Psk64,
    /// <summary>
    /// 128 [A]PSK.
    /// </summary>
    [Description("128 APSK")]
    Psk128,
    /// <summary>
    /// 256 [A]PSK.
    /// </summary>
    [Description("256 APSK")]
    Psk256
  }
}
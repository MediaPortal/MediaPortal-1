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
  /// Quadrature amplitude modulation schemes (DVB-C, ISDB-C and SCTE).
  /// </summary>
  public enum ModulationSchemeQam
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
    /// 16 QAM.
    /// </summary>
    [Description("16 QAM")]
    Qam16,
    /// <summary>
    /// 32 QAM.
    /// </summary>
    [Description("32 QAM")]
    Qam32,
    /// <summary>
    /// 32 QAM.
    /// </summary>
    [Description("64 QAM")]
    Qam64,
    /// <summary>
    /// 128 QAM.
    /// </summary>
    [Description("128 QAM")]
    Qam128,
    /// <summary>
    /// 256 QAM.
    /// </summary>
    [Description("256 QAM")]
    Qam256,
    /// <summary>
    /// 512 QAM.
    /// </summary>
    [Description("512 QAM")]
    Qam512,
    /// <summary>
    /// 1024 QAM.
    /// </summary>
    [Description("1024 QAM")]
    Qam1024,
    /// <summary>
    /// 2048 QAM.
    /// </summary>
    [Description("2048 QAM")]
    Qam2048,
    /// <summary>
    /// 4096 QAM.
    /// </summary>
    [Description("4096 QAM")]
    Qam4096
  }
}
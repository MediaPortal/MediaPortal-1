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
using System.ComponentModel;

namespace Mediaportal.TV.Server.Common.Types.Enum
{
  /// <summary>
  /// Audio input type.
  /// </summary>
  [Flags]
  public enum CaptureSourceAudio
  {
    /// <summary>
    /// No audio.
    /// </summary>
    None = 0,
    /// <summary>
    /// The configured default for the tuner.
    /// </summary>
    [Description("Tuner Default")]
    TunerDefault = 0x0001,
    /// <summary>
    /// Automatic (detection based on related pin index reported by the driver).
    /// </summary>
    Automatic = 0x0002,
    /// <summary>
    /// Tuner.
    /// </summary>
    Tuner = 0x0004,
    /// <summary>
    /// Auxiliary input #1.
    /// </summary>
    [Description("Auxiliary #1")]
    Auxiliary1 = 0x0008,
    /// <summary>
    /// Auxiliary input #2.
    /// </summary>
    [Description("Auxiliary #2")]
    Auxiliary2 = 0x0010,
    /// <summary>
    /// Auxiliary input #3.
    /// </summary>
    [Description("Auxiliary #3")]
    Auxiliary3 = 0x0020,
    /// <summary>
    /// Line input #1.
    /// </summary>
    [Description("Line #1")]
    Line1 = 0x0040,
    /// <summary>
    /// Line input #2.
    /// </summary>
    [Description("Line #2")]
    Line2 = 0x0080,
    /// <summary>
    /// Line input #3.
    /// </summary>
    [Description("Line #3")]
    Line3 = 0x0100,
    /// <summary>
    /// S/PDIF input #1.
    /// </summary>
    [Description("S/PDIF #1")]
    Spdif1 = 0x0200,
    /// <summary>
    /// S/PDIF input #2.
    /// </summary>
    [Description("S/PDIF #2")]
    Spdif2 = 0x0400,
    /// <summary>
    /// S/PDIF input #3.
    /// </summary>
    [Description("S/PDIF #3")]
    Spdif3 = 0x0800,
    /// <summary>
    /// AES/EBU input #1.
    /// </summary>
    [Description("AES/EBU #1")]
    AesEbu1 = 0x1000,
    /// <summary>
    /// AES/EBU input #2.
    /// </summary>
    [Description("AES/EBU #2")]
    AesEbu2 = 0x2000,
    /// <summary>
    /// AES/EBU input #3.
    /// </summary>
    [Description("AES/EBU #3")]
    AesEbu3 = 0x4000,
    /// <summary>
    /// HDMI input #1.
    /// </summary>
    [Description("HDMI #1")]
    Hdmi1 = 0x8000,
    /// <summary>
    /// HDMI input #2.
    /// </summary>
    [Description("HDMI #2")]
    Hdmi2 = 0x010000,
    /// <summary>
    /// HDMI input #3.
    /// </summary>
    [Description("HDMI #3")]
    Hdmi3 = 0x020000
  }
}
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
  /// Capture video sources.
  /// </summary>
  [Flags]
  public enum CaptureSourceVideo
  {
    /// <summary>
    /// No video.
    /// </summary>
    None = 0,
    /// <summary>
    /// The configured default for the tuner.
    /// </summary>
    [Description("Tuner Default")]
    TunerDefault = 0x0001,
    /// <summary>
    /// Tuner.
    /// </summary>
    Tuner = 0x0002,
    /// <summary>
    /// Composite (CVBS) input #1.
    /// </summary>
    [Description("Composite #1")]
    Composite1 = 0x0004,
    /// <summary>
    /// Composite (CVBS) input #2.
    /// </summary>
    [Description("Composite #2")]
    Composite2 = 0x0008,
    /// <summary>
    /// Composite (CVBS) input #3.
    /// </summary>
    [Description("Composite #3")]
    Composite3 = 0x0010,
    /// <summary>
    /// S-video input #1.
    /// </summary>
    [Description("S-video #1")]
    Svideo1 = 0x0020,
    /// <summary>
    /// S-video input #2.
    /// </summary>
    [Description("S-video #2")]
    Svideo2 = 0x0040,
    /// <summary>
    /// S-video input #3.
    /// </summary>
    [Description("S-video #3")]
    Svideo3 = 0x0080,
    /// <summary>
    /// RGB input #1.
    /// </summary>
    [Description("RGB #1")]
    Rgb1 = 0x0100,
    /// <summary>
    /// RGB input #2.
    /// </summary>
    [Description("RGB #2")]
    Rgb2 = 0x0200,
    /// <summary>
    /// RGB input #3.
    /// </summary>
    [Description("RGB #3")]
    Rgb3 = 0x0400,
    /// <summary>
    /// YrYbY input #1.
    /// </summary>
    [Description("YrYbY #1")]
    Yryby1 = 0x0800,
    /// <summary>
    /// YrYbY input #2.
    /// </summary>
    [Description("YrYbY #2")]
    Yryby2 = 0x1000,
    /// <summary>
    /// YrYbY input #3.
    /// </summary>
    [Description("YrYbY #3")]
    Yryby3 = 0x2000,
    /// <summary>
    /// HDMI input #1.
    /// </summary>
    [Description("HDMI #1")]
    Hdmi1 = 0x4000,
    /// <summary>
    /// HDMI input #2.
    /// </summary>
    [Description("HDMI #2")]
    Hdmi2 = 0x8000,
    /// <summary>
    /// HDMI input #3.
    /// </summary>
    [Description("HDMI #3")]
    Hdmi3 = 0x010000
  }
}
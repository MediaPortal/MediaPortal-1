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
  public enum VideoOrCameraProperty
  {
    // From DirectShow IAMVideoProcAmp.
    Brightness,
    Contrast,
    Hue,
    Saturation,
    Sharpness,
    Gamma,
    [Description("Colour Enable")]
    ColorEnable,
    [Description("White Balance")]
    WhiteBalance,
    [Description("Backlight Compensation Enable")]
    BacklightCompensation,
    Gain,
    [Description("Digital Multiplier (Zoom)")]
    DigitalMultiplier,
    [Description("Digital Multiplier (Zoom) Limit")]
    DigitalMultiplierLimit,
    [Description("White Balance Component")]
    WhiteBalanceComponent,      // May not work for DirectShow due to two value structure.
    [Description("Power Line Frequency (Anti-Flicker)")]
    PowerLineFrequency,

    // From DirectShow IAMCameraControl.
    Pan,
    Tilt,
    Roll,
    Zoom,
    Exposure,
    [Description("Aperture")]
    Iris,
    Focus,
    [Description("Scan Mode (Interlaced/Progressive)")]
    ScanMode,
    [Description("Privacy (Video On/Off)")]
    Privacy,
    [Description("Pan/Tilt")]
    PanTilt,                    // May not work for DirectShow due to two value structure.
    [Description("Pan (Relative)")]
    PanRelative,
    [Description("Tilt (Relative)")]
    TiltRelative,
    [Description("Roll (Relative)")]
    RollRelative,
    [Description("Zoom (Relative)")]
    ZoomRelative,
    [Description("Exposure (Relative)")]
    ExposureRelative,
    [Description("Aperture (Relative)")]
    IrisRelative,
    [Description("Focus (Relative)")]
    FocusRelative,
    [Description("Pan/Tilt (Relative)")]
    PanTiltRelative,
    //FocalLength,        DirectShow: Read-only, plus unique structure => won't work.
    [Description("Low Light Compensation (Variable Frame Rate)")]
    AutoExposurePriority
  }
}
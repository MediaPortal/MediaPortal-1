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
  /// Broadcast standards.
  /// </summary>
  [Flags]
  public enum BroadcastStandard
  {
    /// <summary>
    /// Unknown/unsupported standard.
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// External input, for example HDMI, s-video, composite, microphone etc.
    /// </summary>
    [Description("External Input")]
    ExternalInput = 0x00000001,
    /// <summary>
    /// Analog television (NTSC, PAL, SECAM).
    /// </summary>
    [Description("Analog TV")]
    AnalogTelevision = 0x00000002,
    /// <summary>
    /// Amplitude-modulated analog radio.
    /// </summary>
    [Description("AM Radio")]
    AmRadio = 0x00000004,
    /// <summary>
    /// Frequency-modulated analog radio.
    /// </summary>
    [Description("FM Radio")]
    FmRadio = 0x00000008,
    /// <summary>
    /// First generation Digital Video Broadcast standard for cable broadcast.
    /// </summary>
    [Description("DVB-C")]
    DvbC = 0x00000010,
    /// <summary>
    /// Second generation Digital Video Broadcast standard for cable broadcast.
    /// </summary>
    [Description("DVB-C2")]
    DvbC2 = 0x00000020,
    /// <summary>
    /// Digital Video Broadcast standard for digital satellite news gathering.
    /// </summary>
    [Description("DVB-DSNG")]
    DvbDsng = 0x00000040,
    /// <summary>
    /// Digital Video Broadcast standard for internet protocol broadcast.
    /// </summary>
    [Description("DVB-IP")]
    DvbIp = 0x00000080,
    /// <summary>
    /// First generation Digital Video Broadcast standard for satellite broadcast.
    /// </summary>
    [Description("DVB-S")]
    DvbS = 0x00000100,
    /// <summary>
    /// Second generation Digital Video Broadcast standard for satellite broadcast.
    /// </summary>
    [Description("DVB-S2")]
    DvbS2 = 0x00000200,
    /// <summary>
    /// Professional features (eg. 16 and 32 APSK) for the second generation Digital Video Broadcast standard for satellite broadcast.
    /// </summary>
    [Description("DVB-S2 Pro")]
    DvbS2Pro = 0x00000400,
    /// <summary>
    /// Optional extension to the second generation Digital Video Broadcast standard for satellite broadcast.
    /// </summary>
    [Description("DVB-S2X")]
    DvbS2X = 0x00000800,
    /// <summary>
    /// First generation Digital Video Broadcast standard for terrestrial broadcast.
    /// </summary>
    [Description("DVB-T")]
    DvbT = 0x00001000,
    /// <summary>
    /// Second generation Digital Video Broadcast standard for terrestrial broadcast.
    /// </summary>
    [Description("DVB-T2")]
    DvbT2 = 0x00002000,
    /// <summary>
    /// Advanced Television Systems Committee standard for terrestrial broadcast.
    /// </summary>
    [Description("ATSC")]
    Atsc = 0x00004000,
    /// <summary>
    /// Society of Cable and Telecommunications Engineers standard for cable broadcast.
    /// </summary>
    [Description("SCTE")]
    Scte = 0x00008000,
    /// <summary>
    /// Integrated Services Digital Broadcasting standard for cable broadcast.
    /// </summary>
    [Description("ISDB-C")]
    IsdbC = 0x00010000,
    /// <summary>
    /// Integrated Services Digital Broadcasting standard for satellite broadcast.
    /// </summary>
    [Description("ISDB-S")]
    IsdbS = 0x00020000,
    /// <summary>
    /// Integrated Services Digital Broadcasting standard for terrestrial broadcast.
    /// </summary>
    [Description("ISDB-T")]
    IsdbT = 0x00040000,
    /// <summary>
    /// Non-standardised turbo-FEC satellite broadcast.
    /// </summary>
    [Description("Turbo FEC")]
    SatelliteTurboFec = 0x00080000,
    /// <summary>
    /// Motorola DigiCipher 2 (DC II) standard for satellite and cable broadcast.
    /// </summary>
    [Description("DigiCipher 2")]
    DigiCipher2 = 0x00100000,
    /// <summary>
    /// DirecTV standard for satellite broadcast.
    /// </summary>
    [Description("DirecTV")]
    DirecTvDss = 0x00200000,
    /// <summary>
    /// Digital Audio Broadcast standard.
    /// </summary>
    [Description("DAB")]
    Dab = 0x00400000,

    /// <summary>
    /// A mask for identifying analog broadcast standards.
    /// </summary>
    MaskAnalog = AmRadio | AnalogTelevision | FmRadio,
    /// <summary>
    /// A mask for identifying digital broadcast standards.
    /// </summary>
    MaskDigital = ~MaskAnalog & ~ExternalInput,

    /// <summary>
    /// A mask for identifying Digital Video Broadcast standards.
    /// </summary>
    MaskDvb = DvbC | DvbC2 | DvbDsng | DvbIp | DvbS | DvbS2 | DvbS2Pro | DvbS2X | DvbT | DvbT2,
    /// <summary>
    /// A mask for identifying second generation Digital Video Broadcast standards.
    /// </summary>
    MaskDvb2 = DvbC2 | DvbS2 | DvbS2Pro | DvbS2X | DvbT2,
    /// <summary>
    /// A mask for identifying second generation Digital Video Broadcast standards for satellite broadcast.
    /// </summary>
    MaskDvbS2 = DvbS2 | DvbS2Pro | DvbS2X,
    /// <summary>
    /// A mask for identifying Integrated Services Digital Broadcasting broadcast standards.
    /// </summary>
    MaskIsdb = IsdbC | IsdbS | IsdbT,

    /// <summary>
    /// A mask for identifying broadcast standards that are applicable for cable transmission.
    /// </summary>
    MaskCable = AnalogTelevision | DvbC | DvbC2 | IsdbC | Scte,
    /// <summary>
    /// A mask for identifying broadcast standards that are applicable for satellite transmission.
    /// </summary>
    MaskSatellite = DigiCipher2 | DirecTvDss | DvbDsng | DvbS | DvbS2 | DvbS2Pro | DvbS2X | IsdbS | SatelliteTurboFec,
    /// <summary>
    /// A mask for identifying broadcast standards that are applicable for terrestrial transmission.
    /// </summary>
    MaskTerrestrial = AmRadio | AnalogTelevision | Atsc | Dab | FmRadio | DvbT | DvbT2 | IsdbT,

    /// <summary>
    /// A mask for identifying broadcast standards that utilise orthogonal frequency division multiplexing techniques.
    /// </summary>
    MaskOfdm = Dab | DvbC2 | DvbT | DvbT2 | IsdbT,
    /// <summary>
    /// A mask for identifying broadcast standards that utilise phase shift keying modulation techniques.
    /// </summary>
    MaskPsk = MaskSatellite,
    /// <summary>
    /// A mask for identifying broadcast standards that utilise quadrature amplitude modulation techniques.
    /// </summary>
    MaskQam = DvbC | IsdbC | Scte,

    /// <summary>
    /// A mask for identifying broadcast standards that can be used in conjunction with ATSC and/or SCTE service information standards.
    /// </summary>
    MaskAtscScteSi = Atsc | Scte,
    /// <summary>
    /// A mask for identifying broadcast standards that can be used in conjunction with the Digital Video Broadcast service information standard.
    /// </summary>
    MaskDvbSi = MaskDvb | MaskIsdb | SatelliteTurboFec,
    /// <summary>
    /// A mask for identifying broadcast standards that can be used in conjunction with the Freesat service information standard.
    /// </summary>
    MaskFreesatSi = DvbS | DvbS2,
    /// <summary>
    /// A mask for identifying broadcast standards that can be used in conjunction with the MPEG 2 transport stream program and service information standard.
    /// </summary>
    MaskMpeg2TsSi = MaskAtscScteSi | MaskDvbSi | DigiCipher2,
    /// <summary>
    /// A mask for identifying broadcast standards that can be used in conjunction with the OpenTV service information standard.
    /// </summary>
    MaskOpenTvSi = DvbC | DvbC2 | DvbS | DvbS2 | DvbS2Pro | DvbS2X | DvbT | DvbT2
  }
}
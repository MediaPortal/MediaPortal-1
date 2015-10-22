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

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Dvb.Enum
{
  /// <summary>
  /// DVB service types. See EN 300 468 table 81.
  /// </summary>
  public enum ServiceType
  {
    // (0x00 reserved)

    /// <summary>
    /// digital television service
    /// </summary>
    DigitalTelevision = 0x01,
    /// <summary>
    /// digital radio sound service
    /// </summary>
    DigitalRadio,
    /// <summary>
    /// teletext service
    /// </summary>
    Teletext,
    /// <summary>
    /// near video on demand reference service
    /// </summary>
    NvodReference,
    /// <summary>
    /// near video on demand time-shifted service
    /// </summary>
    NvodTimeShifted,
    /// <summary>
    /// mosaic service
    /// </summary>
    Mosaic,
    /// <summary>
    /// frequency modulated radio service
    /// </summary>
    FmRadio,
    /// <summary>
    /// DVB system renewability messages service
    /// </summary>
    DvbSrm,           // 0x08

    // (0x09 reserved)

    /// <summary>
    /// advanced codec digital radio sound service
    /// </summary>
    AdvancedCodecDigitalRadio = 0x0a,
    /// <summary>
    /// advanced codec mosaic service
    /// </summary>
    AdvancedCodecMosaic,
    /// <summary>
    /// data broadcast service
    /// </summary>
    DataBroadcast,    // 0x0c

    // (0x0d reserved for common interface use)

    /// <summary>
    /// return channel via satellite map
    /// </summary>
    RcsMap = 0x0e,

    /// <summary>
    /// return channel via satellite forward link signalling
    /// </summary>
    RcsFls,
    /// <summary>
    /// DVB multimedia home platform service
    /// </summary>
    DvbMhp,           // 0x10
    /// <summary>
    /// MPEG 2 high definition digital television service
    /// </summary>
    Mpeg2HdDigitalTelevision,

    // (0x12 to 0x15 reserved)

    /// <summary>
    /// advanced codec standard definition digital television service
    /// </summary>
    AdvancedCodecSdDigitalTelevision = 0x16,
    /// <summary>
    /// advanced codec standard definition near video on demand time-shifted service
    /// </summary>
    AdvancedCodecSdNvodTimeShifted,
    /// <summary>
    /// advanced codec standard definition near video on demand reference service
    /// </summary>
    AdvancedCodecSdNvodReference,
    /// <summary>
    /// advanced codec high definition digital television
    /// </summary>
    AdvancedCodecHdDigitalTelevision,
    /// <summary>
    /// advanced codec high definition near video on demand time-shifted service
    /// </summary>
    AdvancedCodecHdNvodTimeShifted,
    /// <summary>
    /// advanced codec high definition near video on demand reference service
    /// </summary>
    AdvancedCodecHdNvodReference,
    /// <summary>
    /// advanced codec frame compatible plano-stereoscopic high definition digital television service
    /// </summary>
    AdvancedCodecFrameCompatiblePlanoStereoscopicHdDigitalTelevision,
    /// <summary>
    /// advanced codec frame compatible plano-stereoscopic high definition near video on demand time-shifted service
    /// </summary>
    AdvancedCodecFrameCompatiblePlanoStereoscopicHdNvodTimeShifted,
    /// <summary>
    /// advanced codec frame compatible plano-stereoscopic high definition near video on demand reference service
    /// </summary>
    AdvancedCodecFrameCompatiblePlanoStereoscopicHdNvodReference,
    /// <summary>
    /// HEVC digital television service
    /// </summary>
    HevcDigitalTelevision,  // 0x1f

    // (0x20 to 0x7f reserved)
    // (0x80 to 0xfe user defined)

    /// <summary>
    /// Sky Germany portal service (also known as linked or option services)
    /// </summary>
    SkyGermanyOptionChannel = 0xd3

    // (0xff reserved)
  }
}
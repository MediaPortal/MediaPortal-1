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

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Dvb.Enum
{
  /// <summary>
  /// DVB service types. See EN 300 468 table 87.
  /// </summary>
  internal enum ServiceType : byte
  {
    // (0x00 reserved)

    /// <summary>
    /// digital television service
    /// </summary>
    DigitalTelevision = 0x01,
    /// <summary>
    /// digital radio sound service
    /// </summary>
    DigitalRadio = 0x02,
    /// <summary>
    /// teletext service
    /// </summary>
    Teletext = 0x03,
    /// <summary>
    /// near video on demand reference service
    /// </summary>
    NvodReference = 0x04,
    /// <summary>
    /// near video on demand time-shifted service
    /// </summary>
    NvodTimeShifted = 0x05,
    /// <summary>
    /// mosaic service
    /// </summary>
    Mosaic = 0x06,
    /// <summary>
    /// frequency modulated radio service
    /// </summary>
    FmRadio = 0x07,
    /// <summary>
    /// DVB system renewability messages service
    /// </summary>
    DvbSrm = 0x08,

    // (0x09 reserved)

    /// <summary>
    /// advanced codec digital radio sound service
    /// </summary>
    AdvancedCodecDigitalRadio = 0x0a,
    /// <summary>
    /// advanced codec mosaic service
    /// </summary>
    AdvancedCodecMosaic = 0x0b,
    /// <summary>
    /// data broadcast service
    /// </summary>
    DataBroadcast = 0x0c,

    // (0x0d reserved for common interface use)

    /// <summary>
    /// return channel via satellite map
    /// </summary>
    RcsMap = 0x0e,

    /// <summary>
    /// return channel via satellite forward link signalling
    /// </summary>
    RcsFls = 0x0f,
    /// <summary>
    /// DVB multimedia home platform service
    /// </summary>
    DvbMhp = 0x10,
    /// <summary>
    /// MPEG 2 high definition digital television service
    /// </summary>
    Mpeg2HdDigitalTelevision = 0x11,

    // (0x12 to 0x15 reserved)

    /// <summary>
    /// advanced codec standard definition digital television service
    /// </summary>
    AdvancedCodecSdDigitalTelevision = 0x16,
    /// <summary>
    /// advanced codec standard definition near video on demand time-shifted service
    /// </summary>
    AdvancedCodecSdNvodTimeShifted = 0x17,
    /// <summary>
    /// advanced codec standard definition near video on demand reference service
    /// </summary>
    AdvancedCodecSdNvodReference = 0x18,
    /// <summary>
    /// advanced codec high definition digital television
    /// </summary>
    AdvancedCodecHdDigitalTelevision = 0x19,
    /// <summary>
    /// advanced codec high definition near video on demand time-shifted service
    /// </summary>
    AdvancedCodecHdNvodTimeShifted = 0x1a,
    /// <summary>
    /// advanced codec high definition near video on demand reference service
    /// </summary>
    AdvancedCodecHdNvodReference = 0x1b,
    /// <summary>
    /// advanced codec frame compatible plano-stereoscopic high definition digital television service
    /// </summary>
    AdvancedCodecFrameCompatiblePlanoStereoscopicHdDigitalTelevision = 0x1c,
    /// <summary>
    /// advanced codec frame compatible plano-stereoscopic high definition near video on demand time-shifted service
    /// </summary>
    AdvancedCodecFrameCompatiblePlanoStereoscopicHdNvodTimeShifted = 0x1d,
    /// <summary>
    /// advanced codec frame compatible plano-stereoscopic high definition near video on demand reference service
    /// </summary>
    AdvancedCodecFrameCompatiblePlanoStereoscopicHdNvodReference = 0x1e,
    /// <summary>
    /// HEVC digital television service
    /// </summary>
    HevcDigitalTelevision = 0x1f,

    // (0x20 to 0x7f reserved)
    // (0x80 to 0xfe user defined)

    /// <summary>
    /// Sky Germany portal service (also known as linked or option services)
    /// </summary>
    SkyGermanyOptionChannel = 0xd3

    // (0xff reserved)
  }
}
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
  /// DVB extension descriptor tags.
  /// </summary>
  public enum ExtensionDescriptorTag
  {
    /// <summary>
    /// image icon descriptor
    /// </summary>
    ImageIcon = 0x00,
    /// <summary>
    /// content protection copy management delivery signalling descriptor
    /// </summary>
    CpcmDeliverySignalling = 0x01,
    /// <summary>
    /// content protection descriptor
    /// </summary>
    Cp = 0x02,
    /// <summary>
    /// content protection identifier descriptor
    /// </summary>
    CpIdentifier = 0x03,
    /// <summary>
    /// T2 delivery system descriptor
    /// </summary>
    T2DeliverySystem = 0x04,
    /// <summary>
    /// SH delivery system descriptor
    /// </summary>
    ShDeliverySystem = 0x05,
    /// <summary>
    /// supplementary audio descriptor
    /// </summary>
    SupplementaryAudio = 0x06,
    /// <summary>
    /// network change notify descriptor
    /// </summary>
    NetworkChangeNotify = 0x07,
    /// <summary>
    /// message descriptor
    /// </summary>
    Message = 0x08,
    /// <summary>
    /// target region descriptor
    /// </summary>
    TargetRegion = 0x09,
    /// <summary>
    /// target region name descriptor
    /// </summary>
    TargetRegionName = 0x0a,
    /// <summary>
    /// service relocated descriptor
    /// </summary>
    ServiceRelocated = 0x0b,
    /// <summary>
    /// XAIT PID descriptor
    /// </summary>
    XaitPid = 0x0c,
    /// <summary>
    /// C2 delivery system descriptor
    /// </summary>
    C2DeliverySystem = 0x0d,
    /// <summary>
    /// DTS HD audio stream descriptor
    /// </summary>
    DtsHdAudioStream = 0x0e,
    /// <summary>
    /// DTS neural descriptor
    /// </summary>
    DtsNeural = 0x0f,
    /// <summary>
    /// video depth range descriptor
    /// </summary>
    VideoDepthRange = 0x10,
    /// <summary>
    /// T2 modulator interface descriptor
    /// </summary>
    T2mi = 0x11,

    // 0x12 reserved

    /// <summary>
    /// URI linkage descriptor
    /// </summary>
    UriLinkage = 0x13,
    /// <summary>
    /// broadcast identifier ancillary data descriptor
    /// </summary>
    BciAncillaryData = 0x14,
    /// <summary>
    /// AC-4 descriptor
    /// </summary>
    Ac4 = 0x15,
    /// <summary>
    /// C2 bundle delivery system descriptor
    /// </summary>
    C2BundleDeliverySystem = 0x16
  }
}
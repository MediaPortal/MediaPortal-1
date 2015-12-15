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
  /// DVB MMI message tags.
  /// </summary>
  public enum MmiTag
  {
    /// <summary>
    /// Unknown tag.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Profile enquiry.
    /// </summary>
    ProfileEnquiry = 0x9f8010,
    /// <summary>
    /// Profile.
    /// </summary>
    Profile = 0x9f8011,
    /// <summary>
    /// Profile change.
    /// </summary>
    ProfileChange = 0x9f8012,

    /// <summary>
    /// Application information enquiry.
    /// </summary>
    ApplicationInfoEnquiry = 0x9f8020,
    /// <summary>
    /// Application information.
    /// </summary>
    ApplicationInfo = 0x9f8021,
    /// <summary>
    /// Enter menu.
    /// </summary>
    EnterMenu = 0x9f8022,

    /// <summary>
    /// Conditional access information enquiry.
    /// </summary>
    ConditionalAccessInfoEnquiry = 0x9f8030,
    /// <summary>
    /// Conditional access information.
    /// </summary>
    ConditionalAccessInfo = 0x9f8031,
    /// <summary>
    /// Conditional access information program map table.
    /// </summary>
    ConditionalAccessPmt = 0x9f8032,
    /// <summary>
    /// Conditional access information program map table response.
    /// </summary>
    ConditionalAccessPmtResponse = 0x9f8033,

    /// <summary>
    /// Tune.
    /// </summary>
    Tune = 0x9f8400,
    /// <summary>
    /// Replace.
    /// </summary>
    Replace = 0x9f8401,
    /// <summary>
    /// Clear replace.
    /// </summary>
    ClearReplace = 0x9f8402,
    /// <summary>
    /// Ask release.
    /// </summary>
    AskRelease = 0x9f8403,

    /// <summary>
    /// Date/time enquiry.
    /// </summary>
    DateTimeEnquiry = 0x9f8440,
    /// <summary>
    /// Date/time.
    /// </summary>
    DateTime = 0x9f8441,

    /// <summary>
    /// Close man-machine interface.
    /// </summary>
    CloseMmi = 0x9f8800,
    /// <summary>
    /// Display control.
    /// </summary>
    DisplayControl = 0x9f8801,
    /// <summary>
    /// Display reply.
    /// </summary>
    DisplayReply = 0x9f8802,
    /// <summary>
    /// Text - last.
    /// </summary>
    TextLast = 0x9f8803,
    /// <summary>
    /// Text - more.
    /// </summary>
    TextMore = 0x9f8804,
    /// <summary>
    /// Keypad control.
    /// </summary>
    KeypadControl = 0x9f8805,
    /// <summary>
    /// Key press.
    /// </summary>
    KeyPress = 0x9f8806,
    /// <summary>
    /// Enquiry.
    /// </summary>
    Enquiry = 0x9f8807,
    /// <summary>
    /// Answer.
    /// </summary>
    Answer = 0x9f8808,
    /// <summary>
    /// Menu - last.
    /// </summary>
    MenuLast = 0x9f8809,
    /// <summary>
    /// Menu - more.
    /// </summary>
    MenuMore = 0x9f880a,
    /// <summary>
    /// Menu answer.
    /// </summary>
    MenuAnswer = 0x9f880b,
    /// <summary>
    /// List - last.
    /// </summary>
    ListLast = 0x9f880c,
    /// <summary>
    /// List - more.
    /// </summary>
    ListMore = 0x9f880d,
    /// <summary>
    /// Subtitle segment - last.
    /// </summary>
    SubtitleSegmentLast = 0x9f880e,
    /// <summary>
    /// Subtitle segment - more.
    /// </summary>
    SubtitleSegmentMore = 0x9f880f,
    /// <summary>
    /// Display message.
    /// </summary>
    DisplayMessage = 0x9f8810,
    /// <summary>
    /// Scene end mark.
    /// </summary>
    SceneEndMark = 0x9f8811,
    /// <summary>
    /// Scene done.
    /// </summary>
    SceneDone = 0x9f8812,
    /// <summary>
    /// Scene control.
    /// </summary>
    SceneControl = 0x9f8813,
    /// <summary>
    /// Subtitle download - last.
    /// </summary>
    SubtitleDownloadLast = 0x9f8814,
    /// <summary>
    /// Subtitle download - more.
    /// </summary>
    SubtitleDownloadMore = 0x9f8815,
    /// <summary>
    /// Flush download.
    /// </summary>
    FlushDownload = 0x9f8816,
    /// <summary>
    /// Download reply.
    /// </summary>
    DownloadReply = 0x9f8817,

    /// <summary>
    /// Communication command.
    /// </summary>
    CommsCommand = 0x9f8c00,
    /// <summary>
    /// Connection descriptor.
    /// </summary>
    ConnectionDescriptor = 0x9f8c01,
    /// <summary>
    /// Communication reply.
    /// </summary>
    CommsReply = 0x9f8c02,
    /// <summary>
    /// Communication send - last.
    /// </summary>
    CommsSendLast = 0x9f8c03,
    /// <summary>
    /// Communication send - more.
    /// </summary>
    CommsSendMore = 0x9f8c04,
    /// <summary>
    /// Communication receive - last.
    /// </summary>
    CommsReceiveLast = 0x9f8c05,
    /// <summary>
    /// Communication receive - more.
    /// </summary>
    CommsReceiveMore = 0x9f8c06
  }
}
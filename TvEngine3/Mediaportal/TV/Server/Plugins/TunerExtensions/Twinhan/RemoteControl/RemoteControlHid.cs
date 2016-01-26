#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using DirectShowLib;
using Mediaportal.TV.Server.Plugins.TunerExtension.Twinhan.Enum;
using Mediaportal.TV.Server.Plugins.TunerExtension.Twinhan.RemoteControl.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils;
using Microsoft.Win32;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.Twinhan.RemoteControl
{
  /// <summary>
  /// A class for handling HID remote controls for Twinhan tuners, including clones from TerraTec,
  /// TechniSat and Digital Rise.
  /// </summary>
  internal class RemoteControlHid : ITwinhanRemoteControl
  {
    #region enums

    // http://uploads.bettershopping.eu/ART00784-1-web-big.jpg
    // variant: http://digitalnow.com.au/images/ProRemote.jpg (differences: home, second row, labels under bottom row)
    private enum TwinhanRemoteScanCodeNew
    {
      Power = 0,
      One,
      Two,
      Three,
      Four,
      Five,
      Six,
      Seven,
      Eight,
      Nine,
      Cancel,
      Clear,
      Zero,
      Favourites, // 13

      Up = 16,
      Down,
      Left,
      Right,
      Okay,
      Back,
      Tab,  // 22

      VolumeUp = 26,
      VolumeDown,
      ChannelDown,
      ChannelUp,

      Record = 64,
      Play,
      Pause,
      Stop,
      Rewind,
      FastForward,
      SkipBack, // 70
      SkipForward,
      Screenshot,
      Audio,              // text: SAP
      PictureInPicture,
      FullScreen,
      Subtitles,
      Mute,
      StereoMono,         // text: L/R
      Sleep,              // text: hibernate
      Source, // 80       // text: A/V
      Recall,
      ZoomIn,
      ZoomOut,
      Red,
      Green,
      Yellow,
      Blue, // 87

      Recordings = 92,    // text: record list
      Epg,                // text: info/EPG
      Preview,
      Teletext
    }

    // http://www.sadoun.com/Sat/Products/TwinHan/StarBox-2.jpg
    private enum TwinhanRemoteScanCodeOld
    {
      Tab = 0,
      Two,
      Down,               // overlay: channel down
      One,
      Recordings,         // text: record list
      Up,                 // overlay: channel up
      Three,  // 6

      Four = 9,
      Left = 10,          // overlay: volume down

      Cancel = 12,
      Seven,
      Recall,             // icon: circular arrow
      Teletext,
      Mute,
      Record,
      FastForward,  // 18

      Zero = 21,
      Power,
      Favourites,

      Eight = 25,
      Stop,
      Nine,
      Epg,
      Five,
      Right,              // overlay: volume up
      Six,  // 31

      Rewind = 64,
      Preview = 72,
      Pause = 76,         // text: time shift/pause
      FullScreen = 77,    // [q: German keyboard Y swapped with Z]
      Play = 79,          // overlay: okay
      Screenshot = 84     // text: capture
    }

    private enum TwinhanIrStandard : uint
    {
      Rc5 = 0,
      Nec
    }

    private enum TwinhanRemoteControlType
    {
      Old,
      New
    }

    private enum TwinhanRemoteControlMapping : uint
    {
      DtvDvb = 0,
      Cyberlink,
      InterVideo,
      Mce,
      DtvDvbWmInput,
      Custom,

      DigitalNow = 0x10,

      TerraTec110msRepeat = 0xfb,
      TerraTec220msRepeat,
      TerraTec110msSelectiveRepeat,   // selective = ch+/ch-/vol+/vol-/up/down/left/right
      TerraTec220msSelectiveRepeat,
      Disabled = 0xffff
    }

    #endregion

    #region structs

    [StructLayout(LayoutKind.Sequential)]
    private struct HidRemoteControlConfig   // RC_CONFIG
    {
      public TwinhanIrStandard IrStandard;
      public uint IrSysCodeCheck1;
      public uint IrSysCodeCheck2;
      public TwinhanRemoteControlMapping Mapping;
    }

    #endregion

    #region constants

    private static readonly int HID_REMOTE_CONTROL_CONFIG_SIZE = Marshal.SizeOf(typeof(HidRemoteControlConfig));  // 16
    private static readonly Regex REGEX_SCANCODE_REGVAL = new Regex(@"^RC_Scancode_([0-9A-Fa-f]{2})$", RegexOptions.IgnoreCase);

    #region mappings

    // These are reverse maps: generated key press => scan code.
    // They enable us to figure out which remote button was actually pressed.

    private static readonly IDictionary<int, byte> MAPPING_DTV_DVB = new Dictionary<int, byte>(62)
    {
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_TAB,                                                             0x00 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_2,                                                               0x01 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_NEXT,                                                            0x02 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_1,                                                               0x03 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_L,                                                               0x04 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_PRIOR,                                                           0x05 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_3,                                                               0x06 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_S | (int)VirtualKeyModifier.Control,                             0x07 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_D | (int)VirtualKeyModifier.Control,                             0x08 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_4,                                                               0x09 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_DOWN | (int)VirtualKeyModifier.Shift,                            0x0a },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_ESCAPE,                                                          0x0c },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_7,                                                               0x0d },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_C,                                                               0x0e },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_A,                                                               0x0f },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_M,                                                               0x10 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_HOME,                                                            0x11 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_N,                                                               0x12 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_BACK,                                                            0x13 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_N | (int)VirtualKeyModifier.Control,                             0x14 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_0,                                                               0x15 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_F6 | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Alt), 0x16 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_V,                                                               0x17 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_R | (int)VirtualKeyModifier.Shift,                               0x18 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_8,                                                               0x19 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_END,                                                             0x1a },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_9,                                                               0x1b },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_E,                                                               0x1c },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_5,                                                               0x1d },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_UP | (int)VirtualKeyModifier.Shift,                              0x1e },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_6,                                                               0x1f },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_I,                                                               0x40 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_R | (int)VirtualKeyModifier.Control,                             0x41 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_K | (int)VirtualKeyModifier.Control,                             0x42 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_T | (int)VirtualKeyModifier.Control,                             0x43 },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_OEM_PLUS | (int)VirtualKeyModifier.Control,                      0x45 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_OEM_MINUS | (int)VirtualKeyModifier.Control,                     0x46 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_P | (int)VirtualKeyModifier.Control,                             0x47 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_K,                                                               0x48 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_V | (int)VirtualKeyModifier.Control,                             0x49 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_DELETE,                                                          0x4a },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_UP,                                                              0x4b },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_T,                                                               0x4c },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_Z,                                                               0x4d },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_LEFT,                                                            0x4e },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_RETURN,                                                          0x4f },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_L | (int)VirtualKeyModifier.Control,                             0x50 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_DOWN,                                                            0x51 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_RIGHT,                                                           0x52 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_G | (int)VirtualKeyModifier.Shift,                               0x53 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_P,                                                               0x54 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_T | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Alt),  0x55 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_1 | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Alt),  0x56 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_M | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Alt),  0x57 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_D | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Alt),  0x58 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_2 | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Alt),  0x59 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_F | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Alt),  0x5a },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_H | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Alt),  0x5b },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_P | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Alt),  0x5c },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_V | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Alt),  0x5d },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_Y | (int)VirtualKeyModifier.Shift,                               0x5e },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_B | (int)VirtualKeyModifier.Shift,                               0x5f }
    };

    private static readonly IDictionary<int, byte> MAPPING_CYBERLINK = new Dictionary<int, byte>(51)
    {
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_TAB,               0x00 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_2,                 0x01 },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.ChannelDecrement,             0x02 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_1,                 0x03 },

      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.ChannelIncrement,             0x05 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_3,                 0x06 },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_4,                 0x09 },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.VolumeDecrement,              0x0a },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_ESCAPE,            0x0c },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_7,                 0x0d },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_L | (int)VirtualKeyModifier.Control, 0x0e },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_T | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Shift), 0x0f },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.Mute,                         0x10 },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.Record,                       0x11 },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.FastForward,                  0x12 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_BACK,              0x13 },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.Play,                         0x14 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_0,                 0x15 },
      { (int)TwinhanUsageType.Raw      | 0x16,                                               0x16 },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_8,                 0x19 },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.Stop,                         0x1a },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_9,                 0x1b },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_G | (int)VirtualKeyModifier.Control, 0x1c },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_5,                 0x1d },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.VolumeIncrement,              0x1e },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_6,                 0x1f },

      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.Rewind,                       0x40 },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.ScanPreviousTrack,            0x41 },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.ScanNextTrack,                0x42 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_S | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Shift), 0x43 },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_PRIOR,             0x45 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_NEXT,              0x46 },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_DELETE,            0x4a },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_UP,                0x4b },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.Pause,                        0x4c },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_RETURN | (int)VirtualKeyModifier.Alt, 0x4d },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_LEFT,              0x4e },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_RETURN,            0x4f },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_C | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Shift), 0x50 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_DOWN,              0x51 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_RIGHT,             0x52 },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_Z | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Shift), 0x54 },
      { (int)TwinhanUsageType.Raw      | 0x55,                                               0x55 },
      { (int)TwinhanUsageType.Raw      | 0x56,                                               0x56 },
      { (int)TwinhanUsageType.Raw      | 0x57,                                               0x57 },

      { (int)TwinhanUsageType.Raw      | 0x59,                                               0x59 },
      { (int)TwinhanUsageType.Mce      | (int)MceRemoteUsage.Undefined,                      0x5a },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_HOME | (int)VirtualKeyModifier.Alt, 0x5b },
      { (int)TwinhanUsageType.Raw      | 0x5c,                                               0x5c },
      { (int)TwinhanUsageType.Raw      | 0x5d,                                               0x5d }
    };

    private static readonly IDictionary<int, byte> MAPPING_INTERVIDEO = new Dictionary<int, byte>(30)
    {
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_TAB,               0x00 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_2,                 0x01 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_NEXT,              0x02 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_1,                 0x03 },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_PRIOR,             0x05 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_3,                 0x06 },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_4,                 0x09 },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.VolumeDecrement,              0x0a },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_7,                 0x0d },

      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.Mute,                         0x10 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_R | (int)VirtualKeyModifier.Control,                               0x11 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_F | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Shift),  0x12 },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_P | (int)VirtualKeyModifier.Control,                               0x14 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_0,                 0x15 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_F4 | (int)VirtualKeyModifier.Alt,                                  0x16 },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_8,                 0x19 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_S | (int)VirtualKeyModifier.Control,                               0x1a },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_9,                 0x1b },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_5,                 0x1d },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.VolumeIncrement,              0x1e },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_6,                 0x1f },

      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.ScanPreviousTrack,            0x41 },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.ScanNextTrack,                0x42 },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_RETURN,            0x4f },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_2 | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Shift | VirtualKeyModifier.Alt),     0x55 },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_3 | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Shift | VirtualKeyModifier.Alt),     0x57 },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_1 | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Shift | VirtualKeyModifier.Alt),     0x59 },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_HOME | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Shift | VirtualKeyModifier.Alt),  0x5b },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_4 | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Shift | VirtualKeyModifier.Alt),     0x5c },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_5 | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Shift | VirtualKeyModifier.Alt),     0x5d }
    };

    private static readonly IDictionary<int, byte> MAPPING_MCE = new Dictionary<int, byte>(44)
    {
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_2,                 0x01 },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.ChannelDecrement,             0x02 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_1,                 0x03 },

      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.ChannelIncrement,             0x05 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_3,                 0x06 },

      { (int)TwinhanUsageType.Mce      | (int)MceRemoteUsage.DvdAudio,                       0x08 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_4,                 0x09 },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.VolumeDecrement,              0x0a },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_7,                 0x0d },
      { (int)TwinhanUsageType.Mce      | (int)MceRemoteUsage.DvdAngle,                       0x0e },

      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.Mute,                         0x10 },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.Record,                       0x11 },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.FastForward,                  0x12 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_BACK,              0x13 },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.Play,                         0x14 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_0,                 0x15 },
      { (int)TwinhanUsageType.Raw      | 0x16,                                               0x16 },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_8,                 0x19 },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.Stop,                         0x1a },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_9,                 0x1b },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_5,                 0x1d },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.VolumeIncrement,              0x1e },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_6,                 0x1f },

      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.Rewind,                       0x40 },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.ScanPreviousTrack,            0x41 },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.ScanNextTrack,                0x42 },
      { (int)TwinhanUsageType.Raw      | 0x43,                                               0x43 },

      { (int)TwinhanUsageType.Mce      | (int)MceRemoteUsage.RecordedTv,                     0x47 },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_ESCAPE,            0x4a },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_UP,                0x4b },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.Pause,                        0x4c },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.MediaSelectProgramGuide,      0x4d },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_LEFT,              0x4e },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_RETURN,            0x4f },
      { (int)TwinhanUsageType.Mce      | (int)MceRemoteUsage.LiveTv,                         0x50 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_DOWN,              0x51 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_RIGHT,             0x52 },

      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.ApplicationControlProperties, 0x54 },
      { (int)TwinhanUsageType.Raw      | 0x55,                                               0x55 },

      { (int)TwinhanUsageType.Raw      | 0x57,                                               0x57 },

      { (int)TwinhanUsageType.Raw      | 0x59,                                               0x59 },

      { (int)TwinhanUsageType.Raw      | 0x5c,                                               0x5c },
      { (int)TwinhanUsageType.Raw      | 0x5d,                                               0x5d }
    };

    private static readonly IDictionary<int, byte> MAPPING_DTV_DVB_WM_INPUT = new Dictionary<int, byte>(62)
    {
      { (int)TwinhanUsageType.Ascii    | 0x3a,                                               0x00 },
      { (int)TwinhanUsageType.Ascii    | 0x32,                                               0x01 },
      { (int)TwinhanUsageType.Raw      | 0x02,                                               0x02 },
      { (int)TwinhanUsageType.Ascii    | 0x31,                                               0x03 },
      { (int)TwinhanUsageType.Ascii    | 0x0d,                                               0x04 },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.ChannelIncrement,             0x05 },
      { (int)TwinhanUsageType.Ascii    | 0x33,                                               0x06 },

      { (int)TwinhanUsageType.Mce      | (int)MceRemoteUsage.DvdAudio,                       0x08 },
      { (int)TwinhanUsageType.Ascii    | 0x34,                                               0x09 },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.VolumeDecrement,              0x0a },

      { (int)TwinhanUsageType.Ascii    | 0x41,                                               0x0c },
      { (int)TwinhanUsageType.Ascii    | 0x37,                                               0x0d },
      { (int)TwinhanUsageType.Mce      | (int)MceRemoteUsage.DvdAngle,                       0x0e },
      { (int)TwinhanUsageType.Ascii    | 0x0f,                                               0x0f },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.Mute,                         0x10 },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.Record,                       0x11 },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.FastForward,                  0x12 },
      { (int)TwinhanUsageType.Ascii    | 0x43,                                               0x13 },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.Play,                         0x14 },
      { (int)TwinhanUsageType.Ascii    | 0x30,                                               0x15 },
      { (int)TwinhanUsageType.Ascii    | 0x13,                                               0x16 },
      { (int)TwinhanUsageType.Ascii    | 0x0c,                                               0x17 },
      { (int)TwinhanUsageType.Raw      | 0x18,                                               0x18 },
      { (int)TwinhanUsageType.Ascii    | 0x38,                                               0x19 },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.Stop,                         0x1a },
      { (int)TwinhanUsageType.Ascii    | 0x39,                                               0x1b },
      { (int)TwinhanUsageType.Ascii    | 0x11,                                               0x1c },
      { (int)TwinhanUsageType.Ascii    | 0x35,                                               0x1d },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.VolumeIncrement,              0x1e },
      { (int)TwinhanUsageType.Ascii    | 0x36,                                               0x1f },

      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.Rewind,                       0x40 },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.ScanPreviousTrack,            0x41 },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.ScanNextTrack,                0x42 },
      { (int)TwinhanUsageType.Raw      | 0x43,                                               0x43 },

      { (int)TwinhanUsageType.Ascii    | 0x07,                                               0x45 },
      { (int)TwinhanUsageType.Ascii    | 0x0a,                                               0x46 },
      { (int)TwinhanUsageType.Mce      | (int)MceRemoteUsage.RecordedTv,                     0x47 },
      { (int)TwinhanUsageType.Ascii    | 0x0e,                                               0x48 },
      { (int)TwinhanUsageType.Ascii    | 0x0b,                                               0x49 },
      { (int)TwinhanUsageType.Ascii    | 0x3b,                                               0x4a },
      { (int)TwinhanUsageType.Ascii    | 0x3c,                                               0x4b },
      { (int)TwinhanUsageType.Consumer | (int)HidConsumerUsage.Pause,                        0x4c },
      { (int)TwinhanUsageType.Ascii    | 0x06,                                               0x4d },
      { (int)TwinhanUsageType.Ascii    | 0x3f,                                               0x4e },
      { (int)TwinhanUsageType.Ascii    | 0x42,                                               0x4f },
      { (int)TwinhanUsageType.Ascii    | 0x05,                                               0x50 },
      { (int)TwinhanUsageType.Ascii    | 0x3d,                                               0x51 },
      { (int)TwinhanUsageType.Ascii    | 0x3e,                                               0x52 },
      { (int)TwinhanUsageType.Raw      | 0x53,                                               0x53 },
      { (int)TwinhanUsageType.Ascii    | 0x03,                                               0x54 },
      { (int)TwinhanUsageType.Raw      | 0x55,                                               0x55 },
      { (int)TwinhanUsageType.Raw      | 0x56,                                               0x56 },
      { (int)TwinhanUsageType.Raw      | 0x57,                                               0x57 },
      { (int)TwinhanUsageType.Ascii    | 0x12,                                               0x58 },
      { (int)TwinhanUsageType.Raw      | 0x59,                                               0x59 },
      { (int)TwinhanUsageType.Mce      | (int)MceRemoteUsage.Undefined,                      0x5a },
      { (int)TwinhanUsageType.Raw      | 0x5b,                                               0x5b },
      { (int)TwinhanUsageType.Raw      | 0x5c,                                               0x5c },
      { (int)TwinhanUsageType.Raw      | 0x5d,                                               0x5d },
      { (int)TwinhanUsageType.Raw      | 0x5e,                                               0x5e },
      { (int)TwinhanUsageType.Raw      | 0x5f,                                               0x5f }
    };

    private static readonly IDictionary<int, byte> MAPPING_DNTV = new Dictionary<int, byte>(48)
    {
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_TAB,                                                             0x00 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_2,                                                               0x01 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_D | (int)VirtualKeyModifier.Control,                             0x02 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_1,                                                               0x03 },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_U | (int)VirtualKeyModifier.Control,                             0x05 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_3,                                                               0x06 },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_4,                                                               0x09 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_X | (int)VirtualKeyModifier.Control,                             0x0a },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_F4 | (int)VirtualKeyModifier.Alt,                                0x0c },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_7,                                                               0x0d },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_B | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Alt),  0x0e },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_Y | (int)VirtualKeyModifier.Control,                             0x10 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_R | (int)VirtualKeyModifier.Control,                             0x11 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_F | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Alt),  0x12 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_BACK,                                                            0x13 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_P | (int)VirtualKeyModifier.Control,                             0x14 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_0,                                                               0x15 },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_H | (int)VirtualKeyModifier.Control,                             0x17 },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_8,                                                               0x19 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_S | (int)VirtualKeyModifier.Control,                             0x1a },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_9,                                                               0x1b },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_C | (int)VirtualKeyModifier.Control,                             0x1c },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_5,                                                               0x1d },
      // 0x1e is mapped to the same as 0x05 and 0x42
      //{ (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_U | (int)VirtualKeyModifier.Control,                             0x1e },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_6,                                                               0x1f },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_D | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Alt),  0x40 },
      // 0x40 is mapped to the same as 0x02 and 0x56
      //{ (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_D | (int)VirtualKeyModifier.Control,                             0x41 },
      // 0x42 is mapped to the same as 0x05 and 0x1e
      //{ (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_U | (int)VirtualKeyModifier.Control,                             0x42 },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_Z | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Alt),  0x45 },
      // 0x46 is mapped to the same as 0x45
      //{ (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_Z | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Alt),  0x46 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_F6,                                                              0x47 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_O | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Alt),  0x48 },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_DELETE,                                                          0x4a },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_UP,                                                              0x4b },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_E | (int)VirtualKeyModifier.Control, 0x4c },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_E | (int)(VirtualKeyModifier.Control | VirtualKeyModifier.Alt),  0x4d },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_LEFT,                                                            0x4e },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_RETURN,                                                          0x4f },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_F8,                                                              0x50 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_DOWN,                                                            0x51 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_RIGHT,                                                           0x52 },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_F7,                                                              0x54 },
      // 0x55 is mapped to the same as 0x17
      //{ (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_H | (int)VirtualKeyModifier.Control,                             0x55 },
      // 0x56 is mapped to the same as 0x02 and 0x41
      //{ (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_D | (int)VirtualKeyModifier.Control,                             0x56 },
      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_J | (int)VirtualKeyModifier.Control,                             0x57 },

      // 0x59 is mapped to the same as 0x11
      //{ (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_R | (int)VirtualKeyModifier.Control,                             0x59 },

      // 0x5b is mapped to the same as 0x0a
      //{ (int)UsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_X | (int)VirtualKeyModifier.Control,                             0x5b },

      { (int)TwinhanUsageType.Keyboard | (int)NativeMethods.VirtualKey.VK_F | (int)VirtualKeyModifier.Control,                             0x5d }
    };

    #endregion

    #endregion

    #region variables

    private static HashSet<string> _openProducts = new HashSet<string>();
    private static object _lockListenerNotifier = new object();
    private static HidListener _listener = null;
    private static SystemChangeNotifier _systemChangeNotifier = null;

    private bool _isInterfaceOpen = false;
    private string _tunerExternalId = string.Empty;
    private string _productInstanceId = null;
    private bool _isTerraTecDriver = false;

    private IKsPropertySet _propertySet = null;
    private IoControl _ioControl = null;

    private TwinhanRemoteControlType _remoteControlType;
    private TwinhanRemoteControlMapping _remoteControlMapping;
    // virtual key combination => scan code
    private IDictionary<int, byte> _customMappingTable = new Dictionary<int, byte>(64);

    private string _hidId = string.Empty;
    private object _lockDevices = new object();
    private IList<HumanInterfaceDevice> _devices = null;

    #endregion

    public RemoteControlHid(string productInstanceId, string tunerExternalId, IKsPropertySet propertySet, bool isTerraTecDriver)
    {
      _productInstanceId = productInstanceId;
      _tunerExternalId = tunerExternalId;
      _propertySet = propertySet;
      _isTerraTecDriver = isTerraTecDriver;
    }

    /// <summary>
    /// Open the remote control interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    public bool Open()
    {
      this.LogDebug("Twinhan HID RC: open interface, product instance ID = {0}, tuner external ID = {1}", _productInstanceId ?? "[null]", _tunerExternalId);

      if (_isInterfaceOpen)
      {
        this.LogWarn("Twinhan HID RC: interface is already open");
        return true;
      }
      if (_productInstanceId == null)
      {
        this.LogDebug("Twinhan HID RC: product instance identifier is null");
        return false;
      }
      if (_openProducts.Contains(_productInstanceId))
      {
        this.LogDebug("Twinhan HID RC: multi-tuner product remote control opened for other tuner");
        return true;
      }

      // Enable the HID driver. Note it is possible this has no effect until
      // tuner driver is restarted.
      _ioControl = new IoControl(_propertySet);
      IntPtr buffer = Marshal.AllocCoTaskMem(1);
      try
      {
        Marshal.WriteByte(buffer, 0, 1);
        int hr = _ioControl.Set(IoControlCode.HidRemoteControlEnable, buffer, 1);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          // HID not supported (???).
          this.LogDebug("Twinhan HID RC: failed to enable remote control, hr = 0x{0:x}", hr);
          return false;
        }
      }
      finally
      {
        Marshal.FreeCoTaskMem(buffer);
      }

      // Find the HIDs for the product instance associated with the tuner.
      string productInstanceHardwareId;
      uint productInstanceDevInst;
      if (!FindDeviceForTuner(_tunerExternalId, out productInstanceHardwareId, out productInstanceDevInst))
      {
        return false;
      }
      this.LogDebug("Twinhan HID RC: product hardware ID = {0}", productInstanceHardwareId);
      this.LogDebug("Twinhan HID RC: product dev inst    = {0}", productInstanceDevInst);

      string rootHidId;
      ICollection<string> hidIds;
      if (!FindHidsForDevice(productInstanceDevInst, out rootHidId, out hidIds))
      {
        return false;
      }
      this.LogDebug("Twinhan HID RC: root HID ID         = {0}", rootHidId);

      lock (_devices)
      {
        if (!LoadHids(hidIds, _isTerraTecDriver, out _devices))
        {
          return false;
        }
        this.LogDebug("Twinhan HID RC: child HID count     = {0}", _devices.Count);
      }

      // Load driver configuration.
      if (!ReadConfig(_ioControl, out _remoteControlType, out _remoteControlMapping))
      {
        return false;
      }
      this.LogDebug("Twinhan HID RC: remote control type = {0}", _remoteControlType);
      this.LogDebug("Twinhan HID RC: remote control map  = {0}", _remoteControlMapping);
      // Read the custom mapping table even if not using the custom mapping.
      // This enables the RC_Report key (which is relevant for non-custom
      // mappings) to be checked.
      _customMappingTable = ReadCustomMapping(rootHidId);

      lock (_lockDevices)
      {
        // Open the HIDs.
        foreach (HumanInterfaceDevice d in _devices)
        {
          if (!d.Open())
          {
            foreach (HumanInterfaceDevice d2 in _devices)
            {
              if (d == d2)
              {
                break;
              }
              d.Close();
            }
            return false;
          }
        }
      }

      lock (_lockListenerNotifier)
      {
        // Register to receive input from the HIDs.
        if (_listener == null)
        {
          _listener = new HidListener();
        }
        _listener.OnInput += OnInput;
        lock (_lockDevices)
        {
          _listener.RegisterHids(_devices);
        }

        // Register to be notified if any of the HIDs are removed.
        if (_systemChangeNotifier == null)
        {
          _systemChangeNotifier = new SystemChangeNotifier();
        }
        _systemChangeNotifier.OnDeviceInterfaceChange += OnDeviceChange;
      }

      _openProducts.Add(_productInstanceId);
      _isInterfaceOpen = true;
      return true;
    }

    /// <summary>
    /// Close the remote control interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    public bool Close()
    {
      this.LogDebug("Twinhan HID RC: close interface");

      if (!_isInterfaceOpen)
      {
        return true;
      }

      // Disable the HID driver.
      IntPtr buffer = Marshal.AllocCoTaskMem(1);
      try
      {
        Marshal.WriteByte(buffer, 0, 0);
        int hr = _ioControl.Set(IoControlCode.HidRemoteControlEnable, buffer, 1);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogWarn("Twinhan HID RC: failed to disable remote control, hr = 0x{0:x}", hr);
        }
      }
      finally
      {
        Marshal.FreeCoTaskMem(buffer);
      }

      lock (_lockListenerNotifier)
      {
        // Unregister for HID removal notification.
        if (_systemChangeNotifier != null)
        {
          _systemChangeNotifier.OnDeviceInterfaceChange -= OnDeviceChange;
        }

        // Unregister for receiving input from the HIDs.
        if (_listener != null)
        {
          _listener.OnInput -= OnInput;

          lock (_lockDevices)
          {
            if (_devices != null)
            {
              List<HumanInterfaceDevice> openDevices = new List<HumanInterfaceDevice>(_devices.Count);
              foreach (HumanInterfaceDevice d in _devices)
              {
                if (d.IsOpen)
                {
                  openDevices.Add(d);
                }
              }
              _listener.UnregisterHids(openDevices);
            }
          }
        }
      }

      // Close the HIDs.
      lock (_lockDevices)
      {
        if (_devices != null)
        {
          foreach (HumanInterfaceDevice hid in _devices)
          {
            hid.Close();
            hid.Dispose();
          }
          _devices.Clear();
        }
      }

      _openProducts.Remove(_productInstanceId);
      if (_openProducts.Count == 0)
      {
        lock (_lockListenerNotifier)
        {
          if (_listener != null)
          {
            _listener.Dispose();
            _listener = null;
          }

          if (_systemChangeNotifier != null)
          {
            _systemChangeNotifier.Dispose();
            _systemChangeNotifier = null;
          }
        }
      }

      _ioControl = null;
      _isInterfaceOpen = false;
      return true;
    }

    #region loading

    /// <summary>
    /// Find the device identifier and dev inst for a tuner.
    /// </summary>
    private bool FindDeviceForTuner(string tunerExternalId, out string id, out uint devInst)
    {
      id = string.Empty;
      devInst = 0;

      // Enumerate installed and present media devices.
      Guid mediaClass = NativeMethods.GUID_DEVCLASS_MEDIA;
      IntPtr devInfoSet = NativeMethods.SetupDiGetClassDevs(ref mediaClass, null, IntPtr.Zero, NativeMethods.DiGetClassFlags.DIGCF_PRESENT);
      if (devInfoSet == IntPtr.Zero || devInfoSet == new IntPtr(-1))  // If INVALID_HANDLE_VALUE...
      {
        this.LogError("Twinhan HID RC: failed get media device information set, error = {0}", Marshal.GetLastWin32Error());
        return false;
      }

      try
      {
        StringBuilder deviceId = new StringBuilder((int)NativeMethods.MAX_DEVICE_ID_LEN);
        uint index = 0;
        NativeMethods.SP_DEVINFO_DATA devInfo = new NativeMethods.SP_DEVINFO_DATA();
        devInfo.cbSize = (uint)Marshal.SizeOf(typeof(NativeMethods.SP_DEVINFO_DATA));
        while (NativeMethods.SetupDiEnumDeviceInfo(devInfoSet, index++, ref devInfo))
        {
          // Get the device's ID.
          uint requiredSize;
          if (!NativeMethods.SetupDiGetDeviceInstanceId(devInfoSet, ref devInfo, deviceId, NativeMethods.MAX_DEVICE_ID_LEN, out requiredSize))
          {
            this.LogWarn("Twinhan HID RC: failed to get media device instance ID, error = {0}, dev inst = {1}, required size = {2}", Marshal.GetLastWin32Error(), devInfo.DevInst, requiredSize);
            continue;
          }

          // Is this the tuner we're looking for?
          // Device ID example:         PCI\VEN_1822&DEV_4E35&SUBSYS_00031AE4&REV_01\4&CF81C54&0&00F0
          // Tuner external ID example: @device:pnp:\\?\pci#ven_1822&dev_4e35&subsys_00031ae4&rev_01#4&cf81c54&0&00f0#{71985f48-1ca1-11d3-9cc8-00c04f7971e0}\{acec2d03-bdec-44c0-9a97-c577f6ee2602}
          string tempId = deviceId.ToString();
          if (tunerExternalId.ToLowerInvariant().Contains(tempId.Replace("\\", "#").ToLowerInvariant()))
          {
            id = tempId;
            devInst = devInfo.DevInst;
            return true;
          }
        }

        int error = Marshal.GetLastWin32Error();
        if (error == (int)NativeMethods.SystemErrorCode.ERROR_NO_MORE_ITEMS)
        {
          this.LogError("Twinhan HID RC: failed to find device for tuner after enumerating all media devices, tuner external ID = {0}", tunerExternalId);
        }
        else
        {
          this.LogError("Twinhan HID RC: failed to get next media device information, error = {0}", error);
        }
      }
      finally
      {
        NativeMethods.SetupDiDestroyDeviceInfoList(devInfoSet);
      }
      return false;
    }

    /// <summary>
    /// Find the HID identifiers associated with a dev inst.
    /// </summary>
    private bool FindHidsForDevice(uint devInst, out string rootHidId, out ICollection<string> hidIds)
    {
      rootHidId = string.Empty;
      hidIds = new List<string>();

      uint tempDevInst1;
      NativeMethods.ConfigurationManagerReturnStatus result = NativeMethods.CM_Get_Child(out tempDevInst1, devInst, 0);
      if (result != NativeMethods.ConfigurationManagerReturnStatus.CR_SUCCESS)
      {
        this.LogError("Twinhan HID RC: failed to get root HID device, result = {0}, dev inst = {1}", result, devInst);
        return false;
      }

      StringBuilder deviceId = new StringBuilder((int)NativeMethods.MAX_DEVICE_ID_LEN);
      result = NativeMethods.CM_Get_Device_ID(tempDevInst1, deviceId, NativeMethods.MAX_DEVICE_ID_LEN, 0);
      if (result != NativeMethods.ConfigurationManagerReturnStatus.CR_SUCCESS)
      {
        this.LogError("Twinhan HID RC: failed to get root HID device ID, result = {0}, dev inst = {1}", result, tempDevInst1);
        return false;
      }
      // Example: AVSTREAM\AZUREWAVEPCIHID.VIRTUAL\5&1560A6BC&1&0
      rootHidId = deviceId.ToString();

      uint tempDevInst2;
      result = NativeMethods.CM_Get_Child(out tempDevInst2, tempDevInst1, 0);
      if (result != NativeMethods.ConfigurationManagerReturnStatus.CR_SUCCESS)
      {
        this.LogError("Twinhan HID RC: failed to get first child HID device, result = {0}, dev inst = {1}", result, tempDevInst1);
        return false;
      }
      result = NativeMethods.CM_Get_Device_ID(tempDevInst2, deviceId, NativeMethods.MAX_DEVICE_ID_LEN, 0);
      if (result != NativeMethods.ConfigurationManagerReturnStatus.CR_SUCCESS)
      {
        this.LogError("Twinhan HID RC: failed to get first child HID device ID, result = {0}, dev inst = {1}", result, tempDevInst2);
        return false;
      }
      // Example: \??\HID#AzureWavePciHID.VIRTUAL&Col02#6&32a2cb67&0&0001#{4d1e55b2-f16f-11cf-88cb-001111000030}
      hidIds.Add(deviceId.ToString());
      tempDevInst1 = tempDevInst2;

      while (true)
      {
        result = NativeMethods.CM_Get_Sibling(out tempDevInst2, tempDevInst1, 0);
        if (result != NativeMethods.ConfigurationManagerReturnStatus.CR_SUCCESS)
        {
          if (result != NativeMethods.ConfigurationManagerReturnStatus.CR_NO_SUCH_DEVNODE)
          {
            // Not no more siblings.
            this.LogError("Twinhan HID RC: failed to get next child HID device, result = {0}, dev inst = {1}", result, tempDevInst1);
            return false;
          }
          return true;
        }
        result = NativeMethods.CM_Get_Device_ID(tempDevInst2, deviceId, NativeMethods.MAX_DEVICE_ID_LEN, 0);
        if (result != NativeMethods.ConfigurationManagerReturnStatus.CR_SUCCESS)
        {
          this.LogError("Twinhan HID RC: failed to get next child HID device ID, result = {0}, dev inst = {1}", result, tempDevInst2);
          return false;
        }
        hidIds.Add(deviceId.ToString());
        tempDevInst1 = tempDevInst2;
      }
    }

    /// <summary>
    /// Load the raw input devices associated with a set of HID identifiers.
    /// </summary>
    private bool LoadHids(ICollection<string> hidIds, bool isTerraTecDriver, out IList<HumanInterfaceDevice> devices)
    {
      devices = new List<HumanInterfaceDevice>(15);

      // Get the raw input device list size.
      uint deviceCount = 0;
      uint listDeviceSize = (uint)Marshal.SizeOf(typeof(NativeMethods.RAWINPUTDEVICELIST));
      int result = NativeMethods.GetRawInputDeviceList(null, ref deviceCount, listDeviceSize);
      if (result != (int)NativeMethods.SystemErrorCode.ERROR_SUCCESS)
      {
        this.LogError("Twinhan HID RC: failed to get raw input device list size, result = {0}, error = {1}", result, Marshal.GetLastWin32Error());
        return false;
      }

      // Get the device list.
      NativeMethods.RAWINPUTDEVICELIST[] deviceList = new NativeMethods.RAWINPUTDEVICELIST[deviceCount];
      uint deviceInfoSize = (uint)Marshal.SizeOf(typeof(NativeMethods.RID_DEVICE_INFO));
      IntPtr deviceInfo = Marshal.AllocHGlobal((int)deviceInfoSize);
      try
      {
        result = NativeMethods.GetRawInputDeviceList(deviceList, ref deviceCount, listDeviceSize);
        if (result == -1)
        {
          this.LogError("Twinhan HID RC: failed to get raw input device list, result = {0}, error = {1}", result, Marshal.GetLastWin32Error());
          return false;
        }

        // For each device...
        HashSet<string> missingHidIds = new HashSet<string>(hidIds);
        foreach (NativeMethods.RAWINPUTDEVICELIST d in deviceList)
        {
          string name = ReadRawDeviceName(d.hDevice);

          // Is this one of the HIDs associated with the tuner?
          // HID ID example:          HID\AZUREWAVEPCIHID.VIRTUAL&COL02\6&32A2CB67&0&0001
          // Raw device name example: \??\HID#AzureWavePciHID.VIRTUAL&Col02#6&32a2cb67&0&0001#{4d1e55b2-f16f-11cf-88cb-001111000030}
          foreach (string hidId in hidIds)
          {
            if (name.ToLowerInvariant().Contains(hidId.Replace("\\", "#").ToLowerInvariant()))
            {
              devices.Add(new HumanInterfaceDevice(hidId, name, d.dwType, isTerraTecDriver, d.hDevice));
              missingHidIds.Remove(hidId);
              break;
            }
          }
        }

        if (devices.Count != hidIds.Count)
        {
          // This does happen. For some reason there doesn't seem to be a raw
          // input for HID\AZUREWAVEPCIHID.VIRTUAL&COL06\6&1707C13&0&0005.
          this.LogWarn("Twinhan HID RC: failed to find raw input device(s) corresponding with HID ID(s), missing = [{0}]", missingHidIds);
        }
        return true;
      }
      finally
      {
        Marshal.FreeHGlobal(deviceInfo);
      }
    }

    /// <summary>
    /// Read the name of a raw input device.
    /// </summary>
    private string ReadRawDeviceName(IntPtr device)
    {
      uint deviceNameSize = 256;
      int result = NativeMethods.GetRawInputDeviceInfo(device, NativeMethods.RawInputInfoCommand.RIDI_DEVICENAME, IntPtr.Zero, ref deviceNameSize);
      if (result != 0)
      {
        this.LogError("Twinhan HID RC: failed to get raw input device name size, result = {0}, error = {1}", result, Marshal.GetLastWin32Error());
        return string.Empty;
      }

      IntPtr deviceName = Marshal.AllocHGlobal((int)deviceNameSize * 2);  // size is the character count not byte count
      try
      {
        result = NativeMethods.GetRawInputDeviceInfo(device, NativeMethods.RawInputInfoCommand.RIDI_DEVICENAME, deviceName, ref deviceNameSize);
        if (result > 0)
        {
          return Marshal.PtrToStringUni(deviceName, result - 1); // -1 for NULL termination
        }

        this.LogError("Twinhan HID RC: failed to get raw input device name, result = {0}, error = {1}", result, Marshal.GetLastWin32Error());
        return string.Empty;
      }
      finally
      {
        Marshal.FreeHGlobal(deviceName);
      }
    }

    /// <summary>
    /// Read the configuration for an HID driver.
    /// </summary>
    private bool ReadConfig(IoControl ioControl, out TwinhanRemoteControlType remoteType, out TwinhanRemoteControlMapping mapping)
    {
      remoteType = TwinhanRemoteControlType.Old;
      mapping = TwinhanRemoteControlMapping.DtvDvb;

      IntPtr buffer = Marshal.AllocCoTaskMem(HID_REMOTE_CONTROL_CONFIG_SIZE);
      try
      {
        for (int i = 0; i < HID_REMOTE_CONTROL_CONFIG_SIZE; i++)
        {
          Marshal.WriteByte(buffer, i, 0);
        }
        int returnedByteCount;
        int hr = ioControl.Get(IoControlCode.GetHidRemoteConfig, buffer, HID_REMOTE_CONTROL_CONFIG_SIZE, out returnedByteCount);
        // The Mantis driver does not return the number of bytes populated,
        // so we can only check the HRESULT.
        if (hr != (int)NativeMethods.HResult.S_OK)  // || returnedByteCount != HID_REMOTE_CONTROL_CONFIG_SIZE)
        {
          this.LogWarn("Twinhan HID RC: failed to read HID config, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
          return false;
        }

        HidRemoteControlConfig config = (HidRemoteControlConfig)Marshal.PtrToStructure(buffer, typeof(HidRemoteControlConfig));
        if (config.IrSysCodeCheck1 == 0x1b)
        {
          // DigitalNow QuattroS remote
          remoteType = TwinhanRemoteControlType.New;
        }
        else if (config.IrSysCodeCheck1 == 0xff00)
        {
          remoteType = TwinhanRemoteControlType.Old;
        }
        /*else if (config.IrSysCodeCheck1 == 0xeb14)
        {
          // TerraTec Cinergy remote - scan code mapping not known
        }
        else if (config.IrSysCodeCheck1 == 0x04eb)
        {
          // TerraTec x7 remote - scan code mapping not known
        }
        else if (config.IrSysCodeCheck1 == 0x0af5)
        {
          // TechniSat USB remote - scan code mapping not known
        }*/
        else
        {
          this.LogWarn("Twinhan HID RC: unrecognised IRSYSCODECHECK value 0x{0:x}", config.IrSysCodeCheck1);
        }
        mapping = config.Mapping;
        if (!System.Enum.IsDefined(typeof(TwinhanRemoteControlMapping), config.Mapping))
        {
          this.LogWarn("Twinhan HID RC: unrecognised RC_Configuration value {0}", config.Mapping);
        }
        return true;
      }
      finally
      {
        Marshal.FreeCoTaskMem(buffer);
      }
    }

    /// <summary>
    /// Read the custom scan code mapping for an HID driver.
    /// </summary>
    private IDictionary<int, byte> ReadCustomMapping(string hidId)
    {
      IDictionary<int, byte> mappingTable = new Dictionary<int, byte>(64);
      RegistryView view = RegistryView.Registry64;
      if (!OSInfo.OSInfo.Is64BitOs())
      {
        view = RegistryView.Registry32;
      }

      // Read the HID properties.
      using (RegistryKey key1 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view).OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{745A17A0-74D3-11D0-B6FE-00A0C90F57DA}"))
      {
        if (key1 != null)
        {
          int skCount = key1.SubKeyCount;
          int checkedCount = 0;
          int i = 0;
          // Find the correct HID driver.
          while (checkedCount < skCount)
          {
            using (RegistryKey key2 = key1.OpenSubKey(string.Format("{0:D4}", i++)))
            {
              // HidId looks like AVSTREAM\AZUREWAVEPCIHID.VIRTUAL\5&1560A6BC&1&0.
              // MatchingDeviceId looks like avstream\azurewavepcihid.virtual.
              object deviceId = key2.GetValue("MatchingDeviceId");
              if (deviceId != null && hidId.ToLowerInvariant().Contains(deviceId.ToString()))
              {
                using (RegistryKey key3 = key2.OpenSubKey("DriverData"))
                {
                  if (key3 != null)
                  {
                    foreach (string name in key3.GetValueNames())
                    {
                      if (name.Equals("RC_Report"))
                      {
                        if ((int)key3.GetValue(name) == 0)
                        {
                          // When RC_Report is 0 the virtual HID devices won't
                          // be loaded by the driver; only the HID keyboard
                          // will be loaded. This means any scan codes which
                          // are mapped to MCE, consumer, raw or ASCII HID
                          // actions/codes won't work. Changing the value
                          // would not have any effect until the driver is
                          // reloaded (eg. reboot).
                          this.LogWarn("Twinhan HID RC: RC_Report is 0, buttons mapped to MCE, consumer, raw or ASCII HID won't work");
                        }
                      }
                      else
                      {
                        Match m = REGEX_SCANCODE_REGVAL.Match(name);
                        if (m.Success)
                        {
                          byte scancode = Convert.ToByte(m.Groups[1].Captures[0].Value, 16);
                          int value = (int)key3.GetValue(name);
                          int key = (int)TwinhanUsageType.Keyboard | (value & 0xff);
                          if ((value & 0xff00) != 0)
                          {
                            key |= (int)VirtualKeyModifier.Control;
                          }
                          if ((value & 0xff0000) != 0)
                          {
                            key |= (int)VirtualKeyModifier.Shift;
                          }
                          if ((value & 0xff000000) != 0)
                          {
                            key |= (int)VirtualKeyModifier.Alt;
                          }
                          mappingTable[key] = scancode;
                        }
                      }
                    }
                  }
                }
                break;
              }
            }
          }
        }
      }

      if (mappingTable.Count == 0)
      {
        this.LogWarn("Twinhan HID RC: failed to load custom mapping table for HID {0}", hidId);
      }
      return mappingTable;
    }

    #endregion

    #region input handling

    private void OnInput(IntPtr input)
    {
      uint dataSize = 0;
      uint headerSize = (uint)Marshal.SizeOf(typeof(NativeMethods.RAWINPUTHEADER));
      int result = NativeMethods.GetRawInputData(input, NativeMethods.RawInputDataCommand.RID_INPUT, IntPtr.Zero, ref dataSize, headerSize);
      if (result != 0)
      {
        this.LogError("Twinhan HID RC: failed to get raw input data size, result = {0}, error = {1}", result, Marshal.GetLastWin32Error());
        return;
      }

      IntPtr dataPtr = Marshal.AllocHGlobal((int)dataSize);
      try
      {
        result = NativeMethods.GetRawInputData(input, NativeMethods.RawInputDataCommand.RID_INPUT, dataPtr, ref dataSize, headerSize);
        if (result <= 0)
        {
          this.LogError("Twinhan HID RC: failed to get raw input data, result = {0}, error = {1}, required size = {2}", result, Marshal.GetLastWin32Error(), dataSize);
          return;
        }

        NativeMethods.RAWINPUT rawInput = (NativeMethods.RAWINPUT)Marshal.PtrToStructure(dataPtr, typeof(NativeMethods.RAWINPUT));

        HumanInterfaceDevice device = null;
        TwinhanUsageType usageType;
        int usage;
        string usageName;
        lock (_lockDevices)
        {
          // Determine whether the input was received from one of the tuner's
          // HIDs.
          foreach (HumanInterfaceDevice d in _devices)
          {
            if (d.Handle == rawInput.header.hDevice)
            {
              device = d;
              break;
            }
          }
          if (device == null)
          {
            return;
          }

          // Convert device specific raw input to device independent "usages".
          if (!device.GetUsageFromRawInput(rawInput, dataPtr, out usageType, out usage, out usageName))
          {
            return;
          }
        }

        // Reverse-convert device independent usages back to remote control buttons.
        byte scanCode;
        if (GetScanCodeFromUsage(usageType, usage, out scanCode))
        {
          if (_remoteControlType == TwinhanRemoteControlType.New)
          {
            this.LogDebug("Twinhan HID RC: key press, type = {0}, usage = {1}, button = {2}, HID = {3}", usageType, usageName, (TwinhanRemoteScanCodeNew)scanCode, device.Id);
          }
          else
          {
            this.LogDebug("Twinhan HID RC: key press, type = {0}, usage = {1}, button = {2}, HID = {3}", usageType, usageName, (TwinhanRemoteScanCodeOld)scanCode, device.Id);
          }
        }
        else
        {
          this.LogWarn("Twinhan HID RC: failed to find scan code, type = {0}, usage = {1}, HID = {2}", usageType, usageName, device.Id);
        }
      }
      finally
      {
        Marshal.FreeHGlobal(dataPtr);
      }
    }

    private bool GetScanCodeFromUsage(TwinhanUsageType usageType, int usage, out byte scanCode)
    {
      scanCode = 0;
      if (_isTerraTecDriver)
      {
        // Assume raw input. This could be incorrect. We do know that the
        // TerraTec drivers don't use the internal mapping tables. It is
        // possible that TerraTec uses their "Remote Control Editor" software
        // to translate to configurable key press events outside the driver.
        scanCode = (byte)(usage & 0xff);
        return true;
      }

      int keyCode = (int)usageType | usage;
      if (_remoteControlMapping == TwinhanRemoteControlMapping.DtvDvb)
      {
        return MAPPING_DTV_DVB.TryGetValue(keyCode, out scanCode);
      }
      else if (_remoteControlMapping == TwinhanRemoteControlMapping.Cyberlink)
      {
        return MAPPING_CYBERLINK.TryGetValue(keyCode, out scanCode);
      }
      else if (_remoteControlMapping == TwinhanRemoteControlMapping.InterVideo)
      {
        return  MAPPING_INTERVIDEO.TryGetValue(keyCode, out scanCode);
      }
      else if (_remoteControlMapping == TwinhanRemoteControlMapping.Mce)
      {
        return  MAPPING_MCE.TryGetValue(keyCode, out scanCode);
      }
      else if (_remoteControlMapping == TwinhanRemoteControlMapping.DtvDvbWmInput)
      {
        return  MAPPING_DTV_DVB_WM_INPUT.TryGetValue(keyCode, out scanCode);
      }
      else if (_remoteControlMapping == TwinhanRemoteControlMapping.Custom)
      {
        return  _customMappingTable.TryGetValue(keyCode, out scanCode);
      }
      else if (_remoteControlMapping == TwinhanRemoteControlMapping.DigitalNow)
      {
        return  MAPPING_DNTV.TryGetValue(keyCode, out scanCode);
      }
      return false;
    }

    #endregion

    private void OnDeviceChange(NativeMethods.DBT_MANAGEMENT_EVENT eventType, Guid classGuid, string devicePath)
    {
      // Find the HID associated with this event, if any.
      if (
        string.IsNullOrEmpty(devicePath) ||
        (
          classGuid != NativeMethods.GUID_DEVINTERFACE_HID &&
          classGuid != NativeMethods.GUID_DEVINTERFACE_KEYBOARD &&
          classGuid != NativeMethods.GUID_DEVINTERFACE_MOUSE
        )
      )
      {
        return;
      }

      devicePath = devicePath.ToLowerInvariant();

      ThreadPool.QueueUserWorkItem(delegate
      {
        HumanInterfaceDevice device = null;
        lock (_lockDevices)
        {
          if (_devices != null)
          {
            foreach (HumanInterfaceDevice d in _devices)
            {
              // Note: I'm not actually sure what the device path will look like,
              // so the ID vs. device path check may fail (=> fix it!).
              if (
                d.Id.ToLowerInvariant().Contains(devicePath) &&
                (
                  (d.DeviceType == NativeMethods.RawInputDeviceType.RIM_TYPEHID && classGuid == NativeMethods.GUID_DEVINTERFACE_HID) ||
                  (d.DeviceType == NativeMethods.RawInputDeviceType.RIM_TYPEKEYBOARD && classGuid == NativeMethods.GUID_DEVINTERFACE_KEYBOARD) ||
                  (d.DeviceType == NativeMethods.RawInputDeviceType.RIM_TYPEMOUSE && classGuid == NativeMethods.GUID_DEVINTERFACE_MOUSE)
                )
              )
              {
                device = d;
                break;
              }
            }
          }
        }
        if (device == null)
        {
          return;
        }

        // This change affects one of our HIDs.
        this.LogInfo("Twinhan HID RC: on device change, event type = {0}, device path = {1}, HID = {2}", eventType, devicePath, device.Id);
        if (eventType != NativeMethods.DBT_MANAGEMENT_EVENT.DBT_DEVICEARRIVAL && eventType != NativeMethods.DBT_MANAGEMENT_EVENT.DBT_DEVICEREMOVECOMPLETE)
        {
          return;
        }

        // Close the device if it's open.
        lock (_lockListenerNotifier)
        {
          lock (_lockDevices)
          {
            if (device.IsOpen)
            {
              if (_listener != null)
              {
                _listener.UnregisterHids(new List<HumanInterfaceDevice> { device });
              }
              device.Close();
            }
          }
        }
        if (eventType == NativeMethods.DBT_MANAGEMENT_EVENT.DBT_DEVICEREMOVECOMPLETE)
        {
          return;
        }

        // Reload and reopen the device on arrival. We reload because we don't
        // want to assume that the previous handle is still valid.
        IList<HumanInterfaceDevice> newDevices;
        if (LoadHids(new List<string> { device.Id }, _isTerraTecDriver, out newDevices) && newDevices.Count == 1)
        {
          lock (_lockDevices)
          {
            device.Dispose();
            _devices.Remove(device);
          }

          device = newDevices[0];
          if (device.Open())
          {
            lock (_lockListenerNotifier)
            {
              if (_listener != null)
              {
                _listener.RegisterHids(newDevices);
              }
            }
          }

          lock (_lockDevices)
          {
            _devices.Add(device);
          }
        }
      });
    }
  }
}
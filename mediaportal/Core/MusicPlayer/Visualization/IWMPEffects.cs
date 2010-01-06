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
using System.Runtime.InteropServices;

namespace MediaPortal.Visualization
{
  [
    ComImport,
    Guid("D3984C13-C3CB-48E2-8BE5-5168340B4F35"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
  ]
  public interface IWMPEffects
  {
    // Render using the rectangle on the normalized device context
    //void Render(ref TimedLevel levels, IntPtr hdc, ref RECT r);
    //[UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    int Render(IntPtr timedLevels, IntPtr hdc, IntPtr rect);

    // Provides the # of channels, sample rate and title of current audio
    int MediaInfo(int channelCount, int sampleRate, [In, MarshalAs(UnmanagedType.LPWStr)] string title);

    // Provides the capabilities of the visualization
    //VisualizationCapabilities Capabilities { get; }
    int GetCapabilities(ref int caps);

    // Provides the display title of the visualization
    //string Title { get; }
    //int GetTitle([Out, MarshalAs(UnmanagedType.LPWStr)] out string title);
    int GetTitle([Out, MarshalAs(UnmanagedType.BStr)] out string title);

    // Provides the title for a preset
    //string GetPresetTitle(int preset);
    //int GetPresetTitle(int preset, [Out, MarshalAs(UnmanagedType.LPWStr)] out string title);
    int GetPresetTitle(int preset, [Out, MarshalAs(UnmanagedType.BStr)] out string title);

    // Provides the # of presets supported
    //int PresetCount { get; }
    int GetPresetCount(ref int count);

    // Set/Get the current preset
    //int CurrentPreset { set; get; }
    int SetCurrentPreset(int preset);
    int GetCurrentPreset(ref int preset);

    // Display the property page of the effect (if there is one)
    int DisplayPropertyPage(IntPtr hwndOwner);

    // Called when full screen rendering should start or stop
    int GoFullScreen([MarshalAs(UnmanagedType.Bool)] bool startFullScreen);

    // This is called after a successful a call to GoFullScreen(true).
    // Return failure from this method to signal loss of full screen.
    //int RenderFullScreen(ref TimedLevel levels);
    int RenderFullScreen(IntPtr timedLevels);
  }

  public enum EffectsCapability
  {
    EFFECT_CANGOFULLSCREEN = 0x00000001,
    EFFECT_HASPROPERTYPAGE = 0x00000002,
    EFFECT_VARIABLEFREQSTEP = 0x00000004,
    EFFECT_WINDOWEDONLY = 0x00000008,
    EFFECT2_FULLSCREENEXCLUSIVE = 0x00000010,
  }
}
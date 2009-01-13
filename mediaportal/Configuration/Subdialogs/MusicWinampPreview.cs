#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using MediaPortal.GUI.Library;
using MediaPortal.UserInterface.Controls;
using MediaPortal.Visualization;
using Un4seen.Bass.AddOn.Vis;

namespace MediaPortal.Configuration.Sections
{
  /// <summary>
  /// This Form shows the Preview of Winamp Visualisation Plugins.
  /// </summary>
  public partial class MusicWinampPreview : MPConfigForm
  {
    #region Variables

    private VisualizationInfo VizPluginInfo;
    private int _visHandle = 0;

    private IntPtr hwndWinAmp; // The handle to the Winamp Dummy Window
    private IntPtr hwndGen; // The handle to the Winamp Gen Window

    #endregion

    #region Constructors/Destructors

    public MusicWinampPreview(VisualizationInfo vizinfo)
    {
      InitializeComponent();

      VizPluginInfo = vizinfo;
      BassVis.BASS_WINAMPVIS_Init(
        BassVis.GetWindowLongPtr(GUIGraphicsContext.form.Handle, (int) GWLIndex.GWL_HINSTANCE), this.Handle);
      StartVis();
    }

    #endregion

    #region Private Methods

    private void btClose_Click(object sender, EventArgs e)
    {
      if (_visHandle != 0)
      {
        BassVis.BASS_WINAMPVIS_Free(_visHandle);
      }

      BassVis.BASS_WINAMPVIS_Quit();
      Close();
    }

    private void btStart_Click(object sender, EventArgs e)
    {
      StartVis();
    }

    private void btConfig_Click(object sender, EventArgs e)
    {
      int tmpVis = BassVis.BASS_WINAMPVIS_GetHandle(VizPluginInfo.FilePath);
      if (tmpVis != 0)
      {
        int numModules = BassVis.BASS_WINAMPVIS_GetNumModules(VizPluginInfo.FilePath);
        BassVis.BASS_WINAMPVIS_Config(tmpVis, VizPluginInfo.PresetIndex);
      }
    }

    private void btStop_Click(object sender, EventArgs e)
    {
      if (hwndWinAmp != IntPtr.Zero)
      {
        BassVis.BASS_WINAMPVIS_Stop((int) hwndWinAmp);
        BassVis.BASS_WINAMPVIS_Free(_visHandle);
        _visHandle = 0;
      }
    }

    private void StartVis()
    {
      // Do we have a Vis running, then stop it
      if (_visHandle != 0)
      {
        btStop_Click(null, null);
      }

      // The following Play is necessary for supporting Winamp Viz, which need a playing env, like Geiss 2, Beatharness, etc.
      // Workaround until BassVis 2.3.0.7 is released
      BassVis.BASS_WINAMPVIS_Play(69);

      _visHandle = BassVis.BASS_WINAMPVIS_ExecuteVis(VizPluginInfo.FilePath, VizPluginInfo.PresetIndex, true, true);
      if (_visHandle != 0)
      {
        // get handle to the Winamp Dummy Window
        hwndWinAmp = BassVis.BASS_WINAMPVIS_GetAmpHwnd();

        // Set Status to Playing
        BassVis.BASS_WINAMPVIS_Play((int) hwndWinAmp);

        // Now move the Vis to our own Picturebox
        hwndGen = BassVis.BASS_WINAMPVIS_GetGenHwnd();
        if (hwndGen != IntPtr.Zero)
        {
          BassVis.BASS_WINAMPVIS_SetGenHwndParent(hwndGen,
                                                  pictureBoxVis.Handle, 5, 5,
                                                  pictureBoxVis.Width - 12,
                                                  pictureBoxVis.Height - 12);
        }

        // Set Dummy Song Information for the Plugin
        BassVis.BASS_WINAMPVIS_SetChanInfo(_visHandle, "Mediaportal Preview", "  ", 0, 0, 1, 1);

        int stream = 0;
        BassVis.BASS_WINAMPVIS_RenderStream(stream);
      }
    }

    #endregion
  }
}
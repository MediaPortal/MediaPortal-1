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
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.TagReader;
using Un4seen.Bass.AddOn.Vis;

namespace MediaPortal.Visualization
{
  public class WinampViz : VisualizationBase
  {
    #region Imports

    [DllImport("User32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("User32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
    private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int cx, int cy, bool repaint);

    [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto, EntryPoint = "GetWindow")]
    private static extern IntPtr GetWindow(IntPtr hWnd, int child);

    private const int GW_CHILD = 5;

    #endregion

    #region Variables

    private int visHandle = 0;
    private bool RenderStarted = false;
    private bool firstRun = true;

    private IntPtr hwndWinAmp; // The Winamp fake window, to which we send start and stop commands
    private IntPtr genHwnd; // The internal Gen Window of the Visualisation
    private IntPtr hwndChild; // Handle to the Winamp Child Window.

    private MusicTag trackTag = null;
    private string _songTitle = "   "; // Title of the song played

    #endregion

    #region Constructors/Destructors

    public WinampViz(VisualizationInfo vizPluginInfo, VisualizationWindow vizCtrl)
      : base(vizPluginInfo, vizCtrl)
    {
    }

    #endregion

    #region Public Methods

    public override bool Initialize()
    {
      Bass.PlaybackStateChanged += new BassAudioEngine.PlaybackStateChangedDelegate(PlaybackStateChanged);

      BassVis.BASS_WINAMPVIS_Init(
        BassVis.GetWindowLongPtr(GUIGraphicsContext.form.Handle, (int) GWLIndex.GWL_HINSTANCE),
        VisualizationWindow.Handle);

      // The following Play is necessary for supporting Winamp Viz, which need a playing env, like Geiss 2, Beatharness, etc.
      // Workaround until BassVis 2.3.0.7 is released
      BassVis.BASS_WINAMPVIS_Play(69);

      try
      {
        Log.Info("Visualization Manager: Initializing {0} visualization...", VizPluginInfo.Name);

        if (VizPluginInfo == null)
        {
          Log.Error("Visualization Manager: {0} visualization engine initialization failed! Reason:{1}",
                    VizPluginInfo.Name, "Missing or invalid VisualizationInfo object.");

          return false;
        }

        // For Winamp Visualisations, we need to do the initialisation at a later stage, otherwise we get a hang.
        _Initialized = true;
        firstRun = true;

        RenderStarted = false;
      }

      catch (Exception ex)
      {
        Log.Error(
          "Visualization Manager: Winamp visualization engine initialization failed with the following exception {0}",
          ex);
        return false;
      }

      return _Initialized;
    }

    #endregion

    #region Private Methods

    private void PlaybackStateChanged(object sender, BassAudioEngine.PlayState oldState,
                                      BassAudioEngine.PlayState newState)
    {
      Log.Debug("WinampViz: BassPlayer_PlaybackStateChanged from {0} to {1}", oldState.ToString(), newState.ToString());
      if (newState == BassAudioEngine.PlayState.Playing)
      {
        RenderStarted = false;
        trackTag = TagReader.TagReader.ReadTag(Bass.CurrentFile);
        if (trackTag != null)
        {
          _songTitle = String.Format("{0} - {1}", trackTag.Artist, trackTag.Title);
        }
        else
        {
          _songTitle = "   ";
        }

        // Send a Start command to the Winamp Fake window
        BassVis.BASS_WINAMPVIS_Play((int) hwndWinAmp);
      }
      else if (newState == BassAudioEngine.PlayState.Paused)
      {
        // Send a Start command to the Winamp Fake window
        BassVis.BASS_WINAMPVIS_Pause((int) hwndWinAmp);
      }
      else if (newState == BassAudioEngine.PlayState.Ended)
      {
        // Send a Start command to the Winamp Fake window        
        BassVis.BASS_WINAMPVIS_Stop((int) hwndWinAmp);
        RenderStarted = false;
      }
    }

    #endregion

    #region <Base class> Overloads

    public override bool InitializePreview()
    {
      base.InitializePreview();
      return Initialize();
    }

    public override void Dispose()
    {
      base.Dispose();
      Close();
    }

    public override int RenderVisualization()
    {
      try
      {
        if (VisualizationWindow == null || !VisualizationWindow.Visible || visHandle == 0)
        {
          return 0;
        }

        // Set Song information, so that the plugin can display it
        if (trackTag != null && Bass != null)
        {
          BassVis.BASS_WINAMPVIS_SetChanInfo(visHandle, String.Format("1. {0}", _songTitle), Bass.CurrentFile,
                                             (int) Bass.CurrentPosition, (int) Bass.Duration, 1, 1);
        }
        else
        {
          BassVis.BASS_WINAMPVIS_SetChanInfo(visHandle, _songTitle, "  ", 0, 0, 1, 1);
        }

        if (RenderStarted)
        {
          return 1;
        }

        int stream = 0;

        if (Bass != null)
        {
          stream = (int) Bass.GetCurrentVizStream();
        }

        BassVis.BASS_WINAMPVIS_Play((int) hwndWinAmp);
        RenderStarted = BassVis.BASS_WINAMPVIS_RenderStream(stream);
      }

      catch (Exception)
      {
      }

      return 1;
    }

    public override bool Close()
    {
      try
      {
        if (visHandle != 0)
        {
          BassVis.BASS_WINAMPVIS_Stop((int) hwndWinAmp);
          BassVis.BASS_WINAMPVIS_Free(visHandle);
          BassVis.BASS_WINAMPVIS_Quit();
          visHandle = 0;
        }

        return true;
      }

      catch (Exception)
      {
        return false;
      }
    }

    public override bool Config()
    {
      // We need to stop the Vis first, otherwise some plugins don't allow the config to be called
      if (visHandle != 0)
      {
        BassVis.BASS_WINAMPVIS_Stop((int) hwndWinAmp);
        BassVis.BASS_WINAMPVIS_Free(visHandle);
        visHandle = 0;
      }

      int tmpVis = BassVis.BASS_WINAMPVIS_GetHandle(VizPluginInfo.FilePath);
      if (tmpVis != 0)
      {
        int numModules = BassVis.BASS_WINAMPVIS_GetNumModules(VizPluginInfo.FilePath);
        BassVis.BASS_WINAMPVIS_Config(tmpVis, VizPluginInfo.PresetIndex);
      }

      return true;
    }

    public override bool IsWinampVis()
    {
      return true;
    }

    public override bool WindowChanged(VisualizationWindow vizWindow)
    {
      base.WindowChanged(vizWindow);
      return true;
    }

    public override bool WindowSizeChanged(Size newSize)
    {
      // If width or height are 0 the call to CreateVis will fail.  
      // If width or height are 1 the window is in transition so we can ignore it.
      if (VisualizationWindow.Width <= 1 || VisualizationWindow.Height <= 1)
      {
        return false;
      }

      // Do a move of the Winamp Viz
      if (visHandle != 0)
      {
        hwndChild = GetWindow(VisualizationWindow.Handle, GW_CHILD);
        if (hwndChild != IntPtr.Zero)
        {
          MoveWindow(hwndChild, 0, 0, newSize.Width, newSize.Height, true);
        }
      }

      return true;
    }

    public override bool SetOutputContext(OutputContextType outputType)
    {
      if (VisualizationWindow == null)
      {
        return false;
      }

      if (_Initialized && !firstRun)
      {
        return true;
      }

      // If width or height are 0 the call to CreateVis will fail.  
      // If width or height are 1 the window is in transition so we can ignore it.
      if (VisualizationWindow.Width <= 1 || VisualizationWindow.Height <= 1)
      {
        return false;
      }

      if (VizPluginInfo == null || VizPluginInfo.FilePath.Length == 0 || !File.Exists(VizPluginInfo.FilePath))
      {
        return false;
      }

      if (visHandle != 0)
      {
        bool result = BassVis.BASS_WINAMPVIS_Free(visHandle);
        visHandle = 0;
        RenderStarted = false;
      }

      // Set Dummy Information for the plugin, before creating it
      BassVis.BASS_WINAMPVIS_SetChanInfo(0, _songTitle, "  ", 0, 0, 1, 1);

      // Create the Visualisation
      visHandle = BassVis.BASS_WINAMPVIS_ExecuteVis(VizPluginInfo.FilePath, VizPluginInfo.PresetIndex, true, true);
      if (visHandle != 0)
      {
        // Get a handle to the fake Winamp window created by the plugin
        hwndWinAmp = BassVis.BASS_WINAMPVIS_GetAmpHwnd();
        // get the handle of the internal Winamp Gen Window
        genHwnd = BassVis.BASS_WINAMPVIS_GetGenHwnd();

        // And now move the Plugin into our own Viz window
        BassVis.BASS_WINAMPVIS_SetGenHwndParent(genHwnd, VisualizationWindow.Handle, 0, 0, VisualizationWindow.Width,
                                                VisualizationWindow.Height);
        BassVis.BASS_WINAMPVIS_Play((int) hwndWinAmp);
      }

      // The Winamp Plugin has stolen focus on the MP window. Bring it back to froeground
      SetForegroundWindow(GUIGraphicsContext.form.Handle);

      firstRun = false;
      _Initialized = visHandle != 0;
      return visHandle != 0;
    }

    #endregion
  }
}
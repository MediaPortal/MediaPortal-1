#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

    private BASS_VIS_INFO _mediaInfo = null;

    private bool RenderStarted = false;
    private bool firstRun = true;

    private IntPtr hwndChild; // Handle to the Winamp Child Window.

    private MusicTag trackTag = null;
    private string _songTitle = "   "; // Title of the song played

    #endregion

    #region Constructors/Destructors

    public WinampViz(VisualizationInfo vizPluginInfo, VisualizationWindow vizCtrl)
      : base(vizPluginInfo, vizCtrl) {}

    #endregion

    #region Public Methods

    public override bool Initialize()
    {
      Bass.PlaybackStateChanged += new BassAudioEngine.PlaybackStateChangedDelegate(PlaybackStateChanged);

      _mediaInfo = new BASS_VIS_INFO("", "");

      try
      {
        Log.Info("Visualization Manager: Initializing {0} visualization...", VizPluginInfo.Name);

        if (VizPluginInfo == null)
        {
          Log.Error("Visualization Manager: {0} visualization engine initialization failed! Reason:{1}",
                    VizPluginInfo.Name, "Missing or invalid VisualizationInfo object.");

          return false;
        }

        firstRun = true;
        RenderStarted = false;
        bool result = SetOutputContext(VisualizationWindow.OutputContextType);
        _Initialized = result && _visParam.VisHandle != 0;
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

        _mediaInfo.SongTitle = _songTitle;
        _mediaInfo.SongFile = Bass.CurrentFile;

        BassVis.BASS_VIS_SetPlayState(_visParam, BASSVISPlayState.Play);
      }
      else if (newState == BassAudioEngine.PlayState.Paused)
      {
        BassVis.BASS_VIS_SetPlayState(_visParam, BASSVISPlayState.Pause);
      }
      else if (newState == BassAudioEngine.PlayState.Ended)
      {
        BassVis.BASS_VIS_SetPlayState(_visParam, BASSVISPlayState.Stop);
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
        if (VisualizationWindow == null || !VisualizationWindow.Visible || _visParam.VisHandle == 0)
        {
          return 0;
        }

        // Set Song information, so that the plugin can display it
        if (trackTag != null && Bass != null)
        {
          _mediaInfo.Position = (int)Bass.CurrentPosition;
          _mediaInfo.Duration = (int)Bass.Duration;
          _mediaInfo.PlaylistLen = 1;
          _mediaInfo.PlaylistPos = 1;
        }
        else
        {
          _mediaInfo.Position = 0;
          _mediaInfo.Duration = 0;
          _mediaInfo.PlaylistLen = 0;
          _mediaInfo.PlaylistPos = 0;
        }
        if (IsPreviewVisualization)
        {
          _mediaInfo.SongTitle = "Mediaportal Preview";
        }
        BassVis.BASS_VIS_SetInfo(_visParam, _mediaInfo);

        if (RenderStarted)
        {
          return 1;
        }

        int stream = 0;

        if (Bass != null)
        {
          stream = (int)Bass.GetCurrentVizStream();
        }

        BassVis.BASS_VIS_SetPlayState(_visParam, BASSVISPlayState.Play);
        RenderStarted = BassVis.BASS_VIS_RenderChannel(_visParam, stream);
      }

      catch (Exception) {}

      return 1;
    }

    public override bool Close()
    {
      if (base.Close())
      {
        return true;
      }
      return false;
    }

    public override bool Config()
    {
      // We need to stop the Vis first, otherwise some plugins don't allow the config to be called
      if (_visParam.VisHandle != 0)
      {
        BassVis.BASS_VIS_SetPlayState(_visParam, BASSVISPlayState.Stop);
        BassVis.BASS_VIS_Free(_visParam);
        _visParam.VisHandle = 0;
      }

      int tmpVis = BassVis.BASS_VIS_GetPluginHandle(BASSVISPlugin.BASSVISKIND_WINAMP, VizPluginInfo.FilePath);
      if (tmpVis != 0)
      {
        int numModules = BassVis.BASS_VIS_GetModulePresetCount(_visParam, VizPluginInfo.FilePath);
        BassVis.BASS_VIS_Config(_visParam, 0);
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
      if (_visParam.VisHandle != 0)
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

      if (_visParam.VisHandle != 0)
      {
        BassVis.BASS_VIS_Free(_visParam);
        _visParam.VisHandle = 0;
        RenderStarted = false;
      }

      // Set Dummy Information for the plugin, before creating it
      _mediaInfo.SongTitle = "";
      _mediaInfo.SongFile = "";
      _mediaInfo.Position = 0;
      _mediaInfo.Duration = 0;
      _mediaInfo.PlaylistPos = 0;
      _mediaInfo.PlaylistLen = 0;
      BassVis.BASS_VIS_SetInfo(_visParam, _mediaInfo);

      try
      {
        // Create the Visualisation
        BASS_VIS_EXEC visExec = new BASS_VIS_EXEC(VizPluginInfo.FilePath);
        visExec.AMP_ModuleIndex = VizPluginInfo.PresetIndex;
        visExec.AMP_UseOwnW1 = 1;
        visExec.AMP_UseOwnW2 = 1;
        BassVis.BASS_VIS_ExecutePlugin(visExec, _visParam);
        if (_visParam.VisGenWinHandle != IntPtr.Zero)
        {
          hwndChild = GetWindow(VisualizationWindow.Handle, GW_CHILD);
          if (hwndChild != IntPtr.Zero)
          {
            MoveWindow(hwndChild, 0, 0, VisualizationWindow.Width, VisualizationWindow.Height, true);
          }

          BassVis.BASS_VIS_SetVisPort(_visParam,
                                      _visParam.VisGenWinHandle,
                                      VisualizationWindow.Handle,
                                      0,
                                      0,
                                      VisualizationWindow.Width,
                                      VisualizationWindow.Height);

          BassVis.BASS_VIS_SetPlayState(_visParam, BASSVISPlayState.Play);
        }
        else
        {
          BassVis.BASS_VIS_SetVisPort(_visParam,
                                      _visParam.VisGenWinHandle,
                                      IntPtr.Zero,
                                      0,
                                      0,
                                      0,
                                      0);
        }

        // The Winamp Plugin has stolen focus on the MP window. Bring it back to froeground
        SetForegroundWindow(GUIGraphicsContext.form.Handle);

        firstRun = false;
      }
      catch (Exception ex)
      {
        Log.Error(
          "Visualization Manager: Winamp visualization engine initialization failed with the following exception {0}",
          ex);
      }
      _Initialized = _visParam.VisHandle != 0;
      return _visParam.VisHandle != 0;
    }

    #endregion
  }
}
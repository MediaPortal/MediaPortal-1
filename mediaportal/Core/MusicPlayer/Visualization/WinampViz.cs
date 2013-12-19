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
using System.Drawing;
using System.IO;
using MediaPortal.GUI.Library;
using MediaPortal.MusicPlayer.BASS;
using MediaPortal.Player;
using MediaPortal.TagReader;
using MediaPortal.Util;
using MediaPortal.Playlists;
using BassVis_Api;

namespace MediaPortal.Visualization
{
  public class WinampViz : VisualizationBase, IDisposable
  {
    #region Variables

    private BASSVIS_INFO _mediaInfo = null;
    private BassVis.BASSVISSTATE _visCallback;
    private BASSVIS_PARAM _tmpVisParam = null;
    private MusicTag trackTag = null;
    private PlayListPlayer _playlistPlayer = null;

    private bool RenderStarted = false;
    private bool firstRun = true;
    private string _songTitle = "   "; // Title of the song played
    private string _OldCurrentFile = "   ";
    private int _playlistTitlePos;

    #endregion

    #region Constructors/Destructors

    public WinampViz(VisualizationInfo vizPluginInfo, VisualizationWindow vizCtrl)
      : base(vizPluginInfo, vizCtrl) {}

    #endregion

    #region Public Methods

    public override bool Initialize()
    {
      Bass.PlaybackStateChanged += new BassAudioEngine.PlaybackStateChangedDelegate(PlaybackStateChanged);

      _mediaInfo = new BASSVIS_INFO("", "");

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
      if (_visParam.VisHandle != 0)
      {
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
          _OldCurrentFile = Bass.CurrentFile;

          BassVis.BASSVIS_SetPlayState(_visParam, BASSVIS_PLAYSTATE.Play);
        }
        else if (newState == BassAudioEngine.PlayState.Paused)
        {
          BassVis.BASSVIS_SetPlayState(_visParam, BASSVIS_PLAYSTATE.Pause);
        }
        else if (newState == BassAudioEngine.PlayState.Ended)
        {
          BassVis.BASSVIS_SetPlayState(_visParam, BASSVIS_PLAYSTATE.Stop);
          RenderStarted = false;
        }
      }
    }

    #endregion

    #region BASSVIS_StateCallback()

    public void BASSVIS_StateCallback(BASSVIS_PLAYSTATE NewState)
    {
      //CallBack PlayState for Winamp only
      switch (NewState)
      {

        case BASSVIS_PLAYSTATE.SetPlaylistTitle:

          BassVis.BASSVIS_SetPlayState(_visParam, BASSVIS_PLAYSTATE.SetPlaylistTitle, -1, _songTitle);

          break;
        case BASSVIS_PLAYSTATE.GetPlaylistTitlePos:

          _playlistTitlePos = BassVis.BASSVIS_SetPlayState(_visParam, BASSVIS_PLAYSTATE.GetPlaylistTitlePos);
          break;

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

        // Any is wrong with PlaybackStateChanged, if the songfile automatically changed
        // so i have create a new variable which fix this problem
        if (Bass != null)
        {
          if (Bass.CurrentFile != _OldCurrentFile)
          {
            trackTag = TagReader.TagReader.ReadTag(Bass.CurrentFile);
            if (trackTag != null)
            {
              _songTitle = String.Format("{0} - {1}", trackTag.Artist, trackTag.Title);
              _OldCurrentFile = Bass.CurrentFile;
            }
            else
            {
              _songTitle = "   ";
            }
          }

          // Set Song information, so that the plugin can display it
          if (trackTag != null)
          {
            _playlistPlayer = PlayListPlayer.SingletonPlayer;
            PlayListItem curPlaylistItem = _playlistPlayer.GetCurrentItem();

            MusicStream streams = Bass.GetCurrentStream();
            // Do not change this line many Plugins search for Songtitle with a number before.
            _mediaInfo.SongFile = Bass.CurrentFile;
            _mediaInfo.SongTitle = (_playlistPlayer.CurrentPlaylistPos + 1) + ". " + _songTitle;
            _mediaInfo.Position = (int)(1000 * Bass.CurrentPosition);
            _mediaInfo.Duration = (int)Bass.Duration;
            _mediaInfo.PlaylistLen = 1;
            _mediaInfo.PlaylistPos = _playlistPlayer.CurrentPlaylistPos;
          }
          else
          {
            _mediaInfo.Position = 0;
            _mediaInfo.Duration = 0;
            _mediaInfo.PlaylistLen = 0;
            _mediaInfo.PlaylistPos = 0;
          }
        }

        if (IsPreviewVisualization)
        {
          _mediaInfo.SongTitle = "Mediaportal Preview";
        }
        BassVis.BASSVIS_SetInfo(_visParam, _mediaInfo);

        if (RenderStarted)
        {
          return 1;
        }

        int stream = 0;

        if (Bass != null)
        {
          stream = (int)Bass.GetCurrentVizStream();
        }

        // ckeck is playing
        int nReturn = BassVis.BASSVIS_SetPlayState(_visParam, BASSVIS_PLAYSTATE.IsPlaying);
        if (nReturn == Convert.ToInt32(BASSVIS_PLAYSTATE.Play) && (_visParam.VisHandle != 0))
        {
          // Do not Render without playing
          if (MusicPlayer.BASS.Config.MusicPlayer == AudioPlayer.WasApi)
          {
            RenderStarted = BassVis.BASSVIS_RenderChannel(_visParam, stream, true);
          }
          else
          {
          RenderStarted = BassVis.BASSVIS_RenderChannel(_visParam, stream, false);
          }
        }
      }

      catch (Exception) {}

      return 1;
    }

    public override bool Close()
    {
      Bass.PlaybackStateChanged -= new BassAudioEngine.PlaybackStateChangedDelegate(PlaybackStateChanged);
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
        BassVis.BASSVIS_SetPlayState(_visParam, BASSVIS_PLAYSTATE.Stop);

        BassVis.BASSVIS_Free(_visParam);
        bool bFree = BassVis.BASSVIS_IsFree(_visParam);
        if (bFree)
        {
          _visParam.VisHandle = 0;
        }
        else
        {
          Log.Warn("Visualization Manager: Failed to unload Winamp viz module - {0}", VizPluginInfo.Name);
          _visParam.VisHandle = 0;
        }
      }           

      int tmpVis = BassVis.BASSVIS_GetModuleHandle(BASSVISKind.BASSVISKIND_WINAMP, VizPluginInfo.FilePath);
      if (tmpVis != 0)
      {
        int numModules = BassVis.BASSVIS_GetModulePresetCount(_visParam, VizPluginInfo.FilePath);
        BassVis.BASSVIS_Config(_visParam, 0);
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
        // Hide the Viswindow, so that we don't see it, while moving
        Win32API.ShowWindow(VisualizationWindow.Handle, Win32API.ShowWindowFlags.Hide);        
        _tmpVisParam = new BASSVIS_PARAM(BASSVISKind.BASSVISKIND_WINAMP);
        _tmpVisParam.VisGenWinHandle = VisualizationWindow.Handle;
        BassVis.BASSVIS_Resize(_tmpVisParam, 0, 0, newSize.Width, newSize.Height);
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

      try
      {
        using (Profile.Settings xmlreader = new Profile.MPSettings())
        {
          VizPluginInfo.FFTSensitivity = xmlreader.GetValueAsInt("musicvisualization", "fftSensitivity", 36);
          VizPluginInfo.PresetIndex = xmlreader.GetValueAsInt("musicvisualization", "preset", 0);
        }

        //Remove existing CallBacks
        BassVis.BASSVIS_WINAMPRemoveCallback();

        // Call Play befor use BASSVIS_ExecutePlugin (moved here)
        BassVis.BASSVIS_SetPlayState(_visParam, BASSVIS_PLAYSTATE.Play);

        // Set CallBack for PlayState
        _visCallback = BASSVIS_StateCallback;
        BassVis.BASSVIS_WINAMPSetStateCallback(_visCallback);

        // Hide the Viswindow, so that we don't see it, befor any Render
        Win32API.ShowWindow(VisualizationWindow.Handle, Win32API.ShowWindowFlags.Hide);

        // Create the Visualisation 
        BASSVIS_EXEC visExec = new BASSVIS_EXEC(VizPluginInfo.FilePath);
        visExec.AMP_ModuleIndex = VizPluginInfo.PresetIndex;
        visExec.AMP_UseOwnW1 = 1;
        visExec.AMP_UseOwnW2 = 1;
        // The flag below is needed for the Vis to have it's own message queue
        // Thus it is avoided that it steals focus from MP.
        visExec.AMP_UseFakeWindow = true; 
        
        BassVis.BASSVIS_ExecutePlugin(visExec, _visParam);
        
        if (_visParam.VisHandle != 0)
        {

          // Set the visualization window that was taken over from BASSVIS_ExecutePlugin
          BassVis.BASSVIS_SetVisPort(_visParam,
                                     _visParam.VisGenWinHandle,
                                     VisualizationWindow.Handle,
                                     0,
                                     0,
                                     VisualizationWindow.Width,
                                     VisualizationWindow.Height);

          BassVis.BASSVIS_SetOption(_visParam, BASSVIS_CONFIGFLAGS.BASSVIS_CONFIG_FFTAMP, VizPluginInfo.FFTSensitivity); 

          // SetForegroundWindow
          GUIGraphicsContext.form.Activate();
        }

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
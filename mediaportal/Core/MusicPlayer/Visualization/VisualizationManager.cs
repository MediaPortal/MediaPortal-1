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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using Microsoft.Win32;
using Un4seen.Bass.AddOn.Vis;

namespace MediaPortal.Visualization
{
  public class VisualizationManager : IVisualizationManager, IDisposable
  {
    #region Variables
    private BassAudioEngine Bass = null;
    private List<VisualizationInfo> _VisualizationPluginsInfo = new List<VisualizationInfo>();
    private VisualizationInfo.PluginType CurrentVizType = VisualizationInfo.PluginType.None;
    private IVisualization Viz = null;
    private string VizPath = string.Empty;
    private VisualizationWindow VizRenderWindow = null;

    private int _TargetFPS = 20;
    private BASS_VIS_PARAM _visParam = null;
    #endregion

    #region Properties

    public int TargetFPS
    {
      get { return _TargetFPS; }
      set
      {
        if (_TargetFPS == value)
        {
          return;
        }

        if (_TargetFPS > 60)
        {
          _TargetFPS = 60;
        }

        else if (_TargetFPS < 5)
        {
          _TargetFPS = 5;
        }

        _TargetFPS = value;
        SetVisualizationFPS(_TargetFPS);
      }
    }

    public List<VisualizationInfo> VisualizationPluginsInfo
    {
      get
      {
        if (_VisualizationPluginsInfo.Count == 0)
        {
          GetVisualizationPluginsInfo();
        }

        return _VisualizationPluginsInfo;
      }
    }

    public VisualizationInfo.PluginType CurrentVisualizationType
    {
      get { return CurrentVizType; }
    }
    #endregion

    #region ctor / dtor
    public VisualizationManager(BassAudioEngine bass, VisualizationWindow vizWindow)
    {
      Bass = bass;
      VisualizationBase.Bass = Bass;
      VizRenderWindow = vizWindow;

      if (bass != null)
      {
        Bass.PlaybackStateChanged += new BassAudioEngine.PlaybackStateChangedDelegate(OnPlaybackStateChanged);
      }

      SetVisualizationFPS(_TargetFPS);
    }

    public void Dispose()
    {
      Stop();

      if (Viz != null)
      {
        ((VisualizationBase) Viz).Dispose();
      }
    }
    #endregion

    #region Events
    private void OnPlaybackStateChanged(object sender, BassAudioEngine.PlayState oldState,
                                        BassAudioEngine.PlayState newState)
    {
      if (Viz == null)
      {
        return;
      }

      // Start the visualization render thread the first time an audio file starts playing.
      // Subsequent play state changes will be managed by the VisualizationWindow.
      if (!Viz.Initialized || newState == BassAudioEngine.PlayState.Playing)
      {
        VizRenderWindow.Run = true;
      }
    }
    #endregion

    #region Private Methods
    private bool IsGForceInstalled()
    {
      string mpVizDll = Path.Combine(Application.StartupPath, "mpviz.dll");

      if (!File.Exists(mpVizDll))
      {
        return false;
      }

      SoundSpectrumViz viz =
        new SoundSpectrumViz(new VisualizationInfo(VisualizationInfo.PluginType.GForce, "", "g-force", "", 0));
      bool engineInstalled = viz.IsEngineInstalled();
      viz.Dispose();

      return engineInstalled;
    }

    private bool IsWhiteCapInstalled()
    {
      string mpVizDll = Path.Combine(Application.StartupPath, "mpviz.dll");

      if (!File.Exists(mpVizDll))
      {
        return false;
      }

      SoundSpectrumViz viz =
        new SoundSpectrumViz(new VisualizationInfo(VisualizationInfo.PluginType.WhiteCap, "", "whitecap", "", 0));
      bool engineInstalled = viz.IsEngineInstalled();
      viz.Dispose();

      return engineInstalled;
    }

    private bool IsSoftSkiesInstalled()
    {
      // No support yet!
      return false;
    }

    private VisualizationInfo.PluginType GetVisualizationTypeFromPath(string path)
    {
      Log.Info("Visualization Manager: Getting visualization type from path - {0}", path);

      VisualizationInfo.PluginType vizType = VisualizationInfo.PluginType.None;

      if (path.Length == 0)
      {
        vizType = VisualizationInfo.PluginType.None;
      }

      else if (Path.GetExtension(path).ToLower().CompareTo(".svp") == 0)
      {
        vizType = VisualizationInfo.PluginType.Sonique;
      }

      else if (path.ToLower().CompareTo("g-force") == 0)
      {
        vizType = VisualizationInfo.PluginType.GForce;
      }

      else if (path.ToLower().CompareTo("whitecap") == 0)
      {
        vizType = VisualizationInfo.PluginType.WhiteCap;
      }

      else if (path.ToLower().CompareTo("softskies") == 0)
      {
        vizType = VisualizationInfo.PluginType.SoftSkies;
      }

      else
      {
        vizType = VisualizationInfo.PluginType.Unknown;
      }

      Log.Info("Visualization Manager: Visualization type is {0}", vizType);
      return vizType;
    }

    private void SetVisualizationFPS(int targetFPS)
    {
      if (VizRenderWindow != null)
      {
        VizRenderWindow.SetVisualizationTimer(targetFPS);
      }
    }

    private void CloseCurrentVisualization()
    {
      Log.Info("Visualization Manager: Closing current visualization plugin...");

      VizRenderWindow.StopVisualization();

      if (Viz != null)
      {
        bool result = Viz.Close();
        Viz = null;
        Log.Info("Visualization Manager: Visualization plugin close {0}", (result ? "succeeded" : "failed!"));
      }

      else
      {
        Log.Info("Visualization Manager: Visualization plugin close not required - nothing loaded");
      }
    }
    #endregion

    #region IVisualizationManager Members
    /// <summary>
    /// Search the Visaulation Path for supported Vis
    /// </summary>
    /// <returns></returns>
    public List<VisualizationInfo> GetVisualizationPluginsInfo()
    {
      _VisualizationPluginsInfo.Clear();
      _VisualizationPluginsInfo.Add(new VisualizationInfo("None", true));

      // Close any Visualisation, which we may have running, because of VizManager Init
      CloseCurrentVisualization();

      // Init BassVis. Must be done prior to any first call to BassVis
      BassVis.BASS_VIS_Init(BASSVISPlugin.BASSVISKIND_NONE, BassVis.GetWindowLongPtr(GUIGraphicsContext.form.Handle, (int)GWLIndex.GWL_HINSTANCE), VizRenderWindow.Handle);
      _visParam = new BASS_VIS_PARAM(BASSVISPlugin.BASSVISKIND_NONE);

      string skinFolderPath = Path.Combine(Application.StartupPath, @"musicplayer\plugins\visualizations");

      // Search for Sonique and Winamp Plugins
      string[] soniqueVisPaths = BassVis.BASS_VIS_FindPlugins(BASSVISPlugin.BASSVISKIND_SONIQUE, skinFolderPath, true);
      string[] winampVisPaths = BassVis.BASS_VIS_FindPlugins(BASSVISPlugin.BASSVISKIND_WINAMP, skinFolderPath, true);

      BassVis.BASS_VIS_Quit(_visParam);

      List<VisualizationInfo> wmpPluginsInfo = GetWMPPluginInfo();

      if (IsGForceInstalled())
      {
        VisualizationInfo vizInfo = new VisualizationInfo(VisualizationInfo.PluginType.GForce, string.Empty, "G-Force",
                                                          string.Empty, null);
        _VisualizationPluginsInfo.Add(vizInfo);
      }

      if (IsWhiteCapInstalled())
      {
        VisualizationInfo vizInfo = new VisualizationInfo(VisualizationInfo.PluginType.WhiteCap, string.Empty,
                                                          "WhiteCap", string.Empty, null);
        _VisualizationPluginsInfo.Add(vizInfo);
      }

      if (IsSoftSkiesInstalled())
      {
        VisualizationInfo vizInfo = new VisualizationInfo(VisualizationInfo.PluginType.SoftSkies, string.Empty,
                                                          "SoftSkies", string.Empty, null);
        _VisualizationPluginsInfo.Add(vizInfo);
      }

      if (wmpPluginsInfo != null)
      {
        for (int i = 0; i < wmpPluginsInfo.Count; i++)
        {
          _VisualizationPluginsInfo.Add(wmpPluginsInfo[i]);
        }
      }

      if (soniqueVisPaths != null)
      {
        BassVis.BASS_VIS_Init(BASSVISPlugin.BASSVISKIND_SONIQUE, BassVis.GetWindowLongPtr(GUIGraphicsContext.form.Handle, (int)GWLIndex.GWL_HINSTANCE), VizRenderWindow.Handle);
        _visParam = new BASS_VIS_PARAM(BASSVISPlugin.BASSVISKIND_SONIQUE);
        for (int i = 0; i < soniqueVisPaths.Length; i++)
        {
          string filePath = soniqueVisPaths[i];
          string name = Path.GetFileNameWithoutExtension(filePath);
          BASS_VIS_EXEC visExec = new BASS_VIS_EXEC(filePath);
          visExec.SON_Flags = BASSVISFlags.BASS_VIS_NOINIT; // don't execute the plugin yet
          visExec.SON_ConfigFile = Path.Combine(Path.GetDirectoryName(filePath), "vis.ini");
          BassVis.BASS_VIS_ExecutePlugin(visExec, _visParam);
          string pluginname = BassVis.BASS_VIS_GetPluginName(_visParam);
          if (pluginname != null)
          {
            name = pluginname;
          }
          BassVis.BASS_VIS_Free(_visParam);
          VisualizationInfo vizInfo = new VisualizationInfo(VisualizationInfo.PluginType.Sonique, filePath, name,
                                                            string.Empty, null);
          _VisualizationPluginsInfo.Add(vizInfo);
        }
        BassVis.BASS_VIS_Quit(_visParam);
      }
        
      if (winampVisPaths != null)
      {
        BassVis.BASS_VIS_Init(BASSVISPlugin.BASSVISKIND_WINAMP, BassVis.GetWindowLongPtr(GUIGraphicsContext.form.Handle, (int)GWLIndex.GWL_HINSTANCE), VizRenderWindow.Handle);
        _visParam = new BASS_VIS_PARAM(BASSVISPlugin.BASSVISKIND_WINAMP);
        for (int i = 0; i < winampVisPaths.Length; i++)
        {
          List<string> presets = new List<string>();
          string filePath = winampVisPaths[i];
          string name = Path.GetFileNameWithoutExtension(filePath);
          _visParam.VisHandle = BassVis.BASS_VIS_GetPluginHandle(BASSVISPlugin.BASSVISKIND_WINAMP, filePath);

          string pluginname = BassVis.BASS_VIS_GetPluginName(_visParam);
          if (pluginname != null)
            name = pluginname;

          // Get modules
          int numModules = BassVis.BASS_VIS_GetModulePresetCount(_visParam, filePath);
          if (numModules > 0)
          {
            if (numModules > 0)
            {
              for (int j = 0; j < numModules; j++)
              {
                presets.Add(BassVis.BASS_VIS_GetModulePresetName(_visParam, j, filePath));
              }
            }
            VisualizationInfo vizInfo = new VisualizationInfo(VisualizationInfo.PluginType.Winamp, filePath, name,
                                                              string.Empty, presets);
            if (!vizInfo.IsBlackListed)
            {
              _VisualizationPluginsInfo.Add(vizInfo);
            }
          }
          BassVis.BASS_VIS_Free(_visParam);
        }
        BassVis.BASS_VIS_Quit(_visParam);
      }
      return _VisualizationPluginsInfo;
    }

    /// <summary>
    /// Retrieve Information about WMP Plugins
    /// </summary>
    /// <returns></returns>
    private List<VisualizationInfo> GetWMPPluginInfo()
    {
      RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\MediaPlayer\\Objects\\Effects");
      string[] subKeyNames = key.GetSubKeyNames();

      List<VisualizationInfo> wmpPlugins = new List<VisualizationInfo>();

      for (int i = 0; i < subKeyNames.Length; i++)
      {
        RegistryKey subKey;

        subKey = key.OpenSubKey(subKeyNames[i] + "\\Properties");

        string sCLSID = (string) subKey.GetValue("classid");
        VisualizationInfo wmpVizInfo = LoadWMPPlugin(sCLSID);

        if (wmpVizInfo != null && !wmpVizInfo.IsBlackListed)
        {
          wmpPlugins.Add(wmpVizInfo);
        }
      }

      return wmpPlugins;
    }

    /// <summary>
    /// Load the specified WMP Plugin
    /// </summary>
    /// <param name="sCLSID"></param>
    /// <returns></returns>
    private VisualizationInfo LoadWMPPlugin(string sCLSID)
    {
      try
      {
        WMPVisualizationInfo wmpVizInfo = null;

        try
        {
          wmpVizInfo = new WMPVisualizationInfo(sCLSID);

          if (wmpVizInfo == null)
          {
            return null;
          }

          string vizName = wmpVizInfo.Title;
          List<string> presets = wmpVizInfo.Presets;

          VisualizationInfo vizPluginInfo = new VisualizationInfo(VisualizationInfo.PluginType.WMP, "", vizName, sCLSID,
                                                                  presets);
          return vizPluginInfo;
        }

        catch (Exception)
        {
          return null;
        }

        finally
        {
          if (wmpVizInfo != null)
          {
            wmpVizInfo.Dispose();
            wmpVizInfo = null;
          }
        }
      }

      catch (Exception ex)
      {
        Debug.WriteLine(ex.Message);
        return null;
      }
    }

    private bool InternalCreateVisualization(VisualizationInfo vizPluginInfo, bool isPreview)
    {
      CloseCurrentVisualization();
      CurrentVizType = vizPluginInfo.VisualizationType;

      switch (CurrentVizType)
      {
        case VisualizationInfo.PluginType.None:
        case VisualizationInfo.PluginType.Unknown:
          return false;

        case VisualizationInfo.PluginType.GForce:
          {
            Log.Info("Visualization Manager: Creating new G-Force visualization...");
            Viz = new SoundSpectrumViz(vizPluginInfo, VizRenderWindow);
            break;
          }

        case VisualizationInfo.PluginType.WhiteCap:
          {
            Log.Info("Visualization Manager: Creating new WhiteCap visualization...");
            Viz = new SoundSpectrumViz(vizPluginInfo, VizRenderWindow);
            break;
          }

        case VisualizationInfo.PluginType.SoftSkies:
          {
            Log.Info("Visualization Manager: Creating new SoftSkies visualization...");
            Viz = new SoundSpectrumViz(vizPluginInfo, VizRenderWindow);
            break;
          }

        case VisualizationInfo.PluginType.Sonique:
          {
            Log.Info("Visualization Manager: Creating new Sonique visualization...");
            Viz = new SoniqueViz(vizPluginInfo, VizRenderWindow);
            break;
          }

        case VisualizationInfo.PluginType.Winamp:
          {
            Log.Info("Visualization Manager: Creating new Winamp visualization...");
            Viz = new WinampViz(vizPluginInfo, VizRenderWindow);
            break;
          }

        case VisualizationInfo.PluginType.WMP:
          {
            Log.Info("Visualization Manager: Creating new Windows Media Player visualization...");
            Viz = new WMPViz(vizPluginInfo, VizRenderWindow);
            break;
          }

        default:
          return false;
      }

      VizRenderWindow.Visualization = Viz;

      if (Viz == null)
      {
        return false;
      }

      ((VisualizationBase) Viz).VisualizationCreated +=
        new VisualizationBase.VisualizationCreatedDelegate(OnVisualizationCreated);

      bool result = false;

      if (isPreview)
      {
        VizRenderWindow.IsPreviewVisualization = true;
        result = Viz.InitializePreview();
      }

      else
      {
        VizRenderWindow.IsPreviewVisualization = false;
        result = Viz.Initialize();
      }

      return result;
    }

    public bool CreateVisualization(VisualizationInfo vizPluginInfo)
    {
      return InternalCreateVisualization(vizPluginInfo, false);
    }

    public bool CreatePreviewVisualization(VisualizationInfo vizPluginInfo)
    {
      return InternalCreateVisualization(vizPluginInfo, true);
    }

    private void OnVisualizationCreated(object sender)
    {
      if (Bass.Playing)
      {
        if (Viz != null && Viz.Initialized)
        {
          VizRenderWindow.StartVisualization();
        }
      }
    }

    public bool ResizeVisualizationWindow(Size newSize)
    {
      if (VizRenderWindow != null)
      {
        Viz.WindowSizeChanged(newSize);
        VizRenderWindow.Size = newSize;
        return true;
      }

      else
      {
        return false;
      }
    }

    public bool Start()
    {
      if (Viz != null)
      {
        return Viz.Start();
      }

      return false;
    }

    public bool Pause()
    {
      if (Viz != null)
      {
        return Viz.Pause();
      }

      return false;
    }

    public bool Stop()
    {
      if (Viz != null)
      {
        return Viz.Stop();
      }

      return false;
    }

    public void ShutDown()
    {
      Dispose();
    }

    public void ConfigWinampViz()
    {
      Viz.Config();
    }

    public void InitWinampVis()
    {
      if (Viz.IsWinampVis())
      {
        Viz.SetOutputContext(VizRenderWindow.OutputContextType);
      }
    }

    #endregion
  }
}

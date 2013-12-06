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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MediaPortal.ExtensionMethods;
using MediaPortal.GUI.Library;
using MediaPortal.MusicPlayer.BASS;
using Microsoft.Win32;
using BassVis_Api;

namespace MediaPortal.Visualization
{
  public class VisualizationManager : IVisualizationManager, IDisposable
  {
    #region Variables

    private BassAudioEngine Bass = null;
    private List<VisualizationInfo> _VisualizationPluginsInfo = new List<VisualizationInfo>();
    private VisualizationInfo.PluginType CurrentVizType = VisualizationInfo.PluginType.None;
    private VisualizationInfo.PluginType OldVizType = VisualizationInfo.PluginType.None;
    private IVisualization Viz = null;
    private string VizPath = string.Empty;
    private VisualizationWindow VizRenderWindow = null;
    private string CurrentViz = null;
    private string OldViz = null;
    private int _TargetFPS = 20;
    private BASSVIS_PARAM _visParam = null;

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

    public VisualizationInfo.PluginType OldVisualizationType
    {
      get { return OldVizType; }
    }

    #endregion

    #region ctor / dtor

    public VisualizationManager(BassAudioEngine bass, VisualizationWindow vizWindow)
    {
      Bass = bass;
      VisualizationBase.Bass = Bass;
      VizRenderWindow = vizWindow;

      GetVisualizationPluginsInfo();

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
        ((VisualizationBase)Viz).SafeDispose();
      }

      Bass.PlaybackStateChanged -= new BassAudioEngine.PlaybackStateChangedDelegate(OnPlaybackStateChanged);
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
      viz.SafeDispose();

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
      viz.SafeDispose();

      return engineInstalled;
    }

    private bool IsSoftSkiesInstalled()
    {
      // No support yet!
      return false;
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
      try
      {
        _VisualizationPluginsInfo.Add(new VisualizationInfo("None", true));

        // Close any Visualisation, which we may have running, because of VizManager Init
        CloseCurrentVisualization();

        string skinFolderPath = Path.Combine(Application.StartupPath, @"musicplayer\plugins\visualizations");

        // Search for Sonique and Winamp Plugins
        string[] soniqueVisPaths = BassVis.BASSVIS_FindPlugins(BASSVISKind.BASSVISKIND_SONIQUE, skinFolderPath, true);

        // Search for Bassbox Plugins
        skinFolderPath = Path.Combine(Application.StartupPath, @"BBPlugin");
        string[] bassboxVisPaths = BassVis.BASSVIS_FindPlugins(BASSVISKind.BASSVISKIND_BASSBOX, skinFolderPath, true);

        // Note: Recursive Searches for Winamp Plugins are not supported
        // Winamp plugins expect itself to be stored in a folder named Plugins in the root of the executable. This is where we will search
        // So this is the folder where all the MP Plugins are stored as well
        skinFolderPath = Path.Combine(Application.StartupPath, @"plugins");
        string[] winampVisPaths = BassVis.BASSVIS_FindPlugins(BASSVISKind.BASSVISKIND_WINAMP, skinFolderPath, true);

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

        if (soniqueVisPaths != null && soniqueVisPaths[0] != "")
        {
          BassVis.BASSVIS_Init(BASSVISKind.BASSVISKIND_SONIQUE, GUIGraphicsContext.form.Handle);
          _visParam = new BASSVIS_PARAM(BASSVISKind.BASSVISKIND_SONIQUE);
          for (int i = 0; i < soniqueVisPaths.Length; i++)
          {
            string filePath = soniqueVisPaths[i];
            string name = Path.GetFileNameWithoutExtension(filePath);
            BASSVIS_EXEC visExec = new BASSVIS_EXEC(filePath);
            visExec.SON_Flags = BASSVISFlags.BASSVIS_NOINIT; // don't execute the plugin yet
            visExec.SON_ConfigFile = Path.Combine(Path.GetDirectoryName(filePath), "vis.ini");

            if (_visParam.VisHandle != 0)
            {
              BassVis.BASSVIS_Free(_visParam);
              bool bFree = BassVis.BASSVIS_IsFree(_visParam);
              if (bFree)
              {
                _visParam.VisHandle = 0;
              }
              else
              {
                Log.Warn("VisualisationManager: Failed to unload Sonique viz module - {0}", name);
                _visParam.VisHandle = 0;
              }
            }

            BassVis.BASSVIS_ExecutePlugin(visExec, _visParam);

            string pluginname = BassVis.BASSVIS_GetPluginName(_visParam);
            if (pluginname != null)
            {
              name = pluginname;
            }

            VisualizationInfo vizInfo = new VisualizationInfo(VisualizationInfo.PluginType.Sonique, filePath, name,
                                                              string.Empty, null);
            _VisualizationPluginsInfo.Add(vizInfo);
          }
          BassVis.BASSVIS_Quit(_visParam);
        }

        if (winampVisPaths != null && winampVisPaths[0] != "")
        {
          _visParam = new BASSVIS_PARAM(BASSVISKind.BASSVISKIND_WINAMP);
          for (int i = 0; i < winampVisPaths.Length; i++)
          {
            List<string> presets = new List<string>();
            string filePath = winampVisPaths[i];
            string name = Path.GetFileNameWithoutExtension(filePath);
            _visParam.VisHandle = BassVis.BASSVIS_GetModuleHandle(BASSVISKind.BASSVISKIND_WINAMP, filePath);

            string pluginname = BassVis.BASSVIS_GetPluginName(_visParam);
            if (pluginname != null)
              name = pluginname;

            // Get modules
            int numModules = BassVis.BASSVIS_GetModulePresetCount(_visParam, filePath);
            if (numModules > 0)
            {
              if (numModules > 0)
              {
                for (int j = 0; j < numModules; j++)
                {
                  presets.Add(BassVis.BASSVIS_GetModulePresetName(_visParam, j, filePath));
                }
              }
              VisualizationInfo vizInfo = new VisualizationInfo(VisualizationInfo.PluginType.Winamp, filePath, name,
                                                                string.Empty, presets);
              if (!vizInfo.IsBlackListed)
              {
                _VisualizationPluginsInfo.Add(vizInfo);
              }
            }
          }
        }

        if (bassboxVisPaths != null && bassboxVisPaths[0] != "")
        {
          // use only Filename BassBox can have Presetnames double           
          for (int i = 0; i < bassboxVisPaths.Length; i++)
          {
            string filePath = bassboxVisPaths[i];
            string name = Path.GetFileNameWithoutExtension(filePath);

            VisualizationInfo vizInfo = new VisualizationInfo(VisualizationInfo.PluginType.Bassbox, filePath, name,
                                                              string.Empty, null);
            _VisualizationPluginsInfo.Add(vizInfo);
          }
          BassVis.BASSVIS_Quit(_visParam);
        }
      }
      catch (Exception ex)
      {
        Log.Error("VisualisationManager: Failed to get viz plugins - {0}", ex.ToString());
      }
      return _VisualizationPluginsInfo;
    }

    /// <summary>
    /// Retrieve Information about WMP Plugins
    /// </summary>
    /// <returns></returns>
    private List<VisualizationInfo> GetWMPPluginInfo()
    {
      List<VisualizationInfo> wmpPlugins = new List<VisualizationInfo>();
      try
      {
        using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\MediaPlayer\\Objects\\Effects"))
        {
          string[] subKeyNames = key.GetSubKeyNames();


          for (int i = 0; i < subKeyNames.Length; i++)
          {
            using (RegistryKey subKey = key.OpenSubKey(subKeyNames[i] + "\\Properties"))
            {
              string sCLSID = (string)subKey.GetValue("classid");
              VisualizationInfo wmpVizInfo = LoadWMPPlugin(sCLSID);

              if (wmpVizInfo != null && !wmpVizInfo.IsBlackListed)
              {
                wmpPlugins.Add(wmpVizInfo);
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("VisualisationManager: Failed to get wmp plugin info - {0}", ex.ToString());
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
            wmpVizInfo.SafeDispose();
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
      // Close VisualType is not a BassVis Kind
      if (vizPluginInfo.VisualizationType != VisualizationInfo.PluginType.Sonique &&
          vizPluginInfo.VisualizationType != VisualizationInfo.PluginType.Winamp &&
          vizPluginInfo.VisualizationType != VisualizationInfo.PluginType.Bassbox)
      {
        CloseCurrentVisualization();
      }
      else
      {
        if (OldVizType != vizPluginInfo.VisualizationType)
        {
          CloseCurrentVisualization();
          OldVizType = vizPluginInfo.VisualizationType;
        }
        else
        {
          switch (OldVizType)
          {
            case VisualizationInfo.PluginType.None:
            case VisualizationInfo.PluginType.Unknown:
              return false;

            case VisualizationInfo.PluginType.Sonique:
              {
                Log.Info("Visualization Manager: Free Sonique visualization...");
                BassVis.BASSVIS_Free(Viz.VizParam);
                bool bFree = BassVis.BASSVIS_IsFree(Viz.VizParam);
                if (bFree)
                {
                  Viz.VizParam.VisHandle = 0;
                  Log.Info("Visualization Manager: Sonique visualization " + OldViz + " free succes");
                }
                break;
              }
            case VisualizationInfo.PluginType.Winamp:
              {
                Log.Info("Visualization Manager: Free Winamp visualization...");
                BassVis.BASSVIS_Free(Viz.VizParam);
                bool bFree = BassVis.BASSVIS_IsFree(Viz.VizParam);
                if (bFree)
                {
                  Viz.VizParam.VisHandle = 0;
                  Log.Info("Visualization Manager: Winamp visualization " + OldViz + " free succes");
                }
                break;
              }

            case VisualizationInfo.PluginType.Bassbox:
              {
                Log.Info("Visualization Manager: Free Bassbox visualization...");
                BassVis.BASSVIS_Free(Viz.VizParam);
                bool bFree = BassVis.BASSVIS_IsFree(Viz.VizParam);
                if (bFree)
                {
                  Viz.VizParam.VisHandle = 0;
                  Log.Info("Visualization Manager: Bassbox visualization " + OldViz + " free succes");
                }
                break;
              }
          }
        }
      }

      CurrentVizType = vizPluginInfo.VisualizationType;
      OldViz = vizPluginInfo.Name;
      CurrentViz = OldViz;

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

        case VisualizationInfo.PluginType.Bassbox:
          {
            Log.Info("Visualization Manager: Creating new Bassbox visualization...");
            Viz = new BassboxViz(vizPluginInfo, VizRenderWindow);
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

      ((VisualizationBase)Viz).VisualizationCreated +=
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

    public void InitSoniqueVis()
    {
      if (Viz.IsSoniqueVis())
      {
        Viz.SetOutputContext(VizRenderWindow.OutputContextType);
      }
    }

    public void InitBassboxVis()
    {
      if (Viz.IsSoniqueVis())
      {
        Viz.SetOutputContext(VizRenderWindow.OutputContextType);
      }
    }

    private int GetCurrentVizIndex()
    {
      int i = -1;
      foreach (VisualizationInfo visInfo in _VisualizationPluginsInfo)
      {
        i++;
        if (visInfo.Name == CurrentViz)
        {
          break;
        }
      }
      return i;
    }

    public void GetNextVis()
    {
      int i = GetCurrentVizIndex();

      if (i > -1 && i < _VisualizationPluginsInfo.Count - 1)
      {
        if (CreateVisualization(_VisualizationPluginsInfo[i + 1]))
        {
          VizRenderWindow.Run = true;
          VisualizationInfo currentVis = _VisualizationPluginsInfo[i + 1];
          SaveCurrentViz(currentVis);
        }
      }
      else
      {
        // if last plugin entry reached then use the first plugin in the list
        CurrentViz =  _VisualizationPluginsInfo[0].Name;
        OldVizType = VisualizationInfo.PluginType.None;
        GetNextVis();
      }
    }

    public void GetPrevVis()
    {
      int i = GetCurrentVizIndex();

      if (i > 1) // We still have "None" in the List
      {
        if (CreateVisualization(_VisualizationPluginsInfo[i - 1]))
        {
          VizRenderWindow.Run = true;
          VisualizationInfo currentVis = _VisualizationPluginsInfo[i - 1];
          SaveCurrentViz(currentVis);
        }
      }
      else
      {
        // if first plugin entry reached then use last entry in the list 
        CurrentViz = _VisualizationPluginsInfo[_VisualizationPluginsInfo.Count - 1].Name;
        OldVizType = VisualizationInfo.PluginType.None;
        GetPrevVis();
      }
    }

    private void SaveCurrentViz(VisualizationInfo vizPluginInfo)
    {
      using (Profile.Settings xmlwriter = new Profile.MPSettings())
      {
        xmlwriter.SetValue("musicvisualization", "name", vizPluginInfo.Name);
        xmlwriter.SetValue("musicvisualization", "vizType", ((int)vizPluginInfo.VisualizationType).ToString());
        xmlwriter.SetValue("musicvisualization", "path", vizPluginInfo.FilePath);
        xmlwriter.SetValue("musicvisualization", "clsid", vizPluginInfo.CLSID);
        xmlwriter.SetValue("musicvisualization", "preset", vizPluginInfo.PresetIndex.ToString());
      }
    }

    #endregion
  }
}
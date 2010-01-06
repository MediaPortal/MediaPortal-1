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
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Vis;

namespace MediaPortal.Visualization
{
  public class SoniqueViz : VisualizationBase
  {
    #region Variables

    private IntPtr dataPtr = IntPtr.Zero;
    private IntPtr fftPtr = IntPtr.Zero;
    private bool firstRun = true;
    private BASS_VIS_EXEC visExec;

    #endregion

    #region Constructors/Destructors

    public SoniqueViz(VisualizationInfo vizPluginInfo, VisualizationWindow vizCtrl)
      : base(vizPluginInfo, vizCtrl) {}

    #endregion

    #region IVisualisation Implementation

    public override bool Initialize()
    {
      try
      {
        Log.Info("Visualization Manager: Initializing Sonique visualization: {0}", VizPluginInfo.Name);

        if (VizPluginInfo == null)
        {
          Log.Error("Visualization Manager: Sonique visualization engine initialization failed! Reason:{0}",
                    "Missing or invalid VisualizationInfo object.");

          return false;
        }

        firstRun = true;
        BassVis.BASS_VIS_SetOption(_visParam, BASSVISConfig.BASS_SONIQUEVIS_CONFIG_SLOWFADE, 25);
        bool result = SetOutputContext(VisualizationWindow.OutputContextType);
        _Initialized = result && _visParam.VisHandle != 0;
      }

      catch (Exception ex)
      {
        Log.Error(
          "Visualization Manager: Sonique visualization engine initialization failed with the following exception {0}",
          ex);
        return false;
      }

      return Initialized;
    }

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
        if (VisualizationWindow == null || !VisualizationWindow.Visible)
        {
          return 0;
        }

        IntPtr hdc = VisualizationWindow.CompatibleDC;

        int stream = 0;
        if (Bass != null)
        {
          stream = (int)Bass.GetCurrentVizStream();
        }

        BassVis.BASS_VIS_SONIQUERenderToDC(BASSVISPlugin.BASSVISKIND_SONIQUE, (IntPtr)_visParam.VisHandle, stream, hdc);
      }

      catch (Exception) {}

      return 1;
    }

    public override bool Close()
    {
      try
      {
        base.Close();
        return true;
      }

      catch (Exception ex)
      {
        Console.WriteLine("Visualization Manager: Sonique Close caused an exception: {0}", ex.Message);
        return false;
      }
    }

    public override bool WindowChanged(VisualizationWindow vizWindow)
    {
      base.WindowChanged(vizWindow);

      if (vizWindow == null)
      {
        return false;
      }

      BassVis.BASS_VIS_Resize(_visParam, 0, 0, VisualizationWindow.Width, VisualizationWindow.Height);

      return true;
    }

    public override bool WindowSizeChanged(Size newSize)
    {
      BassVis.BASS_VIS_Resize(_visParam, 0, 0, VisualizationWindow.Width, VisualizationWindow.Height);
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

      // If width or height are 0 the call to BASS_SONIQUEVIS_CreateVis will fail.  
      // If width or height are 1 the window is in transition so we can ignore it.
      if (VisualizationWindow.Width <= 1 || VisualizationWindow.Height <= 1)
      {
        return false;
      }

      if (VizPluginInfo == null || VizPluginInfo.FilePath.Length == 0 || !File.Exists(VizPluginInfo.FilePath))
      {
        return false;
      }

      string vizPath = VizPluginInfo.FilePath;
      string configFile = Path.Combine(Path.GetDirectoryName(vizPath), "vis.ini");

      if (_visParam.VisHandle != 0)
      {
        try
        {
          BassVis.BASS_VIS_Free(_visParam);
        }
        catch (AccessViolationException) {}

        _visParam.VisHandle = 0;
      }

      try
      {
        visExec = new BASS_VIS_EXEC(vizPath);
        visExec.SON_ConfigFile = configFile;
        visExec.SON_Flags = BASSVISFlags.BASS_VIS_DEFAULT;
        visExec.SON_PaintHandle = VisualizationWindow.CompatibleDC;
        visExec.Width = VisualizationWindow.Width;
        visExec.Height = VisualizationWindow.Height;
        visExec.Left = VisualizationWindow.Left;
        visExec.Top = VisualizationWindow.Top;
        BassVis.BASS_VIS_ExecutePlugin(visExec, _visParam);
      }
      catch (Exception) {}
      firstRun = false;
      return _visParam.VisHandle != 0;
    }

    #endregion
  }
}
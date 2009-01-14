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
using MediaPortal.GUI.Library;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Vis;

namespace MediaPortal.Visualization
{
  public class SoniqueViz : VisualizationBase
  {
    private int VisChannel = 0;

    public SoniqueViz(VisualizationInfo vizPluginInfo, VisualizationWindow vizCtrl)
      : base(vizPluginInfo, vizCtrl)
    {
      Log.Info("Visualization Manager: Creating Sonique visualization...");
      BassVis.BASS_SONIQUEVIS_DestroyFakeSoniqueWnd();
      BassVis.BASS_SONIQUEVIS_CreateFakeSoniqueWnd();
      Log.Info("Visualization Manager: Sonique visualization created.");
    }

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

        BassVis.BASS_SONIQUEVIS_SetConfig(BASSSONIQUEVISConfig.BASS_SONIQUEVIS_CONFIG_SLOWFADE, 25);
        bool result = SetOutputContext(VisualizationWindow.OutputContextType);
        _Initialized = result && VisChannel != 0;
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
      IntPtr hdc = IntPtr.Zero;
      Graphics destGraphics = null;

      try
      {
        if (VisualizationWindow == null || !VisualizationWindow.Visible)
        {
          return 0;
        }


        if (VisualizationWindow.InvokeRequired)
        {
          ThreadSafeRenderDelegate d = new ThreadSafeRenderDelegate(RenderVisualization);
          VisualizationWindow.Invoke(d);
          return 0;
        }

        if (VisualizationWindow.OutputContextType == OutputContextType.DeviceContext)
        {
          hdc = VisualizationWindow.CompatibleDC;
        }

        else if (VisualizationWindow.OutputContextType == OutputContextType.WindowHandle)
        {
          destGraphics = Graphics.FromHwnd(VisualizationWindow.CompatibleDC);
          hdc = destGraphics.GetHdc();
        }

        else
        {
          return -1;
        }


        int stream = 0;
        if (!_IsPreviewVisualization)
        {
          if (Bass != null)
          {
            stream = (int) Bass.GetCurrentVizStream();
          }

          if (stream != 0)
          {
            // Now we need to get the sample FFT data from the channel for rendering purposes
            float[] data = new float[1024*4];
            float[] fftData = new float[512]; // to receive 512 byte of fft
            Un4seen.Bass.Bass.BASS_ChannelGetData(stream, ref data[0], data.Length);
            Un4seen.Bass.Bass.BASS_ChannelGetData(stream, ref fftData[0], (int) BASSData.BASS_DATA_FFT1024);

            double pos = 1000*Bass.CurrentPosition; // Convert to milliseconds
            BassVis.BASS_SONIQUEVIS_Render2(VisChannel, ref data[0], ref fftData[0], hdc, BASSStream.BASS_SAMPLE_FLOAT,
                                            (int) pos);
          }
        }
        else
        {
          BassVis.BASS_SONIQUEVIS_Render(VisChannel, stream, hdc);
        }
      }

      catch (Exception)
      {
      }

      finally
      {
        if (destGraphics != null)
        {
          if (hdc != IntPtr.Zero)
          {
            destGraphics.ReleaseHdc(hdc);
          }

          destGraphics.Dispose();
        }
      }

      return 1;
    }

    public override bool Close()
    {
      try
      {
        if (VisChannel != 0)
        {
          BassVis.BASS_SONIQUEVIS_DestroyFakeSoniqueWnd();
          BassVis.BASS_SONIQUEVIS_Free(VisChannel);
          VisChannel = 0;
        }

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

      bool result = SetOutputContext(VisualizationWindow.OutputContextType);
      return result;
    }

    public override bool WindowSizeChanged(Size newSize)
    {
      bool result = SetOutputContext(VisualizationWindow.OutputContextType);
      return result;
    }

    public override bool SetOutputContext(OutputContextType outputType)
    {
      if (VisualizationWindow == null)
      {
        return false;
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

      if (VisChannel != 0)
      {
        bool result;
        result = BassVis.BASS_SONIQUEVIS_Free(VisChannel);
        VisChannel = 0;

        BassVis.BASS_SONIQUEVIS_DestroyFakeSoniqueWnd();
      }

      BassVis.BASS_SONIQUEVIS_CreateFakeSoniqueWnd();
      VisChannel = BassVis.BASS_SONIQUEVIS_CreateVis(vizPath, configFile, BASSVISCreate.BASS_VIS_DEFAULT,
                                                     VisualizationWindow.Width, VisualizationWindow.Height);
      return VisChannel != 0;
    }
  }
}
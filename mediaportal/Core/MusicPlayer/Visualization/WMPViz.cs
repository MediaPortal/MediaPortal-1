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
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;
using Un4seen.Bass;

namespace MediaPortal.Visualization
{
  public class WMPInterop
  {
    public const int MaxSamples = 1024;

    public enum AudioState
    {
      Stopped = 0,
      Paused = 1,
      Playing = 2
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct TimedLevel
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2*MaxSamples)] public byte[] frequency;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2*MaxSamples)] public byte[] waveform;

      public int State;
      public Int64 TimeStamp;
    }

    [DllImport("mpviz.dll", CharSet = CharSet.Auto)]
    internal static extern bool InitWMPEngine([MarshalAs(UnmanagedType.LPWStr)] string strVizCLSID, int presetIndex,
                                              VisualizationBase.OutputContextType outputContextType, IntPtr callBack,
                                              IntPtr hOutput, ref VisualizationBase.RECT rect);

    [DllImport("mpviz.dll", CharSet = CharSet.Auto)]
    internal static extern int RenderWMP(IntPtr pData, ref VisualizationBase.RECT rect);

    [DllImport("mpviz.dll", CharSet = CharSet.Auto)]
    internal static extern bool SetOutputWMP(VisualizationBase.OutputContextType outputContextType, IntPtr hOutput);
  }

  public class WMPViz : VisualizationBase
  {
    #region Variables

    private WMPInterop.TimedLevel TimedLvl = new WMPInterop.TimedLevel();

    #endregion

    #region Properties

    public override bool PreRenderRequired
    {
      get { return true; }
    }

    #endregion

    public WMPViz(VisualizationInfo vizPluginInfo, VisualizationWindow vizCtrl)
      : base(vizPluginInfo, vizCtrl)
    {
      _IsPreviewVisualization = false;
    }

    public override void Dispose()
    {
      base.Dispose();

      SoundSpectrumInterop.Quit();
      SoundSpectrumInterop.ShutDown();
    }

    public override bool Initialize()
    {
      bool result = false;

      try
      {
        Log.Info("  Visualization Manager: Initializing WMP visualization...");

        RECT rect = new RECT();
        rect.left = 0;
        rect.top = 0;
        rect.right = VisualizationWindow.Width;
        rect.bottom = VisualizationWindow.Height;

        OutputContextType outputType = VisualizationWindow.OutputContextType;
        result = WMPInterop.InitWMPEngine(VizPluginInfo.CLSID, VizPluginInfo.PresetIndex, outputType, IntPtr.Zero,
                                          VisualizationWindow.CompatibleDC, ref rect);
        _Initialized = result;
        Log.Info("  Visualization Manager: WMP visualization initialization {0}", (result ? "succeeded." : "failed!"));
      }

      catch (Exception ex)
      {
        Console.WriteLine("CreateGForceVisualization failed with the following exception: {0}", ex);
        Log.Error(
          "  Visualization Manager: WMP visualization engine initialization failed with the following exception {0}", ex);
        return false;
      }

      return result;
    }

    public override bool InitializePreview()
    {
      base.InitializePreview();
      return Initialize();
    }

    public override int RenderVisualization()
    {
      if (VisualizationWindow.InvokeRequired)
      {
        ThreadSafeRenderDelegate d = new ThreadSafeRenderDelegate(RenderVisualization);
        return (int) VisualizationWindow.Invoke(d);
      }

      RECT rect = new RECT();
      rect.left = 0;
      rect.top = 0;
      rect.right = VisualizationWindow.Width;
      rect.bottom = VisualizationWindow.Height;

      TimedLvl.waveform = new byte[2048];
      TimedLvl.frequency = new byte[2048];
      TimedLvl.State = (int) WMPInterop.AudioState.Playing;

      bool gotWaveData = GetWaveData(ref TimedLvl.waveform);
      bool gotfreqData = GetFftData(ref TimedLvl.frequency);

      IntPtr pTimedLevel = Marshal.AllocHGlobal(Marshal.SizeOf(typeof (WMPInterop.TimedLevel)));
      Marshal.StructureToPtr(TimedLvl, pTimedLevel, false);

      int result = 0;
      result = WMPInterop.RenderWMP(pTimedLevel, ref rect);

      Marshal.FreeHGlobal(pTimedLevel);
      return result;
    }

    public override bool Close()
    {
      try
      {
        SoundSpectrumInterop.Quit();
        SoundSpectrumInterop.ShutDown();
      }

      catch
      {
        return false;
      }

      return true;
    }

    public override bool WindowSizeChanged(Size newSize)
    {
      bool result = SetOutputContext(VisualizationWindow.OutputContextType);
      return result;
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

    public override bool SetOutputContext(OutputContextType outputType)
    {
      if (VisualizationWindow == null)
      {
        return false;
      }

      // If width or height are 0 the call will fail.  If width or height are 1 the window is in transition
      if (VisualizationWindow.Width <= 1 || VisualizationWindow.Height <= 1)
      {
        return false;
      }

      bool result = WMPInterop.SetOutputWMP(outputType, VisualizationWindow.CompatibleDC);
      return result;
    }


    private bool GetWaveData(ref byte[] audioData)
    {
      int multiplier = 4;
      const int sampleLength = 2048;
      float[] buf = new float[sampleLength*multiplier];
      //float[] pcm = new float[sampleLength]; ;

      if (!_IsPreviewVisualization)
      {
        int stream = (int) Bass.GetCurrentVizStream();

        if (stream == 0)
        {
          return false;
        }

        int len = Un4seen.Bass.Bass.BASS_ChannelGetData(stream, ref buf[0], buf.Length);
        int x = 0;

        // The pcm buffer contains interleaved left, right, left, ... channel info
        // We need the data to be ordered as follows: 
        //      Left Channel:   bytes 0 - 1023 
        //      Right Channel:  bytes 1024 - 2047 
        for (int i = 0; i < sampleLength; i++)
        {
          try
          {
            // Following code taken as a sugesstion from Symphy (Thx for the contribution)

            // Convert float value between -1 and 1 to 0 and 255
            float val = (buf[i] + 1f)*127.5f;

            if (val < 0)
            {
              val = 0;
            }
            else if (val > 255)
            {
              val = 255;
            }

            // Left Channel
            if (i%2 == 0)
            {
              audioData[x] = (byte) val;
            }
              // Right Channel
            else
            {
              audioData[x + 1024] = (byte) val;
              x++;
            }
          }

          catch (Exception)
          {
            return false;
          }
        }
      }

      else
      {
        // No need to seed it
        Random rand = new Random();
        int x = 0;

        for (int i = 0; i < sampleLength; i++)
        {
          // Left Channel
          if (i%2 == 0)
          {
            audioData[x] = (byte) rand.Next(0, 127);
          }

            // Right Channel
          else
          {
            audioData[x + 1024] = (byte) rand.Next(128, 255);
          }
        }
      }

      return true;
    }

    private bool GetFftData(ref byte[] audioData)
    {
      float[] fft = null;

      if (!_IsPreviewVisualization)
      {
        int stream = (int) Bass.GetCurrentVizStream();

        if (stream == 0)
        {
          return false;
        }

        fft = new float[1024];
        Un4seen.Bass.Bass.BASS_ChannelGetData(stream, ref fft[0], (int) BASSData.BASS_DATA_FFT2048);
      }

      else
      {
        fft = new float[1024];
        float stepValue = 1.0f/1024f;

        Random rand = new Random();

        for (int i = 0; i < fft.Length; i++)
        {
          float val = (float) rand.Next(0, (1024/2))*stepValue;
          fft[i] = val;
        }
      }

      float fftDataLen = (float) fft.Length;

      // The first value is the DC component so we'll skip it and start at index 1
      for (int i = 1; i < 1024; i++)
      {
        try
        {
          // Following code taken as a sugesstion from Symphy (Thx for the contribution)
          float val = (90f + ((float) Math.Log10(fft[i])*20f))*(255f/90f);
          if (val < 0)
          {
            val = 0;
          }
          else if (val > 255)
          {
            val = 255;
          }

          audioData[i] = (byte) val;
          audioData[i + 1024] = (byte) val;
        }

        catch (Exception ex)
        {
          Console.WriteLine("Exception occurred at index {0}: {1}", i, ex.Message);
          return false;
        }
      }

      return true;
    }
  }
}
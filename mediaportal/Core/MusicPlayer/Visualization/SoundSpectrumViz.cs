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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;
using Un4seen.Bass;

namespace MediaPortal.Visualization
{
  public class SoundSpectrumInterop
  {
    #region Visualization Helper Interop Function Definitions

    // SS_GetSoundData_Params -- see SS_GetSoundData.
    public struct SS_GetSoundData_Params
    {
      public int mSize; // Init to sizeof( SS_GetSoundData_Params )

      // PCM data...
      public int inN; // Number of samples requested (ie, number of elements in outSamples[])
      public float inScale; // Scales what gets returned in outSamples[];

      public IntPtr outSamples;
                    // IntPtr to a float[] -- Caller should set to an array to be written to (or NULL if not desired)

      public int outN; // Number of elements of outSamples filled/returned by the SSVisualAPI host.

      // FFT data...
      public int inNumBins; // Number of bins requested (ie, the number of elements in outFFT[])
      public int inStepsPerBin; // The freq span of the spectrum -- this is used when an FFT array is already available.

      public int inStartBin;
                 // The bin num the spectrum starts at -- this is used when an FFT array is already available.

      public IntPtr inFFTParams;
                    // SSFFTParams* - Params to use for an FFT -- this is used when only a PCM array is available.

      public IntPtr outFFT;
                    // IntPtr to a float[] -- Caller should set to an array to be written to (or NULL if not desired)

      public int outNumBins; // Number of elements of outFT filled/returned by the SSVisualAPI host.
    } ;

    [StructLayout(LayoutKind.Sequential)]
    public struct SSFrameBuf
    {
      public UInt32 mBitsPerPixel;
      public UInt32 mBytesPerRow;
      public Int32 mWidth;
      public Int32 mHeight;
      public IntPtr mBits;
    } ;

    public const int SS_GetSoundData = 1197831251;

    [DllImport("mpviz.dll", CharSet = CharSet.Auto)]
    internal static extern bool IsVisualizationInstalled(string vizName);

    [DllImport("mpviz.dll", CharSet = CharSet.Auto)]
    internal static extern bool InitEngine([MarshalAs(UnmanagedType.LPWStr)] string vizName,
                                           VisualizationBase.OutputContextType outputContextType,
                                           SSCallbackDelegate ssCallback, IntPtr hOutput,
                                           ref VisualizationBase.RECT rect);

    [DllImport("mpviz.dll", CharSet = CharSet.Auto)]
    internal static extern bool IsInitialized();

    [DllImport("mpviz.dll", CharSet = CharSet.Auto)]
    internal static extern int Render();

    [DllImport("mpviz.dll", CharSet = CharSet.Auto)]
    internal static extern bool SetOutput(string vizName, VisualizationBase.OutputContextType outputContextType,
                                          IntPtr hOutput, ref VisualizationBase.RECT rect);

    [DllImport("mpviz.dll", CharSet = CharSet.Auto)]
    internal static extern bool Resize(IntPtr hVizWndOrBuffer, ref VisualizationBase.RECT rect, bool isFullscreen);

    // This method MUST be called before the visualization window handle is destroyed!
    [DllImport("mpviz.dll", CharSet = CharSet.Auto)]
    internal static extern bool Quit();

    // This method MUST be called before the visualization window handle is destroyed!
    [DllImport("mpviz.dll", CharSet = CharSet.Auto)]
    internal static extern void ShutDown();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int SSCallbackDelegate(int inCallbackParam, int inMessage, int inParam1, int inParam2);

    internal static SSCallbackDelegate SSCallback = null;

    #endregion
  }

  public class SoundSpectrumViz : VisualizationBase
  {
    #region Variables

    private string SSVisualizationName = string.Empty;
    private bool _autoHideMouse = false;

    #endregion

    #region Properties

    public override bool PreRenderRequired
    {
      get { return true; }
    }

    #endregion

    public SoundSpectrumViz(VisualizationInfo vizPluginInfo)
    {
      SSVisualizationName = vizPluginInfo.Name;
    }

    public SoundSpectrumViz(VisualizationInfo vizPluginInfo, VisualizationWindow vizCtrl)
      : base(vizPluginInfo, vizCtrl)
    {
      Log.Info("Visualization Manager: Creating {0} callback...", vizPluginInfo.Name);
      SoundSpectrumInterop.SSCallback = new SoundSpectrumInterop.SSCallbackDelegate(SSCallbackFunc);
      SSVisualizationName = vizPluginInfo.Name;
      using (
        Settings xmlreader =
          new Settings(Configuration.Config.GetFile(Configuration.Config.Dir.Config, "MediaPortal.xml")))
      {
        _autoHideMouse = xmlreader.GetValueAsBool("general", "autohidemouse", true);
      }
      Log.Info("Visualization Manager: Callback created.");
    }

    public override void Dispose()
    {
      base.Dispose();

      SoundSpectrumInterop.Quit();
      SoundSpectrumInterop.ShutDown();
    }

    public override bool IsEngineInstalled()
    {
      return SoundSpectrumInterop.IsVisualizationInstalled(SSVisualizationName);
    }

    private delegate bool ThreadSafeInitDelegate();

    public override bool Initialize()
    {
      bool result = false;

      try
      {
        DateTime dt = DateTime.Now;
        Log.Info("Visualization Manager: Initializing {0} visualization engine...", VizPluginInfo.Name);

        RECT rect = new RECT();
        rect.left = 0;
        rect.top = 0;
        rect.right = VisualizationWindow.Width;
        rect.bottom = VisualizationWindow.Height;

        OutputContextType outputType = VisualizationWindow.OutputContextType;
        result = SoundSpectrumInterop.InitEngine(SSVisualizationName, outputType, SoundSpectrumInterop.SSCallback,
                                                 VisualizationWindow.CompatibleDC, ref rect);

        // Soundspectrum Graphics always show the cursor, so let's hide it here
        if (GUIGraphicsContext.Fullscreen && _autoHideMouse)
        {
          Cursor.Hide();
        }

        Log.Info("Visualization Manager: {0} visualization engine initialization {1}", VizPluginInfo.Name,
                 (result ? "succeeded." : "failed!"));
      }

      catch (Exception ex)
      {
        Console.WriteLine("CreateGForceVisualization failed with the following exception: {0}", ex);
        Log.Error(
          "  Visualization Manager: {0} visualization engine initialization failed with the following exception {1}",
          VizPluginInfo.Name, ex);
        return false;
      }

      return result;
    }

    public override bool InitializePreview()
    {
      base.InitializePreview();
      return Initialize();
    }

    public override int PreRenderVisualization()
    {
      // The first Render call can take a significant amount of time to return.
      // Consider calling this method from a worker thread to prevent the main thread 
      // from blocking while the SoundSpectrum engine does it's own internal initialization.
      //if (VisualizationWindow.InvokeRequired)
      //{
      //    ThreadSafeRenderDelegate d = new ThreadSafeRenderDelegate(PreRenderVisualization);
      //    return (int)VisualizationWindow.Invoke(d);
      //}

      try
      {
        // Soundspectrum Graphics always show the cursor, so let's hide it here
        if (GUIGraphicsContext.Fullscreen && _autoHideMouse)
        {
          Cursor.Hide();
        }

        return SoundSpectrumInterop.Render();
      }

      catch
      {
        return 0;
      }
    }

    public override bool Config()
    {
      return false;
    }

    public override int RenderVisualization()
    {
      ////if (VisualizationWindow.InvokeRequired)
      ////{
      ////    ThreadSafeRenderDelegate d = new ThreadSafeRenderDelegate(RenderVisualization);
      ////    return (int)VisualizationWindow.Invoke(d);
      ////}

      try
      {
        if (VisualizationWindow == null || !VisualizationWindow.Visible)
        {
          return 0;
        }

        int sleepMS = SoundSpectrumInterop.Render();
        return sleepMS;
      }

      catch (Exception ex)
      {
        Console.WriteLine("Visualization: {0} Render Exception: {1}", VizPluginInfo.Name, ex);
        return 0;
      }
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

      // If width or height are 0 the call will fail.  If width or height are equal to 1 
      // the window is in transition.  
      if (VisualizationWindow.Width <= 1 || VisualizationWindow.Height <= 1)
      {
        return false;
      }

      RECT rect = new RECT();
      rect.left = 0;
      rect.top = 0;
      rect.right = VisualizationWindow.Width;
      rect.bottom = VisualizationWindow.Height;

      bool result = SoundSpectrumInterop.SetOutput(SSVisualizationName, outputType, VisualizationWindow.CompatibleDC,
                                                   ref rect);

      // Soundspectrum Graphics always show the cursor, so let's hide it here
      if (GUIGraphicsContext.Fullscreen && _autoHideMouse)
      {
        Cursor.Hide();
      }

      return result;
    }

    #region SoundSpectrum Callback

    private int NotImplemented = -55012;
    //int NoError = 0;

    private int SSCallbackFunc(int inCallbackParam, int inMessage, int inParam1, int inParam2)
    {
      // return a non zero value if the request was handled
      int returnCode = 0;

      if (!_Initialized)
      {
        _Initialized = true;
        this.VisualizationLoaded();
      }

      if (inMessage == SoundSpectrumInterop.SS_GetSoundData)
      {
        SoundSpectrumInterop.SS_GetSoundData_Params soundDataParams;
        soundDataParams =
          (SoundSpectrumInterop.SS_GetSoundData_Params)
          Marshal.PtrToStructure((IntPtr) inParam1, typeof (SoundSpectrumInterop.SS_GetSoundData_Params));

        if (!IsPreviewVisualization && Bass.State != BassAudioEngine.PlayState.Playing)
        {
          soundDataParams.outN = 0;
          return 0;
        }

        int multiplier = 4;
        int fDataLen = 0;
        int stream = 0;
        bool hasData = false;

        if (!_IsPreviewVisualization)
        {
          stream = (int) Bass.GetCurrentVizStream();
        }

        if (soundDataParams.inN > 0)
        {
          // Set the PCM data

          int reqDataLen = soundDataParams.inN*multiplier;
          float[] pcm = new float[reqDataLen];
          int len = 0;

          if (!IsPreviewVisualization)
          {
            len = Un4seen.Bass.Bass.BASS_ChannelGetData(stream, ref pcm[0], reqDataLen);

            if (len < 1)
            {
              soundDataParams.outN = 0;
              return 0;
            }

            fDataLen = len/multiplier;
          }

            // We're in preview mode so we'll generate dummy FFT data so the viz
            // looks like it's doing something...
          else
          {
            Random rand = new Random();

            for (int i = 0; i < pcm.Length; i++)
            {
              float val = 1.0f/(float) rand.Next(0, 32768);

              // Left Channel
              if (i%2 == 0)
              {
                //pcm[i] = (short)rand.Next(-32767, 1);
                pcm[i] = val;
              }

                // Right Channel
              else
              {
                //pcm[i] = (short)rand.Next(0, 32767);
                pcm[i] = val;
              }
            }

            fDataLen = soundDataParams.inN;
          }

          // Copy the PCM data to the SS_GetSoundData_Params object
          IntPtr pPCMData = (IntPtr) soundDataParams.outSamples;
          Marshal.Copy(pcm, 0, pPCMData, fDataLen);
          soundDataParams.outN = fDataLen;
          hasData = true;
        }


        else
        {
          soundDataParams.outSamples = IntPtr.Zero;
          soundDataParams.outN = 0;
        }

        if (soundDataParams.inNumBins > 0)
        {
          // Set the FFT data

          int requestedFFTBins = soundDataParams.inNumBins;
          int totalRequestedBins = requestedFFTBins + soundDataParams.inStartBin;
          int actualFFTSize = 512;
          const int fftScalingFactor = 100000;

          BASSData binSizeFlag = BASSData.BASS_DATA_FFT1024;

          if (totalRequestedBins < 512)
          {
            binSizeFlag = BASSData.BASS_DATA_FFT512;
            actualFFTSize = 256;
          }

          else if (totalRequestedBins < 1024)
          {
            binSizeFlag = BASSData.BASS_DATA_FFT1024;
            actualFFTSize = 512;
          }

          else if (totalRequestedBins < 2048)
          {
            binSizeFlag = BASSData.BASS_DATA_FFT2048;
            actualFFTSize = 1024;
          }

          else if (totalRequestedBins < 4096)
          {
            binSizeFlag = BASSData.BASS_DATA_FFT4096;
            actualFFTSize = 2048;
          }

          float[] fft = new float[actualFFTSize*2];
          float[] outFFT = null;

          fDataLen = requestedFFTBins;
          int bytesRead = 0;

          if (!IsPreviewVisualization)
          {
            bytesRead = Un4seen.Bass.Bass.BASS_ChannelGetData(stream, ref fft[0], (int) binSizeFlag);

            // The number of "bins" requested is likely to be smaller than the number we get
            // from BASS so we'll need to average some of the bins...
            outFFT = new float[totalRequestedBins];
            float fStep = (float) actualFFTSize/(float) totalRequestedBins;
            int lastBin = 0;

            for (int i = 0; i < outFFT.Length; i++)
            {
              int startBin = lastBin;
              int stopBin = (int) (((float) (i + 1)*fStep) + .5f);
              int totalBins = stopBin - startBin;
              float tempBinValTotal = 0;
              float avgBinVal = 0;

              for (int x = 0; x < totalBins; x++)
              {
                float curFftVal = fft[startBin + x];
                tempBinValTotal += curFftVal;
              }

              avgBinVal = tempBinValTotal/totalBins;
              lastBin = stopBin;

              // Scale the output to it's large enough to be visible
              outFFT[i] = avgBinVal*fftScalingFactor;
            }
          }

            // We're in preview mode so we'll generate dummy FFT data so the viz
            // looks like it's doing something...
          else
          {
            outFFT = new float[totalRequestedBins];
            Random rand = new Random();

            for (int i = 0; i < outFFT.Length; i++)
            {
              float val = (float) rand.Next(100, fftScalingFactor);
              outFFT[i] = val;
            }
          }

          // Copy the FFT data to the SS_GetSoundData_Params object
          Marshal.Copy(outFFT, soundDataParams.inStartBin, (IntPtr) soundDataParams.outFFT, fDataLen);
          soundDataParams.outNumBins = fDataLen;
          hasData = true;
        }

        else
        {
          soundDataParams.outFFT = IntPtr.Zero;
          soundDataParams.outNumBins = 0;
        }

        if (hasData)
        {
          soundDataParams.mSize = Marshal.SizeOf(soundDataParams);
          Marshal.StructureToPtr(soundDataParams, (IntPtr) inParam1, false);
        }
      }

      else
      {
        returnCode = NotImplemented;
      }

      return returnCode;
    }

    #endregion
  }
}
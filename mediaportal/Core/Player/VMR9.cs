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
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using DirectShowLib;
using DShowNET.Helper;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Util;
using Microsoft.DirectX.Direct3D;
using Filter=Microsoft.DirectX.Direct3D.Filter;
using Geometry=MediaPortal.GUI.Library.Geometry;

namespace MediaPortal.Player
{
  /// <summary>
  /// General helper class to add the Video Mixing Render9 filter to a graph
  /// , set it to renderless mode and provide it our own allocator/presentor
  /// This will allow us to render the video to a direct3d texture
  /// which we can use to draw the transparent OSD on top of it
  /// Some classes which work together:
  ///  VMR9Util								: general helper class
  ///  AllocatorWrapper.cs		: implements our own allocator/presentor for vmr9 by implementing
  ///                           IVMRSurfaceAllocator9 and IVMRImagePresenter9
  ///  PlaneScene.cs          : class which draws the video texture onscreen and mixes it with the GUI, OSD,...                          
  /// </summary>
  /// // {324FAA1F-7DA6-4778-833B-3993D8FF4151}

  #region IVMR9PresentCallback interface

  [ComVisible(true), ComImport,
   Guid("324FAA1F-7DA6-4778-833B-3993D8FF4151"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IVMR9PresentCallback
  {
    [PreserveSig]
    int PresentImage(Int16 cx, Int16 cy, Int16 arx, Int16 ary, uint dwImg);

    [PreserveSig]
    int PresentSurface(Int16 cx, Int16 cy, Int16 arx, Int16 ary, uint dwImg);
  }

  #endregion

  public class VMR9Util : IDisposable
  {
    #region imports

    [DllImport("dshowhelper.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe bool Vmr9Init(IVMR9PresentCallback callback, uint dwD3DDevice, IBaseFilter vmr9Filter,
                                               uint monitor);

    [DllImport("dshowhelper.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void Vmr9Deinit();

    [DllImport("dshowhelper.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void Vmr9SetDeinterlaceMode(Int16 mode);

    [DllImport("dshowhelper.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void Vmr9SetDeinterlacePrefs(uint dwMethod);

    [DllImport("dshowhelper.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe bool EvrInit(IVMR9PresentCallback callback, uint dwD3DDevice, IBaseFilter vmr9Filter,
                                              uint monitor); //, uint dwWindow);
    [DllImport("dshowhelper.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void EvrDeinit();

    #endregion

    #region static vars

    public static VMR9Util g_vmr9 = null;
    private static int _instanceCounter = 0;

    #endregion

    #region enums

    private enum Vmr9PlayState
    {
      Playing,
      Repaint
    }

    #endregion

    #region vars

    private PlaneScene _scene = null;
    private bool _useVmr9 = false;
    private IRender _renderFrame;
    private IBaseFilter _vmr9Filter = null;
    private int _videoHeight, _videoWidth;
    private int _videoAspectRatioX, _videoAspectRatioY;
    private IQualProp _qualityInterface = null;
    private int _frameCounter = 0;
    private DateTime _repaintTimer = DateTime.Now;
    private IVMRMixerBitmap9 _vmr9MixerBitmapInterface = null;
    private IGraphBuilder _graphBuilderInterface = null;
    private bool _isVmr9Initialized = false;
    private int _threadId;
    private Vmr9PlayState currentVmr9State = Vmr9PlayState.Playing;
    private string pixelAdaptive = "";
    private string verticalStretch = "";
    private string medianFiltering = "";

    #endregion

    #region ctor

    /// <summary>
    /// Constructor
    /// </summary>
    public VMR9Util()
    {
      _useVmr9 = true;

      if (!GUIGraphicsContext.VMR9Allowed)
      {
        Log.Info("VMR9: ctor() - VMR9 not allowed");
        _useVmr9 = false;
        return;
      }
      _renderFrame = GUIGraphicsContext.RenderGUI;
      if (GUIGraphicsContext.DX9Device == null)
      {
        _useVmr9 = false;
        Log.Warn("VMR9: ctor() - DX9Device == null!");
      }
      if (_renderFrame == null)
      {
        _useVmr9 = false;
        Log.Debug("VMR9: ctor() _renderFrame == null");
      }
      if (g_vmr9 != null || GUIGraphicsContext.Vmr9Active)
      {
        _useVmr9 = false;
        Log.Info("VMR9: ctor() VMR9 already active");
      }
    }

    #endregion

    #region properties

    public bool UseVmr9
    {
      get { return _useVmr9; }
    }

    public int FrameCounter
    {
      get { return _frameCounter; }
      set { _frameCounter = value; }
    }

    /// <summary>
    /// returns the width of the video
    /// </summary>
    public int VideoWidth
    {
      get { return _videoWidth; }
      set { _videoWidth = value; }
    }

    /// <summary>
    /// returns the height of the video
    /// </summary>
    public int VideoHeight
    {
      get { return _videoHeight; }
      set { _videoHeight = value; }
    }


    /// <summary>
    /// returns the width of the video
    /// </summary>
    public int VideoAspectRatioX
    {
      get { return _videoAspectRatioX; }
      set { _videoAspectRatioX = value; }
    }

    /// <summary>
    /// returns the height of the video
    /// </summary>
    public int VideoAspectRatioY
    {
      get { return _videoAspectRatioY; }
      set { _videoAspectRatioY = value; }
    }

    public IPin PinConnectedTo
    {
      get
      {
        if (!_isVmr9Initialized)
        {
          return null;
        }
        if (_vmr9Filter == null || !_useVmr9)
        {
          return null;
        }

        IPin pinIn, pinConnected;
        pinIn = DsFindPin.ByDirection(_vmr9Filter, PinDirection.Input, 0);
        if (pinIn == null)
        {
          //no input pin found, vmr9 is not possible
          return null;
        }
        pinIn.ConnectedTo(out pinConnected);
        DirectShowUtil.ReleaseComObject(pinIn);
        return pinConnected;
      }
    }

    /// <summary>
    /// This method returns true if VMR9 is enabled AND WORKING!
    /// this allows players to check if if VMR9 is working after setting up the playing graph
    /// by checking if VMR9 is possible they can for example fallback to the overlay device
    /// </summary>
    public bool IsVMR9Connected
    {
      get
      {
        if (!_isVmr9Initialized)
        {
          return false;
        }
        // check if vmr9 is enabled and if initialized
        if (_vmr9Filter == null || !_useVmr9)
        {
          Log.Warn("VMR9: Not used or no filter:{0} {1:x}", _useVmr9, _vmr9Filter);
          return false;
        }

        int hr = 0;
        //get the VMR9 input pin#0 is connected
        for (int i = 0; i < 3; ++i)
        {
          IPin pinIn, pinConnected;
          pinIn = DsFindPin.ByDirection(_vmr9Filter, PinDirection.Input, i);
          if (pinIn == null)
          {
            //no input pin found, vmr9 is not possible
            Log.Warn("VMR9: No input pin {0} found", i);
            continue;
          }

          //check if the input is connected to a video decoder
          hr = pinIn.ConnectedTo(out pinConnected);
          if (pinConnected == null)
          {
            //no pin is not connected so vmr9 is not possible
            Log.Warn("VMR9: Pin: {0} not connected: {1:x}", i, hr);
          }
          else
          {
            //Log.Info("vmr9: pin:{0} is connected",i);
            if (pinIn != null)
            {
              hr = DirectShowUtil.ReleaseComObject(pinIn);
            }
            if (pinConnected != null)
            {
              hr = DirectShowUtil.ReleaseComObject(pinConnected);
            }
            return true;
          }
          if (pinIn != null)
          {
            hr = DirectShowUtil.ReleaseComObject(pinIn);
          }
          if (pinConnected != null)
          {
            hr = DirectShowUtil.ReleaseComObject(pinConnected);
          }
        }
        return false;
      } //get {
    } //public bool IsVMR9Connected

    #endregion

    #region public members

    /// <summary>
    /// Add VMR9 filter to graph and configure it
    /// </summary>
    /// <param name="graphBuilder"></param>
    public bool AddVMR9(IGraphBuilder graphBuilder)
    {
      if (!_useVmr9)
      {
        Log.Debug("VMR9: addvmr9 - vmr9 is deactivated");
        return false;
      }
      if (_isVmr9Initialized)
      {
        Log.Debug("VMR9: addvmr9: vmr9 has already been initialized");
        return false;
      }

      bool _useEvr = GUIGraphicsContext.IsEvr;

      if (_instanceCounter != 0)
      {
        Log.Error("VMR9: Multiple instances of VMR9 running!!!");
        throw new Exception("VMR9Helper: Multiple instances of VMR9 running!!!");
      }

      if (_useEvr)
      {
        _vmr9Filter = (IBaseFilter) new EnhancedVideoRenderer();
        Log.Info("VMR9: added EVR Renderer to graph");
      }
      else
      {
        _vmr9Filter = (IBaseFilter) new VideoMixingRenderer9();
        Log.Info("VMR9: added Video Mixing Renderer 9 to graph");
      }

      if (_vmr9Filter == null)
      {
        Error.SetError("Unable to play movie", "Renderer could not be added");
        Log.Error("VMR9: Renderer not installed / cannot be used!");
        return false;
      }

      IntPtr hMonitor;
      AdapterInformation ai = GUIGraphicsContext.currentFullscreenAdapterInfo;
      hMonitor = Manager.GetAdapterMonitor(ai.Adapter);
      IntPtr upDevice = DirectShowUtil.GetUnmanagedDevice(GUIGraphicsContext.DX9Device);

      _scene = new PlaneScene(_renderFrame, this);
      _scene.Init();

      HResult hr;
      if (_useEvr)
      {
        EvrInit(_scene, (uint) upDevice.ToInt32(), _vmr9Filter, (uint) hMonitor.ToInt32());
        //(uint)GUIGraphicsContext.ActiveForm);
        hr = new HResult(graphBuilder.AddFilter(_vmr9Filter, "Enhanced Video Renderer"));
      }
      else
      {
        Vmr9Init(_scene, (uint) upDevice.ToInt32(), _vmr9Filter, (uint) hMonitor.ToInt32());
        hr = new HResult(graphBuilder.AddFilter(_vmr9Filter, "Video Mixing Renderer 9"));
      }

      if (hr != 0)
      {
        if (_useEvr)
        {
          EvrDeinit();
        }
        else
        {
          Vmr9Deinit();
        }
        _scene.Stop();
        _scene.Deinit();
        _scene = null;

        DirectShowUtil.ReleaseComObject(_vmr9Filter);
        _vmr9Filter = null;
        Error.SetError("Unable to play movie", "Unable to initialize Renderer");
        Log.Error("VMR9: Failed to add Renderer to filter graph");
        return false;
      }

      _qualityInterface = _vmr9Filter as IQualProp;
      _vmr9MixerBitmapInterface = _vmr9Filter as IVMRMixerBitmap9;
      _graphBuilderInterface = graphBuilder;
      _instanceCounter++;
      _isVmr9Initialized = true;
      if (!_useEvr)
      {
        SetDeinterlacePrefs();
        OperatingSystem os = Environment.OSVersion;
        if (os.Platform == PlatformID.Win32NT)
        {
          long version = os.Version.Major*10000000 + os.Version.Minor*10000 + os.Version.Build;
          if (version >= 50012600) // we need at least win xp sp2 for VMR9 YUV mixing mode
          {
            IVMRMixerControl9 mixer = _vmr9Filter as IVMRMixerControl9;
            if (mixer != null)
            {
              VMR9MixerPrefs dwPrefs;
              mixer.GetMixingPrefs(out dwPrefs);
              dwPrefs &= ~VMR9MixerPrefs.RenderTargetMask;

              dwPrefs |= VMR9MixerPrefs.RenderTargetYUV;
                // YUV saves graphics bandwith  http://msdn2.microsoft.com/en-us/library/ms788177(VS.85).aspx
              hr.Set(mixer.SetMixingPrefs(dwPrefs));
              Log.Debug("VMR9: Enabled YUV mixing - " + hr.ToDXString());

              using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
              {
                //Enable nonsquaremixing
                if (xmlreader.GetValueAsBool("general", "nonsquare", true))
                {
                  mixer.GetMixingPrefs(out dwPrefs);
                  dwPrefs |= VMR9MixerPrefs.NonSquareMixing;
                  hr.Set(mixer.SetMixingPrefs(dwPrefs));
                  Log.Debug("VRM9: Turning on nonsquare mixing - " + hr.ToDXString());
                  hr.Set(mixer.SetMixingPrefs(dwPrefs));
                }

                // Enable DecimateMask - this will effectively use only half of the input width & length
                if (xmlreader.GetValueAsBool("general", "dx9decimatemask", false))
                {
                  mixer.GetMixingPrefs(out dwPrefs);
                  dwPrefs &= ~VMR9MixerPrefs.DecimateMask;
                  dwPrefs |= VMR9MixerPrefs.DecimateOutput;
                  hr.Set(mixer.SetMixingPrefs(dwPrefs));
                  Log.Debug("VRM9: Enable decimatemask - " + hr.ToDXString());
                  hr.Set(mixer.SetMixingPrefs(dwPrefs));
                }

                // see  D3DTEXTUREFILTERTYPE Enumerated Type documents for further information
                // MixerPref9_PointFiltering
                // MixerPref9_BiLinearFiltering
                // MixerPref9_AnisotropicFiltering
                // MixerPref9_PyramidalQuadFiltering
                // MixerPref9_GaussianQuadFiltering

                mixer.SetMixingPrefs(dwPrefs);
                mixer.GetMixingPrefs(out dwPrefs);
                dwPrefs &= ~VMR9MixerPrefs.FilteringMask;
                string filtermode9 = xmlreader.GetValueAsString("general", "dx9filteringmode", "Gaussian Quad Filtering");
                if (filtermode9 == "Point Filtering")
                {
                  dwPrefs |= VMR9MixerPrefs.PointFiltering;
                }
                else if (filtermode9 == "Bilinear Filtering")
                {
                  dwPrefs |= VMR9MixerPrefs.BiLinearFiltering;
                }
                else if (filtermode9 == "Anisotropic Filtering")
                {
                  dwPrefs |= VMR9MixerPrefs.AnisotropicFiltering;
                }
                else if (filtermode9 == "Pyrimidal Quad Filtering")
                {
                  dwPrefs |= VMR9MixerPrefs.PyramidalQuadFiltering;
                }
                else
                {
                  dwPrefs |= VMR9MixerPrefs.GaussianQuadFiltering;
                }

                hr.Set(mixer.SetMixingPrefs(dwPrefs));
                Log.Debug("VRM9: Set filter mode - " + filtermode9 + " " + hr.ToDXString());
              }
            }
          }
        }
      }
      _threadId = Thread.CurrentThread.ManagedThreadId;
      GUIGraphicsContext.Vmr9Active = true;
      g_vmr9 = this;
      Log.Debug("VMR9: Renderer successfully added");
      return true;
    }

    /// <summary>
    /// repaints the last frame
    /// </summary>
    public void Repaint()
    {
      if (!_isVmr9Initialized)
      {
        return;
      }
      if (currentVmr9State == Vmr9PlayState.Playing)
      {
        return;
      }
      _scene.Repaint();
    }

    public void SetRepaint()
    {
      if (!_isVmr9Initialized)
      {
        return;
      }
      if (!GUIGraphicsContext.Vmr9Active)
      {
        return;
      }
      Log.Debug("VMR9: SetRepaint()");
      FrameCounter = 0;
      _repaintTimer = DateTime.Now;
      currentVmr9State = Vmr9PlayState.Repaint;
      _scene.DrawVideo = false;
    }

    public bool IsRepainting
    {
      get { return (currentVmr9State == Vmr9PlayState.Repaint); }
    }

    public void Process()
    {
      if (!_isVmr9Initialized)
      {
        return;
      }
      if (!GUIGraphicsContext.Vmr9Active)
      {
        return;
      }
      if (g_Player.Playing && g_Player.IsDVD && g_Player.IsDVDMenu)
      {
        GUIGraphicsContext.Vmr9FPS = 0f;
        currentVmr9State = Vmr9PlayState.Playing;
        _scene.DrawVideo = true;
        _repaintTimer = DateTime.Now;
        return;
      }

      TimeSpan ts = DateTime.Now - _repaintTimer;
      int frames = FrameCounter;
      if (ts.TotalMilliseconds >= 1000 || (currentVmr9State == Vmr9PlayState.Repaint && FrameCounter > 0))
      {
        GUIGraphicsContext.Vmr9FPS = ((float) (frames*1000))/((float) ts.TotalMilliseconds);
//         Log.Info("VMR9Helper:frames:{0} fps:{1} time:{2}", frames, GUIGraphicsContext.Vmr9FPS,ts.TotalMilliseconds);
        FrameCounter = 0;

        if (_threadId == Thread.CurrentThread.ManagedThreadId)
        {
          if (_qualityInterface != null)
          {
            VideoRendererStatistics.Update(_qualityInterface);
          }
          else
          {
            Log.Debug("_qualityInterface is null!");
          }
        }
        _repaintTimer = DateTime.Now;
      }

      if (currentVmr9State == Vmr9PlayState.Repaint && frames > 0)
      {
        Log.Debug("VMR9: Repainting -> Playing, Frames: {0}", frames);
        GUIGraphicsContext.Vmr9FPS = 50f;
        currentVmr9State = Vmr9PlayState.Playing;
        _scene.DrawVideo = true;
        _repaintTimer = DateTime.Now;
      }
      else if (currentVmr9State == Vmr9PlayState.Playing && GUIGraphicsContext.Vmr9FPS < 5f)
      {
        Log.Debug("VMR9Helper: Playing -> Repainting, Frames {0}", frames);
        GUIGraphicsContext.Vmr9FPS = 0f;
        currentVmr9State = Vmr9PlayState.Repaint;
        _scene.DrawVideo = false;
      }
    }

    /// <summary>
    /// returns a IVMRMixerBitmap9 interface
    /// </summary>
    public IVMRMixerBitmap9 MixerBitmapInterface
    {
      get { return _vmr9MixerBitmapInterface; }
    }

    public void SetDeinterlacePrefs()
    {
      if (!_isVmr9Initialized)
      {
        return;
      }
      Log.Debug("VMR9: SetDeinterlacePrefs()");
      int DeInterlaceMode = 3;
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        DeInterlaceMode = xmlreader.GetValueAsInt("mytv", "deinterlace", 3);
        //None = 0
        if (DeInterlaceMode == 1)
        {
          DeInterlaceMode = 2; //BOB = 0x02
        }
        if (DeInterlaceMode == 2)
        {
          DeInterlaceMode = 4; //Weave = 0x04
        }
        if (DeInterlaceMode == 3)
        {
          DeInterlaceMode = 1; //NextBest = 0x01
        }
      }
      Vmr9SetDeinterlacePrefs((uint) DeInterlaceMode);
    }

    public void SetDeinterlaceMode()
    {
      if (!GUIGraphicsContext.IsEvr)
      {
        if (!_isVmr9Initialized)
        {
          return;
        }
        Log.Debug("VMR9: SetDeinterlaceMode()");
        IVMRDeinterlaceControl9 deinterlace = (IVMRDeinterlaceControl9) _vmr9Filter;
        IPin InPin = null;
        int hr = _vmr9Filter.FindPin("VMR Input0", out InPin);
        AMMediaType mediatype = new AMMediaType();
        InPin.ConnectionMediaType(mediatype);
        //Start by getting the media type of the video stream.
        //Only VideoInfoHeader2 formats can be interlaced.
        if (mediatype.formatType == FormatType.VideoInfo2)
        {
          Log.Debug("VMR9: SetDeinterlaceMode - FormatType = VideoInfo2");
          int numModes = 0;
          VideoInfoHeader2 VideoHeader2 = new VideoInfoHeader2();
          Marshal.PtrToStructure(mediatype.formatPtr, VideoHeader2);
          VMR9VideoDesc VideoDesc = new VMR9VideoDesc();
          // If the FormatType is VideoInfo2, check the dwInterlaceFlags field for the AMInterlace.IsInterlaced flag.
          //The presence of this flag indicates the video is interlaced.
          if ((VideoHeader2.InterlaceFlags & AMInterlace.IsInterlaced) != 0)
          {
            Log.Debug("VMR9: SetDeinterlaceMode - Interlaced frame detected");
            //Fill in the VMR9VideoDesc structure with a description of the video stream.
            VideoDesc.dwSize = Marshal.SizeOf(VideoDesc); // dwSize: Set this field to sizeof(VMR9VideoDesc).
            VideoDesc.dwSampleWidth = VideoHeader2.BmiHeader.Width; // dwSampleWidth: Set this field to pBMI->biWidth. 
            VideoDesc.dwSampleHeight = VideoHeader2.BmiHeader.Height;
              // dwSampleHeight: Set this field to abs(pBMI->biHeight). 
            //SampleFormat: This field describes the interlace characteristics of the media type.
            //Check the dwInterlaceFlags field in the VIDEOINFOHEADER2 structure, and set SampleFormat equal to the equivalent VMR9_SampleFormat flag.
            if ((VideoHeader2.InterlaceFlags & AMInterlace.IsInterlaced) != 0)
            {
              if ((VideoHeader2.InterlaceFlags & AMInterlace.DisplayModeBobOnly) == 0)
              {
                VideoDesc.SampleFormat = VMR9SampleFormat.ProgressiveFrame;
              }
              if ((VideoHeader2.InterlaceFlags & AMInterlace.OneFieldPerSample) != 0)
              {
                if ((VideoHeader2.InterlaceFlags & AMInterlace.Field1First) != 0)
                {
                  VideoDesc.SampleFormat = VMR9SampleFormat.FieldSingleEven;
                }
                else
                {
                  VideoDesc.SampleFormat = VMR9SampleFormat.FieldSingleOdd;
                }
              }
              if ((VideoHeader2.InterlaceFlags & AMInterlace.Field1First) != 0)
              {
                VideoDesc.SampleFormat = VMR9SampleFormat.FieldInterleavedEvenFirst;
              }
              else
              {
                VideoDesc.SampleFormat = VMR9SampleFormat.FieldInterleavedOddFirst;
              }
            }
            //InputSampleFreq: This field gives the input frequency, which can be calculated from the AvgTimePerFrame field in the VIDEOINFOHEADER2 structure.
            //In the general case, set dwNumerator to 10000000, and set dwDenominator to AvgTimePerFrame. 
            VideoDesc.InputSampleFreq.dwDenominator = 10000000;
            VideoDesc.InputSampleFreq.dwNumerator = (int) VideoHeader2.AvgTimePerFrame;
            //OutputFrameFreq: This field gives the output frequency, which can be calculated from the InputSampleFreq value and the interleaving characteristics of the input stream:
            //Set OutputFrameFreq.dwDenominator equal to InputSampleFreq.dwDenominator.
            //If the input video is interleaved, set OutputFrameFreq.dwNumerator to 2 x InputSampleFreq.dwNumerator. (After deinterlacing, the frame rate is doubled.)
            //Otherwise, set the value to InputSampleFreq.dwNumerator.
            VideoDesc.OutputFrameFreq.dwDenominator = 10000000;
            VideoDesc.OutputFrameFreq.dwNumerator = (int) VideoHeader2.AvgTimePerFrame*2;
            VideoDesc.dwFourCC = VideoHeader2.BmiHeader.Compression; //dwFourCC: Set this field to pBMI->biCompression.
            //Pass the structure to the IVMRDeinterlaceControl9::GetNumberOfDeinterlaceModes method.
            //Call the method twice. The first call returns the number of deinterlace modes the hardware supports for the specified format.
            hr = deinterlace.GetNumberOfDeinterlaceModes(ref VideoDesc, ref numModes, null);
            if (hr == 0 && numModes != 0)
            {
              Guid[] modes = new Guid[numModes];
              {
                //Allocate an array of GUIDs of this size, and call the method again, passing in the address of the array.
                //The second call fills the array with GUIDs. Each GUID identifies one deinterlacing mode. 
                hr = deinterlace.GetNumberOfDeinterlaceModes(ref VideoDesc, ref numModes, modes);
                for (int i = 0; i < numModes; i++)
                {
                  //To get the capabiltiies of a particular mode, call the IVMRDeinterlaceControl9::GetDeinterlaceModeCaps method.
                  //Pass in the same VMR9VideoDesc structure, along with one of the GUIDs from the array.
                  //The method fills a VMR9DeinterlaceCaps structure with the mode capabilities. 
                  VMR9DeinterlaceCaps caps = new VMR9DeinterlaceCaps();
                  caps.dwSize = Marshal.SizeOf(typeof (VMR9DeinterlaceCaps));
                  hr = deinterlace.GetDeinterlaceModeCaps(modes[i], ref VideoDesc, ref caps);
                  if (hr == 0)
                  {
                    Log.Debug("VMR9: AvailableDeinterlaceMode - {0}: {1}", i, modes[i]);
                    switch (caps.DeinterlaceTechnology)
                    {
                        //The algorithm is unknown or proprietary
                      case VMR9DeinterlaceTech.Unknown:
                        {
                          Log.Info("VMR9: Unknown H/W de-interlace mode");
                          break;
                        }
                        //The algorithm creates each missing line by repeating the line above it or below it.
                        //This method creates jagged artifacts and is not recommended.
                      case VMR9DeinterlaceTech.BOBLineReplicate:
                        {
                          Log.Info("VMR9: BOB Line Replicate capable");
                          break;
                        }
                        //The algorithm creates the missing lines by vertically stretching each video field by a factor of two.
                        //For example, it might average two lines or use a (-1, 9, 9, -1)/16 filter across four lines.
                        //Slight vertical adjustments are made to ensure that the resulting image does not "bob" up and down
                      case VMR9DeinterlaceTech.BOBVerticalStretch:
                        {
                          Log.Info("VMR9: BOB Vertical Stretch capable");
                          verticalStretch = modes[i].ToString();
                          break;
                        }
                        //The algorithm uses median filtering to recreate the pixels in the missing lines.
                      case VMR9DeinterlaceTech.MedianFiltering:
                        {
                          Log.Info("VMR9: Median Filtering capable");
                          medianFiltering = modes[i].ToString();
                          break;
                        }
                        //The algorithm uses an edge filter to create the missing lines.
                        //In this process, spatial directional filters are applied to determine the orientation of edges in the picture content.
                        //Missing pixels are created by filtering along (rather than across) the detected edges.
                      case VMR9DeinterlaceTech.EdgeFiltering:
                        {
                          Log.Info("VMR9: Edge Filtering capable");
                          break;
                        }
                        //The algorithm uses spatial or temporal interpolation, switching between the two on a field-by-field basis, depending on the amount of motion.
                      case VMR9DeinterlaceTech.FieldAdaptive:
                        {
                          Log.Info("VMR9: Field Adaptive capable");
                          break;
                        }
                        //The algorithm uses spatial or temporal interpolation, switching between the two on a pixel-by-pixel basis, depending on the amount of motion.
                      case VMR9DeinterlaceTech.PixelAdaptive:
                        {
                          Log.Info("VMR9: Pixel Adaptive capable");
                          pixelAdaptive = modes[i].ToString();
                          break;
                        }
                        //The algorithm identifies objects within a sequence of video fields.
                        //Before it recreates the missing pixels, it aligns the movement axes of the individual objects in the scene to make them parallel with the time axis.
                      case VMR9DeinterlaceTech.MotionVectorSteered:
                        {
                          Log.Info("VMR9: Motion Vector Steered capable");
                          break;
                        }
                    }
                  }
                }
              }
              //Set the MP preferred h/w de-interlace modes in order of quality
              //pixel adaptive, then median filtering & finally vertical stretch
              if (pixelAdaptive != "")
              {
                Guid DeinterlaceMode = new Guid(pixelAdaptive);
                Log.Debug("VMR9: trying pixel adaptive");
                hr = deinterlace.SetDeinterlaceMode(0, DeinterlaceMode);
                if (hr != 0)
                {
                  Log.Error("VMR9: pixel adaptive failed!");
                }
                else
                {
                  Log.Info("VMR9: setting pixel adaptive succeeded");
                  medianFiltering = "";
                  verticalStretch = "";
                }
              }
              if (medianFiltering != "")
              {
                Guid DeinterlaceMode = new Guid(medianFiltering);
                Log.Debug("VMR9: trying median filtering");
                hr = deinterlace.SetDeinterlaceMode(0, DeinterlaceMode);
                if (hr != 0)
                {
                  Log.Error("VMR9: median filtering failed!");
                }
                else
                {
                  Log.Info("VMR9: setting median filtering succeeded");
                  verticalStretch = "";
                }
              }
              if (verticalStretch != "")
              {
                Guid DeinterlaceMode = new Guid(verticalStretch);
                Log.Debug("VMR9: trying vertical stretch");
                hr = deinterlace.SetDeinterlaceMode(0, DeinterlaceMode);
                if (hr != 0)
                {
                  Log.Error("VMR9: Cannot set H/W de-interlace mode - using VMR9 fallback");
                }
                Log.Info("VMR9: setting vertical stretch succeeded");
              }
            }
            else
            {
              Log.Info("VMR9: No H/W de-interlaced modes supported, using fallback preference");
            }
          }
          else
          {
            Log.Info("VMR9: progressive mode detected - no need to de-interlace");
          }
        }
          //If the format type is VideoInfo, it must be a progressive frame.
        else
        {
          Log.Info("VMR9: no need to de-interlace this video source");
        }
        //release the VMR9 pin
        hr = DirectShowUtil.ReleaseComObject(InPin);
        if (hr != 0)
        {
          Log.Error("VMR9: failed releasing InPin 0x:(0)", hr);
        }
        else
        {
          InPin = null;
        }
      }
    }

    public void Enable(bool onOff)
    {
      //Log.Info("Vmr9:Enable:{0}", onOff);
      if (!_isVmr9Initialized)
      {
        return;
      }
      if (_scene != null)
      {
        _scene.Enabled = onOff;
      }
      if (onOff)
      {
        _repaintTimer = DateTime.Now;
        FrameCounter = 50;
      }
    }

    public bool Enabled
    {
      get
      {
        if (!_isVmr9Initialized)
        {
          return true;
        }
        if (_scene == null)
        {
          return true;
        }
        return _scene.Enabled;
      }
    }

    public bool SaveBitmap(Bitmap bitmap, bool show, bool transparent, float alphaValue)
    {
      if (!_isVmr9Initialized)
      {
        return false;
      }
      if (_vmr9Filter == null)
      {
        return false;
      }

      if (MixerBitmapInterface == null)
      {
        return false;
      }

      if (GUIGraphicsContext.Vmr9Active == false)
      {
        Log.Info("SaveVMR9Bitmap() failed - no VMR9");
        return false;
      }
      int hr = 0;
      // transparent image?
      using (MemoryStream mStr = new MemoryStream())
      {
        if (bitmap != null)
        {
          if (transparent == true)
          {
            bitmap.MakeTransparent(Color.Black);
          }
          bitmap.Save(mStr, ImageFormat.Bmp);
          mStr.Position = 0;
        }

        VMR9AlphaBitmap bmp = new VMR9AlphaBitmap();

        if (show == true)
        {
          // get AR for the bitmap
          Rectangle src, dest;
          g_vmr9.GetVideoWindows(out src, out dest);

          int width = g_vmr9.VideoWidth;
          int height = g_vmr9.VideoHeight;

          float xx = (float) src.X/width;
          float yy = (float) src.Y/height;
          float fx = (float) (src.X + src.Width)/width;
          float fy = (float) (src.Y + src.Height)/height;
          //

          using (
            Surface surface = GUIGraphicsContext.DX9Device.CreateOffscreenPlainSurface(GUIGraphicsContext.Width,
                                                                                       GUIGraphicsContext.Height,
                                                                                       Format.X8R8G8B8,
                                                                                       Pool.SystemMemory))
          {
            SurfaceLoader.FromStream(surface, mStr, Filter.None, 0);
            bmp.dwFlags = (VMR9AlphaBitmapFlags) (4 | 8);
            bmp.clrSrcKey = 0;
            unsafe
            {
              bmp.pDDS = (IntPtr) surface.UnmanagedComPointer;
            }
            bmp.rDest = new NormalizedRect();
            bmp.rDest.top = yy;
            bmp.rDest.left = xx;
            bmp.rDest.bottom = fy;
            bmp.rDest.right = fx;
            bmp.fAlpha = alphaValue;
            //Log.Info("SaveVMR9Bitmap() called");
            hr = g_vmr9.MixerBitmapInterface.SetAlphaBitmap(ref bmp);
            if (hr != 0)
            {
              //Log.Info("SaveVMR9Bitmap() failed: error {0:X} on SetAlphaBitmap()",hr);
              return false;
            }
          }
        }
        else
        {
          bmp.dwFlags = (VMR9AlphaBitmapFlags) 1;
          bmp.clrSrcKey = 0;
          bmp.rDest = new NormalizedRect();
          bmp.rDest.top = 0.0f;
          bmp.rDest.left = 0.0f;
          bmp.rDest.bottom = 1.0f;
          bmp.rDest.right = 1.0f;
          bmp.fAlpha = alphaValue;
          hr = g_vmr9.MixerBitmapInterface.UpdateAlphaBitmapParameters(ref bmp);
          if (hr != 0)
          {
            return false;
          }
        }
      }
      // dispose
      return true;
    } // savevmr9bitmap

    public void GetVideoWindows(out Rectangle rSource, out Rectangle rDest)
    {
      Geometry m_geometry = new Geometry();
      // get the window where the video/tv should be shown
      float x = GUIGraphicsContext.VideoWindow.X;
      float y = GUIGraphicsContext.VideoWindow.Y;
      float nw = GUIGraphicsContext.VideoWindow.Width;
      float nh = GUIGraphicsContext.VideoWindow.Height;

      //sanity checks
      if (nw > GUIGraphicsContext.OverScanWidth)
      {
        nw = GUIGraphicsContext.OverScanWidth;
      }
      if (nh > GUIGraphicsContext.OverScanHeight)
      {
        nh = GUIGraphicsContext.OverScanHeight;
      }

      //are we supposed to show video in fullscreen or in a preview window?
      if (GUIGraphicsContext.IsFullScreenVideo || !GUIGraphicsContext.ShowBackground)
      {
        //yes fullscreen, then use the entire screen
        x = GUIGraphicsContext.OverScanLeft;
        y = GUIGraphicsContext.OverScanTop;
        nw = GUIGraphicsContext.OverScanWidth;
        nh = GUIGraphicsContext.OverScanHeight;
      }

      //calculate the video window according to the current aspect ratio settings
      float fVideoWidth = (float) VideoWidth;
      float fVideoHeight = (float) VideoHeight;
      m_geometry.ImageWidth = (int) fVideoWidth;
      m_geometry.ImageHeight = (int) fVideoHeight;
      m_geometry.ScreenWidth = (int) nw;
      m_geometry.ScreenHeight = (int) nh;
      m_geometry.ARType = GUIGraphicsContext.ARType;
      m_geometry.PixelRatio = GUIGraphicsContext.PixelRatio;
      m_geometry.GetWindow(VideoAspectRatioX, VideoAspectRatioY, out rSource, out rDest);
      rDest.X += (int) x;
      rDest.Y += (int) y;
      m_geometry = null;
    }

    #endregion

    #region IDisposeable

    /// <summary>
    /// removes the vmr9 filter from the graph and free up all unmanaged resources
    /// </summary>
    public void Dispose()
    {
      Log.Debug("VMR9: Dispose");
      if (false == _isVmr9Initialized)
      {
        return;
      }
      if (_threadId != Thread.CurrentThread.ManagedThreadId)
      {
        Log.Error("VMR9: Dispose() from wrong thread");
        //return;
      }
      if (_vmr9Filter == null)
      {
        Log.Error("VMR9: Dispose() no filter");
        return;
      }

      if (_scene != null)
      {
        //Log.Info("VMR9Helper:stop planescene");
        _scene.Stop();
        _instanceCounter--;
        _scene.Deinit();
        GUIGraphicsContext.Vmr9Active = false;
        GUIGraphicsContext.Vmr9FPS = 0f;
        GUIGraphicsContext.InVmr9Render = false;
        currentVmr9State = Vmr9PlayState.Playing;
      }
      int result;

      //if (_vmr9MixerBitmapInterface!= null)
      //	while ( (result=DirectShowUtil.ReleaseComObject(_vmr9MixerBitmapInterface))>0); 
      //result=DirectShowUtil.ReleaseComObject(_vmr9MixerBitmapInterface);
      _vmr9MixerBitmapInterface = null;

      //				if (_qualityInterface != null)
      //					while ( (result=DirectShowUtil.ReleaseComObject(_qualityInterface))>0); 
      //DirectShowUtil.ReleaseComObject(_qualityInterface);
      _qualityInterface = null;

      if (GUIGraphicsContext.IsEvr)
      {
          EvrDeinit();
      }
      else
      {
          Vmr9Deinit();
      }

      try
      {
        result = _graphBuilderInterface.RemoveFilter(_vmr9Filter);
        if (result != 0)
        {
          Log.Warn("VMR9: RemoveFilter(): {0}", result);
        }
      }
      catch (Exception)
      {
      }

      try
      {
        do
        {
          result = DirectShowUtil.ReleaseComObject(_vmr9Filter);
          Log.Info("VMR9: ReleaseComObject(): {0}", result);
        } while (result > 0);
      }
      catch (Exception)
      {
      }
      _vmr9Filter = null;
      _graphBuilderInterface = null;
      _scene = null;
      g_vmr9 = null;
      _isVmr9Initialized = false;
    }

    #endregion
  }
}
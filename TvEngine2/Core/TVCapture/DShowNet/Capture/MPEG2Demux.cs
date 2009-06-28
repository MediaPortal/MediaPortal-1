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
using System.Collections;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using DirectShowLib;
using DirectShowLib.SBE;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;

namespace DShowNET.Helper
{
  /// <summary>
  /// 
  /// </summary>
  public class MPEG2Demux : IDisposable
  {
    #region imports

    [ComImport, Guid("6CFAD761-735D-4aa5-8AFC-AF91A7D61EBA")]
    private class VideoAnalyzer
    {
    } ;

    [DllImport("advapi32", CharSet = CharSet.Auto)]
    private static extern bool ConvertStringSidToSid(string pStringSid, ref IntPtr pSID);

    [DllImport("kernel32", CharSet = CharSet.Auto)]
    private static extern IntPtr LocalFree(IntPtr hMem);

    [DllImport("advapi32", CharSet = CharSet.Auto)]
    private static extern ulong RegOpenKeyEx(IntPtr key, string subKey, uint ulOptions, uint sam, out IntPtr resultKey);

    #endregion

    #region variables

    private IGraphBuilder _graphBuilderInterface = null;
    private IMediaControl _mediaControlInterface = null;
    private IMpeg2Demultiplexer _mpeg2DemultiplexerInterface = null;
    private IPin _pinAudioOut = null;
    private IPin _pinVideoout = null;
    private IPin _pinLPCMOut = null;
    private IPin _pinDemuxerInput = null;
    private IPin _pinVideoAnalyzerInput = null;
    private IPin _pinVideoAnalyzerOutput = null;
    private IPin _pinStreamBufferIn0 = null;
    private IPin _pinStreamBufferIn1 = null;
    private IBaseFilter _filterMpeg2Demultiplexer = null;
    private IBaseFilter _filterVideoAnalyzer = null;
    private IBaseFilter _filterStreamBuffer = null;
    private IStreamBufferSink3 _streamBufferSink3Interface = null;
    private IStreamBufferConfigure _streamBufferConfigureInterface = null;
    private bool _isRendered = false;
    private bool _isGraphRunning = false;
    private VideoAnalyzer m_VideoAnalyzer = null;
    private StreamBufferConfig m_StreamBufferConfig = null;
    private IVideoWindow _videoWindowInterface = null;
    private IBasicVideo2 _basicVideoInterface = null;
    private Size _sizeFrame;
    private bool _isOverlayWindowVisible = false;
    private int _recorderId = -1;

    #endregion

    #region dshowhelper.dll Imports

    [DllImport("dshowhelper.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe bool DvrMsCreate(out int id, IBaseFilter streamBufferSink,
                                                  [In, MarshalAs(UnmanagedType.LPWStr)] string strPath,
                                                  uint dwRecordingType);

    [DllImport("dshowhelper.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void DvrMsStart(int id, uint startTime);

    [DllImport("dshowhelper.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void DvrMsStop(int id);

    #endregion

    #region structs

    /// @@@TODO : capture width/height = hardcoded to 720x576
    /// @@@TODO : capture framerate = hardcoded to 25 fps
    private static byte[] Mpeg2ProgramVideo =
      {
        0x00, 0x00, 0x00, 0x00, //00  .hdr.rcSource.left              = 0x00000000
        0x00, 0x00, 0x00, 0x00, //04  .hdr.rcSource.top               = 0x00000000
        0xD0, 0x02, 0x00, 0x00, //08  .hdr.rcSource.right             = 0x000002d0 //720
        0x40, 0x02, 0x00, 0x00, //0c  .hdr.rcSource.bottom            = 0x00000240 //576
        0x00, 0x00, 0x00, 0x00, //10  .hdr.rcTarget.left              = 0x00000000
        0x00, 0x00, 0x00, 0x00, //14  .hdr.rcTarget.top               = 0x00000000
        0xD0, 0x02, 0x00, 0x00, //18  .hdr.rcTarget.right             = 0x000002d0 //720
        0x40, 0x02, 0x00, 0x00, //1c  .hdr.rcTarget.bottom            = 0x00000240// 576
        0x00, 0x09, 0x3D, 0x00, //20  .hdr.dwBitRate                  = 0x003d0900
        0x00, 0x00, 0x00, 0x00, //24  .hdr.dwBitErrorRate             = 0x00000000

        //0x051736=333667-> 10000000/333667 = 29.97fps
        //0x061A80=400000-> 10000000/400000 = 25fps
        0x80, 0x1A, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00,
        //28  .hdr.AvgTimePerFrame            = 0x0000000000051763 ->1000000/ 40000 = 25fps
        0x00, 0x00, 0x00, 0x00, //2c  .hdr.dwInterlaceFlags           = 0x00000000
        0x00, 0x00, 0x00, 0x00, //30  .hdr.dwCopyProtectFlags         = 0x00000000
        0x04, 0x00, 0x00, 0x00, //34  .hdr.dwPictAspectRatioX         = 0x00000004
        0x03, 0x00, 0x00, 0x00, //38  .hdr.dwPictAspectRatioY         = 0x00000003
        0x00, 0x00, 0x00, 0x00, //3c  .hdr.dwReserved1                = 0x00000000
        0x00, 0x00, 0x00, 0x00, //40  .hdr.dwReserved2                = 0x00000000
        0x28, 0x00, 0x00, 0x00, //44  .hdr.bmiHeader.biSize           = 0x00000028
        0xD0, 0x02, 0x00, 0x00, //48  .hdr.bmiHeader.biWidth          = 0x000002d0 //720
        0x40, 0x02, 0x00, 0x00, //4c  .hdr.bmiHeader.biHeight         = 0x00000240 //576
        0x00, 0x00, //50  .hdr.bmiHeader.biPlanes         = 0x0000
        0x00, 0x00, //54  .hdr.bmiHeader.biBitCount       = 0x0000
        0x00, 0x00, 0x00, 0x00, //58  .hdr.bmiHeader.biCompression    = 0x00000000
        0x00, 0x00, 0x00, 0x00, //5c  .hdr.bmiHeader.biSizeImage      = 0x00000000
        0xD0, 0x07, 0x00, 0x00, //60  .hdr.bmiHeader.biXPelsPerMeter  = 0x000007d0
        0x27, 0xCF, 0x00, 0x00, //64  .hdr.bmiHeader.biYPelsPerMeter  = 0x0000cf27
        0x00, 0x00, 0x00, 0x00, //68  .hdr.bmiHeader.biClrUsed        = 0x00000000
        0x00, 0x00, 0x00, 0x00, //6c  .hdr.bmiHeader.biClrImportant   = 0x00000000
        0x98, 0xF4, 0x06, 0x00, //70  .dwStartTimeCode                = 0x0006f498
        //0x56, 0x00, 0x00, 0x00,                         //74  .cbSequenceHeader               = 0x00000056
        0x00, 0x00, 0x00, 0x00, //74  .cbSequenceHeader               = 0x00000000
        0x02, 0x00, 0x00, 0x00, //78  .dwProfile                      = 0x00000002
        0x02, 0x00, 0x00, 0x00, //7c  .dwLevel                        = 0x00000002
        0x00, 0x00, 0x00, 0x00, //80  .Flags                          = 0x00000000
        /*
                   * //  .dwSequenceHeader [1]
                  0x00, 0x00, 0x01, 0xB3, 0x2D, 0x01, 0xE0, 0x24,
                  0x09, 0xC4, 0x23, 0x81, 0x10, 0x11, 0x11, 0x12, 
                  0x12, 0x12, 0x13, 0x13, 0x13, 0x13, 0x14, 0x14, 
                  0x14, 0x14, 0x14, 0x15, 0x15, 0x15, 0x15, 0x15, 
                  0x15, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 
                  0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 
                  0x18, 0x18, 0x18, 0x19, 0x18, 0x18, 0x18, 0x19, 
                  0x1A, 0x1A, 0x1A, 0x1A, 0x19, 0x1B, 0x1B, 0x1B, 
                  0x1B, 0x1B, 0x1C, 0x1C, 0x1C, 0x1C, 0x1E, 0x1E, 
                  0x1E, 0x1F, 0x1F, 0x21, 0x00, 0x00, 0x01, 0xB5, 
                  0x14, 0x82, 0x00, 0x01, 0x00, 0x00*/
        0x00, 0x00, 0x00, 0x00
      };

    private static byte[] MPEG1AudioFormat =
      {
        0x50, 0x00, // format type      = 0x0050=WAVE_FORMAT_MPEG
        0x02, 0x00, // channels
        0x80, 0xBB, 0x00, 0x00, // samplerate       = 0x0000bb80=48000
        0x00, 0x7D, 0x00, 0x00, // nAvgBytesPerSec  = 0x00007d00=32000
        0x00, 0x03, // nBlockAlign      = 0x0300 = 768
        0x10, 0x00, // wBitsPerSample   = 16
        0x16, 0x00, // extra size       = 0x0016 = 22 bytes
        0x02, 0x00, // fwHeadLayer
        0x00, 0x70, 0x17, 0x00, // dwHeadBitrate
        0x01, 0x00, // fwHeadMode
        0x01, 0x00, // fwHeadModeExt
        0x01, 0x00, // wHeadEmphasis
        0x1C, 0x00, // fwHeadFlags
        0x00, 0x00, 0x00, 0x00, // dwPTSLow
        0x00, 0x00, 0x00, 0x00 // dwPTSHigh
      };

    private static byte[] LPCMAudioFormat =
      {
        0x00, 0x00, // format type      = 0x0000=WAVE_FORMAT_UNKNOWN
        0x02, 0x00, // channels
        0x80, 0xBB, 0x00, 0x00, // samplerate       = 0x0000bb80=48000
        0x00, 0x7D, 0x00, 0x00, // nAvgBytesPerSec  = 0x00007d00=32000
        0x00, 0x03, // nBlockAlign      = 0x0300 = 768
        0x10, 0x00, // wBitsPerSample   = 16
        0x16, 0x00, // extra size       = 0x0016 = 22 bytes
      };

    #endregion

    #region ctor

    public MPEG2Demux(ref IGraphBuilder graphBuilder, Size framesize)
    {
      _graphBuilderInterface = graphBuilder;
      _sizeFrame = framesize;
      AddMpeg2Demultiplexer();
    }

    ~MPEG2Demux()
    {
      Dispose();
    }

    #endregion

    #region properties

    public IPin InputPin
    {
      get { return _pinDemuxerInput; }
    }

    public IPin AudioOutputPin
    {
      get { return _pinAudioOut; }
    }

    public IPin LPCMOutputPin
    {
      get { return _pinLPCMOut; }
    }

    public IPin VideoOutputPin
    {
      get { return _pinVideoout; }
    }

    public bool IsRendered
    {
      get { return _isRendered; }
    }

    public IBaseFilter BaseFilter
    {
      get { return (IBaseFilter) _mpeg2DemultiplexerInterface; }
    }

    public bool Overlay
    {
      get { return _isOverlayWindowVisible; }
      set
      {
        if (value == _isOverlayWindowVisible)
        {
          return;
        }
        if (_videoWindowInterface == null)
        {
          return;
        }
        _isOverlayWindowVisible = value;
        if (!_isOverlayWindowVisible)
        {
          if (_videoWindowInterface != null)
          {
            _videoWindowInterface.put_Visible(OABool.False);
          }
        }
        else
        {
          if (_videoWindowInterface != null)
          {
            _videoWindowInterface.put_Visible(OABool.True);
          }
        }
      }
    }

    #endregion

    #region viewing

    /// <summary>
    /// StopViewing() 
    /// If we're currently in viewing mode 
    /// then we just close the overlay window and stop the graph
    /// </summary>
    public void StopViewing(VMR9Util vmr9)
    {
      if (false == _isRendered)
      {
        return;
      }
      Log.Info("mpeg2:StopViewing()");
      _isOverlayWindowVisible = false;
      if (_videoWindowInterface != null)
      {
        _videoWindowInterface.put_Visible(OABool.False);
        _videoWindowInterface.put_MessageDrain(IntPtr.Zero);
      }
      StopGraph();
      if (vmr9 != null)
      {
        vmr9.Dispose();
      }
      DirectShowUtil.RemoveDownStreamFilters(_graphBuilderInterface, _filterMpeg2Demultiplexer, false);
      _videoWindowInterface = null;
      _basicVideoInterface = null;
      _isRendered = false;
    }

    /// <summary>
    /// StartViewing()
    /// Will start the graph and create a new overlay window to show the live-tv in
    /// </summary>
    /// <param name="windowHandle">handle to parent window</param>
    public bool StartViewing(IntPtr windowHandle, VMR9Util vmr9)
    {
      // if the video window already has been created, but is hidden right now
      // then just show it and start the graph
      if (_isRendered)
      {
        _isOverlayWindowVisible = true;
        Log.Info("mpeg2:StartViewing()");
        Overlay = false;
        // start graph
        SetVideoWindow();
        StartGraph();
        Overlay = true;
        return true;
      }

      // video window has not been created yet, so create it
      Log.Info("mpeg2:StartViewing()");
      //render the video output. This will create the overlay render filter
      int hr = _graphBuilderInterface.Render(_pinVideoout);
      if (hr != 0)
      {
        Log.Error("mpeg2:FAILED to render mpeg2demux video out:0x{0:X}", hr);
        return false;
      }
      Log.Info("mpeg2:demux video out connected ");

      //render the audio output pin, this will create the audio renderer which plays the audio part
      IBaseFilter capture;
      //check for Adaptec USB device (uses LPCM pin)
      _graphBuilderInterface.FindFilterByName("Adaptec USB Capture Device", out capture);
      if (capture == null)
      {
        //failed to find Adaptec USB device - see if the PCI device is in the graph
        _graphBuilderInterface.FindFilterByName("Adaptec PCI Capture Device", out capture);
        if (capture == null)
        {
          //No Adaptec device found render std audio as normal
          //Log.Info("mpeg2:FAILED to find Adaptec Capture Device");
          hr = _graphBuilderInterface.Render(_pinAudioOut);
          if (hr != 0)
          {
            Log.Error("mpeg2:FAILED to render mpeg2demux audio out:0x{0:X}", hr);
            return false;
          }
          Log.Info("mpeg2:demux mpeg audio out connected ");
        }
      }
      else
      {
        //Found an aplicable Adaptec capture device
        Log.Info("mpeg2:Found Adaptec Capture Device");
        //However we need to check if this is a MCE device if so render std audio
        Log.Info("mpeg2:Checking if MCE Device");
        IPin pinMpgOut = DsFindPin.ByName(capture, "Mpeg Out");
        if (pinMpgOut == null)
        {
          //Adaptec device is not a MCE version
          hr = _graphBuilderInterface.Render(_pinLPCMOut);
          if (hr != 0)
          {
            Log.Error("mpeg2:FAILED to render mpeg2demux lpcm audio out:0x{0:X}", hr);
            return false;
          }
          Log.Info("mpeg:demux lpcm audio out connected");
        }
        else
        {
          Log.Info("mpeg2:Adaptec MCE device found - connecting mpeg audio");
          hr = _graphBuilderInterface.Render(_pinAudioOut);
          if (hr != 0)
          {
            Log.Error("mpeg2:FAILED to render mpeg2demux mpeg audio out:0x{0:X}", hr);
            return false;
          }
          Log.Info("mpeg2:demux mpeg audio out connected");
        }
      }
      bool useOverlay = true;
      if (vmr9 != null && vmr9.IsVMR9Connected && vmr9.UseVmr9)
      {
        useOverlay = false;
      }
      if (useOverlay)
      {
        // get the interfaces of the overlay window
        _videoWindowInterface = _graphBuilderInterface as IVideoWindow;
        _basicVideoInterface = _graphBuilderInterface as IBasicVideo2;
        if (_videoWindowInterface == null)
        {
          Log.Error("mpeg2:FAILED:could not get IVideoWindow");
          return false;
        }
        // set window message handler
        _videoWindowInterface.put_MessageDrain(GUIGraphicsContext.ActiveForm);
        // set the properties of the overlay window
        hr = _videoWindowInterface.put_Owner(GUIGraphicsContext.ActiveForm);
        if (hr != 0)
        {
          Log.Error("mpeg2:FAILED:set Video window:0x{0:X}", hr);
          return false;
        }
        hr =
          _videoWindowInterface.put_WindowStyle(
            (WindowStyle) ((int) WindowStyle.Child + (int) WindowStyle.ClipChildren + (int) WindowStyle.ClipSiblings));
        if (hr != 0)
        {
          Log.Error("mpeg2:FAILED:set Video window style:0x{0:X}", hr);
          return false;
        }
        // make the overlay window visible
        //    _isOverlayWindowVisible=true;
        hr = _videoWindowInterface.put_Visible(OABool.False);
        //    if( hr != 0 ) 
        //      Log.Info("mpeg2:FAILED:put_Visible:0x{0:X}",hr);
      }
      else
      {
        if (vmr9 != null)
        {
          vmr9.SetDeinterlaceMode();
        }
      }
      Overlay = false;
      // start the graph so we actually get to see the video
      _isRendered = true;
      SetVideoWindow();
      StartGraph();
      Overlay = true;
      return true;
    }

    private void SetVideoWindow()
    {
      if (_videoWindowInterface == null)
      {
        return;
      }

      int iVideoWidth, iVideoHeight;
      int aspectX, aspectY;
      GetVideoSize(out iVideoWidth, out iVideoHeight);
      GetPreferredAspectRatio(out aspectX, out aspectY);
      if (GUIGraphicsContext.IsFullScreenVideo || false == GUIGraphicsContext.ShowBackground)
      {
        float x = GUIGraphicsContext.OverScanLeft;
        float y = GUIGraphicsContext.OverScanTop;
        int nw = GUIGraphicsContext.OverScanWidth;
        int nh = GUIGraphicsContext.OverScanHeight;
        if (nw <= 0 || nh <= 0)
        {
          return;
        }

        Rectangle rSource, rDest;
        Geometry m_geometry = new Geometry();
        m_geometry.ImageWidth = iVideoWidth;
        m_geometry.ImageHeight = iVideoHeight;
        m_geometry.ScreenWidth = nw;
        m_geometry.ScreenHeight = nh;
        m_geometry.ARType = GUIGraphicsContext.ARType;
        m_geometry.PixelRatio = GUIGraphicsContext.PixelRatio;
        m_geometry.GetWindow(aspectX, aspectY, out rSource, out rDest);
        rDest.X += (int) x;
        rDest.Y += (int) y;

        Log.Info("overlay: video WxH  : {0}x{1}", iVideoWidth, iVideoHeight);
        Log.Info("overlay: video AR   : {0}:{1}", aspectX, aspectY);
        Log.Info("overlay: screen WxH : {0}x{1}", nw, nh);
        Log.Info("overlay: AR type    : {0}", GUIGraphicsContext.ARType);
        Log.Info("overlay: PixelRatio : {0}", GUIGraphicsContext.PixelRatio);
        Log.Info("overlay: src        : ({0},{1})-({2},{3})",
                 rSource.X, rSource.Y, rSource.X + rSource.Width, rSource.Y + rSource.Height);
        Log.Info("overlay: dst        : ({0},{1})-({2},{3})",
                 rDest.X, rDest.Y, rDest.X + rDest.Width, rDest.Y + rDest.Height);

        SetSourcePosition(rSource.Left, rSource.Top, rSource.Width, rSource.Height);
        SetDestinationPosition(0, 0, rDest.Width, rDest.Height);
        SetWindowPosition(rDest.Left, rDest.Top, rDest.Width, rDest.Height);
      }
      else
      {
        if (iVideoWidth > 0 && iVideoHeight > 0)
        {
          SetSourcePosition(0, 0, iVideoWidth, iVideoHeight);
        }

        if (GUIGraphicsContext.VideoWindow.Width > 0 && GUIGraphicsContext.VideoWindow.Height > 0)
        {
          SetDestinationPosition(0, 0, GUIGraphicsContext.VideoWindow.Width, GUIGraphicsContext.VideoWindow.Height);
        }

        if (GUIGraphicsContext.VideoWindow.Width > 0 && GUIGraphicsContext.VideoWindow.Height > 0)
        {
          SetWindowPosition(GUIGraphicsContext.VideoWindow.Left, GUIGraphicsContext.VideoWindow.Top,
                            GUIGraphicsContext.VideoWindow.Width, GUIGraphicsContext.VideoWindow.Height);
        }
      }
    }

    /// <summary>
    /// Returns the width/height of the live tv
    /// </summary>
    /// <param name="iWidth">width in pixels of the live tv</param>
    /// <param name="iHeight">height in pixels of the live tv</param>
    public void GetVideoSize(out int iWidth, out int iHeight)
    {
      iWidth = 0;
      iHeight = 0;
      if (_basicVideoInterface == null)
      {
        return;
      }
      _basicVideoInterface.GetVideoSize(out iWidth, out iHeight);
    }

    public void GetPreferredAspectRatio(out int aspectX, out int aspectY)
    {
      aspectX = 4;
      aspectY = 3;
      if (_basicVideoInterface == null)
      {
        return;
      }
      _basicVideoInterface.GetPreferredAspectRatio(out aspectX, out aspectY);
    }

    public void SetDestinationPosition(int x, int y, int width, int height)
    {
      if (!_isRendered)
      {
        return;
      }
      if (_basicVideoInterface == null)
      {
        return;
      }
      if (width <= 0 || height <= 0)
      {
        return;
      }
      if (x < 0 || y < 0)
      {
        return;
      }
      int hr = _basicVideoInterface.SetDestinationPosition(x, y, width, height);
      if (hr != 0)
      {
        Log.Error("mpeg2:FAILED:SetDestinationPosition:0x{0:X} ({1},{2})-{3},{4})", hr, x, y, width, height);
      }
    }

    public void SetSourcePosition(int x, int y, int width, int height)
    {
      if (!_isRendered)
      {
        return;
      }
      if (width <= 0 || height <= 0)
      {
        return;
      }
      if (x < 0 || y < 0)
      {
        return;
      }
      if (_basicVideoInterface == null)
      {
        return;
      }
      int hr = _basicVideoInterface.SetSourcePosition(x, y, width, height);
      if (hr != 0)
      {
        Log.Error("mpeg2:FAILED:SetSourcePosition:0x{0:X} ({1},{2})-{3},{4})", hr, x, y, width, height);
      }
    }

    public void SetWindowPosition(int x, int y, int width, int height)
    {
      if (!_isRendered)
      {
        return;
      }
      if (width <= 0 || height <= 0)
      {
        return;
      }
      if (x < 0 || y < 0)
      {
        return;
      }
      if (_videoWindowInterface == null)
      {
        return;
      }
      int hr = _videoWindowInterface.SetWindowPosition(x, y, width, height);
      if (hr != 0)
      {
        Log.Error("mpeg2:FAILED:SetWindowPosition:0x{0:X} ({1},{2})-{3},{4})", hr, x, y, width, height);
      }
    }

    #endregion

    #region radio

    public void StartListening()
    {
      Log.Info("mpeg2:StartListening() start mediactl");
      if (_isRendered)
      {
        if (_mediaControlInterface == null)
        {
          _mediaControlInterface = _graphBuilderInterface as IMediaControl;
        }

        StartGraph();
        return;
      }

      IBaseFilter capture;
      _graphBuilderInterface.FindFilterByName("Adaptec PCI Capture Device", out capture);
      if (capture == null)
      {
        _graphBuilderInterface.FindFilterByName("Adaptec USB Capture Device", out capture);
        if (capture == null)
        {
          //Log.Info("mpeg2:FAILED to find Adaptec Capture Device");
          int hr = _graphBuilderInterface.Render(_pinAudioOut);
          if (hr != 0)
          {
            Log.Error("mpeg2:FAILED to render mpeg2demux mpeg audio out:0x{0:X}", hr);
          }
          Log.Info("mpeg2:demux mpeg audio out connected ");
        }
      }
      else
      {
        Log.Info("mpeg2:Found Adaptec Capture Device");
        Log.Info("mpeg2:Checking if MCE Device");
        IPin pinMpgOut = DsFindPin.ByName(capture, "Mpeg Out");
        if (pinMpgOut == null)
        {
          int hr = _graphBuilderInterface.Render(_pinLPCMOut);
          if (hr != 0)
          {
            Log.Error("mpeg2:FAILED to render mpeg2demux LPCM audio out:0x{0:X}", hr);
          }
          Log.Info("mpeg2:demux lpcm audio out connected");
        }
        else
        {
          Log.Info("mpeg2:Adaptec MCE device found - connecting mpeg audio");
          int hr = _graphBuilderInterface.Render(_pinAudioOut);
          if (hr != 0)
          {
            Log.Error("mpeg2:FAILED to render mpeg2demux audio out:0x{0:X}", hr);
          }
          Log.Info("mpeg2:demux audio out connected");
        }
      }

      _isRendered = true;
      if (_mediaControlInterface == null)
      {
        _mediaControlInterface = _graphBuilderInterface as IMediaControl;
      }
      StartGraph();
    }

    public void StopListening()
    {
      if (_isRendered == false)
      {
        return;
      }

      StopGraph();

      DirectShowUtil.RemoveDownStreamFilters(_graphBuilderInterface, _filterMpeg2Demultiplexer, false);
      _isRendered = false;
    }

    #endregion

    #region timeshifting

    public void StopTimeShifting()
    {
      try
      {
        if (_isRendered)
        {
          int hr;
          Log.Info("mpeg2:StopTimeShifting()");
          if (_streamBufferSink3Interface != null)
          {
            IStreamBufferSink3 sink3 = _streamBufferSink3Interface as IStreamBufferSink3;
            if (sink3 != null)
            {
              Log.Info("mpeg2:unlock profile");
              hr = sink3.UnlockProfile();
              //if (hr !=0) Log.Info("mpeg2:FAILED to set unlock profile:0x{0:X}",hr);
            }
          }
          StopGraph();

          DirectShowUtil.RemoveDownStreamFilters(_graphBuilderInterface, _filterMpeg2Demultiplexer, false);
          DeleteSBESink();
          _isRendered = false;

          return;
        }
      }
      catch (Exception ex)
      {
        Log.Error("mpeg2:StopTimeShifting() exception:" + ex.ToString());
      }
    }

    public bool StartTimeshifting(string fileName)
    {
      int hr;
      if (!CreateSBESink())
      {
        return false;
      }

      fileName = Path.ChangeExtension(fileName, ".tv");
      Log.Info("mpeg2:StartTimeshifting({0})", fileName);
      int pos = fileName.LastIndexOf(@"\");
      string folder = fileName.Substring(0, pos);
      if (!_isRendered)
      {
        //DeleteOldTimeShiftFiles(folder);
        Log.Info("mpeg2:render graph");
        /// [               ]    [              ]    [                ]
        /// [mpeg2 demux vid] -> [video analyzer] -> [#0              ]
        /// [               ]    [              ]    [  streambuffer  ]
        /// [            aud] ---------------------> [#1              ]


        Log.Info("mpeg2:render to :{0}", fileName);
        if (_pinVideoout == null)
        {
          return false;
        }
        if (_pinVideoAnalyzerInput == null)
        {
          return false;
        }
        if (_pinStreamBufferIn0 == null)
        {
          return false;
        }

        //mpeg2 demux vid->analyzer in
        Log.Info("mpeg2:connect demux video out->analyzer in");
        hr = _graphBuilderInterface.Connect(_pinVideoout, _pinVideoAnalyzerInput);
        if (hr != 0)
        {
          Log.Error("mpeg2:FAILED to connect video out to analyzer:0x{0:X}", hr);
          return false;
        }
        Log.Info("mpeg2:demux video out connected to analyzer");
        //find analyzer out pin

        _pinVideoAnalyzerOutput = DsFindPin.ByDirection(_filterVideoAnalyzer, PinDirection.Output, 0);
        if (_pinVideoAnalyzerOutput == null)
        {
          Log.Error("mpeg2:FAILED to find analyser output pin");
          return false;
        }

        //analyzer out ->streambuffer in#0
        Log.Info("mpeg2:analyzer out->stream buffer");
        hr = _graphBuilderInterface.Connect(_pinVideoAnalyzerOutput, _pinStreamBufferIn0);
        if (hr != 0)
        {
          Log.Error("mpeg2:FAILED to connect analyzer output to streambuffer:0x{0:X}", hr);
          return false;
        }
        Log.Info("mpeg2:connected to streambuffer");

        //find streambuffer in#1 pin
        _pinStreamBufferIn1 = DsFindPin.ByDirection(_filterStreamBuffer, PinDirection.Input, 1);
        if (_pinStreamBufferIn1 == null)
        {
          Log.Error("mpeg2: FAILED to find input pin#1 of streambuffersink");
          return false;
        }
        //mpeg2 demux audio out->streambuffer in#1
        IBaseFilter capture;
        _graphBuilderInterface.FindFilterByName("Adaptec USB Capture Device", out capture);
        if (capture == null)
        {
          _graphBuilderInterface.FindFilterByName("Adaptec PCI Capture Device", out capture);
          if (capture == null)
          {
            //Log.Info("mpeg2:FAILED to find Adaptec Capture Device");
            Log.Info("mpeg2:demux mpeg audio out->stream buffer");
            hr = _graphBuilderInterface.Connect(_pinAudioOut, _pinStreamBufferIn1);
            if (hr != 0)
            {
              Log.Error("mpeg2:FAILED to connect audio out to streambuffer:0x{0:X}", hr);
              return false;
            }
            Log.Info("mpeg2:mpeg audio out connected to streambuffer");
          }
        }
        else
        {
          Log.Info("mpeg2:Found Adaptec Capture Device");
          Log.Info("mpeg2:Checking if MCE Device");
          IPin pinMpgOut = DsFindPin.ByName(capture, "Mpeg Out");
          if (pinMpgOut == null)
          {
            Log.Info("mpeg2:demux lpcm audio out->stream buffer");
            hr = _graphBuilderInterface.Connect(_pinLPCMOut, _pinStreamBufferIn1);
            if (hr != 0)
            {
              Log.Error("mpeg2:FAILED to connect lpcm audio out to streambuffer:0x{0:X}", hr);
              return false;
            }
            Log.Info("mpeg2:lpcm audio out connected to streambuffer");
          }
          else
          {
            Log.Info("mpeg2:Adaptec MCE device found - connecting mpeg audio");
            hr = _graphBuilderInterface.Connect(_pinAudioOut, _pinStreamBufferIn1);
            if (hr != 0)
            {
              Log.Error("mpeg2:FAILED to render mpeg2demux audio out:0x{0:X}", hr);
            }
            Log.Info("mpeg2:demux audio out connected ");
          }
        }

        //set mpeg2 demux as reference clock 
        (_graphBuilderInterface as IMediaFilter).SetSyncSource(_filterMpeg2Demultiplexer as IReferenceClock);

        //set filename
        _streamBufferSink3Interface = _filterStreamBuffer as IStreamBufferSink3;
        if (_streamBufferSink3Interface == null)
        {
          Log.Info("mpeg2:FAILED to get IStreamBufferSink interface");
          return false;
        }

        int iTimeShiftBuffer = 30;
        using (Settings xmlreader = new MPSettings())
        {
          iTimeShiftBuffer = xmlreader.GetValueAsInt("capture", "timeshiftbuffer", 30);
          if (iTimeShiftBuffer < 5)
          {
            iTimeShiftBuffer = 5;
          }
        }
        iTimeShiftBuffer *= 60; //in seconds
        int iFileDuration = iTimeShiftBuffer/6;
        Log.Info("mpeg2:Set folder:{0} filecount 6-8, fileduration:{1} sec", folder, iFileDuration);
        // set streambuffer backing file configuration
        m_StreamBufferConfig = new StreamBufferConfig();
        _streamBufferConfigureInterface = (IStreamBufferConfigure) m_StreamBufferConfig;
        IntPtr HKEY = (IntPtr) unchecked((int) 0x80000002L);
        IStreamBufferInitialize pTemp = (IStreamBufferInitialize) _streamBufferConfigureInterface;
        IntPtr subKey = IntPtr.Zero;
        RegOpenKeyEx(HKEY, "SOFTWARE\\MediaPortal", 0, 0x3f, out subKey);
        hr = pTemp.SetHKEY(subKey);
        if (hr != 0)
        {
          Log.Error("mpeg2: FAILED to set hkey:0x{0:X}", hr);
        }
        hr = _streamBufferConfigureInterface.SetDirectory(folder);
        if (hr != 0)
        {
          Log.Error("mpeg2: FAILED to set backingfile folder:0x{0:X}", hr);
          return false;
        }

#if DEBUG				
        hr=_streamBufferConfigureInterface.SetBackingFileCount(4, 6);    //4-6 files
				if (hr!=0) Log.Info("mpeg2: FAILED to set backingfile count:0x{0:X}",hr);

        hr=_streamBufferConfigureInterface.SetBackingFileDuration( 60); // 60sec * 4 files= 4 mins
        if (hr!=0) Log.Info("mpeg2: FAILED to set backingfile duration:0x{0:X}",hr);
#else
        hr = _streamBufferConfigureInterface.SetBackingFileCount(6, 8); //6-8 files
        if (hr != 0)
        {
          Log.Error("mpeg2: FAILED to set backingfile count:0x{0:X}", hr);
        }
        hr = _streamBufferConfigureInterface.SetBackingFileDuration((int) iFileDuration);
        if (hr != 0)
        {
          Log.Error("mpeg2: FAILED to set backingfile duration:0x{0:X}", hr);
        }
#endif
        IStreamBufferConfigure2 streamConfig2 = m_StreamBufferConfig as IStreamBufferConfigure2;
        if (streamConfig2 != null)
        {
          streamConfig2.SetFFTransitionRates(8, 32);
        }
      }
      // lock profile
      Log.Info("mpeg2:lock profile");
      hr = _streamBufferSink3Interface.LockProfile(fileName);
      if (hr != 0)
      {
        Log.Error("mpeg2:FAILED to set streambuffer filename:0x{0:X}", hr);
        return false;
      }
      _isRendered = true;
      Overlay = false;
      SetVideoWindow();
      StartGraph();
      Overlay = true;
      return true;
    }

    public void SetStartingPoint()
    {
      if (_streamBufferSink3Interface == null)
      {
        return;
      }
      long refTime = 0;
      _streamBufferSink3Interface.SetAvailableFilter(ref refTime);
    }

    #endregion

    #region setup

    private bool AddMpeg2Demultiplexer()
    {
      Log.Info("mpeg2:add new MPEG2 Demultiplexer to graph");
      try
      {
        _filterMpeg2Demultiplexer = (IBaseFilter) new MPEG2Demultiplexer();
      }
      catch (Exception)
      {
      }
      if (_filterMpeg2Demultiplexer == null)
      {
        Log.Error("mpeg2:FAILED to create mpeg2 demuxer");
        return false;
      }
      int hr = _graphBuilderInterface.AddFilter(_filterMpeg2Demultiplexer, "MPEG-2 Demultiplexer");
      if (hr != 0)
      {
        Log.Error("mpeg2:FAILED to add mpeg2 demuxer to graph:0x{0:X}", hr);
        return false;
      }
      _mpeg2DemultiplexerInterface = _filterMpeg2Demultiplexer as IMpeg2Demultiplexer;
      if (_mpeg2DemultiplexerInterface == null)
      {
        return false;
      }
      AMMediaType mpegVideoOut = new AMMediaType();
      mpegVideoOut.majorType = MediaType.Video;
      mpegVideoOut.subType = MediaSubType.Mpeg2Video;
      mpegVideoOut.unkPtr = IntPtr.Zero;
      mpegVideoOut.sampleSize = 0;
      mpegVideoOut.temporalCompression = false;
      mpegVideoOut.fixedSizeSamples = true;
      byte iWidthLo = (byte) (_sizeFrame.Width & 0xff);
      byte iWidthHi = (byte) (_sizeFrame.Width >> 8);
      byte iHeightLo = (byte) (_sizeFrame.Height & 0xff);
      byte iHeightHi = (byte) (_sizeFrame.Height >> 8);
      Mpeg2ProgramVideo[0x08] = iWidthLo;
      Mpeg2ProgramVideo[0x09] = iWidthHi;
      Mpeg2ProgramVideo[0x18] = iWidthLo;
      Mpeg2ProgramVideo[0x19] = iWidthHi;
      Mpeg2ProgramVideo[0x48] = iWidthLo;
      Mpeg2ProgramVideo[0x49] = iWidthHi;
      Mpeg2ProgramVideo[0x0C] = iHeightLo;
      Mpeg2ProgramVideo[0x0D] = iHeightHi;
      Mpeg2ProgramVideo[0x1C] = iHeightLo;
      Mpeg2ProgramVideo[0x1D] = iHeightHi;
      Mpeg2ProgramVideo[0x4C] = iHeightLo;
      Mpeg2ProgramVideo[0x4D] = iHeightHi;
      if (_sizeFrame.Height == 480)
      {
        //ntsc 
        Mpeg2ProgramVideo[0x28] = 0x36;
        Mpeg2ProgramVideo[0x29] = 0x17;
        Mpeg2ProgramVideo[0x2a] = 0x05;
      }
      else
      {
        //pal
        Mpeg2ProgramVideo[0x28] = 0x80;
        Mpeg2ProgramVideo[0x29] = 0x1A;
        Mpeg2ProgramVideo[0x2a] = 0x06;
      }
      mpegVideoOut.formatType = FormatType.Mpeg2Video;
      mpegVideoOut.formatSize = Mpeg2ProgramVideo.GetLength(0);
      mpegVideoOut.formatPtr = Marshal.AllocCoTaskMem(mpegVideoOut.formatSize);
      Marshal.Copy(Mpeg2ProgramVideo, 0, mpegVideoOut.formatPtr, mpegVideoOut.formatSize);
      AMMediaType mpegAudioOut = new AMMediaType();
      mpegAudioOut.majorType = MediaType.Audio;
      mpegAudioOut.subType = MediaSubType.Mpeg2Audio;
      mpegAudioOut.sampleSize = 0;
      mpegAudioOut.temporalCompression = false;
      mpegAudioOut.fixedSizeSamples = true;
      mpegAudioOut.unkPtr = IntPtr.Zero;
      mpegAudioOut.formatType = FormatType.WaveEx;
      mpegAudioOut.formatSize = MPEG1AudioFormat.GetLength(0);
      mpegAudioOut.formatPtr = Marshal.AllocCoTaskMem(mpegAudioOut.formatSize);
      Marshal.Copy(MPEG1AudioFormat, 0, mpegAudioOut.formatPtr, mpegAudioOut.formatSize);
      AMMediaType lpcmAudioOut = new AMMediaType();
      lpcmAudioOut.majorType = MediaType.Audio;
      lpcmAudioOut.subType = MediaSubType.DVD_LPCM_AUDIO;
      lpcmAudioOut.sampleSize = 0;
      lpcmAudioOut.temporalCompression = true;
      lpcmAudioOut.fixedSizeSamples = true;
      lpcmAudioOut.unkPtr = IntPtr.Zero;
      lpcmAudioOut.formatType = FormatType.WaveEx;
      lpcmAudioOut.formatSize = LPCMAudioFormat.GetLength(0);
      lpcmAudioOut.formatPtr = Marshal.AllocCoTaskMem(lpcmAudioOut.formatSize);
      Marshal.Copy(LPCMAudioFormat, 0, lpcmAudioOut.formatPtr, lpcmAudioOut.formatSize);
      Log.Info("mpeg2:create video out pin on MPEG2 demuxer");
      hr = _mpeg2DemultiplexerInterface.CreateOutputPin(mpegVideoOut /*vidOut*/, "video", out _pinVideoout);
      if (hr != 0)
      {
        Log.Error("mpeg2:FAILED to create videout pin:0x{0:X}", hr);
        return false;
      }
      Log.Info("mpeg2:create mpeg audio out pin on MPEG2 demuxer");
      hr = _mpeg2DemultiplexerInterface.CreateOutputPin(mpegAudioOut, "audio", out _pinAudioOut);
      if (hr != 0)
      {
        Log.Error("mpeg2:FAILED to create mpeg audio out pin:0x{0:X}", hr);
        return false;
      }
      Log.Info("mpeg2:create lpcm audio out pin on MPEG2 demuxer");
      hr = _mpeg2DemultiplexerInterface.CreateOutputPin(lpcmAudioOut, "lpcm", out _pinLPCMOut);
      if (hr != 0)
      {
        Log.Error("mpeg2:FAILED to create lpcm audio out pin:0x{0:X}", hr);
        return false;
      }
      Log.Info("mpeg2:find MPEG2 demuxer input pin");
      _pinDemuxerInput = DsFindPin.ByDirection(_filterMpeg2Demultiplexer, PinDirection.Input, 0);
      if (_pinDemuxerInput != null)
      {
        Log.Info("mpeg2:found MPEG2 demuxer input pin");
      }
      else
      {
        Log.Error("mpeg2:FAILED finding MPEG2 demuxer input pin");
        return false;
      }
      _isGraphRunning = false;
      _mediaControlInterface = _graphBuilderInterface as IMediaControl;
      if (_mediaControlInterface == null)
      {
        Log.Error("mpeg2:FAILED to get IMediaControl interface");
        return false;
      }
      return true;
    }

    public bool CreateMappings()
    {
      if (_pinVideoout == null)
      {
        return false;
      }
      if (_pinAudioOut == null)
      {
        return false;
      }
      if (_pinLPCMOut == null)
      {
        return false;
      }
      IMPEG2StreamIdMap pStreamId;
      Log.Info("mpeg2:MPEG2 demuxer map MPG stream 0xe0->video output pin");
      pStreamId = (IMPEG2StreamIdMap) _pinVideoout;
      int hr = pStreamId.MapStreamId(224, MPEG2Program.ElementaryStream, 0, 0);
        // hr := pStreamId.MapStreamId( 224, MPEG2_PROGRAM_ELEMENTARY_STREAM, 0, 0 );
      if (hr != 0)
      {
        Log.Error("mpeg2:FAILED to map stream 0xe0->video:0x{0:X}", hr);
        return false;
      }
      else
      {
        Log.Info("mpeg2:mapped MPEG2 demuxer stream 0xe0->video output ");
      }
      Log.Info("mpeg2:MPEG2 demuxer map MPG stream 0xc0->audio output pin");
      pStreamId = (IMPEG2StreamIdMap) _pinAudioOut;
      hr = pStreamId.MapStreamId(0xC0, MPEG2Program.ElementaryStream, 0, 0);
        // hr := pStreamId.MapStreamId( 0xC0, MPEG2_PROGRAM_ELEMENTARY_STREAM, 0, 0 );
      if (hr != 0)
      {
        Log.Error("mpeg2:FAILED to map stream 0xc0->audio:0x{0:X}", hr);
        return false;
      }
      else
      {
        Log.Info("mpeg2:mapped MPEG2 demuxer stream 0xc0->audio output");
      }
      Log.Info("mpeg2:MPEG2 demuxer map LPCM stream 0xbd->audio output pin");
      pStreamId = (IMPEG2StreamIdMap) _pinLPCMOut;
      hr = pStreamId.MapStreamId(0xBD, MPEG2Program.ElementaryStream, 0xA0, 7);
      if (hr != 0)
      {
        Log.Error("mpeg2:FAILED to map stream 0xbd->audio:0x{0:X}", hr);
        return false;
      }
      else
      {
        Log.Info("mpeg2:mapped MPEG2 demuxer stream 0xbd->audio output");
      }
      return true;
    }

    private void DeleteSBESink()
    {
      int hr;
      _streamBufferConfigureInterface = null;
      _streamBufferSink3Interface = null;
      _filterVideoAnalyzer = null;
      if (_pinVideoAnalyzerInput != null)
      {
        hr = DirectShowUtil.ReleaseComObject(_pinVideoAnalyzerInput);
        _pinVideoAnalyzerInput = null;
      }
      if (_pinVideoAnalyzerOutput != null)
      {
        hr = DirectShowUtil.ReleaseComObject(_pinVideoAnalyzerOutput);
        _pinVideoAnalyzerOutput = null;
      }
      if (_pinStreamBufferIn0 != null)
      {
        hr = DirectShowUtil.ReleaseComObject(_pinStreamBufferIn0);
        _pinStreamBufferIn0 = null;
      }
      if (_pinStreamBufferIn1 != null)
      {
        hr = DirectShowUtil.ReleaseComObject(_pinStreamBufferIn1);
        _pinStreamBufferIn1 = null;
      }
      if (_filterStreamBuffer != null)
      {
        _filterStreamBuffer.Stop();
        while ((hr = DirectShowUtil.ReleaseComObject(_filterStreamBuffer)) > 0)
        {
          ;
        }
        _filterStreamBuffer = null;
        if (hr != 0)
        {
          Log.Info("Sinkgraph:ReleaseComobject(_filterStreamBuffer):{0}", hr);
        }
      }
      if (m_VideoAnalyzer != null)
      {
        while ((hr = DirectShowUtil.ReleaseComObject(m_VideoAnalyzer)) > 0)
        {
          ;
        }
        if (hr != 0)
        {
          Log.Info("Sinkgraph:ReleaseComobject(m_VideoAnalyzer):{0}", hr);
        }
        m_VideoAnalyzer = null;
      }
      if (m_StreamBufferConfig != null)
      {
        while ((hr = DirectShowUtil.ReleaseComObject(m_StreamBufferConfig)) > 0)
        {
          ;
        }
        if (hr != 0)
        {
          Log.Info("Sinkgraph:ReleaseComobject(m_StreamBufferConfig):{0}", hr);
        }
        m_StreamBufferConfig = null;
      }
    }

    private bool CreateSBESink()
    {
      if (m_VideoAnalyzer != null)
      {
        return true;
      }
      Log.Info("mpeg2:add Videoanalyzer");
      try
      {
        m_VideoAnalyzer = new VideoAnalyzer();
        _filterVideoAnalyzer = (IBaseFilter) m_VideoAnalyzer;
      }
      catch (Exception)
      {
      }
      if (_filterVideoAnalyzer == null)
      {
        Log.Error("mpeg2:FAILED to add Videoanalyzer (You need at least Windows XP SP1!!)");
        return false;
      }
      _graphBuilderInterface.AddFilter(_filterVideoAnalyzer, "MPEG-2 Video Analyzer");
      _pinVideoAnalyzerInput = DsFindPin.ByDirection(_filterVideoAnalyzer, PinDirection.Input, 0);
      if (_pinVideoAnalyzerInput == null)
      {
        Log.Info("mpeg2:FAILED to find analyser input pin");
        return false;
      }
      Log.Info("mpeg2:add streambuffersink");
      _filterStreamBuffer = (IBaseFilter) new StreamBufferSink();
      if (_filterStreamBuffer == null)
      {
        Log.Error("mpeg2:FAILED to add streambuffer");
        return false;
      }
      _graphBuilderInterface.AddFilter(_filterStreamBuffer, "SBE SINK");
      IntPtr subKey = IntPtr.Zero;
      IntPtr HKEY = (IntPtr) unchecked((int) 0x80000002L);
      IStreamBufferInitialize pConfig = (IStreamBufferInitialize) _filterStreamBuffer;
      //  IntPtr[] sids = new IntPtr[] {pSid};
      //  int result = pConfig.SetSIDs(1, sids);
      RegOpenKeyEx(HKEY, "SOFTWARE\\MediaPortal", 0, 0x3f, out subKey);
      int hr = pConfig.SetHKEY(subKey);
      if (hr != 0)
      {
        Log.Error("mpeg2: FAILED to set hkey:0x{0:X}", hr);
      }
      _pinStreamBufferIn0 = DsFindPin.ByDirection(_filterStreamBuffer, PinDirection.Input, 0);
      if (_pinStreamBufferIn0 == null)
      {
        Log.Error("mpeg2: FAILED to find input pin#0 of streambuffersink");
        return false;
      }
      return true;
    }

    public void Dispose()
    {
      int hr = 0;
      Log.Info("mpeg2: close interfaces");
      if (_recorderId >= 0)
      {
        StopRecording();
      }
      if (_mediaControlInterface != null)
      {
        StopGraph();
        _mediaControlInterface = null;
      }
      if (_videoWindowInterface != null)
      {
        _videoWindowInterface.put_Visible(OABool.False);
      }
      _videoWindowInterface = null;
      _streamBufferConfigureInterface = null;
      _streamBufferSink3Interface = null;
      _mpeg2DemultiplexerInterface = null;
      _graphBuilderInterface = null;
      _filterVideoAnalyzer = null;
      if (_pinAudioOut != null)
      {
        hr = DirectShowUtil.ReleaseComObject(_pinAudioOut);
        _pinAudioOut = null;
      }
      if (_pinLPCMOut != null)
      {
        hr = DirectShowUtil.ReleaseComObject(_pinLPCMOut);
        _pinLPCMOut = null;
      }
      if (_pinVideoout != null)
      {
        hr = DirectShowUtil.ReleaseComObject(_pinVideoout);
        _pinVideoout = null;
      }
      if (_pinDemuxerInput != null)
      {
        hr = DirectShowUtil.ReleaseComObject(_pinDemuxerInput);
        _pinDemuxerInput = null;
      }
      if (_pinVideoAnalyzerInput != null)
      {
        hr = DirectShowUtil.ReleaseComObject(_pinVideoAnalyzerInput);
        _pinVideoAnalyzerInput = null;
      }
      if (_pinVideoAnalyzerOutput != null)
      {
        hr = DirectShowUtil.ReleaseComObject(_pinVideoAnalyzerOutput);
        _pinVideoAnalyzerOutput = null;
      }
      if (_pinStreamBufferIn0 != null)
      {
        hr = DirectShowUtil.ReleaseComObject(_pinStreamBufferIn0);
        _pinStreamBufferIn0 = null;
      }
      if (_pinStreamBufferIn1 != null)
      {
        hr = DirectShowUtil.ReleaseComObject(_pinStreamBufferIn1);
        _pinStreamBufferIn1 = null;
      }
      if (_filterStreamBuffer != null)
      {
        _filterStreamBuffer.Stop();
        while ((hr = DirectShowUtil.ReleaseComObject(_filterStreamBuffer)) > 0)
        {
          ;
        }
        _filterStreamBuffer = null;
        if (hr != 0)
        {
          Log.Info("Sinkgraph:ReleaseComobject(_filterStreamBuffer):{0}", hr);
        }
      }
      if (m_VideoAnalyzer != null)
      {
        while ((hr = DirectShowUtil.ReleaseComObject(m_VideoAnalyzer)) > 0)
        {
          ;
        }
        if (hr != 0)
        {
          Log.Info("Sinkgraph:ReleaseComobject(m_VideoAnalyzer):{0}", hr);
        }
        m_VideoAnalyzer = null;
      }
      if (m_StreamBufferConfig != null)
      {
        while ((hr = DirectShowUtil.ReleaseComObject(m_StreamBufferConfig)) > 0)
        {
          ;
        }
        if (hr != 0)
        {
          Log.Info("Sinkgraph:ReleaseComobject(m_StreamBufferConfig):{0}", hr);
        }
        m_StreamBufferConfig = null;
      }
      if (_filterMpeg2Demultiplexer != null)
      {
        while ((hr = DirectShowUtil.ReleaseComObject(_filterMpeg2Demultiplexer)) > 0)
        {
          ;
        }
        if (hr != 0)
        {
          Log.Info("Sinkgraph:ReleaseComobject(_filterMpeg2Demultiplexer):{0}", hr);
        }
        _filterMpeg2Demultiplexer = null;
      }
    }

    #endregion

    #region recording

    /// <summary>
    /// This method will start recording and will write all data to fileName
    /// </summary>
    /// <param name="fileName">file where recording should b saved</param>
    /// <param name="isContentRecording">
    /// when true it will make a content recording. A content recording writes the data to a new permanent file. 
    /// when false it will make a reference recording. A reference recording creates a stub file that refers to the existing backing files, which are made permanent. Create a reference recording if you want to save data that has already been captured.</param>
    public bool Record(Hashtable attribtutes, string fileName, bool isContentRecording, DateTime timeProgStart,
                       DateTime timeFirstMoment)
    {
      //      fileName=@"C:\media\movies\test.dvr-ms";
      Log.Info("mpeg2: Record : {0} {1} {2}", fileName, _isRendered, isContentRecording);
      uint recordingType = 0;
      if (isContentRecording)
      {
        recordingType = 0;
      }
      else
      {
        recordingType = 1;
      }

      if (!DvrMsCreate(out _recorderId, (IBaseFilter) _streamBufferSink3Interface, fileName, recordingType))
      {
        Log.Error("mpeg2:StartRecording() FAILED to create recording");
        return false;
      }
      long startTime = 0;

      // if we're making a reference recording
      // then record all content from the past as well
      if (!isContentRecording)
      {
        // so set the startttime...
        int secondsPerFile;
        int minimumFiles, maximumFileCount;
        _streamBufferConfigureInterface.GetBackingFileCount(out minimumFiles, out maximumFileCount);
        _streamBufferConfigureInterface.GetBackingFileDuration(out secondsPerFile);
        startTime = secondsPerFile;
        startTime *= (long) maximumFileCount;

        // if start of program is given, then use that as our starttime
        if (timeProgStart.Year > 2000)
        {
          TimeSpan ts = DateTime.Now - timeProgStart;
          Log.Info("mpeg2:Start recording from {0}:{1:00}:{2:00} which is {3:00}:{4:00}:{5:00} in the past",
                   timeProgStart.Hour, timeProgStart.Minute, timeProgStart.Second,
                   ts.TotalHours, ts.TotalMinutes, ts.TotalSeconds);

          startTime = (long) ts.TotalSeconds;
        }
        else
        {
          Log.Info("mpeg2:record entire timeshift buffer");
        }

        TimeSpan tsMaxTimeBack = DateTime.Now - timeFirstMoment;
        if (startTime > tsMaxTimeBack.TotalSeconds)
        {
          startTime = (long) tsMaxTimeBack.TotalSeconds;
        }
      }

      /*
            foreach (MetadataItem item in attribtutes.Values)
            {
              try
              {
                if (item.Type == MetadataItemType.String)
                  m_recorder.SetAttributeString(item.Name,item.Value.ToString());
                if (item.Type == MetadataItemType.Dword)
                  m_recorder.SetAttributeDWORD(item.Name,UInt32.Parse(item.Value.ToString()));
              }
              catch(Exception){}
            }
      */
      DvrMsStart(_recorderId, (uint) startTime);
      return true;
    }

    public void StopRecording()
    {
      if (_recorderId < 0)
      {
        return;
      }
      Log.Info("mpeg2: stop recording");
      DvrMsStop(_recorderId);
      _recorderId = -1;
    }

    #endregion

    #region start/stop graph

    private void StartGraph()
    {
      if (_mediaControlInterface == null)
      {
        return;
      }
      if (_isGraphRunning)
      {
        return;
      }
      _mediaControlInterface.Run();
      _isGraphRunning = true;
      Log.Info("mpeg2: mediactl started");
    }

    private void StopGraph()
    {
      if (_mediaControlInterface == null)
      {
        return;
      }
      if (!_isGraphRunning)
      {
        return;
      }
      Log.Info("mpeg2: stop mediactl");
      _mediaControlInterface.Stop();
      _isGraphRunning = false;
      Log.Info("mpeg2: stopped mediactl");
    }

    #endregion
  }
}
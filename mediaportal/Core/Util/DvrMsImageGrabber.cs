/* 
 *	Copyright (C) 2005 Team MediaPortal
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
using System;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.Drawing;
using System.Threading;

using System.Runtime.InteropServices;
using System.Windows.Forms;
using DShowNET;
using DShowNET.Helper;
using DirectShowLib;
using MediaPortal.Player;
using MediaPortal.GUI.Library;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;

namespace MediaPortal.Util
{
  public class DvrMsImageGrabber : IDisposable, IVMR9PresentCallback
  {


    #region imports
    [DllImport("dshowhelper.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern bool Vmr9Init(IVMR9PresentCallback callback, uint dwD3DDevice, IBaseFilter vmr9Filter, uint monitor);
    [DllImport("dshowhelper.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern void Vmr9Deinit();
    [DllImport("dshowhelper.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern void Vmr9SetDeinterlaceMode(Int16 mode);
    [DllImport("dshowhelper.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern void Vmr9SetDeinterlacePrefs(uint dwMethod);
    #endregion

    IGraphBuilder _graphBuilder;
    IBaseFilter _videoCodecFilter;
    IBaseFilter _streamBufferFilter;
    IBaseFilter _vmr9Filter;
    IMediaFilter _mediaFilt;
    IMediaPosition _mediaPos;
    IMediaControl _mediaControl;
    DsROTEntry _rotEntry;
    int _frameCounter;
    bool _grabFrame;
    string _fileName;
    ImageFormat _format;
    int _width;
    int _height;
    long _frameToGrab;

    public DvrMsImageGrabber(string dvrMsFileName)
    {
      _grabFrame = false;
      int hr;
      string videoCodec = String.Empty;
      _graphBuilder = (IGraphBuilder)new FilterGraph();
      _rotEntry = new DsROTEntry((IFilterGraph)_graphBuilder);

      _vmr9Filter = (IBaseFilter)new VideoMixingRenderer9();
      _graphBuilder.AddFilter(_vmr9Filter, "Vmr9");

      IntPtr hMonitor;
      AdapterInformation ai = Manager.Adapters.Default;
      hMonitor = Manager.GetAdapterMonitor(ai.Adapter);
      IntPtr upDevice = DirectShowUtil.GetUnmanagedDevice(GUIGraphicsContext.DX9Device);
      Vmr9Init(this, (uint)upDevice.ToInt32(), _vmr9Filter, (uint)hMonitor.ToInt32());


      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        videoCodec = xmlreader.GetValueAsString("mytv", "videocodec", "Mpeg2Dec Filter");
      }
      _videoCodecFilter = DirectShowUtil.AddFilterToGraph(_graphBuilder, videoCodec);

      _streamBufferFilter = new StreamBufferSource() as IBaseFilter;

      _graphBuilder.AddFilter(_streamBufferFilter, "StreamBuffer");
      IFileSourceFilter sourceInterface = _streamBufferFilter as IFileSourceFilter;
      hr = sourceInterface.Load(dvrMsFileName, null);


      // dvrms->videocodec
      IPin pinOut = DsFindPin.ByDirection(_streamBufferFilter, PinDirection.Output, 1);
      IPin pinIn = DsFindPin.ByDirection(_videoCodecFilter, PinDirection.Input, 0);
      hr = _graphBuilder.Connect(pinOut, pinIn);


      // videocodec->vmr9
      pinOut = DsFindPin.ByDirection(_videoCodecFilter, PinDirection.Output, 0);
      pinIn = DsFindPin.ByDirection(_vmr9Filter, PinDirection.Input, 0);
      hr = _graphBuilder.Connect(pinOut, pinIn);



      _mediaFilt = _graphBuilder as IMediaFilter;
      _mediaPos = _graphBuilder as IMediaPosition;
      _mediaControl = _graphBuilder as IMediaControl;
      hr = _mediaFilt.SetSyncSource(null);

    }

    public void GrabFrame(long frame, string fileName, ImageFormat format, int width, int height)
    {
      _fileName = fileName;
      _format = format;
      _width = width;
      _height = height;
      int hr;
      _frameCounter = 0;
      _frameToGrab = frame;
      _grabFrame = true;
      hr = _mediaControl.Run();

      DateTime timeStart = DateTime.Now;
      long prevFrame = 0;
      while (_frameCounter < _frameToGrab)
      {
        System.Windows.Forms.Application.DoEvents();
        TimeSpan ts = DateTime.Now - timeStart;
        if (ts.TotalSeconds > 1)
        {
          if (prevFrame == _frameCounter)
            break;
          prevFrame = _frameCounter;
          timeStart = DateTime.Now;
        }
      }
      _mediaControl.Stop();
    }

    public void Dispose()
    {
      if (_mediaControl != null)
      {
        _mediaControl.Stop();
      }
      _mediaControl = null;
      _mediaPos = null;
      _mediaFilt = null;
      if (_vmr9Filter != null)
      {
        Vmr9Deinit();
        while (Marshal.ReleaseComObject(_vmr9Filter) > 0) ;
      }
      _vmr9Filter = null;

      if (_videoCodecFilter != null)
      {
        while (Marshal.ReleaseComObject(_videoCodecFilter) > 0) ;
      }
      _videoCodecFilter = null;

      if (_streamBufferFilter != null)
      {
        while (Marshal.ReleaseComObject(_streamBufferFilter) > 0) ;
      }
      _streamBufferFilter = null;

      if (_streamBufferFilter != null)
      {
        while (Marshal.ReleaseComObject(_streamBufferFilter) > 0) ;
      }
      _streamBufferFilter = null;

      if (_rotEntry != null)
      {
        _rotEntry.Dispose();
      }
      _rotEntry = null;

      if (_graphBuilder != null)
      {
        while (Marshal.ReleaseComObject(_graphBuilder) > 0) ;
      }
      _graphBuilder = null;

    }


    public int PresentImage(Int16 cx, Int16 cy, Int16 arx, Int16 ary, uint dwImg)
    {
      return 0;
    }

    public int PresentSurface(Int16 cx, Int16 cy, Int16 arx, Int16 ary, uint dwImg)
    {
      if (!_grabFrame) return 0;
      if (cx <= 0 || cy <= 0) return 0;
      _frameCounter++;
      if (_frameCounter == _frameToGrab)
      {
        unsafe
        {
          Surface surface = new Surface(new IntPtr(dwImg));
          Utils.FileDelete(_fileName);
          SurfaceLoader.Save("temp.bmp", ImageFileFormat.Bmp, surface);
          using (Image bmp = Image.FromFile("temp.bmp"))
          {
            //keep aspect ratio:-)
            float ar = ((float)ary) / ((float)arx);
            _height = (int)(((float)_width) * ar);
            using (Bitmap result = new Bitmap(_width, _height))
            {
              using (Graphics g = Graphics.FromImage(result))
              {
                g.DrawImage(bmp, new Rectangle(0, 0, _width, _height));
              }
              result.Save(_fileName, _format);
            }
          }
          Utils.FileDelete("temp.bmp");
        }
      }
      return 0;
    }
  }
}

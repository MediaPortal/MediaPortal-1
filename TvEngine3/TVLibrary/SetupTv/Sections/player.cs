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
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using DirectShowLib;
//using DShowNET.TsFileSink;
using TvLibrary.Log;
using System.Windows.Forms;
namespace SetupTv.Sections
{
  class Player
  {
    #region structs
    static byte[] Mpeg2ProgramVideo = 
    {
          0x00, 0x00, 0x00, 0x00,							//  .hdr.rcSource.left
          0x00, 0x00, 0x00, 0x00,							//  .hdr.rcSource.top
          0xd0, 0x02, 0x00, 0x00,							//  .hdr.rcSource.right
          0x40, 0x02, 0x00, 0x00,							//  .hdr.rcSource.bottom
          0x00, 0x00, 0x00, 0x00,							//  .hdr.rcTarget.left
          0x00, 0x00, 0x00, 0x00,							//  .hdr.rcTarget.top
          0x00, 0x00, 0x00, 0x00,							//  .hdr.rcTarget.right
          0x00, 0x00, 0x00, 0x00,							//  .hdr.rcTarget.bottom
          0xc0, 0xe1, 0xe4, 0x00,							//  .hdr.dwBitRate
          0x00, 0x00, 0x00, 0x00,							//  .hdr.dwBitErrorRate
          0x80, 0x1a, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, //  .hdr.AvgTimePerFrame
          0x00, 0x00, 0x00, 0x00,							//  .hdr.dwInterlaceFlags
          0x00, 0x00, 0x00, 0x00,							//  .hdr.dwCopyProtectFlags
          0x00, 0x00, 0x00, 0x00,							//  .hdr.dwPictAspectRatioX
          0x00, 0x00, 0x00, 0x00,							//  .hdr.dwPictAspectRatioY
          0x00, 0x00, 0x00, 0x00,							//  .hdr.dwReserved1
          0x00, 0x00, 0x00, 0x00,							//  .hdr.dwReserved2
          0x28, 0x00, 0x00, 0x00,							//  .hdr.bmiHeader.biSize
          0xd0, 0x02, 0x00, 0x00,							//  .hdr.bmiHeader.biWidth
          0x40, 0x02, 0x00, 0x00,							//  .hdr.bmiHeader.biHeight
          0x00, 0x00,										//  .hdr.bmiHeader.biPlanes
          0x00, 0x00,										//  .hdr.bmiHeader.biBitCount
          0x00, 0x00, 0x00, 0x00,							//  .hdr.bmiHeader.biCompression
          0x00, 0x00, 0x00, 0x00,							//  .hdr.bmiHeader.biSizeImage
          0xd0, 0x07, 0x00, 0x00,							//  .hdr.bmiHeader.biXPelsPerMeter
          0x42, 0xd8, 0x00, 0x00,							//  .hdr.bmiHeader.biYPelsPerMeter
          0x00, 0x00, 0x00, 0x00,							//  .hdr.bmiHeader.biClrUsed
          0x00, 0x00, 0x00, 0x00,							//  .hdr.bmiHeader.biClrImportant
          0x00, 0x00, 0x00, 0x00,							//  .dwStartTimeCode
          0x4c, 0x00, 0x00, 0x00,							//  .cbSequenceHeader
          0x00, 0x00, 0x00, 0x00,							//  .dwProfile
          0x00, 0x00, 0x00, 0x00,							//  .dwLevel
          0x00, 0x00, 0x00, 0x00,							//  .Flags
					                        //  .dwSequenceHeader [1]
          0x00, 0x00, 0x01, 0xb3, 0x2d, 0x02, 0x40, 0x33, 
          0x24, 0x9f, 0x23, 0x81, 0x10, 0x11, 0x11, 0x12, 
          0x12, 0x12, 0x13, 0x13, 0x13, 0x13, 0x14, 0x14, 
          0x14, 0x14, 0x14, 0x15, 0x15, 0x15, 0x15, 0x15, 
          0x15, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 
          0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 
          0x18, 0x18, 0x18, 0x19, 0x18, 0x18, 0x18, 0x19, 
          0x1a, 0x1a, 0x1a, 0x1a, 0x19, 0x1b, 0x1b, 0x1b, 
          0x1b, 0x1b, 0x1c, 0x1c, 0x1c, 0x1c, 0x1e, 0x1e, 
          0x1e, 0x1f, 0x1f, 0x21, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
          0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
    };

    static byte[] MPEG2AudioFormat =
      {	
        0x50, 0x00,				//wFormatTag
	      0x02, 0x00,				//nChannels
	      0x80, 0xbb, 0x00, 0x00, //nSamplesPerSec
	      0x00, 0x7d, 0x00, 0x00, //nAvgBytesPerSec
	      0x01, 0x00,				//nBlockAlign
	      0x00, 0x00,				//wBitsPerSample
	      0x16, 0x00,				//cbSize
	      0x02, 0x00,				//wValidBitsPerSample
	      0x00, 0xe8,				//wSamplesPerBlock
	      0x03, 0x00,				//wReserved
	      0x01, 0x00, 0x01, 0x00, //dwChannelMask
	      0x01, 0x00, 0x16, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
      };
    #endregion
    [ComImport, Guid("b9559486-E1BB-45D3-A2A2-9A7AFE49B23F")]
    protected class TsReader
    {
    }

    protected IFilterGraph2 _graphBuilder = null;
    protected DsROTEntry _rotEntry;
    protected IBaseFilter _tsReader;
    protected IPin _pinVideo;
    protected IPin _pinAudio;
    IMediaControl _mediaCtrl;
    protected IVideoWindow _videoWin = null;
    protected Form _form;

    public bool Play(string fileName, Form form)
    {
      _form = form;
      Log.WriteFile("play:{0}", fileName);
      int hr;
      _graphBuilder = (IFilterGraph2)new FilterGraph();
      _rotEntry = new DsROTEntry((IFilterGraph)_graphBuilder);

      TsReader reader = new TsReader();
      _tsReader = (IBaseFilter)reader;
      Log.Info("TSReaderPlayer:add TsReader to graph");
      _graphBuilder.AddFilter((IBaseFilter)_tsReader, "TsReader");

      #region load file in TsReader
      Log.WriteFile("load file in Ts");
      IFileSourceFilter interfaceFile = (IFileSourceFilter)_tsReader;
      if (interfaceFile == null)
      {
        Log.WriteFile("TSReaderPlayer:Failed to get IFileSourceFilter");
        return false;
      }
      hr = interfaceFile.Load(fileName, null);

      if (hr != 0)
      {
        Log.WriteFile("TSReaderPlayer:Failed to load file");
        return false;
      }
      #endregion


      #region render pin
      Log.Info("TSReaderPlayer:render TsReader outputs");
      IEnumPins enumPins;
      _tsReader.EnumPins(out enumPins);
      IPin[] pins = new IPin[2];
      int fetched = 0;
      while (enumPins.Next(1, pins, out fetched) == 0)
      {
        if (fetched != 1) break;
        PinDirection direction;
        pins[0].QueryDirection(out direction);
        if (direction == PinDirection.Input)
        {
          Marshal.ReleaseComObject(pins[0]);
          continue;
        }
        _graphBuilder.Render(pins[0]);
        Marshal.ReleaseComObject(pins[0]);
      }
       Marshal.ReleaseComObject(enumPins);
	  
      #endregion


      _videoWin = _graphBuilder as IVideoWindow;
      _videoWin.put_Visible(OABool.True);
      _videoWin.put_Owner(form.Handle);
      _videoWin.put_WindowStyle((WindowStyle)((int)WindowStyle.Child + (int)WindowStyle.ClipSiblings + (int)WindowStyle.ClipChildren));
      _videoWin.put_MessageDrain(form.Handle);

      _videoWin.SetWindowPosition(form.ClientRectangle.X, form.ClientRectangle.Y, form.ClientRectangle.Width, form.ClientRectangle.Height);

      Log.WriteFile("run graph");
      _mediaCtrl = (IMediaControl)_graphBuilder;
      hr = _mediaCtrl.Run();
      Log.WriteFile("TSReaderPlayer:running:{0:X}", hr);

      return true;
    }
    public void Stop()
    {
      if (_videoWin != null)
      {
        _videoWin.put_Visible(OABool.False);
      }
      if (_mediaCtrl != null)
      {
        _mediaCtrl.Stop();
      }
      if (_pinAudio != null)
      {
        Marshal.ReleaseComObject(_pinAudio); _pinAudio = null;
      }
      if (_pinVideo != null)
      {
        Marshal.ReleaseComObject(_pinVideo); _pinVideo = null;
      }
      if (_tsReader != null)
      {
        Marshal.ReleaseComObject(_tsReader); _tsReader = null;
      }
      if (_rotEntry != null)
      {
        _rotEntry.Dispose(); _rotEntry = null;
      }
      
      if (_graphBuilder != null)
      {
        Marshal.ReleaseComObject(_graphBuilder); _graphBuilder = null;
      }
    }
    AMMediaType GetAudioMpg2Media()
    {
      AMMediaType mediaAudio = new AMMediaType();
      mediaAudio.majorType = MediaType.Audio;
      mediaAudio.subType = MediaSubType.Mpeg2Audio;
      mediaAudio.formatType = FormatType.WaveEx;
      mediaAudio.formatPtr = IntPtr.Zero;
      mediaAudio.sampleSize = 1;
      mediaAudio.temporalCompression = false;
      mediaAudio.fixedSizeSamples = true;
      mediaAudio.unkPtr = IntPtr.Zero;
      mediaAudio.formatType = FormatType.WaveEx;
      mediaAudio.formatSize = MPEG2AudioFormat.GetLength(0);
      mediaAudio.formatPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(mediaAudio.formatSize);
      System.Runtime.InteropServices.Marshal.Copy(MPEG2AudioFormat, 0, mediaAudio.formatPtr, mediaAudio.formatSize);
      return mediaAudio;
    }
    AMMediaType GetVideoMpg2Media()
    {
      AMMediaType mediaVideo = new AMMediaType();
      mediaVideo.majorType = MediaType.Video;
      mediaVideo.subType = MediaSubType.Mpeg2Video;
      mediaVideo.formatType = FormatType.Mpeg2Video;
      mediaVideo.unkPtr = IntPtr.Zero;
      mediaVideo.sampleSize = 1;
      mediaVideo.temporalCompression = false;
      mediaVideo.fixedSizeSamples = true;
      mediaVideo.formatSize = Mpeg2ProgramVideo.GetLength(0);
      mediaVideo.formatPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(mediaVideo.formatSize);
      System.Runtime.InteropServices.Marshal.Copy(Mpeg2ProgramVideo, 0, mediaVideo.formatPtr, mediaVideo.formatSize);
      return mediaVideo;
    }
    public void ResizeToParent()
    {
      _videoWin.SetWindowPosition(_form.ClientRectangle.X, _form.ClientRectangle.Y, _form.ClientRectangle.Width, _form.ClientRectangle.Height);
    }
  }
}

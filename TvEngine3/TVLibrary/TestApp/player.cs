using System;
using System.Runtime.InteropServices;
using DirectShowLib;
using TvLibrary.Log;
using System.Windows.Forms;

namespace TestApp
{
  internal class Player
  {
    #region structs

    private static readonly byte[] Mpeg2ProgramVideo =
      {
        0x00, 0x00, 0x00, 0x00, //  .hdr.rcSource.left
        0x00, 0x00, 0x00, 0x00, //  .hdr.rcSource.top
        0xd0, 0x02, 0x00, 0x00, //  .hdr.rcSource.right
        0x40, 0x02, 0x00, 0x00, //  .hdr.rcSource.bottom
        0x00, 0x00, 0x00, 0x00, //  .hdr.rcTarget.left
        0x00, 0x00, 0x00, 0x00, //  .hdr.rcTarget.top
        0x00, 0x00, 0x00, 0x00, //  .hdr.rcTarget.right
        0x00, 0x00, 0x00, 0x00, //  .hdr.rcTarget.bottom
        0xc0, 0xe1, 0xe4, 0x00, //  .hdr.dwBitRate
        0x00, 0x00, 0x00, 0x00, //  .hdr.dwBitErrorRate
        0x80, 0x1a, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, //  .hdr.AvgTimePerFrame
        0x00, 0x00, 0x00, 0x00, //  .hdr.dwInterlaceFlags
        0x00, 0x00, 0x00, 0x00, //  .hdr.dwCopyProtectFlags
        0x00, 0x00, 0x00, 0x00, //  .hdr.dwPictAspectRatioX
        0x00, 0x00, 0x00, 0x00, //  .hdr.dwPictAspectRatioY
        0x00, 0x00, 0x00, 0x00, //  .hdr.dwReserved1
        0x00, 0x00, 0x00, 0x00, //  .hdr.dwReserved2
        0x28, 0x00, 0x00, 0x00, //  .hdr.bmiHeader.biSize
        0xd0, 0x02, 0x00, 0x00, //  .hdr.bmiHeader.biWidth
        0x40, 0x02, 0x00, 0x00, //  .hdr.bmiHeader.biHeight
        0x00, 0x00, //  .hdr.bmiHeader.biPlanes
        0x00, 0x00, //  .hdr.bmiHeader.biBitCount
        0x00, 0x00, 0x00, 0x00, //  .hdr.bmiHeader.biCompression
        0x00, 0x00, 0x00, 0x00, //  .hdr.bmiHeader.biSizeImage
        0xd0, 0x07, 0x00, 0x00, //  .hdr.bmiHeader.biXPelsPerMeter
        0x42, 0xd8, 0x00, 0x00, //  .hdr.bmiHeader.biYPelsPerMeter
        0x00, 0x00, 0x00, 0x00, //  .hdr.bmiHeader.biClrUsed
        0x00, 0x00, 0x00, 0x00, //  .hdr.bmiHeader.biClrImportant
        0x00, 0x00, 0x00, 0x00, //  .dwStartTimeCode
        0x4c, 0x00, 0x00, 0x00, //  .cbSequenceHeader
        0x00, 0x00, 0x00, 0x00, //  .dwProfile
        0x00, 0x00, 0x00, 0x00, //  .dwLevel
        0x00, 0x00, 0x00, 0x00, //  .Flags
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

    private static readonly byte[] MPEG2AudioFormat =
      {
        0x50, 0x00, //wFormatTag
        0x02, 0x00, //nChannels
        0x80, 0xbb, 0x00, 0x00, //nSamplesPerSec
        0x00, 0x7d, 0x00, 0x00, //nAvgBytesPerSec
        0x01, 0x00, //nBlockAlign
        0x00, 0x00, //wBitsPerSample
        0x16, 0x00, //cbSize
        0x02, 0x00, //wValidBitsPerSample
        0x00, 0xe8, //wSamplesPerBlock
        0x03, 0x00, //wReserved
        0x01, 0x00, 0x01, 0x00, //dwChannelMask
        0x01, 0x00, 0x16, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
      };

    #endregion

    [ComImport, Guid("4F8BF30C-3BEB-43A3-8BF2-10096FD28CF2")]
    protected class TsFileSource {}

    protected IFilterGraph2 _graphBuilder;
    protected DsROTEntry _rotEntry;
    protected TsFileSource _tsFileSource;
    protected IBaseFilter _mpegDemux;
    protected IPin _pinVideo;
    protected IPin _pinAudio;
    private IMediaControl _mediaCtrl;
    protected IVideoWindow _videoWin;
    private bool _paused;

    public bool Play(string fileName, Form form)
    {
      fileName += ".tsbuffer";
      Log.WriteFile("play:{0}", fileName);
      _graphBuilder = (IFilterGraph2)new FilterGraph();
      _rotEntry = new DsROTEntry(_graphBuilder);


      Log.WriteFile("add tsfilesource");
      _tsFileSource = new TsFileSource();
      _graphBuilder.AddFilter((IBaseFilter)_tsFileSource, "TsFileSource");

      #region add mpeg-2 demux filter

      Log.WriteFile("add mpeg-2 demux");
      MPEG2Demultiplexer demux = new MPEG2Demultiplexer();
      _mpegDemux = (IBaseFilter)demux;
      int hr = _graphBuilder.AddFilter(_mpegDemux, "MPEG-2 Demultiplexer");

      #endregion

      #region create mpeg2 demux pins

      Log.WriteFile("create mpeg-2 demux pins");
      //create mpeg-2 demux output pins
      IMpeg2Demultiplexer demuxer = _mpegDemux as IMpeg2Demultiplexer;


      if (demuxer != null)
        hr = demuxer.CreateOutputPin(GetAudioMpg2Media(), "Audio", out _pinAudio);
      if (hr != 0)
      {
        Log.WriteFile("unable to create audio pin");
        return false;
      }
      if (demuxer != null)
        hr = demuxer.CreateOutputPin(GetVideoMpg2Media(), "Video", out _pinVideo);
      if (hr != 0)
      {
        Log.WriteFile("unable to create video pin");
        return false;
      }

      #endregion

      #region load file in tsfilesource

      Log.WriteFile("load file in tsfilesource");
      IFileSourceFilter interfaceFile = (IFileSourceFilter)_tsFileSource;
      if (interfaceFile == null)
      {
        Log.WriteFile("TSStreamBufferPlayer9:Failed to get IFileSourceFilter");
        return false;
      }

      AMMediaType mpeg2ProgramStream = new AMMediaType();
      mpeg2ProgramStream.majorType = MediaType.Stream;
      mpeg2ProgramStream.subType = MediaSubType.Mpeg2Program;

      mpeg2ProgramStream.unkPtr = IntPtr.Zero;
      mpeg2ProgramStream.sampleSize = 0;
      mpeg2ProgramStream.temporalCompression = false;
      mpeg2ProgramStream.fixedSizeSamples = true;
      mpeg2ProgramStream.formatType = FormatType.None;
      mpeg2ProgramStream.formatSize = 0;
      mpeg2ProgramStream.formatPtr = IntPtr.Zero;
      hr = interfaceFile.Load(fileName, mpeg2ProgramStream);

      if (hr != 0)
      {
        Log.WriteFile("TSStreamBufferPlayer9:Failed to load file");
        return false;
      }

      #region connect tsfilesource->demux

      Log.WriteFile("connect tsfilesource->demux");
      Log.WriteFile("TSStreamBufferPlayer9:connect tsfilesource->mpeg2 demux");
      IPin pinTsOut = DsFindPin.ByDirection((IBaseFilter)_tsFileSource, PinDirection.Output, 0);
      if (pinTsOut == null)
      {
        Log.WriteFile("TSStreamBufferPlayer9:failed to find output pin of tsfilesource");
        return false;
      }
      IPin pinDemuxIn = DsFindPin.ByDirection(_mpegDemux, PinDirection.Input, 0);
      if (pinDemuxIn == null)
      {
        Log.WriteFile("TSStreamBufferPlayer9:failed to find output pin of tsfilesource");
        return false;
      }

      hr = _graphBuilder.Connect(pinTsOut, pinDemuxIn);
      if (hr != 0)
      {
        Log.WriteFile("TSStreamBufferPlayer9:failed to connect tsfilesource->mpeg2 demux:{0:X}", hr);
        return false;
      }
      Marshal.ReleaseComObject(pinTsOut);
      Marshal.ReleaseComObject(pinDemuxIn);

      #endregion

      #region map demux pids

      Log.WriteFile("map mpeg2 pids");
      IMPEG2StreamIdMap pStreamId = (IMPEG2StreamIdMap)_pinVideo;
      hr = pStreamId.MapStreamId(0xe0, MPEG2Program.ElementaryStream, 0, 0);
      if (hr != 0)
      {
        Log.WriteFile("TSStreamBufferPlayer9: failed to map pid 0xe0->video pin");
        return false;
      }
      pStreamId = (IMPEG2StreamIdMap)_pinAudio;
      hr = pStreamId.MapStreamId(0xc0, MPEG2Program.ElementaryStream, 0, 0);
      if (hr != 0)
      {
        Log.WriteFile("TSStreamBufferPlayer9: failed  to map pid 0xc0->audio pin");
        return false;
      }

      #endregion

      #region render demux audio/video pins

      Log.WriteFile("render pins");
      hr = _graphBuilder.Render(_pinAudio);
      if (hr != 0)
      {
        Log.WriteFile("TSStreamBufferPlayer9:failed to render video output pin:{0:X}", hr);
      }

      hr = _graphBuilder.Render(_pinVideo);
      if (hr != 0)
      {
        Log.WriteFile("TSStreamBufferPlayer9:failed to render audio output pin:{0:X}", hr);
      }

      #endregion

      #endregion

      _videoWin = _graphBuilder as IVideoWindow;
      if (_videoWin != null)
      {
        _videoWin.put_Visible(OABool.True);
        _videoWin.put_Owner(form.Handle);
        _videoWin.put_WindowStyle(
          (WindowStyle)((int)WindowStyle.Child + (int)WindowStyle.ClipSiblings + (int)WindowStyle.ClipChildren));
        _videoWin.put_MessageDrain(form.Handle);
        _videoWin.SetWindowPosition(190, 250, 150, 150);
      }

      Log.WriteFile("run graph");
      _mediaCtrl = (IMediaControl)_graphBuilder;
      hr = _mediaCtrl.Run();
      Log.WriteFile("TSStreamBufferPlayer9:running:{0:X}", hr);

      return true;
    }

    public void Stop()
    {
      _videoWin.put_Visible(OABool.False);
      _mediaCtrl.Stop();

      if (_pinAudio != null)
      {
        Marshal.ReleaseComObject(_pinAudio);
        _pinAudio = null;
      }
      if (_pinVideo != null)
      {
        Marshal.ReleaseComObject(_pinVideo);
        _pinVideo = null;
      }
      if (_mpegDemux != null)
      {
        Marshal.ReleaseComObject(_mpegDemux);
        _mpegDemux = null;
      }
      if (_tsFileSource != null)
      {
        Marshal.ReleaseComObject(_tsFileSource);
        _tsFileSource = null;
      }
      if (_rotEntry != null)
      {
        _rotEntry.Dispose();
        _rotEntry = null;
      }

      if (_graphBuilder != null)
      {
        Marshal.ReleaseComObject(_graphBuilder);
        _graphBuilder = null;
      }
    }

    public bool Paused
    {
      get { return _paused; }
      set
      {
        _paused = value;
        if (_paused)
          _mediaCtrl.Pause();
        else
          _mediaCtrl.Run();
      }
    }

    private static AMMediaType GetAudioMpg2Media()
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
      mediaAudio.formatPtr = Marshal.AllocCoTaskMem(mediaAudio.formatSize);
      Marshal.Copy(MPEG2AudioFormat, 0, mediaAudio.formatPtr, mediaAudio.formatSize);
      return mediaAudio;
    }

    private static AMMediaType GetVideoMpg2Media()
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
      mediaVideo.formatPtr = Marshal.AllocCoTaskMem(mediaVideo.formatSize);
      Marshal.Copy(Mpeg2ProgramVideo, 0, mediaVideo.formatPtr, mediaVideo.formatSize);
      return mediaVideo;
    }
  }
}
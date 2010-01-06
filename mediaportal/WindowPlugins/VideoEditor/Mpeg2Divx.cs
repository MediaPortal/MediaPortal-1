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
using System.Threading;
using DirectShowLib;
using DShowNET.Helper;
using MediaPortal.Core.Transcoding;
using MediaPortal.GUI.Library;
using Quality = MediaPortal.Core.Transcoding.Quality;

namespace WindowPlugins.VideoEditor
{
  internal class Mpeg2Divx : ITranscode
  {
    protected DsROTEntry _rotEntry = null;
    protected IGraphBuilder graphBuilder = null;
    protected IFileSinkFilter bufferSource = null; //IStreamBufferSource bufferSource = null;
    protected IFileSinkFilter2 fileWriterFilter = null; // DShow Filter: file writer
    protected IMediaControl mediaControl = null;
    //protected IStreamBufferMediaSeeking mediaSeeking = null;
    protected IMediaSeeking mediaSeeking = null;
    protected IMediaPosition mediaPos = null;
    protected IBaseFilter divxCodec = null;
    protected IBaseFilter mp3Codec = null;
    protected IBaseFilter Mpeg2VideoCodec = null;
    protected IBaseFilter Mpeg2AudioCodec = null;
    protected IBaseFilter aviMuxer = null;
    protected IBaseFilter mpegDemuxer = null;
    protected IMediaEventEx mediaEvt = null;
    protected int bitrate;
    protected int fps;
    protected Size screenSize;
    protected long m_dDuration;
    protected const int WS_CHILD = 0x40000000; // attributes for video window
    protected const int WS_CLIPCHILDREN = 0x02000000;
    protected const int WS_CLIPSIBLINGS = 0x04000000;

    public Mpeg2Divx() {}

    #region ITranscode Members

    public bool Supports(VideoFormat format)
    {
      if (format == VideoFormat.Divx)
      {
        return true;
      }
      return false;
    }

    public void CreateProfile(Size videoSize, int bitRate, int FPS)
    {
      bitrate = bitRate;
      screenSize = videoSize;
      fps = FPS;
    }

    public bool Transcode(TranscodeInfo info, VideoFormat format, Quality quality, Standard standard)
    {
      if (!Supports(format))
      {
        return false;
      }
      string ext = Path.GetExtension(info.file);
      if (ext.ToLower() != ".mpeg" && ext.ToLower() != ".mpg")
      {
        return false;
      }

      //disable xvid status window while encoding
      /*  try
				{
					using (RegistryKey subkey = Registry.CurrentUser.OpenSubKey(@"Software\GNU\XviD", true))
					{
						if (subkey != null)
						{
							Int32 uivalue = 0;
							subkey.SetValue("display_status", (Int32)uivalue);
							subkey.SetValue("debug", (Int32)uivalue);
							subkey.SetValue("bitrate", (Int32)bitrate);

							uivalue = 1;
							subkey.SetValue("interlacing", (Int32)uivalue);
						}
					}
				}
				catch (Exception)
				{
				}*/
      //Type comtype = null;
      //object comobj = null;
      try
      {
        graphBuilder = (IGraphBuilder)new FilterGraph();

        _rotEntry = new DsROTEntry((IFilterGraph)graphBuilder);

        Log.Info("MPEG2DIVX: add filesource");
        //bufferSource = new AsyncReader();//(IStreamBufferSource)new StreamBufferSource();

        IFileSourceFilter fileSource = (IFileSourceFilter)new AsyncReader(); //(IFileSourceFilter)bufferSource;
        IBaseFilter filter = (IBaseFilter)fileSource;
        graphBuilder.AddFilter(filter, "File Source");

        Log.Info("MPEG2DIVX: load file:{0}", info.file);
        int hr = fileSource.Load(info.file, null);

        string strDemuxerMoniker =
          @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{AFB6C280-2C41-11D3-8A60-0000F81E0E4A}";

        mpegDemuxer = Marshal.BindToMoniker(strDemuxerMoniker) as IBaseFilter;
        if (mpegDemuxer == null)
        {
          Log.Error("MPEG2DIVX:FAILED:unable to add mpeg2 demuxer");
          Cleanup();
          return false;
        }
        hr = graphBuilder.AddFilter(mpegDemuxer, "MPEG-2 Demultiplexer");
        if (hr != 0)
        {
          Log.Error("MPEG2DIVX:FAILED:Add mpeg2 demuxer to filtergraph :0x{0:X}", hr);
          Cleanup();
          return false;
        }
        //add mpeg2 audio/video codecs
        string strVideoCodecMoniker =
          @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{39F498AF-1A09-4275-B193-673B0BA3D478}";
        string strAudioCodec = "MPC - MPA Decoder Filter";
        Log.Info("MPEG2DIVX: add MPV mpeg2 video decoder");
        Mpeg2VideoCodec = Marshal.BindToMoniker(strVideoCodecMoniker) as IBaseFilter;
        if (Mpeg2VideoCodec == null)
        {
          Log.Error("MPEG2DIVX:FAILED:unable to add MPV mpeg2 video decoder");
          Cleanup();
          return false;
        }
        hr = graphBuilder.AddFilter(Mpeg2VideoCodec, "MPC - MPEG-2 Video Decoder (Gabest)");
        if (hr != 0)
        {
          Log.Error("MPEG2DIVX:FAILED:Add MPV mpeg2 video  to filtergraph :0x{0:X}", hr);
          Cleanup();
          return false;
        }

        Log.Info("MPEG2DIVX: add MPA mpeg2 audio codec:{0}", strAudioCodec);
        Mpeg2AudioCodec = DirectShowUtil.AddFilterToGraph(graphBuilder, strAudioCodec);
        if (Mpeg2AudioCodec == null)
        {
          Log.Error("MPEG2DIVX:FAILED:unable to add MPV mpeg2 audio codec");
          Cleanup();
          return false;
        }

        //connect output #0 of streambuffer source->mpeg2 audio codec pin 1
        //connect output #1 of streambuffer source->mpeg2 video codec pin 1
        Log.Info("MPEG2DIVX: connect filesource->mpeg audio/video decoders");
        IPin pinOut0, pinOut1;
        IPin pinIn0, pinIn1;

        pinOut0 = DsFindPin.ByDirection((IBaseFilter)fileSource, PinDirection.Output, 0);
        if (pinOut0 == null)
        {
          Log.Error("MPEG2DIVX:FAILED:unable to get pins of source");
          Cleanup();
          return false;
        }
        pinIn0 = DsFindPin.ByDirection(mpegDemuxer, PinDirection.Input, 0);
        if (pinIn0 == null)
        {
          Log.Error("MPEG2DIVX:FAILED:unable to get pins of demuxer");
          Cleanup();
          return false;
        }

        hr = graphBuilder.Connect(pinOut0, pinIn0);
        if (hr != 0)
        {
          Log.Error("MPEG2DIVX:FAILED:unable to connect audio pins :0x{0:X}", hr);
          Cleanup();
          return false;
        }


        pinOut0 = DsFindPin.ByDirection((IBaseFilter)mpegDemuxer, PinDirection.Output, 0); //audio
        pinOut1 = DsFindPin.ByDirection((IBaseFilter)mpegDemuxer, PinDirection.Output, 1); //video
        if (pinOut0 == null || pinOut1 == null)
        {
          Log.Error("MPEG2DIVX:FAILED:unable to get pins of demuxer");
          Cleanup();
          return false;
        }

        pinIn0 = DsFindPin.ByDirection(Mpeg2VideoCodec, PinDirection.Input, 0); //video
        pinIn1 = DsFindPin.ByDirection(Mpeg2AudioCodec, PinDirection.Input, 0); //audio
        if (pinIn0 == null || pinIn1 == null)
        {
          Log.Error("MPEG2DIVX:FAILED:unable to get pins of mpeg2 video/audio codec");
          Cleanup();
          return false;
        }

        hr = graphBuilder.Connect(pinOut0, pinIn1);
        if (hr != 0)
        {
          Log.Error("MPEG2DIVX:FAILED:unable to connect audio pins :0x{0:X}", hr);
          Cleanup();
          return false;
        }


        hr = graphBuilder.Connect(pinOut1, pinIn0);
        if (hr != 0)
        {
          Log.Error("MPEG2DIVX:FAILED:unable to connect video pins :0x{0:X}", hr);
          Cleanup();
          return false;
        }
        if (!AddCodecs(graphBuilder, info))
        {
          return false;
        }

        //				hr=(graphBuilder as IMediaFilter).SetSyncSource(null);
        //				if (hr!=0)
        //					Log.Error("DVR2XVID:FAILED:to SetSyncSource :0x{0:X}",hr);
        mediaControl = graphBuilder as IMediaControl;
        mediaSeeking = graphBuilder as IMediaSeeking; //fileSource as IMediaSeeking;
        mediaEvt = graphBuilder as IMediaEventEx;
        mediaPos = graphBuilder as IMediaPosition;

        //get file duration
        Log.Info("MPEG2DIVX: Get duration of movie");
        long lTime = 5 * 60 * 60;
        lTime *= 10000000;
        long pStop = 0;
        hr = mediaSeeking.SetPositions(new DsLong(lTime), AMSeekingSeekingFlags.AbsolutePositioning, new DsLong(pStop),
                                       AMSeekingSeekingFlags.NoPositioning);
        if (hr == 0)
        {
          long lStreamPos;
          mediaSeeking.GetCurrentPosition(out lStreamPos); // stream position
          m_dDuration = lStreamPos;
          lTime = 0;
          mediaSeeking.SetPositions(new DsLong(lTime), AMSeekingSeekingFlags.AbsolutePositioning, new DsLong(pStop),
                                    AMSeekingSeekingFlags.NoPositioning);
        }
        double duration = m_dDuration / 10000000d;
        //      Log.Info("DVR2XVID: movie duration:{0}", Util.Utils.SecondsToHMSString((int)duration));

        //				hr=(graphBuilder as IMediaFilter).SetSyncSource(null);
        //				if (hr!=0)
        //					Log.Error("DVR2XVID:FAILED:to SetSyncSource :0x{0:X}",hr);
        hr = mediaControl.Run();
        if (hr != 0)
        {
          Log.Error("MPEG2DIVX:FAILED:unable to start graph :0x{0:X}", hr);
          Cleanup();
          return false;
        }
        int maxCount = 20;
        while (true)
        {
          long lCurrent;
          mediaSeeking.GetCurrentPosition(out lCurrent);
          double dpos = (double)lCurrent;
          dpos /= 10000000d;
          Thread.Sleep(100);
          if (dpos >= 2.0d)
          {
            break;
          }
          maxCount--;
          if (maxCount <= 0)
          {
            break;
          }
        }

        mediaControl.Stop();
        FilterState state;
        mediaControl.GetState(500, out state);
        GC.Collect();
        GC.Collect();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        graphBuilder.RemoveFilter(aviMuxer);
        graphBuilder.RemoveFilter(divxCodec);
        graphBuilder.RemoveFilter(mp3Codec);
        graphBuilder.RemoveFilter((IBaseFilter)fileWriterFilter);
        if (!AddCodecs(graphBuilder, info))
        {
          return false;
        }

        //				hr=(graphBuilder as IMediaFilter).SetSyncSource(null);
        //			if (hr!=0)
        //					Log.Error("DVR2XVID:FAILED:to SetSyncSource :0x{0:X}",hr);

        Log.Info("MPEG2DIVX: start transcoding");
        hr = mediaControl.Run();
        if (hr != 0)
        {
          Log.Error("MPEG2DIVX:FAILED:unable to start graph :0x{0:X}", hr);
          Cleanup();
          return false;
        }
      }
      catch (Exception ex)
      {
        Log.Error("MPEG2DIVX:Unable create graph: {0}", ex.Message);
        Cleanup();
        return false;
      }
      return true;
    }

    public bool IsFinished()
    {
      if (mediaControl == null)
      {
        return true;
      }
      FilterState state;

      mediaControl.GetState(200, out state);
      if (state == FilterState.Stopped)
      {
        Cleanup();
        return true;
      }
      int p1, p2, hr = 0;
      EventCode code;
      hr = mediaEvt.GetEvent(out code, out p1, out p2, 0);
      hr = mediaEvt.FreeEventParams(code, p1, p2);
      if (code == EventCode.Complete || code == EventCode.ErrorAbort)
      {
        Cleanup();
        return true;
      }
      return false;
    }

    public int Percentage()
    {
      if (mediaSeeking == null)
      {
        return 100;
      }
      long lCurrent;
      mediaSeeking.GetCurrentPosition(out lCurrent);
      float percent = ((float)lCurrent) / ((float)m_dDuration);
      percent *= 50.0f;
      if (percent > 100)
      {
        percent = 100;
      }
      return (int)percent;
    }

    public bool IsTranscoding()
    {
      if (IsFinished())
      {
        return false;
      }
      return true;
    }

    private void Cleanup()
    {
      Log.Info("MPEG2DIVX: cleanup");
      if (_rotEntry != null)
      {
        _rotEntry.Dispose();
      }
      _rotEntry = null;

      if (mediaControl != null)
      {
        mediaControl.Stop();
        mediaControl = null;
      }
      fileWriterFilter = null;
      mediaSeeking = null;
      mediaEvt = null;
      mediaPos = null;
      mediaControl = null;

      if (divxCodec != null)
      {
        DirectShowUtil.ReleaseComObject(divxCodec);
      }
      divxCodec = null;

      if (mp3Codec != null)
      {
        DirectShowUtil.ReleaseComObject(mp3Codec);
      }
      mp3Codec = null;

      if (aviMuxer != null)
      {
        DirectShowUtil.ReleaseComObject(aviMuxer);
      }
      aviMuxer = null;

      if (Mpeg2AudioCodec != null)
      {
        DirectShowUtil.ReleaseComObject(Mpeg2AudioCodec);
      }
      Mpeg2AudioCodec = null;

      if (Mpeg2VideoCodec != null)
      {
        DirectShowUtil.ReleaseComObject(Mpeg2VideoCodec);
      }
      Mpeg2VideoCodec = null;


      if (bufferSource != null)
      {
        DirectShowUtil.ReleaseComObject(bufferSource);
      }
      bufferSource = null;

      DirectShowUtil.RemoveFilters(graphBuilder);

      if (graphBuilder != null)
      {
        DirectShowUtil.ReleaseComObject(graphBuilder);
      }
      graphBuilder = null;
      GC.Collect();
      GC.Collect();
      GC.Collect();
      GC.WaitForPendingFinalizers();
    }

    #endregion

    private bool AddCodecs(IGraphBuilder graphBuilder, TranscodeInfo info)
    {
      int hr;
      Log.Info("MPEG2DIVX: add ffdshow (Divx) codec to graph");
      string monikerXVID = @"@device:sw:{33D9A760-90C8-11D0-BD43-00A0C911CE86}\ffdshow video encoder";
      divxCodec = Marshal.BindToMoniker(monikerXVID) as IBaseFilter;
      if (divxCodec == null)
      {
        Log.Error("MPEG2DIVX:FAILED:Unable to create Divx MPEG-4 Codec");
        Cleanup();
        return false;
      }

      hr = graphBuilder.AddFilter(divxCodec, "ffdshow video encoder");
      if (hr != 0)
      {
        Log.Error("MPEG2DIVX:FAILED:Add DivX MPEG-4 Codec to filtergraph :0x{0:X}", hr);
        Cleanup();
        return false;
      }


      Log.Info("MPEG2DIVX: add MPEG3 codec to graph");
      string monikerMPEG3 = @"@device:cm:{33D9A761-90C8-11D0-BD43-00A0C911CE86}\85MPEG Layer-3";
      mp3Codec = Marshal.BindToMoniker(monikerMPEG3) as IBaseFilter;
      if (mp3Codec == null)
      {
        Log.Error("MPEG2DIVX:FAILED:Unable to create MPEG Layer-3 Codec");
        Cleanup();
        return false;
      }

      hr = graphBuilder.AddFilter(mp3Codec, "MPEG Layer-3");
      if (hr != 0)
      {
        Log.Error("MPEG2DIVX:FAILED:Add MPEG Layer-3 to filtergraph :0x{0:X}", hr);
        Cleanup();
        return false;
      }

      //add filewriter 
      Log.Info("DVR2XVID: add FileWriter to graph");
      string monikerFileWrite =
        @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{8596E5F0-0DA5-11D0-BD21-00A0C911CE86}";
      IBaseFilter fileWriterbase = Marshal.BindToMoniker(monikerFileWrite) as IBaseFilter;
      if (fileWriterbase == null)
      {
        Log.Error("MPEG2DIVX:FAILED:Unable to create FileWriter");
        Cleanup();
        return false;
      }


      fileWriterFilter = fileWriterbase as IFileSinkFilter2;
      if (fileWriterFilter == null)
      {
        Log.Error("MPEG2DIVX:FAILED:Add unable to get IFileSinkFilter for filewriter");
        Cleanup();
        return false;
      }

      hr = graphBuilder.AddFilter(fileWriterbase, "FileWriter");
      if (hr != 0)
      {
        Log.Error("MPEG2DIVX:FAILED:Add FileWriter to filtergraph :0x{0:X}", hr);
        Cleanup();
        return false;
      }


      //set output filename
      //AMMediaType mt = new AMMediaType();
      string outputFileName = Path.ChangeExtension(info.file, ".avi");
      Log.Info("MPEG2DIVX: set output file to :{0}", outputFileName);
      hr = fileWriterFilter.SetFileName(outputFileName, null);
      if (hr != 0)
      {
        Log.Error("MPEG2DIVX:FAILED:unable to set filename for filewriter :0x{0:X}", hr);
        Cleanup();
        return false;
      }

      // add avi muxer
      Log.Info("MPEG2DIVX: add AVI Muxer to graph");
      string monikerAviMuxer =
        @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{E2510970-F137-11CE-8B67-00AA00A3F1A6}";
      aviMuxer = Marshal.BindToMoniker(monikerAviMuxer) as IBaseFilter;
      if (aviMuxer == null)
      {
        Log.Error("MPEG2DIVX:FAILED:Unable to create AviMux");
        Cleanup();
        return false;
      }


      hr = graphBuilder.AddFilter(aviMuxer, "AviMux");
      if (hr != 0)
      {
        Log.Error("MPEG2DIVX:FAILED:Add AviMux to filtergraph :0x{0:X}", hr);
        Cleanup();
        return false;
      }


      //connect output of mpeg2 codec to xvid codec
      Log.Info("MPEG2DIVX: connect mpeg2 video codec->divx codec");
      IPin pinOut, pinIn;
      pinIn = DsFindPin.ByDirection(divxCodec, PinDirection.Input, 0);
      if (pinIn == null)
      {
        Log.Error("MPEG2DIVX:FAILED:cannot get input pin of divx codec:0x{0:X}", hr);
        Cleanup();
        return false;
      }
      pinOut = DsFindPin.ByDirection(Mpeg2VideoCodec, PinDirection.Output, 0);
      if (pinOut == null)
      {
        Log.Error("MPEG2DIVX:FAILED:cannot get output pin of mpeg2 video codec :0x{0:X}", hr);
        Cleanup();
        return false;
      }

      hr = graphBuilder.Connect(pinOut, pinIn);
      if (hr != 0)
      {
        Log.Error("MPEG2DIVX:FAILED:unable to connect mpeg2 video codec->divx:0x{0:X}", hr);
        Cleanup();
        return false;
      }

      //connect output of mpeg2 audio codec to mpeg3 codec
      Log.Info("MPEG2DIVX: connect mpeg2 audio codec->mp3 codec");
      pinIn = DsFindPin.ByDirection(mp3Codec, PinDirection.Input, 0);
      if (pinIn == null)
      {
        Log.Error("MPEG2DIVX:FAILED:cannot get input pin of mp3 codec:0x{0:X}", hr);
        Cleanup();
        return false;
      }
      pinOut = DsFindPin.ByDirection(Mpeg2AudioCodec, PinDirection.Output, 0);
      if (pinOut == null)
      {
        Log.Error("MPEG2DIVX:FAILED:cannot get output pin of mpeg2 audio codec :0x{0:X}", hr);
        Cleanup();
        return false;
      }

      hr = graphBuilder.Connect(pinOut, pinIn);
      if (hr != 0)
      {
        Log.Error("MPEG2DIVX:FAILED:unable to connect mpeg2 audio codec->mpeg3:0x{0:X}", hr);
        Cleanup();
        return false;
      }


      //connect output of mpeg3 codec to pin#0 of avimux
      Log.Info("MPEG2DIVX: connect mp3 codec->avimux");
      pinOut = DsFindPin.ByDirection(mp3Codec, PinDirection.Output, 0);
      if (pinOut == null)
      {
        Log.Error("MPEG2DIVX:FAILED:cannot get input pin of mp3 codec:0x{0:X}", hr);
        Cleanup();
        return false;
      }
      pinIn = DsFindPin.ByDirection(aviMuxer, PinDirection.Input, 0);
      if (pinIn == null)
      {
        Log.Error("MPEG2DIVX:FAILED:cannot get output pin of mpeg2 audio codec :0x{0:X}", hr);
        Cleanup();
        return false;
      }

      hr = graphBuilder.Connect(pinOut, pinIn);
      if (hr != 0)
      {
        Log.Error("MPEG2DIVX:FAILED:unable to connect mpeg3 codec->avimux:0x{0:X}", hr);
        Cleanup();
        return false;
      }

      //connect output of xvid codec to pin#1 of avimux
      Log.Info("MPEG2DIVX: connect divx codec->avimux");
      pinOut = DsFindPin.ByDirection(divxCodec, PinDirection.Output, 0);
      if (pinOut == null)
      {
        Log.Error("MPEG2DIVX:FAILED:cannot get input pin of mp3 codec:0x{0:X}", hr);
        Cleanup();
        return false;
      }
      pinIn = DsFindPin.ByDirection(aviMuxer, PinDirection.Input, 1);
      if (pinIn == null)
      {
        Log.Error("MPEG2DIVX:FAILED:cannot get output#1 pin of avimux :0x{0:X}", hr);
        Cleanup();
        return false;
      }

      hr = graphBuilder.Connect(pinOut, pinIn);
      if (hr != 0)
      {
        Log.Error("MPEG2DIVX:FAILED:unable to connect divx codec->avimux:0x{0:X}", hr);
        Cleanup();
        return false;
      }


      //connect avi mux out->filewriter in
      Log.Info("MPEG2DIVX: connect avimux->filewriter");
      pinOut = DsFindPin.ByDirection(aviMuxer, PinDirection.Output, 0);
      if (pinOut == null)
      {
        Log.Error("MPEG2DIVX:FAILED:cannot get output pin of avimux:0x{0:X}", hr);
        Cleanup();
        return false;
      }

      pinIn = DsFindPin.ByDirection(fileWriterbase, PinDirection.Input, 0);
      if (pinIn == null)
      {
        Log.Error("MPEG2DIVX:FAILED:cannot get input pin of Filewriter :0x{0:X}", hr);
        Cleanup();
        return false;
      }
      hr = graphBuilder.Connect(pinOut, pinIn);
      if (hr != 0)
      {
        Log.Error("MPEG2DIVX:FAILED:connect muxer->filewriter :0x{0:X}", hr);
        Cleanup();
        return false;
      }
      return true;
    }

    public void Stop()
    {
      if (mediaControl != null)
      {
        mediaControl.Stop();
      }
      Cleanup();
    }
  }
}
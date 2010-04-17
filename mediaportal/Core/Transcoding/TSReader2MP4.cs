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
using MediaPortal.ExtensionMethods;
using Microsoft.Win32;
using DShowNET.Helper;
using DirectShowLib;
using MediaPortal.GUI.Library;
using MediaPortal.Core.Transcoding;
using System.Runtime.InteropServices;
using MediaPortal.Configuration;

namespace MediaPortal.Core.Transcoding
{
  public class Transcode2MP4 : ITranscode
  {
    [ComImport, Guid("b9559486-E1BB-45D3-A2A2-9A7AFE49B23F")]
    protected class TsReader {}

    protected DsROTEntry _rotEntry = null;
    protected IGraphBuilder graphBuilder = null;
    protected IMediaControl mediaControl = null;
    protected IMediaSeeking mediaSeeking = null;
    protected IMediaPosition mediaPos = null;
    protected IMediaEventEx mediaEvt = null;
    protected IMediaSample mediaSample = null;
    protected IBaseFilter tsreaderSource = null; //TSReader source
    protected IBaseFilter VideoCodec = null; //Video decoder, either MPEG-2 or H.264
    protected IBaseFilter AudioCodec = null; //Audio decoder, either Mpeg-2/AC3 or AAC
    protected IBaseFilter h264Encoder = null; //TBD currently Lead H264 Encoder (4.0)
    protected IBaseFilter aacEncoder = null; //TBD currently Lead AAC Encoder
    protected IBaseFilter mp4Muxer = null; //TBD currently Lead ISO multiplexer
    protected IFileSinkFilter2 fileWriterFilter = null; // DShow Filter: file writer
    protected int bitrate;
    protected double fps;
    protected Size screenSize;
    protected long m_dDuration;
    protected const int WS_CHILD = 0x40000000; // attributes for video window
    protected const int WS_CLIPCHILDREN = 0x02000000;
    protected const int WS_CLIPSIBLINGS = 0x04000000;
    private Guid AVC1 = new Guid("31435641-0000-0010-8000-00AA00389B71");

    public Transcode2MP4() {}

    #region ITranscode Members

    public bool Supports(MediaPortal.Core.Transcoding.VideoFormat format)
    {
      if (format == VideoFormat.MP4) return true;
      return false;
    }

    public void CreateProfile(Size videoSize, int bitRate, double FPS)
    {
      bitrate = bitRate;
      screenSize = videoSize;
      fps = FPS;
    }

    public bool Transcode(TranscodeInfo info, MediaPortal.Core.Transcoding.VideoFormat format,
                          MediaPortal.Core.Transcoding.Quality quality, Standard standard)
    {
      if (!Supports(format)) return false;
      string ext = System.IO.Path.GetExtension(info.file);
      if (ext.ToLower() != ".ts" && ext.ToLower() != ".mpg")
      {
        Log.Info("TSReader2MP4: wrong file format");
        return false;
      }
      try
      {
        graphBuilder = (IGraphBuilder)new FilterGraph();
        _rotEntry = new DsROTEntry((IFilterGraph)graphBuilder);
        Log.Info("TSReader2MP4: add filesource");
        TsReader reader = new TsReader();
        tsreaderSource = (IBaseFilter)reader;
        IBaseFilter filter = (IBaseFilter)tsreaderSource;
        graphBuilder.AddFilter(filter, "TSReader Source");
        IFileSourceFilter fileSource = (IFileSourceFilter)tsreaderSource;
        Log.Info("TSReader2MP4: load file:{0}", info.file);
        int hr = fileSource.Load(info.file, null);
        //add audio/video codecs
        string strVideoCodec = "";
        string strH264VideoCodec = "";
        string strAudioCodec = "";
        string strAACAudioCodec = "";
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.MPSettings())
        {
          strVideoCodec = xmlreader.GetValueAsString("mytv", "videocodec", "");
          strAudioCodec = xmlreader.GetValueAsString("mytv", "audiocodec", "");
          strAACAudioCodec = xmlreader.GetValueAsString("mytv", "aacaudiocodec", "");
          strH264VideoCodec = xmlreader.GetValueAsString("mytv", "h264videocodec", "");
        }
        //Find the type of decoder required for the output video & audio pins on TSReader.
        Log.Info("TSReader2MP4: find tsreader compatible audio/video decoders");
        IPin pinOut0, pinOut1;
        IPin pinIn0, pinIn1;
        pinOut0 = DsFindPin.ByDirection((IBaseFilter)tsreaderSource, PinDirection.Output, 0); //audio
        pinOut1 = DsFindPin.ByDirection((IBaseFilter)tsreaderSource, PinDirection.Output, 1); //video
        if (pinOut0 == null || pinOut1 == null)
        {
          Log.Error("TSReader2MP4: FAILED: unable to get output pins of tsreader");
          Cleanup();
          return false;
        }
        bool usingAAC = false;
        IEnumMediaTypes enumMediaTypes;
        hr = pinOut0.EnumMediaTypes(out enumMediaTypes);
        while (true)
        {
          AMMediaType[] mediaTypes = new AMMediaType[1];
          int typesFetched;
          hr = enumMediaTypes.Next(1, mediaTypes, out typesFetched);
          if (hr != 0 || typesFetched == 0) break;
          if (mediaTypes[0].majorType == MediaType.Audio && mediaTypes[0].subType == MediaSubType.LATMAAC)
          {
            Log.Info("TSReader2MP4: found LATM AAC audio out pin on tsreader");
            usingAAC = true;
          }
        }
        bool usingH264 = false;
        hr = pinOut1.EnumMediaTypes(out enumMediaTypes);
        while (true)
        {
          AMMediaType[] mediaTypes = new AMMediaType[1];
          int typesFetched;
          hr = enumMediaTypes.Next(1, mediaTypes, out typesFetched);
          if (hr != 0 || typesFetched == 0) break;
          if (mediaTypes[0].majorType == MediaType.Video && mediaTypes[0].subType == AVC1)
          {
            Log.Info("TSReader2MP4: found H.264 video out pin on tsreader");
            usingH264 = true;
          }
        }
        //Add the type of decoder required for the output video & audio pins on TSReader.
        Log.Info("TSReader2MP4: add audio/video decoders to graph");
        if (usingH264 == false)
        {
          Log.Info("TSReader2MP4: add mpeg2 video decoder:{0}", strVideoCodec);
          VideoCodec = DirectShowUtil.AddFilterToGraph(graphBuilder, strVideoCodec);
          if (VideoCodec == null)
          {
            Log.Error("TSReader2MP4: unable to add mpeg2 video decoder");
            Cleanup();
            return false;
          }
        }
        else
        {
          Log.Info("TSReader2MP4: add h264 video codec:{0}", strH264VideoCodec);
          VideoCodec = DirectShowUtil.AddFilterToGraph(graphBuilder, strH264VideoCodec);
          if (VideoCodec == null)
          {
            Log.Error("TSReader2MP4: FAILED:unable to add h264 video codec");
            Cleanup();
            return false;
          }
        }
        if (usingAAC == false)
        {
          Log.Info("TSReader2MP4: add mpeg2 audio codec:{0}", strAudioCodec);
          AudioCodec = DirectShowUtil.AddFilterToGraph(graphBuilder, strAudioCodec);
          if (AudioCodec == null)
          {
            Log.Error("TSReader2MP4: FAILED:unable to add mpeg2 audio codec");
            Cleanup();
            return false;
          }
        }
        else
        {
          Log.Info("TSReader2MP4: add aac audio codec:{0}", strAACAudioCodec);
          AudioCodec = DirectShowUtil.AddFilterToGraph(graphBuilder, strAACAudioCodec);
          if (AudioCodec == null)
          {
            Log.Error("TSReader2MP4: FAILED:unable to add aac audio codec");
            Cleanup();
            return false;
          }
        }
        Log.Info("TSReader2MP4: connect tsreader->audio/video decoders");
        //connect output #0 (audio) of tsreader->audio decoder input pin 0
        //connect output #1 (video) of tsreader->video decoder input pin 0
        pinIn0 = DsFindPin.ByDirection(AudioCodec, PinDirection.Input, 0); //audio
        pinIn1 = DsFindPin.ByDirection(VideoCodec, PinDirection.Input, 0); //video
        if (pinIn0 == null || pinIn1 == null)
        {
          Log.Error("TSReader2MP4: FAILED: unable to get pins of video/audio codecs");
          Cleanup();
          return false;
        }
        hr = graphBuilder.Connect(pinOut0, pinIn0);
        if (hr != 0)
        {
          Log.Error("TSReader2MP4: FAILED: unable to connect audio pins :0x{0:X}", hr);
          Cleanup();
          return false;
        }
        hr = graphBuilder.Connect(pinOut1, pinIn1);
        if (hr != 0)
        {
          Log.Error("TSReader2MP4: FAILED: unable to connect video pins :0x{0:X}", hr);
          Cleanup();
          return false;
        }
        //add encoders, muxer & filewriter
        if (!AddCodecs(graphBuilder, info)) return false;
        //setup graph controls
        mediaControl = graphBuilder as IMediaControl;
        mediaSeeking = tsreaderSource as IMediaSeeking;
        mediaEvt = graphBuilder as IMediaEventEx;
        mediaPos = graphBuilder as IMediaPosition;
        //get file duration
        Log.Info("TSReader2MP4: Get duration of recording");
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
        Log.Info("TSReader2MP4: recording duration: {0}", MediaPortal.Util.Utils.SecondsToHMSString((int)duration));
        //run the graph to initialize the filters to be sure
        hr = mediaControl.Run();
        if (hr != 0)
        {
          Log.Error("TSReader2MP4: FAILED: unable to start graph :0x{0:X}", hr);
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
          System.Threading.Thread.Sleep(100);
          if (dpos >= 2.0d) break;
          maxCount--;
          if (maxCount <= 0) break;
        }
        mediaControl.Stop();
        FilterState state;
        mediaControl.GetState(500, out state);
        GC.Collect();
        GC.Collect();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        graphBuilder.RemoveFilter(mp4Muxer);
        graphBuilder.RemoveFilter(h264Encoder);
        graphBuilder.RemoveFilter(aacEncoder);
        graphBuilder.RemoveFilter((IBaseFilter)fileWriterFilter);
        if (!AddCodecs(graphBuilder, info)) return false;
        //Set Encoder quality & Muxer settings
        if (!EncoderSet(graphBuilder, info)) return false;
        //start transcoding - run the graph
        Log.Info("TSReader2MP4: start transcoding");
        //setup flow control
        //need to leverage CBAsePin, CPullPin & IAsyncReader methods.
        IAsyncReader synchVideo = null;
        mediaSample = VideoCodec as IMediaSample;
        hr = synchVideo.SyncReadAligned(mediaSample);
        //So we only parse decoder output whent the encoders are ready.
        hr = mediaControl.Run();
        if (hr != 0)
        {
          Log.Error("TSReader2MP4: FAILED:unable to start graph :0x{0:X}", hr);
          Cleanup();
          return false;
        }
      }
      catch (Exception ex)
      {
        Log.Error("TSReader2MP4: Unable create graph: {0}", ex.Message);
        Cleanup();
        return false;
      }
      return true;
    }

    public bool IsFinished()
    {
      if (mediaControl == null) return true;
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
      if (mediaSeeking == null) return 100;
      long lCurrent;
      mediaSeeking.GetCurrentPosition(out lCurrent);
      float percent = ((float)lCurrent) / ((float)m_dDuration);
      percent *= 50.0f;
      if (percent > 100) percent = 100;
      return (int)percent;
    }

    public bool IsTranscoding()
    {
      if (IsFinished()) return false;
      return true;
    }

    private void Cleanup()
    {
      Log.Info("TSReader2MP4: cleanup");
      if (_rotEntry != null)
      {
        _rotEntry.SafeDispose();
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
      if (h264Encoder != null)
        DirectShowUtil.ReleaseComObject(h264Encoder);
      h264Encoder = null;
      if (aacEncoder != null)
        DirectShowUtil.ReleaseComObject(aacEncoder);
      aacEncoder = null;
      if (mp4Muxer != null)
        DirectShowUtil.ReleaseComObject(mp4Muxer);
      mp4Muxer = null;
      if (AudioCodec != null)
        DirectShowUtil.ReleaseComObject(AudioCodec);
      AudioCodec = null;
      if (VideoCodec != null)
        DirectShowUtil.ReleaseComObject(VideoCodec);
      VideoCodec = null;
      if (tsreaderSource != null)
        DirectShowUtil.ReleaseComObject(tsreaderSource);
      tsreaderSource = null;
      DirectShowUtil.RemoveFilters(graphBuilder);
      if (graphBuilder != null)
        DirectShowUtil.ReleaseComObject(graphBuilder);
      graphBuilder = null;
      GC.Collect();
      GC.Collect();
      GC.Collect();
      GC.WaitForPendingFinalizers();
    }

    #endregion

    private bool AddCodecs(IGraphBuilder graphBuilder, TranscodeInfo info)
    {
      //TODO: Add de-interlacing probably by filter
      int hr;
      Log.Info("TSReader2MP4: add h264 video encoder to graph");
      //Lead H264 Encoder (4.0)
      string monikerH264 = @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{E2B7DF52-38C5-11D5-91F6-00104BDB8FF9}";
      h264Encoder = Marshal.BindToMoniker(monikerH264) as IBaseFilter;
      if (h264Encoder == null)
      {
        Log.Error("TSReader2MP4: FAILED: Unable to create h264 video encoder");
        Cleanup();
        return false;
      }
      hr = graphBuilder.AddFilter(h264Encoder, "h264 video encoder");
      if (hr != 0)
      {
        Log.Error("TSReader2MP4: FAILED: h264 video encoder to filtergraph :0x{0:X}", hr);
        Cleanup();
        return false;
      }
      Log.Info("TSReader2MP4: add aac audio encoder to graph");
      //Monograph AAC Encoder
      //string monikerAAC = @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{88F36DB6-D898-40B5-B409-466A0EECC26A}";
      //Lead AAC Encoder
      string monikerAAC = @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{E2B7DD70-38C5-11D5-91F6-00104BDB8FF9}";
      aacEncoder = Marshal.BindToMoniker(monikerAAC) as IBaseFilter;
      if (aacEncoder == null)
      {
        Log.Error("TSReader2MP4: FAILED: Unable to create aac audio encoder");
        Cleanup();
        return false;
      }
      hr = graphBuilder.AddFilter(aacEncoder, "aac audio encoder");
      if (hr != 0)
      {
        Log.Error("TSReader2MP4: FAILED: Add aac audio encoder to filtergraph :0x{0:X}", hr);
        Cleanup();
        return false;
      }
      // dump filter ????
      //add filewriter 
      Log.Info("TSReader2MP4: add FileWriter to graph");
      string monikerFileWrite =
        @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{8596E5F0-0DA5-11D0-BD21-00A0C911CE86}";
      IBaseFilter fileWriterbase = Marshal.BindToMoniker(monikerFileWrite) as IBaseFilter;
      if (fileWriterbase == null)
      {
        Log.Error("TSReader2MP4: FAILED: Unable to create FileWriter");
        Cleanup();
        return false;
      }
      fileWriterFilter = fileWriterbase as IFileSinkFilter2;
      if (fileWriterFilter == null)
      {
        Log.Error("TSReader2MP4: FAILED: Add unable to get IFileSinkFilter for filewriter");
        Cleanup();
        return false;
      }

      hr = graphBuilder.AddFilter(fileWriterbase, "FileWriter");
      if (hr != 0)
      {
        Log.Error("TSReader2MP4: FAILED: Add FileWriter to filtergraph :0x{0:X}", hr);
        Cleanup();
        return false;
      }
      //set output filename
      string outputFileName = System.IO.Path.ChangeExtension(info.file, ".mp4");
      Log.Info("TSReader2MP4: set output file to :{0}", outputFileName);
      hr = fileWriterFilter.SetFileName(outputFileName, null);
      if (hr != 0)
      {
        Log.Error("TSReader2MP4: FAILED: unable to set filename for filewriter :0x{0:X}", hr);
        Cleanup();
        return false;
      }
      // add mp4 muxer
      Log.Info("TSReader2MP4: add MP4 Muxer to graph");
      //Lead ISO Multiplexer
      string monikermp4Muxer =
        @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{990D1978-E48D-43AF-B12D-24A7456EC89F}";
      mp4Muxer = Marshal.BindToMoniker(monikermp4Muxer) as IBaseFilter;
      if (mp4Muxer == null)
      {
        Log.Error("TSReader2MP4: FAILED: Unable to create MP4Mux");
        Cleanup();
        return false;
      }
      hr = graphBuilder.AddFilter(mp4Muxer, "MP4Mux");
      if (hr != 0)
      {
        Log.Error("TSReader2MP4: FAILED: Add MP4Mux to filtergraph :0x{0:X}", hr);
        Cleanup();
        return false;
      }
      //connect output of audio codec to aac encoder
      IPin pinOut, pinIn;
      Log.Info("TSReader2MP4: connect audio codec->aac encoder");
      pinIn = DsFindPin.ByDirection(aacEncoder, PinDirection.Input, 0);
      if (pinIn == null)
      {
        Log.Error("TSReader2MP4: FAILED: cannot get input pin of aac encoder:0x{0:X}", hr);
        Cleanup();
        return false;
      }
      pinOut = DsFindPin.ByDirection(AudioCodec, PinDirection.Output, 0);
      if (pinOut == null)
      {
        Log.Error("TSReader2MP4: FAILED: cannot get output pin of audio codec :0x{0:X}", hr);
        Cleanup();
        return false;
      }
      hr = graphBuilder.Connect(pinOut, pinIn);
      if (hr != 0)
      {
        Log.Error("TSReader2MP4: FAILED: unable to connect audio codec->aac encoder: 0x{0:X}", hr);
        Cleanup();
        return false;
      }
      //connect output of video codec to h264 encoder
      Log.Info("TSReader2MP4: connect video codec->h264 encoder");
      pinIn = DsFindPin.ByDirection(h264Encoder, PinDirection.Input, 0);
      if (pinIn == null)
      {
        Log.Error("TSReader2MP4: FAILED: cannot get input pin of h264 encoder:0x{0:X}", hr);
        Cleanup();
        return false;
      }
      pinOut = DsFindPin.ByDirection(VideoCodec, PinDirection.Output, 0);
      if (pinOut == null)
      {
        Log.Error("TSReader2MP4: FAILED: cannot get output pin of video codec :0x{0:X}", hr);
        Cleanup();
        return false;
      }
      hr = graphBuilder.Connect(pinOut, pinIn);
      if (hr != 0)
      {
        Log.Error("TSReader2MP4: FAILED: unable to connect video codec->h264 encoder :0x{0:X}", hr);
        Cleanup();
        return false;
      }
      //connect output of aac encoder to pin#0 of mp4mux
      Log.Info("TSReader2MP4: connect aac encoder->mp4mux");
      pinOut = DsFindPin.ByDirection(aacEncoder, PinDirection.Output, 0);
      if (pinOut == null)
      {
        Log.Error("TSReader2MP4: FAILED: cannot get input pin of aac encoder:0x{0:X}", hr);
        Cleanup();
        return false;
      }
      pinIn = DsFindPin.ByDirection(mp4Muxer, PinDirection.Input, 0);
      if (pinIn == null)
      {
        Log.Error("TSReader2MP4: FAILED: cannot get input pin#1 of mp4 muxer :0x{0:X}", hr);
        Cleanup();
        return false;
      }
      hr = graphBuilder.Connect(pinOut, pinIn);
      if (hr != 0)
      {
        Log.Error("TSReader2MP4: FAILED: unable to connect aac encoder->mp4mux: 0x{0:X}", hr);
        Cleanup();
        return false;
      }
      //connect output of h264 encoder to pin#1 of mp4mux
      Log.Info("TSReader2MP4: connect h264 encoder->mp4mux");
      pinOut = DsFindPin.ByDirection(h264Encoder, PinDirection.Output, 0);
      if (pinOut == null)
      {
        Log.Error("TSReader2MP4: FAILED: cannot get input pin of h264 encoder :0x{0:X}", hr);
        Cleanup();
        return false;
      }
      pinIn = DsFindPin.ByDirection(mp4Muxer, PinDirection.Input, 1);
      if (pinIn == null)
      {
        Log.Error("TSReader2MP4: FAILED: cannot get input#0 pin of mp4mux :0x{0:X}", hr);
        Cleanup();
        return false;
      }
      hr = graphBuilder.Connect(pinOut, pinIn);
      if (hr != 0)
      {
        Log.Error("TSReader2MP4: FAILED: unable to connect h264 encoder->mp4mux :0x{0:X}", hr);
        Cleanup();
        return false;
      }
      // dump filter??
      //connect mp4 muxer out->filewriter
      Log.Info("TSReader2MP4: connect mp4mux->filewriter");
      pinOut = DsFindPin.ByDirection(mp4Muxer, PinDirection.Output, 0);
      if (pinOut == null)
      {
        Log.Error("TSReader2MP4: FAILED: cannot get output pin of avimux:0x{0:X}", hr);
        Cleanup();
        return false;
      }
      pinIn = DsFindPin.ByDirection(fileWriterbase, PinDirection.Input, 0);
      if (pinIn == null)
      {
        Log.Error("TSReader2MP4: FAILED: cannot get input pin of filewriter :0x{0:X}", hr);
        Cleanup();
        return false;
      }
      hr = graphBuilder.Connect(pinOut, pinIn);
      if (hr != 0)
      {
        Log.Error("TSReader2MP4: FAILED: connect mp4 muxer->filewriter :0x{0:X}", hr);
        Cleanup();
        return false;
      }
      return true;
    }

    private bool EncoderSet(IGraphBuilder graphBuilder, TranscodeInfo info)
    {
      //Add methods here to set encoder parameters
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
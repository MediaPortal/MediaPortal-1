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
using DShowNET;
using DShowNET.Helper;
using DirectShowLib;
using DirectShowLib.SBE;
using MediaPortal.GUI.Library;
using System.Runtime.InteropServices;

namespace MediaPortal.Core.Transcoding
{
  /// <summary>
  /// Summary description for Dvrms2Mpeg.
  /// </summary>
  public class Dvrms2Mpeg : ITranscode
  {
    protected DsROTEntry _rotEntry = null;
    protected IGraphBuilder graphBuilder = null;
    protected IStreamBufferSource bufferSource = null;
    protected IFileSinkFilter fileWriterFilter = null; // DShow Filter: file writer
    protected IMediaControl mediaControl = null;
    protected IMediaSeeking mediaSeeking = null;
    protected IBaseFilter powerDvdMuxer = null;
    protected IMediaEventEx mediaEvt = null;

    public Dvrms2Mpeg() {}

    #region ITranscode Members

    public bool Supports(MediaPortal.Core.Transcoding.VideoFormat format)
    {
      if (format == VideoFormat.Mpeg2) return true;
      return false;
    }

    public bool Transcode(TranscodeInfo info, MediaPortal.Core.Transcoding.VideoFormat format,
                          MediaPortal.Core.Transcoding.Quality quality, Standard standard)
    {
      if (!Supports(format)) return false;
      string ext = System.IO.Path.GetExtension(info.file);
      if (ext.ToLower() != ".dvr-ms" && ext.ToLower() != ".sbe") return false;

      //Type comtype = null;
      //object comobj = null;
      try
      {
        Log.Info("DVR2MPG: create graph");
        graphBuilder = (IGraphBuilder)new FilterGraph();

        _rotEntry = new DsROTEntry((IFilterGraph)graphBuilder);

        Log.Info("DVR2MPG: add streambuffersource");
        bufferSource = (IStreamBufferSource)new StreamBufferSource();


        IBaseFilter filter = (IBaseFilter)bufferSource;
        graphBuilder.AddFilter(filter, "SBE SOURCE");

        Log.Info("DVR2MPG: load file:{0}", info.file);
        IFileSourceFilter fileSource = (IFileSourceFilter)bufferSource;
        int hr = fileSource.Load(info.file, null);


        Log.Info("DVR2MPG: Add Cyberlink MPEG2 multiplexer to graph");
        string monikerPowerDvdMuxer =
          @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{7F2BBEAF-E11C-4D39-90E8-938FB5A86045}";
        powerDvdMuxer = Marshal.BindToMoniker(monikerPowerDvdMuxer) as IBaseFilter;
        if (powerDvdMuxer == null)
        {
          Log.Warn("DVR2MPG: FAILED:Unable to create Cyberlink MPEG Muxer (PowerDVD)");
          Cleanup();
          return false;
        }

        hr = graphBuilder.AddFilter(powerDvdMuxer, "PDR MPEG Muxer");
        if (hr != 0)
        {
          Log.Warn("DVR2MPG: FAILED:Add Cyberlink MPEG Muxer to filtergraph :0x{0:X}", hr);
          Cleanup();
          return false;
        }

        //add filewriter 
        Log.Info("DVR2MPG: Add FileWriter to graph");
        string monikerFileWrite =
          @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{3E8868CB-5FE8-402C-AA90-CB1AC6AE3240}";
        IBaseFilter fileWriterbase = Marshal.BindToMoniker(monikerFileWrite) as IBaseFilter;
        if (fileWriterbase == null)
        {
          Log.Warn("DVR2MPG: FAILED:Unable to create FileWriter");
          Cleanup();
          return false;
        }


        fileWriterFilter = fileWriterbase as IFileSinkFilter;
        if (fileWriterFilter == null)
        {
          Log.Warn("DVR2MPG: FAILED:Add unable to get IFileSinkFilter for filewriter");
          Cleanup();
          return false;
        }

        hr = graphBuilder.AddFilter(fileWriterbase, "FileWriter");
        if (hr != 0)
        {
          Log.Warn("DVR2MPG: FAILED:Add FileWriter to filtergraph :0x{0:X}", hr);
          Cleanup();
          return false;
        }


        //connect output #0 of streambuffer source->powerdvd audio in
        //connect output #1 of streambuffer source->powerdvd video in
        Log.Info("DVR2MPG: connect streambuffer->multiplexer");
        IPin pinOut0, pinOut1;
        IPin pinIn0, pinIn1;
        pinOut0 = DsFindPin.ByDirection((IBaseFilter)bufferSource, PinDirection.Output, 0);
        pinOut1 = DsFindPin.ByDirection((IBaseFilter)bufferSource, PinDirection.Output, 1);

        pinIn0 = DsFindPin.ByDirection(powerDvdMuxer, PinDirection.Input, 0);
        pinIn1 = DsFindPin.ByDirection(powerDvdMuxer, PinDirection.Input, 1);
        if (pinOut0 == null || pinOut1 == null || pinIn0 == null || pinIn1 == null)
        {
          Log.Warn("DVR2MPG: FAILED:unable to get pins of muxer&source");
          Cleanup();
          return false;
        }

        bool usingAc3 = false;
        AMMediaType amAudio = new AMMediaType();
        amAudio.majorType = MediaType.Audio;
        amAudio.subType = MediaSubType.Mpeg2Audio;
        hr = pinOut0.Connect(pinIn1, amAudio);
        if (hr != 0)
        {
          amAudio.subType = MediaSubType.DolbyAC3;
          hr = pinOut0.Connect(pinIn1, amAudio);
          usingAc3 = true;
        }
        if (hr != 0)
        {
          Log.Warn("DVR2MPG: FAILED: unable to connect audio pins: 0x{0:X}", hr);
          Cleanup();
          return false;
        }

        if (usingAc3)
          Log.Info("DVR2MPG: using AC3 audio");
        else
          Log.Info("DVR2MPG: using MPEG audio");

        AMMediaType amVideo = new AMMediaType();
        amVideo.majorType = MediaType.Video;
        amVideo.subType = MediaSubType.Mpeg2Video;
        hr = pinOut1.Connect(pinIn0, amVideo);
        if (hr != 0)
        {
          Log.Warn("DVR2MPG: FAILED: unable to connect video pins: 0x{0:X}", hr);
          Cleanup();
          return false;
        }


        //connect output of powerdvd muxer->input of filewriter
        Log.Info("DVR2MPG: connect multiplexer->filewriter");
        IPin pinOut, pinIn;
        pinOut = DsFindPin.ByDirection(powerDvdMuxer, PinDirection.Output, 0);
        if (pinOut == null)
        {
          Log.Warn("DVR2MPG: FAILED:cannot get output pin of Cyberlink MPEG muxer :0x{0:X}", hr);
          Cleanup();
          return false;
        }
        pinIn = DsFindPin.ByDirection(fileWriterbase, PinDirection.Input, 0);
        if (pinIn == null)
        {
          Log.Warn("DVR2MPG: FAILED:cannot get input pin of Filewriter :0x{0:X}", hr);
          Cleanup();
          return false;
        }
        AMMediaType mt = new AMMediaType();
        hr = pinOut.Connect(pinIn, mt);
        if (hr != 0)
        {
          Log.Warn("DVR2MPG: FAILED:connect muxer->filewriter :0x{0:X}", hr);
          Cleanup();
          return false;
        }

        //set output filename
        string outputFileName = System.IO.Path.ChangeExtension(info.file, ".mpg");
        Log.Info("DVR2MPG: set output file to :{0}", outputFileName);
        mt.majorType = MediaType.Stream;
        mt.subType = MediaSubTypeEx.MPEG2;

        hr = fileWriterFilter.SetFileName(outputFileName, mt);
        if (hr != 0)
        {
          Log.Warn("DVR2MPG: FAILED:unable to set filename for filewriter :0x{0:X}", hr);
          Cleanup();
          return false;
        }
        mediaControl = graphBuilder as IMediaControl;
        mediaSeeking = graphBuilder as IMediaSeeking;
        mediaEvt = graphBuilder as IMediaEventEx;
        Log.Info("DVR2MPG: start transcoding");
        hr = mediaControl.Run();
        if (hr != 0)
        {
          Log.Warn("DVR2MPG: FAILED:unable to start graph :0x{0:X}", hr);
          Cleanup();
          return false;
        }
      }
      catch (Exception ex)
      {
        Log.Error("DVR2MPG: Unable create graph: {0}", ex.Message);
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
      long lDuration, lCurrent;
      mediaSeeking.GetCurrentPosition(out lCurrent);
      mediaSeeking.GetDuration(out lDuration);
      float percent = ((float)lCurrent) / ((float)lDuration);
      percent *= 100.0f;
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
      Log.Info("DVR2MPG: cleanup");
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
      mediaSeeking = null;
      mediaEvt = null;
      mediaControl = null;

      if (powerDvdMuxer != null)
        DirectShowUtil.ReleaseComObject(powerDvdMuxer);
      powerDvdMuxer = null;

      if (fileWriterFilter != null)
        DirectShowUtil.ReleaseComObject(fileWriterFilter);
      fileWriterFilter = null;

      if (bufferSource != null)
        DirectShowUtil.ReleaseComObject(bufferSource);
      bufferSource = null;

      DirectShowUtil.RemoveFilters(graphBuilder);

      if (graphBuilder != null)
        DirectShowUtil.ReleaseComObject(graphBuilder);
      graphBuilder = null;
    }


    public void Stop()
    {
      if (mediaControl != null)
      {
        mediaControl.Stop();
      }
      Cleanup();
    }

    #endregion
  }
}
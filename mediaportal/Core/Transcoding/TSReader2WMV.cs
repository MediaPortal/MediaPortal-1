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
using System.IO;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using DirectShowLib;
using DShowNET.Helper;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Configuration;
using MediaPortal.Player;

namespace MediaPortal.Core.Transcoding
{
  /// <summary>
  /// This class encodes an video file to .wmv format
  /// </summary>
  public class TSReader2WMV : ITranscode
  {
    [ComImport, Guid("b9559486-E1BB-45D3-A2A2-9A7AFE49B23F")]
    protected class TsReader { }

    #region imports
    [DllImport("dvblib.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int SetWmvProfile(DirectShowLib.IBaseFilter filter, int bitrate, int fps, int screenX, int screenY);
    #endregion

    #region constants
    private const WMVersion DefaultWMversion = WMVersion.V8_0;
    private const int MAXLENPROFNAME = 100;
    private const int MAXLENPROFDESC = 512;
    #endregion

    #region Guids
    Guid AVC1 = new Guid("31435641-0000-0010-8000-00AA00389B71");
    Guid IID_IWMWriterAdvanced2 = new Guid(0x962dc1ec, 0xc046, 0x4db8, 0x9c, 0xc7, 0x26, 0xce, 0xae, 0x50, 0x08, 0x17);
    #endregion

    protected DsROTEntry _rotEntry = null;
    protected IGraphBuilder graphBuilder = null;
    protected IMediaControl mediaControl = null;
    protected IMediaPosition mediaPos = null;
    protected IMediaSeeking mediaSeeking = null;
    protected IMediaEventEx mediaEvt = null;
    protected IBaseFilter tsreaderSource = null; //TSReader source
    protected IBaseFilter VideoCodec = null; //Video decoder, either MPEG-2 or H.264
    protected IBaseFilter AudioCodec = null; //Audio decoder, either Mpeg-2/AC3 or AAC
    protected IBaseFilter fileWriterbase = null;
    protected long m_dDuration;
    protected int bitrate;
    protected double fps;
    protected Size screenSize;
    protected const int WS_CHILD = 0x40000000;	// attributes for video window
    protected const int WS_CLIPCHILDREN = 0x02000000;
    protected const int WS_CLIPSIBLINGS = 0x04000000;



    public TSReader2WMV()
    {
    }

    public void CreateProfile(Size videoSize, int bitRate, double FPS)
    {
      bitrate = bitRate;
      screenSize = videoSize;
      fps = FPS;
    }

    public bool Supports(VideoFormat format)
    {
      if (format == VideoFormat.Wmv) return true;
      return false;
    }

    public bool Transcode(TranscodeInfo info, VideoFormat format, Quality quality, Standard standard)
    {
      try
      {
        if (!Supports(format)) return false;
        string ext = System.IO.Path.GetExtension(info.file);
        if (ext.ToLower() != ".ts" && ext.ToLower() != ".mpg")
        {
          Log.Info("TSReader2WMV: wrong file format");
          return false;
        }
        Log.Info("TSReader2WMV: create graph");
        graphBuilder = (IGraphBuilder)new FilterGraph();
        _rotEntry = new DsROTEntry((IFilterGraph)graphBuilder);
        Log.Info("TSReader2WMV: add filesource");
        TsReader reader = new TsReader();
        tsreaderSource = (IBaseFilter)reader;
        //ITSReader ireader = (ITSReader)reader;
        //ireader.SetTsReaderCallback(this);
        //ireader.SetRequestAudioChangeCallback(this);
        IBaseFilter filter = (IBaseFilter)tsreaderSource;
        graphBuilder.AddFilter(filter, "TSReader Source");
        IFileSourceFilter fileSource = (IFileSourceFilter)tsreaderSource;
        Log.Info("TSReader2WMV: load file:{0}", info.file);
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
        Log.Info("TSReader2WMV: find tsreader compatible audio/video decoders");
        IPin pinOut0, pinOut1;
        IPin pinIn0, pinIn1;
        pinOut0 = DsFindPin.ByDirection((IBaseFilter)tsreaderSource, PinDirection.Output, 0);//audio
        pinOut1 = DsFindPin.ByDirection((IBaseFilter)tsreaderSource, PinDirection.Output, 1);//video
        if (pinOut0 == null || pinOut1 == null)
        {
          Log.Error("TSReader2WMV: FAILED: unable to get output pins of tsreader");
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
            Log.Info("TSReader2WMV: found LATM AAC audio out pin on tsreader");
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
            Log.Info("TSReader2WMV: found H.264 video out pin on tsreader");
            usingH264 = true;
          }
        }
        //Add the type of decoder required for the output video & audio pins on TSReader.
        Log.Info("TSReader2WMV: add audio/video decoders to graph");
        if (usingH264 == false)
        {
          Log.Info("TSReader2WMV: add mpeg2 video decoder:{0}", strVideoCodec);
          VideoCodec = DirectShowUtil.AddFilterToGraph(graphBuilder, strVideoCodec);
          if (VideoCodec == null)
          {
            Log.Error("TSReader2WMV: unable to add mpeg2 video decoder");
            Cleanup();
            return false;
          }
        }
        else
        {
          Log.Info("TSReader2WMV: add h264 video codec:{0}", strH264VideoCodec);
          VideoCodec = DirectShowUtil.AddFilterToGraph(graphBuilder, strH264VideoCodec);
          if (VideoCodec == null)
          {
            Log.Error("TSReader2WMV: FAILED:unable to add h264 video codec");
            Cleanup();
            return false;
          }
        }
        if (usingAAC == false)
        {
          Log.Info("TSReader2WMV: add mpeg2 audio codec:{0}", strAudioCodec);
          AudioCodec = DirectShowUtil.AddFilterToGraph(graphBuilder, strAudioCodec);
          if (AudioCodec == null)
          {
            Log.Error("TSReader2WMV: FAILED:unable to add mpeg2 audio codec");
            Cleanup();
            return false;
          }
        }
        else
        {
          Log.Info("TSReader2WMV: add aac audio codec:{0}", strAACAudioCodec);
          AudioCodec = DirectShowUtil.AddFilterToGraph(graphBuilder, strAACAudioCodec);
          if (AudioCodec == null)
          {
            Log.Error("TSReader2WMV: FAILED:unable to add aac audio codec");
            Cleanup();
            return false;
          }
        }
        Log.Info("TSReader2WMV: connect tsreader->audio/video decoders");
        //connect output #0 (audio) of tsreader->audio decoder input pin 0
        //connect output #1 (video) of tsreader->video decoder input pin 0
        pinIn0 = DsFindPin.ByDirection(AudioCodec, PinDirection.Input, 0);//audio
        pinIn1 = DsFindPin.ByDirection(VideoCodec, PinDirection.Input, 0);//video
        if (pinIn0 == null || pinIn1 == null)
        {
          Log.Error("TSReader2WMV: FAILED: unable to get pins of video/audio codecs");
          Cleanup();
          return false;
        }
        hr = graphBuilder.Connect(pinOut0, pinIn0);
        if (hr != 0)
        {
          Log.Error("TSReader2WMV: FAILED: unable to connect audio pins :0x{0:X}", hr);
          Cleanup();
          return false;
        }
        hr = graphBuilder.Connect(pinOut1, pinIn1);
        if (hr != 0)
        {
          Log.Error("TSReader2WMV: FAILED: unable to connect video pins :0x{0:X}", hr);
          Cleanup();
          return false;
        }
        string outputFilename = System.IO.Path.ChangeExtension(info.file, ".wmv");
        if (!AddWmAsfWriter(outputFilename, quality, standard)) return false;
        Log.Info("TSReader2WMV: start pre-run");
        mediaControl = graphBuilder as IMediaControl;
        mediaSeeking = tsreaderSource as IMediaSeeking;
        mediaEvt = graphBuilder as IMediaEventEx;
        mediaPos = graphBuilder as IMediaPosition;
        //get file duration
        long lTime = 5 * 60 * 60;
        lTime *= 10000000;
        long pStop = 0;
        hr = mediaSeeking.SetPositions(new DsLong(lTime), AMSeekingSeekingFlags.AbsolutePositioning, new DsLong(pStop), AMSeekingSeekingFlags.NoPositioning);
        if (hr == 0)
        {
          long lStreamPos;
          mediaSeeking.GetCurrentPosition(out lStreamPos); // stream position
          m_dDuration = lStreamPos;
          lTime = 0;
          mediaSeeking.SetPositions(new DsLong(lTime), AMSeekingSeekingFlags.AbsolutePositioning, new DsLong(pStop), AMSeekingSeekingFlags.NoPositioning);
        }
        double duration = m_dDuration / 10000000d;
        Log.Info("TSReader2WMV: movie duration:{0}", Util.Utils.SecondsToHMSString((int)duration));
        hr = mediaControl.Run();
        if (hr != 0)
        {
          Log.Error("TSReader2WMV: FAILED: unable to start graph :0x{0:X}", hr);
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
        Log.Info("TSReader2WMV: pre-run done");
        Log.Info("TSReader2WMV: Get duration of movie");
        mediaControl.Stop();
        FilterState state;
        mediaControl.GetState(500, out state);
        GC.Collect(); GC.Collect(); GC.Collect(); GC.WaitForPendingFinalizers();
        Log.Info("TSReader2WMV: reconnect mpeg2 video codec->ASF WM Writer");
        graphBuilder.RemoveFilter(fileWriterbase);
        if (!AddWmAsfWriter(outputFilename, quality, standard)) return false;
        Log.Info("TSReader2WMV: Start transcoding");
        hr = mediaControl.Run();
        if (hr != 0)
        {
          Log.Error("TSReader2WMV:FAILED:unable to start graph :0x{0:X}", hr);
          Cleanup();
          return false;
        }
      }
      catch (Exception e)
      {
        // TODO: Handle exceptions.
        Log.Error("unable to transcode file:{0} message:{1}", info.file, e.Message);
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

    void Cleanup()
    {
      Log.Info("TSReader2WMV: cleanup");
      if (mediaControl != null)
      {
        mediaControl.Stop();
        mediaControl = null;
      }
      mediaSeeking = null;
      mediaEvt = null;
      mediaPos = null;
      mediaControl = null;
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
      if (_rotEntry != null)
      {
        _rotEntry.Dispose();
      }
      _rotEntry = null;
      if (graphBuilder != null)
        DirectShowUtil.ReleaseComObject(graphBuilder); graphBuilder = null;
      GC.Collect();
      GC.Collect();
      GC.Collect();
      GC.WaitForPendingFinalizers();
    }

    public void Stop()
    {
      if (mediaControl != null)
      {
        mediaControl.Stop();
      }
      Cleanup();
    }

    bool AddWmAsfWriter(string fileName, Quality quality, Standard standard)
    {
      //add asf file writer
      IPin pinOut0, pinOut1;
      IPin pinIn0, pinIn1;
      Log.Info("TSReader2WMV: add WM ASF Writer to graph");
      string monikerAsfWriter = @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{7C23220E-55BB-11D3-8B16-00C04FB6BD3D}";
      fileWriterbase = Marshal.BindToMoniker(monikerAsfWriter) as IBaseFilter;
      if (fileWriterbase == null)
      {
        Log.Error("TSReader2WMV:FAILED:Unable to create ASF WM Writer");
        Cleanup();
        return false;
      }
      int hr = graphBuilder.AddFilter(fileWriterbase, "WM ASF Writer");
      if (hr != 0)
      {
        Log.Error("TSReader2WMV:FAILED:Add ASF WM Writer to filtergraph :0x{0:X}", hr);
        Cleanup();
        return false;
      }
      IFileSinkFilter2 fileWriterFilter = fileWriterbase as IFileSinkFilter2;
      if (fileWriterFilter == null)
      {
        Log.Error("DVR2XVID:FAILED:Add unable to get IFileSinkFilter for filewriter");
        Cleanup();
        return false;
      }
      hr = fileWriterFilter.SetFileName(fileName, null);
      hr = fileWriterFilter.SetMode(AMFileSinkFlags.OverWrite);
      Log.Info("TSReader2WMV: connect audio/video codecs outputs -> ASF WM Writer");
      //connect output #0 of videocodec->asf writer pin 1
      //connect output #0 of audiocodec->asf writer pin 0
      pinOut0 = DsFindPin.ByDirection((IBaseFilter)AudioCodec, PinDirection.Output, 0);
      pinOut1 = DsFindPin.ByDirection((IBaseFilter)VideoCodec, PinDirection.Output, 0);
      if (pinOut0 == null || pinOut1 == null)
      {
        Log.Error("TSReader2WMV:FAILED:unable to get outpins of video codec");
        Cleanup();
        return false;
      }
      pinIn0 = DsFindPin.ByDirection(fileWriterbase, PinDirection.Input, 0);
      if (pinIn0 == null)
      {
        Log.Error("TSReader2WMV:FAILED:unable to get pins of asf wm writer");
        Cleanup();
        return false;
      }
      hr = graphBuilder.Connect(pinOut0, pinIn0);
      if (hr != 0)
      {
        Log.Error("TSReader2WMV:FAILED:unable to connect audio pins :0x{0:X}", hr);
        Cleanup();
        return false;
      }
      pinIn1 = DsFindPin.ByDirection(fileWriterbase, PinDirection.Input, 1);
      if (pinIn1 == null)
      {
        Log.Error("TSReader2WMV:FAILED:unable to get pins of asf wm writer");
        Cleanup();
        return false;
      }
      hr = graphBuilder.Connect(pinOut1, pinIn1);
      if (hr != 0)
      {
        Log.Error("TSReader2WMV:FAILED:unable to connect video pins :0x{0:X}", hr);
        Cleanup();
        return false;
      }
      IConfigAsfWriter config = fileWriterbase as IConfigAsfWriter;
      IWMProfileManager profileManager = null;
      IWMProfileManager2 profileManager2 = null;
      IWMProfile profile = null;
      hr = WMLib.WMCreateProfileManager(out profileManager);
      string strprofileType = "";
      switch (quality)
      {
        case Quality.HiDef:
          //hr = WMLib.WMCreateProfileManager(out profileManager);
          if (standard == Standard.Film)
          {
            strprofileType = Config.GetFile(Config.Dir.Base, @"Profiles\MPHiDef-FILM.prx");
          }
          if (standard == Standard.NTSC)
          {
            strprofileType = Config.GetFile(Config.Dir.Base, @"Profiles\MPHiDef-NTSC.prx");
          }
          if (standard == Standard.PAL)
          {
            strprofileType = Config.GetFile(Config.Dir.Base, @"Profiles\MPHiDef-PAL.prx");
          }
          Log.Info("TSReader2WMV: set WMV HiDef quality profile {0}", strprofileType);
          break;
        case Quality.VeryHigh:
          if (standard == Standard.Film)
          {
            strprofileType = Config.GetFile(Config.Dir.Base, @"Profiles\MPVeryHigh-FILM.prx");
          }
          if (standard == Standard.NTSC)
          {
            strprofileType = Config.GetFile(Config.Dir.Base, @"Profiles\MPVeryHigh-NTSC.prx");
          }
          if (standard == Standard.PAL)
          {
            strprofileType = Config.GetFile(Config.Dir.Base, @"Profiles\MPVeryHigh-PAL.prx");
          }
          Log.Info("TSReader2WMV: set WMV Very High quality profile {0}", strprofileType);
          break;
        case Quality.High:
          if (standard == Standard.Film)
          {
            strprofileType = Config.GetFile(Config.Dir.Base, @"Profiles\MPHigh-FILM.prx");
          }
          if (standard == Standard.NTSC)
          {
            strprofileType = Config.GetFile(Config.Dir.Base, @"Profiles\MPHigh-NTSC.prx");
          }
          if (standard == Standard.PAL)
          {
            strprofileType = Config.GetFile(Config.Dir.Base, @"Profiles\MPHigh-PAL.prx");
          }
          Log.Info("TSReader2WMV: set WMV High quality profile {0}", strprofileType);
          break;
        case Quality.Medium:
          if (standard == Standard.Film)
          {
            strprofileType = Config.GetFile(Config.Dir.Base, @"Profiles\MPMedium-FILM.prx");
          }
          if (standard == Standard.NTSC)
          {
            strprofileType = Config.GetFile(Config.Dir.Base, @"Profiles\MPMedium-NTSC.prx");
          }
          if (standard == Standard.PAL)
          {
            strprofileType = Config.GetFile(Config.Dir.Base, @"Profiles\MPMedium-PAL.prx");
          }
          Log.Info("TSReader2WMV: set WMV Medium quality profile {0}", strprofileType);
          break;
        case Quality.Low:
          if (standard == Standard.Film)
          {
            strprofileType = Config.GetFile(Config.Dir.Base, @"Profiles\MPLow-FILM.prx");
          }
          if (standard == Standard.NTSC)
          {
            strprofileType = Config.GetFile(Config.Dir.Base, @"Profiles\MPLow-NTSC.prx");
          }
          if (standard == Standard.PAL)
          {
            strprofileType = Config.GetFile(Config.Dir.Base, @"Profiles\MPLow-PAL.prx");
          }
          Log.Info("TSReader2WMV: set WMV Low quality profile {0}", strprofileType);
          break;
        case Quality.Portable:
          if (standard == Standard.Film)
          {
            strprofileType = Config.GetFile(Config.Dir.Base, @"Profiles\MPPortable-FILM.prx");
          }
          if (standard == Standard.NTSC)
          {
            strprofileType = Config.GetFile(Config.Dir.Base, @"Profiles\MPPortable-NTSC.prx");
          }
          if (standard == Standard.PAL)
          {
            strprofileType = Config.GetFile(Config.Dir.Base, @"Profiles\MPPortable-PAL.prx");
          }
          Log.Info("TSReader2WMV: set WMV Portable quality profile {0}", strprofileType);
          break;
        case Quality.Custom:
          //load custom profile
          string customBitrate = "";
          //Adjust the parameters to suit the custom settings the user has selected.
          switch (bitrate)
          {
            case 0:
              customBitrate = "100Kbs";
              break;
            case 1:
              customBitrate = "256Kbs";
              break;
            case 2: customBitrate = "384Kbs";
              break;
            case 3:
              customBitrate = "768Kbs";
              break;
            case 4:
              customBitrate = "1536Kbs";
              break;
            case 5:
              customBitrate = "3072Kbs";
              break;
            case 6:
              customBitrate = "5376Kbs";
              break;
          }
          Log.Info("TSReader2WMV: custom bitrate = {0}", customBitrate);
          //TODO: get fps values & frame size
          //TODO: adjust settings required
          //Call the SetCutomPorfile method to load the custom profile, adjust it's params from user settings & then save it.
          //SetCutomProfile(videoBitrate, audioBitrate, videoHeight, videoWidth, videoFps); //based on user inputs
          //We then reload it after as per other quality settings / profiles.
          strprofileType = Config.GetFile(Config.Dir.Base, @"Profiles\MPCustom.prx");
          Log.Info("TSReader2WMV: set WMV Custom quality profile {0}", strprofileType);
          break;
      }
      //Loads profile from the above quality selection.
      StreamReader prx = new StreamReader(strprofileType);
      String profileContents = prx.ReadToEnd();
      profileManager2 = profileManager as IWMProfileManager2;

      hr = profileManager2.LoadProfileByData(profileContents, out profile);
      if (hr != 0)
      {
        Log.Info("TSReader2WMV: get WMV profile - FAILED! {0}", hr);
        Cleanup();
        return false;
      }
      Log.Info("TSReader2WMV: load profile - SUCCESS!");
      //configures the WM ASF Writer to the chosen profile
      hr = config.ConfigureFilterUsingProfile(profile);
      if (hr != 0)
      {
        Log.Info("TSReader2WMV: configure profile - FAILED! {0}", hr);
        Cleanup();
        return false;
      }
      Log.Info("TSReader2WMV: configure profile - SUCCESS!");
      //TODO: Add DB recording information into WMV.

      //Release resorces
      if (profile != null)
      {
        Marshal.ReleaseComObject(profile);
        profile = null;
      }
      if (profileManager != null)
      {
        Marshal.ReleaseComObject(profileManager);
        profileManager = null;
      }
      return true;
    }

    void SetCutomProfile(int vidbitrate, int audbitrate, int vidheight, int vidwidth, double fps)
    {
      //seperate method atm braindump for adjusting an existing profile (prx file)
      //method call is not enabled yet
      IWMProfileManager profileManager = null;
      IWMProfileManager2 profileManager2 = null;
      IWMProfile profile = null;
      IWMStreamConfig streamConfig;
      //IWMInputMediaProps inputProps = null;
      IWMProfileManagerLanguage profileManagerLanguage = null;
      WMVersion wmversion = WMVersion.V8_0;
      int nbrProfiles = 0;
      short langID;
      StringBuilder profileName = new StringBuilder(MAXLENPROFNAME);
      StringBuilder profileDescription = new StringBuilder(MAXLENPROFDESC);
      int profileNameLen = MAXLENPROFNAME;
      int profileDescLen = MAXLENPROFDESC;
      profileName.Length = 0;
      profileDescription.Length = 0;
      double videoFps = fps;
      long singleFramePeriod = (long)((10000000L / fps));
      //Guid guidInputType;
      //int dwInputCount = 0;
      int hr;
      int videoBitrate = vidbitrate;
      int audioBitrate = audbitrate;
      int videoHeight = vidheight;
      int videoWidth = vidwidth;
      double videofps = fps;
      int streamCount = 0;
      IWMMediaProps streamMediaProps = null;
      IntPtr mediaTypeBufferPtr = IntPtr.Zero;
      uint mediaTypeBufferSize = 0;
      Guid streamType = Guid.Empty;
      WmMediaType videoMediaType = new WmMediaType();
      //Set WMVIDEOHEADER
      WMVIDEOINFOHEADER videoInfoHeader = new WMVIDEOINFOHEADER();
      //Setup the profile manager
      hr = WMLib.WMCreateProfileManager(out profileManager);
      profileManager2 = (IWMProfileManager2)profileManager;
      //Set profile version - possibly not needed in this case.
      profileManager2.SetSystemProfileVersion(WMVersion.V8_0);
      //get the profile to modify
      string strprofileType = Config.GetFile(Config.Dir.Base, @"Profiles\MPCustom.prx");
      //read the profile contents
      StreamReader prx = new StreamReader(strprofileType);
      String profileContents = prx.ReadToEnd();
      profileManager2 = profileManager as IWMProfileManager2;
      profileManagerLanguage = profileManager as IWMProfileManagerLanguage;
      hr = profileManager2.GetSystemProfileVersion(out wmversion);
      Log.Info("TSReader2WMV: WM version=" + wmversion.ToString());
      hr = profileManagerLanguage.GetUserLanguageID(out langID);
      Log.Info("TSReader2WMV: WM language ID=" + langID.ToString());
      hr = profileManager2.SetSystemProfileVersion(DefaultWMversion);
      hr = profileManager2.GetSystemProfileCount(out nbrProfiles);
      Log.Info("TSReader2WMV: ProfileCount=" + nbrProfiles.ToString());
      //load the profile contents
      hr = profileManager.LoadProfileByData(profileContents, out profile);
      //get the profile name
      hr = profile.GetName(profileName, ref profileNameLen);
      Log.Info("TSReader2WMV: profile name {0}", profileName.ToString());
      //get the profile description
      hr = profile.GetDescription(profileDescription, ref profileDescLen);
      Log.Info("TSReader2WMV: profile description {0}", profileDescription.ToString());
      //get the stream count
      hr = profile.GetStreamCount(out streamCount);
      for (int i = 0; i < streamCount; i++)
      {
        profile.GetStream(i, out streamConfig);
        streamMediaProps = (IWMMediaProps)streamConfig;
        streamConfig.GetStreamType(out streamType);
        if (streamType == MediaType.Video)
        {
          //adjust the video details based on the user input values.
          streamConfig.SetBitrate(videoBitrate);
          streamConfig.SetBufferWindow(-1); //3 or 5 seconds ???
          streamMediaProps.GetMediaType(IntPtr.Zero, ref mediaTypeBufferSize);
          mediaTypeBufferPtr = Marshal.AllocHGlobal((int)mediaTypeBufferSize);
          streamMediaProps.GetMediaType(mediaTypeBufferPtr, ref mediaTypeBufferSize);
          Marshal.PtrToStructure(mediaTypeBufferPtr, videoMediaType);
          Marshal.FreeHGlobal(mediaTypeBufferPtr);
          Marshal.PtrToStructure(videoMediaType.pbFormat, videoInfoHeader);
          videoInfoHeader.TargetRect.right = 0; // set to zero to take source size
          videoInfoHeader.TargetRect.bottom = 0; // set to zero to take source size
          videoInfoHeader.BmiHeader.Width = videoWidth;
          videoInfoHeader.BmiHeader.Height = videoHeight;
          videoInfoHeader.BitRate = videoBitrate;
          videoInfoHeader.AvgTimePerFrame = singleFramePeriod; //Need to check how this is to be calculated
          IntPtr vidInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WMVIDEOINFOHEADER)));
          Marshal.StructureToPtr(videoInfoHeader, vidInfoPtr, false);
          videoMediaType.pbFormat = vidInfoPtr;
          hr = streamMediaProps.SetMediaType(videoMediaType);
          Marshal.FreeHGlobal(vidInfoPtr);
        }
        if (streamType == MediaType.Audio)
        {
          //adjust the audio details based on the user input
          //audio is determined from bitrate selection and thus effects audio profile.
          hr = streamConfig.SetBitrate(audioBitrate);
          hr = streamConfig.SetBufferWindow(-1); //3 or 5 seconds ???
          //TODO: set the WaveformatEx profile info etc
        }
        //recofigures the stream ready for saving
        hr = profile.ReconfigStream(streamConfig);
      }
      //save the profile
      //You should make two calls to SaveProfile.
      //On the first call, pass NULL as pwszProfile.
      int profileLength = 0;
      hr = profileManager2.SaveProfile(profile, null, ref profileLength);
      //On return, the value of pdwLength is set to the length required to hold the profile in string form.
      //TODO: set memory buffer to profileLength
      //Then you can allocate the required amount of memory for the buffer and pass a pointer to it as pwszProfile on the second call.
      hr = profileManager2.SaveProfile(profile, profileContents, ref profileLength);
    }
  }
}

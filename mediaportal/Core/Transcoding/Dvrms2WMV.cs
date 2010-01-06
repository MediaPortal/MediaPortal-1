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
using System.IO;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using DirectShowLib;
using DirectShowLib.SBE;
using DShowNET.Helper;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Configuration;


namespace MediaPortal.Core.Transcoding
{
  /// <summary>
  /// This class encodes an video file to .wmv format
  /// </summary>
  public class DVRMS2WMV : ITranscode
  {
    #region imports

    [DllImport("dvblib.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int SetWmvProfile(DirectShowLib.IBaseFilter filter, int bitrate, int fps, int screenX,
                                            int screenY);

    #endregion

    #region constants

    private const WMVersion DefaultWMversion = WMVersion.V8_0;
    private const int MAXLENPROFNAME = 100;
    private const int MAXLENPROFDESC = 512;

    #endregion

    #region Guids

    private Guid IID_IWMWriterAdvanced2 = new Guid(0x962dc1ec, 0xc046, 0x4db8, 0x9c, 0xc7, 0x26, 0xce, 0xae, 0x50, 0x08,
                                                   0x17);

    #endregion

    protected DsROTEntry _rotEntry = null;
    protected IGraphBuilder graphBuilder = null;
    protected IStreamBufferSource bufferSource = null;
    protected IMediaControl mediaControl = null;
    protected IMediaPosition mediaPos = null;
    protected IStreamBufferMediaSeeking mediaSeeking = null;
    protected IMediaEventEx mediaEvt = null;
    protected IBaseFilter Mpeg2VideoCodec = null;
    protected IBaseFilter Mpeg2AudioCodec = null;
    protected IBaseFilter fileWriterbase = null;
    protected long m_dDuration;
    protected int bitrate;
    protected double fps;
    protected Size screenSize;
    protected const int WS_CHILD = 0x40000000; // attributes for video window
    protected const int WS_CLIPCHILDREN = 0x02000000;
    protected const int WS_CLIPSIBLINGS = 0x04000000;

    public DVRMS2WMV() {}

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
        if (ext.ToLower() != ".dvr-ms" && ext.ToLower() != ".sbe")
        {
          Log.Info("DVRMS2WMV: wrong file format");
          return false;
        }
        Log.Info("DVRMS2WMV: create graph");
        graphBuilder = (IGraphBuilder)new FilterGraph();
        _rotEntry = new DsROTEntry((IFilterGraph)graphBuilder);
        Log.Info("DVRMS2WMV: add streambuffersource");
        bufferSource = (IStreamBufferSource)new StreamBufferSource();
        IBaseFilter filter = (IBaseFilter)bufferSource;
        graphBuilder.AddFilter(filter, "SBE SOURCE");
        Log.Info("DVRMS2WMV: load file:{0}", info.file);
        IFileSourceFilter fileSource = (IFileSourceFilter)bufferSource;
        int hr = fileSource.Load(info.file, null);
        //add mpeg2 audio/video codecs
        string strVideoCodec = "";
        string strAudioCodec = "";
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.MPSettings())
        {
          strVideoCodec = xmlreader.GetValueAsString("mytv", "videocodec", "MPC - MPEG-2 Video Decoder (Gabest)");
          strAudioCodec = xmlreader.GetValueAsString("mytv", "audiocodec", "MPC - MPA Decoder Filter");
        }
        Log.Info("DVRMS2WMV: add mpeg2 video codec:{0}", strVideoCodec);
        Mpeg2VideoCodec = DirectShowUtil.AddFilterToGraph(graphBuilder, strVideoCodec);
        if (hr != 0)
        {
          Log.Error("DVRMS2WMV:FAILED:Add mpeg2 video  to filtergraph :0x{0:X}", hr);
          Cleanup();
          return false;
        }
        Log.Info("DVRMS2WMV: add mpeg2 audio codec:{0}", strAudioCodec);
        Mpeg2AudioCodec = DirectShowUtil.AddFilterToGraph(graphBuilder, strAudioCodec);
        if (Mpeg2AudioCodec == null)
        {
          Log.Error("DVRMS2WMV:FAILED:unable to add mpeg2 audio codec");
          Cleanup();
          return false;
        }
        Log.Info("DVRMS2WMV: connect streambufer source->mpeg audio/video decoders");
        //connect output #0 of streambuffer source->mpeg2 audio codec pin 1
        //connect output #1 of streambuffer source->mpeg2 video codec pin 1
        IPin pinOut0, pinOut1;
        IPin pinIn0, pinIn1;
        pinOut0 = DsFindPin.ByDirection((IBaseFilter)bufferSource, PinDirection.Output, 0); //audio
        pinOut1 = DsFindPin.ByDirection((IBaseFilter)bufferSource, PinDirection.Output, 1); //video
        if (pinOut0 == null || pinOut1 == null)
        {
          Log.Error("DVRMS2WMV:FAILED:unable to get pins of source");
          Cleanup();
          return false;
        }
        pinIn0 = DsFindPin.ByDirection(Mpeg2VideoCodec, PinDirection.Input, 0); //video
        pinIn1 = DsFindPin.ByDirection(Mpeg2AudioCodec, PinDirection.Input, 0); //audio
        if (pinIn0 == null || pinIn1 == null)
        {
          Log.Error("DVRMS2WMV:FAILED:unable to get pins of mpeg2 video/audio codec");
          Cleanup();
          return false;
        }
        hr = graphBuilder.Connect(pinOut0, pinIn1);
        if (hr != 0)
        {
          Log.Error("DVRMS2WMV:FAILED:unable to connect audio pins :0x{0:X}", hr);
          Cleanup();
          return false;
        }
        hr = graphBuilder.Connect(pinOut1, pinIn0);
        if (hr != 0)
        {
          Log.Error("DVRMS2WMV:FAILED:unable to connect video pins :0x{0:X}", hr);
          Cleanup();
          return false;
        }
        string outputFilename = System.IO.Path.ChangeExtension(info.file, ".wmv");
        if (!AddWmAsfWriter(outputFilename, quality, standard)) return false;
        Log.Info("DVRMS2WMV: start pre-run");
        mediaControl = graphBuilder as IMediaControl;
        mediaSeeking = bufferSource as IStreamBufferMediaSeeking;
        mediaEvt = graphBuilder as IMediaEventEx;
        mediaPos = graphBuilder as IMediaPosition;
        //get file duration
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
        Log.Info("DVRMS2WMV: movie duration:{0}", Util.Utils.SecondsToHMSString((int)duration));
        hr = mediaControl.Run();
        if (hr != 0)
        {
          Log.Error("DVRMS2WMV:FAILED:unable to start graph :0x{0:X}", hr);
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
        Log.Info("DVRMS2WMV: pre-run done");
        Log.Info("DVRMS2WMV: Get duration of movie");
        mediaControl.Stop();
        FilterState state;
        mediaControl.GetState(500, out state);
        GC.Collect();
        GC.Collect();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        Log.Info("DVRMS2WMV: reconnect mpeg2 video codec->ASF WM Writer");
        graphBuilder.RemoveFilter(fileWriterbase);
        if (!AddWmAsfWriter(outputFilename, quality, standard)) return false;
        Log.Info("DVRMS2WMV: Start transcoding");
        hr = mediaControl.Run();
        if (hr != 0)
        {
          Log.Error("DVRMS2WMV:FAILED:unable to start graph :0x{0:X}", hr);
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

    private void Cleanup()
    {
      Log.Info("DVRMS2WMV: cleanup");
      if (mediaControl != null)
      {
        mediaControl.Stop();
        mediaControl = null;
      }
      mediaSeeking = null;
      mediaEvt = null;
      mediaPos = null;
      mediaControl = null;
      if (Mpeg2AudioCodec != null)
        DirectShowUtil.ReleaseComObject(Mpeg2AudioCodec);
      Mpeg2AudioCodec = null;
      if (Mpeg2VideoCodec != null)
        DirectShowUtil.ReleaseComObject(Mpeg2VideoCodec);
      Mpeg2VideoCodec = null;
      if (bufferSource != null)
        DirectShowUtil.ReleaseComObject(bufferSource);
      bufferSource = null;
      DirectShowUtil.RemoveFilters(graphBuilder);
      if (_rotEntry != null)
      {
        _rotEntry.Dispose();
      }
      _rotEntry = null;
      if (graphBuilder != null)
        DirectShowUtil.ReleaseComObject(graphBuilder);
      graphBuilder = null;
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

    private bool AddWmAsfWriter(string fileName, Quality quality, Standard standard)
    {
      //add asf file writer
      IPin pinOut0, pinOut1;
      IPin pinIn0, pinIn1;
      Log.Info("DVRMS2WMV: add WM ASF Writer to graph");
      string monikerAsfWriter =
        @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{7C23220E-55BB-11D3-8B16-00C04FB6BD3D}";
      fileWriterbase = Marshal.BindToMoniker(monikerAsfWriter) as IBaseFilter;
      if (fileWriterbase == null)
      {
        Log.Error("DVRMS2WMV:FAILED:Unable to create ASF WM Writer");
        Cleanup();
        return false;
      }

      int hr = graphBuilder.AddFilter(fileWriterbase, "WM ASF Writer");
      if (hr != 0)
      {
        Log.Error("DVRMS2WMV:FAILED:Add ASF WM Writer to filtergraph :0x{0:X}", hr);
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
      Log.Info("DVRMS2WMV: connect audio/video codecs outputs -> ASF WM Writer");
      //connect output #0 of videocodec->asf writer pin 1
      //connect output #0 of audiocodec->asf writer pin 0
      pinOut0 = DsFindPin.ByDirection((IBaseFilter)Mpeg2AudioCodec, PinDirection.Output, 0);
      pinOut1 = DsFindPin.ByDirection((IBaseFilter)Mpeg2VideoCodec, PinDirection.Output, 0);
      if (pinOut0 == null || pinOut1 == null)
      {
        Log.Error("DVRMS2WMV:FAILED:unable to get outpins of video codec");
        Cleanup();
        return false;
      }
      pinIn0 = DsFindPin.ByDirection(fileWriterbase, PinDirection.Input, 0);
      if (pinIn0 == null)
      {
        Log.Error("DVRMS2WMV:FAILED:unable to get pins of asf wm writer");
        Cleanup();
        return false;
      }
      hr = graphBuilder.Connect(pinOut0, pinIn0);
      if (hr != 0)
      {
        Log.Error("DVRMS2WMV:FAILED:unable to connect audio pins :0x{0:X}", hr);
        Cleanup();
        return false;
      }
      pinIn1 = DsFindPin.ByDirection(fileWriterbase, PinDirection.Input, 1);
      if (pinIn1 == null)
      {
        Log.Error("DVRMS2WMV:FAILED:unable to get pins of asf wm writer");
        Cleanup();
        return false;
      }
      hr = graphBuilder.Connect(pinOut1, pinIn1);
      if (hr != 0)
      {
        Log.Error("DVRMS2WMV:FAILED:unable to connect video pins :0x{0:X}", hr);
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
          Log.Info("DVRMS2WMV: set WMV HiDef quality profile {0}", strprofileType);
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
          Log.Info("DVRMS2WMV: set WMV Very High quality profile {0}", strprofileType);
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
          Log.Info("DVRMS2WMV: set WMV High quality profile {0}", strprofileType);
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
          Log.Info("DVRMS2WMV: set WMV Medium quality profile {0}", strprofileType);
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
          Log.Info("DVRMS2WMV: set WMV Low quality profile {0}", strprofileType);
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
          Log.Info("DVRMS2WMV: set WMV Portable quality profile {0}", strprofileType);
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
            case 2:
              customBitrate = "384Kbs";
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
          Log.Info("DVRMS2WMV: custom bitrate = {0}", customBitrate);
          //TODO: get fps values & frame size
          //TODO: adjust settings required
          //Call the SetCutomPorfile method to load the custom profile, adjust it's params from user settings & then save it.
          //SetCutomProfile(videoBitrate, audioBitrate, videoHeight, videoWidth, videoFps); //based on user inputs
          //We then reload it after as per other quality settings / profiles.
          strprofileType = Config.GetFile(Config.Dir.Base, @"Profiles\MPCustom.prx");
          Log.Info("DVRMS2WMV: set WMV Custom quality profile {0}", strprofileType);
          break;
      }
      //Loads profile from the above quality selection.
      StreamReader prx = new StreamReader(strprofileType);
      String profileContents = prx.ReadToEnd();
      profileManager2 = profileManager as IWMProfileManager2;

      hr = profileManager2.LoadProfileByData(profileContents, out profile);
      if (hr != 0)
      {
        Log.Info("DVRMS2WMV: get WMV profile - FAILED! {0}", hr);
        Cleanup();
        return false;
      }
      Log.Info("DVRMS2WMV: load profile - SUCCESS!");
      //configures the WM ASF Writer to the chosen profile
      hr = config.ConfigureFilterUsingProfile(profile);
      if (hr != 0)
      {
        Log.Info("DVRMS2WMV: configure profile - FAILED! {0}", hr);
        Cleanup();
        return false;
      }
      Log.Info("DVRMS2WMV: configure profile - SUCCESS!");
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

    private void SetCutomProfile(int vidbitrate, int audbitrate, int vidheight, int vidwidth, double fps)
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
      Log.Info("DVRMS2WMV: WM version=" + wmversion.ToString());
      hr = profileManagerLanguage.GetUserLanguageID(out langID);
      Log.Info("DVRMS2WMV: WM language ID=" + langID.ToString());
      hr = profileManager2.SetSystemProfileVersion(DefaultWMversion);
      hr = profileManager2.GetSystemProfileCount(out nbrProfiles);
      Log.Info("DVRMS2WMV: ProfileCount=" + nbrProfiles.ToString());
      //load the profile contents
      hr = profileManager.LoadProfileByData(profileContents, out profile);
      //get the profile name
      hr = profile.GetName(profileName, ref profileNameLen);
      Log.Info("DVRMS2WMV: profile name {0}", profileName.ToString());
      //get the profile description
      hr = profile.GetDescription(profileDescription, ref profileDescLen);
      Log.Info("DVRMS2WMV: profile description {0}", profileDescription.ToString());
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
          IntPtr vidInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof (WMVIDEOINFOHEADER)));
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
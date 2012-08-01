#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using DirectShowLib;
using DShowNET.Helper;
using MediaPortal.Configuration;
using MediaPortal.ExtensionMethods;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Player.Subtitles;
using MediaPortal.Player.PostProcessing;

namespace MediaPortal.Player
{
  [ComImport, Guid("17989414-C927-4D73-AB6C-19DF37602AC4"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IGraphRebuildDelegate
  {
    [PreserveSig]
    int RebuildPin(IFilterGraph pGraph, IPin pPin);
  }

  [ComVisible(true),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("FC4801A3-2BA9-11CF-A229-00AA003D7352")]
  public interface IObjectWithSite
  {
    void SetSite([In, MarshalAs(UnmanagedType.IUnknown)] Object pUnkSite);
    void GetSite(ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out Object ppvSite);
  }

  [ComImport, Guid("B98D13E7-55DB-4385-A33D-09FD1BA26338")]
  public class LAVSplitterSource { }

  [ComImport, Guid("171252A0-8820-4AFE-9DF8-5C92B2D66B04")]
  public class LAVSplitter { }

  public class VideoPlayerVMR9 : VideoPlayerVMR7, IGraphRebuildDelegate
  {
    protected VMR9Util Vmr9 = null;
    private Guid LATMAAC = new Guid("000000ff-0000-0010-8000-00aa00389b71");
    private Guid FileSourceSync = new Guid("1AC0BEBD-4D2B-45AD-BCEB-F2C41C5E3788");
    private Dictionary<string, object> PostProcessFilterVideo = new Dictionary<string, object>();
    private Dictionary<string, object> PostProcessFilterAudio = new Dictionary<string, object>();
    private Dictionary<string, object> PostProcessFilterMPAudio = new Dictionary<string, object>();
    public FilterConfig filterConfig;
    public FilterCodec filterCodec;

    public VideoPlayerVMR9()
    {
      _mediaType = g_Player.MediaType.Video;
    }

    public VideoPlayerVMR9(g_Player.MediaType type)
    {
      _mediaType = type;
    }

    protected override void OnInitialized()
    {
      if (Vmr9 != null)
      {
        Vmr9.Enable(true);
        _updateNeeded = true;
        SetVideoWindow();
      }
    }

    public class FilterCodec
    {
      public IBaseFilter _audioRendererFilter { get; set; }
      public IBaseFilter VideoCodec { get; set; }
      public IBaseFilter AudioCodec { get; set; }
    }

    public virtual FilterCodec GetFilterCodec()
    {
      FilterCodec filterCodec = new FilterCodec();

      filterCodec.VideoCodec = null;
      filterCodec.AudioCodec = null;
      filterCodec._audioRendererFilter = null;

      return filterCodec;
    }

    public class FilterConfig
    {
      public FilterConfig()
      {
        OtherFilters = new List<string>();
      }

      public bool bAutoDecoderSettings { get; set; }
      public bool bForceSourceSplitter { get; set; }
      public bool wmvAudio { get; set; }
      public bool autoloadSubtitles { get; set; }
      public string strsplitterfilter { get; set; }
      public string strsplitterfilefilter { get; set; }
      public string Video { get; set; }
      public string VideoH264 { get; set; }
      public string VideoVC1 { get; set; }
      public string VideoVC1I { get; set; }
      public string VideoXVID { get; set; }
      public string Audio { get; set; }
      public string AudioAAC { get; set; }
      public string AudioRenderer { get; set; }
      public string strextAudioCodec { get; set; }
      public string strextAudioSource { get; set; }
      public List<string> OtherFilters { get; set; }
    }

    protected virtual FilterConfig GetFilterConfiguration()
    {
      FilterConfig filterConfig = new FilterConfig();

      using (Settings xmlreader = new MPSettings())
      {

        // get pre-defined filter setup
        filterConfig.bAutoDecoderSettings = xmlreader.GetValueAsBool("movieplayer", "autodecodersettings", false);
        filterConfig.bForceSourceSplitter = xmlreader.GetValueAsBool("movieplayer", "forcesourcesplitter", false);
        filterConfig.wmvAudio = xmlreader.GetValueAsBool("movieplayer", "wmvaudio", false);
        filterConfig.autoloadSubtitles = xmlreader.GetValueAsBool("subtitles", "enabled", false);
        filterConfig.strsplitterfilter = xmlreader.GetValueAsString("movieplayer", "splitterfilter", "");
        filterConfig.strsplitterfilefilter = xmlreader.GetValueAsString("movieplayer", "splitterfilefilter", "");
        filterConfig.Video = xmlreader.GetValueAsString("movieplayer", "mpeg2videocodec", "");
        filterConfig.Audio = xmlreader.GetValueAsString("movieplayer", "mpeg2audiocodec", "");
        filterConfig.AudioAAC = xmlreader.GetValueAsString("movieplayer", "aacaudiocodec", "");
        filterConfig.VideoH264 = xmlreader.GetValueAsString("movieplayer", "h264videocodec", "");
        filterConfig.VideoVC1 = xmlreader.GetValueAsString("movieplayer", "vc1videocodec", "");
        filterConfig.VideoVC1I = xmlreader.GetValueAsString("movieplayer", "vc1ivideocodec", "");
        filterConfig.VideoXVID = xmlreader.GetValueAsString("movieplayer", "xvidvideocodec", "");
        filterConfig.AudioRenderer = xmlreader.GetValueAsString("movieplayer", "audiorenderer",
                                                                "Default DirectSound Device");
        filterConfig.strextAudioSource = xmlreader.GetValueAsString("movieplayer", "AudioExtSplitterFilter", "");
        filterConfig.strextAudioCodec = xmlreader.GetValueAsString("movieplayer", "AudioExtFilter", "");

        // get post-processing filter setup
        int i = 0;
        while (xmlreader.GetValueAsString("movieplayer", "filter" + i, "undefined") != "undefined")
        {
          if (xmlreader.GetValueAsBool("movieplayer", "usefilter" + i, false))
          {
            filterConfig.OtherFilters.Add(xmlreader.GetValueAsString("movieplayer", "filter" + i, "undefined"));
          }
          i++;
        }
      }
      return filterConfig;
    }

    protected void disableCC()
    {
      while (true)
      {
        IBaseFilter basefilter;
        DirectShowUtil.FindFilterByClassID(graphBuilder, ClassId.Line21_1, out basefilter);
        if (basefilter == null)
          DirectShowUtil.FindFilterByClassID(graphBuilder, ClassId.Line21_2, out basefilter);
        if (basefilter != null)
        {
          graphBuilder.RemoveFilter(basefilter);
          DirectShowUtil.ReleaseComObject(basefilter);
          basefilter = null;
          Log.Info("VideoPlayer9: Cleanup Captions");
        }
        else
          break;
      }
    }

    protected void disableISR()
    {
      //remove InternalScriptRenderer as it takes subtitle pin
      IBaseFilter isr = null;
      DirectShowUtil.FindFilterByClassID(graphBuilder, ClassId.InternalScriptRenderer, out isr);
      if (isr != null)
      {
        graphBuilder.RemoveFilter(isr);
        DirectShowUtil.ReleaseComObject(isr);
      }
    }

    protected void disableVobsub()
    {
      while (true)
      {
        IBaseFilter basefilter;
        DirectShowUtil.FindFilterByClassID(graphBuilder, ClassId.DirectVobSubAutoload, out basefilter);
        if (basefilter == null)
          DirectShowUtil.FindFilterByClassID(graphBuilder, ClassId.DirectVobSubNormal, out basefilter);
        if (basefilter != null)
        {
          graphBuilder.RemoveFilter(basefilter);
          DirectShowUtil.ReleaseComObject(basefilter);
          basefilter = null;
          Log.Info("VideoPlayer9: Cleanup DirectVobSub");
        }
        else
          break;
      }
    }

    /*protected void EnableClock()
    {
      //Get Audio Renderer
      if (filterConfig.AudioRenderer.Length > 0 && filterCodec._audioRendererFilter == null)
      {
        filterCodec._audioRendererFilter = DirectShowUtil.GetFilterByName(graphBuilder, filterConfig.AudioRenderer);
      }
      if (filterCodec._audioRendererFilter != null)
      {
        IMediaFilter mp = (IMediaFilter)graphBuilder;
        IReferenceClock clock = (IReferenceClock)filterCodec._audioRendererFilter;
        int hr = mp.SetSyncSource(null);
        hr = mp.SetSyncSource(clock);
      }
    }*/

    protected void RemoveAudioR()
    {
      //Get Audio Renderer
      if (filterConfig.AudioRenderer.Length > 0 && filterCodec._audioRendererFilter == null)
      {
        filterCodec._audioRendererFilter = DirectShowUtil.GetFilterByName(graphBuilder, filterConfig.AudioRenderer);
      }
      //Detection if it's the good audio renderer connected
      bool ResultPinAudioRenderer = false;
      IPin PinAudioRenderer = DsFindPin.ByDirection(filterCodec._audioRendererFilter, PinDirection.Input, 0); //audio
      if (PinAudioRenderer != null)
        DirectShowUtil.IsPinConnected(PinAudioRenderer, out ResultPinAudioRenderer);
      if (!ResultPinAudioRenderer && filterCodec._audioRendererFilter != null)
      {
        this.graphBuilder.RemoveFilter(filterCodec._audioRendererFilter);
        DirectShowUtil.ReleaseComObject(filterCodec._audioRendererFilter);
        filterCodec._audioRendererFilter = null;
      }
      if (PinAudioRenderer != null)
      {
        DirectShowUtil.ReleaseComObject(PinAudioRenderer);
        PinAudioRenderer = null;
      }
    }

    public int RebuildPin(IFilterGraph pGraph, IPin pPin)
    {
      //Set codec bool to false
      ResetCodecBool();

      IPin pinTo;
      if (pPin != null)
      {
        int hr = pPin.ConnectedTo(out pinTo);
        if (hr >= 0 && pinTo != null)
        {
          PinInfo pInfo;
          pinTo.QueryPinInfo(out pInfo);
          FilterInfo fInfo;
          pInfo.filter.QueryFilterInfo(out fInfo);
          if (pPin != null)
          {
            RebuildMediaType(pPin);
            Log.Debug("VideoPlayer9: Rebuild LAV Delegate Info filter Name - {0}", fInfo.achName);

            if (MediatypeVideo)
            {
              //Video Part
              if (h264Codec)
              {
                if (fInfo.achName == filterConfig.VideoH264)
                {
                  RebuildRelease(pInfo, fInfo, pinTo, pPin);
                  return 1;
                }
              }
              else if (vc1Codec)
              {
                if (fInfo.achName == filterConfig.VideoVC1)
                {
                  RebuildRelease(pInfo, fInfo, pinTo, pPin);
                  return 1;
                }
              }
              else if (vc1ICodec)
              {
                if (fInfo.achName == filterConfig.VideoVC1I)
                {
                  RebuildRelease(pInfo, fInfo, pinTo, pPin);
                  return 1;
                }
              }
              else if (xvidCodec)
              {
                if (fInfo.achName == filterConfig.VideoXVID)
                {
                  RebuildRelease(pInfo, fInfo, pinTo, pPin);
                  return 1;
                }
              }
              else if (fInfo.achName == filterConfig.Video)
              {
                RebuildRelease(pInfo, fInfo, pinTo, pPin);
                return 1;
              }
              iChangedMediaTypes = 2;
              DoGraphRebuild();
              Log.Debug("VideoPlayer9: Rebuild LAV Delegate filter Video");
              RebuildRelease(pInfo, fInfo, pinTo, pPin);
              return 1;
            }
            else if (MediatypeAudio)
            {
              //Audio Part 
              if (aacCodec)
              {
                if (fInfo.achName == filterConfig.AudioAAC)
                {
                  RebuildRelease(pInfo, fInfo, pinTo, pPin);
                  return 1;
                }
              }
              if (aacCodecLav && fInfo.achName == LAV_AUDIO)
              {
                if (fInfo.achName == filterConfig.AudioAAC)
                {
                  RebuildRelease(pInfo, fInfo, pinTo, pPin);
                  return 1;
                }
              }
              else if ((!aacCodecLav || !aacCodec) && fInfo.achName == filterConfig.Audio)
              {
                RebuildRelease(pInfo, fInfo, pinTo, pPin);
                return 1;
              }
              iChangedMediaTypes = 1;
              DoGraphRebuild();
              Log.Debug("VideoPlayer9: Rebuild LAV Delegate filter Audio");
              RebuildRelease(pInfo, fInfo, pinTo, pPin);
              return 1;
            }
            else if (MediatypeSubtitle)
            {
              RebuildRelease(pInfo, fInfo, pinTo, pPin);
              return -1;
            }
          }
          DsUtils.FreePinInfo(pInfo);
          DirectShowUtil.ReleaseComObject(fInfo.pGraph);
          DirectShowUtil.ReleaseComObject(pinTo);
          pinTo = null;
        }
        DirectShowUtil.ReleaseComObject(pPin);
        pPin = null;
      }
      Log.Debug("VideoPlayer9: Rebuild LAV Delegate (No Rebuild from MP, LAV Will doing the job)");
      return -1;
    }

    protected void RebuildRelease(PinInfo pInfo, FilterInfo fInfo, IPin pinTo, IPin pPin)
    {
      DsUtils.FreePinInfo(pInfo);
      DirectShowUtil.ReleaseComObject(fInfo.pGraph);
      DirectShowUtil.ReleaseComObject(pinTo);
      pinTo = null;
      DirectShowUtil.ReleaseComObject(pPin);
      pPin = null;
    }

    protected void RebuildMediaType(IPin pPin)
    {
      //Detection if the Video Stream is VC-1 on output pin of the splitter
      IEnumMediaTypes enumMediaTypesAudioVideo;
      int hr = pPin.EnumMediaTypes(out enumMediaTypesAudioVideo);
      while (true)
      {
        AMMediaType[] mediaTypes = new AMMediaType[1];
        int typesFetched;
        hr = enumMediaTypesAudioVideo.Next(1, mediaTypes, out typesFetched);
        if (hr != 0 || typesFetched == 0) break;
        if (mediaTypes[0].majorType == MediaType.Video)
        {
          if (mediaTypes[0].subType == MediaSubType.VC1)
          {
            Log.Info("VideoPlayer9: found VC-1 video out pin");
            vc1Codec = true;
          }
          if (mediaTypes[0].subType == MediaSubType.H264 || mediaTypes[0].subType == MediaSubType.AVC1)
          {
            Log.Info("VideoPlayer9: found H264 video out pin");
            h264Codec = true;
          }
          if (mediaTypes[0].subType == MediaSubType.XVID || mediaTypes[0].subType == MediaSubType.xvid ||
              mediaTypes[0].subType == MediaSubType.dx50 || mediaTypes[0].subType == MediaSubType.DX50 ||
              mediaTypes[0].subType == MediaSubType.divx || mediaTypes[0].subType == MediaSubType.DIVX)
          {
            Log.Info("VideoPlayer9: found XVID video out pin");
            xvidCodec = true;
          }
          MediatypeVideo = true;
        }
        else if (mediaTypes[0].majorType == MediaType.Audio)
        {
          //Detection if the Audio Stream is AAC on output pin of the splitter
          if (mediaTypes[0].subType == MediaSubType.LATMAAC || mediaTypes[0].subType == MediaSubType.AAC)
          {
            Log.Info("VideoPlayer9: found AAC Audio out pin");
            aacCodec = true;
          }
          if (mediaTypes[0].subType == MediaSubType.LATMAACLAF)
          {
            Log.Info("VideoPlayer9: found AAC LAVF Audio out pin");
            aacCodecLav = true;
          }
          MediatypeAudio = true;
        }
        else if (mediaTypes[0].majorType == MediaType.Subtitle)
        {
          MediatypeSubtitle = true;
        }
      }
      DirectShowUtil.ReleaseComObject(enumMediaTypesAudioVideo);
      enumMediaTypesAudioVideo = null;
    }

    protected void LoadLAVSplitter(string LAVFilter)
    {
      // Prepare delegate for rebuilddelegate with Lavsplitter
      if (LAVFilter == LAV_SPLITTER_FILTER_SOURCE)
      {
        LAVSplitterSource reader = new LAVSplitterSource();
        _interfaceSourceFilter = reader as IBaseFilter;
        var objectWithSite = reader as IObjectWithSite;
        if (objectWithSite != null)
        {
          objectWithSite.SetSite(this);
        }
      }
      else if (LAVFilter == LAV_SPLITTER_FILTER)
      {
        LAVSplitter reader = new LAVSplitter();
        Splitter = reader as IBaseFilter;
        var objectWithSite = reader as IObjectWithSite;
        if (objectWithSite != null)
        {
          objectWithSite.SetSite(this);
        }
      }
    }

    /// <summary> create the used COM components and get the interfaces. </summary>
    protected override bool GetInterfaces()
    {
      GetInterface = true;
      try
      {
        graphBuilder = (IGraphBuilder) new FilterGraph();
        _rotEntry = new DsROTEntry((IFilterGraph) graphBuilder);
        // add preferred video & audio codecs
        int hr;
        filterConfig = GetFilterConfiguration();
        //Get filterCodecName
        filterCodec = GetFilterCodec();

        if (filterConfig.bAutoDecoderSettings)
        {
          AutoRenderingCheck = true;
          return AutoRendering(this.filterConfig.wmvAudio);
        }

        string extension = Path.GetExtension(m_strCurrentFile).ToLowerInvariant();

        //Get video/audio Info
        _mediaInfo = new MediaInfoWrapper(m_strCurrentFile);

        //Manually add codecs based on file extension if not in auto-settings
        // switch back to directx fullscreen mode
        Log.Info("VideoPlayer9: Enabling DX9 exclusive mode");
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 1, 0, null);
        GUIWindowManager.SendMessage(msg);

        // add the VMR9 in the graph
        // after enabeling exclusive mode, if done first it causes MediPortal to minimize if for example the "Windows key" is pressed while playing a video
        Vmr9 = new VMR9Util();
        if (File.Exists(m_strCurrentFile) && extension != ".dts" && extension != ".mp3" && extension != ".mka" && extension != ".ac3")
        {
          Vmr9.AddVMR9(graphBuilder);
          Vmr9.Enable(false);
        }
        else
        {
          AudioOnly = true;
        }

        if (extension == ".mpls" || extension == ".bdmv")
          filterConfig.bForceSourceSplitter = false;

        if (filterConfig.strsplitterfilter == LAV_SPLITTER_FILTER_SOURCE && filterConfig.bForceSourceSplitter)
        {
          LoadLAVSplitter(LAV_SPLITTER_FILTER_SOURCE);
          hr = graphBuilder.AddFilter(_interfaceSourceFilter, LAV_SPLITTER_FILTER_SOURCE);
          DsError.ThrowExceptionForHR(hr);

          Log.Debug("VideoPlayer9: Add LAVSplitter Source to graph");

          IFileSourceFilter interfaceFile = (IFileSourceFilter)_interfaceSourceFilter;
          hr = interfaceFile.Load(m_strCurrentFile, null);

          if (hr != 0)
          {
            Error.SetError("Unable to play movie", "Unable build graph for VMR9");
            Cleanup();
            return false;
          }
        }
        else
        {
          _interfaceSourceFilter = filterConfig.bForceSourceSplitter
                                     ? DirectShowUtil.AddFilterToGraph(graphBuilder, filterConfig.strsplitterfilter)
                                     : null;
          if (_interfaceSourceFilter == null && !filterConfig.bForceSourceSplitter)
          {
            graphBuilder.AddSourceFilter(m_strCurrentFile, null, out _interfaceSourceFilter);
          }
          else
          {
            try
            {
              int result = ((IFileSourceFilter) _interfaceSourceFilter).Load(m_strCurrentFile, null);
              if (result != 0)
              {
                graphBuilder.RemoveFilter(_interfaceSourceFilter);
                DirectShowUtil.ReleaseComObject(_interfaceSourceFilter);
                _interfaceSourceFilter = null;
                graphBuilder.AddSourceFilter(m_strCurrentFile, null, out _interfaceSourceFilter);
              }
            }

            catch (Exception ex)
            {
              Log.Error(
                "VideoPlayer9: Exception loading Source Filter setup in setting in DShow graph , try to load by merit",
                ex);
              graphBuilder.RemoveFilter(_interfaceSourceFilter);
              DirectShowUtil.ReleaseComObject(_interfaceSourceFilter);
              _interfaceSourceFilter = null;
              graphBuilder.AddSourceFilter(m_strCurrentFile, null, out _interfaceSourceFilter);
            }
          }

          //Detection of File Source (Async.) as source filter, return true if found
          IBaseFilter fileSyncbaseFilter = null;
          DirectShowUtil.FindFilterByClassID(graphBuilder, ClassId.FilesyncSource, out fileSyncbaseFilter);
          if (fileSyncbaseFilter == null)
            graphBuilder.FindFilterByName("File Source (Async.)", out fileSyncbaseFilter);
          if (fileSyncbaseFilter != null && filterConfig.bForceSourceSplitter)
          {
            FileSync = true;
            DirectShowUtil.ReleaseComObject(fileSyncbaseFilter);
            fileSyncbaseFilter = null;
            if (filterConfig.strsplitterfilefilter == LAV_SPLITTER_FILTER)
            {
              LoadLAVSplitter(LAV_SPLITTER_FILTER);
              hr = graphBuilder.AddFilter(Splitter, LAV_SPLITTER_FILTER);
              DsError.ThrowExceptionForHR(hr);

              Log.Debug("VideoPlayer9: Add LAVSplitter to graph");

              if (hr != 0)
              {
                Error.SetError("Unable to play movie", "Unable build graph for VMR9");
                Cleanup();
                return false;
              }
            }
            else
            {
              Splitter = DirectShowUtil.AddFilterToGraph(graphBuilder, filterConfig.strsplitterfilefilter);
            }
          }
        }

        // Add preferred video filters
        UpdateFilters("Video");

        //Add Audio Renderer
        if (filterConfig.AudioRenderer.Length > 0 && filterCodec._audioRendererFilter == null)
        {
          filterCodec._audioRendererFilter = DirectShowUtil.AddAudioRendererToGraph(graphBuilder,
                                                                                    filterConfig.AudioRenderer, false);
        }

        #region load external audio streams

        // check if current "File" is a file... it could also be a URL
        // Directory.Getfiles, ... will other give us an exception
        if (File.Exists(m_strCurrentFile) && !AudioOnly)
        {
          //load audio file (ac3, dts, mka, mp3) only with if the name matches partially with video file.
          string[] audioFiles = Directory.GetFiles(Path.GetDirectoryName(m_strCurrentFile),
                                                   Path.GetFileNameWithoutExtension(m_strCurrentFile) + "*.*");
          bool audioSwitcherLoaded = false;
          foreach (string file in audioFiles)
          {
            switch (Path.GetExtension(file))
            {
              case ".mp3":
              case ".dts":
              case ".mka":
              case ".ac3":
                if (!audioSwitcherLoaded)
                {
                  IBaseFilter switcher = DirectShowUtil.GetFilterByName(graphBuilder, MEDIAPORTAL_AUDIOSWITCHER_FILTER);
                  if (switcher != null)
                  {
                    DirectShowUtil.ReleaseComObject(switcher);
                    switcher = null;
                  }
                  else
                  {
                    _audioSwitcher = DirectShowUtil.AddFilterToGraph(graphBuilder, MEDIAPORTAL_AUDIOSWITCHER_FILTER);
                  }
                  audioSwitcherLoaded = true;
                }
                _AudioSourceFilter = DirectShowUtil.AddFilterToGraph(graphBuilder, FILE_SYNC_FILTER);
                int result = ((IFileSourceFilter) _AudioSourceFilter).Load(file, null);

                //Force using LAVFilter
                _AudioExtSplitterFilter = DirectShowUtil.AddFilterToGraph(graphBuilder, LAV_SPLITTER_FILTER);

                if (result != 0 || _AudioExtSplitterFilter == null)
                {
                  if (_AudioSourceFilter != null)
                  {
                    graphBuilder.RemoveFilter(_AudioSourceFilter);
                    DirectShowUtil.ReleaseComObject(_AudioSourceFilter);
                    _AudioSourceFilter = null;
                  }
                  if (_AudioExtSplitterFilter != null)
                  {
                    graphBuilder.RemoveFilter(_AudioExtSplitterFilter);
                    DirectShowUtil.ReleaseComObject(_AudioExtSplitterFilter);
                    _AudioExtSplitterFilter = null;
                  }
                  //Trying Add Audio decoder in graph
                  AddFilterToGraphAndRelease(filterConfig.Audio);
                  graphBuilder.RenderFile(file, string.Empty);
                  Log.Debug("VideoPlayerVMR9 : External audio file loaded \"{0}\"", file);
                  AudioExternal = true;
                  break;
                }

                //Add Audio decoder in graph
                _AudioExtFilter = DirectShowUtil.AddFilterToGraph(graphBuilder, filterConfig.Audio);

                //Connect Filesource with the splitter
                IPin pinOutAudioExt1 = DsFindPin.ByDirection((IBaseFilter) _AudioSourceFilter, PinDirection.Output, 0);
                IPin pinInAudioExt2 = DsFindPin.ByDirection((IBaseFilter) _AudioExtSplitterFilter, PinDirection.Input, 0);
                hr = graphBuilder.Connect(pinOutAudioExt1, pinInAudioExt2);

                //Connect Splitter with the Audio Decoder
                IPin pinOutAudioExt3 = DsFindPin.ByDirection((IBaseFilter) _AudioExtSplitterFilter, PinDirection.Output,
                                                             0);
                IPin pinInAudioExt4 = DsFindPin.ByDirection((IBaseFilter) _AudioExtFilter, PinDirection.Input, 0);
                hr = graphBuilder.Connect(pinOutAudioExt3, pinInAudioExt4);

                //Render outpin from Audio Decoder
                DirectShowUtil.RenderUnconnectedOutputPins(graphBuilder, _AudioExtFilter);

                //Cleanup External Audio (Release)
                if (_AudioSourceFilter != null)
                {
                  DirectShowUtil.ReleaseComObject(_AudioSourceFilter);
                  _AudioSourceFilter = null;
                }
                if (_AudioExtSplitterFilter != null)
                {
                  DirectShowUtil.ReleaseComObject(_AudioExtSplitterFilter);
                  _AudioExtSplitterFilter = null;
                }
                if (_AudioExtFilter != null)
                {
                  DirectShowUtil.ReleaseComObject(_AudioExtFilter);
                  _AudioExtFilter = null;
                }
                if (pinOutAudioExt1 != null)
                {
                  DirectShowUtil.ReleaseComObject(pinOutAudioExt1);
                  pinOutAudioExt1 = null;
                }
                if (pinInAudioExt2 != null)
                {
                  DirectShowUtil.ReleaseComObject(pinInAudioExt2);
                  pinInAudioExt2 = null;
                }
                if (pinOutAudioExt3 != null)
                {
                  DirectShowUtil.ReleaseComObject(pinOutAudioExt3);
                  pinOutAudioExt3 = null;
                }
                if (pinInAudioExt4 != null)
                {
                  DirectShowUtil.ReleaseComObject(pinInAudioExt4);
                  pinInAudioExt4 = null;
                }

                Log.Debug("VideoPlayerVMR9 : External audio file loaded \"{0}\"", file);
                AudioExternal = true;
                break;
            }
          }
        }

        #endregion

        // Add preferred audio filters
        UpdateFilters("Audio");

        #region Set High Audio

        //Set High Resolution Output > 2 channels
        IBaseFilter baseFilter = null;
        bool FFDShowLoaded = false;
        graphBuilder.FindFilterByName("WMAudio Decoder DMO", out baseFilter);
        if (baseFilter != null && filterConfig.wmvAudio != false) //Also check configuration option enabled
        {
          //Set the filter setting to enable more than 2 audio channels
          const string g_wszWMACHiResOutput = "_HIRESOUTPUT";
          object val = true;
          IPropertyBag propBag = (IPropertyBag) baseFilter;
          hr = propBag.Write(g_wszWMACHiResOutput, ref val);
          if (hr != 0)
          {
            Log.Info("VideoPlayer9: Unable to turn WMAudio multichannel on. Reason: {0}", hr);
          }
          else
          {
            Log.Info("VideoPlayer9: WMAudio Decoder now set for > 2 audio channels");
          }
          if (!FFDShowLoaded)
          {
            IBaseFilter FFDShowAudio = DirectShowUtil.GetFilterByName(graphBuilder, FFDSHOW_AUDIO_DECODER_FILTER);
            if (FFDShowAudio != null)
            {
              DirectShowUtil.ReleaseComObject(FFDShowAudio);
              FFDShowAudio = null;
            }
            else
            {
              _FFDShowAudio = DirectShowUtil.AddFilterToGraph(graphBuilder, FFDSHOW_AUDIO_DECODER_FILTER);
            }
            FFDShowLoaded = true;
          }
          DirectShowUtil.ReleaseComObject(baseFilter);
          baseFilter = null;
        }

        #endregion

        if (_interfaceSourceFilter != null)
        {
          DirectShowUtil.RenderGraphBuilderOutputPins(graphBuilder, _interfaceSourceFilter);
        }

        //Test and remove orphelin Audio Renderer
        //RemoveAudioR();

        //remove InternalScriptRenderer as it takes subtitle pin
        disableISR();

        //disable Closed Captions!
        disableCC();

        DirectShowUtil.RemoveUnusedFiltersFromGraph(graphBuilder);

        //remove orphelin audio renderer
        RemoveAudioR();

        //EnableClock();

        if (Vmr9 == null || !Vmr9.IsVMR9Connected && !AudioOnly)
        {
          Log.Error("VideoPlayer9: Failed to render file -> vmr9");
          mediaCtrl = null;
          Cleanup();
          return false;
        }

        mediaCtrl = (IMediaControl) graphBuilder;
        mediaEvt = (IMediaEventEx) graphBuilder;
        mediaSeek = (IMediaSeeking) graphBuilder;
        mediaPos = (IMediaPosition) graphBuilder;
        basicAudio = (IBasicAudio) graphBuilder;
        videoWin = (IVideoWindow) graphBuilder;
        m_iVideoWidth = Vmr9.VideoWidth;
        m_iVideoHeight = Vmr9.VideoHeight;
        Vmr9.SetDeinterlaceMode();
        return true;
      }
      catch (Exception ex)
      {
        Error.SetError("Unable to play movie", "Unable build graph for VMR9");
        Log.Error("VideoPlayer9: Exception while creating DShow graph {0} {1}", ex.Message, ex.StackTrace);
        Cleanup();
        return false;
      }
    }

    private void AddFilterToGraphAndRelease(string filter) 
    {
      var dsFilter = DirectShowUtil.AddFilterToGraph(graphBuilder, filter);
      DirectShowUtil.ReleaseComObject(dsFilter);
    }

    protected override void DoGraphRebuild()
    {
      if (mediaCtrl != null)
      {
        /*firstinit = false;
        FilterState state;
        int hr = mediaCtrl.GetState(1000, out state);
        if (state == FilterState.Stopped)
        {
          firstinit = true;
        }

        //int hr;
        try
        {
          hr = mediaCtrl.Stop();
          DsError.ThrowExceptionForHR(hr);
        }
        catch (Exception error)
        {
          Log.Error("VideoPlayer9: Error stopping graph: {0}", error.Message);
          //MovieEnded();
        }

        try
        {
          //Make sure the graph has really stopped
          hr = mediaCtrl.GetState(1000, out state);
          DsError.ThrowExceptionForHR(hr);
          if (state != FilterState.Stopped)
          {
            Log.Error("VideoPlayer9: graph still running");
          }
        }
        catch (Exception error)
        {
          Log.Error("VideoPlayer9: Error checking graph state: {0}", error.Message);
        }*/

        // this is a hack for MS Video Decoder and AC3 audio change
        // would suggest to always do full audio and video rendering for all filters
        /*IBaseFilter MSVideoCodec = null;
        graphBuilder.FindFilterByName("Microsoft DTV-DVD Video Decoder", out MSVideoCodec);
        if (MSVideoCodec != null)
        {
          iChangedMediaTypes = 3;
          DirectShowUtil.ReleaseComObject(MSVideoCodec); MSVideoCodec = null;
        }*/
        // hack end
        switch (iChangedMediaTypes)
        {
          case 1: // audio changed
            Log.Info("VideoPlayer9: Rerendering audio pin of Splitter Source filter.");
            UpdateFilters("Audio");
            break;
          case 2: // video changed
            Log.Info("VideoPlayer9: Rerendering video pin of Splitter Source filter.");
            UpdateFilters("Video");
            break;
          /*case 3: // both changed
            Log.Info("VideoPlayer9: Rerendering audio and video pins of Splitter Source filter.");
            UpdateFilters("Audio");
            UpdateFilters("Video");
            break;*/
        }
        if (iChangedMediaTypes != 1 && VideoChange)
        {
          //Release and init Post Process Filter
          if (PostProcessingEngine.engine != null)
          {
            PostProcessingEngine.GetInstance().FreePostProcess();
          }
          IPostProcessingEngine postengine = PostProcessingEngine.GetInstance(true);
          if (!postengine.LoadPostProcessing(graphBuilder))
          {
            PostProcessingEngine.engine = new PostProcessingEngine.DummyEngine();
          }

          //Reload ffdshow or Directvobsub subtitle engine
          if (SubEngine.engine.ToString() != "MediaPortal.Player.Subtitles.MpcEngine" && SubEngine.engine.ToString() != "MediaPortal.Player.Subtitles.DummyEngine")
          {
            disableVobsub();
            SubEngine.GetInstance().FreeSubtitles();
            ISubEngine engine = SubEngine.GetInstance(true);
            if (!engine.LoadSubtitles(graphBuilder, m_strCurrentFile))
            {
              SubEngine.engine = new SubEngine.DummyEngine();
            }
          }
        }
        if (iChangedMediaTypes != 2)
        {
          if (AudioExternal)
          {
            RemoveAudioR();
          }
        }
        if (_interfaceSourceFilter != null)
        {
          DirectShowUtil.RenderGraphBuilderOutputPins(graphBuilder, _interfaceSourceFilter);
          /*if (AudioExternal)
            EnableClock();*/
        }
        if (iChangedMediaTypes != 1 && VideoChange)
        {
          if (SubEngine.engine.ToString() != "MediaPortal.Player.Subtitles.FFDShowEngine" && SubEngine.engine.ToString() != "MediaPortal.Player.Subtitles.DummyEngine")
          {
            FFDShowEngine.DisableFFDShowSubtitles(graphBuilder);
          }
        }

        //remove InternalScriptRenderer as it takes subtitle pin
        disableISR();

        //disable Closed Captions!
        disableCC();

        DirectShowUtil.RemoveUnusedFiltersFromGraph(graphBuilder);

        //remove orphelin audio renderer
        if (iChangedMediaTypes != 2)
        {
          RemoveAudioR();
        }

        /*if (!firstinit && !g_Player.Paused)
        {
          try
          {
            hr = mediaCtrl.Run();
            DsError.ThrowExceptionForHR(hr);
          }
          catch (Exception error)
          {
            Log.Error("VideoPlayer9: Error starting graph: {0}", error.Message);
            Cleanup();
            return;
          }
          Log.Info("VideoPlayer9: Reconfigure graph done");
        }*/
      }
    }

    protected void PostProcessAddVideo()
    {
      foreach (string filter in this.filterConfig.OtherFilters)
      {
        if (FilterHelper.GetVideoCodec().Contains(filter.ToString()) && filter.ToString() != "Core CC Parser")
        {
          var comObject = DirectShowUtil.AddFilterToGraph(graphBuilder, filter);
          if (comObject != null)
          {
            PostProcessFilterVideo.Add(filter, comObject);
          }
        }
      }
    }

    protected void PostProcessAddAudio()
    {
      foreach (string filter in this.filterConfig.OtherFilters)
      {
        if (FilterHelper.GetAudioCodec().Contains(filter.ToString()) && filter.ToString() != "MediaPortal AudioSwitcher")
        {
          var comObject = DirectShowUtil.AddFilterToGraph(graphBuilder, filter);
          if (comObject != null)
          {
            PostProcessFilterAudio.Add(filter, comObject);
          }
        }
      }
    }

    protected void PostProcessAddMPAudio()
    {
      foreach (string filter in this.filterConfig.OtherFilters)
      {
        if (FilterHelper.GetAudioCodec().Contains(filter.ToString()) && filter.ToString() == "MediaPortal AudioSwitcher")
        {
          var comObject = DirectShowUtil.AddFilterToGraph(graphBuilder, filter);
          if (comObject != null)
          {
            PostProcessFilterMPAudio.Add(filter, comObject);
          }
        }
      }
    }

    protected void UpdateFilters(string selection)
    {
      if (selection == "Video")
      {
        VideoChange = false;
        if (PostProcessFilterVideo.Count > 0)
        {
          foreach (var ppFilter in PostProcessFilterVideo)
          {
            if (ppFilter.Value != null)
            {
              DirectShowUtil.RemoveFilters(graphBuilder, ppFilter.Key);
              DirectShowUtil.ReleaseComObject(ppFilter.Value);//, 5000);
            }
          }
          PostProcessFilterVideo.Clear();
          Log.Info("VideoPlayer9: UpdateFilters Cleanup PostProcessVideo");
        }
      }
      else
      {
        if (PostProcessFilterAudio.Count > 0)
        {
          foreach (var ppFilter in PostProcessFilterAudio)
          {
            if (ppFilter.Value != null)
            {
              DirectShowUtil.RemoveFilters(graphBuilder, ppFilter.Key);
              DirectShowUtil.ReleaseComObject(ppFilter.Value);//, 5000);
            }
          }
          PostProcessFilterAudio.Clear();
          Log.Info("VideoPlayer9: UpdateFilters Cleanup PostProcessAudio");
        }
        if (PostProcessFilterMPAudio.Count > 0 && !AudioExternal)
        {
          foreach (var ppFilter in PostProcessFilterMPAudio)
          {
            if (ppFilter.Value != null)
            {
              DirectShowUtil.RemoveFilters(graphBuilder, ppFilter.Key);
              DirectShowUtil.ReleaseComObject(ppFilter.Value);//, 5000);
            }
          }
          PostProcessFilterMPAudio.Clear();
          Log.Info("VideoPlayer9: UpdateFilters Cleanup PostProcessMPAudio");
        }
      }

      // we have to find first filter connected to splitter which will be removed
      IPin pinFrom = FileSync ? DirectShowUtil.FindPin(Splitter, PinDirection.Output, selection) : DirectShowUtil.FindPin(_interfaceSourceFilter, PinDirection.Output, selection);
      IPin pinTo;
      if (pinFrom != null)
      {
        int hr = pinFrom.ConnectedTo(out pinTo);
        if (hr >= 0 && pinTo != null)
        {
          PinInfo pInfo;
          pinTo.QueryPinInfo(out pInfo);
          FilterInfo fInfo;
          pInfo.filter.QueryFilterInfo(out fInfo);
          DirectShowUtil.DisconnectAllPins(graphBuilder, pInfo.filter);
          graphBuilder.RemoveFilter(pInfo.filter);
          Log.Debug("VideoPlayer9: UpdateFilters Remove filter - {0}", fInfo.achName);
          DsUtils.FreePinInfo(pInfo);
          DirectShowUtil.ReleaseComObject(fInfo.pGraph);
          DirectShowUtil.ReleaseComObject(pinTo); pinTo = null;
        }
        DirectShowUtil.ReleaseComObject(pinFrom); pinFrom = null;
      }

      if (selection == "Video")
      {
        //Add Post Process Video Codec
        PostProcessAddVideo();

        //Add Video Codec
        if (filterCodec.VideoCodec != null)
        {
          DirectShowUtil.ReleaseComObject(filterCodec.VideoCodec);
          filterCodec.VideoCodec = null;
        }
        filterCodec.VideoCodec = DirectShowUtil.AddFilterToGraph(this.graphBuilder, MatchFilters(selection));

        VideoChange = true;
      }
      else
      {
        //Add Post Process MediaPortal AudioSwitcher Audio Codec
        if (filterConfig.OtherFilters.Contains("MediaPortal AudioSwitcher") && !AudioExternal)
        {
          PostProcessAddMPAudio();
        }

        //Add Post Process Audio Codec
        PostProcessAddAudio();

        //Add Audio Codec
        if (filterCodec.AudioCodec != null)
        {
          DirectShowUtil.ReleaseComObject(filterCodec.AudioCodec);
          filterCodec.AudioCodec = null;
        }
        filterCodec.AudioCodec = DirectShowUtil.AddFilterToGraph(this.graphBuilder, MatchFilters(selection));
      }
    }

    protected void ResetCodecBool()
    {
      vc1ICodec = false;
      vc1Codec = false;
      h264Codec = false;
      xvidCodec = false;
      aacCodec = false;
      aacCodecLav = false;
      MediatypeVideo = false;
      MediatypeAudio = false;
      MediatypeSubtitle = false;
    }

    protected string MatchFilters(string format)
    {
      string AACCodec = "AAC";
      string VC1Codec = "VC-1";

      //Set codec bool to false
      ResetCodecBool();

      IPin pPin = FileSync ? DirectShowUtil.FindPin(Splitter, PinDirection.Output, format) : DirectShowUtil.FindPin(_interfaceSourceFilter, PinDirection.Output, format);

      if (pPin != null)
      {
        RebuildMediaType(pPin);
        DirectShowUtil.ReleaseComObject(pPin); pPin = null;
      }

      //Detection of Interlaced Video, true for all type except .bdmv .mpls
      if (_mediaInfo.IsInterlaced && (string.Equals(_mediaInfo.VideoCodec, VC1Codec)))
      {
        vc1ICodec = true;
        vc1Codec = false;
      }
      //Detection of VC1 Video if Splitter detection Failed, true for all type except .bdmv .mpls
      else if (string.Equals(_mediaInfo.VideoCodec, VC1Codec))
        vc1Codec = true;
      //Detection of AAC Audio
      if (_mediaInfo.AudioCodec.Contains(AACCodec))
        aacCodec = true;
      if (_mediaInfo.VideoCodec.Equals("AVC"))
        h264Codec = true;
      if (_mediaInfo.VideoCodec.Equals("XVID") || _mediaInfo.VideoCodec.Equals("DIVX") || _mediaInfo.VideoCodec.Equals("DX50"))
        xvidCodec = true;

      //Video Part
      if (format == "Video")
      {
        if (h264Codec)
        {
          return filterConfig.VideoH264;
        }
        else if (vc1ICodec)
        {
          return filterConfig.VideoVC1I;
        }
        else if (vc1Codec)
        {
          return filterConfig.VideoVC1;
        }
        else if (xvidCodec)
        {
          return filterConfig.VideoXVID;
        }
        else
        {
          return filterConfig.Video;
        }
      }
      else if (aacCodec || aacCodecLav)
      {
        return filterConfig.AudioAAC;
      }
      else
      {
        return filterConfig.Audio;
      }
    }

    private bool AutoRendering(bool wmvAudio)
    {
      try
      {
        if (!OSInfo.OSInfo.VistaOrLater())
        {
          // step 1: figure out the renderer of the graph to be removed
          int hr = graphBuilder.RenderFile(m_strCurrentFile, string.Empty);
          IEnumFilters enumFilters;
          hr = graphBuilder.EnumFilters(out enumFilters);
          do
          {
            int ffetched;
            IBaseFilter[] filters = new IBaseFilter[1];
            hr = enumFilters.Next(1, filters, out ffetched);
            if (hr == 0 && ffetched > 0)
            {
              IBasicVideo2 localBasicVideo = filters[0] as IBasicVideo2;
              if (localBasicVideo != null)
              {
                graphBuilder.RemoveFilter(filters[0]);
              }
              DirectShowUtil.ReleaseComObject(filters[0]);
            }
          } while (hr == 0);
          DirectShowUtil.ReleaseComObject(enumFilters);
        }

        // switch back to directx fullscreen mode
        Log.Info("VideoPlayer9: Enabling DX9 exclusive mode");
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 1, 0, null);
        GUIWindowManager.SendMessage(msg);

        // step 2: add the VMR9 in the graph
        // after enabeling exclusive mode, if done first it causes MediPortal to minimize if for example the "Windows key" is pressed while playing a video
        Vmr9 = new VMR9Util();
        Vmr9.AddVMR9(graphBuilder);
        Vmr9.Enable(false);

        // Render file in graph
        if (OSInfo.OSInfo.VistaOrLater())
        {
          int hr = graphBuilder.RenderFile(m_strCurrentFile, string.Empty);
        }

        // render
        DirectShowUtil.RenderGraphBuilderOutputPins(graphBuilder, null);

        if (Vmr9 == null || !Vmr9.IsVMR9Connected)
        {
          Log.Error("VideoPlayer9: Failed to render file -> vmr9");
          mediaCtrl = null;
          Cleanup();
          return false;
        }

        mediaCtrl = (IMediaControl)graphBuilder;
        mediaEvt = (IMediaEventEx)graphBuilder;
        mediaSeek = (IMediaSeeking)graphBuilder;
        mediaPos = (IMediaPosition)graphBuilder;
        basicAudio = (IBasicAudio)graphBuilder;
        videoWin = (IVideoWindow)graphBuilder;
        m_iVideoWidth = Vmr9.VideoWidth;
        m_iVideoHeight = Vmr9.VideoHeight;

        Vmr9.SetDeinterlaceMode();
        return true;
      }
      catch (Exception ex)
      {
        Error.SetError("Unable to play movie", "Unable build graph for VMR9");
        Log.Error("VideoPlayer9: Exception while creating DShow graph {0} {1}", ex.Message, ex.StackTrace);
        Cleanup();
        return false;
      }
    }

    protected override void OnProcess()
    {
      if (Vmr9 != null)
      {
        m_iVideoWidth = Vmr9.VideoWidth;
        m_iVideoHeight = Vmr9.VideoHeight;
      }
    }

    /// <summary> do cleanup and release DirectShow. </summary>
    protected override void CloseInterfaces()
    {
      Cleanup();
    }

    protected void Cleanup()
    {
      if (graphBuilder == null)
      {
        return;
      }
      int hr = 0;
      Log.Info("VideoPlayer9: Cleanup DShow graph");
      try
      {
        if (mediaCtrl != null)
        {
          int counter = 0;
          FilterState state;
          hr = mediaCtrl.Stop();
          hr = mediaCtrl.GetState(10, out state);
          while (state != FilterState.Stopped || GUIGraphicsContext.InVmr9Render)
          {
            Log.Debug("VideoPlayer9: graph still running");
            Thread.Sleep(100);
            hr = mediaCtrl.GetState(10, out state);
            counter++;
            if (counter >= 30)
            {
              if (state != FilterState.Stopped)
                Log.Debug("VideoPlayer9: graph still running");
              if (GUIGraphicsContext.InVmr9Render)
                Log.Debug("VideoPlayer9: in renderer");
              break;
            }
          }
          mediaCtrl = null;
        }

        if (Vmr9 != null)
        {
          Vmr9.Enable(false);
        }

        if (mediaEvt != null)
        {
          hr = mediaEvt.SetNotifyWindow(IntPtr.Zero, WM_GRAPHNOTIFY, IntPtr.Zero);
          mediaEvt = null;
        }

        if (videoWin != null)
        {
          hr = videoWin.put_Visible(OABool.False);
          hr = videoWin.put_Owner(IntPtr.Zero);
          videoWin = null;
        }

        mediaSeek = null;
        mediaPos = null;
        basicAudio = null;
        basicVideo = null;
        SubEngine.GetInstance().FreeSubtitles();
        PostProcessingEngine.GetInstance().FreePostProcess();
        Log.Info("VideoPlayer9: Cleanup Sub/PostProcess");

        #region Cleanup Sebastiii

        if (Splitter != null)
        {
          DirectShowUtil.ReleaseComObject(Splitter);//, 5000);
          Splitter = null;
          Log.Info("VideoPlayer9: Cleanup Splitter");
        }

        if (filterCodec != null && filterCodec.VideoCodec != null)
        {
          DirectShowUtil.ReleaseComObject(filterCodec.VideoCodec);//, 5000);
          filterCodec.VideoCodec = null;
          Log.Info("VideoPlayer9: Cleanup VideoCodec");
        }

        if (filterCodec != null && filterCodec.AudioCodec != null)
        {
          DirectShowUtil.ReleaseComObject(filterCodec.AudioCodec);//, 5000);
          filterCodec.AudioCodec = null;
          Log.Info("VideoPlayer9: Cleanup AudioCodec");
        }

        if (filterCodec != null && filterCodec._audioRendererFilter != null)
        {
          //DirectShowUtil.DisconnectAllPins(graphBuilder, filterCodec._audioRendererFilter);
          //graphBuilder.RemoveFilter(filterCodec._audioRendererFilter);
          //while (DirectShowUtil.ReleaseComObject(filterCodec._audioRendererFilter) > 0) ;
          DirectShowUtil.ReleaseComObject(filterCodec._audioRendererFilter);
          filterCodec._audioRendererFilter = null;
          Log.Info("VideoPlayer9: Cleanup AudioRenderer");
        }

        if (_interfaceSourceFilter != null)
        {
          DirectShowUtil.ReleaseComObject(_interfaceSourceFilter);//, 5000);
          _interfaceSourceFilter = null;
          Log.Info("VideoPlayer9: Cleanup InterfaceSourceFilter");
        }

        //Test to ReleaseComObject from PostProcessFilter list objects.
        foreach (var ppFilter in PostProcessFilterVideo)
        {
          if (ppFilter.Value != null) DirectShowUtil.ReleaseComObject(ppFilter.Value);//, 5000);
        }
        PostProcessFilterVideo.Clear();
        foreach (var ppFilter in PostProcessFilterAudio)
        {
          if (ppFilter.Value != null) DirectShowUtil.ReleaseComObject(ppFilter.Value);//, 5000);
        }
        PostProcessFilterAudio.Clear();
        Log.Info("VideoPlayer9: Cleanup PostProcess");
        foreach (var ppFilter in PostProcessFilterMPAudio)
        {
          if (ppFilter.Value != null) DirectShowUtil.ReleaseComObject(ppFilter.Value);//, 5000);
        }
        PostProcessFilterMPAudio.Clear();
        Log.Info("VideoPlayer9: Cleanup MP Audio Swither");

        if (_FFDShowAudio != null)
        {
          DirectShowUtil.ReleaseComObject(_FFDShowAudio);
          _FFDShowAudio = null;
          Log.Info("VideoPlayer9: Cleanup _FFDShowAudio");
        }
        if (_audioSwitcher != null)
        {
          DirectShowUtil.ReleaseComObject(_audioSwitcher);
          _audioSwitcher = null;
          Log.Info("VideoPlayer9: Cleanup _AudioSwitcher");
        }

        #endregion
        if (graphBuilder != null)
        {
          DirectShowUtil.RemoveFilters(graphBuilder);
          if (_rotEntry != null)
          {
            _rotEntry.SafeDispose();
            _rotEntry = null;
          }
          DirectShowUtil.ReleaseComObject(graphBuilder);
          graphBuilder = null;
          Log.Info("VideoPlayer9: Cleanup Graphbuilder");
        }

        if (Vmr9 != null)
        {
          Vmr9.SafeDispose();
          Vmr9 = null;
        }

        GC.Collect();
        GC.Collect();
        GC.Collect();
        GC.WaitForPendingFinalizers();

        GUIGraphicsContext.form.Invalidate(true);
        m_state = PlayState.Init;
      }
      catch (Exception ex)
      {
        Log.Error("VideoPlayer9: Exception while cleanuping DShow graph - {0} {1}", ex.Message, ex.StackTrace);
      }
      //switch back to directx windowed mode
      Log.Info("VideoPlayer9: Disabling DX9 exclusive mode");
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 0, 0, null);
      GUIWindowManager.SendMessage(msg);
    }
  }
}
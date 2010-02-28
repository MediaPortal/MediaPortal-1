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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using DirectShowLib;
using DShowNET.Helper;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Player.Subtitles;

namespace MediaPortal.Player
{
  public class VideoPlayerVMR9 : VideoPlayerVMR7
  {
    protected VMR9Util Vmr9 = null;

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
    
    /// <summary> create the used COM components and get the interfaces. </summary>
    protected override bool GetInterfaces()
    {
      try
      {
        graphBuilder = (IGraphBuilder)new FilterGraph();
        _rotEntry = new DsROTEntry((IFilterGraph)graphBuilder);
        // add preferred video & audio codecs
        int hr;
        int intFilters = 0; // FlipGer: count custom filters
        string strVideoCodec = "";
        string strH264VideoCodec = "";
        string strAudioCodec = "";
        string strAACAudioCodec = "";
        string strAudiorenderer = "";
        string strFilters = ""; // FlipGer: collect custom filters        
        bool wmvAudio;
        bool autoloadSubtitles;
        bool bAutoDecoderSettings = false;

        using (Settings xmlreader = new MPSettings())
        {
          bAutoDecoderSettings = xmlreader.GetValueAsBool("movieplayer", "autodecodersettings", false);
          strVideoCodec = xmlreader.GetValueAsString("movieplayer", "mpeg2videocodec", "");
          strH264VideoCodec = xmlreader.GetValueAsString("movieplayer", "h264videocodec", "");
          strAudioCodec = xmlreader.GetValueAsString("movieplayer", "mpeg2audiocodec", "");
          strAACAudioCodec = xmlreader.GetValueAsString("movieplayer", "aacaudiocodec", "");
          strAudiorenderer = xmlreader.GetValueAsString("movieplayer", "audiorenderer", "Default DirectSound Device");
          wmvAudio = xmlreader.GetValueAsBool("movieplayer", "wmvaudio", false);
          autoloadSubtitles = xmlreader.GetValueAsBool("subtitles", "enabled", false);
          // FlipGer: load infos for custom filters
          int intCount = 0;
          while (xmlreader.GetValueAsString("movieplayer", "filter" + intCount.ToString(), "undefined") != "undefined")
          {
            if (xmlreader.GetValueAsBool("movieplayer", "usefilter" + intCount.ToString(), false))
            {
              strFilters += xmlreader.GetValueAsString("movieplayer", "filter" + intCount.ToString(), "undefined") + ";";
              intFilters++;
            }
            intCount++;
          }
        }
        
        if (bAutoDecoderSettings)
        {
          return AutoRendering(wmvAudio);
        }

        //Manually add codecs based on file extension if not in auto-settings
        // switch back to directx fullscreen mode
        Log.Info("VideoPlayerVMR9: Enabling DX9 exclusive mode");
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 1, 0, null);
        GUIWindowManager.SendMessage(msg);

        // add the VMR9 in the graph
        // after enabeling exclusive mode, if done first it causes MediPortal to minimize if for example the "Windows key" is pressed while playing a video
        Vmr9 = new VMR9Util();
        Vmr9.AddVMR9(graphBuilder);
        Vmr9.Enable(false);

        IBaseFilter source = null;
        graphBuilder.AddSourceFilter(m_strCurrentFile, null, out source);
        string extension = Path.GetExtension(m_strCurrentFile).ToLower();

        switch (extension)
        {
          case ".dvr-ms":
          case ".mpg":
          case ".mpeg":
          case ".bin":
          case ".dat":          
            {
              strH264VideoCodec = "";
              strAACAudioCodec = "";
              break;
            }
          case ".wmv":
            {
              strVideoCodec = "WMVideo Decoder DMO";
              strH264VideoCodec = "";
              strAudioCodec = "WMAudio Decoder DMO";
              strAACAudioCodec = "";              
              break;
            }
          case ".mkv":
          case ".m2ts":
          case ".mp4":
            {
              strVideoCodec = "";
              break;
            }
        }

        if (!string.IsNullOrEmpty(strVideoCodec))
          DirectShowUtil.AddFilterToGraph(graphBuilder, strVideoCodec);
        if (!string.IsNullOrEmpty(strH264VideoCodec) && strVideoCodec != strH264VideoCodec)
          DirectShowUtil.AddFilterToGraph(graphBuilder, strH264VideoCodec);
        if (!string.IsNullOrEmpty(strAudioCodec))
          DirectShowUtil.AddFilterToGraph(graphBuilder, strAudioCodec);
        if (!string.IsNullOrEmpty(strAACAudioCodec) && strAudioCodec != strAACAudioCodec) 
          DirectShowUtil.AddFilterToGraph(graphBuilder, strAACAudioCodec);

        if (strAudiorenderer.Length > 0)
        {
          DirectShowUtil.AddAudioRendererToGraph(graphBuilder, strAudiorenderer, false);
        }
        //We now add custom filters after the Audio Renderer as AC3Filter failed to connect otherwise.
        //FlipGer: add custom filters to graph        
        string[] arrFilters = strFilters.Split(';');
        for (int i = 0; i < intFilters; i++)
        {
          DirectShowUtil.AddFilterToGraph(graphBuilder, arrFilters[i]);
        }

        //Set High Resolution Output > 2 channels
        IBaseFilter baseFilter = null;
        graphBuilder.FindFilterByName("WMAudio Decoder DMO", out baseFilter);
        if (baseFilter != null && wmvAudio != false) //Also check configuration option enabled
        {
          //Set the filter setting to enable more than 2 audio channels
          const string g_wszWMACHiResOutput = "_HIRESOUTPUT";
          object val = true;
          IPropertyBag propBag = (IPropertyBag)baseFilter;
          hr = propBag.Write(g_wszWMACHiResOutput, ref val);
          if (hr != 0)
          {
            Log.Info("VideoPlayerVMR9: Write failed: g_wszWMACHiResOutput {0}", hr);
          }
          else
          {
            Log.Info("VideoPlayerVMR9: WMAudio Decoder now set for > 2 audio channels");
          }
          DirectShowUtil.ReleaseComObject(baseFilter); baseFilter = null;
        }

        DirectShowUtil.RenderGraphBuilderOutputPins(graphBuilder, source);
        DirectShowUtil.ReleaseComObject(source); source = null;
        DirectShowUtil.RemoveUnusedFiltersFromGraph(graphBuilder);

        #region Subtitles

        SubEngine.GetInstance().LoadSubtitles(graphBuilder, m_strCurrentFile);

        #endregion //Subtitles

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

    private bool AutoRendering(bool wmvAudio)
    {
      try
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

        // switch back to directx fullscreen mode
        Log.Info("VideoPlayerVMR9: Enabling DX9 exclusive mode");
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 1, 0, null);
        GUIWindowManager.SendMessage(msg);

        // step 2: add the VMR9 in the graph
        // after enabeling exclusive mode, if done first it causes MediPortal to minimize if for example the "Windows key" is pressed while playing a video
        Vmr9 = new VMR9Util();
        Vmr9.AddVMR9(graphBuilder);
        Vmr9.Enable(false);

        // render
        DirectShowUtil.RenderGraphBuilderOutputPins(graphBuilder, null);
        #region Subtitles

        SubEngine.GetInstance().LoadSubtitles(graphBuilder, m_strCurrentFile);

        #endregion //Subtitles

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

        if (Vmr9 != null)
        {
          Vmr9.Enable(false);
          Vmr9.Dispose();
          Vmr9 = null;
        }

        if (graphBuilder != null)
        {
          DirectShowUtil.RemoveFilters(graphBuilder);
          if (_rotEntry != null)
          {
            _rotEntry.Dispose();
            _rotEntry = null;
          }
          DirectShowUtil.ReleaseComObject(graphBuilder);
          graphBuilder = null;
        }

        GUIGraphicsContext.form.Invalidate(true);
        m_state = PlayState.Init;
      }
      catch (Exception ex)
      {
        Log.Error("VideoPlayerVMR9: Exception while cleanuping DShow graph - {0} {1}", ex.Message, ex.StackTrace);
      }
      //switch back to directx windowed mode
      Log.Info("VideoPlayerVMR9: Disabling DX9 exclusive mode");
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 0, 0, null);
      GUIWindowManager.SendMessage(msg);
    }
  }
}
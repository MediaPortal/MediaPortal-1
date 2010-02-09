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
        string videoFilterPriority1 = "";
        string videoFilterPriority2 = "";
        string audioFilterPriority1 = "";
        string audioFilterPriority2 = "";        
        bool wmvAudio;
        bool autoloadSubtitles;
        bool bAutoDecoderSettings = false;        
        List<String> videoFilterList = new List<String>();

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

        //Manually add codecs based on file extension if not in auto-settings
        if (!bAutoDecoderSettings)
        {
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

          if (strH264VideoCodec == strVideoCodec)
            strH264VideoCodec = "";
          if (strAACAudioCodec == strAudioCodec)
            strAACAudioCodec = "";

          videoFilterPriority1 = strVideoCodec;
          videoFilterPriority2 = strH264VideoCodec;
          audioFilterPriority1 = strAudioCodec;
          audioFilterPriority2 = strAACAudioCodec;

          switch (extension)
          {
            case ".avi":
              {
                IBaseFilter destination = DirectShowUtil.AddFilterToGraph(graphBuilder, "AVI Splitter");
                IPin pinFrom = DsFindPin.ByDirection(source, PinDirection.Output, 0);
                IPin pinTo = DsFindPin.ByDirection(destination, PinDirection.Input, 0);
                graphBuilder.ConnectDirect(pinFrom, pinTo, null);
                DirectShowUtil.ReleaseComObject(source); source = null;
                source = DirectShowUtil.GetFilterByName(graphBuilder, "AVI Splitter");
                DirectShowUtil.ReleaseComObject(destination); destination = null;
                DirectShowUtil.ReleaseComObject(pinFrom); pinFrom = null;
                DirectShowUtil.ReleaseComObject(pinTo); pinTo = null;
                break;
              }
            case ".wmv":
              {
                videoFilterPriority1 = "WMVideo Decoder DMO";
                videoFilterPriority2 = "";
                audioFilterPriority1 = "WMAudio Decoder DMO";
                audioFilterPriority2 = "";
                break;
              }
            case ".mkv":
            case ".m2ts":
            case ".mp4":
              {
                videoFilterPriority1 = strH264VideoCodec;
                videoFilterPriority2 = strVideoCodec;
                break;
              }
          }

          if (!DirectShowUtil.TryConnect(graphBuilder, source, MediaType.Video, videoFilterPriority1))
            DirectShowUtil.TryConnect(graphBuilder, source, MediaType.Video, videoFilterPriority2);

          if (!DirectShowUtil.TryConnect(graphBuilder, source, MediaType.Audio, audioFilterPriority1) && strAACAudioCodec != strAudioCodec)
            DirectShowUtil.TryConnect(graphBuilder, source, MediaType.Audio, audioFilterPriority2);

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

          DirectShowUtil.RenderGraphBuilderOutputPins(graphBuilder, source);
          DirectShowUtil.ReleaseComObject(source); source = null;
        }
        else
        {
          #region auto
          // in order to retain the same decoding chain than one would get in graphedit when using the DShow filter priorities only, we have
          // to be a bit slick. You'll find out that if you add the renderer first to the graph then render a file you don't end up with
          // the same decoding chain than just rendering the file (which in turn uses VMR7). Usually ffdshow (and maybe other filters which
          // usually handled raw video) don't end up being used.
          // My first attempt at going around that was first render the file, then remove the renderer used by default in the graph and
          // reconnect to the VMR9. Trouble is for an unknown reason when DX exclusive is set, in some occasions it would not connect. 
          // I have no idea why this happens.
          // therefore I went another way: before turning the DX surface to exclusive mode, I render the file, look for the renderer, from
          // there crawl back the video decoding chain storing the name of each filters, back to the source.
          // With that list of filters, I then switch the DX surface to exclusive mode, add the VMR9 renderer along with all the filters
          // I got from my previous parsing, and finally render the file. And I finally get the same video decoding chain I got from graphedit
          // but with VMR9 in exclusive mode now.
          // step 1: figure out the renderer of the graph
          hr = graphBuilder.RenderFile(m_strCurrentFile, string.Empty);
          // then, go over all the filters of the graph to find out the renderer that has been used as a default renderer (depends on merit)
          // from there, go back up the video decoding chain, storing the name of each filters, except the last one (source)
          // this list will be used on the final graph step, to add the VMR9 output and all the filters, and then render the file
          IBaseFilter currentVideoRenderer = null;
          ArrayList ret = new ArrayList();
          IEnumFilters enumFilters;
          int hrFilters = graphBuilder.EnumFilters(out enumFilters);
          do
          {
            int ffetched;
            IBaseFilter[] filters = new IBaseFilter[1];
            hrFilters = enumFilters.Next(1, filters, out ffetched);
            if (hrFilters == 0 && ffetched > 0)
            {
              FilterInfo info;
              filters[0].QueryFilterInfo(out info);
              String sName = info.achName;
              // a renderer can provide a IBasicVideo2 interface - nothing else can (as far as I know)
              // so it's a good way to figure it out
              IBasicVideo2 localBasicVideo = filters[0] as IBasicVideo2;
              if (localBasicVideo != null)
              {
                currentVideoRenderer = filters[0];
              }
              else
              {
                DirectShowUtil.ReleaseComObject(filters[0]);
              }
            }
          } while (hrFilters == 0 && currentVideoRenderer == null);
          DirectShowUtil.ReleaseComObject(enumFilters);
          if (currentVideoRenderer != null)
          {
            // step 2: once the renderer is found, go back up the chain and make the list of decoding steps
            // get the pin that is connected to the current renderer
            IPin iPinSource = DirectShowUtil.FindSourcePinOf(currentVideoRenderer);
            do
            {
              PinInfo outputInfo;
              iPinSource.QueryPinInfo(out outputInfo);
              FilterInfo info;
              outputInfo.filter.QueryFilterInfo(out info);
              DirectShowUtil.ReleaseComObject(iPinSource);
              iPinSource = DirectShowUtil.FindSourcePinOf(outputInfo.filter);
              DirectShowUtil.ReleaseComObject(outputInfo.filter);
              if (iPinSource != null)
              {
                videoFilterList.Add(info.achName);
              }
            } while (iPinSource != null);
            // clear up the graph
            if (graphBuilder != null)
            {
              while ((hr = DirectShowUtil.ReleaseComObject(graphBuilder)) > 0) ;
              graphBuilder = null;
            }
            graphBuilder = (IGraphBuilder)new FilterGraph();
          }
          #endregion

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
        }
        //Check if the WMAudio Decoder DMO filter is in the graph if so set High Resolution Output > 2 channels
        IBaseFilter baseFilter;
        graphBuilder.FindFilterByName("WMAudio Decoder DMO", out baseFilter);
        if (baseFilter != null && wmvAudio != false) //Also check configuration option enabled
        {
          Log.Info("VideoPlayerVMR9: Found WMAudio Decoder DMO");
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
          DirectShowUtil.ReleaseComObject(baseFilter);
        }
        
        if (bAutoDecoderSettings == true)
        {
          // switch back to directx fullscreen mode
          Log.Info("VideoPlayerVMR9: Enabling DX9 exclusive mode");
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 1, 0, null);
          GUIWindowManager.SendMessage(msg);

          // add the VMR9 in the graph
          // after enabeling exclusive mode, if done first it causes MediPortal to minimize if for example the "Windows key" is pressed while playing a video
          Vmr9 = new VMR9Util();
          Vmr9.AddVMR9(graphBuilder);
          Vmr9.Enable(false);

          // step 3: add the filters we got from step 2, and render the file
          // if for some reason the parsing failed, we end up doing exactly the same than as usual, ie VMR9 + render.
          
          foreach (string sFilter in videoFilterList)
          {
            DirectShowUtil.AddFilterToGraph(graphBuilder, sFilter);            
          }
          // render
          hr = graphBuilder.RenderFile(m_strCurrentFile, string.Empty);
        }
        
        if (Vmr9 == null)
        {
          Error.SetError("Unable to play movie", "Unable to render file. Missing codecs?");
          Log.Error("VideoPlayer9: Failed to render file -> vmr9");
          Cleanup();
          return false;
        }
        mediaCtrl = (IMediaControl)graphBuilder;
        mediaEvt = (IMediaEventEx)graphBuilder;
        mediaSeek = (IMediaSeeking)graphBuilder;
        mediaPos = (IMediaPosition)graphBuilder;
        basicAudio = graphBuilder as IBasicAudio;
        DirectShowUtil.EnableDeInterlace(graphBuilder);
        m_iVideoWidth = Vmr9.VideoWidth;
        m_iVideoHeight = Vmr9.VideoHeight;

        #region Subtitles

        SubEngine.GetInstance().LoadSubtitles(graphBuilder, m_strCurrentFile);

        #endregion //Subtitles

        if (!Vmr9.IsVMR9Connected)
        {
          //VMR9 is not supported, switch to overlay
          mediaCtrl = null;
          Cleanup();
          return false;
        }
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

        videoWin = graphBuilder as IVideoWindow;
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
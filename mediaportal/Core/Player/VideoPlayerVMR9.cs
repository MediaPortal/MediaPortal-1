#region Copyright (C) 2005-2008 Team MediaPortal

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

#endregion

using System;
using System.Windows.Forms;
using System.Collections;
using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using DirectShowLib;
using DShowNET.Helper;
using MediaPortal.Configuration;
using System.Threading;
using System.Collections.Generic;

namespace MediaPortal.Player
{
  public class VideoPlayerVMR9 : VideoPlayerVMR7
  {
    VMR9Util Vmr9 = null;
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
      DsRect rect = new DsRect();
      rect.top = 0;
      rect.bottom = GUIGraphicsContext.form.Height;
      rect.left = 0;
      rect.right = GUIGraphicsContext.form.Width;
      try
      {
        graphBuilder = (IGraphBuilder)new FilterGraph();
        // add preferred video & audio codecs
        int hr;
        bool bAutoDecoderSettings = false;
        string strVideoCodec = "";
        string strH264VideoCodec = "";
        string strAudioCodec = "";
        string strAACAudioCodec = "";
        string strAudiorenderer = "";
        int intFilters = 0; // FlipGer: count custom filters
        string strFilters = ""; // FlipGer: collect custom filters
        bool wmvAudio;
        bool useVobSub;
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          bAutoDecoderSettings = xmlreader.GetValueAsBool("movieplayer", "autodecodersettings", false);
          strVideoCodec = xmlreader.GetValueAsString("movieplayer", "mpeg2videocodec", "");
          strH264VideoCodec = xmlreader.GetValueAsString("movieplayer", "h264videocodec", "");
          strAudioCodec = xmlreader.GetValueAsString("movieplayer", "mpeg2audiocodec", "");
          strAACAudioCodec = xmlreader.GetValueAsString("movieplayer", "aacaudiocodec", "");
          strAudiorenderer = xmlreader.GetValueAsString("movieplayer", "audiorenderer", "Default DirectSound Device");
          wmvAudio = xmlreader.GetValueAsBool("movieplayer", "wmvaudio", false);
          useVobSub = xmlreader.GetValueAsBool("subtitles", "enabled", false);
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
        List<String> videoFilterList = new List<String>();
        //Manually add codecs based on file extension if not in auto-settings
        if (bAutoDecoderSettings == false)
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

          string extension = System.IO.Path.GetExtension(m_strCurrentFile).ToLower();
          if (extension.Equals(".dvr-ms") || extension.Equals(".mpg") || extension.Equals(".mpeg") || extension.Equals(".bin") || extension.Equals(".dat"))
          {
            if (strVideoCodec.Length > 0) videoCodecFilter = DirectShowUtil.AddFilterToGraph(graphBuilder, strVideoCodec);
            if (strAudioCodec.Length > 0) audioCodecFilter = DirectShowUtil.AddFilterToGraph(graphBuilder, strAudioCodec);
          }
          if (extension.Equals(".wmv"))
          {
            videoCodecFilter = DirectShowUtil.AddFilterToGraph(graphBuilder, "WMVideo Decoder DMO");
            audioCodecFilter = DirectShowUtil.AddFilterToGraph(graphBuilder, "WMAudio Decoder DMO");
          }
          if (extension.Equals(".mp4") || extension.Equals(".mkv"))
          {
            if (strH264VideoCodec.Length > 0) h264videoCodecFilter = DirectShowUtil.AddFilterToGraph(graphBuilder, strH264VideoCodec);
            if (strAACAudioCodec.Length > 0) aacaudioCodecFilter = DirectShowUtil.AddFilterToGraph(graphBuilder, strAACAudioCodec);
          }
        }
        else
        {
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
                DirectShowUtil.ReleaseComObject(filters[0]);
            }
          }
          while (hrFilters == 0 && currentVideoRenderer == null);
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
                videoFilterList.Add(info.achName);
            }
            while (iPinSource != null);
            // clear up the graph
            if (graphBuilder != null)
            {
              while ((hr = DirectShowUtil.ReleaseComObject(graphBuilder)) > 0) ;
              graphBuilder = null;
            }
            graphBuilder = (IGraphBuilder)new FilterGraph();
            // switch back to directx fullscreen mode
            Log.Info("VideoPlayerVMR9: Enabling DX9 exclusive mode");
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 1, 0, null);
            GUIWindowManager.SendMessage(msg);
          }
        }
        if (strAudiorenderer.Length > 0) audioRendererFilter = DirectShowUtil.AddAudioRendererToGraph(graphBuilder, strAudiorenderer, false);
        //We now add custom filters after the Audio Renderer as AC3Filter failed to connect otherwise.
        //FlipGer: add custom filters to graph
        customFilters = new IBaseFilter[intFilters];
        string[] arrFilters = strFilters.Split(';');
        for (int i = 0; i < intFilters; i++)
        {
          customFilters[i] = DirectShowUtil.AddFilterToGraph(graphBuilder, arrFilters[i]);
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
          // step 3: add the VMR9 renderer, along with the filters we got from step 2, and render the file
          // if for some reason the parsing failed, we end up doing exactly the same than as usual, ie VMR9 + render.
          // add the VMR9 in the graph
          Vmr9 = new VMR9Util();
          Vmr9.AddVMR9(graphBuilder);
          Vmr9.Enable(false);
          foreach (string sFilter in videoFilterList)
          {
            IBaseFilter newFilter = DirectShowUtil.AddFilterToGraph(graphBuilder, sFilter);
            DirectShowUtil.ReleaseComObject(newFilter);
          }
          // render
          hr = graphBuilder.RenderFile(m_strCurrentFile, string.Empty);
        }
        else
        {
          // render
          hr = graphBuilder.RenderFile(m_strCurrentFile, string.Empty);
        }
        if (Vmr9 == null)
        {
          Error.SetError("Unable to play movie", "Unable to render file. Missing codecs?");
          Log.Error("VideoPlayer9: Failed to render file -> vmr9");
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
        ushort b;
        unchecked
        {
          b = (ushort)0xfffff845;
        }

        #region VobSub
        // If Vobsub filter is loaded into the graph (either automatically or by adding as
        // 'postprocessing filter' then configure it using settings stored in mediaportal.xml
        if (vob != null) //Release old vobsub com object, if somehow a previous instance exists.
        {
          Log.Info("VideoPlayerVMR9: release vob sub filter");
          DirectShowUtil.ReleaseComObject(vob);
          vob = null;
        }
        //Find VobSub filter instance.
        //Try the "autoload" filter first.
        Guid classID = new Guid("9852A670-F845-491B-9BE6-EBD841B8A613");
        DirectShowUtil.FindFilterByClassID(graphBuilder, classID, out vob);
        vobSub = null;
        vobSub = (IDirectVobSub)vob;
        if (vobSub == null)
        {
          //Try the "normal" filter then.
          classID = new Guid("93A22E7A-5091-45ef-BA61-6DA26156A5D0");
          //Log.Info("VideoPlayerVMR9: add normal vob sub filter");
          DirectShowUtil.FindFilterByClassID(graphBuilder, classID, out vob);
          vobSub = (IDirectVobSub)vob;
        }
        //if the directvobsub filter has not been added to the graph. (i.e. with evr)
        //we add a bit more intelligence to determine if subtitles are enabled.
        //and if subtitles are present for the video / movie then we add it if necessary to the graph.
        if (vobSub == null)
        {
          Log.Info("VideoPlayerVMR9: no vob sub filter in the current graph");
          //the filter has not been added lets check if it should be added or not.
          if (useVobSub != false)
          {
            Log.Info("VideoPlayerVMR9: subtitles enabled - checking if subtitles are present");
            //check if a subtitle extension exists
            bool subsPresent = false;
            string look4sub = System.IO.Path.ChangeExtension(m_strCurrentFile, null).ToLower();
            if (System.IO.File.Exists(look4sub + ".srt") || System.IO.File.Exists(look4sub + ".sub"))
            {
              subsPresent = true;
            }
            if (!subsPresent)
            {
              Log.Info("VideoPlayerVMR9: no compatible subtitles found");
            }
            else
            {
              //add the filter to the graph
              Log.Info("VideoPlayerVMR9: subtitles present adding DirectVobSub filter to the current graph");
              IBaseFilter directvobsub = DirectShowUtil.AddFilterToGraph(graphBuilder, "DirectVobSub");
              if (directvobsub == null)
              {
                Log.Info("VideoPlayerVMR9: DirectVobSub filter not found! You need to install DirectVobSub v2.39");
                vobSub = null;
              }
              classID = new Guid("93A22E7A-5091-45ef-BA61-6DA26156A5D0");
              Log.Info("VideoPlayerVMR9: add normal vob sub filter");
              DirectShowUtil.FindFilterByClassID(graphBuilder, classID, out vob);
              vobSub = (IDirectVobSub)vob;
            }
          }
          else
          {
            Log.Info("VideoPlayerVMR9: subtitles are not enabled");
          }
        }
        if (vobSub != null)
        {
          using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
          {
            if (useVobSub != false)
            {
              Log.Info("VideoPlayerVMR9: Setting DirectVobsub parameters");
              //string defaultLanguage;
              string strTmp = "";
              string strFont = xmlreader.GetValueAsString("subtitles", "fontface", "Arial");
              int iFontSize = xmlreader.GetValueAsInt("subtitles", "fontsize", 18);
              bool bBold = xmlreader.GetValueAsBool("subtitles", "bold", true);
              //defaultLanguage = xmlreader.GetValueAsString("subtitles", "language", "English");
              strTmp = xmlreader.GetValueAsString("subtitles", "color", "ffffff");
              long iColor = Convert.ToInt64(strTmp, 16);
              int iShadow = xmlreader.GetValueAsInt("subtitles", "shadow", 5);
              LOGFONT logFont = new LOGFONT();
              int txtcolor;
              bool fShadow, fOutLine, fAdvancedRenderer = false;
              int size = Marshal.SizeOf(typeof(LOGFONT));
              vobSub.get_TextSettings(logFont, size, out txtcolor, out fShadow, out fOutLine, out fAdvancedRenderer);
              FontStyle fontStyle = FontStyle.Regular;
              if (bBold) fontStyle = FontStyle.Bold;
              System.Drawing.Font Subfont = new System.Drawing.Font(strFont, iFontSize, fontStyle, System.Drawing.GraphicsUnit.Point, 1);
              Subfont.ToLogFont(logFont);
              int R = (int)((iColor >> 16) & 0xff);
              int G = (int)((iColor >> 8) & 0xff);
              int B = (int)((iColor) & 0xff);
              txtcolor = (B << 16) + (G << 8) + R;
              if (iShadow > 0) fShadow = true;
              vobSub.put_TextSettings(logFont, size, txtcolor, fShadow, fOutLine, fAdvancedRenderer);
              // Now check if vobsub's video input is not connected.
              // Check only if vmr9 is connected (render was successful).
              if (Vmr9.IsVMR9Connected)
              {
                IPin pinVideoIn = DsFindPin.ByDirection(vob, PinDirection.Input, 0);
                // Check if video input pin is connected
                IPin pinVideoTo = null;
                pinVideoIn.ConnectedTo(out pinVideoTo);
                if (hr != 0 || pinVideoTo == null)
                {
                  // Pin is not connected. Connect it.
                  Log.Info("VideoPlayerVMR9: Connect vobsub's video pins!");
                  // This is the pin that we will connect to vobsub's input.
                  pinVideoTo = Vmr9.PinConnectedTo;
                  // We have to re-add and re-initialize vmr9 as we cannot connect to it once it has been connected to
                  Vmr9.Dispose();
                  // Just in any case...
                  pinVideoTo.Disconnect();
                  //Now force connection to vobsub
                  hr = graphBuilder.Connect(pinVideoTo, pinVideoIn);
                  if (hr != 0)
                  {
                    Log.Info("VideoPlayerVMR9: could not connect Vobsub's input video pin...");
                    return false;
                  }
                  Log.Info("VideoPlayerVMR9: Vobsub's video input pin connected...");
                  DirectShowUtil.ReleaseComObject(pinVideoTo);
                  //Add vmr9 again
                  Vmr9.AddVMR9(graphBuilder);
                  Vmr9.Enable(false);
                  // Now render vobsub's video output pin.
                  pinVideoTo = DirectShowUtil.FindPin(vob, PinDirection.Output, "Output");
                  if (pinVideoTo == null)
                  {
                    Log.Info("VideoPlayerVMR9: Vobsub output pin NOT FOUND!");
                    return false;
                  }
                  hr = graphBuilder.Render(pinVideoTo);
                  if (hr != 0)
                  {
                    Log.Info("VideoPlayerVMR9: could not connect Vobsub to Vmr9 Renderer");
                    return false;
                  }
                  Log.Info("VideoPlayerVMR9: Vobsub connected to Vmr9 Renderer...");
                }
                else DirectShowUtil.ReleaseComObject(pinVideoTo);
                DirectShowUtil.ReleaseComObject(pinVideoIn);
                // Query VobSub's subtitle input pin (first one).
                IPin pinSubIn = DirectShowUtil.FindPin(vob, PinDirection.Input, "Input");
                if (pinSubIn != null)
                {
                  // Check if subtitle input pin is connected
                  IPin pinSubTo = null;
                  pinSubIn.ConnectedTo(out pinSubTo);
                  if (hr != 0 || pinSubTo == null)
                  {
                    // Not connected.
                    // Check if Haali Media Splitter is in the graph.
                    Guid hmsclassID = new Guid("55DA30FC-F16B-49FC-BAA5-AE59FC65F82D"); //Haali
                    IBaseFilter hms = null;
                    DirectShowUtil.FindFilterByClassID(graphBuilder, hmsclassID, out hms);
                    if (hms != null)
                    {
                      // It is. Connect it' subtitle output pin (if any) to Vobsub's subtitle input.
                      Log.Info("VideoPlayerVMR9: Connecting Haali's subtitle output to Vobsub's input.");
                      pinSubTo = DirectShowUtil.FindPin(hms, PinDirection.Output, "Subtitle");
                      if (pinSubTo != null)
                      {
                        // Disconnect Haali's output if connected.
                        IPin pinSubToConnectedTo = null;
                        pinSubTo.ConnectedTo(out pinSubToConnectedTo);
                        if (pinSubToConnectedTo != null)
                        {
                          pinSubTo.Disconnect();
                          DirectShowUtil.ReleaseComObject(pinSubToConnectedTo);
                        }
                        // Now, connect Haali and Vobsub.
                        hr = graphBuilder.ConnectDirect(pinSubTo, pinSubIn, null);
                        if (hr != 0) Log.Info("VideoPlayerVMR9: Haali - Vobsub connect failed: {0}", hr);
                        DirectShowUtil.ReleaseComObject(pinSubTo);
                      }
                      DirectShowUtil.ReleaseComObject(hms);
                    }
                  }
                  else DirectShowUtil.ReleaseComObject(pinSubTo);
                  DirectShowUtil.ReleaseComObject(pinSubIn);
                }
                // Force vobsub to reload available subtitles.
                // This is needed if added as postprocessing filter.
                vobSub.put_FileName(m_strCurrentFile);
              }
            }
            //subtitles are not enabled, remove DirectVobSub from the graph & reconnect accordingly.
            else
            {
              Log.Info("VideoPlayerVMR9: Subtitles are disabled but DirectVobSub is in the graph. Removing it accordingly");
              // Check if video input pin is connected
              // If not just remove the DirectVobSub filter.
              IPin pinVideoIn = DsFindPin.ByDirection(vob, PinDirection.Input, 0);
              IPin pinInputIn = DsFindPin.ByDirection(vob, PinDirection.Input, 1);
              //find directvobsub's video input pin source output pin
              IPin pinVideoFrom = null;
              hr = pinVideoIn.ConnectedTo(out pinVideoFrom);
              //find DirectVobSub's subtitle input source output pin
              IPin pinSubtitleFrom = null;
              hr = pinInputIn.ConnectedTo(out pinSubtitleFrom);
              PinInfo pininfo;
              if (hr != 0 || pinVideoFrom == null)
              {
                //video input pin is not connected
                Log.Info("VideoPlayerVMR9: DirectVobSub not connected, removing...");
                //first check if the subtitle pin is connected (i.e. mkv's), if so disconnect
                if (pinSubtitleFrom != null)
                {
                  pinSubtitleFrom.QueryPinInfo(out pininfo);
                  hr = pinSubtitleFrom.Disconnect();
                  if (hr != 0)
                  {
                    Log.Info("VideoPlayerVMR9: DirectVobSub failed disconnecting source subtitle output pin {0}", pininfo.name);
                  }
                }
                graphBuilder.RemoveFilter(vob);
                while ((hr = DirectShowUtil.ReleaseComObject(vobSub)) > 0) ;
                vobSub = null;
                while ((hr = DirectShowUtil.ReleaseComObject(vob)) > 0) ;
                vob = null;
              }
              else
              {
                //video pin connected, disconnect it.
                //also disconnect the subtitle input pin & output pin.
                pinVideoFrom.QueryPinInfo(out pininfo);
                Log.Info("VideoPlayerVMR9: DirectVobSub connected, removing...");
                hr = pinVideoFrom.Disconnect();
                if (hr != 0)
                {
                  Log.Info("VideoPlayerVMR9: DirectVobSub failed disconnecting source video output pin: {0}", pininfo.name);
                }
                //check if the subtitle pin is connected also (mkv's), if so disconnect
                if (pinSubtitleFrom != null)
                {
                  pinSubtitleFrom.QueryPinInfo(out pininfo);
                  hr = pinSubtitleFrom.Disconnect();
                  if (hr != 0)
                  {
                    Log.Info("VideoPlayerVMR9: DirectVobSub failed disconnecting source subtitle output pin {0}", pininfo.name);
                  }
                  DirectShowUtil.ReleaseComObject(pinInputIn);
                  DirectShowUtil.ReleaseComObject(pinSubtitleFrom);
                }
                DirectShowUtil.ReleaseComObject(pinVideoIn);
                //remove vmr9 filter so it can be re-initialized later
                Vmr9.Dispose();
                //remove the DirectVobSub filter from the graph
                graphBuilder.RemoveFilter(vob);
                while ((hr = DirectShowUtil.ReleaseComObject(vobSub)) > 0) ;
                vobSub = null;
                while ((hr = DirectShowUtil.ReleaseComObject(vob)) > 0) ;
                vob = null;
                //Add vmr9 again
                Vmr9.AddVMR9(graphBuilder);
                Vmr9.Enable(false);
                if (pinVideoFrom == null)
                {
                  Log.Info("VideoPlayerVMR9: Source output pin NOT FOUND!");
                  return false;
                }
                //reconnect the source output pin to the vmr9/evr filter
                hr = graphBuilder.Render(pinVideoFrom);
                if (hr != 0)
                {
                  Log.Info("VideoPlayerVMR9: Could not connect video out to video renderer: {0}", hr);
                  return false;
                }
                Log.Info("VideoPlayerVMR9: Video out connected to video renderer...");
                DirectShowUtil.ReleaseComObject(pinVideoFrom);
              }
            }
          }
        }
        #endregion //Vobsub

        if (!Vmr9.IsVMR9Connected)
        {
          //VMR9 is not supported, switch to overlay
          mediaCtrl = null;
          Cleanup();
          // return base.GetInterfaces();
          return false;
        }
        Vmr9.SetDeinterlaceMode();
        return true;
      }
      catch (Exception ex)
      {
        Error.SetError("Unable to play movie", "Unable build graph for VMR9");
        Log.Error("VideoPlayer9:exception while creating DShow graph {0} {1}", ex.Message, ex.StackTrace);
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

    void Cleanup()
    {
      if (graphBuilder == null) return;
      int hr;
      Log.Info("VideoPlayer9:cleanup DShow graph");
      try
      {
        videoWin = graphBuilder as IVideoWindow;
        if (videoWin != null)
          videoWin.put_Visible(OABool.False);
        if (Vmr9 != null)
        {
          Vmr9.Enable(false);
        }
        if (mediaCtrl != null)
        {
          int counter = 0;
          while (GUIGraphicsContext.InVmr9Render)
          {
            counter++;
            System.Threading.Thread.Sleep(100);
            if (counter > 100) break;
          }
          hr = mediaCtrl.Stop();
          FilterState state;
          hr = mediaCtrl.GetState(10, out state);
          Log.Info("state:{0} {1:X}", state.ToString(), hr);
          mediaCtrl = null;
        }
        mediaEvt = null;
        if (Vmr9 != null)
        {
          Vmr9.Dispose();
          Vmr9 = null;
        }
        mediaSeek = null;
        mediaPos = null;
        basicAudio = null;
        basicVideo = null;
        videoWin = null;
        if (videoCodecFilter != null)
        {
          while (DirectShowUtil.ReleaseComObject(videoCodecFilter) > 0) ;
          videoCodecFilter = null;
        }
        if (h264videoCodecFilter != null)
        {
          while (DirectShowUtil.ReleaseComObject(h264videoCodecFilter) > 0) ;
          h264videoCodecFilter = null;
        }
        if (audioCodecFilter != null)
        {
          while (DirectShowUtil.ReleaseComObject(audioCodecFilter) > 0) ;
          audioCodecFilter = null;
        }
        if (aacaudioCodecFilter != null)
        {
          while (DirectShowUtil.ReleaseComObject(aacaudioCodecFilter) > 0) ;
          aacaudioCodecFilter = null;
        }
        if (audioRendererFilter != null)
        {
          while (DirectShowUtil.ReleaseComObject(audioRendererFilter) > 0) ;
          audioRendererFilter = null;
        }
        // FlipGer: release custom filters
        for (int i = 0; i < customFilters.Length; i++)
        {
          if (customFilters[i] != null)
          {
            while ((hr = DirectShowUtil.ReleaseComObject(customFilters[i])) > 0) ;
          }
          customFilters[i] = null;
        }
        if (vobSub != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(vobSub)) > 0) ;
          vobSub = null;
        }
        if (vob != null)
        {
          DirectShowUtil.ReleaseComObject(vob);
          vob = null;
        }
        //	DsUtils.RemoveFilters(graphBuilder);
        if (_rotEntry != null)
        {
          _rotEntry.Dispose();
        }
        _rotEntry = null;
        if (graphBuilder != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(graphBuilder)) > 0) ;
          graphBuilder = null;
        }
        GUIGraphicsContext.form.Invalidate(true);
        m_state = PlayState.Init;

        GC.Collect();
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





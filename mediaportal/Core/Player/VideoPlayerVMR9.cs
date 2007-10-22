#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
      Vmr9 = new VMR9Util();
      // switch back to directx fullscreen mode
      Log.Info("VideoPlayerVMR9: Enabling DX9 exclusive mode");
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 1, 0, null);
      GUIWindowManager.SendMessage(msg);
      DsRect rect = new DsRect();
      rect.top = 0;
      rect.bottom = GUIGraphicsContext.form.Height;
      rect.left = 0;
      rect.right = GUIGraphicsContext.form.Width;
      try
      {
        graphBuilder = (IGraphBuilder)new FilterGraph();
        Vmr9.AddVMR9(graphBuilder);
        Vmr9.Enable(false);
        // add preferred video & audio codecs
        string strVideoCodec = "";
        string strH264VideoCodec = "";
        string strAudioCodec = "";
        string strAudiorenderer = "";
        int intFilters = 0; // FlipGer: count custom filters
        string strFilters = ""; // FlipGer: collect custom filters
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          strVideoCodec = xmlreader.GetValueAsString("movieplayer", "mpeg2videocodec", "");
          strH264VideoCodec = xmlreader.GetValueAsString("movieplayer", "h264videocodec", "");
          strAudioCodec = xmlreader.GetValueAsString("movieplayer", "mpeg2audiocodec", "");
          strAudiorenderer = xmlreader.GetValueAsString("movieplayer", "audiorenderer", "Default DirectSound Device");
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
        //Manually add codecs based on file extension
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
          if (strAudioCodec.Length > 0) audioCodecFilter = DirectShowUtil.AddFilterToGraph(graphBuilder, strAudioCodec);
        }
        // FlipGer: add custom filters to graph
        customFilters = new IBaseFilter[intFilters];
        string[] arrFilters = strFilters.Split(';');
        for (int i = 0; i < intFilters; i++)
        {
          customFilters[i] = DirectShowUtil.AddFilterToGraph(graphBuilder, arrFilters[i]);
        }
        if (strAudiorenderer.Length > 0) audioRendererFilter = DirectShowUtil.AddAudioRendererToGraph(graphBuilder, strAudiorenderer, false);
        //Check if the WMAudio Decoder DMO filter is in the graph if so set High Resolution Output > 2 channels
        IBaseFilter baseFilter;
        graphBuilder.FindFilterByName("WMAudio Decoder DMO", out baseFilter);
        int hr;
        if (baseFilter != null)
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
          Marshal.ReleaseComObject(baseFilter);
        }
        hr = graphBuilder.RenderFile(m_strCurrentFile, string.Empty);
        if (hr != 0)
        {
          Error.SetError("Unable to play movie", "Unable to render file. Missing codecs?");
          Log.Error("VideoPlayer9: Failed to render file -> vmr9");
          return false;
        }
        //Use below if some file formats don't play
        //graphBuilder.RenderFile(m_strCurrentFile, string.Empty);

        //Now we check if AC3 Filter is in the graph as a post processing filter with extension .wmv
        //If so we now force WMAudio to connect to the AC3Filter as by default is does not connect
        IBaseFilter ac3Filter;
        graphBuilder.FindFilterByName("AC3Filter", out ac3Filter);
        if (ac3Filter != null & extension.Equals(".wmv"))
        {
          try
          {
            Log.Info("VideoPlayerVMR9: AC3Filter & extension = *.wmv");
            //check if AC3Filter's input pin is not connected
            IPin pinIn, pinConnected;
            pinIn = DsFindPin.ByDirection(ac3Filter, PinDirection.Input, 0);
            //check if the input is connected to an audio decoder
            pinIn.ConnectedTo(out pinConnected);
            if (pinConnected != null)
            {
              //pin is connected so proceed is not possible
              Marshal.ReleaseComObject(pinIn);
              Log.Info("VideoPlayerVMR9: AC3Filter already connected!");
              return false;
            }
            Log.Info("VideoPlayerVMR9: AC3Filter not connected, continue...");
            Marshal.ReleaseComObject(pinIn);
            // We have to remove the audio renderer as we cannot connect to it afterwards once it has been connect too
            graphBuilder.RemoveFilter(audioRendererFilter);
            //Here we find out output & input pins of both audio filters
            IPin sourcePin = null;
            IPin sinkPin = null;
            sourcePin = DirectShowUtil.FindPin(audioCodecFilter, PinDirection.Output, "out0");
            sinkPin = DirectShowUtil.FindPin(ac3Filter, PinDirection.Input, "In");
            if (sourcePin != null && sinkPin != null)
            {
              Log.Info("VideoPlayerVMR9: sinkPin & sourcePin found");
              //Disconnect the WMAudio Decoder filter from the DirectSound Renderer
              graphBuilder.Disconnect(sourcePin);
            }
            else
            {
              Log.Info("VideoPlayerVMR9: sinkPin & sourcePin NOT found");
              Marshal.ReleaseComObject(sourcePin);
              Marshal.ReleaseComObject(sinkPin);
              return false;
            }
            //Now force the connection to AC3 Filter
            hr = graphBuilder.Connect(sourcePin, sinkPin);
            if (hr != 0)
            {
              Log.Info("VideoPlayerVMR9: could not connect WMAudio to AC3Filter...");
              return false;
            }
            Log.Info("VideoPlayerVMR9: WMAudio connected to AC3Filter...");
            Marshal.ReleaseComObject(sourcePin);
            Marshal.ReleaseComObject(sinkPin);
            //Then re-connect the AC3 Filter output to Audio renderer
            IPin ac3OutPin = null;
            IPin dsInPin = null;
            ac3OutPin = DirectShowUtil.FindPin(ac3Filter, PinDirection.Output, "Out");
            if (ac3OutPin != null)
            {
              //We now re-add the audio renderer...
              if (strAudiorenderer.Length > 0) audioRendererFilter = DirectShowUtil.AddAudioRendererToGraph(graphBuilder, strAudiorenderer, false);
              // And now render from the AC3Filter....
              hr = graphBuilder.Render(ac3OutPin);
              if (hr != 0)
              {
                Log.Info("VideoPlayerVMR9: could not connect AC3Filter to Audio Renderer");
                return false;
              }
              Log.Info("VideoPlayerVMR9: AC3Filter connected to Audio Renderer...");
            }
            else
              Log.Info("VideoPlayerVMR9: ac3OutPin NOT FOUND!");
            //We are successful now we release the remaining resources
            Marshal.ReleaseComObject(ac3OutPin);
            Marshal.ReleaseComObject(dsInPin);
          }
          catch (Exception ex)
          {
            //_lastError = String.Format("Unable to create graph");
            Log.Error(ex);
          }
          Marshal.ReleaseComObject(ac3Filter);
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
        Guid classID = new Guid(0x9852a670, b, 0x491b, 0x9b, 0xe6, 0xeb, 0xd8, 0x41, 0xb8, 0xa6, 0x13);
        if (vob != null)
        {
          Marshal.ReleaseComObject(vob);
          vob = null;
        }
        DirectShowUtil.FindFilterByClassID(graphBuilder, classID, out vob);
        vobSub = null;
        vobSub = (IDirectVobSub)vob;
        if (vobSub != null)
        {
          //string defaultLanguage;
          using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
          {
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
            int res = vobSub.put_TextSettings(logFont, size, txtcolor, fShadow, fOutLine, fAdvancedRenderer);
          }
        }
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
            System.Threading.Thread.Sleep(1);
            if (counter > 200) break;
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
          while (Marshal.ReleaseComObject(videoCodecFilter) > 0) ;
          videoCodecFilter = null;
        }
        if (h264videoCodecFilter != null)
        {
          while (Marshal.ReleaseComObject(videoCodecFilter) > 0) ;
          h264videoCodecFilter = null;
        }
        if (audioCodecFilter != null)
        {
          while (Marshal.ReleaseComObject(audioCodecFilter) > 0) ;
          audioCodecFilter = null;
        }
        if (audioRendererFilter != null)
        {
          while (Marshal.ReleaseComObject(audioRendererFilter) > 0) ;
          audioRendererFilter = null;
        }
        // FlipGer: release custom filters
        for (int i = 0; i < customFilters.Length; i++)
        {
          if (customFilters[i] != null)
          {
            while ((hr = Marshal.ReleaseComObject(customFilters[i])) > 0) ;
          }
          customFilters[i] = null;
        }
        if (vobSub != null)
        {
          while ((hr = Marshal.ReleaseComObject(vobSub)) > 0) ;
          vobSub = null;
        }
        if (vob != null) Marshal.ReleaseComObject(vob); vob = null;
        //	DsUtils.RemoveFilters(graphBuilder);
        if (_rotEntry != null)
        {
          _rotEntry.Dispose();
        }
        _rotEntry = null;
        if (graphBuilder != null)
        {
          while ((hr = Marshal.ReleaseComObject(graphBuilder)) > 0) ;
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
      if (!GUIGraphicsContext.IsTvWindow(GUIWindowManager.ActiveWindow))
      {
        Log.Info("VideoPlayerVMR9: Disabling DX9 exclusive mode");
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 0, 0, null);
        GUIWindowManager.SendMessage(msg);
      }
    }
  }
}
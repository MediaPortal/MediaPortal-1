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
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using DirectShowLib;
using MediaPortal.GUI.Library;
using DShowNET.Helper;
using MediaPortal.Profile;
using FFDShow;
using FFDShow.Interfaces;
using MediaPortal.Player.PostProcessing;

namespace MediaPortal.Player.Subtitles
{
  public class FFDShowEngine : SubSettings, ISubEngine, IPostProcessingEngine
  {
    private FFDShowAPI ffdshowAPI;
    private bool hasPostProcessing = false;
    protected int audiodelayInterval;

    public static void DisableFFDShowSubtitles(IGraphBuilder graphBuilder)
    {
      // no instance of engine yet created or no ffdshow api, try to find it
      IBaseFilter baseFilter = null;
      DirectShowUtil.FindFilterByClassID(graphBuilder, FFDShowAPI.FFDShowVideoGuid, out baseFilter);
      if (baseFilter == null)
        DirectShowUtil.FindFilterByClassID(graphBuilder, FFDShowAPI.FFDShowVideoDXVAGuid, out baseFilter);
      if (baseFilter == null)
        DirectShowUtil.FindFilterByClassID(graphBuilder, FFDShowAPI.FFDShowVideoRawGuid, out baseFilter);

      if (baseFilter != null)
      {
        IffdshowDec ffdshowDec = baseFilter as IffdshowDec;
        if (ffdshowDec != null)
        {
          // use a temporary instance of the API, as it is only used here, to disable subs
          FFDShowAPI tempffdshowAPI = new FFDShowAPI((object)baseFilter);
          tempffdshowAPI.DoShowSubtitles = false;
          Log.Info("FFDshow interfaces found -> Subtitles disabled");
          tempffdshowAPI.Dispose();
        }
        else
        {
          DirectShowUtil.ReleaseComObject(baseFilter);
        }
      }
    }

    public static void EnableFFDShowSubtitles(IGraphBuilder graphBuilder)
    {
      // no instance of engine yet created or no ffdshow api, try to find it
      IBaseFilter baseFilter = null;
      DirectShowUtil.FindFilterByClassID(graphBuilder, FFDShowAPI.FFDShowVideoGuid, out baseFilter);
      if (baseFilter == null)
        DirectShowUtil.FindFilterByClassID(graphBuilder, FFDShowAPI.FFDShowVideoDXVAGuid, out baseFilter);
      if (baseFilter == null)
        DirectShowUtil.FindFilterByClassID(graphBuilder, FFDShowAPI.FFDShowVideoRawGuid, out baseFilter);

      if (baseFilter != null)
      {
        IffdshowDec ffdshowDec = baseFilter as IffdshowDec;
        if (ffdshowDec != null)
        {
          // use a temporary instance of the API, as it is only used here, to disable subs
          FFDShowAPI tempffdshowAPI = new FFDShowAPI((object)baseFilter);
          tempffdshowAPI.DoShowSubtitles = true;
          Log.Info("FFDshow interfaces found -> Subtitles disabled");
          tempffdshowAPI.Dispose();
        }
        else
        {
          DirectShowUtil.ReleaseComObject(baseFilter);
        }
      }
    } 

    protected override void LoadAdvancedSettings(Settings xmlreader)
    {
      //TODO : custom settings for FFDShow (normally hold in presets, not sure if this is useful)
      /*int subPicsBufferAhead = xmlreader.GetValueAsInt("subtitles", "subPicsBufferAhead", 3);
      bool pow2textures = xmlreader.GetValueAsBool("subtitles", "pow2tex", false);
      string textureSize = xmlreader.GetValueAsString("subtitles", "textureSize", "Medium");
      bool disableAnimation = xmlreader.GetValueAsBool("subtitles", "disableAnimation", true);

      int w, h;
      int screenW = GUIGraphicsContext.Width;
      int screenH = GUIGraphicsContext.Height;
      bool res1080 = (screenW == 1920 && screenH == 1080);
      bool res720 = (screenW >= 1280 && screenW <= 1368 && screenH >= 720 && screenH <= 768);

      if (textureSize.Equals("Desktop"))
      {
        w = screenW;
        h = screenH;
      }
      else if (textureSize.Equals("Low"))
      {
        if (res1080)
        {
          w = 854;
          h = 480;
        }
        else if (res720)
        {
          w = 512;
          h = 288;
        }
        else
        {
          w = (int)(Math.Round(screenW / 3.0));
          h = (int)(Math.Round(screenH / 3.0));
        }
      }
      else //if (textureSize.Equals("Medium"))
      {
        if (res1080)
        {
          w = 1280;
          h = 720;
        }
        else if (res720)
        {
          w = 854;
          h = 480;
        }
        else
        {
          w = (int)(Math.Round(screenW * 2.0 / 3));
          h = (int)(Math.Round(screenH * 2.0 / 3));
        }
      }
      Log.Debug("FFDShowEngine: using texture size of {0}x{1}", w, h);
      Size size = new Size(w, h);
      MpcSubtitles.SetAdvancedOptions(subPicsBufferAhead, size, pow2textures, disableAnimation);*/
    }

    #region ISubEngine Members

    public bool LoadSubtitles(IGraphBuilder graphBuilder, string filename)
    {
      LoadSettings();

      //remove DirectVobSub
      DirectVobSubUtil.RemoveFromGraph(graphBuilder);
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
      // Window size
      //Size size = new Size(GUIGraphicsContext.Width, GUIGraphicsContext.Height);
      /*List<FFDShowAPI.FFDShowInstance> ffdshowInstance = FFDShowAPI.getFFDShowInstances();
      FFDShowAPI.FFDShowAPI api = new FFDShowAPI();*/


      IBaseFilter baseFilter = null;
      DirectShowUtil.FindFilterByClassID(graphBuilder, FFDShowAPI.FFDShowVideoGuid, out baseFilter);
      if (baseFilter == null)
        DirectShowUtil.FindFilterByClassID(graphBuilder, FFDShowAPI.FFDShowVideoDXVAGuid, out baseFilter);
      if (baseFilter == null)
        DirectShowUtil.FindFilterByClassID(graphBuilder, FFDShowAPI.FFDShowVideoRawGuid, out baseFilter);
      if (baseFilter == null)
        return false;

      ffdshowAPI = new FFDShowAPI((object)baseFilter);

      IffdshowDec ffdshowDec = baseFilter as IffdshowDec;
      if (ffdshowDec == null)
      {
        Log.Error("FFdshow interfaces not found. Try to update FFDShow");
      }
      else
        Log.Info("FFdshow interfaces found");
      if (selectionOff)
      {
        Enable = false;
      }
      else
      {
        Enable = autoShow;
      }
      return true;
    }

    public void FreeSubtitles()
    {
      if (ffdshowAPI != null)
        ffdshowAPI.Dispose();
    }

    public void SaveToDisk()
    {
      //MpcSubtitles.SaveToDisk();
    }

    public bool IsModified()
    {
      return false;
      //return MpcSubtitles.IsModified();
    }

    public AutoSaveTypeEnum AutoSaveType
    {
      get { return this.autoSaveType; }
    }

    public void Render(Rectangle subsRect, Rectangle frameRect)
    {
      /*Rectangle r = posRelativeToFrame ? frameRect : subsRect;
      int posY = adjustPosY * r.Height / GUIGraphicsContext.Height;
      MpcSubtitles.Render(r.X, r.Y + posY, r.Width, r.Height);*/
    }

    public int GetCount()
    {
      int cnt = ffdshowAPI.SubtitleStreams.Count;
      Log.Debug("FFDShowEngine : " + cnt + " subtitle streams");
      return cnt;
    }

    public string GetLanguage(int i)
    {
      FFDShowAPI.Streams subtitleStreams = ffdshowAPI.SubtitleStreams;
      int index = 0;
      foreach (KeyValuePair<int, FFDShowAPI.Stream> streamPair in subtitleStreams)
      {
        if (index == i) return streamPair.Value.languageName;
        index++;
      }
      return "";
    }

    public string GetSubtitleName(int i)
    {
      FFDShowAPI.Streams subtitleStreams = ffdshowAPI.SubtitleStreams;
      int index = 0;
      foreach (KeyValuePair<int, FFDShowAPI.Stream> streamPair in subtitleStreams)
      {
        if (index == i) return streamPair.Value.name;
        index++;
      }
      return "";
    }

    public int Current
    {
      get
      {
        int index = 0;
        FFDShowAPI.Streams subtitleStreams = ffdshowAPI.SubtitleStreams;
        foreach (KeyValuePair<int, FFDShowAPI.Stream> subtitleStream in subtitleStreams)
        {
          if (subtitleStream.Value.enabled)
            return index;
          index++;
        }
        return -1; //Pour que la selection par defaut des subs soit OK avec NO SUBTITLE
      }
      set
      {
        int index = 0;
        FFDShowAPI.Streams subtitleStreams = ffdshowAPI.SubtitleStreams;
        foreach (KeyValuePair<int, FFDShowAPI.Stream> subtitleStream in subtitleStreams)
        {
          if (index == value)
          {
            ffdshowAPI.SubtitleStream = subtitleStream.Key;
            return;
          }
          index++;
        }
      }
    }

    public bool Enable
    {
      get { return ffdshowAPI.SubtitlesEnabled; }
      set { ffdshowAPI.SubtitlesEnabled = value; }
    }

    public int DelayInterval //?? What for ??
    {
      get { return delayInterval; }
    }

    public int Delay
    {
      get { return ffdshowAPI.SubtitlesDelay; }
      set { ffdshowAPI.SubtitlesDelay = value; }
    }

    public void DelayPlus()
    {
      Delay = Delay + delayInterval;
    }

    public void DelayMinus()
    {
      Delay = Delay - delayInterval;
    }

    public void SetTime(long nsSampleTime) //?? What for ??
    {
      //MpcSubtitles.SetTime(nsSampleTime);
    }

    public bool AutoShow
    {
      get { return autoShow; }
      set { autoShow = value; }
    }

    #endregion

    #region IPostProcessing Members

    public bool LoadPostProcessing(IGraphBuilder graphBuilder)
    {
      //LoadSettings();

      using (Settings xmlreader = new MPSettings())
      {
        audiodelayInterval = xmlreader.GetValueAsInt("FFDShow", "audiodelayInterval", 50);
      }

      IBaseFilter baseFilter = null;
      // No Postprocessing for FFDShow DXVA decoder
      DirectShowUtil.FindFilterByClassID(graphBuilder, FFDShowAPI.FFDShowVideoGuid, out baseFilter);
      if (baseFilter == null)
        DirectShowUtil.FindFilterByClassID(graphBuilder, FFDShowAPI.FFDShowVideoRawGuid, out baseFilter);
      if (baseFilter == null) return false;
      ffdshowAPI = new FFDShowAPI((object)baseFilter);
      hasPostProcessing = true;
      return true;
    }

    public bool HasPostProcessing
    {
      get { return hasPostProcessing; }
    }

    public bool EnableResize
    {
      get { return ffdshowAPI.DoResize; }
      set { ffdshowAPI.DoResize = value; }
    }

    public bool EnablePostProcess
    {
      get { return ffdshowAPI.DoPostProcessing; }
      set { ffdshowAPI.DoPostProcessing = value; }
    }

    public bool EnableDeinterlace
    {
      get { return ffdshowAPI.DoDeinterlace; }
      set { ffdshowAPI.DoDeinterlace = value; }
    }

    public bool EnableCrop
    {
      get { return ffdshowAPI.DoCropZoom; }
      set { ffdshowAPI.DoCropZoom = value; }
    }

    public int CropVertical
    {
      get { return ffdshowAPI.CropVertical; }
      set
      {
        ffdshowAPI.DoCropZoom = true;
        ffdshowAPI.CropVertical = value;
      }
    }

    public int CropHorizontal
    {
      get { return ffdshowAPI.CropHorizontal; }
      set
      {
        ffdshowAPI.DoCropZoom = true;
        ffdshowAPI.CropHorizontal = value;
      }
    }

    public void FreePostProcess()
    {
      if (ffdshowAPI != null)
        ffdshowAPI.Dispose();
    }
    
    public int AudioDelay
    {
      get { return ffdshowAPI.AudioDelay; }
      set { ffdshowAPI.AudioDelay = value; }
    }

    public int AudioDelayInterval
    {
      get { return audiodelayInterval; }
    }

    public void AudioDelayMinus()
    {
      AudioDelay = AudioDelay - AudioDelayInterval;
    }

    public void AudioDelayPlus()
    {
      AudioDelay = AudioDelay + AudioDelayInterval;
    }

    #endregion
  }
}
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using DirectShowLib;
using MediaPortal.GUI.Library;
using DShowNET.Helper;
using MediaPortal.Profile;

namespace MediaPortal.Player.Subtitles
{
  public class MpcEngine : SubSettings, ISubEngine
  {
    protected override void LoadAdvancedSettings(Settings xmlreader)
    {
      int subPicsBufferAhead = xmlreader.GetValueAsInt("subtitles", "subPicsBufferAhead", 3);
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
      Log.Debug("MpcEngine: using texture size of {0}x{1}", w, h);
      Size size = new Size(w, h);
      MpcSubtitles.SetAdvancedOptions(subPicsBufferAhead, size, pow2textures, disableAnimation);
    }

    #region ISubEngine Members

    public bool LoadSubtitles(IGraphBuilder graphBuilder, string filename)
    {
      LoadSettings();
      MpcSubtitles.SetDefaultStyle(ref this.defStyle, this.overrideASSStyle);

      //remove DirectVobSub
      DirectVobSubUtil.RemoveFromGraph(graphBuilder);
      {//remove InternalScriptRenderer as it takes subtitle pin
        IBaseFilter isr = null;
        DirectShowUtil.FindFilterByClassID(graphBuilder, ClassId.InternalScriptRenderer, out isr);
        if (isr != null)
        {
          graphBuilder.RemoveFilter(isr);
          DirectShowUtil.ReleaseComObject(isr);
        }
      }

      Size size = new Size(GUIGraphicsContext.Width, GUIGraphicsContext.Height);
      return MpcSubtitles.LoadSubtitles(
        DirectShowUtil.GetUnmanagedDevice(GUIGraphicsContext.DX9Device),
        size, filename, graphBuilder);
    }

    public void FreeSubtitles()
    {
      MpcSubtitles.FreeSubtitles();
    }

    public void SaveToDisk()
    {
      MpcSubtitles.SaveToDisk();
    }

    public bool IsModified()
    {
      return MpcSubtitles.IsModified();
    }

    public AutoSaveTypeEnum AutoSaveType
    {
      get { return this.autoSaveType; }
    }

    public void Render(Rectangle subsRect, Rectangle frameRect)
    {
      Rectangle r = posRelativeToFrame ? frameRect : subsRect;
      MpcSubtitles.Render(r.X, r.Y, r.Width, r.Height);
    }

    public int GetCount()
    {
      return MpcSubtitles.GetCount();
    }

    public string GetLanguage(int i)
    {
      return MpcSubtitles.GetLanguage(i);
    }

    public int Current
    {
      get
      {
        return MpcSubtitles.GetCurrent();
      }
      set
      {
        MpcSubtitles.SetCurrent(value);
      }
    }

    public bool Enable
    {
      get
      {
        return MpcSubtitles.GetEnable();
      }
      set
      {
        MpcSubtitles.SetEnable(value);
      }
    }

    public int DelayInterval
    {
      get 
      {
        return delayInterval;
      }
    }

    public int Delay
    {
      get
      {
        return MpcSubtitles.GetDelay();
      }
      set
      {
        MpcSubtitles.SetDelay(value);
      }
    }

    public void DelayPlus()
    {
      Delay = Delay + delayInterval;
    }

    public void DelayMinus()
    {
      Delay = Delay - delayInterval;
    }

    public void SetTime(long nsSampleTime)
    {
      MpcSubtitles.SetTime(nsSampleTime);
    }

    #endregion


    class MpcSubtitles
    {
      //set default subtitle's style (call before LoadSubtitles to take effect)
      [DllImport("mpcSubs.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
      public static extern void SetDefaultStyle([In] ref SubtitleStyle style, bool overrideUserStyle);

      //load subtitles for video file filename, with given (rendered) graph 
      [DllImport("mpcSubs.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
      public static extern bool LoadSubtitles(IntPtr d3DDev, Size size, string filename, IGraphBuilder graphBuilder);

      //set sample time (set from EVR presenter, not used in case of vmr9)
      [DllImport("mpcSubs.dll", ExactSpelling = true)]
      public static extern void SetTime(long nsSampleTime);

      [DllImport("mpcSubs.dll", ExactSpelling = true)]
      public static extern void SaveToDisk();

      [DllImport("mpcSubs.dll", ExactSpelling = true)]
      public static extern bool IsModified();

      ////
      //subs management functions
      ///

      [DllImport("mpcSubs.dll", ExactSpelling = true)]
      public static extern int GetCount();

      [DllImport("mpcSubs.dll", ExactSpelling = true)]
      [return: MarshalAs(UnmanagedType.BStr)]
      public static extern string GetLanguage(int i);

      [DllImport("mpcSubs.dll", ExactSpelling = true)]
      public static extern int GetCurrent();

      [DllImport("mpcSubs.dll", ExactSpelling = true)]
      public static extern void SetCurrent(int current);

      [DllImport("mpcSubs.dll", ExactSpelling = true)]
      public static extern bool GetEnable();

      [DllImport("mpcSubs.dll", ExactSpelling = true)]
      public static extern void SetEnable(bool enable);

      [DllImport("mpcSubs.dll", ExactSpelling = true)]
      public static extern void Render(int x, int y, int width, int height);

      [DllImport("mpcSubs.dll", ExactSpelling = true)]
      public static extern int GetDelay(); //in milliseconds

      [DllImport("mpcSubs.dll", ExactSpelling = true)]
      public static extern void SetDelay(int delay_ms); //in milliseconds

      [DllImport("mpcSubs.dll", ExactSpelling = true)]
      public static extern void FreeSubtitles();

      [DllImport("mpcSubs.dll", ExactSpelling = true)]
      public static extern void SetAdvancedOptions(int subPicsBufferAhead, Size textureSize, bool pow2tex, bool disableAnimation);
    }
  }
}

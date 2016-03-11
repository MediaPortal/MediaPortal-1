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
using System.Globalization;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using DirectShowLib;
using MediaPortal.GUI.Library;
using DShowNET.Helper;
using MediaPortal.Profile;
using FFDShow;
using FFDShow.Interfaces;

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
      if (selectionOff)
      {
        MpcSubtitles.SetShowForcedOnly(false);
      }
      else
      {
        MpcSubtitles.SetShowForcedOnly(!this.autoShow);
      }
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

      FFDShowEngine.DisableFFDShowSubtitles(graphBuilder);

      Size size = new Size(GUIGraphicsContext.Width, GUIGraphicsContext.Height);

      // Get Default Language from MP Setting and parse it to MPC-HC Engine (needed for forced track)
      string defaultLanguageCulture = "EN";
      string localizedCINameSub = "EN";
      int lcidCI = 0;

      using (Settings xmlreader = new MPSettings())
      {
        try
        {
          if (g_Player.IsVideo && (g_Player.CurrentFile.ToUpperInvariant().Contains(@"\BDMV\INDEX.BDMV")))
          {
            localizedCINameSub = (xmlreader.GetValueAsString("bdplayer", "subtitlelanguage", "English"));
            foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
            {
              if (ci.EnglishName == localizedCINameSub)
              {
                lcidCI = ci.TextInfo.LCID;;
              }
            }
            Log.Info("MpcEngine: Subtitle Blu-ray Player CultureInfo {0}", localizedCINameSub);
          }
          else
          {
            CultureInfo ci = new CultureInfo(xmlreader.GetValueAsString("subtitles", "language", defaultLanguageCulture));
            lcidCI = ci.TextInfo.LCID;
            Log.Info("MpcEngine: Subtitle VideoPlayer CultureInfo {0}", ci);
          }
        }
        catch (Exception ex)
        {
          CultureInfo ci = new CultureInfo(defaultLanguageCulture);
          lcidCI = ci.TextInfo.LCID;
          Log.Error(
            "MpcEngine: SelectSubtitleLanguage - unable to build CultureInfo, make sure MediaPortal.xml is not corrupted! - {0}",
            ex);
        }
      }

      return MpcSubtitles.LoadSubtitles(
        DirectShowUtil.GetUnmanagedDevice(GUIGraphicsContext.DX9Device),
        size, filename, graphBuilder, subPaths, lcidCI);
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
      int posY = adjustPosY * r.Height / GUIGraphicsContext.Height;
      MpcSubtitles.Render(r.X, r.Y + posY, r.Width, r.Height);
    }

    public int GetCount()
    {
      return MpcSubtitles.GetCount();
    }

    public string GetLanguage(int i)
    {
      return MpcSubtitles.GetLanguage(i);
    }

    public string GetSubtitleName(int i)
    {
      return MpcSubtitles.GetTrackName(i);
    }

    public int Current
    {
      get { return MpcSubtitles.GetCurrent(); }
      set { MpcSubtitles.SetCurrent(value); }
    }

    public bool Enable
    {
      get { return MpcSubtitles.GetEnable(); }
      set { MpcSubtitles.SetEnable(value); }
    }

    public int DelayInterval
    {
      get { return delayInterval; }
    }

    public int Delay
    {
      get { return MpcSubtitles.GetDelay(); }
      set { MpcSubtitles.SetDelay(value); }
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

    public bool AutoShow
    {
      get { return autoShow; }
      set
      {
        autoShow = value;
        MpcSubtitles.SetShowForcedOnly(!this.autoShow);
      }
    }

    #endregion

    private class MpcSubtitles
    {
      //set default subtitle's style (call before LoadSubtitles to take effect)
      [DllImport("mpcSubs.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Unicode)]
      public static extern void SetDefaultStyle([In] ref SubtitleStyle style, bool overrideUserStyle);

      //load subtitles for video file filename, with given (rendered) graph 
      [DllImport("mpcSubs.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Unicode)]
      public static extern bool LoadSubtitles(IntPtr d3DDev, Size size, string filename, IGraphBuilder graphBuilder,
                                              string paths, int lcidCI);

      //set sample time (set from EVR presenter, not used in case of vmr9)
      [DllImport("mpcSubs.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
      public static extern void SetTime(long nsSampleTime);

      [DllImport("mpcSubs.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
      public static extern void SaveToDisk();

      [DllImport("mpcSubs.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
      public static extern bool IsModified();

      ////
      //subs management functions
      ///
      [DllImport("mpcSubs.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
      public static extern int GetCount();

      [DllImport("mpcSubs.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
      [return: MarshalAs(UnmanagedType.BStr)]
      public static extern string GetLanguage(int i);

      [DllImport("mpcSubs.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
      [return: MarshalAs(UnmanagedType.BStr)]
      public static extern string GetTrackName(int i);

      [DllImport("mpcSubs.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
      public static extern int GetCurrent();

      [DllImport("mpcSubs.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
      public static extern void SetCurrent(int current);

      [DllImport("mpcSubs.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
      public static extern bool GetEnable();

      [DllImport("mpcSubs.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
      public static extern void SetEnable(bool enable);

      [DllImport("mpcSubs.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
      public static extern void Render(int x, int y, int width, int height);

      [DllImport("mpcSubs.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
      public static extern int GetDelay();

      //in milliseconds

      [DllImport("mpcSubs.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
      public static extern void SetDelay(int delay_ms);

      [DllImport("mpcSubs.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
      public static extern void FreeSubtitles();

      [DllImport("mpcSubs.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
      public static extern void SetAdvancedOptions(int subPicsBufferAhead, Size textureSize, bool pow2tex,
                                                   bool disableAnimation);

      [DllImport("mpcSubs.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
      public static extern void SetShowForcedOnly(bool onlyShowForcedSubs);
    }
  }
}
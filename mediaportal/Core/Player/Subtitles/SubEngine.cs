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
using DirectShowLib;
using System.Drawing;
using MediaPortal.Profile;
using MediaPortal.Configuration;

namespace MediaPortal.Player.Subtitles
{
  public interface ISubEngine
  {
    bool LoadSubtitles(IGraphBuilder graphBuilder, string filename);
    void FreeSubtitles();

    void SaveToDisk();
    bool IsModified();

    AutoSaveTypeEnum AutoSaveType { get; }

    void Render(Rectangle subsRect, Rectangle frameRect);
    void SetTime(long nsSampleTime);

    void SetDevice(IntPtr device);

    ////
    //subs management functions
    ///

    #region Embedded subtitles

    int GetCount();

    string GetLanguage(int i);

    string GetSubtitleName(int i);

    int Current { get; set; }

    #endregion

    bool Enable { get; set; }

    int Delay { get; set; }

    int DelayInterval { get; }

    void DelayPlus();
    void DelayMinus();

    bool AutoShow { get; set; }
  }

  public class SubEngine
  {
    public static ISubEngine engine;

    public static ISubEngine GetInstance()
    {
      return GetInstance(false);
    }

    public static ISubEngine GetInstance(bool forceinitialize)
    {
      if (engine == null || forceinitialize)
      {
        using (Settings xmlreader = new MPSettings())
        {
          string engineType = xmlreader.GetValueAsString("subtitles", "engine", "DirectVobSub");
          if (g_Player.Player is VideoPlayerVMR9)
          {
            if (engineType.Equals("MPC-HC"))
              engine = new MpcEngine();
            else if (engineType.Equals("FFDShow"))
              engine = new FFDShowEngine();
            else if (engineType.Equals("DirectVobSub"))
              engine = new DirectVobSubEngine();
            else
              engine = new DummyEngine();
          }
          else
            engine = new DummyEngine();
        }
      }
      return engine;
    }

    public class DummyEngine : ISubEngine
    {
      #region ISubEngine Members

      public void SetDevice(IntPtr device) {}

      public bool LoadSubtitles(IGraphBuilder graphBuilder, string filename)
      {
        DirectVobSubUtil.RemoveFromGraph(graphBuilder);
        return false;
      }

      public void FreeSubtitles() {}

      public void SaveToDisk() {}

      public bool IsModified()
      {
        return false;
      }

      public AutoSaveTypeEnum AutoSaveType
      {
        get { return AutoSaveTypeEnum.NEVER; }
      }

      public void Render(Rectangle subsRect, Rectangle frameRect) {}

      public void SetTime(long nsSampleTime) {}

      public int GetCount()
      {
        return 0;
      }

      public string GetLanguage(int i)
      {
        return null;
      }

      public string GetSubtitleName(int i)
      {
        return "";
      }

      public int Current
      {
        get { return -1; }
        set { }
      }

      public bool Enable
      {
        get { return false; }
        set { }
      }

      public int Delay
      {
        get { return 0; }
        set { }
      }

      public int DelayInterval
      {
        get { return 0; }
      }

      public void DelayPlus() {}

      public void DelayMinus() {}

      public bool AutoShow
      {
        get { return false; }
        set { }
      }

      #endregion
    }
  }
}
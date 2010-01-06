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

    ////
    //subs management functions
    ///
    int GetCount();

    string GetLanguage(int i);

    int Current { get; set; }

    bool Enable { get; set; }

    int Delay { get; set; }

    int DelayInterval { get; }

    void DelayPlus();
    void DelayMinus();
  }

  public class SubEngine
  {
    private static ISubEngine engine;

    public static ISubEngine GetInstance()
    {
      if (engine == null)
      {
        using (Settings xmlreader = new MPSettings())
        {
          string engineType = xmlreader.GetValueAsString("subtitles", "engine", "DirectVobSub");
          if (engineType.Equals("MPC-HC"))
            engine = new MpcEngine();
          else if (engineType.Equals("DirectVobSub"))
            engine = new DirectVobSubEngine();
          else
            engine = new DummyEngine();
        }
      }
      return engine;
    }

    private class DummyEngine : ISubEngine
    {
      #region ISubEngine Members

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

      #endregion
    }
  }
}
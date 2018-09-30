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
using MediaPortal.Player.LAV;
using MediaPortal.Player.Subtitles;

namespace MediaPortal.Player.PostProcessing
{
  public interface IPostProcessingEngine
  {
    bool HasPostProcessing { get; }
    bool EnableResize { get; set; }
    bool EnablePostProcess { get; set; }
    bool EnableDeinterlace { get; set; }
    bool EnableCrop { get; set; }
    bool LoadPostProcessing(IGraphBuilder graphBuilder);
    int CropVertical { get; set; }
    int CropHorizontal { get; set; }
    int AudioDelay { get; set; }
    int AudioDelayInterval { get; }
    void AudioDelayPlus();
    void AudioDelayMinus();
    void FreePostProcess();
  }

  public class PostProcessingEngine
  {
    public static IPostProcessingEngine engine;

    public static IPostProcessingEngine GetInstance()
    {
      return GetInstance(false);
    }

    public static IPostProcessingEngine GetInstance(bool forceinitialize)
    {
      if (engine == null || forceinitialize)
      {
        /*public static IPostProcessingEngine GetInstance()
              { */
        using (Settings xmlreader = new MPSettings())
        {
          string engineType = xmlreader.GetValueAsString("subtitles", "engine", "FFDShow");
          if (engineType.Equals("FFDShow"))
            engine = new FFDShowEngine();
          else
            engine = new LavEngine();
        }
      }
      return engine;
    }

    public class DummyEngine : IPostProcessingEngine
    {
      #region IPostProcessingEngine Members

      public bool HasPostProcessing
      {
        get { return false; }
      }

      public bool EnableResize
      {
        get { return false; }
        set { }
      }

      public bool EnablePostProcess
      {
        get { return false; }
        set { }
      }

      public bool LoadPostProcessing(IGraphBuilder graphBuilder)
      {
        return false;
      }

      public bool EnableDeinterlace
      {
        get { return false; }
        set { }
      }

      public bool EnableCrop
      {
        get { return false; }
        set { }
      }

      public int CropVertical
      {
        get { return 0; }
        set { }
      }

      public int CropHorizontal
      {
        get { return 0; }
        set { }
      }

      public void FreePostProcess() { }

      public int AudioDelayInterval
      {
        get { return 0; }
      }

      public int AudioDelay
      {
        get { return 0; }
        set { }
      }

      public void AudioDelayMinus() { }
      public void AudioDelayPlus() { }

      #endregion
    }
  }
}
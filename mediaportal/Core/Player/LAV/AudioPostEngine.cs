#region Copyright (C) 2005-2018 Team MediaPortal

// Copyright (C) 2005-2018 Team MediaPortal
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

using DirectShowLib;
using MediaPortal.Profile;

namespace MediaPortal.Player.LAV
{
  public interface IAudioPostEngine
  {
    bool HasAudioEngine { get; }
    bool LoadPostProcessing(IGraphBuilder graphBuilder);
    int AudioDelay { get; set; }
    int AudioDelayInterval { get; }
    void AudioDelayPlus();
    void AudioDelayMinus();
    void FreePostProcess();
  }

  public class AudioPostEngine
  {
    public static IAudioPostEngine engine;

    public static IAudioPostEngine GetInstance()
    {
      return GetInstance(false);
    }

    public static IAudioPostEngine GetInstance(bool forceinitialize)
    {
      if (engine == null || forceinitialize)
      {
        using (Settings xmlreader = new MPSettings())
        {
          if (engine != null && engine.ToString().ToLowerInvariant().Equals("mediaportal.player.lav.lavengine"))
          {
            // Release current engine
            engine.FreePostProcess();
          }
          engine = new LavEngine();
        }
      }
      return engine;
    }

    public class DummyEngine : IAudioPostEngine
    {
      #region IAudioPostEngine Members

      public bool HasAudioEngine
      {
        get { return false; }
      }

      public bool LoadPostProcessing(IGraphBuilder graphBuilder)
      {
        return false;
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
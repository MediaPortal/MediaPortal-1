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

using DirectShowLib;
using MediaPortal.Configuration;
using MediaPortal.Profile;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// Summary description for VideoRendererStatistics.
  /// </summary>
  public class VideoRendererStatistics
  {
    public enum State
    {
      NotUsed,
      NoSignal, // no (or bad) signal found
      Signal, // signal found, but no video detected
      Scrambled, // video is scrambled
      VideoPresent // video present
    }

    private static State videoState = State.NotUsed;
    private static int framesDrawn = 0, avgSyncOffset = 0, avgDevSyncOffset = 0, framesDropped = 0, jitter = 0;
    private static float avgFrameRate = 0f;
    private static int _noSignalTimeOut = -1;

    public static int NoSignalTimeOut
    {
      get
      {
        if (_noSignalTimeOut == -1)
        {
          using (Settings xmlreader = new MPSettings())
          {
            _noSignalTimeOut = xmlreader.GetValueAsInt("debug", "nosignaltimeout", 5);
          }
        }

        return _noSignalTimeOut;
      }
    }

    public static bool IsVideoFound
    {
      get { return (videoState == State.NotUsed || videoState == State.VideoPresent); }
    }

    public static State VideoState
    {
      get { return videoState; }
      set { videoState = value; }
    }

    public static float AverageFrameRate
    {
      get { return avgFrameRate; }
      set { avgFrameRate = value; }
    }

    public static int AverageSyncOffset
    {
      get { return avgSyncOffset; }
      set { avgSyncOffset = value; }
    }

    public static int AverageDeviationSyncOffset
    {
      get { return avgDevSyncOffset; }
      set { avgDevSyncOffset = value; }
    }

    public static int FramesDrawn
    {
      get { return framesDrawn; }
      set { framesDrawn = value; }
    }

    public static int FramesDropped
    {
      get { return framesDropped; }
      set { framesDropped = value; }
    }

    public static int Jitter
    {
      get { return jitter; }
      set { jitter = value; }
    }


    public static void Update(IQualProp quality)
    {
      try
      {
        if (quality != null)
        {
          int framesDrawn = 0, avgFrameRate = 0, avgSyncOffset = 0, avgDevSyncOffset = 0, framesDropped = 0, jitter = 0;
          quality.get_AvgFrameRate(out avgFrameRate);
          quality.get_AvgSyncOffset(out avgSyncOffset);
          quality.get_DevSyncOffset(out avgDevSyncOffset);
          quality.get_FramesDrawn(out framesDrawn);
          quality.get_FramesDroppedInRenderer(out framesDropped);
          quality.get_Jitter(out jitter);
          AverageFrameRate = ((float)avgFrameRate) / 100.0f;
          AverageSyncOffset = avgSyncOffset;
          AverageDeviationSyncOffset = avgDevSyncOffset;
          FramesDrawn = framesDrawn;
          FramesDropped = framesDropped;
          Jitter = jitter;
        }
      }
      catch {}
    }
  }
}
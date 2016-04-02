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

namespace MediaPortal.Player
{
  public class DummyPlayer : IPlayer
  {
    private enum PlayerState
    {
      Idle,
      Playing
    }

    private string _currentFile;
    private PlayerState _state = PlayerState.Idle;
    private bool _isPlaying = false;
    private bool _isPaused = false;
    private MediaInfoWrapper _mediaInfo;

    public DummyPlayer(string filename) {}

    public override bool Play(string strFile)
    {
      if (_state != PlayerState.Idle)
      {
        throw new ArgumentException("state is wrong");
      }
      if (strFile == null)
      {
        throw new ArgumentNullException("file is null");
      }
      if (strFile.Length == 0)
      {
        throw new ArgumentNullException("file is empty");
      }
      _currentFile = strFile;
      _state = PlayerState.Playing;
      _isPlaying = true;
      _isPaused = false;
      _mediaInfo = new MediaInfoWrapper(strFile);
      _mediaInfo.PrintInfo();
      // Console.WriteLine(String.Format("player:{0}", strFile));
      return true;
    }

    public override string CurrentFile
    {
      get
      {
        if (_state != PlayerState.Playing)
        {
          throw new ArgumentException("state is wrong");
        }
        return _currentFile;
      }
    }

    public override bool IsTV
    {
      get
      {
        if (_state != PlayerState.Playing)
        {
          throw new ArgumentException("state is wrong");
        }
        if (_currentFile.ToLowerInvariant().IndexOf(".tv") >= 0)
        {
          return true;
        }
        return false;
      }
    }

    public override double Duration
    {
      get
      {
        if (_state != PlayerState.Playing)
        {
          throw new ArgumentException("state is wrong");
        }
        return _mediaInfo.HasVideo ? _mediaInfo.BestVideoStream.Duration.TotalSeconds : _mediaInfo.BestAudioStream != null ? _mediaInfo.BestAudioStream.Duration.TotalSeconds : 0;
      }
    }

    public override double CurrentPosition
    {
      get
      {
        if (_state != PlayerState.Playing)
        {
          throw new ArgumentException("state is wrong");
        }
        return 10000d;
      }
    }

    public override bool Playing
    {
      get
      {
        if (_state != PlayerState.Playing)
        {
          throw new ArgumentException("state is wrong");
        }
        return _isPlaying;
      }
    }

    public override void Stop()
    {
      if (_state != PlayerState.Playing)
      {
        throw new ArgumentException("state is wrong");
      }
      _isPlaying = false;
      // Console.WriteLine(String.Format("player: stop{0}", CurrentFile));
    }

    public override bool Stopped
    {
      get
      {
        if (_state != PlayerState.Playing)
        {
          throw new ArgumentException("state is wrong");
        }
        return _isPlaying == false;
      }
    }

    public override int AudioStreams { get { return _mediaInfo.AudioStreams.Count; } }

    public override int CurrentAudioStream { get; set; }

    public override AudioStream CurrentAudio { get { return _mediaInfo.AudioStreams[CurrentAudioStream]; } }

    public override AudioStream BestAudio { get { return _mediaInfo.BestAudioStream; } }

    public override int VideoStreams { get { return _mediaInfo.VideoStreams.Count; } }

    public override int CurrentVideoStream { get; set; }

    public override VideoStream CurrentVideo { get { return _mediaInfo.VideoStreams[CurrentVideoStream]; } }

    public override VideoStream BestVideo { get { return _mediaInfo.BestVideoStream; } }

    public override int SubtitleStreams { get { return _mediaInfo.Subtitles.Count; } }

    public override int CurrentSubtitleStream { get; set; }

    public override int EditionStreams { get { return 0; } }

    public override int CurrentEditionStream { get; set; }

    public override bool HasVideo
    {
      get { return _mediaInfo.HasVideo; }
    }

    public override void Dispose() {}

    public override bool Paused
    {
      get
      {
        if (_state != PlayerState.Playing)
        {
          throw new ArgumentException("state is wrong");
        }
        return _isPaused;
      }
    }

    public override void Pause()
    {
      if (_state != PlayerState.Playing)
      {
        throw new ArgumentException("state is wrong");
      }
      _isPaused = !_isPaused;
    }
  }
}
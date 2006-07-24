using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
namespace MediaPortal.Player
{
  public class DummyPlayer : IPlayer
  {
    enum PlayerState
    {
      Idle,
      Playing
    }
    string _currentFile;
    PlayerState _state = PlayerState.Idle;
    bool _isPlaying = false;
    bool _isPaused = false;
    public DummyPlayer(string filename)
    {
    }
    public override bool Play(string strFile)
    {
      if (_state != PlayerState.Idle)
        throw new ArgumentException("state is wrong");
      if (strFile == null)
        throw new ArgumentNullException("file is null");
      if (strFile.Length==0)
        throw new ArgumentNullException("file is empty");
      _currentFile = strFile;
      _state = PlayerState.Playing;
      _isPlaying = true;
      _isPaused = false;
     // Console.WriteLine(String.Format("player:{0}", strFile));
      return true;
    }

    public override string CurrentFile
    {
      get
      {
        if (_state != PlayerState.Playing)
          throw new ArgumentException("state is wrong");
        return _currentFile; 
      }
    }
    public override bool IsTV
    {
      get
      {
        if (_state != PlayerState.Playing)
          throw new ArgumentException("state is wrong");
        if (_currentFile.ToLower().IndexOf(".tv") >= 0) return true;
        return false;
      }
    }
    public override double Duration
    {
      get
      {
        if (_state != PlayerState.Playing)
          throw new ArgumentException("state is wrong");
        return 30000d;
      }
    }

    public override double CurrentPosition
    {
      get
      {
        if (_state != PlayerState.Playing)
          throw new ArgumentException("state is wrong");
        return 10000d;
      }
    }
    public override bool Playing
    {
      get {

        if (_state != PlayerState.Playing)
          throw new ArgumentException("state is wrong");
        return _isPlaying;
      }
    }
    public override void Stop()
    {
      if (_state != PlayerState.Playing)
        throw new ArgumentException("state is wrong");
      _isPlaying = false;
     // Console.WriteLine(String.Format("player: stop{0}", CurrentFile));
    }

    public override bool Stopped
    {
      get
      {
        if (_state != PlayerState.Playing)
          throw new ArgumentException("state is wrong");
        return _isPlaying==false; 
      }
    }

    public override bool HasVideo
    {
      get { return IsTV; }
    }
    public override bool Paused
    {
      get
      {
        if (_state != PlayerState.Playing)
          throw new ArgumentException("state is wrong");
        return _isPaused;
      }
    }
    public override void Pause()
    {
      if (_state != PlayerState.Playing)
        throw new ArgumentException("state is wrong");
      _isPaused = !_isPaused;
    }

  }
}

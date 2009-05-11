#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

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

    public DummyPlayer(string filename)
    {
    }

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
        if (_currentFile.ToLower().IndexOf(".tv") >= 0)
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
        return 30000d;
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

    public override bool HasVideo
    {
      get { return IsTV; }
    }

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
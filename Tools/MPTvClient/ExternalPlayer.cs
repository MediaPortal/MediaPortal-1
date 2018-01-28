#region Copyright (C) 2005-2016 Team MediaPortal

// Copyright (C) 2005-2016 Team MediaPortal
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

using System.Diagnostics;

namespace MPTvClient
{
    internal class ExternalPlayer
  {
    private Process _player = null;

    public bool IsRunning()
    {
      if (_player == null)
        return false;
      else
      {
        _player.Refresh();
        if (_player.HasExited)
          return false;
        else
          return true;
      }
    }

    public bool Start(string exe, string args)
    {
      if (IsRunning())
      {
        if (!Stop())
          return false;
      }
      _player = Process.Start(exe, args);
      return (_player != null);
    }

    public bool Stop()
    {
      if (!IsRunning())
        return true;
      if (!_player.CloseMainWindow())
        _player.Kill();
      _player.Close();
      _player = null;
      return true;
    }
  }
}
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace MPTvClient
{
    class ExternalPlayer
    {
        private Process _player=null;
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
            _player=null;
            return true;
        }
    }
}

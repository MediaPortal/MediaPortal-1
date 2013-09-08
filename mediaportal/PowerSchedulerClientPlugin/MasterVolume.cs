#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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

#region Usings

using System;
using MediaPortal.ExtensionMethods;
using MediaPortal.GUI.Library;
using MediaPortal.Mixer;
using MediaPortal.Player;
using MediaPortal.Profile;

#endregion

namespace MediaPortal.Plugins.Process
{
  /// <summary>
  /// Wrapper class for the master volume handling (dependent on OS and configuration) 
  /// </summary>
  public class MasterVolume : IDisposable
  {
    #region Variables

    private AEDev _audioDefaultDevice = null;

    #endregion

    #region Public methods

    public MasterVolume()
    {
      bool useWaveVolume;

      // If "Upmix stereo to 5.1/7.1" is enabled or the MP volume control is set to "Wave", the setting
      // "digital" is true and the MP volume handler will control the wave volume (not on Windows XP).
      using (Settings reader = new MPSettings())
      {
        useWaveVolume = Environment.OSVersion.Version.Major >= 6 && reader.GetValueAsBool("volume", "digital", false);
      }

      if (useWaveVolume)
      {
        // Since MP's volume handler controls the wave volume, we have to access the Audio Endpoint Device directly
        Log.Debug("PS: Control the master volume by accessing the Audio Endpoint Device directly");
        _audioDefaultDevice = new AEDev();
      }
      else
      {
        // We use MP's volume handler, since it controls the master volume
        Log.Debug("PS: Control the master volume by using MP's volume handler");
      }
    }

    public void Dispose()
    {
      if (_audioDefaultDevice != null)
      {
        _audioDefaultDevice.SafeDispose();
        _audioDefaultDevice = null;
      }
    }

    #endregion

    #region Public properties

    public bool IsMuted
    {
      get
      {
        if (_audioDefaultDevice != null)
          return _audioDefaultDevice.Muted;
        else
          return VolumeHandler.Instance.IsMuted;
      }
      set
      {
        if (_audioDefaultDevice != null)
          _audioDefaultDevice.Muted = value;
        else
          VolumeHandler.Instance.IsMuted = value;
      }
    }

    #endregion
  }
}
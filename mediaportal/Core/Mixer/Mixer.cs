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
using System.Linq;
using MediaPortal.ExtensionMethods;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using NAudio.CoreAudioApi;

namespace MediaPortal.Mixer
{
  public sealed class Mixer : IDisposable
  {
    #region Vars

    private bool _isDefaultDevice = true;
    private bool _isInternalVolumeChange;

    private int[] _volumeTable;

    private MMDeviceEnumerator _mMdeviceEnumerator = new MMDeviceEnumerator();
    private MMDevice _mMdevice;

    #endregion

    #region Methods

    public void Close()
    {
      lock (this)
      {
        if (_handle == IntPtr.Zero)
        {
          return;
        }

        MixerNativeMethods.mixerClose(_handle);

        _handle = IntPtr.Zero;
      }
    }

    public void Dispose()
    {
      _mMdevice?.SafeDispose();
      Close();
    }

    public void Open()
    {
      Open(0, false, null);
    }

    public void Open(int mixerIndex, bool isDigital, int[] volumeTable)
    {
      lock (this)
      {
        try
        {
          if (_mMdeviceEnumerator == null)
            _mMdeviceEnumerator = new MMDeviceEnumerator();

          _mMdevice = _mMdeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
          if (_mMdevice != null)
          {
            Log.Info($"Mixer: default audio device: {_mMdevice.FriendlyName}");

            if (volumeTable != null)
            {
              _volumeTable = volumeTable;
              SetVolumeFromDevice(_mMdevice);
              UpdateDeviceAudioEndpoint();
            }
          }
        }
        catch (Exception)
        {
          _isMuted = false;
          _volume = VolumeMaximum;
        }
      }
    }

    public void SetVolumeFromDevice(MMDevice device)
    {
      // First we need to make sure to convert the 0-100 volume to volume steps
      try
      {
        if (device == null)
          return;

        int currentVolumePercentage = (int) Math.Ceiling(device.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
        _volume = ConvertVolumeToSteps(currentVolumePercentage);
      }
      catch (Exception ex)
      {
        Log.Error($"Mixer: error occured in SetStartupVolume: {ex}");
      }
    }

    public int ConvertVolumeToSteps(int volumePercentage)
    {
      try
      {
        if (_volumeTable == null)
          return 0;

        int totalVolumeSteps = _volumeTable.Length;
        decimal volumePercentageDecimal = (decimal) volumePercentage / 100;
        double index = Math.Floor((double) (volumePercentageDecimal * totalVolumeSteps));

        // Make sure we never go out of bounds
        if (index < 0)
          index = 0;

        while (index >= _volumeTable.Length && index != 0)
          index--;

        // Update volume
        int volumeStep = _volumeTable[(int) index];
        return volumeStep;
      }
      catch (Exception ex)
      {
        Log.Error($"Mixer: error occured in ConvertVolumeToSteps: {ex}");
        return 0;
      }
    }

    public void ChangeAudioDevice(string deviceName, bool setToDefault)
    {
      try
      {
        if (_mMdeviceEnumerator == null)
          _mMdeviceEnumerator = new MMDeviceEnumerator();

        _mMdevice = _mMdeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

        // Need to check for certain strings as well because NAudio doesn''t detect these
        if (setToDefault || deviceName == "Default DirectSound Device" || deviceName == "Default WaveOut Device")
        {
          _mMdevice = _mMdeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
          Log.Info($"Mixer: changed audio device to default : {_mMdevice.FriendlyName}");
          _isDefaultDevice = true;
          return;
        }

        var deviceFound = _mMdeviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)
          .FirstOrDefault(
            device => device.FriendlyName.Trim().ToLowerInvariant() == deviceName.Trim().ToLowerInvariant());

        if (deviceFound != null)
        {
          _mMdevice = deviceFound;
          _isDefaultDevice = false;
          Log.Info($"Mixer: changed audio device to : {deviceFound.FriendlyName}");
        }
        else
        {
          Log.Info($"Mixer: ChangeAudioDevice failed because device {deviceName} was not found, falling back to default");
          _mMdevice = _mMdeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
          _isDefaultDevice = true;
        }

        SetVolumeFromDevice(_mMdevice);
        UpdateDeviceAudioEndpoint();
      }
      catch (Exception ex)
      {
        Log.Error($"Mixer: error occured in ChangeAudioDevice: {ex}");
      }
    }

    private void CheckIfDefaultDeviceStillValid()
    {
      // Check if default device is set and still valid for volume control
      if (_isDefaultDevice)
      {
        var mMdeviceCurrent = _mMdeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        if (mMdeviceCurrent?.ID != _mMdevice?.ID)
        {
          _mMdevice = mMdeviceCurrent;
          SetVolumeFromDevice(_mMdevice);
          UpdateDeviceAudioEndpoint();
        }
      }
    }

    public void UpdateDeviceAudioEndpoint()
    {
      try
      {
        if (_mMdevice?.AudioEndpointVolume != null)
          _mMdevice.AudioEndpointVolume.OnVolumeNotification += AudioEndpointVolume_OnVolumeNotification;
      }
      catch (Exception ex)
      {
        Log.Error($"Mixer: error occured in UpdateDeviceAudioEndpoint: {ex}");
      }
    }

    public MMDeviceCollection AllMultimediaDevices(bool onlyActive)
    {
      DeviceState deviceState = DeviceState.All;
      if(onlyActive)
        deviceState = DeviceState.Active;

      if(_mMdeviceEnumerator == null)
        _mMdeviceEnumerator = new MMDeviceEnumerator();

      var devices = _mMdeviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, deviceState);
      return devices;
    }

    private void AudioEndpointVolume_OnVolumeNotification(NAudio.CoreAudioApi.AudioVolumeNotificationData data)
    {
      if (data?.MasterVolume == null || _isInternalVolumeChange)
        return;

      int volumePercentage = (int)(data.MasterVolume * 100f);
      _volume = ConvertVolumeToSteps(volumePercentage);

      switch (volumePercentage)
      {
        case 0:
          _isMuted = true;    
          break;
        default:
          _isMuted = false;
          break;
      }

      VolumeHandler.Instance.mixer_UpdateVolume();
    }

    #endregion Methods

    #region Properties

    public bool IsMuted
    {
      get { lock (this) return _isMuted; }
      set
      {
        _isInternalVolumeChange = true;
        _isMuted = value;
        _mMdevice.AudioEndpointVolume.Mute = value;
        VolumeHandler.Instance.mixer_UpdateVolume();
        _isInternalVolumeChange = false;
      }
    }

    public int Volume
    {
      get
      {
        lock (this)
        {
          return _volume;
        }
      }
      set
      {
        try
        {
          _isInternalVolumeChange = true;
          CheckIfDefaultDeviceStillValid();

          _volume = value;
          int volumePercentage = (int) Math.Round((double) (100 * value) / VolumeMaximum);

          // Make sure we never go out of scope
          if (volumePercentage < 0)
            volumePercentage = 0;
          else if (volumePercentage > 100)
            volumePercentage = 100;

          if (_mMdevice != null)
          {
            switch (volumePercentage)
            {
              case 0:
                IsMuted = true;
                break;
              case 100:
                _mMdevice.AudioEndpointVolume.MasterVolumeLevelScalar = 1;
                IsMuted = false;
                break;
              default:
                float volume = volumePercentage / 100.0f;
                _mMdevice.AudioEndpointVolume.MasterVolumeLevelScalar = volume;

                IsMuted = false;
                break;
            }
          }

          VolumeHandler.Instance.mixer_UpdateVolume();
          _isInternalVolumeChange = false;
        }
        catch (Exception ex)
        {
          _isInternalVolumeChange = false;
          Log.Error($"Mixer: error occured in Volume: {ex}");
        }
      }
    }

    public int VolumeMaximum => 65535;

    public int VolumeMinimum => 0;

    #endregion Properties

    #region Fields
    private IntPtr _handle;
    private bool _isMuted;
    private int _volume;
    #endregion Fields
  }
}
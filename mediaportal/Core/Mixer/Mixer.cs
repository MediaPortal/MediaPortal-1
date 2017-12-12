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
using CSCore.CoreAudioAPI;
using MediaPortal.ExtensionMethods;
using MediaPortal.GUI.Library;
using MediaPortal.Player;

namespace MediaPortal.Mixer
{
  public sealed class Mixer : IDisposable
  {
    #region Vars

    private bool _isDefaultDevice = true;
    private bool _isInternalVolumeChange;

    private int[] _volumeTable;

    private MMDeviceEnumerator _mMdeviceEnumerator;
    private MMNotificationClient iMultiMediaNotificationClient;
    private MMDevice _mMdevice;
    private AudioEndpointVolume iAudioEndpointVolume;
    private AudioEndpointVolume VolumeDevice { get { return iAudioEndpointVolume; } }
    private CSCore.CoreAudioAPI.AudioEndpointVolumeCallback iAudioEndpointVolumeMixerCallback;
    EventHandler<DefaultDeviceChangedEventArgs> iDefaultDeviceChangedHandler;
    EventHandler<AudioEndpointVolumeCallbackEventArgs> iVolumeChangedHandler;

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
      // Client up our MM objects in reverse order
      if (iAudioEndpointVolumeMixerCallback != null && iAudioEndpointVolume != null)
      {
        iAudioEndpointVolume.UnregisterControlChangeNotify(iAudioEndpointVolumeMixerCallback);
      }

      if (iAudioEndpointVolumeMixerCallback != null)
      {
        iAudioEndpointVolumeMixerCallback.NotifyRecived -= iVolumeChangedHandler;
        iAudioEndpointVolumeMixerCallback = null;
      }

      if (iAudioEndpointVolume != null)
      {
        iAudioEndpointVolume.Dispose();
        iAudioEndpointVolume = null;
      }

      if (_mMdevice != null)
      {
        _mMdevice.Dispose();
        _mMdevice = null;
      }

      if (iMultiMediaNotificationClient != null)
      {
        iMultiMediaNotificationClient.DefaultDeviceChanged -= iDefaultDeviceChangedHandler;
        iMultiMediaNotificationClient.Dispose();
        iMultiMediaNotificationClient = null;
      }

      if (_mMdeviceEnumerator != null)
      {
        _mMdeviceEnumerator.Dispose();
        _mMdeviceEnumerator = null;
      }
      Close();
    }

    public void CreateDevice(EventHandler<DefaultDeviceChangedEventArgs> aDefaultDeviceChangedHandler,
                        EventHandler<AudioEndpointVolumeCallbackEventArgs> aVolumeChangedHandler)
    {
      //Create device and register default device change notification
      _mMdeviceEnumerator = new MMDeviceEnumerator();
      iMultiMediaNotificationClient = new MMNotificationClient(_mMdeviceEnumerator);
      iMultiMediaNotificationClient.DefaultDeviceChanged += iDefaultDeviceChangedHandler = aDefaultDeviceChangedHandler;
      _mMdevice = _mMdeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
      //Register to get volume modifications
      iAudioEndpointVolume = AudioEndpointVolume.FromDevice(_mMdevice);
      iAudioEndpointVolumeMixerCallback = new CSCore.CoreAudioAPI.AudioEndpointVolumeCallback();
      iAudioEndpointVolumeMixerCallback.NotifyRecived += iVolumeChangedHandler = aVolumeChangedHandler;
      iAudioEndpointVolume.RegisterControlChangeNotify(iAudioEndpointVolumeMixerCallback);
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
          CreateDevice(OnDefaultMultiMediaDeviceChanged, OnVolumeNotification);

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

        int currentVolumePercentage = _lastVolume = (int) Math.Ceiling(VolumeDevice.MasterVolumeLevelScalar * 100f);
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

    public int ConvertVolumeToStepsEvent(int volumePercentage)
    {
      try
      {
        if (_volumeTable == null)
          return 0;

        if (volumePercentage > _lastVolume)
        {
          VolumeHandler.Instance.Volume = VolumeHandler.Instance.Next;
        }
        else if (volumePercentage < _lastVolume)
        {
          VolumeHandler.Instance.Volume = VolumeHandler.Instance.Previous;
        }
        return VolumeHandler.Instance.Volume;
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

        // Need to check for certain strings as well because NAudio doesn't detect these
        if (setToDefault || deviceName == "Default DirectSound Device" || deviceName == "Default WaveOut Device")
        {
          _mMdevice = _mMdeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
          Log.Info($"Mixer: changed audio device to default : {_mMdevice.FriendlyName}");
          _isDefaultDevice = true;
          return;
        }

        var deviceFound = _mMdeviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active)
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
        if (mMdeviceCurrent?.DeviceID != _mMdevice?.DeviceID)
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
        //if (_mMdevice?.AudioEndpointVolume != null)
        //  _mMdevice.AudioEndpointVolume.OnVolumeNotification += AudioEndpointVolume_OnVolumeNotification;

        if (_mMdevice != null)
        {
          _mMdevice.Dispose();
          _mMdevice = null;
        }

        if (_mMdeviceEnumerator != null)
        {
          _mMdeviceEnumerator.Dispose();
          _mMdeviceEnumerator = null;
        }

        CreateDevice(OnDefaultMultiMediaDeviceChanged, OnVolumeNotification);

        iVolumeChangedHandler += AudioEndpointVolume_OnVolumeNotification;
        iDefaultDeviceChangedHandler += AudioEndpointDevice_OnVolumeNotification;
      }
      catch (Exception ex)
      {
        Log.Error($"Mixer: error occured in UpdateDeviceAudioEndpoint: {ex}");
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="aEvent"></param>
    public void OnDefaultMultiMediaDeviceChanged(object sender, DefaultDeviceChangedEventArgs aEvent)
    {
      if (aEvent.DataFlow == DataFlow.Render && (aEvent.Role == Role.Multimedia || aEvent.Role == Role.Console))
      {
        if (_mMdevice != null)
        {
          _mMdevice.Dispose();
          _mMdevice = null;
        }

        if (_mMdeviceEnumerator != null)
        {
          _mMdeviceEnumerator.Dispose();
          _mMdeviceEnumerator = null;
        }

        CreateDevice(OnDefaultMultiMediaDeviceChanged, OnVolumeNotification);
        //ResetAudioManagerThreadSafe();
      }
    }

    /// <summary>
    /// Receive volume change notification and reflect changes on our slider.
    /// </summary>
    /// <param name="data"></param>
    public void OnVolumeNotification(object sender, AudioEndpointVolumeCallbackEventArgs aEvent)
    {
      lock (_volumeTable)
      {
        if (_isInternalVolumeChange)
          return;

        if (aEvent != null)
        {
          //Update volume slider
          var volumePercentage = (int)Math.Ceiling(iAudioEndpointVolume.MasterVolumeLevelScalar * 100f);
          _volume = ConvertVolumeToStepsEvent(volumePercentage);
          _isMuted = aEvent.IsMuted;

          // Store current volume value
          _lastVolume = (int) Math.Ceiling(iAudioEndpointVolume.MasterVolumeLevelScalar*100f);
        }

        VolumeHandler.Instance.mixer_UpdateVolume();
      }
    }

    private void AudioEndpointDevice_OnVolumeNotification(object sender, DefaultDeviceChangedEventArgs e)
    {
      // Not used
      // here
    }

    private void AudioEndpointVolume_OnVolumeNotification(object sender, AudioEndpointVolumeCallbackEventArgs e)
    {
      // Not used
      // here
    }

    public MMDeviceCollection AllMultimediaDevices(bool onlyActive)
    {
      DeviceState deviceState = DeviceState.All;
      if(onlyActive)
        deviceState = DeviceState.Active;

      if(_mMdeviceEnumerator == null)
        _mMdeviceEnumerator = new MMDeviceEnumerator();

      var devices = _mMdeviceEnumerator.EnumAudioEndpoints(DataFlow.Render, deviceState);
      return devices;
    }

    //// Not used anymore or not needed with CScore
    //private void AudioEndpointVolume_OnVolumeNotification(CSCore.CoreAudioAPI.AudioVolumeNotificationData data)
    //{
    //  if (data.MasterVolume == null || _isInternalVolumeChange)
    //    return;

    //  int volumePercentage = (int)(data.MasterVolume * 100f);
    //  _volume = ConvertVolumeToSteps(volumePercentage);

    //  switch (volumePercentage)
    //  {
    //    case 0:
    //      _isMuted = true;
    //      break;
    //    default:
    //      _isMuted = false;
    //      break;
    //  }

    //  VolumeHandler.Instance.mixer_UpdateVolume();
    //}

    #endregion Methods

    #region Properties

    public bool IsMuted
    {
      get { lock (this) return _isMuted; }
      set
      {
        _isInternalVolumeChange = true;
        _isMuted = value;
        VolumeDevice.IsMuted = value;
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
                VolumeDevice.MasterVolumeLevelScalar = 1;
                IsMuted = false;
                break;
              default:
                float volume = volumePercentage / 100.0f;
                VolumeDevice.MasterVolumeLevelScalar = volume;

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
    private int _lastVolume;
    #endregion Fields
  }
}
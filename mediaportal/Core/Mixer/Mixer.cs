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
using System.Threading;
using CSCore.CoreAudioAPI;
using DShowNET.Helper;
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
    System.Timers.Timer _dispatchingTimer;
    const string AppName = "MediaPortal";
    private static readonly object CheckAudioLevelsObject = new object();

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
      Stop();
      Close();
    }

    public void CreateDevice(EventHandler<DefaultDeviceChangedEventArgs> aDefaultDeviceChangedHandler,
                        EventHandler<AudioEndpointVolumeCallbackEventArgs> aVolumeChangedHandler)
    {
      try
      {
        //Create device and register default device change notification
        _mMdeviceEnumerator = new MMDeviceEnumerator();
        iMultiMediaNotificationClient = new MMNotificationClient(_mMdeviceEnumerator);
        iMultiMediaNotificationClient.DefaultDeviceChanged += iDefaultDeviceChangedHandler = aDefaultDeviceChangedHandler;
        var mMdeviceList = _mMdeviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active);

        if (mMdeviceList != null && mMdeviceList.Count > 0)
        {
          _mMdevice = _mMdeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
          GUIGraphicsContext.CurrentAudioRendererDevice = _mMdevice.FriendlyName;

          //Register to get volume modifications
          if (_mMdevice != null) iAudioEndpointVolume = AudioEndpointVolume.FromDevice(_mMdevice);
          iAudioEndpointVolumeMixerCallback = new CSCore.CoreAudioAPI.AudioEndpointVolumeCallback();
          iAudioEndpointVolumeMixerCallback.NotifyRecived += iVolumeChangedHandler = aVolumeChangedHandler;
          iAudioEndpointVolume?.RegisterControlChangeNotify(iAudioEndpointVolumeMixerCallback);
        }

        // For audio session
        Stop();
        //DispatchingTimerStart(); // Disable because the check will be done in IsMuted code
      }
      catch (Exception)
      {
        // When no device available
      }
    }

    public void DispatchingTimerStart()
    {
      _dispatchingTimer = new System.Timers.Timer(1000);
      _dispatchingTimer.Elapsed += DispatchingTimer_Elapsed;
      _dispatchingTimer.AutoReset = false;
      _dispatchingTimer.Start();
    }

    private void DispatchingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      if (OSInfo.OSInfo.Win7OrLater())
      {
        CheckAudioLevels();
      }
      _dispatchingTimer?.Start(); // trigger next timer
    }

    public void Stop()
    {
      _dispatchingTimer?.Stop();
      _dispatchingTimer?.Dispose();
    }

    public void CheckAudioLevels()
    {
      lock (CheckAudioLevelsObject)
      {
        try
        {
          using (var sessionManager = GetDefaultAudioSessionManager2(DataFlow.Render))
          {
            if (sessionManager != null)
            {
              using (var sessionEnumerator = sessionManager.GetSessionEnumerator())
              {
                if (sessionEnumerator != null)
                {
                  foreach (var session in sessionEnumerator)
                  {
                    if (session != null)
                    {
                      using (var audioSessionControl2 = session.QueryInterface<AudioSessionControl2>())
                      {
                        if (audioSessionControl2 != null)
                        {
                          var process = audioSessionControl2.Process;
                          string name = audioSessionControl2.DisplayName;
                          if (process != null)
                          {
                            if (name != null && name == "")
                            {
                              name = process.MainWindowTitle;
                            }
                            if (name != null && name == "")
                            {
                              name = process.ProcessName;
                            }
                          }

                          if (name != null && !name.Contains(AppName))
                          {
                            continue;
                          }
                        }

                        using (var simpleVolume = session.QueryInterface<SimpleAudioVolume>())
                        {
                          if (simpleVolume != null)
                          {
                            simpleVolume.MasterVolume = 1;
                            simpleVolume.IsMuted = false;
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
          }
        }
        catch (Exception ex)
        {
          Log.Error("Mixer: Exception in Audio Session {0}", ex);
        }
      }
    }

    private AudioSessionManager2 GetDefaultAudioSessionManager2(DataFlow dataFlow)
    {
      using (var enumerator = new MMDeviceEnumerator())
      {
        if (_mMdeviceEnumerator != null)
        {
          using (var mMdeviceList = _mMdeviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active))
          {
            if (mMdeviceList != null && mMdeviceList.Count > 0)
            {
              using (var device = enumerator.GetDefaultAudioEndpoint(dataFlow, Role.Multimedia))
              {
                if (device != null)
                {
                  var sessionManager = AudioSessionManager2.FromMMDevice(device);
                  return sessionManager;
                }
              }
            }
          }
        }
      }
      return null;
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

          var mMdeviceList = _mMdeviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active);

          if (mMdeviceList != null && mMdeviceList.Count > 0)
          {
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
        }
        catch (Exception ex)
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

        if (VolumeDevice != null)
        {
          int currentVolumePercentage = _lastVolume = (int) Math.Ceiling(VolumeDevice.MasterVolumeLevelScalar * 100f);
          _volume = ConvertVolumeToSteps(currentVolumePercentage);
        }
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
      lock (this)
      {
        try
        {
          if (_volumeTable == null)
            return 0;

          if (VolumeHandler.Instance != null)
          {
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
          return 0;
        }
        catch (Exception ex)
        {
          Log.Error($"Mixer: error occured in ConvertVolumeToSteps: {ex}");
          return 0;
        }
      }
    }

    public bool DetectedDevice()
    {
      if (_mMdeviceEnumerator == null)
        _mMdeviceEnumerator = new MMDeviceEnumerator();

      using (var mMdeviceList = _mMdeviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active))
      {
        return mMdeviceList != null && mMdeviceList.Count > 0;
      }
    }

    public void ChangeAudioDevice(string deviceName, bool setToDefault)
    {
      try
      {
        // Reload filter collection
        FilterHelper.ReloadFilterCollection();

        if (_mMdeviceEnumerator == null)
          _mMdeviceEnumerator = new MMDeviceEnumerator();

        var mMdeviceList = _mMdeviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active);

        if (mMdeviceList != null && mMdeviceList.Count > 0)
        {
          _mMdevice = _mMdeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

          // Need to check for certain strings as well because NAudio doesn't detect these
          if (deviceName != null && (setToDefault || deviceName == "Default DirectSound Device" || deviceName == "Default WaveOut Device"))
          {
            _mMdevice = _mMdeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            if (_mMdevice != null)
            {
              Log.Info($"Mixer: changed audio device to default : {_mMdevice.FriendlyName}");
              _isDefaultDevice = true;
              GUIGraphicsContext.CurrentAudioRendererDevice = _mMdevice.FriendlyName;
            }
            return;
          }

          using (
            var deviceFound =
              _mMdeviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active)
                .FirstOrDefault(
                  device =>
                  {
                    if (device == null) throw new ArgumentNullException(nameof(device));
                    return deviceName != null && device.FriendlyName.Trim().ToLowerInvariant() == deviceName.Trim().ToLowerInvariant();
                  }))
          {
            if (deviceFound != null)
            {
              _mMdevice = deviceFound;
              _isDefaultDevice = false;
              GUIGraphicsContext.CurrentAudioRendererDevice = deviceFound.FriendlyName;
              Log.Info($"Mixer: changed audio device to : {deviceFound.FriendlyName}");
            }
            else
            {
              Log.Info(
                $"Mixer: ChangeAudioDevice failed because device {deviceName} was not found, falling back to default");
              _mMdevice = _mMdeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
              _isDefaultDevice = true;
              GUIGraphicsContext.CurrentAudioRendererDevice = deviceName;
            }
          }

          if (_mMdevice != null) SetVolumeFromDevice(_mMdevice);
          UpdateDeviceAudioEndpoint();
        }
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
        if (_mMdeviceEnumerator != null)
        {
          var mMdeviceList = _mMdeviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active);

          if (mMdeviceList != null && mMdeviceList.Count > 0)
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

        if (iVolumeChangedHandler != null) iVolumeChangedHandler += AudioEndpointVolume_OnVolumeNotification;
        if (iDefaultDeviceChangedHandler != null)
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
      if (aEvent != null && (aEvent.DataFlow == DataFlow.Render && (aEvent.Role == Role.Multimedia || aEvent.Role == Role.Console)))
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
        Stop();
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
          if (iAudioEndpointVolume != null)
          {
            // Force a couple of settings for Windows 10
            if (OSInfo.OSInfo.Win10OrLater() && VolumeHandler.Instance.VolumeStyle() == 5)
            {
              var volumePercentage = (int)Math.Ceiling(iAudioEndpointVolume.MasterVolumeLevelScalar * 100f);
              _volume = ConvertVolumeToStepsEvent(volumePercentage);
            }
            else
            {
              // Needed to use default step issue 2 of 2 instead of 6 of 6
              _volume = (int)Math.Round(aEvent.MasterVolume * VolumeMaximum);
            }

            _isMuted = aEvent.IsMuted;

            // Store current volume value
            _lastVolume = (int) Math.Ceiling(iAudioEndpointVolume.MasterVolumeLevelScalar*100f);
          }
        }

        if (VolumeHandler.Instance != null) VolumeHandler.Instance.mixer_UpdateVolume();
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
        try
        {
          _isInternalVolumeChange = true;
          _isMuted = value;
          if (VolumeDevice != null) VolumeDevice.IsMuted = value;
          VolumeHandler.Instance.mixer_UpdateVolume();
          _isInternalVolumeChange = false;

          if (OSInfo.OSInfo.Win7OrLater())
          {
            // For audio session
            new Thread(() =>
            {
              _isInternalVolumeChange = true;
              Thread.CurrentThread.IsBackground = true;
              CheckAudioLevels();
              Log.Debug("Mixer: CheckAudioLevels");
              _isInternalVolumeChange = false;
            }).Start();
          }
        }
        catch (Exception)
        {
          // When no available
        }

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
                if (VolumeDevice != null) VolumeDevice.MasterVolumeLevelScalar = 1;
                IsMuted = false;
                break;
              default:
                float volume = volumePercentage / 100.0f;
                if (VolumeDevice != null) VolumeDevice.MasterVolumeLevelScalar = volume;

                IsMuted = false;
                break;
            }
          }

          if (VolumeHandler.Instance != null) VolumeHandler.Instance.mixer_UpdateVolume();
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
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
using System.Threading;
using System.Runtime.InteropServices;
using MediaPortal.ExtensionMethods;
using MediaPortal.GUI.Library;
using MediaPortal.Player;

namespace MediaPortal.Mixer
{
  public class Key : IDisposable
  {
    private readonly object syncObject = new object();
    private object padlock;

    public Key() {}
    
    public Key(object locker)
    {
      padlock = locker;
    }

    public void Dispose()
    {
      // when this falls out of scope (after a using {...} ), release the lock
      //Log.Debug("Mixer: MixerLock Dispose()");
      Monitor.Exit(padlock);
      padlock = null;
    }
    
    protected Key MixerLock(int LockTime)
    {
      if (Monitor.TryEnter(this.syncObject, LockTime))
      {
        //Log.Debug("Mixer: MixerLock acquired {0} ms", LockTime);
        return new Key(this.syncObject);
      }
      else
      {
        Log.Error("Mixer: MixerLock timeout {0} ms", LockTime);
        // throw exception, log message, etc.
        throw new TimeoutException("Mixer: MixerLock failed to acquire the lock.");
      }
    }
    
    protected bool IsMixerLocked()
    {
      //Log.Debug("Mixer: MixerLock IsMixerLocked(), padlock:{0}", (padlock != null));
      return (padlock != null);
    }        
  }

  public sealed class Mixer : Key, IDisposable 
  {
    #region Events

    public event MixerEventHandler LineChanged;
    public event MixerEventHandler ControlChanged;

    #endregion Events

    #region Methods

    public void Close()
    {
      try
      {
        lock (this)
        {
          using (MixerLock(lockInfinite))
          {
            if (_handle == IntPtr.Zero)
            {
              return;
            }
    
            MixerNativeMethods.mixerClose(_handle);
    
            _handle = IntPtr.Zero;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error($"Mixer: error occured in Close(): {ex}");
      }
    }

    public void Dispose()
    {
      if (_mixerControlDetailsVolume != null)
      {
        _mixerControlDetailsVolume.SafeDispose();
      }

      if (_mixerControlDetailsMute != null)
      {
        _mixerControlDetailsMute.SafeDispose();
      }

      if (_audioDefaultDevice != null)
      {
        _audioDefaultDevice.OnVolumeNotification -= new AudioEndpointVolumeNotificationDelegate(AudioEndpointVolume_OnVolumeNotification);
        _audioDefaultDevice.SafeDispose();
      }

      if (_mixerEventListener != null)
      {
        _mixerEventListener.LineChanged -= new MixerEventHandler(OnLineChanged);
        _mixerEventListener.ControlChanged -= new MixerEventHandler(OnControlChanged);
      }

      Close();

      if (_mixerEventListener != null)
      {
        _mixerEventListener.DestroyHandle();
        _mixerEventListener = null;
      }
    }

    public void Open()
    {
      Open(0, false);
    }

    public void Open(int mixerIndex, bool isDigital, bool resetDevice = false)
    {
      try
      {
        lock (this)
        {
          using (MixerLock(lockInfinite))
          {
            Log.Debug("Mixer: Open(), mixerIndex = {0}, isDigital = {1}, resetDevice = {2}", mixerIndex, isDigital, resetDevice);
            _useWave = isDigital;
            if (isDigital)
            {
              _componentType = MixerComponentType.SourceWave;
            }
            else
            {
              _componentType = MixerComponentType.DestinationSpeakers;
            }
    
            // Use Endpoint Volume API for Vista/Win7 if master volume is selected and always for Win8 to handle muting of master volume
            if ((OSInfo.OSInfo.VistaOrLater() && _componentType == MixerComponentType.DestinationSpeakers) ||
                OSInfo.OSInfo.Win8OrLater())
            {
              try
              {
                Log.Debug("Mixer: Open(), Endpoint Volume API for Master");
                _audioDefaultDevice = new AEDev(resetDevice);
                if (_audioDefaultDevice != null)
                {
                  _audioDefaultDevice.OnVolumeNotification +=
                    new AudioEndpointVolumeNotificationDelegate(AudioEndpointVolume_OnVolumeNotification);
    
                  _isMuted = _audioDefaultDevice.Muted;
                  _volume = (int)Math.Round(_audioDefaultDevice.MasterVolume * VolumeMaximum);
                }
              }
              catch (Exception ex)
              {
                Log.Error($"Mixer: Open(), Exception in Endpoint Volume API for Master: {ex}");
                _isMuted = false;
                _volume = 100;
              }
            }
            else if (OSInfo.OSInfo.VistaOrLater() && _componentType == MixerComponentType.SourceWave)
            {
              try
              {
                Log.Debug("Mixer: Open(), Endpoint Volume API for Wave");
                _audioDefaultDevice = new AEDev(resetDevice);
                if (_audioDefaultDevice != null)
                {
                  _audioDefaultDevice.OnVolumeNotification +=
                    new AudioEndpointVolumeNotificationDelegate(AudioEndpointVolume_OnVolumeNotification);
                }
              }
              catch (Exception ex)
              {
                Log.Error($"Mixer: Open(), Exception in Endpoint Volume API for Wave: {ex}");
              }
            }
    
            // Use Windows Multimedia mixer functions for XP and for Vista and later if wave volume is selected
            if (_componentType == MixerComponentType.SourceWave || !OSInfo.OSInfo.VistaOrLater())
            {
              if (_mixerEventListener == null)
              {
                _mixerEventListener = new MixerEventListener();
                _mixerEventListener.Start();
              }
              _mixerEventListener.LineChanged += new MixerEventHandler(OnLineChanged);
              _mixerEventListener.ControlChanged += new MixerEventHandler(OnControlChanged);
    
              MixerNativeMethods.MixerControl mc = new MixerNativeMethods.MixerControl();
    
              mc.Size = 0;
              mc.ControlId = 0;
              mc.ControlType = MixerControlType.Volume;
              mc.fdwControl = 0;
              mc.MultipleItems = 0;
              mc.ShortName = string.Empty;
              mc.Name = string.Empty;
              mc.Minimum = 0;
              mc.Maximum = 0;
              mc.Reserved = 0;
    
              IntPtr handle = IntPtr.Zero;
    
              if (
                MixerNativeMethods.mixerOpen(ref handle, mixerIndex, _mixerEventListener.Handle, 0,
                                             MixerFlags.CallbackWindow) !=
                MixerError.None)
              {
                throw new InvalidOperationException();
              }
    
              _handle = handle;
    
              _mixerControlDetailsVolume = GetControl(_componentType, MixerControlType.Volume);
              _mixerControlDetailsMute = GetControl(_componentType, MixerControlType.Mute);
    
              _isMuted = (int)GetValue(_componentType, MixerControlType.Mute) == 1;
              _volume = (int)GetValue(_componentType, MixerControlType.Volume);
              _isMutedWave = _isMuted;
              _volumeWave = _volume;
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error($"Mixer: error occured in Open(): {ex}");
        _isMuted = false;
        _volume = 100;
      }
    }

    private MixerNativeMethods.MixerControlDetails GetControl(MixerComponentType componentType,
                                                              MixerControlType controlType)
    {
      try
      {
        MixerNativeMethods.MixerLine mixerLine = new MixerNativeMethods.MixerLine(componentType);

        if (MixerNativeMethods.mixerGetLineInfoA(_handle, ref mixerLine, MixerLineFlags.ComponentType) !=
            MixerError.None)
        {
          throw new InvalidOperationException("Mixer.GetControl.1");
        }

        using (
          MixerNativeMethods.MixerLineControls mixerLineControls =
            new MixerNativeMethods.MixerLineControls(mixerLine.LineId, controlType))
        {
          if (MixerNativeMethods.mixerGetLineControlsA(_handle, mixerLineControls, MixerLineControlFlags.OneByType) !=
              MixerError.None)
          {
            throw new InvalidOperationException("Mixer.GetControl.2");
          }

          MixerNativeMethods.MixerControl mixerControl =
            (MixerNativeMethods.MixerControl)
              Marshal.PtrToStructure(mixerLineControls.Data, typeof(MixerNativeMethods.MixerControl));

          return new MixerNativeMethods.MixerControlDetails(mixerControl.ControlId);
        }
      }
      catch (Exception)
      {
        // Catch exception when audio device is disconnected
      }
      return null;
    }

    private object GetValue(MixerComponentType componentType, MixerControlType controlType)
    {
      try
      {
        MixerNativeMethods.MixerLine mixerLine = new MixerNativeMethods.MixerLine(componentType);

        if (MixerNativeMethods.mixerGetLineInfoA(_handle, ref mixerLine, MixerLineFlags.ComponentType) !=
            MixerError.None)
        {
          throw new InvalidOperationException("Mixer.OpenControl.1");
        }

        using (
          MixerNativeMethods.MixerLineControls mixerLineControls =
            new MixerNativeMethods.MixerLineControls(mixerLine.LineId, controlType))
        {
          MixerNativeMethods.mixerGetLineControlsA(_handle, mixerLineControls, MixerLineControlFlags.OneByType);
          MixerNativeMethods.MixerControl mixerControl =
            (MixerNativeMethods.MixerControl)
              Marshal.PtrToStructure(mixerLineControls.Data, typeof(MixerNativeMethods.MixerControl));

          using (
            MixerNativeMethods.MixerControlDetails mixerControlDetails =
              new MixerNativeMethods.MixerControlDetails(mixerControl.ControlId))
          {
            MixerNativeMethods.mixerGetControlDetailsA(_handle, mixerControlDetails, 0);

            return Marshal.ReadInt32(mixerControlDetails.Data);
          }
        }
      }
      catch (Exception)
      {
        // Catch exception when audio device is disconnected
      }
      // Set Volume to 30000 when audio recover
      return 30000;
    }

    private void SetValue(MixerNativeMethods.MixerControlDetails control, bool value)
    {
      if (control == null)
      {
        return;
      }

      Marshal.WriteInt32(control.Data, value ? 1 : 0);
      MixerNativeMethods.mixerSetControlDetails(_handle, control, 0);
    }

    private void SetValue(MixerNativeMethods.MixerControlDetails control, int value)
    {
      if (control == null)
      {
        return;
      }

      Marshal.WriteInt32(control.Data, value);
      MixerNativeMethods.mixerSetControlDetails(_handle, control, 0);
    }

    private void OnLineChanged(object sender, MixerEventArgs e)
    {
      LineChanged?.Invoke(sender, e);
    }

    private void OnControlChanged(object sender, MixerEventArgs e)
    {
      bool wasMuted = _isMuted;
      int lastVolume = _volume;
      _isMuted = (int)GetValue(_componentType, MixerControlType.Mute) == 1;
      _volume = (int)GetValue(_componentType, MixerControlType.Volume);

      if (ControlChanged != null && (wasMuted != _isMuted || lastVolume != _volume))
      {
        ControlChanged(sender, e);
      }
    }

    void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
    {
      try
      {
        if ((data?.MasterVolume == null) || IsMixerLocked())
        {
          Log.Debug("Mixer: AudioEndpointVolume_OnVolumeNotification early return,, IsMixerLocked():{0}, data null:{1}", IsMixerLocked(), (data?.MasterVolume == null));
          return;
        }
          
        using (MixerLock(lockTimeout))
        { 
          bool wasMuted = _isMuted;
          int lastVolume = _volume;
          bool waveChange = false;
                  
          bool isMutedMaster = _audioDefaultDevice.Muted;
          int volumeMaster = (int)Math.Round(_audioDefaultDevice.MasterVolume * VolumeMaximum);
          if (_useWave)
          {
            bool isMutedWave = (int)GetValue(_componentType, MixerControlType.Mute) == 1;
            int volumeWave = (int)GetValue(_componentType, MixerControlType.Volume);
            
            if ((isMutedWave != _isMutedWave) || (volumeWave != _volumeWave))
            {
              _isMutedWave = isMutedWave;
              _volumeWave = volumeWave;
              waveChange = true;
            }
          }
                    
          if (waveChange)
          {
            _isMuted = _isMutedWave;
            _volume = _volumeWave;
          }
          else
          {
            _isMuted = isMutedMaster;
            _volume = volumeMaster;
          }
  
          if (wasMuted != _isMuted || lastVolume != _volume)
          {
            Log.Debug("Mixer: AudioEndpointVolume change, new muted = {0}, new volume = {1}, old muted = {2}, old volume = {3}, waveChange = {4}", _isMuted, _volume, wasMuted, lastVolume, waveChange);
          }
    
          if (ControlChanged != null && (wasMuted != _isMuted || 
                                         lastVolume != _volume || 
                                         (_useWave && OSInfo.OSInfo.Win8OrLater() && (isMutedMaster != _isMuted)) ))
          {
            ControlChanged(null, null);
            if (_useWave && OSInfo.OSInfo.Win8OrLater() && (isMutedMaster != _isMuted))
            {
              SetValue(_mixerControlDetailsMute, isMutedMaster);
            }
          }
          
          if (VolumeHandler.Instance != null) VolumeHandler.Instance.mixer_UpdateVolume();
        }
      }
      catch (Exception ex)
      {
        Log.Error($"Mixer: error occured in AudioEndpointVolume_OnVolumeNotification(): {ex}");
      }
    }
    #endregion Methods

    #region Properties

    public bool IsMuted
    {
      get 
      { 
        lock (this) 
        {
          //Log.Debug("Mixer: Get IsMuted = {0}", _isMuted);
          return _isMuted; 
        }
      }
      set
      {
        try
        {
          using (MixerLock(lockTimeout))
          {
            //_isInternalVolumeChange = true;
            if (value != _isMuted)
            {
              Log.Debug("Mixer: Set new IsMuted = {0}, old IsMuted = {1}", value, _isMuted);
            }
            if (OSInfo.OSInfo.VistaOrLater() && (_componentType == MixerComponentType.DestinationSpeakers))
            {
              if (_audioDefaultDevice != null)
              {
                _audioDefaultDevice.Muted = value;
              }
            }
            else
            {
              //SetValue(_mixerControlDetailsMute, _isMuted = value);
              SetValue(_mixerControlDetailsMute, value);
              if (_useWave && OSInfo.OSInfo.Win8OrLater())
              {
                if (_audioDefaultDevice != null)
                {
                  _audioDefaultDevice.Muted = value;
                }
              }
            }
            if (VolumeHandler.Instance != null) VolumeHandler.Instance.mixer_UpdateVolume();
            //_isInternalVolumeChange = false;
          }
        }
        catch (Exception ex)
        {
          Log.Error($"Mixer: error occured in IsMuted(): {ex}");
        }
      }
    }


    public int Volume
    {
      get 
      { 
        lock (this) 
        {
          //Log.Debug("Mixer: Get Volume = {0}", _volume);
          return _volume; 
        }
      }
      set
      {
        try
        {
          using (MixerLock(lockTimeout))
          {
            //_isInternalVolumeChange = true;
            if (value != _volume)
            {
              Log.Debug("Mixer: Set new Volume = {0}, old Volume = {1}", value, _volume);
            }
            if (OSInfo.OSInfo.VistaOrLater() && (_componentType == MixerComponentType.DestinationSpeakers))
            {
              if (_audioDefaultDevice != null)
              {
                _audioDefaultDevice.MasterVolume = (float)((float)(value) / (float)(this.VolumeMaximum));
              }
            }
            else
            {
              //SetValue(_mixerControlDetailsVolume, _volume = Math.Max(this.VolumeMinimum, Math.Min(this.VolumeMaximum, value)));
              SetValue(_mixerControlDetailsVolume, Math.Max(this.VolumeMinimum, Math.Min(this.VolumeMaximum, value)));
              if (_useWave && OSInfo.OSInfo.Win8OrLater())
              {
                if (_audioDefaultDevice != null)
                {
                  _audioDefaultDevice.MasterVolume = (float)((float)(value) / (float)(this.VolumeMaximum));
                }
              }
            }
            if (VolumeHandler.Instance != null) VolumeHandler.Instance.mixer_UpdateVolume();
            //_isInternalVolumeChange = false;
          }
        }
        catch (Exception ex)
        {
          Log.Error($"Mixer: error occured in Volume(): {ex}");
        }
      }
    }

    public int VolumeMaximum
    {
      get { return 65535; }
    }

    public int VolumeMinimum
    {
      get { return 0; }
    }

    #endregion Properties

    #region Fields

    private MixerComponentType _componentType = MixerComponentType.DestinationSpeakers;
    private IntPtr _handle;
    private bool _isMuted;
    private int _volume;
    private bool _isMutedWave;
    private int _volumeWave;
    private static MixerEventListener _mixerEventListener;
    private MixerNativeMethods.MixerControlDetails _mixerControlDetailsVolume;
    private MixerNativeMethods.MixerControlDetails _mixerControlDetailsMute;
    public AEDev _audioDefaultDevice;
    private bool _useWave;
//    private bool _isInternalVolumeChange;
    private const int lockTimeout = 50; // milliseconds
    private const int lockInfinite = System.Threading.Timeout.Infinite;

    #endregion Fields
  }
  
}
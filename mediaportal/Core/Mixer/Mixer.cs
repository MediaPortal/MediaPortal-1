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
using System.Runtime.InteropServices;
using MediaPortal.ExtensionMethods;

namespace MediaPortal.Mixer
{
  public sealed class Mixer : IDisposable
  {
    #region Events

    public event MixerEventHandler LineChanged;
    public event MixerEventHandler ControlChanged;
   
    #endregion Events

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

    public void Open(int mixerIndex, bool isDigital)
    {
      lock (this)
      {
        _waveVolume = isDigital;
        if (isDigital)
        {
          _componentType = MixerComponentType.SourceWave;
        }
        else
        {
          _componentType = MixerComponentType.DestinationSpeakers;
        }
        // not enough to change this..

        // Use Endpoint Volume API for Vista/Win7 if master volume is selected and always for Win8 to handle muting of master volume
        if ((OSInfo.OSInfo.VistaOrLater() && _componentType == MixerComponentType.DestinationSpeakers) ||
            OSInfo.OSInfo.Win8OrLater())
        {
          try
          {
            _audioDefaultDevice = new AEDev();
            if (_audioDefaultDevice != null)
            {
              _audioDefaultDevice.OnVolumeNotification +=
                new AudioEndpointVolumeNotificationDelegate(AudioEndpointVolume_OnVolumeNotification);

              _isMuted = _audioDefaultDevice.Muted;
              _volume = (int) Math.Round(_audioDefaultDevice.MasterVolume*VolumeMaximum);
            }
          }
          catch (Exception)
          {
            _isMuted = false;
            _volume = 100;
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

          _isMuted = (int) GetValue(_componentType, MixerControlType.Mute) == 1;
          _volume = (int) GetValue(_componentType, MixerControlType.Volume);
        }
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
              Marshal.PtrToStructure(mixerLineControls.Data, typeof (MixerNativeMethods.MixerControl));

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
      if (LineChanged != null)
      {
        LineChanged(sender, e);
      }
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
      bool wasMuted = _isMuted;
      int lastVolume = _volume;
      _isMuted = _audioDefaultDevice.Muted;
      if (_waveVolume && OSInfo.OSInfo.Win8OrLater())
      {
        _isMutedVolume = (int) GetValue(_componentType, MixerControlType.Mute) == 1;
      }
      _volume = (int)Math.Round(_audioDefaultDevice.MasterVolume * VolumeMaximum);

      if (ControlChanged != null && (wasMuted != _isMuted || lastVolume != _volume))
      {
        ControlChanged(null, null);
        if (_waveVolume && OSInfo.OSInfo.Win8OrLater() && (_isMutedVolume != IsMuted))
        {
          SetValue(_mixerControlDetailsMute, _isMuted);
        }
      }
    }

    #endregion Methods

    #region Properties

    public bool IsMuted
    {
      get { lock (this) return _isMuted; }
      set
      {
        lock (this)
        {
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
            if (_waveVolume && OSInfo.OSInfo.Win8OrLater())
            {
              if (_audioDefaultDevice != null)
              {
                _audioDefaultDevice.Muted = value;
              }
            }
          }
        }
      }
    }


    public int Volume
    {
      get { lock (this) return _volume; }
      set
      {
        lock (this)
        {
          if (OSInfo.OSInfo.VistaOrLater() && (_componentType == MixerComponentType.DestinationSpeakers))
          {
            if (_audioDefaultDevice != null)
            {
              _audioDefaultDevice.MasterVolume = (float) ((float) (value)/(float) (this.VolumeMaximum));
            }
          }
          else
          {
            //SetValue(_mixerControlDetailsVolume, _volume = Math.Max(this.VolumeMinimum, Math.Min(this.VolumeMaximum, value)));
            SetValue(_mixerControlDetailsVolume, Math.Max(this.VolumeMinimum, Math.Min(this.VolumeMaximum, value)));
            if (_waveVolume && OSInfo.OSInfo.Win8OrLater())
            {
              if (_audioDefaultDevice != null)
              {
                _audioDefaultDevice.MasterVolume = (float) ((float) (value)/(float) (this.VolumeMaximum));
              }
            }
          }
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
    private bool _isMutedVolume;
    private static MixerEventListener _mixerEventListener;
    private int _volume;
    private MixerNativeMethods.MixerControlDetails _mixerControlDetailsVolume;
    private MixerNativeMethods.MixerControlDetails _mixerControlDetailsMute;
    private AEDev _audioDefaultDevice;
    private bool _waveVolume;

    #endregion Fields
  }
}
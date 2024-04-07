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

using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using CSCore.CoreAudioAPI;
using MediaPortal.ExtensionMethods;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using Microsoft.Win32;
using Action = MediaPortal.GUI.Library.Action;

namespace MediaPortal.Player
{
  /// <summary>
  /// Looks like this class is notably taking care of broadcasting various volume change events.
  /// This is intended to be used as a singleton.
  /// The singleton instance should be created from MP main thread.
  /// </summary>
  public class VolumeHandler
  {
    #region Vars

    static HideVolumeOSD.HideVolumeOSDLib VolumeOSD;
    static MMDeviceEnumerator _MMdeviceEnumerator = new MMDeviceEnumerator();
    static int _volumeStyle = 0;

    #endregion

    #region Constructors

    public VolumeHandler() : this(LoadFromRegistry()) { }

    public VolumeHandler(int[] volumeTable)
    {
      if (OSInfo.OSInfo.Win10OrLater())
      {
        if (_MMdeviceEnumerator == null)
          _MMdeviceEnumerator = new MMDeviceEnumerator();

        var mMdeviceList = _MMdeviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active);

        if (mMdeviceList.Count > 0)
        {

          var mMdevice = _MMdeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
          if (mMdevice != null)
          {
            using (Settings reader = new MPSettings())
            {
              int levelStyle = reader.GetValueAsInt("volume", "startupstyle", 0);

              if (levelStyle == 0)
              {
                _startupVolume = Math.Max(0, Math.Min(65535, reader.GetValueAsInt("volume", "lastknown", 52428)));
              }

              if (levelStyle == 1)
              {
              }

              if (levelStyle == 2)
              {
                _startupVolume = Math.Max(0, Math.Min(65535, reader.GetValueAsInt("volume", "startuplevel", 52428)));
              }

              IsDigital = reader.GetValueAsBool("volume", "digital", false);

              _showVolumeOSD = reader.GetValueAsBool("volume", "defaultVolumeOSD", true);

              hideWindowsOSD = reader.GetValueAsBool("volume", "hideWindowsOSD", false);
            }

            try
            {
              _volumeTable = volumeTable;
              _mixer10 = new Mixer.Mixer10();
              _mixer10.Open(0, IsDigital, volumeTable);
            }
            catch (Exception ex)
            {
              Log.Error("VolumeHandler: Mixer exception during init {0}", ex);
            }

            if (OSInfo.OSInfo.Win8OrLater() && hideWindowsOSD)
            {
              try
              {
                bool tempShowVolumeOSD = _showVolumeOSD;

                _showVolumeOSD = true;

                VolumeOSD = new HideVolumeOSD.HideVolumeOSDLib(IsMuted);
                VolumeOSD.HideOSD();

                _showVolumeOSD = tempShowVolumeOSD;
              }
              catch
              {
              }
            }
          }
        }
        else
        {
          _volumeTable = volumeTable;
        }
      }
      else
      {
        if (GUIGraphicsContext.DeviceAudioConnected > 0)
        {
          using (Settings reader = new MPSettings())
          {
            int levelStyle = reader.GetValueAsInt("volume", "startupstyle", 0);

            if (levelStyle == 0)
            {
              _startupVolume = Math.Max(0, Math.Min(65535, reader.GetValueAsInt("volume", "lastknown", 52428)));
            }

            if (levelStyle == 1)
            {
            }

            if (levelStyle == 2)
            {
              _startupVolume = Math.Max(0, Math.Min(65535, reader.GetValueAsInt("volume", "startuplevel", 52428)));
            }

            IsDigital = reader.GetValueAsBool("volume", "digital", false);

            _showVolumeOSD = reader.GetValueAsBool("volume", "defaultVolumeOSD", true);

            hideWindowsOSD = reader.GetValueAsBool("volume", "hideWindowsOSD", false);
          }

          try
          {
            _mixer = new Mixer.Mixer();
            _mixer.Open(0, IsDigital);
            _volumeTable = volumeTable;
            _mixer.ControlChanged += mixer_ControlChanged;
          }
          catch (Exception ex)
          {
            Log.Error("VolumeHandler: Mixer exception when init {0}", ex);
          }

          if (OSInfo.OSInfo.Win8OrLater() && hideWindowsOSD)
          {
            try
            {
              bool tempShowVolumeOSD = _showVolumeOSD;

              _showVolumeOSD = true;

              VolumeOSD = new HideVolumeOSD.HideVolumeOSDLib(IsMuted);
              VolumeOSD.HideOSD();

              _showVolumeOSD = tempShowVolumeOSD;
            }
            catch
            {
            }
          }
        }
        else
        {
          _volumeTable = volumeTable;
        }
      }
    }

    public bool hideWindowsOSD { get; set; }

    #endregion Constructors

    #region Methods

    /// <summary>
    /// Create our volume handler singleton.
    /// </summary>
    public static void CreateInstance()
    {
      if (OSInfo.OSInfo.Win10OrLater())
      {
        _instance = Create();
      }
      else
      {
        if (_instance == null)
        {
          _instance = Create();
        }
      }
    }

    /// <summary>
    /// Create a volume handler.
    /// </summary>
    /// <returns>A newly created volume handler.</returns>
    private static VolumeHandler Create()
    {
      if (OSInfo.OSInfo.Win10OrLater())
      {
        if (_MMdeviceEnumerator == null)
          _MMdeviceEnumerator = new MMDeviceEnumerator();

        var mMdeviceList = _MMdeviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active);

        if (mMdeviceList.Count > 0)
        {
          var mMdevice = _MMdeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
          if (mMdevice != null)
          {
            using (Settings reader = new MPSettings())
            {
              _volumeStyle = reader.GetValueAsInt("volume", "handler", 1);

              switch (_volumeStyle)
              {
                // classic volume table
                case 0:
                  return new VolumeHandler(new[]
                    {0, 6553, 13106, 19659, 26212, 32765, 39318, 45871, 52424, 58977, 65535});
                // windows default from registry
                case 1:
                  return new VolumeHandler();
                // logarithmic
                case 2:
                  return new VolumeHandler(new[]
                  {
                    0, 1039, 1234, 1467, 1744, 2072, 2463, 2927, 3479, 4135, 4914, 5841, 6942, 8250,
                    9806, 11654, 13851, 16462, 19565, 23253, 27636, 32845, 39037, 46395, 55141, 65535
                  });
                // custom user setting
                case 3:
                  return new VolumeHandlerCustom();
                // defaults to vista safe "0, 4095, 8191, 12287, 16383, 20479, 24575, 28671, 32767, 36863, 40959, 45055, 49151, 53247, 57343, 61439, 65535"
                // Vista recommended values
                case 4:
                  return new VolumeHandler(new[]
                  {
                    0, 4095, 8191, 12287, 16383, 20479, 24575, 28671, 32767, 36863, 40959, 45055,
                    49151,
                    53247, 57343, 61439, 65535
                  });
                // Windows 10
                case 5:
                  return new VolumeHandler(new[]
                  {
                    0, 1310, 2620, 3930, 5240, 6550, 7860, 9170, 10480, 11790, 13100, 14410, 15720, 17030, 18340, 19650,
                    20960, 22270, 23580, 24890, 26200, 27510, 28820, 30130, 31440,
                    32750, 34060, 35370, 36680, 37990, 39300, 40610, 41920, 43230, 44540, 45850, 47160, 48470, 49780,
                    51090, 52400, 53710, 55020, 56330, 57640, 58950, 60260, 61570,
                    62880, 64190, 65535
                  });
                default:
                  return new VolumeHandlerCustom();
              }
            }
          }
        }
      }
      else
      {
        if (GUIGraphicsContext.DeviceAudioConnected > 0)
        {
          using (Settings reader = new MPSettings())
          {
            int volumeStyle = reader.GetValueAsInt("volume", "handler", 1);

            switch (volumeStyle)
            {
              // classic volume table
              case 0:
                return new VolumeHandler(new[]
                  {0, 6553, 13106, 19659, 26212, 32765, 39318, 45871, 52424, 58977, 65535});
              // windows default from registry
              case 1:
                return new VolumeHandler();
              // logarithmic
              case 2:
                return new VolumeHandler(new[]
                {
                  0, 1039, 1234, 1467, 1744, 2072, 2463, 2927, 3479, 4135, 4914, 5841, 6942, 8250,
                  9806, 11654, 13851, 16462, 19565, 23253, 27636, 32845, 39037, 46395, 55141, 65535
                });
              // custom user setting
              case 3:
                return new VolumeHandlerCustom();
              // defaults to vista safe "0, 4095, 8191, 12287, 16383, 20479, 24575, 28671, 32767, 36863, 40959, 45055, 49151, 53247, 57343, 61439, 65535"
              // Vista recommended values
              case 4:
                return new VolumeHandler(new[]
                {
                  0, 4095, 8191, 12287, 16383, 20479, 24575, 28671, 32767, 36863, 40959, 45055,
                  49151,
                  53247, 57343, 61439, 65535
                });
              // Windows 10
              case 5:
                return new VolumeHandler(new[]
                {
                  0, 1310, 2620, 3930, 5240, 6550, 7860, 9170, 10480, 11790, 13100, 14410, 15720, 17030, 18340, 19650,
                  20960, 22270, 23580, 24890, 26200, 27510, 28820, 30130, 31440,
                  32750, 34060, 35370, 36680, 37990, 39300, 40610, 41920, 43230, 44540, 45850, 47160, 48470, 49780,
                  51090, 52400, 53710, 55020, 56330, 57640, 58950, 60260, 61570,
                  62880, 64190, 65535
                });
              default:
                return new VolumeHandlerCustom();
            }
          }
        }
      }
      return new VolumeHandlerCustom();
    }

    public virtual int VolumeStyle()
    {
      return _volumeStyle;
    }

    public static void Dispose()
    {
      if (_instance == null)
      {
        return;
      }

      if (OSInfo.OSInfo.Win10OrLater())
      {
        if (_instance._mixer10 != null)
        {
          using (Settings writer = new MPSettings())
          {
            writer.SetValue("volume", "lastknown", _instance._mixer10.Volume);
          }

          _instance._mixer10.SafeDispose();
          _instance._mixer10 = null;
        }
      }
      else if (_instance._mixer != null)
      {
        using (Settings writer = new MPSettings())
        {
          writer.SetValue("volume", "lastknown", _instance._mixer.Volume);
        }

        _instance._mixer.ControlChanged -= mixer_ControlChanged;

        _instance._mixer.SafeDispose();
        _instance._mixer = null;
      }
      _instance = null;
      GUIGraphicsContext.VolumeHandler = null;
    }

    public virtual void UnMute()
    {
      if (OSInfo.OSInfo.Win10OrLater())
      {
        _mixer10.IsMuted = false;
      }
      else
      {
        _mixer.IsMuted = false;
      }
    }

    private static int[] LoadFromRegistry()
    {
      using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Multimedia\Audio\VolumeControl"))
      {
        if (key == null)
        {
          return SystemTable;
        }

        if (Equals(key.GetValue("EnableVolumeTable", 0), 0))
        {
          return SystemTable;
        }

        var buffer = (byte[])key.GetValue("VolumeTable", null);

        if (buffer == null)
        {
          return SystemTable;
        }

        // Windows documentation states that a volume table must consist of between 11 and 201 entries
        if ((buffer.Length / 4) < 11 || (buffer.Length / 4) > 201)
        {
          return SystemTable;
        }

        // create an array large enough to hold the system's volume table
        var volumeTable = new int[buffer.Length / 4];

        for (int index = 0, offset = 0; index < volumeTable.Length; index++, offset += 4)
        {
          volumeTable[index] = Marshal.ReadInt32(buffer, offset);
        }

        return volumeTable;
      }
    }

    protected virtual void SetVolume(int volume)
    {
      if (OSInfo.OSInfo.Win10OrLater())
      {
        if (_mixer10 != null)
        {
          if (_mixer10.IsMuted)
          {
            _mixer10.IsMuted = false;
          }
          _mixer10.Volume = volume;
        }
      }
      else
      {
        if (_mixer != null)
        {
          // Check if mixer is still attached to the audio device we started with
          if (_mixer._audioDefaultDevice != null &&
              _mixer._audioDefaultDevice.DeviceId != _mixer._audioDefaultDevice.DeviceIdCurrent)
          {
            _mixer = new Mixer.Mixer();
            _mixer.Open(0, IsDigital, true);
            _mixer.ControlChanged += mixer_ControlChanged;

            if (_mixer == null)
              return;
          }

          if (_mixer.IsMuted)
          {
            _mixer.IsMuted = false;
          }

          _mixer.Volume = volume;
        }
      }
    }

    protected internal virtual void ChangeAudioDevice(string deviceName, bool setToDefault)
    {
      if (OSInfo.OSInfo.Win10OrLater())
      {
        _mixer10?.ChangeAudioDevice(deviceName, setToDefault);
      }
    }

    protected internal virtual bool DetectedDevice()
    {
      bool validate = false;
      if (OSInfo.OSInfo.Win10OrLater())
      {
        var device = _mixer10?.DetectedDevice();
        if (device != null)
        {
          validate = true;
        }
      }
      return validate;
    }

    protected virtual void SetVolume(bool isMuted)
    {
      if (OSInfo.OSInfo.Win10OrLater())
      {
        if (_mixer10 != null)
        {
          _mixer10.IsMuted = isMuted;
        }
      }
      else
      {
        if (_mixer != null)
        {
          _mixer.IsMuted = isMuted;
        }
      }
    }

    private void HandleGUIOnControlChange()
    {
      try
      {
        var msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_AUDIOVOLUME_CHANGED, 0, 0, 0, 0, 0, 0)
        {
          Label = Instance.Step.ToString(CultureInfo.InvariantCulture),
          Label2 = Instance.StepMax.ToString(CultureInfo.InvariantCulture),
          Label3 = Instance.IsMuted.ToString(CultureInfo.InvariantCulture)
        };
        GUIGraphicsContext.SendMessage(msg);

        var showVolume = new Action(Action.ActionType.ACTION_SHOW_VOLUME, 0, 0);
        GUIWindowManager.OnAction(showVolume);
      }
      catch (Exception ex)
      {
        Log.Error($"VolumeHandler: error occured in HandleGUIOnControlChange: {ex}");
      }
    }

    private static void mixer_ControlChanged(object sender, Mixer.MixerEventArgs e)
    {
      Instance.HandleGUIOnControlChange();
      GUIGraphicsContext.VolumeOverlay = true;
      GUIGraphicsContext.VolumeOverlayTimeOut = DateTime.Now;
      Instance.UpdateVolumeProperties();
    }

    public void mixer_UpdateVolume()
    {
      Instance.HandleGUIOnControlChange();
      GUIGraphicsContext.VolumeOverlay = true;
      GUIGraphicsContext.VolumeOverlayTimeOut = DateTime.Now;
      Instance.UpdateVolumeProperties();
    }

    public void UpdateVolumeProperties()
    {
      float fRange = (float)(Instance.Maximum - Instance.Minimum);
      float fPos = (float)(Instance.Volume - Instance.Minimum);
      float fPercent = (fPos / fRange) * 100.0f;
      GUIPropertyManager.SetProperty("#volume.percent", ((int)Math.Round(fPercent)).ToString());
      GUIPropertyManager.SetProperty("#volume.mute", Instance.IsMuted.ToString().ToLowerInvariant());
    }

    #endregion Methods

    #region Properties

    public virtual int Volume
    {
      get
      {
        if (OSInfo.OSInfo.Win10OrLater())
        {
          if (_mixer10 != null)
          {
            return _mixer10.Volume;
          }
        }
        else
        {
          if (_mixer != null)
          {
            return _mixer.Volume;
          }
        }

        return 0;
      }
      set { SetVolume(value); }
    }

    public virtual bool IsMuted
    {
      get
      {
        if (OSInfo.OSInfo.Win10OrLater())
        {
          if (_mixer10 != null)
          {
            return _mixer10.IsMuted;
          }
        }
        else
        {
          if (_mixer != null)
          {
            return _mixer.IsMuted;
          }
        }

        return false;
      }
      set { SetVolume(value); }
    }

    public virtual bool IsEnabledVolumeOSD
    {
      get { return _showVolumeOSD; }
      set { _showVolumeOSD = value; }
    }

    public virtual int Next
    {
      get
      {
        if (_volumeTable != null)
        {
          lock (_volumeTable)
            foreach (int vol in _volumeTable.Where(vol => Volume < vol))
            {
              return vol;
            }
          return Maximum;
        }
        return 0;
      }
    }

    public virtual int Maximum
    {
      get { return 65535; }
    }

    public virtual int Minimum
    {
      get { return 0; }
    }

    public virtual int Step
    {
      get
      {
        lock (_volumeTable)
          for (int index = 0; index < _volumeTable.Length; ++index)
          {
            if (Volume <= _volumeTable[index])
            {
              return index;
            }
          }
        return _volumeTable.Length;
      }
    }

    public virtual int StepMax
    {
      get { lock (_volumeTable) return _volumeTable.Length; }
    }

    public virtual int Previous
    {
      get
      {
        if (_volumeTable != null)
        {
          lock (_volumeTable)
            for (int index = _volumeTable.Length - 1; index >= 0; --index)
            {
              if (Volume > _volumeTable[index])
              {
                return _volumeTable[index];
              }
            }
          return Minimum;
        }
        return 0;
      }
    }

    protected virtual int[] Table
    {
      set { lock (_volumeTable) _volumeTable = value; }
    }

    /// <summary>
    /// Provide our instance singleton.
    /// Can be null until explicitly created through CreateInstance.
    /// </summary>
    public static VolumeHandler Instance
    {
      get
      {
//        if (OSInfo.OSInfo.Win10OrLater())
//        {
//          if (_instance == null)
//            CreateInstance();
//          return _instance;
//        }

        if (_instance == null)
          CreateInstance();

        return _instance;
      }
    }


    #endregion Properties

    #region Fields

    private Mixer.Mixer _mixer;
    private Mixer.Mixer10 _mixer10;

    private static VolumeHandler _instance;
    public static bool IsDigital;

    private static readonly int[] SystemTable = new[]
                                                   {
                                                     0, 1039, 1234, 1467, 1744, 2072,
                                                     2463, 2927, 3479, 4135, 4914, 5841,
                                                     6942, 8250, 9806, 11654, 13851, 16462,
                                                     19565, 23253, 27636, 32845, 39037, 46395,
                                                     55141, 65535
                                                   };

    private int[] _volumeTable;
    private int _startupVolume;
    private static bool _showVolumeOSD;

    #endregion Fields
  }
}
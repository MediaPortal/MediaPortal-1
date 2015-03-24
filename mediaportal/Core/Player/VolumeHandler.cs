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
using MediaPortal.ExtensionMethods;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using Microsoft.Win32;
using Action = MediaPortal.GUI.Library.Action;

namespace MediaPortal.Player
{
  public class VolumeHandler
  {
    #region Vars

    public static class Win32Api
    {
      public const int CM_LOCATE_DEVNODE_NORMAL = 0x00000000;
      public const int CM_REENUMERATE_NORMAL = 0x00000000;
      public const int CR_SUCCESS = 0x00000000;

      [DllImport("CfgMgr32.dll", SetLastError = true)]
      public static extern int CM_Locate_DevNodeA(ref int pdnDevInst, string pDeviceID, int ulFlags);

      [DllImport("CfgMgr32.dll", SetLastError = true)]
      public static extern int CM_Reenumerate_DevNode(int dnDevInst, int ulFlags);
    }

    #endregion

    #region Constructors

    public VolumeHandler() : this(LoadFromRegistry()) {}

    public VolumeHandler(int[] volumeTable)
    {
      if (GUIGraphicsContext.DeviceAudioConnected)
      {
        bool isDigital;

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

          isDigital = reader.GetValueAsBool("volume", "digital", false);

          _showVolumeOSD = reader.GetValueAsBool("volume", "defaultVolumeOSD", true);
        }

        try
        {
          _mixer = new Mixer.Mixer();
          _mixer.Open(0, isDigital);
          _volumeTable = volumeTable;
          _mixer.ControlChanged += mixer_ControlChanged;
        }
        catch (Exception ex)
        {
          Log.Error("VolumeHandler: Mixer exception when init {0}", ex);
          int pdnDevInst = 0;

          if (Win32Api.CM_Locate_DevNodeA(ref pdnDevInst, null, Win32Api.CM_LOCATE_DEVNODE_NORMAL) != Win32Api.CR_SUCCESS)
          {
            throw new Exception("something...");
          }

          if (Win32Api.CM_Reenumerate_DevNode(pdnDevInst, Win32Api.CM_REENUMERATE_NORMAL) != Win32Api.CR_SUCCESS)
          {
            Log.Error("VolumeHandler: Audio device not refreshed when init {0}", ex);
          }
        }
      }
      else
      {
        _volumeTable = volumeTable;
      }
    }

    #endregion Constructors

    #region Methods

    private static VolumeHandler CreateInstance()
    {
      if (GUIGraphicsContext.DeviceAudioConnected)
      {
        using (Settings reader = new MPSettings())
        {
          int volumeStyle = reader.GetValueAsInt("volume", "handler", 1);

          switch (volumeStyle)
          {
              // classic volume table
            case 0:
              return new VolumeHandler(new[] {0, 6553, 13106, 19659, 26212, 32765, 39318, 45871, 52424, 58977, 65535});
              // windows default from registry
            case 1:
              return new VolumeHandler();
              // logarithmic
            case 2:
              return new VolumeHandler(new[]
                                       {
                                         0, 1039, 1234, 1467, 1744, 2072, 2463, 2927, 3479, 4135, 4914, 5841, 6942, 8250,
                                         9806
                                         , 11654, 13851, 16462, 19565, 23253, 27636, 32845, 39037, 46395, 55141, 65535
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
            default:
              return new VolumeHandlerCustom();
          }
        }
      }
      return new VolumeHandlerCustom();
    }

    public static void Dispose()
    {
      if (_instance == null)
      {
        return;
      }
      if (_instance._mixer != null)
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
      _mixer.IsMuted = false;
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
      if (_mixer != null)
      {
        if (_mixer.IsMuted)
        {
          _mixer.IsMuted = false;
        }
        _mixer.Volume = volume;
      }
    }

    protected virtual void SetVolume(bool isMuted)
    {
      if (_mixer != null)
      {
        _mixer.IsMuted = isMuted;
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
      catch (Exception e)
      {
        Log.Info("VolumeHandler.HandleGUIOnControlChange: {0}", e.ToString());
      }
    }

    private static void mixer_ControlChanged(object sender, Mixer.MixerEventArgs e)
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
        if (_mixer != null)
        {
          return _mixer.Volume;
        }
        return 0;
      }
      set { SetVolume(value); }
    }

    public virtual bool IsMuted
    {
      get
      {
        if (_mixer != null)
        {
          return _mixer.IsMuted;
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

    public static VolumeHandler Instance
    {
      get { return _instance ?? (_instance = CreateInstance()); }
    }

    #endregion Properties

    #region Fields

    private Mixer.Mixer _mixer;
    private static VolumeHandler _instance;

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
    private bool _showVolumeOSD;

    #endregion Fields
  }
}
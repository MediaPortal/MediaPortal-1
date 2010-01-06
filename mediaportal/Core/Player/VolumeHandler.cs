#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Runtime.InteropServices;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using Microsoft.Win32;

namespace MediaPortal.Player
{
  public class VolumeHandler
  {
    #region Vars

    #endregion

    #region Constructors

    public VolumeHandler()
      : this(LoadFromRegistry()) {}

    public VolumeHandler(int[] volumeTable)
    {
      bool isDigital = true;
      //string mixerControlledComponent = "Wave";

      using (Settings reader = new MPSettings())
      {
        int levelStyle = reader.GetValueAsInt("volume", "startupstyle", 0);

        if (levelStyle == 0)
        {
          _startupVolume = Math.Max(0, Math.Min(65535, reader.GetValueAsInt("volume", "lastknown", 52428)));
        }

        if (levelStyle == 1)
        {
          _startupVolume = _mixer.Volume;
        }

        if (levelStyle == 2)
        {
          _startupVolume = Math.Max(0, Math.Min(65535, reader.GetValueAsInt("volume", "startuplevel", 52428)));
        }

        //mixerControlledComponent = reader.GetValueAsString("volume", "controlledMixer", "Wave");
        isDigital = reader.GetValueAsBool("volume", "digital", false);

        _showVolumeOSD = reader.GetValueAsBool("volume", "defaultVolumeOSD", true);
      }

      _mixer = new Mixer.Mixer();
      _mixer.Open(0, isDigital);
      _volumeTable = volumeTable;
    }

    #endregion Constructors

    #region Methods

    private static VolumeHandler CreateInstance()
    {
      using (Settings reader = new MPSettings())
      {
        int volumeStyle = reader.GetValueAsInt("volume", "handler", 1);

        switch (volumeStyle)
        {
            // classic volume table
          case 0:
            return new VolumeHandler(new int[] {0, 6553, 13106, 19659, 26212, 32765, 39318, 45871, 52424, 58977, 65535});
            // windows default from registry
          case 1:
            return new VolumeHandler();
            // logarithmic
          case 2:
            return
              new VolumeHandler(new int[]
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
            return
              new VolumeHandler(new int[]
                                  {
                                    0, 4095, 8191, 12287, 16383, 20479, 24575, 28671, 32767, 36863, 40959, 45055, 49151,
                                    53247, 57343, 61439, 65535
                                  });
          default:
            return new VolumeHandlerCustom();
        }
      }
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

        _instance._mixer.Dispose();
        _instance._mixer = null;
      }
      _instance = null;
    }

    private static int[] LoadFromRegistry()
    {
      using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Multimedia\Audio\VolumeControl"))
      {
        if (key == null)
        {
          return _systemTable;
        }

        if (Equals(key.GetValue("EnableVolumeTable", 0), 0))
        {
          return _systemTable;
        }

        byte[] buffer = (byte[])key.GetValue("VolumeTable", null);

        if (buffer == null)
        {
          return _systemTable;
        }

        // Windows documentation states that a volume table must consist of between 11 and 201 entries
        if ((buffer.Length / 4) < 11 || (buffer.Length / 4) > 201)
        {
          return _systemTable;
        }

        // create an array large enough to hold the system's volume table
        int[] volumeTable = new int[buffer.Length / 4];

        for (int index = 0, offset = 0; index < volumeTable.Length; index++, offset += 4)
        {
          volumeTable[index] = Marshal.ReadInt32(buffer, offset);
        }

        return volumeTable;
      }
    }

    protected virtual void SetVolume(int volume)
    {
      try
      {
        _mixer.Volume = volume;

        if (_mixer.IsMuted)
        {
          _mixer.IsMuted = false;
        }

        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_AUDIOVOLUME_CHANGED, 0, 0, 0, 0, 0, 0);
        msg.Label = Instance.Step.ToString();
        msg.Label2 = Instance.StepMax.ToString();
        msg.Label3 = Instance.IsMuted.ToString();
        GUIGraphicsContext.SendMessage(msg);

        if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVFULLSCREEN ||
            GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO && _showVolumeOSD)
        {
          Action showVolume = new Action(Action.ActionType.ACTION_SHOW_VOLUME, 0, 0);
          GUIWindowManager.OnAction(showVolume);
        }
      }
      catch (Exception e)
      {
        Log.Info("VolumeHandler.SetVolume: {0}", e.Message);
      }
    }

    protected virtual void SetVolume(bool isMuted)
    {
      try
      {
        _mixer.IsMuted = isMuted;


        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_AUDIOVOLUME_CHANGED, 0, 0, 0, 0, 0, 0);
        msg.Label = Instance.Step.ToString();
        msg.Label2 = Instance.StepMax.ToString();
        msg.Label3 = Instance.IsMuted.ToString();
        GUIGraphicsContext.SendMessage(msg);


        if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVFULLSCREEN ||
            GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO && _showVolumeOSD)
        {
          Action showVolume = new Action(Action.ActionType.ACTION_SHOW_VOLUME, 0, 0);
          GUIWindowManager.OnAction(showVolume);
        }
      }
      catch (Exception e)
      {
        Log.Info("VolumeHandler.SetVolume: {0}", e.Message);
      }
    }

    #endregion Methods

    #region Properties

    public virtual int Volume
    {
      get { return _mixer.Volume; }
      set { SetVolume(value); }
    }

    public virtual bool IsMuted
    {
      get { return _mixer.IsMuted; }
      set { SetVolume(value); }
    }

    public virtual int Next
    {
      get
      {
        lock (_volumeTable)
          for (int index = 0; index < _volumeTable.Length; ++index)
          {
            if (this.Volume < _volumeTable[index])
            {
              return _volumeTable[index];
            }
          }
        return this.Maximum;
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
            if (this.Volume <= _volumeTable[index])
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
        lock (_volumeTable)
          for (int index = _volumeTable.Length - 1; index >= 0; --index)
          {
            if (this.Volume > _volumeTable[index])
            {
              return _volumeTable[index];
            }
          }
        return this.Minimum;
      }
    }

    protected virtual int[] Table
    {
      set { lock (_volumeTable) _volumeTable = value; }
    }

    public static VolumeHandler Instance
    {
      get { return _instance == null ? _instance = CreateInstance() : _instance; }
    }

    #endregion Properties

    #region Fields

    private Mixer.Mixer _mixer;
    private static VolumeHandler _instance;

    private readonly int[] _linearTable = new int[]
                                            {
                                              0, 2621, 5243, 7864, 10486, 13107,
                                              15728, 18350, 20971, 23593, 26214, 28835,
                                              31457, 34078, 36700, 39321, 41942, 44564,
                                              47185, 49807, 52428, 55049, 57671, 60292,
                                              62914, 65535
                                            };

    private static readonly int[] _systemTable = new int[]
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
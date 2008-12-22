/* 
 *	Copyright (C) 2005-2008 Team MediaPortal - micheloe, patrick, diehard2
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


using System;
using System.Collections.Generic;
using System.Text;

using DirecTV;

using MediaPortal.GUI.Library;

namespace ProcessPlugins.DirectTVTunerPlugin
{
  public class DirecTVSettings
  {
    #region global variables

    public static readonly string[] BoxTypes = new string[4] { "RCA (Old)", "RCA (New)", "D10-100", "D10-200" };
    public static readonly string[] KeyMaps = new string[2] { "RCA keymap", "D10-100/D10-200 keymap" };

    private const string _sectionName = "directvtuner";

    #endregion

    #region private variables

    private SerialInterface.BoxType _type;
    private CommandSet _commandSet;
    private KeyMap _keyMap;
    private string _boxName;
    private string _keyMapName;
    private string _serialPortName;
    private int _baudRate;
    private int _readTimeout;
    private bool _useOldCommandSet;
    private bool _useSetChannelForTune;
    private bool _poweron;
    private bool _debug;
    private bool _advanced;
    private bool _twowaydisable;
    private bool _hideOSD;
    private bool _allowDigitalSubchannels;

    #endregion

    #region C#tor

    public DirecTVSettings()
    {
    }

    #endregion

    #region Serialization

    public void LoadSettings()
    {
      using (MediaPortal.Profile.Settings reader = new MediaPortal.Profile.Settings("mediaportal.xml"))
      {
        _boxName = reader.GetValueAsString(_sectionName, "boxtype", BoxTypes[1]);
        _useOldCommandSet = reader.GetValueAsBool(_sectionName, "oldcommandset", false);
        _keyMapName = reader.GetValueAsString(_sectionName, "keymap", KeyMaps[1]);
        _serialPortName = reader.GetValueAsString(_sectionName, "serialportname", "COM1");
        _baudRate = reader.GetValueAsInt(_sectionName, "baudrate", 9600);
        _readTimeout = reader.GetValueAsInt(_sectionName, "readtimeout", 1000);
        _poweron = reader.GetValueAsBool(_sectionName, "poweron", false);
        _useSetChannelForTune = reader.GetValueAsBool(_sectionName, "usesetchannel", false);
        _debug = reader.GetValueAsBool(_sectionName, "debug", false);
        _advanced = reader.GetValueAsBool(_sectionName, "advanced", false);
        _twowaydisable = reader.GetValueAsBool(_sectionName, "twowaydisable", false);
        _hideOSD = reader.GetValueAsBool(_sectionName, "hideOSD", false);
        _allowDigitalSubchannels = reader.GetValueAsBool(_sectionName, "allowsubchannels", false);
      }
      BoxName = _boxName;
      KeyMapName = _keyMapName;
      UseOldCommandSet = _useOldCommandSet;
    }

    public void SaveSettings()
    {
      using (MediaPortal.Profile.Settings writer = new MediaPortal.Profile.Settings("mediaportal.xml"))
      {
        writer.SetValue(_sectionName, "boxtype", _boxName);
        writer.SetValueAsBool(_sectionName, "oldcommandset", _useOldCommandSet);
        writer.SetValue(_sectionName, "keymap", _keyMapName);
        writer.SetValue(_sectionName, "serialportname", _serialPortName);
        writer.SetValue(_sectionName, "baudrate", _baudRate);
        writer.SetValue(_sectionName, "readtimeout", _readTimeout);
        writer.SetValueAsBool(_sectionName, "poweron", _poweron);
        writer.SetValueAsBool(_sectionName, "usesetchannel", _useSetChannelForTune);
        writer.SetValueAsBool(_sectionName, "debug", _debug);
        writer.SetValueAsBool(_sectionName, "advanced", _advanced);
        writer.SetValueAsBool(_sectionName, "twowaydisable", _twowaydisable);
        writer.SetValueAsBool(_sectionName, "hideOSD", _hideOSD);
        writer.SetValueAsBool(_sectionName, "allowsubchannels", _allowDigitalSubchannels);
      }
    }

    #endregion

    #region Properties

    public SerialInterface.BoxType BoxType
    {
      get { return _type; }
      set
      {
        switch (value)
        {
          case SerialInterface.BoxType.RCA_Old:
            _boxName = BoxTypes[0];
            break;
          case SerialInterface.BoxType.RCA_New:
            _boxName = BoxTypes[1];
            break;
          case SerialInterface.BoxType.D10_100:
            _boxName = BoxTypes[2];
            break;
          case SerialInterface.BoxType.D10_200:
            _boxName = BoxTypes[3];
            break;
          default:
            throw new ArgumentException("Invalid BoxType specified", value.ToString());
        }
        _type = value;
      }
    }

    public string BoxName
    {
      get { return _boxName; }
      set
      {
        if (value.Equals(BoxTypes[0]))
        {
          _type = SerialInterface.BoxType.RCA_Old;
        }
        else if (value.Equals(BoxTypes[1]))
        {
          _type = SerialInterface.BoxType.RCA_New;
        }
        else if (value.Equals(BoxTypes[2]))
        {
          _type = SerialInterface.BoxType.D10_100;
        }
        else if (value.Equals(BoxTypes[3]))
        {
          _type = SerialInterface.BoxType.D10_200;
        }
        else
        {
          throw new ArgumentException("Invalid BoxType specified", value);
        }
        _boxName = value;
      }
    }

    public KeyMap KeyMap
    {
      get { return _keyMap; }
    }

    public string KeyMapName
    {
      get { return _keyMapName; }
      set
      {
        if (value.Equals(KeyMaps[0]))
        {
          _keyMap = SerialInterface.keyMap_RCA;
        }
        else if (value.Equals(KeyMaps[1]))
        {
          _keyMap = SerialInterface.keyMap_D10100_D10200;
        }
        else
        {
          throw new ArgumentException("Invalid default KeyMap name specified", value);
        }
        _keyMapName = value;
      }
    }

    public bool UseOldCommandSet
    {
      get { return _useOldCommandSet; }
      set
      {
        if (value)
        {
          _commandSet = SerialInterface.oldCommandSet;
        }
        else
        {
          _commandSet = SerialInterface.newCommandSet;
        }
        _useOldCommandSet = value;
      }
    }

    public CommandSet CommandSet
    {
      get { return _commandSet; }
    }

    public string SerialPortName
    {
      get { return _serialPortName; }
      set { _serialPortName = value; }
    }

    public int BaudRate
    {
      get { return _baudRate; }
      set { _baudRate = value; }
    }

    public int ReadTimeout
    {
      get { return _readTimeout; }
      set { _readTimeout = value; }
    }

    public bool PowerOn
    {
      get { return _poweron; }
      set { _poweron = value; }
    }

    public bool UseSetChannelForTune
    {
      get { return _useSetChannelForTune; }
      set { _useSetChannelForTune = value; }
    }

    public bool Debug
    {
      get { return _debug; }
      set { _debug = value; }
    }

    public bool Advanced
    {
      get {return _advanced;}
      set {_advanced = value;}
    }

    public bool TwoWayDisable
    {
      get { return _twowaydisable; }
      set { _twowaydisable = value; }
    }

    public bool HideOSD
    {
      get { return _hideOSD; }
      set { _hideOSD = value; }
    }

    public bool AllowDigitalSubchannels
    {
      get { return _allowDigitalSubchannels; }
      set { _allowDigitalSubchannels = value; }
    }
    #endregion

  }
}

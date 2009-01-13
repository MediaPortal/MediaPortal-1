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
using System.IO.Ports;

namespace DirecTV
{
  public class SerialInterface
  {
    #region structs

    public struct Response
    {
      public const byte VALID_COMMAND = 0xF0;
      public const byte INVALID_COMMAND = 0xF1;
      public const byte COMMAND_IS_BEING_PROCESSED = 0xF2;
      public const byte TIMEOUT_WAITING_FOR_COMMAND = 0xF3;
      public const byte COMMAND_COMPLETED_SUCCESSFULLY = 0xF4;
      public const byte COMMAND_FAILED = 0xF5;
      public const byte ILLEGAL_CHARACTER_RECEIVED = 0xFB;
      public const byte BUFFER_UNDERFLOW = 0xFD;
      public const byte BUFFER_OVERFLOW = 0xFF;
      public const byte NULL_BYTE = 0x00;
    }

    #endregion

    #region Command Sets

    public static readonly CommandSet oldCommandSet = new CommandSet(
      "oldCommandSet", // Command Set name
      new Command(0x01), // Power Off
      new Command(0x02), // Power On
      new Command(0x05), // Show Text
      new Command(0x06), // Hide Text
      new Command(0x07, 0, 2), // Get Channel Number
      new Command(0x0A, 0, 8), // Cold Boot
      new Command(0x0B, 0, 8), // Warm Boot
      new Command(0x10, 0, 1), // Get Signal Strength
      new Command(0x11, 0, 7), // Get Date, Time, Day of week
      new Command(0x13), // Enable Ir Remote
      new Command(0x14), // Disable Ir Remote
      new Command(0x45, 3), // Remote Control Key
      new Command(0x46, 2), // Set Channel Number
      new Command(0x4A, 2) // Display Text
      );

    public static readonly CommandSet newCommandSet = new CommandSet(
      "newCommandSet", // Command Set name
      new Command(0x81), // Power Off
      new Command(0x82), // Power On
      new Command(0x00), // Show Text (not available)
      new Command(0x86), // Hide Text
      new Command(0x87, 0, 4), // Get Channel Number
      new Command(0x00), // Cold Boot (not available)
      new Command(0x00), // Warm Boot (not available)
      new Command(0x90, 0, 1), // Get Signal Strength
      new Command(0x91, 0, 7), // Get Date, Time, Day of week
      new Command(0x93), // Enable Ir Remote
      new Command(0x94), // Disable Ir Remote
      new Command(0xA5, 3), // Remote Control Key
      new Command(0xA6, 4), // Set Channel Number
      new Command(0xAA, 2) // Display Text
      );

    #endregion

    #region KeyMaps

    public static readonly KeyMap keyMap_RCA = new KeyMap(
      "RCA",
      0xA8, 0xA9, 0xA6, 0xA7, 0x9E, 0xC3, 0xC3, 0xC5, 0xC6, 0xC7, 0xC8, 0xC9, 0xCA, 0xCB, 0xCC,
      0xCD, 0xCE, 0xCF, 0x00, 0xD2, 0xD3, 0xD5, 0xD8, 0xE5, 0xF7, 0x00, 0x00, 0x00, 0x00
      );

    public static readonly KeyMap keyMap_D10100_D10200 = new KeyMap(
      "D10-100/200",
      0x9A, 0x9B, 0x9C, 0x9D, 0x00, 0xC3, 0xA0, 0xD4, 0xE9, 0xE8, 0xE7, 0xE6, 0xE5, 0xE4, 0xE3,
      0xE2, 0xE1, 0xE0, 0xA5, 0xD1, 0xD2, 0xD5, 0xD6, 0xD3, 0xF7, 0xA1, 0xA2, 0xA3, 0xA4
      );

    #endregion

    #region enums

    public enum BoxType
    {
      RCA_Old,
      RCA_New,
      D10_100,
      D10_200
    }

    #endregion

    #region Debug message handling

    public delegate void DebugMessageHandler(string format, params object[] args);

    public event DebugMessageHandler OnDebugMessage;

    #endregion

    #region vars

    // Which box are we using?
    private BoxType _box;
    // Which command set should we use for this box?
    private CommandSet _commandSet;
    // Which remote keymapping should we use for this box?
    private KeyMap _keyMap;
    // Serial Port Name to which the DirecTV box is connected
    private string _portName;
    // Baudrate to use while talking to the DirecTV box
    private int _baudRate;
    // Serial port read timeout
    private int _readTimeout;
    // Should we use set the set channel command for tuning channels? (not recommended for any box but RCA)
    private bool _useSetChannelForTune;
    // Power on before tuning?
    private bool _powerOnBeforeTuning = false;
    // Disable two way communication
    private bool _twowaydisable = false;
    //Hide the OSD on D10-1/200 boxes
    private bool _hideOSD = false;
    // Allow tuning digital subchannels on HTL-HD boxes
    private bool _allowSubchannels = false;

    #region defaults

    // default box type
    private const BoxType _defaultBoxType = BoxType.D10_200;
    // default COM port
    private const string _defaultPortName = "COM1";
    // default baud rate
    private const int _defaultBaudRate = 9600;
    // default serial read timeout
    private const int _defaultReadTimeout = 1000;
    // serial line protocol command prefix
    private const byte COMMAND_PREFIX = 0xFA;
    // data to append to commands on D10-200 boxes
    private const byte D10200_COMMAND_SUFFIX = 0x0D;

    #endregion

    private SerialPort _serialPort;

    #endregion

    #region C#tor

    public SerialInterface()
      : this(_defaultBoxType)
    {
    }

    public SerialInterface(BoxType box)
      : this(box, _defaultPortName)
    {
    }

    public SerialInterface(BoxType box, string portName)
      : this(box, portName, _defaultBaudRate)
    {
    }

    public SerialInterface(BoxType box, string portName, int baudRate)
      : this(box, portName, baudRate, _defaultReadTimeout)
    {
    }

    public SerialInterface(BoxType box, CommandSet set, KeyMap map)
      : this(box, set, map, _defaultPortName)
    {
    }

    public SerialInterface(BoxType box, CommandSet set, KeyMap map, string portName)
      : this(box, set, map, portName, _defaultBaudRate)
    {
    }

    public SerialInterface(BoxType box, CommandSet set, KeyMap map, string portName, int baudRate)
      : this(box, set, map, portName, _defaultBaudRate, _defaultReadTimeout)
    {
    }

    public SerialInterface(BoxType box, string portName, int baudRate, int readTimeout)
    {
      _box = box;
      switch (box)
      {
        case BoxType.RCA_Old:
          _commandSet = oldCommandSet;
          _keyMap = keyMap_RCA;
          UseSetChannelForTune = true;
          break;
        case BoxType.RCA_New:
          _commandSet = newCommandSet;
          _keyMap = keyMap_RCA;
          UseSetChannelForTune = true;
          break;
        case BoxType.D10_100:
        case BoxType.D10_200:
          _commandSet = newCommandSet;
          _keyMap = keyMap_D10100_D10200;
          UseSetChannelForTune = false;
          break;
      }
      PortName = portName;
      BaudRate = baudRate;
      _readTimeout = readTimeout;
    }

    public SerialInterface(BoxType box, CommandSet set, KeyMap map, string portName, int baudRate, int readTimeout)
    {
      _box = box;
      _commandSet = set;
      _keyMap = map;
      PortName = portName;
      BaudRate = baudRate;
      _readTimeout = readTimeout;
      if (box == BoxType.RCA_Old || box == BoxType.RCA_New)
      {
        UseSetChannelForTune = true;
      }
    }

    #endregion

    #region Properties

    public BoxType DirecTVBoxType
    {
      get { return _box; }
      set { _box = value; }
    }

    public CommandSet DirecTVCommandSet
    {
      get { return _commandSet; }
      set { _commandSet = value; }
    }

    public KeyMap DirecTVKeyMap
    {
      get { return _keyMap; }
      set { _keyMap = value; }
    }

    public KeyMap KeyMapRCA
    {
      get { return keyMap_RCA; }
    }

    public KeyMap KeyMapD10100_D10200
    {
      get { return KeyMapD10100_D10200; }
    }

    public string PortName
    {
      get { return _portName; }
      set
      {
        bool found = false;
        foreach (string portName in SerialPort.GetPortNames())
        {
          if (portName.ToLower().Equals(value.ToLower()))
          {
            _portName = value;
            found = true;
          }
        }
        if (!found)
        {
          WriteDebug("DirecTV.SerialInterface: invalid COM port: {0}", value);
          throw new ArgumentOutOfRangeException("PortName", value, "Not a valid COM port");
        }
      }
    }

    public int BaudRate
    {
      get { return _baudRate; }
      set
      {
        switch (value)
        {
          case 110:
          case 300:
          case 600:
          case 1200:
          case 4800:
          case 9600:
          case 19200:
          case 38400:
          case 57600:
          case 76800:
          case 115200:
          case 153600:
          case 230400:
          case 307200:
          case 460800:
            _baudRate = value;
            break;
          default:
            WriteDebug("DirecTV.SerialInterface: invalid baud rate: {0}", value);
            throw new ArgumentOutOfRangeException("BaudRate", value, "Not a valid baudrate");
        }
      }
    }

    public int ReadTimeout
    {
      get { return _readTimeout; }
      set { _readTimeout = value; }
    }

    public bool UseSetChannelForTune
    {
      get { return _useSetChannelForTune; }
      set { _useSetChannelForTune = value; }
    }

    public bool PowerOnBeforeTuning
    {
      get { return _powerOnBeforeTuning; }
      set { _powerOnBeforeTuning = value; }
    }

    public bool AllowDigitalSubchannels
    {
      get { return _allowSubchannels; }
      set { _allowSubchannels = value; }
    }

    #endregion

    #region Serial Interface Control

    public void OpenPort()
    {
      if (_serialPort != null && _serialPort.IsOpen)
      {
        return;
      }
      _serialPort = new SerialPort(PortName, BaudRate);
      _serialPort.ReadTimeout = ReadTimeout;
      _serialPort.Open();
      if (_serialPort.IsOpen)
      {
        WriteDebug("DirecTV.SerialInterface: Serial port {0} opened (baudrate:{1})", PortName, BaudRate);
      }
      else
      {
        WriteDebug("DirecTV.SerialInterface: unable to open port {0} ! (baudrate:{1})", PortName, BaudRate);
      }
    }

    public void ClosePort()
    {
      if (_serialPort.IsOpen)
      {
        _serialPort.Close();
        WriteDebug("DirecTV.SerialInterface: Serial port {0} closed", PortName);
      }
      else
      {
        WriteDebug("DirecTV.SerialInterface: Serial port {1} already closed!", PortName);
      }
    }

    #endregion

    #region SendCommand helpers

    private byte[] GetCommandData(byte cmd)
    {
      byte[] cmdData;
      if (_box == BoxType.D10_200)
      {
        cmdData = new byte[3];
        cmdData[2] = D10200_COMMAND_SUFFIX;
      }
      else
      {
        cmdData = new byte[2];
      }
      cmdData[0] = COMMAND_PREFIX;
      cmdData[1] = cmd;
      return cmdData;
    }

    private bool ReadResponse(byte verify, out byte expect, out byte response)
    {
      expect = verify;
      response = 0x00;

      try
      {
        response = (byte) _serialPort.ReadByte();
        WriteDebug("DirecTV.SerialInterface.ReadResponse(): received: {0:x}", response);
        return (response == expect);
      }
      catch (TimeoutException)
      {
        WriteDebug("DirecTV.SerialInterface.ReadResponse(): SerialPortTimedOut");
      }
      catch (Exception ex)
      {
        WriteDebug("DirecTV.SerialInterface.ReadResponse(): Exception: " + ex.ToString());
      }

      return false;
    }

    private void ReadPossibleTrailingData()
    {
      try
      {
        string dummy = _serialPort.ReadExisting();
        WriteDebug("DirecTV.SerialInterface.ReadPossibleTrailingData(): ignored trailing garbage: {0}", dummy);
        dummy = null;
      }
      catch (InvalidOperationException)
      {
      }
    }

    private string ToString(byte data)
    {
      return String.Format("{0.x}", data);
    }

    private string ToString(byte[] data)
    {
      if (data == null)
      {
        return string.Empty;
      }
      string tmp = string.Empty;
      foreach (byte b in data)
      {
        tmp += String.Format("{0:x}", b) + " ";
      }
      return tmp;
    }

    #endregion

    #region DirecTV Serial Protocol handling

    public void SendCommand(Command cmd, out byte[] receivedData)
    {
      byte reponse;
      byte expect;

      // Start with sending the command to the box
      byte[] cmdData = GetCommandData(cmd.command);

      WriteDebug("DirecTV.SerialInterface.SendCommand(): send command: {0}, size {1}",
                 ToString(cmdData), cmdData.Length);

      _serialPort.Write(cmdData, 0, cmdData.Length);

      // Check if command was recognised by the box
      if (TwoWayDisable == false)
      {
        if (ReadResponse(Response.VALID_COMMAND, out expect, out reponse))
        {
          // it was, now check if we need to send some additional data
          WriteDebug("DirecTV.SerialInterface.SendCommand(): command accepted by box");
          if (cmd.bytesToSend > 0 && cmd.dataToSend.Length > 0)
          {
            // we need to send additional data, send it
            WriteDebug("DirecTV.SerialInterface.SendCommand(): send data: {0}", ToString(cmd.dataToSend));
            _serialPort.Write(cmd.dataToSend, 0, cmd.dataToSend.Length);
            // check if the data was correctly received by the box
            if (ReadResponse(Response.COMMAND_IS_BEING_PROCESSED, out expect, out reponse))
            {
              // it was, so now check if the command succeeded
              WriteDebug("DirecTV.SerialInterface.SendCommand(): data is being processed by box");

              // The HTL-HD box sends a null byte back before the Successful so check for it
              bool okContinue = false;
              ReadResponse(Response.NULL_BYTE, out expect, out reponse);
              if (reponse == (byte) Response.NULL_BYTE)
              {
                WriteDebug(
                  "DirecTV.SerialInterface.SendCommand(): Null Byte Recieved Will Now Check for Successful Completion");
                if (ReadResponse(Response.COMMAND_COMPLETED_SUCCESSFULLY, out expect, out reponse))
                {
                  okContinue = true;
                }
              }
              else if (reponse == (byte) Response.COMMAND_COMPLETED_SUCCESSFULLY)
              {
                okContinue = true;
              }

              if (okContinue)
              {
                // command succeeded, check if we expect additional data
                if (cmd.bytesToReceive > 0)
                {
                  // we do, so let's read that data and return that
                  byte[] buf = new byte[cmd.bytesToReceive];
                  _serialPort.Read(buf, 0, cmd.bytesToReceive);
                  receivedData = buf;
                  WriteDebug("DirecTV.SerialInterface.SendCommand(): receive data: {0}", ToString(receivedData));
                }
                else
                {
                  // no data expected, so return a null-byte
                  receivedData = new byte[1] {0x00};
                }
                // make sure all received data is read
                ReadPossibleTrailingData();
                WriteDebug(
                  "DirecTV.SerialInterface.SendCommand({0}) succeeded: expected {1:x} and received {2:x} data: {3}",
                  ToString(cmdData), expect, reponse, ToString(receivedData));
                return;
              }
            }
            else
            {
              WriteDebug("DirecTV.SerialInterface.SendCommand(): data NOT accepted by box!");
            }
          }
          else
          {
            // no extra data needs to be send and command was valid
            WriteDebug("DirecTV.SerialInterface.SendCommand({0}) succeeded: expected {1:x} and received {2:x}",
                       ToString(cmdData), expect, reponse);
            // no data expected, so return a null-byte
            receivedData = new byte[1] {0x00};
            return;
          }
        }

        WriteDebug("DirecTV.SerialInterface.SendCommand(): command NOT accepted by box!");

        // If we get here, the command must somehow have failed
        // make sure all received data is read
        ReadPossibleTrailingData();
        WriteDebug("DirecTV.SerialInterface.SendCommand({0}) failed: expected {1:x} but received {2:x}",
                   ToString(cmdData), expect, reponse);
        throw new InvalidOperationException("Unable to send command to DirecTV box");
      }
      else
      {
        WriteDebug("DirecTV.SerialInterface.SendCommand(): send data: {0}", ToString(cmd.dataToSend));
        _serialPort.Write(cmd.dataToSend, 0, cmd.dataToSend.Length);
        byte[] temp = new byte[1];
        temp[0] = 0x00;
        _serialPort.Write(temp, 0, 1);
        receivedData = temp;
      }
    }

    public void SendCommand(Command cmd)
    {
      byte[] receivedData;
      SendCommand(cmd, out receivedData);
      receivedData = null;
    }

    #endregion

    #region TuneToChannel / Remote key helpers

    public byte[] GetKeyMapPadBytes(BoxType box)
    {
      switch (box)
      {
        case BoxType.RCA_Old:
        case BoxType.RCA_New:
          return new byte[2] {0x00, 0x00};
        default:
          return new byte[2] {0x00, 0x01};
      }
    }

    private byte GetRemoteKeyCode(char num)
    {
      switch (num)
      {
        case '0':
          return _keyMap.KEY_0;
        case '1':
          return _keyMap.KEY_1;
        case '2':
          return _keyMap.KEY_2;
        case '3':
          return _keyMap.KEY_3;
        case '4':
          return _keyMap.KEY_4;
        case '5':
          return _keyMap.KEY_5;
        case '6':
          return _keyMap.KEY_6;
        case '7':
          return _keyMap.KEY_7;
        case '8':
          return _keyMap.KEY_8;
        case '9':
          return _keyMap.KEY_9;
        case '-':
          return _keyMap.DASH;
        default:
          return 0x00;
      }
    }

    private void ParseMajorMinorChannel(string channel, out int channelMajor, out int channelMinor)
    {
      channelMajor = -1;
      channelMinor = -1;

      // If the channel string contains a dash "-" 
      // or has a length of 6 assume Major-Minor Channel
      if (channel.Contains("-"))
      {
        try
        {
          channelMajor = Int32.Parse(channel.Substring(0, channel.IndexOf("-")));

          if (channel.Length > (channel.IndexOf("-") + 1))
          {
            channelMinor = Int32.Parse(channel.Substring(channel.IndexOf("-") + 1, 1));
          }
        }
        catch
        {
        }
      }
      else if (channel.Length == 6)
      {
        try
        {
          // Assume first four Major and Last two minor
          channelMajor = Int32.Parse(channel.Substring(0, 4));
          channelMinor = Int32.Parse(channel.Substring(4));
        }
        catch
        {
        }
      }
      else
      {
        try
        {
          channelMajor = Int32.Parse(channel);
          channelMinor = 0;
        }
        catch
        {
        }
      }
    }

    private void TuneWithSetChannel(int channel)
    {
      Command cmd = _commandSet.SET_CHANNEL_NUMBER.Clone();
      if (channel < 256)
      {
        cmd.dataToSend[0] = 0x00;
        cmd.dataToSend[1] = (byte) channel;
      }
      else if (channel < 65536)
      {
        byte[] chdata = BitConverter.GetBytes(channel);
        cmd.dataToSend[0] = chdata[1];
        cmd.dataToSend[1] = chdata[0];
      }
      else
      {
        throw new ArgumentOutOfRangeException("channel", channel, "not a valid channel number");
      }
      if (cmd.bytesToSend > 2)
      {
        for (int i = 2; i < cmd.bytesToSend; i++)
        {
          cmd.dataToSend[i] = 0xFF;
        }
      }
      SendCommand(cmd);
    }

    private void TuneWithSetChannel(string channel)
    {
      WriteDebug("DirecTV.SerialInterface.TuneWithSetChannel: Attempting to tune STRING Channel: {0}", channel);
      Command cmd = _commandSet.SET_CHANNEL_NUMBER.Clone();

      int chMajor = 0;
      int chMinor = 0;

      ParseMajorMinorChannel(channel, out chMajor, out chMinor);

      if (chMajor < 0)
      {
        WriteDebug("DirecTV.SerialInterface.TuneWithSetChannel: Invalid Channel Received: {0} - Channel String: {1}",
                   chMajor.ToString(), channel);
        return;
      }

      if (chMajor < 256)
      {
        cmd.dataToSend[0] = 0x00;
        cmd.dataToSend[1] = (byte) chMajor;
      }
      else if (chMajor < 65536)
      {
        byte[] chdata = BitConverter.GetBytes(chMajor);
        cmd.dataToSend[0] = chdata[1];
        cmd.dataToSend[1] = chdata[0];
      }
      else
      {
        WriteDebug("DirecTV.SerialInterface.TuneWithSetChannel: Channel Value Too Large: {0} - Channel String: {1}",
                   chMajor.ToString(), channel);
        return;
      }

      if (chMinor > 0 && cmd.bytesToSend > 2)
      {
        //byte[] chdata = BitConverter.GetBytes(chMinor);
        cmd.dataToSend[2] = 0x00; // chdata[1];
        cmd.dataToSend[3] = (byte) chMinor; // chdata[0];
      }
      else if (cmd.bytesToSend > 2)
      {
        for (int i = 2; i < cmd.bytesToSend; i++)
        {
          cmd.dataToSend[i] = 0xFF;
        }
      }

      SendCommand(cmd);
    }

    private void TuneWithRemoteKeys(int channel)
    {
      WriteDebug("DirecTV: In tuning");
      if (channel > 65535)
      {
        throw new ArgumentOutOfRangeException("channel", channel, "not a valid channel number");
      }
      string strChannel = channel.ToString();
      WriteDebug("DirecTV: Channel " + strChannel);
      Command cmd = _commandSet.REMOTE_CONTROL_KEY.Clone();
      GetKeyMapPadBytes(_box).CopyTo(cmd.dataToSend, 0);
      for (int i = 0; i < strChannel.Length; i++)
      {
        cmd.dataToSend[2] = GetRemoteKeyCode(strChannel[i]);
        WriteDebug("Directv: Sending command - {0}", cmd.bytesToSend);
        SendCommand(cmd);
      }
      if (TwoWayDisable == false)
      {
        cmd.dataToSend[2] = _keyMap.EXIT;
        WriteDebug("DirecTV " + cmd.bytesToSend.ToString());
        SendCommand(cmd);
      }
      if (HideOSD == true)
      {
        cmd.dataToSend[2] = 0x86;
        SendCommand(cmd);
      }
    }

    private void TuneWithRemoteKeys(string channel)
    {
      WriteDebug("DirecTV.SerialInterface.TuneWithRemoteKeys: Attempting to tune STRING Channel: {0}", channel);
      for (int i = 0; i < channel.Length; i++)
      {
        if (GetRemoteKeyCode(channel[i]) == 0x00)
        {
          WriteDebug("DirecTV.SerialInterface.TuneWithRemoteKeys: Invalid Character Found in Channel: {0}", channel);
          return;
        }
      }

      int chMajor = 0;
      int chMinor = 0;

      ParseMajorMinorChannel(channel, out chMajor, out chMinor);

      if (chMajor > 65535 || chMajor < 0)
      {
        WriteDebug("DirecTV.SerialInterface.TuneWithRemoteKeys: Channel Value Invalid: {0} - Channel String: {1}",
                   chMajor.ToString(), channel);
        return;
      }

      Command cmd = _commandSet.REMOTE_CONTROL_KEY.Clone();

      GetKeyMapPadBytes(_box).CopyTo(cmd.dataToSend, 0);

      for (int i = 0; i < channel.Length; i++)
      {
        cmd.dataToSend[2] = GetRemoteKeyCode(channel[i]);
        SendCommand(cmd);
      }
      cmd.dataToSend[2] = _keyMap.EXIT;
      SendCommand(cmd);
    }

    #endregion

    #region Generic channel tuning interface

    public void TuneToChannel(int channel)
    {
      if (_serialPort == null || !_serialPort.IsOpen)
      {
        OpenPort();
      }
      if (PowerOnBeforeTuning)
      {
        WriteDebug("DirecTV.SerialInterface.TuneToChannel(): send power on command");
        PowerOn();
      }
      if (UseSetChannelForTune)
      {
        WriteDebug("DirecTV.SerialInterface.TuneToChannel(): tuning to channel {0} with set channel command", channel);
        TuneWithSetChannel(channel);
      }
      else
      {
        WriteDebug("DirecTV.SerialInterface.TuneToChannel(): tuning to channel {0} with remote keypresses", channel);
        TuneWithRemoteKeys(channel);
      }
    }

    public void TuneToChannel(string channel)
    {
      if (_serialPort == null || !_serialPort.IsOpen)
      {
        OpenPort();
      }
      if (PowerOnBeforeTuning)
      {
        WriteDebug("DirecTV.SerialInterface.TuneToChannel(): send power on command");
        PowerOn();
      }
      if (UseSetChannelForTune)
      {
        WriteDebug("DirecTV.SerialInterface.TuneToChannel(): tuning to channel {0} with set channel command", channel);
        TuneWithSetChannel(channel);
      }
      else
      {
        WriteDebug("DirecTV.SerialInterface.TuneToChannel(): tuning to channel {0} with remote keypresses", channel);
        TuneWithRemoteKeys(channel);
      }
    }

    #endregion

    #region power handling

    public void PowerOn()
    {
      Command cmd = _commandSet.POWER_ON.Clone();
      SendCommand(cmd);
    }

    public void PowerOff()
    {
      Command cmd = _commandSet.POWER_OFF.Clone();
      SendCommand(cmd);
    }

    #endregion

    #region Debug interface

    private void WriteDebug(string format, params object[] args)
    {
      if (OnDebugMessage != null)
      {
        OnDebugMessage(format, args);
      }
    }

    public void DumpConfig()
    {
      WriteDebug(
        "DirecTV.SerialInterface() created; boxtype: {0}, commandset: {1}, keymap: {2}, port: {3}, baudrate: {4}, timeout: {5}, poweron: {6}",
        _box, _commandSet.Name, _keyMap.KeyMapName, PortName, BaudRate, ReadTimeout, PowerOnBeforeTuning
        );
    }

    #endregion

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
  }
}
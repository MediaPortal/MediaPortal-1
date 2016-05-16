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
using System.Text;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.HauppaugeBlaster
{
  internal class Blaster
  {
    #region enums

    private enum HcwResult : int
    {
      Success = 0,
      Fail,
      DeviceNotInDatabase,
      CodeNotInDatabase,
      KeyNotInDatabase,
      NotSupported,
      CorruptDataReceived,
      TimeOut,
      ChecksumFailed,
      FirmwareIncompatible,
      InvalidParameter,
      NotInitialised
    }

    #endregion

    #region DLL imports

    /// <summary>
    /// Open the blaster interface.
    /// </summary>
    /// <remarks>
    /// Defaults to port 1. Call UIR_SetPort() afterwards to change this.
    /// </remarks>
    /// <param name="showErrorMessageBox"><c>True</c> to show message boxes describing errors if an error occurs.</param>
    /// <param name="portType">0 (= auto), 226, 250 (= CIR???)</param>
    /// <returns>the type of the IR port opened if successful, otherwise <c>0</c></returns>
    [DllImport("hcwIRblast.dll")]
    private static extern short UIR_Open([MarshalAs(UnmanagedType.Bool)] bool showErrorMessageBox, short portType);

    /// <summary>
    /// Close the blaster interface.
    /// </summary>
    /// <returns><c>true</c> if successful, otherwise <c>false</c></returns>
    [DllImport("hcwIRblast.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UIR_Close();

    /// <summary>
    /// Get the Hauppauge BlastCfg.exe configuration.
    /// </summary>
    /// <remarks>
    /// There is separate configuration for each port + device + code set
    /// combination. To retrieve a specific combination call UIR_SetPort() to
    /// set the active port and pass the desired device and code set values as
    /// parameters.
    /// 
    /// To retrieve the current/default configuration for the active port,
    /// pass device and code set as -1.
    /// </remarks>
    [DllImport("hcwIRblast.dll")]
    private static extern HcwResult UIR_GetConfig(int device, int codeSet, ref Configuration config);

    /// <summary>
    /// Set the Hauppauge BlastCfg.exe configuration.
    /// </summary>
    /// <remarks>
    /// The configuration is saved for the active port with the device and code
    /// set specified by the config parameter.
    /// </remarks>
    [DllImport("hcwIRblast.dll")]
    private static extern HcwResult UIR_SetConfig(ref Configuration config);

    [DllImport("hcwIRblast.dll")]
    private static extern HcwResult UIR_GetKeyCodeRaw(int unknown1, out int unknown2);

    [DllImport("hcwIRblast.dll")]
    private static extern HcwResult UIR_SendKeyCode(int device, int codeSet, int keyCode);

    [DllImport("hcwIRblast.dll")]
    private static extern HcwResult UIR_LearnKeyCode(int device, int codeSet, int keyCode, int unknown);

    [DllImport("hcwIRblast.dll")]
    private static extern HcwResult UIR_GetLearnedCode(int device, int codeSet, int keyCode, IntPtr buffer, int bufferSize, out int usedBufferSize);

    [DllImport("hcwIRblast.dll")]
    private static extern HcwResult UIR_is_device_available(int device);

    [DllImport("hcwIRblast.dll")]
    private static extern HcwResult UIR_is_code_available(int device, int codeSet);

    [DllImport("hcwIRblast.dll")]
    private static extern HcwResult UIR_is_key_available(int device, int codeSet, int keyCode);

    /// <remarks>
    /// Blast plain channel numbers (integers) up to 4 digits in length.
    /// Two part channel numbers not supported.
    /// </remarks>
    [DllImport("hcwIRblast.dll")]
    private static extern HcwResult UIR_GotoChannel(int device, int codeSet, int channelNumber);

    /// <summary>
    /// Get version information for hcwIRblast.dll.
    /// </summary>
    /// <param name="versionString">A buffer to hold the version information.</param>
    /// <param name="bufferSize">As an input, the size of the buffer; as an output, the number of bytes used in the buffer.</param>
    [DllImport("hcwIRblast.dll")]
    private static extern HcwResult UIR_GetVersionStr([MarshalAs(UnmanagedType.LPStr)] StringBuilder versionString, ref int bufferSize);

    /// <summary>
    /// Get the number of blaster ports available which are of the type of the currently opened port.
    /// </summary>
    /// <returns></returns>
    [DllImport("hcwIRblast.dll")]
    private static extern int UIR_GetPortCount();

    /// <summary>
    /// Set the blaster port number to use.
    /// </summary>
    /// <param name="portNumber">The port number. Should be between 1 and the result of UIR_GetPortCount(), inclusive.</param>
    [DllImport("hcwIRblast.dll")]
    private static extern HcwResult UIR_SetPort(int portNumber);

    /// <remarks>
    /// unknown1 should be between 1 and 5, inclusive.
    ///   3 = get emitter count
    /// </remarks>
    [DllImport("hcwIRblast.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IR_ProdTestCIR(short unknown1, int unknown2);

    /// <remarks>
    /// If you pass a numeric string and UseMajorMinorFormat is FALSE, behaviour should be identical to UIR_GotoChannel().
    /// If you pass a numeric string and UseMajorMinorFormat is TRUE:
    /// - value should be less than or equal 10000
    /// - digit 1 and 2 are the major channel number; digits 3, 4 and 5 are the minor channel number
    /// 
    /// If you pass a NON-numeric string it is expected to be in one of two formats.
    /// Format 1:
    /// - [major channel number] + '.' + [minor channel number]
    /// - major channel number may have up to 3 digits
    /// - minor channel number may have up to 2 digits
    /// 
    /// Format 2:
    /// - '{' + ['BS' OR 'CS' OR 'IT' OR 'NO'] + [number, value 1 to 12 inclusive]
    /// - the number is a key code (1, 2.. 9, 0, 84 [unknown], 85 [unknown])
    /// </remarks>
    [DllImport("hcwIRblast.dll")]
    private static extern HcwResult UIR_GotoChannelEx(int device, int codeSet, [MarshalAs(UnmanagedType.LPStr)] string channelNumber);

    #endregion

    #region structs

    /// <summary>
    /// Hauppauge BlastCfg.exe configuration. BlasterCfg.exe is installed
    /// with the Hauppauge IRBlast package.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct Configuration
    {
      public int StructSize;
      public int Version;
      public int Region;
      public int Device;
      public int Vendor;
      public int CodeSet;
      [MarshalAs(UnmanagedType.Bool)]
      public bool SendPowerOn;          // send a power command before the channel number
      public int DelayPowerOn;          // delay after sending power command; unit = ms
      public int MinimumDigits;         // enables zero padding the channel number
      public int DelayInterDigit;       // delay between sending each digit in the channel number; unit = ms
      [MarshalAs(UnmanagedType.Bool)]
      public bool SendEnter;            // send an enter/select command after multi-digit channel numbers
      public int DelayEnter;            // delay after sending enter/select command; unit = ms
      public int DelayTune;             // delay after sending channel number *without enter/select command; unit = ms
      public int DelaySingleDigit;      // delay after sending a single digit channel number; unit = ms
      [MarshalAs(UnmanagedType.Bool)]
      public bool UseMajorMinorFormat;  // enables support for ATSC two part channel numbers with UIR_GotoChannelEx()
    }

    #endregion

    #region variables

    private object _accessLock = new object();
    private bool _isInterfaceOpen = false;
    private Configuration _config;
    private int _port = 0;
    private int _portCount = 0;
    private string _version = string.Empty;

    #endregion

    public bool OpenInterface()
    {
      lock (_accessLock)
      {
        if (_isInterfaceOpen)
        {
          return true;
        }
        try
        {
          this.LogDebug("Hauppauge blaster: open interface");
          int blasterType = UIR_Open(false, 0);
          if (blasterType == 0)
          {
            this.LogError("Hauppauge blaster: failed to open interface");
            return _isInterfaceOpen;
          }
          this.LogDebug("  blaster type  = {0}", blasterType);

          _port = 1;
          _portCount = UIR_GetPortCount();
          this.LogDebug("  port count    = {0}", _portCount);

          int bufferSize = 256;
          StringBuilder version = new StringBuilder(bufferSize);
          HcwResult result = UIR_GetVersionStr(version, ref bufferSize);
          if (result != HcwResult.Success)
          {
            this.LogError("Hauppauge blaster: failed to read version, result = {0}, buffer size = {1}", result, bufferSize);
            return _isInterfaceOpen;
          }
          _version = version.ToString();
          this.LogDebug("  UIR version   = {0}", _version);

          _config = new Configuration();
          _config.StructSize = Marshal.SizeOf(typeof(Configuration));  // 60
          result = UIR_GetConfig(-1, -1, ref _config);
          if (result != HcwResult.Success)
          {
            this.LogError("Hauppauge blaster: failed to read configuration, result = {0}", result);
            return _isInterfaceOpen;
          }

          _isInterfaceOpen = true;

          this.LogDebug("  version       = {0}", _config.Version);
          this.LogDebug("  region        = {0}", _config.Region);
          this.LogDebug("  device        = {0}", _config.Device);
          this.LogDebug("  vendor        = {0}", _config.Vendor);
          this.LogDebug("  code set      = {0}", _config.CodeSet);
          this.LogDebug("  send power?   = {0}", _config.SendPowerOn);
          this.LogDebug("  power delay   = {0} ms", _config.DelayPowerOn);
          this.LogDebug("  min. digits   = {0}", _config.MinimumDigits);
          this.LogDebug("  digit delay   = {0} ms", _config.DelayInterDigit);
          this.LogDebug("  send enter?   = {0}", _config.SendEnter);
          this.LogDebug("  enter delay   = {0} ms", _config.DelayEnter);
          this.LogDebug("  tune delay MD = {0} ms", _config.DelayTune);
          this.LogDebug("  tune delay SD = {0} ms", _config.DelaySingleDigit);
          this.LogDebug("  maj./min.?    = {0}", _config.UseMajorMinorFormat);
        }
        catch (Exception ex)
        {
          this.LogError(ex, "Hauppauge blaster: failed to open interface");
        }
        return _isInterfaceOpen;
      }
    }

    public void CloseInterface()
    {
      lock (_accessLock)
      {
        try
        {
          if (_isInterfaceOpen)
          {
            this.LogDebug("Hauppauge blaster: close interface");
            UIR_Close();
            _isInterfaceOpen = false;
            _port = 0;
            _portCount = 0;
            _version = string.Empty;
          }
        }
        catch (Exception ex)
        {
          this.LogError(ex, "Hauppauge blaster: failed to close interface");
        }
      }
    }

    public bool IsInterfaceOpen
    {
      get
      {
        return _isInterfaceOpen;
      }
    }

    public int PortCount
    {
      get
      {
        return _portCount;
      }
    }

    public string Version
    {
      get
      {
        return _version;
      }
    }

    public bool BlastChannelNumber(string channelNumber, int port)
    {
      this.LogDebug("Hauppauge blaster: blast channel number, number = {0}, port = {1}", channelNumber, port);

      lock (_accessLock)
      {
        if (!_isInterfaceOpen && !OpenInterface())
        {
          return false;
        }

        int channelNumberAsInt;
        bool isIntChannelNumber = int.TryParse(channelNumber, out channelNumberAsInt);
        if (!_config.UseMajorMinorFormat && !isIntChannelNumber)
        {
          this.LogError("Hauppauge blaster: unsupported channel number format");
          return false;
        }
        if (isIntChannelNumber && (channelNumberAsInt <= 0 || channelNumberAsInt >= 9999))
        {
          this.LogError("Hauppauge blaster: invalid channel number {0}", channelNumber);
          return false;
        }

        if (port < 1 || port > PortCount)
        {
          this.LogError("Hauppauge blaster: invalid port number, port = {0}, port count = {1}", port, _portCount);
          return false;
        }

        HcwResult result;
        if (_port != port)
        {
          this.LogDebug("Hauppauge blaster: set port, current = {0}, count = {1}", _port, _portCount);
          try
          {
            result = UIR_SetPort(port);
            if (result != HcwResult.Success)
            {
              this.LogError("Hauppauge blaster: failed to set port, result = {0}, current port = {1}, new port = {2}, port count = {3}", result, _port, port, _portCount);
              return false;
            }
            _port = port;
          }
          catch (Exception ex)
          {
            this.LogError(ex, "Hauppauge blaster: failed to set port, current port = {0}, new port = {1}, port count = {2}", _port, port, _portCount);
            return false;
          }
        }

        try
        {
          if (isIntChannelNumber)
          {
            result = UIR_GotoChannel(_config.Device, _config.CodeSet, channelNumberAsInt);
          }
          else
          {
            result = UIR_GotoChannelEx(_config.Device, _config.CodeSet, channelNumber);
          }
          if (result == HcwResult.Success)
          {
            return true;
          }
          this.LogError("Hauppauge blaster: failed to blast channel number, result = {0}, number = {1}, port = {2}, device = {3}, code set = {4}", result, channelNumber, _port, _config.Device, _config.CodeSet);
        }
        catch (Exception ex)
        {
          this.LogError(ex, "Hauppauge blaster: failed to blast channel number, number = {0}, port = {1}, device = {2}, code set = {3}", channelNumber, _port, _config.Device, _config.CodeSet);
        }
        return false;
      }
    }
  }
}
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
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.Diseqc;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.Diseqc.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.TVLibrary.Implementations
{
  /// <summary>
  /// A controller class for DiSEqC devices. This controller is able to control
  /// positioners and switches.
  /// </summary>
  internal class DiseqcController : IDiseqcController
  {
    #region constants

    private const double EARTH_FLATTENING_FACTOR = 1.00 / 298.25642;
    private const double EARTH_ECCENTRICITY_FACTOR = EARTH_FLATTENING_FACTOR * (2 - EARTH_FLATTENING_FACTOR);
    private const double RADIUS_SATELLITE_ORBIT = 42164.573;  // distance from the centre of Earth to the geostationary satellite orbit; unit = km
    private const double RADIUS_EARTH_EQUATOR = 6378.1366;    // distance from the centre of Earth to sea level at the equator; unit = km
    private const double REFRACTION_CONST_A0 = 0.58804392;
    private const double REFRACTION_CONST_A1 = -0.17941557;
    private const double REFRACTION_CONST_A2 = 0.29906946e-1;
    private const double REFRACTION_CONST_A3 = -0.25187400e-2;
    private const double REFRACTION_CONST_A4 = 0.82622101e-4;

    private const double LONGITUDE_UNKNOWN = 10000;

    #endregion

    #region variables

    private IDiseqcDevice _device = null;
    private volatile bool _cancelTune = false;

    /// <summary>
    /// Enable or disable always sending DiSEqC commands.
    /// </summary>
    /// <remarks>
    /// DiSEqC commands are usually only sent when changing to a channel on a
    /// different switch port or at a different positioner location. Enabling
    /// this option will cause DiSEqC commands to be sent on each channel
    /// change. My experiments with a wide variety of tuners suggest that only
    /// TeVii tuners require this setting to be enabled.
    /// </remarks>
    private bool _alwaysSendCommands = false;

    private bool _sendCommands = true;  // Ensure that we always send commands on first tune.

    private double _siteLatitude = 0;   // unit = degrees; positive values represent East
    private double _siteLongitude = 0;  // unit = degrees; positive values represent North
    private int _siteAltitude = 0;      // unit = metres [above sea level]

    private double _positionerSpeedSlow = TunerSatellite.DISEQC_MOTOR_DEFAULT_SPEED_SLOW;
    private double _positionerSpeedFast = TunerSatellite.DISEQC_MOTOR_DEFAULT_SPEED_FAST;

    private int _currentPosition = 0;
    private double _currentLongitude = LONGITUDE_UNKNOWN;
    private int _currentStepsAzimuth = 0;
    private int _currentStepsElevation = 0;
    private DiseqcPort _currentPort = DiseqcPort.None;
    private Polarisation _currentPolarisation = Polarisation.Automatic;
    private bool _currentIsHighBand = false;

    #endregion

    /// <summary>
    /// Initialise a new instance of the <see cref="DiseqcController"/> class.
    /// </summary>
    /// <param name="device">A tuner's DiSEqC control interface.</param>
    public DiseqcController(IDiseqcDevice device)
    {
      _device = device;
      if (device == null)
      {
        throw new ArgumentException("DiSEqC device is null.");
      }
    }

    /// <summary>
    /// Reload the controller's configuration.
    /// </summary>
    /// <param name="configuration">The configuration of the associated tuner.</param>
    public void ReloadConfiguration(Tuner configuration)
    {
      this.LogDebug("DiSEqC: reload configuration");

      if (configuration != null)
      {
        _alwaysSendCommands = configuration.AlwaysSendDiseqcCommands;
        if (_alwaysSendCommands)
        {
          _sendCommands = true;
        }
      }
      else
      {
        _alwaysSendCommands = false;
      }

      _siteLatitude = SettingsManagement.GetValue("usalsLatitude", 0.0);
      _siteLongitude = SettingsManagement.GetValue("usalsLongitude", 0.0);
      _siteAltitude = SettingsManagement.GetValue("usalsAltitude", 0);

      _positionerSpeedSlow = SettingsManagement.GetValue("diseqcMotorSpeedSlow", TunerSatellite.DISEQC_MOTOR_DEFAULT_SPEED_SLOW);
      _positionerSpeedFast = SettingsManagement.GetValue("diseqcMotorSpeedFast", TunerSatellite.DISEQC_MOTOR_DEFAULT_SPEED_FAST);

      this.LogDebug("  always send commands = {0}", _alwaysSendCommands);
      this.LogDebug("  USALS location/site...");
      this.LogDebug("    latitude           = {0}°", _siteLatitude);
      this.LogDebug("    longitude          = {0}°", _siteLongitude);
      this.LogDebug("    altitude           = {0} m", _siteAltitude);
      this.LogDebug("  positioner speed...");
      this.LogDebug("    slow               = {0} °/s", _positionerSpeedSlow);
      this.LogDebug("    fast               = {0} °/s", _positionerSpeedFast);
    }

    /// <summary>
    /// Tune to a specific channel.
    /// </summary>
    /// <remarks>
    /// In practise tuning means sending the required switch and positioner
    /// command(s) to enable receiving the channel.
    /// </remarks>
    /// <param name="channel">The channel to tune to.</param>
    /// <param name="satellite">The tuner-specific tuning parameters for the satellite that <paramref name="channel">the channel</paramref> is broadcast from.</param>
    public void Tune(IChannelSatellite channel, TunerSatellite satellite)
    {
      // If the channel is not set, this is a request for the controller to
      // forget the currently tuned channel. This will force commands to be
      // sent on the next call to Tune().
      if (channel == null)
      {
        _sendCommands = true;
        return;
      }

      // There is a well defined order in which commands may be sent:
      // "raw" DiSEqC commands -> DiSEqC 1.0 (committed) -> tone burst (simple DiSEqC) -> 22 kHz tone on/off
      this.LogDebug("DiSEqC: tune");
      _cancelTune = false;

      bool isHighBand = SatelliteLnbHandler.Is22kToneOn(channel.Frequency);

      // Switch command.
      DiseqcPort diseqcPort = (DiseqcPort)satellite.DiseqcPort;
      bool sendCommand = diseqcPort != DiseqcPort.None;
      if (sendCommand)
      {
        // There is a valid switch command to send, but we might not need/want to send it.
        if (
          !_sendCommands &&
          _currentPort == (DiseqcPort)satellite.DiseqcPort &&
          (
          // Polarisation and high band only matter for DiSEqC 1.0 switch
          // commands.
            (
              diseqcPort != DiseqcPort.PortA &&
              diseqcPort != DiseqcPort.PortB &&
              diseqcPort != DiseqcPort.PortC &&
              diseqcPort != DiseqcPort.PortD
            ) ||
            (
              _currentPolarisation == channel.Polarisation &&
              _currentIsHighBand == isHighBand
            )
          )
        )
        {
          sendCommand = false;
        }
      }
      if (!sendCommand)
      {
        this.LogDebug("DiSEqC: no need to send switch command");
      }
      else
      {
        byte[] command = new byte[4];
        command[0] = (byte)DiseqcFrame.CommandFirstTransmissionNoReply;
        command[1] = (byte)DiseqcAddress.AnySwitch;
        command[3] = 0xf0;
        int portNumber = GetPortNumber(diseqcPort);
        if (
          diseqcPort == DiseqcPort.PortA ||
          diseqcPort == DiseqcPort.PortB ||
          diseqcPort == DiseqcPort.PortC ||
          diseqcPort == DiseqcPort.PortD
        )
        {
          this.LogDebug("DiSEqC: DiSEqC 1.0 switch command, port = {0}", diseqcPort);
          command[2] = (byte)DiseqcCommand.WriteN0;
          command[3] |= (byte)(isHighBand ? 1 : 0);
          command[3] |= (byte)(SatelliteLnbHandler.IsHighVoltage(channel.Polarisation) ? 2 : 0);
          command[3] |= (byte)((portNumber - 1) << 2);
        }
        else
        {
          this.LogDebug("DiSEqC: DiSEqC 1.1 switch command, port = {0}", diseqcPort);
          command[2] = (byte)DiseqcCommand.WriteN1;
          command[3] |= (byte)(portNumber - 1);
        }

        ThrowExceptionIfTuneCancelled();
        _device.SendCommand(command);
        ThrowExceptionIfTuneCancelled();
      }

      // Positioner movement.
      bool sentSwitchCommand = sendCommand;
      sendCommand = satellite.DiseqcMotorPosition != TunerSatellite.DISEQC_MOTOR_POSITION_NONE;
      if (sendCommand)
      {
        // There is a valid positioner command to send, but we might not need/want to send it.
        if (
          !_sendCommands &&
          _currentStepsAzimuth == 0 &&
          _currentStepsElevation == 0 &&
          (
            // stored position
            (
              satellite.DiseqcMotorPosition != TunerSatellite.DISEQC_MOTOR_POSITION_USALS &&
              _currentPosition == satellite.DiseqcMotorPosition &&
              _currentLongitude == 0
            ) ||
            // positioned by longitude (USALS)
            (
              satellite.DiseqcMotorPosition == TunerSatellite.DISEQC_MOTOR_POSITION_USALS &&
              _currentPosition == 0 &&
              _currentLongitude == (double)satellite.Satellite.Longitude / 10
            )
          )
        )
        {
          sendCommand = false;
        }
      }
      if (!sendCommand)
      {
        this.LogDebug("DiSEqC: no need to send positioner command");
      }
      else
      {
        if (sentSwitchCommand)
        {
          // Assume the positioner is connected to the switch port that we just
          // selected. Give the positioner microcontroller time to power up.
          System.Threading.Thread.Sleep(100);
          ThrowExceptionIfTuneCancelled();
        }

        double speed = _positionerSpeedSlow;
        if (SatelliteLnbHandler.IsHighVoltage(_currentPolarisation))
        {
          speed = _positionerSpeedFast;
        }
        double newLongitude = (double)satellite.Satellite.Longitude / 10;
        double distance;  // unit = degrees; longitude difference
        if (_currentLongitude == LONGITUDE_UNKNOWN || _currentStepsAzimuth != 0 || _currentStepsElevation != 0)
        {
          distance = 90;  // assumed/average distance; intended to be a high estimate to avoid failure to lock on signal
        }
        else
        {
          distance = Math.Abs(newLongitude - _currentLongitude);
        }
        if (satellite.DiseqcMotorPosition == TunerSatellite.DISEQC_MOTOR_POSITION_USALS)
        {
          this.LogDebug("DiSEqC: USALS positioner command(s), longitude = {0}", satellite.Satellite.LongitudeString());
          GoToAngularPosition(newLongitude);
        }
        else
        {
          this.LogDebug("DiSEqC: positioner command(s), position = {0}", satellite.DiseqcMotorPosition);
          GoToStoredPosition((byte)satellite.DiseqcMotorPosition);
          _currentLongitude = newLongitude;
        }

        ThrowExceptionIfTuneCancelled();
        double waitTimeMilliSeconds = distance * 1000 / speed;
        this.LogDebug("DiSEqC: wait for positioner movement, distance = {0}°, speed = {1} °/s, wait time = {2} ms", distance, speed, waitTimeMilliSeconds);
        System.Threading.Thread.Sleep((int)waitTimeMilliSeconds);
      }

      // Tone burst and final state.
      ToneBurst toneBurst = (ToneBurst)satellite.ToneBurst;
      if (toneBurst != ToneBurst.None)
      {
        if (sendCommand)
        {
          // Delay to clearly distinguish the previously sent positioner
          // command from the burst we're about to send.
          System.Threading.Thread.Sleep(15);
        }
        _device.SendCommand(toneBurst);
        sentSwitchCommand = true;
      }

      if ((sentSwitchCommand || sendCommand) && isHighBand)
      {
        // Delay to clearly distinguish the previously sent positioner and/or
        // switch command from the continuous tone.
        System.Threading.Thread.Sleep(15);
      }
      _device.SetToneState(isHighBand ? Tone22kState.On : Tone22kState.Off);

      _sendCommands = _alwaysSendCommands;
      _currentPort = diseqcPort;
      _currentPolarisation = channel.Polarisation;
      _currentIsHighBand = isHighBand;
    }

    /// <summary>
    /// Cancel the current tuning process.
    /// </summary>
    public void CancelTune()
    {
      this.LogDebug("DiSEqC: cancel tune");
      _cancelTune = true;
    }

    #region IDiseqcController members

    /// <summary>
    /// Reset a device.
    /// </summary>
    public void Reset()
    {
      this.LogDebug("DiSEqC: reset");
      byte[] cmd = new byte[3];
      cmd[0] = (byte)DiseqcFrame.CommandFirstTransmissionNoReply;
      cmd[1] = (byte)DiseqcAddress.Any;
      cmd[2] = (byte)DiseqcCommand.Reset;
      _device.SendCommand(cmd);
      System.Threading.Thread.Sleep(100);

      this.LogDebug("DiSEqC: clear reset");
      cmd[0] = (byte)DiseqcFrame.CommandFirstTransmissionNoReply;
      cmd[1] = (byte)DiseqcAddress.Any;
      cmd[2] = (byte)DiseqcCommand.ClearReset;
      _device.SendCommand(cmd);
    }

    #region positioner (motor) control

    /// <summary>
    /// Stop the movement of a positioner device.
    /// </summary>
    public void Stop()
    {
      this.LogDebug("DiSEqC: stop positioner");
      byte[] cmd = new byte[3];
      cmd[0] = (byte)DiseqcFrame.CommandFirstTransmissionNoReply;
      cmd[1] = (byte)DiseqcAddress.AnyPositioner;
      cmd[2] = (byte)DiseqcCommand.Halt;
      _device.SendCommand(cmd);
    }

    /// <summary>
    /// Set the Eastward soft-limit of movement for a positioner device.
    /// </summary>
    public void SetEastLimit()
    {
      this.LogDebug("DiSEqC: set East limit");
      byte[] cmd = new byte[3];
      cmd[0] = (byte)DiseqcFrame.CommandFirstTransmissionNoReply;
      cmd[1] = (byte)DiseqcAddress.AzimuthPositioner;
      cmd[2] = (byte)DiseqcCommand.LimitEast;
      _device.SendCommand(cmd);
    }

    /// <summary>
    /// Set the Westward soft-limit of movement for a positioner device.
    /// </summary>
    public void SetWestLimit()
    {
      this.LogDebug("DiSEqC: set West limit");
      byte[] cmd = new byte[3];
      cmd[0] = (byte)DiseqcFrame.CommandFirstTransmissionNoReply;
      cmd[1] = (byte)DiseqcAddress.AzimuthPositioner;
      cmd[2] = (byte)DiseqcCommand.LimitWest;
      _device.SendCommand(cmd);
    }

    /// <summary>
    /// Enable or disable the movement soft-limits for a positioner device.
    /// </summary>
    public bool ForceLimits
    {
      set
      {
        byte[] cmd;
        if (value)
        {
          this.LogDebug("DiSEqC: enable limits");
          cmd = new byte[4];
          cmd[0] = (byte)DiseqcFrame.CommandFirstTransmissionNoReply;
          cmd[1] = (byte)DiseqcAddress.AzimuthPositioner;
          cmd[2] = (byte)DiseqcCommand.StorePosition;
          cmd[3] = 0;
        }
        else
        {
          this.LogDebug("DiSEqC: disable limits");
          cmd = new byte[3];
          cmd[0] = (byte)DiseqcFrame.CommandFirstTransmissionNoReply;
          cmd[1] = (byte)DiseqcAddress.AzimuthPositioner;
          cmd[2] = (byte)DiseqcCommand.LimitsOff;
        }
        _device.SendCommand(cmd);
      }
    }

    /// <summary>
    /// Drive a positioner device in a given direction.
    /// </summary>
    /// <param name="direction">The direction to move in.</param>
    /// <param name="stepCount">The number of position steps to move.</param>
    public void Drive(DiseqcDirection direction, byte stepCount)
    {
      this.LogDebug("DiSEqC: drive, direction = {0}, step count = {1}", direction, stepCount);
      if (stepCount == 0 || stepCount > 128)
      {
        // Prevent time-out-based movement (not supported).
        return;
      }

      Stop();
      byte[] cmd = new byte[4];
      cmd[0] = (byte)DiseqcFrame.CommandFirstTransmissionNoReply;
      if (direction == DiseqcDirection.West)
      {
        cmd[1] = (byte)DiseqcAddress.AzimuthPositioner;
        cmd[2] = (byte)DiseqcCommand.DriveWest;
        _currentStepsAzimuth -= stepCount;
      }
      else if (direction == DiseqcDirection.East)
      {
        cmd[1] = (byte)DiseqcAddress.AzimuthPositioner;
        cmd[2] = (byte)DiseqcCommand.DriveEast;
        _currentStepsAzimuth += stepCount;
      }
      else if (direction == DiseqcDirection.Up)
      {
        cmd[1] = (byte)DiseqcAddress.ElevationPositioner;
        cmd[2] = (byte)DiseqcCommand.DriveWest;
        _currentStepsElevation -= stepCount;
      }
      else if (direction == DiseqcDirection.Down)
      {
        cmd[1] = (byte)DiseqcAddress.ElevationPositioner;
        cmd[2] = (byte)DiseqcCommand.DriveEast;
        _currentStepsElevation += stepCount;
      }
      cmd[3] = (byte)(0x100 - stepCount);
      _device.SendCommand(cmd);
    }

    /// <summary>
    /// Store the current position of a positioner device for later use.
    /// </summary>
    /// <param name="position">The identifier to use for the position.</param>
    public void StorePosition(byte position)
    {
      this.LogDebug("DiSEqC: store current position, position = {0}", position);
      if (position <= 0)
      {
        throw new ArgumentException("DiSEqC position cannot be less than or equal to zero.");
      }

      byte[] cmd = new byte[4];
      cmd[0] = (byte)DiseqcFrame.CommandFirstTransmissionNoReply;
      cmd[1] = (byte)DiseqcAddress.AzimuthPositioner;
      cmd[2] = (byte)DiseqcCommand.StorePosition;
      cmd[3] = position;
      _device.SendCommand(cmd);

      // The current position becomes our reference.
      _currentPosition = position;
      _currentLongitude = LONGITUDE_UNKNOWN;
      _currentStepsAzimuth = 0;
      _currentStepsElevation = 0;
    }

    /// <summary>
    /// Drive a positioner device to its reference position.
    /// </summary>
    public void GoToReferencePosition()
    {
      this.LogDebug("DiSEqC: go to reference position");

      byte[] cmd = new byte[4];
      cmd[0] = (byte)DiseqcFrame.CommandFirstTransmissionNoReply;
      cmd[1] = (byte)DiseqcAddress.AzimuthPositioner;
      cmd[2] = (byte)DiseqcCommand.GotoPosition;
      cmd[3] = 0;
      _device.SendCommand(cmd);

      // The current position becomes our reference.
      _currentPosition = 0;
      _currentLongitude = LONGITUDE_UNKNOWN;
      _currentStepsAzimuth = 0;
      _currentStepsElevation = 0;
    }

    /// <summary>
    /// Drive a positioner device to a previously stored position.
    /// </summary>
    /// <param name="position">The position to drive to.</param>
    public void GoToStoredPosition(byte position)
    {
      this.LogDebug("DiSEqC: go to stored position, position = {0}", position);
      if (position <= 0)
      {
        throw new ArgumentException("DiSEqC position cannot be less than or equal to zero.");
      }

      byte[] cmd = new byte[4];
      cmd[0] = (byte)DiseqcFrame.CommandFirstTransmissionNoReply;
      cmd[1] = (byte)DiseqcAddress.AzimuthPositioner;
      cmd[2] = (byte)DiseqcCommand.GotoPosition;
      cmd[3] = position;
      _device.SendCommand(cmd);

      // The current position becomes our reference.
      _currentPosition = position;
      _currentLongitude = LONGITUDE_UNKNOWN;
      _currentStepsAzimuth = 0;
      _currentStepsElevation = 0;
    }

    /// <summary>
    /// Drive a positioner device to a given longitude.
    /// </summary>
    /// <param name="longitude">The longiude to drive to. Range -180 (180W) to 180 (180E).</param>
    public void GoToAngularPosition(double longitude)
    {
      this.LogDebug("DiSEqC: go to angular position, longitude = {0}°", longitude);

      double angle;
      if (!CalculateUsalsAngle(longitude, out angle))
      {
        return;
      }

      byte[] cmd = new byte[5];
      cmd[0] = (byte)DiseqcFrame.CommandFirstTransmissionNoReply;
      cmd[1] = (byte)DiseqcAddress.AzimuthPositioner;
      cmd[2] = (byte)DiseqcCommand.GotoAngularPosition;
      int intAngle = (int)Math.Round(angle * 16);
      if (intAngle > 0)
      {
        cmd[3] = 0xe0;
      }
      else
      {
        cmd[3] = 0xd0;
        intAngle *= -1;
      }
      cmd[3] |= (byte)((intAngle >> 8) & 0x0f);
      cmd[4] = (byte)(intAngle & 0xff);

      _device.SendCommand(cmd);

      _currentPosition = 0;
      _currentLongitude = longitude;
      _currentStepsAzimuth = 0;
      _currentStepsElevation = 0;
    }

    /// <summary>
    /// Get the current position of a positioner device.
    /// </summary>
    /// <param name="position">The stored position identifier corresponding with the current base position.</param>
    /// <param name="longitude">The longitude corresponding with the current base position.</param>
    /// <param name="stepsAzimuth">The number of steps taken from the base position on the azmutal axis.</param>
    /// <param name="stepsElevation">The number of steps taken from the base position on the vertical (elevation) axis.</param>
    public void GetPosition(out int position, out double longitude, out int stepsAzimuth, out int stepsElevation)
    {
      position = _currentPosition;
      longitude = _currentLongitude;
      stepsAzimuth = _currentStepsAzimuth;
      stepsElevation = _currentStepsElevation;
    }

    #endregion

    #endregion

    #region static helpers

    /// <summary>
    /// Get the switch port number (or LNB number) for a given DiSEqC switch command.
    /// </summary>
    /// <param name="command">The DiSEqC switch command.</param>
    /// <returns>the switch port number associated with the command</returns>
    private static int GetPortNumber(DiseqcPort command)
    {
      switch (command)
      {
        case DiseqcPort.None:
          return 0;
        // DiSEqC 1.0
        case DiseqcPort.PortA:
          return 1;
        case DiseqcPort.PortB:
          return 2;
        case DiseqcPort.PortC:
          return 3;
        case DiseqcPort.PortD:
          return 4;
        // DiSEqC 1.1
        case DiseqcPort.Port1:
          return 1;
        case DiseqcPort.Port2:
          return 2;
        case DiseqcPort.Port3:
          return 3;
        case DiseqcPort.Port4:
          return 4;
        case DiseqcPort.Port5:
          return 5;
        case DiseqcPort.Port6:
          return 6;
        case DiseqcPort.Port7:
          return 7;
        case DiseqcPort.Port8:
          return 8;
        case DiseqcPort.Port9:
          return 9;
        case DiseqcPort.Port10:
          return 10;
        case DiseqcPort.Port11:
          return 11;
        case DiseqcPort.Port12:
          return 12;
        case DiseqcPort.Port13:
          return 13;
        case DiseqcPort.Port14:
          return 14;
        case DiseqcPort.Port15:
          return 15;
        case DiseqcPort.Port16:
          return 16;
      }
      return 0;
    }

    /// <summary>
    /// Convert Degrees to Radians.
    /// </summary>
    private static double Radians(double number)
    {
      return number * Math.PI / 180;
    }

    /// <summary>
    /// Convert Radians to Degrees.
    /// </summary>
    private static double Degrees(double number)
    {
      return number * 180 / Math.PI;
    }

    #endregion

    /// <summary>
    /// Calculate the USALS angle for a satellite at a particular longitude.
    /// </summary>
    /// <param name="longitude">The satellite longitude. Range -180 (180W) to 180 (180E).</param>
    /// <param name"angle">The USALS angle. Range ~-90 (right) to 90 (left).</param>
    /// <returns><c>true</c> if the satellite is visible and the USALs angle is valid, otherwise <c>false</c></returns>
    private bool CalculateUsalsAngle(double longitude, out double angle)
    {
      // Refer to http://www.celestrak.com/NORAD/elements/supplemental/IESS_412_Rev_2.pdf
      angle = 0;

      double sinSiteLatitude = Math.Sin(Radians(_siteLatitude));
      double cosSiteLatitude = Math.Cos(Radians(_siteLatitude));
      int siteHeightAboveSeaLevel = _siteAltitude / 1000;   // divide converts metres to kilo-metres, as required for the calculation

      double R = RADIUS_EARTH_EQUATOR / Math.Sqrt(1 - EARTH_ECCENTRICITY_FACTOR * sinSiteLatitude * sinSiteLatitude);
      double Ra = (R + siteHeightAboveSeaLevel) * cosSiteLatitude;
      double Rz = (R * Math.Pow(1 - EARTH_FLATTENING_FACTOR, 2) + siteHeightAboveSeaLevel) * sinSiteLatitude;

      // Note: satellite latitude assumed to be 0 degrees.
      double delta_r_x = RADIUS_SATELLITE_ORBIT * Math.Cos(Radians(longitude - _siteLongitude)) - Ra;
      double delta_r_z = -Rz;
      double delta_r_zenith = delta_r_x * cosSiteLatitude + delta_r_z * sinSiteLatitude;
      if (delta_r_zenith < -3000)
      {
        // Satellite below horizon => not visible. Note that the code below
        // should still calculate azimuth and elevation correctly if this
        // condition were removed. Just saving effort...
        this.LogWarn("DiSEqC: failed to calculate USALS angle for satellite below the horizon, satellite longitude = {0}°, site latitude = {1}°, site longitude = {2}°, site altitude = {3} m", longitude, _siteLatitude, _siteLongitude, _siteAltitude);
        return false;
      }

      // Azimuth, range 0 to 360.
      double delta_r_y = RADIUS_SATELLITE_ORBIT * Math.Sin(Radians(longitude - _siteLongitude));
      double delta_r_north = -delta_r_x * sinSiteLatitude + delta_r_z * cosSiteLatitude;
      double Az = Degrees(Math.Atan(delta_r_y / delta_r_north));
      if (delta_r_north <= 0)
      {
        Az = 180 + Az;
      }
      else
      {
        Az = (360 + Az) % 360;
      }

      // If EL_geometric is negative it means the satellite is below the
      // horizon and not visible at the site. I've assumed that the elevation
      // equation/model should be symmetrical. That's probably wrong... but in
      // practice it doesn't really matter.
      double EL_geometric = Degrees(Math.Atan(delta_r_zenith / Math.Sqrt(Math.Pow(delta_r_north, 2) + Math.Pow(delta_r_y, 2))));
      double EL_observed;
      if (Math.Abs(EL_geometric) > 10.2)
      {
        EL_observed = EL_geometric + 0.01617 * Math.Cos(Radians(EL_geometric)) / Math.Sin(Radians(EL_geometric));
      }
      else
      {
        // Account for atmospheric refraction. The document doesn't specify,
        // but I think the model is only meant to calculate positive values of
        // elevation.
        double x = Math.Abs(EL_geometric) + 0.589;
        EL_observed = Math.Abs(EL_geometric) +
                      REFRACTION_CONST_A0 +
                      REFRACTION_CONST_A1 * x +
                      REFRACTION_CONST_A2 * Math.Pow(x, 2) +
                      REFRACTION_CONST_A3 * Math.Pow(x, 3) +
                      REFRACTION_CONST_A4 * Math.Pow(x, 4);
        if (EL_geometric < 0)
        {
          EL_observed *= -1;
        }
      }

      double numerator = -Math.Cos(Radians(EL_observed)) * Math.Sin(Radians(Az));
      double denominator = Math.Sin(Radians(EL_observed)) * cosSiteLatitude - Math.Cos(Radians(EL_observed)) * sinSiteLatitude * Math.Cos(Radians(Az));

      angle = Degrees(Math.Atan(numerator / denominator));
      if (_siteLatitude >= 0)
      {
        // Northern hemisphere
        // East = left = positive
        // West = right = negative
        angle *= -1;
      }
      // Southern hemisphere (already correct)
      // East = right = negative
      // West = left = positive
      this.LogDebug("DiSEqC: calculated USALS angle, satellite longitude = {0}°, site latitude = {1}°, site longitude = {2}°, site altitude = {3} m, azimuth = {4}, elevation = {5} ({6}), angle = {7}", longitude, _siteLatitude, _siteLongitude, _siteAltitude, Az, EL_observed, EL_geometric, angle);
      return true;
    }

    /// <summary>
    /// Check if the current tuning process has been cancelled and throw an
    /// exception if it has.
    /// </summary>
    private void ThrowExceptionIfTuneCancelled()
    {
      if (_cancelTune)
      {
        throw new TvExceptionTuneCancelled();
      }
    }
  }
}
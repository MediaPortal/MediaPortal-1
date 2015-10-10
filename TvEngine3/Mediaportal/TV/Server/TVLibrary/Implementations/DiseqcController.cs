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
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.Diseqc;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner.Diseqc.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.TVLibrary.Implementations
{
  /// <summary>
  /// A controller class for DiSEqC devices. This controller is able to control positioners and
  /// switches.
  /// </summary>
  internal class DiseqcController : IDiseqcController
  {
    #region constants

    private const double EARTH_FLATTENING_FACTOR = 1.00 / 298.25642;
    private const double EARTH_ECCENTRICITY_FACTOR = EARTH_FLATTENING_FACTOR * (2 - EARTH_FLATTENING_FACTOR);
    private const double RADIUS_SATELLITE_ORBIT = 42164.573;  // distance from the centre of Earth to the geostationary satellite orbit
    private const double RADIUS_EARTH_EQUATOR = 6378.1366;    // distance from the centre of Earth to sea level at the equator
    private const double REFRACTION_CONST_A0 = 0.58804392;
    private const double REFRACTION_CONST_A1 = -0.17941557;
    private const double REFRACTION_CONST_A2 = 0.29906946e-1;
    private const double REFRACTION_CONST_A3 = -0.25187400e-2;
    private const double REFRACTION_CONST_A4 = 0.82622101e-4;

    #endregion

    #region variables

    private int _tunerId = -1;
    private IDiseqcDevice _device = null;
    private IChannelSatellite _previousChannel = null;
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

    private double _siteLatitude = 0;
    private double _siteLongitude = 0;

    private int _currentPosition = -1;  // Ensure that we always send motor commands on first tune.
    private double _currentLongitude = 0;
    private int _currentStepsAzimuth;
    private int _currentStepsElevation;

    #endregion

    /// <summary>
    /// Initialise a new instance of the <see cref="DiseqcController"/> class.
    /// </summary>
    /// <param name="tunerId">The identifier for the associated tuner.</param>
    /// <param name="device">A tuner's DiSEqC control interface.</param>
    public DiseqcController(int tunerId, IDiseqcDevice device)
    {
      _tunerId = tunerId;
      _device = device;
      if (device == null)
      {
        throw new ArgumentException("DiSEqC device is null.");
      }
      ReloadConfiguration();
    }

    #region IDiseqcController members

    /// <summary>
    /// Reload the controller's configuration.
    /// </summary>
    public void ReloadConfiguration()
    {
      this.LogDebug("DiSEqC: reload configuration");
      Tuner tuner = TunerManagement.GetTuner(_tunerId, TunerIncludeRelationEnum.None);
      if (tuner != null)
      {
        _alwaysSendCommands = tuner.AlwaysSendDiseqcCommands;
      }
      else
      {
        _alwaysSendCommands = false;
      }
      this.LogDebug("  always send commands = {0}", _alwaysSendCommands);
    }

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

    /// <summary>
    /// Tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public void Tune(IChannelSatellite channel)
    {
      // If the channel is not set, this is a request for the controller to
      // forget the previously tuned channel. This will force commands to be
      // sent on the next call to Tune().
      if (channel == null)
      {
        _previousChannel = null;
        return;
      }

      // There is a well defined order in which commands may be sent:
      // "raw" DiSEqC commands -> DiSEqC 1.0 (committed) -> tone burst (simple DiSEqC) -> 22 kHz tone on/off
      this.LogDebug("DiSEqC: tune");
      _cancelTune = false;

      int lnbLof;
      int lnbLofSwitch;
      Tone22kState bandSelectionTone;
      Polarisation bandSelectionPolarisation;
      channel.LnbType.GetTuningParameters(channel.Frequency, channel.Polarisation, Tone22kState.Automatic, out lnbLof, out lnbLofSwitch, out bandSelectionTone, out bandSelectionPolarisation);
      bool isHighBand = channel.Frequency > lnbLofSwitch && lnbLofSwitch > 0;

      // Switch command.
      bool sendCommand = channel.DiseqcSwitchPort != DiseqcPort.None &&
        channel.DiseqcSwitchPort != DiseqcPort.SimpleA &&
        channel.DiseqcSwitchPort != DiseqcPort.SimpleB;
      if (sendCommand)
      {
        bool wasHighBand = !isHighBand;
        Polarisation previousChannelBandSelectionPolarisation = Polarisation.Automatic;
        if (_previousChannel != null)
        {
          Tone22kState previousChannelBandSelectionTone;
          channel.LnbType.GetTuningParameters(_previousChannel.Frequency, _previousChannel.Polarisation, Tone22kState.Automatic, out lnbLof, out lnbLofSwitch, out previousChannelBandSelectionTone, out previousChannelBandSelectionPolarisation);
          wasHighBand = _previousChannel.Frequency > lnbLofSwitch && lnbLofSwitch > 0;
        }

        // If we get to here then there is a valid command to send, but we
        // might not need/want to send it.
        if (
          !_alwaysSendCommands &&
          _previousChannel != null &&
          _previousChannel.DiseqcSwitchPort == channel.DiseqcSwitchPort &&
          (
            // Polarisation and high band only matter for DiSEqC 1.0 switch
            // commands.
            (
              channel.DiseqcSwitchPort != DiseqcPort.PortA &&
              channel.DiseqcSwitchPort != DiseqcPort.PortB &&
              channel.DiseqcSwitchPort != DiseqcPort.PortC &&
              channel.DiseqcSwitchPort != DiseqcPort.PortD
            ) ||
            (
              previousChannelBandSelectionPolarisation == bandSelectionPolarisation &&
              wasHighBand == isHighBand
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
        int portNumber = GetPortNumber(channel.DiseqcSwitchPort);
        if (
          channel.DiseqcSwitchPort == DiseqcPort.PortA ||
          channel.DiseqcSwitchPort == DiseqcPort.PortB ||
          channel.DiseqcSwitchPort == DiseqcPort.PortC ||
          channel.DiseqcSwitchPort == DiseqcPort.PortD
        )
        {
          this.LogDebug("DiSEqC: DiSEqC 1.0 switch command");
          command[2] = (byte)DiseqcCommand.WriteN0;
          bool isHorizontal = bandSelectionPolarisation == Polarisation.LinearHorizontal || channel.Polarisation == Polarisation.CircularLeft;
          command[3] |= (byte)(isHighBand ? 1 : 0);
          command[3] |= (byte)((isHorizontal) ? 2 : 0);
          command[3] |= (byte)((portNumber - 1) << 2);
        }
        else
        {
          this.LogDebug("DiSEqC: DiSEqC 1.1 switch command");
          command[2] = (byte)DiseqcCommand.WriteN1;
          command[3] |= (byte)(portNumber - 1);
        }

        ThrowExceptionIfTuneCancelled();
        _device.SendCommand(command);
        ThrowExceptionIfTuneCancelled();
      }

      // Positioner movement.
      bool sentSwitchCommand = sendCommand;
      sendCommand = channel.DiseqcPositionerSatelliteIndex >= 0;
      if (sendCommand)
      {
        if (
          !_alwaysSendCommands &&
          _currentStepsAzimuth == 0 &&
          _currentStepsElevation == 0 &&
          channel.DiseqcPositionerSatelliteIndex == _currentPosition
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

        this.LogDebug("DiSEqC: positioner command(s)");
        GoToStoredPosition((byte)channel.DiseqcPositionerSatelliteIndex);
        ThrowExceptionIfTuneCancelled();
      }

      // Tone burst and final state.
      if (channel.DiseqcSwitchPort == DiseqcPort.SimpleA || channel.DiseqcSwitchPort == DiseqcPort.SimpleB)
      {
        if (sendCommand)
        {
          // Delay to clearly distinguish the previously sent positioner
          // command from the burst we're about to send.
          System.Threading.Thread.Sleep(15);
        }
        if (channel.DiseqcSwitchPort == DiseqcPort.SimpleA)
        {
          _device.SendCommand(ToneBurst.ToneBurst);
        }
        else
        {
          _device.SendCommand(ToneBurst.DataBurst);
        }
        sentSwitchCommand = true;
      }

      if ((sentSwitchCommand || sendCommand) && bandSelectionTone == Tone22kState.On)
      {
        // Delay to clearly distinguish the previously sent positioner and/or
        // switch command from the continuous tone.
        System.Threading.Thread.Sleep(15);
      }
      _device.SetToneState(bandSelectionTone);

      _previousChannel = channel;
    }

    /// <summary>
    /// Cancel the current tuning process.
    /// </summary>
    public void CancelTune()
    {
      this.LogDebug("DiSEqC: cancel tune");
      _cancelTune = true;
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
    public void DriveMotor(DiseqcDirection direction, byte stepCount)
    {
      this.LogDebug("DiSEqC: drive motor, direction = {0}, step count = {1}", direction, stepCount);
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
      _currentLongitude = 0;
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
      _currentLongitude = 0;
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
      _currentLongitude = 0;
      _currentStepsAzimuth = 0;
      _currentStepsElevation = 0;
    }

    /// <summary>
    /// Drive a positioner device to a given longitude.
    /// </summary>
    /// <param name="longitude">The longiude to drive to. Range -180 (180W) to 180 (180E).</param>
    public void GoToAngularPosition(double longitude)
    {
      this.LogDebug("DiSEqC: go to angular position, longitude = {0}", longitude);

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
        // DiSEqC 1.0 simple
        case DiseqcPort.SimpleA:
          return 1;
        case DiseqcPort.SimpleB:
          return 2;
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
      int siteHeightAboveSeaLevel = 0;  // not known

      double sinSiteLatitude = Math.Sin(Radians(_siteLatitude));
      double cosSiteLatitude = Math.Cos(Radians(_siteLatitude));

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
        this.LogWarn("DiSEqC: failed to calculate USALS angle for satellite below the horizon, satellite longitude = {0}, site latitude = {1}, site longitude = {2}", longitude, _siteLatitude, _siteLongitude);
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
      this.LogDebug("DiSEqC: calculated USALS angle, satellite longitude = {0}, site latitude = {1}, site longitude = {2}, azimuth = {3}, elevation = {4} ({5}), angle = {6}", longitude, _siteLatitude, _siteLongitude, Az, EL_observed, EL_geometric, angle);
      return true;
    }

    /// <summary>
    /// Check if the current tuning process has been cancelled and throw an exception if it has.
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
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
using TvLibrary.Interfaces;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// Class which handles DiSEqC motors
  /// </summary>
  public class DiSEqCMotor : IDiSEqCMotor
  {
    #region enums

    /// <summary>
    /// DisEqC motor commands
    /// </summary>
    public enum DiSEqCCommands : byte
    {
      /// <summary>
      /// Reset
      /// </summary>
      Reset = 0,

      /// <summary>
      /// ClearReset
      /// </summary>
      ClearReset = 0x1,

      /// <summary>
      /// StandBye
      /// </summary>
      StandBye = 0x2,

      /// <summary>
      /// StandByeOff
      /// </summary>
      StandByeOff = 0x3,

      /// <summary>
      /// PowerOn
      /// </summary>
      PowerOn = 0x3,

      /// <summary>
      /// halt motor
      /// </summary>
      Halt = 0x60, //  3 bytes
      /// <summary>
      /// turn soft limits off
      /// </summary>
      LimitsOff = 0x63, //  3 bytes
      /// <summary>
      /// read current position (requires diseqc 2.2)
      /// </summary>
      ReadPosition = 0x64, //  3 bytes
      /// <summary>
      /// sets the east limit
      /// </summary>
      SetEastLimit = 0x66, //  3 bytes  
      /// <summary>
      /// sets the west limit
      /// </summary>
      SetWestLimit = 0x67,
      /// <summary>
      /// move east
      /// </summary>
      DriveEast = 0x68, //  4 bytes  
      /// <summary>
      /// move west
      /// </summary>
      DriveWest = 0x69, //  4 bytes  
      /// <summary>
      /// store current position
      /// </summary>
      StorePositions = 0x6a, //  4 bytes  
      /// <summary>
      /// goto stored position
      /// </summary>
      GotoPosition = 0x6b, //  4 bytes  
      /// <summary>
      /// goto angular position
      /// </summary>
      GotoAngularPosition = 0x6e, //  5 bytes  
      /// <summary>
      /// recalcuate positions
      /// </summary>
      RecalculatePositions = 0x6f //  4/6 bytes  
    }

    ///<summary>
    /// DiseqC Position flags
    ///</summary>
    public enum DiSEqCPositionFlags : byte
    {
      /// <summary>
      /// last command has completed
      /// </summary>
      CommandCompleted = 0x80,
      /// <summary>
      /// software limits are enabled
      /// </summary>
      SoftwareLimitsEnabled = 0x40,
      /// <summary>
      /// last movement was west
      /// </summary>
      DirectionWest = 0x20,
      /// <summary>
      /// motor is running
      /// </summary>
      MotorRunning = 0x10,
      /// <summary>
      /// software limits are reached
      /// </summary>
      SoftwareLimitReached = 0x8,
      /// <summary>
      /// power is not available
      /// </summary>
      PowerNotAvailable = 0x4,
      /// <summary>
      /// hardware switch is activated
      /// </summary>
      HardwareSwitchActivated = 0x2,
      /// <summary>
      /// reference position is corrupted or lost
      /// </summary>
      PositionReferenceLost = 0x1,
    }

    /// <summary>
    /// DiseqC Framing
    /// </summary>
    public enum DiSEqCFraming : byte
    {
      /// <summary>
      /// diseqc framing byte, first transmission
      /// </summary>
      FirstTransmission = 0xe0,
      /// <summary>
      /// diseqc framing byte, repeated transmission
      /// </summary>
      RepeatedTransmission = 0xe1,
      /// <summary>
      /// diseqc framing byte first transmission, request a reply
      /// </summary>
      FirstTransmissionReply = 0xe2,
      /// <summary>
      /// diseqc framing byte repeated transmission, request a reply
      /// </summary>
      RepeatedTransmissionReply = 0xe3,
      /// <summary>
      /// diseqc reply ok, no errors detected
      /// </summary>
      ReplyOk = 0xe4,
      /// <summary>
      /// diseqc reply error, command not supported
      /// </summary>
      ReplyCommandNotSupported = 0xe5,
      /// <summary>
      /// diseqc reply error, parity error detected
      /// </summary>
      ReplyParityError = 0xe6,
      /// <summary>
      /// diseqc reply error, unknown command
      /// </summary>
      ReplyUnknownCommand = 0xe7
    }

    /// <summary>
    /// DiseqC Movement
    /// </summary>
    public enum DiSEqCMovement : byte
    {
      /// <summary>
      /// wildcard for both directions
      /// </summary>
      Both = 0x30,
      /// <summary>
      /// move along azimutal axis
      /// </summary>
      Azimutal = 0x31,
      /// <summary>
      /// move along elivation axis
      /// </summary>
      Elivation = 0x32
    }

    #endregion

    #region variables

    private readonly IDiSEqCController _controller;
    private int _currentPosition = -1;
    private int _currentStepsAzimuth;
    private int _currentStepsElevation;
    private double _siteLat = 0;
    private double _siteLong = 0;
    private bool _usalsEnabled = false;
    private double _currentSatLong = 0;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="DiSEqCMotor"/> class.
    /// </summary>
    /// <param name="controller">The controller.</param>
    public DiSEqCMotor(IDiSEqCController controller)
    {
      _controller = controller;
    }

    /// <summary>
    /// Reset.
    /// </summary>
    public void Reset()
    {
      byte[] cmd = new byte[3];
      Log.Log.Write("DiSEqC: ClearReset");
      cmd[0] = (byte)DiSEqCFraming.FirstTransmission;
      cmd[1] = 0x10;
      cmd[2] = (byte)DiSEqCCommands.ClearReset;
      _controller.SendDiSEqCCommand(cmd);
      System.Threading.Thread.Sleep(100);

      Log.Log.Write("DiSEqC: PowerOn");
      cmd[0] = (byte)DiSEqCFraming.FirstTransmission;
      cmd[1] = 0x10;
      cmd[2] = (byte)DiSEqCCommands.PowerOn;
      _controller.SendDiSEqCCommand(cmd);
      System.Threading.Thread.Sleep(100);

      Log.Log.Write("DiSEqC: reset");
      cmd[0] = (byte)DiSEqCFraming.FirstTransmission;
      cmd[1] = 0x10;
      cmd[2] = (byte)DiSEqCCommands.Reset;
      _controller.SendDiSEqCCommand(cmd);
      System.Threading.Thread.Sleep(100);

      Log.Log.Write("DiSEqC: clear reset");
      cmd[0] = (byte)DiSEqCFraming.FirstTransmission;
      cmd[1] = 0x10;
      cmd[2] = (byte)DiSEqCCommands.ClearReset;
      _controller.SendDiSEqCCommand(cmd);
      System.Threading.Thread.Sleep(100);

      Log.Log.Write("DiSEqC: PowerOn");
      cmd[0] = (byte)DiSEqCFraming.FirstTransmission;
      cmd[1] = 0x10;
      cmd[2] = (byte)DiSEqCCommands.PowerOn;
      _controller.SendDiSEqCCommand(cmd);
    }

    /// <summary>
    /// Stops the motor.
    /// </summary>
    public void StopMotor()
    {
      Log.Log.Write("DiSEqC: stop motor");
      byte[] cmd = new byte[3];
      cmd[0] = (byte)DiSEqCFraming.FirstTransmission;
      cmd[1] = (byte)DiSEqCMovement.Azimutal;
      cmd[2] = (byte)DiSEqCCommands.Halt;
      _controller.SendDiSEqCCommand(cmd);
      System.Threading.Thread.Sleep(100);
    }

    /// <summary>
    /// Sets the east limit.
    /// </summary>
    public void SetEastLimit()
    {
      Log.Log.Write("DiSEqC: set east limit");
      byte[] cmd = new byte[3];
      cmd[0] = (byte)DiSEqCFraming.FirstTransmission;
      cmd[1] = (byte)DiSEqCMovement.Azimutal;
      cmd[2] = (byte)DiSEqCCommands.SetEastLimit;
      _controller.SendDiSEqCCommand(cmd);
      System.Threading.Thread.Sleep(100);
    }

    /// <summary>
    /// Sets the west limit.
    /// </summary>
    public void SetWestLimit()
    {
      Log.Log.Write("DiSEqC: set west limit");
      byte[] cmd = new byte[3];
      cmd[0] = (byte)DiSEqCFraming.FirstTransmission;
      cmd[1] = (byte)DiSEqCMovement.Azimutal;
      cmd[2] = (byte)DiSEqCCommands.SetWestLimit;
      _controller.SendDiSEqCCommand(cmd);
      System.Threading.Thread.Sleep(100);
    }


    /// <summary>
    /// Enable/disables the west/east limits.
    /// </summary>
    public bool ForceLimits
    {
      set
      {
        if (value)
        {
          Log.Log.Write("DiSEqC: enable limits");
          byte[] cmd = new byte[4];
          cmd[0] = (byte)DiSEqCFraming.FirstTransmission;
          cmd[1] = (byte)DiSEqCMovement.Azimutal;
          cmd[2] = (byte)DiSEqCCommands.StorePositions;
          cmd[3] = 0;
          _controller.SendDiSEqCCommand(cmd);
          System.Threading.Thread.Sleep(100);
        }
        else
        {
          Log.Log.Write("DiSEqC: disable limits");
          byte[] cmd = new byte[3];
          cmd[0] = (byte)DiSEqCFraming.FirstTransmission;
          cmd[1] = (byte)DiSEqCMovement.Azimutal;
          cmd[2] = (byte)DiSEqCCommands.LimitsOff;
          _controller.SendDiSEqCCommand(cmd);
          System.Threading.Thread.Sleep(100);
        }
      }
    }

    /// <summary>
    /// Get's or Set's whether to use Usals
    /// </summary>
    public bool UsalsEnabled
    {
        set { _usalsEnabled = value; }
        get { return _usalsEnabled; }
    }

    /// <summary>
    /// Drives the motor.
    /// </summary>
    /// <param name="direction">The direction.</param>
    /// <param name="steps">the number of steps to move.</param>
    public void DriveMotor(DiSEqCDirection direction, byte steps)
    {
      if (steps == 0)
        return;
      StopMotor();
      Log.Log.Write("DiSEqC: drive motor {0} for {1} steps", direction.ToString(), steps);
      byte[] cmd = new byte[4];
      cmd[0] = (byte)DiSEqCFraming.FirstTransmission;
      if (direction == DiSEqCDirection.West)
      {
        cmd[1] = (byte)DiSEqCMovement.Azimutal;
        cmd[2] = (byte)DiSEqCCommands.DriveWest;
        _currentStepsAzimuth -= steps;
      }
      else if (direction == DiSEqCDirection.East)
      {
        cmd[1] = (byte)DiSEqCMovement.Azimutal;
        cmd[2] = (byte)DiSEqCCommands.DriveEast;
        _currentStepsAzimuth += steps;
      }
      else if (direction == DiSEqCDirection.Up)
      {
        cmd[1] = (byte)DiSEqCMovement.Elivation;
        cmd[2] = (byte)DiSEqCCommands.DriveWest;
        _currentStepsElevation -= steps;
      }
      else if (direction == DiSEqCDirection.Down)
      {
        cmd[1] = (byte)DiSEqCMovement.Elivation;
        cmd[2] = (byte)DiSEqCCommands.DriveEast;
        _currentStepsElevation += steps;
      }
      cmd[3] = (byte)(0x100 - steps);
      _controller.SendDiSEqCCommand(cmd);
      System.Threading.Thread.Sleep(100);
      //System.Threading.Thread.Sleep(1000*steps);
      //StopMotor();
    }

    /// <summary>
    /// Stores the position.
    /// </summary>
    public void StorePosition(byte position)
    {
      if (position <= 0)
        throw new ArgumentException("position");
      Log.Log.Write("DiSEqC: store current position in {0}", position);
      byte[] cmd = new byte[4];
      cmd[0] = (byte)DiSEqCFraming.FirstTransmission;
      cmd[1] = (byte)DiSEqCMovement.Azimutal;
      cmd[2] = (byte)DiSEqCCommands.StorePositions;
      cmd[3] = position;
      _controller.SendDiSEqCCommand(cmd);
      System.Threading.Thread.Sleep(100);
      cmd[0] = (byte)DiSEqCFraming.RepeatedTransmission;
      _controller.SendDiSEqCCommand(cmd);
      System.Threading.Thread.Sleep(100);
      _currentPosition = position;
      _currentStepsAzimuth = 0;
      _currentStepsElevation = 0;
    }

    /// <summary>
    /// Goto's the sattelite reference position.
    /// </summary>
    public void GotoReferencePosition()
    {
      Log.Log.Write("DiSEqC: goto reference position");
      byte[] cmd = new byte[4];
      cmd[0] = (byte)DiSEqCFraming.FirstTransmission;
      cmd[1] = (byte)DiSEqCMovement.Azimutal;
      cmd[2] = (byte)DiSEqCCommands.GotoPosition;
      cmd[3] = 0;
      _controller.SendDiSEqCCommand(cmd);
      System.Threading.Thread.Sleep(100);
      cmd[0] = (byte)DiSEqCFraming.RepeatedTransmission;
      _controller.SendDiSEqCCommand(cmd);
      System.Threading.Thread.Sleep(100);
      _currentPosition = 0;
      _currentStepsAzimuth = 0;
      _currentStepsElevation = 0;
    }

    /// <summary>
    /// Goto's the position.
    /// </summary>
    public void GotoPosition(byte position)
    {
      if (position <= 0)
        throw new ArgumentException("position");
      if (_currentStepsAzimuth == 0 && _currentStepsElevation == 0 && position == _currentPosition)
        return;
      Log.Log.Write("DiSEqC: goto position {0}", position);
      byte[] cmd = new byte[4];
      cmd[0] = (byte)DiSEqCFraming.FirstTransmission;
      cmd[1] = (byte)DiSEqCMovement.Azimutal;
      cmd[2] = (byte)DiSEqCCommands.GotoPosition;
      cmd[3] = position;
      _controller.SendDiSEqCCommand(cmd);
      System.Threading.Thread.Sleep(100);
      cmd[0] = (byte)DiSEqCFraming.RepeatedTransmission;
      _controller.SendDiSEqCCommand(cmd);
      System.Threading.Thread.Sleep(100);
      _currentPosition = position;
      _currentStepsAzimuth = 0;
      _currentStepsElevation = 0;
    }

    /// <summary>
    /// Gets the current motor position.
    /// </summary>
    public void GetPosition(out int satellitePosition, out int stepsAzimuth, out int stepsElevation)
    {
      satellitePosition = _currentPosition;
      stepsAzimuth = _currentStepsAzimuth;
      stepsElevation = _currentStepsElevation;
    }

          /// <summary>
    /// Goto the USALS position.
    /// </summary>
    public void GotoUSALSPosition(double satLong, bool IsSetup)
    {
        if (satLong == _currentSatLong)
            return;

        Log.Log.Write("DiSEqC: gotoUSALS Satellite Longitude {0}", satLong);
        
        double WaitOut = 25000;

        if (_currentSatLong != 0)
        {
            WaitOut = (Math.Abs(_currentSatLong - satLong) * 600);
        }

        int[] gotoXTable;

        gotoXTable = new int[10];
        gotoXTable[0] = 0x00;
        gotoXTable[1] = 0x02;
        gotoXTable[2] = 0x03;
        gotoXTable[3] = 0x05;
        gotoXTable[4] = 0x06;
        gotoXTable[5] = 0x08;
        gotoXTable[6] = 0x0A;
        gotoXTable[7] = 0x0B;
        gotoXTable[8] = 0x0D;
        gotoXTable[9] = 0x0E;

        double SatLon = satLong;

        Int32 RotorCmd;

       
        double SiteLat = _siteLat;
        double SiteLon = _siteLong;

        if (SiteLon < 0)
            SiteLon = 360 - Math.Abs(SiteLon);

        if (SatLon < 0)
            SatLon = 360 - Math.Abs(SatLon);

        double azimuth = calcAzimuth(SatLon, SiteLat, SiteLon);
        double elevation = calcElevation(SatLon, SiteLat, SiteLon);
        double declination = calcDeclination(SiteLat, azimuth, elevation);
        double satHourAngle = calcSatHourangle(azimuth, elevation, declination, SiteLat);

        if (SiteLat >= 0)
        {
            // Northern Hemisphere
            int tmp = (int)Math.Round(Math.Abs(180 - satHourAngle) * 10.0);
            RotorCmd = (tmp / 10) * 0x10 + gotoXTable[tmp % 10];

            if (satHourAngle < 180)  // the east
                RotorCmd |= 0xE000;
            else                     // west
                RotorCmd |= 0xD000;
        }
        else
        {
            // Southern Hemisphere
            if (satHourAngle < 180)  // the east
            {
                int tmp = (int)Math.Round(Math.Abs(satHourAngle) * 10.0);
                RotorCmd = (tmp / 10) * 0x10 + gotoXTable[tmp % 10];
                RotorCmd |= 0xD000;
            }
            else
            {                     // west
                int tmp = (int)Math.Round(Math.Abs(360 - satHourAngle) * 10.0);
                RotorCmd = (tmp / 10) * 0x10 + gotoXTable[tmp % 10];
                RotorCmd |= 0xE000;
            }
        }
        
        byte[] cmd = new byte[5];
        cmd[0] = (byte)DiSEqCFraming.FirstTransmission;
        cmd[1] = (byte)DiSEqCMovement.Azimutal;
        cmd[2] = (byte)DiSEqCCommands.GotoAngularPosition;
        cmd[3] = (byte)(RotorCmd >> 8);
        cmd[4] = (byte)(RotorCmd & 0xFF);
        _controller.SendDiSEqCCommand(cmd);
        System.Threading.Thread.Sleep(100);
        cmd[0] = (byte)DiSEqCFraming.RepeatedTransmission;
        _controller.SendDiSEqCCommand(cmd);
        if (IsSetup)
            WaitOut = 100;
        System.Threading.Thread.Sleep(Convert.ToInt32(WaitOut));
        _currentSatLong = satLong;
        _currentStepsAzimuth = 0;
        _currentStepsElevation = 0;
    }

    /// <summary>
    /// Set Site Latitude
    /// </summary>
    /// <param name="Lat">Latitude</param>
    public double SiteLat
    {
        set
        {
            Log.Log.Debug("Site Latitude Changed to - " + value);
            _siteLat = value;
        }
    }

    /// <summary>
    /// Set Site Longitude
    /// </summary>
    /// <param name="Long">Longitude</param>
    public double SiteLong
    {
        set
        {
            Log.Log.Debug("Site Longitude Changed to - " + value);
            _siteLong = value;
        }
    }

    /// <summary>
    /// Convert Degrees to Radians
    /// </summary>
    double Radians( double number )
    {
        return number * Math.PI / 180;
    }

    /// <summary>
    /// Convert Radians to Degrees
    /// </summary>
    double Deg( double number )
    {
        return number*180/Math.PI;
    }

    /// <summary>
    /// Reverse Longitude
    /// </summary>
    double Rev(double number)
    {
        return number - Math.Floor(number / 360.0) * 360;
    }

    /// <summary>
    /// Calculates Elevation
    /// </summary>
    double calcElevation(double SatLon, double SiteLat, double SiteLon)
    {
    int Height_over_ocean = 0;
    const double a0 = 0.58804392, a1 = -0.17941557, a2 = 0.29906946E-1, a3 = -0.25187400E-2, a4 = 0.82622101E-4;
	const double f = 1.00 / 298.257;	// Earth flattning factor
	const double r_sat = 42164.57;		// Distance from earth centre to satellite
	const double r_eq = 6378.14;		// Earth radius
	double sinRadSiteLat = Math.Sin(Radians(SiteLat)), cosRadSiteLat = Math.Cos(Radians(SiteLat));
	double Rstation = r_eq / (Math.Sqrt(1.00 - f * (2.00 - f) * sinRadSiteLat * sinRadSiteLat));
	double Ra = (Rstation + Height_over_ocean) * cosRadSiteLat;
	double Rz = Rstation * (1.00 - f) * (1.00 - f) * sinRadSiteLat;
	double alfa_rx = r_sat * Math.Cos(Radians(SatLon - SiteLon)) - Ra;
	double alfa_ry = r_sat * Math.Sin(Radians(SatLon - SiteLon));
	double alfa_rz = -Rz, alfa_r_north = -alfa_rx * sinRadSiteLat + alfa_rz * cosRadSiteLat;
	double alfa_r_zenith = alfa_rx * cosRadSiteLat + alfa_rz * sinRadSiteLat;
	double El_geometric = Deg(Math.Atan(alfa_r_zenith / Math.Sqrt(alfa_r_north * alfa_r_north + alfa_ry * alfa_ry)));
	double x = Math.Abs(El_geometric + 0.589);
	double refraction = Math.Abs(a0 + a1 * x + a2 * x * x + a3 * x * x * x + a4 * x * x * x * x);
	double El_observed = 0.00;

	if (El_geometric > 10.2)
        El_observed = El_geometric + 0.01617 * (Math.Cos(Radians(Math.Abs(El_geometric))) / Math.Sin(Radians(Math.Abs(El_geometric))));
	else {
		El_observed = El_geometric + refraction;
	}

	if (alfa_r_zenith < -3000)
		El_observed = -99;

	return El_observed;
    }

    /// <summary>
    /// Calculates Azimuth
    /// </summary>
    double calcAzimuth(double SatLon, double SiteLat, double SiteLon)
    {
        int Height_over_ocean = 0;
	    const double f = 1.00 / 298.257;	// Earth flattning factor
	    const double r_sat = 42164.57;		// Distance from earth centre to satellite
	    const double r_eq = 6378.14;		// Earth radius
    
    	double sinRadSiteLat = Math.Sin(Radians(SiteLat)), cosRadSiteLat = Math.Cos(Radians(SiteLat));
    	double Rstation = r_eq / (Math.Sqrt(1 - f * (2 - f) * sinRadSiteLat * sinRadSiteLat));
    	double Ra = (Rstation + Height_over_ocean) * cosRadSiteLat;
    	double Rz = Rstation * (1 - f) * (1 - f) * sinRadSiteLat;
    	double alfa_rx = r_sat * Math.Cos(Radians(SatLon - SiteLon)) - Ra;
    	double alfa_ry = r_sat * Math.Sin(Radians(SatLon - SiteLon));
    	double alfa_rz = -Rz;
    	double alfa_r_north = -alfa_rx * sinRadSiteLat + alfa_rz * cosRadSiteLat;
    	double Azimuth = 0.00;
    
    	if (alfa_r_north < 0)
		    Azimuth = 180 + Deg(Math.Atan(alfa_ry / alfa_r_north));
	    else
            Azimuth = Rev(360 + Deg(Math.Atan(alfa_ry / alfa_r_north)));
    
    	return Azimuth;
    }

    /// <summary>
    /// Calculates Declination
    /// </summary>
    double calcDeclination(double SiteLat, double Azimuth, double Elevation)
    {
        return Deg(Math.Asin(Math.Sin(Radians(Elevation)) *
                    Math.Sin(Radians(SiteLat)) +
                    Math.Cos(Radians(Elevation)) *
                    Math.Cos(Radians(SiteLat)) +
                    Math.Cos(Radians(Azimuth))));
    }

    /// <summary>
    /// Calculates Satellite Hour Angle
    /// </summary>
    double calcSatHourangle( double Azimuth, double Elevation, double Declination, double Lat )
    {
    	double a = - Math.Cos(Radians(Elevation)) *
					 Math.Sin(Radians(Azimuth)),
               b =   Math.Sin(Radians(Elevation)) *
					 Math.Cos(Radians(Lat)) -
					 Math.Cos(Radians(Elevation)) *
					 Math.Sin(Radians(Lat)) *
					 Math.Cos(Radians(Azimuth)),

        // Works for all azimuths (northern & sourhern hemisphere)
					 returnvalue = 180 + Deg(Math.Atan(a/b));
        if ( Azimuth > 270 )
	    {
		    returnvalue = ( (returnvalue-180) + 360 );
		    if (returnvalue>360)
			    returnvalue = 360 - (returnvalue-360);
        }

	    if ( Azimuth < 90 )
		    returnvalue = ( 180 - returnvalue );

	    return returnvalue;
    }
  
  }


}
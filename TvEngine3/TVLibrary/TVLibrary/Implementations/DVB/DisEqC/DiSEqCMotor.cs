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
  }
}
using System;
using System.Collections.Generic;
using System.Text;
using TvLibrary.Log;
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
      /// halt motor
      /// </summary>
      Halt = 0x60,                    //  3 bytes
      /// <summary>
      /// turn soft limits off
      /// </summary>
      LimitsOff = 0x63,               //  3 bytes
      /// <summary>
      /// read current position (requires diseqc 2.2)
      /// </summary>
      ReadPosition = 0x64,            //  3 bytes
      /// <summary>
      /// sets the east limit
      /// </summary>
      SetEastLimit = 0x66,            //  3 bytes  
      /// <summary>
      /// sets the west limit
      /// </summary>
      SetWestLimit = 0x67,
      /// <summary>
      /// move east
      /// </summary>
      DriveEast = 0x68,               //  4 bytes  
      /// <summary>
      /// move west
      /// </summary>
      DriveWest = 0x69,               //  4 bytes  
      /// <summary>
      /// store current position
      /// </summary>
      StorePositions = 0x6a,          //  4 bytes  
      /// <summary>
      /// goto stored position
      /// </summary>
      GotoPosition = 0x6b,            //  4 bytes  
      /// <summary>
      /// goto angular position
      /// </summary>
      GotoAngularPosition = 0x6e,     //  5 bytes  
      /// <summary>
      /// recalcuate positions
      /// </summary>
      RecalculatePositions = 0x6f     //  4/6 bytes  
    }
    public enum DiSEqCPositionFlags : byte
    {
      CommandCompleted = 0x80,
      SoftwareLimitsEnabled = 0x40,
      DirectionWest = 0x20,
      MotorRunning = 0x10,
      SoftwareLimitReached = 0x8,
      PowerNotAvailable = 0x4,
      HardwareSwitchActivated = 0x2,
      PositionReferenceLost = 0x1,
    }
    #endregion

    #region constants
    /// <summary>
    /// move along the azimutal axis
    /// </summary>
    const byte Azimutal = 0x31;
    /// <summary>
    /// move along the elivation axis
    /// </summary>
    const byte Elivation = 0x32;
    /// <summary>
    /// apply to both directions
    /// </summary>
    const byte AllDirections = 0x30;
    /// <summary>
    /// diseqc framing byte
    /// </summary>
    const byte FramingByte = 0xe0;
    #endregion

    #region variables
    IDiSEqCController _controller;
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
    /// Stops the motor.
    /// </summary>
    public void StopMotor()
    {
      Log.Log.Write("DiSEqC: stop motor");
      byte[] cmd = new byte[3];
      cmd[0] = FramingByte;
      cmd[1] = AllDirections;
      cmd[2] = (byte)DiSEqCCommands.Halt;
      _controller.SendDiSEqCCommand(cmd);
    }

    /// <summary>
    /// Sets the east limit.
    /// </summary>
    public void SetEastLimit()
    {
      Log.Log.Write("DiSEqC: set east limit");
      byte[] cmd = new byte[3];
      cmd[0] = FramingByte;
      cmd[1] = AllDirections;
      cmd[2] = (byte)DiSEqCCommands.SetEastLimit;
      _controller.SendDiSEqCCommand(cmd);
    }

    /// <summary>
    /// Sets the west limit.
    /// </summary>
    public void SetWestLimit()
    {
      Log.Log.Write("DiSEqC: set west limit");
      byte[] cmd = new byte[3];
      cmd[0] = FramingByte;
      cmd[1] = AllDirections;
      cmd[2] = (byte)DiSEqCCommands.SetWestLimit;
      _controller.SendDiSEqCCommand(cmd);
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
          cmd[0] = FramingByte;
          cmd[1] = AllDirections;
          cmd[2] = (byte)DiSEqCCommands.StorePositions;
          cmd[3] = 0;
          _controller.SendDiSEqCCommand(cmd);
        }
        else
        {
          Log.Log.Write("DiSEqC: disable limits");
          byte[] cmd = new byte[3];
          cmd[0] = FramingByte;
          cmd[1] = AllDirections;
          cmd[2] = (byte)DiSEqCCommands.LimitsOff;
          _controller.SendDiSEqCCommand(cmd);
        }
      }
    }

    /// <summary>
    /// Drives the motor.
    /// </summary>
    /// <param name="direction">The direction.</param>
    /// <param name="numberOfSeconds">the number of seconds to move.</param>
    public void DriveMotor(DiSEqCDirection direction, byte numberOfSeconds)
    {
      Log.Log.Write("DiSEqC: drive motor {0} for {1} seconds", direction.ToString(), numberOfSeconds);
      byte[] cmd = new byte[4];
      cmd[0] = FramingByte;
      cmd[1] = Azimutal;
      if (direction == DiSEqCDirection.West)
      {
        cmd[2] = (byte)DiSEqCCommands.DriveWest;
      }
      else
      {
        cmd[2] = (byte)DiSEqCCommands.DriveEast;
      }
      cmd[3] = numberOfSeconds;
      int milliSecs = ((int)numberOfSeconds) * 1000;
      DateTime start = DateTime.Now;
      _controller.SendDiSEqCCommand(cmd);
      while (true)
      {
        TimeSpan ts = DateTime.Now - start;
        if (ts.TotalMilliseconds >= milliSecs) break;
        System.Threading.Thread.Sleep(10);
      }
      StopMotor();
    }

    /// <summary>
    /// Stores the position.
    /// </summary>
    public void StorePosition(byte position)
    {
      if (position <= 0) throw new ArgumentException("position");
      Log.Log.Write("DiSEqC: store current position in {0}",position);
      byte[] cmd = new byte[4];
      cmd[0] = FramingByte;
      cmd[1] = AllDirections;
      cmd[2] = (byte)DiSEqCCommands.StorePositions;
      cmd[3] = position;
      _controller.SendDiSEqCCommand(cmd);
    }

    /// <summary>
    /// Goto's the sattelite reference position.
    /// </summary>
    public void GotoReferencePosition()
    {
      Log.Log.Write("DiSEqC: goto reference position");
      byte[] cmd = new byte[4];
      cmd[0] = FramingByte;
      cmd[1] = AllDirections;
      cmd[2] = (byte)DiSEqCCommands.GotoPosition;
      cmd[3] = 0;
      _controller.SendDiSEqCCommand(cmd);
    }

    /// <summary>
    /// Goto's the position.
    /// </summary>
    public void GotoPosition(byte position)
    {
      if (position <= 0) throw new ArgumentException("position");
      Log.Log.Write("DiSEqC: goto position {0}", position);
      byte[] cmd = new byte[4];
      cmd[0] = FramingByte;
      cmd[1] = AllDirections;
      cmd[2] = (byte)DiSEqCCommands.GotoPosition;
      cmd[3] = position;
      _controller.SendDiSEqCCommand(cmd);
    }

    /// <summary>
    /// Gets the current motor position.
    /// </summary>
    public void GetPosition()
    {
      Log.Log.Write("DiSEqC: get current position (requires diSEqC 2.2)");
      byte[] cmd = new byte[3];
      cmd[0] = FramingByte;
      cmd[1] = AllDirections;
      cmd[2] = (byte)DiSEqCCommands.ReadPosition;
      if (_controller.SendDiSEqCCommand(cmd))
      {
        byte[] reply;
        if (_controller.ReadDiSEqCCommand(out reply))
        {
          Log.Log.Error("DiSEqC motor: status:");
          if ((reply[0] & (byte)DiSEqCPositionFlags.CommandCompleted) != 0)
            Log.Log.Error("  command completed");
          if ((reply[0] & (byte)DiSEqCPositionFlags.DirectionWest) != 0)
            Log.Log.Error("  Last movement = west");
          if ((reply[0] & (byte)DiSEqCPositionFlags.HardwareSwitchActivated) != 0)
            Log.Log.Error("  Hardware switch is activated");
          if ((reply[0] & (byte)DiSEqCPositionFlags.MotorRunning) != 0)
            Log.Log.Error("  Motor is running");
          if ((reply[0] & (byte)DiSEqCPositionFlags.PositionReferenceLost) != 0)
            Log.Log.Error("  Position reference lost or corrupted");
          if ((reply[0] & (byte)DiSEqCPositionFlags.PowerNotAvailable) != 0)
            Log.Log.Error("  Power not available");
          if ((reply[0] & (byte)DiSEqCPositionFlags.SoftwareLimitReached) != 0)
            Log.Log.Error("  Software Limit Reached");
          if ((reply[0] & (byte)DiSEqCPositionFlags.SoftwareLimitsEnabled) != 0)
            Log.Log.Error("  Software Limit Enabled");
        }
        else
        {
          Log.Log.Error("DiSEqC motor: Unable to read current position");
        }
      }
      else
      {
        Log.Log.Error("DiSEqC motor: unable to send cmd to get current position");
      }
    }
  }
}

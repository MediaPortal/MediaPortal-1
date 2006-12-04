using System;
using System.Collections.Generic;
using System.Text;

namespace TvLibrary.Interfaces
{
  #region enums
  /// <summary>
  /// DisEqC directions
  /// </summary>
  public enum DiSEqCDirection
  {
    /// <summary>
    /// move west
    /// </summary>
    West,

    /// <summary>
    /// move east
    /// </summary>
    East
  }
  #endregion
  /// <summary>
  /// interface for controlling DiSEqC motors
  /// </summary>
  public interface IDiSEqCMotor
  {
    /// <summary>
    /// Stops the motor.
    /// </summary>
    void StopMotor();
    /// <summary>
    /// Sets the east limit.
    /// </summary>
    void SetEastLimit();
    /// <summary>
    /// Sets the west limit.
    /// </summary>
    void SetWestLimit();
    /// <summary>
    /// Enable/disables the west/east limits.
    /// </summary>
    bool ForceLimits { set;}
    /// <summary>
    /// Drives the motor.
    /// </summary>
    /// <param name="direction">The direction.</param>
    /// <param name="numberOfSeconds">the number of seconds to move.</param>
    void DriveMotor(DiSEqCDirection direction, byte numberOfSeconds);
    /// <summary>
    /// Stores the position.
    /// </summary>
    void StorePosition(byte position);

    /// <summary>
    /// Goto's the sattelite reference position.
    /// </summary>
    void GotoReferencePosition();
    /// <summary>
    /// Goto's the position.
    /// </summary>
    void GotoPosition(byte position);
    /// <summary>
    /// Gets the current motor position.
    /// </summary>
    void GetPosition();
  }
}

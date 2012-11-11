using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces.Device;

namespace Mediaportal.TV.Server.TVService.Interfaces.CardHandler
{
  public interface IDisEqcManagement
  {
    /// <summary>
    /// returns the current diseqc motor position
    /// </summary>
    /// <param name="satellitePosition">The satellite position.</param>
    /// <param name="stepsAzimuth">The steps azimuth.</param>
    /// <param name="stepsElevation">The steps elevation.</param>
    void GetPosition(out int satellitePosition, out int stepsAzimuth, out int stepsElevation);

    /// <summary>
    /// resets the diseqc motor.
    /// </summary>
    void Reset();

    /// <summary>
    /// stops the diseqc motor
    /// </summary>
    void StopMotor();

    /// <summary>
    /// sets the east limit of the diseqc motor
    /// </summary>
    void SetEastLimit();

    /// <summary>
    /// sets the west limit of the diseqc motor
    /// </summary>
    void SetWestLimit();

    /// <summary>
    /// Enables or disables the use of the west/east limits
    /// </summary>
    /// <param name="onOff">if set to <c>true</c> [on off].</param>
    void EnableEastWestLimits(bool onOff);

    /// <summary>
    /// Drives the diseqc motor in the direction specified by the number of steps
    /// </summary>
    /// <param name="direction">The direction.</param>
    /// <param name="numberOfSteps">The number of steps.</param>
    void DriveMotor(DiseqcDirection direction, byte numberOfSteps);

    /// <summary>
    /// Stores the current diseqc motor position
    /// </summary>
    /// <param name="position">The position.</param>
    void StoreCurrentPosition(byte position);

    /// <summary>
    /// Drives the diseqc motor to the reference positition
    /// </summary>
    void GotoReferencePosition();

    /// <summary>
    /// Drives the diseqc motor to the specified position
    /// </summary>
    /// <param name="position">The position.</param>
    void GotoStoredPosition(byte position);
  }
}
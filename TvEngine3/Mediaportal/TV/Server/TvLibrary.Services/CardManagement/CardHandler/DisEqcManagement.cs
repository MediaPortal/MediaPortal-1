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

using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Device;


namespace TvService
{
  public class DisEqcManagement
  {
    private readonly ITvCardHandler _cardHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DisEqcManagement"/> class.
    /// </summary>
    /// <param name="cardHandler">The card handler.</param>
    public DisEqcManagement(ITvCardHandler cardHandler)
    {
      _cardHandler = cardHandler;
    }

    #region DiSEqC

    /// <summary>
    /// returns the current diseqc motor position
    /// </summary>
    /// <param name="satellitePosition">The satellite position.</param>
    /// <param name="stepsAzimuth">The steps azimuth.</param>
    /// <param name="stepsElevation">The steps elevation.</param>
    public void GetPosition(out int satellitePosition, out int stepsAzimuth, out int stepsElevation)
    {
      satellitePosition = -1;
      stepsAzimuth = 0;
      stepsElevation = 0;
      if (_cardHandler.IsLocal == false)
      {
        return;
      }

      IDiseqcController controller = _cardHandler.Card.DiseqcController;
      if (controller == null)
        return;
      controller.GetPosition(out satellitePosition, out stepsAzimuth, out stepsElevation);
    }

    /// <summary>
    /// resets the diseqc motor.
    /// </summary>
    public void Reset()
    {
      if (_cardHandler.IsLocal == false)
      {
        return;
      }

      IDiseqcController controller = _cardHandler.Card.DiseqcController;
      if (controller == null)
        return;
      controller.Reset();
    }

    /// <summary>
    /// stops the diseqc motor
    /// </summary>
    public void StopMotor()
    {
      if (_cardHandler.IsLocal == false)
      {
        return;
      }

      IDiseqcController controller = _cardHandler.Card.DiseqcController;
      if (controller == null)
        return;
      controller.Stop();
    }

    /// <summary>
    /// sets the east limit of the diseqc motor
    /// </summary>
    public void SetEastLimit()
    {
      if (_cardHandler.IsLocal == false)
      {
        return;
      }

      IDiseqcController controller = _cardHandler.Card.DiseqcController;
      if (controller == null)
        return;
      controller.SetEastLimit();
    }

    /// <summary>
    /// sets the west limit of the diseqc motor
    /// </summary>
    public void SetWestLimit()
    {
      if (_cardHandler.IsLocal == false)
      {
        return;
      }

      IDiseqcController controller = _cardHandler.Card.DiseqcController;
      if (controller == null)
        return;
      controller.SetWestLimit();
    }

    /// <summary>
    /// Enables or disables the use of the west/east limits
    /// </summary>
    /// <param name="onOff">if set to <c>true</c> [on off].</param>
    public void EnableEastWestLimits(bool onOff)
    {
      if (_cardHandler.IsLocal == false)
      {
        return;
      }

      IDiseqcController controller = _cardHandler.Card.DiseqcController;
      if (controller == null)
        return;
      controller.ForceLimits = onOff;
    }

    /// <summary>
    /// Drives the diseqc motor in the direction specified by the number of steps
    /// </summary>
    /// <param name="direction">The direction.</param>
    /// <param name="numberOfSteps">The number of steps.</param>
    public void DriveMotor(DiseqcDirection direction, byte numberOfSteps)
    {
      if (_cardHandler.IsLocal == false)
      {
        return;
      }

      IDiseqcController controller = _cardHandler.Card.DiseqcController;
      if (controller == null)
        return;
      controller.DriveMotor(direction, numberOfSteps);
    }

    /// <summary>
    /// Stores the current diseqc motor position
    /// </summary>
    /// <param name="position">The position.</param>
    public void StoreCurrentPosition(byte position)
    {
      if (_cardHandler.IsLocal == false)
      {
        return;
      }

      IDiseqcController controller = _cardHandler.Card.DiseqcController;
      if (controller == null)
        return;
      controller.StorePosition(position);
    }

    /// <summary>
    /// Drives the diseqc motor to the reference positition
    /// </summary>
    public void GotoReferencePosition()
    {
      if (_cardHandler.IsLocal == false)
      {
        return;
      }

      IDiseqcController controller = _cardHandler.Card.DiseqcController;
      if (controller == null)
        return;
      controller.GotoReferencePosition();
    }

    /// <summary>
    /// Drives the diseqc motor to the specified position
    /// </summary>
    /// <param name="position">The position.</param>
    public void GotoStoredPosition(byte position)
    {
      if (_cardHandler.IsLocal == false)
      {
        return;
      }

      IDiseqcController controller = _cardHandler.Card.DiseqcController;
      if (controller == null)
        return;
      controller.GotoPosition(position);
    }

    #endregion
  }
}
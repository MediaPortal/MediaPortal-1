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

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception
{
  /// <summary>
  /// Exception thrown by the TV library when physical tuning cannot be
  /// attempted because the tuning parameters for the specified satellite are
  /// not defined.
  /// </summary>
  [Serializable]
  public class TvExceptionSatelliteNotReceivable : TvException
  {
    /// <summary>
    /// Initialise a new instance of the <see cref="TvExceptionSatelliteNotReceivable"/> class.
    /// </summary>
    /// <param name="tunerId">The tuner's identifier.</param>
    /// <param name="longitude">The satellite's longitude.</param>
    public TvExceptionSatelliteNotReceivable(int tunerId, string longitude)
      : base("Failed to tune satellite at {0} with tuner {0}.", longitude, tunerId)
    {
    }
  }
}
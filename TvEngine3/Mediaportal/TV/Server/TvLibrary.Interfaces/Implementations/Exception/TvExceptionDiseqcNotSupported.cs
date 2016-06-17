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
  /// attempted because the tuner does not support DiSEqC.
  /// </summary>
  [Serializable]
  public class TvExceptionDiseqcNotSupported : TvException
  {
    /// <summary>
    /// Initialise a new instance of the <see cref="TvExceptionDiseqcNotSupported"/> class.
    /// </summary>
    /// <param name="tunerId">The tuner's identifier.</param>
    public TvExceptionDiseqcNotSupported(int tunerId)
      : base("Failed to tune with tuner {0}. DiSEqC is required but not supported.", tunerId)
    {
    }
  }
}
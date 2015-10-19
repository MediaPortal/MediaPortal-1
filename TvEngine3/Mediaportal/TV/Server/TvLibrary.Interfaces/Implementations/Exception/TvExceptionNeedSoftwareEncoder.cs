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
  /// Exception thrown by the TV library when it is not possible to load an
  /// analog tuner or capture device because compatible software encoders are
  /// not available.
  /// </summary>
  [Serializable]
  public class TvExceptionNeedSoftwareEncoder : TvException
  {
    /// <summary>
    /// Initialise a new instance of the <see cref="TvExceptionNeedSoftwareEncoder"/> class.
    /// </summary>
    /// <param name="needVideoEncoder"><c>True</c> if the system is missing a compatible video encoder; <c>false</c> if the system is missing a compatible audio encoder.</param>
    /// <param name="isCompatibilityIssue"><c>True</c> if the preferred encoder(s) were not able to be used; <c>false</c> if the system does not have any encoders installed.</param>
    public TvExceptionNeedSoftwareEncoder(bool needVideoEncoder, bool isCompatibilityIssue)
      : base("Software {0} encoder required. {1}", needVideoEncoder ? "video" : "audio", isCompatibilityIssue ? "Current preferred encoder(s) are not compatible with the tuner." : "There are no compatible encoders installed.")
    {
    }
  }
}
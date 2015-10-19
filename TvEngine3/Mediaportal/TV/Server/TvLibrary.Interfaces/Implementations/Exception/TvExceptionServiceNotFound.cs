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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception
{
  /// <summary>
  /// Exception thrown by the TV library when physical tuning succeeds but the
  /// service is not found.
  /// </summary>
  /// <remarks>
  /// This exception may be thrown when:
  /// 1. The service is not running.
  /// 2. The service has been moved to a different transmitter.
  /// 3. The tuner has somehow tuned to the wrong transmitter.
  /// </remarks>
  [Serializable]
  public class TvExceptionServiceNotFound : TvException
  {
    /// <summary>
    /// Initialise a new instance of the <see cref="TvExceptionServiceNotFound"/> class.
    /// </summary>
    /// <param name="channel">The tuning and service details for the service.</param>
    public TvExceptionServiceNotFound(IChannel service)
      : base("The service is not found. It may be not running or have moved to a different transmitter.{0}{1}", Environment.NewLine, service)
    {
    }
  }
}
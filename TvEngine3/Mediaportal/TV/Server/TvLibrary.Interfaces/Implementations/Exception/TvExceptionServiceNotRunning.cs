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
  /// Exception thrown by the TV library when asked to tune a service that is
  /// currently not broadcasting.
  /// </summary>
  [Serializable]
  public class TvExceptionServiceNotRunning : TvException
  {
    /// <summary>
    /// Initialise a new instance of the <see cref="TvExceptionServiceNotRunning"/> class.
    /// </summary>
    /// <param name="service">The tuning and service details for the service that is not running.</param>
    public TvExceptionServiceNotRunning(IChannel service)
      : base("The service is currently not broadcasting.{0}{1}", Environment.NewLine, service)
    {
    }
  }
}
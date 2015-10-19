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
  /// Exception thrown by the TV library when physical tuning fails.
  /// </summary>
  [Serializable]
  public class TvExceptionNoSignal : TvException
  {
    /// <summary>
    /// Initialise a new instance of the <see cref="TvExceptionNoSignal"/> class.
    /// </summary>
    /// <param name="tunerId">The tuner's identifier.</param>
    /// <param name="tuningDetail">The tuning details for the transmitter that could not be locked.</param>
    public TvExceptionNoSignal(int tunerId, IChannel tuningDetail)
      : base("Tuner {0} failed to lock on signal.{1}{2}", tunerId, Environment.NewLine, tuningDetail)
    {
    }

    /// <summary>
    /// Initialise a new instance of the <see cref="TvExceptionNoSignal"/> class.
    /// </summary>
    /// <param name="tunerId">The tuner's identifier.</param>
    /// <param name="tuningDetail">The tuning details for the transmitter that could not be locked.</param>
    /// <param name="context">Optional context.</param>
    /// <param name="contextArgs">Optional context arguments.</param>
    public TvExceptionNoSignal(int tunerId, IChannel tuningDetail, string context, params object[] contextArgs)
      : base("Tuner {0} failed to lock on signal. {1}{2}{3}", tunerId, string.Format(context, contextArgs), Environment.NewLine, tuningDetail)
    {
    }
  }
}
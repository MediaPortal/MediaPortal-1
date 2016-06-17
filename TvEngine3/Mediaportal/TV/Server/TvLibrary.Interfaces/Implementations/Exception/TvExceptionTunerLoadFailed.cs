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
  /// Exception thrown by the TV library when it fails to load a tuner.
  /// </summary>
  [Serializable]
  public class TvExceptionTunerLoadFailed : TvException
  {
    /// <summary>
    /// Initialise a new instance of the <see cref="TvExceptionTunerLoadFailed"/> class.
    /// </summary>
    /// <param name="tunerId">The tuner's identifier.</param>
    /// <param name="context">Optional context.</param>
    /// <param name="contextArgs">Optional context arguments.</param>
    public TvExceptionTunerLoadFailed(int tunerId, string context, params object[] contextArgs)
      : base("Failed to load tuner {0}. {1}", tunerId, string.Format(context, contextArgs))
    {
    }

    /// <summary>
    /// Initialise a new instance of the <see cref="TvExceptionTunerLoadFailed"/> class.
    /// </summary>
    /// <param name="tunerId">The tuner's identifier.</param>
    /// <param name="innerException">The inner exception, if any.</param>
    public TvExceptionTunerLoadFailed(int tunerId, System.Exception innerException)
      : base(innerException, "Failed to load tuner {0}.", tunerId)
    {
    }

    /// <summary>
    /// Initialise a new instance of the <see cref="TvExceptionTunerLoadFailed"/> class.
    /// </summary>
    /// <param name="tunerId">The tuner's identifier.</param>
    /// <param name="innerException">The inner exception, if any.</param>
    /// <param name="context">Optional context.</param>
    /// <param name="contextArgs">Optional context arguments.</param>
    public TvExceptionTunerLoadFailed(int tunerId, System.Exception innerException, string context, params object[] contextArgs)
      : base(innerException, "Failed to load tuner {0}. {1}", tunerId, string.Format(context, contextArgs))
    {
    }
  }
}
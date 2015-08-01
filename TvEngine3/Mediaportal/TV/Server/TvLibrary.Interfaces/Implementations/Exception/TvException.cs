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
  /// A generic exception thrown by the TV library when no other more specific
  /// exception is appropriate.
  /// </summary>
  [Serializable]
  public class TvException : System.Exception
  {
    /// <summary>
    /// Initialise a new instance of the <see cref="TvException"/> class.
    /// </summary>
    /// <param name="message">A contextual message.</param>
    /// <param name="args">The context message arguments.</param>
    public TvException(string message, params object[] args)
      : base(string.Format(message, args))
    {
    }

    /// <summary>
    /// Initialise a new instance of the <see cref="TvException"/> class.
    /// </summary>
    /// <param name="innerException">The inner exception, if any.</param>
    /// <param name="message">A contextual message.</param>
    /// <param name="args">The context message arguments.</param>
    public TvException(System.Exception innerException, string message, params object[] args)
      : base(string.Format(message), innerException)
    {
    }
  }
}
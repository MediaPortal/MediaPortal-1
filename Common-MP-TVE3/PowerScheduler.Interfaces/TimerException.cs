#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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

#region Usings

using System;

#endregion

namespace TvEngine.PowerScheduler
{
  /// <summary>
  /// Used by the <see cref="MediaPortal.TV.Recording.WaitableTimer"/>
  /// to report errors.
  /// </summary>
  public class TimerException : Exception
  {
    /// <summary>
    /// Create a new instance of this exception.
    /// </summary>
    /// <param name="sReason">Some text to describe the error condition.</param>
    public TimerException(string sReason) : base(sReason) {}

    /// <summary>
    /// Create a new instance of this exception.
    /// </summary>
    /// <param name="sReason">Some text to describe the error condition.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, 
    /// or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    public TimerException(string sReason, Exception innerException) : base(sReason, innerException) { }
  }
}
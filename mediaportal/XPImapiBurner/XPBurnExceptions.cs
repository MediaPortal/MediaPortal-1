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

using System;
using System.Runtime.Serialization;

namespace XPBurn
{
  /// <summary>
  /// This is the base exception type from which all burner exceptions are thrown.  No other burner 
  /// specific exceptions are currently thrown.
  /// </summary>
  [Serializable]
  public class XPBurnException : ApplicationException
  {
    /// <summary>
    /// Creates the burner exception.
    /// </summary>
    public XPBurnException() {}

    /// <summary>
    /// Creates the burner exception with a message.
    /// </summary>
    /// <param name="message">The message that the dialog box containing the exception will display.</param>
    public XPBurnException(string message) : base(message) {}

    /// <summary>
    /// Creates the burner exception with a message and an inner exception.
    /// </summary>
    /// <param name="message">The message that the dialog box contianing the excpetion will display.</param>
    /// <param name="inner">The inner exception to be stored.</param>
    public XPBurnException(string message, Exception inner) : base(message, inner) {}

    /// <summary>
    /// Creates the burner exception with serialization info.
    /// </summary>
    /// <param name="info">The serialization info to be passed.</param>
    /// <param name="context">The streaming context to be passed.</param>
    public XPBurnException(SerializationInfo info, StreamingContext context) : base(info, context) {}
  }
}
/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;

namespace TvLibrary
{
  /// <summary>
  /// Exception class for the tv library
  /// </summary>
  [Serializable]
  public class TvExceptionNoSignal : Exception
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="TvException"/> class.
    /// </summary>
    public TvExceptionNoSignal()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TvException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public TvExceptionNoSignal(string message)
      : base(message)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="TvException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public TvExceptionNoSignal(string message, Exception innerException)
      : base(message, innerException)
    {
    }
  }

  /// <summary>
  /// Exception class for the tv library
  /// </summary>
  [Serializable]
  public class TvExceptionGraphBuildingFailed : Exception
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="TvException"/> class.
    /// </summary>
    public TvExceptionGraphBuildingFailed()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TvException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public TvExceptionGraphBuildingFailed(string message)
      : base(message)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="TvException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public TvExceptionGraphBuildingFailed(string message, Exception innerException)
      : base(message, innerException)
    {
    }
  }


  /// <summary>
  /// Exception class for the tv library
  /// </summary>
  [Serializable]
  public class TvExceptionSWEncoderMissing : Exception
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="TvException"/> class.
    /// </summary>
    public TvExceptionSWEncoderMissing()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TvException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public TvExceptionSWEncoderMissing(string message)
      : base(message)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="TvException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public TvExceptionSWEncoderMissing(string message, Exception innerException)
      : base(message, innerException)
    {
    }
  }

  /// <summary>
  /// Exception class for the tv library
  /// </summary>
  [Serializable]
  public class TvException : Exception
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="TvException"/> class.
    /// </summary>
    public TvException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TvException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public TvException(string message)
      : base(message)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="TvException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public TvException(string message, Exception innerException)
      : base(message, innerException)
    {
    }
  }
}
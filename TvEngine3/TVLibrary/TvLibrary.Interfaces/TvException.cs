/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Collections.Generic;
using System.Text;
using TvLibrary.Interfaces;

namespace TvLibrary
{
  [Serializable]
  public class TvException : Exception
  {
    // Summary:
    //     Initializes a new instance of the System.Exception class.
    public TvException()
    {
    }

    //
    // Summary:
    //     Initializes a new instance of the System.Exception class with a specified
    //     error message.
    //
    // Parameters:
    //   message:
    //     The message that describes the error.
    public TvException(string message)
      : base(message)
    {
    }


    //
    // Summary:
    //     Initializes a new instance of the System.Exception class with a specified
    //     error message and a reference to the inner exception that is the cause of
    //     this exception.
    //
    // Parameters:
    //   message:
    //     The error message that explains the reason for the exception.
    //
    //   innerException:
    //     The exception that is the cause of the current exception, or a null reference
    //     (Nothing in Visual Basic) if no inner exception is specified.
    public TvException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

  }
}

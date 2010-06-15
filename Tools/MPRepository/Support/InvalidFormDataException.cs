#region Copyright (C) 2005-2009 Team MediaPortal

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

#endregion

using System;

namespace MPRepository.Web.Support
{
  /// <summary>
  /// This exception is raised if the web application is unable to find data expected in the form
  /// </summary>
  public class InvalidFormDataException : Exception
  {

    #region Constructors/Destructors

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message">Explain what invalid information was received</param>
    public InvalidFormDataException(string message)
      : base(message)
    {
    }

    #endregion

  }
}
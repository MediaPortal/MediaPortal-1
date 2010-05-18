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
using System.Collections.Generic;
using System.Text;
using MPRepository.Support;
using MPRepository.Items;

namespace MPRepository.Storage
{

  /// <summary>
  /// This class represents a file stored in the repository. It is used to keep track of 
  /// the location of the file and to keep the relation between the file and the 
  /// MPItemVersion it is used by.
  /// </summary>
  public class MPFile : IdentityFieldProvider<MPFile>
  {

    #region Constructors/Destructors

    public MPFile()
    {
    }

    #endregion

    #region Public Properties

    public virtual string Filename { get; set; }

    /// <summary>
    /// The full path name of the file on the filesystem.
    /// </summary>
    public virtual string Location { get; set; }

    public virtual MPItemVersion ItemVersion { get; set; }

    #endregion


  }
}

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
using System.Linq;
using System.Text;
using MPRepository.Users;
using MPRepository.Support;

namespace MPRepository.Items
{

  /// <summary>
  /// A comment for an item
  /// </summary>
  public class MPItemComment : IdentityFieldProvider<MPItemComment>, IComparable
  {

    #region Constructors/Destructors

    public MPItemComment()
    {
      Time = DateTime.Now;
    }

    #endregion

    #region Properties

    // Public Properties

    public virtual MPUser User { get; set; }

    public virtual DateTime Time { get; set; }

    public virtual string Text { get; set; }

    #endregion

    #region <Interface> Implementations

    public virtual int CompareTo(object obj)
    {
      MPItemComment other = (MPItemComment) obj;
      if (this.Time == other.Time)
      {
        return this.User.Id.CompareTo(other.User.Id);
      }
      else
      {
        return this.Time.CompareTo(other.Time);
      }
    }


    #endregion


  }
}

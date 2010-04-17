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
using Iesi.Collections.Generic;
using System.Text;
using MPRepository.Support;

namespace MPRepository.Users
{
  /// <summary>
  /// This class is for a user of the MP Repository.
  /// It relies on VBulletin for userid, name and login
  /// </summary>
  public class MPUser : IdentityFieldProvider<MPUser>
  {

    #region Constructors/Destructors

    public MPUser()
    {
      Permissions = new HashedSet<MPUserPermission>();
    }

    #endregion

    #region Properties

    public virtual string Handle { get; set; }

    public virtual string Name { get; set; }

    public virtual string EMail { get; set; }

    public virtual DateTime LastLogin { get; set; }

    public virtual ISet<MPUserPermission> Permissions { get; set; }

    #endregion

    #region Public Methods

    public virtual bool hasPermission(string permission)
    {
      foreach (MPUserPermission perm in Permissions)
      {
        if (String.Equals(perm.Name, permission, StringComparison.OrdinalIgnoreCase))
        {
          return true;
        }
      }
      return false;
    }

    public override string ToString()
    {
      return Handle;
    }

    #endregion

  }
}

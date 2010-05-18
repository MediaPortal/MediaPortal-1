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
using MPRepository.Storage;
using MPRepository.Users;

namespace MPRepository.Items
{
  /// <summary>
  /// This class describes a single version of an item in the repository
  /// </summary>
  public class MPItemVersion : IdentityFieldProvider<MPItemVersion>, IComparable
  {

    #region Enums

    public enum MPDevelopmentStatus
    {
      Alpha = 1,
      Beta,
      Stable,
      Obsolete
    }

    public enum MPAvailableStatus
    {
      Unapproved = 1,
      Available,
      Removed
    }

    #endregion

    #region Constructors/Destructors

    public MPItemVersion()
    {
      Files = new List<MPFile>();
      IsDeleted = false;
      AvailableStatus = MPAvailableStatus.Unapproved;
      UpdateDate = DateTime.Now;
    }

    #endregion

    #region Properties

    // Public Properties

    public virtual MPUser Uploader { get; set; }

    public virtual string Version { get; set; }

    public virtual DateTime UpdateDate { get; set; }

    public virtual MPDevelopmentStatus DevelopmentStatus { get; set; }

    public virtual string MPVersionMin { get; set; }

    public virtual string MPVersionMax { get; set; }

    public virtual string ReleaseNotes { get; set; }

    public virtual IList<MPFile> Files { get; set; }

    public virtual bool IsDeleted { get; set; }

    public virtual MPAvailableStatus AvailableStatus { get; set; }

    public virtual int Downloads { get; set; }

    public virtual MPItem Item { get; set; }

    #endregion

    #region <Interface> Implementations

    public virtual int CompareTo(object obj)
    {
      // TODO: We should probably store a decrypted, easy to compare version 
      // of the version number so we wouldn't have to run string analysis for
      // version number comparison.
      // Uses reverse comparison since we want the latest version first.
      return Utils.VersionCompare(((MPItemVersion)obj).Version, this.Version);
    }

    #endregion

  }
}

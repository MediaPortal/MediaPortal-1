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
using MPRepository.Users;

namespace MPRepository.Items
{
  /// <summary>
  /// This class is the basic item managed by the repository. The actual 
  /// contents are stored per version in MPItemVersion
  /// </summary>
  public class MPItem : IdentityFieldProvider<MPItem>, NamedComponent
  {

    #region Utility Classes

    /// <summary>
    /// This class handles the rating of an item.
    /// Currently, it uses a running average.
    /// </summary>
    public class MPItemRating
    {

      static private readonly int MaxRating = 5;

      int votes = 0;
      int votesTotal = 0;

      /// <summary>
      /// Rating is calculated using a simple average, by keeping track of the total 
      /// and recalculating the avg on display (as opposed to e.g. keeping a moving average)
      /// </summary>
      public float Rating
      {
        get { return (votesTotal / votes); }
      }

      public int Votes
      {
        get { return votes; }
      }

      public void AddRating(int rating)
      {
        if ((rating > MaxRating) || (rating < 0))
        {
          // Invalid rating. Report error?
          return;
        }
        this.votesTotal += rating;
        this.votes++;

      }

    }

    #endregion

    #region Constructors/Destructors

    public MPItem()
    {
      IsDeleted = false;
      Categories = new HashedSet<MPCategory>();
      Versions = new List<MPItemVersion>();
      Comments = new List<MPItemComment>();
      Tags = new HashedSet<MPTag>();
    }

    #endregion

    #region Properties

    // Public Properties 

    public virtual string Name { get; set; }

    public virtual MPItemType Type { get; set; }

    public virtual ISet<MPCategory> Categories { get; set; }

    public virtual string Description { get; set; }

    public virtual string DescriptionShort { get; set; }

    public virtual string License { get; set; }

    public virtual bool LicenseMustAccept { get; set; }

    public virtual string Author { get; set; }

    public virtual string Homepage { get; set; }

    public virtual IList<MPItemVersion> Versions { get; set; }

    public virtual IList<MPItemComment> Comments { get; set; }

    public virtual ISet<MPTag> Tags { get; set; }

    public virtual MPItemRating Rating { get; set; }

    public virtual bool IsDeleted { get; set; }

    #endregion

    #region Public Methods

    public virtual MPItemVersion AddVersion()
    {
      MPItemVersion newVersion = new MPItemVersion();
      Versions.Add(newVersion);
      return newVersion;
    }

    public virtual void AddComment(MPUser user, string text)
    {
      MPItemComment comment = new MPItemComment();
      comment.User = user;
      comment.Text = text;
      Comments.Add(comment);
    }

    public virtual void Delete()
    {
      IsDeleted = true;
      foreach (MPItemVersion version in Versions)
      {
        version.IsDeleted = true;
      }
    }

    #endregion


  }
}

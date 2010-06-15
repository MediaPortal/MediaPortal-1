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
using System.Data;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using MPRepository.Controller;

namespace MPRepository.Web.Support
{
  /// <summary>
  /// Used to hold the item query results for display
  /// </summary>
  public class DisplayItem : IDisplayObject
  {

    #region Variables

    // Protected Variables
    protected Int64 id;
    protected string name;
    protected string descriptionShort;
    protected decimal rating;
    protected decimal downloads;
    protected string author;
    protected DateTime lastUpdated;

    #endregion

    #region Properties

    public virtual Int64 Id
    {
      get { return id; }
      set { id = value; }
    }

    public virtual string Name
    {
      get { return name; }
      set { name = value; }
    }

    public virtual string DescriptionShort
    {
      get { return descriptionShort; }
      set { descriptionShort = value; }
    }

    public virtual decimal Rating
    {
      get { return rating; }
      set { rating = value; }
    }

    public virtual decimal Downloads
    {
      get { return downloads; }
      set { downloads = value; }
    }

    public virtual string Author
    {
      get { return author; }
      set { author = value; }
    }

    public virtual DateTime LastUpdated
    {
      get { return lastUpdated; }
      set { lastUpdated = value; }
    }

    #endregion

    #region <Interface> Implementations

    public virtual string QueryStringSelect
    {
      get
      {
        return
          "select MPItem.Id, MPItem.Name, MPItem.DescriptionShort, votesTotal/votes as Rating, " +
          "sum(Downloads) as Downloads, Author, max(UpdateDate) as LastUpdated " +
          "from MPItem " +
          "left outer join MPItemVersion on (MPItemVersion.MPItem = MPItem.Id) " +
          "group by MPItem.Id ";
        // TODO: Only show items with an available version

      }
    }

    #endregion


  }
}

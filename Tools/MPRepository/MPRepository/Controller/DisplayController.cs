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
using NHibernate;
using NHibernate.Transform;

namespace MPRepository.Controller
{

  /// <summary>
  /// This class provides data layer integration for the display layer, mostly just hiding hibernate. 
  /// </summary>
  public class DisplayController
  {

    #region Enums

    public enum SortDirection
    {
      Ascending,
      Descending
    }

    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">The type of the objects to be retrieved. Must implement DisplayObject</typeparam>
    /// <param name="orderField"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    public static IList<T> RetrieveAll<T>(MPRSession session, string orderField, SortDirection direction)
      where T : IDisplayObject, new()
    {

      // Get the query string of type T.
      // TODO: I would be really happy to find a way to do that without an instance
      T displayObject = new T(); 
      string sql = ((IDisplayObject)displayObject).QueryStringSelect;

      if (orderField != null)
      {
        sql += " order by " + orderField + (direction == SortDirection.Ascending ? " asc " : " desc ");
      }
      IList<T> items = session.Session.CreateSQLQuery(sql)
        .SetResultTransformer(Transformers.AliasToBean(typeof(T)))
        .List<T>();

      return items;

    }

    public static IList<T> RetrieveAll<T>(MPRSession session)
      where T : IDisplayObject, new()
    {
      return RetrieveAll<T>(session, null, SortDirection.Ascending);
    }



  }

}

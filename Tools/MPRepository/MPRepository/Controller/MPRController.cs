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
using NHibernate;
using NHibernate.Criterion;
using MPRepository.Items;
using MPRepository.Storage;
using MPRepository.Users;
using MPRepository.Support;

namespace MPRepository.Controller
{
  /// <summary>
  /// This class handles the access from the View layer to the different managed entities
  /// </summary>
  public class MPRController
  {

    #region Enums

    public enum Direction
    {
      Ascending,
      Descending
    }

    #endregion

    public static MPRSession StartSession()
    {
      MPRSession session = new MPRSession();
      session.Session = DatabaseHelper.GetCurrentSession();
      session.Transaction = session.Session.BeginTransaction();
      return session;
    }

    public static void EndSession(MPRSession session, bool commit)
    {
      if ((session != null) && (session.Transaction != null))
      {
        if (commit)
        {
          session.Transaction.Commit();
        }
        else
        {
          session.Transaction.Rollback();
        }
        session.Transaction = null;
      }
    }

    /// <summary>
    /// Deletes an object of a specified type.
    /// </summary>
    /// <param name="session">The currently active persistence session.</param>
    /// <param name="itemsToDelete">The items to delete.</param>
    /// <typeparam name="T">The type of objects to delete.</typeparam>
    public static void Delete<T>(MPRSession session, T item)
    {
      session.Session.Delete(item);
    }

    /// <summary>
    /// Deletes objects of a specified type.
    /// </summary>
    /// <param name="session">The currently active persistence session.</param>
    /// <param name="itemsToDelete">The items to delete.</param>
    /// <typeparam name="T">The type of objects to delete.</typeparam>
    public static void Delete<T>(MPRSession session, IList<T> itemsToDelete)
    {
      foreach (T item in itemsToDelete)
      {
        session.Session.Delete(item);
      }
    }

    /// <summary>
    /// Retrieves all objects of a given type.
    /// </summary>
    /// <param name="session">The currently active persistence session.</param>
    /// <typeparam name="T">The type of the objects to be retrieved.</typeparam>
    /// <returns>A list of all objects of the specified type.</returns>
    public static IList<T> RetrieveAll<T>(MPRSession session)
    {
      // Retrieve all objects of the type passed in
      return session.Session.CreateCriteria(typeof(T))
        .List<T>();
    }

    /// <summary>
    /// Retrieves objects of a specified type where a specified property equals a specified value.
    /// </summary>
    /// <typeparam name="T">The type of the objects to be retrieved.</typeparam>
    /// <param name="session">The currently active persistence session.</param>
    /// <param name="propertyName">The name of the property to be tested.</param>
    /// <param name="propertyValue">The value that the named property must hold.</param>
    /// <returns>A list of all objects meeting the specified criteria.</returns>
    public static IList<T> RetrieveEquals<T>(MPRSession session, string propertyName, object propertyValue)
    {
      return session.Session.CreateCriteria(typeof(T))
        .Add(Expression.Eq(propertyName, propertyValue))
        .List<T>();
    }

    /// <summary>
    /// Retrieves objects of a specified type by an foreign-key column
    /// </summary>
    /// <typeparam name="T">The type of the objects to be retrieved.</typeparam>
    /// <param name="session">The currently active persistence session.</param>
    /// <param name="key">The name of the column to be tested.</param>
    /// <param name="value">The value that the named property must hold.</param>
    /// <returns>A list of all objects meeting the specified criteria.</returns>
    public static IList<T> RetrieveByForeignKey<T>(MPRSession session, string key, Int64 value)
    {
      return session.Session.CreateCriteria(typeof(T))
        .Add(Expression.Eq(key + ".Id", value))
        .List<T>();
    }


    /// <summary>
    /// Retrieves objects of a specified type by an foreign-key column, with ordering specified by another column
    /// </summary>
    /// <typeparam name="T">The type of the objects to be retrieved.</typeparam>
    /// <param name="session">The currently active persistence session.</param>
    /// <param name="key">The name of the key property/field.</param>
    /// <param name="value">The value that the named property must hold.</param>
    /// <param name="sortKey">The property/field by which to sort</param>
    /// <param name="dir">The direction of the sorting</param>
    /// <returns>A list of all objects meeting the specified criteria.</returns>
    public static IList<T> RetrieveByForeignKey<T>(MPRSession session, string key, Int64 value, string sortKey, Direction dir)
    {
      return session.Session.CreateCriteria(typeof(T))
        .Add(Expression.Eq(key + ".Id", value))
        .AddOrder(dir == Direction.Ascending ? Order.Asc(sortKey) : Order.Desc(sortKey))
        .List<T>();
    }

    /// <summary>
    /// Retrieves object of a specified type by its Id field
    /// </summary>
    /// <typeparam name="T">The type of the objects to be retrieved.</typeparam>
    /// <param name="session">The currently active persistence session.</param>
    /// <param name="id">The id of the object.</param>
    /// <returns>The object or null if not found</returns>
    public static T RetrieveById<T>(MPRSession session, Int64 id)
    {
      return session.Session.Get<T>(id);
    }

    public static IList<T> RetrieveByIdList<T>(MPRSession session, List<Int64> ids)
    {
      return session.Session.CreateCriteria(typeof(T))
        .Add(Expression.In("Id", ids))
        .List<T>();        
    }

    /// <summary>
    /// Saves an object and its persistent children.
    /// </summary>
    public static void Save<T>(MPRSession session, T item)
    {
      session.Session.SaveOrUpdate(item);
    }

    public static void Save<T>(MPRSession session, T item, out Int64 newId)
    {
      newId = (Int64)session.Session.Save(item);
    }


    public static ISet<MPTag> GetTags(MPRSession session, string tagNameList)
    {
      string[] tagNames = tagNameList.Split(',');
      IList<MPTag> tags = session.Session.CreateCriteria(typeof(MPTag))
        .Add(Expression.In("Name", tagNames))
        .List<MPTag>();
      Set<string> tagNamesExisting = new HashedSet<string>();
      foreach (MPTag tag in tags)
      {
        tagNamesExisting.Add(tag.Name);
      }
      ISet<MPTag> results = new HashedSet<MPTag>(tags);
      foreach (string tagName in (new HashedSet<string>(tagNames)).Minus(tagNamesExisting))
      {
        MPTag tag = new MPTag { Name = tagName };
        session.Session.Save(tag);
        results.Add(tag);
      }
      return results;
    }

  }
}

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
using System.Configuration;
using System.Collections.Generic;
using MPRepository.Controller;
using MPRepository.Items;

namespace MPRepository.Web.Support
{
  public class DataSourceWrapper<T>
  {

    public IList<T> GetAll()
    {
      MPRSession session = MPRController.StartSession();
      IList<T> items = MPRController.RetrieveAll<T>(session);
      MPRController.EndSession(session, true);
      return items;
    }

    public IList<T> GetById(Int64 id)
    {
      MPRSession session = MPRController.StartSession();
      List<T> items = new List<T>();
      items.Add(MPRController.RetrieveById<T>(session, id));
      MPRController.EndSession(session, true);
      return items;
    }

    public IList<T> GetByForeignKey(string key, Int64 value)
    {
      MPRSession session = MPRController.StartSession();
      IList<T> items = MPRController.RetrieveByForeignKey<T>(session, key, value);      
      MPRController.EndSession(session, true);
      return items;
    }

    public IList<T> GetByForeignKey(string key, Int64 value, string sortKey, MPRController.Direction direction)
    {
      MPRSession session = MPRController.StartSession();
      IList<T> items = MPRController.RetrieveByForeignKey<T>(session, key, value, sortKey, direction);
      MPRController.EndSession(session, true);
      return items;
    }


    public Int64 Insert(T item)
    {
      MPRSession session = MPRController.StartSession();
      Int64 newId;
      MPRController.Save<T>(session, item, out newId);     
      MPRController.EndSession(session, true);
      return newId;
    }

    public void Update(T item)
    {
      MPRSession session = MPRController.StartSession();
      MPRController.Save<T>(session, item);
      MPRController.EndSession(session, true);
    }

    public void Delete(T item)
    {
      // TODO: actually delete
    }

  }


}

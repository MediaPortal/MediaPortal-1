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
using NUnit.Framework;
using MPRepository.Controller;
using MPRepository.Web.Support;
using NHibernate;
using NHibernate.Transform;

namespace MPRepository.Tests
{
  [TestFixture]
  public class DisplayTests
  {

    [Test]
    public void TestDisplayItemFake()
    {
      MPRSession session = MPRController.StartSession();
      IQuery query = session.Session.CreateSQLQuery(
        "select 'myname' as Name, 'my description' as DescriptionShort, 3.5 as Rating, 13500 as Downloads, str_to_date('4/5/2009','%d/%m/%Y') as LastUpdate"
        )
        .SetResultTransformer(Transformers.AliasToBean(typeof(DisplayItem)));
      IList<DisplayItem> items = query.List<DisplayItem>();


      foreach (DisplayItem item in items)
      {
        System.Console.Write("{0} : {1} : {2} : ", item.Name, item.DescriptionShort, item.Rating);
        System.Console.WriteLine("{0} : {1}", item.Downloads, item.LastUpdated);
      }
       
    }

    [Test]
    public void TestDisplayItemsAll()
    {
      MPRSession session = MPRController.StartSession();

      IList<DisplayItem> items = DisplayController.RetrieveAll<DisplayItem>(session, "LastUpdated", DisplayController.SortDirection.Descending);

      foreach (DisplayItem item in items)
      {
        System.Console.Write("{0} : {1} : {2} : ", item.Name, item.DescriptionShort, item.Rating);
        System.Console.WriteLine("{0} : {1}", item.Downloads, item.LastUpdated);
      }

    }





  }
}

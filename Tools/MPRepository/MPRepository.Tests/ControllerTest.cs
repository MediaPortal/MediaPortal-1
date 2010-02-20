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
using MPRepository.Items;
using MPRepository.Storage;
using MPRepository.Controller;
using MPRepository.Users;

using NHibernate.Criterion;

namespace MPRepository.Tests
{
  [TestFixture]
  public class ControllerTest
  {

    [Test]
    public void TestControllerTags()
    {

      MPRSession session = MPRController.StartSession();
      IList<MPTag> tags = MPRController.RetrieveAll<MPTag>(session);

      foreach (MPTag tag in tags)
      {
        System.Console.WriteLine("{0} : {1}", tag.Name, tag.Description);
      }

      MPRController.EndSession(session, true);

    }

    [Test]
    public void TestForeignKey()
    {
      MPRSession session = MPRController.StartSession();
      IList<MPItemVersion> versions = 
        MPRController.RetrieveByForeignKey<MPItemVersion>(session, "Item", 11);

      foreach (MPItemVersion version in versions)
      {
        System.Console.WriteLine("{0} : {1}", version.Version, version.UpdateDate);
      }

      MPRController.EndSession(session, true);
    }


  }
}

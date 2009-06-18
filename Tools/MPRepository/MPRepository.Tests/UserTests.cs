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
using MPRepository.Users;
using MPRepository.Controller;

namespace MPRepository.Tests
{
  [TestFixture]
  public class UserTests
  {
    [Test]
    public void TestPermissionAdd()
    {
      string[,] permissions = 
      {
        { "Download", "Can download files" },
        { "Add", "Can upload files" },
        { "Approve", "Allowed to approve awaiting items in the queue" },
        { "Delete", "Allowed to delete items" },
      };

      MPRSession session = MPRController.StartSession();
      for (int i = 0; i <= permissions.GetUpperBound(0); i++)
      {   
        MPUserPermission permission = new MPUserPermission();
        permission.Name = permissions[i,0];
        permission.Description = permissions[i,1];
        
        MPRController.Save<MPUserPermission>(session, permission);
      }
      MPRController.EndSession(session, true);
    }


    [Test]
    public void TestUserAdd()
    {

      MPRSession session = MPRController.StartSession();

      MPUser user = new MPUser();

      user.Name = "joe submitter";
      user.EMail = "mp-test@localhost";
      user.Handle = "submitter";

      IList<MPUserPermission> permissions = session.Session
        .CreateQuery("from MPUserPermission perms where perms.Name in ('Download', 'Add')")
        .List<MPUserPermission>();
      user.Permissions.AddAll(permissions);

      MPRController.Save<MPUser>(session, user);

      user = new MPUser();
      
      user.Name = "james admin";
      user.EMail = "mp-test@localhost";
      user.Handle = "admin";

      permissions = session.Session
        .CreateQuery("from MPUserPermission perms where perms.Name in ('Download', 'Add', 'Approve', 'Delete')")
        .List<MPUserPermission>();
      user.Permissions.AddAll(permissions);

      MPRController.Save<MPUser>(session, user);
      
      
      MPRController.EndSession(session, true);

    }


    [Test]
    public void TestGetUserByHandle()
    {
      string handle = "admin";

      MPRSession session = MPRController.StartSession();
     
      IList<MPUser> users = MPRController.RetrieveEquals<MPUser>(session, "Handle", handle);

      Assert.That(users.Count, Is.EqualTo(1));

      MPUser user = users[0];
      System.Console.WriteLine("User {0}:\n{1}\n{2}\n{3} permissions", user.Handle, user.Name, user.EMail, user.Permissions.Count);

      Assert.That(user.Permissions.Count, Is.EqualTo(4));

      MPRController.EndSession(session, true);

    }


  }
}

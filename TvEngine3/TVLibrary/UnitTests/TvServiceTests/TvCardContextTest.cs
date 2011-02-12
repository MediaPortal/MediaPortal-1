using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using TvControl;
using TvLibrary.Interfaces;
using TypeMock.ArrangeActAssert;
using TvService;

namespace TVServiceTests
{
  [TestFixture]
  [Isolated]
  public class TvCardContextTest
  {


    [Test]
    public void AddNewUserTest()
    {
      ITvCardContext ctx = new TvCardContext();

      IUser u1 = new User("u1", false, 1);
      ctx.Add(u1);

      Assert.IsTrue(UserExists(ctx, u1), "user is not found");
    }

    [Test]
    public void AddUserOwnerTest()
    {
      ITvCardContext ctx = new TvCardContext();

      IUser u1 = new User("u1", false, 1);
      ctx.Add(u1);

      Assert.IsTrue(ctx.IsOwner(u1), "user is not owner");
    }

    [Test]
    public void AddAnotherUserNotOwnerTest()
    {
      ITvCardContext ctx = new TvCardContext();

      IUser u1 = new User("u1", false, 1);
      ctx.Add(u1);

      User u2 = new User("u2", false, 2);
      ctx.Add(u2);

      Assert.IsTrue(ctx.IsOwner(u1), "user is owner");
      Assert.IsFalse(ctx.IsOwner(u2), "user is not owner");
    }

    [Test]
    public void AddAlreadyExistingUserTest()
    {
      ITvCardContext ctx = new TvCardContext();

      IUser u1 = new User("u1", false, 1);
      ctx.Add(u1);

      User u2 = new User("u1", false, 2);
      ctx.Add(u2);

      Assert.AreEqual(1, ctx.Users.Length, "user count wrong");
      Assert.IsTrue(UserExists(ctx, u2), "user is not found");
    }

    [Test]
    public void RemoveUserTest()
    {
      ITvCardContext ctx = new TvCardContext();

      IUser u1 = new User("u1", false, 1);
      ctx.Add(u1);      
      ctx.Remove(u1);
      Assert.AreEqual(0, ctx.Users.Length, "user count wrong");
      Assert.IsFalse(UserExists(ctx, u1), "user is found");

      Assert.IsTrue(((TvCardContext)ctx).UsersOld.Contains(u1), "user not found in history");      
    }

    [Test]
    public void RemoveUserChangeOwnerShipToSchedulerTest()
    {
      ITvCardContext ctx = new TvCardContext();

      IUser u1 = new User("u1", false, 1);
      IUser u2 = new User("u2", false, 1);
      IUser scheduler = new User("scheduler", true, 1);
      ctx.Add(u1);
      ctx.Add(u2);
      ctx.Add(scheduler);
      ctx.Lock(scheduler); //set ownership

      ctx.Remove(u2);

      Assert.IsTrue(ctx.IsOwner(scheduler), "scheduler user is not owner");      
    }

    [Test]
    public void RemoveUserChangeOwnerShipToUserTest()
    {
      ITvCardContext ctx = new TvCardContext();

      IUser u1 = new User("u1", false, 1);
      IUser u2 = new User("u2", false, 1);
      IUser u3 = new User("u3", false, 1);      
      ctx.Add(u1);
      ctx.Add(u2);
      ctx.Add(u3);

      ctx.Lock(u3); //set ownership      
      ctx.Remove(u3);

      Assert.IsTrue(ctx.IsOwner(u1), "user1 user is not owner");
    }

    [Test]
    public void RemoveSchedulerChangeOwnerShipToUserTest()
    {
      ITvCardContext ctx = new TvCardContext();

      IUser scheduler = new User("scheduler", true, 1);
      IUser u1 = new User("u1", false, 1);
      IUser u2 = new User("u2", false, 1);

      ctx.Add(scheduler);
      ctx.Lock(scheduler); //set ownership      
      ctx.Add(u1);
      ctx.Add(u2);
      
      ctx.Remove(scheduler);

      Assert.IsTrue(ctx.IsOwner(u1), "user1 is not owner");
    }

    [Test]
    public void RemoveAdminUserNotInHistoryTest()
    {
      ITvCardContext ctx = new TvCardContext();

      IUser u1 = new User("u1", true, 1);
      ctx.Add(u1);
      ctx.Remove(u1);

      Assert.IsFalse(((TvCardContext)ctx).UsersOld.Contains(u1), "user found in history");      
    }


    [Test]
    public void RemoveNonExistingUserTest()
    {
      ITvCardContext ctx = new TvCardContext();

      IUser u1 = new User("u1", false, 1);
      User u2 = new User("u2", false, 2);
      ctx.Add(u1);
      ctx.Remove(u2);

      Assert.IsFalse(((TvCardContext)ctx).UsersOld.Contains(u1), "user found in history");
      Assert.IsFalse(((TvCardContext)ctx).UsersOld.Contains(u2), "user found in history");
    }

    [Test]
    public void RemoveOneOnlyUserNoOwnerTest()
    {
      ITvCardContext ctx = new TvCardContext();

      IUser u1 = new User("u1", false, 1);
      ctx.Add(u1);
      ctx.Remove(u1);
      Assert.IsTrue(ctx.IsOwner(u1), "user is owner");
    }

    [Test]
    public void RemoveOneOfManyUserNotOwnerTest()
    {
      ITvCardContext ctx = new TvCardContext();

      IUser u1 = new User("u1", false, 1);
      ctx.Add(u1);

      User u2 = new User("u2", false, 2);
      ctx.Add(u2);

      ctx.Remove(u1);
      Assert.IsTrue(ctx.IsOwner(u2), "user is not owner");
    }


    [Test]
    public void HeartBeatUserTest()
    {
      ITvCardContext ctx = new TvCardContext();

      IUser u1 = new User("u1", false, 1);
      DateTime oldHB = u1.HeartBeat;
      ctx.Add(u1);      

      ctx.HeartBeatUser(u1);

      Assert.AreNotEqual(u1.HeartBeat, oldHB, "heartbeat hasnt changed");
    }

    [Test]
    public void GetExistingUserTest()
    {
      ITvCardContext ctx = new TvCardContext();

      IUser u1 = new User("u1", false, 1);      
      ctx.Add(u1);

      IUser getUser = new User("u1", false, 1);
      ctx.GetUser(ref getUser);

      Assert.AreEqual(u1.Name, getUser.Name, "user name not equal");
      Assert.AreEqual(u1.CardId, getUser.CardId, "user cardid not equal");
    }

    [Test]
    public void GetNonExistingUserTest()
    {
      ITvCardContext ctx = new TvCardContext();

      IUser u1 = new User("u1", false, 1);
      ctx.Add(u1);

      IUser getUser = new User("u2", false, 2);
      ctx.GetUser(ref getUser);

      Assert.AreNotEqual(u1.Name, getUser.Name, "user name equal");
      Assert.AreNotEqual(u1.CardId, getUser.CardId,"user cardid equal");
    }

    [Test]
    public void GetExistingUserWithCardIDTest()
    {
      ITvCardContext ctx = new TvCardContext();

      IUser u1 = new User("u1", false, 1);
      ctx.Add(u1);

      IUser getUser = new User("u1", false, 2);
      ctx.GetUser(ref getUser, 1);

      Assert.AreEqual(u1.Name, getUser.Name, "user name not equal");
      Assert.AreEqual(u1.CardId, getUser.CardId, "user cardid not equal");
    }

    [Test]
    public void GetNonExistingUserWithCardIDTest()
    {
      ITvCardContext ctx = new TvCardContext();

      IUser u1 = new User("u1", false, 1);
      ctx.Add(u1);

      IUser getUser = new User("u2", false, 1);
      ctx.GetUser(ref getUser, 2);

      Assert.AreNotEqual(u1.Name, getUser.Name, "user name equal");
      Assert.AreNotEqual(2, getUser.CardId, "user cardid equal");
    }

    [Test]
    public void GetExistingUserExistsTest()
    {
      ITvCardContext ctx = new TvCardContext();

      IUser u1 = new User("u1", false, 1);
      ctx.Add(u1);

      IUser getUser = new User("u1", false, 1);
      bool exists = false;
      ctx.GetUser(ref getUser, out exists);

      Assert.IsTrue(exists, "user does not exists");
    }

    [Test]
    public void GetNonExistingUserExistsTest()
    {
      ITvCardContext ctx = new TvCardContext();

      IUser u1 = new User("u1", false, 1);
      ctx.Add(u1);

      IUser getUser = new User("u2", false, 2);      
      bool exists = false;
      ctx.GetUser(ref getUser, out exists);

      Assert.IsFalse(exists, "user exists");
    }

    public void ExistingUserDoesExistsTest()
    {
      ITvCardContext ctx = new TvCardContext();

      IUser u1 = new User("u1", false, 1);
      ctx.Add(u1);            

      Assert.IsTrue(ctx.DoesExists(u1), "user does not exists");
    }

    [Test]
    public void NonExistingUserDoesExistsTest()
    {
      ITvCardContext ctx = new TvCardContext();

      IUser u1 = new User("u1", false, 1);
      ctx.Add(u1);

      IUser getUser = new User("u2", false, 2);

      Assert.IsFalse(ctx.DoesExists(getUser), "user exists");
    }


    [Test]
    public void ExistingUserGetUserTest()
    {
      ITvCardContext ctx = new TvCardContext();

      IUser u1 = new User("u1", false, 1);
      u1.SubChannel = 2;
      ctx.Add(u1);

      IUser getUser;
      ctx.GetUser(2, out getUser);

      Assert.NotNull(getUser, "user is null");
      Assert.AreEqual("u1", getUser.Name);
      Assert.AreEqual(2, getUser.SubChannel);
      Assert.AreEqual(1, getUser.CardId);
    }

    [Test]
    public void NonExistingUserGetUserTest()
    {
      ITvCardContext ctx = new TvCardContext();

      IUser u1 = new User("u1", false, 1);
      u1.SubChannel = 2;
      ctx.Add(u1);

      IUser getUser;
      ctx.GetUser(1, out getUser);

      Assert.IsNull(getUser, "user is not null");      
    }

    [Test]
    public void ExistingUserSetTimeshiftStoppedReasonTest()
    {
      ITvCardContext ctx = new TvCardContext();

      IUser u1 = new User("u1", false, 1);      
      ctx.Add(u1);      

      User u2 = new User("u1", false, 1);
      ctx.SetTimeshiftStoppedReason(u2,TvStoppedReason.KickedByAdmin);
      ctx.Remove(u1);
      Assert.AreEqual(TvStoppedReason.KickedByAdmin, ctx.GetTimeshiftStoppedReason(u2), "tvstoppedreason not the same");            
    }


    [Test]
    public void NonExistingUserSetTimeshiftStoppedReasonTest()
    {
      ITvCardContext ctx = new TvCardContext();

      IUser u1 = new User("u1", false, 1);
      u1.SubChannel = 2;
      ctx.Add(u1);      

      User u2 = new User("u2", false, 2);
      ctx.SetTimeshiftStoppedReason(u2, TvStoppedReason.KickedByAdmin);
      ctx.Remove(u1);
      Assert.AreEqual(TvStoppedReason.UnknownReason, ctx.GetTimeshiftStoppedReason(u1), "tvstoppedreason the same");            
    }



    [Test]
    public void ExistingUserContainsUsersForSubchannelTest()
    {
      ITvCardContext ctx = new TvCardContext();

      IUser u1 = new User("u1", false, 1);
      u1.SubChannel = 2;
      ctx.Add(u1);
      
      Assert.IsTrue(ctx.ContainsUsersForSubchannel(2), "user with subchannel not found");
    }

    [Test]
    public void NonExistingUserContainsUsersForSubchannelTest()
    {
      ITvCardContext ctx = new TvCardContext();

      IUser u1 = new User("u1", false, 1);
      u1.SubChannel = 2;
      ctx.Add(u1);

      Assert.IsFalse(ctx.ContainsUsersForSubchannel(1), "user with subchannel found");
    }

    [Test]
    public void ClearTest()
    {
      ITvCardContext ctx = new TvCardContext();

      for (int i = 0; i < 10; i++)
      {
        IUser u1 = new User("u" + i, false, 1);
        ctx.Add(u1);
      }
      ctx.Clear();
      Assert.AreEqual(10, ((TvCardContext)ctx).UsersOld.Count, "oldusers count wrong");
      Assert.AreEqual(0, ctx.Users.Length, "users count wrong");
    }       

    private bool UserExists (ITvCardContext ctx, IUser user)
    {
      foreach (User u in ctx.Users)
      {
        if (
            u.CardId == user.CardId && 
            u.IdChannel == user.IdChannel && 
            u.IsAdmin == user.IsAdmin && 
            u.Name == user.Name && 
            u.SubChannel == user.SubChannel)
        {
          return true;
        }
      }
      return false;
    }
  }
}

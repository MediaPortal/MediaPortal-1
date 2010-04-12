/*
 * Test cases
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: TestUser.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Collections;
using Gentle.Common;
using NUnit.Framework;

namespace Gentle.Framework.Tests
{
	[TestFixture]
	public class TestUser
	{
		private User u1, u2;
		private Role[] fixedRoles;

		[SetUp]
		public void Init()
		{
			// disable query caching
			GentleSettings.SkipQueryExecution = false;
			// clean any existing entries
			SqlBuilder sb = new SqlBuilder( StatementType.Delete, typeof(UserRole) );
			Broker.Execute( sb.GetStatement() );
			// clean out roles table 
			sb = new SqlBuilder( StatementType.Delete, typeof(Role) );
			Broker.Execute( sb.GetStatement() );
			// populate table with known roles
			fixedRoles = new Role[Enum.GetNames( typeof(Roles) ).Length];
			int index = 0;
			foreach( string roleName in Enum.GetNames( typeof(Roles) ) )
			{
				Role role = new Role( (Roles) Enum.Parse( typeof(Roles), roleName, false ) );
				role.Persist();
				fixedRoles[ index++ ] = role;
			}
		}

		[TearDown]
		public void Exit()
		{
			GentleSettings.CacheObjects = false;
			GentleSettings.CacheStatements = false;
			GentleSettings.SkipQueryExecution = false;
			// wipe tables
			Broker.Execute( "delete from UserRoles" );
			Broker.Execute( "delete from Roles" );
			Broker.Execute( "delete from Users" );
		}

		/// <summary>
		/// Test case for verifying basic type IO using Gentle-generated SQL. 
		/// </summary>
		[Test]
		public void TestCRUD()
		{
			u1 = new User( "Ford", "Prefect", Roles.Employee );
			// insert
			u1.Persist();
			Assert.IsTrue( u1.Id != 0, "No id generated for the User just inserted!" );
			// select
			u2 = new User( u1.Id );
			// verify select/insert
			Assert.IsTrue( u2.Id != 0, "The object could not be retrieved from the database!" );
			Assert.AreEqual( u1.Id, u2.Id, "The object could not be retrieved from the database!" );
			Assert.AreEqual( "Ford", u2.FirstName, "The object was not properly retrieved on construction!" );
			// update
			u2.FirstName = "Arthur";
			u2.LastName = "Dent";
			u2.PrimaryRole = Roles.Customer;
			u2.Persist();
			// verify update
			u1 = new User( u2.Id );
			Assert.AreEqual( u2.FirstName, u1.FirstName, "FirstName not updated!" );
			Assert.AreEqual( u2.LastName, u1.LastName, "LastName not updated!" );
			Assert.AreEqual( u2.PrimaryRole, u1.PrimaryRole, "PrimaryRole not updated!" );
			// delete
			u2.Remove();
			// verify delete by selecting the now non-existing row
			try
			{
				u2 = new User( u1.Id );
				Assert.IsNull( u2, "Object not removed" );
			}
			catch( GentleException fe )
			{
				Assert.AreEqual( Error.UnexpectedRowCount, fe.Error, "Unexpected error occurred!" );
			}
		}

		[Test]
		public void Test_GentleList()
		{
			Role r1 = fixedRoles[ 1 ];
			Role r2 = fixedRoles[ 3 ];
			// create a user with a known id
			u1 = new User( "John", "Doe", Roles.Customer );
			u1.Persist();
			// create 1:n type list
			GentleList list = new GentleList( typeof(UserRole), u1 );
			Assert.AreEqual( 0, list.Count, "Relation table not empty." );
			// verify add
			list.Add( new UserRole( u1.Id, r1.Id ) );
			Assert.AreEqual( 1, list.Count, "No element in relation table." );
			list.Add( new UserRole( u1.Id, r2.Id ) );
			Assert.AreEqual( 2, list.Count, "Wrong number of elements in relation table." );
			// verify read upon create
			list = new GentleList( typeof(UserRole), u1 );
			Assert.AreEqual( 2, list.Count, "Elements in relation table was not automatically retrieved." );
			// verify remove
			list.Remove( list[ 0 ] );
			Assert.AreEqual( 1, list.Count, "Relation element was not removed from list." );
			list.Remove( list[ 0 ] );
			Assert.AreEqual( 0, list.Count, "Relation element was not removed from list." );
			list = new GentleList( typeof(UserRole), u1 );
			Assert.AreEqual( 0, list.Count, "Relation element was not removed from database." );
			u1.Remove();
		}

		[Test]
		public void Test_GentleRelation()
		{
			Role r1 = fixedRoles[ 1 ];
			// create a user with a known id
			u1 = new User( "John", "Doe", Roles.Customer );
			u1.Persist();
			// create 1:n type list
			GentleRelation list = new GentleRelation( typeof(UserRole), u1, typeof(Role) );
			Assert.AreEqual( 0, list.Count, "Relation table not empty." );
			// verify add
			list.Add( r1 );
			list.Add( fixedRoles[ 2 ] );
			Assert.AreEqual( 2, list.Count, "Wrong number of elements in relation table." );
			// verify read upon create
			list = new GentleRelation( typeof(UserRole), u1, typeof(Role) );
			Assert.AreEqual( 2, list.Count, "Elements in relation table not automatically retrieved." );
			// verify remove
			list.Remove( r1 );
			Assert.AreEqual( 1, list.Count, "Relation element was not removed from list." );
			list.Remove( fixedRoles[ 2 ] );
			Assert.AreEqual( 0, list.Count, "Relation element was not removed from list." );
			list = new GentleRelation( typeof(UserRole), u1, typeof(Role) );
			Assert.AreEqual( 0, list.Count, "Relation element was not removed from database." );
			u1.Remove();
		}

		/// <summary>
		/// This test case verifies the automatic management of both the relation and the 
		/// related objects, i.e. when you add a Role object it is persisted along with an
		/// entry in the UserRole table, and vice versa when Role objects are removed.
		/// </summary>
		[Test]
		public void Test_NtoM()
		{
			// create a user with a known id
			u1 = new User( "John", "Doe", Roles.Customer );
			u1.Persist();
			Role r1 = fixedRoles[ 2 ];
			Assert.IsNotNull( u1, "Test case invalid unless a record with UserId=" + u1.Id + " exists" );
			// add role
			u1.Roles.Add( r1 );
			// verify UserRole table
			IList relations = Broker.RetrieveList( typeof(UserRole) );
			Assert.AreEqual( 1, relations.Count, "No relation created." );
			// remove role
			u1.Roles.Remove( r1 );
			// verify UserRole table
			relations = Broker.RetrieveList( typeof(UserRole) );
			Assert.AreEqual( 0, relations.Count, "Relation not removed." );
			u1.Remove();
		}

		/// <summary>
		/// This test case extends the N:M test above by also storing an additional
		/// value in the relation table (a reference to a Member instance).
		/// </summary>
		[Test]
		public void Test_NtoM_WithExtraRelationColumn()
		{
			// create a user with a known id
			u1 = new User( "John", "Doe", Roles.Customer );
			u1.Persist();
			Role r1 = fixedRoles[ 2 ];
			Assert.IsNotNull( u1, "Test case invalid unless a record with UserId=" + u1.Id + " exists" );
			Member m = Member.Retrieve( 1 );
			// add role
			u1.MemberRoles.Add( r1, m );
			// verify UserRole table
			IList relations = Broker.RetrieveList( typeof(UserRole) );
			Assert.AreEqual( 1, relations.Count, "No relation created." );
			UserRole ur = relations[ 0 ] as UserRole;
			Assert.AreEqual( m.Id, ur.MemberId, "Relation did not save additional type reference." );
			// remove role
			u1.MemberRoles.Remove( r1 );
			// verify UserRole table
			relations = Broker.RetrieveList( typeof(UserRole) );
			Assert.AreEqual( 0, relations.Count, "Relation not removed." );
			u1.Remove();
		}

		/// <summary>
		/// This test case verifies the automatic retrieval of objects during list instantiation.
		/// </summary>
		[Test]
		public void Test_AutoRead()
		{
			// create a user with a known id
			u1 = new User( "John", "Doe", Roles.Customer );
			u1.Persist();
			Role r1 = fixedRoles[ 1 ];
			Role r2 = fixedRoles[ 2 ];
			// add a role (will insert the userrole relation)
			Assert.AreEqual( 0, u1.Roles.Count, "Roles list be empty when starting out." );
			u1.Roles.Add( r1 );
			Assert.AreEqual( 1, u1.Roles.Count, "Roles list must contain an entry after calling Add." );
			u1.Roles.Add( r2 );
			Assert.AreEqual( 2, u1.Roles.Count, "Roles list must contain an entry after calling Add." );
			// read user 1 again
			u2 = new User( u1.Id );
			Assert.AreEqual( u1.Roles.Count, u2.Roles.Count, "Roles not auto-populated during list creation." );
			// verify UserRole table
			IList relations = Broker.RetrieveList( typeof(UserRole) );
			Assert.AreEqual( 2, relations.Count, "Relations not added." );
			// add the same role again
			//u2.Roles.Add( r1 );
			//u1 = new User( 1 );
			//Assert.AreEqual( 1, u1.Roles.Count, "Roles was added twice - no duplicate filtering." );
			// remove role
			u1.Roles.Remove( r1 );
			u1 = new User( u1.Id );
			Assert.AreEqual( 1, u1.Roles.Count, "Role not removed." );
			// remove role
			u1.Roles.Remove( r2 );
			u1 = new User( u1.Id );
			Assert.AreEqual( 0, u1.Roles.Count, "Role not removed." );
			// verify UserRole table
			relations = Broker.RetrieveList( typeof(UserRole) );
			Assert.AreEqual( 0, relations.Count, "Relations not removed." );
			u1.Remove();
		}

		[Test]
		public void Test_GentleListWithCaching()
		{
			// enable all caching 
			GentleSettings.CacheObjects = true;
			GentleSettings.CacheStatements = true;
			GentleSettings.SkipQueryExecution = true;
			CacheManager.Clear();

			// create a user with a known id
			u1 = new User( "John", "Doe", Roles.Customer );
			u1.Persist();
			// create n:m type list
			GentleList list = new GentleList( typeof(Role), u1, typeof(UserRole) );
			Assert.AreEqual( 0, list.Count, "Test requires that tables are initially empty." );
			// verify add
			Role r1 = new Role( 0, "Role 1" );
			r1.Persist();
			list.Add( r1 );
			Assert.AreEqual( 1, list.Count, "No element in relation table." );
			Role r2 = new Role( 0, "Role 2" );
			r2.Persist();
			list.Add( r2 );
			Assert.AreEqual( 2, list.Count, "Wrong number of elements in relation table." );
			// verify read upon create
			list = new GentleList( typeof(Role), u1, typeof(UserRole) );
			Assert.AreEqual( 2, list.Count, "Elements in relation table were not automatically retrieved." );
			// verify remove
			list.Remove( r1 );
			Assert.AreEqual( 1, list.Count, "Element was not removed from list." );
			// verify add after remove
			Role r3 = new Role( 0, "Role 3" );
			r3.Persist();
			list.Add( r3 );
			Assert.AreEqual( 2, list.Count, "Wrong number of elements in relation table." );
			// remove remaining elements
			list.Remove( r2 );
			list.Remove( r3 );
			Assert.AreEqual( 0, list.Count, "Relation element was not removed from list." );
			list = new GentleList( typeof(UserRole), u1 );
			Assert.AreEqual( 0, list.Count, "Relation element was not removed from database." );
			u1.Remove();
		}
	}
}
/*
 * Test cases
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: TestMemberCC.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Collections;
using Gentle.Common;
using NUnit.Framework;

namespace Gentle.Framework.Tests
{
	/// <summary>
	/// This class holds test cases for exercising the Gentle framework using the 
	/// MemberCC class. The database must have been created and populated with the 
	/// supplied test data for the tests to work.
	/// The MemberCC class is like the Member class with added concurrency control.
	/// </summary>
	[TestFixture]
	public class TestMemberCC
	{
		private MemberCC m1, m2;
		private MailingList list;

		[SetUp]
		public void Init()
		{
			// make sure we have only 4 members 
			SqlBuilder sb = new SqlBuilder( StatementType.Delete, typeof(MemberCC) );
			sb.AddConstraint( Operator.GreaterThan, "Id", 4 );
			Broker.Execute( sb.GetStatement( true ) );
			// make sure we have only 3 lists 
			sb = new SqlBuilder( StatementType.Delete, typeof(MailingList) );
			sb.AddConstraint( Operator.GreaterThan, "Id", 3 );
			Broker.Execute( sb.GetStatement( true ) );
			// create an empty mailing list
			list = new MailingList( "SomeList", "some.sender@doe.com" );
			list.Persist();
		}

		[TearDown]
		public void Final()
		{
			// make sure we have only the default 4 members 
			SqlBuilder sb = new SqlBuilder( StatementType.Delete, typeof(MemberCC) );
			sb.AddConstraint( Operator.GreaterThan, "Id", 4 );
			Broker.Execute( sb.GetStatement( true ) );
			// remove the list
			list.Remove();
		}

		/// <summary>
		/// Test case for verifying the four basic statement types (CRUD for Create, Read, Update, 
		/// Delete). This method executes 6 statements (of which the last select returns no data).
		/// </summary>
		[Test]
		public void TestCRUD()
		{
			GentleSettings.CacheObjects = false;
			if( GentleSettings.ConcurrencyControl )
			{
				m1 = new MemberCC( list.Id, "John Doe", "john@doe.com" );
				// insert
				m1.Persist();
				Assert.AreEqual( m1.Name, "John Doe", "The object was not properly inserted!" );
				Assert.AreEqual( m1.Address, "john@doe.com", "The object was not properly inserted!" );
				// select
				m2 = MemberCC.Retrieve( m1.Id );
				// verify select/insert
				Assert.IsTrue( m2.Id != 0, "The object could not be retrieved from the database!" );
				Assert.IsTrue( m2.Id > 4, "Existing id was reused!" );
				Assert.AreEqual( m1.Id, m2.Id, "The object could not be retrieved from the database!" );
				Assert.AreEqual( "John Doe", m2.Name, "The object was not properly retrieved on construction!" );
				Assert.AreEqual( "john@doe.com", m2.Address,
				                 "The object was not properly retrieved on construction!" );
				Assert.AreEqual( m1.DatabaseVersion, m2.DatabaseVersion, "Database revision not retrieved!" );
				// update
				m2.Name = "Jane Doe";
				m2.Address = "jane@doe.com";
				m2.Persist();
				Assert.AreEqual( m1.DatabaseVersion + 1, m2.DatabaseVersion, "Database revision not incremented!" );
				// verify update
				m1 = MemberCC.Retrieve( m2.Id );
				Assert.AreEqual( m2.Name, m1.Name, "Name not updated!" );
				Assert.AreEqual( m2.Address, m1.Address, "SenderAddress not updated!" );
				Assert.AreEqual( m1.DatabaseVersion, m2.DatabaseVersion, "Database revision not retrieved!" );
				// delete
				m2.Remove();
				// verify delete by counting the number of rows
				SqlBuilder sb = new SqlBuilder( StatementType.Count, typeof(MemberCC) );
				sb.AddConstraint( Operator.Equals, "Id", m1.Id );
				SqlResult sr = Broker.Execute( sb.GetStatement( true ) );
				Assert.AreEqual( 0, sr.Count, "Object not removed" );
			}
		}

		/// <summary>
		/// Verify that the StatementType.Count produces correct results.
		/// </summary>
		[Test]
		public void TestCount()
		{
			if( GentleSettings.ConcurrencyControl )
			{
				GentleSqlFactory sf = Broker.GetSqlFactory();
				// first, select the number of expected entries
				SqlResult sr = Broker.Execute( String.Format( "select count(*) as RecordCount from {0}{1}",
				                                              sf.GetTableName( "ListMember" ), sf.GetStatementTerminator() ) );
				Assert.IsNotNull( sr );
				Assert.AreEqual( 0, sr.ErrorCode );
				Assert.IsNotNull( sr.Rows );
				Assert.AreEqual( 1, sr.Rows.Count );
				object[] row = (object[]) sr.Rows[ 0 ];
				int expected = Convert.ToInt32( row[ 0 ] );
				// verify StatementType.Count retrieval
				SqlBuilder sb = new SqlBuilder();
				sr = Broker.Execute( sb.GetStatement( StatementType.Count, typeof(MemberCC) ) );
				Assert.AreEqual( expected, sr.Count );
			}
		}

		/// <summary>
		/// Verify that concurrency control updates the database version.
		/// </summary>
		[Test]
		public void TestConcurrencyControlValue()
		{
			if( GentleSettings.ConcurrencyControl )
			{
				m1 = MemberCC.Retrieve( 1 );
				long version = m1.DatabaseVersion;
				m1.Persist();
				Assert.AreEqual( version + 1, m1.DatabaseVersion, "Object version was not updated." );
				m2 = MemberCC.Retrieve( 1 );
				Assert.AreEqual( m1.DatabaseVersion, m2.DatabaseVersion, "Database version was not updated." );
			}
		}

		/// <summary>
		/// Verify that GentleList works when concurrency control is enabled.
		/// </summary>
		[Test]
		public void TestGentleListWithConcurrency()
		{
			if( GentleSettings.ConcurrencyControl )
			{
				MailingList list = MailingList.Retrieve( 1 );
				GentleList members = new GentleList( typeof(MemberCC), list );
				Assert.AreEqual( 2, members.Count, "List not initialized." );
				IList check = Broker.RetrieveList( typeof(MemberCC), list.GetKey() );
				Assert.AreEqual( 2, check.Count, "List contents dubious." );
			}
		}

		/// <summary>
		/// Verify that concurrency control prevents updates using old data.
		/// </summary>
		[Test, ExpectedException( typeof(GentleException), "TEST" )]
		public void TestConcurrencyControlError()
		{
			try
			{
				// object uniqing defeats this test
				GentleSettings.CacheObjects = false;
				Check.LogError( LogCategories.General, "The following error message has been provoked as part of a test case:" );
				if( GentleSettings.ConcurrencyControl )
				{
					m1 = MemberCC.Retrieve( 1 );
					m2 = MemberCC.Retrieve( 1 );
					long version = m1.DatabaseVersion;
					m1.Persist(); // ok
					m2.Persist(); // fails as it is the same record as m1
				}
				else
				{
					// fake expected result
					Check.Fail( Error.UnexpectedRowCount, 0, 0 );
				}
			}
			catch( GentleException ge )
			{
				if( ge.Error == Error.UnexpectedRowCount )
				{
					throw new GentleException( Error.Unspecified, "TEST" );
				}
				else
				{
					throw;
				}
			}
		}
	}
}
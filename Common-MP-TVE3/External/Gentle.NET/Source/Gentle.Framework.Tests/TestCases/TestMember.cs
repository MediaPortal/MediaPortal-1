/*
 * Test cases
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: TestMember.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Collections;
using Gentle.Common;
using NUnit.Framework;

namespace Gentle.Framework.Tests
{
	/// <summary>
	/// This class holds test cases for exercising the Gentle framework using the 
	/// Member class. The database must have been created and populated with the 
	/// supplied test data for the tests to work.
	/// </summary>
	[TestFixture]
	public class TestMember
	{
		private Member m1, m2;
		private MailingList list;

		[SetUp]
		public void Init()
		{
			GentleSettings.CacheObjects = false;
			CacheManager.Clear();
			// make sure we have only 4 members 
			SqlBuilder sb = new SqlBuilder( StatementType.Delete, typeof(Member) );
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
			SqlBuilder sb = new SqlBuilder( StatementType.Delete, typeof(Member) );
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
			m1 = new Member( list.Id, "John Doe", "john@doe.com" );
			// insert
			m1.Persist();
			Assert.AreEqual( m1.Name, "John Doe", "The object was not properly inserted!" );
			Assert.AreEqual( m1.Address, "john@doe.com", "The object was not properly inserted!" );
			// select
			m2 = Member.Retrieve( m1.Id );
			// verify select/insert
			Assert.IsTrue( m2.Id != 0, "The object could not be retrieved from the database!" );
			Assert.AreEqual( m1.Id, m2.Id, "The object could not be retrieved from the database!" );
			Assert.AreEqual( "John Doe", m2.Name, "The object was not properly retrieved on construction!" );
			Assert.AreEqual( "john@doe.com", m2.Address,
			                 "The object was not properly retrieved on construction!" );
			// update
			m2.Name = "Jane Doe";
			m2.Address = "jane@doe.com";
			m2.Persist();
			// verify update
			m1 = Member.Retrieve( m2.Id );
			Assert.AreEqual( m2.Name, m1.Name, "Name not updated!" );
			Assert.AreEqual( m2.Address, m1.Address, "SenderAddress not updated!" );
			// delete
			m2.Remove();
			// verify delete by counting the number of rows
			SqlBuilder sb = new SqlBuilder( StatementType.Count, typeof(Member) );
			sb.AddConstraint( Operator.Equals, "Id", m1.Id );
			SqlResult sr = Broker.Execute( sb.GetStatement( true ) );
			Assert.AreEqual( 0, sr.Count, "Object not removed" );
		}

		/// <summary>
		/// Verify that the StatementType.Count produces correct results.
		/// </summary>
		[Test]
		public void TestCount()
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
			sr = Broker.Execute( sb.GetStatement( StatementType.Count, typeof(Member) ) );
			Assert.AreEqual( expected, sr.Count );
		}

		/// <summary>
		/// Verify that foreign key translation works when columns do no use same name in
		/// both tables.
		/// </summary>
		[Test]
		public void TestForeignKeyTranslation()
		{
			foreach( MailingList ml in MailingList.ListAll )
			{
				foreach( Member m in ml.Members )
				{
					IList lists = m.MemberOfList;
					Assert.IsTrue( lists.Count > 0, "Member mismatch due to column name translation error." );
				}
			}
		}
	}
}
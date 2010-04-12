/*
 * Test cases
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: TestGuidIP.cs 1232 2008-03-14 05:36:00Z mm $
 */
using System;
using NUnit.Framework;

namespace Gentle.Framework.Tests
{
	/// <summary>
	/// This class holds test cases for exercising the Gentle framework using the 
	/// GuidHolderIP class. The purpose of these tests is to ensure that Gentle can
	/// work with objects implementing IPersistent (rather than inheriting from the
	/// Persistent base class).
	/// </summary>
	[TestFixture]
	public class TestGuidHolderIP
	{
		private GuidHolderIP o1, o2;
		private bool runTest;

		[SetUp]
		public void Init()
		{
			try
			{
				GentleSqlFactory sf = Broker.GetSqlFactory();
				// this will throw an exception because under normal operation it would indicate an error
				runTest = sf.GetDbType( typeof(Guid) ) != sf.NO_DBTYPE;
			}
			catch
			{
				runTest = false;
			}
		}

		[TearDown]
		public void Final()
		{
			SqlBuilder sb = new SqlBuilder( StatementType.Delete, typeof(GuidHolderIP) );
			Broker.Execute( sb.GetStatement( true ) );
		}

		/// <summary>
		/// Test case for verifying the four basic statement types (CRUD for Create, Read, Update, 
		/// Delete). This method executes 5 statements.
		/// </summary>
		[Test]
		public void TestCRUD()
		{
			// skip test if GUIDs are not supported by database backend
			if( runTest )
			{
				o1 = new GuidHolderIP( 42 );
				// insert
				o1.Persist();
				// select
				o2 = GuidHolderIP.Retrieve( o1.Id );
				// verify select/insert
				Assert.IsNotNull( o2.Id, "The object could not be retrieved from the database!" );
				Assert.AreEqual( o1.Id, o2.Id, "The object could not be retrieved from the database!" );
				Assert.AreEqual( o1.SomeValue, o2.SomeValue, "The object could not be retrieved from the database!" );
				// update
				o2.Persist();
				// verify update
				o1 = GuidHolderIP.Retrieve( o2.Id );
				// delete
				o2.Remove();
				// verify delete by counting the number of rows
				SqlBuilder sb = new SqlBuilder( StatementType.Count, typeof(GuidHolderIP) );
				sb.AddConstraint( Operator.Equals, "Id", o1.Id );
				SqlResult sr = Broker.Execute( sb.GetStatement( true ) );
				Assert.AreEqual( 0, sr.Count, "Object not removed" );
			}
		}
	}
}
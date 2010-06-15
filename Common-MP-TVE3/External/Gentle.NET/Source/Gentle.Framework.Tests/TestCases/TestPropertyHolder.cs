/*
 * Test cases
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: TestPropertyHolder.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Collections;
using System.Threading;
using Gentle.Common;
using NUnit.Framework;

namespace Gentle.Framework.Tests
{
	[TestFixture]
	public class TestPropertyHolder
	{
		private PropertyHolder a, b;

		public void CRUD( int loopFactor )
		{
			for( int i = 0; i < loopFactor; i++ )
			{
				TestCRUD();
			}
		}

		[SetUp]
		public void Init()
		{
			GentleSettings.CacheObjects = false;
			CacheManager.Clear();
			// make sure table is empty before we start
			SqlBuilder sb = new SqlBuilder( StatementType.Delete, typeof(PropertyHolder) );
			Broker.Execute( sb.GetStatement( true ) );
		}

		[TearDown]
		public void Exit()
		{
			// clean up after running tests
			SqlBuilder sb = new SqlBuilder( StatementType.Delete, typeof(PropertyHolder) );
			Broker.Execute( sb.GetStatement( true ) );
		}

		[Test]
		[Ignore( "Requires content in db" )]
		public void TestNullHandling()
		{
			a = PropertyHolder.Retrieve( 2107 );
		}

		[Test]
		public void TestEmptyTableListSelect()
		{
			IList list = Broker.RetrieveList( typeof(PropertyHolder) );
			Assert.IsNotNull( list, "No list instance returned." );
			Assert.AreEqual( 0, list.Count, "List not empty!" );
		}

		[Test]
		public void TestCRUD()
		{
			a = new PropertyHolder( 0, "MyPH", 2, 3, 4, 5.0, true, DateTime.Now, DateTime.Now,
			                        "char", "nchar", "varchar", "nvarchar", "text", "ntext" );
			// insert
			a.Persist();
			Assert.AreEqual( a.Name, "MyPH" );
			// select
			b = PropertyHolder.Retrieve( a.Id );
			// verify select/insert
			Assert.IsTrue( b.Id != 0 );
			Assert.AreEqual( a.Id, b.Id );
			Assert.AreEqual( "MyPH", b.Name );
			// update
			b.Name = "NewPH";
			b.TDateTime = DateTime.MinValue; // should result in DBNull being written
			b.Persist();
			// verify update
			a = Broker.RetrieveInstance( typeof(PropertyHolder), b.GetKey() ) as PropertyHolder;
			Assert.AreEqual( b.Name, a.Name );
			Assert.AreEqual( DateTime.MinValue, a.TDateTime );
			// delete
			b.Remove();
			// verify delete by counting the number of rows
			SqlBuilder sb = new SqlBuilder( StatementType.Count, typeof(PropertyHolder) );
			sb.AddConstraint( Operator.Equals, "Id", a.Id );
			SqlResult sr = Broker.Execute( sb.GetStatement( true ) );
			Assert.AreEqual( 0, sr.Count, "Object not removed" );
		}

		[Test]
		public void TestMagicValue()
		{
			int nullValue = -1;
			a = new PropertyHolder( 0, "MyPH", nullValue, nullValue, 4, 5.0, true, new DateTime( 2000, 1, 20 ),
			                        DateTime.Now, "char", "nchar", "varchar", "nvarchar", "text", "ntext" );
			// insert
			a.Persist();
			Assert.AreEqual( a.Name, "MyPH" );
			// select raw data
			GentleSqlFactory sf = Broker.GetSqlFactory();
			SqlResult sr = Broker.Execute( "select TInt from " + sf.GetTableName( "PropertyHolder" ) );
			// verify that TInt of nullValue was converted to DB null
			Assert.IsNull( sr[ 0, "TInt" ], "NullValue was not converted to null." );
			// verify select/insert
			b = PropertyHolder.Retrieve( a.Id );
			Assert.IsTrue( b.Id != 0 );
			Assert.AreEqual( a.Id, b.Id );
			Assert.AreEqual( "MyPH", b.Name );
			Assert.AreEqual( nullValue, b.TInt, "Database NULL was not converted to NullValue." );
			Key key = new Key( typeof(PropertyHolder), true, "Id", a.Id );
			b = Broker.RetrieveInstance( typeof(PropertyHolder), key ) as PropertyHolder;
			// verify select/insert
			Assert.IsTrue( b.Id != 0 );
			Assert.AreEqual( a.Id, b.Id );
			Assert.AreEqual( "MyPH", b.Name );
			Assert.AreEqual( nullValue, b.TInt, "Database NULL was not converted to NullValue." );
			// delete
			b.Remove();
		}

		[Test]
		public void TestNullStringMagicValue()
		{
			GentleSettings.CacheObjects = false;
			CacheManager.Clear();
			// verify INSERT using NULL
			PropertyHolder ph = new PropertyHolder();
			ph.Name = "first";
			ph.TDateTimeNN = DateTime.Now;
			ph.Persist();
			int testPropertyHolderID = ph.Id;
			// check null column on database 
			SqlResult sr = Broker.Execute( "select ph_Name, TVarChar from PropertyHolder where ph_ID = " + testPropertyHolderID );
			Assert.AreEqual( "first", sr[ 0, "ph_Name" ] );
			Assert.AreEqual( null, sr[ 0, "TVarChar" ] );
			// verify UPDATE using NULL
			ph.Name = "second";
			ph.Persist();
			// check null column on database 
			sr = Broker.Execute( "select ph_Name, TVarChar from PropertyHolder where ph_ID = " + testPropertyHolderID );
			Assert.AreEqual( "second", sr[ 0, "ph_Name" ] );
			Assert.AreEqual( null, sr[ 0, "TVarChar" ] );
			// re-load and assert that null is converted to MagicValue
			ph = PropertyHolder.Retrieve( testPropertyHolderID );
			Assert.AreEqual( "second", ph.Name );
			Assert.AreEqual( "", ph.TVarChar );
			// verify UPDATE using ""
			ph.Name = "third";
			ph.Persist();
			// check null column on database 
			sr = Broker.Execute( "select ph_Name, TVarChar from PropertyHolder where ph_ID = " + testPropertyHolderID );
			Assert.AreEqual( "third", sr[ 0, "ph_Name" ] );
			Assert.AreEqual( null, sr[ 0, "TVarChar" ], "Database field no longer Null" );
			// verify INSERT using ""
			ph = new PropertyHolder();
			ph.Name = "fourth";
			ph.TDateTimeNN = DateTime.Now;
			ph.TVarChar = "";
			ph.Persist();
			// check null column on database 
			sr = Broker.Execute( "select ph_Name, TVarChar from PropertyHolder where ph_ID = " + ph.Id );
			Assert.AreEqual( "fourth", sr[ 0, "ph_Name" ] );
			Assert.AreEqual( null, sr[ 0, "TVarChar" ], "Database field no longer Null" );
		}

		/// <summary>
		/// Simple method for looping the CRUD test case (for performance measurement).
		/// </summary>
		[Test]
		[Ignore( "This test case is a template for simple performance tests." )]
		public void TestCRUD_X()
		{
			CRUD( 10 ); // modify the parameter to change the number of loops executed 
		}

		/// <summary>
		/// Test that a database field with a name different than the object property is 
		/// correctly found. For instance a table "MyTable" with the primary key of MyTable_Id
		/// will be able to populate a property identified by MyObject.Id.
		/// </summary>
		[Test]
		public void TestIdFieldNotSame()
		{
			// avoid using DateTime.Now when comparing objects/DTs (because of common db rounding mismatches)
			a = new PropertyHolder( 0, "MyPH", 2, 3, 4, 5.0, true, new DateTime( 2000, 1, 20 ), new DateTime( 2000, 1, 30 ),
			                        "char", "nchar", "varchar", "nvarchar", "text", "ntext" );
			// insert
			a.Persist();
			Assert.AreEqual( a.Name, "MyPH" );
			Assert.AreEqual( a.TDecimal, 4, "TDecimal not updated." );
			// select
			b = PropertyHolder.Retrieve( a.Id );
			Assert.AreEqual( a, b, "Refresh did not properly retrieve all fields." );
			a = (PropertyHolder) Broker.RetrieveInstance( a.GetType(), new Key( typeof(PropertyHolder), true, "Id", a.Id ) );
			Assert.AreEqual( a, b, "Refresh and retrieve did not yield identical objects." );
			// cleanup
			a.Remove();
		}

		/// <summary>
		/// Test that a database field can be used multiple times in constraints.
		/// </summary>
		[Test]
		public void TestColumnInMultipleConstraints()
		{
			a = new PropertyHolder( 0, "MyPH", 2, 3, 4, 5.0, true, new DateTime( 2000, 1, 20 ), DateTime.Now,
			                        "char", "nchar", "varchar", "nvarchar", "text", "ntext" );
			// insert
			a.Persist();
			// select
			SqlBuilder sb = new SqlBuilder( StatementType.Select, typeof(PropertyHolder) );
			sb.AddConstraint( Operator.GreaterThan, "TDateTime", new DateTime( 1999, 1, 1 ) );
			sb.AddConstraint( Operator.LessThan, "TDateTime", new DateTime( 2001, 1, 1 ) );
			SqlStatement stmt = sb.GetStatement();
			SqlResult sr = stmt.Execute();
			Assert.AreEqual( 1, sr.RowsContained, "Statement did not fetch the expected row." );
		}

		[Test]
		public void TestMySQLDateTimeBug()
		{
			// insert first
			a = new PropertyHolder( 0, "MyPH1", 2, 3, 4, 5.0, true, DateTime.Now, DateTime.Now,
			                        "char", "nchar", "varchar", "nvarchar", "text", "ntext" );
			a.Persist();
			// wait a bit (dont set below 1s as milliseconds are not preserved by MySQL provider)
			Thread.Sleep( 1000 );
			// insert second
			a = new PropertyHolder( 0, "MyPH2", 2, 3, 4, 5.0, true, DateTime.Now, DateTime.Now,
			                        "char", "nchar", "varchar", "nvarchar", "text", "ntext" );
			a.Persist();
			// select
			IList list = PropertyHolder.ListAll;
			DateTime last = DateTime.MinValue;
			foreach( PropertyHolder ph in list )
			{
				Assert.IsTrue( last != ph.TDateTime, "DateTime not correctly retrieved or saved." );
				last = ph.TDateTime;
			}
		}

		[Test]
		public void TestNullValueForDecimal()
		{
			ObjectMap om = ObjectFactory.GetMap( null, typeof(PropertyHolder) );
			FieldMap fm = om.GetFieldMap( "TDecimal" );
			decimal dec = (decimal) fm.NullValue;
			Assert.AreEqual( Decimal.MinValue, dec, "Invalid NullValue for test case." );
			// test NULL handling for decimals
			a = new PropertyHolder( 0, "MyPH", 2, 3, Decimal.MinValue, 5.0, true, DateTime.Now, DateTime.Now,
			                        "char", "nchar", "varchar", "nvarchar", "text", "ntext" );
			a.Persist();
			// verify that NULL is inserted when using NullValue
			SqlResult sr = Broker.Execute( "select TDecimal from PropertyHolder where ph_Id = " + a.Id );
			Assert.IsNull( sr[ 0, "TDecimal" ], "Default NullValue was not converted to NULL on insert." );
			// verify that object creation uses correct NullValue
			b = PropertyHolder.Retrieve( a.Id );
			Assert.AreEqual( a.TDecimal, b.TDecimal );
			Assert.AreEqual( Decimal.MinValue, b.TDecimal );
			// verify that translation is disabled for ordinary values
			b.TDecimal = 4;
			b.Persist();
			a = PropertyHolder.Retrieve( b.Id );
			Assert.AreEqual( 4, a.TDecimal );
			// verify that translation is disabled for MaxValue
			// diabled: SQL Server 2005 only works with 2^38-1 (whereas MaxValue is 2^96-1)..
			//a.TDecimal = Decimal.MaxValue;
			//a.Persist();
			//sr = Broker.Execute( "select TDecimal from PropertyHolder where ph_Id = "+a.Id );
			//Assert.IsNotNull( sr[ 0, "TDecimal" ], "Erronous NullValue conversion for MaxValue." );
			// cleanup
			a.Remove();
		}
	}
}
/*
 * Test cases
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: TestGuid.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using NUnit.Framework;

namespace Gentle.Framework.Tests
{
	/// <summary>
	/// This class holds test cases for exercising the Gentle framework using the 
	/// Member class. The database must have been created and populated with the 
	/// supplied test data for the tests to work.
	/// </summary>
	[TestFixture]
	public class TestGuidHolder
	{
		private GuidHolder o1, o2;
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
			SqlBuilder sb = new SqlBuilder( StatementType.Delete, typeof(GuidHolder) );
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
				o1 = new GuidHolder( 42 );
				// insert
				o1.Persist();
				// select
				o2 = GuidHolder.Retrieve( o1.Id );
				// verify select/insert
				Assert.IsNotNull( o2.Id, "The object could not be retrieved from the database!" );
				Assert.AreEqual( o1.Id, o2.Id, "The object could not be retrieved from the database!" );
				Assert.AreEqual( o1.SomeValue, o2.SomeValue, "The object could not be retrieved from the database!" );
				// update
				o2.SomeValue = 1234;
				o2.Persist();
				// verify update
				o1 = GuidHolder.Retrieve( o2.Id );
				Assert.AreEqual( o2.Id, o1.Id, "The object could not be retrieved from the database!" );
				// delete
				o2.Remove();
				// verify delete by counting the number of rows
				SqlBuilder sb = new SqlBuilder( StatementType.Count, typeof(GuidHolder) );
				sb.AddConstraint( Operator.Equals, "Id", o1.Id );
				SqlResult sr = Broker.Execute( sb.GetStatement( true ) );
				Assert.AreEqual( 0, sr.Count, "Object not removed" );
			}
		}

		/*
		[Test]
		public void TestGuidWithJetProvider1()
		{
			if( Broker.ProviderName == "Jet" )
			{
				o1 = new GuidHolder( 42 );
				SqlBuilder sb = new SqlBuilder( StatementType.Insert, typeof(GuidHolder) );
				SqlStatement stmt = sb.GetStatement( false );
				OleDbParameter p = stmt.Command.Parameters[ 0 ] as OleDbParameter;
				Assert.AreEqual( OleDbType.Guid, p.OleDbType );
				stmt.SetParameters( o1, true );
				Assert.AreEqual( OleDbType.Guid, p.OleDbType );
				Assert.AreEqual( o1.Id, p.Value );
			}
		}

		[Test]
		public void TestGuidWithJetProvider2()
		{
			GentleSettings.CacheObjects = false;
			OleDbConnection conn;
			OleDbCommand cmd;
			OleDbParameter param1, param2;
			// only run this obscure test for Jet
			if( Broker.ProviderName == "Jet" )
			{
				o1 = new GuidHolder( 42 );

				// insert (use plain ADO.NET)
				conn = Broker.GetNewConnection() as OleDbConnection;
				cmd = conn.CreateCommand();
				cmd.CommandText = "insert into GuidHolder ( [Guid], SomeValue ) values ( ?, ? );";
				param1 = new OleDbParameter( "[?Guid]", OleDbType.Guid );
				param1.Value = o1.Id;
				cmd.Parameters.Add( param1 );
				param2 = new OleDbParameter( "[?SomeValue]", OleDbType.Integer );
				param2.Value = 42;
				cmd.Parameters.Add( param2 );
				// execute insert
				Assert.AreEqual( 1, cmd.ExecuteNonQuery(), "No rows were modified." );
				conn.Close();
				
				// update (use plain ADO.NET)
				conn = Broker.GetNewConnection() as OleDbConnection;
				cmd = conn.CreateCommand();
				cmd.CommandText = "update GuidHolder set SomeValue = ? where [Guid] = ?;";
				param1 = new OleDbParameter( "[?SomeValue]", OleDbType.Integer );
				param1.Value = 1234;
				cmd.Parameters.Add( param1 );
				param2 = new OleDbParameter( "[?Guid]", OleDbType.Guid );
				param2.Value = o1.Id;
				cmd.Parameters.Add( param2 );
				
				// execute update
                Assert.AreEqual( 1, cmd.ExecuteNonQuery(), "No rows were modified." );
				conn.Close();
				
				// verify update
				o1 = GuidHolder.Retrieve( o1.Id );
				Assert.AreEqual( 1234, o1.SomeValue, "The object/row was not correctly updated." );
				
				// delete
				o1.Remove();
			}
		}
		*/
	}
}
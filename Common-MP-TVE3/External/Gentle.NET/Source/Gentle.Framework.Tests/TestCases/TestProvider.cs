/*
 * Test cases
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: TestProvider.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Collections;
using System.Data;
using Gentle.Common;
using NUnit.Framework;

namespace Gentle.Framework.Tests
{
	/// <summary>
	/// </summary>
	[TestFixture]
	public class TestProvider
	{
		private AnalyzerLevel origLevel;

		[SetUp]
		public void Init()
		{
			ObjectFactory.ClearMaps();
			Broker.ClearPersistenceBroker();
			// make sure analyzer errors are ignored for this test
			origLevel = GentleSettings.AnalyzerLevel;
			GentleSettings.AnalyzerLevel = AnalyzerLevel.None;
		}

		[TearDown]
		public void Exit()
		{
			ObjectFactory.ClearMaps();
			Broker.ClearPersistenceBroker();
			// restore original analyzer value
			GentleSettings.AnalyzerLevel = origLevel;
		}

		/// <summary>
		/// Verify parameter order when using unnamed parameters
		/// </summary>
		public void ExecuteTestUnnamedParameterOrder( string providerName )
		{
			GentleSettings.AnalyzerLevel = AnalyzerLevel.None;
			PersistenceBroker broker = new PersistenceBroker( providerName, "..." );
			GentleSqlFactory sf = broker.GetSqlFactory();
			if( ! sf.HasCapability( Capability.NamedParameters ) )
			{
				// create parameterized query
				SqlBuilder sb = new SqlBuilder( broker, StatementType.Select, typeof(MailingList) );
				sb.AddConstraint( Operator.Equals, "SenderAddress", "SenderAddress" );
				sb.AddConstraint( Operator.Equals, "Name", "Name" );
				SqlStatement stmt = sb.GetStatement( true );
				foreach( IDataParameter param in stmt.Command.Parameters )
				{
					Assert.IsTrue( param.ParameterName.IndexOf( (string) param.Value ) >= 0, "1: Parameter order not correctly maintained." );
				}
				// retry using parameters in reverse order 
				sb = new SqlBuilder( broker, StatementType.Select, typeof(MailingList) );
				sb.AddConstraint( Operator.Equals, "Name", "Name" );
				sb.AddConstraint( Operator.Equals, "SenderAddress", "SenderAddress" );
				stmt = sb.GetStatement( true );
				foreach( IDataParameter param in stmt.Command.Parameters )
				{
					Assert.IsTrue( param.ParameterName.IndexOf( (string) param.Value ) >= 0, "2: Parameter order not correctly maintained." );
				}
			}
		}

		/// <summary>
		/// Verify parameter order when using unnamed parameters
		/// </summary>
		[Test]
		public void TestUnnamedParameterOrderJet()
		{
			ExecuteTestUnnamedParameterOrder( "Jet" );
		}

		/// <summary>
		/// Verify parameter order when using unnamed parameters
		/// </summary>
		[Test]
		public void TestUnnamedParameterOrderSybase()
		{
			ExecuteTestUnnamedParameterOrder( "Sybase" );
		}

		/// <summary>
		/// Verify parameter order when using unnamed parameters
		/// </summary>
		[Test]
		public void TestManualProviderCreation()
		{
			string name = Configurator.GetKey( null, "Gentle.Framework/DefaultProvider/@name" );
			string connStr = Configurator.GetKey( null, "Gentle.Framework/DefaultProvider/@connectionString" );
			string schema = Configurator.GetKey( null, "Gentle.Framework/DefaultProvider/@schema" );
			//connStr = @"Server=(local)\SQLExpress;Database=Gentle;Integrated Security=true";
			PersistenceBroker broker = ProviderFactory.GetProvider( "test", name, connStr, schema ).Broker;
			IList list = broker.RetrieveList( typeof(MailingList) );
			Assert.IsNotNull( list, "No result returned." );
			Assert.IsTrue( list.Count > 0, "List was empty." );
			MailingList ml = list[ 0 ] as MailingList;
			ml.Persist();
		}

		/// <summary>
		/// Verify the Operator.In constraint.
		/// </summary>
		[Test]
		public void TestCustomListUsingInConstraint()
		{
			// subselects not supported by the inferior mysql engine - skip test. 
			if( ! Broker.ProviderName.Equals( "MySQL" ) )
			{
				GentleSqlFactory sf = Broker.GetSqlFactory();
				// first, select the number of expected entries
				SqlBuilder sb = new SqlBuilder( StatementType.Count, typeof(MailingList) );
				string sql = String.Format( "select distinct MemberAddress from {0}",
				                            sf.GetTableName( "ListMember" ) );
				sb.AddConstraint( Operator.In, "SenderAddress", Broker.Execute( sql ), "MemberAddress" );
				SqlResult sr = Broker.Execute( sb.GetStatement() );
				int expected = sr.Count;
				// verify list retrieval (using IList data)
				IList lists = MailingList.ListByCustomListConstraint();
				Assert.IsNotNull( lists );
				Assert.AreEqual( expected, lists.Count );
				// verify same result using alternative approach (using SqlResult data)
				sb = new SqlBuilder( StatementType.Select, typeof(Member) );
				SqlResult members = sb.GetStatement( true ).Execute();
				sb = new SqlBuilder( StatementType.Select, typeof(MailingList) );
				// use column name as indexer into SqlResult for list constraints
				sb.AddConstraint( Operator.In, "SenderAddress", members, "MemberAddress" );
				SqlStatement stmt = sb.GetStatement( true );
				lists = ObjectFactory.GetCollection( typeof(MailingList), stmt.Execute() );
				Assert.IsNotNull( lists );
				Assert.AreEqual( expected, lists.Count );
			}
		}
	}
}
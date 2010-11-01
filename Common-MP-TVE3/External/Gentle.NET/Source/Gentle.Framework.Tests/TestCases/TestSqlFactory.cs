/*
 * Abstract tester for SqlFactory implementations.
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: TestSqlFactory.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using NUnit.Framework;

namespace Gentle.Framework.Tests
{
	/// <summary>
	/// Test cases for the SQL Server rdbms engine.
	/// </summary>
	[TestFixture]
	public class TestSqlFactorySQLServer : TestSqlFactory
	{
		/// <summary>
		/// Test case initialization.
		/// </summary>
		[SetUp]
		public void Init()
		{
			Init( "SQLServer" );
		}

		/// <summary>
		/// Test case for verifying rdbms-specific SqlFactory output.
		/// </summary>
		[Test]
		public override void SqlFactoryOutput()
		{
			Assert.AreEqual( "@", sf.GetParameterPrefix() );
			Assert.AreEqual( "List", sf.GetTableName( "List" ) );
		}
	}

	/// <summary>
	/// Test cases for the Firebird rdbms engine.
	/// </summary>
	[TestFixture]
	public class TestSqlFactoryFirebird : TestSqlFactory
	{
		[SetUp]
		public void Init()
		{
			Init( "Firebird" );
		}

		public override void SqlFactoryOutput()
		{
			Assert.AreEqual( "@", sf.GetParameterPrefix() );
			Assert.AreEqual( "", sf.GetParameterSuffix() );
			//Assert.IsTrue( sf.HasCapability( Capability.BatchQuery ) );
			Assert.AreEqual( "List", sf.GetTableName( "List" ) );
		}
	}

	/// <summary>
	/// Test cases for the MS Access (Jet) rdbms engine.
	/// </summary>
	[TestFixture]
	public class TestSqlFactoryJet : TestSqlFactory
	{
		[SetUp]
		public void Init()
		{
			Init( "Jet" );
		}

		public override void SqlFactoryOutput()
		{
			Assert.AreEqual( "[?", sf.GetParameterPrefix() );
			Assert.AreEqual( "]", sf.GetParameterSuffix() );
			Assert.IsFalse( sf.HasCapability( Capability.BatchQuery ) );
			Assert.AreEqual( "List", sf.GetTableName( "List" ) );
		}
	}

	/// <summary>
	/// Test cases for the PostgreSQL rdbms engine.
	/// </summary>
	[TestFixture]
	public class TestSqlFactoryPostgreSQL : TestSqlFactory
	{
		/// <summary>
		/// Test case initialization.
		/// </summary>
		[SetUp]
		public void Init()
		{
			Init( "PostgreSQL" );
		}

		/// <summary>
		/// Test case for verifying rdbms-specific SqlFactory output.
		/// </summary>
		[Test]
		public override void SqlFactoryOutput()
		{
			Assert.AreEqual( ":", sf.GetParameterPrefix() );
			if( sf.GetTableName( "List" ).IndexOf( '.' ) != -1 )
			{
				Assert.AreEqual( "public.List", sf.GetTableName( "List" ) );
			}
			else
			{
				Assert.AreEqual( "List", sf.GetTableName( "List" ) );
			}
		}
	}

	/// <summary>
	/// Test cases for the MySQL rdbms engine.
	/// </summary>
	[TestFixture]
	public class TestSqlFactoryMySQL : TestSqlFactory
	{
		/// <summary>
		/// Test case initialization.
		/// </summary>
		[SetUp]
		public void Init()
		{
			Init( "MySQL" );
		}

		/// <summary>
		/// Test case for verifying rdbms-specific SqlFactory output.
		/// </summary>
		[Test]
		public override void SqlFactoryOutput()
		{
			Assert.AreEqual( "?", sf.GetParameterPrefix() );
			Assert.AreEqual( "List", sf.GetTableName( "List" ) );
		}
	}

	/// <summary>
	/// Test cases for the Oracle rdbms engine.
	/// </summary>
	[TestFixture]
	public class TestSqlFactoryOracle : TestSqlFactory
	{
		/// <summary>
		/// Test case initialization.
		/// </summary>
		[SetUp]
		public void Init()
		{
			Init( "Oracle" );
		}

		/// <summary>
		/// Test case for verifying rdbms-specific SqlFactory output.
		/// </summary>
		[Test]
		public override void SqlFactoryOutput()
		{
			Assert.AreEqual( ":", sf.GetParameterPrefix() );
			Assert.AreEqual( "", sf.GetStatementTerminator() );
			Assert.AreEqual( "List", sf.GetTableName( "List" ) );
		}
	}

	/*
	/// <summary>
	/// Test cases for the Sybase ASA rdbms engine.
	/// </summary>
	[TestFixture]
	public class TestSqlFactorySybaseAsa : TestSqlFactory
	{
		/// <summary>
		/// Test case initialization.
		/// </summary>
		[SetUp]
		public new void Init()
		{
			PersistenceBroker.ClearBroker();
			pb = PersistenceBroker.GetBroker( "SybaseAsaEngine", null );
			base.Init();
		}

		/// <summary>
		/// Test case for verifying rdbms-specific SqlFactory output.
		/// </summary>
		[Test]
		public override void SqlFactoryOutput()
		{
			Assert.IsTrue( sf is SybaseAsaFactory, "Wrong factory created." );
			Assert.AreEqual( "@", sf.GetParameterPrefix() );
			Assert.AreEqual( "List", sf.GetTableName( "List" ) );
		}
	}
	*/

	/// <summary>
	/// Test cases for verifying the syntax of generated sql statements.
	/// </summary>
	[TestFixture]
	public abstract class TestSqlFactory
	{
		protected GentleSqlFactory sf;
		protected SqlBuilder sb;
		protected PersistenceBroker broker;
		protected SqlStatement stmt;
		protected AnalyzerLevel origLevel;

		public virtual void Init( string providerName )
		{
			ObjectFactory.ClearMaps();
			Broker.ClearPersistenceBroker();
			ObjectFactory.ClearMaps();
			IGentleProvider provider = ProviderFactory.GetProvider( providerName, "" );
			Assert.AreEqual( provider.Name, providerName, "Wrong provider returned from factory!" );
			sf = provider.GetSqlFactory();
			broker = new PersistenceBroker( provider );
			sb = new SqlBuilder( provider );
			// make sure analyzer errors are ignored for this test
			origLevel = GentleSettings.AnalyzerLevel;
			GentleSettings.AnalyzerLevel = AnalyzerLevel.None;
		}

		[TearDown]
		public virtual void Exit()
		{
			ObjectFactory.ClearMaps();
			Broker.ClearPersistenceBroker();
			// restore original value
			GentleSettings.AnalyzerLevel = origLevel;
		}

		/// <summary>
		/// Abstract test case definition required by the Mono TestRunner program.
		/// </summary>
		[Test]
		public abstract void SqlFactoryOutput();

		private string GetParam( string paramName )
		{
			if( sf.HasCapability( Capability.NamedParameters ) )
			{
				return String.Format( "{0}{1}{2}", sf.GetParameterPrefix(), paramName, sf.GetParameterSuffix() );
			}
			else
			{
				return "?";
			}
		}

		/// <summary>
		/// Test case for verifying the syntax of generated select statements.
		/// </summary>
		[Test]
		public void Select()
		{
			stmt = sb.GetStatement( StatementType.Select, typeof(MailingList) );
			string expected = String.Format( "select ListId, ListName, SenderAddress from {0} where ListId = {1}{2}",
			                                 sf.GetTableName( "List" ), GetParam( "ListId" ), sf.GetStatementTerminator() );
			Assert.AreEqual( expected, stmt.Sql );
		}

		/// <summary>
		/// Test case for verifying the syntax of generated row count select statements.
		/// </summary>
		[Test]
		public void Count()
		{
			stmt = sb.GetStatement( StatementType.Count, typeof(MailingList) );
			string expected = String.Format( "select count(*) as RecordCount from {0}{1}",
			                                 sf.GetTableName( "List" ), sf.GetStatementTerminator() );
			Assert.AreEqual( expected, stmt.Sql );
		}

		/// <summary>
		/// Test case for verifying the syntax of generated select without constraint statements.
		/// </summary>
		[Test]
		public void SelectList()
		{
			stmt = sb.GetStatement( StatementType.Select, typeof(MailingList), true );
			string expected = String.Format( "select ListId, ListName, SenderAddress from {0}{1}",
			                                 sf.GetTableName( "List" ), sf.GetStatementTerminator() );
			Assert.AreEqual( expected, stmt.Sql );
		}

		/// <summary>
		/// Test case for verifying the syntax of generated insert statements.
		/// </summary>
		[Test]
		public virtual void Insert()
		{
			stmt = sb.GetStatement( StatementType.Insert, typeof(MailingList) );
			string expected = String.Format( "insert into {0} ( ListName, SenderAddress ) values ( {1}, {2} )",
			                                 sf.GetTableName( "List" ), GetParam( "ListName" ), GetParam( "SenderAddress" ) );
			// append get identity select where supported			
			if( sf.HasCapability( Capability.BatchQuery ) )
			{
				ObjectMap map = ObjectFactory.GetMap( broker, typeof(MailingList) );
				expected = sf.GetIdentitySelect( expected, map );
			}
			expected += sf.GetStatementTerminator();
			// compare expected output with SqlBuilder output
			Assert.AreEqual( expected, stmt.Sql );
		}

		/// <summary>
		/// Test case for verifying the syntax of generated update statements.
		/// </summary>
		[Test]
		public void Update()
		{
			stmt = sb.GetStatement( StatementType.Update, typeof(MailingList) );
			string expected = String.Format( "update {0} set ListName = {1}, SenderAddress = {2} where ListId = {3}{4}",
			                                 sf.GetTableName( "List" ), GetParam( "ListName" ), GetParam( "SenderAddress" ),
			                                 GetParam( "ListId" ), sf.GetStatementTerminator() );
			// compare expected output with SqlBuilder output
			Assert.AreEqual( expected, stmt.Sql );
		}

		/// <summary>
		/// Test case for verifying the syntax of generated delete statements.
		/// </summary>
		[Test]
		public void Delete()
		{
			stmt = sb.GetStatement( StatementType.Delete, typeof(MailingList) );
			string expected = String.Format( "delete from {0} where ListId = {1}{2}",
			                                 sf.GetTableName( "List" ), GetParam( "ListId" ), sf.GetStatementTerminator() );
			// compare expected output with SqlBuilder output
			Assert.AreEqual( expected, stmt.Sql );
		}
	}
}
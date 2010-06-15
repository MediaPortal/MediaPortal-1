/*
 * Test cases
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: TestList.cs 1241 2008-04-21 14:49:35Z mm $
 */

using System;
using System.Collections;
using System.Data;
using Gentle.Common;
using NUnit.Framework;

namespace Gentle.Framework.Tests
{
	/// <summary>
	/// This class holds test cases for exercising the Gentle framework using the 
	/// MailingList and Member classes. The database must have been created and
	/// populated with the supplied test data for the tests to work.
	/// </summary>
	[TestFixture]
	public class TestList
	{
		private MailingList l1, l2;

		/// <summary>
		/// Repeat the TestCRUD test case any number of times.
		/// </summary>
		/// <param name="loopFactor">The number of times to repeat the test</param>
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
			// make sure we have only 4 members 
			SqlBuilder sb = new SqlBuilder( StatementType.Delete, typeof(Member) );
			sb.AddConstraint( Operator.GreaterThan, "Id", 4 );
			Broker.Execute( sb.GetStatement( true ) );
			// make sure we have only 3 lists 
			sb = new SqlBuilder( StatementType.Delete, typeof(MailingList) );
			sb.AddConstraint( Operator.GreaterThan, "Id", 3 );
			Broker.Execute( sb.GetStatement( true ) );
			GentleSettings.CacheObjects = false;
			CacheManager.Clear();
		}

		[TearDown]
		public void Exit()
		{
		}

		/// <summary>
		/// Test case for verifying the four basic statement types (CRUD for Create, Read, Update, 
		/// Delete). This method executes 6 statements (of which the last select returns no data).
		/// </summary>
		[Test]
		public void TestCRUD()
		{
			l1 = new MailingList( "Test 1", "test@test.dk" );
			// insert
			l1.Persist();
			Assert.IsTrue( l1.Id > 0, "The List object was not assigned an id by the database!" );
			Assert.AreEqual( l1.Name, "Test 1", "The List object was not properly inserted!" );
			Assert.AreEqual( l1.SenderAddress, "test@test.dk", "The List object was not properly inserted!" );
			// select
			l2 = MailingList.Retrieve( l1.Id );
			// verify select/insert
			Assert.IsTrue( l2.Id != 0, "The List object could not be retrieved from the database!" );
			Assert.AreEqual( l1.Id, l2.Id, "The List object could not be retrieved from the database!" );
			Assert.AreEqual( "Test 1", l2.Name, "The List object was not properly retrieved on construction!" );
			Assert.AreEqual( "test@test.dk", l2.SenderAddress,
			                 "The List object was not properly retrieved on construction!" );
			// update
			l2.Name = "Test 2";
			l2.SenderAddress = "sender@test2.com";
			l2.Persist();
			// verify update
			l1 = MailingList.Retrieve( l2.Id );
			Assert.AreEqual( l2.Name, l1.Name, "Name not updated!" );
			Assert.AreEqual( l2.SenderAddress, l1.SenderAddress, "SenderAddress not updated!" );
			// delete
			l2.Remove();
			// verify delete by counting the number of rows
			SqlBuilder sb = new SqlBuilder( StatementType.Count, typeof(MailingList) );
			sb.AddConstraint( Operator.Equals, "Id", l1.Id );
			SqlResult sr = Broker.Execute( sb.GetStatement( true ) );
			Assert.AreEqual( 0, sr.Count, "Object not removed" );
		}

		/// <summary>
		/// Simple method for looping the CRUD test case. While it is disabled in itself, the
		/// thread safety tests depend on it being here.
		/// </summary>
		[Test]
		[Ignore( "This test case is a template for simple performance tests." )]
		public void TestCRUD_X()
		{
			CRUD( 1000 ); // modify the parameter to change the number of loops executed 
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
			                                              sf.GetTableName( "List" ), sf.GetStatementTerminator() ) );
			Assert.IsNotNull( sr );
			Assert.AreEqual( 0, sr.ErrorCode );
			Assert.IsNotNull( sr.Rows );
			Assert.AreEqual( 1, sr.Rows.Count );
			object[] row = (object[]) sr.Rows[ 0 ];
			int expected = Convert.ToInt32( row[ 0 ] );
			// verify StatementType.Count retrieval
			SqlBuilder sb = new SqlBuilder();
			sr = Broker.Execute( sb.GetStatement( StatementType.Count, typeof(MailingList) ) );
			Assert.AreEqual( expected, sr.Count );
		}

		/// <summary>
		/// Verify that selecting a list of a given type produces correct results.
		/// </summary>
		[Test]
		public void TestTypeInstanceRetrieval()
		{
			MailingList ml = MailingList.Retrieve( "info-sender@foobar.org" );
			Assert.IsNotNull( ml );
			Assert.AreEqual( "info-sender@foobar.org", ml.SenderAddress );
		}

		/// <summary>
		/// Verify that selecting a list of a given type produces correct results.
		/// </summary>
		[Test]
		public void TestTypeListRetrieval()
		{
			GentleSqlFactory sf = Broker.GetSqlFactory();
			// first, select the number of expected entries
			SqlBuilder sb = new SqlBuilder( StatementType.Count, typeof(MailingList) );
			SqlResult sr = Broker.Execute( sb.GetStatement( true ) );
			int expected = sr.Count;
			// verify list retrieval (entire table)
			IList lists = MailingList.ListAll;
			Assert.IsNotNull( lists );
			Assert.AreEqual( expected, lists.Count );
		}

		/// <summary>
		/// Verify that selecting a list can be constrained to return a maximum number of rows. 
		/// </summary>
		[Test]
		public void TestRowLimitRetrieval()
		{
			GentleSqlFactory sf = Broker.GetSqlFactory();
			// first verify that data set is ok for test
			IList lists = MailingList.ListAll;
			Assert.IsNotNull( lists, "Test case invalid if row count is below 3." );
			Assert.IsTrue( lists.Count >= 3, "Test case invalid if row count is below 3." );
			// try with 1 row limit
			SqlBuilder sb = new SqlBuilder( StatementType.Select, typeof(MailingList) );
			sb.SetRowLimit( 1 );
			SqlResult sr = Broker.Execute( sb.GetStatement( true ) );
			Assert.IsTrue( sr.ErrorCode == 0 && sr.RowsContained > 0, "No rows were selected." );
			Assert.AreEqual( 1, sr.RowsContained, "Result set was not limited." );
			// try again this time with 2 rows
			sb = new SqlBuilder( StatementType.Select, typeof(MailingList) );
			sb.SetRowLimit( 2 );
			sr = Broker.Execute( sb.GetStatement( true ) );
			Assert.IsTrue( sr.ErrorCode == 0 && sr.RowsContained > 0, "No rows were selected." );
			Assert.AreEqual( 2, sr.RowsContained, "Result set was not limited." );
		}

		/// <summary>
		/// Verify that paging using SqlResult.Next/Previous works.
		/// </summary>
		[Test]
		public void TestPaging()
		{
			GentleSqlFactory sf = Broker.GetSqlFactory();
			if( sf.HasCapability( Capability.Paging ) )
			{
				// first verify that data set is ok for test
				IList lists = MailingList.ListAll;
				Assert.IsNotNull( lists, "Test case invalid if row count is below 3." );
				Assert.IsTrue( lists.Count >= 3, "Test case invalid if row count is below 3." );
				// try with 1 row limit
				SqlBuilder sb = new SqlBuilder( StatementType.Select, typeof(MailingList) );
				sb.SetRowLimit( 1 );
				SqlResult sr = Broker.Execute( sb.GetStatement( true ) );
				// verify result
				Assert.IsTrue( sr.ErrorCode == 0 && sr.RowsContained > 0, "No rows were selected." );
				Assert.AreEqual( 1, sr.RowsContained, "Result set 1 was not limited." );
				MailingList ml = ObjectFactory.GetInstance( typeof(MailingList), sr ) as MailingList;
				Assert.AreEqual( (lists[ 0 ] as MailingList).Id, ml.Id, "Wrong row(s) retrieved." );
				// get next result
				sr = sr.Next();
				Assert.IsTrue( sr.ErrorCode == 0 && sr.RowsContained > 0, "No rows were selected." );
				Assert.AreEqual( 1, sr.RowsContained, "Result set 2 was not limited." );
				ml = ObjectFactory.GetInstance( typeof(MailingList), sr ) as MailingList;
				Assert.AreEqual( (lists[ 1 ] as MailingList).Id, ml.Id, "Wrong row(s) retrieved." );
				// get next result
				sr = sr.Next();
				Assert.IsTrue( sr.ErrorCode == 0 && sr.RowsContained > 0, "No rows were selected." );
				Assert.AreEqual( 1, sr.RowsContained, "Result set 3 was not limited." );
				ml = ObjectFactory.GetInstance( typeof(MailingList), sr ) as MailingList;
				Assert.AreEqual( (lists[ 2 ] as MailingList).Id, ml.Id, "Wrong row(s) retrieved." );
				// try the previous again
				sr = sr.Previous();
				Assert.IsTrue( sr.ErrorCode == 0 && sr.RowsContained > 0, "No rows were selected." );
				Assert.AreEqual( 1, sr.RowsContained, "Result set 4 was not limited." );
				ml = ObjectFactory.GetInstance( typeof(MailingList), sr ) as MailingList;
				Assert.AreEqual( (lists[ 1 ] as MailingList).Id, ml.Id, "Wrong row(s) retrieved." );

				//Try Page Numbers..
				sr = sr.Page( 2 );
				Assert.IsTrue( sr.ErrorCode == 0 && sr.RowsContained > 0, "No rows were selected." );
				Assert.AreEqual( 1, sr.RowsContained, "Result set 5 was not limited." );
				ml = ObjectFactory.GetInstance( typeof(MailingList), sr ) as MailingList;
				Assert.AreEqual( (lists[ 1 ] as MailingList).Id, ml.Id, "Wrong row(s) retrieved." );

				sr = sr.Page( 1 );
				Assert.IsTrue( sr.ErrorCode == 0 && sr.RowsContained > 0, "No rows were selected." );
				Assert.AreEqual( 1, sr.RowsContained, "Result set 6 was not limited." );
				ml = ObjectFactory.GetInstance( typeof(MailingList), sr ) as MailingList;
				Assert.AreEqual( (lists[ 0 ] as MailingList).Id, ml.Id, "Wrong row(s) retrieved." );

				sr = sr.Next();
				Assert.IsTrue( sr.ErrorCode == 0 && sr.RowsContained > 0, "No rows were selected." );
				Assert.AreEqual( 1, sr.RowsContained, "Result set 7 was not limited." );
				ml = ObjectFactory.GetInstance( typeof(MailingList), sr ) as MailingList;
				Assert.AreEqual( (lists[ 1 ] as MailingList).Id, ml.Id, "Wrong row(s) retrieved." );

				sr = sr.Page( 3 );
				Assert.IsTrue( sr.ErrorCode == 0 && sr.RowsContained > 0, "No rows were selected." );
				Assert.AreEqual( 1, sr.RowsContained, "Result set 8 was not limited." );
				ml = ObjectFactory.GetInstance( typeof(MailingList), sr ) as MailingList;
				Assert.AreEqual( (lists[ 2 ] as MailingList).Id, ml.Id, "Wrong row(s) retrieved." );

				//Test for Page 0 - should return first page
				sr = sr.Page( 0 );
				Assert.IsTrue( sr.ErrorCode == 0 && sr.RowsContained > 0, "No rows were selected." );
				Assert.AreEqual( 1, sr.RowsContained, "Result set 9 was not limited." );
				ml = ObjectFactory.GetInstance( typeof(MailingList), sr ) as MailingList;
				Assert.AreEqual( (lists[ 0 ] as MailingList).Id, ml.Id, "Wrong row(s) retrieved." );

				//Test for invalid page - should return no rows
				sr = sr.Page( 1 + (lists.Count / sr.Statement.RowLimit) );
				Assert.IsTrue( sr.ErrorCode == 0 && sr.RowsContained == 0 );
			}
		}

		/// <summary>
		/// Verify that element ordering works when selecting multiple rows.
		/// </summary>
		[Test]
		public void TestOrderByRetrieval()
		{
			GentleSqlFactory sf = Broker.GetSqlFactory();
			// first verify that data set is ok for test
			SqlBuilder sb = new SqlBuilder( StatementType.Select, typeof(MailingList) );
			// get ascending
			sb.AddOrderByField( true, "SenderAddress" );
			SqlResult sr = Broker.Execute( sb.GetStatement( true ) );
			Assert.IsTrue( sr.ErrorCode == 0 && sr.RowsContained == MailingList.ListAll.Count,
			               "Wrong number of rows were selected." );
			IList lists = ObjectFactory.GetCollection( typeof(MailingList), sr );
			Assert.IsNotNull( lists, "Test case invalid if row count is not 3." );
			Assert.IsTrue( lists.Count == 3, "Test case invalid if row count is not 3." );
			l1 = lists[ 0 ] as MailingList;
			l2 = lists[ 2 ] as MailingList;
			Assert.IsTrue( l1.SenderAddress.StartsWith( "ann" ), "Test case invalid if row order is wrong." );
			Assert.IsTrue( l2.SenderAddress.StartsWith( "inf" ), "Test case invalid if row order is wrong." );
			// now fetch the reverse ordered list
			sb = new SqlBuilder( StatementType.Select, typeof(MailingList) );
			sb.AddOrderByField( false, "SenderAddress" );
			sr = Broker.Execute( sb.GetStatement( true ) );
			Assert.IsTrue( sr.ErrorCode == 0 && sr.RowsContained == MailingList.ListAll.Count,
			               "Wrong number of rows were selected." );
			IList lists2 = ObjectFactory.GetCollection( typeof(MailingList), sr );
			l1 = lists2[ 0 ] as MailingList;
			l2 = lists2[ 2 ] as MailingList;
			Assert.IsTrue( l1.SenderAddress.StartsWith( "inf" ), "Result set was in wrong order." );
			Assert.IsTrue( l2.SenderAddress.StartsWith( "ann" ), "Result set was in wrong order." );
		}

		/// <summary>
		/// Verify that foreign keys can be resolved and used to select a list of objects.
		/// </summary>
		[Test]
		public void TestOtherTypeListRetrieval()
		{
			GentleSqlFactory sf = Broker.GetSqlFactory();
			// select all lists
			IList lists = MailingList.ListAll;
			foreach( MailingList list in lists )
			{
				// get the expected member count
				SqlBuilder sb = new SqlBuilder( StatementType.Count, typeof(Member) );
				sb.AddConstraint( Operator.Equals, "ListId", list.Id );
				SqlResult sr = Broker.Execute( sb.GetStatement( true ) );
				int expected = sr.Count;
				// verify list retrieval (entire table)
				IList members = list.Members;
				Assert.IsNotNull( members );
				Assert.AreEqual( expected, members.Count );
			}
		}

		// [Ignore("Incomplete, but does execise parts of the framework")]
		[Test]
		public void TestForeignKeyRetrieval()
		{
			IList lists = MailingList.ListAll;
			foreach( MailingList ml in lists )
			{
				IList members = ml.ListMembersBySenderAddress;
			}
		}

		/// <summary>
		/// Verify the Operator.Like constraint.
		/// </summary>
		[Test]
		public void TestCustomListUsingLikeConstraint()
		{
			GentleSqlFactory sf = Broker.GetSqlFactory();
			// first, select the number of expected entries
			SqlBuilder sb = new SqlBuilder( StatementType.Count, typeof(MailingList) );
			sb.AddConstraint( Operator.Like, "SenderAddress", "%.com" );
			SqlResult sr = Broker.Execute( sb.GetStatement( true ) );
			int expected = sr.Count;
			// verify list retrieval 
			IList lists = MailingList.ListByDomain( ".com" );
			Assert.IsNotNull( lists );
			Assert.AreEqual( expected, lists.Count );
		}

		/// <summary>
		/// Verify that the analyzer also works with views.
		/// </summary>
		[Test, Ignore("The class MailingList currently maps directly to the List table. View analysis isn't tested.")]
		public void TestAnalyzerWithView_ColumnTypeInformation()
		{
			try
			{
				if( Broker.SessionBroker.Provider.Name == "SQLServer" )
				{
					// check that string field type was set correctly by analyzer
					ObjectMap om = ObjectFactory.GetMap( Broker.SessionBroker, typeof(MailingList) );
					FieldMap fm = om.GetFieldMap( "SenderAddress" );
					Assert.IsTrue( fm.DbType != -1, "DbType was not set" );
				}
			}
			catch
			{
				Assert.Fail( "A database view called MailingList is required for this test." );
			}
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

		[Test]
		public void TestGentleListNoParent()
		{
			IList dblist = Broker.RetrieveList( typeof(MailingList) );
			GentleList list = new GentleList( typeof(MailingList) );
			Assert.AreEqual( dblist.Count, list.Count, "GentleList did not retrieve same object count as Broker.RetrieveList" );
			// verify backwards compatibility (i.e. that the list performs object uniqing for items added more than once)
			Broker.RetrieveList( typeof(MailingList), list );
			Assert.AreEqual( dblist.Count, list.Count, "GentleList does not perform object uniqing as expected." );
		}

		[Test]
		public void TestTransaction()
		{
			// create some variables to work with
			l1 = MailingList.Retrieve( 1 ); // reuse existing list
			Member member = new Member( l1.Id, "Trans", "fish@tank.com" );
			// create and populate the transaction
			Transaction t = new Transaction();
			t.Update( l1 );
			t.Persist( member );
			t.Commit();
			// try updates and deletes
			member.Name = "Action";
			t = new Transaction();
			t.Update( member );
			t.Remove( member );
			t.Commit();
		}

		[Test]
		public void TestTransactionWithNestedCalls()
		{
			Transaction transaction = new Transaction();
			// create some variables to work with
			try
			{
				IList lists = MailingList.ListAll;
				foreach( MailingList list in lists )
				{
					Key key = Key.GetKey( true, list );
					IList members = transaction.RetrieveList( typeof(Member), key, null );
					foreach( Member member in members )
					{
						transaction.Persist( member );
					}
					transaction.Persist( list );
				}
				transaction.Commit();
			}
			catch( Exception e )
			{
				string x = e.Message;
				transaction.Rollback();
				throw;
			}
		}

		[Test, Ignore( "This test requires manual checking of the log file for a provoked error." )]
		public void TestTransaction_DisposeFailure()
		{
			// create a new transaction
			Transaction t = new Transaction();
			// forget to call Commit or Rollback/Dispose
			t = null;
			GC.Collect();
			// now go check the log file - there should be a single "Failure to call Dispose on Transaction" message 
		}

		[Test, Ignore( "This test requires manual checking of the log file for a provoked error." )]
		public void TestTransaction_DisposeProperly()
		{
			// create a new transaction and dispose of it again
			using( Transaction t = new Transaction() )
			{
			}
			GC.Collect();
			// now go check the log file - there should be no "Failure to call Dispose on Transaction" message.
		}

		/// <summary>
		/// Test automatic DataView generation using objects as data source.
		/// </summary>
		[Test]
		public void TestObjectViewGeneration()
		{
			DataView dv = MailingList.ViewAll;
			// TODO add stuff to validate the contents of the view
		}

		/// <summary>
		/// Test automatic DataView generation using an SqlResult as data source.
		/// </summary>
		[Test]
		public void TestSqlResultViewGeneration()
		{
			SqlBuilder sb = new SqlBuilder( StatementType.Select, typeof(MailingList) );
			SqlResult sr = sb.GetStatement().Execute();
			DataView dv = ObjectView.GetDataView( sr );
			// TODO add stuff to validate the contents of the view
		}

		[Test]
		public void TestSimpleSql()
		{
			GentleSqlFactory sf = Broker.GetSqlFactory();
			string sql = String.Format( "select count(*) as RecordCount from {0}{1}",
			                            sf.GetTableName( "List" ), sf.GetStatementTerminator() );
			SqlBuilder sb = new SqlBuilder();
			SqlStatement stmt = sb.GetStatement( sql, StatementType.Select, typeof(MailingList) );
			Assert.IsNotNull( stmt, "stmt is null" );
			SqlResult sr = Broker.Execute( stmt );
			Assert.IsNotNull( sr );
			Assert.AreEqual( 0, sr.ErrorCode );
			Assert.IsNotNull( sr.Rows.Count );
			Assert.AreEqual( 1, sr.Rows.Count );
			object[] row = (object[]) sr.Rows[ 0 ];
			int expected = Convert.ToInt32( row[ 0 ] );
			// verify StatementType.Count retrieval
			sr = Broker.Execute( sb.GetStatement( StatementType.Count, typeof(MailingList) ) );
			Assert.AreEqual( expected, sr.Count );
		}

		[Test]
		public void TestComplexSql()
		{
			if( Broker.SessionBroker.ProviderName == "SQLServer" )
			{
				string sql = "select * from list, listmember where list.listid = listmember.listid order by list.listid, memberid";
				SqlBuilder sb = new SqlBuilder();
				SqlStatement stmt = sb.GetStatement( sql, StatementType.Select, typeof(MailingList) );
				SqlResult sr = Broker.Execute( stmt );
				Assert.IsNotNull( sr );
				Assert.IsNotNull( sr.Rows );
				Assert.AreEqual( 4, sr.Rows.Count, "Complex Sql Error " );
			}
		}

		[Test]
		public void TestListAll()
		{
			IList list2 = MailingList.ListAll;
			Assert.IsTrue( list2.Count == 3, "number is error" );
		}

		[Test]
		public void TestExecuteSql()
		{
			if( Broker.SessionBroker.ProviderName == "SQLServer" )
			{
				string sql = "select * from list, listmember where list.listid = listmember.listid order by list.listid, memberid";
				SqlResult sr = Broker.Execute( sql, StatementType.Select, typeof(MailingList) );
				Assert.IsNotNull( sr );
				Assert.IsNotNull( sr.Rows );
				Assert.AreEqual( 4, sr.RowsContained, "Complex Sql Error" );
			}
		}

		[Test]
		public void TestMultiBroker()
		{
			if( GentleSettings.DefaultProviderConnectionString.IndexOf( "127.0.0.1" ) != -1 )
			{
				// use modified connection string
				string connStr = GentleSettings.DefaultProviderConnectionString.Replace( "127.0.0.1", "(local)" );
				PersistenceBroker pb = new PersistenceBroker( GentleSettings.DefaultProviderName, connStr );
				// fetch list
				IList pbList = pb.RetrieveList( typeof(MailingList) );
				Assert.AreEqual( Broker.RetrieveList( typeof(MailingList) ).Count, pbList.Count, "Not same result." );
				// check that connstr remains same when using SqlBuilder with custom broker
				SqlBuilder sb = new SqlBuilder( pb, StatementType.Select, typeof(MailingList) );
				SqlStatement stmt = sb.GetStatement( true );
				SqlResult sr = stmt.Execute();
				Assert.AreEqual( sr.SessionBroker.Provider.ConnectionString, connStr, "ConnStr not preserved." );
			}
		}

		[Test]
		public virtual void TestSqlOverride()
		{
			if( ! Broker.ProviderName.Equals( "PostgreSQL" ) )
			{
				SqlBuilder sql = new SqlBuilder( StatementType.Select, typeof(MailingList) );
				SqlStatement statement = sql.GetStatement( true );
				ObjectMap map = ObjectFactory.GetMap( null, typeof(MailingList) );
				statement.Command.CommandText = String.Format( "select * from {0}", map.TableName );
				SqlResult result = statement.Execute();
				IList list = ObjectFactory.GetCollection( typeof(MailingList), result );
				Assert.AreEqual( 3, list.Count );
			}
		}

		[Test]
		public virtual void TestNullNotEquals()
		{
			SqlBuilder sb = new SqlBuilder( StatementType.Select, typeof(Member) );
			sb.AddConstraint( Operator.NotEquals, "MemberName", null );
			SqlStatement stmt = sb.GetStatement( true );
		}
	}
}
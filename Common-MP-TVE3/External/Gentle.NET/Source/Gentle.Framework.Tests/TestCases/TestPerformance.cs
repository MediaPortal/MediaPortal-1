/*
 * Performance tests
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: TestPerformance.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Collections;
using Gentle.Common;
using NUnit.Framework;

namespace Gentle.Framework.Tests
{

	#region Helper classes for computing run times
	internal class Probe
	{
		//public const string fmtTime = "[-][d.]hh:mm:ss[.ff]";
		protected DateTime[] marks;
		protected int[] operations;
		protected int mark;

		public Probe( int marks, bool start )
		{
			this.marks = new DateTime[Math.Max( 2, marks )];
			operations = new int[Math.Max( 2, marks )];
			mark = -1;
			if( start )
			{
				Mark( 0 );
			}
		}

		public Probe( bool start ) : this( 3, start )
		{
		}

		public void Start()
		{
			mark = -1;
			Mark( 0 );
		}

		public int Mark( int ops )
		{
			DateTime now = DateTime.Now;
			marks[ ++mark ] = now;
			operations[ mark ] = ops;
			return mark;
		}

		public int End( int ops )
		{
			return Mark( ops );
		}

		public DateTime this[ int mark ]
		{
			get { return marks[ mark ]; }
		}

		public Interval Elapsed
		{
			get { return Diff( 0, mark ); }
		}

		public Interval Diff( int firstMark, int secondMark )
		{
			long timeDiff = marks[ secondMark ].Ticks - marks[ firstMark ].Ticks;
			return new Interval( timeDiff, operations[ secondMark ] );
			// int operationsDiff = operations[ secondMark ] - operations[ firstMark ];
			// return new Interval( timeDiff, operationsDiff );
		}

		public static string ToString( long ticks )
		{
			return new TimeSpan( ticks ).ToString();
		}

		public override string ToString()
		{
			return Elapsed.ToString();
		}

		public static TimeSpan AbsDiff( TimeSpan ts1, TimeSpan ts2 )
		{
			if( ts1.Ticks > ts2.Ticks )
			{
				return ts1.Subtract( ts2 );
			}
			else
			{
				return ts2.Subtract( ts1 );
			}
		}
	}

	internal class Interval
	{
		protected long ticks;
		protected int operations;

		public Interval( long ticks, int operations )
		{
			this.ticks = ticks;
			this.operations = operations;
		}

		public static Interval operator -( Interval i1, Interval i2 )
		{
			return new Interval( i1.Ticks - i2.Ticks, i1.Operations - i2.Operations );
		}

		public static Interval operator +( Interval i1, Interval i2 )
		{
			return new Interval( i1.Ticks + i2.Ticks, i1.Operations + i2.Operations );
		}

		public static Interval operator *( Interval i1, int scale )
		{
			return new Interval( i1.Ticks * scale, i1.Operations * scale );
		}

		public static Interval operator /( Interval i1, int scale )
		{
			return new Interval( i1.Ticks / scale, i1.Operations / scale );
		}

		public static string ToString( long ticks )
		{
			return new TimeSpan( ticks ).ToString();
		}

		public override string ToString()
		{
			return ToString( ticks );
		}

		public long Ticks
		{
			get { return ticks; }
		}
		public int Operations
		{
			get { return operations; }
		}
		public long TicksPerOperation
		{
			get { return ticks / operations; }
		}
		public string TimePerOperation
		{
			get { return ToString( ticks / operations ); }
		}
	}
	#endregion

	/// <summary>
	/// This class holds test cases for exercising the Gentle framework using the 
	/// MailingList and Member classes. The database must have been created and
	/// populated with the supplied test data for the tests to work.
	/// </summary>
	[TestFixture] //, Ignore("Performance tests are disabled by default.")]
	public class TestPerformance
	{
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

		public void Insert( int count )
		{
			MailingList ml;
			for( int i = 0; i < count; i++ )
			{
				ml = new MailingList( "xxx" + i, "xxx" + i + "@xxx.xxx" );
				ml.Persist();
			}
		}

		[TearDown]
		public void CleanUp()
		{
			SqlBuilder sb = new SqlBuilder( StatementType.Delete, typeof(MailingList) );
			sb.AddConstraint( Operator.GreaterThan, "Id", 3 );
			Broker.Execute( sb.GetStatement( true ) );
		}

		/// <summary>
		/// Test case for verifying the four basic statement types (CRUD for Create, Read, Update, 
		/// Delete). This method executes 6 statements (of which the last select returns no data).
		/// </summary>
		[Test]
		public void TestCRUD()
		{
			MailingList l1, l2;
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

		[Test]
		public void TestCRUDOverhead()
		{
			Probe probe = new Probe( 3, true );
			CRUD( 1 );
			probe.Mark( 1 );
			CRUD( 1 );
			probe.End( 1 );
			Check.LogInfo( LogCategories.General, "=================[ CRUD ]=================" );
			Check.LogInfo( LogCategories.General, "First       : {0}", probe.Diff( 0, 1 ) );
			Check.LogInfo( LogCategories.General, "Second      : {0}", probe.Diff( 1, 2 ) );
			Check.LogInfo( LogCategories.General, "Total       : {0}", probe );
			Check.LogInfo( LogCategories.General, "Overhead    : {0}", probe.Diff( 0, 1 ) - probe.Diff( 1, 2 ) );
		}

		[Test]
		public void TestInsertOverhead()
		{
			Probe probe = new Probe( 3, true );
			Insert( 1 );
			probe.Mark( 1 );
			Insert( 1 );
			probe.End( 1 );
			Check.LogInfo( LogCategories.General, "=================[ Insert ]=================" );
			Check.LogInfo( LogCategories.General, "First       : {0}", probe.Diff( 0, 1 ) );
			Check.LogInfo( LogCategories.General, "Second      : {0}", probe.Diff( 1, 2 ) );
			Check.LogInfo( LogCategories.General, "Total       : {0}", probe );
			Check.LogInfo( LogCategories.General, "Overhead    : {0}", probe.Diff( 0, 1 ) - probe.Diff( 1, 2 ) );
		}

		[Test]
		public void TestInsertBatch1000()
		{
			// skip Jet/Access - simply too slow for my temper..
			if( Broker.ProviderName != "Jet" )
			{
				Probe probe = new Probe( 3, true );
				Insert( 1 );
				probe.Mark( 1 );
				Insert( 1000 );
				probe.End( 1000 );
				Check.LogInfo( LogCategories.General, "=================[ Insert Batch ]=================" );
				Check.LogInfo( LogCategories.General, "First       : {0}", probe.Diff( 0, 1 ) );
				Check.LogInfo( LogCategories.General, "Next 1.000  : {0}", probe.Diff( 1, 2 ) );
				Check.LogInfo( LogCategories.General, "Time/Op     : {0}", probe.Diff( 1, 2 ).TimePerOperation );
			}
		}

		[Test, Ignore( "Takes time." )]
		public void TestInsertBatch10000()
		{
			Probe probe = new Probe( 3, true );
			Insert( 1 );
			probe.Mark( 1 );
			Insert( 10000 );
			probe.End( 10000 );
			Check.LogInfo( LogCategories.General, "=================[ Insert Batch ]=================" );
			Check.LogInfo( LogCategories.General, "First       : {0}", probe.Diff( 0, 1 ) );
			Check.LogInfo( LogCategories.General, "Next 10.000 : {0}", probe.Diff( 1, 2 ) );
			Check.LogInfo( LogCategories.General, "Time/Op     : {0}", probe.Diff( 1, 2 ).TimePerOperation );
		}

		[Test]
		public void TestSelectOverhead()
		{
			Probe probe = new Probe( 3, true );
			MailingList ml = MailingList.Retrieve( 1 );
			probe.Mark( 1 );
			ml = MailingList.Retrieve( 1 );
			probe.End( 1 );
			Check.LogInfo( LogCategories.General, "=================[ Select ]=================" );
			Check.LogInfo( LogCategories.General, "First       : {0}", probe.Diff( 0, 1 ) );
			Check.LogInfo( LogCategories.General, "Second      : {0}", probe.Diff( 1, 2 ) );
			Check.LogInfo( LogCategories.General, "Total       : {0}", probe );
			Check.LogInfo( LogCategories.General, "Overhead    : {0}", probe.Diff( 0, 1 ) - probe.Diff( 1, 2 ) );
		}

		[Test]
		public void TestSelectBatch1000()
		{
			// skip Jet/Access - simply too slow for my temper..
			if( Broker.ProviderName != "Jet" )
			{
				Insert( 997 ); // assume first 3 exist
				Probe probe = new Probe( 3, true );
				MailingList ml = MailingList.Retrieve( 1 );
				probe.Mark( 1 );
				IList list = MailingList.ListAll;
				probe.End( list.Count );
				Check.LogInfo( LogCategories.General, "=================[ Select Batch ]=================" );
				Check.LogInfo( LogCategories.General, "First       : {0}", probe.Diff( 0, 1 ) );
				Check.LogInfo( LogCategories.General, "Next 1.000  : {0}", probe.Diff( 1, 2 ) );
				Check.LogInfo( LogCategories.General, "Time/Op     : {0}", probe.Diff( 1, 2 ).TimePerOperation );
			}
		}

		[Test, Ignore( "Takes time." )]
		public void TestSelectBatch10000()
		{
			Insert( 9997 ); // assume first 3 exist
			Probe probe = new Probe( 4, true );
			MailingList ml = MailingList.Retrieve( 1 );
			probe.Mark( 1 );
			IList list = MailingList.ListAll;
			probe.Mark( list.Count );
			SqlResult sr = Broker.Retrieve( typeof(MailingList), new Key( typeof(MailingList), true ) );
			probe.End( sr.RowsContained );
			Check.LogInfo( LogCategories.General, "=================[ Select Batch ]=================" );
			Check.LogInfo( LogCategories.General, "First       : {0}", probe.Diff( 0, 1 ) );
			Check.LogInfo( LogCategories.General, "Next 10.000 : {0}", probe.Diff( 1, 2 ) );
			Check.LogInfo( LogCategories.General, "Time/Op     : {0}", probe.Diff( 1, 2 ).TimePerOperation );
			Check.LogInfo( LogCategories.General, "Raw 10.000  : {0}", probe.Diff( 2, 3 ) );
			Check.LogInfo( LogCategories.General, "Time/RawOp  : {0}", probe.Diff( 2, 3 ).TimePerOperation );
		}

		[Test, Ignore( "Takes a lot of time, then fails due to too large result set." )]
		public void TestInsertSelectBatch()
		{
			Probe probe = new Probe( 3, true );
			Insert( 1 );
			probe.Mark( 1 );
			Insert( 100000 );
			probe.Mark( 100000 );
			Check.LogInfo( LogCategories.General, "=================[ Insert Large Batch ]=================" );
			Check.LogInfo( LogCategories.General, "First Insert: {0}", probe.Diff( 0, 1 ) );
			Check.LogInfo( LogCategories.General, "Next 100.000: {0}", probe.Diff( 1, 2 ) );
			Check.LogInfo( LogCategories.General, "Time/Op     : {0}", probe.Diff( 1, 2 ).TimePerOperation );
			probe = new Probe( 3, true );
			MailingList ml = MailingList.Retrieve( 1 );
			probe.Mark( 1 );
			IList list = MailingList.ListAll;
			probe.End( list.Count );
			Check.LogInfo( LogCategories.General, "=================[ Select Large Batch ]=================" );
			Check.LogInfo( LogCategories.General, "First Select: {0}", probe.Diff( 0, 1 ) );
			Check.LogInfo( LogCategories.General, "Next 100.000: {0}", probe.Diff( 1, 2 ) );
			Check.LogInfo( LogCategories.General, "Time/Op     : {0}", probe.Diff( 1, 2 ).TimePerOperation );
		}
	}
}
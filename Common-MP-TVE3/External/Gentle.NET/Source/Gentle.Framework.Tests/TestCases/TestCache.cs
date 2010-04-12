/*
 * Test cases
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: TestCache.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Collections;
using System.Threading;
using Gentle.Common;
using NUnit.Framework;

namespace Gentle.Framework.Tests
{
	/// <summary>
	/// This class tests various aspects of the built-in object caching 
	/// and uniqing functionality.
	/// </summary>
	[TestFixture]
	public class TestCache
	{
		private MailingList l1, l2;

		[SetUp]
		public void Init()
		{
			CacheManager.Clear();
			GentleSettings.CacheObjects = true;
		}

		[TearDown]
		public void Exit()
		{
		}

		[Test]
		public void TestCacheWeakReferenceExpiration()
		{
			l1 = MailingList.Retrieve( 1 );
			CacheManager.Clear();
			CacheManager.Insert( "mylist", l1 );
			Assert.AreEqual( 1, CacheManager.Count, "Item not added to cache." );
			l1 = null;
			GC.Collect();
			l2 = CacheManager.Get( "mylist" ) as MailingList;
			Assert.IsNull( l2, "Item was still found in the cache." );
			Assert.AreEqual( 0, CacheManager.Count, "Item not removed from the cache." );
		}

		[Test]
		public void TestCacheRemove()
		{
			l1 = MailingList.Retrieve( 1 );
			CacheManager.Clear();
			CacheManager.Insert( "mylist", l1 );
			Assert.AreEqual( 1, CacheManager.Count, "Item not added to cache." );
			CacheManager.Remove( "mylist" );
			l1 = CacheManager.Get( "mylist" ) as MailingList;
			Assert.IsNull( l1, "Item was still found in the cache." );
			Assert.AreEqual( 0, CacheManager.Count, "Item not removed from the cache." );
		}

		[Test]
		public void TestObjectUniqing()
		{
			GentleSettings.SkipQueryExecution = false;
			ObjectMap map = ObjectFactory.GetMap( Broker.SessionBroker, typeof(MailingList) );
			if( map.CacheStrategy != CacheStrategy.Never )
			{
				// test without cache
				CacheManager.Clear();
				GentleSettings.CacheObjects = false;
				int cacheCount = CacheManager.Count;
				l1 = MailingList.Retrieve( 1 );
				Assert.IsTrue( CacheManager.Count == cacheCount, "Item was erroneously added to cache." );
				l2 = MailingList.Retrieve( 1 );
				Assert.AreNotSame( l1, l2, "Object references are supposed to be different." );
				Check.LogInfo( LogCategories.General, "TestObjectUniqing --- after execution (without cache):" );
				GentleStatistics.LogStatistics( LogCategories.Cache );
				// test with cache
				GentleSettings.CacheObjects = true;
				l1 = MailingList.Retrieve( 1 );
				Assert.IsTrue( CacheManager.Count == ++cacheCount, "Item was not added to cache." );
				l2 = MailingList.Retrieve( 1 );
				Assert.IsTrue( CacheManager.Count == cacheCount, "Item was added to cache again." );
				Assert.AreSame( l1, l2, "Object references are supposed to be identical." );
				Check.LogInfo( LogCategories.General, "TestObjectUniqing --- after execution (with cache)" );
				GentleStatistics.LogStatistics( LogCategories.Cache );
			}
		}

		[Test]
		public void TestSkipQueryExecution()
		{
			GentleSettings.CacheStatements = true;
			GentleSettings.CacheObjects = true;
			GentleSettings.SkipQueryExecution = true;
			ObjectMap map = ObjectFactory.GetMap( Broker.SessionBroker, typeof(MailingList) );
			if( map.CacheStrategy != CacheStrategy.Never )
			{
				CacheManager.Clear();
				l1 = MailingList.Retrieve( 1 );
				Assert.IsTrue( CacheManager.Count == 2, "Expected both query result info and object in cache." );
				for( int i = 0; i < 10; i++ )
				{
					l1 = MailingList.Retrieve( 1 );
				}
				Assert.IsTrue( CacheManager.Count == 2, "Unexpected change in cache contents." );
				// strangely enough, SkippedQueries is only 9 when executing all tests, but 
				// 10 when this test is executed alone?! 
				Assert.IsTrue( GentleStatistics.SkippedQueries >= 9,
				               String.Format( "Unexpected number of skipped queries (was: {0} expected: 10).", GentleStatistics.SkippedQueries ) );
			}
		}

		private static void RetrieveSpecificList()
		{
			MailingList ml = MailingList.Retrieve( 1 );
		}

		[Test]
		public void TestUniqingScope()
		{
			GentleSettings.CacheObjects = true;
			GentleSettings.SkipQueryExecution = false;
			GentleSettings.UniqingScope = UniqingScope.Thread;
			CacheManager.Clear();
			ObjectMap map = ObjectFactory.GetMap( Broker.SessionBroker, typeof(MailingList) );
			if( map.CacheStrategy != CacheStrategy.Never )
			{
				// access in this thread (populates cache)
				int cacheCount1stThread = CacheManager.Count;
				l1 = MailingList.Retrieve( 1 );
				Assert.IsTrue( CacheManager.Count == ++cacheCount1stThread, "Item was not added to cache." );
				l2 = MailingList.Retrieve( 1 );
				Assert.IsTrue( CacheManager.Count == cacheCount1stThread, "Item was added to cache again." );
				Assert.AreSame( l1, l2, "Object references are supposed to be identical." );
				Check.LogInfo( LogCategories.General, "TestUniqingScope --- after execution (thread {0}):", SystemSettings.ThreadIdentity );
				GentleStatistics.LogStatistics( LogCategories.Cache );
				// access same type in separate thread
				Thread thread = new Thread( RetrieveSpecificList );
				// remember the threads id (check for name to match SystemSettings.ThreadIdentity behavior)
				string threadId = thread.Name != null ? thread.Name : thread.GetHashCode().ToString();
				int cacheCount2ndThread = CacheManager.GetCount( threadId );
				thread.Start();
				thread.Join(); // wait for completion
				// we should see only a mailinglist being added to the cache
				Assert.AreEqual( cacheCount1stThread, CacheManager.Count, "Item was added to wrong cache store." );
				Assert.AreEqual( ++cacheCount2ndThread, CacheManager.GetCount( threadId ), "Item was not added to cache for 2nd thread." );
				Check.LogInfo( LogCategories.General, "TestUniqingScope --- after execution (thread {0}):", thread.GetHashCode() );
				GentleStatistics.LogStatistics( LogCategories.Cache );
				// under normal circumstances we should make sure to clean items belonging to the 
				// terminated thread; lets test that this works too :)
				CacheManager.Clear( threadId );
				Assert.AreEqual( --cacheCount2ndThread, CacheManager.GetCount( threadId ), "Items were not properly flushed from the cache." );
			}
		}

		[Test]
		public void TestQueryResultInvalidationAndUniqing()
		{
			GentleSettings.CacheObjects = true;
			GentleSettings.SkipQueryExecution = false;
			GentleSettings.UniqingScope = UniqingScope.Thread;
			CacheManager.Clear();
			ObjectMap map = ObjectFactory.GetMap( Broker.SessionBroker, typeof(MailingList) );
			if( map.CacheStrategy != CacheStrategy.Never )
			{
				// read all objects before insert
				IList before = Broker.RetrieveList( typeof(MailingList) );
				Assert.IsTrue( before.Count > 0, "Unable to run test without data." );
				// insert a new instance
				MailingList ml = new MailingList( "test", "test@test.com" );
				ml.Persist();
				// read all objects after insert
				IList after = Broker.RetrieveList( typeof(MailingList) );
				Assert.IsTrue( after.Contains( ml ), "The object just persisted not included in result" +
				                                     " (cache not invalidated and/or object not added to cache)." );
				// compare lists
				Assert.AreEqual( before.Count + 1, after.Count, "Invalid cache result." );
				// re-read single object
				MailingList mlread = MailingList.Retrieve( ml.Id );
				Assert.AreSame( ml, mlread, "Uniqing broken: new instance returned." );
			}
		}

		[Test]
		public void TestCacheNeverStrategy()
		{
			GentleSettings.CacheObjects = true;
			GentleSettings.SkipQueryExecution = true;
			GentleSettings.UniqingScope = UniqingScope.Thread;
			CacheManager.Clear();

			ObjectMap map = ObjectFactory.GetMap( Broker.SessionBroker, typeof(MailingList) );
			map.CacheStrategy = CacheStrategy.Never;

			int beforeCount = CacheManager.Count;

			MailingList.Retrieve( 1 );
			MailingList.Retrieve( 2 );
			Assert.AreEqual( beforeCount, CacheManager.Count );

			MailingList ml = new MailingList( "test", "test@test.com" );
			ml.Persist();
			Assert.AreEqual( beforeCount, CacheManager.Count );

			ml.Remove();
			Assert.AreEqual( beforeCount, CacheManager.Count );
			Assert.AreEqual( 0, GentleStatistics.CacheHits );
			Assert.AreEqual( 0, GentleStatistics.CacheMisses );
			Assert.AreEqual( beforeCount, GentleStatistics.CacheSize );
		}

		[Test, Ignore( "Takes quite a while so disabled by default" )]
		public void TestCachePerformance_Clear()
		{
			CacheManager.Clear();
			Probe probe = new Probe( 3, true );
			for( int i = 10000; i < 20000; i++ )
			{
				CacheManager.Insert( i.ToString(), i );
			}
			probe.Mark( 1 );
			CacheManager.Clear();
			probe.End( 1 );
			Check.LogInfo( LogCategories.General, "Insert 10000: {0}", probe.Diff( 0, 1 ) );
			Check.LogInfo( LogCategories.General, "Clear       : {0}", probe.Diff( 1, 2 ) );
			Check.LogInfo( LogCategories.General, "Total       : {0}", probe );
		}

		[Test, Ignore( "Takes quite a while so disabled by default" )]
		public void TestCachePerformance_Remove()
		{
			CacheManager.Clear();
			Probe probe = new Probe( 3, true );
			for( int i = 10000; i < 20000; i++ )
			{
				CacheManager.Insert( i.ToString(), i );
			}
			probe.Mark( 1 );
			for( int i = 10000; i < 20000; i++ )
			{
				CacheManager.Remove( i.ToString() );
			}
			probe.End( 1 );
			Check.LogInfo( LogCategories.General, "Insert 10000: {0}", probe.Diff( 0, 1 ) );
			Check.LogInfo( LogCategories.General, "Remove 10000: {0}", probe.Diff( 1, 2 ) );
			Check.LogInfo( LogCategories.General, "Total       : {0}", probe );
		}
	}
}
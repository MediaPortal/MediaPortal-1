/*
 * Test cases
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: TestProviderFactory.cs 1232 2008-03-14 05:36:00Z mm $
 */
using Gentle.Common;
using NUnit.Framework;

namespace Gentle.Framework.Tests
{
	/// <summary>
	/// Summary description for TestProviderFactory.
	/// </summary>
	[TestFixture]
	public class TestProviderFactory
	{
		[SetUp]
		public void Init()
		{
			CacheManager.Clear();
			GentleSettings.CacheObjects = true;
		}

		[Test]
		public void TestMultipleProviders()
		{
			PersistenceBroker pb1 = new PersistenceBroker( "SQLServer", "first" );
			Assert.AreEqual( "first", pb1.Provider.ConnectionString );
			PersistenceBroker pb2 = new PersistenceBroker( "SQLServer", "second" );
			Assert.AreEqual( "second", pb2.Provider.ConnectionString );
			Assert.AreEqual( "first", pb1.Provider.ConnectionString );
		}

		[Test]
		public void TestCachingObjectOnly()
		{
			MailingList ml;
			CacheManager.Clear();
			// make sure only objects (and not statements or result sets) are cached
			GentleSettings.CacheObjects = true;
			GentleSettings.CacheStatements = false;
			GentleSettings.SkipQueryExecution = false;
			ObjectMap map = ObjectFactory.GetMap( null, typeof(MailingList) );
			int expected = CacheManager.Count + 1;
			ml = MailingList.Retrieve( 1 );
			Assert.AreEqual( expected, CacheManager.Count, "Incorrect cache count." );
			ml = MailingList.Retrieve( 1 );
			Assert.AreEqual( expected, CacheManager.Count, "Incorrect cache count." );
		}
	}
}
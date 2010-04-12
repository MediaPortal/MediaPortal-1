/*
 * Test cases
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: TestThreadSafety.cs 1232 2008-03-14 05:36:00Z mm $
 */
using System.Threading;
using NUnit.Framework;

namespace Gentle.Framework.Tests
{
	/// <summary>
	/// This class holds test cases that test for thread safety.
	/// </summary>
	[TestFixture]
	public class TestThreadSafety
	{
		private TestList tl1, tl2, tl3;
		private Thread t1, t2, t3;

		/// <summary>
		/// Test case initialization.
		/// </summary>
		[SetUp]
		public void Init()
		{
			tl1 = new TestList();
			tl2 = new TestList();
			tl3 = new TestList();
			t1 = new Thread( tl1.TestCRUD_X );
			t2 = new Thread( tl2.TestCRUD_X );
			t3 = new Thread( tl3.TestCRUD_X );
		}

		/// <summary>
		/// Test threaded access to the framework. 
		/// Note: this test does not really prove anything (including thread safety).
		/// </summary>
		[Test]
		[Ignore( "The value of this test case is somewhat uncertain." )]
		public void TestThreadedCRUD()
		{
			// The MySQL provider appears to starve out threads, causing this test case
			// to fail with a "Underlying socket closed." when the loop count (as defined
			// in the TestCRUD_X method) is high and the running time of the threads increases.
			if( ! Broker.ProviderName.Equals( "MySQL" ) )
			{
				t1.Start();
				t2.Start();
				t3.Start();
				while( t1.IsAlive || t2.IsAlive || t3.IsAlive )
				{
					;
				}
			}
		}

		[Test]
		public void TestThreadedAnalyser()
		{
			Thread t1, t2;

			TestReservedWords tm1, tm2;
			tm1 = new TestReservedWords();
			tm2 = new TestReservedWords();

			t1 = new Thread( tm1.TestCRUD );
			t2 = new Thread( tm2.TestCRUD );

			t1.Start();
			t2.Start();
		}
	}
}
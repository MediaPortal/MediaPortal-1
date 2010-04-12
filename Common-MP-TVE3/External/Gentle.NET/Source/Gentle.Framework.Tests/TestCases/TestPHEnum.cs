/*
 * Test cases
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: TestPHEnum.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using NUnit.Framework;

namespace Gentle.Framework.Tests
{
	[TestFixture]
	public class TestPHEnum
	{
		private PHEnum a;
		private PropertyHolder b;

		[SetUp]
		public void Init()
		{
			// make sure table is empty before we start
			SqlBuilder sb = new SqlBuilder( StatementType.Delete, typeof(PHEnum) );
			Broker.Execute( sb.GetStatement( true ) );
		}

		[TearDown]
		public void Exit()
		{
			// clean up after running tests
			SqlBuilder sb = new SqlBuilder( StatementType.Delete, typeof(PHEnum) );
			Broker.Execute( sb.GetStatement( true ) );
		}

		[Test]
		public void TestEnum()
		{
			if( Broker.Provider.GetAnalyzer() != null && GentleSettings.AnalyzerLevel != AnalyzerLevel.None )
			{
				a = new PHEnum( 0, DayOfWeek.Monday, DayOfWeek.Monday, DayOfWeek.Monday );
				// insert
				a.Persist();
				// select as PropertyHolder to get string representations
				b = PropertyHolder.Retrieve( a.Id );
				// verify select/insert
				Assert.AreEqual( "Monday", b.Name );
				Assert.AreEqual( "Monday", b.TNVarChar );
				Assert.AreEqual( "Monday", b.TNText );
				// update
				a.SetEnum( DayOfWeek.Tuesday );
				a.Persist();
				// select and verify update
				a = PHEnum.Retrieve( a.Id );
				Assert.AreEqual( DayOfWeek.Tuesday, a.AsText );
				Assert.AreEqual( DayOfWeek.Tuesday, a.AsNVarChar );
				Assert.AreEqual( DayOfWeek.Tuesday, a.AsNText );
				// clean up
				a.Remove();
			}
		}
	}
}
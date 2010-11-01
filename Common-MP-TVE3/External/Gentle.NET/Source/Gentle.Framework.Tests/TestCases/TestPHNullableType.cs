/*
 * Test cases
 * Copyright (C) 2008 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: TestPHNullableType.cs 1233 2008-03-14 06:29:09Z mm $
 */

using System;
using NUnit.Framework;

namespace Gentle.Framework.Tests
{
	[TestFixture]
	public class TestPHNullableType
	{
		private PHNullableType obj1;

		[SetUp]
		public void Init()
		{
			// make sure table is empty before we start
			SqlBuilder sb = new SqlBuilder( StatementType.Delete, typeof(PHNullableType) );
			Broker.Execute( sb.GetStatement( true ) );
		}

		[TearDown]
		public void Exit()
		{
			// clean up after running tests
			SqlBuilder sb = new SqlBuilder( StatementType.Delete, typeof(PHNullableType) );
			Broker.Execute( sb.GetStatement( true ) );
		}

		[Test]
		public void TestNullableType()
		{
			if( Broker.Provider.GetAnalyzer() != null && GentleSettings.AnalyzerLevel != AnalyzerLevel.None )
			{
				obj1 = new PHNullableType( 0, "Test", null );
				// insert
				obj1.Persist();
				// select raw data to verify that null was written
				SqlResult sr = GetRawTableData();
				Assert.AreEqual( 1, sr.RowsContained );
				Assert.IsNull( sr[ 0, "TDateTime" ], "Expected null to be written, but database contained the value {0}.", sr[ 0, "TDateTime" ] );
				// update
				obj1.NullableDate = null;
				obj1.Persist();
				// select raw data to verify that null was written
				sr = GetRawTableData();
				Assert.AreEqual( 1, sr.RowsContained );
				Assert.IsNull( sr[ 0, "TDateTime" ], "Expected null to be written, but database contained the value {0}.", sr[ 0, "TDateTime" ] );
				// select and verify update
				obj1 = PHNullableType.Retrieve( obj1.Id );
				Assert.IsNull( obj1.NullableDate, "Expected null to be read but actual value read was {0}.", obj1.NullableDate );
				Assert.AreEqual( null, obj1.NullableDate );
				// clean up
				obj1.Remove();
			}
		}

		private SqlResult GetRawTableData()
		{
			SqlBuilder sb = new SqlBuilder( StatementType.Select, typeof(PHNullableType) );
			SqlStatement stmt = sb.GetStatement( true );
			// override generated statement to make sure we execute this without any type association
			stmt = new SqlStatement( StatementType.Select, Broker.Provider.GetCommand(), stmt.Sql );
			SqlResult sr = stmt.Execute();
			return sr;
		}
	}
}
/*
 * Test cases
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: TestConstraint.cs 1232 2008-03-14 05:36:00Z mm $
 */
using System;
using NUnit.Framework;

namespace Gentle.Framework.Tests
{
	/// <summary>
	/// This class holds test cases for exercising the Gentle framework using the 
	/// MailingList and Member classes. The database must have been created and
	/// populated with the supplied test data for the tests to work.
	/// </summary>
	[TestFixture]
	public class TestConstraint
	{
		private int GetCount( SqlBuilder builder )
		{
			SqlStatement statement = builder.GetStatement();
			SqlResult result = statement.Execute();
			int count = result.Count;
			return count;
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
		}

		/// <summary>
		/// Verify the GreaterThanOrEquals constraint.
		/// </summary>
		[Test]
		public void TestGreaterThanOrEqualsConstraint()
		{
			SqlBuilder builder = new SqlBuilder( StatementType.Count, typeof(MailingList) );
			builder.AddConstraint( Operator.GreaterThanOrEquals, "Id", 3 );
			int count = GetCount( builder );
			Assert.AreEqual( 1, count );
		}

		/// <summary>
		/// Verify the LessThan constraint.
		/// </summary>
		[Test]
		public void TestLessThanConstraint()
		{
			SqlBuilder builder = new SqlBuilder( StatementType.Count, typeof(MailingList) );
			builder.AddConstraint( Operator.LessThan, "Id", 3 );
			int count = GetCount( builder );
			Assert.AreEqual( 2, count );
		}

		/// <summary>
		/// Verify range of constraints.
		/// </summary>
		[Test]
		public void TestRangeConstraint()
		{
			SqlBuilder builder = new SqlBuilder( StatementType.Count, typeof(MailingList) );
			builder.AddConstraint( Operator.GreaterThanOrEquals, "Id", 2 );
			builder.AddConstraint( Operator.LessThan, "Id", 3 );
			int count = GetCount( builder );
			Assert.AreEqual( 1, count );
		}

		/// <summary>
		/// Verify range of constraints using a custom clause.
		/// </summary>
		[Test]
		public void TestRangeConstraintCustom()
		{
			SqlBuilder builder = new SqlBuilder( StatementType.Count, typeof(MailingList) );
			builder.AddConstraint( Operator.GreaterThanOrEquals, "Id", 2 );
			GentleSqlFactory sf = Broker.GetSqlFactory();
			string clause = String.Format( "{0} < 3", "ListId" );
			builder.AddConstraint( clause );
			int count = GetCount( builder );
			Assert.AreEqual( 1, count );
		}
	}
}
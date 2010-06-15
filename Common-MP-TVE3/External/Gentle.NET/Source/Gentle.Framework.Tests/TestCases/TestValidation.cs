/*
 * Test cases
 * Copyright (C) 2005 Clayton Harbour
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: TestValidation.cs  $
 */

using System;
using NUnit.Framework;

namespace Gentle.Framework.Tests
{
	/// <summary>
	/// This class holds test cases for exercising the Gentle framework RegexValidation.
	/// </summary>
	[TestFixture]
	public class TestValidation
	{
		private Member m1;
		private MailingList list;

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
			// create an empty mailing list
			list = new MailingList( "SomeList", "some.sender@doe.com" );
			list.Persist();
		}

		[TearDown]
		public void Final()
		{
			// make sure we have only the default 4 members 
			SqlBuilder sb = new SqlBuilder( StatementType.Delete, typeof(Member) );
			sb.AddConstraint( Operator.GreaterThan, "Id", 4 );
			Broker.Execute( sb.GetStatement( true ) );
			// remove the list
			list.Remove();
		}

		/// <summary>
		/// Test that the regex validator allows good data to pass validation
		/// </summary>
		[Test]
		public void TestRegexGoodData()
		{
			// test that a `good` email address can be entered in the database
			m1 = new Member( list.Id, "John Doe", "john@doe.com" );
			// insert
			m1.Persist();
			Assert.AreEqual( m1.Name, "John Doe", "The object was not properly inserted!" );
			Assert.AreEqual( m1.Address, "john@doe.com", "The object was not properly inserted!" );
			// delete
			m1.Remove();
		}

		/// <summary>
		/// Test that the regex validator catches `bad` data.
		/// </summary>
		[Test]
		[ExpectedException( typeof(ValidationException) )]
		public void TestRegexBad()
		{
			// test that a `bad` email address throws an exception
			m1 = new Member( list.Id, "John Doe", "john-doe.com" );
			// insert
			m1.Persist();
		}

		/// <summary>
		/// Test that a required field with data passes validation.
		/// </summary>
		[Test]
		public void TestRequiredGood()
		{
			// test that an email address (required field) entered passes validation
			m1 = new Member( list.Id, "John Doe", "john@doe.com" );
			// insert
			m1.Persist();
			Assert.AreEqual( m1.Name, "John Doe", "The object was not properly inserted!" );
			Assert.AreEqual( m1.Address, "john@doe.com", "The object was not properly inserted!" );
			// delete
			m1.Remove();
		}

		/// <summary>
		/// Test that a required field without data fails validation.
		/// </summary>
		[Test]
		[ExpectedException( typeof(ValidationException) )]
		public void TestRequiredBad()
		{
			// test that an empty email address throws an exception
			m1 = new Member( list.Id, string.Empty, "john@doe.com" );
			// insert
			m1.Persist();
		}

		/// <summary>
		/// Test that a required field without data fails validation.
		/// </summary>
		[Test]
		public void TestRequiredBadButAllowNull()
		{
			// test that an empty email address throws an exception
			m1 = new Member( list.Id, null, "john@doe.com" );
			// insert
			m1.Persist();
			// delete
			m1.Remove();
		}

		[Test]
		public void TestRangeBad()
		{
			Numbers numbers = new Numbers();
			numbers.NInt16 = 10; // range: 20-100
			numbers.NInt32 = 101; // range: <= 100
			numbers.NInt = 101; // range: 20-100
			numbers.NInt64 = 99; // range: >= 100
			numbers.NLong = 19; // range: 20-100
			numbers.NFloat = 100.4F; // range: 20.3-100.3
			numbers.NDblO = 100.5; // range: <= 100.4
			numbers.NDouble = 2D; // range: 20.5-100.5
			try
			{
				ValidationBroker.Validate( numbers );
			}
			catch( ValidationException )
			{
				Assert.AreEqual( 8, numbers.ValidationMessages.Count );
			}
		}

		[Test]
		public void TestDateRangeGood()
		{
			m1 = new MemberValidation( 0, 1, "John Doe", "john@doe.com" );
			// insert - no exception because default dates are inside valid range
			m1.Persist();
			// clean up
			m1.Remove();
		}

		[Test, ExpectedException( typeof(ValidationException) )]
		public void TestDateRangeBad()
		{
			MemberValidation m1 = new MemberValidation( 0, 1, "John Doe", "john@doe.com" );
			// set date to something outside range
			m1.DateTime1 = new DateTime( 2005, 01, 01 );
			// insert - should fail
			m1.Persist();
			// clean up
			m1.Remove();
		}

		[Test]
		public void CustomValidatorGood()
		{
			// no exception because listid > id
			m1 = new MemberValidation( 0, 1, "John Doe", "john@doe.com" );
			// insert
			m1.Persist();
			// clean up
			m1.Remove();
		}

		[Test, ExpectedException( typeof(ValidationException) )]
		public void CustomValidatorBad()
		{
			// exception because listid < id
			m1 = new MemberValidation( 1, 0, "John Doe", "john@doe.com" );
			// insert
			m1.Persist();
			// clean up (not executed unless Persist erroneously succeeds)
			m1.Remove();
		}
	}
}
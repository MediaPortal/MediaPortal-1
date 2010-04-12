/*
 * Test cases
 * Copyright (C) 2004 Clayton Harbour
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: TestReflector.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System.Collections;
using Gentle.Common;
using NUnit.Framework;

namespace Gentle.Framework.Tests
{
	/// <summary>
	/// Test cases for the Reflector helper class.
	/// </summary>
	[TestFixture]
	public class TestReflector
	{
		[Test]
		public void FindPublicPropertyWithAttribute()
		{
			IList result = Reflector.FindMembers( Reflector.InstanceCriteria, typeof(GuidHolder), false, typeof(TableColumnAttribute) );
			Assert.AreEqual( 2, result.Count, "Did not locate all publicly decorated properties." );
		}

		[Test]
		public void FindPublicPropertyWithAttributes()
		{
			IList result = Reflector.FindMembers( Reflector.InstanceCriteria, typeof(GuidHolder), false,
			                                      typeof(TableColumnAttribute), typeof(PrimaryKeyAttribute) );
			Assert.AreEqual( 2, result.Count, "Did not locate all public properties with TableColumn attribute." );
			MemberAttributeInfo mai = (MemberAttributeInfo) result[ 0 ];
			Assert.IsNotNull( mai.Attributes, "Did not locate any attributes." );
			Assert.AreEqual( 2, mai.Attributes.Count, "Did not locate both attributes on first member." );
		}

		[Test]
		public void FindPrivateField()
		{
			IList result = Reflector.FindMembers( Reflector.InstanceCriteria, typeof(MailingList), false, typeof(TableColumnAttribute) );
			Assert.AreEqual( 3, result.Count, "Did not locate all private fields with primary attribute." );
		}
	}
}
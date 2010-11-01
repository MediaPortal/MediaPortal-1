/*
 * Test cases
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: TestMessages.cs 1232 2008-03-14 05:36:00Z mm $
 */

using Gentle.Common;
using NUnit.Framework;

namespace Gentle.Framework.Tests
{
	[TestFixture]
	public class TestMessages
	{
		[Test]
		public void TestErrorMessageNoArgs()
		{
			string expected = null;
			try
			{
				Check.Fail( Error.NoProviders );
			}
			catch( GentleException ge )
			{
				expected = ge.Message;
			}
			string msg = Messages.GetMsg( Error.NoProviders );
			Assert.IsNotNull( msg, "No message returned." );
			Assert.AreEqual( expected, msg, "Wrong message returned." );
			Assert.IsTrue( msg != null && msg.IndexOf( "provider" ) != -1, "Wrong message returned." );
		}

		[Test]
		public void TestErrorMessageSingleArg()
		{
			string expected = null;
			try
			{
				Check.Fail( Error.InvalidRequest, "TEST" );
			}
			catch( GentleException ge )
			{
				expected = ge.Message;
			}
			string msg = Messages.GetMsg( Error.InvalidRequest, "TEST" );
			Assert.IsNotNull( msg, "No message returned." );
			Assert.AreEqual( expected, msg, "Wrong message returned." );
			Assert.IsTrue( msg != null && msg.IndexOf( "TEST" ) != -1, "Wrong message returned." );
		}
	}
}
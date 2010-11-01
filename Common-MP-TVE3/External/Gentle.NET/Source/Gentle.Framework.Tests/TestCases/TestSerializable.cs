/*
 * Test cases
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: TestSerializable.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System.IO;
using System.Xml.Serialization;
using NUnit.Framework;

namespace Gentle.Framework.Tests
{
	/// <summary>
	/// This class holds test cases that test for serialization.
	/// </summary>
	[TestFixture]
	public class TestSerializable
	{
		/// <summary>
		/// Test that the <see cref="Persistent"/> object can be serialized using the 
		/// <see cref="XmlSerializer"/> class.
		/// </summary>
		[Test]
		public void TestSerialize()
		{
			Member m = new Member( 1, "John Doe", "john@doe.com" );
			XmlSerializer serializer = new XmlSerializer( m.GetType() );
			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter( stream );
			serializer.Serialize( writer, m );
			writer.Close();
		}
	}
}
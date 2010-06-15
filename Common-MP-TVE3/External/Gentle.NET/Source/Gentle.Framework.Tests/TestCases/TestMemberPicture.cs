/*
 * Test cases
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: TestMemberPicture.cs 1232 2008-03-14 05:36:00Z mm $
 */
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using NUnit.Framework;

namespace Gentle.Framework.Tests
{
	/// <summary>
	/// This class holds test cases for exercising the Gentle framework using the 
	/// Member class. The database must have been created and populated with the 
	/// supplied test data for the tests to work.
	/// </summary>
	[TestFixture]
	public class TestMemberPicture
	{
		// locations to search for the image file required to run these tests
		private const string PATH1 = "../../tests/database files/";
		private const string PATH2 = "c:/code/gentle.net/source/gentle.framework/tests/database files/";

		private MemberPicture mp1, mp2;
		private Image picture;
		private int pictureSize;
		private bool runTest;

		[SetUp]
		public void Init()
		{
			try
			{
				GentleSqlFactory sf = Broker.GetSqlFactory();
				// this will throw an exception because under normal operation it would indicate an error
				runTest = (sf.GetDbType( typeof(Guid) ) != sf.NO_DBTYPE) || (sf.GetDbType( typeof(byte[]) ) != sf.NO_DBTYPE);
				picture = GetPicture();
				pictureSize = GetSize( picture );
			}
			catch
			{
				runTest = false;
			}
		}

		[TearDown]
		public void Final()
		{
			if( runTest )
			{
				SqlBuilder sb = new SqlBuilder( StatementType.Delete, typeof(MemberPicture) );
				Broker.Execute( sb.GetStatement( true ) );
			}
		}

		/// <summary>
		/// Test case for verifying the four basic statement types (CRUD for Create, Read, Update, 
		/// Delete). This method executes 6 statements (of which the last select returns no data).
		/// </summary>
		[Test]
		public void TestCRUD()
		{
			// skip test if picture data was not read (check the PATH constant above!)
			if( runTest && picture != null )
			{
				mp1 = new MemberPicture( picture, 1 );
				// insert
				mp1.Persist();
				Assert.AreEqual( mp1.MemberId, 1, "The object was not properly inserted!" );
				Assert.AreEqual( GetSize( mp1.Picture ), pictureSize, "The object was not properly inserted!" );
				// select
				mp2 = MemberPicture.Retrieve( mp1.Id );
				// verify select/insert
				Assert.IsNotNull( mp2.Id, "The object could not be retrieved from the database!" );
				Assert.AreEqual( mp1.Id, mp2.Id, "The object could not be retrieved from the database!" );
				Assert.AreEqual( pictureSize, GetSize( mp2.Picture ), "The object was not properly retrieved on construction!" );
				Assert.AreEqual( mp1.MemberId, mp2.MemberId,
				                 "The object was not properly retrieved on construction!" );
				// update
				mp2.MemberId = 2;
				mp2.Persist();
				// verify update
				mp1 = MemberPicture.Retrieve( mp2.Id );
				Assert.AreEqual( mp2.MemberId, mp1.MemberId, "MemberId not updated!" );
				// delete
				mp2.Remove();
				// verify delete by counting the number of rows
				SqlBuilder sb = new SqlBuilder( StatementType.Count, typeof(MemberPicture) );
				sb.AddConstraint( Operator.Equals, "Id", mp1.Id );
				SqlResult sr = Broker.Execute( sb.GetStatement( true ) );
				Assert.AreEqual( 0, sr.Count, "Object not removed" );
			}
		}

		private Image GetPicture()
		{
			Image _picture = null;
			try
			{
				// first try without path
				_picture = Image.FromFile( "Gentle.jpg" );
			}
			catch
			{
				try
				{
					// then try relative path
					_picture = Image.FromFile( PATH1 + "Gentle.jpg" );
				}
				catch
				{
					// last resort using absolute path (works for me ;-)
					_picture = Image.FromFile( PATH2 + "Gentle.jpg" );
				}
			}
			Assert.IsNotNull( _picture );
			return _picture;
		}

		private int GetSize( Image picture )
		{
			MemoryStream memoryStream = new MemoryStream();
			picture.Save( memoryStream, ImageFormat.Jpeg );
			return memoryStream.ToArray().Length;
		}
	}
}
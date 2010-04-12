using System;
using NUnit.Framework;

namespace Gentle.Framework.Tests
{
	/// <summary>
	/// Summary description for TestDateTimeNullValue.
	/// </summary>
	[TestFixture]
	public class TestDateTimeNullValue
	{
		[SetUp, TearDown]
		public void Init()
		{
			Broker.Execute( "delete from PropertyHolder" );
		}

		private string fd( DateTime d )
		{
			return d.ToString( "yyyy-MM-dd HH:mm:ss" );
		}

		[Test]
		public void TestMinValue()
		{
			PHDateTimeNullValueMin obj1;
			PHDateTimeNullValueMin obj2;
			DateTime now = DateTime.Now;

			// verify that NULL is inserted when using NullValue
			obj1 = new PHDateTimeNullValueMin( now, DateTime.MinValue );
			obj1.Persist();
			SqlResult sr = Broker.Execute( "select TDateTime from PropertyHolder where ph_Id = " + obj1.Id );
			Assert.IsNull( sr[ 0, "TDateTime" ], "Default NullValue was not converted to NULL on insert." );
			// verify that object creation uses correct NullValue
			Key key = new Key( typeof(PHDateTimeNullValueMin), true, "dt", obj1.DT );
			obj2 = (PHDateTimeNullValueMin) Broker.RetrieveInstance( typeof(PHDateTimeNullValueMin), key );
			Assert.AreEqual( fd( now ), fd( obj2.DTNN ) );
			Assert.AreEqual( fd( DateTime.MinValue ), fd( obj2.DT ) );
			// verify that translation is disabled for ordinary values
			obj2.DT = now;
			obj2.Persist();
			obj1 = PHDateTimeNullValueMin.Retrieve( obj2.Id );
			Assert.AreEqual( fd( now ), fd( obj1.DT ) );
			// verify that translation is disabled for MaxValue
			obj1.DT = DateTime.MaxValue;
			obj1.Persist();
			sr = Broker.Execute( "select TDateTime from PropertyHolder where ph_Id = " + obj1.Id );
			Assert.IsNotNull( sr[ 0, "TDateTime" ], "Erronous NullValue conversion for MaxValue." );
		}

		[Test]
		public void TestMaxValue()
		{
			PHDateTimeNullValueMax obj1;
			PHDateTimeNullValueMax obj2;
			DateTime now = DateTime.Now;

			// verify that NULL is inserted when using NullValue
			obj1 = new PHDateTimeNullValueMax( now, DateTime.MaxValue );
			obj1.Persist();
			SqlResult sr = Broker.Execute( "select TDateTime from PropertyHolder where ph_Id = " + obj1.Id );
			Assert.IsNull( sr[ 0, "TDateTime" ], "Default NullValue was not converted to NULL on insert." );
			// verify that object creation uses correct NullValue
			Key key = new Key( typeof(PHDateTimeNullValueMax), true, "dt", obj1.DT );
			obj2 = (PHDateTimeNullValueMax) Broker.RetrieveInstance( typeof(PHDateTimeNullValueMax), key );
			Assert.AreEqual( fd( now ), fd( obj2.DTNN ) );
			Assert.AreEqual( fd( DateTime.MaxValue ), fd( obj2.DT ) );
			// verify that translation is disabled for ordinary values
			obj2.DT = now;
			obj2.Persist();
			obj1 = PHDateTimeNullValueMax.Retrieve( obj2.Id );
			Assert.AreEqual( fd( now ), fd( obj1.DT ) );
			// verify that translation is disabled for MinValue
			obj1.DT = DateTime.MinValue;
			obj1.Persist();
			sr = Broker.Execute( "select TDateTime from PropertyHolder where ph_Id = " + obj1.Id );
			Assert.IsNotNull( sr[ 0, "TDateTime" ], "Erronous NullValue conversion for MinValue." );
		}
	}
}
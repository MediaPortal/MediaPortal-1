using Gentle.Provider.Firebird;
using NUnit.Framework;
using Rhino.Mocks;

namespace Gentle.Framework.Tests.TestCases
{
	/// <summary>
	/// This class tests <see cref="FirebirdFactory"/> class.
	/// </summary>
	[TestFixture]
	public class TestFirebirdFactory
	{
		private FirebirdFactory factory;
		private MockRepository mocks = new MockRepository();

		[SetUp]
		public void Init()
		{
			factory = new FirebirdFactory(
				mocks.CreateMock( typeof(IGentleProvider) ) as IGentleProvider );
			mocks.ReplayAll();
		}

		[TearDown]
		public void Final()
		{
			mocks.VerifyAll();
		}

		/// <summary>
		/// This test case tests <c>IsReservedWord</c> method.
		/// </summary>
		[Test]
		public void TestReservedWord()
		{
			Assert.IsTrue( factory.IsReservedWord( "Trigger" ) );
		}

		/// <summary>
		/// This test case tests <c>QuoteReservedWord</c> method.
		/// </summary>
		[Test]
		public void TestQuoteReservedWord()
		{
			Assert.AreEqual( "\"Trigger\"", factory.QuoteReservedWord( "Trigger" ) );
		}
	}
}
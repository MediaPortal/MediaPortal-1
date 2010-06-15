using NUnit.Framework;

namespace Gentle.Framework.Tests
{
	/// <summary>
	/// Test editing of a table that uses reserved words.
	/// </summary>
	[TestFixture]
	public class TestReservedWords
	{
		private ReservedWords rw1, rw2;
		private bool runTests = Broker.ProviderName == "SQLServer";

		/// <summary>
		/// Clean up table.
		/// </summary>
		[TearDown]
		public void CleanUp()
		{
			if( runTests )
			{
				SqlBuilder sb = new SqlBuilder( StatementType.Delete, typeof(ReservedWords) );
				SqlStatement stmt = sb.GetStatement( true );
				stmt.Execute();
			}
		}

		/// <summary>
		/// Test the standard CRUD operations.
		/// </summary>
		[SetUp]
		public void TestCRUD()
		{
			if( runTests )
			{
				rw1 = new ReservedWords( 0, "order", 2, "of", "group" );
				// test insert
				rw1.Persist();
				// test select
				rw2 = ReservedWords.Retrieve( rw1.Identity );
				Assert.IsNotNull( rw2 );
				Assert.AreEqual( rw1.Identity, rw2.Identity );
				Assert.AreEqual( "order", rw2.Order );
				Assert.AreEqual( 2, rw2.Value );
				Assert.AreEqual( "of", rw2.Of );
				Assert.AreEqual( "group", rw2.Group );
				// test update
				rw1.Of = "hello";
				rw1.Persist();
				rw2 = ReservedWords.Retrieve( rw1.Identity );
				Assert.AreEqual( rw1.Of, rw2.Of );
				// cleanup
				rw2.Remove();
			}
		}

		/// <summary>
		/// Test select using non-identity criteria.
		/// </summary>
		[Test]
		public void TestSelectNonIdentity()
		{
			if( runTests )
			{
				// populate
				rw1 = new ReservedWords( 0, "order", 2, "of", "group" );
				rw1.Persist();
				// test select
				rw2 = ReservedWords.Retrieve( "group" );
				Assert.IsNotNull( rw2 );
				Assert.AreEqual( rw1.Identity, rw2.Identity );
				Assert.AreEqual( "order", rw2.Order );
				Assert.AreEqual( 2, rw2.Value );
				Assert.AreEqual( "of", rw2.Of );
				Assert.AreEqual( "group", rw2.Group );
			}
		}

		//TODO: Add OrderBy to retrieve a list of items...
	}
}
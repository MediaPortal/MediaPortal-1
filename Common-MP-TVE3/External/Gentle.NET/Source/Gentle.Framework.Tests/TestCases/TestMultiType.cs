/*
 * Test cases
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: TestMultiType.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Collections;
using Gentle.Common;
using NUnit.Framework;

namespace Gentle.Framework.Tests
{
	/// <summary>
	/// This class holds test cases for exercising the Gentle framework using the 
	/// Member class. The database must have been created and populated with the 
	/// supplied test data for the tests to work.
	/// </summary>
	[TestFixture]
	public class TestMultiType
	{
		private MultiType o1, o2;

		[SetUp]
		public void Init()
		{
			GentleSettings.CacheObjects = false;
			CacheManager.Clear();
			SqlBuilder sb = new SqlBuilder( StatementType.Delete, typeof(MultiType) );
			Broker.Execute( sb.GetStatement( true ) );
		}

		[TearDown]
		public void Final()
		{
			SqlBuilder sb = new SqlBuilder( StatementType.Delete, typeof(MultiType) );
			Broker.Execute( sb.GetStatement( true ) );
		}

		/// <summary>
		/// Test case for verifying the four basic statement types (CRUD for Create, Read, Update, 
		/// Delete). This method executes 5 statements.
		/// </summary>
		[Test]
		public void TestCRUD_Dog()
		{
			o1 = new Dog();
			// insert
			o1.Persist();
			Assert.IsTrue( o1.Id > 0, "No identity assigned on insert." );
			// select
			o2 = Dog.Retrieve( o1.Id );
			// verify select/insert
			Assert.IsNotNull( o2.Id, "The object could not be retrieved from the database!" );
			Assert.AreEqual( o1.Id, o2.Id, "The object could not be retrieved from the database!" );
			// update
			o2.Persist();
			// verify update
			o1 = Dog.Retrieve( o2.Id );
			// delete
			o2.Remove();
			// verify delete by counting the number of rows
			SqlBuilder sb = new SqlBuilder( StatementType.Count, typeof(Dog) );
			sb.AddConstraint( Operator.Equals, "Id", o1.Id );
			SqlResult sr = Broker.Execute( sb.GetStatement( true ) );
			Assert.AreEqual( 0, sr.Count, "Object not removed" );
		}

		/// <summary>
		/// Test case for verifying the four basic statement types (CRUD for Create, Read, Update, 
		/// Delete). This method executes 5 statements.
		/// </summary>
		[Test]
		public void TestCRUD_Cat()
		{
			o1 = new Cat();
			// insert
			o1.Persist();
			Assert.IsTrue( o1.Id > 0, "No identity assigned on insert." );
			// select
			o2 = Cat.Retrieve( o1.Id );
			// verify select/insert
			Assert.IsNotNull( o2.Id, "The object could not be retrieved from the database!" );
			Assert.AreEqual( o1.Id, o2.Id, "The object could not be retrieved from the database!" );
			// update
			o2.Persist();
			// verify update
			o1 = Cat.Retrieve( o2.Id );
			// delete
			o2.Remove();
			// verify delete by counting the number of rows
			SqlBuilder sb = new SqlBuilder( StatementType.Count, typeof(Cat) );
			sb.AddConstraint( Operator.Equals, "Id", o1.Id );
			SqlResult sr = Broker.Execute( sb.GetStatement( true ) );
			Assert.AreEqual( 0, sr.Count, "Object not removed" );
		}

		/// <summary>
		/// Test case for verifying that the current object type is correctly written to
		/// the inheritance column.
		/// </summary>
		[Test]
		public void TestTypeFieldOnPersist()
		{
			o1 = new Dog();
			Assert.IsNull( o1.Type, "Type was initialized by object itself." );
			// insert
			o1.Persist();
			Assert.IsNotNull( o1.Type, "Type was not initialized on insert (persist)." );
			string typeName = o1.Type.Split( ',' )[ 0 ];
			Assert.AreEqual( "Gentle.Framework.Tests.Dog", typeName, "Invalid type string." );
			// check that updates dont mess with the string
			o1.Persist();
			Assert.IsNotNull( o1.Type, "Type was not initialized on update (persist)." );
			typeName = o1.Type.Split( ',' )[ 0 ];
			Assert.AreEqual( "Gentle.Framework.Tests.Dog", typeName, "Invalid type string." );
			// delete
			o1.Remove();
		}

		/// <summary>
		/// Test case for verifying that we can convert strings to types. Note that in
		/// ObjectFactory we need to load the assembly first.
		/// </summary>
		[Test]
		public void TestStringToType()
		{
			// create entry in db
			o1 = new Dog();
			Type type = Type.GetType( "Gentle.Framework.Tests.Dog" );
			Assert.IsNotNull( type, "Unable to create type from string." );
			type = Type.GetType( o1.GetType().FullName );
			Assert.IsNotNull( type, "Unable to create type from string." );
		}

		/// <summary>
		/// Test case for verifying that the current object type is correctly written to
		/// the inheritance column.
		/// </summary>
		[Test]
		public void TestTypeOverride()
		{
			// create entry in db
			o1 = new Dog();
			o1.Persist();
			// check dynamic type override when using base class retrieve
			o2 = Animal.Retrieve( o1.Id );
			Assert.IsTrue( o2 is Dog, "Object construction did not use type from database." );
			// delete
			o2.Remove();
		}

		/// <summary>
		/// Test case for verifying that the current object type is correctly written to
		/// the inheritance column.
		/// </summary>
		[Test]
		public void TestTypeFilter()
		{
			// create entries of different types in db
			o1 = new Dog();
			o1.Persist();
			o2 = new Cat();
			o2.Persist();
			// fetch Dog list
			IList list = Broker.RetrieveList( typeof(Dog) );
			Assert.AreEqual( 1, list.Count, "Filter error." );
			Assert.AreEqual( typeof(Dog), list[ 0 ].GetType(), "Filter error." );
			// fetch Cat list
			list = Broker.RetrieveList( typeof(Cat) );
			Assert.AreEqual( 1, list.Count, "Filter error." );
			Assert.AreEqual( typeof(Cat), list[ 0 ].GetType(), "Filter error." );
			// fetch Animal list
			list = Broker.RetrieveList( typeof(Animal) );
			Assert.AreEqual( 2, list.Count, "Filter error." );
			// delete
			o1.Remove();
			o2.Remove();
		}

		[Test]
		public void TestCachingWithInheritanceAndTypeDiscriminator()
		{
			bool cacheState = GentleSettings.CacheObjects;
			GentleSettings.CacheObjects = true;
			GentleSettings.SkipQueryExecution = false;
			CacheManager.Clear();
			// populate database
			Animal animal = new Animal();
			animal.Persist();
			// retrieve existing object but as subclass - we should get new instance
			Dog dog = Dog.Retrieve( animal.Id );
			Assert.AreNotSame( dog, animal, "Inheritance selection is not working." );
			Animal a = Animal.Retrieve( animal.Id );
			Assert.AreSame( animal, a, "Uniqing is not working." );
			// cleanup
			animal.Remove();
			GentleSettings.CacheObjects = cacheState;
		}
	}
}
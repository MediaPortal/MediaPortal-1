/*
 * Test cases
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: GuidHolder.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;

namespace Gentle.Framework.Tests
{
	/// <summary>
	/// The TestGuid class is used as a standalone tester for the GUID data type. It also demonstrates
	/// how the Gentle attributes can be used on properties instead of fields. Note that this class
	/// does not use an auto-generated primary key.
	/// </summary>
	[TableName( "GuidHolder" )]
	public class GuidHolder : Persistent
	{
		private Guid id;
		private int someValue;

		#region Constructors
		public GuidHolder( Guid id, int someValue )
		{
			this.id = id;
			this.someValue = someValue;
		}

		public GuidHolder( int someValue ) : this( Guid.NewGuid(), someValue )
		{
		}

		public GuidHolder() : this( Guid.NewGuid(), 0 )
		{
		}

		public static GuidHolder Retrieve( Guid id )
		{
			Key key = new Key( typeof(GuidHolder), true, "Id", id );
			return Broker.RetrieveInstance( typeof(GuidHolder), key ) as GuidHolder;
		}
		#endregion

		#region Properties
		[TableColumn( "Guid", NotNull = true ), PrimaryKey]
		public Guid Id
		{
			get { return id; }
			set { id = value; }
		}
		[TableColumn( "SomeValue", NotNull = true )]
		public int SomeValue
		{
			get { return someValue; }
			set { someValue = value; }
		}
		#endregion
	}
}
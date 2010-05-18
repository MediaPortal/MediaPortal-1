/*
 * Test cases
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: GuidHolderIP.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using Gentle.Common;

namespace Gentle.Framework.Tests
{
	/// <summary>
	/// This class is used to test that Gentle handles types not inheriting from Persistent,
	/// but only implementing the IPersistent interface. It is not related to the GuidHolder
	/// class except in that it maps to the same table.
	/// </summary>
	[TableName( "GuidHolder" )]
	public class GuidHolderIP : BrokerLock, IPersistent
	{
		[TableColumn( "Guid", NotNull = true ), PrimaryKey]
		private Guid id;
		[TableColumn( "SomeValue", NotNull = true )]
		private int someValue;
		protected bool isPersisted;

		#region Constructors
		public GuidHolderIP( Guid id, int someValue ) : base( null )
		{
			this.id = id;
			this.someValue = someValue;
		}

		public GuidHolderIP( int someValue ) : this( Guid.NewGuid(), someValue )
		{
		}

		public GuidHolderIP() : this( Guid.NewGuid(), 0 )
		{
		}
		#endregion

		#region Gentle Access Methods (DAL)
		/// <summary>
		/// Abstract method for subclasses to implement. It should return a <see cref="Key"/> 
		/// instance containing all the primary key properties of this object.
		/// </summary>
		/// <returns></returns>
		public virtual Key GetKey()
		{
			// the Key class knows how to build a key using reflection
			return Key.GetKey( true, this );
		}

		/// <summary>
		/// Persist the current instance to the database.
		/// </summary>
		public virtual void Persist()
		{
			broker.Persist( this );
		}

		/// <summary>
		/// Persist the current instance to the database as part of the given transaction.
		/// </summary>
		/// <param name="transaction">The transaction in which to execute the operation.</param>
		public virtual void Persist( Transaction transaction )
		{
			Check.VerifyNotNull( transaction, Error.NullParameter, "transaction" );
			transaction.Persist( this );
		}

		/// <summary>
		/// Remove the current instance from the database. 
		/// </summary>
		public virtual void Remove()
		{
			if( ! isPersisted )
			{
				Check.Fail( Error.DeveloperError, "Unable to remove objects that have not yet been persisted." );
			}
			broker.Remove( this );
		}

		/// <summary>
		/// Remove the current instance from the database. 
		/// </summary>
		/// <param name="transaction">The transaction in which to execute the operation.</param>
		public virtual void Remove( Transaction transaction )
		{
			if( ! isPersisted )
			{
				Check.Fail( Error.DeveloperError, "Unable to remove objects that have not yet been persisted." );
			}
			transaction.Remove( this );
		}

		/// <summary>
		/// Refresh the current instance with the values from the database. This is
		/// effectively the same as creating a new instance using the key of the 
		/// current object and updating properties on the current object using the
		/// values from the retrieved object. This method has serious performance
		/// implications if used a lot and it should therefore be used sparingly.
		/// Use a static Retrieve method to fetch a new instance instead and save
		/// the overhead incurred by having to copy the property values.
		/// </summary>
		public virtual void Refresh()
		{
			broker.Refresh( this );
		}

		public static GuidHolderIP Retrieve( Guid id )
		{
			Key key = new Key( typeof(GuidHolderIP), true, "id", id );
			return Broker.RetrieveInstance( typeof(GuidHolderIP), key ) as GuidHolderIP;
		}
		#endregion

		#region IPersistent (DAL)
		public bool IsPersisted
		{
			get { return isPersisted; }
			set { isPersisted = value; }
		}
		#endregion

		#region Properties
		public Guid Id
		{
			get { return id; }
		}
		public int SomeValue
		{
			get { return someValue; }
			set { someValue = value; }
		}
		#endregion
	}
}
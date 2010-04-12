/*
 * The base class for your new persistable business objects
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: ContextPersistent.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Collections;
using System.Xml.Serialization;
using Gentle.Common;
using Gentle.Common.Attributes;

namespace Gentle.Framework
{
	/// <summary>
	/// <p>This class is currently work in progress and is not intended for general consumption.</p>
	/// <p>This class is intended as an alternate base class for new persistent business objects.
	/// By inheriting from <see cref="ContextBoundObject"/> it is possible to automatically flag
	/// the object instance as dirty when persistent properties are modified, support lazy loading,
	/// perform parameter validation and code assertions. The downside to all of this is a heavy
	/// performance cost.</p>
	/// <p>This class is also intended as an example of how you can create your own base class
	/// for persistent objects. It is currently work-in-progress.</p>
	/// <p>As much of the IPersistent interface as possible should be provided by this class.</p>
	/// </summary>
	public abstract class ContextPersistent : ContextBoundObject, IPersistent, IValidationPersistent, IBrokerLock
	{
		#region Members
		internal bool isPersisted; // determines whether to insert or update
		[TableColumn, Concurrency]
		internal int databaseVersion; // for automatic support for optimistic offline locking of rows
		[NonSerialized]
		private IList validationMessages;
		[NonSerialized]
		protected PersistenceBroker broker;
		#endregion

		#region Constructors
		/// <summary>
		/// Use this constructor for new (unpersisted) instances.
		/// The default provider will be used to access the database.
		/// </summary>
		protected ContextPersistent() : this( 0, false, null )
		{
		}

		/// <summary>
		/// This is the generic base constructor will a full argument set.
		/// </summary>
		/// <param name="version">The version of this record in the database or 0 for new instances.</param>
		/// <param name="isPersisted">A boolean to indicate whether this object has been persisted.</param>
		/// <param name="broker">The PersistenceBroker to use for connecting to the database.</param>
		public ContextPersistent( int version, bool isPersisted, PersistenceBroker broker )
		{
			databaseVersion = version;
			this.isPersisted = isPersisted;
			if( broker == null )
			{
				this.broker = new PersistenceBroker( GetType() );
			}
			else // use supplied broker and associated provider
			{
				this.broker = broker;
			}
		}
		#endregion

		#region Properties
		/// <summary>
		/// True if this object instance has been persisted to the database. Updates to
		/// properties of this instance since does not change the value of this property,
		/// whose sole purpose is to determine if the object needs to be inserted or updated.
		/// </summary>
		[Visible( false ), XmlAttribute]
		public virtual bool IsPersisted
		{
			get { return isPersisted; }
			set { isPersisted = value; }
		}
		/// <summary>
		/// Validation messages reported by the validation framework.
		/// </summary>
		[Visible( false ), XmlIgnore]
		public virtual IList ValidationMessages
		{
			get
			{
				if( validationMessages == null )
				{
					validationMessages = new ArrayList();
				}
				return validationMessages;
			}
		}
		#endregion

		#region IPersistent

		#region GetKey
		/// <summary>
		/// Abstract method for subclasses to implement. It should return a <see cref="Key"/> 
		/// instance containing all the primary key properties of this object.
		/// </summary>
		/// <returns></returns>
		public virtual Key GetKey()
		{
			// the Key class knows how to build a key using reflection
			return Key.GetKey( SessionBroker, true, this );
		}
		#endregion

		#region Persist
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
		#endregion

		#region Remove
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
			Check.VerifyNotNull( transaction, Error.NullParameter, "transaction" );
			transaction.Remove( this );
		}
		#endregion

		#region Refresh
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

		/// <summary>
		/// Refresh the current instance with the values from the database. This is
		/// effectively the same as creating a new instance using the key of the 
		/// current object and updating properties on the current object using the
		/// values from the retrieved object. This method has serious performance
		/// implications if used a lot and it should therefore be used sparingly.
		/// Use a static Retrieve method to fetch a new instance instead and save
		/// the overhead incurred by having to copy the property values.
		/// </summary>
		/// <param name="transaction">The transaction in which to execute the operation.</param>
		public virtual void Refresh( Transaction transaction )
		{
			Check.VerifyNotNull( transaction, Error.NullParameter, "transaction" );
			transaction.Refresh( this );
		}
		#endregion

		#endregion

		#region Retrieve
		/// <summary>
		/// Retrieve the object of the specified type and identified by the given key.
		/// </summary>
		/// <param name="type">The type of the Persistent descendant</param>
		/// <param name="key">The key identifying the object</param>
		/// <returns>An SqlResult instance</returns>
		public static object Retrieve( Type type, Key key )
		{
			return Retrieve( null, type, key );
		}

		/// <summary>
		/// Retrieve the object of the specified type and identified by the given key. This method will
		/// throw an exception if not exactly one row in the database matches.
		/// </summary>
		/// <param name="broker">The PersistenceBroker and associated provider used fetch the data</param>
		/// <param name="type">The type of the Persistent descendant</param>
		/// <param name="key">The key identifying the object</param>
		/// <returns>An object instance of the specified type</returns>
		public static object Retrieve( PersistenceBroker broker, Type type, Key key )
		{
			if( broker == null )
			{
				// use default broker instance by use of the Broker class
				return Broker.RetrieveInstance( type, key );
			}
			else // use supplied broker instance
			{
				return broker.RetrieveInstance( type, key );
			}
		}
		#endregion

		#region Retrieve List
		/// <summary>
		/// Retrieve all objects of a certain type (plain select of all rows in a the
		/// table, i.e. a statement without any where-clause).
		/// </summary>
		/// <param name="type">The type of the Persistent descendant to retrieve</param>
		/// <returns>A collection of objects of the given type</returns>
		public static IList RetrieveList( Type type )
		{
			return RetrieveList( null, type );
		}

		/// <summary>
		/// Retrieve all objects of a certain type (plain select of all rows in a the
		/// table, i.e. a statement without any where-clause).
		/// </summary>
		/// <param name="broker">The PersistenceBroker and associated provider used fetch the data</param>
		/// <param name="type">The type of the Persistent descendant to retrieve</param>
		/// <returns>A collection of objects of the given type</returns>
		public static IList RetrieveList( PersistenceBroker broker, Type type )
		{
			if( broker == null )
			{
				// use default broker instance by use of the Broker class
				return Broker.RetrieveList( type );
			}
			else // use supplied broker instance
			{
				return broker.RetrieveList( type );
			}
		}

		/// <summary>
		/// Retrieve a list of objects of the given type constrained by the properties of
		/// this object instance.
		/// </summary>
		/// <param name="type">The type of objects being retrieved</param>
		/// <param name="propertyNames">A list of property names on the current object</param>
		/// <returns>A collection of objects of the given type</returns>
		protected virtual IList RetrieveList( Type type, params string[] propertyNames )
		{
			if( GetType().Equals( type ) )
			{
				// if the list type we're selecting is same type as self no foreign key
				// translation is required - simply use the name given
				return broker.RetrieveList( type, Key.GetKey( broker, true, this, propertyNames ) );
			}
			else
			{
				// get the key with values from current object
				Key key = Key.GetKey( broker, true, this, propertyNames );
				return broker.RetrieveList( type, key );
			}
		}
		#endregion

		#region IBrokerLock Members
		/// <summary>
		/// The session broker provides a lock to the database engine. This is
		/// useful when connecting to multiple databases in a single application.
		/// </summary>
		[XmlIgnore]
		public PersistenceBroker SessionBroker
		{
			get { return broker; }
			set { broker = value; }
		}
		#endregion
	}
}
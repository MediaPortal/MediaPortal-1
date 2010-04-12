using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Gentle.Common;

/*
 * The Transaction class for excecuting transacted statements
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: Transaction.cs 1234 2008-03-14 11:41:44Z mm $
 */

namespace Gentle.Framework
{
	/// <summary>
	/// Class to encapsulate persistent objects participating in a transaction. Use this object
	/// if you need transaction protection for updating multiple objects.
	/// </summary>
	public class Transaction : BrokerLock, IDisposable
	{
		protected IDbConnection dbConnection;
		protected IDbTransaction dbTransaction;
		// initially set to true because class has a destructor
		protected bool isRegisteredForFinalize = true;

		#region Constructors
		/// <summary>
		/// Create a new transaction object. Use this to execute tasks in a transaction
		/// using the default database provider.
		/// </summary>
		public Transaction() : this( null )
		{
		}

		/// <summary>
		/// Create a new transaction object. Use this to execute tasks in a transaction
		/// using the specified PersistenceBroker and associated database provider.
		/// </summary>
		public Transaction( PersistenceBroker broker ) : base( broker )
		{
			Initialize();
		}
		#endregion

		#region Initialization
		/// <summary>
		/// Initialize connection and transaction objects.
		/// </summary>
		private void Initialize()
		{
			if( ! IsInitialized )
			{
				dbConnection = broker.Provider.GetConnection();
				dbTransaction = dbConnection.BeginTransaction();
				// make sure that we do not re-register instances already registered for finalization
				if( ! isRegisteredForFinalize )
				{
					GC.ReRegisterForFinalize( this );
				}
			}
		}

		/// <summary>
		/// True when underlying connection and transaction object references exist. When this
		/// property is true, clients must remember to call either Commit or Rollback to 
		/// release the resources used.
		/// </summary>
		public bool IsInitialized
		{
			get { return dbTransaction != null; }
		}
		#endregion

		#region Statement Execution
		/// <summary>
		/// Execute a custom statement in this transaction.
		/// </summary>
		/// <param name="stmt">The SqlStatement to add</param>
		public virtual SqlResult Execute( SqlStatement stmt )
		{
			Initialize();
			return broker.Execute( stmt, dbTransaction );
		}

		/// <summary>
		/// Persist the given IPersistance instance in this transaction.
		/// </summary>
		/// <param name="entity">The object to persist</param>
		public virtual void Persist( IEntity entity )
		{
			Initialize();
			broker.Persist( entity, dbTransaction );
		}

		/// <summary>
		/// Insert the given object instance in this transaction.
		/// </summary>
		/// <param name="entity">The object to insert</param>
		public virtual void Insert( object entity )
		{
			Initialize();
			broker.Insert( entity, dbTransaction );
		}

		/// <summary>
		/// Update the given object instance in this transaction.
		/// </summary>
		/// <param name="entity">The object to update</param>
		public virtual void Update( object entity )
		{
			Initialize();
			broker.Update( entity, dbTransaction );
		}

		/// <summary>
		/// Remove the given object instance in this transaction.
		/// </summary>
		/// <param name="entity">The object to remove</param>
		public virtual void Remove( object entity )
		{
			Initialize();
			broker.Remove( entity, dbTransaction );
		}

		/// <summary>
		/// Remove the object given its type and a key.
		/// </summary>
		/// <param name="type">The type of object</param>
		/// <param name="key">The key indentifying the object</param>
		public virtual void Remove( Type type, Key key )
		{
			Initialize();
			broker.Remove( type, key, dbTransaction );
		}

		/// <summary>
		/// Remove the object given its type and a key.
		/// </summary>
		/// <typeparam name="T">The type of objects to create</typeparam>
		/// <param name="key">The key indentifying the object</param>
		public virtual void Remove<T>( Key key )
		{
			Initialize();
			broker.Remove<T>( key, dbTransaction );
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
		public virtual void Refresh( object entity )
		{
			Initialize();
			broker.Refresh( entity, dbTransaction );
		}

		/// <summary>
		/// Retrieve an instance of the specified type in this transaction.
		/// </summary>
		public virtual object RetrieveInstance( Type type, Key key )
		{
			Initialize();
			return broker.RetrieveInstance( type, key, dbConnection, dbTransaction );
		}

		/// <summary>
		/// Retrieve an instance of the specified type in this transaction.
		/// </summary>
		/// <typeparam name="T">The type of objects to create</typeparam>
		public virtual T RetrieveInstance<T>( Key key )
		{
			Initialize();
			return broker.RetrieveInstance<T>( key, dbConnection, dbTransaction );
		}

		/// <summary>
		/// Retrieve multiple instances of the given type in this transaction. The retrieved 
		/// rows are limited by the fields and values specified in the given <see cref="Key"/> 
		/// instance.
		/// </summary>
		/// <param name="type">The type of objects to create</param>
		/// <param name="key">The key of the objects to retrieve</param>
		/// <param name="result">An optional existing container in which to store the created 
		/// objects. If this parameter is null a new IList instance will be created.</param>
		/// <returns>An array containing the created object instances</returns>
		public virtual IList RetrieveList( Type type, Key key, IList result )
		{
			Initialize();
			return broker.RetrieveList( type, key, result, dbConnection, dbTransaction );
		}

		/// <summary>
		/// Retrieve multiple instances of the given type in this transaction. The retrieved 
		/// rows are limited by the fields and values specified in the given <see cref="Key"/> 
		/// instance.
		/// </summary>
		/// <typeparam name="T">The type of objects to create</typeparam>
		/// <param name="key">The key of the objects to retrieve</param>
		/// <param name="result">An optional existing container in which to store the created 
		/// objects. If this parameter is null a new IList instance will be created.</param>
		/// <returns>An array containing the created object instances</returns>
		public virtual IList<T> RetrieveList<T>( Key key, IList<T> result )
		{
			Initialize();
			return broker.RetrieveList<T>( key, result, dbConnection, dbTransaction );
		}
		#endregion

		#region Commit and Rollback
		/// <summary>
		/// Commit this transaction. 
		/// </summary>
		public void Commit()
		{
			try
			{
				if( IsInitialized )
				{
					dbTransaction.Commit();
				}
			}
			finally
			{
				Dispose();
			}
		}

		/// <summary>
		/// Rollback this transaction.
		/// </summary>
		public void Rollback()
		{
			try
			{
				if( IsInitialized )
				{
					dbTransaction.Rollback();
				}
			}
			finally
			{
				Dispose();
			}
		}
		#endregion

		#region IDisposable (and other cleanup code)
		~Transaction()
		{
			// report missing Dispose calls (failure to properly call Commit or Rollback)
			if( IsInitialized )
			{
				Check.LogError( LogCategories.General, "Failed to properly Dispose an instance of Transaction!" );
			}
			Dispose( false );
		}

		public void Dispose()
		{
			// called by user - release all resources
			Dispose( true );
			// suppress call to destructor by removing instance from finalization queue
			GC.SuppressFinalize( this );
			// make sure that we re-register for finalization if object is re-initialized
			isRegisteredForFinalize = false;
		}

		protected virtual void Dispose( bool isDisposing )
		{
			// check to see if we have any resources to free
			if( IsInitialized )
			{
				if( isDisposing )
				{
					try
					{
						dbConnection.Close();
						dbConnection = null;
						dbTransaction.Dispose();
						dbTransaction = null;
					}
					catch( InvalidOperationException )
					{
						// these can safely be ignored - just means that transaction has already terminated
					}
					catch( Exception e )
					{
						Check.Fail( e, Error.Unspecified,
						            "The request could not be completed. Please see the inner exception for details on this error." );
					}
				}
			}
			else if( dbConnection != null && dbConnection.State == ConnectionState.Open )
			{
				// connection but no transaction - this should never happen, but better safe than sorry
				dbConnection.Close();
			}
		}
		#endregion
	}
}
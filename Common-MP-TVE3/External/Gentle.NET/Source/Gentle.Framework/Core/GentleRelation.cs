/*
 * Helper class used by GentleList to manage n:m relations
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: GentleRelation.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Collections;
using Gentle.Common;

namespace Gentle.Framework
{
	/// <summary>
	/// This is a container class for managed storage of objects. 
	/// </summary>
	public class GentleRelation : BrokerLock, ICollection
	{
		#region Members
		/// <summary>
		/// An array of types involved in this relation.
		/// </summary>
		protected Type[] relatedTypes;
		/// <summary>
		/// A GentleList used to automate the persistence of the contained objects.
		/// </summary>
		protected GentleList relations;
		/// <summary>
		/// An ObjectMap describing the type contained in this list.
		/// </summary>
		protected ObjectMap containedMap;
		/// <summary>
		/// The parent object of this list.
		/// </summary>
		protected IPersistent parent;
		#endregion

		#region Constructors
		/// <summary>
		/// Create a new list for storing the specified type of objects.
		/// </summary>
		public GentleRelation( Type containedType, params Type[] relatedTypes ) :
			this( null, containedType, null, relatedTypes )
		{
		}

		/// <summary>
		/// Create a new list for storing the specified type of objects.
		/// </summary>
		public GentleRelation( Type containedType, IPersistent parent, params Type[] relatedTypes )
			: this( null, containedType, parent, relatedTypes )
		{
		}

		/// <summary>
		/// Create a new list for storing the specified type of objects.
		/// </summary>
		public GentleRelation( PersistenceBroker broker, Type containedType, params Type[] relatedTypes ) :
			this( broker, containedType, null, relatedTypes )
		{
		}

		/// <summary>
		/// Create a new list for storing the specified type of objects.
		/// </summary>
		public GentleRelation( PersistenceBroker broker, Type containedType, IPersistent parent,
		                       params Type[] relatedTypes ) : base( broker )
		{
			Check.VerifyNotNull( relatedTypes, Error.NullParameter, "relatedTypes" );
			/*Check.Verify( parent == null && relatedTypes.Length == 2 ||
				parent != null && relatedTypes.Length == 1, Error.NotImplemented,
			              "The GentleRelation class is currently only able to manage two-way relations." ); */
			this.relatedTypes = relatedTypes;
			this.parent = parent;
			containedMap = ObjectFactory.GetMap( broker, containedType );
			// note: "this." required on broker param or original null param might get passed
			relations = new GentleList( this.broker, containedType, parent );
			// verify that the linked types are supported by Gentle
			foreach( Type relatedType in relatedTypes )
			{
				Check.Verify( relatedType.IsSubclassOf( typeof(Persistent) ) || relatedType.GetInterface( "IPersistent" ) != null,
				              Error.UnsupportedType, relatedType );
			}
		}
		#endregion

		#region Add
		/// <summary>
		/// Add an object to the list. The object is created using values from the list of related 
		/// objects passed. This means you dont pass the relation object itself, but rather the 
		/// list of objects which are related (in an n:m fashion).
		/// </summary>
		/// <param name="relatedObjects">The list of related objects</param>
		/// <returns>The index of the newly created relation</returns>
		public virtual int Add( params object[] relatedObjects )
		{
			return Add( null, relatedObjects );
		}

		/// <summary>
		/// Add an object to the list. The object is created using values from the list of related 
		/// objects passed. This means you dont pass the relation object itself, but rather the 
		/// list of objects which are related (in an n:m fashion).
		/// </summary>
		/// <param name="transaction">The transaction within which to execute statements.</param>
		/// <param name="relatedObjects">The list of related objects</param>
		/// <returns>The index of the newly created relation</returns>
		public virtual int Add( Transaction transaction, params object[] relatedObjects )
		{
			Check.VerifyNotNull( relatedObjects, Error.NullParameter, "relatedObjects" );
			int expectedArgs = parent == null ? 1 : 0;
			expectedArgs += relatedTypes.Length;
			Check.VerifyEquals( expectedArgs, relatedObjects.Length, Error.DeveloperError,
			                    "Invalid use of GentleList (arguments to Add does not match managed type count)." );
			Key key = new Key( containedMap.Type, true );
			key = Key.GetKey( broker, key, true, parent );
			// only add first object at this point
			key = Key.GetKey( broker, key, true, relatedObjects[ 0 ] );
			// check to see if relation exists based on first object alone 
			object relation = relations.Find( key );
			if( relation != null )
			{
				return relations.IndexOf( relation );
			}
			// make sure the relation is constructed using all objects passed
			for( int i = 1; i < relatedObjects.Length; i++ )
			{
				key = Key.GetKey( broker, key, true, relatedObjects[ i ] );
			}
			relation = containedMap.Construct( key, broker );
			return relations.Add( transaction, relation );
		}
		#endregion

		#region Remove
		/// <summary>
		/// Removes a relation from the list. The relation to remove is found using values from the 
		/// list of related objects passed. This means you dont pass the relation object itself, but 
		/// rather the list of objects which are related (in an n:m fashion).
		/// </summary>
		/// <param name="relatedObjects">The list of related objects</param>
		public virtual void Remove( params object[] relatedObjects )
		{
			Remove( null, relatedObjects );
		}

		/// <summary>
		/// Removes a relation from the list. The relation to remove is found using values from the 
		/// list of related objects passed. This means you dont pass the relation object itself, but 
		/// rather the list of objects which are related (in an n:m fashion).
		/// </summary>
		/// <param name="transaction">The transaction within which to execute statements.</param>
		/// <param name="relatedObjects">The list of related objects</param>
		public virtual void Remove( Transaction transaction, params object[] relatedObjects )
		{
			Check.VerifyNotNull( relatedObjects, Error.NullParameter, "relatedObjects" );
			Check.Verify( relatedObjects.Length > 0, Error.EmptyListParameter, "relatedObjects" );
			/*Check.Verify( parent == null && relatedTypes.Length == 2 ||
				parent != null && relatedTypes.Length == 1, Error.NotImplemented,
			              "The GentleRelation class is currently only able to manage two-way relations." );*/
			Key key = new Key( containedMap.Type, true );
			key = Key.GetKey( broker, key, true, parent );
			// only add first object at this point
			key = Key.GetKey( broker, key, true, relatedObjects[ 0 ] );
			// do not use additional relation objects when removing
			object relation = relations.Find( key );
			if( relation != null )
			{
				relations.Remove( transaction, relation );
			}
		}
		#endregion

		#region Count
		/// <summary>
		/// Returns the number of relation objects currently in the list.
		/// </summary>
		public int Count
		{
			get { return relations.Count; }
		}
		#endregion

		#region ICollection Members
		public bool IsSynchronized
		{
			get { return relations.IsSynchronized; }
		}

		public void CopyTo( Array array, int index )
		{
			relations.CopyTo( array, index );
		}

		public object SyncRoot
		{
			get { return relations.SyncRoot; }
		}
		#endregion

		#region IEnumerable Members
		public IEnumerator GetEnumerator()
		{
			return relations.GetEnumerator();
		}
		#endregion
	}
}
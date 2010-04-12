/*
 * Typed Collection for use with the .NET DataGrid control
 * Copyright (C) 2004 Andreas Seibt
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: TypedArrayList.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Gentle.Common.Attributes;
using Gentle.Common.Util;

namespace Gentle.Common
{
	/// <summary>
	/// A strongly typed ArrayList with binding capabilities.
	/// <seealso cref="AllowSortAttribute"/>
	/// <seealso cref="CaptionAttribute"/>
	/// <seealso cref="ReadOnlyAttribute"/>
	/// <seealso cref="VisibleAttribute"/>
	/// </summary>
	/// <remarks>
	/// <p>Implemententation of a strongly typed array list of objects with full support
	/// for data binding (e.g. for use with a Datagrid control. </p>
	/// <p>The <c>TypedArrayList</c> class implements the <see cref="System.ComponentModel.ITypedList"/>
	/// and <see cref="System.ComponentModel.IBindingList"/> interfaces to provide databinding
	/// to list controls like the DataGrid control with full support for adding and removing
	/// entries and sorting the displayed list.</p>
	/// <p>The property methods of the types stored in the list can be flagged with a set 
	/// of attributes to control the the behaviour of the data grid.</p><br/>
	/// <table class="dtTABLE" cellspacing="0" style="width: 60%">
	///		<tr>
	///			<th width="10%">Attribute name</th>
	///			<th width="90%">usage</th>
	///		</tr>
	///		<tr>
	///			<td width="10%">Caption</td>
	///			<td width="90%">Defines the caption of the column in the data grid.</td>
	///		</tr>
	///		<tr>
	///			<td width="10%">ReadOnly</td>
	///			<td width="90%">(From <see cref="System.ComponentModel"/>) Flags the column in the data grid as read-only.</td>
	///		</tr>
	///		<tr>
	///			<td width="10%">AllowSort</td>
	///			<td width="90%">Flags the column in the data grid as sortable.</td>
	///		</tr>
	///		<tr>
	///			<td width="20%">Visible</td>
	///			<td width="80%">Defines if the property is a visible column in the DataGrid control.</td>
	///		</tr>
	/// </table><br/>
	/// <p>There is one restriction for the types stored in the <c>TypedArrayList</c> if you want
	/// to use the editing facilities of the DataGrid control. The types stored in the list must
	/// implement the <see cref="Gentle.Common.IBindable"/> interface. If you don't do this, adding
	/// and removing objects in the DataGrid will not work properly.</p>
	/// </remarks>
	public class TypedArrayList : CollectionBase, ITypedList, IBindingList
	{
		#region Members
		// Occurs when the list changes or an item in the list changes
		private ListChangedEventHandler listChanged;
		/// <summary>
		///  The type of the objects in the list.
		/// </summary>
		protected Type containedType;
		// A list of PropertyInfo objects for the properties of the containing objects
		private ArrayList properties = new ArrayList();
		// Flags, if the current TypedArrayList is sorted
		private bool isSorted; // initialized to false by .NET CLR
		// The direction of sorting
		private ListSortDirection listSortDirection = ListSortDirection.Ascending;
		// The Descriptor of the property which is the current sort column
		private PropertyDescriptor sortProperty;
		// Fixed size flag (to allow write protection of the list)
		private bool isFixedSize;
		#endregion //End of Members

		#region Constructors
		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="containedType">The type of the object stored in the ArrayList</param>
		public TypedArrayList( Type containedType )
		{
			this.containedType = containedType;
			AnalyseObjectType();
		}
		#endregion //End of Constructors

		#region Events
		/// <summary>
		/// Implementation of <see cref="System.ComponentModel.IBindingList.ListChanged"/>
		/// </summary>
		public event ListChangedEventHandler ListChanged
		{
			add { listChanged += value; }
			remove { listChanged -= value; }
		}

		/// <summary>
		/// Fires the ListChanged event for all attached listeners.
		/// </summary>
		/// <param name="e">The <see cref="System.ComponentModel.ListChangedEventArgs"/> for the event.</param>
		protected void OnListChanged( ListChangedEventArgs e )
		{
			if( listChanged != null )
			{
				listChanged( this, e );
			}
		}
		#endregion Events

		#region Methods
		/// <summary>
		/// Store a list of all property methods of the type stored in the ArrayList. 
		/// </summary>
		/// <remarks>
		/// The list is used by the <see cref="System.ComponentModel.ITypedList.GetItemProperties"/>.
		/// Because using Reflection is time consuming the property methods are reflected once
		/// while constructing an object of the TypedArrayList class.
		/// </remarks>
		private void AnalyseObjectType()
		{
			foreach( PropertyInfo propertyInfo in containedType.GetProperties() )
			{
				properties.Add( propertyInfo );
			}
		}

		/// <summary>
		/// Implements the RemoveObject event for the object stored in the list.
		/// </summary>
		/// <param name="value">The object which causes the event</param>
		/// <param name="e">The event arguments</param>
		protected void RemoveChild( object value, EventArgs e )
		{
			Remove( value );
		}

		/// <summary>
		/// Helper method for the <see cref="Find"/> method. Checks
		/// if the current object meets the search crieria.
		/// </summary>
		/// <param name="data">The current object.</param>
		/// <param name="searchValue">The searched object.</param>
		/// <returns>´<b>True</b>, if the current object meets the search
		/// criteria, <b>false</b> if not.</returns>
		protected bool Match( object data, object searchValue )
		{
			if( data == null || searchValue == null )
			{
				return data == searchValue;
			}
			Type dataType = data.GetType();

			if( dataType != searchValue.GetType() )
			{
				throw new ArgumentException( "Objects must be of the same type" );
			}

			bool isString = dataType == typeof(string);
			if( ! (dataType.IsValueType || isString) )
			{
				throw new ArgumentException( "Objects must be value types or strings" );
			}

			if( isString )
			{
				return String.Compare( (string) data, (string) searchValue, true, CultureInfo.CurrentCulture ) == 0;
			}
			else
			{
				return Comparer.Default.Compare( data, searchValue ) == 0;
			}
		}
		#endregion Methods

		#region Overridden CollectionBase methods
		/// <summary>
		/// See <see cref="System.Collections.CollectionBase.OnClearComplete"/>
		/// </summary>
		protected override void OnClearComplete()
		{
			OnListChanged( new ListChangedEventArgs( ListChangedType.Reset, 0 ) );
		}

		/// <summary>
		/// See <see cref="System.Collections.CollectionBase.OnInsertComplete"/>
		/// </summary>
		protected override void OnInsertComplete( int index, object value )
		{
			OnListChanged( new ListChangedEventArgs( ListChangedType.ItemAdded, index ) );
		}

		/// <summary>
		/// See <see cref="System.Collections.CollectionBase.OnRemoveComplete"/>
		/// </summary>
		protected override void OnRemoveComplete( int index, object value )
		{
			OnListChanged( new ListChangedEventArgs( ListChangedType.ItemDeleted, index, 0 ) );
		}

		/// <summary>
		/// See <see cref="System.Collections.CollectionBase.OnSetComplete"/>
		/// </summary>
		protected override void OnSetComplete( int index, object oldValue, object newValue )
		{
			OnListChanged( new ListChangedEventArgs( ListChangedType.ItemChanged, index, 0 ) );
		}
		#endregion CollectionBase base methods

		#region ITypedList Members
		/// <summary>
		/// Implementation of <see cref="System.ComponentModel.ITypedList.GetItemProperties"/>.
		/// </summary>
		/// <remarks>
		/// For each property method of the type contained in the list, a <c>PropertyDescriptor</c>
		/// will be created. If a property is flagged <i>not visible</i>, it will be omitted.
		/// The <c>TypedArrayList</c> uses a custom implementation of the PropertyDescriptor,
		/// <see cref="CaptionPropertyDescriptor"/>, to support interpreting attributes.
		/// </remarks>
		public PropertyDescriptorCollection GetItemProperties( PropertyDescriptor[] listAccessors )
		{
			ArrayList descriptors = new ArrayList();
			foreach( PropertyInfo propertyInfo in properties )
			{
				object[] attributes = propertyInfo.GetCustomAttributes( typeof(VisibleAttribute), true );
				CaptionPropertyDescriptor cpd = null;
				if( attributes.Length == 1 )
				{
					VisibleAttribute visible = (VisibleAttribute) attributes[ 0 ];
					if( visible.Value )
					{
						cpd = new CaptionPropertyDescriptor( propertyInfo.Name, propertyInfo );
					}
				}
				else
				{
					cpd = new CaptionPropertyDescriptor( propertyInfo.Name, propertyInfo );
				}
				if( cpd != null && cpd.OriginalDescriptor != null )
				{
					descriptors.Add( cpd );
				}
			}
			PropertyDescriptor[] propertyDescriptors = new PropertyDescriptor[descriptors.Count];
			descriptors.CopyTo( propertyDescriptors, 0 );
			return new PropertyDescriptorCollection( propertyDescriptors );
		}

		/// <summary>
		/// Implementation of <see cref="System.ComponentModel.ITypedList.GetListName"/>.
		/// </summary>
		/// <returns>The name of the type of the objects in the list.</returns>
		public string GetListName( PropertyDescriptor[] listAccessors )
		{
			return containedType.Name;
		}
		#endregion ITypedList Members

		#region IBindingList Members
		/// <summary>
		/// Implementation of <see cref="System.ComponentModel.IBindingList.AddIndex"/>.
		/// </summary>
		public void AddIndex( PropertyDescriptor property )
		{
			isSorted = true;
			sortProperty = property;
		}

		/// <summary>
		/// Implementation of <see cref="System.ComponentModel.IBindingList.AllowNew"/>.
		/// </summary>
		public bool AllowNew
		{
			get { return ! IsFixedSize; }
		}

		/// <summary> 
		/// Sorts a TypedArrayList with the selected property.
		/// </summary> 
		public void SortBy( string propertyName, ListSortDirection direction )
		{
			try
			{
				PropertyDescriptor myProperty = TypeDescriptor.GetProperties( containedType ).Find( propertyName, true );
				ApplySort( myProperty, direction );
			}
			catch( Exception ex )
			{
				throw new ArgumentException( "The contained type " + containedType.FullName +
				                             " does not contain a property named " + propertyName, ex );
			}
		}

		/// <summary>
		/// Implementation of <see cref="System.ComponentModel.IBindingList.ApplySort"/>.
		/// </summary>
		public void ApplySort( PropertyDescriptor property, ListSortDirection direction )
		{
			isSorted = true;
			sortProperty = property;
			listSortDirection = direction;

			ArrayList a = new ArrayList();

			if( property is CaptionPropertyDescriptor )
			{
				CaptionPropertyDescriptor prop = property as CaptionPropertyDescriptor;
				if( prop.AllowSort )
				{
					InnerList.Sort( new ObjectPropertyComparer( prop.OriginalName ) );
					if( direction == ListSortDirection.Descending )
					{
						InnerList.Reverse();
					}
				}
			}
			else
			{
				InnerList.Sort( new ObjectPropertyComparer( property.Name ) );
				if( direction == ListSortDirection.Descending )
				{
					InnerList.Reverse();
				}
			}
		}

		/// <summary>
		/// Implementation of <see cref="System.ComponentModel.IBindingList.SortProperty"/>.
		/// </summary>
		public PropertyDescriptor SortProperty
		{
			get { return sortProperty; }
		}

		/// <summary>
		/// Implementation of <see cref="System.ComponentModel.IBindingList.Find"/>.
		/// </summary>
		public int Find( PropertyDescriptor property, object key )
		{
			foreach( object o in this )
			{
				if( Match( containedType.GetProperty( property.Name ).GetValue( o, null ), key ) )
				{
					return IndexOf( o );
				}
			}
			return -1;
		}

		/// <summary>
		/// Implementation of <see cref="System.ComponentModel.IBindingList.SupportsSorting"/>.
		/// </summary>
		public bool SupportsSorting
		{
			get { return true; }
		}

		/// <summary>
		/// Implementation of <see cref="System.ComponentModel.IBindingList.IsSorted"/>.
		/// </summary>
		public bool IsSorted
		{
			get { return isSorted; }
		}

		/// <summary>
		/// Implementation of <see cref="System.ComponentModel.IBindingList.AllowRemove"/>.
		/// </summary>
		public bool AllowRemove
		{
			get { return ! IsFixedSize; }
		}

		/// <summary>
		/// Implementation of <see cref="System.ComponentModel.IBindingList.SupportsSearching"/>.
		/// </summary>
		public bool SupportsSearching
		{
			get { return true; }
		}

		/// <summary>
		/// Implementation of <see cref="System.ComponentModel.IBindingList.SortDirection"/>.
		/// </summary>
		public ListSortDirection SortDirection
		{
			get { return listSortDirection; }
		}

		/// <summary>
		/// Implementation of <see cref="System.ComponentModel.IBindingList.SupportsChangeNotification"/>.
		/// </summary>
		public bool SupportsChangeNotification
		{
			get { return true; }
		}

		/// <summary>
		/// Implementation of <see cref="System.ComponentModel.IBindingList.RemoveSort"/>.
		/// </summary>
		public void RemoveSort()
		{
			isSorted = false;
			sortProperty = null;
		}

		/// <summary>
		/// Implementation of <see cref="System.ComponentModel.IBindingList.AddNew"/>.
		/// </summary>
		/// <remarks>
		/// The implementation uses reflection to instantiate an object of the current 
		/// object type. If the types contained in the list do not implement the IBindable
		/// interface, adding new objects to the list will not work properly.
		/// </remarks>
		public object AddNew()
		{
			object o = containedType.InvokeMember( null,
			                                       BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic |
			                                       BindingFlags.Instance | BindingFlags.CreateInstance,
			                                       null, null, null );
			EventInfo evt = containedType.GetEvent( "RemoveObject", BindingFlags.Instance | BindingFlags.Public );
			if( evt != null )
			{
				Delegate deleg = Delegate.CreateDelegate( evt.EventHandlerType, this, "RemoveChild" );
				evt.AddEventHandler( o, deleg );
			}
			OnValidate( o );
			OnInsert( InnerList.Count, o );
			int index = InnerList.Add( o );
			try
			{
				OnInsertComplete( index, o );
			}
			catch( Exception )
			{
				RemoveAt( index );
				throw;
			}
			return o;
		}

		/// <summary>
		/// Implementation of <see cref="System.ComponentModel.IBindingList.AllowEdit"/>.
		/// </summary>
		public bool AllowEdit
		{
			get { return true; }
		}

		/// <summary>
		/// Implementation of <see cref="System.ComponentModel.IBindingList.RemoveIndex"/>.
		/// </summary>
		public void RemoveIndex( PropertyDescriptor property )
		{
			sortProperty = null;
		}
		#endregion IBindingList Members

		#region IList Members (defined in Collection Base)
		/// <summary>
		/// Implementation of <see cref="System.Collections.IList.IsReadOnly"/>.
		/// </summary>
		public bool IsReadOnly
		{
			get { return InnerList.IsReadOnly; }
		}

		/// <summary>
		/// Implementation of <see cref="System.Collections.IList.this"/>.
		/// </summary>
		public object this[ int index ]
		{
			get { return InnerList[ index ]; }
			set
			{
				Check.Verify( value != null && value.GetType().Equals( containedType ),
				              Error.InvalidType, value.GetType(), containedType );
				InnerList[ index ] = value;
				OnListChanged( new ListChangedEventArgs( ListChangedType.ItemChanged, index, 0 ) );
			}
		}

		/// <summary>
		/// Implementation of <see cref="System.Collections.IList.Insert"/>.
		/// </summary>
		public void Insert( int index, object value )
		{
			Check.Verify( value != null && value.GetType().Equals( containedType ),
			              Error.InvalidType, value.GetType(), containedType );
			InnerList.Insert( index, value );
			OnListChanged( new ListChangedEventArgs( ListChangedType.ItemAdded, index, 0 ) );
		}

		/// <summary>
		/// Implementation of <see cref="System.Collections.IList.Remove"/>.
		/// </summary>
		public void Remove( object value )
		{
			int index = InnerList.IndexOf( value );
			InnerList.Remove( value );
			OnListChanged( new ListChangedEventArgs( ListChangedType.ItemDeleted, index, 0 ) );
		}

		/// <summary>
		/// Implementation of <see cref="System.Collections.IList.RemoveAt"/>.
		/// </summary>
		public new void RemoveAt( int index )
		{
			InnerList.RemoveAt( index );
			OnListChanged( new ListChangedEventArgs( ListChangedType.ItemDeleted, index, 0 ) );
		}

		/// <summary>
		/// Implementation of <see cref="System.Collections.IList.Contains"/>.
		/// </summary>
		public bool Contains( object value )
		{
			return InnerList.Contains( value );
		}

		/// <summary>
		/// Implementation of <see cref="System.Collections.IList.Clear"/>.
		/// </summary>
		public new void Clear()
		{
			InnerList.Clear();
			OnListChanged( new ListChangedEventArgs( ListChangedType.Reset, 0 ) );
		}

		/// <summary>
		/// Implementation of <see cref="System.Collections.IList.IndexOf"/>.
		/// </summary>
		public int IndexOf( object value )
		{
			return InnerList.IndexOf( value );
		}

		/// <summary>
		/// Implementation of <see cref="System.Collections.IList.Add"/>.
		/// </summary>
		public int Add( object value )
		{
			Check.Verify( value != null && value.GetType().Equals( containedType ),
			              Error.InvalidType, value.GetType(), containedType );
			int index = InnerList.Add( value );
			OnListChanged( new ListChangedEventArgs( ListChangedType.ItemAdded, index, 0 ) );
			return index;
		}

		/// <summary>
		/// Implementation of <see cref="System.Collections.IList.IsFixedSize"/>.
		/// </summary>
		public bool IsFixedSize
		{
			get { return isFixedSize ? true : InnerList.IsFixedSize; }
			set { isFixedSize = value; }
		}
		#endregion

		#region ICollection Members (defined in CollectionBase)
		/// <summary>
		/// Implementation of <see cref="System.Collections.ICollection.IsSynchronized"/>.
		/// </summary>
		public bool IsSynchronized
		{
			get { return InnerList.IsSynchronized; }
		}

		/// <summary>
		/// Implementation of <see cref="System.Collections.ICollection.CopyTo"/>.
		/// </summary>
		public void CopyTo( Array array, int index )
		{
			bool throwException = false;
			Type objType = null;
			foreach( object obj in array )
			{
				if( array == null )
				{
					throwException = true;
					break;
				}
				objType = obj.GetType();
				if( ! objType.Equals( containedType ) )
				{
					throwException = true;
					break;
				}
			}
			if( throwException )
			{
				Check.Fail( Error.InvalidType, objType, containedType );
			}
			InnerList.CopyTo( array, index );
		}

		/// <summary>
		/// Implementation of <see cref="System.Collections.ICollection.SyncRoot"/>.
		/// </summary>
		public object SyncRoot
		{
			get { return InnerList.SyncRoot; }
		}
		#endregion
	}
}
using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using Gentle.Common;
using Gentle.Common.Attributes;

/*
 * Typed Array Item to work with the Gentle.NET object library
 * Copyright (C) 2005 Julian Bright
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: TypedArrayItemBase.cs 1232 2008-03-14 05:36:00Z mm $
 */

namespace Gentle.Framework
{
	/// <summary>
	/// Validate event handler
	/// </summary>
	public delegate void ValidateEventHandler( object sender, EventArgs e );

	/// <summary>
	/// Typed array item base class that implements IBindable and takes care
	/// of save and restore the original values for the parent object
	/// </summary>
	public class TypedArrayItemBase : Persistent, IBindable
	{
		#region Private Fields
		private object parentInstance;
		private Type parentType;
		private ArrayList properties = new ArrayList();
		private bool isNew = true;
		private bool isDirty;
		private bool isBeingEdited;
		private ArrayList oldValues = new ArrayList();
		#endregion

		/// <summary>
		/// Default constructor that marks the object as new.
		/// A parent should set the IsNew to false for a loaded record
		/// </summary>
		public TypedArrayItemBase()
		{
			// Set the parent instance and type
			parentInstance = this;
			parentType = GetType();
			// Analyse the objects properties to build up a value list
			AnalyseObjectType();
		}

		/// <summary>
		/// Validation event that the user can implement if they need to validate the
		/// object before completing an edit.  An exception should be thrown if error
		/// </summary>
		public event ValidateEventHandler ValidateObject;

		#region Methods
		/// <summary>
		/// The validate method for ensuring the object is well formed
		/// </summary>
		public virtual void OnValidate()
		{
			if( ValidateObject != null )
			{
				ValidateObject( this, new EventArgs() );
			}
		}

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
			foreach( PropertyInfo propertyInfo in parentType.GetProperties() )
			{
				// skip properties which we cannot both read and write, since we need to do both
				// to get and restore the property values
				if( propertyInfo.CanRead && propertyInfo.CanWrite )
				{
					properties.Add( propertyInfo );
				}
			}
		}
		#endregion

		#region Public Properties
		/// <summary>
		/// The is new property is set by the parent
		/// </summary>
		[Visible( false )]
		protected bool IsNew
		{
			get { return isNew; }
			set { isNew = value; }
		}
		/// <summary>
		/// Indicates if the object has been modified
		/// </summary>
		[Visible( false )]
		protected bool IsDirty
		{
			get { return isDirty; }
		}
		/// <summary>
		/// Property to return if the object is currently being edited
		/// </summary>
		[Visible( false )]
		protected bool IsBeingEdited
		{
			get { return isBeingEdited; }
		}
		#endregion

		#region IEditableObject Members
		/// <summary>
		/// The remove object event to call back into the TypedArrayList
		/// </summary>
		public event RemoveEventHandler RemoveObject;

		/// <summary>
		/// 
		/// </summary>
		void IEditableObject.BeginEdit()
		{
			if( isBeingEdited )
			{
				return;
			}
			isBeingEdited = true;

			// iterate over each of the properties, and save the old values that are visible
			foreach( PropertyInfo propertyInfo in properties )
			{
				object[] attributes = propertyInfo.GetCustomAttributes( typeof(VisibleAttribute), true );
				if( attributes.Length == 1 )
				{
					VisibleAttribute visible = (VisibleAttribute) attributes[ 0 ];
					if( visible.Value )
					{
						oldValues.Add( Reflector.GetValue( propertyInfo, parentInstance ) );
					}
				}
				else
				{
					oldValues.Add( Reflector.GetValue( propertyInfo, parentInstance ) );
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		void IEditableObject.EndEdit()
		{
			// make sure that the object is actually being edited.
			if( ! isBeingEdited )
			{
				return;
			}

			// call the validate method if need to raise events
			OnValidate();

			// there is nothing to do with the temporary values that we kept
			// since the values are already committed. Therefore, we clear
			// the temporary object array.
			oldValues.Clear();

			// set editting to false
			isBeingEdited = false;

			// set the dirty flag to true and new to false
			isDirty = true;
			isNew = false;
		}

		/// <summary>
		/// 
		/// </summary>
		void IEditableObject.CancelEdit()
		{
			// make sure that the object is actually being edited.
			if( ! isBeingEdited )
			{
				return;
			}

			// invoke RemoveObject(), this should remove the item of the array
			if( isNew && RemoveObject != null )
			{
				RemoveObject( this, new EventArgs() );
			}

			// restore the old values back since we are not a new object
			int i = 0;
			foreach( PropertyInfo propertyInfo in properties )
			{
				bool restore = propertyInfo.CanWrite;
				object[] attributes = propertyInfo.GetCustomAttributes( typeof(VisibleAttribute), true );
				if( attributes.Length == 1 )
				{
					VisibleAttribute visible = (VisibleAttribute) attributes[ 0 ];
					restore &= visible.Value;
				}
				if( restore )
				{
					Reflector.SetValue( propertyInfo, parentInstance, oldValues[ i++ ] );
				}
			}
			// set the dirty flag to false
			isDirty = false;
		}
		#endregion
	}
}
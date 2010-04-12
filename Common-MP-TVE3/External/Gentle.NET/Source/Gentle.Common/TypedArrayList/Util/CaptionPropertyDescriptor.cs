/*
 * A property descriptor for the TypedArrayList
 * Copyright (C) 2004 Andreas Seibt
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: CaptionPropertyDescriptor.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.ComponentModel;
using System.Reflection;
using Gentle.Common.Attributes;

namespace Gentle.Common.Util
{
	/// <summary>
	/// A custom <see cref="System.ComponentModel.PropertyDescriptor"/> implementation.
	/// </summary>
	/// <remarks>
	/// <p>The CaptionPropertyDescriptor is used in the implementation of the 
	/// <see cref="System.ComponentModel.ITypedList"/> interface. 
	/// It enhances the <see cref="System.ComponentModel.PropertyDescriptor"/>
	/// to handle the custom attributes <c>Caption</c>, <c>ReadOnly</c> and <c>AllowSort</c>.</p>
	/// </remarks>
	public class CaptionPropertyDescriptor : PropertyDescriptor
	{
		#region Members
		// The PropertyInfo for a property of the class contained in TypedArrayList
		private PropertyInfo propertyInfo;
		// A copy of the standard PropertyDescriptor for a property of the class contained in TypedArrayList
		private PropertyDescriptor originalDescriptor;
		#endregion Members

		#region Constructors
		/// <summary>
		/// Initialize a new <c>CaptionPropertyDescriptor</c> with the specified name
		/// and <c>PropertyInfo</c>.
		/// </summary>
		/// <param name="name">The name of the property</param>
		/// <param name="propertyInfo">The <c>PropertyInfo</c> for the property.</param>
		internal CaptionPropertyDescriptor( string name, PropertyInfo propertyInfo ) : base( name, null )
		{
			this.propertyInfo = propertyInfo;
			originalDescriptor = FindOrigPropertyDescriptor( propertyInfo );
		}
		#endregion Constructors

		#region Overridden base accessors
		/// <summary>
		/// Gets the type of the component this property is bound to.
		/// <seealso cref="System.ComponentModel.PropertyDescriptor.ComponentType">
		/// <c>System.ComponentModel.PropertyDescriptor.ComponentType()</c>
		/// </seealso>
		/// </summary>
		/// <value>A <see cref="System.Type"><c>System.Type</c></see> that represents the type of component 
		/// this property is bound to. When <see cref="System.ComponentModel.PropertyDescriptor.GetValue"><c>PropertyDescriptor.GetValue</c></see>
		/// or <see cref="System.ComponentModel.PropertyDescriptor.SetValue"><c>PropertyDescriptor.SetValue</c></see>
		/// are invoked, the object specified might be an instance of this type.</value>
		public override Type ComponentType
		{
			get { return originalDescriptor.ComponentType; }
		}

		/// <summary>
		/// Gets the name that can be displayed in a window, such as a Properties window.
		/// If the <c>Caption</c> attribute is defined for the property representing by this
		/// <c>PropertyDescriptor</c>, the attribute's value is returned
		/// instead of the original name.
		/// <seealso cref="System.ComponentModel.MemberDescriptor.DisplayName">
		/// <c>System.ComponentModel.PropertyDescriptor.DisplayName</c>
		/// </seealso>.
		/// </summary>
		/// <value>The name to display for the member.</value>
		public override string DisplayName
		{
			get
			{
				object[] attributes = propertyInfo.GetCustomAttributes( typeof(CaptionAttribute), true );
				if( attributes.Length == 1 )
				{
					CaptionAttribute caption = (CaptionAttribute) attributes[ 0 ];
					return caption.Value;
				}
				return base.DisplayName;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this property is read-only.
		/// <seealso cref="System.ComponentModel.PropertyDescriptor.IsReadOnly">
		/// <c>System.ComponentModel.PropertyDescriptor.IsReadOnly()</c>
		/// </seealso>
		/// </summary>
		/// <returns><b>true</b>, if the <c>ReadOnly</c> is defined for the current property of the class.</returns>
		public override bool IsReadOnly
		{
			get
			{
				bool isReadonly = propertyInfo.GetCustomAttributes( typeof(ReadOnlyAttribute), true ).Length != 0;
				return originalDescriptor.IsReadOnly || isReadonly;
			}
		}

		/// <summary>
		/// Gets the name of the member.
		/// <seealso cref="System.ComponentModel.MemberDescriptor.Name" />
		/// </summary>
		/// <value>The name of the member.</value>
		/// <remarks>
		/// <p>If the <c>Caption</c> attribute is defined for the property representing by this
		/// <c>PropertyDescriptor</c>, the attribute's value is returned
		/// instead of the original name.</p>
		/// This property is only overridden because the <c>DataGrid</c> don't use
		/// the <see cref="System.ComponentModel.MemberDescriptor.Name"><c>PropertyDescriptor.Name</c></see>
		/// property.
		/// </remarks>
		public override string Name
		{
			get
			{
				//				object[] attributes = propertyInfo.GetCustomAttributes( typeof(CaptionAttribute), true );
				//				if( attributes.Length == 1 )
				//				{
				//					CaptionAttribute caption = (CaptionAttribute) attributes[ 0 ];
				//					return caption.Value;
				//				}

				// always return the actual member name here
				return base.Name;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this property is read-only.
		/// <seealso cref="System.ComponentModel.PropertyDescriptor.PropertyType" />
		/// </summary>
		/// <value><b>true</b> if the property is read-only; otherwise, <b>false</b>.</value>
		public override Type PropertyType
		{
			get { return originalDescriptor.PropertyType; }
		}
		#endregion Overridden base accessors

		#region Overridden base methods
		/// <summary>
		/// Returns whether resetting an object changes its value.
		/// <seealso cref="System.ComponentModel.PropertyDescriptor.CanResetValue" />
		/// </summary>
		/// <param name="component">The component to test for reset capability.</param>
		/// <returns><b>true</b> if resetting the component changes its value; otherwise, <b>false</b>.</returns>
		public override bool CanResetValue( object component )
		{
			return originalDescriptor.CanResetValue( component );
		}

		/// <summary>
		/// Gets the current value for this property on a component.
		/// <seealso cref="System.ComponentModel.PropertyDescriptor.GetValue" />
		/// </summary>
		/// <param name="component">The component with the property for which to retrieve the value.</param>
		/// <returns>The value of a property for a given component.</returns>
		public override object GetValue( object component )
		{
			return originalDescriptor.GetValue( component );
		}

		/// <summary>
		/// Resets the value for this property of the component to the default value.
		/// <seealso cref="System.ComponentModel.PropertyDescriptor.ResetValue">
		/// <c>System.ComponentModel.PropertyDescriptor.ResetValue</c>
		/// </seealso>
		/// </summary>
		/// <param name="component">The component with the property value that is to be reset to the default value.</param>
		public override void ResetValue( object component )
		{
			originalDescriptor.ResetValue( component );
		}

		/// <summary>
		/// Sets the value of the component to a different value.
		/// <seealso cref="System.ComponentModel.PropertyDescriptor.SetValue">
		/// <c>System.ComponentModel.PropertyDescriptor.SetValue()</c>
		/// </seealso>
		/// </summary>
		/// <param name="component">The component with the property value that is to be set.</param>
		/// <param name="value">The new value.</param>
		public override void SetValue( object component, object value )
		{
			originalDescriptor.SetValue( component, value );
		}

		/// <summary>
		/// Determines a value indicating whether the value of this property needs to be persisted.
		/// <seealso cref="System.ComponentModel.PropertyDescriptor.ShouldSerializeValue">
		/// <c>System.ComponentModel.PropertyDescriptor.ShouldSerialzeValue</c>
		/// </seealso>
		/// </summary>
		/// <param name="component">The component with the property to be examined for persistence.</param>
		/// <returns><b>true</b> if the property should be persisted; otherwise, <b>false</b>.</returns>
		public override bool ShouldSerializeValue( object component )
		{
			return originalDescriptor.ShouldSerializeValue( component );
		}
		#endregion Overridden base methods

		#region Accessors
		/// <summary>
		/// Returns a value that indicates if the property which is represented by the member
		/// allows sorting in the <c>DataGrid</c>.
		/// </summary>
		/// <value><b>true</b>, if the <c>ReadOnly</c> is defined for the current property of the class.</value>
		public bool AllowSort
		{
			get
			{
				object[] attributes = propertyInfo.GetCustomAttributes( typeof(AllowSortAttribute), true );
				if( attributes.Length == 1 )
				{
					AllowSortAttribute sorting = (AllowSortAttribute) attributes[ 0 ];
					return sorting.Value;
				}
				else
				{
					return false;
				}
			}
		}

		/// <summary>
		/// Returns the original member display name.
		/// <seealso cref="System.ComponentModel.MemberDescriptor.DisplayName">
		/// <c>System.ComponentModel.PropertyDescriptor.DisplayName</c>
		/// </seealso>.
		/// </summary>
		/// <value>The display name of the member.</value>
		public string OriginalDisplayName
		{
			get { return base.DisplayName; }
		}

		/// <summary>
		/// Returns the original member name.
		/// <seealso cref="System.ComponentModel.MemberDescriptor.Name">
		/// <c>System.ComponentModel.PropertyDescriptor.Name</c>
		/// </seealso>.
		/// </summary>
		/// <value>The name of the member.</value>
		public string OriginalName
		{
			get { return base.Name; }
		}

		/// <summary>
		/// The original descriptor.
		/// </summary>
		public PropertyDescriptor OriginalDescriptor
		{
			get { return originalDescriptor; }
		}
		#endregion Accessors

		#region Methods
		/// <summary>
		/// Retrieve the standard <c>PropertyDescriptor</c> for a property.
		/// </summary>
		/// <param name="propertyInfo">The property for which the <c>PropertyDescriptor</c> should be searched.</param>
		/// <returns>
		/// The standard <c>PropertyDescriptor</c> for the property or <b>null</b>
		/// if no <c>PropertyDescriptor</c> was found.</returns>
		private static PropertyDescriptor FindOrigPropertyDescriptor( PropertyInfo propertyInfo )
		{
			foreach( PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties( propertyInfo.DeclaringType ) )
			{
				if( propertyDescriptor.Name.Equals( propertyInfo.Name ) )
				{
					return propertyDescriptor;
				}
			}
			return null;
		}
		#endregion Methods
	}
}
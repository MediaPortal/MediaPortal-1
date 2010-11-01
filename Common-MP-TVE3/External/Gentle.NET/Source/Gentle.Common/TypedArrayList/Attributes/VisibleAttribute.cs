/*
 * Attribute for use with the TypedArrayList
 * Copyright (C) 2004 Andreas Seibt
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: VisibleAttribute.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;

namespace Gentle.Common.Attributes
{
	/// <summary>
	/// Controlling the visibility of a property in a DataGrid control.
	/// </summary>
	/// <remarks>
	/// <p>This attribute is intended to uses with the <see cref="TypedArrayList"/>
	/// class. For the objects contained in the <c>TypedArrayList</c> you can determine if the 
	/// property should be visible in the DataGrid control.</p>
	/// <p>Per default (if this attribute isn't present) all public properties of the objects contained
	/// in the <c>TypedArrayList</c> are visible columns in the DataGrid control.</p>
	/// <p>If you set the visibility of a property in your class to false, you can't access it in the
	/// DataGrid control because there is no column for this property. If you want to access the property
	/// in the DataGrid you have to set the ColumnWidth of the DataGrid column to zero.</p>
	/// </remarks>
	/// <example>
	/// <code>
	/// public class Person
	/// {
	///   private string mName;
	/// 	
	///   <b>[Visible(false)]</b>
	///   public string Name
	///   {
	///     get { return mName; }
	///     set { mName = value; }
	///   }
	/// }
	/// </code>
	/// </example>
	[AttributeUsage( AttributeTargets.Property, AllowMultiple = false )]
	public sealed class VisibleAttribute : Attribute
	{
		private bool isVisible;

		/// <summary>
		/// Default constructor for the attribute.
		/// </summary>
		/// <param name="visible">
		/// <div class="tablediv">
		/// <table class="dtTABLE" cellspacing="0">
		///		<tr>
		///			<td><b>true</b></td>
		///			<td>The property is a visible column in the DataGrid control</td>
		///		</tr>
		///		<tr>
		///			<td><b>false</b></td>
		///			<td>The property will not be shown in the DataGrid control</td>
		///		</tr>
		/// </table>
		/// </div>
		/// </param>
		public VisibleAttribute( bool visible )
		{
			isVisible = visible;
		}

		/// <summary>
		/// The attribute's value.
		/// </summary>
		/// <value>The column header text for a <c>DataGrid</c>.</value>
		public bool Value
		{
			get { return isVisible; }
		}
	}
}
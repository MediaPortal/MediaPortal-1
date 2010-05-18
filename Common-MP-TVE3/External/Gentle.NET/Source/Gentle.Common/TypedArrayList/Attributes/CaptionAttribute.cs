/*
 * Attribute for use with the TypedArrayList
 * Copyright (C) 2004 Andreas Seibt
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: CaptionAttribute.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;

namespace Gentle.Common.Attributes
{
	/// <summary>
	/// Defining the caption of a column in a DataGrid control.
	/// </summary>
	/// <remarks>
	/// <p>This attribute is intended to uses with the <see cref="TypedArrayList"/> class.
	/// Per default the DataGrid control retrieve the names of all public property methods of the objects
	/// which are displayed in the DataGrid control by Reflection and uses them as column headers.
	/// The <c>TypedArrayList</c> use this attribute to display an alternative caption.</p> 
	/// </remarks>
	/// <example>
	/// <code>
	/// public class Person
	/// {
	///   private string mName;
	/// 	
	///   <b>[Caption("Last Name")]</b>
	///   public string Name
	///   {
	///     get { return mName; }
	///     set { mName = value; }
	///   }
	/// }
	/// </code>
	/// </example>
	[AttributeUsage( AttributeTargets.Property, AllowMultiple = false )]
	public sealed class CaptionAttribute : Attribute
	{
		private string caption;

		/// <summary>
		/// Default constructor for the attribute.
		/// </summary>
		/// <param name="caption">
		/// The display name of the property in the column header of a <c>DataGrid</c>.
		/// </param>
		public CaptionAttribute( string caption )
		{
			this.caption = caption;
		}

		/// <summary>
		/// The attribute's value.
		/// </summary>
		/// <value>
		/// The column header text for a <c>DataGrid</c>.
		/// </value>
		public string Value
		{
			get { return caption; }
		}
	}
}
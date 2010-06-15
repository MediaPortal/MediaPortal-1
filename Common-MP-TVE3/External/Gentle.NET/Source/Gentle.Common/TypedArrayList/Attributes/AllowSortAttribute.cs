/*
 * Attribute for use with the TypedArrayList
 * Copyright (C) 2004 Andreas Seibt
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: AllowSortAttribute.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;

namespace Gentle.Common.Attributes
{
	/// <summary>
	/// Controlling the Sorting capbility of a column in a DataGrid control.
	/// </summary>
	/// <remarks>
	/// <p>This attribute is intended to uses with the <see cref="TypedArrayList"/>
	/// class. For the objects contained in the <c>TypedArrayList</c> you can determine if a property of
	/// the object may act as a sortable column in the corresponding DataGrid control.</p>
	/// <p>Per default (if the attribute isn't used) sorting capability is enabled.</p>
	/// </remarks>
	/// <example>
	/// <code>
	/// public class Person
	/// {
	///   private string mName;
	/// 	
	///   <b>[AllowSort(false)]</b>
	///   public string Name
	///   {
	///     get { return mName; }
	///     set { mName = value; }
	///   }
	/// }
	/// </code>
	/// </example>
	[AttributeUsage( AttributeTargets.Property, AllowMultiple = false )]
	public sealed class AllowSortAttribute : Attribute
	{
		private bool isSortAllowed = true;

		/// <summary>
		/// Default constructor for the attribute.
		/// </summary>
		/// <param name="allowSort">
		/// <div class="tablediv">
		/// <table class="dtTABLE" cellspacing="0">
		///		<tr>
		///			<td><b>true</b></td>
		///			<td>Sorting is enabled for the corresponding <c>DataGrid</c> column</td>
		///		</tr>
		///		<tr>
		///			<td><b>false</b></td>
		///			<td>Sorting is disabled for the corresponding <c>DataGrid</c> column</td>
		///		</tr>
		/// </table>
		/// </div>
		/// </param>
		public AllowSortAttribute( bool allowSort )
		{
			isSortAllowed = allowSort;
		}

		/// <summary>
		/// Returns the current value of the attribute.
		/// </summary>
		/// <value>
		/// A boolean flag, which tells if the corresponding <c>DataGrid</c> column
		/// is sortable or not.
		/// </value>
		public bool Value
		{
			get { return isSortAllowed; }
		}
	}
}
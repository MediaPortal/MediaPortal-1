/*
 * The core attributes for decorating business objects
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: ForeignKeyAttribute.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using Gentle.Common;

namespace Gentle.Framework
{
	/// <summary>
	/// Use this attribute to mark properties referencing data in a remote table. It is
	/// used by the framework to map property (and the corresponding column) names of one
	/// object to other objects.
	/// </summary>
	[AttributeUsage( AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true )]
	public sealed class ForeignKeyAttribute : Attribute
	{
		private string foreignTable;
		private string foreignColumn;

		/// <summary>
		/// Construct a new ForeignKey attribute. This will relate the decorated property
		/// to the specified column in the given table.
		/// </summary>
		/// <param name="foreignTable">The table to relate to</param>
		/// <param name="foreignColumn">The column to relate to</param>
		public ForeignKeyAttribute( string foreignTable, string foreignColumn )
		{
			this.foreignTable = foreignTable;
			this.foreignColumn = foreignColumn;
		}

		/// <summary>
		/// Construct a new ForeignKey attribute. This will relate the decorated property
		/// to the specified column in the given table.
		/// </summary>
		/// <param name="foreignReference">The table and column to relate to (formatted as "table.column").</param>
		public ForeignKeyAttribute( string foreignReference )
		{
			int splitPos = foreignReference.IndexOf( "." );
			Check.Verify( splitPos > 0, "Invalid argument (must use \"Table.Column\" notation)." );
			foreignTable = foreignReference.Substring( 0, splitPos );
			foreignColumn = foreignReference.Substring( splitPos + 1, foreignReference.Length - splitPos - 1 );
		}

		/// <summary>
		/// The table to which this instance relates.
		/// </summary>
		public string ForeignTable
		{
			get { return foreignTable; }
		}

		/// <summary>
		/// The column to which this instance relates.
		/// </summary>
		public string ForeignColumn
		{
			get { return foreignColumn; }
		}
	}
}
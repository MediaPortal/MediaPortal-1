/*
 * The core attributes for decorating business objects
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: TableNameAttribute.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using Gentle.Common;

namespace Gentle.Framework
{
	/// <summary>
	/// Use this attribute on classes that should be persistable. Only classes decorated
	/// with this attribute are supported by the persistence framework.
	/// </summary>
	[AttributeUsage( AttributeTargets.Class, AllowMultiple = false, Inherited = true )]
	public sealed class TableNameAttribute : Attribute
	{
		private string name;
		private string schema;
		private CacheStrategy cacheStrategy = GentleSettings.DefaultCacheStrategy;

		/// <summary>
		/// The constructor for the TableName attribute.
		/// </summary>
		/// <param name="name">The name of the database table used to store instances of this class.</param>
		public TableNameAttribute( string name )
		{
			this.name = name;
		}

		/// <summary>
		/// The constructor for the TableName attribute.
		/// </summary>
		/// <param name="name">The name of the database table used to store instances of this class.</param>
		/// <param name="strategy">The cache stratgey to use for instances of this type. <see 
		/// cref="CacheStrategy"/> for a list of available options.</param>
		public TableNameAttribute( string name, CacheStrategy strategy )
		{
			this.name = name;
			cacheStrategy = strategy;
		}

		/// <summary>
		/// The name of the database table used to store instances of this class.
		/// </summary>
		public string Name
		{
			get { return name; }
		}

		/// <summary>
		/// The optional schema name with which to prefix the table name in queries.
		/// This value overrides the default schema definition (if present) in the
		/// configuration file. Note: this property is currently unused. 
		/// </summary>
		public string Schema
		{
			get { return schema; }
			set { schema = value; }
		}

		/// <summary>
		/// The cache behavior for objects of this type. <see cref="CacheStrategy"/> 
		/// for a list of available options.
		/// </summary>
		public CacheStrategy CacheStrategy
		{
			get { return cacheStrategy; }
			set { cacheStrategy = value; }
		}
	}
}
/*
 * Namespace Summary
 * Copyright (C) 2004 Morten Mertner
 *
 * This library is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included license.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: NamespaceDoc.cs 807 2005-07-10 20:36:01Z mm $
 */

namespace Gentle.Framework
{
	/* This class is a dummy class used by NDoc to provide a namespace summary. */

	/// <summary>
	/// <p>The <see cref="Gentle.Framework"/> namespace contains the persistence framework.</p><br/>
	/// <list type="bullet">
	///   <item>
	///     <description>The main access point into the framework functionality is provided 
	///		  by the <see cref="PersistenceBroker"/> class. The <see cref="Broker"/> class
	///		  allows you to use a default PersistenceBroker instance (using the provider and
	///		  connection information specified using the DefaultProvider config setting).</description></item>
	///   <item>
	///     <description>
	///       <para>To create persistable business objects decorate the class using the
	///         <see cref="TableNameAttribute"/> attribute and all persistent properties using the 
	///         <see cref="TableColumnAttribute"/> attribute.</para>
	///       <para>Depending on how much metadata Gentle is able to obtain from the database, you may
	///         need to decorate primary and foreign keys with the <see cref="PrimaryKeyAttribute"/> and 
	///         <see cref="ForeignKeyAttribute"/> attributes respectively.</para> 
	///       <para>Business objects may optionally inherit from the <see cref="Persistent"/> class (or implement
	///         the <see cref="IPersistent"/> interface) and gain additional features, such as a set of convenient 
	///         access methods. It is also possible to use the framework without using these.</para>
	///       <para>A returning issue with object to database mapping is handling of null values. If
	///         a value type property (such as an int or bool) maps to a column that allows null
	///         values, use the NullValue property of the TableColumn attribute to specify the
	///         value that will be assigned to properties on read and converted into null on 
	///         insert.</para></description></item>
	///   <item>
	///     <description>     
	///       <para>To execute custom SQL statements use the <see cref="SqlStatement"/> class. Use the 
	///         <see cref="SqlBuilder"/> class to construct instances of this class whenever the statement
	///         is for a supported type (i.e. a class decorated with the proper attributes).</para>
	///       <para>Query results (when executed directly) are always packaged in the <see cref="SqlResult"/> 
	///         class, which holds any returned rows and has useful properties (such as standardized 
	///         return codes). The RetrieveInstance and RetriveList methods allow you to bypass the 
	///         result and have Gentle create objects automatically.</para>
	///       <para>Multiple statements (including non-SQL commands) can be transacted (grouped) using the 
	///         <see cref="Transaction"/> class.</para></description>
	///   </item>
	/// </list>
	/// <br/>
	/// <p>The source code of the Gentle.Framework project is organized as follows:</p>
	/// <list type="table">
	///   <listheader>
	///     <term>Folder</term>
	///     <description>Description</description></listheader>
	///   <item>
	///     <term>Attributes</term>
	///     <description>All attributes defined by Gentle with which you can decorate your objects
	///     and thus gain use of the provided functionality.</description></item>
	///   <item>
	///     <term>Client</term>
	///     <description>All classes relevant to clients of the framework, that is, readers of 
	///     this document.</description></item>
	///   <item>
	///     <term>Core</term>
	///     <description>The core functionality of the framework. Some of these are also of
	///       interest to clients/users.</description></item>
	///   <item>
	///     <term>General</term>
	///     <description>Helper classes for various purposes, such as config file handling, 
	///     error reporting, etc.</description></item>
	///   <item>  
	///     <term>Interfaces</term>
	///     <description>Interfaces used internally by Gentle.</description></item>
	///   <item>  
	///     <term>Provider</term>
	///     <description>Base classes to use when implementing support for a database provider.</description></item>
	///   <item>  
	///     <term>Query</term>
	///     <description>Classes used when constructing custom SQL queries in a database-independent way.
	///     Note that classes in this folder is currently work in progress.</description></item>
	/// </list>
	/// <p>Enjoy! :-)</p>
	/// </summary>
	public class NamespaceDoc
	{
		private NamespaceDoc()
		{
		}
	}
}
/*
 * Enumeration of possible error conditions
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: Error.cs 1232 2008-03-14 05:36:00Z mm $
 */

namespace Gentle.Common
{
	/// <summary>
	/// <p>This enumeration lists all common error conditions, their severity and a 
	/// default error message.</p>
	/// <p>Unspecified errors or errors with no severity attribute are treated as critical.</p>
	/// <p>At this time almost all errors have been brainlessly classified as critical pending
	/// a review at some future time.</p>
	/// </summary>
	public enum Error
	{
		/// <summary>
		/// This error is used when no connection to the database server could be
		/// established. This is usually caused by errors in the connection
		/// string, but can also be due to network or database server problems.
		/// </summary>
		[Level( Severity.Critical ),
		 Message( "The database backend (provider {0}) could not be reached.\r\n" +
		          "Check the connection string: {1}" )]
		DatabaseUnavailable,

		/// <summary>
		/// This error is used when an error in the use of Gentle was detected 
		/// (e.g. invalid use of or missing custom attributes). No default message
		/// is provided for this error (if used via the Check class the first
		/// argument will be used as format string for remaining arguments).
		/// </summary>
		[Level( Severity.Error )]
		DeveloperError,

		/// <summary>
		/// This error is used when a list parameter with content (i.e. not null
		/// and with at least one element) was expected.
		/// </summary>
		[Level( Severity.Error ),
		 Message( "The given collection parameter {0} was null or empty." )]
		EmptyListParameter,

		/// <summary>
		/// This error is used when a DateTime has an invalid NullValue.
		/// </summary>
		[Level( Severity.Error ),
		 Message( "The column {0} has an invalid NullValue." )]
		InvalidNullValue,

		/// <summary>
		/// This error is used when Gentle fails to load a provider library defined
		/// in the configuration file.
		/// </summary>
		[Level( Severity.Warning ),
		 Message( "The assembly {0} could not be loaded or is not a valid Gentle provider." )]
		InvalidProviderLibrary,

		/// <summary>
		/// This error is used when Gentle detects an invalid request, such as trying to
		/// execute a missing stored procedure. It is also used when a request cannot be
		/// satisfied (e.g. trying to obtain an SqlStatement from SqlBuilder before all
		/// required information has been set).
		/// </summary>
		[Level( Severity.Error ),
		 Message( "The operation could not be performed: {0}" )]
		InvalidRequest,

		/// <summary>
		/// This error is used when an object is added to a TypedArrayList, and the objects
		/// type does not match the type specified when the list was created.
		/// </summary>
		[Level( Severity.Error ),
		 Message( "Unable to add object of type {0} to a list of {1} instances." )]
		InvalidType,

		/// <summary>
		/// This error is used when Gentle fails to find a matching node (for a required
		/// setting) in the configuration store.
		/// </summary>
		[Level( Severity.Critical ),
		 Message( "Missing configuration key: No XML node was found using XPath {0}." )]
		MissingConfigurationKey,

		/// <summary>
		/// This error is used when Gentle finds a member (field or property) carrying
		/// the TableColumn attribute that references a non-existing column.
		/// </summary>
		[Level( Severity.Critical ),
		 Message( "The member {0} on class {1} maps to a non-existing column named {2}." )]
		NoColumnForMember,

		/// <summary>
		/// This error is used when the Broker class is used and no DefaultProvider was
		/// defined in the configuration file.
		/// </summary>
		[Level( Severity.Critical ),
		 Message( "No <DefaultProvider> entry found in the configuration file." )]
		NoDefaultProvider,

		/// <summary>
		/// This error is used when a row identity was expected but none was received. 
		/// This usually only happens if the analyzer has been disabled (or is incomplete 
		/// for the provider used) and the table definition is out of sync with the TableName
		/// attribute definition used.
		/// </summary>
		[Level( Severity.Error ),
		 Message( "The database did not return a new row identity when one was expected." )]
		NoIdentityReturned,

		/// <summary>
		/// This error is used when no new connection to the database server could be
		/// established. This is usually caused by a resource leak that drains the
		/// connection pool before the GC can reclaim them, but can also be caused
		/// by a variety of other problems.
		/// </summary>
		[Level( Severity.Critical ),
		 Message( "Could not open a new database connection (provider {0}).\r\n" +
		          "Make sure you do not have a resource leak that drains the connection pool.\r\n" +
		          "Connection string used: {1}" )]
		NoNewConnection,

		/// <summary>
		/// This error is used when no type is associated with a table (the available
		/// databases are enumerated by the analyzer when in "Full" mode). Errors of
		/// this type can safely be ignored.
		/// </summary>
		[Level( Severity.Info ),
		 Message( "No business object maps to the table {0} (found in the database during schema analysis)." )]
		NoObjectMapForTable,

		/// <summary>
		/// This error is used when no member was found with the specified name. This usually
		/// occurs due to spelling or case sensitivity errors in string parameters.
		/// </summary>
		[Level( Severity.Error ),
		 Message( "Class {0} does not have a property named {1}." )]
		NoProperty,

		/// <summary>
		/// This error is used when a column is found that does not permit null values, yet the
		/// type to which it maps does not have a member mapping to this column. This error is thrown
		/// by the database analyzer in order to prevent later errors when trying to insert to the
		/// table. 
		/// </summary>
		[Level( Severity.Error ),
		 Message( "The column {0} cannot be null but type {1} does not define a corresponding member.\r\n" +
		          "Verify that the member carries the TableColumn attribute and has the correct column name." )]
		NoPropertyForNotNullColumn,

		/// <summary>
		/// This error is used when no database provider libraries were defined in the configuration.
		/// </summary>
		[Level( Severity.Critical ),
		 Message( "No database providers were specified in the configuration file (section <Providers>), " +
		          "or the corresponding assembly files could not be found. Make sure that you have added " +
		          "a reference to the provider libraries so that they get copied to the output folder." )]
		NoProviders,

		/// <summary>
		/// This error is used when a non-existing query parameter is referenced. This usually occurs
		/// due to spelling or case sensitivity errors.
		/// </summary>
		[Level( Severity.Critical ),
		 Message( "The query had no parameter named {0}." )]
		NoSuchParameter,

		/// <summary>
		/// This error is used when no matching database record was found. This can be
		/// the result of a concurrency error or due to invalid parameter values.
		/// </summary>
		[Level( Severity.Warning ),
		 Message( "No record for type {0} could be found using key {1}.\r\n" +
		          "Additional information: {2}" )]
		NoSuchRecord,

		/// <summary>
		/// This error is used when no configuration store could be found. Gentle tries to
		/// obtain its configuration from both the standard .NET configuration file (using
		/// the <see cref="GentleSectionHandler"/> class to read the &lt;gentle&gt; section)
		/// and the standalone Gentle.config file.
		/// </summary>
		[Level( Severity.Critical ),
		 Message( "No configuration store could be found." )]
		NoConfigStoreFound,

		/// <summary>
		/// This error is used when a non-existing view is referenced. This usually occurs
		/// due to spelling or case sensitivity errors in parameters from client-side code.
		/// </summary>
		[Level( Severity.Error ),
		 Message( "No view named {0} was found for type {1}." )]
		NoSuchView,

		/// <summary>
		/// This error is used when the requested method has not yet been implemented. 
		/// This usually occurs due to incomplete development code being accessed.
		/// </summary>
		[Level( Severity.Critical ),
		 Message( "The requested feature has not yet been implemented. {0}" )]
		NotImplemented,

		/// <summary>
		/// This error is used when a null parameter is passed to a method that does not
		/// permit null values. This usually occurs due to errors in client-side code.
		/// </summary>
		[Level( Severity.Error ),
		 Message( "The parameter {0} was null (not allowed here)." )]
		NullParameter,

		/// <summary>
		/// This error is used when trying to set the identity value on a class, but the
		/// class does not have an identity column.
		/// </summary>
		[Level( Severity.Error ),
		 Message( "Cannot set the identity value (type {0} does not define any identity members)." )]
		NullIdentity,

		/// <summary>
		/// This error is used when a member was null, but a not-null value was required.
		/// This usually occurs due to errors in client-side code.
		/// </summary>
		[Level( Severity.Error ),
		 Message( "The property named {0} of class {1} was null (not allowed here)." )]
		NullProperty,

		/// <summary>
		/// This error is used when a column contained a null value and the corresponding
		/// member does not permit null values. This usually occurs due to a missing NullValue
		/// declaration on the TableColumn attribute of the member.
		/// </summary>
		[Level( Severity.Error ),
		 Message( "The column {0} was null but no NullValue was declared for the corresponding property on class {1}." )]
		NullWithNoNullValue,

		/// <summary>
		/// This error is used when a property is read-only, but write access was required. This can
		/// usually be corrected by moving the TableColumn attribute to the underlying member, or by
		/// adding a setter for the property.
		/// </summary>
		[Level( Severity.Critical ),
		 Message( "The property named {0} of class {1} was read only." )]
		PropertyReadOnly,

		/// <summary>
		/// This error is used when a property is write-only, but read access was required. This can
		/// usually be corrected by moving the TableColumn attribute to the underlying member, or by
		/// adding a getter for the property.
		/// </summary>
		[Level( Severity.Critical ),
		 Message( "The property named {0} of class {1} was write only." )]
		PropertyWriteOnly,

		/// <summary>
		/// This error is used when statement execution fails. This is usually due to syntax errors
		/// in the specified SQL string.
		/// </summary>
		[Level( Severity.Error ),
		 Message( "An error was encountered while executing the query:\r\n" +
		          "{1}\r\n" + "The message from the backend was:\r\n" + "{0}" )]
		StatementError,

		/// <summary>
		/// This error is used when an assembly name cannot be resolved to an Assembly instance. This 
		/// may occur due to missing project references or invalid location of the required DLL. When
		/// the Inheritance attribute is used it may also occur due to invalid names in the 
		/// database column storing the full type name.
		/// </summary>
		[Level( Severity.Critical ),
		 Message( "The assembly named {0} could not be loaded." )]
		UnknownAssembly,

		/// <summary>
		/// This error is used when a provider cannot be found in the list of registered providers.
		/// This usually occurs due to a missing entry (or incorrect name) in the configuration file.   
		/// </summary>
		[Level( Severity.Error ),
		 Message( "The provider named {0} could not be found.\r\n" +
		          "Check the provider name in the config file, and make sure the dll is in the bin folder." )]
		UnknownProvider,

		/// <summary>
		/// This error is used when a type references a table that does not exist. This usually
		/// occurs due to spelling or case sensitivity errors in the declaration of the TableName
		/// attribute.
		/// </summary>
		[Level( Severity.Critical ),
		 Message( "The table {0} to which type {1} maps does not exist." )]
		UnknownTable,

		/// <summary>
		/// This error is used when the returned number of rows does not match the expected count.
		/// This can be due to concurrency (record changed by another process) or parameter value 
		/// errors.
		/// </summary>
		[Level( Severity.Error ),
		 Message( "The number of returned rows {0} did not match the expected count of {1}.\r\n" +
		          "If concurrency control is enabled this may indicate that the record was updated or deleted by another process." )]
		UnexpectedRowCount,

		/// <summary>
		/// This error is used for unclassified errors of all kinds.
		/// </summary>
		[Level( Severity.Critical )]
		Unspecified,

		/// <summary>
		/// This error is used when an unsupported database type is encountered during analysis
		/// of the database schema.
		/// </summary>
		[Level( Severity.Critical ),
		 Message( "The database type {0} is not yet supported by provider {1}." )]
		UnsupportedColumnType,

		/// <summary>
		/// This error is used when an explicit configuration file path (location of Gentle.config)
		/// was specified in the AppSettings section of the .NET configuration file, yet the path
		/// specified was not valid. This can be due to errors in the URI, an illegal path, or an
		/// unsupported destination (only files are supported).
		/// </summary>
		[Level( Severity.Error ),
		 Message( "The configuration file path \"{0}\" is not supported or invalid." )]
		UnsupportedConfigStoreUri,

		/// <summary>
		/// This error is used when an unsupported database function is called.
		/// </summary>
		[Level( Severity.Critical ),
		 Message( "This functionality is not yet supported by provider {1}." )]
		UnsupportedDatabaseFunction,

		/// <summary>
		/// This error is used when an unsupported member type is encountered. To correct the error
		/// you must remove the TableColumn attribute, or move it to a member of a different type.
		/// </summary>
		[Level( Severity.Critical ),
		 Message( "The member type {0} is not yet supported by {1}." )]
		UnsupportedPropertyType,

		/// <summary>
		/// This error is used when an unsupported type is passed as argument to a method that
		/// required the type to be supported. This usually occurs due to a missing TableName 
		/// attribute on the class declaration, but can also be remedied by having the class
		/// implement the ITableName interface.
		/// </summary>
		[Level( Severity.Critical ),
		 Message( "The supplied class {0} is not supported by Gentle.NET.\r\n" +
		          "Classes must either carry the TableName attribute or implement ITableName." )]
		UnsupportedType,

		/// <summary>
		/// This error is used when an error occurred during generation of a custom view.
		/// </summary>
		[Level( Severity.Critical ),
		 Message( "Unable to generate view named {0} for type {1}." )]
		ViewError,

		/// <summary>
		/// This error is used when a business object contains an incorrect value.
		/// </summary>
		[Level( Severity.Error ),
		 Message( "Validation error." )]
		Validation
	}
}
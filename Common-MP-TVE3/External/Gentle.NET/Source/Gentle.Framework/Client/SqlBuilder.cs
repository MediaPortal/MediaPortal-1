/*
 * An SQL query construction facility
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: SqlBuilder.cs 1234 2008-03-14 11:41:44Z mm $
 */

using System;
using System.Collections;
using System.Data;
using System.Text;
using Gentle.Common;

namespace Gentle.Framework
{
	/// <summary>
	/// Enumeration of the comparison operators supported in SQL statements.
	/// </summary>
	public enum Operator
	{
		/// <summary>
		/// This is the equals operator for strict equality comparisons. The framework automatically
		/// uses the correct syntax for null value comparisons.
		/// </summary>
		Equals,
		/// <summary>
		/// This is the negated equals operator. The framework automatically uses the correct syntax 
		/// for null value comparisons.
		/// </summary>
		NotEquals,
		/// <summary>
		/// This is the equals operator for partial equality comparisons. The parameter value 
		/// should be a string to match with percent characters used as wildcards.
		/// </summary>
		Like,
		/// <summary>
		/// This is the negated equals operator for partial equality comparisons. The parameter value 
		/// should be a string to match with percent characters used as wildcards. 
		/// </summary>
		NotLike,
		/// <summary>
		/// This is the less than operator.
		/// </summary>
		LessThan,
		/// <summary>
		/// This is the less than or equals operator.
		/// </summary>
		LessThanOrEquals,
		/// <summary>
		/// This is the greater than operator.
		/// </summary>
		GreaterThan,
		/// <summary>
		/// This is the greater than or equals operator.
		/// </summary>
		GreaterThanOrEquals,
		/// <summary>
		/// This is the in operator which tests for set membership. Constraints using Operator.In can 
		/// only be added by specifying a list of elements (though the lists may contain 0 or 1 elements). 
		/// For details, please refer to the corresponing SqlBuilder.AddConstraint methods. 
		/// </summary>
		In,
		/// <summary>
		/// This is the negated in operator which tests for set non-membership.
		/// </summary>
		NotIn
	} ;

	/// <summary>
	/// Enumeration of the logic operators supported in SQL statements. This LogicOperators can only be 
	/// used under with customConstraints. For details, please refer to the AddConstraint 
	/// method that accepts this as a parameter.
	/// </summary>
	public enum LogicalOperator
	{
		/// <summary>
		/// This is the AND operator.
		/// </summary>
		And,
		/// <summary>
		/// This is the OR operator
		/// </summary>
		Or
	} ;

	/// <summary>
	/// 	<p>This class can be used to construct instances of the <see cref="SqlStatement"/> class. As such,
	/// its use is comparable to that of the <see cref="StringBuilder"/> class.</p>
	/// 	<p>Using this class it is easy to construct fairly complex queries for supported types (see
	/// <see cref="TableNameAttribute"/>).</p>
	/// </summary>
	public class SqlBuilder : BrokerLock
	{
		#region Members
		// various internally used strings
		private const string fmtSelect = "select {0}{1} from {2} {3}{4}{5}{6}";
		private const string fmtTop = "top {0} ";
		private const string fmtWhere = " where {0}";
		private const string fmtConstraint = "{0} {1} {2} ";
		private const string fmtInsert = "insert into {0} ( {1} ) values ( {2} )";
		private const string fmtUpdate = "update {0} set {1}{2}";
		private const string fmtUpdateSet = "{0} = {1}";
		private const string fmtDelete = "delete from {0} {1}";
		private const string fmtOrderByField = "{0}{1} {2}";
		private const string fmtOrderBy = " order by {0}";
		private const string fmtRowLimit = " limit {0}";
		private const string fmtRowOffset = " offset {0}";

		// {0} = Field list of table, {1} = Field list of table,
		// {2} = original sql clause with order by
		// {3} = rowLimit + rowOffest, {4} = rowOffset 
		private const string fmtOracleLimit =
			"select {0} from (select {1}, rownum r from ({2}) where rownum <= {3}) where r > {4}";

		// Uwe Kitzmann: change template to support Sybase ASA dialect of paged queries
		private const string fmtSybaseASALimit = " top {0} ";
		private const string fmtSybaseASAOffset = " start at {0} ";

		// Firebird
		private const string fmtFirebirdLimit = " first {0} ";
		private const string fmtFirebirdOffset = " skip {0} ";

		// temporary fields used to hold data until the final SqlStatement is constructed
		private IDbCommand cmd;
		private StatementType stmtType;
		private string tableName;
		private ArrayList fields; // list of field names
		private ArrayList quotedFields; // list of quoted field names (same order as fields)
		private ArrayList constraints; //protected Hashtable constraints; // parameter name -> FieldMap instance
		private string customConstraint;
		private Hashtable parameterValues; // parameter name -> value
		private ArrayList parameterOrder; // for remembering parameter order when named params are unsupported
		private int rowLimit; // the maximum number of rows to return
		private int rowOffset; // the number of rows to skip
		private string orderBy = "";
		private string concurrencyField;
		private string logicalOperator = " and ";
		               // the logical operator that will be used with custom constraints. Defaults to " and " so nothing will break. ;)
		// additional framework objects used to perform our duties
		private IGentleProvider provider;
		private GentleSqlFactory sf;
		private ObjectMap map;
		#endregion

		#region Convenience Constructors (for Gentle 1.0.4 and lower compatibility)
		public SqlBuilder() : this( (IGentleProvider) null, StatementType.Unknown, null, LogicalOperator.And )
		{
		}

		public SqlBuilder( StatementType stmtType ) : this( (IGentleProvider) null, stmtType, null, LogicalOperator.And )
		{
		}

		public SqlBuilder( StatementType stmtType, Type type ) : this( (IGentleProvider) null, stmtType, type, LogicalOperator.And )
		{
		}
		#endregion

		#region Convenience Constructors (using PersistenceBroker)
		/// <summary>
		/// Construct a new SqlBuilder instance for constructing a statement of the given type
		/// for the specified business class type.
		/// </summary>
		/// <param name="broker">The PersistenceBroker instance to use for database access.</param>
		/// <param name="stmtType">The type of SQL statement to construct</param>
		/// <param name="type">The object class to work on</param>
		public SqlBuilder( PersistenceBroker broker, StatementType stmtType, Type type ) : this( broker, stmtType, type, LogicalOperator.And )
		{
		}

		public SqlBuilder( PersistenceBroker broker, StatementType stmtType ) : this( broker, stmtType, null, LogicalOperator.And )
		{
		}

		public SqlBuilder( PersistenceBroker broker ) : this( broker, StatementType.Unknown, null, LogicalOperator.And )
		{
		}

		/// <summary>
		/// Construct a new SqlBuilder instance for constructing a statement of the given type
		/// for the specified business class type.
		/// </summary>
		/// <param name="broker">The PersistenceBroker instance to use for database access.</param>
		/// <param name="stmtType">The type of SQL statement to construct</param>
		/// <param name="type">The object class to work on</param>
		/// <param name="logicalOperator">The logic operator used with constraints (can be either AND or OR)</param>
		public SqlBuilder( PersistenceBroker broker, StatementType stmtType, Type type, LogicalOperator logicalOperator ) :
			this( broker != null ? broker.Provider : null, stmtType, type, logicalOperator )
		{
		}
		#endregion

		#region Convenience Constructors (using IGentleProvider)
		/// <summary>
		/// Construct a new SqlBuilder instance for constructing a statement of the given type.
		/// </summary>
		/// <param name="provider">The IGentleProvider used to obtain an SqlFactory</param>
		public SqlBuilder( IGentleProvider provider ) : this( provider, StatementType.Unknown, null, LogicalOperator.And )
		{
		}

		/// <summary>
		/// Construct a new SqlBuilder instance for constructing a statement of the given type.
		/// </summary>
		/// <param name="provider">The IGentleProvider used to obtain an SqlFactory</param>
		/// <param name="stmtType">The type of SQL statement to construct</param>
		public SqlBuilder( IGentleProvider provider, StatementType stmtType ) : this( provider, stmtType, null, LogicalOperator.And )
		{
		}

		/// <summary> Construct a new SqlBuilder instance for constructing a statement of the 
		/// given type for the specified business class type.
		/// </summary> 
		/// <param name="provider">The IGentleProvider used to obtain an SqlFactory</param>
		/// <param name="stmtType">The type of SQL statement to construct</param>
		/// <param name="type">The object class to work on</param>
		public SqlBuilder( IGentleProvider provider, StatementType stmtType, Type type ) :
			this( provider, stmtType, type, LogicalOperator.And )
		{
		}
		#endregion

		#region Constructor (implementation)
		/// <summary>
		/// Construct a new SqlBuilder instance for constructing a statement of the given type
		/// for the specified business class type.
		/// </summary>
		/// <param name="provider">The IGentleProvider used to obtain an SqlFactory</param>
		/// <param name="stmtType">The type of SQL statement to construct</param>
		/// <param name="type">The object class to work on</param>
		/// <param name="logicalOperator">The logic operator used with constraints (can be either AND or OR)</param>
		public SqlBuilder( IGentleProvider provider, StatementType stmtType, Type type, LogicalOperator logicalOperator ) :
			base( provider != null ? new PersistenceBroker( provider ) : type != null ? new PersistenceBroker( type ) : null )
		{
			this.provider = provider ?? broker.Provider;
			sf = this.provider.GetSqlFactory();
			cmd = sf.GetCommand();
			this.stmtType = stmtType;
			map = type != null ? ObjectFactory.GetMap( SessionBroker, type ) : null;
			this.logicalOperator = logicalOperator == LogicalOperator.Or ? " or " : " and ";
			fields = new ArrayList(); // list of fields 
			quotedFields = new ArrayList(); // list of fields 
			constraints = new ArrayList(); // list of string constraints
			parameterValues = new Hashtable();
			parameterOrder = new ArrayList();
		}
		#endregion

		#region Misc
		/// <summary>
		/// Set the type of statement (select/insert/update/delete) to construct.
		/// </summary>
		/// <param name="stmtType"></param>
		public void SetStatementType( StatementType stmtType )
		{
			this.stmtType = stmtType;
		}

		/// <summary>
		/// <p>The character or string used to prefix parameters in SQL statements.</p>
		/// <p>The value of this property varies between different persistence engines.</p>
		/// </summary>
		public string ParameterPrefix
		{
			get { return sf.GetParameterPrefix(); }
		}

		/// <summary>
		/// <p>The character or string used to suffix parameters in SQL statements.</p>
		/// <p>The value of this property varies between different persistence engines.</p>
		/// </summary>
		public string ParameterSuffix
		{
			get { return sf.GetParameterSuffix(); }
		}

		/// <summary>
		/// <p>Get the statement for retrieving last inserted row id for auto-generated id columns.</p>
		/// <p>The value of this property varies between different persistence engines.</p>
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="om"></param>
		/// <returns></returns>
		public string GetIdentitySelect( string sql, ObjectMap om )
		{
			if( sf.HasCapability( Capability.BatchQuery ) )
			{
				return sf.GetIdentitySelect( sql, om );
			}
			else
			{
				//Since the persistence engine does not support Batch Command Processing
				//Two seperate commands must be executed.
				//Execute the results of GentleSqlFactory.GetIdentitySelect to get the last inserted row id
				return sql;
			}
		}
		#endregion

		#region SetTable Methods
		/// <summary>
		/// Set the name of the table to operate on.
		/// </summary>
		/// <param name="name">The table name to use in the constructed query.</param>
		public void SetTable( string name )
		{
			if( name != null ) // ignore trying to set a null value
			{
				tableName = name;
			}
		}

		/// <summary>
		/// Set the name of the table to operate on from the TableName attribute of the given type.
		/// </summary>
		/// <param name="type">The type from which to extract the table name.</param>
		public void SetTable( Type type )
		{
			ObjectMap om = ObjectFactory.GetMap( SessionBroker, type );
			SetTable( om.QuotedTableName );
		}
		#endregion

		#region Paging Methods
		/// <summary>
		/// Set the maximum number of rows to return. This setting is only used with select statements.
		/// </summary>
		/// <param name="rowLimit">The maximum number of rows to return. If the value is 0 or less no
		/// restrictions will be applied.</param>
		public void SetRowLimit( int rowLimit )
		{
			this.rowLimit = rowLimit > 0 ? rowLimit : 0;
		}

		/// <summary>
		/// Set the maximum number of rows to return. This setting is only used with select statements.
		/// </summary>
		/// <param name="rowOffset">The number of rows to skip. If the value is 0 or less no
		/// restrictions will be applied.</param>
		public void SetRowOffset( int rowOffset )
		{
			this.rowOffset = rowOffset > 0 ? rowOffset : 0;
		}
		#endregion

		#region AddField/AddOrderBy/AddConcurrencyControl Methods
		/// <summary>
		/// Add a property by which the result should be sorted. Multiple fields can be added and will
		/// be used in the order they are given. 
		/// This setting is only used with select statements.
		/// </summary>
		/// <param name="isAscending">Specifies the sort order applied to the given field</param>
		/// <param name="fieldName">The name of the property by which to order the result</param>
		public void AddOrderByField( bool isAscending, string fieldName )
		{
			Check.VerifyNotNull( map, Error.NotImplemented, "Support for order by requires that a " +
			                                                "type has been specified for this statement." );
			FieldMap fm = map.GetFieldMap( fieldName );
			// try looking for a matching column name if we couldn't find a property 
			if( fm == null )
			{
				fm = map.GetFieldMapFromColumn( fieldName );
			}
			Check.VerifyNotNull( fm, Error.NoProperty, map.Type, fieldName );
			// TODO check for unsupported types, e.g. "text" 
			orderBy += String.Format( fmtOrderByField, orderBy.Length > 0 ? ", " : "",
			                          fm.QuotedColumnName, isAscending ? "asc" : "desc" );
		}

		/// <summary>
		/// Add a custom clause to the order by section of the SQL statement.
		/// </summary>
		/// <param name="orderClause">The clause to add to the order by section. Examples for
		/// this parameter might be "name ASC" or "COALESCE(NameDE, NameEN) ASC".</param>
		public void AddOrderByField( string orderClause )
		{
			orderBy += String.Format( "{0}{1}", (orderBy.Length > 0 ? (", ") : ("")), orderClause );
		}

		/// <summary>
		/// Add all properties carrying the TableColumn attribute as fields (e.g. as selected columns
		/// for select statements).
		/// </summary>
		/// <param name="type">The type to process.</param>
		public void AddFields( Type type )
		{
			ObjectMap om = ObjectFactory.GetMap( SessionBroker, type );
			foreach( FieldMap fm in om.Fields )
			{
				bool addField = ! fm.IsPrimaryKey ||
				                (fm.IsPrimaryKey && ! fm.IsAutoGenerated && stmtType != StatementType.Update);
				addField &= ! fm.IsConcurrencyColumn || (GentleSettings.ConcurrencyControl && fm.IsConcurrencyColumn);
				addField &= ! fm.IsSoftDeleteColumn ||
				            (fm.IsConcurrencyColumn || (fm.IsSoftDeleteColumn && stmtType == StatementType.SoftDelete));

				// Exclude fields that are marked as readonly from insert and update statements
				if( (stmtType == StatementType.Insert || stmtType == StatementType.Update) && fm.IsReadOnly )
				{
					addField = false;
				}

				// Exclude fields that are marked as readonly from insert and update statements
				if( (stmtType == StatementType.Insert ||
				     stmtType == StatementType.Update ||
				     stmtType == StatementType.SoftDelete) && fm.IsReadOnly )
				{
					addField = false;
				}
				// Exclude PK fields from being updated
				//if ( stmtType == StatementType.SoftDelete && fm.IsPrimaryKey)
				//	addField = false;

				// prevent fields from being added twice
				if( addField && ! fields.Contains( fm.ColumnName ) )
				{
					fields.Add( fm.ColumnName );
					quotedFields.Add( fm.QuotedColumnName );
					// regular fields are also parameters for insert and update statements
					bool addParam = stmtType == StatementType.Insert || stmtType == StatementType.SoftDelete;
					addParam |= ! fm.IsPrimaryKey && stmtType == StatementType.Update;
					if( addParam )
					{
						// sf.HasCapability( Capability.NamedParameters ) && 
						bool isAltName = fm.IsConcurrencyColumn;
						isAltName &= stmtType == StatementType.Update || stmtType == StatementType.SoftDelete;
						string name = isAltName ? "New" + fm.ColumnName : fm.ColumnName;
						AddParameter( name, fm );
					}
				}
			}
		}

		/// <summary>
		/// Add all properties carrying both the TableColumn and PrimaryKey attributes
		/// as either fields or parameters.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="isParameter"></param>
		public void AddKeyFields( Type type, bool isParameter )
		{
			ObjectMap om = ObjectFactory.GetMap( SessionBroker, type );
			if( om != null )
			{
				foreach( FieldMap fm in om.Fields )
				{
					if( fm.IsPrimaryKey )
					{
						// prevent fields from being added twice
						if( ! fields.Contains( fm.ColumnName ) )
						{
							fields.Add( fm.ColumnName );
							quotedFields.Add( fm.QuotedColumnName );
						}
						// key fields are parameters for everything but insert statements
						//if( isParameter )
						//	AddParameter( fm );
					}
				}
			}
		}
		#endregion

		#region AddConstraint(s) Methods (using normal command parameters)
		/// <summary>
		/// Add all properties carrying both the TableColumn and PrimaryKey attributes
		/// as constraint parameters.
		/// </summary>
		/// <param name="type">The type to process.</param>
		[Obsolete]
		public void AddConstraints( Type type )
		{
			// by default (since this was 1.2.0 behavior), only apply checks to update statements
			AddConstraints( type, stmtType == StatementType.Update );
		}

		/// <summary>
		/// Add all properties carrying both the TableColumn and PrimaryKey attributes
		/// as constraint parameters.
		/// </summary>
		/// <param name="type">The type to process.</param>
		/// <param name="isWithConcurrency">True if concurrency restrictions should be applied.</param>
		public void AddConstraints( Type type, bool isWithConcurrency )
		{
			ObjectMap om = ObjectFactory.GetMap( SessionBroker, type );
			if( om != null )
			{
				foreach( FieldMap fm in om.Fields )
				{
					if( fm.IsPrimaryKey )
					{
						constraints.Add( fm.ColumnName );
						// constraints are parameters for everything but insert statements
						if( stmtType != StatementType.Insert )
						{
							AddParameter( fm );
						}
					}
						// concurrency control parameter needs to be added twice with update/delete statements
						// note: Gentle 1.2.0 and lower only checked concurrency on updates
					else if( isWithConcurrency && fm.IsConcurrencyColumn && GentleSettings.ConcurrencyControl )
					{
						constraints.Add( fm.ColumnName );
						AddParameter( fm.ColumnName, fm.Type );
						concurrencyField = fm.QuotedColumnName;
					}
				}
			}
		}

		/// <summary>
		/// Add all specified constraint parameters. The fields array must contain pairs
		/// consisting of a field name and its type. 
		/// </summary>
		/// <param name="op">The operator used in the constraint</param>
		/// <param name="fields">The fields to constrain on. Property names are preferred but
		/// column names can alse be used.</param>
		public void AddConstraints( Operator op, params object[] fields )
		{
			if( fields != null && fields.Length % 2 == 0 )
			{
				for( int i = 0; i < fields.Length; i = i + 2 )
				{
					AddConstraint( op, (string) fields[ i ], fields[ i + 1 ] );
				}
			}
		}

		/// <summary>
		/// Add a single fixed constraint with the given value and operator.
		/// Case insensitive on the LIKE operator.
		/// </summary>
		/// <param name="op">The operator used in the constraint</param>
		/// <param name="field">The field to constrain on. Property names are preferred but
		/// column names can alse be used.</param>
		/// <param name="value">The value used in the constraint</param>
		public void AddConstraint( Operator op, string field, object value )
		{
			AddConstraint( op, field, value, false );
		}

		/// <summary>
		/// Add a single fixed constraint with the given value and operator.
		/// </summary>
		/// <param name="op">The operator used in the constraint</param>
		/// <param name="field">The field to constrain on. Property names are preferred but
		/// column names can alse be used.</param>
		/// <param name="value">The value used in the constraint</param>
		/// <param name="isCaseSensitive">The indicator for setting the LIKE operator case sensitive</param>
		public void AddConstraint( Operator op, string field, object value, bool isCaseSensitive )
		{
			Check.VerifyNotNull( map, Error.DeveloperError, "You must specify the object type that is " +
			                                                "being selected before adding constraints. Use the SqlBuilder constructor for this." );
			Check.VerifyNotNull( field, Error.NullParameter, "field" );
			// assume input is property names and we need to use column names
			FieldMap fm = map.GetFieldMap( field );
			// try looking for a matching column name if we couldn't find a property 
			if( fm == null )
			{
				fm = map.GetFieldMapFromColumn( field );
			}
			string plainColumn = fm != null ? fm.ColumnName : field;
			string column = fm != null
			                	? fm.QuotedColumnName
			                	:
			                		sf.IsReservedWord( field ) ? sf.QuoteReservedWord( field ) : field;
			// determine if supplied value results in a parameter value of DBNull.Value
			object paramValue = SqlStatement.GetParameterValue( value, map,
			                                                    fm != null ? fm : map.GetFieldMapFromColumn( field ),
			                                                    stmtType );
			// add statement substring to custom constraints
			if( paramValue == DBNull.Value || paramValue == null )
			{
				// parameter is null - add plain text constraint
				AddConstraint( Format( "{0} {1} {2} {3}", column, AsOperatorBegin( op, true ),
				                       "NULL", AsOperatorEnd( op ) ) );
			}
			else
			{
				// automatically add % around parameters (if missing) for LIKE constraints
				if( op == Operator.Like || op == Operator.NotLike )
				{
					paramValue = paramValue.ToString();
					if( ((string) paramValue).IndexOf( '%' ) == -1 )
					{
						// TODO: Check all providers. MySql, SqlServer only case insensitive
						if( (provider.Name.StartsWith( "Oracle" ) || provider.Name.Equals( "PostgreSQL" )) && !isCaseSensitive )
						{
							column = "UPPER(" + column + ")";
							paramValue = "%" + paramValue.ToString().ToUpper() + "%";
						}
						else
						{
							// TODO: Check all providers
							Check.Verify( !provider.Name.StartsWith( "Oracle" ) && !provider.Name.Equals( "PostgreSQL" ) && isCaseSensitive,
							              Error.UnsupportedDatabaseFunction, "Only Oracle and PostgreSQL support case sensitive search" );
							paramValue = "%" + paramValue + "%";
						}
					}
				}
				string paramName = GetUniqueParamName( plainColumn );
				// add a normal parameterized constraint
				AddConstraint( Format( "{0} {1} {2} {3}", column, AsOperatorBegin( op, false ),
				                       AsParameter( paramName ), AsOperatorEnd( op ) ) );
				// add as parameter 
				if( fm != null )
				{
					AddParameter( paramName, fm );
				}
				else
				{
					AddParameter( paramName, paramValue != null ? paramValue.GetType() : null );
				}
				// store parameter value for later
				parameterValues[ paramName ] = paramValue;
			}
		}
		#endregion

		#region AddConstraint(s) Methods for text-formatted constraints (no command parameters)
		/// <summary>
		/// This method is for adding set membership constraints using either Operator.In or
		/// Operator.NotIn. If an invalid operator is used this method will throw an exception.
		/// </summary>
		/// <param name="op">The operator used for this constraint.</param>
		/// <param name="fieldMap">The FieldMap instance for the field to constrain. The framework 
		/// assumes property names but
		/// also checks column names if no property could be found.</param>
		/// <param name="data">The set of data to constrain on.</param>
		/// <param name="constraintMap">If the constraint data set holds objects this is the property
		/// name to gather from all objects. If the data set is an SqlResult this is the column name
		/// to use from all rows. String-type fields are quoted, all other types are not. This method
		/// performs no additional type checking and is likely to fail (when the statement is executed)
		/// for esoteric types such as decimals or dates.</param>
		public void AddConstraint( Operator op, FieldMap fieldMap, ICollection data, FieldMap constraintMap )
		{
			Check.VerifyNotNull( fieldMap, Error.NullParameter, "field" );
			Check.VerifyNotNull( data, Error.NullParameter, "data" );
			Check.IsTrue( op == Operator.In || op == Operator.NotIn, Error.DeveloperError,
			              "This AddConstraint method must be used only with the In and NotIn operators." );
			// TODO make this a config option (e.g. "strict" behavior)
			// Check.Verify( data.Count > 0, Error.EmptyListParameter, "data" );
			if( data.Count > 0 )
			{
				// convert supplied collection into comma-delimited list of values 
				string[] list = new string[data.Count];
				int count = 0;
				foreach( object obj in data )
				{
					if( constraintMap == null )
					{
						list[ count++ ] = Convert.ToString( obj );
					}
					else
					{
						list[ count++ ] = Convert.ToString( constraintMap.GetValue( obj ) );
					}
				}
				// determine whether field type needs quoting
				bool needsQuoting = fieldMap.Type.Equals( typeof(string) );
				needsQuoting |= fieldMap.Type.Equals( typeof(DateTime) );
				// quote GUIDs (only when they are not in binary form)
				needsQuoting |= fieldMap.Type.Equals( typeof(Guid) ) && (fieldMap.Size == 0 || fieldMap.Size > 16);
				// add the constraint
				AddConstraint( String.Format( "{0} {1} {2} {3}", fieldMap.QuotedColumnName,
				                              AsOperatorBegin( op, false ),
				                              AsCommaList( list, needsQuoting ),
				                              AsOperatorEnd( op ) ) );
			}
		}

		/// <summary>
		/// This method is for adding set membership constraints using either Operator.In or
		/// Operator.NotIn. If an invalid operator is used this method will throw an exception.
		/// </summary>
		/// <param name="op">The operator used for this constraint.</param>
		/// <param name="field">The name of the field to constrain. Gentle assumes a property name
		/// is specified but also checks for column names if no property matched.</param>
		/// <param name="data">The set of data to use in the constraint.</param>
		/// <param name="constraintField">If the constraint data set holds objects this is the property
		/// name whose values will be gathered from all objects. If null is passed the data collection
		/// is treated as list of plain values.</param>
		public void AddConstraint( Operator op, string field, ICollection data, string constraintField )
		{
			// first we need to determine whether data is a list of objects or plain values
			FieldMap constraintMap = null;
			if( data != null && data.Count > 0 && constraintField != null )
			{
				foreach( object obj in data )
				{
					if( ObjectFactory.IsTypeSupported( obj.GetType() ) )
					{
						ObjectMap om = ObjectFactory.GetMap( SessionBroker, obj );
						constraintMap = om.GetFieldMap( constraintField );
					}
					break; // always break (foreach is just a convenient way to access an ICollection)
				}
			}
			// find map for given field name
			FieldMap fm = map.GetFieldMap( field );
			if( fm == null )
			{
				fm = map.GetFieldMapFromColumn( field );
			}
			Check.VerifyNotNull( fm, Error.DeveloperError, "The type {0} being selected " +
			                                               "does not have a property or column named {1}.", map.Type, field );
			// delegate to other method - field will be null if data contained plain value types
			AddConstraint( op, fm, data, constraintMap );
		}

		/// <summary>
		/// This method is for adding set membership constraints using either Operator.In or
		/// Operator.NotIn. If an invalid operator is used this method will throw an exception.
		/// </summary>
		/// <param name="op">The operator used for this constraint.</param>
		/// <param name="field">The field to constrain. The framework assumes property names but
		/// also checks column names if no property could be found.</param>
		/// <param name="sr">The SqlResult containing the data used to constrain the field.</param>
		/// <param name="constraintField">This is the column name to use from all rows in the SqlResult. 
		/// String-type fields are quoted, all other types are not. This method performs no additional 
		/// type checking and is likely to fail (when the statement is executed) for esoteric types 
		/// such as decimals or dates.</param>
		public void AddConstraint( Operator op, string field, SqlResult sr, string constraintField )
		{
			if( sr != null && sr.RowsContained > 0 && sr.ColumnNames.Length > 0 )
			{
				AddConstraint( op, field, sr.TransposeToFieldList( constraintField, false ), null );
			}
		}

		/// <summary>
		/// This method is for adding set membership constraints using either Operator.In or
		/// Operator.NotIn. If an invalid operator is used this method will throw an exception.
		/// </summary>
		/// <param name="op">The operator used for this constraint.</param>
		/// <param name="field">The field to constrain. The framework assumes property names but
		/// also checks column names if no property could be found.</param>
		/// <param name="sr">The SqlResult containing the data used to constrain the field. The
		/// SqlResult must either contain only one column or contain a column named as the field
		/// parameter.</param>
		public void AddConstraint( Operator op, string field, SqlResult sr )
		{
			if( sr == null )
			{
				// resolve method overloading ambiguity by explicitly casting
				// the null value to object
				AddConstraint( op, field, (object) null );
			}
			else
			{
				string constraintField = null;
				if( sr.ColumnNames.Length == 1 )
				{
					constraintField = sr.ColumnNames[ 0 ];
				}
				else if( sr.ColumnNames.Length > 1 )
				{
					// no constraint field given and more than 1 column in result 
					// try to find column named field
					foreach( string column in sr.ColumnNames )
					{
						if( column.Equals( field ) )
						{
							constraintField = field;
							break;
						}
					}
				}
				else
				{
					Check.Fail( Error.NullParameter, "sr" );
				}
				Check.VerifyNotNull( constraintField, Error.NullParameter, "constraintField" );
				AddConstraint( op, field, sr, constraintField );
			}
		}

		/// <summary>
		/// Add a custom constraint as a single specified constraint clause. Use this for
		/// subselects or other "complicated" constraints (unfortunately, these cannot be 
		/// parameterized at the moment). This method will use the logicOperator, therefore
		/// if you have used a <see cref="LogicalOperator"/> it will be used here.
		/// </summary>
		/// <param name="clause">A valid constraint clause.</param>
		/// <example>AddConstraint( "SUM(MemberCount) > 5" )</example>
		public void AddConstraint( string clause )
		{
			customConstraint += (customConstraint != null) ? (logicalOperator + clause) : clause;
		}
		#endregion

		#region Internal Formatting Methods
		internal bool IsParamNameUsed( string name )
		{
			bool used = false;
			foreach( string existing in parameterValues.Keys )
			{
				used |= name == existing;
			}
			return used;
		}

		internal string GetUniqueParamName( string name )
		{
			int paramNumber = 1;
			string paramName = name;
			while( IsParamNameUsed( paramName ) )
			{
				// MS Access fails when appending an "X" or a number.. which is very strange! 
				// Appending the letter A seems to work.. bizarre! =)
				// Note: modified to append number inside A-A chars (which fixes GOPF-193,
				// yet hopefully still works with Access).
				paramName = String.Format( "{0}A{1}A", name, paramNumber++ );
			}
			return paramName;
		}

		/// <summary>
		/// Produce the actual SQL string for the specified <see cref="Operator"/>. This is the part
		/// of the SQL string between the column name and the parameter value.
		/// </summary>
		/// <param name="op">The operator to convert to SQL</param>
		/// <param name="isValueNull">This parameter indicates whether the value of the parameter is null. This
		/// is required because different operators must be used for null equality checls.</param>
		/// <returns>The SQL string for the specified operator</returns>
		protected virtual string AsOperatorBegin( Operator op, bool isValueNull )
		{
			switch( op )
			{
				case Operator.Equals:
					return isValueNull ? "is" : "=";
				case Operator.NotEquals:
					if( isValueNull )
					{
						return "is not";
					}
					else // TODO move checks like these to the GentleSqlFactory classes
					{
						return provider.Name == "Jet" ? "<>" : "!=";
					}
				case Operator.GreaterThan:
					return ">";
				case Operator.GreaterThanOrEquals:
					return ">=";
				case Operator.LessThan:
					return "<";
				case Operator.LessThanOrEquals:
					return "<=";
				case Operator.Like:
					return "like";
				case Operator.NotLike:
					return "not like";
				case Operator.In:
					return "in (";
				case Operator.NotIn:
					return "not in (";
				default:
					Check.Fail( Error.InvalidRequest, "Unable to format query string for unknown operator." );
					return null;
			}
		}

		/// <summary>
		/// Produce the actual SQL string for the specified <see cref="Operator"/>. This is the part
		/// of the SQL string after the parameter value. This string is usually empty.
		/// </summary>
		/// <param name="op">The operator to convert to SQL</param>
		/// <returns>The SQL string for the specified operator</returns>
		protected virtual string AsOperatorEnd( Operator op )
		{
			if( op == Operator.In || op == Operator.NotIn )
			{
				return ")";
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// This method will convert a list of field names into a comma delimited string of 
		/// unquoted field names.
		/// </summary>
		/// <param name="fields">The fields to delimit.</param>
		/// <returns>The comma delimited field list.</returns>
		protected virtual string AsCommaList( ICollection fields )
		{
			return AsCommaList( fields, false );
		}

		/// <summary>
		/// This method will convert a list of field names into a comma delimited string of 
		/// field names. Quoting of the individual fields is optional.
		/// </summary>
		/// <param name="fields">The fields to delimit.</param>
		/// <param name="quoteFields">Whether to apply quotes around the fields.</param>
		/// <returns>The coma delimited field list.</returns>
		protected virtual string AsCommaList( ICollection fields, bool quoteFields )
		{
			StringBuilder result = new StringBuilder();
			foreach( string s in fields )
			{
				if( result.Length > 0 )
				{
					result.Append( ", " );
				}
				if( quoteFields )
				{
					result.AppendFormat( "{0}{1}{0}", sf.GetQuoteCharacter(), s );
				}
				else
				{
					result.Append( s );
				}
			}
			return result.ToString();
		}

		/// <summary>
		/// This method returns the SQL string for restricting the number of rows if a limit
		/// has been specified and the empty string otherwise. 
		/// </summary>
		/// <returns>A fragment for use in an sql statement.</returns>
		protected virtual string GetTopLimit()
		{
			return rowLimit > 0 ? String.Format( fmtTop, rowLimit ) : "";
		}

		/// <summary>
		/// This method returns the SQL string for restricting the number of rows if a limit
		/// has been specified and the empty string otherwise. 
		/// </summary>
		/// <returns>A fragment for use in an sql statement.</returns>
		protected virtual string GetRowLimit()
		{
			return rowLimit > 0 ? String.Format( fmtRowLimit, rowLimit ) : "";
		}

		/// <summary>
		/// This method returns the SQL string for skipping past a number of rows. This is useful
		/// in combination with RowLimit for paging of data. 
		/// </summary>
		/// <returns>A fragment for use in an sql statement.</returns>
		protected virtual string GetRowOffset()
		{
			// always include offset if a row limit is applied (required for paging)
			return rowLimit > 0 || rowOffset > 0 ? String.Format( fmtRowOffset, rowOffset ) : "";
		}

		internal string GetPagedSql( int page, string sql, int rowLimit, ref int rowOffset )
		{
			string oldTop = string.Empty;
			string newTop = string.Empty;
			if( provider.Name == "SQLServer" )
			{
				int top = rowLimit + rowOffset;
				oldTop = String.Format( fmtTop, top );
				top = rowLimit * page;
				top = top <= 0 ? rowLimit : top;
				newTop = String.Format( fmtTop, top );
				rowOffset = top - rowLimit;
				return sql.Replace( oldTop, newTop );
			}
			else if( provider.Name.StartsWith( "Oracle" ) )
			{
				// select * from (select rownum r, ListName, SenderAddress, ListId from List where rownum <= 3 order by ListId) where r > 0  

				// fmtOracleLimit = "select * from ({0}) where rownum <= {1}{2}";
				int end = rowLimit + rowOffset;
				string oldEnd = String.Format( "rownum <= {0}", end );
				//				end = rowLimit * (page + 1);
				end = rowLimit * page;
				string newEnd = String.Format( "rownum <= {0}", end <= 0 ? rowLimit : end );
				//				string newEnd = String.Format( "rownum <= {0}", end ); 
				sql = sql.Replace( oldEnd, newEnd );
				// fmtOracleOffset = " and rownum > {0}";
				int start = rowOffset;
				string oldStart = String.Format( "r > {0}", start );
				//				start = rowLimit * page; 
				start = rowLimit * (page - 1);
				string newStart = String.Format( "r > {0}", start < 0 ? 0 : start );
				//				string newStart = String.Format( "r > {0}", start ); 
				rowOffset = start;
				return sql.Replace( oldStart, newStart );
			}
			else if( provider.Name.Equals( "Firebird" ) )
			{
				int offset = page <= 1 ? 0 : rowLimit * (page - 1);
				string oldOffset = String.Format( fmtFirebirdOffset, rowOffset );
				string newOffset = String.Format( fmtFirebirdOffset, offset < 0 ? 0 : offset );
				rowOffset = offset;
				return sql.Replace( oldOffset, newOffset );
			}
			else
			{
				int offset = page <= 1 ? 0 : rowLimit * (page - 1);
				string oldOffset = String.Format( fmtRowOffset, rowOffset );
				string newOffset = String.Format( fmtRowOffset, offset < 0 ? 0 : offset );
				rowOffset = offset;
				return sql.Replace( oldOffset, newOffset );
			}
		}

		/*
		internal string GetPagedSql( bool isNext, string sql, int rowLimit, int rowOffset )
		{
			if( provider.Name == "SQLServer" )
			{
				int top = rowLimit + rowOffset;
				string oldTop = String.Format( fmtTop, top );
				top = isNext ? 2 * rowLimit + rowOffset : rowOffset;
				string newTop = String.Format( fmtTop, top <= 0 ? rowLimit : top );
				return sql.Replace( oldTop, newTop );
			}
			else if( provider.Name.StartsWith( "Oracle" ) )
			{
				// select * from (select rownum r, ListName, SenderAddress, ListId from List where rownum <= 3 order by ListId) where r > 0  

				// fmtOracleLimit = "select * from ({0}) where rownum <= {1}{2}";
				int end = rowLimit + rowOffset;
				string oldEnd = String.Format( "rownum <= {0}", end );
				end = isNext ? end + rowLimit : end - rowLimit;
				string newEnd = String.Format( "rownum <= {0}", end <= 0 ? rowLimit : end );
				sql = sql.Replace( oldEnd, newEnd );
				// fmtOracleOffset = " and rownum > {0}";
				int start = rowOffset;
				string oldStart = String.Format( "r > {0}", start );
				start = isNext ? start + rowLimit : start - rowLimit;
				string newStart = String.Format( "r > {0}", start < 0 ? 0 : start );
				return sql.Replace( oldStart, newStart );
			}
			else if( provider.Name.Equals( "Firebird" ) )
			{
				int offset = isNext ? rowOffset + rowLimit : rowOffset - rowLimit;
				string oldOffset = rowOffset > 0 ? String.Format( fmtFirebirdOffset, rowOffset + 1 ) : "";
				string oldLimitAndOffset = rowLimit > 0 ? String.Format( fmtFirebirdLimit, rowLimit ) + oldOffset : oldOffset;
				string newOffset = String.Format( fmtFirebirdOffset, offset < 0 ? 1 : offset + 1 );
				string newLimitAndOffset = rowLimit > 0 ? String.Format( fmtFirebirdLimit, rowLimit ) + newOffset : newOffset;
				return sql.Replace( oldLimitAndOffset, newLimitAndOffset );
			}
				// Uwe Kitzmann: added support for Sybase ASA
			else if( provider.Name.StartsWith( "SybaseASA" ) )
			{
				int offset = isNext ? rowOffset + rowLimit : rowOffset - rowLimit;

				string oldOffset = rowOffset > 0 ? String.Format( fmtSybaseASAOffset, rowOffset + 1 ) : "";
				string oldLimitAndOffset = rowLimit > 0 ? String.Format( fmtSybaseASALimit, rowLimit ) + oldOffset : oldOffset;

				string newOffset = String.Format( fmtSybaseASAOffset, offset < 0 ? 1 : offset + 1 );
				string newLimitAndOffset = rowLimit > 0 ? String.Format( fmtSybaseASALimit, rowLimit ) + newOffset : newOffset;

				return sql.Replace( oldLimitAndOffset, newLimitAndOffset );
			}
			else
			{
				int offset = isNext ? rowOffset + rowLimit : rowOffset - rowLimit;
				string oldOffset = String.Format( fmtRowOffset, rowOffset );
				string newOffset = String.Format( fmtRowOffset, offset < 0 ? 0 : offset );
				return sql.Replace( oldOffset, newOffset );
			}
		}
		*/

		/// <summary>
		/// Formats the given table name for use in queries. This may include prefixing
		/// it with a schema name or suffixing it with an alias (for multi-table selects).
		/// </summary>
		/// <param name="tableName">The table name to format</param>
		/// <returns>The formatted table name</returns>
		protected virtual string GetTableName( string tableName )
		{
			return sf.GetTableName( tableName );
		}

		/// <summary>
		/// This method will format a list of fields for use in an update statement.
		/// </summary>
		/// <returns>A fragment for use in an sql statement.</returns>
		protected virtual string AsFieldUpdateList()
		{
			StringBuilder result = new StringBuilder();
			for( int i = 0; i < fields.Count; i++ )
			{
				string f = (string) fields[ i ];
				string qf = (string) quotedFields[ i ];
				if( result.Length > 0 )
				{
					result.Append( ", " );
				}
				bool isControlValue = concurrencyField != null && concurrencyField.Equals( f );
				result.Append( Format( fmtUpdateSet, qf, AsParameter( isControlValue ? "New" + f : f ) ) );
			}
			return result.ToString();
		}

		/// <summary>
		/// This method will format a list of fields as parameters for use in an sql statement.
		/// </summary>
		/// <param name="fields">The fields that are being updated.</param>
		/// <returns>A fragment for use in an sql statement.</returns>
		protected virtual string AsParameterList( ICollection fields )
		{
			StringBuilder result = new StringBuilder();
			foreach( string s in fields )
			{
				if( result.Length > 0 )
				{
					result.Append( ", " );
				}
				result.Append( AsParameter( s ) );
			}
			return result.ToString();
		}

		/// <summary>
		/// This method will format a single field name as a parameter for use in an sql statement.
		/// </summary>
		/// <param name="name">The parameter name.</param>
		/// <returns>A fragment for use in an sql statement.</returns>
		protected virtual string AsParameter( string name )
		{
			// Uwe Kitzmann: Added support for PositionalParameters which get replaced hard-coded here by a question mark
			if( sf.HasCapability( Capability.NamedParameters ) )
			{
				return ParameterPrefix + name + ParameterSuffix;
			}
			else
			{
				return "?";
			}
		}

		/// <summary>
		/// This method aggregates all the constraints specified previously by calls to the
		/// SqlBuilder.AddConstraint methods.
		/// </summary>
		/// <returns>A fragment for use in an sql statement.</returns>
		protected virtual string GetConstraints()
		{
			StringBuilder result = new StringBuilder();
			// add any field constraints 
			if( constraints != null && constraints.Count > 0 )
			{
				foreach( string c in constraints )
				{
					if( result.Length > 0 )
					{
						result.Append( logicalOperator );
					}
					string quoted = sf.IsReservedWord( c ) ? sf.QuoteReservedWord( c ) : c;
					//bool isNull = parameterValues.ContainsKey( c ) && parameterValues[ c ] == null;
					//result.Append( Format( fmtConstraint, quoted, isNull ? "is" : "=", AsParameter( c ) ) );
					result.Append( Format( fmtConstraint, quoted, "=", AsParameter( c ) ) );
				}
			}
			// add any custom constraints
			if( customConstraint != null && customConstraint.Length > 0 )
			{
				if( result.Length > 0 )
				{
					result.Append( logicalOperator );
				}
				result.Append( customConstraint );
			}
			return result.Length > 0 ? Format( fmtWhere, result.ToString() ) : "";
		}

		/// <summary>
		/// This method returns the "order by" clause formatted for use in SQL queries.
		/// </summary>
		protected string GetOrderBy()
		{
			return orderBy != null && orderBy.Length > 0 ? String.Format( fmtOrderBy, orderBy ) : "";
		}

		/// <summary>
		/// This method formats a string and is merely a shortcut ro the String.Format method.
		/// </summary>
		protected virtual string Format( string fmt, params object[] args )
		{
			return String.Format( fmt, args );
		}
		#endregion

		#region AddParameter Methods
		/// <summary>
		/// Add a parameter to the current statement.
		/// </summary>
		/// <param name="name">The parameter name to use if not the column name defined in the FieldMap</param>
		/// <param name="fm">The FieldMap describing this parameter</param>
		internal void AddParameter( string name, FieldMap fm )
		{
			//string paramName = sf.GetParameterPrefix() + (name != null ? name : fm.ColumnName) + sf.GetParameterSuffix();
			string paramName = name != null ? name : fm.ColumnName;
			string paramKey = sf.GetParameterCollectionKey( paramName );
			// only add parameters once
			// Uwe Kitzmann: data providers with positional parameters may need more than one occurence of the same parameter
			// Uwe Kitzmann: for example "update mytable set field1 = ?, field2 = ? where field1 = ? and field2 = ?"
			if( ! cmd.Parameters.Contains( paramKey ) || ! sf.HasCapability( Capability.NamedParameters ) )
			{
				if( fm.HasDbType )
				{
					// all insert/update columns must use this method
					sf.AddParameter( cmd, paramKey, fm.DbType );
				}
				else
				{
					sf.AddParameter( cmd, paramKey, fm.Type );
				}
				// also remember the order in which we add parameters (using the original name)
				parameterOrder.Add( paramName );
			}
		}

		internal void AddParameter( FieldMap fm )
		{
			AddParameter( null, fm );
		}

		/// <summary>
		/// Add a parameter to the current statement.
		/// </summary>
		/// <param name="name">The name of this parameter.</param>
		/// <param name="type">The system type of this parameter.</param>
		public void AddParameter( string name, Type type )
		{
			string paramName = sf.GetParameterPrefix() + name + sf.GetParameterSuffix();
			// only add parameters once
			if( ! cmd.Parameters.Contains( paramName ) || ! sf.HasCapability( Capability.NamedParameters ) )
			{
				sf.AddParameter( cmd, name, type );
				// also remember the order in which we add parameters
				parameterOrder.Add( name );
			}
		}

		/// <summary>
		/// Add a parameter to the current statement.
		/// </summary>
		/// <param name="name">The name of this parameter.</param>
		/// <param name="dbType">The database type of this parameter.</param>
		public void AddParameter( string name, long dbType )
		{
			string paramName = sf.GetParameterPrefix() + name + sf.GetParameterSuffix();
			// only add parameters once
			if( ! cmd.Parameters.Contains( paramName ) || ! sf.HasCapability( Capability.NamedParameters ) )
			{
				sf.AddParameter( cmd, name, dbType );
				// also remember the order in which we add parameters
				parameterOrder.Add( name );
			}
		}
		#endregion

		#region ToString
		/// <summary>
		/// Construct and emit the SQL string represented by this instance. Note that this method
		/// will convert any double spaces into a single space (this also affects non-parameterized
		/// query parameters embedded in the SQL string). 
		/// </summary>
		/// <returns>The SQL string which would be in the generated <see cref="SqlStatement"/> class.</returns>
		public override string ToString()
		{
			string sql;
			switch( stmtType )
			{
				case StatementType.Select:
					// SQL Server and MS Access
					if( provider.Name == "SQLServer" || provider.Name == "Jet" )
					{
						sql = Format( fmtSelect, GetTopLimit(), AsCommaList( quotedFields ),
						              GetTableName( tableName ), GetConstraints(), GetOrderBy(), "", "" );
					}
						// Oracle and OracleODP
					else if( provider.Name.StartsWith( "Oracle" ) )
					{
						if( rowLimit > 0 )
						{
							// Rebuild the sql: add OrderBy and virtual rownunm column
							string[] primaryKeys = map.GetPrimaryKeyNames( true );
							foreach( string pk in primaryKeys )
							{
								AddOrderByField( true, pk );
							}
							sql = Format( fmtSelect, "", AsCommaList( quotedFields ),
							              GetTableName( tableName ), GetConstraints(), GetOrderBy(), "", "" );
							sql = Format( fmtOracleLimit, AsCommaList( quotedFields ), AsCommaList( quotedFields ),
							              sql, rowLimit + rowOffset, rowOffset );
						}
						else
						{
							sql = Format( fmtSelect, "", AsCommaList( quotedFields ),
							              GetTableName( tableName ), GetConstraints(), GetOrderBy(), "", "" );
						}
					}
					else if( provider.Name.Equals( "Firebird" ) )
					{
						string limit = (rowLimit > 0 || rowOffset > 0) ? String.Format( fmtFirebirdLimit, rowLimit ) : "";
						string offset = (rowLimit > 0 || rowOffset > 0) ? String.Format( fmtFirebirdOffset, rowOffset ) : "";
						sql = Format( fmtSelect, limit + offset, AsCommaList( quotedFields ),
						              GetTableName( tableName ), GetConstraints(), GetOrderBy(), "", "" );
					}
						// Uwe Kitzmann: Support for Sybase ASA
					else if( provider.Name.StartsWith( "SybaseASA" ) )
					{
						string limit = rowLimit > 0 ? String.Format( fmtSybaseASALimit, rowLimit ) : "";
						string offset = rowOffset > 0 ? String.Format( fmtSybaseASAOffset, rowOffset ) : "";

						StringBuilder limitAndOffset = new StringBuilder( limit );
						limitAndOffset.Append( offset );

						sql = Format( fmtSelect, limitAndOffset, AsCommaList( quotedFields ),
						              GetTableName( tableName ), GetConstraints(), GetOrderBy(), "", "" );
					}
					else
					{
						sql = Format( fmtSelect, "", AsCommaList( quotedFields ),
						              GetTableName( tableName ), GetConstraints(), GetOrderBy(),
						              GetRowLimit(), GetRowOffset() );
					}
					break;
				case StatementType.Count:
					sql = Format( fmtSelect, "", "count(*) as RecordCount",
					              GetTableName( tableName ), GetConstraints(), "", "", "" );
					break;
				case StatementType.Insert:
					sql = Format( fmtInsert, GetTableName( tableName ), AsCommaList( quotedFields ),
					              AsParameterList( fields ) );
					// support retrieval of auto-generated row identity 
					if( map != null && map.IdentityMap != null )
					{
						if( sf.HasCapability( Capability.BatchQuery ) )
						{
							sql = GetIdentitySelect( sql, map );
						}
					}
					break;
				case StatementType.Update:
				case StatementType.SoftDelete:
					sql = Format( fmtUpdate, GetTableName( tableName ), AsFieldUpdateList(),
					              GetConstraints() );
					break;
				case StatementType.Delete:
					sql = Format( fmtDelete, GetTableName( tableName ), GetConstraints() );
					break;
				default:
					return "";
			}
			sql += sf.GetStatementTerminator();
			// before returning the sql string make sure it is beautiful to behold ;-)
			while( sql.IndexOf( "  " ) > 0 )
			{
				sql = sql.Replace( "  ", " " );
			}
			sql = sql.Replace( " ;", ";" );
			return sql.TrimEnd();
		}
		#endregion

		#region GetStatement Overloads
		/// <summary>
		/// Construct an SqlStatement object using the current state of this SqlBuilder instance.
		/// This method is different from the other GetStatement methods in that it does not add
		/// any fields to select or perform any other magic behind the scenes.
		/// </summary>
		/// <returns>An executable SQL statement object</returns>
		[Obsolete]
		public SqlStatement GetStatementExplicit()
		{
			Check.Verify( stmtType != StatementType.Unknown, Error.InvalidRequest,
			              "You must set the StatementType before calling this method." );
			Check.Verify( tableName != null && tableName.Length > 0, Error.InvalidRequest,
			              "No table name specified for current query." );
			return new SqlStatement( stmtType, cmd, ToString(), rowLimit, rowOffset );
		}

		/// <summary>
		/// Build a persistence engine specific SQL statement for the given type.
		/// </summary>
		/// <param name="stmtType">Type of statement to produce (select/insert/update/delete)</param>
		/// <param name="type">The object class/type</param>
		/// <returns>An executable SQL statement object</returns>
		public SqlStatement GetStatement( StatementType stmtType, Type type )
		{
			return GetStatement( stmtType, tableName, type, false );
		}

		/// <summary>
		/// Build a persistence engine specific SQL statement. The object class and statement type must
		/// have been set prior to calling this method.
		/// </summary>
		/// <param name="isCollection">For select statements, whether to return a list of items. If this
		/// value is true the primary key field(s) will be used as constraint in the query (if it is
		/// false, no constraints are automatically added by Gentle).</param>
		/// <returns>An executable SQL statement object</returns>
		public SqlStatement GetStatement( bool isCollection )
		{
			Check.VerifyNotNull( map, Error.DeveloperError,
			                     "You must set the type being selected before using this method." );
			return GetStatement( stmtType, tableName, map.Type, isCollection );
		}

		/// <summary>
		/// Build a persistence engine specific SQL statement. The object class and statement type must
		/// have been set prior to calling this method. When used with a select statement this method
		/// assumes that your are selecting multiple objects and will not apply any constraints (aside
		/// from those manually added).
		/// </summary>
		/// <returns>An executable SQL statement object</returns>
		public SqlStatement GetStatement()
		{
			Check.VerifyNotNull( map, Error.DeveloperError,
			                     "You must set the type being selected before using this method." );
			return GetStatement( stmtType, tableName, map.Type, true );
		}

		public SqlStatement GetStatement( StatementType stmtType, Type type, bool isCollection )
		{
			return GetStatement( stmtType, null, type, isCollection );
		}

		public SqlStatement GetStatement( string sql, StatementType stmtType, Type type )
		{
			PersistenceBroker broker = null;
			if( type == null )
			{
				broker = new PersistenceBroker( provider );
			}
			else
			{
				broker = new PersistenceBroker( type );
			}
			return new SqlStatement( broker, stmtType, cmd, sql, type, rowLimit, rowOffset );
		}
		#endregion

		#region GetStatement Implementation
		/// <summary>
		/// Build a persistence engine specific SQL statement for the given object class, using the
		/// class name as table name and properties for columns.
		/// </summary>
		/// <param name="stmtType">Type of statement to produce (select/insert/update/delete)</param>
		/// <param name="tableName">The table name to use in the constructed query or null to use 
		/// the table name associated with the type</param>
		/// <param name="type">The object class/type</param>
		/// <param name="isCollection">For select statements, whether to return a list of items</param>
		/// <returns>An executable SQL statement object</returns>
		public SqlStatement GetStatement( StatementType stmtType, string tableName, Type type, bool isCollection )
		{
			Check.Verify( stmtType != StatementType.Unknown,
			              "You must set the StatementType before obtaining the SqlStatement." );
			Check.Verify( map == null || map.Type == type, Error.DeveloperError,
			              "The type for this statement has already been set." );
			map = ObjectFactory.GetMap( SessionBroker, type );
			this.stmtType = stmtType;
			if( tableName == null || tableName.Equals( String.Empty ) )
			{
				SetTable( type );
			}
			if( stmtType != StatementType.Count )
			{
				// add key field(s) to select clause (and as parameter if not a collection)
				if( stmtType == StatementType.Select )
				{
					AddKeyFields( type, ! isCollection );
				}
				AddFields( type );
				if( stmtType != StatementType.Insert && ! isCollection )
				{
					bool isWithConcurrency = GentleSettings.ConcurrencyControl && ! isCollection;
					isWithConcurrency &= stmtType == StatementType.Update ||
					                     stmtType == StatementType.Delete || stmtType == StatementType.SoftDelete;
					AddConstraints( type, isWithConcurrency );
				}
			}
			// add filtering for soft deleted objects
			if( (stmtType == StatementType.Select || stmtType == StatementType.Count) && map.IsSoftDelete )
			{
				AddConstraint( Operator.NotEquals, map.SoftDeleteMap.QuotedColumnName, -1 );
			}
			// set any previously given parameter values on statement
			if( parameterOrder != null && parameterOrder.Count > 0 )
			{
				int index = 0;
				foreach( string param in parameterOrder )
				{
					object paramValue = parameterValues[ param ];
					// Uwe Kitzmann: if data provider supports positional parameters only, 
					// a call to AsParameter returns a "?" and not the parameter
					// IDataParameter dp = (IDataParameter) cmd.Parameters[ AsParameter( param ) ];
					IDataParameter dp;
					// some provider libraries use the actual name (e.g. mysql) and
					// others add prefixes and suffixes (e.g. mssql) 
					if( cmd.Parameters.Contains( param ) )
					{
						dp = (IDataParameter) cmd.Parameters[ param ];
					}
					else
					{
						string paramName = ParameterPrefix + param + ParameterSuffix;
						if( cmd.Parameters.Contains( paramName ) )
						{
							dp = (IDataParameter) cmd.Parameters[ paramName ];
						}
						else
						{
							dp = (IDataParameter) cmd.Parameters[ index++ ];
						}
					}
					Check.VerifyNotNull( dp, Error.Unspecified, "Unable to locate parameter: " + param );
					dp.Value = paramValue;
				}
			}
			return new SqlStatement( SessionBroker, stmtType, cmd, ToString(), type, rowLimit, rowOffset );
		}
		#endregion
	}
}
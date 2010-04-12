/*
 * Class to handle object construction (one map for every ConstructorInfo)
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: ConstructorMap.cs 1239 2008-04-16 22:58:28Z mm $
 */

using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Gentle.Common;
using TypeConverter=System.ComponentModel.TypeConverter;

namespace Gentle.Framework
{
	internal class ConstructorMap
	{
		private readonly ObjectMap objectMap;
		private readonly ConstructorInfo constructorInfo;
		private readonly ColumnInfo columnInfo;
		// calculated information
		private BitArray columnConstructorMask; // marks fields used in constructor
		private BitArray columnReflectionMask; // marks fields set using reflection
		private BitArray columnUnusedMask; // marks unused fields (columns with no target)
		private BitArray columnTypeConvertMask; // marks columns that may need type conversion
		private BitArray columnPrimaryKeyMask; // marks primary key columns
		private int[] columnOrderMap;
		private long cost;
		private bool isValid;
		private int columnConstructorUsageCount; // number of fields used in constructor call
		private int constructorParameterCount;
		private bool isPerfectMatch;

		public ConstructorMap( ObjectMap objectMap, ConstructorInfo constructorInfo, ColumnInfo columnInfo, object[] row )
		{
			this.objectMap = objectMap;
			this.constructorInfo = constructorInfo;
			this.columnInfo = columnInfo;
			InitBitArrays( Math.Max( objectMap.Fields.Count, row.Length ) );
			InitConstructorMap( row );
		}

		private void InitBitArrays( int length )
		{
			columnConstructorMask = new BitArray( length );
			columnReflectionMask = new BitArray( length );
			columnUnusedMask = new BitArray( length );
			columnTypeConvertMask = new BitArray( length );
			columnPrimaryKeyMask = new BitArray( length );
		}

		#region Private Helper Methods
		private void InitConstructorMap( object[] row )
		{
			columnOrderMap = new int[columnInfo.Names.Length];
			ParameterInfo[] pis = constructorInfo.GetParameters();
			constructorParameterCount = pis.Length;
			// use a counter to determine whether we have a column for every parameter
			int noColumnForParameter = constructorParameterCount;
			bool isPerfectColumnOrder = true;
			// process columns
			for( int i = 0; i < columnInfo.Names.Length; i++ )
			{
				FieldMap fm = columnInfo.Fields[ i ];
				string columnName = fm != null ? fm.ColumnName.ToLower() : columnInfo.Names[ i ].ToLower();
				string memberName = fm != null ? fm.MemberName.ToLower() : null;
				bool foundParam = false;
				for( int j = 0; j < pis.Length; j++ )
				{
					string parameterName = pis[ j ].Name.ToLower();
					if( parameterName == columnName || (fm != null && parameterName == memberName) )
					{
						noColumnForParameter--;
						foundParam = true;
						columnConstructorUsageCount += 1;
						columnConstructorMask[ i ] = true;
						columnOrderMap[ i ] = j;
						isPerfectColumnOrder &= i == j;
						// also flag primary keys
						if( fm != null && fm.IsPrimaryKey )
						{
							columnPrimaryKeyMask[ i ] = true;
						}
						break;
					}
				}
				if( ! foundParam )
				{
					if( fm != null )
					{
						// column not included in constructor but member field is present
						columnReflectionMask[ i ] = true;
						cost += 1000;
						// also flag primary keys
						if( fm.IsPrimaryKey )
						{
							columnPrimaryKeyMask[ i ] = true;
						}
					}
					else
					{
						// unused column - not in constructor or as member field
						columnUnusedMask[ i ] = true;
						// log a warning 
						Check.LogWarning( LogCategories.Metadata,
						                  "The column {0} does not match any member or constructor parameter " +
						                  "of type {1}. It will therefore not be used during object construction.",
						                  columnInfo.Names[ i ], objectMap.Type );
					}
				}
				// determine if type conversion is required for column
				if( fm != null )
				{
					// type conversion required for nullable columns mapping to not-nullable system type
					// or when result set type is different from member/parameter type
					if( fm.NullValue != null ||
						(row[ i ] != null && fm.Type != row[ i ].GetType() && ! fm.IsGenericNullableType) )
					{
						columnTypeConvertMask[ i ] = true;
						cost += 1;
					}
				}
			}
			// score 100 if parameter and column count differ
			cost += columnConstructorUsageCount == pis.Length ? 0 : 100;
			// score 300 if column order does not match parameter order
			cost += isPerfectColumnOrder ? 0 : 300;
			// score 600 if type conversion for any column is required
			cost += AllUnset( columnTypeConvertMask ) ? 0 : 600;
			// determine whether we have a perfect match (can use direct constructor invocation)
			isPerfectMatch = isPerfectColumnOrder && columnConstructorUsageCount == pis.Length;
			isPerfectMatch &= AllUnset( columnUnusedMask ) && AllUnset( columnTypeConvertMask );
			isPerfectMatch &= cost == 0;
			// isValid tells whether this CM can be used with the given columns
			isValid = noColumnForParameter == 0;
		}

		private object[] GetParams( object[] row )
		{
			object[] constructorParams = new object[columnConstructorUsageCount];
			for( int i = 0; i < row.Length; i++ )
			{
				// only include columns in constructor
				if( columnConstructorMask[ i ] )
				{
					// only convert column if type is assignment incompatible with constructor argument
					if( columnTypeConvertMask[ i ] )
					{
						constructorParams[ columnOrderMap[ i ] ] = ConvertType( columnInfo.Fields[ i ], row[ i ] );
					}
					else
					{
						constructorParams[ columnOrderMap[ i ] ] = row[ i ];
					}
				}
			}
			return constructorParams;
		}

		private void UpdateMembers( object result, object[] row )
		{
			for( int i = 0; i < row.Length; i++ )
			{
				if( columnReflectionMask[ i ] )
				{
					FieldMap fm = columnInfo.Fields[ i ];
					if( fm != null ) // TODO perhaps we should throw an error when field is unknown
					{
						fm.SetValue( result, row[ i ] );
					}
				}
			}
		}

		private object ConvertType( FieldMap fm, object val )
		{
			Check.VerifyNotNull( fm, Error.NullParameter, "fm" );
			// convert DBNull to system null
			if( val != null && val.Equals( DBNull.Value ) )
			{
				val = null;
			}
			// perform null handling (NullValue translation)
			if( val == null && ! fm.IsNullAssignable )
			{
				Check.Verify( fm.NullValue != null, Error.NullWithNoNullValue, fm.ColumnName, fm.Type );
				return fm.NullValue;
			}
			else
			{
				if( val != null )
				{
					Type type = val.GetType();
					// trim strings.. otherwise char columns are as wide as their size
					if( fm.Type == typeof(string) || fm.Type == typeof(Guid) )
					{
						if( fm.Type == typeof(Guid) )
						{
							if( fm.Size == 16 ) // binary compressed version
							{
								val = Common.TypeConverter.ToGuid( (string) val );
							}
							else
							{
								val = new Guid( val.ToString() );
							}
						}
						else
						{
							string strval = (string) val;
							// size is 0 for variable width columns
							// assume we should trim all fixed-width columns
							val = fm.Size > 0 ? strval.TrimEnd() : strval;
						}
					}
					else if( fm.Type == typeof(bool) && type != typeof(bool) )
					{
						// if property is boolean but database uses integers we need to convert
						// the type before updating it
						val = Convert.ToBoolean( val );
					}
					else if( fm.Type.IsEnum && ! type.IsEnum )
					{
						// check whether enum should be stored as string or numeric value
						// TODO we should check if enum requires 64-bit conversion 
						// val = fm.HandleEnumAsString ? Enum.Parse( fm.Type, Convert.ToString( val ), true ) : Enum.ToObject( fm.Type, Convert.ToInt32( val ) );
						val = Common.TypeConverter.Get( fm.Type, val );
					}
					else if( fm.Type == typeof(decimal) && type != typeof(decimal) )
					{
						val = Convert.ToDecimal( val, NumberFormatInfo.InvariantInfo );
					}
					else if( fm.Type != type )
					{
						TypeConverter typeConv = TypeDescriptor.GetConverter( fm.Type );
						if( typeConv != null && typeConv.CanConvertFrom( type ) )
						{
							val = typeConv.ConvertFrom( val );
						}
						else
						{
							// check for the existence of a TypeConverterAttribute for the field/property
							object[] attrs = fm.MemberInfo.GetCustomAttributes( typeof(TypeConverterAttribute), false );
							if( attrs.Length == 1 )
							{
								TypeConverterAttribute tca = (TypeConverterAttribute) attrs[ 0 ];
								TypeConverter typeConverter = (TypeConverter)
								                              Activator.CreateInstance( Type.GetType( tca.ConverterTypeName ) );
								if( typeConverter != null && typeConverter.CanConvertFrom( val.GetType() ) )
								{
									val = typeConverter.ConvertFrom( val );
								}
								else
								{
									val = Convert.ChangeType( val, fm.Type );
								}
							}
							else
							{
								val = Convert.ChangeType( val, fm.Type );
							}
						}
					}
				}
				else
				{
					// allow NullValue conversion for null strings
					if( fm.Type == typeof(string) && fm.NullValue != null )
					{
						val = fm.NullValue;
					}
				}
				return val;
			}
		}
		#endregion

		public IEntity Construct( object[] row, PersistenceBroker broker )
		{
			object result;
			// if row content and element order matches constructor exactly
			if( isPerfectMatch )
			{
				result = constructorInfo.Invoke( Reflector.InstanceCriteria, null, row, null );
			}
			else
			{
				object[] constructorParams = GetParams( row );
				result = constructorInfo.Invoke( Reflector.InstanceCriteria, null, constructorParams, null );
				if( AnySet( columnReflectionMask ) )
				{
					UpdateMembers( result, row );
				}
			}
			if( broker != null && result is BrokerLock )
			{
				(result as BrokerLock).SessionBroker = broker;
			}
			return result as IEntity;
		}

		#region BitArray Helpers
		/// <summary>
		/// Test whether at least one bit is set in the array. Replaces the old "long != 0" check.
		/// </summary>
		private bool AnySet( BitArray bits )
		{
			foreach( bool bit in bits )
			{
				if( bit )
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Test whether no bits are set in the array. Replaces the old "long == 0" check.
		/// </summary>
		private bool AllUnset( BitArray bits )
		{
			foreach( bool bit in bits )
			{
				if( bit )
				{
					return false;
				}
			}
			return true;
		}
		#endregion

		/// <summary>
		/// Construct a unique string a 
		/// </summary>
		internal string GetRowHashKey( object[] row )
		{
			Hashtable values = new Hashtable();
			for( int i = 0; i < row.Length; i++ )
			{
				if( columnPrimaryKeyMask[ i ] )
				{
					object rowValue = row[ i ] != null ? row[ i ].ToString() : "null";
					values[ columnInfo.Fields[ i ].MemberName ] = rowValue;
				}
			}
			return objectMap.GetInstanceHashKey( values );
		}

		#region Properties
		public ObjectMap ObjectMap
		{
			get { return objectMap; }
		}

		public long Cost
		{
			get { return cost; }
		}

		public bool IsPerfectMatch
		{
			get { return isPerfectMatch; }
		}

		public bool IsValid
		{
			get { return isValid; }
		}
		#endregion
	}
}
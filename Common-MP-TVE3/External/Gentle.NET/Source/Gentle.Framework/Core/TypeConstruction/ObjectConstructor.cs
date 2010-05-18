/*
 * 
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: ObjectConstructor.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Collections;
using System.Reflection;
using System.Text;
using Gentle.Common;

namespace Gentle.Framework
{
	public class ObjectConstructor
	{
		private ObjectMap objectMap;
		private IList constructorInfos;
		private Hashtable constructorMaps;
		private Hashtable columnInfos;

		public ObjectConstructor( ObjectMap objectMap )
		{
			this.objectMap = objectMap;
			constructorInfos = Reflector.FindConstructors( objectMap.Type );
			constructorMaps = new Hashtable();
			columnInfos = new Hashtable();
		}

		/// <summary>
		/// Method to scan for and determine the least expensive constructor for a given array of 
		/// column names. 
		/// </summary>
		/// <param name="columnNames">The column names of the result set</param>
		/// <param name="row">A sample row used to determine if type conversion is needed</param>
		/// <returns>A hash which can be used as constructor selector subsequently or 0 if no
		/// valid constructor could be found for the given columns.</returns>
		public int DetermineConstructor( string[] columnNames, object[] row )
		{
			int hash = GetFieldComboHashCode( columnNames );
			// make a quick exit if we've already processed this column combination
			if( constructorMaps.ContainsKey( hash ) )
			{
				return hash;
			}
			else
			{
				if( ! columnInfos.ContainsKey( hash ) )
				{
					columnInfos[ hash ] = new ColumnInfo( objectMap, columnNames );
				}
				ConstructorMap best = null;
				foreach( ConstructorInfo ci in constructorInfos )
				{
					ConstructorMap cm = new ConstructorMap( objectMap, ci, columnInfos[ hash ] as ColumnInfo, row );
					if( cm.IsValid )
					{
						if( best == null || cm.Cost < best.Cost )
						{
							best = cm;
						}
					}
				}
				if( best != null )
				{
					constructorMaps[ hash ] = best;
					// warn user that code has non-optimal performance
					if( best.Cost > 0 )
					{
						Check.LogInfo( LogCategories.Metadata, "Best available constructor for type {0} is non-optimal (cost {1}).", objectMap.Type, best.Cost );
						Check.LogInfo( LogCategories.Metadata,
						               "See http://www.mertner.com/confluence/pages/viewpage.action?pageId=240 for additional information." );
					}
				}
				else // no valid constructor found at all
				{
					StringBuilder sb = new StringBuilder();
					for( int i = 0; i < columnNames.Length; i++ )
					{
						sb.AppendFormat( "{0}{1}={2}", i % 3 != 0 ? ", " : Environment.NewLine, i, columnNames[ i ] );
					}
					Check.Fail( Error.Unspecified, "No constructor found for type {0} using columns: {1}",
					            objectMap.Type, sb.ToString() );
				}
				return hash;
			}
		}

		public IEntity Construct( int columnComboHashCode, object[] row, PersistenceBroker broker )
		{
			// TODO improve error msg
			Check.Verify( constructorMaps.ContainsKey( columnComboHashCode ), Error.Unspecified,
			              "Invalid columnComboHashCode parameter (no associated ConstructorMap instance)." );
			ConstructorMap cm = constructorMaps[ columnComboHashCode ] as ConstructorMap;
			IEntity result = null;
			// perform object uniqing and caching
			if( GentleSettings.CacheObjects && objectMap.CacheStrategy != CacheStrategy.Never )
			{
				string key = cm.GetRowHashKey( row );
				result = CacheManager.Get( key ) as IEntity;
				if( result == null )
				{
					result = cm.Construct( row, broker );
					CacheManager.Insert( key, result, objectMap.CacheStrategy );
				}
				else
				{
					GentleStatistics.UniqingCount += 1;
				}
			}
			else
			{
				result = cm.Construct( row, broker );
			}
			return result;
		}

		public static int GetFieldComboHashCode( string[] names )
		{
			if( names == null || names.Length <= 0 )
			{
				return 0;
			}
			string combine = "";
			foreach( string name in names )
			{
				combine += name + "|";
			}
			return combine.ToLower().GetHashCode();
		}
	}
}
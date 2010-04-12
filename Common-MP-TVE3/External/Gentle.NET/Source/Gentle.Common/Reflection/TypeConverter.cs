/*
 * Helper class for converting into various types
 * Copyright (C) 2004 Morten Mertner
 *
 * This library is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: TypeConverter.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;

namespace Gentle.Common
{
	/// <summary>
	/// Helper class for converting into various types.
	/// </summary>
	public class TypeConverter
	{
		private TypeConverter()
		{
		}

		#region Get methods
		/// <summary>
		/// Convert the supplied XmlNode into the specified target type. Only the InnerXml portion
		/// of the XmlNode is used in the conversion, unless the target type is itself an XmlNode.
		/// </summary>
		/// <param name="targetType">The type into which to convert</param>
		/// <param name="node">The source value used in the conversion operation</param>
		/// <returns>The converted value</returns>
		public static object Get( Type targetType, XmlNode node )
		{
			if( targetType == typeof(XmlNode) )
			{
				return node;
			}
			else
			{
				return Get( targetType, node.InnerXml );
			}
		}

		/// <summary>
		/// Convert the supplied string into the specified target type. 
		/// </summary>
		/// <param name="targetType">The type into which to convert</param>
		/// <param name="value">The source value used in the conversion operation</param>
		/// <returns>The converted value</returns>
		public static object Get( Type targetType, string value )
		{
			if( targetType.IsEnum )
			{
				try
				{
					return Enum.Parse( targetType, value );
				}
				catch // assume enum value was an int that needs conversion first
				{
					return Enum.ToObject( targetType, Convert.ToInt32( value ) );
				}
			}
			if( null != value )
			{
				if( targetType == typeof(Guid) && value.GetType() == typeof(string) )
				{
					return new Guid( value );
				}
				else
				{
					return Convert.ChangeType( value, targetType );
				}
			}
			return null;
		}

		/// <summary>
		/// Convert the supplied object into the specified target type. 
		/// </summary>
		/// <param name="targetType">The type into which to convert</param>
		/// <param name="obj">The source value used in the conversion operation</param>
		/// <returns>The converted value</returns>
		public static object Get( Type targetType, object obj )
		{
			if( targetType.IsEnum )
			{
				try
				{
					return Enum.Parse( targetType, obj.ToString() );
				}
				catch // assume enum value was an int that needs conversion first
				{
					return Enum.ToObject( targetType, Convert.ToInt32( obj ) );
				}
			}
			if( null != obj )
			{
				if( targetType == typeof(Guid) && obj.GetType() == typeof(string) )
				{
					return new Guid( obj as string );
				}
				else
				{
					return Convert.ChangeType( obj, targetType );
				}
			}
			return null;
		}
		#endregion

		#region Null Handling Methods
		/// <summary>
		/// Check whether null can be assigned to instances of the given type.
		/// </summary>
		/// <param name="type">The type to check for null assignment compatibility.</param>
		/// <returns>True if null can be assigned, false otherwise.</returns>
		public static bool IsNullAssignable( Type type )
		{
			return ! type.IsValueType;
		}
		#endregion

		#region Boolean conversions
		/// <summary>
		/// </summary>
		public static bool IsFixedNumeric( Type type )
		{
			return type == typeof(byte) || type == typeof(short) || type == typeof(int) || type == typeof(long);
		}

		/// <summary>
		/// </summary>
		public static bool GetBoolean( object value )
		{
			if( value == null )
			{
				return false;
			}
			if( IsFixedNumeric( value.GetType() ) )
			{
				long numericValue = (long) Get( typeof(long), value );
				return numericValue != 0;
			}
			else
			{
				return Convert.ToBoolean( value );
			}
		}
		#endregion

		#region GUID conversions
		/// <summary>
		/// Convert the binary string (16 bytes) into a Guid.
		/// </summary>
		public static Guid ToGuid( string guid )
		{
			char[] charBuffer = guid.ToCharArray();
			byte[] byteBuffer = new byte[16];
			int nCurrByte = 0;
			foreach( char currChar in charBuffer )
			{
				byteBuffer[ nCurrByte++ ] = (byte) currChar;
			}
			return new Guid( byteBuffer );
		}

		/// <summary>
		/// Convert the Guid into a binary string.
		/// </summary>
		public static string ToBinaryString( Guid guid )
		{
			StringBuilder sb = new StringBuilder( 16 );
			foreach( byte currByte in (guid).ToByteArray() )
			{
				sb.Append( (char) currByte );
			}
			return sb.ToString();
		}
		#endregion

		#region Object Cloning
		/// <summary>
		/// Perform a deep cloning of the supplied object instance using serialization.
		/// </summary>
		/// <param name="obj">The object to clone.</param>
		/// <returns>A copy of the supplied instance.</returns>
		public object CloneDeep( object obj )
		{
			object result = null;
			using( MemoryStream ms = new MemoryStream() )
			{
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize( ms, this );
				ms.Seek( 0, SeekOrigin.Begin );
				result = formatter.Deserialize( ms );
			}
			return result;
		}
		#endregion
	}
}
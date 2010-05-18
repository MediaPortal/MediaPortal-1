/*
 * Helper class for performing various reflection tasks
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: Reflector.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Collections;
using System.Reflection;

namespace Gentle.Common
{
	/// <summary>
	/// Helper class for performing various reflection tasks, such as gathering information
	/// on members and their custom attributes.
	/// </summary>
	public class Reflector
	{
		/// <summary>
		/// Search criteria encompassing all public and non-public members.
		/// </summary>
		public static readonly BindingFlags DefaultCriteria = BindingFlags.Public | BindingFlags.NonPublic;
		/// <summary>
		/// Search criteria encompassing all public and non-public instance members.
		/// </summary>
		public static readonly BindingFlags InstanceCriteria = DefaultCriteria | BindingFlags.Instance;
		/// <summary>
		/// Search criteria encompassing all public and non-public static members, including those of parent classes.
		/// </summary>
		public static readonly BindingFlags StaticCriteria = DefaultCriteria | BindingFlags.Static | BindingFlags.FlattenHierarchy;
		/// <summary>
		/// Search criteria encompassing all members, including those of parent classes.
		/// </summary>
		public static readonly BindingFlags AllCriteria = InstanceCriteria | StaticCriteria;

		private Reflector()
		{
		}

		#region Standard Reflection Helpers
		/// <summary>
		/// Find and return a list of all available (non-abstract) constructors for the specified type.
		/// </summary>
		/// <param name="type">The type to reflect on</param>
		/// <returns>A list of useable constructors</returns>
		public static IList FindConstructors( Type type )
		{
			ArrayList result = new ArrayList();
			foreach( ConstructorInfo constructorInfo in type.GetConstructors( InstanceCriteria ) )
			{
				// exclude abstract constructors (weird concept anyway)
				if( ! constructorInfo.IsAbstract )
				{
					result.Add( constructorInfo );
				}
			}
			return result;
		}

		/// <summary>
		/// Find and return a list of members for the specified type.
		/// </summary>
		/// <param name="criteria">The search criteria used to restrict the members included in the search</param>
		/// <param name="type">The type to reflect on</param>
		/// <param name="allowDuplicates">When true, multiple instances of the same attribute type are allowed 
		/// in the result. When false, only the first instance found will be included</param>
		/// <param name="attributeTypes">The list of attribute types to search for.</param>
		/// <returns>A list of MemberAttributeInfo objects with information on the member and 
		/// the attributes with which it is decorated</returns>
		public static IList FindMembers( BindingFlags criteria, Type type, bool allowDuplicates, params Type[] attributeTypes )
		{
			ArrayList result = new ArrayList();
			foreach( MemberInfo memberInfo in type.GetMembers( criteria ) )
			{
				IList attributes = FindAttributes( allowDuplicates, memberInfo, attributeTypes );
				if( attributes != null && attributes.Count > 0 )
				{
					result.Add( new MemberAttributeInfo( memberInfo, attributes ) );
				}
			}
			return result;
		}

		/// <summary>
		/// Find a specific named member on the given type.
		/// </summary>
		/// <param name="criteria">The search criteria used to restrict the members included in the search</param>
		/// <param name="name">The name of the member to find</param>
		/// <param name="type">The type to reflect on</param>
		/// <returns>A single MemberInfo instance of the first found match or null if no match was found</returns>
		public static MemberInfo FindMember( BindingFlags criteria, string name, Type type )
		{
			MemberInfo[] mis = type.GetMember( name, criteria );
			return mis != null && mis.Length > 0 ? mis[ 0 ] : null;
		}

		/// <summary>
		/// Find a specific attribute type on the given enumeration instance.
		/// </summary>
		/// <param name="instance">An enumeration value on which to search for the attribute.</param>
		/// <param name="attributeType">The attribute type to search for.</param>
		/// <returns>An instance of the attribute type specified if it was found on the instance.</returns>
		public static Attribute FindAttribute( Enum instance, Type attributeType )
		{
			Type type = instance.GetType();
			MemberInfo[] mis = type.GetMember( instance.ToString(), StaticCriteria );
			if( mis != null && mis.Length > 0 )
			{
				MemberInfo memberInfo = mis[ 0 ];
				IList attrs = FindAttributes( false, memberInfo, attributeType );
				if( attrs != null && attrs.Count > 0 )
				{
					return attrs[ 0 ] as Attribute;
				}
			}
			return null;
		}

		/// <summary>
		/// Helper method used to find attributes associated with the specified member.
		/// </summary>
		private static IList FindAttributes( bool allowDuplicates, MemberInfo memberInfo, params Type[] attributeTypes )
		{
			IList result = new ArrayList();
			foreach( Type attributeType in attributeTypes )
			{
				object[] attrs = memberInfo.GetCustomAttributes( attributeType, true );
				int max = allowDuplicates ? attrs.Length : 1;
				if( attrs != null && attrs.Length > 0 )
				{
					for( int i = 0; i < max; i++ )
					{
						result.Add( attrs[ i ] );
					}
				}
			}
			return result;
		}
		#endregion

		#region Get/Set Member Value
		/// <summary>
		/// Return the value of the specified member on the given instance.
		/// </summary>
		/// <param name="memberInfo">The member whose value should be extracted.</param>
		/// <param name="instance">The instance holding the member.</param>
		/// <returns>The value of the speficied member.</returns>
		public static object GetValue( MemberInfo memberInfo, object instance )
		{
			if( memberInfo is PropertyInfo )
			{
				return (memberInfo as PropertyInfo).GetValue( instance, null );
			}
			else
			{
				return (memberInfo as FieldInfo).GetValue( instance );
			}
		}

		/// <summary>
		/// Return the value of the specified member on the given instance.
		/// </summary>
		/// <param name="name">The name of the member whose value should be extracted. Only instance
		/// fields will be considered by this method. If multiple members are found with the same 
		/// name, the value of the first will be used.</param>
		/// <param name="instance">The instance holding the member.</param>
		/// <returns>The value of the speficied member.</returns>
		public static object GetValue( string name, object instance )
		{
			MemberInfo memberInfo = FindMember( InstanceCriteria, name, instance.GetType() );
			Check.VerifyNotNull( memberInfo, Error.DeveloperError, "The property {0} was not found on type {1}.",
			                     name, instance.GetType() );
			if( memberInfo is PropertyInfo )
			{
				return (memberInfo as PropertyInfo).GetValue( instance, null );
			}
			else
			{
				return (memberInfo as FieldInfo).GetValue( instance );
			}
		}

		/// <summary>
		/// Update the specified member on the given instance with the supplied value.
		/// </summary>
		/// <param name="memberInfo">The member whose value should be updated.</param>
		/// <param name="instance">The instance holding the member.</param>
		/// <param name="value">The value to use in the update.</param>
		public static void SetValue( MemberInfo memberInfo, object instance, object value )
		{
			if( memberInfo is PropertyInfo )
			{
				(memberInfo as PropertyInfo).SetValue( instance, value, InstanceCriteria, null, null, null );
			}
			else
			{
				(memberInfo as FieldInfo).SetValue( instance, value, InstanceCriteria, null, null );
			}
		}
		#endregion
	}
}
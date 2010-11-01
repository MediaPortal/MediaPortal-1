/*
 * Helper class for dynamic invocation of a method
 * Copyright (C) 2005 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: MethodInvoker.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Collections;
using System.Reflection;

namespace Gentle.Common
{
	/// <summary>
	/// Helper class used to store information on a particular method, and the
	/// ability to execute calls on it (if supplied with valid parameters).
	/// </summary>
	public class MethodInvoker
	{
		private MethodInfo methodInfo;
		private ParameterInfo[] parameterInfos;
		private object[] parameterDefaultValues;
		private int requiredParameters;

		#region Constructor
		/// <summary>
		/// Construct a new MethodInvoker instance.
		/// </summary>
		/// <param name="methodInfo">The <see cref="MethodInfo"/> for the method that this instance wraps.</param>
		/// <param name="requiredParameters">The number of parameters (counting
		/// from left to right) that are required to call the method. Any subsequent 
		/// parameters may be substituted with a default value (if available) or null.</param>
		public MethodInvoker( MethodInfo methodInfo, int requiredParameters )
		{
			this.methodInfo = methodInfo;
			this.requiredParameters = requiredParameters;
			parameterInfos = methodInfo.GetParameters();
			parameterDefaultValues = new object[parameterInfos.Length];
		}
		#endregion

		#region Properties
		/// <summary>
		/// The <see cref="MethodInfo"/> for the method that this instance wraps.
		/// </summary>
		public MethodInfo MethodInfo
		{
			get { return methodInfo; }
		}
		/// <summary>
		/// The number of parameters (counting from left to right) that are required 
		/// to call the method. Any subsequent parameters may be substituted with a 
		/// default value (if available) or null.
		/// </summary>
		public int RequiredParameters
		{
			get { return requiredParameters; }
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Store a default value for the given parameter. This value will be used if no
		/// value has been supplied for the parameter, and it is not a required parameter.
		/// </summary>
		/// <param name="parameterName">The name (case-insensitive) of the parameter to 
		/// look for.</param>
		/// <param name="value">The default value to use when none is otherwise given.</param>
		public void SetDefaultValue( string parameterName, object value )
		{
			int index = FindParameter( parameterName );
			Check.Verify( index >= 0, Error.DeveloperError, "Method does not have a parameter named " + parameterName );
			parameterDefaultValues[ index ] = value;
		}

		/// <summary>
		/// Prepare a <see cref="MethodInvokable"/> instance for calling the underlying method
		/// using the supplied parameters. If the call cannot be made using the given parameters,
		/// null is returned.
		/// </summary>
		/// <param name="parameters">A hashtable of parameter name/value pairs.</param>
		/// <returns>A <see cref="MethodInvokable"/> instance prepared for execution of the
		/// method call.</returns>
		public MethodInvokable PrepareInvoke( Hashtable parameters )
		{
			int normalCount = 0; // number of fields filled with regular parameter values
			int defaultCount = 0; // number of fields filled using default values
			int nullCount = 0; // number of fields filled using null
			// process method parameters
			object[] invokeParameters = new object[parameterInfos.Length];
			for( int i = 0; i < parameterInfos.Length; i++ )
			{
				ParameterInfo pi = parameterInfos[ i ];
				string parameterName = pi.Name.ToLower();
				// ignore a leading underscore in parameter names
				if( parameterName.StartsWith( "_" ) )
				{
					parameterName = parameterName.Substring( 1, parameterName.Length - 1 );
				}
				if( parameters.ContainsKey( parameterName ) )
				{
					object val = parameters[ parameterName ];
					if( val != null && val.GetType() != pi.ParameterType )
					{
						val = TypeConverter.Get( pi.ParameterType, val );
					}
					invokeParameters[ i ] = val;
					normalCount++;
				}
				else
				{
					// use >= because i is 0-based whereas rP is 1-based
					if( i >= requiredParameters )
					{
						// see if we have a default parameter value or if null is allowed
						if( parameterDefaultValues[ i ] != null )
						{
							invokeParameters[ i ] = parameterDefaultValues[ i ];
							defaultCount++;
						}
						else if( TypeConverter.IsNullAssignable( pi.ParameterType ) )
						{
							invokeParameters[ i ] = null;
							nullCount++;
						}
					}
				}
			}
			// we must have a value for every method parameter
			bool isValid = parameterInfos.Length == normalCount + defaultCount + nullCount;
			// we must use all configured values in the config file
			isValid &= parameters.Count == normalCount;
			if( ! isValid )
			{
				return null;
			}
			// method can be called using supplied parameters.. 
			int matchIndicator = normalCount << 16 - defaultCount << 8 - nullCount;
			return new MethodInvokable( this, matchIndicator, invokeParameters );
		}

		/// <summary>
		/// Invoke the underlying method on the given target object using the supplied parameter values.
		/// Any exception raised by performing the method call is logged and then exposed as-is.
		/// </summary>
		/// <param name="target">The object on which to invoke the method.</param>
		/// <param name="parameterValues">The parameter values used to invoke the method.</param>
		/// <returns>The return value of the invocation.</returns>
		public object Invoke( object target, object[] parameterValues )
		{
			try
			{
				return MethodInfo.Invoke( target, Reflector.InstanceCriteria, null, parameterValues, null );
			}
			catch( Exception e )
			{
				Check.LogError( LogCategories.General, e );
				throw; // expose the original error raised by callback target
			}
		}

		/// <summary>
		/// Invoke the underlying method on the given target object using the supplied parameter values.
		/// Any exception raised by performing the method call is logged and then exposed as-is.
		/// </summary>
		/// <param name="target">The object on which to invoke the method.</param>
		/// <param name="parameters">A hashtable of parameter name/value pairs.</param>
		/// <returns>The return value of the invocation.</returns>
		public object Invoke( object target, Hashtable parameters )
		{
			MethodInvokable mi = PrepareInvoke( parameters );
			if( mi.MatchIndicator >= 0 )
			{
				return Invoke( target, mi.ParameterValues );
			}
			else
			{
				Check.Fail( Error.DeveloperError, "Unable to invoke method using given parameters." );
				return null;
			}
		}
		#endregion

		#region Private Helpers
		private int FindParameter( string parameterName )
		{
			if( parameterInfos == null || parameterInfos.Length == 0 )
			{
				return -1;
			}
			for( int i = 0; i < parameterInfos.Length; i++ )
			{
				if( parameterInfos[ i ].Name == parameterName )
				{
					if( parameterInfos[ i ].Name == parameterName )
					{
						return i;
					}
				}
			}
			return -1;
		}
		#endregion
	}
}
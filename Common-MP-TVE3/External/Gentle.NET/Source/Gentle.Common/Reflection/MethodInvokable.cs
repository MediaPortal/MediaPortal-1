/*
 * Helper class for dynamic invocation of a method
 * Copyright (C) 2005 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: MethodInvokable.cs 1232 2008-03-14 05:36:00Z mm $
 */

namespace Gentle.Common
{
	/// <summary>
	/// This class wraps a single invokable method call. It contains information
	/// on the method to call as well as the parameters to use in the method call.
	/// This intermediary class is used by the MethodDispatcher to select the best
	/// match to call from a given set of available methods (and a single set of
	/// parameter values).
	/// </summary>
	public class MethodInvokable
	{
		/// <summary>
		/// The <see cref="MethodInvoker"/> wrapping the method to call.
		/// </summary>
		public readonly MethodInvoker MethodInvoker;
		/// <summary>
		/// A value indicating how good a match this is (for a given set of parameters). This value 
		/// is used by the <see cref="MethodDispatcher"/> to decide which method to invoke from a 
		/// set of choices.
		/// </summary>
		public readonly int MatchIndicator;
		/// <summary>
		/// The parameter values used when invoking the method.
		/// </summary>
		public readonly object[] ParameterValues;

		/// <summary>
		/// Construct a new MethodInvokable instance in preparation for executing
		/// the actual method call.
		/// </summary>
		/// <param name="invoker">The <see cref="MethodInvoker"/> wrapping the method to call.</param>
		/// <param name="matchIndicator">A value indicating how good a match this is (for a given
		/// set of parameters). This value is used by the <see cref="MethodDispatcher"/> to decide
		/// which method to invoke from a set of choices.</param>
		/// <param name="parameterValues">The parameter values used when invoking the method.</param>
		public MethodInvokable( MethodInvoker invoker, int matchIndicator, object[] parameterValues )
		{
			MethodInvoker = invoker;
			MatchIndicator = matchIndicator;
			ParameterValues = parameterValues;
		}

		/// <summary>
		/// Perform the actual method invocation.
		/// </summary>
		/// <param name="target">The object to call the method on.</param>
		/// <returns>The return value of the method call.</returns>
		public object Invoke( object target )
		{
			return MethodInvoker.Invoke( target, ParameterValues );
		}
	}
}
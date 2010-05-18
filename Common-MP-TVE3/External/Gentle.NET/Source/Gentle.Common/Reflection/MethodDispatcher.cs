/*
 * Helper class for determining the best method given a list of available parameters
 * Copyright (C) 2005 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: MethodDispatcher.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System.Collections;

namespace Gentle.Common
{
	/// <summary>
	/// Helper class for determining the best matching method from a set of available
	/// methods. The best matching method is determined from the given set of parameters
	/// on the call to <see cref="Invoke"/>.
	/// </summary>
	public class MethodDispatcher
	{
		private object _lock = new object();
		private IList invokers;
		private Hashtable dispatchCache;

		/// <summary>
		/// Construct a new MethodDispatcher instance. Initially only a single method must be given,
		/// however, to make full use of this class you may add additional candidate methods using
		/// the <see cref="AddInvoker"/> method.
		/// </summary>
		/// <param name="invoker"></param>
		public MethodDispatcher( MethodInvoker invoker )
		{
			invokers = new ArrayList();
			invokers.Add( invoker );
			dispatchCache = new Hashtable();
		}

		/// <summary>
		/// Add a method to the list of available methods for this method dispatcher.
		/// </summary>
		/// <param name="invoker">A MethodInvoker instance with information on the new method.</param>
		public void AddInvoker( MethodInvoker invoker )
		{
			lock( _lock )
			{
				invokers.Add( invoker );
			}
		}

		/// <summary>
		/// Invoke the best available match (from a list of methods available) for the supplied parameters. 
		/// If no method can be called using the supplied parameters, an exception is raised. Any exceptions
		/// raised by the called method will be logged and then re-thrown.
		/// </summary>
		/// <param name="target">The object on which to invoke a method.</param>
		/// <param name="parameters">A hashtable of parameter name/value pairs. All parameters given
		/// must be used in the method call in order for a method to be considered.</param>
		/// <returns>The return value of the invocation.</returns>
		public object Invoke( object target, Hashtable parameters )
		{
			MethodInvokable invokable = DetermineBestMatch( parameters );
			Check.VerifyNotNull( invokable, Error.DeveloperError, "No compatible method found to invoke for the given parameters." );
			return invokable.Invoke( target );
		}

		#region Private Helper Methods
		private MethodInvokable DetermineBestMatch( Hashtable parameters )
		{
			MethodInvokable best = null;
			foreach( MethodInvoker invoker in invokers )
			{
				MethodInvokable invokable = invoker.PrepareInvoke( parameters );
				bool isBetter = best == null && invokable != null && invokable.MatchIndicator > 0;
				isBetter |= best != null && invokable != null && invokable.MatchIndicator > best.MatchIndicator;
				if( isBetter )
				{
					best = invokable;
				}
			}
			return best;
		}
		#endregion
	}
}
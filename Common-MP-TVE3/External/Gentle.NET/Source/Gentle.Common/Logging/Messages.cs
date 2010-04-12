/*
 * Helper class for retrieving messages from an external source (i.e. the config file)
 * Copyright (C) 2004 Morten Mertner
 *
 * This library is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: Messages.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Collections;
using System.Text;

namespace Gentle.Common
{
	/// <summary>
	/// Helper class for handling the formatting of error messages.
	/// </summary>
	public class Messages
	{
		private const string msgTemplate = "Error: {0}{1}{2}{1}";
		private const string msgNoMsg = "Sorry, no message text defined for this error.";
		private static object msgLock = new object();
		private static Hashtable messages = new Hashtable();

		static Messages()
		{
			ExtractMessages();
		}

		#region Private Helper Methods
		private static void ExtractMessages()
		{
			lock( msgLock )
			{
				foreach( Error error in Enum.GetValues( typeof(Error) ) )
				{
					Attribute attr = Reflector.FindAttribute( error, typeof(MessageAttribute) );
					if( attr != null )
					{
						messages[ error.ToString() ] = attr;
					}
				}
			}
		}

		internal static int GetFormatStringArgumentCount( string formatString )
		{
			try
			{
				int args = 0;
				int pos = formatString.IndexOf( '{' );
				while( pos++ > 0 )
				{
					int endpos = formatString.IndexOf( '}', pos );
					args = Convert.ToInt32( formatString.Substring( pos, endpos - pos ) ) + 1;
					pos = formatString.IndexOf( '{', endpos );
				}
				return args;
			}
			catch
			{
				return -1;
			}
		}

		private static string FormatNoMessage( params object[] args )
		{
			if( args == null || args.Length == 0 )
			{
				return msgNoMsg;
			}
			else if( args.Length == 1 )
			{
				return Convert.ToString( args[ 0 ] );
			}
			else
			{
				// determine if first argument is a format string
				if( args[ 0 ] != null && args[ 0 ].GetType() == typeof(string) )
				{
					string fmtMsg = args[ 0 ] as string;
					if( GetFormatStringArgumentCount( fmtMsg ) == args.Length - 1 )
					{
						object[] newArgs = new object[args.Length - 1];
						Array.Copy( args, 1, newArgs, 0, newArgs.Length );
						return Format( fmtMsg, newArgs );
					}
				}
				// output the default message and all arguments
				StringBuilder sb = new StringBuilder();
				sb.Append( msgNoMsg + Environment.NewLine );
				for( int i = 0; i < args.Length; i++ )
				{
					if( args[ i ] != null )
					{
						sb.AppendFormat( "{0}={1} (type {2}){3}",
						                 i, args[ i ], args[ i ].GetType(), Environment.NewLine );
					}
					else
					{
						sb.AppendFormat( "{0}=null{1}", i, Environment.NewLine );
					}
				}
				return sb.ToString();
			}
		}

		// note: locking not required here since accessed only through public GetMsg method
		private static string FormatMessage( Error error, params object[] args )
		{
			MessageAttribute ma = messages[ error.ToString() ] as MessageAttribute;
			if( ma == null || args.Length != ma.ArgumentCount )
			{
				throw new GentleException( Error.Unspecified,
				                           String.Format( "Unexpected argument count (passed {0}, expected {1}).",
				                                          args.Length, ma.ArgumentCount ) );
			}
			else
			{
				return Format( ma.Message, args );
			}
		}

		internal static string Format( string msg, params object[] args )
		{
			try
			{
				return String.Format( msg, args );
			}
			catch
			{
				// someone passed us invalid format arguments for a message,
				// attempt to return a sensible error message
				return String.Format( "Error formatting message: {0}{1}Using args: {2}{1}",
				                      msg, Environment.NewLine, FormatNoMessage( args ) );
			}
		}
		#endregion

		/// <summary>
		/// This method creates a formatted error message constructed using the format 
		/// string associated with the given error and the list of supplied arguments.
		/// The number of arguments must match that of the format string.
		/// </summary>
		/// <param name="error">The error for which to return an error message.</param>
		/// <param name="args">The arguments (if any) to use in formatting the message.</param>
		/// <returns>The formatted error message.</returns>
		public static string GetMsg( Error error, params object[] args )
		{
			lock( msgLock )
			{
				if( messages.ContainsKey( error.ToString() ) )
				{
					return FormatMessage( error, args );
				}
				else
				{
					return FormatNoMessage( args );
				}
			}
		}
	}
}
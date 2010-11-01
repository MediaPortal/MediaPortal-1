/*
 * Utility class for doing assertions, error handling and reporting/publishing
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: Check.cs 1232 2008-03-14 05:36:00Z mm $
 */
using System;
using System.Collections;
using System.Diagnostics;
using log4net;
using log4net.Config;
using log4net.Core;

// attribute to tell log4net where to obtain its configuration from

[assembly: XmlConfigurator( Watch = true )]

namespace Gentle.Common
{
	/// <summary>
	/// <p>This helper class is used throughout the framework. It provides assertion methods,
	/// standardized error reporting and automatic exception creation. The exception type
	/// raised is hardcoded in the static constructor.</p>
	/// </summary>
	public sealed class Check
	{
		#region Members
		private const string LoggingPrefix = "Gentle.Framework/Options/Logging/";
		private static readonly Hashtable loggers = new Hashtable();

		/// <summary>
		/// This defines the minimum severity level at which exceptions will be
		/// logged. All exceptions with a lower severity will not be logged.
		/// </summary>
		// only errors above this level will be published
		[Configuration( LoggingPrefix + "Verbosity", ConfigKeyPresence.Optional )]
		private static Severity verbosity = Severity.Debug;

		/// <summary>
		/// This defines the minimum severity level at which exceptions will be
		/// re-thrown once logging is done. All exceptions with a lower severity
		/// will be silently "swallowed" - use with caution!
		/// </summary>
		[Configuration( LoggingPrefix + "Frailty", ConfigKeyPresence.Optional )]
		private static Severity frailty = Severity.Debug;

		/// <summary>
		/// Log topic categories enabled (default is none).
		/// </summary>
		public static LogCategories LogCategories = LogCategories.None;
		#endregion

		#region Constructor (static)
		static Check()
		{
			try
			{
				Configurator.Configure( typeof(Check) );
			}
			catch( Exception ex )
			{
				LogError( LogCategories.General, ex );
			}
		}
		#endregion

		#region Configuration Callback (log categories)
		/// <summary>
		/// Callback method for configuration options to turn logging in the available
		/// categories on or off.
		/// </summary>
		/// <param name="name">The name of the logging category.</param>
		/// <param name="enabled">True if logging should be enabled for the 
		/// specified category. Note that subsequent entries in the config
		/// file may override earlier definitions.</param>
		[Configuration( LoggingPrefix + "Category" )]
		private static void RegisterLogCategory( LogCategories name, bool enabled )
		{
			if( enabled )
			{
				LogCategories |= name;
			}
			else
			{
				LogCategories &= ~name;
			}
		}
		#endregion

		#region Log Category Helper
		/// <summary>
		/// Call this method to determine whether the passed category (or combination
		/// of categories) is enabled.
		/// </summary>
		/// <param name="category">The category or categories to process.</param>
		/// <returns>True if all of the specified categories are enabled.</returns>
		public static bool IsLogEnabled( LogCategories category )
		{
			return (LogCategories & category) != 0;
		}
		#endregion

		#region Message Formatting Methods
		private static string Format( string errorMsg, params object[] args )
		{
			return String.Format( errorMsg, args );
		}

		private static string FormatLine( string errorMsg, params object[] args )
		{
			return String.Format( errorMsg, args ) + Environment.NewLine;
		}

		internal static Severity GetSeverity( Error error )
		{
			Attribute attr = Reflector.FindAttribute( error, typeof(LevelAttribute) );
			// assume the worst - return critical if no Level attribute was found
			return attr != null ? (attr as LevelAttribute).Severity : Severity.Critical;
		}

		/// <summary>
		/// Internal method performing error logging and throwing of exceptions.
		/// </summary>
		/// <param name="severity">The severity of the error.</param>
		/// <param name="error">The error classification.</param>
		/// <param name="e">The exception leading to this error (if any).</param>
		/// <param name="msg">The error message for this error.</param>
		internal static void FailWith( Severity severity, Error error, Exception e, string msg )
		{
			if( error == Error.Unspecified || severity >= verbosity )
			{
				LogMessage( LogCategories.General, severity, msg, e );
			}
			if( error == Error.Unspecified || severity >= frailty )
			{
				// make sure framework only throws GentleExceptions 
				if( e != null && e is GentleException )
				{
					throw e;
				}
				else
				{
					throw new GentleException( error, msg, e );
				}
			}
		}
		#endregion

		#region Assertions
		/// <summary>
		/// This method formats and logs an error message, then raises a GentleException.
		/// </summary>
		public static void Fail( string msg, params object[] args )
		{
			FailWith( Severity.Critical, Error.Unspecified, null, Format( msg, args ) );
		}

		/// <summary>
		/// This method formats and logs an error message, then raises a GentleException.
		/// </summary>
		public static void Fail( Exception e, Error error, params object[] args )
		{
			FailWith( GetSeverity( error ), error, e, Messages.GetMsg( error, args ) );
		}

		/// <summary>
		/// This method formats and logs an error message, then raises a GentleException.
		/// </summary>
		public static void Fail( Error error, params object[] args )
		{
			Fail( null, error, args );
		}

		/// <summary>
		/// This method is used to perform assertions. If the assertion fails an error message
		/// will be logged and a GentleException thrown.
		/// </summary>
		public static void IsTrue( bool condition, Error error, params object[] args )
		{
			if( ! condition )
			{
				Fail( error, args );
			}
		}

		/// <summary>
		/// This method is used to perform assertions. If the assertion fails an error message
		/// will be logged and a GentleException thrown.
		/// </summary>
		public static void IsTrue( bool condition, string errorMsg, params object[] args )
		{
			if( ! condition )
			{
				Fail( Error.Unspecified, errorMsg, args );
			}
		}

		/// <summary>
		/// This method is used to perform assertions. If the assertion fails an error message
		/// will be logged and a GentleException thrown.
		/// </summary>
		public static void Verify( bool condition, Error error, params object[] args )
		{
			IsTrue( condition, error, args );
		}

		/// <summary>
		/// This method is used to perform assertions. If the assertion fails an error message
		/// will be logged and a GentleException thrown.
		/// </summary>
		public static void Verify( bool condition, string errorMsg, params object[] args )
		{
			IsTrue( condition, errorMsg, args );
		}

		/// <summary>
		/// This method is used to perform assertions. If the assertion fails an error message
		/// will be logged and a GentleException thrown.
		/// </summary>
		public static void VerifyEquals( object obj1, object obj2, Error error, params object[] args )
		{
			if( ! obj1.Equals( obj2 ) )
			{
				Fail( error, args );
			}
		}

		/// <summary>
		/// This method is used to perform assertions. If the assertion fails an error message
		/// will be logged and a GentleException thrown.
		/// </summary>
		public static void VerifyEquals( object obj1, object obj2, string errorMsg, params object[] args )
		{
			if( ! ((obj1 == null && obj2 == null) || obj1.Equals( obj2 )) )
			{
				Fail( errorMsg, args );
			}
		}

		/// <summary>
		/// This method is used to perform assertions. If the assertion fails an error message
		/// will be logged and a GentleException thrown.
		/// </summary>
		public static void VerifyNotNull( object obj1, Error error, params object[] args )
		{
			if( obj1 == null )
			{
				Fail( error, args );
			}
		}

		/// <summary>
		/// This method is used to perform assertions. If the assertion fails an error message
		/// will be logged and a GentleException thrown.
		/// </summary>
		public static void VerifyNotNull( object obj1, string errorMsg, params object[] args )
		{
			if( obj1 == null )
			{
				Fail( errorMsg, args );
			}
		}

		/// <summary>
		/// This method is used to perform assertions. If the assertion fails an error message
		/// will be logged and a GentleException thrown.
		/// </summary>
		public static void VerifyIsNull( object obj1, Error error, params object[] args )
		{
			if( obj1 != null )
			{
				Fail( error, args );
			}
		}

		/// <summary>
		/// This method is used to perform assertions. If the assertion fails an error message
		/// will be logged and a GentleException thrown.
		/// </summary>
		public static void VerifyIsNull( object obj1, string errorMsg, params object[] args )
		{
			if( obj1 != null )
			{
				Fail( errorMsg, args );
			}
		}

		/// <summary>
		/// This method is used to perform assertions. If the assertion fails an error message
		/// will be logged and a GentleException thrown.
		/// </summary>
		public static void VerifyNull( object obj1, Error error, params object[] args )
		{
			if( obj1 != null )
			{
				Fail( error, args );
			}
		}

		/// <summary>
		/// This method is used to perform assertions. If the assertion fails an error message
		/// will be logged and a GentleException thrown.
		/// </summary>
		public static void VerifyNull( object obj1, string errorMsg, params object[] args )
		{
			if( obj1 != null )
			{
				Fail( errorMsg, args );
			}
		}

		/// <summary>
		/// This method is used to select the first object not being null from a list of 2 objects.
		/// </summary>
		public static object IfNull( object obj, object def )
		{
			return obj != null ? obj : def;
		}
		#endregion

		#region Call Stack Inspection
		/// <summary>
		/// Check whether the specified tracer string occurs in the stack trace of the call
		/// leading to the exception. This method excludes the last three method calls from
		/// the stack trace analysis, assuming that the exception was provoked by a call
		/// to one of the Check.Verify or Check.Fail methods.
		/// </summary>
		/// <param name="tracer">The case-sensitive string to look for in the stack trace.
		/// Usually this will be "Class.Method" or a similar fragment.</param>
		/// <param name="e">The exception whose call stack is to be inspected.</param>
		/// <returns>True if the tracer is found in the stack trace.</returns>
		public static bool IsCalledFrom( string tracer, Exception e )
		{
			return IsCalledFrom( tracer, e, 2 );
		}

		/// <summary>
		/// Check whether the specified tracer string occurs in the stack trace of the call
		/// leading to the exception.
		/// </summary>
		/// <param name="tracer">The case-sensitive string to look for in the stack trace.
		/// Usually this will be "Class.Method" or a similar fragment.</param>
		/// <param name="e">The exception whose call stack is to be inspected.</param>
		/// <param name="skipFrames">Skip this number of stack frames from the search.</param>
		/// <returns>True if the tracer is found in the stack trace.</returns>
		private static bool IsCalledFrom( string tracer, Exception e, int skipFrames )
		{
#if( DEBUG )
			skipFrames += 2;
#endif
			try
			{
				return IsCalledFrom( tracer, new StackTrace( e, skipFrames, true ) );
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Check whether the specified tracer string occurs in the given stack trace.
		/// </summary>
		/// <param name="tracer">The case-sensitive string to look for in the stack trace.
		/// Usually this will be "Class.Method" or a similar fragment.</param>
		/// <param name="stackTrace">The stack trace to be inspected.</param>
		/// <returns>True if the tracer is found in the stack trace.</returns>
		public static bool IsCalledFrom( string tracer, StackTrace stackTrace )
		{
			return tracer != null && stackTrace != null && stackTrace.ToString().IndexOf( tracer ) != -1;
		}

		/// <summary>
		/// Determine and return information on the calling class, that is, the class making
		/// the initial call to the Check class. If an exception is passed, the stack trace
		/// leading to that exception will be used instead.
		/// </summary>
		/// <returns>The class executing the current call to the Check class.</returns>
		public static Type GetExternalCaller( Exception e )
		{
			int skipFrames = 1;
#if( DEBUG )
			skipFrames += 2;
#endif
			try
			{
				StackTrace st = e != null ? new StackTrace( e, skipFrames, false ) : new StackTrace( skipFrames, false );
				for( int i = 0; i < st.FrameCount - 1; i++ )
				{
					StackFrame sf = st.GetFrame( i );
					Type dt = sf.GetMethod().DeclaringType;
					if( dt != typeof(Check) )
					{
						return dt;
					}
				}
				// unable to determine any external caller
				return typeof(Check);
			}
			catch // required because this method has caused problems under Mono
			{
				// unable to determine any external caller
				return typeof(Check);
			}
		}
		#endregion

		#region Log Output
		/// <summary>
		/// Log a message.  The current logging level is used to determine
		///		if the message is appended to the configured appender
		///		or if it is ignored.
		/// </summary>
		/// <param name="category">The category to which this log statement belongs.</param>
		/// <param name="s">The severity of the logging message.</param>
		/// <param name="errorMsg">A concise description of the problem encountered.</param>
		/// <param name="args">Variable values that are to be captured with the logging statement.</param>
		public static void Log( LogCategories category, Severity s, string errorMsg, params object[] args )
		{
			if( args != null && args.Length > 0 )
			{
				LogMessage( category, s, Format( s + ": " + errorMsg, args ), null );
			}
			else
			{
				LogMessage( category, s, errorMsg, null );
			}
		}

		/// <summary>
		/// Log a message.  The specified <see cref="Severity"/> level is compared against
		/// the current logging levels to determine if the message is logged or ignored.
		/// </summary>
		/// <param name="category">The category to which this log statement belongs.</param>
		/// <param name="s">The severity level of the logging message.</param>
		/// <param name="msg">The message to log.</param>
		public static void Log( LogCategories category, Severity s, string msg )
		{
			LogMessage( category, s, msg, null );
		}

		/// <summary>
		/// Log a message.  The specified <see cref="Severity"/> level is compared against
		/// the current logging levels to determine if the message is logged or ignored.
		/// </summary>
		/// <param name="category">The category to which this log statement belongs.</param>
		/// <param name="s">The severity level of the logging message.</param>
		/// <param name="msg">The message to log.</param>
		/// <param name="e">An exception to associate with the error being logged.</param>
		public static void Log( LogCategories category, Severity s, string msg, Exception e )
		{
			LogMessage( category, s, msg, e );
		}

		/// <summary>
		/// Log a message.  Actually perform the logging message to the
		///		appender specifified in the configuration file.
		/// </summary>
		/// <param name="category">The category to which this log statement belongs.</param>
		/// <param name="s">A <see cref="Severity"/> level which is used to determine if 
		/// the message should be logged or ignored.</param>
		/// <param name="msg">A string value describing the message.</param>
		/// <param name="e">An exception that has occurred.  If no exception has occurred, use <code>null</code>.</param>
		internal static void LogMessage( LogCategories category, Severity s, string msg, Exception e )
		{
			//			string platform = System.Environment.OSVersion.Platform.ToString();
			//			if( platform.StartsWith( "Win" ) )
			//				LogMessage( category, GetExternalCaller( e ), GetLevel( s ), msg, e );	
			//			else
			LogMessage( category, typeof(Check), GetLevel( s ), msg, e );
		}
		#endregion

		#region Log Output Convenience Methods
		/// <summary>
		/// Convenience methods for logging with a predefined severity level.
		/// </summary>
		public static void LogDebug( LogCategories category, string errorMsg, params object[] args )
		{
			Log( category, Severity.Debug, errorMsg, args );
		}

		/// <summary>
		/// Convenience methods for logging with a predefined severity level.
		/// </summary>
		public static void LogInfo( LogCategories category, string errorMsg, params object[] args )
		{
			Log( category, Severity.Info, errorMsg, args );
		}

		/// <summary>
		/// Convenience methods for logging with a predefined severity level.
		/// </summary>
		public static void LogWarning( LogCategories category, string errorMsg, params object[] args )
		{
			Log( category, Severity.Warning, errorMsg, args );
		}

		/// <summary>
		/// Log an error message. The error message is delegated to the log4net 
		/// appender(s) in the app config file.
		/// </summary>
		/// <param name="category">The category to which this log statement belongs.</param>
		/// <param name="errorMsg">A description of what has occurred.</param>
		/// <param name="args">Variable values that were present at
		///	the time the error occurred.</param>
		public static void LogError( LogCategories category, string errorMsg, params object[] args )
		{
			Log( category, Severity.Error, errorMsg, args );
		}

		/// <summary>
		/// Log an error message. Include the exception that has occurred
		///	in the text of the error message.
		/// </summary>
		/// <param name="category">The category to which this log statement belongs.</param>
		/// <param name="e">The exception to be logged.</param>
		public static void LogError( LogCategories category, Exception e )
		{
			Log( category, Severity.Error, "", e );
		}
		#endregion

		#region Log4net Output Helpers
		private static Level GetLevel( Severity s )
		{
			switch( s )
			{
				case Severity.Debug:
					return Level.Debug;
				case Severity.Info:
					return Level.Info;
				case Severity.Warning:
					return Level.Warn;
				case Severity.Error:
					return Level.Error;
				case Severity.Critical:
					return Level.Critical;
				default:
					return Level.Debug;
			}
		}

		private static ILog GetLogger( Type originator )
		{
			if( loggers.ContainsKey( originator ) )
			{
				return loggers[ originator ] as ILog;
			}
			else
			{
				ILog log = LogManager.GetLogger( originator );
				loggers[ originator ] = log;
				return log;
			}
		}

		private static void LogMessage( LogCategories category, Type originator, Level level, string msg, Exception e )
		{
			if( IsLogEnabled( category ) )
			{
				// If the standard .NET configuration file has syntax errors, or contains
				// unmatched section declarations and corresponding XML block, .NET throws an
				// error when log4net first accesses the file. These errors are handled by
				// log4net but are visible inside a debugger (you may ignore them).
				ILog log = GetLogger( originator );
				if( log != null )
				{
					log.Logger.Log( originator, level, msg, e );
				}
			}
		}
		#endregion
	}
}
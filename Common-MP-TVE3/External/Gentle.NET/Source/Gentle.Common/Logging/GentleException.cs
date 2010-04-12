/*
 * The Exception class used throughout the framework
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: GentleException.cs 1232 2008-03-14 05:36:00Z mm $
 */

using System;
using System.Runtime.Serialization;
using System.Text;

namespace Gentle.Common
{
	/// <summary>
	/// The exception class thrown by classes in the Gentle.Framework namespace. Because
	/// all new infomation resides in the ExceptionInfo descendant the Exception class itself
	/// doesn't add anything new.
	/// </summary>
	[Serializable]
	public class GentleException : ApplicationException
	{
		internal static bool verbose = true;
		/// <summary>
		/// An enumeration value indicating the cause of the current exception.
		/// </summary>
		private Error error;

		#region Constructors
		/// <summary>
		/// Constructor taking an <see cref="Error"/> and a custom message to go with the error. 
		/// The framework consistently uses the <see cref="Error"/> enumeration to classify 
		/// raised exceptions.
		/// </summary>
		/// <param name="error">The Error condition leading to this exception</param>
		/// <param name="msg">An additional message text</param>
		public GentleException( Error error, string msg ) : this( error, msg, null )
		{
		}

		/// <summary>
		/// Constructor taking an <see cref="Error"/>, a custom message to go with the error
		/// and the exception leading to this error. The framework consistently uses the 
		/// <see cref="Error"/> enumeration to classify raised exceptions.
		/// </summary>
		/// <param name="error">The Error condition leading to this exception</param>
		/// <param name="msg">A specific message to go with the error</param>
		/// <param name="e">An existing exception instance</param>
		public GentleException( Error error, string msg, Exception e ) : base( msg, e )
		{
			this.error = error;
		}

		/// <summary>
		/// Deserialization constructor (required for serializing exceptions)
		/// </summary>
		public GentleException( SerializationInfo info, StreamingContext context ) : base( info, context )
		{
			error = (Error) info.GetInt32( "Error" );
		}
		#endregion

		#region ISerializable Implementation
		/// <summary>
		/// ISerializable method used by the serialization engine to save custom fields
		/// </summary>
		public override void GetObjectData( SerializationInfo info, StreamingContext context )
		{
			base.GetObjectData( info, context );
			info.AddValue( "Error", (int) error );
		}
		#endregion

		/// <summary>
		/// The Error enum specifies the error class and is also used to obtain the message used.
		/// </summary>
		public Error Error
		{
			get { return error; }
		}

		/// <summary>
		/// The severity of the error leading to this exception.
		/// </summary>
		public Severity Severity
		{
			get { return GetSeverity( error ); }
		}

		private static Severity GetSeverity( Error error )
		{
			Attribute[] attr = (Attribute[]) error.GetType().GetCustomAttributes( typeof(LevelAttribute), true );
			if( attr != null && attr.Length > 0 && attr[ 0 ] is LevelAttribute )
			{
				return (attr[ 0 ] as LevelAttribute).Severity;
			}
			return Severity.Unclassified;
		}

		/// <summary>
		/// Returns the current GentleException and all nested exceptions as a multiline string.
		/// </summary>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			// if error is specified, prefix output with error and possibly severity info
			if( error != Error.Unspecified )
			{
				sb.AppendFormat( "Error: {0}{1}", error, GetSeverity( error ), Environment.NewLine );
				Severity s = GetSeverity( error );
				if( s != Severity.Unclassified )
				{
					sb.AppendFormat( "Severity: {0}{1}", s, Environment.NewLine );
				}
				sb.Append( Environment.NewLine );
			}
			sb.Append( base.ToString() );
			// this allows us to easily toggle whether inner exceptions are included or not
			if( verbose )
			{
				Exception x = base.InnerException;
				while( x != null )
				{
					sb.Append( x.ToString() );
					x = x.InnerException;
				}
			}
			return sb.ToString();
		}
	}
}
// ------------------------------------------------------------------
// DirectX.Capture
//
// History:
//	2003-Jan-24		BL		- created
//
// Copyright (c) 2003 Brian Low
// ------------------------------------------------------------------

using System;
using System.Runtime.InteropServices; 
using DShowNET;

namespace DirectX.Capture
{
	/// <summary>
	///  Represents a physical connector or source on an audio/video device.
	/// </summary>
	public class Source : IDisposable
	{

		// --------------------- Private/Internal properties -------------------------

		protected string				name;				// Name of the source



		// ----------------------- Public properties -------------------------

		/// <summary> The name of the source. Read-only. </summary>
		public string Name { get { return( name ); } }

		/// <summary> Obtains the String representation of this instance. </summary>
		public override string ToString() { return( Name ); }

		/// <summary> Is this source enabled. </summary>
		public virtual bool Enabled 
		{
			get { throw new NotSupportedException( "This method should be overriden in derrived classes." ); } 
			set { throw new NotSupportedException( "This method should be overriden in derrived classes." ); } 
		}

		
		// -------------------- Constructors/Destructors ----------------------

		/// <summary> Release unmanaged resources. </summary>
		~Source()
		{
			Dispose();
		}


		
		// -------------------- IDisposable -----------------------

		/// <summary> Release unmanaged resources. </summary>
		public virtual void Dispose()
		{
			name = null;
		}

	}
}

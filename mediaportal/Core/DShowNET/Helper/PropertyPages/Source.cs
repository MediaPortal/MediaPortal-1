#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Runtime.InteropServices; 
using DirectShowLib;

namespace DShowNET.Helper
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

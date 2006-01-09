/* 
 *	Copyright (C) 2005 Team MediaPortal
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

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DirectShowLib;

namespace DShowNET.Helper
{
	/// <summary>
	///  Property pages for a DirectShow filter (e.g. hardware device). These
	///  property pages do not support persisting their settings. 
	/// </summary>
	public class DirectShowPropertyPage : PropertyPage
	{
		// ---------------- Properties --------------------

		/// <summary> COM ISpecifyPropertyPages interface </summary>
		protected ISpecifyPropertyPages specifyPropertyPages;


		// ---------------- Constructors --------------------

		/// <summary> Constructor </summary>
		public DirectShowPropertyPage(string name, ISpecifyPropertyPages specifyPropertyPages)
		{
			Name = name;
			SupportsPersisting = false;
			this.specifyPropertyPages = specifyPropertyPages;
		}



		// ---------------- Public Methods --------------------

		/// <summary> 
		///  Show the property page. Some property pages cannot be displayed 
		///  while previewing and/or capturing. 
		/// </summary>
		public override void Show(Control owner)
		{
			DsCAUUID cauuid = new DsCAUUID();
			try
			{
				int hr = specifyPropertyPages.GetPages( out cauuid );
				if ( hr != 0 ) Marshal.ThrowExceptionForHR( hr );

				object o = specifyPropertyPages;
				hr = OleCreatePropertyFrame( owner.Handle, 30, 30, null, 1,
					ref o, cauuid.cElems, cauuid.pElems, 0, 0, IntPtr.Zero );
			}
			finally
			{
				if( cauuid.pElems != IntPtr.Zero )
					Marshal.FreeCoTaskMem( cauuid.pElems );
			}
		}

		/// <summary> Release unmanaged resources </summary>
		public new void Dispose()
		{
			if ( specifyPropertyPages != null )
				Marshal.ReleaseComObject( specifyPropertyPages ); specifyPropertyPages = null;
		}



		// ---------------- DLL Imports --------------------

		[DllImport("olepro32.dll", CharSet=CharSet.Unicode, ExactSpelling=true) ]
		private static extern int OleCreatePropertyFrame( 
			IntPtr hwndOwner, int x, int y,
			string lpszCaption, int cObjects,
			[In, MarshalAs(UnmanagedType.Interface)] ref object ppUnk,
			int cPages,	IntPtr pPageClsID, int lcid, int dwReserved, IntPtr pvReserved );


	}
}

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
using System.Windows.Forms;
using DShowNET;

namespace DirectX.Capture
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

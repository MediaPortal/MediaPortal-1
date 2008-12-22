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
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using DirectShowLib;
#pragma warning disable 618
namespace DShowNET.Helper
{
	/// <summary>
	///	 A collection of Filter objects (DirectShow filters).
	///	 This is used by the <see cref="Capture"/> class to provide
	///	 lists of capture devices and compression filters. This class
	///	 cannot be created directly.
	/// </summary>
	public class FilterCollection : CollectionBase
	{
		/// <summary> Populate the collection with a list of filters from a particular category. </summary>
		public FilterCollection(Guid category)
		{
			getFilters( category );
		}

    /// <summary> Populate the collection with a list of filters from a particular category. </summary>
    public FilterCollection(Guid category, bool resolveNames)
    {
      getFilters(category);
      foreach (Filter f in InnerList)
      {
        f.ResolveName();
      }
    }

		/// <summary> Populate the InnerList with a list of filters from a particular category </summary>
		protected void getFilters(Guid category)
		{
			int					hr;
			object				comObj = null;
			ICreateDevEnum		enumDev = null;
      IEnumMoniker enumMon = null;
      IMoniker[] mon = new IMoniker[1];

			try 
			{
				// Get the system device enumerator
				Type srvType = Type.GetTypeFromCLSID( ClassId.SystemDeviceEnum );
				if( srvType == null )
					throw new NotImplementedException( "System Device Enumerator" );
				comObj = Activator.CreateInstance( srvType );
				enumDev = (ICreateDevEnum) comObj;

				// Create an enumerator to find filters in category
				hr = enumDev.CreateClassEnumerator(  category, out enumMon, 0 );
        if( hr != 0 )
        {
          return;//throw new NotSupportedException( "No devices of the category" );
        }
				// Loop through the enumerator
        IntPtr f = IntPtr.Zero;
				do
				{
					// Next filter
					hr = enumMon.Next( 1, mon,  f );
					if( (hr != 0) || (mon[0] == null) )
						break;
					
					// Add the filter
					Filter filter = new Filter( mon[0] );
					InnerList.Add( filter );

					// Release resources
					DirectShowUtil.ReleaseComObject( mon[0] );
					mon[0] = null;
				}
				while(true);

				// Sort
				//InnerList.Sort();
			}
			finally
			{
				enumDev = null;
				if( mon[0] != null )
					DirectShowUtil.ReleaseComObject( mon[0] ); mon[0] = null;
				if( enumMon != null )
					DirectShowUtil.ReleaseComObject( enumMon ); enumMon = null;
				if( comObj != null )
					DirectShowUtil.ReleaseComObject( comObj ); comObj = null;
			}
		}

		/// <summary> Get the filter at the specified index. </summary>
		public Filter this[int index]
		{
			get { 
        if (index >= InnerList.Count) return null;
        return( (Filter) InnerList[index] ); 
      }
		}
	}
}

// ZipConstants.cs
// Copyright (C) 2001 Mike Krueger
//
// This file was translated from java, it was part of the GNU Classpath
// Copyright (C) 2001 Free Software Foundation, Inc.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// As a special exception, if you link this library with other files to
// produce an executable, this library does not by itself cause the
// resulting executable to be covered by the GNU General Public License.
// This exception does not however invalidate any other reasons why the
// executable file might be covered by the GNU General Public License.

namespace NZlib.Zip {
	
	/// <summary>
	/// This class contains constants used for zip.
	/// </summary>
	public class ZipConstants
	{
		/* The local file header */
		public const int LOCHDR = 30;
		public const int LOCSIG = 'P' | ('K' << 8) | (3 << 16) | (4 << 24);
		
		public const int LOCVER =  4;
		public const int LOCFLG =  6;
		public const int LOCHOW =  8;
		public const int LOCTIM = 10;
		public const int LOCCRC = 14;
		public const int LOCSIZ = 18;
		public const int LOCLEN = 22;
		public const int LOCNAM = 26;
		public const int LOCEXT = 28;
		
		/* The Data descriptor */
		public const int EXTSIG = 'P' | ('K' << 8) | (7 << 16) | (8 << 24);
		public const int EXTHDR = 16;
		
		public const int EXTCRC =  4;
		public const int EXTSIZ =  8;
		public const int EXTLEN = 12;
		
		/* The central directory file header */
		public const int CENSIG = 'P' | ('K' << 8) | (1 << 16) | (2 << 24);
		
		/* The central directory file header for 64bit ZIP*/
		public const int CENSIG64 = 0x06064b50;
		
		public const int CENHDR = 46;
		
		public const int CENVEM =  4;
		public const int CENVER =  6;
		public const int CENFLG =  8;
		public const int CENHOW = 10;
		public const int CENTIM = 12;
		public const int CENCRC = 16;
		public const int CENSIZ = 20;
		public const int CENLEN = 24;
		public const int CENNAM = 28;
		public const int CENEXT = 30;
		public const int CENCOM = 32;
		public const int CENDSK = 34;
		public const int CENATT = 36;
		public const int CENATX = 38;
		public const int CENOFF = 42;
		
		/* The entries in the end of central directory */
		public const int ENDSIG = 'P' | ('K' << 8) | (5 << 16) | (6 << 24);
		public const int ENDHDR = 22;
		
		/* The following two fields are missing in SUN JDK */
		public const int ENDNRD =  4;
		public const int ENDDCD =  6;
		
		public const int ENDSUB =  8;
		public const int ENDTOT = 10;
		public const int ENDSIZ = 12;
		public const int ENDOFF = 16;
		public const int ENDCOM = 20;
	}
}

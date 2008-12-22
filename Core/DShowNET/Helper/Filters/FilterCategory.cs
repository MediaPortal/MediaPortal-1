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
using System.Collections.Generic;
using System.Text;

namespace DShowNET.Helper
{
  public class FilterCategory		// uuids.h  :  CLSID_*
  {
    /// <summary> CLSID_AudioRendererCategory, audio render category </summary>
    public static readonly Guid AudioRendererDevice = new Guid(0xe0f158e1, 0xcb04, 0x11d0, 0xbd, 0x4e, 0x0, 0xa0, 0xc9, 0x11, 0xce, 0x86);

    /// <summary> CLSID_AudioInputDeviceCategory, audio capture category </summary>
    public static readonly Guid AudioInputDevice = new Guid(0x33d9a762, 0x90c8, 0x11d0, 0xbd, 0x43, 0x00, 0xa0, 0xc9, 0x11, 0xce, 0x86);

    /// <summary> CLSID_VideoInputDeviceCategory, video capture category </summary>
    public static readonly Guid VideoInputDevice = new Guid(0x860BB310, 0x5D01, 0x11d0, 0xBD, 0x3B, 0x00, 0xA0, 0xC9, 0x11, 0xCE, 0x86);

    /// <summary> CLSID_VideoCompressorCategory, Video compressor category </summary>
    public static readonly Guid VideoCompressorCategory = new Guid(0x33d9a760, 0x90c8, 0x11d0, 0xbd, 0x43, 0x0, 0xa0, 0xc9, 0x11, 0xce, 0x86);
    public static readonly Guid AM_KSTvTuner = new Guid(0xA799A800, 0xA46D, 0x11D0, 0xA1, 0x8C, 0x00, 0xA0, 0x24, 0x01, 0xDC, 0xD4);
    public static readonly Guid AM_KS_BDA_RECEIVER_COMPONENT = new Guid(0xFD0A5AF4, 0xB41D, 0x11d2, 0x9c, 0x95, 0x00, 0xc0, 0x4f, 0x79, 0x71, 0xe0);
    public static readonly Guid AM_KSCrossBar = new Guid(0xA799A801, 0xA46D, 0x11D0, 0xA1, 0x8C, 0x00, 0xA0, 0x24, 0x01, 0xDC, 0xD4);
    public static readonly Guid AM_KSEncoder = new Guid(0x19689bf6, 0xc384, 0x48fd, 0xad, 0x51, 0x90, 0xe5, 0x8c, 0x79, 0xf7, 0xb);
    /// <summary> CLSID_AudioCompressorCategory, audio compressor category </summary>
    public static readonly Guid AudioCompressorCategory = new Guid(0x33d9a761, 0x90c8, 0x11d0, 0xbd, 0x43, 0x0, 0xa0, 0xc9, 0x11, 0xce, 0x86);

    /// <summary> CLSID_LegacyAmFilterCategory, legacy filters </summary>
    public static readonly Guid LegacyAmFilterCategory = new Guid(0x083863F1, 0x70DE, 0x11d0, 0xBD, 0x40, 0x00, 0xA0, 0xC9, 0x11, 0xCE, 0x86);

    /// <summary>
    /// #MW# CLSID_ActiveMovieCategory, a superset of all the available filters
    /// </summary>
    public static readonly Guid ActiveMovieCategory = new Guid(0xda4e3da0, 0xd07d, 0x11d0, 0xbd, 0x50, 0x0, 0xa0, 0xc9, 0x11, 0xce, 0x86);

    public static readonly Guid IID_IKsPropertySet = new Guid(0x31efac30, 0x515c, 0x11d0, 0xa9, 0xaa, 0x00, 0xaa, 0x00, 0x61, 0xbe, 0x93);

  }
}

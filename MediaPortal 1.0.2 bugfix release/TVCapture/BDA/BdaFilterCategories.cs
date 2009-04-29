#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

namespace MediaPortal.TV.BDA
{
  /// <summary>
  /// BDA Filter Categories
  /// </summary>
  public class FilterCategories
  {
    public static Guid KSCATEGORY_BDA_RECEIVER_COMPONENT = new Guid(0xFD0A5AF4, 0xB41D, 0x11d2, 0x9c, 0x95, 0x00, 0xc0,
                                                                    0x4f, 0x79, 0x71, 0xe0);

    public static Guid KSCATEGORY_BDA_NETWORK_TUNER = new Guid(0x71985f48, 0x1ca1, 0x11d3, 0x9c, 0xc8, 0x00, 0xc0, 0x4f,
                                                               0x79, 0x71, 0xe0);

    public static Guid KSCATEGORY_BDA_NETWORK_EPG = new Guid(0x71985f49, 0x1ca1, 0x11d3, 0x9c, 0xc8, 0x00, 0xc0, 0x4f,
                                                             0x79, 0x71, 0xe0);

    public static Guid KSCATEGORY_BDA_IP_SINK = new Guid(0x71985f4a, 0x1ca1, 0x11d3, 0x9c, 0xc8, 0x00, 0xc0, 0x4f, 0x79,
                                                         0x71, 0xe0);

    public static Guid KSCATEGORY_BDA_NETWORK_PROVIDER = new Guid(0x71985f4b, 0x1ca1, 0x11d3, 0x9c, 0xc8, 0x00, 0xc0,
                                                                  0x4f, 0x79, 0x71, 0xe0);

    public static Guid KSCATEGORY_BDA_TRANSPORT_INFORMATION = new Guid(0xa2e3074f, 0x6c3d, 0x11d3, 0xb6, 0x53, 0x00,
                                                                       0xc0, 0x4f, 0x79, 0x49, 0x8e);
  }
}
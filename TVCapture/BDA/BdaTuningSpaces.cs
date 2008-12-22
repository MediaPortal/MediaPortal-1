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

namespace MediaPortal.TV.BDA
{
  public class TuningSpaces
  {
    public static Guid CLSID_SystemTuningSpaces = new Guid("D02AAC50-027E-11d3-9D8E-00C04F72D980");

    public static Guid CLSID_TuningSpace = new Guid("5FFDC5E6-B83A-4b55-B6E8-C69E765FE9DB");

    public static Guid CLSID_ATSCTuningSpace = new Guid("A2E30750-6C3D-11d3-B653-00C04F79498E");

    public static Guid CLSID_DVBTuningSpace = new Guid("C6B14B32-76AA-4a86-A7AC-5C79AAF58DA7");

    public static Guid CLSID_DVBSTuningSpace = new Guid("B64016F3-C9A2-4066-96F0-BD9563314726");
  }
}

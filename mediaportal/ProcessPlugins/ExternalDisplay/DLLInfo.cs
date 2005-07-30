/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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

using System.Runtime.InteropServices;

namespace ProcessPlugins.ExternalDisplay
{
  /// <summary>
  /// This structure is returned from the LCDHype drivers and contains important information about the display
  /// </summary>
  [StructLayout(LayoutKind.Sequential )]
  public struct DLLInfo
  {
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
    public char[] IDArray;             //Display description
    [MarshalAs(UnmanagedType.I1)]
    public bool SupportGfxLCD;         //does this driver support graphical LCDs?
    [MarshalAs(UnmanagedType.I1)]
    public bool SupportTxtLCD;         //does this driver support text
    [MarshalAs(UnmanagedType.I1)]
    public bool SupportLightSlider;    //does this driver support the light control slider
    [MarshalAs(UnmanagedType.I1)]
    public bool SupportContrastSlider; //does this driver support the contrast slider
    [MarshalAs(UnmanagedType.I1)]
    public bool SupportOutports;		   //does this driver support outports for controlling external circuits
    [MarshalAs(UnmanagedType.U1)]
    public byte CCharWidth;            //custom char width in pixels
    [MarshalAs(UnmanagedType.U1)]
    public byte CCharHeight;           //custom char height in pixels
    [MarshalAs(UnmanagedType.U1)]
    public byte FontPitch;             //fontpitch of LCD in pixels
  }
}

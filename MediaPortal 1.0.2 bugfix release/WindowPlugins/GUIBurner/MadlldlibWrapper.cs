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

//////////////////////////////////////////////////////////////////////////////
//
// MadlldlibWrapper -- Wrapper class to the madlldlib.dll
//
// Direct connection to DLL using P/Invoke. Provides the data structures and 
// function mappings.
//
//
//            !!!!!!! THIS CLASS LICENSED UNDER THE GPL !!!!!!!
//             See readme.txt in this distribution for details. 
//
//  	
///////////////////////////////////////////////////////////////////////////////
//
// Copyright (C) 2004 J. A. Robson, http://www.arbingersys.com
// 
// This program is free software; you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the Free 
// Software Foundation; either version 2 of the License, or (at your option) 
// any later version.
// 
// This program is distributed in the hope that it will be useful, but WITHOUT 
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or 
// FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for 
// more details.
// 
// You should have received a copy of the GNU General Public License along 
// with this program; if not, write to the Free Software Foundation, Inc., 
// 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace MediaPortal.GUI.GUIBurner
{
  // Alias; see SF_INFO struct:

  using sf_count_t = Int64;
#if PLATFORM_64
	using size_t = System.UInt64;
#else
  using size_t = UInt32;

#endif


  internal class MadlldlibWrapper
  {
    // Type declarations hence

    public enum mad_layer : int
    {
      MAD_LAYER_I = 1, // Layer I 
      MAD_LAYER_II = 2, // Layer II 
      MAD_LAYER_III = 3 // Layer III 
    } ;


    public enum mad_mode : int
    {
      MAD_MODE_SINGLE_CHANNEL = 0, // Single channel 
      MAD_MODE_DUAL_CHANNEL = 1, // Dual channel 
      MAD_MODE_JOINT_STEREO = 2, // Joint (MS/intensity) stereo 
      MAD_MODE_STEREO = 3 // Normal LR stereo 
    } ;


    public enum mad_emphasis : int
    {
      MAD_EMPHASIS_NONE = 0, // No emphasis 
      MAD_EMPHASIS_50_15_US = 1, // 50/15 microsecs emphasis 
      MAD_EMPHASIS_CCITT_J_17 = 3, // CCITT J.17 emphasis 
      MAD_EMPHASIS_RESERVED = 2 // Unknown emphasis 
    } ;


    //		public struct mad_timer_t 
    //		{
    //	  public long seconds;     // Whole seconds 
    //  public uint fraction;    // 1/MAD_TIMER_RESOLUTION seconds 
    //};


    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct mad_header
    {
      public mad_layer layer; // Audio layer (1, 2, or 3) 
      public mad_mode mode; // Channel mode 
      public int mode_extension; // Additional mode info 
      public mad_emphasis emphasis; // De-emphasis to use

      public uint bitrate; // Stream bitrate (bps) 
      public uint samplerate; // Sampling frequency (Hz) 

      public ushort crc_check; // Frame CRC accumulator 
      public ushort crc_target; // Final target CRC checksum 

      public int flags; // Flags 
      public int private_bits; // Private bits 

      //public mad_timer_t duration;        // Audio playing time of frame 
      public long seconds; // Whole seconds 
      public uint fraction; // 1/MAD_TIMER_RESOLUTION seconds 
    } ;


    // Decoding flags

    public const int DEC_WAV = 1;
    public const int DEC_PCM = 0;

    // Method declarations hence		

    // Callback function is used to pass values
    // related to the decoding process back to
    // the calling code. Use return value to 
    // cancel conversion (set to false)

    public delegate bool
      Callback(uint frameCount, uint byteCount, ref mad_header madHeader, bool kill);


    // Conversion routine referencing callback--
    // see madlldlib.cpp source for details about 
    // this function

    [DllImport("madlldlib.dll", EntryPoint = "CbMpegAudioDecoder")]
    public static extern int
      DecodeMP3(
      string inFile,
      string outFile,
      int decodeType, // WAV or PCM
      // Marshalled status message:
      [MarshalAs(UnmanagedType.LPStr)] StringBuilder statmsg,
      Callback updateFunction // Callback function
      );
  }
}
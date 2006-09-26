/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Collections.Generic;
using System.Text;

namespace ID3
{
  class FrameNames
  {
    public const string AENC = "AENC"; // Audio encryption 
    public const string APIC = "APIC"; // Attached picture 
    public const string COMM = "COMM"; // Comments 
    public const string COMR = "COMR"; // Commercial frame 
    public const string ENCR = "ENCR"; // Encryption method registration 
    public const string EQUA = "EQUA"; // Equalization 
    public const string ETCO = "ETCO"; // Event timing codes 
    public const string GEOB = "GEOB"; // General encapsulated object 
    public const string GRID = "GRID"; // Group identification registration 
    public const string IPLS = "IPLS"; // Involved people list 
    public const string LINK = "LINK"; // Linked information 
    public const string MCDI = "MCDI"; // Music CD identifier 
    public const string MLLT = "MLLT"; // MPEG location lookup table 
    public const string OWNE = "OWNE"; // Ownership frame 
    public const string PRIV = "PRIV"; // Private frame 
    public const string PCNT = "PCNT"; // Play counter 
    public const string POPM = "POPM"; // Popularimeter 
    public const string POSS = "POSS"; // Position synchronisation frame 
    public const string RBUF = "RBUF"; // Recommended buffer size 
    public const string RVAD = "RVAD"; // Relative volume adjustment 
    public const string RVRB = "RVRB"; // Reverb 
    public const string SYLT = "SYLT"; // Synchronized lyric/text 
    public const string YTC = "YTC";  // Synchronized tempo codes 
    public const string TALB = "TALB"; // Album/Movie/Show title 
    public const string TBPM = "TBPM"; // BPM (beats per minute) 
    public const string TCOM = "TCOM"; // Composer 
    public const string TCON = "TCON"; // Content type 
    public const string TCOP = "TCOP"; // Copyright message 
    public const string TDAT = "TDAT"; // Date 
    public const string TDLY = "TDLY"; // Playlist delay 
    public const string TENC = "TENC"; // Encoded by 
    public const string TEXT = "TEXT"; // Lyricist/Text writer 
    public const string TFLT = "TFLT"; // File type 
    public const string TIME = "TIME"; // Time 
    public const string TIT1 = "TIT1"; // Content group description 
    public const string TIT2 = "TIT2"; // Title/songname/content description 
    public const string TIT3 = "TIT3"; // Subtitle/Description refinement 
    public const string TKEY = "TKEY"; // Initial key 
    public const string TLAN = "TLAN"; // Language(s) 
    public const string TLEN = "TLEN"; // Length 
    public const string TMED = "TMED"; // Media type 
    public const string TOAL = "TOAL"; // Original album/movie/show title 
    public const string TOFN = "TOFN"; // Original filename 
    public const string TOLY = "TOLY"; // Original lyricist(s)/text writer(s) 
    public const string TOPE = "TOPE"; // Original artist(s)/performer(s) 
    public const string TORY = "TORY"; // Original release year 
    public const string TOWN = "TOWN"; // File owner/licensee 
    public const string TPE1 = "TPE1"; // Lead performer(s)/Soloist(s) 
    public const string TPE2 = "TPE2"; // Band/orchestra/accompaniment 
    public const string TPE3 = "TPE3"; // Conductor/performer refinement 
    public const string TPE4 = "TPE4"; // Interpreted, remixed, or otherwise modified by 
    public const string TPOS = "TPOS"; // Part of a set 
    public const string TPUB = "TPUB"; // Publisher 
    public const string TRCK = "TRCK"; // Track number/Position in set 
    public const string TRDA = "TRDA"; // Recording dates 
    public const string TRSN = "TRSN"; // Internet radio station name 
    public const string TRSO = "TRSO"; // Internet radio station owner 
    public const string TSIZ = "TSIZ"; // Size 
    public const string TSRC = "TSRC"; // ISRC (international standard recording code) 
    public const string TSSE = "TSSE"; // Software/Hardware and settings used for encoding 
    public const string TYER = "TYER"; // Year 
    public const string TXXX = "TXXX"; // User defined text information frame 

    // Other text Frames
    public const string TDEN = "TDEN"; // Encoding time 
    public const string TDOR = "TDOR"; // Original release time 
    public const string TDRC = "TDRC"; // Recording time 
    public const string TDRL = "TDRL"; // Release time 
    public const string TDTG = "TDTG"; // Tagging time 
    public const string TSOA = "TSOA"; // Album sort order 
    public const string TSOP = "TSOP"; // Performer sort order 
    public const string TSOT = "TSOT"; // Title sort order 

    public const string UFID = "UFID"; // Unique file identifier 
    public const string USER = "USER"; // Terms of use 
    public const string USLT = "USLT"; // Unsychronized lyric/text transcription 
    public const string WCOM = "WCOM"; // Commercial information 
    public const string WCOP = "WCOP"; // Copyright/Legal information 
    public const string WOAF = "WOAF"; // Official audio file webpage 
    public const string WOAR = "WOAR"; // Official artist/performer webpage 
    public const string WOAS = "WOAS"; // Official audio source webpage 
    public const string WORS = "WORS"; // Official internet radio station homepage 
    public const string WPAY = "WPAY"; // Payment 
    public const string WPUB = "WPUB"; // Publishers official webpage 
    public const string WXXX = "WXXX"; // User defined URL link frame 
  }

  class FrameNamesV2
  {
    public const string BUF = "BUF"; // Recommended buffer size
    public const string CNT = "CNT"; // Placounter
    public const string COM = "COM"; // Comments
    public const string CRA = "CRA"; // Audio encryption
    public const string CRM = "CRM"; // Encrypted meta frame
    public const string ETC = "ETC"; // Event timing codes
    public const string EQU = "EQU"; // Equalization
    public const string GEO = "GEO"; // General encapsulated object
    public const string IPL = "IPL"; // Involved people list
    public const string LNK = "LNK"; // Linked information
    public const string MCI = "MCI"; // Music CD Identifier
    public const string MLL = "MLL"; // MPEG location lookup table
    public const string PIC = "PIC"; // Attached picture
    public const string POP = "POP"; // Popularimeter
    public const string REV = "REV"; // Reverb
    public const string RVA = "RVA"; // Relative volume adjustment
    public const string SLT = "SLT"; // Synchronized lyric/text
    public const string STC = "STC"; // Synced tempo codes
    public const string TAL = "TAL"; // Album/Movie/Show title
    public const string TBP = "TBP"; // BPM (Beats Per Minute)
    public const string TCM = "TCM"; // Composer
    public const string TCO = "TCO"; // Content type
    public const string TCR = "TCR"; // Copyright message
    public const string TDA = "TDA"; // Date
    public const string TDY = "TDY"; // Playlist delay
    public const string TEN = "TEN"; // Encoded by
    public const string TFT = "TFT"; // File type
    public const string TIM = "TIM"; // Time
    public const string TKE = "TKE"; // Initial key
    public const string TLA = "TLA"; // Language(s)
    public const string TLE = "TLE"; // Length
    public const string TMT = "TMT"; // Media type
    public const string TOA = "TOA"; // Original artist(s)/performer(s)
    public const string TOF = "TOF"; // Original filename
    public const string TOL = "TOL"; // Original Lyricist(s)/text writer(s)
    public const string TOR = "TOR"; // Original release year
    public const string TOT = "TOT"; // Original album/Movie/Show title
    public const string TP1 = "TP1"; // Lead artist(s)/Lead performer(s)/Soloist(s)/Performing group
    public const string TP2 = "TP2"; // Band/Orchestra/Accompaniment
    public const string TP3 = "TP3"; // Conductor/Performer refinement
    public const string TP4 = "TP4"; // Interpreted, remixed, or otherwise modified by
    public const string TPA = "TPA"; // Part of a set
    public const string TPB = "TPB"; // Publisher
    public const string TRC = "TRC"; // ISRC (International Standard Recording Code)
    public const string TRD = "TRD"; // Recording dates
    public const string TRK = "TRK"; // Track number/Position in set
    public const string TSI = "TSI"; // Size
    public const string TSS = "TSS"; // Software/hardware and settings used for encoding
    public const string TT1 = "TT1"; // Content group description
    public const string TT2 = "TT2"; // Title/Songname/Content description
    public const string TT3 = "TT3"; // Subtitle/Description refinement
    public const string TXT = "TXT"; // Lyricist/text writer
    public const string TXX = "TXX"; // User defined text information frame
    public const string TYE = "TYE"; // Year
    public const string UFI = "UFI"; // Unique file identifier
    public const string ULT = "ULT"; // Unsychronized lyric/text transcription
    public const string WAF = "WAF"; // Official audio file webpage
    public const string WAR = "WAR"; // Official artist/performer webpage
    public const string WAS = "WAS"; // Official audio source webpage
    public const string WCM = "WCM"; // Commercial information
    public const string WCP = "WCP"; // Copyright/Legal information
    public const string WPB = "WPB"; // Publishers official webpage
    public const string WXX = "WXX"; // User defined URL link frame
  }
}

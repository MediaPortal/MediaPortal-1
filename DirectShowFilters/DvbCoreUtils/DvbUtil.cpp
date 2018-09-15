/* 
 *	Copyright (C) 2006-2008 Team MediaPortal
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
#include <windows.h>
#include "..\shared\DvbUtil.h"

/* CRC table for PSI sections */
static DWORD crc_table[256] = {
0x00000000, 0x04c11db7, 0x09823b6e, 0x0d4326d9, 0x130476dc, 0x17c56b6b,
0x1a864db2, 0x1e475005, 0x2608edb8, 0x22c9f00f, 0x2f8ad6d6, 0x2b4bcb61,
0x350c9b64, 0x31cd86d3, 0x3c8ea00a, 0x384fbdbd, 0x4c11db70, 0x48d0c6c7,
0x4593e01e, 0x4152fda9, 0x5f15adac, 0x5bd4b01b, 0x569796c2, 0x52568b75,
0x6a1936c8, 0x6ed82b7f, 0x639b0da6, 0x675a1011, 0x791d4014, 0x7ddc5da3,
0x709f7b7a, 0x745e66cd, 0x9823b6e0, 0x9ce2ab57, 0x91a18d8e, 0x95609039,
0x8b27c03c, 0x8fe6dd8b, 0x82a5fb52, 0x8664e6e5, 0xbe2b5b58, 0xbaea46ef,
0xb7a96036, 0xb3687d81, 0xad2f2d84, 0xa9ee3033, 0xa4ad16ea, 0xa06c0b5d,
0xd4326d90, 0xd0f37027, 0xddb056fe, 0xd9714b49, 0xc7361b4c, 0xc3f706fb,
0xceb42022, 0xca753d95, 0xf23a8028, 0xf6fb9d9f, 0xfbb8bb46, 0xff79a6f1,
0xe13ef6f4, 0xe5ffeb43, 0xe8bccd9a, 0xec7dd02d, 0x34867077, 0x30476dc0,
0x3d044b19, 0x39c556ae, 0x278206ab, 0x23431b1c, 0x2e003dc5, 0x2ac12072,
0x128e9dcf, 0x164f8078, 0x1b0ca6a1, 0x1fcdbb16, 0x018aeb13, 0x054bf6a4, 
0x0808d07d, 0x0cc9cdca, 0x7897ab07, 0x7c56b6b0, 0x71159069, 0x75d48dde,
0x6b93dddb, 0x6f52c06c, 0x6211e6b5, 0x66d0fb02, 0x5e9f46bf, 0x5a5e5b08,
0x571d7dd1, 0x53dc6066, 0x4d9b3063, 0x495a2dd4, 0x44190b0d, 0x40d816ba,
0xaca5c697, 0xa864db20, 0xa527fdf9, 0xa1e6e04e, 0xbfa1b04b, 0xbb60adfc,
0xb6238b25, 0xb2e29692, 0x8aad2b2f, 0x8e6c3698, 0x832f1041, 0x87ee0df6,
0x99a95df3, 0x9d684044, 0x902b669d, 0x94ea7b2a, 0xe0b41de7, 0xe4750050,
0xe9362689, 0xedf73b3e, 0xf3b06b3b, 0xf771768c, 0xfa325055, 0xfef34de2,
0xc6bcf05f, 0xc27dede8, 0xcf3ecb31, 0xcbffd686, 0xd5b88683, 0xd1799b34,
0xdc3abded, 0xd8fba05a, 0x690ce0ee, 0x6dcdfd59, 0x608edb80, 0x644fc637,
0x7a089632, 0x7ec98b85, 0x738aad5c, 0x774bb0eb, 0x4f040d56, 0x4bc510e1,
0x46863638, 0x42472b8f, 0x5c007b8a, 0x58c1663d, 0x558240e4, 0x51435d53,
0x251d3b9e, 0x21dc2629, 0x2c9f00f0, 0x285e1d47, 0x36194d42, 0x32d850f5,
0x3f9b762c, 0x3b5a6b9b, 0x0315d626, 0x07d4cb91, 0x0a97ed48, 0x0e56f0ff,
0x1011a0fa, 0x14d0bd4d, 0x19939b94, 0x1d528623, 0xf12f560e, 0xf5ee4bb9,
0xf8ad6d60, 0xfc6c70d7, 0xe22b20d2, 0xe6ea3d65, 0xeba91bbc, 0xef68060b,
0xd727bbb6, 0xd3e6a601, 0xdea580d8, 0xda649d6f, 0xc423cd6a, 0xc0e2d0dd,
0xcda1f604, 0xc960ebb3, 0xbd3e8d7e, 0xb9ff90c9, 0xb4bcb610, 0xb07daba7,
0xae3afba2, 0xaafbe615, 0xa7b8c0cc, 0xa379dd7b, 0x9b3660c6, 0x9ff77d71,
0x92b45ba8, 0x9675461f, 0x8832161a, 0x8cf30bad, 0x81b02d74, 0x857130c3,
0x5d8a9099, 0x594b8d2e, 0x5408abf7, 0x50c9b640, 0x4e8ee645, 0x4a4ffbf2,
0x470cdd2b, 0x43cdc09c, 0x7b827d21, 0x7f436096, 0x7200464f, 0x76c15bf8,
0x68860bfd, 0x6c47164a, 0x61043093, 0x65c52d24, 0x119b4be9, 0x155a565e,
0x18197087, 0x1cd86d30, 0x029f3d35, 0x065e2082, 0x0b1d065b, 0x0fdc1bec,
0x3793a651, 0x3352bbe6, 0x3e119d3f, 0x3ad08088, 0x2497d08d, 0x2056cd3a,
0x2d15ebe3, 0x29d4f654, 0xc5a92679, 0xc1683bce, 0xcc2b1d17, 0xc8ea00a0,
0xd6ad50a5, 0xd26c4d12, 0xdf2f6bcb, 0xdbee767c, 0xe3a1cbc1, 0xe760d676,
0xea23f0af, 0xeee2ed18, 0xf0a5bd1d, 0xf464a0aa, 0xf9278673, 0xfde69bc4,
0x89b8fd09, 0x8d79e0be, 0x803ac667, 0x84fbdbd0, 0x9abc8bd5, 0x9e7d9662,
0x933eb0bb, 0x97ffad0c, 0xafb010b1, 0xab710d06, 0xa6322bdf, 0xa2f33668,
0xbcb4666d, 0xb8757bda, 0xb5365d03, 0xb1f740b4};

//*******************************************************************
//* calculate crc for a data block
//* data : block of data   
//* len  : length of data
//*******************************************************************
DWORD crc32 (char *data, int len)
{
	register int i;
	DWORD crc = 0xffffffff;

	for (i=0; i<len; i++)
		crc = (crc << 8) ^ crc_table[((crc >> 24) ^ *data++) & 0xff];

	return crc;
}

//CDvbUtil::CDvbUtil(void)
//{
//}
//
//CDvbUtil::~CDvbUtil(void)
//{
//}
//
//void CDvbUtil::getString468A(BYTE *buf, int bufLen, char *text, int textLen)
//{
//  BYTE c;
//
//	int bufIndex = 0, textIndex = 0;
//
//  if (buf == NULL) return;
//  if (bufLen < 1) return;
//  if (text == NULL) return;
//  if (textLen < 2) return;
//
//  // reserve place for terminating 0
//  textLen--;
//  c = buf[bufIndex++];
//  if (c >= 0x20) //No encoding byte at start, default to DVB version of ISO-6937 encoding and convert to UTF-8
//  {
//    text[textIndex++] = 0x15; //Add UTF-8 encoding indicator to start of output string
//    text[textIndex] = 0;
//    WORD w;
//    int cl;
//    bufIndex--; //Start at beginning of buffer
//    while (bufIndex < bufLen)
//    {
//      c = buf[bufIndex++];
//      if (c == 0x8A) //CR/LF
//      {
//        c = '\r';
//      }
//      else if ((c <= 0x1F) || ((c >= 0x7F) && (c <= 0x9F))) //Ignore unsupported characters
//      {
//        continue; // ignore
//      }      
//      switch (c) {
//        //single byte characters
//        case 0xA4: w = (WORD)0x20AC; break; //Euro sign in DVB Standard "ETSI EN 300 468" as "Character code table 00" - the only difference to ISO 6937
//        case 0xA8: w = (WORD)0x00A4; break;
//        case 0xA9: w = (WORD)0x2018; break;
//        case 0xAA: w = (WORD)0x201C; break;
//        case 0xAC: w = (WORD)0x2190; break;
//        case 0xAD: w = (WORD)0x2191; break;
//        case 0xAE: w = (WORD)0x2192; break;
//        case 0xAF: w = (WORD)0x2193; break;
//        case 0xB4: w = (WORD)0x00D7; break;
//        case 0xB8: w = (WORD)0x00F7; break;
//        case 0xB9: w = (WORD)0x2019; break;
//        case 0xBA: w = (WORD)0x201D; break;
//        case 0xD0: w = (WORD)0x2015; break;
//        case 0xD1: w = (WORD)0xB9;   break;
//        case 0xD2: w = (WORD)0xAE;   break;
//        case 0xD3: w = (WORD)0xA9;   break;
//        case 0xD4: w = (WORD)0x2122; break;
//        case 0xD5: w = (WORD)0x266A; break;
//        case 0xD6: w = (WORD)0xAC;   break;
//        case 0xD7: w = (WORD)0xA6;   break;
//        case 0xDC: w = (WORD)0x215B; break;
//        case 0xDD: w = (WORD)0x215C; break;
//        case 0xDE: w = (WORD)0x215D; break;
//        case 0xDF: w = (WORD)0x215E; break;
//        case 0xE0: w = (WORD)0x2126; break;
//        case 0xE1: w = (WORD)0xC6;   break;
//        case 0xE2: w = (WORD)0x0110; break;
//        case 0xE3: w = (WORD)0xAA;   break;
//        case 0xE4: w = (WORD)0x0126; break;
//        case 0xE6: w = (WORD)0x0132; break;
//        case 0xE7: w = (WORD)0x013F; break;
//        case 0xE8: w = (WORD)0x0141; break;
//        case 0xE9: w = (WORD)0xD8;   break;
//        case 0xEA: w = (WORD)0x0152; break;
//        case 0xEB: w = (WORD)0xBA;   break;
//        case 0xEC: w = (WORD)0xDE;   break;
//        case 0xED: w = (WORD)0x0166; break;
//        case 0xEE: w = (WORD)0x014A; break;
//        case 0xEF: w = (WORD)0x0149; break;
//        case 0xF0: w = (WORD)0x0138; break;
//        case 0xF1: w = (WORD)0xE6;   break;
//        case 0xF2: w = (WORD)0x0111; break;
//        case 0xF3: w = (WORD)0xF0;   break;
//        case 0xF4: w = (WORD)0x0127; break;
//        case 0xF5: w = (WORD)0x0131; break;
//        case 0xF6: w = (WORD)0x0133; break;
//        case 0xF7: w = (WORD)0x0140; break;
//        case 0xF8: w = (WORD)0x0142; break;
//        case 0xF9: w = (WORD)0xF8;   break;
//        case 0xFA: w = (WORD)0x0153; break;
//        case 0xFB: w = (WORD)0xDF;   break;
//        case 0xFC: w = (WORD)0xFE;   break;
//        case 0xFD: w = (WORD)0x0167; break;
//        case 0xFE: w = (WORD)0x014B; break;
//        case 0xFF: w = (WORD)0xAD;   break;
//        //multibyte region C1
//        case 0xC1:
//          if (bufIndex >= bufLen) continue;
//          c = buf[bufIndex++];
//          switch (c)
//          {
//            case 0x41: w = (WORD)0xC0; break;
//            case 0x45: w = (WORD)0xC8; break;
//            case 0x49: w = (WORD)0xCC; break;
//            case 0x4F: w = (WORD)0xD2; break;
//            case 0x55: w = (WORD)0xD9; break;
//            case 0x61: w = (WORD)0xE0; break;
//            case 0x65: w = (WORD)0xE8; break;
//            case 0x69: w = (WORD)0xEC; break;
//            case 0x6F: w = (WORD)0xF2; break;
//            case 0x75: w = (WORD)0xF9; break;
//            default:   w = (WORD)c;    break; // unknown character --> fallback
//          }
//          break;
//        //multibyte region C2
//        case 0xC2:
//          if (bufIndex >= bufLen) continue;
//          c = buf[bufIndex++];
//          switch (c)
//          {
//            case 0x20: w = (WORD)0xB4;   break;
//            case 0x41: w = (WORD)0xC1;   break;
//            case 0x43: w = (WORD)0x0106; break;
//            case 0x45: w = (WORD)0xC9;   break;
//            case 0x49: w = (WORD)0xCD;   break;
//            case 0x4C: w = (WORD)0x0139; break;
//            case 0x4E: w = (WORD)0x0143; break;
//            case 0x4F: w = (WORD)0xD3;   break;
//            case 0x52: w = (WORD)0x0154; break;
//            case 0x53: w = (WORD)0x015A; break;
//            case 0x55: w = (WORD)0xDA;   break;
//            case 0x59: w = (WORD)0xDD;   break;
//            case 0x5A: w = (WORD)0x0179; break;
//            case 0x61: w = (WORD)0xE1;   break;
//            case 0x63: w = (WORD)0x0107; break;
//            case 0x65: w = (WORD)0xE9;   break;
//            case 0x69: w = (WORD)0xED;   break;
//            case 0x6C: w = (WORD)0x013A; break;
//            case 0x6E: w = (WORD)0x0144; break;
//            case 0x6F: w = (WORD)0xF3;   break;
//            case 0x72: w = (WORD)0x0155; break;
//            case 0x73: w = (WORD)0x015B; break;
//            case 0x75: w = (WORD)0xFA;   break;
//            case 0x79: w = (WORD)0xFD;   break;
//            case 0x7A: w = (WORD)0x017A; break;
//            default:   w = (WORD)c;      break; // unknown character --> fallback
//          }
//          break;
//        //multibyte region C3
//        case 0xC3:
//          if (bufIndex >= bufLen) continue;
//          c = buf[bufIndex++];
//          switch (c)
//          {
//            case 0x41: w = (WORD)0xC2;   break;
//            case 0x43: w = (WORD)0x0108; break;
//            case 0x45: w = (WORD)0xCA;   break;
//            case 0x47: w = (WORD)0x011C; break;
//            case 0x48: w = (WORD)0x0124; break;
//            case 0x49: w = (WORD)0xCE;   break;
//            case 0x4A: w = (WORD)0x0134; break;
//            case 0x4F: w = (WORD)0xD4;   break;
//            case 0x53: w = (WORD)0x015C; break;
//            case 0x55: w = (WORD)0xDB;   break;
//            case 0x57: w = (WORD)0x0174; break;
//            case 0x59: w = (WORD)0x0176; break;
//            case 0x61: w = (WORD)0xE2;   break;
//            case 0x63: w = (WORD)0x0109; break;
//            case 0x65: w = (WORD)0xEA;   break;
//            case 0x67: w = (WORD)0x011D; break;
//            case 0x68: w = (WORD)0x0125; break;
//            case 0x69: w = (WORD)0xEE;   break;
//            case 0x6A: w = (WORD)0x0135; break;
//            case 0x6F: w = (WORD)0xF4;   break;
//            case 0x73: w = (WORD)0x015D; break;
//            case 0x75: w = (WORD)0xFB;   break;
//            case 0x77: w = (WORD)0x0175; break;
//            case 0x79: w = (WORD)0x0177; break;
//            default:   w = (WORD)c;      break; // unknown character --> fallback
//          }
//          break;
//        //multibyte region C4
//        case 0xC4:
//          if (bufIndex >= bufLen) continue;
//          c = buf[bufIndex++];
//          switch (c)
//          {
//            case 0x41: w = (WORD)0x00C3; break;
//            case 0x49: w = (WORD)0x0128; break;
//            case 0x4E: w = (WORD)0x00D1; break;
//            case 0x4F: w = (WORD)0x00D5; break;
//            case 0x55: w = (WORD)0x0168; break;
//            case 0x61: w = (WORD)0x00E3; break;
//            case 0x69: w = (WORD)0x0129; break;
//            case 0x6E: w = (WORD)0x00F1; break;
//            case 0x6F: w = (WORD)0x00F5; break;
//            case 0x75: w = (WORD)0x0169; break;
//            default:   w = (WORD)c;      break; // unknown character --> fallback
//          }
//          break;
//        //multibyte region C5
//        case 0xC5:
//          if (bufIndex >= bufLen) continue;
//          c = buf[bufIndex++];
//          switch (c)
//          {
//            case 0x20: w = (WORD)0x00AF; break;
//            case 0x41: w = (WORD)0x0100; break;
//            case 0x45: w = (WORD)0x0112; break;
//            case 0x49: w = (WORD)0x012A; break;
//            case 0x4F: w = (WORD)0x014C; break;
//            case 0x55: w = (WORD)0x016A; break;
//            case 0x61: w = (WORD)0x0101; break;
//            case 0x65: w = (WORD)0x0113; break;
//            case 0x69: w = (WORD)0x012B; break;
//            case 0x6F: w = (WORD)0x014D; break;
//            case 0x75: w = (WORD)0x016B; break;
//            default:   w = (WORD)c;      break; // unknown character --> fallback
//          }
//          break;
//        //multibyte region C6
//        case 0xC6:
//          if (bufIndex >= bufLen) continue;
//          c = buf[bufIndex++];
//          switch (c)
//          {
//            case 0x20: w = (WORD)0x02D8; break;
//            case 0x41: w = (WORD)0x0102; break;
//            case 0x47: w = (WORD)0x011E; break;
//            case 0x55: w = (WORD)0x016C; break;
//            case 0x61: w = (WORD)0x0103; break;
//            case 0x67: w = (WORD)0x011F; break;
//            case 0x75: w = (WORD)0x016D; break;
//            default:   w = (WORD)c;      break; // unknown character --> fallback
//          }
//          break;
//        //multibyte region C7
//        case 0xC7:
//          if (bufIndex >= bufLen) continue;
//          c = buf[bufIndex++];
//          switch (c)
//          {
//            case 0x20: w = (WORD)0x02D9; break;
//            case 0x43: w = (WORD)0x010A; break;
//            case 0x45: w = (WORD)0x0116; break;
//            case 0x47: w = (WORD)0x0120; break;
//            case 0x49: w = (WORD)0x0130; break;
//            case 0x5A: w = (WORD)0x017B; break;
//            case 0x63: w = (WORD)0x010B; break;
//            case 0x65: w = (WORD)0x0117; break;
//            case 0x67: w = (WORD)0x0121; break;
//            case 0x7A: w = (WORD)0x017C; break;
//            default:   w = (WORD)c;      break; // unknown character --> fallback
//          }
//          break;
//        //multibyte region C8
//        case 0xC8:
//          if (bufIndex >= bufLen) continue;
//          c = buf[bufIndex++];
//          switch (c)
//          {
//            case 0x20: w = (WORD)0x00A8; break;
//            case 0x41: w = (WORD)0x00C4; break;
//            case 0x45: w = (WORD)0x00CB; break;
//            case 0x49: w = (WORD)0x00CF; break;
//            case 0x4F: w = (WORD)0x00D6; break;
//            case 0x55: w = (WORD)0x00DC; break;
//            case 0x59: w = (WORD)0x0178; break;
//            case 0x61: w = (WORD)0x00E4; break;
//            case 0x65: w = (WORD)0x00EB; break;
//            case 0x69: w = (WORD)0x00EF; break;
//            case 0x6F: w = (WORD)0x00F6; break;
//            case 0x75: w = (WORD)0x00FC; break;
//            case 0x79: w = (WORD)0x00FF; break;
//            default:   w = (WORD)c;     break; // unknown character --> fallback
//          }
//          break;
//        //multibyte region CA
//        case 0xCA:
//          if (bufIndex >= bufLen) continue;
//          c = buf[bufIndex++];
//          switch (c)
//          {
//            case 0x20: w = (WORD)0x02DA; break;
//            case 0x41: w = (WORD)0xC5;   break;
//            case 0x55: w = (WORD)0x016E; break;
//            case 0x61: w = (WORD)0xE5;   break;
//            case 0x75: w = (WORD)0x016F; break;
//            default:   w = (WORD)c;      break; // unknown character --> fallback
//          }
//          break;
//        //multibyte region CB
//        case 0xCB:
//          if (bufIndex >= bufLen) continue;
//          c = buf[bufIndex++];
//          switch (c)
//          {
//            case 0x20: w = (WORD)0xB8;   break;
//            case 0x43: w = (WORD)0xC7;   break;
//            case 0x47: w = (WORD)0x0122; break;
//            case 0x4B: w = (WORD)0x136;  break;
//            case 0x4C: w = (WORD)0x013B; break;
//            case 0x4E: w = (WORD)0x0145; break;
//            case 0x52: w = (WORD)0x0156; break;
//            case 0x53: w = (WORD)0x015E; break;
//            case 0x54: w = (WORD)0x0162; break;
//            case 0x63: w = (WORD)0xE7;   break;
//            case 0x67: w = (WORD)0x0123; break;
//            case 0x6B: w = (WORD)0x0137; break;
//            case 0x6C: w = (WORD)0x013C; break;
//            case 0x6E: w = (WORD)0x0146; break;
//            case 0x72: w = (WORD)0x0157; break;
//            case 0x73: w = (WORD)0x015F; break;
//            case 0x74: w = (WORD)0x0163; break;
//            default:   w = (WORD)c;      break; // unknown character --> fallback
//          }
//          break;
//        //multibyte region CD
//        case 0xCD:
//          if (bufIndex >= bufLen) continue;
//          c = buf[bufIndex++];
//          switch (c)
//          {
//            case 0x20: w = (WORD)0x02DD; break;
//            case 0x4F: w = (WORD)0x0150; break;
//            case 0x55: w = (WORD)0x0170; break;
//            case 0x6F: w = (WORD)0x0151; break;
//            case 0x75: w = (WORD)0x0171; break;
//            default:   w = (WORD)c;      break; // unknown character --> fallback
//          }
//          break;
//        //multibyte region CE
//        case 0xCE:
//          if (bufIndex >= bufLen) continue;
//          c = buf[bufIndex++];
//          switch (c)
//          {
//            case 0x20: w = (WORD)0x02DB; break;
//            case 0x41: w = (WORD)0x0104; break;
//            case 0x45: w = (WORD)0x0118; break;
//            case 0x49: w = (WORD)0x012E; break;
//            case 0x55: w = (WORD)0x0172; break;
//            case 0x61: w = (WORD)0x0105; break;
//            case 0x65: w = (WORD)0x0119; break;
//            case 0x69: w = (WORD)0x012F; break;
//            case 0x75: w = (WORD)0x0173; break;
//            default:   w = (WORD)c;      break; // unknown character --> fallback
//          }
//          break;
//        //multibyte region CF
//        case 0xCF:
//          if (bufIndex >= bufLen) continue;
//          c = buf[bufIndex++];
//          switch (c)
//          {
//            case 0x20: w = (WORD)0x02C7; break;
//            case 0x43: w = (WORD)0x010C; break;
//            case 0x44: w = (WORD)0x010E; break;
//            case 0x45: w = (WORD)0x011A; break;
//            case 0x4C: w = (WORD)0x013D; break;
//            case 0x4E: w = (WORD)0x0147; break;
//            case 0x52: w = (WORD)0x0158; break;
//            case 0x53: w = (WORD)0x0160; break;
//            case 0x54: w = (WORD)0x0164; break;
//            case 0x5A: w = (WORD)0x017D; break;
//            case 0x63: w = (WORD)0x010D; break;
//            case 0x64: w = (WORD)0x010F; break;
//            case 0x65: w = (WORD)0x011B; break;
//            case 0x6C: w = (WORD)0x013E; break;
//            case 0x6E: w = (WORD)0x0148; break;
//            case 0x72: w = (WORD)0x0159; break;
//            case 0x73: w = (WORD)0x0161; break;
//            case 0x74: w = (WORD)0x0165; break;
//            case 0x7A: w = (WORD)0x017E; break;
//            default:   w = (WORD)c;      break; // unknown character --> fallback
//          }
//          break;
//        // rest is the same
//        default: w = (WORD)c; break;
//      }
//      
//      //Convert to UTF-8
//      if (w < 0x80)
//        cl = 1;
//      else if (w < 0x800)
//        cl = 2;
//      else
//        cl = 3;
//      if (textIndex + cl >= textLen)
//        break;
//      if (w < 0x80)
//      {
//        text[textIndex++] = (char)w;
//      }
//      else if (w < 0x800)
//      {
//        text[textIndex++] = (char)((w >> 6) | 0xC0);
//        text[textIndex++] = (char)((w & 0x3F) | 0x80);
//      }
//      else
//      {
//        text[textIndex++] = (char)((w >> 12) | 0xE0);
//        text[textIndex++] = (char)(((w >> 6) & 0x3F) | 0x80);
//        text[textIndex++] = (char)((w & 0x3F) | 0x80);
//      }
//    }
//  }
//  else if (c == 0x11) //ISO/IEC 10646 encoding
//  {
//    // process 2 byte unicode characters by reencoding it 
//    // to UTF-8 to avoid zero bytes inside string
//    WORD w;
//    text[textIndex++] = 0x15;
//    text[textIndex] = 0;
//    while (bufIndex + 1 < bufLen)
//    {
//      w = (buf[bufIndex++] << 8);
//      w |= buf[bufIndex++];
//      if (w == 0xE08A) //CR/LF
//        w = '\r';
//      else if ((w <= 0x1F) || ((w >= 0xE07F) && (w <= 0xE09F))) //Ignore unsupported characters
//        w = 0;
//      if (w != 0)
//      {
//        if (w < 0x80)
//          c = 1;
//        else if (w < 0x800)
//          c = 2;
//        else
//          c = 3;
//        if (textIndex + c >= textLen)
//          break;
//        if (w < 0x80)
//          text[textIndex++] = (char)w;
//        else if (w < 0x800)
//        {
//          text[textIndex++] = (char)((w >> 6) | 0xC0);
//          text[textIndex++] = (char)((w & 0x3F) | 0x80);
//        }
//        else
//        {
//          text[textIndex++] = (char)((w >> 12) | 0xE0);
//          text[textIndex++] = (char)(((w >> 6) & 0x3F) | 0x80);
//          text[textIndex++] = (char)((w & 0x3F) | 0x80);
//        }
//      }
//    }
//  }
//  else if (c == 0x15) //UTF-8 encoding
//  {
//    // Copy first byte
//    text[textIndex++] = c;
//    text[textIndex] = 0;
//    
//    //Process remaining bytes
//    while ((bufIndex < bufLen) && (textIndex < textLen))
//    {
//      c = buf[bufIndex++];
//      if (c != 0)
//        text[textIndex++] = c;
//    }
//  }
//  else //All other encodings
//  {
//    // Deal with first byte - check for character coding info
//    if (c == 0x10) // three byte encoding
//    {      
//      if ((textLen >= 3) && (buf[2] >= 0x1) && (buf[2] <= 0xF))
//      {
//        text[textIndex++] = c;
//        text[textIndex++] = 0x20; //Make 2nd output byte non-zero
//        text[textIndex++] = buf[2];
//        text[textIndex] = 0;
//        bufIndex += 2;
//      }
//      else
//      {
//        text[textIndex] = 0;
//        return;
//      }
//    }
//    else //Single-byte encoding
//    {
//      if ((c < 0x7F) || (c > 0x9F)) //Only allow used/supported values
//      {
//        text[textIndex++] = c; //Copy first byte (may be coding selector byte, or first character if default coding table used)
//      }
//      else if (c == 0x8A) //CR/LF
//      {
//        text[textIndex++] = '\r';
//      }
//      text[textIndex] = 0;
//    }
//    
//    //Process remaining bytes
//    while ((bufIndex < bufLen) && (textIndex < textLen))
//    {
//      c = buf[bufIndex++];
//      if (c == 0x8A) //CR/LF
//      {
//        c = '\r';
//      }
//      else if ((c <= 0x1F) || ((c >= 0x7F) && (c <= 0x9F))) //Ignore unsupported characters
//      {
//        c = 0; // ignore
//      }
//
//      if (c != 0)
//        text[textIndex++] = c;
//    }
//  }
//  text[textIndex] = 0;
//}

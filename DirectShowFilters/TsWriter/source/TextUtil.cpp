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

#include "TextUtil.h"
#include "FreesatHuffmanTables.h"

extern void LogDebug(const char *fmt, ...) ;

CTextUtil::CTextUtil(void)
{
}

CTextUtil::~CTextUtil(void)
{
}

void CTextUtil::DvbTextToString(BYTE *buf, int bufLen, char *text, int textLen)
{
  BYTE c;

	int bufIndex = 0, textIndex = 0;

  if (buf == NULL) return;
  if (bufLen < 1) return;
  if (text == NULL) return;
  if (textLen < 2) return;

  // reserve place for terminating 0
  textLen--;
  c = buf[bufIndex++];
  if (c >= 0x20) //No encoding byte at start, default to DVB version of ISO-6937 encoding and convert to UTF-8
  {
    text[textIndex++] = 0x15; //Add UTF-8 encoding indicator to start of output string
    text[textIndex] = 0;
    WORD w;
    int cl;
    bufIndex--; //Start at beginning of buffer
    while (bufIndex < bufLen)
    {
      c = buf[bufIndex++];
      if (c == 0x8A) //CR/LF
      {
        c = '\r';
      }
      else if ((c <= 0x1F) || ((c >= 0x7F) && (c <= 0x9F))) //Ignore unsupported characters
      {
        continue; // ignore
      }      
      switch (c) {
        //single byte characters
        case 0xA4: w = (WORD)0x20AC; break; //Euro sign in DVB Standard "ETSI EN 300 468" as "Character code table 00" - the only difference to ISO 6937
        case 0xA8: w = (WORD)0x00A4; break;
        case 0xA9: w = (WORD)0x2018; break;
        case 0xAA: w = (WORD)0x201C; break;
        case 0xAC: w = (WORD)0x2190; break;
        case 0xAD: w = (WORD)0x2191; break;
        case 0xAE: w = (WORD)0x2192; break;
        case 0xAF: w = (WORD)0x2193; break;
        case 0xB4: w = (WORD)0x00D7; break;
        case 0xB8: w = (WORD)0x00F7; break;
        case 0xB9: w = (WORD)0x2019; break;
        case 0xBA: w = (WORD)0x201D; break;
        case 0xD0: w = (WORD)0x2015; break;
        case 0xD1: w = (WORD)0xB9;   break;
        case 0xD2: w = (WORD)0xAE;   break;
        case 0xD3: w = (WORD)0xA9;   break;
        case 0xD4: w = (WORD)0x2122; break;
        case 0xD5: w = (WORD)0x266A; break;
        case 0xD6: w = (WORD)0xAC;   break;
        case 0xD7: w = (WORD)0xA6;   break;
        case 0xDC: w = (WORD)0x215B; break;
        case 0xDD: w = (WORD)0x215C; break;
        case 0xDE: w = (WORD)0x215D; break;
        case 0xDF: w = (WORD)0x215E; break;
        case 0xE0: w = (WORD)0x2126; break;
        case 0xE1: w = (WORD)0xC6;   break;
        case 0xE2: w = (WORD)0x0110; break;
        case 0xE3: w = (WORD)0xAA;   break;
        case 0xE4: w = (WORD)0x0126; break;
        case 0xE6: w = (WORD)0x0132; break;
        case 0xE7: w = (WORD)0x013F; break;
        case 0xE8: w = (WORD)0x0141; break;
        case 0xE9: w = (WORD)0xD8;   break;
        case 0xEA: w = (WORD)0x0152; break;
        case 0xEB: w = (WORD)0xBA;   break;
        case 0xEC: w = (WORD)0xDE;   break;
        case 0xED: w = (WORD)0x0166; break;
        case 0xEE: w = (WORD)0x014A; break;
        case 0xEF: w = (WORD)0x0149; break;
        case 0xF0: w = (WORD)0x0138; break;
        case 0xF1: w = (WORD)0xE6;   break;
        case 0xF2: w = (WORD)0x0111; break;
        case 0xF3: w = (WORD)0xF0;   break;
        case 0xF4: w = (WORD)0x0127; break;
        case 0xF5: w = (WORD)0x0131; break;
        case 0xF6: w = (WORD)0x0133; break;
        case 0xF7: w = (WORD)0x0140; break;
        case 0xF8: w = (WORD)0x0142; break;
        case 0xF9: w = (WORD)0xF8;   break;
        case 0xFA: w = (WORD)0x0153; break;
        case 0xFB: w = (WORD)0xDF;   break;
        case 0xFC: w = (WORD)0xFE;   break;
        case 0xFD: w = (WORD)0x0167; break;
        case 0xFE: w = (WORD)0x014B; break;
        case 0xFF: w = (WORD)0xAD;   break;
        //multibyte region C1
        case 0xC1:
          if (bufIndex >= bufLen) continue;
          c = buf[bufIndex++];
          switch (c)
          {
            case 0x41: w = (WORD)0xC0; break;
            case 0x45: w = (WORD)0xC8; break;
            case 0x49: w = (WORD)0xCC; break;
            case 0x4F: w = (WORD)0xD2; break;
            case 0x55: w = (WORD)0xD9; break;
            case 0x61: w = (WORD)0xE0; break;
            case 0x65: w = (WORD)0xE8; break;
            case 0x69: w = (WORD)0xEC; break;
            case 0x6F: w = (WORD)0xF2; break;
            case 0x75: w = (WORD)0xF9; break;
            default:   w = (WORD)c;    break; // unknown character --> fallback
          }
          break;
        //multibyte region C2
        case 0xC2:
          if (bufIndex >= bufLen) continue;
          c = buf[bufIndex++];
          switch (c)
          {
            case 0x20: w = (WORD)0xB4;   break;
            case 0x41: w = (WORD)0xC1;   break;
            case 0x43: w = (WORD)0x0106; break;
            case 0x45: w = (WORD)0xC9;   break;
            case 0x49: w = (WORD)0xCD;   break;
            case 0x4C: w = (WORD)0x0139; break;
            case 0x4E: w = (WORD)0x0143; break;
            case 0x4F: w = (WORD)0xD3;   break;
            case 0x52: w = (WORD)0x0154; break;
            case 0x53: w = (WORD)0x015A; break;
            case 0x55: w = (WORD)0xDA;   break;
            case 0x59: w = (WORD)0xDD;   break;
            case 0x5A: w = (WORD)0x0179; break;
            case 0x61: w = (WORD)0xE1;   break;
            case 0x63: w = (WORD)0x0107; break;
            case 0x65: w = (WORD)0xE9;   break;
            case 0x69: w = (WORD)0xED;   break;
            case 0x6C: w = (WORD)0x013A; break;
            case 0x6E: w = (WORD)0x0144; break;
            case 0x6F: w = (WORD)0xF3;   break;
            case 0x72: w = (WORD)0x0155; break;
            case 0x73: w = (WORD)0x015B; break;
            case 0x75: w = (WORD)0xFA;   break;
            case 0x79: w = (WORD)0xFD;   break;
            case 0x7A: w = (WORD)0x017A; break;
            default:   w = (WORD)c;      break; // unknown character --> fallback
          }
          break;
        //multibyte region C3
        case 0xC3:
          if (bufIndex >= bufLen) continue;
          c = buf[bufIndex++];
          switch (c)
          {
            case 0x41: w = (WORD)0xC2;   break;
            case 0x43: w = (WORD)0x0108; break;
            case 0x45: w = (WORD)0xCA;   break;
            case 0x47: w = (WORD)0x011C; break;
            case 0x48: w = (WORD)0x0124; break;
            case 0x49: w = (WORD)0xCE;   break;
            case 0x4A: w = (WORD)0x0134; break;
            case 0x4F: w = (WORD)0xD4;   break;
            case 0x53: w = (WORD)0x015C; break;
            case 0x55: w = (WORD)0xDB;   break;
            case 0x57: w = (WORD)0x0174; break;
            case 0x59: w = (WORD)0x0176; break;
            case 0x61: w = (WORD)0xE2;   break;
            case 0x63: w = (WORD)0x0109; break;
            case 0x65: w = (WORD)0xEA;   break;
            case 0x67: w = (WORD)0x011D; break;
            case 0x68: w = (WORD)0x0125; break;
            case 0x69: w = (WORD)0xEE;   break;
            case 0x6A: w = (WORD)0x0135; break;
            case 0x6F: w = (WORD)0xF4;   break;
            case 0x73: w = (WORD)0x015D; break;
            case 0x75: w = (WORD)0xFB;   break;
            case 0x77: w = (WORD)0x0175; break;
            case 0x79: w = (WORD)0x0177; break;
            default:   w = (WORD)c;      break; // unknown character --> fallback
          }
          break;
        //multibyte region C4
        case 0xC4:
          if (bufIndex >= bufLen) continue;
          c = buf[bufIndex++];
          switch (c)
          {
            case 0x41: w = (WORD)0x00C3; break;
            case 0x49: w = (WORD)0x0128; break;
            case 0x4E: w = (WORD)0x00D1; break;
            case 0x4F: w = (WORD)0x00D5; break;
            case 0x55: w = (WORD)0x0168; break;
            case 0x61: w = (WORD)0x00E3; break;
            case 0x69: w = (WORD)0x0129; break;
            case 0x6E: w = (WORD)0x00F1; break;
            case 0x6F: w = (WORD)0x00F5; break;
            case 0x75: w = (WORD)0x0169; break;
            default:   w = (WORD)c;      break; // unknown character --> fallback
          }
          break;
        //multibyte region C5
        case 0xC5:
          if (bufIndex >= bufLen) continue;
          c = buf[bufIndex++];
          switch (c)
          {
            case 0x20: w = (WORD)0x00AF; break;
            case 0x41: w = (WORD)0x0100; break;
            case 0x45: w = (WORD)0x0112; break;
            case 0x49: w = (WORD)0x012A; break;
            case 0x4F: w = (WORD)0x014C; break;
            case 0x55: w = (WORD)0x016A; break;
            case 0x61: w = (WORD)0x0101; break;
            case 0x65: w = (WORD)0x0113; break;
            case 0x69: w = (WORD)0x012B; break;
            case 0x6F: w = (WORD)0x014D; break;
            case 0x75: w = (WORD)0x016B; break;
            default:   w = (WORD)c;      break; // unknown character --> fallback
          }
          break;
        //multibyte region C6
        case 0xC6:
          if (bufIndex >= bufLen) continue;
          c = buf[bufIndex++];
          switch (c)
          {
            case 0x20: w = (WORD)0x02D8; break;
            case 0x41: w = (WORD)0x0102; break;
            case 0x47: w = (WORD)0x011E; break;
            case 0x55: w = (WORD)0x016C; break;
            case 0x61: w = (WORD)0x0103; break;
            case 0x67: w = (WORD)0x011F; break;
            case 0x75: w = (WORD)0x016D; break;
            default:   w = (WORD)c;      break; // unknown character --> fallback
          }
          break;
        //multibyte region C7
        case 0xC7:
          if (bufIndex >= bufLen) continue;
          c = buf[bufIndex++];
          switch (c)
          {
            case 0x20: w = (WORD)0x02D9; break;
            case 0x43: w = (WORD)0x010A; break;
            case 0x45: w = (WORD)0x0116; break;
            case 0x47: w = (WORD)0x0120; break;
            case 0x49: w = (WORD)0x0130; break;
            case 0x5A: w = (WORD)0x017B; break;
            case 0x63: w = (WORD)0x010B; break;
            case 0x65: w = (WORD)0x0117; break;
            case 0x67: w = (WORD)0x0121; break;
            case 0x7A: w = (WORD)0x017C; break;
            default:   w = (WORD)c;      break; // unknown character --> fallback
          }
          break;
        //multibyte region C8
        case 0xC8:
          if (bufIndex >= bufLen) continue;
          c = buf[bufIndex++];
          switch (c)
          {
            case 0x20: w = (WORD)0x00A8; break;
            case 0x41: w = (WORD)0x00C4; break;
            case 0x45: w = (WORD)0x00CB; break;
            case 0x49: w = (WORD)0x00CF; break;
            case 0x4F: w = (WORD)0x00D6; break;
            case 0x55: w = (WORD)0x00DC; break;
            case 0x59: w = (WORD)0x0178; break;
            case 0x61: w = (WORD)0x00E4; break;
            case 0x65: w = (WORD)0x00EB; break;
            case 0x69: w = (WORD)0x00EF; break;
            case 0x6F: w = (WORD)0x00F6; break;
            case 0x75: w = (WORD)0x00FC; break;
            case 0x79: w = (WORD)0x00FF; break;
            default:   w = (WORD)c;     break; // unknown character --> fallback
          }
          break;
        //multibyte region CA
        case 0xCA:
          if (bufIndex >= bufLen) continue;
          c = buf[bufIndex++];
          switch (c)
          {
            case 0x20: w = (WORD)0x02DA; break;
            case 0x41: w = (WORD)0xC5;   break;
            case 0x55: w = (WORD)0x016E; break;
            case 0x61: w = (WORD)0xE5;   break;
            case 0x75: w = (WORD)0x016F; break;
            default:   w = (WORD)c;      break; // unknown character --> fallback
          }
          break;
        //multibyte region CB
        case 0xCB:
          if (bufIndex >= bufLen) continue;
          c = buf[bufIndex++];
          switch (c)
          {
            case 0x20: w = (WORD)0xB8;   break;
            case 0x43: w = (WORD)0xC7;   break;
            case 0x47: w = (WORD)0x0122; break;
            case 0x4B: w = (WORD)0x136;  break;
            case 0x4C: w = (WORD)0x013B; break;
            case 0x4E: w = (WORD)0x0145; break;
            case 0x52: w = (WORD)0x0156; break;
            case 0x53: w = (WORD)0x015E; break;
            case 0x54: w = (WORD)0x0162; break;
            case 0x63: w = (WORD)0xE7;   break;
            case 0x67: w = (WORD)0x0123; break;
            case 0x6B: w = (WORD)0x0137; break;
            case 0x6C: w = (WORD)0x013C; break;
            case 0x6E: w = (WORD)0x0146; break;
            case 0x72: w = (WORD)0x0157; break;
            case 0x73: w = (WORD)0x015F; break;
            case 0x74: w = (WORD)0x0163; break;
            default:   w = (WORD)c;      break; // unknown character --> fallback
          }
          break;
        //multibyte region CD
        case 0xCD:
          if (bufIndex >= bufLen) continue;
          c = buf[bufIndex++];
          switch (c)
          {
            case 0x20: w = (WORD)0x02DD; break;
            case 0x4F: w = (WORD)0x0150; break;
            case 0x55: w = (WORD)0x0170; break;
            case 0x6F: w = (WORD)0x0151; break;
            case 0x75: w = (WORD)0x0171; break;
            default:   w = (WORD)c;      break; // unknown character --> fallback
          }
          break;
        //multibyte region CE
        case 0xCE:
          if (bufIndex >= bufLen) continue;
          c = buf[bufIndex++];
          switch (c)
          {
            case 0x20: w = (WORD)0x02DB; break;
            case 0x41: w = (WORD)0x0104; break;
            case 0x45: w = (WORD)0x0118; break;
            case 0x49: w = (WORD)0x012E; break;
            case 0x55: w = (WORD)0x0172; break;
            case 0x61: w = (WORD)0x0105; break;
            case 0x65: w = (WORD)0x0119; break;
            case 0x69: w = (WORD)0x012F; break;
            case 0x75: w = (WORD)0x0173; break;
            default:   w = (WORD)c;      break; // unknown character --> fallback
          }
          break;
        //multibyte region CF
        case 0xCF:
          if (bufIndex >= bufLen) continue;
          c = buf[bufIndex++];
          switch (c)
          {
            case 0x20: w = (WORD)0x02C7; break;
            case 0x43: w = (WORD)0x010C; break;
            case 0x44: w = (WORD)0x010E; break;
            case 0x45: w = (WORD)0x011A; break;
            case 0x4C: w = (WORD)0x013D; break;
            case 0x4E: w = (WORD)0x0147; break;
            case 0x52: w = (WORD)0x0158; break;
            case 0x53: w = (WORD)0x0160; break;
            case 0x54: w = (WORD)0x0164; break;
            case 0x5A: w = (WORD)0x017D; break;
            case 0x63: w = (WORD)0x010D; break;
            case 0x64: w = (WORD)0x010F; break;
            case 0x65: w = (WORD)0x011B; break;
            case 0x6C: w = (WORD)0x013E; break;
            case 0x6E: w = (WORD)0x0148; break;
            case 0x72: w = (WORD)0x0159; break;
            case 0x73: w = (WORD)0x0161; break;
            case 0x74: w = (WORD)0x0165; break;
            case 0x7A: w = (WORD)0x017E; break;
            default:   w = (WORD)c;      break; // unknown character --> fallback
          }
          break;
        // rest is the same
        default: w = (WORD)c; break;
      }
      
      //Convert to UTF-8
      if (w < 0x80)
        cl = 1;
      else if (w < 0x800)
        cl = 2;
      else
        cl = 3;
      if (textIndex + cl >= textLen)
        break;
      if (w < 0x80)
      {
        text[textIndex++] = (char)w;
      }
      else if (w < 0x800)
      {
        text[textIndex++] = (char)((w >> 6) | 0xC0);
        text[textIndex++] = (char)((w & 0x3F) | 0x80);
      }
      else
      {
        text[textIndex++] = (char)((w >> 12) | 0xE0);
        text[textIndex++] = (char)(((w >> 6) & 0x3F) | 0x80);
        text[textIndex++] = (char)((w & 0x3F) | 0x80);
      }
    }
  }
  else if (c == 0x11) //ISO/IEC 10646 encoding
  {
    // process 2 byte unicode characters by reencoding it 
    // to UTF-8 to avoid zero bytes inside string
    WORD w;
    text[textIndex++] = 0x15;
    text[textIndex] = 0;
    while (bufIndex + 1 < bufLen)
    {
      w = (buf[bufIndex++] << 8);
      w |= buf[bufIndex++];
      if (w == 0xE08A) //CR/LF
        w = '\r';
      else if ((w <= 0x1F) || ((w >= 0xE07F) && (w <= 0xE09F))) //Ignore unsupported characters
        w = 0;
      if (w != 0)
      {
        if (w < 0x80)
          c = 1;
        else if (w < 0x800)
          c = 2;
        else
          c = 3;
        if (textIndex + c >= textLen)
          break;
        if (w < 0x80)
          text[textIndex++] = (char)w;
        else if (w < 0x800)
        {
          text[textIndex++] = (char)((w >> 6) | 0xC0);
          text[textIndex++] = (char)((w & 0x3F) | 0x80);
        }
        else
        {
          text[textIndex++] = (char)((w >> 12) | 0xE0);
          text[textIndex++] = (char)(((w >> 6) & 0x3F) | 0x80);
          text[textIndex++] = (char)((w & 0x3F) | 0x80);
        }
      }
    }
  }
  else if (c == 0x15) //UTF-8 encoding
  {
    // Copy first byte
    text[textIndex++] = c;
    text[textIndex] = 0;
    
    //Process remaining bytes
    while ((bufIndex < bufLen) && (textIndex < textLen))
    {
      c = buf[bufIndex++];
      if (c != 0)
        text[textIndex++] = c;
    }
  }
  else //All other encodings
  {
    // Deal with first byte - check for character coding info
    if (c == 0x10) // three byte encoding
    {      
      if ((textLen >= 3) && (buf[2] >= 0x1) && (buf[2] <= 0xF))
      {
        text[textIndex++] = c;
        text[textIndex++] = 0x20; //Make 2nd output byte non-zero
        text[textIndex++] = buf[2];
        text[textIndex] = 0;
        bufIndex += 2;
      }
      else
      {
        text[textIndex] = 0;
        return;
      }
    }
    else //Single-byte encoding
    {
      if ((c < 0x7F) || (c > 0x9F)) //Only allow used/supported values
      {
        text[textIndex++] = c; //Copy first byte (may be coding selector byte, or first character if default coding table used)
      }
      else if (c == 0x8A) //CR/LF
      {
        text[textIndex++] = '\r';
      }
      text[textIndex] = 0;
    }
    
    //Process remaining bytes
    while ((bufIndex < bufLen) && (textIndex < textLen))
    {
      c = buf[bufIndex++];
      if (c == 0x8A) //CR/LF
      {
        c = '\r';
      }
      else if ((c <= 0x1F) || ((c >= 0x7F) && (c <= 0x9F))) //Ignore unsupported characters
      {
        c = 0; // ignore
      }

      if (c != 0)
        text[textIndex++] = c;
    }
  }
  text[textIndex] = 0;
}

string CTextUtil::FreesatHuffmanToString(BYTE *src, int size)
{
  string uncompressed;
  int j,k;
  unsigned char *data;
  int uncompressed_size = 0x102;
  int u;
  int bit;
  short offset;
  unsigned short *base;
  unsigned char *next_node;
  unsigned char node;
  unsigned char prevc;
  unsigned char nextc;

  if (src[1] == 1 || src[1] == 2) 
  {
    uncompressed.append(1,0x15); //Add UTF-8 encoding selector byte to start of output string
    if (src[1] == 1) 
    {
      data = raw_huffman_data1;
    }
    else 
    {
      data = raw_huffman_data2;
    }
    src += 2;
    j = 0;
    u = 0;
    prevc = START;
    do
    {
      offset = bitrev16(((unsigned short *)data)[prevc]);
      base = (unsigned short *)&data[offset];
      node = 0;
      do
      {
        bit = (src[j>>3] >> (7-(j&7))) & 1;
        j++;
        next_node = (unsigned char *)&base[node];
        node = next_node[bit];
      }
      while ((next_node[bit] & 0x80) == 0);
      nextc = next_node[bit] ^ 0x80;
      if (nextc == 0x1b)
      {
        do
        {
          nextc = 0;
          for (k=0; k<8; k++)
          {
            bit = (src[j>>3] >> (7-(j&7))) & 1;
            nextc = (nextc <<1) | bit;
            j++;
          }
          if (u >= uncompressed_size)
          {
            return 0;
          }
          uncompressed.append(1,nextc);
        }
        while (nextc & 0x80);
      }
      else
      {
        if (u >= uncompressed_size)
        {
          LogDebug("need realloc, uncompressed_size=%d", uncompressed_size);
          return uncompressed;
        }
        uncompressed.append(1,nextc);
      }
      prevc = nextc;
    }
    while(nextc != STOP);
    prevc = nextc;
    uncompressed.append(1,'\0');
    return uncompressed;
  }
  else
  {
    LogDebug("bad huffman table, %d, only support for 1, 2", src[0]);
    return uncompressed;
  }
  return uncompressed;
}

constexpr char hexmap[] = {'0', '1', '2', '3', '4', '5', '6', '7',
                           '8', '9', 'a', 'b', 'c', 'd', 'e', 'f'};

string CTextUtil::hexStr(const string& in)
{
  string out;
  if (in.c_str() == NULL)
      return out;
  const char* ch = in.c_str();
  int len = in.length();  
  for (int i = 0; i < len; ++i)
  {    
    out.append(1, hexmap[(*ch & 0xF0) >> 4]);
    out.append(1, hexmap[*ch++ & 0x0F]);
  }
  out.append(1, '\0'); //Add null to end of string  
  return out;
}

//This function is derived from the example code here - 
//https://stackoverflow.com/questions/23689733/convert-string-from-utf-8-to-iso-8859-1
string CTextUtil::UTF8toISO8859_1(const string& in)
{
  string out;
  if (in.c_str() == NULL)
      return out;
  
  unsigned int codepoint;
  const char* ch = in.c_str();
  while (*ch != '\0')
  {
    if (*ch <= 0x7f)
      codepoint = *ch;
    else if (*ch <= 0xbf)
      codepoint = (codepoint << 6) | (*ch & 0x3f);
    else if (*ch <= 0xdf)
      codepoint = *ch & 0x1f;
    else if (*ch <= 0xef)
      codepoint = *ch & 0x0f;
    else
      codepoint = *ch & 0x07;
    ++ch;
    if (((*ch & 0xc0) != 0x80) && (codepoint <= 0x10ffff))
    {
      if (codepoint <= 255)
      {
        out.append(1, static_cast<char>(codepoint));
      }
      else
      {
        // out-of-bounds characters
        out.append(1, ' '); //Insert space
      }
    }
  }
  out.append(1, '\0'); //Add null to end of string
  return out;
}

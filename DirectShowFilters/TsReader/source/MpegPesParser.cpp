/* 
 *	Copyright (C) 2006 Team MediaPortal
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

#pragma warning(disable:4996)
#pragma warning(disable:4995)
#include <windows.h>
#include <commdlg.h>
#include <bdatypes.h>
#include <time.h>
#include <streams.h>
#include <initguid.h>

#include "MpegPesParser.h"
#include "..\..\shared\tsheader.h"
extern void LogDebug(const char *fmt, ...) ;

const byte PES_VIDEO = 0xe0;
const byte PES_AUDIO_MPEG = 0xc0;
const byte PES_PRIVATE1 = 0xbd;
const byte PES_PADDING = 0xbe;
const byte PES_PRIVATE2 = 0xbf;
const byte PES_VIDEO_VC1 = 0xfd;
const byte PES_PRIVATE_SUBTITLE = 0x20;
const byte PES_PRIVATE_AC3 = 0x80;
const byte PES_PRIVATE_AC3_PLUS = 0xc0;
const byte PES_PRIVATE_DTS_HD = 0x88;
const byte PES_PRIVATE_LPCM = 0xa0;
const byte PES_PRIVATE_AC3_TRUE_HD = 0xb0;

const byte MPEG2_SEQ_CODE = 0xb3;
const byte MPEG2_SEQ_EXT = 0xb5;
const int H264_PREFIX = 0x00000107;

bool CMpegPesParser::SequenceFound(byte *tsPacket, int offset, byte marker)
{
	// does the offset+header exceed our tspacket
	if (offset+4>188)
		return false;
	return (tsPacket[offset] == 0 && tsPacket[offset + 1] == 0 && tsPacket[offset + 2] == 1 && tsPacket[offset + 3] == marker);
}

int CMpegPesParser::SearchSequence(byte* tsPacket,int offset,byte marker)
{
	for (int i=offset;i<183;i++)
	{
		if (SequenceFound(tsPacket,i,marker))
			return i;
	}
	return -1;
}

void CMpegPesParser::SetAspectRatio(int aspectRatioIndex,MPEG2VIDEOINFO &mpeg2VideoInfo)
{
	switch (aspectRatioIndex)
	{
	case 1:
		mpeg2VideoInfo.hdr.dwPictAspectRatioX=1;
		mpeg2VideoInfo.hdr.dwPictAspectRatioY=1;
		break;
	case 2:
		mpeg2VideoInfo.hdr.dwPictAspectRatioX=4;
		mpeg2VideoInfo.hdr.dwPictAspectRatioY=3;
		break;
	case 3:
		mpeg2VideoInfo.hdr.dwPictAspectRatioX=16;
		mpeg2VideoInfo.hdr.dwPictAspectRatioY=9;
		break;
	case 4:
		mpeg2VideoInfo.hdr.dwPictAspectRatioX=10;
		mpeg2VideoInfo.hdr.dwPictAspectRatioY=9;
		break;
	default:
		mpeg2VideoInfo.hdr.dwPictAspectRatioX=0;
		mpeg2VideoInfo.hdr.dwPictAspectRatioY=0;
		break;
	}
}

void CMpegPesParser::ParseVideoExtensionHeader(byte* tsPacket,int offset,MPEG2VIDEOINFO &mpeg2VideoInfo)
{
	int extension_start_code_identifier=(tsPacket[offset]>>4);
	switch (extension_start_code_identifier)
	{
		case 0: //reserved
			break;
		case 1:
			//ParseSequenceExtension(tsPacket,offset,mpeg2VideoInfo);
			break;
		case 2:
			//ParseDisplayExtension(tsPacket,offset,mpeg2VideoInfo);
			break;
		case 3:
			//ParseQuantMatrixExtension(tsPacket,offset,mpeg2VideoInfo);
			break;
		case 4: // Copyright extension
			mpeg2VideoInfo.hdr.dwCopyProtectFlags=(tsPacket[offset] & 0x10);
			break;
		case 5:
			//ParseScalableExtension(tsPacket,offset,mpeg2VideoInfo);
			break;
		case 6: //reserved
			break;
		case 7:
			//ParsePictureDisplayExtension(tsPacket,offset,mpeg2VideoInfo);
			break;
		case 8:
			if ((tsPacket[offset+4] & 1)==1) // progressive flag
				mpeg2VideoInfo.hdr.dwInterlaceFlags=AMINTERLACE_FieldPatBothRegular;
			else
				mpeg2VideoInfo.hdr.dwInterlaceFlags=AMINTERLACE_IsInterlaced | AMINTERLACE_DisplayModeBobOrWeave;
			break;
		case 9:
			//ParsePictureSpatialScalableExtension(tsPacket,offset,mpeg2VideoInfo);
			break;
		case 10:
			//ParsePictureTemporalScalableExtension(tsPacket,offset,mpeg2VideoInfo);
			break;
	}
}

void CMpegPesParser::ParseMpeg2Video(byte* tsPacket,int offset,MPEG2VIDEOINFO &mpeg2VideoInfo)
{
	//LogDebug("Found MPEG2 VIDEO");
	int width=((tsPacket[offset]<<8) + tsPacket[offset + 1]) >> 4;
	int height=((tsPacket[offset+1]<<8)+tsPacket[offset+2])& 0x0FFF;
	if (width<100 || height<100)
		return;
	offset+=3;
	int frameRateIndex = tsPacket[offset] & 0x0F;
  int aspectRatioIndex = (tsPacket[offset] & 0xF0) >> 4;
	offset++;
  int bitRate = ((tsPacket[offset])<<8)+tsPacket[offset+1];
  bitRate <<= 2;
  byte lastTwo = tsPacket[offset + 2];
  lastTwo >>= 6;
  bitRate |= lastTwo;
  bitRate *= 400;

	mpeg2VideoInfo.hdr.rcSource.right=width;
	mpeg2VideoInfo.hdr.rcSource.bottom=height;

	mpeg2VideoInfo.hdr.dwBitRate=bitRate;
	SetAspectRatio(aspectRatioIndex,mpeg2VideoInfo);
	mpeg2VideoInfo.hdr.dwReserved2=0;

	mpeg2VideoInfo.hdr.bmiHeader.biSize=40;
	mpeg2VideoInfo.hdr.bmiHeader.biWidth=mpeg2VideoInfo.hdr.rcSource.right;
	mpeg2VideoInfo.hdr.bmiHeader.biHeight=mpeg2VideoInfo.hdr.rcSource.bottom;
	mpeg2VideoInfo.hdr.bmiHeader.biPlanes=1;

	// search for extension data
	//LogDebug("Mpeg2VideoInfo set. Now searchning for extension headers...");
  offset=SearchSequence(tsPacket,offset,MPEG2_SEQ_EXT);
	while (offset!=-1)
	{
		offset+=4;
		//LogDebug("Found extension header with id %d",tsPacket[offset]>>4);
		ParseVideoExtensionHeader(tsPacket,offset,mpeg2VideoInfo);
		offset=SearchSequence(tsPacket,offset,MPEG2_SEQ_CODE);
	}
	mpeg2VideoInfo.hdr.rcTarget.left=0;
	//LogDebug("res=%dx%d aspectRatio=%d:%d bitrate=%d isInterlaced=%d",mpeg2VideoInfo.hdr.rcSource.right,mpeg2VideoInfo.hdr.rcSource.bottom,mpeg2VideoInfo.hdr.dwPictAspectRatioX,mpeg2VideoInfo.hdr.dwPictAspectRatioY,mpeg2VideoInfo.hdr.dwBitRate,(mpeg2VideoInfo.hdr.dwInterlaceFlags & AMINTERLACE_IsInterlaced==AMINTERLACE_IsInterlaced));
}

byte GetNextBit(byte* tsPacket,int &bit)
{
	if (bit>(187*8))
	{
		bit=0;
		return 0;
	}
	byte ret = (byte)(((tsPacket[bit / 8]) >> (7 - (bit % 8))) & 1);
  bit++;
	if (bit>(187*8))
	{
		bit=0;
		return 0;
	}
  return ret;
}

int GetNextExpGolomb(byte *tsPacket,int &curBitPos)
{
	// Header exceeds current packet
	if (curBitPos==0)
		return 0;
	int leadingZeroBits = -1;
  byte b = 0;
  for (; b == 0; leadingZeroBits++)
	{
		b = GetNextBit(tsPacket,curBitPos);
		// Header exceeds current packet
		if (curBitPos==0)
			return 0;
	}
  int codeNum = int(1 << leadingZeroBits);
  codeNum -= 1;
  int part2 = 0;
  for (; leadingZeroBits > 0; leadingZeroBits-- )
  {
		b = GetNextBit(tsPacket,curBitPos);
		// Header exceeds current packet
		if (curBitPos==0)
			return 0;
    part2 = part2 << 1;
    part2 |= b;
  }
  codeNum += part2;
  return codeNum;
}

void CMpegPesParser::ParseH264Video(byte* tsPacket,int offset,MPEG2VIDEOINFO &mpeg2VideoInfo)
{
	//LogDebug("FOUND H264 VIDEO");
	// get the width first
	int curBitPos=(offset*8)+24;
	GetNextExpGolomb(tsPacket,curBitPos);
	GetNextExpGolomb(tsPacket,curBitPos);
  int pic = GetNextExpGolomb(tsPacket,curBitPos);
  if (pic == 0)
		GetNextExpGolomb(tsPacket,curBitPos);     
  else if (pic == 1)
  {
		GetNextBit(tsPacket,curBitPos);
    GetNextExpGolomb(tsPacket,curBitPos);
    GetNextExpGolomb(tsPacket,curBitPos);
    int numFrame = GetNextExpGolomb(tsPacket,curBitPos);
    for (int i = 0; i < numFrame; i++)
			GetNextExpGolomb(tsPacket,curBitPos);
	}
  GetNextExpGolomb(tsPacket,curBitPos);
  GetNextBit(tsPacket,curBitPos);
	// Header did not fit in packet
	if (curBitPos==0)
		return;
	int width = GetNextExpGolomb(tsPacket,curBitPos);
  width++;
  width <<= 4;
  int height = GetNextExpGolomb(tsPacket,curBitPos);
  height++;
  height <<= 4;
	if (width<100 || height<100)
		return;
	mpeg2VideoInfo.hdr.rcSource.right=width;
	mpeg2VideoInfo.hdr.rcSource.bottom=height;
	//set to progressive as default
	mpeg2VideoInfo.hdr.dwInterlaceFlags=AMINTERLACE_FieldPatBothRegular;
	mpeg2VideoInfo.hdr.dwPictAspectRatioX=16;
	mpeg2VideoInfo.hdr.dwPictAspectRatioY=9;
	mpeg2VideoInfo.hdr.bmiHeader.biSize=40;
	mpeg2VideoInfo.hdr.bmiHeader.biWidth=mpeg2VideoInfo.hdr.rcSource.right;
	mpeg2VideoInfo.hdr.bmiHeader.biHeight=mpeg2VideoInfo.hdr.rcSource.bottom;
	mpeg2VideoInfo.hdr.bmiHeader.biPlanes=1;
	// we can only guess the interlaced flag and the ar since they are not directly given
	if (height == 540 || height == 544 || height==288 || height==240)
	{
		mpeg2VideoInfo.hdr.dwInterlaceFlags=AMINTERLACE_IsInterlaced | AMINTERLACE_DisplayModeBobOrWeave;
		if (height==288 || height==240)
		{
			mpeg2VideoInfo.hdr.dwPictAspectRatioX=4;
			mpeg2VideoInfo.hdr.dwPictAspectRatioY=3;
		}
	}
	mpeg2VideoInfo.hdr.dwBitRate=15000000;
	mpeg2VideoInfo.hdr.dwReserved2=0;
	mpeg2VideoInfo.hdr.rcTarget.left=0;
}

void CMpegPesParser::ParseVideo(byte* tsPacket,int offset,MPEG2VIDEOINFO &mpeg2VideoInfo)
{
	//LogDebug("Found VIDEO");
	int off=SearchSequence(tsPacket,offset,MPEG2_SEQ_CODE);
	if (off!=-1)
		ParseMpeg2Video(tsPacket,off+4,mpeg2VideoInfo);
	          
	int marker = 0xffffffff;
  for (; offset < 188; offset++)
  {
		marker = marker << 8;
    marker &= 0xffffff00;
    marker += tsPacket[offset];
    if ((marker & 0xffffff9f) == H264_PREFIX)
    {
			break;
		}
  }
	if (offset<185)
		ParseH264Video(tsPacket,offset+1,mpeg2VideoInfo);
}

void CMpegPesParser::OnTsPacket(byte *tsPacket, CTsHeader header, MPEG2VIDEOINFO &mpeg2VideoInfo)
{
	if (!header.HasPayload || !header.PayloadUnitStart)
		return;
	int offset=header.PayLoadStart;
	// Is an mpeg header following?
	if (tsPacket[offset]!=0 || tsPacket[offset+1]!=0 || tsPacket[offset+2]!=1)
		return;
	
	int marker=tsPacket[offset+3];
	offset+=4;

	switch (marker)
	{
		case PES_VIDEO:
			ParseVideo(tsPacket,offset,mpeg2VideoInfo);
			break;
	}
}

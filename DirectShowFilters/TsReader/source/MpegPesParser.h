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
#pragma once
#include "buffer.h"
#include "MultiFileReader.h"
#include "tsDuration.h"
#include "pcrdecoder.h"
#include "..\..\shared\packetSync.h"
#include "pidtable.h"
#include "..\..\shared\tsheader.h"
#include "patparser.h"
#include "channelInfo.h"
#include "PidTable.h"
#include "TSThread.h"
#include "..\..\shared\Pcr.h"
#include <vector>
#include <map>
#include <dvdmedia.h>

class CMpegPesParser
{
private:
	bool SequenceFound(byte* tsPacket,int offset,byte marker);
	int SearchSequence(byte* tsPacket,int offset,byte marker);

	byte GetNextBit(byte* tsPacket,int &bit) ;
	int GetNextExpGolomb(byte *tsPacket,int &curBitPos) ;
	void CMpegPesParser::ScalingListSkip(byte* tsPacket,int &bit,int skip) ;



	
	void SetAspectRatio(int aspectRatioIndex,MPEG2VIDEOINFO &mpeg2VideoInfo);
	void ParseVideoExtensionHeader(byte* tsPacket,int offset,MPEG2VIDEOINFO &mpeg2VideoInfo);
	bool ParseMpeg2Video(byte* tsPacket,int offset,MPEG2VIDEOINFO &mpeg2VideoInfo);
	bool ParseH264Video(byte* tsPacket,int offset,MPEG2VIDEOINFO &mpeg2VideoInfo);
	bool ParseVideo(byte* tsPacket,int offset,MPEG2VIDEOINFO &mpeg2VideoInfo, int videoServiceType);

	int  m_Length ;
public:
	bool OnTsPacket(byte* Frame,int Length,MPEG2VIDEOINFO &mpeg2VideoInfo, int videoServiceType);
};


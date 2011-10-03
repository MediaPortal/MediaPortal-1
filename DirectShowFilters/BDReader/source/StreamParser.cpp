/*
 *  Copyright (C) 2005-2011 Team MediaPortal
 *  http://www.team-mediaportal.com
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

#include "StdAfx.h"

#include <streams.h>
#include "StreamParser.h"
#include <initguid.h>
#include <bluray.h>

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

extern void LogDebug(const char *fmt, ...) ;

StreamParser::StreamParser()
{
	pmt = CMediaType();
	pmt.InitMediaType();
	pmt.bFixedSizeSamples = false;
	basicVideoInfo = BasicVideoInfo();
}

bool StreamParser::Parse(byte* tsPacket, int serviceType)
{
	bool parsed = false;
	__int64 framesize = hdrParser.GetSize();

	if (serviceType == BLURAY_STREAM_TYPE_VIDEO_MPEG1 ||
      serviceType == BLURAY_STREAM_TYPE_VIDEO_MPEG2)
	{
		seqhdr seq;
		if (hdrParser.Read(seq,framesize,&pmt))
		{
			//hdrParser.DumpSequenceHeader(seq);
			basicVideoInfo.width=seq.width;
			basicVideoInfo.height=seq.height;
			basicVideoInfo.fps=1000 / (seq.ifps /10000);
			basicVideoInfo.arx=seq.arx;
			basicVideoInfo.ary=seq.ary;
			if (seq.progressive==0)
				basicVideoInfo.isInterlaced=1;
			else
				basicVideoInfo.isInterlaced=0;
			basicVideoInfo.streamType=1; //MPEG2
			basicVideoInfo.isValid=true;
			parsed=true;
		}
	}
	else if (serviceType == BLURAY_STREAM_TYPE_VIDEO_H264)
	{
		avchdr avc;
		if (hdrParser.Read(avc,framesize,&pmt))
		{
			//hdrParser.DumpAvcHeader(avc);
			basicVideoInfo.width = avc.width;
			basicVideoInfo.height = avc.height;
			basicVideoInfo.fps = 1000 / (avc.AvgTimePerFrame / 10000);
			//basicVideoInfo.arx=avc.arx;
			//basicVideoInfo.ary=avc.ary;
			if (!avc.progressive)
				basicVideoInfo.isInterlaced = 1;
			else
				basicVideoInfo.isInterlaced = 0;
			basicVideoInfo.streamType = 2; // H264
			basicVideoInfo.isValid = true;
			parsed = true;
		}
	}
	else if (serviceType == BLURAY_STREAM_TYPE_VIDEO_VC1)
	{
		vc1hdr vc1;
		if (hdrParser.Read(vc1,framesize,&pmt))
		{
			//hdrParser.DumpAvcHeader(avc);
			basicVideoInfo.width = vc1.width;
			basicVideoInfo.height = vc1.height;
			//basicVideoInfo.fps=1000 / (vc1.AvgTimePerFrame /10000);

      // TODO 
      basicVideoInfo.fps = 23987;
      
			basicVideoInfo.arx = vc1.height;
			basicVideoInfo.ary = vc1.width;
      if(vc1.interlace > 0 ) // TODO check this
		    basicVideoInfo.isInterlaced = 1;
      else
			  basicVideoInfo.isInterlaced = 0;
			basicVideoInfo.streamType = 3; // VC1? TODO check
			basicVideoInfo.isValid = true;
			parsed = true;
		}
	}
  else if (serviceType == BLURAY_STREAM_TYPE_AUDIO_MPEG1 ||
           serviceType == BLURAY_STREAM_TYPE_AUDIO_MPEG2)
  {
		mpahdr mpa;
		parsed = hdrParser.Read(mpa, framesize, &pmt);
  }
  else if (serviceType == BLURAY_STREAM_TYPE_AUDIO_AC3 ||
           serviceType == BLURAY_STREAM_TYPE_AUDIO_AC3PLUS)
  {
    ac3hdr ac3;
		parsed = hdrParser.Read(ac3, framesize, &pmt);
  }
  else if (serviceType == BLURAY_STREAM_TYPE_AUDIO_LPCM)
  {
		bdlpcmhdr lpcm;
		parsed = hdrParser.Read(lpcm, framesize, &pmt);
  }
  else if (serviceType == BLURAY_STREAM_TYPE_AUDIO_DTS ||
           serviceType == BLURAY_STREAM_TYPE_AUDIO_DTSHD ||
           serviceType == BLURAY_STREAM_TYPE_AUDIO_DTSHD_MASTER)
  {
		dtshdr dts;
		parsed = hdrParser.Read(dts, framesize, &pmt);
  }
  else if (serviceType == BLURAY_STREAM_TYPE_AUDIO_TRUHD)
  {
    thdhdr thd;
		parsed = hdrParser.Read(thd, framesize, &pmt);
  }

	return parsed;
}

bool StreamParser::OnTsPacket(byte *Frame, int Length, int serviceType)
{
	hdrParser.Reset(Frame, Length);
	return Parse(Frame, serviceType);
}


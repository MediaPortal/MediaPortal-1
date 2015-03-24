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
#include "StdAfx.h"

#include <winsock2.h>
#include <ws2tcpip.h>

#include <commdlg.h>
#include <bdatypes.h>
#include <time.h>
#include <streams.h>
#include <initguid.h>

#include "MpegPesParser.h"
#include "PmtParser.h"

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

extern void LogDebug(const char *fmt, ...) ;

CMpegPesParser::CMpegPesParser()
{
	pmt=CMediaType();
	pmt.InitMediaType();
	pmt.bFixedSizeSamples=false;
	
	audPmt=CMediaType();
	audPmt.InitMediaType();
	audPmt.bFixedSizeSamples=false;
	
	basicVideoInfo=BasicVideoInfo();
	basicAudioInfo=BasicAudioInfo();
}

bool CMpegPesParser::ParseVideo(byte* tsPacket,bool isMpeg2,bool reset)
{
	bool parsed=false;
  __int64 framesize=hdrParser.GetSize();

	if (isMpeg2)
	{
		seqhdr seq;
		if (hdrParser.Read(seq,framesize,&pmt,reset))
		{
			//hdrParser.DumpSequenceHeader(seq);
			basicVideoInfo.width=seq.width;
			basicVideoInfo.height=seq.height;
			basicVideoInfo.fps=10000000.0 / (double)seq.ifps;
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
	else 
	{
	  // avchdr avc;
		if (hdrParser.Read(avc,framesize,&pmt,reset))
		{
			//hdrParser.DumpAvcHeader(avc);
			basicVideoInfo.width=avc.width;
			basicVideoInfo.height=avc.height;
			basicVideoInfo.fps=10000000.0 / (double)avc.AvgTimePerFrame;
			basicVideoInfo.arx=avc.arx;
			basicVideoInfo.ary=avc.ary;
			if (!avc.progressive)
				basicVideoInfo.isInterlaced=1;
			else
				basicVideoInfo.isInterlaced=0;
			basicVideoInfo.streamType=2; // H264
			basicVideoInfo.isValid=true;
			parsed=true;
		}
	}
	return parsed;
}

bool CMpegPesParser::OnTsPacket(byte *Frame,int Length,bool isMpeg2,bool reset)
{
	//LogDebug("Framesize: %i",Length);
	//if (Length<=100) return false ; // arbitrary for safety.
  CAutoLock lock (&m_sectionVideoPmt);
	hdrParser.Reset(Frame,Length);
	return ParseVideo(Frame,isMpeg2,reset);
}

void CMpegPesParser::VideoReset()
{
  CAutoLock lock (&m_sectionVideoPmt);

	basicVideoInfo.width=0;
	basicVideoInfo.height=0;
	basicVideoInfo.fps=0;
	basicVideoInfo.arx=0;
	basicVideoInfo.ary=0;
	basicVideoInfo.isInterlaced=0;
	basicVideoInfo.streamType=0;
	basicVideoInfo.isValid=false;
}

void CMpegPesParser::VideoValidReset()
{
  CAutoLock lock (&m_sectionVideoPmt);
	basicVideoInfo.isValid=false;	
}

bool CMpegPesParser::ParseAudio(byte* audioPacket, int streamType, bool reset)
{
	bool parsed=false;
 
  if (audioPacket!=NULL)
  {
    switch (streamType)
    {
      case SERVICE_TYPE_AUDIO_MPEG1:
      case SERVICE_TYPE_AUDIO_MPEG2:
      {
        mpahdr mpa;
    	  __int64 framesize=hdrParser.GetSize();
      	if (hdrParser.Read(mpa,framesize,false,&audPmt)) //Don't allow v2.5
      	{
          basicAudioInfo.sampleRate=mpa.nSamplesPerSec;
          basicAudioInfo.channels = (mpa.channels == 3) ? 1 : 2;
          basicAudioInfo.aacObjectType=0;
          basicAudioInfo.streamType = streamType;
          basicAudioInfo.bitrate = mpa.nBytesPerSec*8;
      	  basicAudioInfo.pmtValid = true;	
          basicAudioInfo.isValid = true;
      	  parsed=true;
      	}
      }
      break;
      case SERVICE_TYPE_AUDIO_AAC:
      {
        aachdr aac;
    	  __int64 framesize=hdrParser.GetSize();
      	if (hdrParser.Read(aac,framesize,&audPmt))
      	{
          basicAudioInfo.sampleRate=aac.nSamplesPerSec;
          basicAudioInfo.channels=aac.channels;    
          basicAudioInfo.aacObjectType=aac.profile+1;
          basicAudioInfo.streamType = streamType;
          basicAudioInfo.bitrate = aac.nBytesPerSec*8;
      	  basicAudioInfo.pmtValid = true;	
          basicAudioInfo.isValid = true;
      	  parsed=true;
      	}
      }
      break;
      case SERVICE_TYPE_AUDIO_AC3:
      {
        ac3hdr ac3;
    	  __int64 framesize=hdrParser.GetSize();
      	if (hdrParser.Read(ac3,framesize,&audPmt))
      	{
        	basicAudioInfo.sampleRate = ac3.nSamplesPerSec;        
	        basicAudioInfo.channels = ac3.nChannels;
          basicAudioInfo.aacObjectType=0;
          basicAudioInfo.streamType = streamType;
          basicAudioInfo.bitrate = ac3.nBytesPerSec*8;
      	  basicAudioInfo.pmtValid = true;	
          basicAudioInfo.isValid = true;
      	  parsed=true;
      	}
      }
      break;
      case SERVICE_TYPE_AUDIO_DD_PLUS:
      case SERVICE_TYPE_AUDIO_E_AC3:
      {
        eac3hdr eac3;
    	  __int64 framesize=hdrParser.GetSize();
      	if (hdrParser.Read(eac3,framesize,&audPmt))
      	{
        	basicAudioInfo.sampleRate = eac3.nSamplesPerSec;        
	        basicAudioInfo.channels = eac3.nChannels;
          basicAudioInfo.aacObjectType=0;
          basicAudioInfo.streamType = streamType;
          basicAudioInfo.bitrate = eac3.nBytesPerSec*8;
      	  basicAudioInfo.pmtValid = true;	
          basicAudioInfo.isValid = true;
          //LogDebug("hdrParser: E-AC3 frmsiz = %d, sampleRate = %d", eac3.frmsiz, basicAudioInfo.sampleRate);
      	  parsed=true;
      	}
      }
      break;
    }
  }

  if (!parsed) //Create default info
  {
  	basicAudioInfo.sampleRate=48000;
  	basicAudioInfo.channels=2;
    basicAudioInfo.aacObjectType=0;
  	basicAudioInfo.streamType = streamType;
    basicAudioInfo.bitrate=0;
  	basicAudioInfo.pmtValid=false;	
  	basicAudioInfo.isValid=true;	
  	
  	if (streamType == SERVICE_TYPE_AUDIO_LATM_AAC)
  	{
      basicAudioInfo.aacObjectType=2; //AAC-LC
  	}  	
  	
  	if (streamType == SERVICE_TYPE_AUDIO_AC3 ||
  	    streamType == SERVICE_TYPE_AUDIO_DD_PLUS ||
  	    streamType == SERVICE_TYPE_AUDIO_E_AC3)
  	{
      basicAudioInfo.channels=6;
  	}  	
  }
	
	return parsed;
}

bool CMpegPesParser::OnAudioPacket(byte *Frame, int Length, int streamType, unsigned int streamIndex, bool reset)
{
  CAutoLock lock (&m_sectionAudioPmt);
  if (Frame != NULL)
  {
	  hdrParser.Reset(Frame,Length);
  }
  basicAudioInfo.streamIndex = streamIndex;
	return ParseAudio(Frame, streamType, reset);
}

void CMpegPesParser::AudioReset()
{
  CAutoLock lock (&m_sectionAudioPmt);

	basicAudioInfo.isValid=false;	
	basicAudioInfo.sampleRate=0;
	basicAudioInfo.channels=0;
  basicAudioInfo.aacObjectType=0;
	basicAudioInfo.streamType = SERVICE_TYPE_AUDIO_UNKNOWN;
  basicAudioInfo.streamIndex=0;
  basicAudioInfo.bitrate=0;
	basicAudioInfo.pmtValid=false;	
}

void CMpegPesParser::AudioValidReset()
{
  CAutoLock lock (&m_sectionAudioPmt);
	basicAudioInfo.isValid=false;	
}

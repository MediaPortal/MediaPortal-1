/**********
This library is free software; you can redistribute it and/or modify it under
the terms of the GNU Lesser General Public License as published by the
Free Software Foundation; either version 2.1 of the License, or (at your
option) any later version. (See <http://www.gnu.org/copyleft/lesser.html>.)

This library is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public License for
more details.

You should have received a copy of the GNU Lesser General Public License
along with this library; if not, write to the Free Software Foundation, Inc.,
59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
**********/
// "liveMedia"
// Copyright (c) 1996-2006 Live Networks, Inc.  All rights reserved.
// A 'ServerMediaSubsession' object that creates new, unicast, "RTPSink"s
// on demand, from a MPEG-2 Transport Stream file.
// Implementation

#include "MPEG2TransportFileServerMediaSubsession.hh"
#include "SimpleRTPSink.hh"
#include "ByteStreamFileSource.hh"
#include "MPEG2TransportStreamFramer.hh"
#include <streams.h>
#include "../TsFileDuration.h"

extern void Log(const char *fmt, ...) ;

MPEG2TransportFileServerMediaSubsession*
MPEG2TransportFileServerMediaSubsession::createNew(UsageEnvironment& env,
						   char const* fileName,
						   Boolean reuseFirstSource) {
  return new MPEG2TransportFileServerMediaSubsession(env, fileName, reuseFirstSource);
}

MPEG2TransportFileServerMediaSubsession
::MPEG2TransportFileServerMediaSubsession(UsageEnvironment& env,
                                      char const* fileName, Boolean reuseFirstSource)
  : FileServerMediaSubsession(env, fileName, reuseFirstSource) 
{
  Log("MPEG2TransportFileServerMediaSubsession:ctor");
}

MPEG2TransportFileServerMediaSubsession
::~MPEG2TransportFileServerMediaSubsession() 
{
  Log("MPEG2TransportFileServerMediaSubsession:dtor");
}

#define TRANSPORT_PACKET_SIZE 188
#define TRANSPORT_PACKETS_PER_NETWORK_PACKET 7
// The product of these two numbers must be enough to fit within a network packet

FramedSource* MPEG2TransportFileServerMediaSubsession
::createNewStreamSource(unsigned /*clientSessionId*/, unsigned& estBitrate) {
  estBitrate = 5000; // kbps, estimate

  // Create the video source:
  unsigned const inputDataChunkSize
    = TRANSPORT_PACKETS_PER_NETWORK_PACKET*TRANSPORT_PACKET_SIZE;
  //ByteStreamFileSource* fileSource
  //  = ByteStreamFileSource::createNew(envir(), fFileName, inputDataChunkSize);
  TsStreamFileSource* fileSource = TsStreamFileSource::createNew(envir(), fFileName, inputDataChunkSize);
  if (fileSource == NULL) return NULL;
  fFileSize = fileSource->fileSize();

  strcpy(m_fileName,fFileName);
  // Create a framer for the Transport Stream:
  return MPEG2TransportStreamFramer::createNew(envir(), fileSource);
}

RTPSink* MPEG2TransportFileServerMediaSubsession
::createNewRTPSink(Groupsock* rtpGroupsock,
		   unsigned char /*rtpPayloadTypeIfDynamic*/,
		   FramedSource* /*inputSource*/) {
  return SimpleRTPSink::createNew(envir(), rtpGroupsock,
				  33, 90000, "video", "mp2t",
				  1, True, False /*no 'M' bit*/);
}

float MPEG2TransportFileServerMediaSubsession::duration() const
{
  CTsFileDuration duration;
  duration.SetFileName((char*)m_fileName);
  duration.OpenFile();
  duration.UpdateDuration();
  duration.CloseFile();
  return duration.Duration();
}

void MPEG2TransportFileServerMediaSubsession::seekStreamSource(FramedSource* inputSource, float seekNPT)
{
  MPEG2TransportStreamFramer* framer=(MPEG2TransportStreamFramer*)inputSource;
  TsStreamFileSource* source=(TsStreamFileSource*)framer->inputSource();
  if (seekNPT==0.0f)
  {
    source->seekToByteAbsolute(0LL);
    return;
  }
  float fileDuration=duration();
  if (seekNPT<0) seekNPT=0;
  if (seekNPT>(fileDuration-0.5f)) seekNPT=(fileDuration-0.5f);
  if (seekNPT <0) seekNPT=0;
  float pos=seekNPT / fileDuration;
  __int64 fileSize=source->fileSize();
  pos*=fileSize;
  pos/=188;
  pos*=188;
  __int64 newPos=(__int64) pos;

  source->seekToByteAbsolute(newPos);
	Log("ts seekStreamSource %f / %f ->%d", seekNPT,fileDuration, (DWORD)newPos);
}
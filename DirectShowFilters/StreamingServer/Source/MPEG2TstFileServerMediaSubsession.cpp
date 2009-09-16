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
// Copyright (c) 1996-2007 Live Networks, Inc.  All rights reserved.
// A 'ServerMediaSubsession' object that creates new, unicast, "RTPSink"s
// on demand, from a MPEG-2 Transport Stream file.
// Implementation
#include <streams.h>
#include "MPEG2TstFileServerMediaSubsession.h"
extern void LogDebug(const char *fmt, ...) ;

MPEG2TstFileServerMediaSubsession* MPEG2TstFileServerMediaSubsession::createNew(UsageEnvironment& env,char const* fileName,Boolean reuseFirstSource) 
{
  return new MPEG2TstFileServerMediaSubsession(env, fileName, reuseFirstSource);
}

MPEG2TstFileServerMediaSubsession ::MPEG2TstFileServerMediaSubsession(UsageEnvironment& env, char const* fileName, Boolean reuseFirstSource)
: FileServerMediaSubsession(env, fileName, reuseFirstSource) 
{
  LogDebug("MPEG2TstFileServerMediaSubsession::ctor:%x",this);
}

void MPEG2TstFileServerMediaSubsession::OnDelete()
{
  LogDebug("MPEG2TstFileServerMediaSubsession::OnDelete:%x",this);
}

MPEG2TstFileServerMediaSubsession::~MPEG2TstFileServerMediaSubsession() 
{
  LogDebug("MPEG2TstFileServerMediaSubsession::dtor:%x",this);
}

#define TRANSPORT_PACKET_SIZE 188
#define TRANSPORT_PACKETS_PER_NETWORK_PACKET 7
// The product of these two numbers must be enough to fit within a network packet

FramedSource* MPEG2TstFileServerMediaSubsession ::createNewStreamSource(unsigned /*clientSessionId*/, unsigned& estBitrate) 
{
  LogDebug("MPEG2TstFileServerMediaSubsession:createNewStreamSource:%x",this);
  estBitrate = 5000; // kbps, estimate

  // Create the video source:
  unsigned const inputDataChunkSize = TRANSPORT_PACKETS_PER_NETWORK_PACKET*TRANSPORT_PACKET_SIZE;
  TsStreamFileSource* fileSource = TsStreamFileSource::createNew(envir(), fFileName, inputDataChunkSize);
  if (fileSource == NULL) return NULL;
  fFileSize = fileSource->fileSize();
  strcpy(m_fileName,fFileName);

  // Create a MPEG demultiplexor that reads from that source.
  MPEG1or2Demux* demux = MPEG1or2Demux::createNew(envir(), fileSource,true);
  // Create, from this, a source that returns raw PES packets:
  MPEG1or2DemuxedElementaryStream* pesSource = demux->newRawPESStream();
  
  // And, from this, a filter that converts to MPEG-2 Transport Stream frames:
  MPMPEG2TransportStreamFromPESSource* tsSource= MPMPEG2TransportStreamFromPESSource::createNew(envir(), pesSource);

  // Create a framer for the Transport Stream:
  MPEG2TransportStreamFramer* framer= MPEG2TransportStreamFramer::createNew(envir(), tsSource);
  
  return framer;
}

RTPSink* MPEG2TstFileServerMediaSubsession ::createNewRTPSink(Groupsock* rtpGroupsock, unsigned char /*rtpPayloadTypeIfDynamic*/, FramedSource* /*inputSource*/) 
{
  return SimpleRTPSink::createNew(envir(), rtpGroupsock,
				  33, 90000, "video", "mp2t",
				  1, True, False /*no 'M' bit*/);
}

void MPEG2TstFileServerMediaSubsession::seekStreamSource(FramedSource* inputSource, double seekNPT)
{
  MPEG2TransportStreamFramer* framer=(MPEG2TransportStreamFramer*)inputSource;
  MPMPEG2TransportStreamFromPESSource*  tsSource=(MPMPEG2TransportStreamFromPESSource*)framer->inputSource();
  MPEG1or2DemuxedElementaryStream* demuxStream= (MPEG1or2DemuxedElementaryStream*)tsSource->InputSource();
  MPEG1or2Demux& demuxer= demuxStream->sourceDemux();
  TsStreamFileSource* source=(TsStreamFileSource*)demuxer.inputSource();
  if (seekNPT==0.0f)
  {
    source->seekToByteAbsolute(0LL);
    return;
  }
  float fileDuration=duration();
  if (seekNPT<0) seekNPT=0;
  if (seekNPT>(fileDuration-0.5f)) seekNPT=(fileDuration-0.5f);
  if (seekNPT <0) seekNPT=0;
  double pos=seekNPT / fileDuration;
  __int64 fileSize=source->fileSize();
  pos*=fileSize;
  pos/=188;
  pos*=188;
  __int64 newPos=(__int64) pos;

  source->seekToByteAbsolute(newPos);
	LogDebug("ts seekStreamSource %f / %f ->%d", seekNPT,fileDuration, (DWORD)newPos);
  
}
float MPEG2TstFileServerMediaSubsession::duration() const
{
  CTsFileDuration duration;
  duration.SetFileName((char*)fFileName);
  duration.OpenFile();
  duration.UpdateDuration();
  duration.CloseFile();
  return duration.Duration();

}
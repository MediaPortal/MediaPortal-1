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
#include <streams.h>
#include "MPEG2TstFileServerMediaSubsession.h"
#include "SimpleRTPSink.hh"
#include "ByteStreamFileSource.hh"
#include "MPEG2TransportStreamFramer.hh"
#include "MPEG1or2Demux.hh"
#include "MPEG2TransportStreamFromPESSource.hh"
#include "TsFileDuration.h"
MPEG2TstFileServerMediaSubsession* MPEG2TstFileServerMediaSubsession::createNew(UsageEnvironment& env,char const* fileName,Boolean reuseFirstSource) 
{
  return new MPEG2TstFileServerMediaSubsession(env, fileName, reuseFirstSource);
}

MPEG2TstFileServerMediaSubsession ::MPEG2TstFileServerMediaSubsession(UsageEnvironment& env, char const* fileName, Boolean reuseFirstSource)
: FileServerMediaSubsession(env, fileName, reuseFirstSource) 
{
}

MPEG2TstFileServerMediaSubsession::~MPEG2TstFileServerMediaSubsession() 
{
}

#define TRANSPORT_PACKET_SIZE 188
#define TRANSPORT_PACKETS_PER_NETWORK_PACKET 7
// The product of these two numbers must be enough to fit within a network packet

FramedSource* MPEG2TstFileServerMediaSubsession ::createNewStreamSource(unsigned /*clientSessionId*/, unsigned& estBitrate) 
{
  estBitrate = 5000; // kbps, estimate

  CTsFileDuration duration;
  duration.SetFileName((char*)fFileName);
  duration.OpenFile();
  duration.UpdateDuration();
  duration.CloseFile();
  _duration=duration.Duration();

  // Create the video source:
  unsigned const inputDataChunkSize = TRANSPORT_PACKETS_PER_NETWORK_PACKET*TRANSPORT_PACKET_SIZE;
  ByteStreamFileSource* fileSource = ByteStreamFileSource::createNew(envir(), fFileName, inputDataChunkSize);
  if (fileSource == NULL) return NULL;
  fFileSize = fileSource->fileSize();

  // Create a MPEG demultiplexor that reads from that source.
  MPEG1or2Demux* baseDemultiplexor = MPEG1or2Demux::createNew(envir(), fileSource);

  // Create, from this, a source that returns raw PES packets:
  MPEG1or2DemuxedElementaryStream* pesSource = baseDemultiplexor->newRawPESStream();
  
  // And, from this, a filter that converts to MPEG-2 Transport Stream frames:
  MPEG2TransportStreamFromPESSource* tsSource  = MPEG2TransportStreamFromPESSource::createNew(envir(), pesSource);

  // Create a framer for the Transport Stream:
  return MPEG2TransportStreamFramer::createNew(envir(), tsSource);
}

RTPSink* MPEG2TstFileServerMediaSubsession ::createNewRTPSink(Groupsock* rtpGroupsock, unsigned char /*rtpPayloadTypeIfDynamic*/, FramedSource* /*inputSource*/) 
{
  return SimpleRTPSink::createNew(envir(), rtpGroupsock,
				  33, 90000, "video", "mp2t",
				  1, True, False /*no 'M' bit*/);
}

float MPEG2TstFileServerMediaSubsession::duration() const
{
  return _duration;
}
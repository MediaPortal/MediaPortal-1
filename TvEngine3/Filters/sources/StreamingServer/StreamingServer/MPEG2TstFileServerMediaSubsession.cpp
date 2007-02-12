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
extern void Log(const char *fmt, ...) ;

MPEG2TstFileServerMediaSubsession* MPEG2TstFileServerMediaSubsession::createNew(UsageEnvironment& env,char const* fileName,Boolean reuseFirstSource) 
{
  return new MPEG2TstFileServerMediaSubsession(env, fileName, reuseFirstSource);
}

MPEG2TstFileServerMediaSubsession ::MPEG2TstFileServerMediaSubsession(UsageEnvironment& env, char const* fileName, Boolean reuseFirstSource)
: FileServerMediaSubsession(env, fileName, reuseFirstSource) 
{
  Log("MPEG2TstFileServerMediaSubsession::ctor");
  m_fileSource=NULL;
  m_baseDemultiplexor=NULL;
  m_pesSource=NULL;
  m_tsSource=NULL;
}

void MPEG2TstFileServerMediaSubsession::OnDelete()
{
  Log("MPEG2TstFileServerMediaSubsession::OnDelete");
  if (m_fileSource!=NULL) 
    Medium::close(m_fileSource);
  m_fileSource=NULL;

  if (m_baseDemultiplexor!=NULL)
    Medium::close(m_baseDemultiplexor);
  m_baseDemultiplexor=NULL;

  if (m_pesSource!=NULL)
    Medium::close(m_pesSource);
  m_pesSource=NULL;

  m_tsSource=NULL;
}

MPEG2TstFileServerMediaSubsession::~MPEG2TstFileServerMediaSubsession() 
{
  Log("MPEG2TstFileServerMediaSubsession::dtor");
  if (m_fileSource!=NULL) 
    Medium::close(m_fileSource);
  m_fileSource=NULL;

  if (m_baseDemultiplexor!=NULL)
    Medium::close(m_baseDemultiplexor);
  m_baseDemultiplexor=NULL;

  if (m_pesSource!=NULL)
    Medium::close(m_pesSource);
  m_pesSource=NULL;

  if (m_tsSource!=NULL)
    Medium:close(m_tsSource);
  m_tsSource=NULL;
}

#define TRANSPORT_PACKET_SIZE 188
#define TRANSPORT_PACKETS_PER_NETWORK_PACKET 7
// The product of these two numbers must be enough to fit within a network packet

FramedSource* MPEG2TstFileServerMediaSubsession ::createNewStreamSource(unsigned /*clientSessionId*/, unsigned& estBitrate) 
{
  Log("MPEG2TstFileServerMediaSubsession:createNewStreamSource");
  estBitrate = 5000; // kbps, estimate

  CTsFileDuration duration;
  duration.SetFileName((char*)fFileName);
  duration.OpenFile();
  duration.UpdateDuration();
  duration.CloseFile();
  _duration=duration.Duration();

  // Create the video source:
  unsigned const inputDataChunkSize = TRANSPORT_PACKETS_PER_NETWORK_PACKET*TRANSPORT_PACKET_SIZE;
  ByteStreamFileSource* m_fileSource = ByteStreamFileSource::createNew(envir(), fFileName, inputDataChunkSize);
  if (m_fileSource == NULL) return NULL;
  fFileSize = m_fileSource->fileSize();

  // Create a MPEG demultiplexor that reads from that source.
  m_baseDemultiplexor = MPEG1or2Demux::createNew(envir(), m_fileSource);

  // Create, from this, a source that returns raw PES packets:
  m_pesSource = m_baseDemultiplexor->newRawPESStream();
  
  // And, from this, a filter that converts to MPEG-2 Transport Stream frames:
  m_tsSource  = MPEG2TransportStreamFromPESSource::createNew(envir(), m_pesSource);

  // Create a framer for the Transport Stream:
  MPEG2TransportStreamFramer* framer= MPEG2TransportStreamFramer::createNew(envir(), m_tsSource);
  framer->SetOnDelete(this);
  return framer;
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
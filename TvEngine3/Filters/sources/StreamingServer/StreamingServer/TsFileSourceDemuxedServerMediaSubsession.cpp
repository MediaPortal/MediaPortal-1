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
// Copyright (c) 1996-2005 Live Networks, Inc.  All rights reserved.
// A 'ServerMediaSubsession' object that creates new, unicast, "RTPSink"s
// on demand, from a MPEG-1 or 2 demuxer.
// Implementation

#include "TsFileSourceDemuxedServerMediaSubsession.hh"
#include "MPEG1or2AudioStreamFramer.hh"
#include "MPEG1or2AudioRTPSink.hh"
#include "MPEG1or2VideoStreamFramer.hh"
#include "MPEG1or2VideoRTPSink.hh"
#include "AC3AudioStreamFramer.hh"
#include "AC3AudioRTPSink.hh"
#include "TsStreamFileSource.hh"
extern void Log(const char *fmt, ...) ;
TsFileSourceDemuxedServerMediaSubsession* TsFileSourceDemuxedServerMediaSubsession::createNew(TsFileSinkDemux& demux, u_int8_t streamIdTag,Boolean reuseFirstSource, Boolean iFramesOnly, double vshPeriod) 
{
  return new TsFileSourceDemuxedServerMediaSubsession(demux, streamIdTag,
						  reuseFirstSource,
						  iFramesOnly, vshPeriod);
}

TsFileSourceDemuxedServerMediaSubsession::TsFileSourceDemuxedServerMediaSubsession(TsFileSinkDemux& demux,u_int8_t streamIdTag, Boolean reuseFirstSource,Boolean iFramesOnly, double vshPeriod)
  : OnDemandServerMediaSubsession(demux.envir(), reuseFirstSource),
    fOurDemux(demux), fStreamIdTag(streamIdTag),
    fIFramesOnly(iFramesOnly), fVSHPeriod(vshPeriod) 
{
}

TsFileSourceDemuxedServerMediaSubsession::~TsFileSourceDemuxedServerMediaSubsession() 
{
}

FramedSource* TsFileSourceDemuxedServerMediaSubsession::createNewStreamSource(unsigned clientSessionId, unsigned& estBitrate) 
{
  FramedSource* es = NULL;
  do 
	{
    es = fOurDemux.newElementaryStream(clientSessionId, fStreamIdTag);
    if (es == NULL) break;

    if ((fStreamIdTag&0xF0) == 0xC0 /*MPEG audio*/) 
		{
      estBitrate = 128; // kbps, estimate
      return MPEG1or2AudioStreamFramer::createNew(envir(), es);
    } 
		else if ((fStreamIdTag&0xF0) == 0xE0 /*video*/) 
		{
      estBitrate = 4600; // kbps, estimate
      return MPEG1or2VideoStreamFramer::createNew(envir(), es,fIFramesOnly, fVSHPeriod);
    } 
		else if (fStreamIdTag == 0xBD /*AC-3 audio*/) 
		{
      estBitrate = 192; // kbps, estimate
      return AC3AudioStreamFramer::createNew(envir(), es);
    } 
		else 
		{ // unknown stream type
      break;
    }
  } while (0);

  // An error occurred:
  Medium::close(es);
  return NULL;
}

RTPSink* TsFileSourceDemuxedServerMediaSubsession::createNewRTPSink(Groupsock* rtpGroupsock, unsigned char rtpPayloadTypeIfDynamic,FramedSource* inputSource) 
{
  if ((fStreamIdTag&0xF0) == 0xC0 /*MPEG audio*/) 
	{
    return MPEG1or2AudioRTPSink::createNew(envir(), rtpGroupsock);
  } 
	else if ((fStreamIdTag&0xF0) == 0xE0 /*video*/) 
	{
    return MPEG1or2VideoRTPSink::createNew(envir(), rtpGroupsock);
  } 
	else if (fStreamIdTag == 0xBD /*AC-3 audio*/) 
	{
    // Get the sampling frequency from the audio source; use it for the RTP frequency:
    AC3AudioStreamFramer* audioSource
      = (AC3AudioStreamFramer*)inputSource;
    return AC3AudioRTPSink::createNew(envir(), rtpGroupsock, rtpPayloadTypeIfDynamic,audioSource->samplingRate());
  } 
	else 
	{
    return NULL;
  }
}

void TsFileSourceDemuxedServerMediaSubsession::seekStreamSource(FramedSource* inputSource, float seekNPT) 
{
	Log("seekStreamSource %d", seekNPT);
  unsigned const size = fOurDemux.fileSize();
  float const dur = fOurDemux.fileDuration();
  unsigned absBytePosition = dur == 0.0 ? 0 : (unsigned)((seekNPT/dur)*size);

	if (absBytePosition>size) absBytePosition=size;
  // "inputSource" is a 'framer'
  // Flush its data, to account for the seek that we're about to do:
  if ((fStreamIdTag&0xF0) == 0xC0 /*MPEG audio*/) 
	{
    MPEG1or2AudioStreamFramer* framer = (MPEG1or2AudioStreamFramer*)inputSource;
    framer->flushInput();
  } 
	else if ((fStreamIdTag&0xF0) == 0xE0 /*video*/) 
	{
    MPEG1or2VideoStreamFramer* framer = (MPEG1or2VideoStreamFramer*)inputSource;
    framer->flushInput();
  }

  // "inputSource" is a filter; its input source is the original elem stream source:
  MPEG1or2DemuxedElementaryStream* elemStreamSource
    = (MPEG1or2DemuxedElementaryStream*)(((FramedFilter*)inputSource)->inputSource());

  // Next, get the original source demux:
  MPEG1or2Demux& sourceDemux = elemStreamSource->sourceDemux();

  // and flush its input buffers:
  sourceDemux.flushInput();

  // Then, get the original input file stream from the source demux:
  TsStreamFileSource* inputFileSource
    = (TsStreamFileSource*)(sourceDemux.inputSource());
  // Note: We can make that cast, because we know that the demux was originally
  // created from a "TsStreamFileSource".

  // Do the appropriate seek within the input file stream:
  inputFileSource->seekToByteAbsolute(absBytePosition);
	Log("Seek %d duration:%f length:%d pos:%d",
		(int)seekNPT,(int)dur,(int)size,(int)absBytePosition);
}

float TsFileSourceDemuxedServerMediaSubsession::duration() const 
{
  return 3600;//fOurDemux.fileDuration();
}

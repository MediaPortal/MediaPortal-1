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
// C++ header

#ifndef _TS_FILE_SERVER_MEDIA_SUBSESSION_H
#define _TS_FILE_SERVER_MEDIA_SUBSESSION_H

#ifndef _FILE_SERVER_MEDIA_SUBSESSION_HH
#include "FileServerMediaSubsession.hh"
#endif
#include "TsStreamFileSource.hh"
#ifndef _MPEG2_TRANSPORT_STREAM_INDEX_FILE_HH
#include "MPEG2TransportStreamIndexFile.hh"
#endif

class TsClientTrickPlayState; // forward

class TsFileServerMediaSubsession: public FileServerMediaSubsession{
public:
  static TsFileServerMediaSubsession*
  createNew(UsageEnvironment& env,
	    char const* dataFileName, char const* indexFileName,
	    Boolean reuseFirstSource);

protected:
  TsFileServerMediaSubsession(UsageEnvironment& env,
					  char const* fileName,
					  MPEG2TransportStreamIndexFile* indexFile,
					  Boolean reuseFirstSource);
      // called only by createNew();
  virtual ~TsFileServerMediaSubsession();
  virtual float duration() const;

private: // redefined virtual functions
  // Note that because - to implement 'trick play' operations - we're operating on
  // more than just the input source, we reimplement some functions that are
  // already implemented in "OnDemandServerMediaSubsession", rather than
  // reimplementing "seekStreamSource()" and "setStreamSourceScale()":
  virtual void startStream(unsigned clientSessionId, void* streamToken,
                           TaskFunc* rtcpRRHandler,
                           void* rtcpRRHandlerClientData,
                           unsigned short& rtpSeqNum,
                           unsigned& rtpTimestamp);
  virtual void pauseStream(unsigned clientSessionId, void* streamToken);
  virtual void seekStream(unsigned clientSessionId, void* streamToken, double seekNPT);
  virtual void setStreamScale(unsigned clientSessionId, void* streamToken, float scale);
  virtual void deleteStream(unsigned clientSessionId, void*& streamToken);

  // The virtual functions thare are usually implemented by "ServerMediaSubsession"s:
  virtual FramedSource* createNewStreamSource(unsigned clientSessionId,
					      unsigned& estBitrate);
  virtual RTPSink* createNewRTPSink(Groupsock* rtpGroupsock,
                                    unsigned char rtpPayloadTypeIfDynamic,
				    FramedSource* inputSource);

  virtual void testScaleFactor(float& scale);
  virtual void seekStreamSource(FramedSource* inputSource, double seekNPT);

private:
  TsClientTrickPlayState* lookupClient(unsigned clientSessionId);

  MPEG2TransportStreamIndexFile* fIndexFile;
  char m_fileName[MAX_PATH];
  HashTable* fClientSessionHashTable; // indexed by client session id
};

#endif
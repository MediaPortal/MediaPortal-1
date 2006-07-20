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
// A server demultiplexer for a MPEG 1 or 2 Program Stream
// C++ header

#ifndef TsFileSinkDemux_HH
#define TsFileSinkDemux_HH

#ifndef _SERVER_MEDIA_SESSION_HH
#include "ServerMediaSession.hh"
#endif
#ifndef _MPEG_1OR2_DEMUXED_ELEMENTARY_STREAM_HH
#include "MPEG1or2DemuxedElementaryStream.hh"
#endif

#include "TsStreamFileSource.hh"
class TsFileSinkDemux: public Medium {
public:
  static TsFileSinkDemux*   createNew(UsageEnvironment& env, char const* fileName, Boolean reuseFirstSource);

  ServerMediaSubsession* newAudioServerMediaSubsession(); // MPEG-1 or 2 audio
  ServerMediaSubsession* newVideoServerMediaSubsession(Boolean iFramesOnly = False,
						       double vshPeriod = 5.0
		       /* how often (in seconds) to inject a Video_Sequence_Header,
			  if one doesn't already appear in the stream */);
  ServerMediaSubsession* newAC3AudioServerMediaSubsession(); // AC-3 audio (from VOB)

  __int64 fileSize() const;
  float fileDuration()  ;

private:
  TsFileSinkDemux(UsageEnvironment& env, char const* fileName,Boolean reuseFirstSource);
      // called only by createNew();
  virtual ~TsFileSinkDemux();

private:
  friend class TsFileSourceDemuxedServerMediaSubsession;
  MPEG1or2DemuxedElementaryStream* newElementaryStream(unsigned clientSessionId,u_int8_t streamIdTag);

private:
 // TsStreamFileSource* m_pFileSource;
  char const* fFileName;
  __int64 fFileSize;
  float fFileDuration;
  Boolean fReuseFirstSource;
  MPEG1or2Demux* fSession0Demux;
  MPEG1or2Demux* fLastCreatedDemux;
  u_int8_t fLastClientSessionId;
	UsageEnvironment* m_env;
  DWORD _lastTicks;
  bool _updatingDuration;
};

#endif

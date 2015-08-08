/* 
*  Copyright (C) 2006-2009 Team MediaPortal
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
#pragma once
#ifndef _MPEG2_TRANSPORT_STREAM_MULTIPLEXOR_HH
#include "MPEG2TransportStreamMultiplexor.hh"
#endif
#ifndef _MPEG_1OR2_DEMUXED_ELEMENTARY_STREAM_HH
#include "MPEG1or2DemuxedElementaryStream.hh"
#endif


class MPMPEG2TransportStreamFromPESSource : public MPEG2TransportStreamMultiplexor
{
  public:
    static MPMPEG2TransportStreamFromPESSource* createNew(UsageEnvironment& env,
                                                          MPEG1or2DemuxedElementaryStream* inputSource,
                                                          bool isVideoSource);

  protected:
    // called only by createNew();
    MPMPEG2TransportStreamFromPESSource(UsageEnvironment& env,
                                        MPEG1or2DemuxedElementaryStream* inputSource,
                                        bool isVideoSource);
    virtual ~MPMPEG2TransportStreamFromPESSource();

  //---------------------------------------------------------------------------
  // LIVE555 MPEG2TransportStreamFromPESSource.h code copied 1:1.
  private:
    virtual void doStopGettingFrames();
    virtual void awaitNewBuffer(unsigned char* oldBuffer);

    static void afterGettingFrame(void* clientData,
                                  unsigned frameSize,
				                          unsigned numTruncatedBytes,
				                          struct timeval presentationTime,
				                          unsigned durationInMicroseconds);
    void afterGettingFrame1(unsigned frameSize,
			                      unsigned numTruncatedBytes,
			                      struct timeval presentationTime,
			                      unsigned durationInMicroseconds);

    MPEG1or2DemuxedElementaryStream* fInputSource;
    unsigned char* fInputBuffer;
  //---------------------------------------------------------------------------
};
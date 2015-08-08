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

//-----------------------------------------------------------------------------
// LIVE555 MPEG2TransportStreamFromPESSource.cpp code copied 1:1 except:
// 1. Include line.
// 2. MPMPEG2TransportStreamFromPESSource instead of MPEG2TransportStreamFromPESSource.
// 3. Additional isVideoSource parameter on createNew() and constructor.
// 4. Assignment to fHaveVideoStreams in constructor.
// 5. Contents of afterGettingFrame1().
// 6. Formatting.

#include "MPMPEG2TransportStreamFromPESSource.h"


#define MAX_PES_PACKET_SIZE (6 + 65535)


MPMPEG2TransportStreamFromPESSource* MPMPEG2TransportStreamFromPESSource::createNew(UsageEnvironment& env,
                                                                                    MPEG1or2DemuxedElementaryStream* inputSource,
                                                                                    bool isVideoSource)
{
  return new MPMPEG2TransportStreamFromPESSource(env, inputSource, isVideoSource);
}

MPMPEG2TransportStreamFromPESSource::MPMPEG2TransportStreamFromPESSource(UsageEnvironment& env,
                                                                          MPEG1or2DemuxedElementaryStream* inputSource,
                                                                          bool isVideoSource)
  : MPEG2TransportStreamMultiplexor(env), fInputSource(inputSource)
{
  fInputBuffer = new unsigned char[MAX_PES_PACKET_SIZE];
  fHaveVideoStreams = isVideoSource ? True : False;
}

MPMPEG2TransportStreamFromPESSource::~MPMPEG2TransportStreamFromPESSource()
{
  Medium::close(fInputSource);
  delete[] fInputBuffer;
}

void MPMPEG2TransportStreamFromPESSource::doStopGettingFrames()
{
  fInputSource->stopGettingFrames();
}

void MPMPEG2TransportStreamFromPESSource::awaitNewBuffer(unsigned char* /*oldBuffer*/)
{
  fInputSource->getNextFrame(fInputBuffer,
                              MAX_PES_PACKET_SIZE,
			                        afterGettingFrame,
                              this,
			                        FramedSource::handleClosure,
                              this);
}

void MPMPEG2TransportStreamFromPESSource::afterGettingFrame(void* clientData,
                                                            unsigned frameSize,
		                                                        unsigned numTruncatedBytes,
		                                                        struct timeval presentationTime,
		                                                        unsigned durationInMicroseconds)
{
  MPMPEG2TransportStreamFromPESSource* source = (MPMPEG2TransportStreamFromPESSource*)clientData;
  source->afterGettingFrame1(frameSize,
                              numTruncatedBytes,
			                        presentationTime,
                              durationInMicroseconds);
}

void MPMPEG2TransportStreamFromPESSource::afterGettingFrame1(unsigned frameSize,
		                                                          unsigned /*numTruncatedBytes*/,
		                                                          struct timeval /*presentationTime*/,
		                                                          unsigned /*durationInMicroseconds*/)
{
  if (frameSize < 4)
  {
    return;
  }

  // Force LIVE555 to throw away video stream data when we don't want it. It
  // should be ignoring this data anyway... but of course, it doesn't!
  if (!fHaveVideoStreams && frameSize >= 4)
  {
    unsigned char streamId = fInputBuffer[3];
    if ((streamId & 0xf0) == 0xe0)  // video stream?
    {
      fInputBuffer[3] = 0xbe;       // mark as padding so ignored
    }
    else if (streamId == 0xbc && frameSize >= 16)  // program stream map
    {
      unsigned short programStreamMapLength = (fInputBuffer[4] << 8) | fInputBuffer[5];
      if (programStreamMapLength >= 10 && frameSize >= (unsigned)programStreamMapLength + 6)
      {
        unsigned short programStreamInfoLength = (fInputBuffer[8] << 8) | fInputBuffer[9];
        if (programStreamInfoLength + 10 <= programStreamMapLength)
        {
          unsigned short pointer = 10 + programStreamInfoLength;  // first ES map length byte
          unsigned short elementaryStreamMapLength = (fInputBuffer[pointer] << 8) | fInputBuffer[pointer + 1];
          pointer += 2;
          if (programStreamInfoLength + elementaryStreamMapLength + 10 <= programStreamMapLength)
          {
            unsigned short endOfElementaryStreamMap = pointer + elementaryStreamMapLength;
            while (pointer + 3 < endOfElementaryStreamMap)
            {
              unsigned char streamType = fInputBuffer[pointer];
              if (streamType == 1 || streamType == 2 || streamType == 0x10 || streamType == 0x1b)   // video stream?
              {
                fInputBuffer[pointer] = 0xbe;     // mark as padding so ignored
              }
              pointer += 2;
              unsigned short elementaryStreamInfoLength = (fInputBuffer[pointer] << 8) | fInputBuffer[pointer + 1];
              pointer += elementaryStreamInfoLength;
            }
          }
        }
      }
    }
  }

  handleNewBuffer(fInputBuffer,
                  frameSize,
                  fInputSource->mpegVersion(),
                  fInputSource->lastSeenSCR());
}
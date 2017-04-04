/**********
This library is free software; you can redistribute it and/or modify it under
the terms of the GNU Lesser General Public License as published by the
Free Software Foundation; either version 3 of the License, or (at your
option) any later version. (See <http://www.gnu.org/copyleft/lesser.html>.)

This library is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public License for
more details.

You should have received a copy of the GNU Lesser General Public License
along with this library; if not, write to the Free Software Foundation, Inc.,
51 Franklin Street, Fifth Floor, Boston, MA 02110-1301  USA
**********/
// "liveMedia"
// Copyright (c) 1996-2017 Live Networks, Inc.  All rights reserved.
// A class for generating MPEG-2 Transport Stream from one or more input
// Elementary Stream data sources
// C++ header

#ifndef _MPEG2_TRANSPORT_STREAM_MULTIPLEXOR_HH
#define _MPEG2_TRANSPORT_STREAM_MULTIPLEXOR_HH

#ifndef _FRAMED_SOURCE_HH
#include "FramedSource.hh"
#endif
#ifndef _MPEG_1OR2_DEMUX_HH
#include "MPEG1or2Demux.hh" // for SCR
#endif

#define PID_TABLE_SIZE 256

class MPEG2TransportStreamMultiplexor: public FramedSource {
public:
  Boolean canDeliverNewFrameImmediately() const { return fInputBufferBytesUsed < fInputBufferSize; }
      // Can be used by a downstream reader to test whether the next call to "doGetNextFrame()"
      // will deliver data immediately).

protected:
  MPEG2TransportStreamMultiplexor(UsageEnvironment& env);
  virtual ~MPEG2TransportStreamMultiplexor();

  virtual void awaitNewBuffer(unsigned char* oldBuffer) = 0;
      // implemented by subclasses

  void handleNewBuffer(unsigned char* buffer, unsigned bufferSize,
		       int mpegVersion, MPEG1or2Demux::SCR scr, int16_t PID = -1);
      // called by "awaitNewBuffer()"
      // Note: For MPEG-4 video, set "mpegVersion" to 4; for H.264 video, set "mpegVersion" to 5. 
      // The buffer is assumed to be a PES packet, with a proper PES header.
      // If "PID" is not -1, then it (currently, only the low 8 bits) is used as the stream's PID,
      // otherwise the "stream_id" in the PES header is reused to be the stream's PID.

private:
  // Redefined virtual functions:
  virtual void doGetNextFrame();

private:
  void deliverDataToClient(u_int8_t pid, unsigned char* buffer, unsigned bufferSize,
			   unsigned& startPositionInBuffer);

  void deliverPATPacket();
  void deliverPMTPacket(Boolean hasChanged);

  void setProgramStreamMap(unsigned frameSize);

protected:
  Boolean fHaveVideoStreams;

private:
  unsigned fOutgoingPacketCounter;
  unsigned fProgramMapVersion;
  u_int8_t fPreviousInputProgramMapVersion, fCurrentInputProgramMapVersion;
      // These two fields are used if we see "program_stream_map"s in the input.
  struct {
    unsigned counter;
    u_int8_t streamType; // for use in Program Maps
  } fPIDState[PID_TABLE_SIZE];
  u_int8_t fPCR_PID, fCurrentPID;
      // Note: We map 8-bit stream_ids directly to PIDs
  MPEG1or2Demux::SCR fPCR;
  unsigned char* fInputBuffer;
  unsigned fInputBufferSize, fInputBufferBytesUsed;
  Boolean fIsFirstAdaptationField;
};


// The CRC calculation function that Transport Streams use.  We make this function public
// here in case it's useful elsewhere:
u_int32_t calculateCRC(u_int8_t const* data, unsigned dataLength, u_int32_t initialValue = 0xFFFFFFFF);

#endif

/* 
 *  Copyright (C) 2005-2013 Team MediaPortal
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
#include <ctime>
#include <DShow.h>    // (media types), IMediaSample, IPin, REFERENCE_TIME
#include <streams.h>  // CAutoLock, CBaseFilter, CCritSec, CMediaType, CRenderedInputPin
#include <WinError.h> // HRESULT
#include "..\..\shared\FileWriter.h"
#include "..\..\shared\PacketSync.h"
#include "..\..\shared\PidTable.h"
#include "IMuxInputPin.h"
#include "IStreamMultiplexer.h"

using namespace std;


#define NOT_RECEIVING -1

#define STREAM_TYPE_MPEG1_SYSTEM_STREAM 0xfe
#define STREAM_TYPE_MPEG2_PROGRAM_STREAM 0xfd
#define STREAM_TYPE_MPEG2_TRANSPORT_STREAM 0xfc
#define STREAM_TYPE_TELETEXT 0xfb
#define STREAM_TYPE_VPS 0xfa
#define STREAM_TYPE_WSS 0xf9

#define RECEIVE_BUFFER_TS_PACKET_COUNT 5
#define RECEIVE_BUFFER_SIZE TS_PACKET_LEN * RECEIVE_BUFFER_TS_PACKET_COUNT


// Note: order is important! Some Mainconcept audio encoders seem to accept the
// first proposed media type. If that first type is a video type, it tricks us
// into thinking that the encoder is delivering video... but it is still
// delivering audio as usual. Moral of the story: audio should be before video.
const AMOVIESETUP_MEDIATYPE INPUT_MEDIA_TYPES[] =
{
  { &MEDIATYPE_Audio, &MEDIASUBTYPE_MPEG1Payload },
  { &MEDIATYPE_Stream, &MEDIASUBTYPE_MPEG1Audio },
  { &MEDIATYPE_Audio, &MEDIASUBTYPE_MPEG1AudioPayload },

  { &MEDIATYPE_Audio, &MEDIASUBTYPE_MPEG2_AUDIO },
  { &MEDIATYPE_Stream, &MEDIASUBTYPE_MPEG2_AUDIO },

  { &MEDIATYPE_Video, &MEDIASUBTYPE_MPEG1Payload },
  { &MEDIATYPE_Video, &MEDIASUBTYPE_MPEG1Video },
  { &MEDIATYPE_Stream, &MEDIASUBTYPE_MPEG1Video },

  { &MEDIATYPE_Video, &MEDIASUBTYPE_MPEG2_VIDEO },
  { &MEDIATYPE_Stream, &MEDIASUBTYPE_MPEG2_VIDEO },

  { &MEDIATYPE_VBI, &MEDIASUBTYPE_TELETEXT },
  { &MEDIATYPE_VBI, &MEDIASUBTYPE_VPS },
  { &MEDIATYPE_VBI, &MEDIASUBTYPE_WSS },

  { &MEDIATYPE_Stream, &MEDIASUBTYPE_MPEG1System },
  { &MEDIATYPE_Stream, &MEDIASUBTYPE_MPEG2_PROGRAM },

  { &MEDIATYPE_Stream, &MEDIASUBTYPE_MPEG2_TRANSPORT }
};

// This array describes the recognised stream type for each media type in the
// above array.
const unsigned char STREAM_TYPES[] =
{
  STREAM_TYPE_AUDIO_MPEG1,
  STREAM_TYPE_AUDIO_MPEG1,
  STREAM_TYPE_AUDIO_MPEG1,

  STREAM_TYPE_AUDIO_MPEG2_PART3,
  STREAM_TYPE_AUDIO_MPEG2_PART3,

  STREAM_TYPE_VIDEO_MPEG1,
  STREAM_TYPE_VIDEO_MPEG1,
  STREAM_TYPE_VIDEO_MPEG1,

  STREAM_TYPE_VIDEO_MPEG2,
  STREAM_TYPE_VIDEO_MPEG2,

  STREAM_TYPE_TELETEXT,
  STREAM_TYPE_VPS,        // video programme system
  STREAM_TYPE_WSS,        // wide screen signalling

  STREAM_TYPE_MPEG1_SYSTEM_STREAM,
  STREAM_TYPE_MPEG2_PROGRAM_STREAM,

  STREAM_TYPE_MPEG2_TRANSPORT_STREAM
};

const unsigned char MPEG1_VIDEO_INPUT_MEDIA_TYPE_COUNT = 3;
const unsigned char MPEG1_AUDIO_INPUT_MEDIA_TYPE_COUNT = 3;
const unsigned char MPEG2_VIDEO_INPUT_MEDIA_TYPE_COUNT = 2;
const unsigned char MPEG2_AUDIO_INPUT_MEDIA_TYPE_COUNT = 2;
const unsigned char TELETEXT_INPUT_MEDIA_TYPE_COUNT = 1;
const unsigned char VPS_INPUT_MEDIA_TYPE_COUNT = 1;
const unsigned char WSS_INPUT_MEDIA_TYPE_COUNT = 1;
const unsigned char PROGRAM_STREAM_INPUT_MEDIA_TYPE_COUNT = 2;
const unsigned char TRANSPORT_STREAM_INPUT_MEDIA_TYPE_COUNT = 1;

const unsigned char INPUT_MEDIA_TYPE_COUNT = MPEG1_VIDEO_INPUT_MEDIA_TYPE_COUNT +
                                              MPEG2_VIDEO_INPUT_MEDIA_TYPE_COUNT +
                                              MPEG1_AUDIO_INPUT_MEDIA_TYPE_COUNT +
                                              MPEG2_AUDIO_INPUT_MEDIA_TYPE_COUNT +
                                              TELETEXT_INPUT_MEDIA_TYPE_COUNT +
                                              VPS_INPUT_MEDIA_TYPE_COUNT +
                                              WSS_INPUT_MEDIA_TYPE_COUNT +
                                              PROGRAM_STREAM_INPUT_MEDIA_TYPE_COUNT +
                                              TRANSPORT_STREAM_INPUT_MEDIA_TYPE_COUNT;


class CMuxInputPin : public CRenderedInputPin, CPacketSync, public IMuxInputPin
{
  public:
    CMuxInputPin(unsigned char id,
                  IStreamMultiplexer* multiplexer,
                  CBaseFilter* filter,
                  CCritSec* filterLock,
                  CCritSec& receiveLock,
                  HRESULT* hr);
    virtual ~CMuxInputPin();

    HRESULT BreakConnect();
    HRESULT CheckMediaType(const CMediaType* mediaType);
    HRESULT CompleteConnect(IPin* receivePin);
    HRESULT GetMediaType(int position, CMediaType* mediaType);
    STDMETHODIMP Receive(IMediaSample* sample);
    STDMETHODIMP ReceiveCanBlock();
    HRESULT Run(REFERENCE_TIME startTime);
    HRESULT SetMediaType(const CMediaType* mediaType);

    unsigned char GetId() const;
    unsigned char GetStreamType() const;
    clock_t GetReceiveTime() const;
    HRESULT StartDumping(const wchar_t* fileName);
    HRESULT StopDumping();

  private:
    void OnTsPacket(unsigned char* tsPacket);

    unsigned char m_pinId;
    unsigned char m_streamType;
    clock_t m_receiveTime;
    CCritSec& m_receiveLock;

    IStreamMultiplexer* m_multiplexer;

    short m_tsReceiveBufferOffset;
    unsigned char m_tsReceiveBuffer[RECEIVE_BUFFER_SIZE];

    bool m_isDumpEnabled;
    FileWriter m_dumpFileWriter;
    CCritSec m_dumpLock;
};
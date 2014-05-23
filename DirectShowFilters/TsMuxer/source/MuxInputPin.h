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
#include <streams.h>
#include <Windows.h>
#include "..\..\shared\FileWriter.h"
#include "..\..\shared\PacketSync.h"
#include "..\..\shared\PidTable.h"
#include "IMuxInputPin.h"
#include "IStreamMultiplexer.h"


#define NOT_RECEIVING -1

#define STREAM_TYPE_MPEG1_SYSTEM_STREAM 0xfe
#define STREAM_TYPE_MPEG2_PROGRAM_STREAM 0xfd
#define STREAM_TYPE_MPEG2_TRANSPORT_STREAM 0xfc
#define STREAM_TYPE_TELETEXT 0xfb

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

  { &MEDIATYPE_Stream, &MEDIASUBTYPE_MPEG1System },
  { &MEDIATYPE_Stream, &MEDIASUBTYPE_MPEG2_PROGRAM },

  { &MEDIATYPE_Stream, &MEDIASUBTYPE_MPEG2_TRANSPORT }
};

// This array describes the recognised stream type for each media type in the
// above array.
const int STREAM_TYPES[] =
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

  STREAM_TYPE_MPEG1_SYSTEM_STREAM,
  STREAM_TYPE_MPEG2_PROGRAM_STREAM,

  STREAM_TYPE_MPEG2_TRANSPORT_STREAM
};

const int MPEG1_VIDEO_INPUT_MEDIA_TYPE_COUNT = 3;
const int MPEG1_AUDIO_INPUT_MEDIA_TYPE_COUNT = 3;
const int MPEG2_VIDEO_INPUT_MEDIA_TYPE_COUNT = 2;
const int MPEG2_AUDIO_INPUT_MEDIA_TYPE_COUNT = 2;
const int TELETEXT_INPUT_MEDIA_TYPE_COUNT = 1;
const int PROGRAM_STREAM_INPUT_MEDIA_TYPE_COUNT = 2;
const int TRANSPORT_STREAM_INPUT_MEDIA_TYPE_COUNT = 1;

const int INPUT_MEDIA_TYPE_COUNT = (MPEG1_VIDEO_INPUT_MEDIA_TYPE_COUNT + MPEG1_AUDIO_INPUT_MEDIA_TYPE_COUNT) +
                                    (MPEG2_VIDEO_INPUT_MEDIA_TYPE_COUNT + MPEG2_AUDIO_INPUT_MEDIA_TYPE_COUNT) +
                                    TELETEXT_INPUT_MEDIA_TYPE_COUNT +
                                    PROGRAM_STREAM_INPUT_MEDIA_TYPE_COUNT +
                                    TRANSPORT_STREAM_INPUT_MEDIA_TYPE_COUNT;


class CMuxInputPin : public CRenderedInputPin, CPacketSync, public IMuxInputPin
{
  public:
    CMuxInputPin(byte id, IStreamMultiplexer* multiplexer, CBaseFilter* filter, CCritSec* filterLock, CCritSec* receiveLock, HRESULT* hr);
    virtual ~CMuxInputPin(void);

    HRESULT BreakConnect();
    HRESULT CheckMediaType(const CMediaType* mediaType);
    HRESULT CompleteConnect(IPin* receivePin);
    STDMETHODIMP EndOfStream();
    HRESULT GetMediaType(int position, CMediaType* mediaType);
    STDMETHODIMP NewSegment(REFERENCE_TIME startTime, REFERENCE_TIME stopTime, double rate);
    STDMETHODIMP Receive(IMediaSample* sample);
    STDMETHODIMP ReceiveCanBlock();
    HRESULT SetMediaType(const CMediaType* mediaType);

    byte GetId();
    byte GetStreamType();
    DWORD GetReceiveTickCount();
    HRESULT StartDumping(wchar_t* fileName);
    HRESULT StopDumping();

  private:
    void OnTsPacket(byte* tsPacket);

    byte m_pinId;
    byte m_streamType;
    DWORD m_receiveTickCount;
    CCritSec* m_receiveLock;

    IStreamMultiplexer* m_multiplexer;

    short m_tsReceiveBufferOffset;
    byte m_tsReceiveBuffer[RECEIVE_BUFFER_SIZE];

    bool m_isDumpEnabled;
    FileWriter m_dumpFileWriter;
    CCritSec m_dumpLock;
};
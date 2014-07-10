/*
    Copyright (C) 2007-2010 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2.  If not, see <http://www.gnu.org/licenses/>.
*/

#include "StdAfx.h"

#include "PacketInputFormat.h"
#include "StreamPackageDataRequest.h"
#include "StreamPackagePacketRequest.h"
#include "ErrorCodes.h"

#define STREAM_READ_BUFFER_SIZE                                       32768

CPacketInputFormat::CPacketInputFormat(HRESULT *result, IPacketDemuxer *demuxer, const wchar_t *streamFormat)
{
  // set AVInputFormat default values
  this->codec_tag = NULL;
  this->extensions = NULL;
  this->flags = 0;
  this->next = NULL;
  this->priv_class = NULL;
  this->raw_codec_id = 0;
  this->read_close = NULL;
  this->read_pause = NULL;
  this->read_play = NULL;
  this->read_probe = NULL;
  this->read_seek2 = NULL;
  this->read_timestamp = NULL;

  // set AVInputFormat specified values
  this->name = PACKET_INPUT_FORMAT_IDENTIFIER;
  this->long_name = PACKET_INPUT_FORMAT_LONG_NAME;
  this->priv_data_size = 0;
  this->read_header = &this->ReadHeader;
  this->read_packet = &this->ReadPacket;
  this->read_seek = &this->Seek;

  this->streamFormatContext = NULL;
  this->streamIoContext = NULL;
  this->streamIoContextBufferPosition = 0;
  this->internalFlags = PACKET_INPUT_FORMAT_FLAG_NONE;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    CHECK_POINTER_DEFAULT_HRESULT(*result, demuxer);
    CHECK_POINTER_DEFAULT_HRESULT(*result, streamFormat);

    if (SUCCEEDED(*result))
    {
      this->demuxer = demuxer;
      this->streamFormat = Duplicate(streamFormat);

      CHECK_POINTER_HRESULT(*result, this->streamFormat, *result, E_OUTOFMEMORY);
    }
  }
}

CPacketInputFormat::~CPacketInputFormat(void)
{
  FREE_MEM(this->streamFormat);
}

/* get methods */

/* set methods */

/* other methods */

bool CPacketInputFormat::IsSetFlags(uint64_t flags)
{
  return ((this->internalFlags & flags) == flags);
}

/* static methods */

int CPacketInputFormat::ReadHeader(AVFormatContext *formatContext)
{
  CPacketInputFormat *caller = static_cast<CPacketInputFormat *>(formatContext->iformat);
  int ret = 0;

  caller->streamFormatContext = avformat_alloc_context();
  ret = (caller->streamFormatContext != NULL) ? ret : (-1);

  if (ret >= 0)
  {
    uint8_t *buffer = (uint8_t *)av_mallocz(STREAM_READ_BUFFER_SIZE + FF_INPUT_BUFFER_PADDING_SIZE);
    caller->streamIoContext = avio_alloc_context(buffer, STREAM_READ_BUFFER_SIZE, 0, caller, CPacketInputFormat::StreamRead, NULL, CPacketInputFormat::StreamSeek);
    ret = ((caller->streamIoContext != NULL) && (buffer != NULL)) ? ret : (-1);

    if (ret >= 0)
    {
      caller->streamFormatContext->pb = caller->streamIoContext;

      char *streamFormatA = ConvertToMultiByteW(caller->streamFormat);

      AVInputFormat *avStreamFormat = NULL;
      if (streamFormatA != NULL)
      {
        avStreamFormat = av_find_input_format(streamFormatA);
      }

      FREE_MEM(streamFormatA);

      ret = avformat_open_input(&caller->streamFormatContext, "", avStreamFormat, NULL);
      ret = (caller->streamFormatContext->nb_streams != 0) ? ret : (-1);

      if (ret >= 0)
      {
        AVStream *inputStream = caller->streamFormatContext->streams[0];
        AVStream *stream = avformat_new_stream(formatContext, NULL);
        ret = (stream != NULL) ? ret : (-1);

        if (ret >= 0)
        {
          stream->codec->codec_type = inputStream->codec->codec_type;
          stream->codec->codec_id = inputStream->codec->codec_id;
        }
      }
    }

    if (caller->streamIoContext != NULL)
    {
      av_free(caller->streamIoContext->buffer);
      av_free(caller->streamIoContext);
      caller->streamIoContext = NULL;
      caller->streamIoContextBufferPosition = 0;
    }
  }

  if (caller->streamFormatContext)
  {
    avformat_close_input(&caller->streamFormatContext);
    caller->streamFormatContext = NULL;
  }

  caller->internalFlags |= (ret >= 0) ? PACKET_INPUT_FORMAT_FLAG_RESET_PACKET_COUNTER : PACKET_INPUT_FORMAT_FLAG_NONE;
  return ret;
}

int CPacketInputFormat::ReadPacket(AVFormatContext *formatContext, AVPacket *packet)
{
  CPacketInputFormat *caller = static_cast<CPacketInputFormat *>(formatContext->iformat);
  int ret = 0;
  CMediaPacket *mediaPacket = NULL;

  ret = caller->demuxer->GetNextMediaPacket(&mediaPacket, caller->IsSetFlags(PACKET_INPUT_FORMAT_FLAG_RESET_PACKET_COUNTER) ? STREAM_PACKAGE_PACKET_REQUEST_FLAG_RESET_PACKET_COUNTER : STREAM_PACKAGE_PACKET_REQUEST_FLAG_NONE);
  caller->internalFlags &= ~PACKET_INPUT_FORMAT_FLAG_RESET_PACKET_COUNTER;

  switch (ret)
  {
  case S_OK:
    break;
  case S_FALSE:
    ret = AVERROR(EAGAIN);
    break;
  case E_CONNECTION_LOST_TRYING_REOPEN:
    caller->internalFlags |= PACKET_INPUT_FORMAT_FLAG_DISCONTINUITY;
    ret = AVERROR(EAGAIN);
    break;
  default:
    ret = AVERROR_EOF;
    break;
  }

  if (ret == 0)
  {
    ret = av_new_packet(packet, mediaPacket->GetBuffer()->GetBufferOccupiedSpace());

    if (ret == 0)
    {
      mediaPacket->GetBuffer()->CopyFromBuffer(packet->data, mediaPacket->GetBuffer()->GetBufferOccupiedSpace());

      packet->pts = (mediaPacket->GetPresentationTimestamp() != MEDIA_PACKET_PRESENTATION_TIMESTAMP_UNDEFINED) ? mediaPacket->GetPresentationTimestampInDirectShowTimeUnits() : AV_NOPTS_VALUE;
      packet->flags |= (mediaPacket->IsDiscontinuity() | caller->IsSetFlags(PACKET_INPUT_FORMAT_FLAG_DISCONTINUITY)) ? AV_PKT_FLAG_CORRUPT : 0;

      caller->internalFlags &= ~PACKET_INPUT_FORMAT_FLAG_DISCONTINUITY;
    }
  }

  FREE_MEM_CLASS(mediaPacket);

  return ret;
}

int CPacketInputFormat::Seek(AVFormatContext *formatContext, int stream_index, int64_t timestamp, int flags)
{
  CPacketInputFormat *caller = static_cast<CPacketInputFormat *>(formatContext->iformat);

  return -1;
}

int CPacketInputFormat::StreamRead(void *opaque, uint8_t *buf, int buf_size)
{
  CPacketInputFormat *caller = static_cast<CPacketInputFormat *>(opaque);

  int result = caller->demuxer->StreamReadPosition(caller->streamIoContextBufferPosition, buf, buf_size, STREAM_PACKAGE_DATA_REQUEST_FLAG_NONE);

  if (result > 0)
  {
    caller->streamIoContextBufferPosition += result;
  }

  return result;
}

int64_t CPacketInputFormat::StreamSeek(void *opaque,  int64_t offset, int whence)
{
  CPacketInputFormat *caller = static_cast<CPacketInputFormat *>(opaque);

  int64_t result = 0;
  bool resultSet = false;

  if (whence == SEEK_SET)
  {
    caller->streamIoContextBufferPosition = offset;
  }
  else if (whence == SEEK_CUR)
  {
    caller->streamIoContextBufferPosition += offset;
  }
  /*else if (whence == SEEK_END)
  {
    caller->streamIoContextBufferPosition = total - offset;
  }
  else if (whence == AVSEEK_SIZE)
  {
    result = total;
    resultSet = true;
  }*/
  else
  {
    result = E_INVALIDARG;
    resultSet = true;
  }

  if (!resultSet)
  {
    result = caller->streamIoContextBufferPosition;
    resultSet = true;
  }

  return result;
}
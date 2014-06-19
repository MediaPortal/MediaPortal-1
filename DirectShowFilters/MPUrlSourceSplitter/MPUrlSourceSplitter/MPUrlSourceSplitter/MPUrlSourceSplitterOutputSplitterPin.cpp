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

#include "MPUrlSourceSplitterOutputSplitterPin.h"

#include "LockMutex.h"

#include "moreuuids.h"
#include "H264Nalu.h"

#pragma warning( push )
#pragma warning( disable : 4018 )
#pragma warning( disable : 4244 )
extern "C" {
#define AVCODEC_X86_MATHOPS_H
#include "libavcodec/get_bits.h"
}
#pragma warning( pop )

#define AAC_ADTS_HEADER_SIZE                                          7

#ifdef _DEBUG
#define MODULE_NAME                                               L"MPUrlSourceSplitterOutputSplitterPind"
#else
#define MODULE_NAME                                               L"MPUrlSourceSplitterOutputSplitterPin"
#endif

CMPUrlSourceSplitterOutputSplitterPin::CMPUrlSourceSplitterOutputSplitterPin(LPCWSTR pName, CBaseFilter *pFilter, CCritSec *pLock, HRESULT *phr, CLogger *logger, CParameterCollection *parameters, CMediaTypeCollection *mediaTypes, const wchar_t *containerFormat)
  : CMPUrlSourceSplitterOutputPin(pName, pFilter, pLock, phr, logger, parameters, mediaTypes)
{
  this->h264Buffer = NULL;
  this->h264PacketCollection = NULL;

  if ((phr != NULL) && (SUCCEEDED(*phr)))
  {
    this->h264Buffer = new COutputPinPacket(phr);
    this->h264PacketCollection = new COutputPinPacketCollection(phr);
    CHECK_POINTER_HRESULT(*phr, this->h264Buffer, *phr, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*phr, this->h264PacketCollection, *phr, E_OUTOFMEMORY);

    if (SUCCEEDED(*phr) && (containerFormat != NULL))
    {
      this->flags |= (wcscmp(L"mpegts", containerFormat) == 0) ? MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_CONTAINER_MPEG_TS : MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_NONE;
      this->flags |= (wcscmp(L"mpeg", containerFormat) == 0) ? MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_CONTAINER_MPEG : MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_NONE;
      this->flags |= (wcscmp(L"wtv", containerFormat) == 0) ? MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_CONTAINER_WTV : MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_NONE;
      this->flags |= (wcscmp(L"asf", containerFormat) == 0) ? MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_CONTAINER_ASF : MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_NONE;
      this->flags |= (wcscmp(L"ogg", containerFormat) == 0) ? MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_CONTAINER_OGG : MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_NONE;
      this->flags |= (wcscmp(L"matroska", containerFormat) == 0) ? MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_CONTAINER_MATROSKA : MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_NONE;
      this->flags |= (wcscmp(L"avi", containerFormat) == 0) ? MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_CONTAINER_AVI : MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_NONE;
      this->flags |= (wcscmp(L"mp4", containerFormat) == 0) ? MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_CONTAINER_MP4 : MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_NONE;
    }
  }

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_PIN_END_FORMAT, MODULE_NAME, METHOD_CONSTRUCTOR_NAME, this->m_pName));
}

CMPUrlSourceSplitterOutputSplitterPin::~CMPUrlSourceSplitterOutputSplitterPin(void)
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_PIN_START_FORMAT, MODULE_NAME, METHOD_DESTRUCTOR_NAME, this->m_pName));

  CAMThread::CallWorker(CMD_EXIT);
  CAMThread::Close();

  FREE_MEM_CLASS(this->h264Buffer);
  FREE_MEM_CLASS(this->h264PacketCollection);


  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_PIN_END_FORMAT, MODULE_NAME, METHOD_DESTRUCTOR_NAME, this->m_pName));
}

// CBaseOutputPin methods

HRESULT CMPUrlSourceSplitterOutputSplitterPin::DeliverBeginFlush()
{
  HRESULT result = __super::DeliverBeginFlush();

  // flush parser
  FREE_MEM_CLASS(this->h264Buffer);
  this->h264PacketCollection->Clear();

  return result;
}

HRESULT CMPUrlSourceSplitterOutputSplitterPin::QueuePacket(COutputPinPacket *packet, DWORD timeout)
{
  HRESULT result = S_OK;

  {
    CLockMutex lock(this->mediaPacketsLock, timeout);
    result = (lock.IsLocked()) ? S_OK : VFW_E_TIMEOUT;

    if (SUCCEEDED(result))
    {
      if (this->mediaTypeToSend != NULL)
      {
        packet->SetMediaType(CreateMediaType(this->mediaTypeToSend));
        FREE_MEM_CLASS(this->mediaTypeToSend);
      }

      if (packet->IsEndOfStream())
      {
        // add packet to output packet collection
        result = this->mediaPackets->Add(packet) ? result : E_OUTOFMEMORY;

        CHECK_CONDITION_EXECUTE(SUCCEEDED(result), this->flags |= MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_END_OF_STREAM);
      }
      else
      {
        // parse packet (if necessary)
        result = this->Parse(this->m_mt.subtype, packet);
      }
    }
  }

  return result;
}

/* get methods */

/* set methods */

/* other methods */

/* protected methods */

#define MOVE_TO_H264_START_CODE(b, e) while(b <= e-4 && !((*(DWORD *)b == 0x01000000) || ((*(DWORD *)b & 0x00FFFFFF) == 0x00010000))) b++; if((b <= e-4) && *(DWORD *)b == 0x01000000) b++;

HRESULT CMPUrlSourceSplitterOutputSplitterPin::Parse(GUID subType, COutputPinPacket *packet)
{
  HRESULT result = (packet != NULL) ? S_OK : E_INVALIDARG;

  if (SUCCEEDED(result) && (subType != this->mediaTypeSubType))
  {
    this->mediaTypeSubType = subType;

    FREE_MEM_CLASS(this->h264Buffer);
    this->h264PacketCollection->Clear();
  }

  if (SUCCEEDED(result))
  {
    if (packet->IsPacketParsed())
    {
      // add packet to output packet collection
      packet->SetLoadedToMemoryTime(GetTickCount());
      result = this->mediaPackets->Add(packet) ? result : E_OUTOFMEMORY;
    }
    else if (this->mediaTypeSubType == MEDIASUBTYPE_AVC1 &&
      (this->IsContainerMpegTs() || this->IsContainerMpeg() || this->IsContainerWtv() || this->IsContainerAsf() || ((this->IsContainerOgg() || this->IsContainerMatroska()) && packet->IsH264AnnexB())))
    {
      if (this->h264Buffer == NULL)
      {
        // initialize H264 Annex B buffer with current output pin packet data
        this->h264Buffer = new COutputPinPacket(&result);
        CHECK_POINTER_HRESULT(result, this->h264Buffer, result, E_OUTOFMEMORY);

        CHECK_CONDITION_HRESULT(result, this->h264Buffer->GetBuffer()->InitializeBuffer(packet->GetBuffer()->GetBufferSize()), result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          // copy packet data to H264 buffer
          this->h264Buffer->SetDemuxerId(packet->GetDemuxerId());
          this->h264Buffer->SetStreamPid(packet->GetStreamPid());
          this->h264Buffer->SetDiscontinuity(packet->IsDiscontinuity());
          this->h264Buffer->SetSyncPoint(packet->IsSyncPoint());
          this->h264Buffer->SetStartTime(packet->GetStartTime());
          this->h264Buffer->SetEndTime(packet->GetEndTime());
          this->h264Buffer->SetMediaType(packet->GetMediaType());

          // reset incoming packet data
          packet->SetDiscontinuity(false);
          packet->SetSyncPoint(false);
          packet->SetStartTime(COutputPinPacket::INVALID_TIME);
          packet->SetEndTime(COutputPinPacket::INVALID_TIME);
          packet->SetMediaType(NULL);
        }

        CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(this->h264Buffer));
      }

      // add packet data to H264 buffer (in case of error, no data are added)
      CHECK_CONDITION_HRESULT(result, this->h264Buffer->GetBuffer()->AddToBufferWithResize(packet->GetBuffer()) == packet->GetBuffer()->GetBufferOccupiedSpace(), result, E_OUTOFMEMORY);

      if (SUCCEEDED(result) && (this->h264Buffer->GetBuffer()->GetBufferOccupiedSpace() > 0))
      {
        unsigned int bufferSize = this->h264Buffer->GetBuffer()->GetBufferOccupiedSpace();
        ALLOC_MEM_DEFINE_SET(buffer, unsigned char, bufferSize, 0);
        CHECK_POINTER_HRESULT(result, buffer, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          this->h264Buffer->GetBuffer()->CopyFromBuffer(buffer, bufferSize);

          unsigned char *start = buffer;
          unsigned char *end = buffer + bufferSize;

          MOVE_TO_H264_START_CODE(start, end);

          unsigned int h264PacketCollectionCount = this->h264PacketCollection->Count();
          while (SUCCEEDED(result) && (start <= (end - 4)))
          {
            unsigned char *next = start + 1;
            MOVE_TO_H264_START_CODE(next, end);

            // end of buffer reached
            if (next >= (end - 4))
            {
              break;
            }

            unsigned int size = next - start;
            CH264Nalu nalu;
            nalu.SetBuffer(start, size, 0);

            COutputPinPacket *packetToCollection = new COutputPinPacket(&result);
            CHECK_POINTER_HRESULT(result, packetToCollection, result, E_OUTOFMEMORY);
            CHECK_CONDITION_HRESULT(result, packetToCollection->GetBuffer()->InitializeBuffer(this->h264Buffer->GetBuffer()->GetBufferOccupiedSpace()), result, E_OUTOFMEMORY);

            while (SUCCEEDED(result) && nalu.ReadNext())
            {
              unsigned int tempSize = nalu.GetDataLength() + 4;
              ALLOC_MEM_DEFINE_SET(temp, unsigned char, tempSize, 0);
              CHECK_POINTER_HRESULT(result, temp, result, E_OUTOFMEMORY);

              if (SUCCEEDED(result))
              {
                // write size of the NALU in big endian
                AV_WB32(temp, nalu.GetDataLength());
                memcpy(temp + 4, nalu.GetDataBuffer(), nalu.GetDataLength());

                result = (packetToCollection->GetBuffer()->AddToBufferWithResize(temp, tempSize) == tempSize) ? result : E_OUTOFMEMORY;
              }

              FREE_MEM(temp);
            }

            if (FAILED(result) || (packetToCollection->GetBuffer()->GetBufferOccupiedSpace() == 0))
            {
              // no data or error
              FREE_MEM_CLASS(packetToCollection);
              break;
            }

            if (SUCCEEDED(result))
            {
              // no error and we have some data

              packetToCollection->SetDemuxerId(this->h264Buffer->GetDemuxerId());
              packetToCollection->SetStreamPid(this->h264Buffer->GetStreamPid());
              packetToCollection->SetDiscontinuity(this->h264Buffer->IsDiscontinuity());
              packetToCollection->SetSyncPoint(this->h264Buffer->IsSyncPoint());
              packetToCollection->SetStartTime(this->h264Buffer->GetStartTime());
              packetToCollection->SetEndTime(this->h264Buffer->GetEndTime());
              packetToCollection->SetMediaType(this->h264Buffer->GetMediaType());

              this->h264Buffer->SetDiscontinuity(false);
              this->h264Buffer->SetSyncPoint(false);
              this->h264Buffer->SetStartTime(COutputPinPacket::INVALID_TIME);
              this->h264Buffer->SetEndTime(COutputPinPacket::INVALID_TIME);
              this->h264Buffer->SetMediaType(NULL);

              // add to H264 packet collection
              result = this->h264PacketCollection->Add(packetToCollection) ? result : E_OUTOFMEMORY;

              if (SUCCEEDED(result))
              {
                if (packet->GetStartTime() != COutputPinPacket::INVALID_TIME)
                {
                  this->h264Buffer->SetStartTime(packet->GetStartTime());
                  this->h264Buffer->SetEndTime(packet->GetEndTime());

                  packet->SetStartTime(COutputPinPacket::INVALID_TIME);
                  packet->SetEndTime(COutputPinPacket::INVALID_TIME);
                }

                if (packet->IsDiscontinuity())
                {
                  this->h264Buffer->SetDiscontinuity(true);
                  packet->SetDiscontinuity(false);
                }

                if (packet->IsSyncPoint())
                {
                  this->h264Buffer->SetSyncPoint(true);
                  packet->SetSyncPoint(false);
                }

                if (this->h264Buffer->GetMediaType() != NULL)
                {
                  DeleteMediaType(this->h264Buffer->GetMediaType());
                  this->h264Buffer->SetMediaType(NULL);
                }

                this->h264Buffer->SetMediaType(packet->GetMediaType());
                packet->SetMediaType(NULL);
              }
            }

            CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(packetToCollection));

            start = next;
          }

          if (SUCCEEDED(result))
          {
            if (start > buffer)
            {
              this->h264Buffer->GetBuffer()->RemoveFromBufferAndMove(start - buffer);
            }
          }
          else
          {
            // error occured, clean this->h264PacketCollection to previous state
            while (this->h264PacketCollection->Count() != h264PacketCollectionCount)
            {
              this->h264PacketCollection->Remove(this->h264PacketCollection->Count() - 1);
            }
          }
        }

        FREE_MEM(buffer);
      }

      // if no error, delete processed packet
      CHECK_CONDITION_EXECUTE(SUCCEEDED(result), FREE_MEM_CLASS(packet));

      // process H264 packet collection and queue output packets if possible

      if (SUCCEEDED(result))
      {
        // output packet is processed
        // any error in next code is ignored, this->h264PacketCollection will be processed with next Parse() call

        unsigned int nextPacketIndex = 0;
        do
        {
          REFERENCE_TIME packetStart = COutputPinPacket::INVALID_TIME;
          REFERENCE_TIME packetEnd = COutputPinPacket::INVALID_TIME;
          nextPacketIndex = 0;

          // skip first packet
          for (unsigned int i = 1; (SUCCEEDED(result) && (i < this->h264PacketCollection->Count())); i++)
          {
            COutputPinPacket *temp = this->h264PacketCollection->GetItem(i);
            ALLOC_MEM_DEFINE_SET(buffer, unsigned char, temp->GetBuffer()->GetBufferOccupiedSpace(), 0);
            CHECK_POINTER_HRESULT(result, buffer, result, E_OUTOFMEMORY);

            if (SUCCEEDED(result))
            {
              temp->GetBuffer()->CopyFromBuffer(buffer, temp->GetBuffer()->GetBufferOccupiedSpace());

              if ((buffer[4] & 0x1F) == 0x09)
              {
                this->SetHasAccessUnitDelimiters(true);
              }

              if (((buffer[4] & 0x1F) == 0x09) || ((!this->HasAccessUnitDelimiters()) && (temp->GetStartTime() != COutputPinPacket::INVALID_TIME)))
              {
                nextPacketIndex = i;

                if ((temp->GetStartTime() == COutputPinPacket::INVALID_TIME) && (packetStart != COutputPinPacket::INVALID_TIME))
                {
                  temp->SetStartTime(packetStart);
                  temp->SetEndTime(packetEnd);
                }

                break;
              }

              if (packetStart == COutputPinPacket::INVALID_TIME)
              {
                packetStart = temp->GetStartTime();
                packetEnd = temp->GetEndTime();
              }
            }

            FREE_MEM(buffer);
          }

          if (SUCCEEDED(result) && (nextPacketIndex != 0))
          {
            COutputPinPacket *queuePacket = new COutputPinPacket(&result);
            CHECK_POINTER_HRESULT(result, queuePacket, result, E_OUTOFMEMORY);

            if (SUCCEEDED(result))
            {
              // count needed memory for output packet
              unsigned int neededSpace = 0;
              for (unsigned int i = 0; (SUCCEEDED(result) && (i < nextPacketIndex)); i++)
              {
                COutputPinPacket *temp = this->h264PacketCollection->GetItem(i);

                neededSpace += temp->GetBuffer()->GetBufferOccupiedSpace();
              }

              // copy data from first packet in H264 collection
              COutputPinPacket *firstPacket = this->h264PacketCollection->GetItem(0);
              CHECK_CONDITION_HRESULT(result, queuePacket->GetBuffer()->InitializeBuffer(neededSpace), result, E_OUTOFMEMORY);

              if (SUCCEEDED(result))
              {
                queuePacket->GetBuffer()->AddToBufferWithResize(firstPacket->GetBuffer());

                queuePacket->SetStartTime(firstPacket->GetStartTime());
                queuePacket->SetEndTime(firstPacket->GetEndTime());
                queuePacket->SetDemuxerId(firstPacket->GetDemuxerId());
                queuePacket->SetStreamPid(firstPacket->GetStreamPid());
                queuePacket->SetMediaType(firstPacket->GetMediaType());
                // clear media type in first packet to avoid crash in freeing memory
                firstPacket->SetMediaType(NULL);
                queuePacket->SetFlags(firstPacket->GetFlags());
              }
            }

            // copy data from H264 packet collection until next packet index
            for (unsigned int i = 1; (SUCCEEDED(result) && (i < nextPacketIndex)); i++)
            {
              COutputPinPacket *temp = this->h264PacketCollection->GetItem(i);
              queuePacket->GetBuffer()->AddToBufferWithResize(temp->GetBuffer());
            }

            // add packet to output collection
            queuePacket->SetLoadedToMemoryTime(GetTickCount());
            CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = this->mediaPackets->Add(queuePacket) ? result : E_OUTOFMEMORY);

            // delete processed H264 packets
            for (unsigned int i = 0; (SUCCEEDED(result) && (i < nextPacketIndex)); i++)
            {
              this->h264PacketCollection->Remove(0);
            }

            CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(queuePacket));
          }
        }
        while (nextPacketIndex != 0);

        // ignore error, output packet is already processed
        result = S_OK;
      }
    }
    else if (this->mediaTypeSubType == MEDIASUBTYPE_HDMVSUB)
    {
      unsigned int bufferSize = packet->GetBuffer()->GetBufferOccupiedSpace();
      ALLOC_MEM_DEFINE_SET(buffer, unsigned char, bufferSize, 0);
      CHECK_POINTER_HRESULT(result, buffer, result, E_OUTOFMEMORY);

      unsigned int pgsBufferOccupied = 0;
      ALLOC_MEM_DEFINE_SET(pgsBuffer, unsigned char, bufferSize, 0);
      CHECK_POINTER_HRESULT(result, pgsBuffer, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        packet->GetBuffer()->CopyFromBuffer(buffer, bufferSize);

        unsigned char *bufferStart = buffer;
        unsigned char *bufferEnd = buffer + bufferSize;
        unsigned char segmentType;
        unsigned int segmentLength;

        if (bufferSize < 3)
        {
          // too short PGS packet
          // if no error, flag packet as parsed
          // if error occur while adding to collection, next time it will be directly added to collection without parsing
          packet->SetFlags(packet->GetFlags() | OUTPUT_PIN_PACKET_FLAG_PACKET_PARSED);

          // add packet to output packet collection
          packet->SetLoadedToMemoryTime(GetTickCount());
          result = this->mediaPackets->Add(packet) ? result : E_OUTOFMEMORY;
        }
        else
        {
          while ((bufferStart + 3) < bufferEnd)
          {
            const unsigned char *segmentStart = bufferStart;
            const unsigned int segmentBufferLength = bufferEnd - bufferStart;

            segmentType = AV_RB8(bufferStart);
            segmentLength = AV_RB16(bufferStart + 1);

            if (segmentLength > (segmentBufferLength - 3))
            {
              // segment length is bigger then input buffer
              segmentLength = segmentBufferLength - 3;
            }

            bufferStart += 3;

            // presentation segment
            if ((segmentType == 0x16) && (segmentLength > 10))
            {
              // segment layout
              // 2 bytes width
              // 2 bytes height
              // 1 unknown byte
              // 2 bytes id
              // 1 byte composition state (0x00 = normal, 0x40 = ACQU_POINT (?), 0x80 = epoch start (new frame), 0xC0 = epoch continue)
              // 2 unknown bytes
              // 1 byte object number

              unsigned char objectNumber = bufferStart[10];

              if (objectNumber == 0)
              {
                this->SetPGSDropState(false);
              }
              else if (segmentLength >= 0x13)
              {
                // 1 byte window_id
                // 1 byte object_cropped_flag: 0x80, forced_on_flag = 0x040, 6bit reserved
                unsigned char forcedFlag = bufferStart[14];
                this->SetPGSDropState(!(forcedFlag & 0x40));
                // 2 bytes x
                // 2 bytes y
                // total length = 19 bytes
              }
            }

            if (!this->IsPGSDropState())
            {
              memcpy(pgsBuffer + pgsBufferOccupied, segmentStart, segmentLength + 3);
              pgsBufferOccupied += segmentLength + 3;
            }

            bufferStart += segmentLength;
          }

          if (pgsBufferOccupied > 0)
          {
            packet->GetBuffer()->ClearBuffer();
            packet->GetBuffer()->AddToBuffer(pgsBuffer, pgsBufferOccupied);
          }
          else
          {
            FREE_MEM_CLASS(packet);
          }
        }
      }

      FREE_MEM(buffer);
      FREE_MEM(pgsBuffer);
    }
    else if (this->mediaTypeSubType == MEDIASUBTYPE_HDMV_LPCM_AUDIO)
    {
      // add packet to output packet collection, if successful, change it's data
      packet->SetLoadedToMemoryTime(GetTickCount());
      result = this->mediaPackets->Add(packet) ? result : E_OUTOFMEMORY;

      CHECK_CONDITION_EXECUTE(SUCCEEDED(result), packet->GetBuffer()->RemoveFromBuffer(4));
    }
    else if (packet->IsPacketMovText())
    {
      unsigned int bufferSize = packet->GetBuffer()->GetBufferOccupiedSpace();

      if (bufferSize > 2)
      {
        ALLOC_MEM_DEFINE_SET(buffer, unsigned char, bufferSize, 0);
        CHECK_POINTER_HRESULT(result, buffer, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          packet->GetBuffer()->CopyFromBuffer(buffer, bufferSize);
          unsigned int size = (buffer[0] << 8) | buffer[1];

          if (size <= (bufferSize - 2))
          {
            packet->GetBuffer()->ClearBuffer();
            packet->GetBuffer()->AddToBuffer(buffer + 2, size);

            // if no error, flag packet as parsed
            // if error occur while adding to collection, next time it will be directly added to collection without parsing
            packet->SetFlags(packet->GetFlags() | OUTPUT_PIN_PACKET_FLAG_PACKET_PARSED);

            // add packet to output packet collection
            packet->SetLoadedToMemoryTime(GetTickCount());
            result = this->mediaPackets->Add(packet) ? result : E_OUTOFMEMORY;
          }
          else
          {
            FREE_MEM_CLASS(packet);
          }
        }

        FREE_MEM(buffer);
      }
      else
      {
        FREE_MEM_CLASS(packet);
      }
    }
    else if (this->mediaTypeSubType == MEDIASUBTYPE_AAC && ((!this->IsContainerMatroska()) && (!this->IsContainerMp4())))
    {
      unsigned int bufferSize = packet->GetBuffer()->GetBufferOccupiedSpace();
      ALLOC_MEM_DEFINE_SET(buffer, unsigned char, bufferSize, 0);
      CHECK_POINTER_HRESULT(result, buffer, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        packet->GetBuffer()->CopyFromBuffer(buffer, bufferSize);

        GetBitContext gb;
        init_get_bits(&gb, buffer, AAC_ADTS_HEADER_SIZE * 8);

        // check if its really ADTS
        if (get_bits(&gb, 12) == 0xFFF)
        {
          skip_bits1(&gb);              /* id */
          skip_bits(&gb, 2);            /* layer */
          int crc_abs = get_bits1(&gb); /* protection_absent */

          packet->GetBuffer()->RemoveFromBuffer(AAC_ADTS_HEADER_SIZE + 2*!crc_abs);
        }

        // if no error, flag packet as parsed
        // if error occur while adding to collection, next time it will be directly added to collection without parsing
        packet->SetFlags(packet->GetFlags() | OUTPUT_PIN_PACKET_FLAG_PACKET_PARSED);
      }

      FREE_MEM(buffer);

      packet->SetLoadedToMemoryTime(GetTickCount());
      CHECK_CONDITION_HRESULT(result, this->mediaPackets->Add(packet), result, E_OUTOFMEMORY);
    }
    else
    {
      // add packet to output packet collection
      packet->SetLoadedToMemoryTime(GetTickCount());
      result = this->mediaPackets->Add(packet) ? result : E_OUTOFMEMORY;
    }
  }

  return result;
}

void CMPUrlSourceSplitterOutputSplitterPin::SetHasAccessUnitDelimiters(bool hasAccessUnitDelimiters)
{
  this->flags &= ~MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_HAS_ACCESS_UNIT_DELIMITERS;
  this->flags |= (hasAccessUnitDelimiters) ? MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_HAS_ACCESS_UNIT_DELIMITERS : MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_NONE;
}

void CMPUrlSourceSplitterOutputSplitterPin::SetPGSDropState(bool pgsDropState)
{
  this->flags &= ~MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_PGS_DROP_STATE;
  this->flags |= (pgsDropState) ? MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_PGS_DROP_STATE : MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_NONE;
}

bool CMPUrlSourceSplitterOutputSplitterPin::IsContainerMpegTs(void)
{
  return this->IsSetFlags(MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_CONTAINER_MPEG_TS);
}

bool CMPUrlSourceSplitterOutputSplitterPin::IsContainerMpeg(void)
{
  return this->IsSetFlags(MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_CONTAINER_MPEG);
}

bool CMPUrlSourceSplitterOutputSplitterPin::IsContainerWtv(void)
{
  return this->IsSetFlags(MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_CONTAINER_WTV);
}

bool CMPUrlSourceSplitterOutputSplitterPin::IsContainerAsf(void)
{
  return this->IsSetFlags(MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_CONTAINER_ASF);
}

bool CMPUrlSourceSplitterOutputSplitterPin::IsContainerOgg(void)
{
  return this->IsSetFlags(MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_CONTAINER_OGG);
}

bool CMPUrlSourceSplitterOutputSplitterPin::IsContainerMatroska(void)
{
  return this->IsSetFlags(MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_CONTAINER_MATROSKA);
}

bool CMPUrlSourceSplitterOutputSplitterPin::IsContainerAvi(void)
{
  return this->IsSetFlags(MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_CONTAINER_AVI);
}

bool CMPUrlSourceSplitterOutputSplitterPin::IsContainerMp4(void)
{
  return this->IsSetFlags(MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_CONTAINER_MP4);
}

bool CMPUrlSourceSplitterOutputSplitterPin::HasAccessUnitDelimiters(void)
{
  return this->IsSetFlags(MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_HAS_ACCESS_UNIT_DELIMITERS);
}

bool CMPUrlSourceSplitterOutputSplitterPin::IsPGSDropState(void)
{
  return this->IsSetFlags(MP_URL_SOURCE_SPLITTER_OUTPUT_SPLITTER_PIN_FLAG_PGS_DROP_STATE);
}

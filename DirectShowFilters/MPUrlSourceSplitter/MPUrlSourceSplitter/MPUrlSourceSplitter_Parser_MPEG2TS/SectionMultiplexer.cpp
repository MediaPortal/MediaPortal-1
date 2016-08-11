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

#include "stdafx.h"

#include "SectionMultiplexer.h"
#include "TsPacketCollection.h"
#include "ProgramSpecificInformationPacket.h"
#include "TsPacketConstants.h"

CSectionMultiplexer::CSectionMultiplexer(HRESULT *result, unsigned int pid, unsigned int requestedPid, unsigned int continuityCounter)
  : CFlags()
{
  this->pid = pid;
  this->requestedPid = requestedPid;
  this->continuityCounter = continuityCounter;
  this->streamFragmentContexts = NULL;
  this->sections = NULL;
  this->sectionPayloadCount = UINT_MAX;
  this->currentPacket = NULL;

  if ((result != NULL) && SUCCEEDED(*result))
  {
    this->streamFragmentContexts = new CMpeg2tsStreamFragmentContextCollection(result);
    this->sections = new CSectionCollection(result);

    CHECK_POINTER_HRESULT(*result, this->streamFragmentContexts, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->sections, *result, E_OUTOFMEMORY);
  }
}

CSectionMultiplexer::~CSectionMultiplexer()
{
  FREE_MEM_CLASS(this->streamFragmentContexts);
  FREE_MEM_CLASS(this->sections);
  FREE_MEM_CLASS(this->currentPacket);
}

/* get methods */

unsigned int CSectionMultiplexer::GetPID(void)
{
  return this->pid;
}

unsigned int CSectionMultiplexer::GetRequestedPID(void)
{
  return this->requestedPid;
}

unsigned int CSectionMultiplexer::GetContinuityCounter(void)
{
  return this->continuityCounter;
}

/* set methods */

/* other methods */

bool CSectionMultiplexer::AddStreamFragmentContext(CMpeg2tsStreamFragment *streamFragment, unsigned int packetIndex, unsigned int sectionPayloads)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, streamFragment);

  if (SUCCEEDED(result))
  {
    // it is more probable that requested stream fragment will be on end of collection

    CMpeg2tsStreamFragmentContext *context = NULL;

    for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->streamFragmentContexts->Count())); i++)
    {
      CMpeg2tsStreamFragmentContext *temp = this->streamFragmentContexts->GetItem(this->streamFragmentContexts->Count() - 1 - i);

      if (temp->GetFragment() == streamFragment)
      {
        // found stream fragment and found context
        context = temp;
        break;
      }
    }

    if (SUCCEEDED(result) && (context == NULL))
    {
      // no context found, create new one
      context = new CMpeg2tsStreamFragmentContext(&result, streamFragment);
      CHECK_POINTER_HRESULT(result, context, result, E_OUTOFMEMORY);

      CHECK_CONDITION_HRESULT(result, this->streamFragmentContexts->Add(context), result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(context));
    }

    if (SUCCEEDED(result))
    {
      // we have context, context is added only once per stream fragment

      CTsPacketContext *packetContext = new CTsPacketContext(&result);
      CHECK_POINTER_HRESULT(result, packetContext, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        packetContext->SetTsPacketIndex(packetIndex);
        packetContext->SetSectionPayloadCount(sectionPayloads);
      }

      CHECK_CONDITION_HRESULT(result, context->GetPacketContexts()->Add(packetContext), result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(packetContext));
    }

    if (SUCCEEDED(result))
    {
      // increase multiplexer reference count
      this->IncreaseReferenceCount(streamFragment);
    }
  }

  return SUCCEEDED(result);
}

HRESULT CSectionMultiplexer::MultiplexSections(void)
{
  HRESULT result = S_OK;

  if (this->sections->Count() != 0)
  {
    // there are some section ready to split to stream fragments
    // try to split sections into packets by packet contexts (maintain count of sections in packet)
    // whole section is splitted, there are not remaining data from previous section

    unsigned int streamFragmentContextIndex = 0;
    unsigned int packetContextIndex = 0;

    CTsPacketCollection *packetCollection = new CTsPacketCollection(&result);
    CHECK_POINTER_HRESULT(result, packetCollection, result, E_OUTOFMEMORY);

    for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->sections->Count())); i++)
    {
      CSection *section = this->sections->GetItem(i);
      unsigned int processedSectionData = 0;

      while (SUCCEEDED(result) && (processedSectionData < section->GetSectionSize()))
      {
        if (this->currentPacket == NULL)
        {
          // need to create PSI packet

          CMpeg2tsStreamFragmentContext *streamFragmentContext = this->streamFragmentContexts->GetItem(streamFragmentContextIndex);
          CTsPacketContext *packetContext = streamFragmentContext->GetPacketContexts()->GetItem(packetContextIndex);
          this->sectionPayloadCount = (this->sectionPayloadCount == UINT_MAX) ? packetContext->GetSectionPayloadCount() : this->sectionPayloadCount;

          bool payloadUnitStartFlag = false;

          if ((processedSectionData != 0) && (packetContext->GetSectionPayloadCount() == 1))
          {
            // PSI packet without payload unit start flag
            payloadUnitStartFlag = false;
          }
          else if ((processedSectionData != 0) && (packetContext->GetSectionPayloadCount() > 1))
          {
            // PSI packet with payload unit start flag
            payloadUnitStartFlag = true;
          }
          else if (processedSectionData == 0)
          {
            // PSI packet with payload unit start flag
            payloadUnitStartFlag = true;
          }
          else
          {
            // this should not happen
            result = E_NOTIMPL;
          }

          // table ID for PSI packet is not necessary
          this->currentPacket = new CProgramSpecificInformationPacket(&result, this->requestedPid, 0);
          CHECK_POINTER_HRESULT(result, this->currentPacket, result, E_OUTOFMEMORY);

          if (SUCCEEDED(result))
          {
            this->currentPacket->SetAdaptationFieldControl(TS_PACKET_ADAPTATION_FIELD_CONTROL_ONLY_PAYLOAD);
            this->currentPacket->SetPayloadUnitStart(payloadUnitStartFlag);
            this->currentPacket->SetContinuityCounter(this->continuityCounter);

            this->continuityCounter++;
            this->continuityCounter &= TS_PACKET_HEADER_CONTINUITY_COUNTER_MASK;
          }

          CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(this->currentPacket));
        }

        if (SUCCEEDED(result))
        {
          unsigned int payloadSize = 0;

          result = this->currentPacket->ParseSectionData(section->GetSection() + processedSectionData, section->GetSectionSize() - processedSectionData, processedSectionData == 0, true, &payloadSize);

          if (SUCCEEDED(result))
          {
            CMpeg2tsStreamFragmentContext *streamFragmentContext = this->streamFragmentContexts->GetItem(streamFragmentContextIndex);

            if (payloadSize == 0)
            {
              // we finished with MPEG2 TS packet, it is full
              this->sectionPayloadCount = 0;
            }
            else
            {
              // decrease section payload count, we used one payload
              this->sectionPayloadCount--;
            }

            // if section payload count is 0 then move indexes (streamFragmentContextIndex or packetContextIndex)
            if (this->sectionPayloadCount == 0)
            {
              packetContextIndex++;
              if (packetContextIndex == streamFragmentContext->GetPacketContexts()->Count())
              {
                packetContextIndex = 0;
                streamFragmentContextIndex++;
              }
            }

            if ((payloadSize == 0) || (this->sectionPayloadCount == 0) || (!this->currentPacket->IsPayloadUnitStart()))
            {
              // PSI packet is full or can't contain new section, store it in packet collection and create new one
              CHECK_CONDITION_HRESULT(result, packetCollection->Add(this->currentPacket), result, E_OUTOFMEMORY);
              CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(this->currentPacket));

              // if current packet is added, reset current packet
              this->currentPacket = NULL;
            }

            this->sectionPayloadCount = (this->sectionPayloadCount == 0) ? UINT_MAX : this->sectionPayloadCount;

            processedSectionData += payloadSize;
          }
        }
      }
    }

    if (SUCCEEDED(result))
    {
      // processed all sections
      this->sections->Clear();

      // store TS packets into stream fragments
      unsigned int packetIndex = 0;

      while (SUCCEEDED(result) && (this->streamFragmentContexts->Count() > 0) && (packetIndex < packetCollection->Count()))
      {
        CMpeg2tsStreamFragmentContext *streamFragmentContext = this->streamFragmentContexts->GetItem(0);

        while (SUCCEEDED(result) && (streamFragmentContext->GetPacketContexts()->Count() > 0) && (packetIndex < packetCollection->Count()))
        {
          CTsPacketContext *packetContext = streamFragmentContext->GetPacketContexts()->GetItem(0);
          unsigned char *buffer = streamFragmentContext->GetFragment()->GetBuffer()->GetInternalBuffer();

          memcpy(buffer + packetContext->GetTsPacketIndex() * TS_PACKET_SIZE, packetCollection->GetItem(packetIndex++)->GetPacket(), TS_PACKET_SIZE);
          
          this->DecreaseReferenceCount(streamFragmentContext->GetFragment());
          streamFragmentContext->GetPacketContexts()->Remove(0);
        }

        if (streamFragmentContext->GetPacketContexts()->Count() == 0)
        {
          this->streamFragmentContexts->Remove(0);
        }
      }
    }

    FREE_MEM_CLASS(packetCollection);
  }

  return result;
}

HRESULT CSectionMultiplexer::FlushStreamFragmentContexts(void)
{
  HRESULT result = S_OK;

  CTsPacket *packet = CTsPacket::CreateNullPacket();
  CHECK_POINTER_HRESULT(result, packet, result, E_OUTOFMEMORY);

  if (SUCCEEDED(result))
  {
    while (SUCCEEDED(result) && (this->streamFragmentContexts->Count() > 0))
    {
      CMpeg2tsStreamFragmentContext *streamFragmentContext = this->streamFragmentContexts->GetItem(0);

      while (SUCCEEDED(result) && (streamFragmentContext->GetPacketContexts()->Count() > 0))
      {
        CTsPacketContext *packetContext = streamFragmentContext->GetPacketContexts()->GetItem(0);
        unsigned char *buffer = streamFragmentContext->GetFragment()->GetBuffer()->GetInternalBuffer();

        memcpy(buffer + packetContext->GetTsPacketIndex() * TS_PACKET_SIZE, packet->GetPacket(), TS_PACKET_SIZE);

        this->DecreaseReferenceCount(streamFragmentContext->GetFragment());
        streamFragmentContext->GetPacketContexts()->Remove(0);
      }

      if (streamFragmentContext->GetPacketContexts()->Count() == 0)
      {
        this->streamFragmentContexts->Remove(0);
      }
    }
  }

  // remove used MPEG2 TS packet, it is not needed more
  FREE_MEM_CLASS(packet);
  return result;
}

/* protected methods */

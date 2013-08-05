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

#include "RtpSource.h"
#include "ProtocolInterface.h"

#include <windows.h>
#include <stdio.h>
#include <shlobj.h>

// protocol implementation name
#define PROTOCOL_IMPLEMENTATION_NAME                                    _T("CMPIPTV_RTP")

// methods' names
#define METHOD_ADD_PACKET_NAME                                          _T("AddPacket()")
#define METHOD_GET_PACKET_DATA_NAME                                     _T("GetPacketData()")

RtpSource::RtpSource(CLogger *logger) 
{
  this->logger = logger;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);
  this->firstRtpPacket = NULL;
  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);
}

RtpSource::~RtpSource() 
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME);

  while (1)
  {
    if (!this->RemoveRtpPacket(0))
    {
      // error while removing RTP packet
      // this means that we are at the end
      break;
    }
  }
  
  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME);
}

bool RtpSource::AddPacket(RtpPacket *rtpPacket) 
{
  bool result = (rtpPacket != NULL);

  if (result)
  {
    result &= rtpPacket->IsRtpPacket();
  }

  if (result)
  {
    this->logger->Log(LOGGER_DATA, _T("%s: %s: sequence number: %u"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_ADD_PACKET_NAME, rtpPacket->GetSequenceNumber());

    // check where packet need to be inserted
    if (this->firstRtpPacket == NULL)
    {
      this->firstRtpPacket = rtpPacket;
    }
    else
    {
      unsigned int rtpPacketSequenceNumber = rtpPacket->GetSequenceNumber();

      // in RTP packets chain cannot be wrong packets

      RtpPacket *firstPacket = NULL;
      RtpPacket *secondPacket = this->firstRtpPacket;

      // go to the end of chain
      while (secondPacket != NULL)
      {
        firstPacket = secondPacket;
        secondPacket = secondPacket->GetNextPacket();
      }

      // now go from end of chain to start of chain
      while (1)
      {
        bool between = true;

        if ((firstPacket != NULL) && (secondPacket != NULL))
        {
          between &= this->IsPacketBetween(rtpPacket, firstPacket, secondPacket);
        }
        else if (firstPacket != NULL)
        {
          // rtpPacket is after first packet ?
          between &= (this->PacketSequenceNumberDifference(firstPacket, rtpPacket) >= 0);
        }
        else if (secondPacket != NULL)
        {
          // rtpPacket is before second packet ?
          between &= (this->PacketSequenceNumberDifference(rtpPacket, secondPacket) <= 0);
        }

        if (between)
        {
          // got combination of two packets
          break;
        }

        secondPacket = firstPacket;
        if (firstPacket != NULL)
        {
          firstPacket = firstPacket->GetPreviousPacket();
        }
      }

      if ((firstPacket == NULL) && (secondPacket != NULL))
      {
        // packet have to be added before this->firstRtpPacket
        this->firstRtpPacket->SetPreviousPacket(rtpPacket);
        rtpPacket->SetNextPacket(this->firstRtpPacket);
        rtpPacket->SetPreviousPacket(NULL);
        this->firstRtpPacket = rtpPacket;
      }
      else if ((firstPacket != NULL) && (secondPacket != NULL))
      {
        // packet have to be added between first and second packet
        firstPacket->SetNextPacket(rtpPacket);
        secondPacket->SetPreviousPacket(rtpPacket);
        rtpPacket->SetPreviousPacket(firstPacket);
        rtpPacket->SetNextPacket(secondPacket);
      }
      else if ((firstPacket != NULL) && (secondPacket == NULL))
      {
        // packet have to be added at the end (after first packet)
        firstPacket->SetNextPacket(rtpPacket);
        rtpPacket->SetPreviousPacket(firstPacket);
        rtpPacket->SetNextPacket(NULL);
      }
    }
  }

  return result;
}

bool RtpSource::RemoveRtpPacket(int index)
{
  bool result = false;

  RtpPacket *packetToRemove = this->GetInternalRtpPacket(index);

  if (packetToRemove != NULL)
  {
    result = this->RemoveInternalRtpPacket(packetToRemove);
  }

  return result;
}

bool RtpSource::RemoveRtpPacket(unsigned int sequenceNumber)
{
  bool result = false;

  RtpPacket *packetToRemove = this->GetInternalRtpPacket(sequenceNumber);
  if (packetToRemove != NULL)
  {
    result = this->RemoveInternalRtpPacket(packetToRemove);
  }

  return result;
}

bool RtpSource::RemoveInternalRtpPacket(RtpPacket *rtpPacket)
{
  bool result = false;

  if (rtpPacket != NULL)
  {
    RtpPacket *previousRtpPacket = rtpPacket->GetPreviousPacket();;
    RtpPacket *nextRtpPacket = rtpPacket->GetNextPacket();

    if (previousRtpPacket != NULL)
    {
      // next RTP packet can be NULL, in that case previous RTP packet will be last
      previousRtpPacket->SetNextPacket(nextRtpPacket);
    }
    else
    {
      // first packet will be deleted, first packet need to be moved to next packet
      // if next RTP packet is NULL then packet to delete was one and only and this->firstRtpPacket will be set to NULL
      this->firstRtpPacket = nextRtpPacket;
    }

    if (nextRtpPacket != NULL)
    {
      // previous RTP packet can be NULL, in that case next RTP packet will be first
      nextRtpPacket->SetPreviousPacket(previousRtpPacket);
    }

    delete rtpPacket;
    result = true;
  }

  return result;
}

RtpPacket *RtpSource::GetInternalRtpPacket(int index)
{
  RtpPacket *result = NULL;
  if ((this->firstRtpPacket != NULL) && (index >= 0))
  {
    RtpPacket *currentRtpPacket = this->firstRtpPacket;
    for (int i = 0; ((i < index) && (currentRtpPacket != NULL)); i++)
    {
      currentRtpPacket = currentRtpPacket->GetNextPacket();
    }

    if (currentRtpPacket != NULL)
    {
      result = currentRtpPacket;
    }
  }

  return result;
}

RtpPacket *RtpSource::GetInternalRtpPacket(unsigned int sequenceNumber)
{
  RtpPacket *result = NULL;

  RtpPacket *currentRtpPacket = this->firstRtpPacket;
  while ((currentRtpPacket != NULL) && (result == NULL))
  {
    if (currentRtpPacket->GetSequenceNumber() == sequenceNumber)
    {
      result = currentRtpPacket;
    }
    currentRtpPacket = currentRtpPacket->GetNextPacket();
  }

  return result;
}

RtpPacket *RtpSource::GetRtpPacket(int index)
{
  RtpPacket *result = this->GetInternalRtpPacket(index);

  if (result != NULL)
  {
    result = result->Clone();
  }

  return result;
}

RtpPacket *RtpSource::GetRtpPacket(unsigned int sequenceNumber)
{
  RtpPacket *result = this->GetInternalRtpPacket(sequenceNumber);

  if (result != NULL)
  {
    result = result->Clone();
  }

  return result;
}

bool RtpSource::IsPacketBetween(RtpPacket *currentPacket, RtpPacket *previousPacket, RtpPacket *nextPacket)
{
  if ((currentPacket == NULL) || (previousPacket == NULL) || (nextPacket == NULL))
  {
    return false;
  }

  bool between = true;

  between &= (this->PacketSequenceNumberDifference(previousPacket, currentPacket) >= 0);
  between &= (this->PacketSequenceNumberDifference(currentPacket, nextPacket) >= 0);

  return between;
}

bool RtpSource::IsSequenceContinuous(unsigned int firstSequenceNumber, unsigned int secondSequenceNumber)
{
  return (((firstSequenceNumber + 1) == secondSequenceNumber) || 
          ((firstSequenceNumber == RTP_PACKET_MAXIMUM_SEQUENCE_NUMBER) && (secondSequenceNumber == 0)));
}

unsigned int RtpSource::GetPacketData(char* buffer, unsigned int length, unsigned int *firstSequenceNumber, unsigned int *lastSequenceNumber, bool getUncontinousPackets) 
{
  unsigned int result = UINT_MAX;
  bool setFirstSequenceNumber = false;

  if ((this->firstRtpPacket != NULL) && (firstSequenceNumber != NULL) && (lastSequenceNumber != NULL))
  {
    result = 0;
    bool finishWork = false;
    RtpPacket *currentRtpPacket = this->firstRtpPacket;

    while (!finishWork)
    {
      unsigned int freeSpace = length - result;
      finishWork = (currentRtpPacket->GetDataLength() >= freeSpace);
      if (!finishWork)
      {
        // in buffer is enough free space for data from first RTP packet
        int copiedDataLength = currentRtpPacket->GetData(buffer + result, freeSpace);
        if (copiedDataLength >= 0)
        {
          // no error occured
          result += copiedDataLength;
          if (!setFirstSequenceNumber)
          {
            *firstSequenceNumber = currentRtpPacket->GetSequenceNumber();
            setFirstSequenceNumber = true;
          }
          else
          {
            *lastSequenceNumber = currentRtpPacket->GetSequenceNumber();
          }
        }
        // if error occured we skip this RTP packet

        if (currentRtpPacket->GetNextPacket() != NULL)
        {
          // check if next packet is continous
          bool isContinous = this->IsSequenceContinuous(currentRtpPacket->GetSequenceNumber(), currentRtpPacket->GetNextPacket()->GetSequenceNumber());
          if (!isContinous)
          {
            this->logger->Log(LOGGER_WARNING, _T("%s: %s: got uncontinous packet, last sequence number: %u"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_GET_PACKET_DATA_NAME, currentRtpPacket->GetSequenceNumber());
          }

          finishWork |= ((!getUncontinousPackets) && (!isContinous));
        }

        finishWork |= (currentRtpPacket->GetNextPacket() == NULL);

        if (!finishWork)
        {
          // move to next packet
          currentRtpPacket = currentRtpPacket->GetNextPacket();
        }
      }
    }
  }

  return result;
}

unsigned int RtpSource::GetAndRemovePacketData(char* buffer, unsigned int length, unsigned int *firstSequenceNumber, unsigned int *lastSequenceNumber, bool getUncontinousPackets) 
{
  unsigned int result = this->GetPacketData(buffer, length, firstSequenceNumber, lastSequenceNumber, getUncontinousPackets);

  if (result != UINT_MAX)
  {
    // remove packets from chain
    unsigned int removedDataLength = 0;
    while (result != removedDataLength)
    {
      if (this->firstRtpPacket == NULL)
      {
        // this should not happen but for sure
        break;
      }

      unsigned int packetDataLength = this->firstRtpPacket->GetDataLength();
      if (packetDataLength != UINT_MAX)
      {
        removedDataLength += packetDataLength;
      }

      // in any case remove first packet
      this->RemoveRtpPacket(0);
    }
  }

  return result;
}

bool RtpSource::IsRtpPacket(char *buffer, unsigned int length)
{
  RtpPacket *rtpPacket = new RtpPacket(buffer, length, NULL, NULL);
  bool isRtpPacket = rtpPacket->IsRtpPacket();
  delete rtpPacket;

  return isRtpPacket;
}

bool RtpSource::ProcessPacket(char *buffer, unsigned int length) 
{
  RtpPacket *rtpPacket = new RtpPacket(buffer, length, NULL, NULL);
  if (!rtpPacket->IsRtpPacket())
  {
    // some problem occured
    return false;
  }
  return (this->AddPacket(rtpPacket));
}

int RtpSource::PacketSequenceNumberDifference(RtpPacket *firstPacket, RtpPacket *secondPacket)
{
  int result = INT_MAX;

  if ((firstPacket != NULL) && (secondPacket != NULL))
  {
    unsigned int firstSequenceNumber = firstPacket->GetSequenceNumber();
    unsigned int secondSequenceNumber = secondPacket->GetSequenceNumber();

    if ((firstSequenceNumber != UINT_MAX) && (secondSequenceNumber != UINT_MAX))
    {
      result = 0;

      if (min((firstSequenceNumber - secondSequenceNumber) & RTP_PACKET_MAXIMUM_SEQUENCE_NUMBER, (secondSequenceNumber - firstSequenceNumber) & RTP_PACKET_MAXIMUM_SEQUENCE_NUMBER) <= RTP_MAXIMUM_DIFFERENCE_SEQUENCE_NUMBER)
      {
        // can be a case that first SN is bigger than second SN
        // can be a case that second SN is after first SN (bigger in logical way) but less in number (e.g first SN is 0xFFFF, second SN is 0x0001)
        unsigned int diff1 = firstSequenceNumber - secondSequenceNumber;
        unsigned int diff2 = secondSequenceNumber + RTP_PACKET_MAXIMUM_SEQUENCE_NUMBER + 1 - firstSequenceNumber;

        // can be a case that second SN is bigger than first SN
        // can be a case that second SN is before first SN (lower in logical way) but higher in number (e.g first SN is 0x0001, second SN is 0xFFFF)
        unsigned int diff3 = secondSequenceNumber - firstSequenceNumber;
        unsigned int diff4 = firstSequenceNumber + RTP_PACKET_MAXIMUM_SEQUENCE_NUMBER + 1 - secondSequenceNumber;

        // the smallest difference is right, but still we need direction
        if ((diff1 < diff2) && (diff1 < diff3) && (diff1 < diff4))
        {
          // diff1 is lowest
          // first SN is bigger than second SN, direction is from second to first
          result = -(int)diff1;
        }
        else if ((diff2 < diff1) && (diff2 < diff3) && (diff2 < diff4))
        {
          // diff2 is lowest
          // second SN is bigger than first SN, direction is from first to second
          result = (int)diff2;
        }
        else if ((diff3 < diff1) && (diff3 < diff2) && (diff3 < diff4))
        {
          // diff3 is lowest
          // second SN is bigger than first SN, direction is from first to second
          result = (int)diff3;
        }
        else
        {
          // diff4 is lowest
          // first SN is bigger than second SN, direction is from second to first
          result = -(int)diff4;
        }
      }
      else
      {
        // direction is always from first SN to second SN
        result = min(firstSequenceNumber - secondSequenceNumber, secondSequenceNumber - firstSequenceNumber);
      }
    }
  }

  return result;
}

unsigned int RtpSource::GetPacketCount(void)
{
  unsigned int result = 0;
  RtpPacket *currentRtpPacket = this->firstRtpPacket;

  while (currentRtpPacket != NULL)
  {
    result++;
    currentRtpPacket = currentRtpPacket->GetNextPacket();
  }

  return result;
}

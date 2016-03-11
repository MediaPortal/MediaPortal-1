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

#include "PmtParser.h"
#include "ProtocolInterface.h"

PmtParser::PmtParser(Crc32 *crc32)
{
  this->packet = NULL;
  this->packetLength = 0;
  this->packetCrc32 = 0;
  this->crc32 = crc32;
  this->crc32created = false;
  this->streamDescriptions = new PmtStreamDescriptionCollection();

  if (this->crc32 == NULL)
  {
    this->crc32 = new Crc32();
    if (this->crc32 != NULL)
    {
      this->crc32created = true;
    }
  }
}

PmtParser::~PmtParser(void)
{
  if (this->crc32created && (this->crc32 != NULL))
  {
    delete this->crc32;
    this->crc32 = NULL;
  }
  if (this->streamDescriptions != NULL)
  {
    delete this->streamDescriptions;
    this->streamDescriptions = NULL;
  }
  FREE_MEM(packet);
}

bool PmtParser::SetPmtData(const unsigned char *data, unsigned int length)
{
  FREE_MEM(this->packet);
  if ((length > 0) && (data != NULL))
  {
    this->ParsePmtData(data, length);
  }

  return (this->packet != NULL);
}

unsigned char *PmtParser::GetPmtPacket()
{
  unsigned char *result = NULL;

  if ((this->packet != NULL) && (this->crc32 != NULL))
  {
    // compute packet length
    unsigned int length = this->packetLength;
    unsigned int count = this->streamDescriptions->Count();
    for (unsigned int i = 0; i < count; i++)
    {
      length += this->streamDescriptions->GetItem(i)->GetDescriptionLength();
    }

    // create new packet
    result = ALLOC_MEM_SET(result, unsigned char, DVB_PACKET_SIZE, 0xFF);
    if (result != NULL)
    {
      // copy data from current packet
      memcpy(result, this->packet, this->packetLength);
      unsigned int start = this->packetLength;
      for (unsigned int i = 0; i < count; i++)
      {
        PmtStreamDescription *description = this->streamDescriptions->GetItem(i);
        unsigned int descriptionLength = description->GetDescriptionLength();
        unsigned char *descriptionData = description->GetDescription();

        if (descriptionData != NULL)
        {
          memcpy(result + start, descriptionData, descriptionLength);
          start += descriptionLength;
        }
        FREE_MEM(descriptionData);
      }

      unsigned int computedCrc32 = this->crc32->Compute((char *)(result + 5), length - 5);

      result[start] = (computedCrc32 >> 24) & 0x000000FF;
      result[start + 1] = (computedCrc32 >> 16) & 0x000000FF;
      result[start + 2] = (computedCrc32 >> 8) & 0x000000FF;
      result[start + 3] = computedCrc32 & 0x000000FF;
    }
  }

  return result;
}

unsigned int PmtParser::GetPacketPid()
{
  unsigned int result = UINT_MAX;

  if ((this->packet != NULL) && (this->packetLength >= 4))
  {
    result = (this->packet[1] & 0x1F) << 8;
    result |= this->packet[2];
  }

  return result;
}

bool PmtParser::SetPacketPid(unsigned int pid)
{
  bool result = false;

  if ((this->packet != NULL) && (this->packetLength >= 4))
  {
    this->packet[1] = (this->packet[1] & 0xE0) | ((pid >> 8) & 0x1F);
    this->packet[2] = pid & 0x000000FF;
    result = true;
  }

  return result;
}

unsigned int PmtParser::GetTableId()
{
  unsigned int result = UINT_MAX;

  if ((this->packet != NULL) && (this->packetLength >= 6))
  {
    result = this->packet[5];
  }

  return result;
}

bool PmtParser::GetSectionSyntaxIndicator()
{
  bool result = false;

  if ((this->packet != NULL) && (this->packetLength >= 7))
  {
    result = ((this->packet[6] & 0x80) == 0);
  }

  return result;
}

unsigned int PmtParser::GetSectionLength()
{
  unsigned int result = UINT_MAX;

  if ((this->packet != NULL) && (this->packetLength >= 8))
  {
    result = (this->packet[6] & 0x0F) << 8;
    result += this->packet[7];
  }

  return result;
}

unsigned int PmtParser::GetProgramNumber()
{
  unsigned int result = UINT_MAX;

  if ((this->packet != NULL) && (this->packetLength >= 10))
  {
    result = this->packet[8] << 8;
    result += this->packet[9];
  }

  return result;
}

bool PmtParser::SetProgramNumber(unsigned int programNumber)
{
  bool result = false;

  if ((this->packet != NULL) && (this->packetLength >= 10))
  {
    this->packet[8] = (programNumber >> 8) & 0x000000FF;
    this->packet[9] = programNumber & 0x000000FF;

    result = true;
  }

  return result;
}

unsigned int PmtParser::GetVersionNumber()
{
  unsigned int result = UINT_MAX;

  if ((this->packet != NULL) && (this->packetLength >= 11))
  {
    result = (this->packet[10] & 0x3E) >> 1;
  }

  return result;
}

bool PmtParser::GetCurrentNextIndicator()
{
 bool result = false;

  if ((this->packet != NULL) && (this->packetLength >= 11))
  {
    result = ((this->packet[10] & 0x01) != 0);
  }

  return result;
}

unsigned int PmtParser::GetSectionNumber()
{
  unsigned int result = UINT_MAX;

  if ((this->packet != NULL) && (this->packetLength >= 12))
  {
    result = this->packet[11];
  }

  return result;
}

unsigned int PmtParser::GetLastSectionNumber()
{
  unsigned int result = UINT_MAX;

  if ((this->packet != NULL) && (this->packetLength >= 13))
  {
    result = this->packet[12];
  }

  return result;
}

unsigned int PmtParser::GetPcrPid()
{
  unsigned int result = UINT_MAX;

  if ((this->packet != NULL) && (this->packetLength >= 15))
  {
    result = (this->packet[13] & 0x1F) << 8;
    result += this->packet[14];
  }

  return result;
}

unsigned int PmtParser::GetProgramInfoLength()
{
  unsigned int result = UINT_MAX;

  if ((this->packet != NULL) && (this->packetLength >= 17))
  {
    result = (this->packet[15] & 0x0F) << 8;
    result += this->packet[16];
  }

  return result;
}

unsigned char *PmtParser::GetProgramInfo()
{
  unsigned char *result = NULL;

  unsigned int programInfoLength = this->GetProgramInfoLength();
  if ((programInfoLength != UINT_MAX) && (programInfoLength > 0) && (this->packetLength >= (17 + programInfoLength)))
  {
    result = ALLOC_MEM_SET(result, unsigned char, programInfoLength, 0);
    if (result != NULL)
    {
      memcpy(result, this->packet + 17, programInfoLength);
    }
  }

  return result;
}

unsigned int PmtParser::GetStreamDataLength()
{
  unsigned int result = UINT_MAX;

  if (this->packet != NULL)
  {
    result = 0;
    unsigned int count = this->streamDescriptions->Count();
    for (unsigned int i = 0; i < count; i++)
    {
      PmtStreamDescription *description = this->streamDescriptions->GetItem(i);
      if (description->GetDescriptionLength() != UINT_MAX)
      {
        result += description->GetDescriptionLength();
      }
    }
  }

  return result;
}

unsigned int PmtParser::GetStreamCount()
{
  unsigned int result = UINT_MAX;

  if (this->packet != NULL)
  {
    result = this->streamDescriptions->Count();
  }

  return result;
}

bool PmtParser::ParsePmtData(const unsigned char *data, unsigned int length)
{
  FREE_MEM(this->packet);
  this->packetLength = 0;

  bool result = false;

  if ((data != NULL) && (length == DVB_PACKET_SIZE))
  {
    unsigned int sectionLength = (data[6] & 0x0F) << 8;
    sectionLength += data[7];

    unsigned int programInfoLength = (data[15] & 0x0F) << 8;
    programInfoLength += data[16];

    if ((sectionLength > (9 + programInfoLength)))
    {
      // program info is localised 9 bytes after section length
      // section length must be greater than 9 + programInfoLength

      // in worst case the last 4 bytes will be CRC32
      // minimum stream description has 5 bytes
      // the length of stream description can variate because of stream descriptor

      // last 4 bytes is CRC32
      int allStreamDescriptionLength = (int)sectionLength - 9 - (int)programInfoLength - 4;

      if (allStreamDescriptionLength >= 0)
      {
        // all stream description length must be greater or equal to zero
        // if not, than something is wrong
        unsigned int contentLength = ((unsigned int)allStreamDescriptionLength) + 17 + programInfoLength + 4;
        if (contentLength <= length)
        {
          // content length must fit in DVB packet
          // now should be all information correct

          unsigned int streamsStart = 17 + programInfoLength;
          int processed = 0;

          while (processed < allStreamDescriptionLength)
          {
            PmtStreamDescription *description = new PmtStreamDescription(data + streamsStart + processed, length - streamsStart - processed);
            if (description != NULL)
            {
              if (description->IsValid())
              {
                if (this->streamDescriptions->Add(description))
                {
                  processed += description->GetDescriptionLength();
                }
                else
                {
                  break;
                }
              }
              else
              {
                break;
              }
            }
            else
            {
              break;
            }
          }

          if (processed == allStreamDescriptionLength)
          {
            // everything is correct
            result = true;
          }

          if (result)
          {
            // PMT stream descriptions are correctly parsed
            // now store all remaining DVB packet data

            this->packetLength = streamsStart;
            this->packet = ALLOC_MEM_SET(this->packet, unsigned char, this->packetLength, 0);
            if (this->packet != NULL)
            {
              memcpy(this->packet, data, this->packetLength);
            }
            else
            {
              // error occured
              result = false;
            }
          }

          if (result)
          {
            // copy CRC32
            unsigned int crc32Start = ((unsigned int)allStreamDescriptionLength) + 17 + programInfoLength;

            this->packetCrc32 = (data[crc32Start] << 24);
            this->packetCrc32 |= (data[crc32Start + 1] << 16);
            this->packetCrc32 |= (data[crc32Start + 2] << 8);
            this->packetCrc32 |= data[crc32Start + 3];
          }
        }
      }
    }
  }

  if (!result)
  {
    // error occured
    // clear all parsed information
    this->streamDescriptions->Clear();
    this->packetLength = 0;
    this->packetCrc32 = 0;
    FREE_MEM(this->packet);
  }

  return result;
}

bool PmtParser::IsValidPacket()
{
  bool result = (this->packet != NULL);
  result &= (this->GetTableId() != UINT_MAX);
  result &= (this->GetSectionLength() != UINT_MAX);
  result &= (this->GetProgramNumber() != UINT_MAX);
  result &= (this->GetVersionNumber() != UINT_MAX);
  result &= (this->GetSectionNumber() != UINT_MAX);
  result &= (this->GetLastSectionNumber() != UINT_MAX);
  result &= (this->GetPcrPid() != UINT_MAX);
  result &= (this->GetProgramInfoLength() != UINT_MAX);
  result &= (this->GetStreamCount() != UINT_MAX) && (this->GetStreamCount() > 0);
  result &= (this->CheckCrc32());

  return result;
}

unsigned int PmtParser::GetCrc32()
{
  unsigned int result = UINT_MAX;

  if (this->packet != NULL)
  {
    result = this->packetCrc32;
  }

  return result;
}

bool PmtParser::CheckCrc32()
{
  bool result = false;

  if ((this->packet != NULL) && (this->crc32 != NULL))
  {
    // compute packet length
    unsigned int length = this->packetLength - 5;
    unsigned int count = this->streamDescriptions->Count();
    for (unsigned int i = 0; i < count; i++)
    {
      length += this->streamDescriptions->GetItem(i)->GetDescriptionLength();
    }

    // create new packet
    ALLOC_MEM_DEFINE_SET(temp, unsigned char, length, 0);
    if (temp != NULL)
    {
      // copy data from current packet (starting at 5 byte)
      memcpy(temp, this->packet + 5, this->packetLength - 5);
      unsigned int start = this->packetLength - 5;
      for (unsigned int i = 0; i < count; i++)
      {
        PmtStreamDescription *description = this->streamDescriptions->GetItem(i);
        unsigned int descriptionLength = description->GetDescriptionLength();
        unsigned char *descriptionData = description->GetDescription();

        if (descriptionData != NULL)
        {
          memcpy(temp + start, descriptionData, descriptionLength);
          start += descriptionLength;
        }
        FREE_MEM(descriptionData);
      }

      unsigned int computedCrc32 = this->crc32->Compute((char *)temp, length);
      result = (this->packetCrc32 == computedCrc32);
    }
    FREE_MEM(temp);
  }

  return result;
}

bool PmtParser::RecalculateCrc32()
{
  bool result = false;

  if ((this->packet != NULL) && (this->crc32 != NULL))
  {
    unsigned char *data = this->GetPmtPacket();
    if (data != NULL)
    {
      PmtParser *parser = new PmtParser(this->crc32);
      if (parser != NULL)
      {
        if (parser->SetPmtData(data, DVB_PACKET_SIZE))
        {
          this->SetCrc32(parser->GetCrc32());
          result = true;
        }
        delete parser;
      }
    }
    FREE_MEM(data);
  }

  return result;
}

bool PmtParser::SetCrc32(unsigned int crc32)
{
  bool result = false;

  if (this->packet != NULL)
  {
    this->packetCrc32 = crc32;
    result = true;
  }

  return result;
}

PmtStreamDescriptionCollection *PmtParser::GetPmtStreamDescriptions(void)
{
  return this->streamDescriptions;
}

bool PmtParser::RecalculateSectionLength(void)
{
  bool result = false;

  if ((this->packet != NULL) && (this->packetLength >= 17))
  {
    // remove first 8 bytes, plus 4 bytes CRC32
    unsigned int sectionLength = this->packetLength - 4;

    unsigned int count = this->streamDescriptions->Count();
    for (unsigned int i = 0; i < count; i++)
    {
      sectionLength += this->streamDescriptions->GetItem(i)->GetDescriptionLength();
    }
    
    result = this->SetSectionLength(sectionLength);
  }

  return result;
}

bool PmtParser::SetSectionLength(unsigned int length)
{
  bool result = false;

  if ((this->packet != NULL) && (this->packetLength >= 8) && (length <= 0x00000FFF))
  {
    this->packet[7] = length & 0xFF;
    length >>= 8;
    this->packet[6] &= 0xF0;
    this->packet[6] |= length & 0x0F;

    result = true;
  }

  return result;
}
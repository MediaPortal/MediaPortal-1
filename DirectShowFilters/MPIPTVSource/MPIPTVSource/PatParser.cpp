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

#include "PatParser.h"
#include "ProtocolInterface.h"

PatParser::PatParser(Crc32 *crc32)
{
  this->packet = NULL;
  this->packetLength = 0;
  this->crc32 = crc32;
  this->crc32created = false;

  if (this->crc32 == NULL)
  {
    this->crc32 = new Crc32();
    if (this->crc32 != NULL)
    {
      this->crc32created = true;
    }
  }
}

PatParser::~PatParser(void)
{
  if (this->crc32created && (this->crc32 != NULL))
  {
    delete this->crc32;
    this->crc32 = NULL;
  }
  FREE_MEM(packet);
}

bool PatParser::SetPatData(const unsigned char *data, unsigned int length)
{
  FREE_MEM(this->packet);
  if ((length > 0) && (data != NULL))
  {
    this->packet = ALLOC_MEM_SET(this->packet, unsigned char, length, 0);
    if (packet != NULL)
    {
      this->packetLength = length;
      memcpy(this->packet, data, length);
    }
  }

  return (this->packet != NULL);
}

unsigned char *PatParser::GetPatPacket()
{
  unsigned char *result = NULL;

  if (this->packet != NULL)
  {
    result = ALLOC_MEM_SET(result, unsigned char, this->packetLength, 0);
    if (result != NULL)
    {
      memcpy(result, this->packet, this->packetLength);
    }
  }

  return result;
}

unsigned int PatParser::GetTableId()
{
  unsigned int result = UINT_MAX;

  if ((this->packet != NULL) && (this->packetLength == DVB_PACKET_SIZE))
  {
    result = this->packet[5];
  }

  return result;
}

bool PatParser::GetSectionSyntaxIndicator()
{
  bool result = false;

  if ((this->packet != NULL) && (this->packetLength == DVB_PACKET_SIZE))
  {
    result = ((this->packet[6] & 0x80) == 0);
  }

  return result;
}

unsigned int PatParser::GetSectionLength()
{
  unsigned int result = UINT_MAX;

  if ((this->packet != NULL) && (this->packetLength == DVB_PACKET_SIZE))
  {
    result = (this->packet[6] & 0x0F) << 8;
    result += this->packet[7];
  }

  return result;
}

unsigned int PatParser::GetTransportStreamId()
{
  unsigned int result = UINT_MAX;

  if ((this->packet != NULL) && (this->packetLength == DVB_PACKET_SIZE))
  {
    result = this->packet[8] << 8;
    result += this->packet[9];
  }

  return result;
}

unsigned int PatParser::GetVersionNumber()
{
  unsigned int result = UINT_MAX;

  if ((this->packet != NULL) && (this->packetLength == DVB_PACKET_SIZE))
  {
    result = (this->packet[10] & 0x3E) >> 1;
  }

  return result;
}

bool PatParser::GetCurrentNextIndicator()
{
 bool result = false;

  if ((this->packet != NULL) && (this->packetLength == DVB_PACKET_SIZE))
  {
    result = ((this->packet[10] & 0x01) != 0);
  }

  return result;
}

unsigned int PatParser::GetSectionNumber()
{
  unsigned int result = UINT_MAX;

  if ((this->packet != NULL) && (this->packetLength == DVB_PACKET_SIZE))
  {
    result = this->packet[11];
  }

  return result;
}

unsigned int PatParser::GetLastSectionNumber()
{
  unsigned int result = UINT_MAX;

  if ((this->packet != NULL) && (this->packetLength == DVB_PACKET_SIZE))
  {
    result = this->packet[12];
  }

  return result;
}

unsigned int PatParser::GetProgramNumberCount()
{
  unsigned int result = UINT_MAX;

  if ((this->packet != NULL) && (this->packetLength == DVB_PACKET_SIZE))
  {
    unsigned int i = 13;
    result = 0;
    while ((i + 4) < this->packetLength)
    {
      unsigned int value = (this->packet[i] << 24) | (this->packet[i + 1] << 16) | (this->packet[i + 2] << 8) | this->packet[i + 3];
      if (value == 0xFFFFFFFF)
      {
        if (result > 0)
        {
          // correct, last 4 bytes is CRC32
          result--;
        }
        else
        {
          // error, at least one program have to be specified
          result = UINT_MAX;
        }
        break;
      }
      result++;
      i += 4;
    }
  }

  return result;
}

unsigned int PatParser::GetProgramNumber(unsigned int position)
{
  unsigned int result = UINT_MAX;

  if ((this->packet != NULL) && (this->packetLength == DVB_PACKET_SIZE))
  {
    unsigned int programCount = this->GetProgramNumberCount();

    if ((programCount != UINT_MAX) && (position < programCount))
    {
      result = (this->packet[position * 4 + 13] << 8) | this->packet[position * 4 + 14];
    }
  }

  return result;
}

unsigned int PatParser::GetPid(unsigned int position)
{
  unsigned int result = UINT_MAX;

  if ((this->packet != NULL) && (this->packetLength == DVB_PACKET_SIZE))
  {
    unsigned int programCount = this->GetProgramNumberCount();

    if ((programCount != UINT_MAX) && (position < programCount))
    {
      result = (this->packet[position * 4 + 15] << 8) | this->packet[position * 4 + 16];
      result &= 0x00001FFF;
    }
  }

  return result;
}

bool PatParser::IsValidPacket()
{
  bool result = (this->packet != NULL) && (this->packetLength == DVB_PACKET_SIZE);
  result &= (this->GetTableId() != UINT_MAX);
  result &= (this->GetSectionLength() != UINT_MAX);
  result &= (this->GetTransportStreamId() != UINT_MAX);
  result &= (this->GetVersionNumber() != UINT_MAX);
  result &= (this->GetSectionNumber() != UINT_MAX);
  result &= (this->GetLastSectionNumber() != UINT_MAX);
  result &= (this->GetProgramNumberCount() != UINT_MAX) && (this->GetProgramNumberCount() > 0);
  result &= (this->CheckCrc32());

  return result;
}

unsigned int PatParser::GetCrc32()
{
  unsigned int result = UINT_MAX;

  if ((this->packet != NULL) && (this->packetLength == DVB_PACKET_SIZE))
  {
    unsigned int programNumberCount = this->GetProgramNumberCount();
    if (programNumberCount != UINT_MAX)
    {
      result = (this->packet[programNumberCount * 4 + 13] << 24) | (this->packet[programNumberCount * 4 + 14] << 16) | (this->packet[programNumberCount * 4 + 15] << 8) | this->packet[programNumberCount * 4 + 16];
    }
  }

  return result;
}

bool PatParser::CheckCrc32()
{
  bool result = false;

  if ((this->packet != NULL) && (this->packetLength == DVB_PACKET_SIZE) && (this->crc32 != NULL))
  {
    unsigned int programNumberCount = this->GetProgramNumberCount();
    if (programNumberCount != UINT_MAX)
    {
      unsigned int dataLength = programNumberCount * 4 + 8;

      unsigned int packetCrc32 = this->GetCrc32();
      unsigned int computedCrc32 = this->crc32->Compute((char *)(this->packet + 5), dataLength);

      result = (packetCrc32 == computedCrc32);
    }
  }

  return result;
}

bool PatParser::SetProgramNumber(unsigned int position, unsigned int programNumber)
{
  bool result = false;

  if ((this->packet != NULL) && (this->packetLength == DVB_PACKET_SIZE))
  {
    unsigned int programCount = this->GetProgramNumberCount();

    if ((programCount != UINT_MAX) && (position < programCount))
    {
      this->packet[position * 4 + 14] = programNumber & 0x000000FF;
      this->packet[position * 4 + 13] = (programNumber >> 8) & 0x000000FF;

      result = true;
    }
  }

  return result;
}

bool PatParser::SetPid(unsigned int position, unsigned int pid)
{
  bool result = false;

  if ((this->packet != NULL) && (this->packetLength == DVB_PACKET_SIZE))
  {
    unsigned int programCount = this->GetProgramNumberCount();

    if ((programCount != UINT_MAX) && (position < programCount))
    {
      this->packet[position * 4 + 16] = pid & 0x000000FF;
      this->packet[position * 4 + 15] = (pid >> 8) & 0x0000001F;

      result = true;
    }
  }

  return result;
}

bool PatParser::RecalculateCrc32()
{
  bool result = false;

  if ((this->packet != NULL) && (this->packetLength == DVB_PACKET_SIZE) && (this->crc32 != NULL))
  {
    unsigned int programNumberCount = this->GetProgramNumberCount();
    if (programNumberCount != UINT_MAX)
    {
      unsigned int dataLength = programNumberCount * 4 + 8;
      unsigned int computedCrc32 = this->crc32->Compute((char *)(this->packet + 5), dataLength);
      result = this->SetCrc32(computedCrc32);
    }
  }

  return result;
}

bool PatParser::SetCrc32(unsigned int crc32)
{
  bool result = false;

  if ((this->packet != NULL) && (this->packetLength == DVB_PACKET_SIZE))
  {
    unsigned int programNumberCount = this->GetProgramNumberCount();
    if (programNumberCount != UINT_MAX)
    {
      this->packet[programNumberCount * 4 + 16] = crc32 & 0x000000FF;
      this->packet[programNumberCount * 4 + 15] = (crc32 >> 8) & 0x000000FF;
      this->packet[programNumberCount * 4 + 14] = (crc32 >> 16) & 0x000000FF;
      this->packet[programNumberCount * 4 + 13] = (crc32 >> 24) & 0x000000FF;

      result = true;
    }
  }

  return result;
}
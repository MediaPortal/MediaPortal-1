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

#include "PmtStreamDescription.h"

PmtStreamDescription::PmtStreamDescription(const unsigned char *data, unsigned int length)
{
  this->description = NULL;
  this->length = 0;

  if ((data != NULL) && (length > 0))
  {
    unsigned int streamDescriptionLength = 5 + ((data[3] & 0x0F) << 8) + data[4];
    if (length >= streamDescriptionLength)
    {
      this->description = ALLOC_MEM_SET(this->description, unsigned char, streamDescriptionLength, 0);
      if (this->description != NULL)
      {
        this->length = streamDescriptionLength;
        memcpy(this->description, data, this->length);
      }
    }
  }
}

PmtStreamDescription::~PmtStreamDescription(void)
{
  FREE_MEM(this->description);
}

unsigned int PmtStreamDescription::GetDescriptionLength()
{
  return this->length;
}

unsigned char *PmtStreamDescription::GetDescription()
{
  unsigned char *result = NULL;

  if (this->length > 0)
  {
    result = ALLOC_MEM_SET(result, unsigned char, this->length, 0);
    if (result != NULL)
    {
      memcpy(result, this->description, this->length);
    }
  }

  return result;
}

unsigned int PmtStreamDescription::GetStreamType()
{
  unsigned int result = UINT_MAX;

  if (this->length > 0)
  {
    result = this->description[0];
  }

  return result;
}

unsigned int PmtStreamDescription::GetStreamPid()
{
  unsigned int result = UINT_MAX;

  if (this->length > 0)
  {
    result = ((this->description[1] & 0x1F) << 8) | this->description[2];
  }

  return result;
}

unsigned int PmtStreamDescription::GetStreamDescriptorLength()
{
  unsigned int result = UINT_MAX;

  if (this->length > 0)
  {
    result = ((this->description[3] & 0x0F) << 8) | this->description[4];
  }

  return result;
}

unsigned char *PmtStreamDescription::GetStreamDescriptor()
{
  unsigned char *result = NULL;

  if (this->length > 5)
  {
    unsigned int descriptorLength = this->length - 5;
    result = ALLOC_MEM_SET(result, unsigned char, descriptorLength, 0);
    if (result != NULL)
    {
      memcpy(result, this->description + 5, descriptorLength);
    }
  }

  return result;
}

bool PmtStreamDescription::IsValid()
{
  unsigned int streamType = this->GetStreamType();
  unsigned int streamPid = this->GetStreamPid();
  unsigned int streamDescriptorLength = this->GetStreamDescriptorLength();

  return ((streamType != UINT_MAX) && (streamPid != UINT_MAX) && (streamDescriptorLength != UINT_MAX));
}
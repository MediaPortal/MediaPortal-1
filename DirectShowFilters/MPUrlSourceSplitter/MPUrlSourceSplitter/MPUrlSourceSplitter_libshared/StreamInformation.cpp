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

#include "StreamInformation.h"

CStreamInformation::CStreamInformation(HRESULT *result)
  : CFlags()
{
  this->streamInputFormat = NULL;
}

CStreamInformation::~CStreamInformation(void)
{
  FREE_MEM(this->streamInputFormat);
}

/* get methods */

const wchar_t *CStreamInformation::GetStreamInputFormat(void)
{
  return this->streamInputFormat;
}

/* set methods */

bool CStreamInformation::SetStreamInputFormat(const wchar_t *streamInputFormat)
{
  SET_STRING_RETURN_WITH_NULL(this->streamInputFormat, streamInputFormat);
}

void CStreamInformation::SetContainer(bool container)
{
  this->flags &= ~STREAM_INFORMATION_FLAG_CONTAINER;
  this->flags |= (container) ? STREAM_INFORMATION_FLAG_CONTAINER : STREAM_INFORMATION_FLAG_NONE;
}

void CStreamInformation::SetPackets(bool packets)
{
  this->flags &= ~STREAM_INFORMATION_FLAG_PACKETS;
  this->flags |= (packets) ? STREAM_INFORMATION_FLAG_PACKETS : STREAM_INFORMATION_FLAG_NONE;
}

/* other methods */

bool CStreamInformation::IsContainer(void)
{
  return this->IsSetFlags(STREAM_INFORMATION_FLAG_CONTAINER);
}

bool CStreamInformation::IsPackets(void)
{
  return this->IsSetFlags(STREAM_INFORMATION_FLAG_PACKETS);
}

void CStreamInformation::Clear(void)
{
  FREE_MEM(this->streamInputFormat);
  this->flags = STREAM_INFORMATION_FLAG_NONE;
}
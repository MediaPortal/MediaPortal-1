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

#include "TransportStreamProgramMapParserKnownSectionContext.h"
#include "Section.h"

CTransportStreamProgramMapParserKnownSectionContext::CTransportStreamProgramMapParserKnownSectionContext(HRESULT *result, uint16_t programNumber, unsigned int crc32)
{
  this->programNumber = programNumber;
  this->crc32 = crc32;
}

CTransportStreamProgramMapParserKnownSectionContext::~CTransportStreamProgramMapParserKnownSectionContext()
{
}

/* get methods */

uint16_t CTransportStreamProgramMapParserKnownSectionContext::GetProgramNumber(void)
{
  return this->programNumber;
}

unsigned int CTransportStreamProgramMapParserKnownSectionContext::GetCrc32(void)
{
  return this->crc32;
}

/* set methods */

void CTransportStreamProgramMapParserKnownSectionContext::SetCrc32(unsigned int crc32)
{
  this->crc32 = crc32;
}

/* other methods */

/* protected methods */
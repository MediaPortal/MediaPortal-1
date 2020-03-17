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

#include "TransportStreamProgramMapSectionMultiplexer.h"
#include "TransportStreamProgramMapSection.h"


CTransportStreamProgramMapSectionMultiplexer::CTransportStreamProgramMapSectionMultiplexer(HRESULT *result, unsigned int pid, unsigned int requestedPid, unsigned int continuityCounter)
  : CSectionMultiplexer(result, pid, requestedPid, continuityCounter)
{
}

CTransportStreamProgramMapSectionMultiplexer::~CTransportStreamProgramMapSectionMultiplexer()
{
}

/* get methods */

/* set methods */

/* other methods */

HRESULT CTransportStreamProgramMapSectionMultiplexer::AddSection(CSection *section)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, section);

  if (SUCCEEDED(result))
  {
    CTransportStreamProgramMapSection *transportStreamProgramMapSection = dynamic_cast<CTransportStreamProgramMapSection *>(section);
    CHECK_POINTER_HRESULT(result, transportStreamProgramMapSection, result, E_INVALIDARG);

    CHECK_CONDITION_HRESULT(result, this->sections->Add(section), result, E_OUTOFMEMORY);
  }

  return result;
}

/* protected methods */

void CTransportStreamProgramMapSectionMultiplexer::IncreaseReferenceCount(CMpeg2tsStreamFragment *streamFragment)
{
  streamFragment->SetMultiplexerTransportStreamProgramMapSectionReferenceCount(streamFragment->GetMultiplexerTransportStreamProgramMapSectionReferenceCount() + 1);
}

void CTransportStreamProgramMapSectionMultiplexer::DecreaseReferenceCount(CMpeg2tsStreamFragment *streamFragment)
{
  streamFragment->SetMultiplexerTransportStreamProgramMapSectionReferenceCount(streamFragment->GetMultiplexerTransportStreamProgramMapSectionReferenceCount() - 1);
}

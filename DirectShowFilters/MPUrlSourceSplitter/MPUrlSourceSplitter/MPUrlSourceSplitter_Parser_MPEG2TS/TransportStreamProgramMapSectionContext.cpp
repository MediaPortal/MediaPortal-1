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

#include "TransportStreamProgramMapSectionContext.h"

CTransportStreamProgramMapSectionContext::CTransportStreamProgramMapSectionContext(HRESULT *result)
  : CSectionContext(result)
{
  this->pid = TRANSPORT_STREAM_PROGRAM_MAP_PID_NOT_DEFINED;
}

CTransportStreamProgramMapSectionContext::~CTransportStreamProgramMapSectionContext(void)
{
}

/* get methods */

CTransportStreamProgramMapSection *CTransportStreamProgramMapSectionContext::GetOriginalSection(void)
{
  return (CTransportStreamProgramMapSection *)__super::GetOriginalSection();
}

CTransportStreamProgramMapSection *CTransportStreamProgramMapSectionContext::GetUpdatedSection(void)
{
  return (CTransportStreamProgramMapSection *)__super::GetUpdatedSection();
}

uint16_t CTransportStreamProgramMapSectionContext::GetPID(void)
{
  return this->pid;
}

/* set methods */

bool CTransportStreamProgramMapSectionContext::SetOriginalSection(CSection *section)
{
  CTransportStreamProgramMapSection *transportStreamProgramMapSectionContext = dynamic_cast<CTransportStreamProgramMapSection *>(section);

  return (transportStreamProgramMapSectionContext == NULL) ? false : __super::SetOriginalSection(transportStreamProgramMapSectionContext);
}

void CTransportStreamProgramMapSectionContext::SetPID(uint16_t pid)
{
  this->pid = pid;
}

/* other methods */

bool CTransportStreamProgramMapSectionContext::CreateUpdatedSection(void)
{
  HRESULT result = S_OK;
  FREE_MEM_CLASS(this->updatedSection);

  this->updatedSection = (CTransportStreamProgramMapSection *)this->originalSection->Clone();
  CHECK_POINTER_HRESULT(result, this->updatedSection, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(this->updatedSection));
  return SUCCEEDED(result);
}

/* protected methods */
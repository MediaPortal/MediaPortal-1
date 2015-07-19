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

#include "TransportStreamProgramMapParserContext.h"

CTransportStreamProgramMapParserContext::CTransportStreamProgramMapParserContext(HRESULT *result, uint16_t pid)
  : CParserContext(result)
{
  this->leaveProgramElements = NULL;
  this->knownSections = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->parser = new CTransportStreamProgramMapParser(result, pid);
    this->leaveProgramElements = new CProgramElementCollection(result);
    this->knownSections = new CTransportStreamProgramMapParserKnownSectionContextCollection(result);

    CHECK_POINTER_HRESULT(*result, this->parser, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->leaveProgramElements, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->knownSections, *result, E_OUTOFMEMORY);
  }
}

CTransportStreamProgramMapParserContext::~CTransportStreamProgramMapParserContext(void)
{
  FREE_MEM_CLASS(this->leaveProgramElements);
  FREE_MEM_CLASS(this->knownSections);
}

/* get methods */

CTransportStreamProgramMapParser *CTransportStreamProgramMapParserContext::GetParser(void)
{
  return (CTransportStreamProgramMapParser *)__super::GetParser();
}

CProgramElementCollection *CTransportStreamProgramMapParserContext::GetLeaveProgramElements(void)
{
  return this->leaveProgramElements;
}

/* set methods */

HRESULT CTransportStreamProgramMapParserContext::SetKnownSection(CSection *section)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, section);

  if (SUCCEEDED(result))
  {
    CTransportStreamProgramMapSection *transportStreamProgramMapSection = dynamic_cast<CTransportStreamProgramMapSection *>(section);
    CHECK_POINTER_HRESULT(result, transportStreamProgramMapSection, result, E_INVALIDARG);

    CHECK_CONDITION_HRESULT(result, this->knownSections->Add((uint16_t)transportStreamProgramMapSection->GetProgramNumber(), transportStreamProgramMapSection->GetCrc32()), result, E_FAIL);
  }

  return result;
}

void CTransportStreamProgramMapParserContext::SetFilterProgramElements(bool filterProgramElements)
{
  this->flags &= ~TRANSPORT_STREAM_PROGRAM_MAP_PARSER_CONTEXT_FLAG_FILTER_PROGRAM_ELEMENTS;
  this->flags |= filterProgramElements ? TRANSPORT_STREAM_PROGRAM_MAP_PARSER_CONTEXT_FLAG_FILTER_PROGRAM_ELEMENTS : TRANSPORT_STREAM_PROGRAM_MAP_PARSER_CONTEXT_FLAG_NONE;
}

/* other methods */

void CTransportStreamProgramMapParserContext::Clear(void)
{
  __super::Clear();
}

bool CTransportStreamProgramMapParserContext::IsKnownSection(CSection *section)
{
  CTransportStreamProgramMapSection *transportStreamProgramMapSection = dynamic_cast<CTransportStreamProgramMapSection *>(section);
  bool result = (transportStreamProgramMapSection != NULL);

  if (result)
  {
    result = this->knownSections->Contains((uint16_t)transportStreamProgramMapSection->GetProgramNumber(), transportStreamProgramMapSection->GetCrc32());
  }

  return result;
}

bool CTransportStreamProgramMapParserContext::IsFilterProgramElements(void)
{
  return this->IsSetFlags(TRANSPORT_STREAM_PROGRAM_MAP_PARSER_CONTEXT_FLAG_FILTER_PROGRAM_ELEMENTS);
}

/* protected methods */

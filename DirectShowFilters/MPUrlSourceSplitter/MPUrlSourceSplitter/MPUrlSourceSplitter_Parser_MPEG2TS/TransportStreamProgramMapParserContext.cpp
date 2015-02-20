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

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->parser = new CTransportStreamProgramMapParser(result, pid);
    this->leaveProgramElements = new CProgramElementCollection(result);

    CHECK_POINTER_HRESULT(*result, this->parser, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->leaveProgramElements, *result, E_OUTOFMEMORY);
  }
}

CTransportStreamProgramMapParserContext::~CTransportStreamProgramMapParserContext(void)
{
  FREE_MEM_CLASS(this->leaveProgramElements);
}

/* get methods */

CTransportStreamProgramMapParser *CTransportStreamProgramMapParserContext::GetParser(void)
{
  return (CTransportStreamProgramMapParser *)__super::GetParser();
}

CTransportStreamProgramMapSectionContext *CTransportStreamProgramMapParserContext::GetSectionContext(void)
{
  return (CTransportStreamProgramMapSectionContext *)__super::GetSectionContext();
}

CProgramElementCollection *CTransportStreamProgramMapParserContext::GetLeaveProgramElements(void)
{
  return this->leaveProgramElements;
}

/* set methods */

void CTransportStreamProgramMapParserContext::SetFilterProgramElements(bool filterProgramElements)
{
  this->flags &= ~TRANSPORT_STREAM_PROGRAM_MAP_PARSER_CONTEXT_FLAG_FILTER_PROGRAM_ELEMENTS;
  this->flags |= filterProgramElements ? TRANSPORT_STREAM_PROGRAM_MAP_PARSER_CONTEXT_FLAG_FILTER_PROGRAM_ELEMENTS : TRANSPORT_STREAM_PROGRAM_MAP_PARSER_CONTEXT_FLAG_NONE;
}

/* other methods */

bool CTransportStreamProgramMapParserContext::IsFilterProgramElements(void)
{
  return this->IsSetFlags(TRANSPORT_STREAM_PROGRAM_MAP_PARSER_CONTEXT_FLAG_FILTER_PROGRAM_ELEMENTS);
}

HRESULT CTransportStreamProgramMapParserContext::CreateSectionContext(void)
{
  HRESULT result = S_OK;
  FREE_MEM_CLASS(this->sectionContext);

  this->sectionContext = new CTransportStreamProgramMapSectionContext(&result, this);
  CHECK_POINTER_HRESULT(result, this->sectionContext, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(this->sectionContext));
  return result;
}

/* protected methods */

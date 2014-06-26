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

#include "RtspResponseHeaderFactory.h"

#include "RtspSequenceResponseHeader.h"
#include "RtspPublicResponseHeader.h"
#include "RtspLocationResponseHeader.h"
#include "RtspServerResponseHeader.h"
#include "RtspAllowResponseHeader.h"
#include "RtspContentBaseResponseHeader.h"
#include "RtspContentLengthResponseHeader.h"
#include "RtspContentLocationResponseHeader.h"
#include "RtspContentTypeResponseHeader.h"
#include "RtspTransportResponseHeader.h"
#include "RtspSessionResponseHeader.h"
#include "RtspRtpInfoResponseHeader.h"

CRtspResponseHeaderFactory::CRtspResponseHeaderFactory(void)
{
}

CRtspResponseHeaderFactory::~CRtspResponseHeaderFactory(void)
{
}

CRtspResponseHeader *CRtspResponseHeaderFactory::CreateResponseHeader(const wchar_t *buffer, unsigned int length)
{
  CRtspResponseHeader *result = NULL;
  HRESULT continueParsing = ((buffer != NULL) && (length > 0)) ? S_OK : E_INVALIDARG;

  if (SUCCEEDED(continueParsing))
  {
    CRtspResponseHeader *header = new CRtspResponseHeader(&continueParsing);
    CHECK_POINTER_HRESULT(continueParsing, header, continueParsing, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(continueParsing, header->Parse(buffer, length), continueParsing, E_FAIL);

    if (SUCCEEDED(continueParsing))
    {
      // insert most specific response headers on top
      CREATE_SPECIFIC_RESPONSE_HEADER(CRtspSequenceResponseHeader, buffer, length, continueParsing, result);
      CREATE_SPECIFIC_RESPONSE_HEADER(CRtspPublicResponseHeader, buffer, length, continueParsing, result);
      CREATE_SPECIFIC_RESPONSE_HEADER(CRtspLocationResponseHeader, buffer, length, continueParsing, result);
      CREATE_SPECIFIC_RESPONSE_HEADER(CRtspServerResponseHeader, buffer, length, continueParsing, result);
      CREATE_SPECIFIC_RESPONSE_HEADER(CRtspAllowResponseHeader, buffer, length, continueParsing, result);
      CREATE_SPECIFIC_RESPONSE_HEADER(CRtspContentBaseResponseHeader, buffer, length, continueParsing, result);
      CREATE_SPECIFIC_RESPONSE_HEADER(CRtspContentLengthResponseHeader, buffer, length, continueParsing, result);
      CREATE_SPECIFIC_RESPONSE_HEADER(CRtspContentLocationResponseHeader, buffer, length, continueParsing, result);
      CREATE_SPECIFIC_RESPONSE_HEADER(CRtspContentTypeResponseHeader, buffer, length, continueParsing, result);
      CREATE_SPECIFIC_RESPONSE_HEADER(CRtspTransportResponseHeader, buffer, length, continueParsing, result);
      CREATE_SPECIFIC_RESPONSE_HEADER(CRtspSessionResponseHeader, buffer, length, continueParsing, result);
      CREATE_SPECIFIC_RESPONSE_HEADER(CRtspRtpInfoResponseHeader, buffer, length, continueParsing, result);
    }

    CHECK_CONDITION_NOT_NULL_EXECUTE(result, FREE_MEM_CLASS(header));

    if (SUCCEEDED(continueParsing) && (result == NULL))
    {
      result = header;
    }
  }

  CHECK_CONDITION_EXECUTE(FAILED(continueParsing), FREE_MEM_CLASS(result));
  return result;
}
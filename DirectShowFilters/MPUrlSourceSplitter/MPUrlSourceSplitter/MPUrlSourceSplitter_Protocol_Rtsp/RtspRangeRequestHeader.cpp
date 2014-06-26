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

#include "RtspRangeRequestHeader.h"

CRtspRangeRequestHeader::CRtspRangeRequestHeader(HRESULT *result)
  : CRtspRequestHeader(result)
{
}

CRtspRangeRequestHeader::~CRtspRangeRequestHeader(void)
{
}

/* get methods */

const wchar_t *CRtspRangeRequestHeader::GetName(void)
{
  return RTSP_RANGE_REQUEST_HEADER_NAME;
}

const wchar_t *CRtspRangeRequestHeader::GetValue(void)
{
  return __super::GetValue();
}

/* set methods */

bool CRtspRangeRequestHeader::SetName(const wchar_t *name)
{
  // we never set name
  return false;
}

bool CRtspRangeRequestHeader::SetValue(const wchar_t *value)
{
  // we never set value
  return false;
}

/* other methods */

/* protected methods */

bool CRtspRangeRequestHeader::CloneInternal(CHttpHeader *clone)
{
  return __super::CloneInternal(clone);
}

CHttpHeader *CRtspRangeRequestHeader::CreateHeader(void)
{
  HRESULT result = S_OK;
  CRtspRangeRequestHeader *header = new CRtspRangeRequestHeader(&result);
  CHECK_POINTER_HRESULT(result, header, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(header));
  return header;
}
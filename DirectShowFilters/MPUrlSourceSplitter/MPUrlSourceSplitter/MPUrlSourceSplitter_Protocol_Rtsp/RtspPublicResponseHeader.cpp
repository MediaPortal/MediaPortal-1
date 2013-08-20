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

#include "RtspPublicResponseHeader.h"

CRtspPublicResponseHeader::CRtspPublicResponseHeader(void)
  : CRtspResponseHeader()
{
  this->flags = FLAG_RTSP_PUBLIC_RESPONSE_HEADER_METHOD_NONE;
}

CRtspPublicResponseHeader::~CRtspPublicResponseHeader(void)
{
}

/* get methods */

/* set methods */

/* other methods */

CRtspPublicResponseHeader *CRtspPublicResponseHeader::Clone(void)
{
  return (CRtspPublicResponseHeader *)__super::Clone();
}

bool CRtspPublicResponseHeader::CloneInternal(CHttpHeader *clonedHeader)
{
  bool result = __super::CloneInternal(clonedHeader);
  CRtspPublicResponseHeader *header = dynamic_cast<CRtspPublicResponseHeader *>(clonedHeader);
  result &= (header != NULL);

  if (result)
  {
    header->flags = this->flags;
  }

  return result;
}

CHttpHeader *CRtspPublicResponseHeader::GetNewHeader(void)
{
  return new CRtspPublicResponseHeader();
}

bool CRtspPublicResponseHeader::Parse(const wchar_t *header, unsigned int length)
{
  bool result = __super::Parse(header, length);

  if (result)
  {
    result &= (_wcsicmp(this->name, RTSP_PUBLIC_RESPONSE_HEADER_TYPE) == 0);

    if (result)
    {
      unsigned int valueLength = wcslen(this->value);
      if (IndexOf(this->value, valueLength, RTSP_PUBLIC_RESPONSE_HEADER_METHOD_DESCRIBE, RTSP_PUBLIC_RESPONSE_HEADER_METHOD_DESCRIBE_LENGTH) != (-1))
      {
        this->flags |= FLAG_RTSP_PUBLIC_RESPONSE_HEADER_METHOD_DESCRIBE;
      }

      if (IndexOf(this->value, valueLength, RTSP_PUBLIC_RESPONSE_HEADER_METHOD_ANNOUNCE, RTSP_PUBLIC_RESPONSE_HEADER_METHOD_ANNOUNCE_LENGTH) != (-1))
      {
        this->flags |= FLAG_RTSP_PUBLIC_RESPONSE_HEADER_METHOD_ANNOUNCE;
      }

      if (IndexOf(this->value, valueLength, RTSP_PUBLIC_RESPONSE_HEADER_METHOD_GET_PARAMETER, RTSP_PUBLIC_RESPONSE_HEADER_METHOD_GET_PARAMETER_LENGTH) != (-1))
      {
        this->flags |= FLAG_RTSP_PUBLIC_RESPONSE_HEADER_METHOD_GET_PARAMETER;
      }

      if (IndexOf(this->value, valueLength, RTSP_PUBLIC_RESPONSE_HEADER_METHOD_OPTIONS, RTSP_PUBLIC_RESPONSE_HEADER_METHOD_OPTIONS_LENGTH) != (-1))
      {
        this->flags |= FLAG_RTSP_PUBLIC_RESPONSE_HEADER_METHOD_OPTIONS;
      }

      if (IndexOf(this->value, valueLength, RTSP_PUBLIC_RESPONSE_HEADER_METHOD_PAUSE, RTSP_PUBLIC_RESPONSE_HEADER_METHOD_PAUSE_LENGTH) != (-1))
      {
        this->flags |= FLAG_RTSP_PUBLIC_RESPONSE_HEADER_METHOD_PAUSE;
      }

      if (IndexOf(this->value, valueLength, RTSP_PUBLIC_RESPONSE_HEADER_METHOD_PLAY, RTSP_PUBLIC_RESPONSE_HEADER_METHOD_PLAY_LENGTH) != (-1))
      {
        this->flags |= FLAG_RTSP_PUBLIC_RESPONSE_HEADER_METHOD_PLAY;
      }

      if (IndexOf(this->value, valueLength, RTSP_PUBLIC_RESPONSE_HEADER_METHOD_RECORD, RTSP_PUBLIC_RESPONSE_HEADER_METHOD_RECORD_LENGTH) != (-1))
      {
        this->flags |= FLAG_RTSP_PUBLIC_RESPONSE_HEADER_METHOD_RECORD;
      }

      if (IndexOf(this->value, valueLength, RTSP_PUBLIC_RESPONSE_HEADER_METHOD_REDIRECT, RTSP_PUBLIC_RESPONSE_HEADER_METHOD_REDIRECT_LENGTH) != (-1))
      {
        this->flags |= FLAG_RTSP_PUBLIC_RESPONSE_HEADER_METHOD_REDIRECT;
      }

      if (IndexOf(this->value, valueLength, RTSP_PUBLIC_RESPONSE_HEADER_METHOD_SETUP, RTSP_PUBLIC_RESPONSE_HEADER_METHOD_SETUP_LENGTH) != (-1))
      {
        this->flags |= FLAG_RTSP_PUBLIC_RESPONSE_HEADER_METHOD_SETUP;
      }

      if (IndexOf(this->value, valueLength, RTSP_PUBLIC_RESPONSE_HEADER_METHOD_SET_PARAMETER, RTSP_PUBLIC_RESPONSE_HEADER_METHOD_SET_PARAMETER_LENGTH) != (-1))
      {
        this->flags |= FLAG_RTSP_PUBLIC_RESPONSE_HEADER_METHOD_SET_PARAMETER;
      }

      if (IndexOf(this->value, valueLength, RTSP_PUBLIC_RESPONSE_HEADER_METHOD_TEARDOWN, RTSP_PUBLIC_RESPONSE_HEADER_METHOD_TEARDOWN_LENGTH) != (-1))
      {
        this->flags |= FLAG_RTSP_PUBLIC_RESPONSE_HEADER_METHOD_TEARDOWN;
      }
    }
  }

  if (result)
  {
    this->responseHeaderType = Duplicate(RTSP_PUBLIC_RESPONSE_HEADER_TYPE);
    result &= (this->responseHeaderType != NULL);
  }

  return result;
}

bool CRtspPublicResponseHeader::IsDefinedDescribeMethod(void)
{
  return this->IsSetFlag(FLAG_RTSP_PUBLIC_RESPONSE_HEADER_METHOD_DESCRIBE);
}

bool CRtspPublicResponseHeader::IsDefinedAnnounceMethod(void)
{
  return this->IsSetFlag(FLAG_RTSP_PUBLIC_RESPONSE_HEADER_METHOD_ANNOUNCE);
}

bool CRtspPublicResponseHeader::IsDefinedGetParameterMethod(void)
{
  return this->IsSetFlag(FLAG_RTSP_PUBLIC_RESPONSE_HEADER_METHOD_GET_PARAMETER);
}

bool CRtspPublicResponseHeader::IsDefinedOptionsMethod(void)
{
  return this->IsSetFlag(FLAG_RTSP_PUBLIC_RESPONSE_HEADER_METHOD_OPTIONS);
}

bool CRtspPublicResponseHeader::IsDefinedPauseMethod(void)
{
  return this->IsSetFlag(FLAG_RTSP_PUBLIC_RESPONSE_HEADER_METHOD_PAUSE);
}

bool CRtspPublicResponseHeader::IsDefinedPlayMethod(void)
{
  return this->IsSetFlag(FLAG_RTSP_PUBLIC_RESPONSE_HEADER_METHOD_PLAY);
}

bool CRtspPublicResponseHeader::IsDefinedRecordMethod(void)
{
  return this->IsSetFlag(FLAG_RTSP_PUBLIC_RESPONSE_HEADER_METHOD_RECORD);
}

bool CRtspPublicResponseHeader::IsDefinedRedirectMethod(void)
{
  return this->IsSetFlag(FLAG_RTSP_PUBLIC_RESPONSE_HEADER_METHOD_REDIRECT);
}

bool CRtspPublicResponseHeader::IsDefinedSetupMethod(void)
{
  return this->IsSetFlag(FLAG_RTSP_PUBLIC_RESPONSE_HEADER_METHOD_SETUP);
}

bool CRtspPublicResponseHeader::IsDefinedSetParameterMethod(void)
{
  return this->IsSetFlag(FLAG_RTSP_PUBLIC_RESPONSE_HEADER_METHOD_SET_PARAMETER);
}

bool CRtspPublicResponseHeader::IsDefinedTeardownMethod(void)
{
  return this->IsSetFlag(FLAG_RTSP_PUBLIC_RESPONSE_HEADER_METHOD_TEARDOWN);
}

bool CRtspPublicResponseHeader::IsSetFlag(unsigned int flag)
{
  return ((this->flags & flag) == flag);
}
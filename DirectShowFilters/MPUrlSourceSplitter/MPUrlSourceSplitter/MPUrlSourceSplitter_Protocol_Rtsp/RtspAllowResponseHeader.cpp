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

#include "RtspAllowResponseHeader.h"

CRtspAllowResponseHeader::CRtspAllowResponseHeader(void)
  : CRtspResponseHeader()
{
  this->flags = FLAG_RTSP_ALLOW_RESPONSE_HEADER_METHOD_NONE;
}

CRtspAllowResponseHeader::~CRtspAllowResponseHeader(void)
{
}

/* get methods */

/* set methods */

/* other methods */

CRtspAllowResponseHeader *CRtspAllowResponseHeader::Clone(void)
{
  return (CRtspAllowResponseHeader *)__super::Clone();
}

bool CRtspAllowResponseHeader::CloneInternal(CHttpHeader *clonedHeader)
{
  bool result = __super::CloneInternal(clonedHeader);
  CRtspAllowResponseHeader *header = dynamic_cast<CRtspAllowResponseHeader *>(clonedHeader);
  result &= (header != NULL);

  if (result)
  {
    header->flags = this->flags;
  }

  return result;
}

CHttpHeader *CRtspAllowResponseHeader::GetNewHeader(void)
{
  return new CRtspAllowResponseHeader();
}

bool CRtspAllowResponseHeader::Parse(const wchar_t *header, unsigned int length)
{
  bool result = __super::Parse(header, length);

  if (result)
  {
    result &= (_wcsicmp(this->name, RTSP_ALLOW_RESPONSE_HEADER_TYPE) == 0);

    if (result)
    {
      unsigned int valueLength = wcslen(this->value);
      if (IndexOf(this->value, valueLength, RTSP_ALLOW_RESPONSE_HEADER_METHOD_DESCRIBE, RTSP_ALLOW_RESPONSE_HEADER_METHOD_DESCRIBE_LENGTH) != (-1))
      {
        this->flags |= FLAG_RTSP_ALLOW_RESPONSE_HEADER_METHOD_DESCRIBE;
      }

      if (IndexOf(this->value, valueLength, RTSP_ALLOW_RESPONSE_HEADER_METHOD_ANNOUNCE, RTSP_ALLOW_RESPONSE_HEADER_METHOD_ANNOUNCE_LENGTH) != (-1))
      {
        this->flags |= FLAG_RTSP_ALLOW_RESPONSE_HEADER_METHOD_ANNOUNCE;
      }

      if (IndexOf(this->value, valueLength, RTSP_ALLOW_RESPONSE_HEADER_METHOD_GET_PARAMETER, RTSP_ALLOW_RESPONSE_HEADER_METHOD_GET_PARAMETER_LENGTH) != (-1))
      {
        this->flags |= FLAG_RTSP_ALLOW_RESPONSE_HEADER_METHOD_GET_PARAMETER;
      }

      if (IndexOf(this->value, valueLength, RTSP_ALLOW_RESPONSE_HEADER_METHOD_OPTIONS, RTSP_ALLOW_RESPONSE_HEADER_METHOD_OPTIONS_LENGTH) != (-1))
      {
        this->flags |= FLAG_RTSP_ALLOW_RESPONSE_HEADER_METHOD_OPTIONS;
      }

      if (IndexOf(this->value, valueLength, RTSP_ALLOW_RESPONSE_HEADER_METHOD_PAUSE, RTSP_ALLOW_RESPONSE_HEADER_METHOD_PAUSE_LENGTH) != (-1))
      {
        this->flags |= FLAG_RTSP_ALLOW_RESPONSE_HEADER_METHOD_PAUSE;
      }

      if (IndexOf(this->value, valueLength, RTSP_ALLOW_RESPONSE_HEADER_METHOD_PLAY, RTSP_ALLOW_RESPONSE_HEADER_METHOD_PLAY_LENGTH) != (-1))
      {
        this->flags |= FLAG_RTSP_ALLOW_RESPONSE_HEADER_METHOD_PLAY;
      }

      if (IndexOf(this->value, valueLength, RTSP_ALLOW_RESPONSE_HEADER_METHOD_RECORD, RTSP_ALLOW_RESPONSE_HEADER_METHOD_RECORD_LENGTH) != (-1))
      {
        this->flags |= FLAG_RTSP_ALLOW_RESPONSE_HEADER_METHOD_RECORD;
      }

      if (IndexOf(this->value, valueLength, RTSP_ALLOW_RESPONSE_HEADER_METHOD_REDIRECT, RTSP_ALLOW_RESPONSE_HEADER_METHOD_REDIRECT_LENGTH) != (-1))
      {
        this->flags |= FLAG_RTSP_ALLOW_RESPONSE_HEADER_METHOD_REDIRECT;
      }

      if (IndexOf(this->value, valueLength, RTSP_ALLOW_RESPONSE_HEADER_METHOD_SETUP, RTSP_ALLOW_RESPONSE_HEADER_METHOD_SETUP_LENGTH) != (-1))
      {
        this->flags |= FLAG_RTSP_ALLOW_RESPONSE_HEADER_METHOD_SETUP;
      }

      if (IndexOf(this->value, valueLength, RTSP_ALLOW_RESPONSE_HEADER_METHOD_SET_PARAMETER, RTSP_ALLOW_RESPONSE_HEADER_METHOD_SET_PARAMETER_LENGTH) != (-1))
      {
        this->flags |= FLAG_RTSP_ALLOW_RESPONSE_HEADER_METHOD_SET_PARAMETER;
      }

      if (IndexOf(this->value, valueLength, RTSP_ALLOW_RESPONSE_HEADER_METHOD_TEARDOWN, RTSP_ALLOW_RESPONSE_HEADER_METHOD_TEARDOWN_LENGTH) != (-1))
      {
        this->flags |= FLAG_RTSP_ALLOW_RESPONSE_HEADER_METHOD_TEARDOWN;
      }
    }
  }

  if (result)
  {
    this->responseHeaderType = Duplicate(RTSP_ALLOW_RESPONSE_HEADER_TYPE);
    result &= (this->responseHeaderType != NULL);
  }

  return result;
}

bool CRtspAllowResponseHeader::IsDefinedDescribeMethod(void)
{
  return this->IsSetFlag(FLAG_RTSP_ALLOW_RESPONSE_HEADER_METHOD_DESCRIBE);
}

bool CRtspAllowResponseHeader::IsDefinedAnnounceMethod(void)
{
  return this->IsSetFlag(FLAG_RTSP_ALLOW_RESPONSE_HEADER_METHOD_ANNOUNCE);
}

bool CRtspAllowResponseHeader::IsDefinedGetParameterMethod(void)
{
  return this->IsSetFlag(FLAG_RTSP_ALLOW_RESPONSE_HEADER_METHOD_GET_PARAMETER);
}

bool CRtspAllowResponseHeader::IsDefinedOptionsMethod(void)
{
  return this->IsSetFlag(FLAG_RTSP_ALLOW_RESPONSE_HEADER_METHOD_OPTIONS);
}

bool CRtspAllowResponseHeader::IsDefinedPauseMethod(void)
{
  return this->IsSetFlag(FLAG_RTSP_ALLOW_RESPONSE_HEADER_METHOD_PAUSE);
}

bool CRtspAllowResponseHeader::IsDefinedPlayMethod(void)
{
  return this->IsSetFlag(FLAG_RTSP_ALLOW_RESPONSE_HEADER_METHOD_PLAY);
}

bool CRtspAllowResponseHeader::IsDefinedRecordMethod(void)
{
  return this->IsSetFlag(FLAG_RTSP_ALLOW_RESPONSE_HEADER_METHOD_RECORD);
}

bool CRtspAllowResponseHeader::IsDefinedRedirectMethod(void)
{
  return this->IsSetFlag(FLAG_RTSP_ALLOW_RESPONSE_HEADER_METHOD_REDIRECT);
}

bool CRtspAllowResponseHeader::IsDefinedSetupMethod(void)
{
  return this->IsSetFlag(FLAG_RTSP_ALLOW_RESPONSE_HEADER_METHOD_SETUP);
}

bool CRtspAllowResponseHeader::IsDefinedSetParameterMethod(void)
{
  return this->IsSetFlag(FLAG_RTSP_ALLOW_RESPONSE_HEADER_METHOD_SET_PARAMETER);
}

bool CRtspAllowResponseHeader::IsDefinedTeardownMethod(void)
{
  return this->IsSetFlag(FLAG_RTSP_ALLOW_RESPONSE_HEADER_METHOD_TEARDOWN);
}

bool CRtspAllowResponseHeader::IsSetFlag(unsigned int flag)
{
  return ((this->flags & flag) == flag);
}
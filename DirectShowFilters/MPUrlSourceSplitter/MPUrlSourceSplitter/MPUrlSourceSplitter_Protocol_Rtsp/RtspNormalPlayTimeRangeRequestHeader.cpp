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

#include "RtspNormalPlayTimeRangeRequestHeader.h"

CRtspNormalPlayTimeRangeRequestHeader::CRtspNormalPlayTimeRangeRequestHeader(void)
  : CRtspRangeRequestHeader()
{
  this->flags = FLAG_NORMAL_PLAY_TIME_NONE;
  this->startTime = TIME_UNSPECIFIED;
  this->endTime = TIME_UNSPECIFIED;
}

CRtspNormalPlayTimeRangeRequestHeader::~CRtspNormalPlayTimeRangeRequestHeader(void)
{
}

/* get methods */

const wchar_t *CRtspNormalPlayTimeRangeRequestHeader::GetValue(void)
{
  FREE_MEM(this->value);

  const wchar_t *format = NULL;

  if (this->IsSetFlag(FLAG_NORMAL_PLAY_TIME_START | FLAG_NORMAL_PLAY_TIME_END))
  {
    format = NORMAL_PLAY_TIME_START_END_VALUE_FORMAT;
  }
  else if (this->IsSetFlag(FLAG_NORMAL_PLAY_TIME_START))
  {
    format = NORMAL_PLAY_TIME_START_VALUE_FORMAT;
  }
  else if (this->IsSetFlag(FLAG_NORMAL_PLAY_TIME_END))
  {
    format = NORMAL_PLAY_TIME_END_VALUE_FORMAT;
  }

  if (format != NULL)
  {
    this->value = FormatString(format, this->GetStartTime(), this->GetEndTime());
  }

  return __super::GetValue();
}

unsigned int CRtspNormalPlayTimeRangeRequestHeader::GetStartTime(void)
{
  return this->startTime;
}

unsigned int CRtspNormalPlayTimeRangeRequestHeader::GetEndTime(void)
{
  return this->endTime;
}

unsigned int CRtspNormalPlayTimeRangeRequestHeader::GetFlags(void)
{
  return this->flags;
}

/* set methods */

bool CRtspNormalPlayTimeRangeRequestHeader::SetValue(const wchar_t *value)
{
  // we never set value
  return false;
}

void CRtspNormalPlayTimeRangeRequestHeader::SetStartTime(unsigned int startTime)
{
  this->startTime = startTime;
}

void CRtspNormalPlayTimeRangeRequestHeader::SetEndTime(unsigned int endTime)
{
  this->endTime = endTime;
}

void CRtspNormalPlayTimeRangeRequestHeader::SetFlags(unsigned int flags)
{
  this->flags = flags;
}

/* other methods */

bool CRtspNormalPlayTimeRangeRequestHeader::IsSetStart(void)
{
  return this->IsSetFlag(FLAG_NORMAL_PLAY_TIME_START);
}

bool CRtspNormalPlayTimeRangeRequestHeader::IsSetEnd(void)
{
  return this->IsSetFlag(FLAG_NORMAL_PLAY_TIME_END);
}

bool CRtspNormalPlayTimeRangeRequestHeader::IsSetFlag(unsigned int flag)
{
  return ((this->flags & flag) == flag);
}

CRtspNormalPlayTimeRangeRequestHeader *CRtspNormalPlayTimeRangeRequestHeader::Clone(void)
{
  return (CRtspNormalPlayTimeRangeRequestHeader *)__super::Clone();
}

bool CRtspNormalPlayTimeRangeRequestHeader::CloneInternal(CHttpHeader *clonedHeader)
{
  bool result = __super::CloneInternal(clonedHeader);
  CRtspNormalPlayTimeRangeRequestHeader *header = dynamic_cast<CRtspNormalPlayTimeRangeRequestHeader *>(clonedHeader);
  result &= (header != NULL);

  if (result)
  {
    header->flags = this->flags;
    header->startTime = this->startTime;
    header->endTime = this->endTime;
  }

  return result;
}

CHttpHeader *CRtspNormalPlayTimeRangeRequestHeader::GetNewHeader(void)
{
  return new CRtspNormalPlayTimeRangeRequestHeader();
}
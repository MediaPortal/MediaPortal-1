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

#include "Stream.h"

CStream::CStream(void)
{
  this->language = NULL;
  this->pid = 0;
  this->streamInfo = NULL;
}

CStream::~CStream(void)
{
  FREE_MEM(this->language);
  FREE_MEM_CLASS(this->streamInfo);
}

/* get methods */

CStreamInfo *CStream::GetStreamInfo(void)
{
  return this->streamInfo;
}

unsigned int CStream::GetPid(void)
{
  return this->pid;
}

const wchar_t *CStream::GetLanguage(void)
{
  return this->language;
}

/* set methods */

void CStream::SetPid(unsigned int pid)
{
  this->pid = pid;
}

bool CStream::SetLanguage(const wchar_t *language)
{
  SET_STRING_RETURN_WITH_NULL(this->language, language);
}

/* other methods */

HRESULT CStream::CreateStreamInfo(void)
{
  FREE_MEM_CLASS(this->streamInfo);
  this->streamInfo = new CStreamInfo();

  return (this->streamInfo != NULL) ? S_OK : E_OUTOFMEMORY;
}

HRESULT CStream::CreateStreamInfo(AVFormatContext *formatContext, AVStream *stream, const wchar_t *containerFormat)
{
  FREE_MEM_CLASS(this->streamInfo);
  HRESULT result = S_OK;

  this->streamInfo = new CStreamInfo(formatContext, stream, containerFormat, &result);
  CHECK_POINTER_HRESULT(result, this->streamInfo, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(this->streamInfo));
  return result;
}
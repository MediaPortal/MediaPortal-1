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

CStream::CStream(HRESULT *result)
  : CFlags()
{
  this->language = NULL;
  this->pid = 0;
  this->streamInfo = NULL;
  this->streamType = Unknown;
  this->seekIndexEntries = NULL;
  this->formatContext = NULL;
  this->stream = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->seekIndexEntries = new CSeekIndexEntryCollection(result);
    CHECK_POINTER_HRESULT(*result, this->seekIndexEntries, *result, E_OUTOFMEMORY);
  }
}

CStream::~CStream(void)
{
  FREE_MEM(this->language);
  FREE_MEM_CLASS(this->streamInfo);
  FREE_MEM_CLASS(this->seekIndexEntries);
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

CStream::StreamType CStream::GetStreamType(void)
{
  return this->streamType;
}

CSeekIndexEntryCollection *CStream::GetSeekIndexEntries(void)
{
  return this->seekIndexEntries;
}

AVFormatContext *CStream::GetAVFormatContext(void)
{
  return this->formatContext;
}

AVStream *CStream::GetAVStream(void)
{
  return this->stream;
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

void CStream::SetStreamType(StreamType streamType)
{
  this->streamType = streamType;
}

void CStream::SetDiscontinuity(bool discontinuity)
{
  this->flags &= ~STREAM_FLAG_DISCONTINUITY;
  this->flags |= discontinuity ? STREAM_FLAG_DISCONTINUITY : STREAM_FLAG_NONE;
}

/* other methods */

CStream *CStream::Clone(void)
{
  HRESULT result = S_OK;
  CStream *stream = new CStream(&result);
  CHECK_POINTER_HRESULT(result, stream, result, E_OUTOFMEMORY);

  if (SUCCEEDED(result))
  {
    SET_STRING_HRESULT_WITH_NULL(stream->language, this->language, result);
    stream->pid = this->pid;
    stream->streamType = this->streamType;
    stream->formatContext = this->formatContext;
    stream->stream = this->stream;
    stream->flags = this->flags;

    CHECK_CONDITION_HRESULT(result, stream->seekIndexEntries->Append(this->seekIndexEntries), result, E_OUTOFMEMORY);
    stream->streamInfo = (this->streamInfo != NULL) ? this->streamInfo->Clone() : NULL;
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(stream));
  return stream;
}

bool CStream::IsDiscontinuity(void)
{
  return this->IsSetFlags(STREAM_FLAG_DISCONTINUITY);
}

HRESULT CStream::CreateStreamInfo(AVFormatContext *formatContext, AVStream *stream, const wchar_t *containerFormat)
{
  FREE_MEM_CLASS(this->streamInfo);
  HRESULT result = S_OK;

  this->streamInfo = new CStreamInfo(&result, formatContext, stream, containerFormat);
  CHECK_POINTER_HRESULT(result, this->streamInfo, result, E_OUTOFMEMORY);

  if (SUCCEEDED(result))
  {
    this->formatContext = formatContext;
    this->stream = stream;
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(this->streamInfo));
  return result;
}
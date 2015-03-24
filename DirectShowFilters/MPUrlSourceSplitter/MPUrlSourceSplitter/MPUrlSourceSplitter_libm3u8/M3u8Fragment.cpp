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

#include "M3u8Fragment.h"

CM3u8Fragment::CM3u8Fragment(HRESULT *result)
  : CFlags()
{
  this->sequenceNumber = SEQUENCE_NUMBER_NOT_SPECIFIED;
  this->duration = DURATION_NOT_SPECIFIED;
  this->uri = NULL;
  this->offset = OFFSET_NOT_SPECIFED;
  this->length = LENGTH_NOT_SPECIFIED;
}

CM3u8Fragment::~CM3u8Fragment(void)
{
  FREE_MEM(this->uri);
}

/* get methods */

unsigned int CM3u8Fragment::GetSequenceNumber(void)
{
  return this->sequenceNumber;
}

unsigned int CM3u8Fragment::GetDuration(void)
{
  return this->duration;
}

const wchar_t *CM3u8Fragment::GetUri(void)
{
  return this->uri;
}

unsigned int CM3u8Fragment::GetOffset(void)
{
  return this->offset;
}

unsigned int CM3u8Fragment::GetLength(void)
{
  return this->length;
}

/* set methods */

void CM3u8Fragment::SetSequenceNumber(unsigned int sequenceNumber)
{
  this->sequenceNumber = sequenceNumber;
}

void CM3u8Fragment::SetDuration(unsigned int duration)
{
  this->duration = duration;
}

bool CM3u8Fragment::SetUri(const wchar_t *uri)
{
  SET_STRING_RETURN_WITH_NULL(this->uri, uri);
}

void CM3u8Fragment::SetOffset(unsigned int offset)
{
  this->offset = offset;
}

void CM3u8Fragment::SetLength(unsigned int length)
{
  this->length = length;
}

void CM3u8Fragment::SetDiscontinuity(bool discontinuity)
{
  this->flags &= ~M3U8_FRAGMENT_FLAG_DISCONTINUITY;
  this->flags |= discontinuity ? M3U8_FRAGMENT_FLAG_DISCONTINUITY : M3U8_FRAGMENT_FLAG_NONE;
}

void CM3u8Fragment::SetEncrypted(bool ecnrypted)
{
  this->flags &= ~M3U8_FRAGMENT_FLAG_ENCRYPTED;
  this->flags |= ecnrypted ? M3U8_FRAGMENT_FLAG_ENCRYPTED : M3U8_FRAGMENT_FLAG_NONE;
}

void CM3u8Fragment::SetEndOfStream(bool endOfStream)
{
  this->flags &= ~M3U8_FRAGMENT_FLAG_END_OF_STREAM;
  this->flags |= endOfStream ? M3U8_FRAGMENT_FLAG_END_OF_STREAM : M3U8_FRAGMENT_FLAG_NONE;
}

/* other methods */

bool CM3u8Fragment::IsDiscontinuity(void)
{
  return this->IsSetFlags(M3U8_FRAGMENT_FLAG_DISCONTINUITY);
}

bool CM3u8Fragment::IsEncrypted(void)
{
  return this->IsSetFlags(M3U8_FRAGMENT_FLAG_ENCRYPTED);
}

bool CM3u8Fragment::IsEndOfStream(void)
{
  return this->IsSetFlags(M3U8_FRAGMENT_FLAG_END_OF_STREAM);
}

/* protected methods */
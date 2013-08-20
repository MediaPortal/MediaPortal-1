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

#include "MSHSStream.h"
#include "MSHS_Elements.h"
#include "BufferHelper.h"

CMSHSStream::CMSHSStream(void)
  : CSerializable()
{
  this->displayHeight = 0;
  this->displayWidth = 0;
  this->maxHeight = 0;
  this->maxWidth = 0;
  this->name = NULL;
  this->subType = NULL;
  this->timeScale = 0;
  this->type = NULL;
  this->url = NULL;
  this->tracks = new CMSHSTrackCollection();
  this->streamFragments = new CMSHSStreamFragmentCollection();
}

CMSHSStream::~CMSHSStream(void)
{
  FREE_MEM(this->name);
  FREE_MEM(this->subType);
  FREE_MEM(this->type);
  FREE_MEM(this->url);
  FREE_MEM_CLASS(this->tracks);
  FREE_MEM_CLASS(this->streamFragments);
}

/* get methods */

const wchar_t *CMSHSStream::GetType(void)
{
  return this->type;
}

const wchar_t *CMSHSStream::GetSubType(void)
{
  return this->subType;
}

const wchar_t *CMSHSStream::GetUrl(void)
{
  return this->url;
}

uint64_t CMSHSStream::GetTimeScale(void)
{
  return this->timeScale;
}

const wchar_t *CMSHSStream::GetName(void)
{
  return this->name;
}

uint32_t CMSHSStream::GetMaxWidth(void)
{
  return this->maxWidth;
}

uint32_t CMSHSStream::GetMaxHeight(void)
{
  return this->maxHeight;
}

uint32_t CMSHSStream::GetDisplayWidth(void)
{
  return this->displayWidth;
}

uint32_t CMSHSStream::GetDisplayHeight(void)
{
  return this->displayHeight;
}

CMSHSTrackCollection *CMSHSStream::GetTracks(void)
{
  return this->tracks;
}

CMSHSStreamFragmentCollection *CMSHSStream::GetStreamFragments(void)
{
  return this->streamFragments;
}

/* set methods */

bool CMSHSStream::SetType(const wchar_t *type)
{
  SET_STRING_RETURN_WITH_NULL(this->type, type);
}

bool CMSHSStream::SetSubType(const wchar_t *subType)
{
  SET_STRING_RETURN_WITH_NULL(this->subType, subType);
}

bool CMSHSStream::SetUrl(const wchar_t *url)
{
  SET_STRING_RETURN_WITH_NULL(this->url, url);
}

void CMSHSStream::SetTimeScale(uint64_t timeScale)
{
  this->timeScale = timeScale;
}

bool CMSHSStream::SetName(const wchar_t *name)
{
  SET_STRING_RETURN_WITH_NULL(this->name, name);
}

void CMSHSStream::SetMaxWidth(uint32_t maxWidth)
{
  this->maxWidth = maxWidth;
}

void CMSHSStream::SetMaxHeight(uint32_t maxHeight)
{
  this->maxHeight = maxHeight;
}

void CMSHSStream::SetDisplayWidth(uint32_t displayWidth)
{
  this->displayWidth = displayWidth;
}

void CMSHSStream::SetDisplayHeight(uint32_t displayHeight)
{
  this->displayHeight = displayHeight;
}

/* other methods */

bool CMSHSStream::IsVideo(void)
{
  return (wcscmp(this->GetType(), STREAM_TYPE_VIDEO) == 0);
}

bool CMSHSStream::IsAudio(void)
{
  return (wcscmp(this->GetType(), STREAM_TYPE_AUDIO) == 0);
}

bool CMSHSStream::IsText(void)
{
  return (wcscmp(this->GetType(), STREAM_TYPE_TEXT) == 0);
}

uint32_t CMSHSStream::GetSerializeSize(void)
{
  uint32_t required = 24;
  required += this->GetSerializeStringSize(this->type);
  required += this->GetSerializeStringSize(this->subType);
  required += this->GetSerializeStringSize(this->url);
  required += this->GetSerializeStringSize(this->name);
  required += this->tracks->GetSerializeSize();
  required += this->streamFragments->GetSerializeSize();

  return required;
}

bool CMSHSStream::Serialize(uint8_t *buffer)
{
  bool result = __super::Serialize(buffer);
  uint32_t position = __super::GetSerializeSize();

  // store type
  result &= this->SerializeString(buffer + position, this->type);
  position += this->GetSerializeStringSize(this->type);

  // store subType
  result &= this->SerializeString(buffer + position, this->subType);
  position += this->GetSerializeStringSize(this->subType);

  // store url
  result &= this->SerializeString(buffer + position, this->url);
  position += this->GetSerializeStringSize(this->url);

  WBE64INC(buffer, position, this->timeScale);

  // store name
  result &= this->SerializeString(buffer + position, this->name);
  position += this->GetSerializeStringSize(this->name);

  WBE32INC(buffer, position, this->maxWidth);
  WBE32INC(buffer, position, this->maxHeight);
  WBE32INC(buffer, position, this->displayWidth);
  WBE32INC(buffer, position, this->displayHeight);

  // store tracks
  result &= this->tracks->Serialize(buffer + position);
  position += this->tracks->GetSerializeSize();

  // store stream fragments
  result &= this->streamFragments->Serialize(buffer + position);
  position += this->streamFragments->GetSerializeSize();

  return result;
}

bool CMSHSStream::Deserialize(const uint8_t *buffer)
{
  FREE_MEM_CLASS(this->tracks);
  FREE_MEM_CLASS(this->streamFragments);
  FREE_MEM(this->type);
  FREE_MEM(this->subType);
  FREE_MEM(this->url);
  FREE_MEM(this->name);

  this->tracks = new CMSHSTrackCollection();
  this->streamFragments = new CMSHSStreamFragmentCollection();

  bool result = (__super::Deserialize(buffer) && (this->tracks != NULL) && (this->streamFragments != NULL));
  uint32_t position = __super::GetSerializeSize();

  // store type
  if (result)
  {
    result &= this->DeserializeString(buffer + position, &this->type);
    position += this->GetSerializeStringSize(this->type);
  }

  // store subType
  if (result)
  {
    result &= this->DeserializeString(buffer + position, &this->subType);
    position += this->GetSerializeStringSize(this->subType);
  }

  // store url
  if (result)
  {
    result &= this->DeserializeString(buffer + position, &this->url);
    position += this->GetSerializeStringSize(this->url);
  }

  if (result)
  {
    RBE64INC(buffer, position, this->timeScale);
  }

  // store name
  if (result)
  {
    result &= this->DeserializeString(buffer + position, &this->name);
    position += this->GetSerializeStringSize(this->name);
  }

  if (result)
  {
    RBE32INC(buffer, position, this->maxWidth);
    RBE32INC(buffer, position, this->maxHeight);
    RBE32INC(buffer, position, this->displayWidth);
    RBE32INC(buffer, position, this->displayHeight);
  }

  // store tracks
  if (result)
  {
    result &= this->tracks->Deserialize(buffer + position);
    position += this->tracks->GetSerializeSize();
  }

  // store stream fragments
  if (result)
  {
    result &= this->streamFragments->Deserialize(buffer + position);
    position += this->streamFragments->GetSerializeSize();
  }

  return result;
}
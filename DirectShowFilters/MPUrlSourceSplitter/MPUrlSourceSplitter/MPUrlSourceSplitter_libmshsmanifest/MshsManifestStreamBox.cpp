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

#include "MshsManifestStreamBox.h"
#include "MshsManifestElements.h"
#include "BoxCollection.h"
#include "BufferHelper.h"
#include "BoxConstants.h"

CMshsManifestStreamBox::CMshsManifestStreamBox(HRESULT *result)
  : CBox(result)
{
  this->displayHeight = 0;
  this->displayWidth = 0;
  this->maxHeight = 0;
  this->maxWidth = 0;
  this->name = NULL;
  this->subType = NULL;
  this->timeScale = 0;
  this->streamBoxType = NULL;
  this->url = NULL;
  this->tracks = NULL;
  this->streamFragments = NULL;
  this->type = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->tracks = new CMshsManifestTrackBoxCollection(result);
    this->streamFragments = new CMshsManifestStreamFragmentBoxCollection(result);
    this->type = Duplicate(MSHS_MANIFEST_STREAM_BOX_TYPE);

    CHECK_POINTER_HRESULT(*result, this->type, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->tracks, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->streamFragments, *result, E_OUTOFMEMORY);
  }
}

CMshsManifestStreamBox::~CMshsManifestStreamBox(void)
{
  FREE_MEM(this->name);
  FREE_MEM(this->subType);
  FREE_MEM(this->streamBoxType);
  FREE_MEM(this->url);
  FREE_MEM_CLASS(this->tracks);
  FREE_MEM_CLASS(this->streamFragments);
}

/* get methods */

const wchar_t *CMshsManifestStreamBox::GetStreamBoxType(void)
{
  return this->streamBoxType;
}

const wchar_t *CMshsManifestStreamBox::GetSubType(void)
{
  return this->subType;
}

const wchar_t *CMshsManifestStreamBox::GetUrl(void)
{
  return this->url;
}

uint64_t CMshsManifestStreamBox::GetTimeScale(void)
{
  return this->timeScale;
}

const wchar_t *CMshsManifestStreamBox::GetName(void)
{
  return this->name;
}

uint32_t CMshsManifestStreamBox::GetMaxWidth(void)
{
  return this->maxWidth;
}

uint32_t CMshsManifestStreamBox::GetMaxHeight(void)
{
  return this->maxHeight;
}

uint32_t CMshsManifestStreamBox::GetDisplayWidth(void)
{
  return this->displayWidth;
}

uint32_t CMshsManifestStreamBox::GetDisplayHeight(void)
{
  return this->displayHeight;
}

CMshsManifestTrackBoxCollection *CMshsManifestStreamBox::GetTracks(void)
{
  return this->tracks;
}

CMshsManifestStreamFragmentBoxCollection *CMshsManifestStreamBox::GetStreamFragments(void)
{
  return this->streamFragments;
}

/* set methods */

bool CMshsManifestStreamBox::SetStreamBoxType(const wchar_t *streamBoxType)
{
  SET_STRING_RETURN_WITH_NULL(this->streamBoxType, streamBoxType);
}

bool CMshsManifestStreamBox::SetSubType(const wchar_t *subType)
{
  SET_STRING_RETURN_WITH_NULL(this->subType, subType);
}

bool CMshsManifestStreamBox::SetUrl(const wchar_t *url)
{
  SET_STRING_RETURN_WITH_NULL(this->url, url);
}

void CMshsManifestStreamBox::SetTimeScale(uint64_t timeScale)
{
  this->timeScale = timeScale;
}

bool CMshsManifestStreamBox::SetName(const wchar_t *name)
{
  SET_STRING_RETURN_WITH_NULL(this->name, name);
}

void CMshsManifestStreamBox::SetMaxWidth(uint32_t maxWidth)
{
  this->maxWidth = maxWidth;
}

void CMshsManifestStreamBox::SetMaxHeight(uint32_t maxHeight)
{
  this->maxHeight = maxHeight;
}

void CMshsManifestStreamBox::SetDisplayWidth(uint32_t displayWidth)
{
  this->displayWidth = displayWidth;
}

void CMshsManifestStreamBox::SetDisplayHeight(uint32_t displayHeight)
{
  this->displayHeight = displayHeight;
}

/* other methods */

bool CMshsManifestStreamBox::IsVideo(void)
{
  return (wcscmp(this->GetStreamBoxType(), STREAM_TYPE_VIDEO) == 0);
}

bool CMshsManifestStreamBox::IsAudio(void)
{
  return (wcscmp(this->GetStreamBoxType(), STREAM_TYPE_AUDIO) == 0);
}

bool CMshsManifestStreamBox::IsText(void)
{
  return (wcscmp(this->GetStreamBoxType(), STREAM_TYPE_TEXT) == 0);
}

wchar_t *CMshsManifestStreamBox::GetParsedHumanReadable(const wchar_t *indent)
{
  return NULL;
}

/* protected methods */

uint64_t CMshsManifestStreamBox::GetBoxSize(void)
{
  uint64_t result = 48;

  result += (this->streamBoxType != NULL) ? (wcslen(this->streamBoxType) * sizeof(wchar_t)) : 0;
  result += (this->subType != NULL) ? (wcslen(this->subType) * sizeof(wchar_t)) : 0;
  result += (this->url != NULL) ? (wcslen(this->url) * sizeof(wchar_t)) : 0;
  result += (this->name != NULL) ? (wcslen(this->name) * sizeof(wchar_t)) : 0;

  for (unsigned int i = 0; i < this->tracks->Count(); i++)
  {
    result += this->tracks->GetItem(i)->GetSize();
  }
  for (unsigned int i = 0; i < this->streamFragments->Count(); i++)
  {
    result += this->streamFragments->GetItem(i)->GetSize();
  }

  uint64_t boxSize = __super::GetBoxSize();
  result = (boxSize != 0) ? (result + boxSize) : 0; 

  return result;
}

bool CMshsManifestStreamBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  FREE_MEM(this->type);
  FREE_MEM(this->subType);
  FREE_MEM(this->url);
  FREE_MEM(this->name);

  this->tracks->Clear();
  this->streamFragments->Clear();

  if (__super::ParseInternal(buffer, length, false))
  {
    this->flags &= ~BOX_FLAG_PARSED;
    this->flags |= (wcscmp(this->type, MSHS_MANIFEST_STREAM_BOX_TYPE) == 0) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;
    
    if (this->IsSetFlags(BOX_FLAG_PARSED))
    {
      // box is MSHS manifest stream box, parse all values
      uint32_t position = this->HasExtendedHeader() ? BOX_HEADER_LENGTH_SIZE64 : BOX_HEADER_LENGTH;
      HRESULT continueParsing = (this->GetSize() <= (uint64_t)length) ? S_OK : E_OUTOFMEMORY;

      if (SUCCEEDED(continueParsing))
      {
        RBE64INC(buffer, position, this->timeScale);
        RBE32INC(buffer, position, this->maxWidth);
        RBE32INC(buffer, position, this->maxHeight);
        RBE32INC(buffer, position, this->displayWidth);
        RBE32INC(buffer, position, this->displayHeight);

        RBE32INC_DEFINE(buffer, position, typeLength, uint32_t);
        // check if we have enough data in buffer for type
        CHECK_CONDITION_HRESULT(continueParsing, (position + typeLength * sizeof(wchar_t)) <= length, continueParsing, E_OUTOFMEMORY);

        if (SUCCEEDED(continueParsing) && (typeLength != 0))
        {
          this->streamBoxType = ALLOC_MEM_SET(this->streamBoxType, wchar_t, (typeLength + 1), 0);
          CHECK_POINTER_HRESULT(continueParsing, this->streamBoxType, continueParsing, E_OUTOFMEMORY);

          if (SUCCEEDED(continueParsing))
          {
            memcpy(this->streamBoxType, buffer + position, typeLength * sizeof(wchar_t));
            position += typeLength * sizeof(wchar_t);
          }
        }

        RBE32INC_DEFINE(buffer, position, subTypeLength, uint32_t);
        // check if we have enough data in buffer for sub type
        CHECK_CONDITION_HRESULT(continueParsing, (position + subTypeLength * sizeof(wchar_t)) <= length, continueParsing, E_OUTOFMEMORY);

        if (SUCCEEDED(continueParsing) && (subTypeLength != 0))
        {
          this->subType = ALLOC_MEM_SET(this->subType, wchar_t, (subTypeLength + 1), 0);
          CHECK_POINTER_HRESULT(continueParsing, this->subType, continueParsing, E_OUTOFMEMORY);

          if (SUCCEEDED(continueParsing))
          {
            memcpy(this->subType, buffer + position, subTypeLength * sizeof(wchar_t));
            position += subTypeLength * sizeof(wchar_t);
          }
        }

        RBE32INC_DEFINE(buffer, position, urlLength, uint32_t);
        // check if we have enough data in buffer for url
        CHECK_CONDITION_HRESULT(continueParsing, (position + urlLength * sizeof(wchar_t)) <= length, continueParsing, E_OUTOFMEMORY);

        if (SUCCEEDED(continueParsing) && (urlLength != 0))
        {
          this->url = ALLOC_MEM_SET(this->url, wchar_t, (urlLength + 1), 0);
          CHECK_POINTER_HRESULT(continueParsing, this->url, continueParsing, E_OUTOFMEMORY);

          if (SUCCEEDED(continueParsing))
          {
            memcpy(this->url, buffer + position, urlLength * sizeof(wchar_t));
            position += urlLength * sizeof(wchar_t);
          }
        }

        RBE32INC_DEFINE(buffer, position, nameLength, uint32_t);
        // check if we have enough data in buffer for name
        CHECK_CONDITION_HRESULT(continueParsing, (position + nameLength * sizeof(wchar_t)) <= length, continueParsing, E_OUTOFMEMORY);

        if (SUCCEEDED(continueParsing) && (nameLength != 0))
        {
          this->name = ALLOC_MEM_SET(this->name, wchar_t, (nameLength + 1), 0);
          CHECK_POINTER_HRESULT(continueParsing, this->name, continueParsing, E_OUTOFMEMORY);

          if (SUCCEEDED(continueParsing))
          {
            memcpy(this->name, buffer + position, nameLength * sizeof(wchar_t));
            position += nameLength * sizeof(wchar_t);
          }
        }

        RBE32INC_DEFINE(buffer, position, trackCount, uint32_t);

        for (uint32_t i = 0; (SUCCEEDED(continueParsing) && (i < trackCount)); i++)
        {
          CMshsManifestTrackBox *track = new CMshsManifestTrackBox(&continueParsing);
          CHECK_POINTER_HRESULT(continueParsing, track, continueParsing, E_OUTOFMEMORY);

          CHECK_CONDITION_HRESULT(continueParsing, track->Parse(buffer + position, length - position), continueParsing, E_OUTOFMEMORY);
          CHECK_CONDITION_HRESULT(continueParsing, this->tracks->Add(track), continueParsing, E_OUTOFMEMORY);

          CHECK_CONDITION_EXECUTE(SUCCEEDED(continueParsing), position += (uint32_t)track->GetSize());
          CHECK_CONDITION_EXECUTE(FAILED(continueParsing), FREE_MEM_CLASS(track));
        }

        RBE32INC_DEFINE(buffer, position, streamFragmentCount, uint32_t);

        for (uint32_t i = 0; (SUCCEEDED(continueParsing) && (i < streamFragmentCount)); i++)
        {
          CMshsManifestStreamFragmentBox *streamFragment = new CMshsManifestStreamFragmentBox(&continueParsing);
          CHECK_POINTER_HRESULT(continueParsing, streamFragment, continueParsing, E_OUTOFMEMORY);

          CHECK_CONDITION_HRESULT(continueParsing, streamFragment->Parse(buffer + position, length - position), continueParsing, E_OUTOFMEMORY);
          CHECK_CONDITION_HRESULT(continueParsing, this->streamFragments->Add(streamFragment), continueParsing, E_OUTOFMEMORY);

          CHECK_CONDITION_EXECUTE(SUCCEEDED(continueParsing), position += (uint32_t)streamFragment->GetSize());
          CHECK_CONDITION_EXECUTE(FAILED(continueParsing), FREE_MEM_CLASS(streamFragment));
        }
      }

      if (SUCCEEDED(continueParsing) && processAdditionalBoxes)
      {
        this->ProcessAdditionalBoxes(buffer, length, position);
      }
      
      this->flags &= ~BOX_FLAG_PARSED;
      this->flags |= SUCCEEDED(continueParsing) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;
    }
  }

  return this->IsSetFlags(BOX_FLAG_PARSED);
}

uint32_t CMshsManifestStreamBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    WBE64INC(buffer, result, this->timeScale);
    WBE32INC(buffer, result, this->maxWidth);
    WBE32INC(buffer, result, this->maxHeight);
    WBE32INC(buffer, result, this->displayWidth);
    WBE32INC(buffer, result, this->displayHeight);

    unsigned int typeLength = (this->streamBoxType != NULL) ? wcslen(this->streamBoxType) : 0;
    WBE32INC(buffer, result, typeLength);

    if (typeLength > 0)
    {
      memcpy(buffer + result, this->streamBoxType, typeLength * sizeof(wchar_t));
      result += typeLength * sizeof(wchar_t);
    }

    unsigned int subTypeLength = (this->subType != NULL) ? wcslen(this->subType) : 0;
    WBE32INC(buffer, result, subTypeLength);

    if (subTypeLength > 0)
    {
      memcpy(buffer + result, this->subType, subTypeLength * sizeof(wchar_t));
      result += subTypeLength * sizeof(wchar_t);
    }

    unsigned int urlLength = (this->url != NULL) ? wcslen(this->url) : 0;
    WBE32INC(buffer, result, urlLength);

    if (urlLength > 0)
    {
      memcpy(buffer + result, this->url, urlLength * sizeof(wchar_t));
      result += urlLength * sizeof(wchar_t);
    }

    unsigned int nameLength = (this->name != NULL) ? wcslen(this->name) : 0;
    WBE32INC(buffer, result, nameLength);

    if (nameLength > 0)
    {
      memcpy(buffer + result, this->name, nameLength * sizeof(wchar_t));
      result += nameLength * sizeof(wchar_t);
    }

    WBE32INC(buffer, result, this->tracks->Count());
    for (unsigned int i = 0; ((result != 0) && (i < this->tracks->Count())); i++)
    {
      result = this->tracks->GetItem(i)->GetBox(buffer + result, length - result) ? result : 0;
      result += (result != 0) ? (uint32_t)this->tracks->GetItem(i)->GetSize() : 0;
    }

    WBE32INC(buffer, result, this->streamFragments->Count());
    for (unsigned int i = 0; ((result != 0) && (i < this->streamFragments->Count())); i++)
    {
      result = this->streamFragments->GetItem(i)->GetBox(buffer + result, length - result) ? result : 0;
      result += (result != 0) ? (uint32_t)this->streamFragments->GetItem(i)->GetSize() : 0;
    }

    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}
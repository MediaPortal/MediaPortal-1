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

#include "MshsManifestSmoothStreamingMediaBox.h"
#include "BoxCollection.h"
#include "BufferHelper.h"
#include "BoxConstants.h"

CMshsManifestSmoothStreamingMediaBox::CMshsManifestSmoothStreamingMediaBox(HRESULT *result)
  : CBox(result)
{
  this->majorVersion = MANIFEST_MAJOR_VERSION;
  this->minorVersion = MANIFEST_MINOR_VERSION;
  this->timeScale = MANIFEST_TIMESCALE_DEFAULT;
  this->duration = 0;
  this->protections = NULL;
  this->streams = NULL;
  this->type = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->protections = new CMshsManifestProtectionBoxCollection(result);
    this->streams = new CMshsManifestStreamBoxCollection(result);
    this->type = Duplicate(MSHS_MANIFEST_SMOOTH_STREAMING_MEDIA_BOX_TYPE);

    CHECK_POINTER_HRESULT(*result, this->type, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->protections, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->streams, *result, E_OUTOFMEMORY);
  }
}

CMshsManifestSmoothStreamingMediaBox::~CMshsManifestSmoothStreamingMediaBox(void)
{
  FREE_MEM_CLASS(this->protections);
  FREE_MEM_CLASS(this->streams);
}

/* get methods */

uint32_t CMshsManifestSmoothStreamingMediaBox::GetMajorVersion(void)
{
  return this->majorVersion;
}

uint32_t CMshsManifestSmoothStreamingMediaBox::GetMinorVersion(void)
{
  return this->minorVersion;
}

uint64_t CMshsManifestSmoothStreamingMediaBox::GetTimeScale(void)
{
  return this->timeScale;
}

uint64_t CMshsManifestSmoothStreamingMediaBox::GetDuration(void)
{
  return this->duration;
}

CMshsManifestProtectionBoxCollection *CMshsManifestSmoothStreamingMediaBox::GetProtections(void)
{
  return this->protections;
}

CMshsManifestStreamBoxCollection *CMshsManifestSmoothStreamingMediaBox::GetStreams(void)
{
  return this->streams;
}

bool CMshsManifestSmoothStreamingMediaBox::GetBox(uint8_t *buffer, uint32_t length)
{
  return (this->GetBoxInternal(buffer, length, true) != 0);
}

/* set methods */

void CMshsManifestSmoothStreamingMediaBox::SetMajorVersion(uint32_t majorVersion)
{
  this->majorVersion = majorVersion;
}

void CMshsManifestSmoothStreamingMediaBox::SetMinorVersion(uint32_t minorVersion)
{
  this->minorVersion = minorVersion;
}

void CMshsManifestSmoothStreamingMediaBox::SetTimeScale(uint64_t timeScale)
{
  this->timeScale = timeScale;
}

void CMshsManifestSmoothStreamingMediaBox::SetDuration(uint64_t duration)
{
  this->duration = duration;
}

/* other methods */

bool CMshsManifestSmoothStreamingMediaBox::IsProtected(void)
{
  return (this->GetProtections()->Count() != 0);
}

wchar_t *CMshsManifestSmoothStreamingMediaBox::GetParsedHumanReadable(const wchar_t *indent)
{
  return NULL;
}

/* protected methods */

uint64_t CMshsManifestSmoothStreamingMediaBox::GetBoxSize(void)
{
  uint64_t result = 32;
  
  for (unsigned int i = 0; i < this->protections->Count(); i++)
  {
    result += this->protections->GetItem(i)->GetSize();
  }
  for (unsigned int i = 0; i < this->streams->Count(); i++)
  {
    result += this->streams->GetItem(i)->GetSize();
  }

  uint64_t boxSize = __super::GetBoxSize();
  result = (boxSize != 0) ? (result + boxSize) : 0; 

  return result;
}

bool CMshsManifestSmoothStreamingMediaBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  this->protections->Clear();
  this->streams->Clear();

  if (__super::ParseInternal(buffer, length, false))
  {
    this->flags &= ~BOX_FLAG_PARSED;
    this->flags |= (wcscmp(this->type, MSHS_MANIFEST_SMOOTH_STREAMING_MEDIA_BOX_TYPE) == 0) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;
    
    if (this->IsSetFlags(BOX_FLAG_PARSED))
    {
      // box is MSHS manifest smooth streaming media box, parse all values
      uint32_t position = this->HasExtendedHeader() ? BOX_HEADER_LENGTH_SIZE64 : BOX_HEADER_LENGTH;
      HRESULT continueParsing = (this->GetSize() <= (uint64_t)length) ? S_OK : E_OUTOFMEMORY;

      if (SUCCEEDED(continueParsing))
      {
        RBE32INC(buffer, position, this->majorVersion);
        RBE32INC(buffer, position, this->minorVersion);
        RBE64INC(buffer, position, this->timeScale);
        RBE64INC(buffer, position, this->duration);

        RBE32INC_DEFINE(buffer, position, protectionCount, uint32_t);

        for (uint32_t i = 0; (SUCCEEDED(continueParsing) && (i < protectionCount)); i++)
        {
          CMshsManifestProtectionBox *protection = new CMshsManifestProtectionBox(&continueParsing);
          CHECK_POINTER_HRESULT(continueParsing, protection, continueParsing, E_OUTOFMEMORY);

          CHECK_CONDITION_HRESULT(continueParsing, protection->Parse(buffer + position, length - position), continueParsing, E_OUTOFMEMORY);
          CHECK_CONDITION_HRESULT(continueParsing, this->protections->Add(protection), continueParsing, E_OUTOFMEMORY);

          CHECK_CONDITION_EXECUTE(SUCCEEDED(continueParsing), position += (uint32_t)protection->GetSize());
          CHECK_CONDITION_EXECUTE(FAILED(continueParsing), FREE_MEM_CLASS(protection));
        }

        RBE32INC_DEFINE(buffer, position, streamCount, uint32_t);

        for (uint32_t i = 0; (SUCCEEDED(continueParsing) && (i < streamCount)); i++)
        {
          CMshsManifestStreamBox *stream = new CMshsManifestStreamBox(&continueParsing);
          CHECK_POINTER_HRESULT(continueParsing, stream, continueParsing, E_OUTOFMEMORY);

          CHECK_CONDITION_HRESULT(continueParsing, stream->Parse(buffer + position, length - position), continueParsing, E_OUTOFMEMORY);
          CHECK_CONDITION_HRESULT(continueParsing, this->streams->Add(stream), continueParsing, E_OUTOFMEMORY);

          CHECK_CONDITION_EXECUTE(SUCCEEDED(continueParsing), position += (uint32_t)stream->GetSize());
          CHECK_CONDITION_EXECUTE(FAILED(continueParsing), FREE_MEM_CLASS(stream));
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

uint32_t CMshsManifestSmoothStreamingMediaBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    WBE32INC(buffer, result, this->majorVersion);
    WBE32INC(buffer, result, this->minorVersion);
    WBE64INC(buffer, result, this->timeScale);
    WBE64INC(buffer, result, this->duration);

    WBE32INC(buffer, result, this->protections->Count());
    for (unsigned int i = 0; ((result != 0) && (i < this->protections->Count())); i++)
    {
      result = this->protections->GetItem(i)->GetBox(buffer + result, length - result) ? result : 0;
      result += (result != 0) ? (uint32_t)this->protections->GetItem(i)->GetSize() : 0;
    }

    WBE32INC(buffer, result, this->streams->Count());
    for (unsigned int i = 0; ((result != 0) && (i < this->streams->Count())); i++)
    {
      result = this->streams->GetItem(i)->GetBox(buffer + result, length - result) ? result : 0;
      result += (result != 0) ? (uint32_t)this->streams->GetItem(i)->GetSize() : 0;
    }

    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}
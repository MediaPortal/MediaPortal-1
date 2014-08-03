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

#include "BootstrapInfoBox.h"

CBootstrapInfoBox::CBootstrapInfoBox(HRESULT *result)
  : CFullBox(result)
{
  this->bootstrapInfoVersion = 0;
  this->profile = 0;
  this->live = false;
  this->update = false;
  this->timeScale = 0;
  this->currentMediaTime = 0;
  this->smpteTimeCodeOffset;
  this->movieIdentifier = NULL;
  this->serverEntryTable = NULL;
  this->qualityEntryTable = NULL;
  this->drmData = NULL;
  this->metaData = NULL;
  this->segmentRunTable = NULL;
  this->fragmentRunTable = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->serverEntryTable = new CBootstrapInfoServerEntryCollection(result);
    this->qualityEntryTable = new CBootstrapInfoQualityEntryCollection(result);
    this->segmentRunTable = new CSegmentRunTableBoxCollection(result);
    this->fragmentRunTable = new CFragmentRunTableBoxCollection(result);

    CHECK_POINTER_HRESULT(*result, this->serverEntryTable, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->qualityEntryTable, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->segmentRunTable, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->fragmentRunTable, *result, E_OUTOFMEMORY);
  }
}

CBootstrapInfoBox::~CBootstrapInfoBox(void)
{
  FREE_MEM(this->movieIdentifier);
  FREE_MEM(this->drmData);
  FREE_MEM(this->metaData);
  FREE_MEM_CLASS(this->serverEntryTable);
  FREE_MEM_CLASS(this->qualityEntryTable);
  FREE_MEM_CLASS(this->segmentRunTable);
  FREE_MEM_CLASS(this->fragmentRunTable);
}

bool CBootstrapInfoBox::Parse(const uint8_t *buffer, uint32_t length)
{
  return this->ParseInternal(buffer, length, true);
}

wchar_t *CBootstrapInfoBox::GetParsedHumanReadable(const wchar_t *indent)
{
  wchar_t *result = NULL;
  wchar_t *previousResult = __super::GetParsedHumanReadable(indent);

  if ((previousResult != NULL) && (this->IsParsed()))
  {
    // prepare server entry table
    wchar_t *serverEntry = NULL;
    wchar_t *tempIndent = FormatString(L"%s\t", indent);
    for (unsigned int i = 0; i < this->serverEntryTable->Count(); i++)
    {
      CBootstrapInfoServerEntry *bootstrapInfoServerEntry = this->serverEntryTable->GetItem(i);
      wchar_t *tempServerEntry = FormatString(
        L"%s%s%s'%s'",
        (i == 0) ? L"" : serverEntry,
        (i == 0) ? L"" : L"\n",
        tempIndent,
        bootstrapInfoServerEntry->GetServerEntry()
        );
      FREE_MEM(serverEntry);

      serverEntry = tempServerEntry;
    }

    // prepare quality entry table
    wchar_t *qualityEntry = NULL;
    for (unsigned int i = 0; i < this->qualityEntryTable->Count(); i++)
    {
      CBootstrapInfoQualityEntry *bootstrapInfoQualityEntry = this->qualityEntryTable->GetItem(i);
      wchar_t *tempQualityEntry = FormatString(
        L"%s%s%s'%s'",
        (i == 0) ? L"" : qualityEntry,
        (i == 0) ? L"" : L"\n",
        tempIndent,
        bootstrapInfoQualityEntry->GetQualityEntry()
        );
      FREE_MEM(qualityEntry);

      qualityEntry = tempQualityEntry;
    }

    // prepare segment run table
    wchar_t *segmentRunEntry = NULL;
    for (unsigned int i = 0; i < this->segmentRunTable->Count(); i++)
    {
      CSegmentRunTableBox *bootstrapInfoSegmentRunTableBox = this->segmentRunTable->GetItem(i);
      wchar_t *tempSegmentRunEntry = FormatString(
        L"%s%s%s--- segment run table box %d start ---\n%s\n%s--- segment run table box %d end ---",
        (i == 0) ? L"" : segmentRunEntry,
        (i == 0) ? L"" : L"\n",
        tempIndent, i + 1,
        bootstrapInfoSegmentRunTableBox->GetParsedHumanReadable(tempIndent),
        tempIndent, i + 1);
      FREE_MEM(segmentRunEntry);
      segmentRunEntry = tempSegmentRunEntry;
    }

    // prepare fragment run table
    wchar_t *fragmentRunEntry = NULL;
    for (unsigned int i = 0; i < this->fragmentRunTable->Count(); i++)
    {
      CFragmentRunTableBox *bootstrapInfoFragmentRunTableBox = this->fragmentRunTable->GetItem(i);
      wchar_t *tempFragmentRunEntry = FormatString(
        L"%s%s%s--- fragment run table box %d start ---\n%s\n%s--- fragment run table box %d end ---",
        (i == 0) ? L"" : fragmentRunEntry,
        (i == 0) ? L"" : L"\n",
        tempIndent, i + 1,
        bootstrapInfoFragmentRunTableBox->GetParsedHumanReadable(tempIndent),
        tempIndent, i + 1);
      FREE_MEM(fragmentRunEntry);
      fragmentRunEntry = tempFragmentRunEntry;
    }

    FREE_MEM(tempIndent);

    // prepare finally human readable representation
    result = FormatString(L"%s\n" \
      L"%sBootstrap info version: %u\n" \
      L"%sProfile: %u\n" \
      L"%sLive: %s\n" \
      L"%sUpdate: %s\n" \
      L"%sTime scale: %u\n" \
      L"%sCurrent media time: %llu\n" \
      L"%sSMPTE time code offset: %llu\n" \
      L"%sMovie identifier: '%s'\n" \
      L"%sServer entry count: %d\n" \
      L"%s%s" \
      L"%sQuality entry count: %d\n" \
      L"%s%s" \
      L"%sDRM data: '%s'\n" \
      L"%sMetadata: '%s'\n" \
      L"%sSegment run table count: %d\n" \
      L"%s%s" \
      L"%sFragment run table count: %d" \
      L"%s%s",
      
      previousResult,
      indent, this->bootstrapInfoVersion,
      indent, this->profile,
      indent, this->live ? L"true" : L"false",
      indent, this->update ? L"true" : L"false",
      indent, this->timeScale,
      indent, this->currentMediaTime,
      indent, this->smpteTimeCodeOffset,
      indent, this->movieIdentifier,
      indent, this->serverEntryTable->Count(),
      (serverEntry == NULL) ? L"" : serverEntry, (serverEntry == NULL) ? L"" : L"\n",
      indent, this->qualityEntryTable->Count(),
      (qualityEntry == NULL) ? L"" : qualityEntry, (qualityEntry == NULL) ? L"" : L"\n",
      indent, this->drmData,
      indent, this->metaData,
      indent, this->segmentRunTable->Count(),
      (segmentRunEntry == NULL) ? L"" : segmentRunEntry, (segmentRunEntry == NULL) ? L"" : L"\n",
      indent, this->fragmentRunTable->Count(),
      (fragmentRunEntry == NULL) ? L"" : L"\n", (fragmentRunEntry == NULL) ? L"" : fragmentRunEntry

      );

    FREE_MEM(serverEntry);
    FREE_MEM(qualityEntry);
    FREE_MEM(segmentRunEntry);
    FREE_MEM(fragmentRunEntry);
  }

  FREE_MEM(previousResult);

  return result;
}

uint32_t CBootstrapInfoBox::GetBootstrapInfoVersion(void)
{
  return this->bootstrapInfoVersion;
}

uint8_t CBootstrapInfoBox::GetProfile(void)
{
  return this->profile;
}

bool CBootstrapInfoBox::IsLive(void)
{
  return this->live;
}

bool CBootstrapInfoBox::IsUpdate(void)
{
  return this->update;
}

unsigned int CBootstrapInfoBox::GetTimeScale(void)
{
  return this->timeScale;
}

uint64_t CBootstrapInfoBox::GetCurrentMediaTime(void)
{
  return this->currentMediaTime;
}

uint64_t CBootstrapInfoBox::GetSmpteTimeCodeOffset(void)
{
  return this->smpteTimeCodeOffset;
}

const wchar_t *CBootstrapInfoBox::GetMovieIdentifier(void)
{
  return this->movieIdentifier;
}

CBootstrapInfoServerEntryCollection *CBootstrapInfoBox::GetServerEntryTable(void)
{
  return this->serverEntryTable;
}

CBootstrapInfoQualityEntryCollection *CBootstrapInfoBox::GetQualityEntryTable(void)
{
  return this->qualityEntryTable;
}

const wchar_t *CBootstrapInfoBox::GetDrmData(void)
{
  return this->drmData;
}

const wchar_t *CBootstrapInfoBox::GetMetaData(void)
{
  return this->metaData;
}

CSegmentRunTableBoxCollection *CBootstrapInfoBox::GetSegmentRunTable(void)
{
  return this->segmentRunTable;
}

CFragmentRunTableBoxCollection *CBootstrapInfoBox::GetFragmentRunTable(void)
{
  return this->fragmentRunTable;
}

bool CBootstrapInfoBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  this->bootstrapInfoVersion = 0;
  this->profile = 0;
  this->live = false;
  this->update = false;
  this->timeScale = 0;
  this->currentMediaTime = 0;
  this->smpteTimeCodeOffset;
  FREE_MEM(this->movieIdentifier);
  if (this->serverEntryTable != NULL)
  {
    this->serverEntryTable->Clear();
  }
  if (this->qualityEntryTable != NULL)
  {
    this->qualityEntryTable->Clear();
  }
  FREE_MEM(this->drmData);
  FREE_MEM(this->metaData);
  if (this->segmentRunTable != NULL)
  {
    this->segmentRunTable->Clear();
  }
  if (this->fragmentRunTable != NULL)
  {
    this->fragmentRunTable->Clear();
  }

  bool result = (this->serverEntryTable != NULL) && (this->qualityEntryTable != NULL) && (this->segmentRunTable != NULL) && (this->fragmentRunTable != NULL);
  // in bad case we don't have tables, but still it can be valid box
  result &= __super::ParseInternal(buffer, length, false);
  this->flags &= ~BOX_FLAG_PARSED;

  if (result)
  {
    this->flags |= (wcscmp(this->type, BOOTSTRAP_INFO_BOX_TYPE) == 0) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;

    if (this->IsSetFlags(BOX_FLAG_PARSED))
    {
      // box is bootstrap info box, parse all values
      uint32_t position = this->HasExtendedHeader() ? BOX_HEADER_LENGTH_SIZE64 : BOX_HEADER_LENGTH;

      // until smpteTimeCodeOffset end is 29 bytes
      HRESULT continueParsing = ((position + 29) <= length) ? S_OK : E_NOT_VALID_STATE;
      
      if (SUCCEEDED(continueParsing))
      {
        position += 4;

        RBE32INC(buffer, position, this->bootstrapInfoVersion);
        RBE8INC_DEFINE(buffer, position, profileLiveUpdate, uint8_t);

        this->profile = (profileLiveUpdate >> 6);
        this->live = ((profileLiveUpdate & 0x20) != 0);
        this->update = ((profileLiveUpdate & 0x10) != 0);

        RBE32INC(buffer, position, this->timeScale);
        RBE64INC(buffer, position, this->currentMediaTime);
        RBE64INC(buffer, position, this->smpteTimeCodeOffset);
      }

      if (SUCCEEDED(continueParsing))
      {
        uint32_t positionAfter = position;
        continueParsing = this->GetString(buffer, length, position, &this->movieIdentifier, &positionAfter);

        CHECK_CONDITION_EXECUTE(SUCCEEDED(continueParsing), position = positionAfter);
      }

      CHECK_CONDITION_HRESULT(continueParsing, position < length, continueParsing, E_NOT_VALID_STATE);
      if (SUCCEEDED(continueParsing))
      {
        // server entry count and server entry table
        RBE8INC_DEFINE(buffer, position, serverEntryCount , uint8_t);
        CHECK_CONDITION_HRESULT(continueParsing, position < length, continueParsing, E_NOT_VALID_STATE);

        for(uint8_t i = 0; (SUCCEEDED(continueParsing) && (i < serverEntryCount)); i++)
        {
          uint32_t positionAfter = position;
          wchar_t *serverEntry = NULL;
          continueParsing = this->GetString(buffer, length, position, &serverEntry, &positionAfter);

          if (SUCCEEDED(continueParsing))
          {
            position = positionAfter;

            // create server entry item in server entry table
            CBootstrapInfoServerEntry *bootstrapInfoServerEntry = new CBootstrapInfoServerEntry(&continueParsing, serverEntry);
            CHECK_POINTER_HRESULT(continueParsing, bootstrapInfoServerEntry, continueParsing, E_OUTOFMEMORY);

            CHECK_CONDITION_HRESULT(continueParsing, this->serverEntryTable->Add(bootstrapInfoServerEntry), continueParsing, E_OUTOFMEMORY);
            CHECK_CONDITION_EXECUTE(FAILED(continueParsing), FREE_MEM_CLASS(bootstrapInfoServerEntry));
          }

          FREE_MEM(serverEntry);

          CHECK_CONDITION_HRESULT(continueParsing, position < length, continueParsing, E_NOT_VALID_STATE);
        }
      }

      CHECK_CONDITION_HRESULT(continueParsing, position < length, continueParsing, E_NOT_VALID_STATE);
      if (SUCCEEDED(continueParsing))
      {
        // quality entry count and quality entry table
        RBE8INC_DEFINE(buffer, position, qualityEntryCount, uint8_t);
        CHECK_CONDITION_HRESULT(continueParsing, position < length, continueParsing, E_NOT_VALID_STATE);

        for(uint8_t i = 0; (SUCCEEDED(continueParsing) && (i < qualityEntryCount)); i++)
        {
          uint32_t positionAfter = position;
          wchar_t *qualityEntry = NULL;
          continueParsing = this->GetString(buffer, length, position, &qualityEntry, &positionAfter);

          if (SUCCEEDED(continueParsing))
          {
            position = positionAfter;

            // create quality entry item in quality entry table
            CBootstrapInfoQualityEntry *bootstrapInfoQualityEntry = new CBootstrapInfoQualityEntry(&continueParsing, qualityEntry);
            CHECK_POINTER_HRESULT(continueParsing, bootstrapInfoQualityEntry, continueParsing, E_OUTOFMEMORY);

            CHECK_CONDITION_HRESULT(continueParsing, this->qualityEntryTable->Add(bootstrapInfoQualityEntry), continueParsing, E_OUTOFMEMORY);
            CHECK_CONDITION_EXECUTE(FAILED(continueParsing), FREE_MEM_CLASS(bootstrapInfoQualityEntry));
          }

          FREE_MEM(qualityEntry);

          CHECK_CONDITION_HRESULT(continueParsing, position < length, continueParsing, E_NOT_VALID_STATE);
        }
      }

      CHECK_CONDITION_HRESULT(continueParsing, position < length, continueParsing, E_NOT_VALID_STATE);
      if (SUCCEEDED(continueParsing))
      {
        // read DRM data string
        uint32_t positionAfter = position;
        continueParsing = this->GetString(buffer, length, position, &this->drmData, &positionAfter);

        CHECK_CONDITION_EXECUTE(SUCCEEDED(continueParsing), position = positionAfter);
      }

      CHECK_CONDITION_HRESULT(continueParsing, position < length, continueParsing, E_NOT_VALID_STATE);
      if (SUCCEEDED(continueParsing))
      {
        // read metadata string
        uint32_t positionAfter = position;
        continueParsing = this->GetString(buffer, length, position, &this->metaData, &positionAfter);

        CHECK_CONDITION_EXECUTE(SUCCEEDED(continueParsing), position = positionAfter);
      }

      CHECK_CONDITION_HRESULT(continueParsing, position < length, continueParsing, E_NOT_VALID_STATE);
      if (SUCCEEDED(continueParsing))
      {
        // read segment run table count and segment run table
        RBE8INC_DEFINE(buffer, position, segmentRunTableCount, uint8_t);
        CHECK_CONDITION_HRESULT(continueParsing, position < length, continueParsing, E_NOT_VALID_STATE);

        for (uint8_t i = 0; (SUCCEEDED(continueParsing) && (i < segmentRunTableCount)); i++)
        {
          CSegmentRunTableBox *segmentRunTableBox = new CSegmentRunTableBox(&continueParsing);
          CHECK_POINTER_HRESULT(continueParsing, segmentRunTableBox, continueParsing, E_OUTOFMEMORY);

          CHECK_CONDITION_HRESULT(continueParsing, segmentRunTableBox->Parse(buffer + position, length - position), continueParsing, E_NOT_VALID_STATE);
          CHECK_CONDITION_HRESULT(continueParsing, this->segmentRunTable->Add(segmentRunTableBox), continueParsing, E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(SUCCEEDED(continueParsing), position += (uint32_t)segmentRunTableBox->GetSize());
          CHECK_CONDITION_EXECUTE(FAILED(continueParsing), FREE_MEM_CLASS(segmentRunTableBox));
        }
      }

      CHECK_CONDITION_HRESULT(continueParsing, position < length, continueParsing, E_NOT_VALID_STATE);
      if (SUCCEEDED(continueParsing))
      {
        // read fragment run table count and fragment run table
        RBE8INC_DEFINE(buffer, position, fragmentRunTableCount, uint8_t);
        CHECK_CONDITION_HRESULT(continueParsing, position < length, continueParsing, E_NOT_VALID_STATE);

        for (uint8_t i = 0; (SUCCEEDED(continueParsing) && (i < fragmentRunTableCount)); i++)
        {
          CFragmentRunTableBox *fragmentRunTableBox = new CFragmentRunTableBox(&continueParsing);
          CHECK_POINTER_HRESULT(continueParsing, fragmentRunTableBox, continueParsing, E_OUTOFMEMORY);

          CHECK_CONDITION_HRESULT(continueParsing, fragmentRunTableBox->Parse(buffer + position, length - position), continueParsing, E_NOT_VALID_STATE);
          CHECK_CONDITION_HRESULT(continueParsing, this->fragmentRunTable->Add(fragmentRunTableBox), continueParsing, E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(SUCCEEDED(continueParsing), position += (uint32_t)fragmentRunTableBox->GetSize());
          CHECK_CONDITION_EXECUTE(FAILED(continueParsing), FREE_MEM_CLASS(fragmentRunTableBox));
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
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

#include "FragmentRunTableBox.h"

CFragmentRunTableBox::CFragmentRunTableBox(HRESULT *result)
  : CFullBox(result)
{
  this->timeScale = 0;
  this->qualitySegmentUrlModifiers = NULL;
  this->fragmentRunEntryTable = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->qualitySegmentUrlModifiers = new CQualitySegmentUrlModifierCollection(result);
    this->fragmentRunEntryTable = new CFragmentRunEntryCollection(result);

    CHECK_POINTER_HRESULT(*result, this->qualitySegmentUrlModifiers, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->fragmentRunEntryTable, *result, E_OUTOFMEMORY);
  }
}

CFragmentRunTableBox::~CFragmentRunTableBox(void)
{
  FREE_MEM_CLASS(this->qualitySegmentUrlModifiers);
  FREE_MEM_CLASS(this->fragmentRunEntryTable);
}

bool CFragmentRunTableBox::Parse(const uint8_t *buffer, uint32_t length)
{
  return this->ParseInternal(buffer, length, true);
}

wchar_t *CFragmentRunTableBox::GetParsedHumanReadable(const wchar_t *indent)
{
  wchar_t *result = NULL;
  wchar_t *previousResult = __super::GetParsedHumanReadable(indent);

  if ((previousResult != NULL) && (this->IsParsed()))
  {
    // prepare quality segment url modifier collection
    wchar_t *qualitySegmentUrlModifier = NULL;
    wchar_t *tempIndent = FormatString(L"%s\t", indent);
    for (unsigned int i = 0; i < this->qualitySegmentUrlModifiers->Count(); i++)
    {
      CQualitySegmentUrlModifier *qualitySegmentUrlModifierEntry = this->qualitySegmentUrlModifiers->GetItem(i);
      wchar_t *tempqualitySegmentUrlModifierEntry = FormatString(
        L"%s%s%s'%s'",
        (i == 0) ? L"" : qualitySegmentUrlModifier,
        (i == 0) ? L"" : L"\n",
        tempIndent,
        qualitySegmentUrlModifierEntry->GetQualitySegmentUrlModifier()
        );
      FREE_MEM(qualitySegmentUrlModifier);

      qualitySegmentUrlModifier = tempqualitySegmentUrlModifierEntry;
    }

    // prepare fragment run entry table
    wchar_t *fragmentRunEntry = NULL;
    for (unsigned int i = 0; i < this->fragmentRunEntryTable->Count(); i++)
    {
      CFragmentRunEntry *fragmentRunEntryEntry = this->fragmentRunEntryTable->GetItem(i);

      wchar_t *tempFragmentRunEntry = FormatString(
        L"%s%s%sFirst fragment: %u, first fragment timestamp: %llu, fragment duration: %u, end of stream: %u, discontinuity fragment numbering: %u, discontinuity timestamps: %u",
        (i == 0) ? L"" : fragmentRunEntry,
        (i == 0) ? L"" : L"\n",
        tempIndent,
        fragmentRunEntryEntry->GetFirstFragment(),
        fragmentRunEntryEntry->GetFirstFragmentTimestamp(),
        fragmentRunEntryEntry->GetFragmentDuration(),
        fragmentRunEntryEntry->IsEndOfPresentation() ? 1 : 0,
        fragmentRunEntryEntry->IsDiscontinuityFragmentNumbering() ? 1 : 0,
        fragmentRunEntryEntry->IsDiscontinuityTimestamps() ? 1 : 0
        );
      FREE_MEM(fragmentRunEntry);

      fragmentRunEntry = tempFragmentRunEntry;
    }
    FREE_MEM(tempIndent);

    // prepare finally human readable representation
    result = FormatString( \
      L"%s\n" \
      L"%sTime scale: %u\n" \
      L"%sQuality entry count: %d\n" \
      L"%s%s" \
      L"%sFragment run entry count: %d" \
      L"%s%s",

      previousResult,
      indent, this->timeScale,
      indent, this->qualitySegmentUrlModifiers->Count(),
      (qualitySegmentUrlModifier == NULL) ? L"" : qualitySegmentUrlModifier, (qualitySegmentUrlModifier == NULL) ? L"" : L"\n",
      indent, this->fragmentRunEntryTable->Count(),
      (fragmentRunEntry == NULL) ? L"" : L"\n", (fragmentRunEntry == NULL) ? L"" : fragmentRunEntry

      );
  }

  FREE_MEM(previousResult);

  return result;
}

unsigned int CFragmentRunTableBox::GetTimeScale(void)
{
  return this->timeScale;
}

CQualitySegmentUrlModifierCollection *CFragmentRunTableBox::GetQualitySegmentUrlModifiers(void)
{
  return this->qualitySegmentUrlModifiers;
}

CFragmentRunEntryCollection *CFragmentRunTableBox::GetFragmentRunEntryTable(void)
{
  return this->fragmentRunEntryTable;
}

bool CFragmentRunTableBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  bool result = (this->qualitySegmentUrlModifiers != NULL) && (this->fragmentRunEntryTable != NULL);
  // in bad case we don't have tables, but still it can be valid box
  result &= __super::ParseInternal(buffer, length, false);
  this->flags &= ~BOX_FLAG_PARSED;

  if (result)
  {
    this->flags |= (wcscmp(this->type, FRAGMENT_RUN_TABLE_BOX_TYPE) == 0) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;

    if (this->IsSetFlags(BOX_FLAG_PARSED))
    {
      // box is bootstrap info box, parse all values
      uint32_t position = this->HasExtendedHeader() ? BOX_HEADER_LENGTH_SIZE64 : BOX_HEADER_LENGTH;

      // until time scale end is 8 bytes
      HRESULT continueParsing = ((position + 8) <= length) ? S_OK : E_NOT_VALID_STATE;
      
      if (SUCCEEDED(continueParsing))
      {
        position += 4;

        this->timeScale = RBE32INC(buffer, position, this->timeScale);
      }

      CHECK_CONDITION_HRESULT(continueParsing, position < length, continueParsing, E_NOT_VALID_STATE);
      if (SUCCEEDED(continueParsing))
      {
        // quality entry count and quality segment url modifiers
        RBE8INC_DEFINE(buffer, position, qualityEntryCount, uint32_t);
        CHECK_CONDITION_HRESULT(continueParsing, position < length, continueParsing, E_NOT_VALID_STATE);

        for(uint32_t i = 0; (SUCCEEDED(continueParsing) && (i < qualityEntryCount)); i++)
        {
          uint32_t  positionAfter = position;
          wchar_t *qualitySegmentUrlModifier = NULL;
          continueParsing = this->GetString(buffer, length, position, &qualitySegmentUrlModifier, &positionAfter);

          if (SUCCEEDED(continueParsing))
          {
            position = positionAfter;

            // create quality segment url modifier item in quality segment url modifier collection
            CQualitySegmentUrlModifier *qualitySegmentUrlModifierEntry = new CQualitySegmentUrlModifier(&continueParsing, qualitySegmentUrlModifier);
            CHECK_POINTER_HRESULT(continueParsing, qualitySegmentUrlModifierEntry, continueParsing, E_OUTOFMEMORY);

            CHECK_CONDITION_HRESULT(continueParsing, this->qualitySegmentUrlModifiers->Add(qualitySegmentUrlModifierEntry), continueParsing, E_OUTOFMEMORY);
            CHECK_CONDITION_EXECUTE(FAILED(continueParsing), FREE_MEM_CLASS(qualitySegmentUrlModifierEntry));
          }

          FREE_MEM(qualitySegmentUrlModifier);

          CHECK_CONDITION_HRESULT(continueParsing, position < length, continueParsing, E_NOT_VALID_STATE);
        }
      }

      CHECK_CONDITION_HRESULT(continueParsing, (position + 4) < length, continueParsing, E_NOT_VALID_STATE);
      if (SUCCEEDED(continueParsing))
      {
        // fragment run entry count and fragment run entry table
        RBE32INC_DEFINE(buffer, position, fragmentRunEntryCount, uint32_t);

        uint32_t cumulatedFragmentCount = 0;

        for(uint32_t  i = 0; (SUCCEEDED(continueParsing) && (i < fragmentRunEntryCount)); i++)
        {
          // minimum fragment size is 16 bytes
          // but fragment can be last in buffer
          CHECK_CONDITION_HRESULT(continueParsing, (position + 15) < length, continueParsing, E_NOT_VALID_STATE);

          if (SUCCEEDED(continueParsing))
          {
            RBE32INC_DEFINE(buffer, position, firstFragment, uint32_t);
            RBE64INC_DEFINE(buffer, position, firstFragmentTimestamp, uint64_t);
            RBE32INC_DEFINE(buffer, position, fragmentDuration, uint32_t);

            if (fragmentDuration == 0)
            {
              uint32_t discontinuityFlags = FRAGMENT_RUN_ENTRY_FLAG_NONE;
              uint32_t discontinuityIndicator = 0;

              CHECK_CONDITION_HRESULT(continueParsing, position < length, continueParsing, E_NOT_VALID_STATE);

              CHECK_CONDITION_EXECUTE(SUCCEEDED(continueParsing), RBE8INC(buffer, position, discontinuityIndicator));

              switch (discontinuityIndicator)
              {
              case FRAGMENT_RUN_TABLE_BOX_END_OF_PRESENTATION:
                discontinuityFlags |= FRAGMENT_RUN_ENTRY_FLAG_END_OF_PRESENTATION;
                break;
              case FRAGMENT_RUN_TABLE_BOX_DISCONTINUITY_FRAGMENT_NUMBERING:
                discontinuityFlags |= FRAGMENT_RUN_ENTRY_FLAG_DISCONTINUITY_FRAGMENT_NUMBERING;
                break;
              case FRAGMENT_RUN_TABLE_BOX_DISCONTINUITY_TIMESTAMPS:
                discontinuityFlags |= FRAGMENT_RUN_ENTRY_FLAG_DISCONTINUITY_TIMESTAMPS;
                break;
              case FRAGMENT_RUN_TABLE_BOX_DISCONTINUITY_FRAGMENT_NUMBERING_AND_TIMESTAMPS:
                discontinuityFlags |= FRAGMENT_RUN_ENTRY_FLAG_DISCONTINUITY_FRAGMENT_NUMBERING | FRAGMENT_RUN_ENTRY_FLAG_DISCONTINUITY_TIMESTAMPS;
                break;
              }

              if (SUCCEEDED(continueParsing))
              {
                // discontinuity flags set to last fragment run entry
                CFragmentRunEntry *entry = this->fragmentRunEntryTable->GetItem(this->fragmentRunEntryTable->Count() - 1);
                entry->SetFlags(entry->GetFlags() | discontinuityFlags);
              }

              cumulatedFragmentCount++;
            }
            else
            {
              // change cumulated fragment count (if needed)
              CFragmentRunEntry *previousFragment = this->fragmentRunEntryTable->GetItem(this->fragmentRunEntryTable->Count() - 1);
              if ((previousFragment != NULL) && (!previousFragment->IsSetFlags(FRAGMENT_RUN_ENTRY_FLAG_DISCONTINUITY_FRAGMENT_NUMBERING)))
              {
                cumulatedFragmentCount += firstFragment - previousFragment->GetFirstFragment();
              }
              
              CFragmentRunEntry *fragment = new CFragmentRunEntry(&continueParsing, firstFragment, firstFragmentTimestamp, fragmentDuration, cumulatedFragmentCount);
              CHECK_POINTER_HRESULT(continueParsing, fragment, continueParsing, E_OUTOFMEMORY);

              CHECK_CONDITION_HRESULT(continueParsing, this->fragmentRunEntryTable->Add(fragment), continueParsing, E_OUTOFMEMORY);
              CHECK_CONDITION_EXECUTE(FAILED(continueParsing), FREE_MEM_CLASS(fragment));
            }
          }
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
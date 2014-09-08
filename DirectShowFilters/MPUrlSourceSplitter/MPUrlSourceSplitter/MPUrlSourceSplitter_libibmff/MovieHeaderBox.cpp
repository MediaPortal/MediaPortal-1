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

#include "MovieHeaderBox.h"
#include "BoxCollection.h"

CMovieHeaderBox::CMovieHeaderBox(HRESULT *result)
  : CFullBox(result)
{
  this->creationTime = 0;
  this->modificationTime = 0;
  this->timeScale = 0;
  this->duration = 0;
  this->type = NULL;
  this->rate = NULL;
  this->volume = NULL;
  this->matrix = NULL;
  this->nextTrackId = 0;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->type = Duplicate(MOVIE_HEADER_BOX_TYPE);
    this->rate = new CFixedPointNumber(result, 16, 16);
    this->volume = new CFixedPointNumber(result, 8, 8);
    this->matrix = new CMatrix(result);

    CHECK_POINTER_HRESULT(*result, this->type, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->rate, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->volume, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->matrix, *result, E_OUTOFMEMORY);

    if (SUCCEEDED(*result))
    {
      // set unity matrix
      this->matrix->GetItem(0)->SetIntegerPart(1);
      this->matrix->GetItem(4)->SetIntegerPart(1);
      this->matrix->GetItem(8)->SetIntegerPart(1);
    }
  }
}

CMovieHeaderBox::~CMovieHeaderBox(void)
{
  FREE_MEM_CLASS(this->rate);
  FREE_MEM_CLASS(this->volume);
  FREE_MEM_CLASS(this->matrix);
}

/* get methods */

bool CMovieHeaderBox::GetBox(uint8_t *buffer, uint32_t length)
{
  return (this->GetBoxInternal(buffer, length, true) != 0);
}

uint64_t CMovieHeaderBox::GetCreationTime(void)
{
  return this->creationTime;
}

uint64_t CMovieHeaderBox::GetModificationTime(void)
{
  return this->modificationTime;
}

uint32_t CMovieHeaderBox::GetTimeScale(void)
{
  return this->timeScale;
}

uint64_t CMovieHeaderBox::GetDuration(void)
{
  return this->duration;
}

CFixedPointNumber *CMovieHeaderBox::GetRate(void)
{
  return this->rate;
}

CFixedPointNumber *CMovieHeaderBox::GetVolume(void)
{
  return this->volume;
}

CMatrix *CMovieHeaderBox::GetMatrix(void)
{
  return this->matrix;
}

uint32_t CMovieHeaderBox::GetNextTrackId(void)
{
  return this->nextTrackId;
}

/* set methods */

void CMovieHeaderBox::SetCreationTime(uint64_t creationTime)
{
  this->creationTime = creationTime;
}

void CMovieHeaderBox::SetModificationTime(uint64_t modificationTime)
{
  this->modificationTime = modificationTime;
}

void CMovieHeaderBox::SetTimeScale(uint32_t timeScale)
{
  this->timeScale = timeScale;
}

void CMovieHeaderBox::SetDuration(uint64_t duration)
{
  this->duration = duration;
}

void CMovieHeaderBox::SetNextTrackId(uint32_t nextTrackId)
{
  this->nextTrackId = nextTrackId;
}

/* other methods */

bool CMovieHeaderBox::Parse(const uint8_t *buffer, uint32_t length)
{
  return this->ParseInternal(buffer, length, true);
}

wchar_t *CMovieHeaderBox::GetParsedHumanReadable(const wchar_t *indent)
{
  wchar_t *result = NULL;
  wchar_t *previousResult = __super::GetParsedHumanReadable(indent);

  if ((previousResult != NULL) && (this->IsParsed()))
  {
    // prepare matrix
    wchar_t *matrix = NULL;
    wchar_t *tempIndent = FormatString(L"%s\t", indent);
    for (unsigned int i = 0; i < this->matrix->Count(); i += 3)
    {
      CFixedPointNumber *num1 = this->matrix->GetItem(i);
      CFixedPointNumber *num2 = this->matrix->GetItem(i + 1);
      CFixedPointNumber *num3 = this->matrix->GetItem(i + 2);

      wchar_t *tempMatrix = FormatString(
        L"%s%s%s( %5u.%u %5u.%u %5u.%u )",
        (i == 0) ? L"" : matrix,
        (i == 0) ? L"" : L"\n",
        tempIndent,
        num1->GetIntegerPart(), num1->GetFractionPart(),
        num2->GetIntegerPart(), num2->GetFractionPart(),
        num3->GetIntegerPart(), num3->GetFractionPart()
        );
      FREE_MEM(matrix);

      matrix = tempMatrix;
    }

    // prepare finally human readable representation
    result = FormatString(
      L"%s\n" \
      L"%sCreation time: %I64u\n" \
      L"%sModification time: %I64u\n" \
      L"%sTime scale: %u\n" \
      L"%sDuration: %I64u\n" \
      L"%sRate: %u.%u\n" \
      L"%sVolume: %u.%u\n" \
      L"%sMatrix:\n" \
      L"%s%s" \
      L"%sNext track ID: %u"
      ,
      
      previousResult,
      indent, this->GetCreationTime(),
      indent, this->GetModificationTime(),
      indent, this->GetTimeScale(),
      indent, this->GetDuration(),
      indent, this->GetRate()->GetIntegerPart(), this->GetRate()->GetFractionPart(),
      indent, this->GetVolume()->GetIntegerPart(), this->GetRate()->GetFractionPart(),
      indent,
      (matrix == NULL) ? L"" : matrix, (matrix == NULL) ? L"" : L"\n",
      indent, this->GetNextTrackId()
      );

    FREE_MEM(matrix);
    FREE_MEM(tempIndent);
  }

  FREE_MEM(previousResult);

  return result;
}

uint64_t CMovieHeaderBox::GetBoxSize(void)
{
  uint64_t result = 0;

  switch(this->GetVersion())
  {
  case 0:
    result = MOVIE_HEADER_DATA_VERSION_0_SIZE;
    break;
  case 1:
    result = MOVIE_HEADER_DATA_VERSION_1_SIZE;
    break;
  default:
    break;
  }

  if (result != 0)
  {
    uint64_t boxSize = __super::GetBoxSize();
    result = (boxSize != 0) ? (result + boxSize) : 0; 
  }

  return result;
}

bool CMovieHeaderBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  FREE_MEM_CLASS(this->rate);
  FREE_MEM_CLASS(this->volume);
  FREE_MEM_CLASS(this->matrix);

  this->creationTime = 0;
  this->modificationTime = 0;
  this->timeScale = 0;
  this->duration = 0;
  this->nextTrackId = 0;

  if (__super::ParseInternal(buffer, length, false))
  {
    this->flags &= ~BOX_FLAG_PARSED;
    this->flags |= (wcscmp(this->type, MOVIE_HEADER_BOX_TYPE) == 0) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;

    if (this->IsSetFlags(BOX_FLAG_PARSED))
    {
      // box is movie header box, parse all values
      uint32_t position = this->HasExtendedHeader() ? FULL_BOX_HEADER_LENGTH_SIZE64 : FULL_BOX_HEADER_LENGTH;
      HRESULT continueParsing = (this->GetSize() <= (uint64_t)length) ? S_OK : E_NOT_VALID_STATE;

      if (SUCCEEDED(continueParsing))
      {
        this->rate = new CFixedPointNumber(&continueParsing, 16, 16);
        this->volume = new CFixedPointNumber(&continueParsing, 8, 8);
        this->matrix = new CMatrix(&continueParsing);

        CHECK_POINTER_HRESULT(continueParsing, this->rate, continueParsing, E_OUTOFMEMORY);
        CHECK_POINTER_HRESULT(continueParsing, this->volume, continueParsing, E_OUTOFMEMORY);
        CHECK_POINTER_HRESULT(continueParsing, this->matrix, continueParsing, E_OUTOFMEMORY);
      }

      if (SUCCEEDED(continueParsing))
      {
        switch (this->GetVersion())
        {
        case 0:
          RBE32INC(buffer, position, this->creationTime);
          RBE32INC(buffer, position, this->modificationTime);
          RBE32INC(buffer, position, this->timeScale);
          RBE32INC(buffer, position, this->duration);
          break;
        case 1:
          RBE64INC(buffer, position, this->creationTime);
          RBE64INC(buffer, position, this->modificationTime);
          RBE32INC(buffer, position, this->timeScale);
          RBE64INC(buffer, position, this->duration);
          break;
        default:
          continueParsing = E_FAIL;
          break;
        }
      }

      CHECK_CONDITION_HRESULT(continueParsing, this->rate->SetNumber(RBE32(buffer, position)), continueParsing, E_OUTOFMEMORY);
      position += 4;
      
      CHECK_CONDITION_HRESULT(continueParsing, this->volume->SetNumber(RBE16(buffer, position)), continueParsing, E_OUTOFMEMORY);
      position += 2;

      // skip 10 reserved bytes
      position += 10;

      // read matrix
      for (unsigned int i = 0; (SUCCEEDED(continueParsing) && (i < this->matrix->Count())); i++)
      {
        CHECK_CONDITION_HRESULT(continueParsing, this->matrix->GetItem(i)->SetNumber(RBE32(buffer, position)), continueParsing, E_OUTOFMEMORY);
        position += 4;
      }

      // skip 6 * bit(32)
      position += 24;

      if (SUCCEEDED(continueParsing))
      {
        RBE32INC(buffer, position, this->nextTrackId);
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

uint32_t CMovieHeaderBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    switch (this->GetVersion())
    {
    case 0:
      WBE32INC(buffer, result, this->GetCreationTime());
      WBE32INC(buffer, result, this->GetModificationTime());
      WBE32INC(buffer, result, this->GetTimeScale());
      WBE32INC(buffer, result, this->GetDuration());
      break;
    case 1:
      WBE64INC(buffer, result, this->GetCreationTime());
      WBE64INC(buffer, result, this->GetModificationTime());
      WBE32INC(buffer, result, this->GetTimeScale());
      WBE64INC(buffer, result, this->GetDuration());
      break;
    default:
      result = 0;
      break;
    }

    if (result != 0)
    {
      WBE32INC(buffer, result, this->GetRate()->GetNumber());
      WBE16INC(buffer, result, this->GetVolume()->GetNumber());

      // skip 10 bytes reserved
      result += 10;

      // write matrix
      for (unsigned int i = 0; (i < this->GetMatrix()->Count()); i++)
      {
        WBE32INC(buffer, result, this->GetMatrix()->GetItem(i)->GetNumber());
      }

      // skip 6 * bit(32)
      result += 24;

      WBE32INC(buffer, result, this->GetNextTrackId());
    }

    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}
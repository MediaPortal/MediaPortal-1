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

#include "TrackHeaderBox.h"
#include "BoxCollection.h"

CTrackHeaderBox::CTrackHeaderBox(void)
  : CFullBox()
{
  this->type = Duplicate(TRACK_HEADER_BOX_TYPE);
  this->creationTime = 0;
  this->modificationTime = 0;
  this->trackId = 0;
  this->duration = 0;
  this->layer = 0;
  this->alternateGroup = 0;
  this->volume = new CFixedPointNumber(8, 8);
  this->matrix = new CMatrix();
  this->width = new CFixedPointNumber(16, 16);
  this->height = new CFixedPointNumber(16, 16);

  // set unity matrix
  this->matrix->GetItem(0)->SetIntegerPart(1);
  this->matrix->GetItem(4)->SetIntegerPart(1);
  this->matrix->GetItem(8)->SetIntegerPart(1);
}

CTrackHeaderBox::~CTrackHeaderBox(void)
{
  FREE_MEM_CLASS(this->volume);
  FREE_MEM_CLASS(this->matrix);
  FREE_MEM_CLASS(this->width);
  FREE_MEM_CLASS(this->height);
}

/* get methods */

bool CTrackHeaderBox::GetBox(uint8_t *buffer, uint32_t length)
{
  return (this->GetBoxInternal(buffer, length, true) != 0);
}

uint64_t CTrackHeaderBox::GetCreationTime(void)
{
  return this->creationTime;
}

uint64_t CTrackHeaderBox::GetModificationTime(void)
{
  return this->modificationTime;
}

uint32_t CTrackHeaderBox::GetTrackId(void)
{
  return this->trackId;
}

int16_t CTrackHeaderBox::GetLayer(void)
{
  return this->layer;
}

int16_t CTrackHeaderBox::GetAlternateGroup(void)
{
  return this->alternateGroup;
}

uint64_t CTrackHeaderBox::GetDuration(void)
{
  return this->duration;
}

CFixedPointNumber *CTrackHeaderBox::GetVolume(void)
{
  return this->volume;
}

CMatrix *CTrackHeaderBox::GetMatrix(void)
{
  return this->matrix;
}

CFixedPointNumber *CTrackHeaderBox::GetWidth(void)
{
  return this->width;
}

CFixedPointNumber *CTrackHeaderBox::GetHeight(void)
{
  return this->height;
}

/* set methods */

void CTrackHeaderBox::SetCreationTime(uint64_t creationTime)
{
  this->creationTime = creationTime;
}

void CTrackHeaderBox::SetModificationTime(uint64_t modificationTime)
{
  this->modificationTime = modificationTime;
}

void CTrackHeaderBox::SetTrackId(uint32_t trackId)
{
  this->trackId = trackId;
}

void CTrackHeaderBox::SetLayer(int16_t layer)
{
  this->layer = layer;
}

void CTrackHeaderBox::SetAlternateGroup(int16_t alternateGroup)
{
  this->alternateGroup = alternateGroup;
}

void CTrackHeaderBox::SetDuration(uint64_t duration)
{
  this->duration = duration;
}

/* other methods */

bool CTrackHeaderBox::Parse(const uint8_t *buffer, uint32_t length)
{
  return this->ParseInternal(buffer, length, true);
}

wchar_t *CTrackHeaderBox::GetParsedHumanReadable(const wchar_t *indent)
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
      L"%sCreation time: %llu\n" \
      L"%sModification time: %llu\n" \
      L"%sTrack ID: %u\n" \
      L"%sDuration: %llu\n" \
      L"%sLayer: %d\n" \
      L"%sAlternate group: %d\n" \
      L"%sVolume: %u.%u\n" \
      L"%sMatrix:\n" \
      L"%s%s\n" \
      L"%sWidth: %u.%u\n" \
      L"%sHeight: %u.%u"
      ,
      
      previousResult,
      indent, this->GetCreationTime(),
      indent, this->GetModificationTime(),
      indent, this->GetTrackId(),
      indent, this->GetDuration(),
      indent, this->GetLayer(),
      indent, this->GetAlternateGroup(),
      indent, this->GetVolume()->GetIntegerPart(), this->GetVolume()->GetFractionPart(),
      indent,
      (matrix == NULL) ? L"" : matrix, (matrix == NULL) ? L"" : L"\n",
      indent, this->GetWidth()->GetIntegerPart(), this->GetWidth()->GetFractionPart(),
      indent, this->GetHeight()->GetIntegerPart(), this->GetHeight()->GetFractionPart()
      );

    FREE_MEM(matrix);
    FREE_MEM(tempIndent);
  }

  FREE_MEM(previousResult);

  return result;
}

uint64_t CTrackHeaderBox::GetBoxSize(void)
{
  uint64_t result = 0;

  switch(this->GetVersion())
  {
  case 0:
    result = 20;
    break;
  case 1:
    result = 32;
    break;
  default:
    break;
  }

  if (result != 0)
  {
    result += 60;
  }

  if (result != 0)
  {
    uint64_t boxSize = __super::GetBoxSize();
    result = (boxSize != 0) ? (result + boxSize) : 0; 
  }

  return result;
}

bool CTrackHeaderBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  FREE_MEM_CLASS(this->volume);
  FREE_MEM_CLASS(this->matrix);
  FREE_MEM_CLASS(this->width);
  FREE_MEM_CLASS(this->height);

  this->creationTime = 0;
  this->modificationTime = 0;
  this->trackId = 0;
  this->duration = 0;
  this->layer = 0;
  this->alternateGroup = 0;
  this->volume = new CFixedPointNumber(8, 8);
  this->matrix = new CMatrix();
  this->width = new CFixedPointNumber(16, 16);
  this->height = new CFixedPointNumber(16, 16);

  bool result = ((this->volume != NULL) && (this->matrix != NULL) && (this->width != NULL) && (this->height != NULL));
  // in bad case we don't have objects, but still it can be valid box
  result &= __super::ParseInternal(buffer, length, false);

  if (result)
  {
    if (wcscmp(this->type, TRACK_HEADER_BOX_TYPE) != 0)
    {
      // incorect box type
      this->parsed = false;
    }
    else
    {
      // box is file type box, parse all values
      uint32_t position = this->HasExtendedHeader() ? FULL_BOX_HEADER_LENGTH_SIZE64 : FULL_BOX_HEADER_LENGTH;
      bool continueParsing = (this->GetSize() <= (uint64_t)length);

      if (continueParsing)
      {
        switch (this->GetVersion())
        {
        case 0:
          RBE32INC(buffer, position, this->creationTime);
          RBE32INC(buffer, position, this->modificationTime);
          RBE32INC(buffer, position, this->trackId);
          // skip int(32) reserved field
          position += 4;
          RBE32INC(buffer, position, this->duration);
          break;
        case 1:
          RBE64INC(buffer, position, this->creationTime);
          RBE64INC(buffer, position, this->modificationTime);
          RBE32INC(buffer, position, this->trackId);
          // skip int(32) reserved field
          position += 4;
          RBE64INC(buffer, position, this->duration);
          break;
        default:
          continueParsing = false;
          break;
        }
      }

      if (continueParsing)
      {
        // skip 2 x int(32) reserved field
        position += 8;
        RBE16INC(buffer, position, this->layer);
        RBE16INC(buffer, position, this->alternateGroup);
        continueParsing &= this->volume->SetIntegerPart(RBE8(buffer, position));
        position++;
        continueParsing &= this->volume->SetFractionPart(RBE8(buffer, position));
        position++;
        // skip int(16) reserved field
        position += 2;

        // read matrix
        for (unsigned int i = 0; (continueParsing && (i < this->matrix->Count())); i++)
        {
          continueParsing &= this->matrix->GetItem(i)->SetNumber(RBE32(buffer, position));
          position += 4;
        }

        continueParsing &= this->width->SetIntegerPart(RBE16(buffer, position));
        position += 2;
        continueParsing &= this->width->SetFractionPart(RBE16(buffer, position));
        position += 2;

        continueParsing &= this->height->SetIntegerPart(RBE16(buffer, position));
        position += 2;
        continueParsing &= this->height->SetFractionPart(RBE16(buffer, position));
        position += 2;
      }

      if (continueParsing && processAdditionalBoxes)
      {
        this->ProcessAdditionalBoxes(buffer, length, position);
      }

      this->parsed = continueParsing;
    }
  }

  result = this->parsed;

  return result;
}

uint32_t CTrackHeaderBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    switch (this->GetVersion())
    {
    case 0:
      WBE32INC(buffer, result, this->GetCreationTime());
      WBE32INC(buffer, result, this->GetModificationTime());
      WBE32INC(buffer, result, this->GetTrackId());
      // skip int(32) reserved field
      result += 4;
      WBE32INC(buffer, result, this->GetDuration());
      break;
    case 1:
      WBE64INC(buffer, result, this->GetCreationTime());
      WBE64INC(buffer, result, this->GetModificationTime());
      WBE32INC(buffer, result, this->GetTrackId());
      // skip int(32) reserved field
      result += 4;
      WBE64INC(buffer, result, this->GetDuration());
      break;
    default:
      result = 0;
      break;
    }

    if (result != 0)
    {
      // skip 2 x int(32) reserved field
      result += 8;

      WBE16INC(buffer, result, this->GetLayer());
      WBE16INC(buffer, result, this->GetAlternateGroup());
      WBE16INC(buffer, result, this->GetVolume()->GetNumber());

      // skip int(16)
      result += 2;

      // write matrix
      for (unsigned int i = 0; (i < this->GetMatrix()->Count()); i++)
      {
        WBE32INC(buffer, result, this->GetMatrix()->GetItem(i)->GetNumber());
      }

      WBE32INC(buffer, result, this->GetWidth()->GetNumber());
      WBE32INC(buffer, result, this->GetHeight()->GetNumber());
    }
    
    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}
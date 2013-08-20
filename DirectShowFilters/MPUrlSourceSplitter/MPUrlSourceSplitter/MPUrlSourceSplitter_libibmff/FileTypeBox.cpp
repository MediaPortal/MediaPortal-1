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

#include "FileTypeBox.h"
#include "BoxCollection.h"

CFileTypeBox::CFileTypeBox(void)
  : CBox()
{
  this->majorBrand = new CBrand();
  this->compatibleBrands = new CBrandCollection();
  this->type = Duplicate(FILE_TYPE_BOX_TYPE);
}

CFileTypeBox::~CFileTypeBox(void)
{
  FREE_MEM_CLASS(this->majorBrand);
  FREE_MEM_CLASS(this->compatibleBrands);
}

/* get methods */

CBrand *CFileTypeBox::GetMajorBrand(void)
{
  return this->majorBrand;
}

uint32_t CFileTypeBox::GetMinorVersion(void)
{
  return this->minorVersion;
}

CBrandCollection *CFileTypeBox::GetCompatibleBrands(void)
{
  return this->compatibleBrands;
}

bool CFileTypeBox::GetBox(uint8_t *buffer, uint32_t length)
{
  return (this->GetBoxInternal(buffer, length, true) != 0);
}

/* set methods */

bool CFileTypeBox::SetMinorVersion(uint32_t minorVersion)
{
  this->minorVersion = minorVersion;
  return true;
}

/* other methods */

bool CFileTypeBox::Parse(const uint8_t *buffer, uint32_t length)
{
  return this->ParseInternal(buffer, length, true);
}

wchar_t *CFileTypeBox::GetParsedHumanReadable(const wchar_t *indent)
{
  wchar_t *result = NULL;
  wchar_t *previousResult = __super::GetParsedHumanReadable(indent);

  if ((previousResult != NULL) && (this->IsParsed()))
  {
    // prepare compatible brands collection
    wchar_t *compatibleBrands = NULL;
    wchar_t *tempIndent = FormatString(L"%s\t", indent);
    for (unsigned int i = 0; i < this->compatibleBrands->Count(); i++)
    {
      CBrand *brand = this->compatibleBrands->GetItem(i);
      wchar_t *tempCompatibleBrands = FormatString(
        L"%s%s%s'%s'",
        (i == 0) ? L"" : compatibleBrands,
        (i == 0) ? L"" : L"\n",
        tempIndent,
        brand->GetBrandString()
        );
      FREE_MEM(compatibleBrands);

      compatibleBrands = tempCompatibleBrands;
    }

    // prepare finally human readable representation
    result = FormatString(
      L"%s\n" \
      L"%sBrand: %s\n" \
      L"%sMinor version: %u\n" \
      L"%sCompatible brands:" \
      L"%s%s",
      
      previousResult,
      indent, this->majorBrand->GetBrandString(),
      indent, this->minorVersion,
      indent,
      (compatibleBrands == NULL) ? L"" : L"\n", (compatibleBrands == NULL) ? L"" : compatibleBrands
      );

    FREE_MEM(compatibleBrands);
    FREE_MEM(tempIndent);
  }

  FREE_MEM(previousResult);

  return result;
}

uint64_t CFileTypeBox::GetBoxSize(void)
{
  uint64_t result = 8 + this->GetCompatibleBrands()->Count() * 4;

  if (result != 0)
  {
    uint64_t boxSize = __super::GetBoxSize();
    result = (boxSize != 0) ? (result + boxSize) : 0; 
  }

  return result;
}

bool CFileTypeBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  FREE_MEM_CLASS(this->majorBrand);
  FREE_MEM_CLASS(this->compatibleBrands);

  this->majorBrand = new CBrand();
  this->compatibleBrands = new CBrandCollection();

  bool result = ((this->majorBrand != NULL) && (this->compatibleBrands != NULL));
  // in bad case we don't have objects, but still it can be valid box
  result &= __super::ParseInternal(buffer, length, false);

  if (result)
  {
    if (wcscmp(this->type, FILE_TYPE_BOX_TYPE) != 0)
    {
      // incorect box type
      this->parsed = false;
    }
    else
    {
      // box is file type box, parse all values
      uint32_t position = this->HasExtendedHeader() ? BOX_HEADER_LENGTH_SIZE64 : BOX_HEADER_LENGTH;
      bool continueParsing = (((position + 8) <= length) && (this->GetSize() <= (uint64_t)length));

      if (continueParsing)
      {
        // major brand + minor version = 8 bytes
        continueParsing &= this->majorBrand->SetBrand(RBE32(buffer, position));
        position += 4;

        RBE32INC(buffer, position, this->minorVersion);
      }

      if (continueParsing)
      {
        // the last bytes in file type box are compatible brands (each 4 characters)
        while (continueParsing && ((position + 3) < (uint32_t)this->GetSize()))
        {
          CBrand *brand = new CBrand();
          continueParsing &= (brand != NULL);

          if (continueParsing)
          {
            continueParsing &= brand->SetBrand(RBE32(buffer, position));
            if (continueParsing)
            {
              continueParsing &= this->compatibleBrands->Add(brand);
            }
          }

          if (!continueParsing)
          {
            FREE_MEM_CLASS(brand);
          }

          position += 4;
        }
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

uint32_t CFileTypeBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    WBE32INC(buffer, result, this->GetMajorBrand()->GetBrand());
    WBE32INC(buffer, result, this->GetMinorVersion());

    for (unsigned int i = 0; i < this->GetCompatibleBrands()->Count(); i++)
    {
      CBrand *brand = this->GetCompatibleBrands()->GetItem(i);

      WBE32INC(buffer, result, brand->GetBrand());
    }

    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}

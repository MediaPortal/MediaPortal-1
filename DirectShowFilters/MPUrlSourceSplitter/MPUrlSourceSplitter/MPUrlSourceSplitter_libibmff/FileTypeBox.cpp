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

CFileTypeBox::CFileTypeBox(HRESULT *result)
  : CBox(result)
{
  this->majorBrand = NULL;
  this->compatibleBrands = NULL;
  this->type = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->type = Duplicate(FILE_TYPE_BOX_TYPE);
    this->majorBrand = new CBrand(result);
    this->compatibleBrands = new CBrandCollection(result);

    CHECK_POINTER_HRESULT(*result, this->type, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->majorBrand, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->compatibleBrands, *result, E_OUTOFMEMORY);
  }
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
  this->compatibleBrands->Clear();

  if (__super::ParseInternal(buffer, length, false))
  {
    this->flags &= ~BOX_FLAG_PARSED;
    this->flags |= (wcscmp(this->type, FILE_TYPE_BOX_TYPE) == 0) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;

    if (this->IsSetFlags(BOX_FLAG_PARSED))
    {
      // box is file type box, parse all values
      uint32_t position = this->HasExtendedHeader() ? BOX_HEADER_LENGTH_SIZE64 : BOX_HEADER_LENGTH;
      HRESULT continueParsing = (this->GetSize() <= (uint64_t)length) ? S_OK : E_NOT_VALID_STATE;

      if (SUCCEEDED(continueParsing))
      {
        this->majorBrand = new CBrand(&continueParsing);
        CHECK_POINTER_HRESULT(continueParsing, this->majorBrand, continueParsing, E_OUTOFMEMORY);
      }

      if (SUCCEEDED(continueParsing))
      {
        // major brand + minor version = 8 bytes
        CHECK_CONDITION_HRESULT(continueParsing, this->majorBrand->SetBrand(RBE32(buffer, position)), continueParsing, E_OUTOFMEMORY);
        position += 4;

        RBE32INC(buffer, position, this->minorVersion);
      }

      if (SUCCEEDED(continueParsing))
      {
        // the last bytes in file type box are compatible brands (each 4 characters)
        while (SUCCEEDED(continueParsing) && ((position + 3) < (uint32_t)this->GetSize()))
        {
          CBrand *brand = new CBrand(&continueParsing);
          CHECK_POINTER_HRESULT(continueParsing, brand, continueParsing, E_OUTOFMEMORY);
          
          CHECK_CONDITION_HRESULT(continueParsing, brand->SetBrand(RBE32(buffer, position)), continueParsing, E_OUTOFMEMORY);

          CHECK_CONDITION_HRESULT(continueParsing, this->compatibleBrands->Add(brand), continueParsing, E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(FAILED(continueParsing), FREE_MEM_CLASS(brand));

          position += 4;
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

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

#include "MshsManifestTrackBox.h"
#include "BoxCollection.h"
#include "BufferHelper.h"
#include "BoxConstants.h"

CMshsManifestTrackBox::CMshsManifestTrackBox(HRESULT *result)
  : CBox(result)
{
  this->index = 0;
  this->bitrate = 0;
  this->maxWidth = 0;
  this->maxHeight = 0;
  this->codecPrivateData = NULL;
  this->samplingRate = 0;
  this->channels = 0;
  this->bitsPerSample = 0;
  this->packetSize = 0;
  this->audioTag = 0;
  this->fourCC = NULL;
  this->nalUnitLengthField = MSHS_NAL_UNIT_LENGTH_DEFAULT;
  this->customAttributes = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->customAttributes = new CMshsManifestCustomAttributeBoxCollection(result);

    CHECK_POINTER_HRESULT(*result, this->customAttributes, *result, E_OUTOFMEMORY);
  }
}

CMshsManifestTrackBox::~CMshsManifestTrackBox(void)
{
  FREE_MEM(this->codecPrivateData);
  FREE_MEM_CLASS(this->customAttributes);
}

/* get methods */

uint32_t CMshsManifestTrackBox::GetIndex(void)
{
  return this->index;
}

uint32_t CMshsManifestTrackBox::GetBitrate(void)
{
  return this->bitrate;
}

uint32_t CMshsManifestTrackBox::GetMaxWidth(void)
{
  return this->maxWidth;
}

uint32_t CMshsManifestTrackBox::GetMaxHeight(void)
{
  return this->maxHeight;
}

const wchar_t *CMshsManifestTrackBox::GetCodecPrivateData(void)
{
  return this->codecPrivateData;
}

uint32_t CMshsManifestTrackBox::GetSamplingRate(void)
{
  return this->samplingRate;
}

uint16_t CMshsManifestTrackBox::GetChannels(void)
{
  return this->channels;
}

uint16_t CMshsManifestTrackBox::GetBitsPerSample(void)
{
  return this->bitsPerSample;
}

uint32_t CMshsManifestTrackBox::GetPacketSize(void)
{
  return this->packetSize;
}

uint32_t CMshsManifestTrackBox::GetAudioTag(void)
{
  return this->audioTag;
}

const wchar_t *CMshsManifestTrackBox::GetFourCC(void)
{
  return this->fourCC;
}

uint16_t CMshsManifestTrackBox::GetNalUnitLengthField(void)
{
  return this->nalUnitLengthField;
}

CMshsManifestCustomAttributeBoxCollection *CMshsManifestTrackBox::GetCustomAttributes(void)
{
  return this->customAttributes;
}

bool CMshsManifestTrackBox::GetBox(uint8_t *buffer, uint32_t length)
{
  return (this->GetBoxInternal(buffer, length, true) != 0);
}

/* set methods */

void CMshsManifestTrackBox::SetIndex(uint32_t index)
{
  this->index = index;
}

void CMshsManifestTrackBox::SetBitrate(uint32_t bitrate)
{
  this->bitrate = bitrate;
}

void CMshsManifestTrackBox::SetMaxWidth(uint32_t maxWidth)
{
  this->maxWidth = maxWidth;
}

void CMshsManifestTrackBox::SetMaxHeight(uint32_t maxHeight)
{
  this->maxHeight = maxHeight;
}

bool CMshsManifestTrackBox::SetCodecPrivateData(const wchar_t *codecPrivateData)
{
  SET_STRING_RETURN_WITH_NULL(this->codecPrivateData, codecPrivateData);
}

void CMshsManifestTrackBox::SetSamplingRate(uint32_t samplingRate)
{
  this->samplingRate = samplingRate;
}

void CMshsManifestTrackBox::SetChannels(uint16_t channels)
{
  this->channels = channels;
}

void CMshsManifestTrackBox::SetBitsPerSample(uint16_t bitsPerSample)
{
  this->bitsPerSample = bitsPerSample;
}

void CMshsManifestTrackBox::SetPacketSize(uint32_t packetSize)
{
  this->packetSize = packetSize;
}

void CMshsManifestTrackBox::SetAudioTag(uint32_t audioTag)
{
  this->audioTag = audioTag;
}

bool CMshsManifestTrackBox::SetFourCC(const wchar_t *fourCC)
{
  SET_STRING_RETURN_WITH_NULL(this->fourCC, fourCC);
}

void CMshsManifestTrackBox::SetNalUnitLengthField(uint16_t nalUnitLengthField)
{
  this->nalUnitLengthField = nalUnitLengthField;
}

/* other methods */

bool CMshsManifestTrackBox::Parse(const uint8_t *buffer, uint32_t length)
{
  return this->ParseInternal(buffer, length, true);
}

wchar_t *CMshsManifestTrackBox::GetParsedHumanReadable(const wchar_t *indent)
{
  return NULL;
}

/* protected methods */

uint64_t CMshsManifestTrackBox::GetBoxSize(void)
{
  uint64_t result = 46;

  result += (this->codecPrivateData != NULL) ? (wcslen(this->codecPrivateData) * sizeof(wchar_t)) : 0;
  result += (this->fourCC != NULL) ? (wcslen(this->fourCC) * sizeof(wchar_t)) : 0;

  for (unsigned int i = 0; i < this->customAttributes->Count(); i++)
  {
    result += this->customAttributes->GetItem(i)->GetSize();
  }

  uint64_t boxSize = __super::GetBoxSize();
  result = (boxSize != 0) ? (result + boxSize) : 0; 

  return result;
}

bool CMshsManifestTrackBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  FREE_MEM(this->codecPrivateData);
  FREE_MEM(this->fourCC);
  this->customAttributes->Clear();

  if (__super::ParseInternal(buffer, length, false))
  {
    this->flags &= ~BOX_FLAG_PARSED;
    this->flags |= (wcscmp(this->type, MSHS_MANIFEST_TRACK_BOX_TYPE) == 0) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;
    
    if (this->IsSetFlags(BOX_FLAG_PARSED))
    {
      // box is MSHS manifest track box, parse all values
      uint32_t position = this->HasExtendedHeader() ? BOX_HEADER_LENGTH_SIZE64 : BOX_HEADER_LENGTH;
      HRESULT continueParsing = (this->GetSize() <= (uint64_t)length) ? S_OK : E_OUTOFMEMORY;

      if (SUCCEEDED(continueParsing))
      {
        RBE32INC(buffer, position, this->index);
        RBE32INC(buffer, position, this->bitrate);
        RBE32INC(buffer, position, this->maxWidth);
        RBE32INC(buffer, position, this->maxHeight);
        RBE32INC(buffer, position, this->samplingRate);
        RBE16INC(buffer, position, this->channels);
        RBE16INC(buffer, position, this->bitsPerSample);
        RBE32INC(buffer, position, this->packetSize);
        RBE32INC(buffer, position, this->audioTag);
        RBE16INC(buffer, position, this->nalUnitLengthField);

        RBE32INC_DEFINE(buffer, position, codecPrivateDataLength, uint32_t);

        // check if we have enough data in buffer for codec private data
        CHECK_CONDITION_HRESULT(continueParsing, (this->GetSize() + codecPrivateDataLength * sizeof(wchar_t)) <= length, continueParsing, E_OUTOFMEMORY);

        if (SUCCEEDED(continueParsing) && (codecPrivateDataLength != 0))
        {
          this->codecPrivateData = ALLOC_MEM_SET(this->codecPrivateData, wchar_t, (codecPrivateDataLength + 1), 0);
          CHECK_POINTER_HRESULT(continueParsing, this->codecPrivateData, continueParsing, E_OUTOFMEMORY);

          if (SUCCEEDED(continueParsing))
          {
            memcpy(this->codecPrivateData, buffer + position, codecPrivateDataLength * sizeof(wchar_t));
            position += codecPrivateDataLength * sizeof(wchar_t);
          }
        }

        RBE32INC_DEFINE(buffer, position, fourCCLength, uint32_t);

        // check if we have enough data in buffer for name
        CHECK_CONDITION_HRESULT(continueParsing, (this->GetSize() + fourCCLength * sizeof(wchar_t)) <= length, continueParsing, E_OUTOFMEMORY);

        if (SUCCEEDED(continueParsing) && (fourCCLength != 0))
        {
          this->fourCC = ALLOC_MEM_SET(this->fourCC, wchar_t, (fourCCLength + 1), 0);
          CHECK_POINTER_HRESULT(continueParsing, this->fourCC, continueParsing, E_OUTOFMEMORY);

          if (SUCCEEDED(continueParsing))
          {
            memcpy(this->fourCC, buffer + position, fourCCLength * sizeof(wchar_t));
            position += fourCCLength * sizeof(wchar_t);
          }
        }

        RBE32INC_DEFINE(buffer, position, customAttributeCount, uint32_t);

        for (uint32_t i = 0; (SUCCEEDED(continueParsing) && (i < customAttributeCount)); i++)
        {
          CMshsManifestCustomAttributeBox *customAttribute = new CMshsManifestCustomAttributeBox(&continueParsing);
          CHECK_POINTER_HRESULT(continueParsing, customAttribute, continueParsing, E_OUTOFMEMORY);

          CHECK_CONDITION_HRESULT(continueParsing, customAttribute->Parse(buffer + position, length - position), continueParsing, E_OUTOFMEMORY);
          CHECK_CONDITION_HRESULT(continueParsing, this->customAttributes->Add(customAttribute), continueParsing, E_OUTOFMEMORY);

          CHECK_CONDITION_EXECUTE(SUCCEEDED(continueParsing), position += (uint32_t)customAttribute->GetSize());
          CHECK_CONDITION_EXECUTE(FAILED(continueParsing), FREE_MEM_CLASS(customAttribute));
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

uint32_t CMshsManifestTrackBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    WBE32INC(buffer, result, this->index);
    WBE32INC(buffer, result, this->bitrate);
    WBE32INC(buffer, result, this->maxWidth);
    WBE32INC(buffer, result, this->maxHeight);
    WBE32INC(buffer, result, this->samplingRate);
    WBE16INC(buffer, result, this->channels);
    WBE16INC(buffer, result, this->bitsPerSample);
    WBE32INC(buffer, result, this->packetSize);
    WBE32INC(buffer, result, this->audioTag);
    WBE16INC(buffer, result, this->nalUnitLengthField);

    unsigned int codecPrivateDataLength = (this->codecPrivateData != NULL) ? wcslen(this->codecPrivateData) : 0;
    WBE32INC(buffer, result, codecPrivateDataLength);

    if (codecPrivateDataLength > 0)
    {
      memcpy(buffer + result, this->codecPrivateData, codecPrivateDataLength * sizeof(wchar_t));
      result += codecPrivateDataLength * sizeof(wchar_t);
    }

    unsigned int fourCCLength = (this->fourCC != NULL) ? wcslen(this->fourCC) : 0;
    WBE32INC(buffer, result, fourCCLength);

    if (fourCCLength > 0)
    {
      memcpy(buffer + result, this->fourCC, fourCCLength * sizeof(wchar_t));
      result += fourCCLength * sizeof(wchar_t);
    }

    WBE32INC(buffer, result, this->customAttributes->Count());
    for (unsigned int i = 0; ((result != 0) && (i < this->customAttributes->Count())); i++)
    {
      result = this->customAttributes->GetItem(i)->GetBox(buffer + result, length - result) ? result : 0;
      result += (result != 0) ? (uint32_t)this->customAttributes->GetItem(i)->GetSize() : 0;
    }

    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}
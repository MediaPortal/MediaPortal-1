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

#include "AVCConfigurationBox.h"
#include "BoxCollection.h"

CAVCConfigurationBox::CAVCConfigurationBox(HRESULT *result)
  : CBox(result)
{
  this->type = NULL;
  this->avcDecoderConfiguration = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->type = Duplicate(AVC_CONFIGURATION_BOX_TYPE);
    this->avcDecoderConfiguration = new CAVCDecoderConfiguration(result);

    CHECK_POINTER_HRESULT(*result, this->type, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->avcDecoderConfiguration, *result, E_OUTOFMEMORY);
  }
}

CAVCConfigurationBox::~CAVCConfigurationBox(void)
{
  FREE_MEM_CLASS(this->avcDecoderConfiguration);
}

/* get methods */

CAVCDecoderConfiguration *CAVCConfigurationBox::GetAVCDecoderConfiguration(void)
{
  return this->avcDecoderConfiguration;
}

/* set methods */

/* other methods */

wchar_t *CAVCConfigurationBox::GetParsedHumanReadable(const wchar_t *indent)
{
  wchar_t *result = NULL;
  wchar_t *previousResult = __super::GetParsedHumanReadable(indent);

  if ((previousResult != NULL) && (this->IsParsed()))
  {
    // prepare finally human readable representation
    result = FormatString(
      L"%s"
      ,
      
      previousResult
      );
  }

  FREE_MEM(previousResult);

  return result;
}

uint64_t CAVCConfigurationBox::GetBoxSize(void)
{
  uint64_t result = 0;

  for (unsigned int i = 0; i < this->GetAVCDecoderConfiguration()->GetSequenceParameterSetNALUnits()->Count(); i++)
  {
    result += 2 + this->GetAVCDecoderConfiguration()->GetSequenceParameterSetNALUnits()->GetItem(i)->GetLength();
  }
  for (unsigned int i = 0; i < this->GetAVCDecoderConfiguration()->GetPictureParameterSetNALUnits()->Count(); i++)
  {
    result += 2 + this->GetAVCDecoderConfiguration()->GetPictureParameterSetNALUnits()->GetItem(i)->GetLength();
  }

  result += 7;

  if (result != 0)
  {
    uint64_t boxSize = __super::GetBoxSize();
    result = (boxSize != 0) ? (result + boxSize) : 0; 
  }

  return result;
}

bool CAVCConfigurationBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  //FREE_MEM_CLASS(this->avcDecoderConfiguration);
  //this->avcDecoderConfiguration = new CAVCDecoderConfiguration();

  //bool result = (this->avcDecoderConfiguration != NULL);
  //result &= __super::ParseInternal(buffer, length, false);

  //if (result)
  //{
  //  if (wcscmp(this->type, AVC_CONFIGURATION_BOX_TYPE) != 0)
  //  {
  //    // incorect box type
  //    this->parsed = false;
  //  }
  //  else
  //  {
  //    // box is AVC configuration type box, parse all values
  //    uint32_t position = this->HasExtendedHeader() ? BOX_HEADER_LENGTH_SIZE64 : BOX_HEADER_LENGTH;
  //    bool continueParsing = (this->GetSize() <= (uint64_t)length);

  //    if (continueParsing)
  //    {
  //      this->GetAVCDecoderConfiguration()->SetConfigurationVersion(RBE8(buffer, position));
  //      position++;

  //      this->GetAVCDecoderConfiguration()->SetAvcProfileIndication(RBE8(buffer, position));
  //      position++;

  //      this->GetAVCDecoderConfiguration()->SetProfileCompatibility(RBE8(buffer, position));
  //      position++;

  //      this->GetAVCDecoderConfiguration()->SetAvcLevelIndication(RBE8(buffer, position));
  //      position++;

  //      this->GetAVCDecoderConfiguration()->SetLengthSizeMinusOne(RBE8(buffer, position) & 0x03);
  //      position++;

  //      RBE8INC_DEFINE(buffer, position, sequenceParameterSetNALUnitCount, uint8_t);
  //      sequenceParameterSetNALUnitCount &= 0x1F;

  //      for (unsigned int i = 0; (continueParsing && (i < sequenceParameterSetNALUnitCount)); i++)
  //      {
  //        CSequenceParameterSetNALUnit *unit = new CSequenceParameterSetNALUnit();
  //        RBE16INC_DEFINE(buffer, position, length, uint16_t);

  //        continueParsing &= unit->SetBuffer(buffer + position, length);
  //        position += length;

  //        if (continueParsing)
  //        {
  //          continueParsing &= this->GetAVCDecoderConfiguration()->GetSequenceParameterSetNALUnits()->Add(unit);
  //        }
  //      }

  //      RBE8INC_DEFINE(buffer, position, pictureParameterSetNALUnitCount, uint8_t);

  //      for (unsigned int i = 0; (continueParsing && (i < pictureParameterSetNALUnitCount)); i++)
  //      {
  //        CPictureParameterSetNALUnit *unit = new CPictureParameterSetNALUnit();
  //        RBE16INC_DEFINE(buffer, position, length, uint16_t);

  //        continueParsing &= unit->SetBuffer(buffer + position, length);
  //        position += length;

  //        if (continueParsing)
  //        {
  //          continueParsing &= this->GetAVCDecoderConfiguration()->GetPictureParameterSetNALUnits()->Add(unit);
  //        }
  //      }
  //    }

  //    if (continueParsing && processAdditionalBoxes)
  //    {
  //      this->ProcessAdditionalBoxes(buffer, length, position);
  //    }

  //    this->parsed = continueParsing;
  //  }
  //}

  //result = this->parsed;

  //return result;


  if (__super::ParseInternal(buffer, length, false))
  {
    this->flags &= ~BOX_FLAG_PARSED;
    this->flags |= (wcscmp(this->type, AVC_CONFIGURATION_BOX_TYPE) == 0) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;

    if (this->IsSetFlags(BOX_FLAG_PARSED))
    {
      // box is media data box, parse all values
      uint32_t position = this->HasExtendedHeader() ? BOX_HEADER_LENGTH_SIZE64 : BOX_HEADER_LENGTH;
      HRESULT continueParsing = (this->GetSize() <= (uint64_t)length) ? S_OK : E_NOT_VALID_STATE;

      FREE_MEM_CLASS(this->avcDecoderConfiguration);
      this->avcDecoderConfiguration = new CAVCDecoderConfiguration(&continueParsing);
      CHECK_POINTER_HRESULT(continueParsing, this->avcDecoderConfiguration, continueParsing, E_OUTOFMEMORY);

      if (SUCCEEDED(continueParsing))
      {
        this->GetAVCDecoderConfiguration()->SetConfigurationVersion(RBE8(buffer, position));
        position++;

        this->GetAVCDecoderConfiguration()->SetAvcProfileIndication(RBE8(buffer, position));
        position++;

        this->GetAVCDecoderConfiguration()->SetProfileCompatibility(RBE8(buffer, position));
        position++;

        this->GetAVCDecoderConfiguration()->SetAvcLevelIndication(RBE8(buffer, position));
        position++;

        this->GetAVCDecoderConfiguration()->SetLengthSizeMinusOne(RBE8(buffer, position) & 0x03);
        position++;

        RBE8INC_DEFINE(buffer, position, sequenceParameterSetNALUnitCount, uint8_t);
        sequenceParameterSetNALUnitCount &= 0x1F;

        for (unsigned int i = 0; (SUCCEEDED(continueParsing) && (i < sequenceParameterSetNALUnitCount)); i++)
        {
          CSequenceParameterSetNALUnit *unit = new CSequenceParameterSetNALUnit(&continueParsing);
          CHECK_POINTER_HRESULT(continueParsing, unit, continueParsing, E_OUTOFMEMORY);

          if (SUCCEEDED(continueParsing))
          {
            RBE16INC_DEFINE(buffer, position, length, uint16_t);

            CHECK_CONDITION_HRESULT(continueParsing, unit->SetBuffer(buffer + position, length), continueParsing, E_OUTOFMEMORY);
            position += length;
          }

          CHECK_CONDITION_HRESULT(continueParsing, this->GetAVCDecoderConfiguration()->GetSequenceParameterSetNALUnits()->Add(unit), continueParsing, E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(FAILED(continueParsing), FREE_MEM_CLASS(unit));
        }

        if (SUCCEEDED(continueParsing))
        {
          RBE8INC_DEFINE(buffer, position, pictureParameterSetNALUnitCount, uint8_t);

          for (unsigned int i = 0; (SUCCEEDED(continueParsing) && (i < pictureParameterSetNALUnitCount)); i++)
          {
            CPictureParameterSetNALUnit *unit = new CPictureParameterSetNALUnit(&continueParsing);
            CHECK_POINTER_HRESULT(continueParsing, unit, continueParsing, E_OUTOFMEMORY);

            if (SUCCEEDED(continueParsing))
            {
              RBE16INC_DEFINE(buffer, position, length, uint16_t);

              CHECK_CONDITION_HRESULT(continueParsing, unit->SetBuffer(buffer + position, length), continueParsing, E_OUTOFMEMORY);
              position += length;
            }

            CHECK_CONDITION_HRESULT(continueParsing, this->GetAVCDecoderConfiguration()->GetPictureParameterSetNALUnits()->Add(unit), continueParsing, E_OUTOFMEMORY);
            CHECK_CONDITION_EXECUTE(FAILED(continueParsing), FREE_MEM_CLASS(unit));
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

uint32_t CAVCConfigurationBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);
  result = (this->GetAVCDecoderConfiguration()->GetLengthSizeMinusOne()  < 4) ? result : 0;
  result = (this->GetAVCDecoderConfiguration()->GetSequenceParameterSetNALUnits()->Count() < 32) ? result : 0;

  if (result != 0)
  {
    WBE8INC(buffer, result, this->GetAVCDecoderConfiguration()->GetConfigurationVersion());
    WBE8INC(buffer, result, this->GetAVCDecoderConfiguration()->GetAvcProfileIndication());
    WBE8INC(buffer, result, this->GetAVCDecoderConfiguration()->GetProfileCompatibility());
    WBE8INC(buffer, result, this->GetAVCDecoderConfiguration()->GetAvcLevelIndication());
    WBE8INC(buffer, result, (this->GetAVCDecoderConfiguration()->GetLengthSizeMinusOne() + 0xFC));
    WBE8INC(buffer, result, (this->GetAVCDecoderConfiguration()->GetSequenceParameterSetNALUnits()->Count() + 0xE0));

    for (unsigned int i = 0; i < this->GetAVCDecoderConfiguration()->GetSequenceParameterSetNALUnits()->Count(); i++)
    {
      CSequenceParameterSetNALUnit *unit = this->GetAVCDecoderConfiguration()->GetSequenceParameterSetNALUnits()->GetItem(i);

      uint16_t length = unit->GetLength();
      WBE16INC(buffer, result, length);
      if (length != 0)
      {
        memcpy(buffer + result, unit->GetBuffer(), length);
        result += length;
      }
    }

    WBE8INC(buffer, result, this->GetAVCDecoderConfiguration()->GetPictureParameterSetNALUnits()->Count());

    for (unsigned int i = 0; i < this->GetAVCDecoderConfiguration()->GetPictureParameterSetNALUnits()->Count(); i++)
    {
      CPictureParameterSetNALUnit *unit = this->GetAVCDecoderConfiguration()->GetPictureParameterSetNALUnits()->GetItem(i);

      uint16_t length = unit->GetLength();
      WBE16INC(buffer, result, length);
      if (length != 0)
      {
        memcpy(buffer + result, unit->GetBuffer(), length);
        result += length;
      }
    }

    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}
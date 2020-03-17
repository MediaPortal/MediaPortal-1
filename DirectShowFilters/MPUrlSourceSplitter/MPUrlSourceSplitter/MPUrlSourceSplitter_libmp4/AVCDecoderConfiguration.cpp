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

#include "AVCDecoderConfiguration.h"

CAVCDecoderConfiguration::CAVCDecoderConfiguration(HRESULT *result)
{
  this->configurationVersion = 1;
  this->avcProfileIndication = 0;
  this->profileCompatibility = 0;
  this->avcLevelIndication = 0;
  this->lengthSizeMinusOne = 0;
  this->sequenceParameterSetNALUnits = NULL;
  this->pictureParameterSetNALUnits = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->sequenceParameterSetNALUnits = new CSequenceParameterSetNALUnitCollection(result);
    this->pictureParameterSetNALUnits = new CPictureParameterSetNALUnitCollection(result);

    CHECK_POINTER_HRESULT(*result, this->sequenceParameterSetNALUnits, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->pictureParameterSetNALUnits, *result, E_OUTOFMEMORY);
  }
}

CAVCDecoderConfiguration::~CAVCDecoderConfiguration(void)
{
  FREE_MEM_CLASS(this->sequenceParameterSetNALUnits);
  FREE_MEM_CLASS(this->pictureParameterSetNALUnits);
}

/* get methods */

uint8_t CAVCDecoderConfiguration::GetConfigurationVersion(void)
{
  return this->configurationVersion;
}

uint8_t CAVCDecoderConfiguration::GetAvcProfileIndication(void)
{
  return this->avcProfileIndication;
}

uint8_t CAVCDecoderConfiguration::GetProfileCompatibility(void)
{
  return this->profileCompatibility;
}

uint8_t CAVCDecoderConfiguration::GetAvcLevelIndication(void)
{
  return this->avcLevelIndication;
}

uint8_t CAVCDecoderConfiguration::GetLengthSizeMinusOne(void)
{
  return this->lengthSizeMinusOne;
}

CSequenceParameterSetNALUnitCollection *CAVCDecoderConfiguration::GetSequenceParameterSetNALUnits(void)
{
  return this->sequenceParameterSetNALUnits;
}

CPictureParameterSetNALUnitCollection *CAVCDecoderConfiguration::GetPictureParameterSetNALUnits(void)
{
  return this->pictureParameterSetNALUnits;
}

/* set methods */

void CAVCDecoderConfiguration::SetConfigurationVersion(uint8_t configurationVersion)
{
  this->configurationVersion = configurationVersion;
}

void CAVCDecoderConfiguration::SetAvcProfileIndication(uint8_t avcProfileIndication)
{
  this->avcProfileIndication = avcProfileIndication;
}

void CAVCDecoderConfiguration::SetProfileCompatibility(uint8_t profileCompatibility)
{
  this->profileCompatibility = profileCompatibility;
}

void CAVCDecoderConfiguration::SetAvcLevelIndication(uint8_t avcLevelIndication)
{
  this->avcLevelIndication = avcLevelIndication;
}

void CAVCDecoderConfiguration::SetLengthSizeMinusOne(uint8_t lengthSizeMinusOne)
{
  this->lengthSizeMinusOne = lengthSizeMinusOne;
}

/* other methods */
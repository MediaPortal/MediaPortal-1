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

#pragma once

#ifndef __AVC_DECODER_CONFIGURATION_DEFINED
#define __AVC_DECODER_CONFIGURATION_DEFINED

#include "PictureParameterSetNALUnitCollection.h"
#include "SequenceParameterSetNALUnitCollection.h"

#include <stdint.h>

class CAVCDecoderConfiguration
{
public:
  // creates a new instance of CAVCDecoderConfiguration class
  CAVCDecoderConfiguration(HRESULT *result);

  // destructor
  ~CAVCDecoderConfiguration(void);

  /* get methods */

  // gets configuration version
  // constant, set to 1
  // @return : configuration version
  uint8_t GetConfigurationVersion(void);

  // gets the profile code as defined in ISO/IEC 14496-10
  // @return : profile code as defined in ISO/IEC 14496-10
  uint8_t GetAvcProfileIndication(void);

  // gets byte defined exactly the same as the byte which occurs between the profile_IDC and level_IDC
  // in a sequence parameter set (SPS), as defined in ISO/IEC 14496-10
  // @return : byte defined exactly the same as the byte which occurs between the profile_IDC and level_IDC
  uint8_t GetProfileCompatibility(void);

  // gets the level code as defined in ISO/IEC 14496-10
  // @return : the level code as defined in ISO/IEC 14496-10
  uint8_t GetAvcLevelIndication(void);

  // gets the length in bytes of the NALUnitLength field in an AVC video sample or AVC parameter set sample
  // of the associated stream minus one
  // for example, a size of one byte is indicated with a value of 0
  // the value of this field shall be one of 0, 1, or 3 corresponding to a length encoded with 1, 2, or 4 bytes, respectively
  // @return : the length in bytes of the NALUnitLength field in an AVC video sample or AVC parameter set sample of the associated stream minus one
  uint8_t GetLengthSizeMinusOne(void);

  // gets sequence parameter set NAL units
  // @return : sequence parameter set NAL units
  CSequenceParameterSetNALUnitCollection *GetSequenceParameterSetNALUnits(void);

  // gets picture parameter set NAL units
  // @return : picture parameter set NAL units
  CPictureParameterSetNALUnitCollection *GetPictureParameterSetNALUnits(void);

  /* set methods */

  // sets configuration version
  // @param configuration version : the configuration verrsion to set
  void SetConfigurationVersion(uint8_t configurationVersion);

  // sets the profile code as defined in ISO/IEC 14496-10
  // @param avcProfileIndication : the profile code as defined in ISO/IEC 14496-10 to set
  void SetAvcProfileIndication(uint8_t avcProfileIndication);

  // sets byte defined exactly the same as the byte which occurs between the profile_IDC and level_IDC
  // in a sequence parameter set (SPS), as defined in ISO/IEC 14496-10
  // @param profileCompatibility : byte defined exactly the same as the byte which occurs between the profile_IDC and level_IDC to set
  void SetProfileCompatibility(uint8_t profileCompatibility);

  // sets the level code as defined in ISO/IEC 14496-10
  // @param avcLevelIndication : the level code as defined in ISO/IEC 14496-10 to set
  void SetAvcLevelIndication(uint8_t avcLevelIndication);

  // the length in bytes of the NALUnitLength field in an AVC video sample or AVC parameter set sample
  // of the associated stream minus one
  // for example, a size of one byte is indicated with a value of 0
  // the value of this field shall be one of 0, 1, or 3 corresponding to a length encoded with 1, 2, or 4 bytes, respectively
  // @param lengthSizeMinusOne : the length in bytes to set
  void SetLengthSizeMinusOne(uint8_t lengthSizeMinusOne);

  /* other methods */

private:

  // constant, set to 1
  uint8_t configurationVersion;

  // contains the profile code as defined in ISO/IEC 14496-10
  uint8_t avcProfileIndication;

  // byte defined exactly the same as the byte which occurs between the profile_IDC and level_IDC
  // in a sequence parameter set (SPS), as defined in ISO/IEC 14496-10
  uint8_t profileCompatibility;

  // the level code as defined in ISO/IEC 14496-10
  uint8_t avcLevelIndication;

  // the length in bytes of the NALUnitLength field in an AVC video sample or AVC parameter set sample
  // of the associated stream minus one
  // for example, a size of one byte is indicated with a value of 0
  // the value of this field shall be one of 0, 1, or 3 corresponding to a length encoded with 1, 2, or 4 bytes, respectively
  uint8_t lengthSizeMinusOne;

  CSequenceParameterSetNALUnitCollection *sequenceParameterSetNALUnits;

  CPictureParameterSetNALUnitCollection *pictureParameterSetNALUnits;
};

#endif
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

#include "AttributeFactory.h"
#include "MethodAttribute.h"
#include "UriAttribute.h"
#include "BandwidthAttribute.h"
#include "CodecsAttribute.h"
#include "ProgramIdAttribute.h"

//#include "ResolutionAttribute.h"
//#include "AudioAttribute.h"
//#include "VideoAttribute.h"
//#include "SubtitlesAttribute.h"
//#include "ClosedCaptionsAttribute.h"
//#include "InitializationVectorAttribute.h"
//#include "KeyFormatAttribute.h"
//#include "KeyFormatVersionsAttribute.h"
//#include "ByteRangeAttribute.h"
//#include "TimeOffsetAttribute.h"
//#include "PreciseAttribute.h"
//#include "TypeAttribute.h"
//#include "GroupIdAttribute.h"
//#include "LanguageAttribute.h"
//#include "AssociatedLanguageAttribute.h"
//#include "NameAttribute.h"
//#include "DefaultAttribute.h"
//#include "AutoselectAttribute.h"
//#include "ForcedAttribute.h"
//#include "InstreamIdAttribute.h"
//#include "CharacteristicsAttribute.h"

CAttributeFactory::CAttributeFactory(HRESULT *result)
{
}

CAttributeFactory::~CAttributeFactory(void)
{
}

/* get methods */

/* set methods */

/* other methods */

CAttribute *CAttributeFactory::CreateAttribute(unsigned int version, const wchar_t *buffer, unsigned int length)
{
  CAttribute *result = NULL;
  HRESULT continueParsing = ((buffer != NULL) && (length > 0)) ? S_OK : E_INVALIDARG;

  if (SUCCEEDED(continueParsing))
  {
    CAttribute *attribute = new CAttribute(&continueParsing);
    CHECK_POINTER_HRESULT(continueParsing, attribute, continueParsing, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(continueParsing, attribute->Parse(version, buffer, length), continueParsing, E_FAIL);

    if (SUCCEEDED(continueParsing))
    {
      CREATE_SPECIFIC_ATTRIBUTE(attribute, METHOD_ATTRIBUTE_NAME, CMethodAttribute, continueParsing, result, version);
      CREATE_SPECIFIC_ATTRIBUTE(attribute, URI_ATTRIBUTE_NAME, CUriAttribute, continueParsing, result, version);
      CREATE_SPECIFIC_ATTRIBUTE(attribute, BANDWIDTH_ATTRIBUTE_NAME, CBandwidthAttribute, continueParsing, result, version);
      CREATE_SPECIFIC_ATTRIBUTE(attribute, CODECS_ATTRIBUTE_NAME, CCodecsAttribute, continueParsing, result, version);
      CREATE_SPECIFIC_ATTRIBUTE(attribute, PROGRAM_ID_ATTRIBUTE_NAME, CProgramIdAttribute, continueParsing, result, version);

      //CREATE_SPECIFIC_ATTRIBUTE(attribute, RESOLUTION_ATTRIBUTE_NAME, CResolutionAttribute, continueParsing, result);
      //CREATE_SPECIFIC_ATTRIBUTE(attribute, AUDIO_ATTRIBUTE_NAME, CAudioAttribute, continueParsing, result);
      //CREATE_SPECIFIC_ATTRIBUTE(attribute, VIDEO_ATTRIBUTE_NAME, CVideoAttribute, continueParsing, result);
      //CREATE_SPECIFIC_ATTRIBUTE(attribute, SUBTITLES_ATTRIBUTE_NAME, CSubtitlesAttribute, continueParsing, result);
      //CREATE_SPECIFIC_ATTRIBUTE(attribute, CLOSED_CAPTIONS_ATTRIBUTE_NAME, CClosedCaptionsAttribute, continueParsing, result);
      //CREATE_SPECIFIC_ATTRIBUTE(attribute, INITIALIZATION_VECTOR_ATTRIBUTE_NAME, CInitializationVectorAttribute, continueParsing, result);
      //CREATE_SPECIFIC_ATTRIBUTE(attribute, KEY_FORMAT_ATTRIBUTE_NAME, CKeyFormatAttribute, continueParsing, result);
      //CREATE_SPECIFIC_ATTRIBUTE(attribute, KEY_FORMAT_VERSIONS_ATTRIBUTE_NAME, CKeyFormatVersionsAttribute, continueParsing, result);
      //CREATE_SPECIFIC_ATTRIBUTE(attribute, BYTE_RANGE_ATTRIBUTE_NAME, CByteRangeAttribute, continueParsing, result);
      //CREATE_SPECIFIC_ATTRIBUTE(attribute, TIME_OFFSET_ATTRIBUTE_NAME, CTimeOffsetAttribute, continueParsing, result);
      //CREATE_SPECIFIC_ATTRIBUTE(attribute, PRECISE_ATTRIBUTE_NAME, CPreciseAttribute, continueParsing, result);
      //CREATE_SPECIFIC_ATTRIBUTE(attribute, TYPE_ATTRIBUTE_NAME, CTypeAttribute, continueParsing, result);
      //CREATE_SPECIFIC_ATTRIBUTE(attribute, GROUP_ID_ATTRIBUTE_NAME, CGroupIdAttribute, continueParsing, result);
      //CREATE_SPECIFIC_ATTRIBUTE(attribute, LANGUAGE_ATTRIBUTE_NAME, CLanguageAttribute, continueParsing, result);
      //CREATE_SPECIFIC_ATTRIBUTE(attribute, ASSOCIATED_LANGUAGE_ATTRIBUTE_NAME, CAssociatedLanguageAttribute, continueParsing, result);
      //CREATE_SPECIFIC_ATTRIBUTE(attribute, NAME_ATTRIBUTE_NAME, CNameAttribute, continueParsing, result);
      //CREATE_SPECIFIC_ATTRIBUTE(attribute, DEFAULT_ATTRIBUTE_NAME, CDefaultAttribute, continueParsing, result);
      //CREATE_SPECIFIC_ATTRIBUTE(attribute, AUTOSELECT_ATTRIBUTE_NAME, CAutoselectAttribute, continueParsing, result);
      //CREATE_SPECIFIC_ATTRIBUTE(attribute, FORCED_ATTRIBUTE_NAME, CForcedAttribute, continueParsing, result);
      //CREATE_SPECIFIC_ATTRIBUTE(attribute, INSTREAM_ID_ATTRIBUTE_NAME, CInstreamIdAttribute, continueParsing, result);
      //CREATE_SPECIFIC_ATTRIBUTE(attribute, CHARACTERISTICS_ATTRIBUTE_NAME, CCharacteristicsAttribute, continueParsing, result);*/
    }

    CHECK_CONDITION_NOT_NULL_EXECUTE(result, FREE_MEM_CLASS(attribute));

    if (SUCCEEDED(continueParsing) && (result == NULL))
    {
      result = attribute;
    }

    CHECK_CONDITION_EXECUTE(FAILED(continueParsing), FREE_MEM_CLASS(attribute));
  }

  CHECK_CONDITION_EXECUTE(FAILED(continueParsing), FREE_MEM_CLASS(result));

  return result;
}

/* protected methods */



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

#include "TagFactory.h"

#include "AllowCacheTag.h"
//#include "ByteRangeTag.h"
//#include "DiscontinuitySequenceTag.h"
#include "DiscontinuityTag.h"
#include "EndListTag.h"
#include "HeaderTag.h"
//#include "IndependedSegmentsTag.h"
//#include "IntraFrameOnlyTag.h"
//#include "IntraFrameStreamVariantTag.h"
#include "KeyTag.h"
//#include "MapTag.h"
#include "MediaSequenceTag.h"
//#include "MediaTag.h"
#include "PlaylistTypeTag.h"
#include "ProgramDateTimeTag.h"
//#include "StartTag.h"
#include "StreamVariantTag.h"
#include "TargetDurationTag.h"
#include "VersionTag.h"
#include "DurationTitleTag.h"
#include "DurationTitleFloatingTag.h"
#include "ErrorCodes.h"

CTagFactory::CTagFactory(HRESULT *result)
{
}

CTagFactory::~CTagFactory(void)
{
}

/* get methods */

/* set methods */

/* other methods */

CTag *CTagFactory::CreateTag(HRESULT *result, unsigned int version, CGeneralTag *generalTag)
{
  CTag *tag = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    CHECK_POINTER_DEFAULT_HRESULT(*result, generalTag);

    if (SUCCEEDED(*result))
    {
      CTag *temp = new CTag(result);
      CHECK_POINTER_HRESULT(*result, temp, *result, E_OUTOFMEMORY);

      CHECK_CONDITION_HRESULT(*result, temp->ParseGeneralTag(generalTag, version), *result, E_M3U8_NO_TAG_FOUND);

      if (SUCCEEDED(*result))
      {
        // insert most specific tags on top

        CREATE_SPECIFIC_TAG(temp, TAG_ALLOW_CACHE, CAllowCacheTag, (*result), tag, version);
        //CREATE_SPECIFIC_TAG(temp, TAG_BYTE_RANGE, CByteRangeTag, (*result), tag, version);
        //CREATE_SPECIFIC_TAG(temp, TAG_DISCONTINUITY_SEQUENCE, CDiscontinuitySequenceTag, (*result), tag, version);
        CREATE_SPECIFIC_TAG(temp, TAG_DISCONTINUITY, CDiscontinuityTag, (*result), tag, version);
        CREATE_SPECIFIC_TAG(temp, TAG_END_LIST, CEndListTag, (*result), tag, version);
        CREATE_SPECIFIC_TAG(temp, TAG_HEADER, CHeaderTag, (*result), tag, version);
        //CREATE_SPECIFIC_TAG(temp, TAG_INDEPENDED_SEGMENTS, CIndependedSegmentsTag, (*result), tag, version);
        //CREATE_SPECIFIC_TAG(temp, TAG_INTRA_FRAME_ONLY, CIntraFrameOnlyTag, (*result), tag, version);
        //CREATE_SPECIFIC_TAG(temp, TAG_INTRA_FRAME_STREAM_VARIANT, CIntraFrameStreamVariantTag, (*result), tag, version);
        CREATE_SPECIFIC_TAG(temp, TAG_KEY, CKeyTag, (*result), tag, version);
        //CREATE_SPECIFIC_TAG(temp, TAG_MAP, CMapTag, (*result), tag, version);
        CREATE_SPECIFIC_TAG(temp, TAG_MEDIA_SEQUENCE, CMediaSequenceTag, (*result), tag, version);
        //CREATE_SPECIFIC_TAG(temp, TAG_MEDIA, CMediaTag, (*result), tag, version);
        CREATE_SPECIFIC_TAG(temp, TAG_PLAYLIST_TYPE, CPlaylistTypeTag, (*result), tag, version);
        CREATE_SPECIFIC_TAG(temp, TAG_PROGRAM_DATE_TIME, CProgramDateTimeTag, (*result), tag, version);
        //CREATE_SPECIFIC_TAG(temp, TAG_START, CStartTag, (*result), tag);
        CREATE_SPECIFIC_TAG(temp, TAG_STREAM_VARIANT, CStreamVariantTag, (*result), tag, version);
        CREATE_SPECIFIC_TAG(temp, TAG_TARGET_DURATION, CTargetDurationTag, (*result), tag, version);
        CREATE_SPECIFIC_TAG(temp, TAG_VERSION, CVersionTag, (*result), tag, version);
        CREATE_SPECIFIC_TAG(temp, TAG_DURATION_TITLE, CDurationTitleTag, (*result), tag, version);
        CREATE_SPECIFIC_TAG(temp, TAG_DURATION_TITLE_FLOATING, CDurationTitleFloatingTag, (*result), tag, version);
      }

      CHECK_CONDITION_NOT_NULL_EXECUTE(tag, FREE_MEM_CLASS(temp));

      if (SUCCEEDED(*result) && (tag == NULL))
      {
        tag = temp;
      }

      CHECK_CONDITION_EXECUTE(FAILED(*result), FREE_MEM_CLASS(temp));

    }

    CHECK_CONDITION_EXECUTE(FAILED(*result), FREE_MEM_CLASS(tag));
  }

  return tag;
}
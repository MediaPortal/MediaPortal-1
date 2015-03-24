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

#include "MediaPlaylistV05.h"
#include "ErrorCodes.h"
#include "DurationTitleFloatingTag.h"
#include "DiscontinuityTag.h"
#include "KeyTag.h"
#include "MethodAttribute.h"
#include "EndListTag.h"
#include "ByteRangeTag.h"

CMediaPlaylistV05::CMediaPlaylistV05(HRESULT *result)
  : CMediaPlaylist(result)
{
}

CMediaPlaylistV05::~CMediaPlaylistV05(void)
{
}

/* get methods */

unsigned int CMediaPlaylistV05::GetVersion(void)
{
  return PLAYLIST_VERSION_05;
}

/* set methods */

/* other methods */

/* protected methods */

HRESULT CMediaPlaylistV05::CheckPlaylistVersion(void)
{
  return (PLAYLIST_VERSION_05 == this->detectedVersion) ? S_OK : E_M3U8_NOT_SUPPORTED_PLAYLIST_VERSION;
}

HRESULT CMediaPlaylistV05::ParseTagsAndPlaylistItemsInternal(void)
{
  HRESULT result = __super::ParseTagsAndPlaylistItemsInternal();

  if (SUCCEEDED(result))
  {
    CMediaSequenceTag *mediaSequenceTag = this->tags->GetMediaSequence();
    unsigned int mediaSequence = (mediaSequenceTag != NULL) ? mediaSequenceTag->GetSequenceNumber() : MEDIA_SEQUENCE_ID_V05_DEFAULT;

    unsigned int offset = 0;

    for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->playlistItems->Count())); i++)
    {
      CPlaylistItem *item = this->playlistItems->GetItem(i);

      CM3u8Fragment *fragment = new CM3u8Fragment(&result);
      CHECK_POINTER_HRESULT(result, fragment, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        fragment->SetSequenceNumber(mediaSequence++);
        CHECK_CONDITION_HRESULT(result, fragment->SetUri(item->GetItemContent()), result, E_OUTOFMEMORY);

        for (unsigned int j = 0; (SUCCEEDED(result) && (j < item->GetTags()->Count())); j++)
        {
          CTag *tag = item->GetTags()->GetItem(j);

          CDurationTitleFloatingTag *durationTitle = dynamic_cast<CDurationTitleFloatingTag *>(tag);
          CDiscontinuityTag *discontinuity = dynamic_cast<CDiscontinuityTag *>(tag);
          CKeyTag *key = dynamic_cast<CKeyTag *>(tag);
          CByteRangeTag *byteRange = dynamic_cast<CByteRangeTag *>(tag);

          if (durationTitle != NULL)
          {
            fragment->SetDuration(durationTitle->GetDuration());
          }

          if (discontinuity != NULL)
          {
            fragment->SetDiscontinuity(true);
          }

          if (key != NULL)
          {
            CMethodAttribute *method = dynamic_cast<CMethodAttribute *>(key->GetAttributes()->GetAttribute(METHOD_ATTRIBUTE_NAME, true));
            CHECK_POINTER_HRESULT(result, method, result, E_M3U8_NOT_VALID_PLAYLIST);

            if (SUCCEEDED(result))
            {
              fragment->SetEncrypted(!method->IsNone());
            }
          }

          if (byteRange != NULL)
          {
            CHECK_CONDITION_HRESULT(result, byteRange->GetLength() != BYTE_RANGE_LENGTH_NOT_SPECIFIED, result, E_M3U8_NOT_VALID_PLAYLIST);
            
            if (SUCCEEDED(result))
            {
              CHECK_CONDITION_EXECUTE(byteRange->GetOffset() != BYTE_RANGE_OFFSET_NOT_SPECIFIED, offset = byteRange->GetOffset());

              fragment->SetOffset(offset);
              fragment->SetLength(byteRange->GetLength());

              offset += byteRange->GetLength();
            }
          }
        }
      }

      CHECK_CONDITION_HRESULT(result, fragment->GetDuration() != DURATION_NOT_SPECIFIED, result, E_M3U8_NOT_VALID_PLAYLIST);
      CHECK_CONDITION_HRESULT(result, this->fragments->Add(fragment), result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(fragment));
    }

    if (SUCCEEDED(result) && (this->fragments->Count() != 0))
    {
      // check end list tag
      if (this->tags->GetEndList() != NULL)
      {
        CM3u8Fragment *fragment = this->fragments->GetItem(this->fragments->Count() - 1);

        fragment->SetEndOfStream(true);
      }
    }
  }

  return result;
}
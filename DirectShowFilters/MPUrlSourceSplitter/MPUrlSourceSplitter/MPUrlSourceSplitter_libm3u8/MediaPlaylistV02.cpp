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

#include "MediaPlaylistV02.h"
#include "ErrorCodes.h"
#include "DurationTitleTag.h"
#include "DiscontinuityTag.h"
#include "KeyTag.h"
#include "MethodAttribute.h"
#include "EndListTag.h"

CMediaPlaylistV02::CMediaPlaylistV02(HRESULT *result)
  : CMediaPlaylist(result)
{
}

CMediaPlaylistV02::~CMediaPlaylistV02(void)
{
}

/* get methods */

unsigned int CMediaPlaylistV02::GetVersion(void)
{
  return PLAYLIST_VERSION_02;
}

/* set methods */

/* other methods */

/* protected methods */

HRESULT CMediaPlaylistV02::CheckPlaylistVersion(void)
{
  HRESULT result = (PLAYLIST_VERSION_02 == this->detectedVersion) ? S_OK : E_M3U8_NOT_SUPPORTED_PLAYLIST_VERSION;

  CHECK_CONDITION_EXECUTE(SUCCEEDED(result), this->flags |= PLAYLIST_FLAG_DETECTED_VERSION_01);

  return result;
}

HRESULT CMediaPlaylistV02::ParseTagsAndPlaylistItemsInternal(void)
{
  HRESULT result = __super::ParseTagsAndPlaylistItemsInternal();

  if (SUCCEEDED(result))
  {
    // master playlist version 02 has these tags:
    // EXTM3U - header tag, it is checked in CPlaylist
    // EXTINF - playlist item tag, MUST NOT be in master playlist
    // EXT-X-TARGETDURATION - playlist tag, approximate duration of the next media file that will be added to the main presentation - ignored
    // EXT-X-MEDIA-SEQUENCE - playlist tag, indicates the sequence number of the first URI that appears in a playlist file
    // EXT-X-KEY - multiple playlist item tag, provides information necessary to decrypt media files that follow it - E_DRM_PROTECTED
    // EXT-X-PROGRAM-DATE-TIME - playlist item tag, associates the beginning of the next media file with an absolute date and/or time - ignored
    // EXT-X-ALLOW-CACHE - playlist tag, indicates whether the client MAY cache downloaded media files for later replay - ignored
    // EXT-X-ENDLIST - playlist tag, indicates that no more media files will be added to the Playlist file
    // EXT-X-STREAM-INF - playlist item tag, indicates that the next URI in the playlist file identifies another playlist file
    // EXT-X-DISCONTINUITY - playlist item tag, indicates that the media file following it has different characteristics than the one that preceded it
    // EXT-X-VERSION - playlist tag, the compatibility version of the playlist file

    CMediaSequenceTag *mediaSequenceTag = this->tags->GetMediaSequence();
    unsigned int mediaSequence = (mediaSequenceTag != NULL) ? mediaSequenceTag->GetSequenceNumber() : MEDIA_SEQUENCE_ID_DEFAULT;

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

          CDurationTitleTag *durationTitle = dynamic_cast<CDurationTitleTag *>(tag);
          CDiscontinuityTag *discontinuity = dynamic_cast<CDiscontinuityTag *>(tag);
          CKeyTag *key = dynamic_cast<CKeyTag *>(tag);

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
        }
      }

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
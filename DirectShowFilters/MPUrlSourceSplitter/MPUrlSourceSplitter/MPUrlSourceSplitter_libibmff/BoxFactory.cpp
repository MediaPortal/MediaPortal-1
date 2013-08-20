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

#include "BoxFactory.h"
#include "FileTypeBox.h"
#include "MediaDataBox.h"
#include "MovieBox.h"
#include "MovieHeaderBox.h"
#include "UserDataBox.h"
#include "MetaBox.h"
#include "TrackBox.h"
#include "TrackHeaderBox.h"
#include "MediaBox.h"
#include "HandlerBox.h"
#include "MediaHeaderBox.h"
#include "DataInformationBox.h"
#include "DataEntryUrlBox.h"
#include "DataEntryUrnBox.h"
#include "DataReferenceBox.h"
#include "VideoMediaHeaderBox.h"
#include "SoundMediaHeaderBox.h"
#include "HintMediaHeaderBox.h"
#include "NullMediaHeaderBox.h"
#include "PixelAspectRatioBox.h"
#include "CleanApertureBox.h"
#include "BitRateBox.h"
#include "ChunkOffsetBox.h"
#include "ChunkLargeOffsetBox.h"
#include "SampleToChunkBox.h"
#include "TimeToSampleBox.h"
#include "MovieFragmentBox.h"
#include "MovieExtendsBox.h"
#include "MovieExtendsHeaderBox.h"
#include "TrackExtendsBox.h"
#include "MovieFragmentHeaderBox.h"
#include "TrackFragmentBox.h"
#include "TrackFragmentHeaderBox.h"
#include "UuidBox.h"
#include "TrackRunBox.h"

CBoxFactory::CBoxFactory(void)
{
}

CBoxFactory::~CBoxFactory(void)
{
}

CBox *CBoxFactory::CreateBox(const uint8_t *buffer, uint32_t length)
{
  CBox *result = NULL;
  bool continueParsing = ((buffer != NULL) && (length > 0));

  if (continueParsing)
  {
    CBox *box = new CBox();
    continueParsing &= (box != NULL);

    if (continueParsing)
    {
      continueParsing &= box->Parse(buffer, length);
      if (continueParsing)
      {
        CREATE_SPECIFIC_BOX(box, FILE_TYPE_BOX_TYPE, CFileTypeBox, buffer, length, continueParsing, result);
        CREATE_SPECIFIC_BOX(box, MOVIE_BOX_TYPE, CMovieBox, buffer, length, continueParsing, result);
        CREATE_SPECIFIC_BOX(box, MOVIE_HEADER_BOX_TYPE, CMovieHeaderBox, buffer, length, continueParsing, result);
        CREATE_SPECIFIC_BOX(box, MEDIA_DATA_BOX_TYPE, CMediaDataBox, buffer, length, continueParsing, result);
        CREATE_SPECIFIC_BOX(box, USER_DATA_BOX_TYPE, CUserDataBox, buffer, length, continueParsing, result);
        CREATE_SPECIFIC_BOX(box, META_BOX_TYPE, CMetaBox, buffer, length, continueParsing, result);
        CREATE_SPECIFIC_BOX(box, TRACK_BOX_TYPE, CTrackBox, buffer, length, continueParsing, result);
        CREATE_SPECIFIC_BOX(box, TRACK_HEADER_BOX_TYPE, CTrackHeaderBox, buffer, length, continueParsing, result);
        CREATE_SPECIFIC_BOX(box, MEDIA_BOX_TYPE, CMediaBox, buffer, length, continueParsing, result);
        CREATE_SPECIFIC_BOX(box, HANDLER_BOX_TYPE, CHandlerBox, buffer, length, continueParsing, result);
        CREATE_SPECIFIC_BOX(box, MEDIA_HEADER_BOX_TYPE, CMediaHeaderBox, buffer, length, continueParsing, result);
        CREATE_SPECIFIC_BOX(box, DATA_INFORMATION_BOX_TYPE, CDataInformationBox, buffer, length, continueParsing, result);
        CREATE_SPECIFIC_BOX(box, DATA_ENTRY_URL_BOX_TYPE, CDataEntryUrlBox, buffer, length, continueParsing, result);
        CREATE_SPECIFIC_BOX(box, DATA_ENTRY_URN_BOX_TYPE, CDataEntryUrnBox, buffer, length, continueParsing, result);
        CREATE_SPECIFIC_BOX(box, DATA_REFERENCE_BOX_TYPE, CDataReferenceBox, buffer, length, continueParsing, result);
        CREATE_SPECIFIC_BOX(box, VIDEO_MEDIA_HEADER_BOX_TYPE, CVideoMediaHeaderBox, buffer, length, continueParsing, result);
        CREATE_SPECIFIC_BOX(box, SOUND_MEDIA_HEADER_BOX_TYPE, CSoundMediaHeaderBox, buffer, length, continueParsing, result);
        CREATE_SPECIFIC_BOX(box, HINT_MEDIA_HEADER_BOX_TYPE, CHintMediaHeaderBox, buffer, length, continueParsing, result);
        CREATE_SPECIFIC_BOX(box, NULL_MEDIA_HEADER_BOX_TYPE, CNullMediaHeaderBox, buffer, length, continueParsing, result);
        CREATE_SPECIFIC_BOX(box, PIXEL_ASPECT_RATIO_BOX_TYPE, CPixelAspectRatioBox, buffer, length, continueParsing, result);
        CREATE_SPECIFIC_BOX(box, CLEAN_APERTURE_BOX_TYPE, CCleanApertureBox, buffer, length, continueParsing, result);
        CREATE_SPECIFIC_BOX(box, BITRATE_BOX_TYPE, CBitrateBox, buffer, length, continueParsing, result);
        CREATE_SPECIFIC_BOX(box, CHUNK_OFFSET_BOX_TYPE, CChunkOffsetBox, buffer, length, continueParsing, result);
        CREATE_SPECIFIC_BOX(box, CHUNK_LARGE_OFFSET_BOX_TYPE, CChunkLargeOffsetBox, buffer, length, continueParsing, result);
        CREATE_SPECIFIC_BOX(box, SAMPLE_TO_CHUNK_BOX_TYPE, CSampleToChunkBox, buffer, length, continueParsing, result);
        CREATE_SPECIFIC_BOX(box, TIME_TO_SAMPLE_BOX_TYPE, CTimeToSampleBox, buffer, length, continueParsing, result);
        CREATE_SPECIFIC_BOX(box, MOVIE_FRAGMENT_BOX_TYPE, CMovieFragmentBox, buffer, length, continueParsing, result);
        CREATE_SPECIFIC_BOX(box, MOVIE_EXTENDS_BOX_TYPE, CMovieExtendsBox, buffer, length, continueParsing, result);
        CREATE_SPECIFIC_BOX(box, MOVIE_EXTENDS_HEADER_BOX_TYPE, CMovieExtendsHeaderBox, buffer, length, continueParsing, result);
        CREATE_SPECIFIC_BOX(box, TRACK_EXTENDS_BOX_TYPE, CTrackExtendsBox, buffer, length, continueParsing, result);
        CREATE_SPECIFIC_BOX(box, MOVIE_FRAGMENT_HEADER_BOX_TYPE, CMovieFragmentHeaderBox, buffer, length, continueParsing, result);
        CREATE_SPECIFIC_BOX(box, TRACK_FRAGMENT_BOX_TYPE, CTrackFragmentBox, buffer, length, continueParsing, result);
        CREATE_SPECIFIC_BOX(box, TRACK_FRAGMENT_HEADER_BOX_TYPE, CTrackFragmentHeaderBox, buffer, length, continueParsing, result);
        CREATE_SPECIFIC_BOX(box, UUID_BOX_TYPE, CUuidBox, buffer, length, continueParsing, result);
        CREATE_SPECIFIC_BOX(box, TRACK_RUN_BOX_TYPE, CTrackRunBox, buffer, length, continueParsing, result);
      }
    }

    if (continueParsing && (result == NULL))
    {
      result = box;
    }

    if (!continueParsing)
    {
      FREE_MEM_CLASS(box);
    }
  }

  return result;
}
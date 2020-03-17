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

#ifndef __TRACK_HEADER_BOX_DEFINED
#define __TRACK_HEADER_BOX_DEFINED

#include "FullBox.h"
#include "FixedPointNumber.h"
#include "Matrix.h"

#define TRACK_HEADER_BOX_TYPE                                         L"tkhd"

// indicates that the track is enabled
#define FLAGS_TRACK_ENABLED                                           0x00000001
// indicates that the track is used in the presentation
#define FLAGS_TRACK_IN_MOVIE                                          0x00000002
// indicates that the track is used when previewing the presentation
#define FLAGS_TRACK_IN_PREVIEW                                        0x00000004

#define TRACK_HEADER_BOX_FLAG_NONE                                    FULL_BOX_FLAG_NONE

#define TRACK_HEADER_BOX_FLAG_LAST                                    (FULL_BOX_FLAG_LAST + 0)

class CTrackHeaderBox :
  public CFullBox
{
public:
  // initializes a new instance of CTrackHeaderBox class
  CTrackHeaderBox(HRESULT *result);

  // destructor
  virtual ~CTrackHeaderBox(void);

  /* get methods */

  // gets the creation time of this track (in seconds since midnight, Jan. 1, 1904, in UTC time)
  // @return : the creation time of this track (in seconds since midnight, Jan. 1, 1904, in UTC time)
  virtual uint64_t GetCreationTime(void);

  // gets the most recent time the track was modified (in seconds since midnight, Jan. 1, 1904, in UTC time)
  // @return : the most recent time the track was modified (in seconds since midnight, Jan. 1, 1904, in UTC time)
  virtual uint64_t GetModificationTime(void);

  // gets unique track ID over the entire life-time of this presentation
  // track IDs are never re-used and cannot be zero
  // @return : unique track ID over the entire life-time of this presentation
  virtual uint32_t GetTrackId(void);

  // gets front-to-back ordering of video tracks; tracks with lower numbers are closer to the viewer
  // 0 is the normal value, and -1 would be in front of track 0, and so on
  // @return : front-to-back ordering of video tracks
  virtual int16_t GetLayer(void);

  // gets a group or collection of tracks
  // if this field is 0 there is no information on possible relations to other tracks
  // if this field is not 0, it should be the same for tracks that contain alternate data for one another
  // and different for tracks belonging to different such groups
  // only one track within an alternate group should be played or streamed at any one time, and must be distinguishable
  // from other tracks in the group via attributes such as bitrate, codec, language, packet size etc
  // a group may have only one member
  // @return : group or collection of tracks
  virtual int16_t GetAlternateGroup(void);

  // gets  duration of this track (in the timescale indicated in the Movie Header Box)
  // the value of this field is equal to the sum of the durations of all of the track’s edits
  // if there is no edit list, then the duration is the sum of the sample durations, converted into the timescale
  // in the Movie Header Box
  // if the duration of this track cannot be determined then duration is set to all 1s (32-bit maxint)
  // @return : duration of this track (in the timescale indicated in the Movie Header Box)
  virtual uint64_t GetDuration(void);

  // gets value specifying the track's relative audio volume
  // full volume is 1.0 and is the normal value
  // its value is irrelevant for a purely visual track
  // tracks may be composed by combining them according to their volume, and then using the overall Movie Header Box volume
  // setting; or more complex audio composition (e.g. MPEG-4 BIFS) may be used
  // @return : value specifying the track's relative audio volume
  virtual CFixedPointNumber *GetVolume(void);

  // gets transformation matrix for the video; (u,v,w) are restricted here to (0,0,1)
  // @return : transformation matrix for the video
  virtual CMatrix *GetMatrix(void);

  // gets track's visual presentation width
  // @return : track's visual presentation width
  virtual CFixedPointNumber *GetWidth(void);

  // gets track's visual presentation height
  // @return : track's visual presentation height
  virtual CFixedPointNumber *GetHeight(void);

  /* set methods */

  // sets the creation time of this track (in seconds since midnight, Jan. 1, 1904, in UTC time)
  // @param creationTime : the creation time of this track (in seconds since midnight, Jan. 1, 1904, in UTC time) to set
  virtual void SetCreationTime(uint64_t creationTime);

  // sets the most recent time the track was modified (in seconds since midnight, Jan. 1, 1904, in UTC time)
  // @param modificationTime : the most recent time the track was modified (in seconds since midnight, Jan. 1, 1904, in UTC time) to set
  virtual void SetModificationTime(uint64_t modificationTime);

  // sets unique track ID over the entire life-time of this presentation
  // track IDs are never re-used and cannot be zero
  // @param trackId : unique track ID over the entire life-time of this presentation to set
  virtual void SetTrackId(uint32_t trackId);

  // sets front-to-back ordering of video tracks; tracks with lower numbers are closer to the viewer
  // 0 is the normal value, and -1 would be in front of track 0, and so on
  // @param layer : front-to-back ordering of video tracks to set
  virtual void SetLayer(int16_t layer);

  // sets a group or collection of tracks
  // if this field is 0 there is no information on possible relations to other tracks
  // if this field is not 0, it should be the same for tracks that contain alternate data for one another
  // and different for tracks belonging to different such groups
  // only one track within an alternate group should be played or streamed at any one time, and must be distinguishable
  // from other tracks in the group via attributes such as bitrate, codec, language, packet size etc
  // a group may have only one member
  // @param alternateGroup : group or collection of tracks to set
  virtual void SetAlternateGroup(int16_t alternateGroup);

  // sets  duration of this track (in the timescale indicated in the Movie Header Box)
  // the value of this field is equal to the sum of the durations of all of the track’s edits
  // if there is no edit list, then the duration is the sum of the sample durations, converted into the timescale
  // in the Movie Header Box
  // if the duration of this track cannot be determined then duration is set to all 1s (32-bit maxint)
  // @param duration : duration of this track (in the timescale indicated in the Movie Header Box) to set
  virtual void SetDuration(uint64_t duration);

  /* other methods */

  // gets box data in human readable format
  // @param indent : string to insert before each line
  // @return : box data in human readable format or NULL if error
  virtual wchar_t *GetParsedHumanReadable(const wchar_t *indent);

protected:

  // declares the creation time of this track (in seconds since midnight, Jan. 1, 1904, in UTC time)
  uint64_t creationTime;

  // declares the most recent time the track was modified (in seconds since midnight, Jan. 1, 1904, in UTC time)
  uint64_t modificationTime;

  // uniquely identifies this track over the entire life-time of this presentation
  // track IDs are never re-used and cannot be zero
  uint32_t trackId;

  // front-to-back ordering of video tracks; tracks with lower numbers are closer to the viewer
  // 0 is the normal value, and -1 would be in front of track 0, and so on
  int16_t layer;

  // specifies a group or collection of tracks
  // if this field is 0 there is no information on possible relations to other tracks
  // if this field is not 0, it should be the same for tracks that contain alternate data for one another
  // and different for tracks belonging to different such groups
  // only one track within an alternate group should be played or streamed at any one time, and must be distinguishable
  // from other tracks in the group via attributes such as bitrate, codec, language, packet size etc
  // a group may have only one member
  int16_t alternateGroup;

  // indicates the duration of this track (in the timescale indicated in the Movie Header Box)
  // the value of this field is equal to the sum of the durations of all of the track’s edits
  // if there is no edit list, then the duration is the sum of the sample durations, converted into the timescale
  // in the Movie Header Box
  // if the duration of this track cannot be determined then duration is set to all 1s (32-bit maxint)
  uint64_t duration;

  // fixed 8.8 value specifying the track's relative audio volume
  // full volume is 1.0 (0x0100) and is the normal value
  // its value is irrelevant for a purely visual track
  // tracks may be composed by combining them according to their volume, and then using the overall Movie Header Box volume
  // setting; or more complex audio composition (e.g. MPEG-4 BIFS) may be used
  CFixedPointNumber *volume;

  // transformation matrix for the video; (u,v,w) are restricted here to (0,0,1)
  CMatrix *matrix;

  // track's visual presentation width as fixed-point 16.16 values
  CFixedPointNumber *width;

  // track's visual presentation height as fixed-point 16.16 values
  CFixedPointNumber *height;

  // gets whole box size
  // method is called to determine whole box size for storing box into buffer
  // @return : size of box 
  virtual uint64_t GetBoxSize(void);

  // parses data in buffer
  // @param buffer : buffer with box data for parsing
  // @param length : the length of data in buffer
  // @param processAdditionalBoxes : specifies if additional boxes have to be processed
  // @return : true if parsed successfully, false otherwise
  virtual bool ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes);

  // gets whole box into buffer (buffer must be allocated before)
  // @param buffer : the buffer for box data
  // @param length : the length of buffer for data
  // @param processAdditionalBoxes : specifies if additional boxes have to be processed (added to buffer)
  // @return : number of bytes stored into buffer, 0 if error
  virtual uint32_t GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes);
};

#endif
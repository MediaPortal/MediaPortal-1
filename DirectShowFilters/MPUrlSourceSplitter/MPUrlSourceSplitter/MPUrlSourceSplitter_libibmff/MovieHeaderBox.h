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

#ifndef __MOVIE_HEADER_BOX_DEFINED
#define __MOVIE_HEADER_BOX_DEFINED

#include "FullBox.h"
#include "FixedPointNumber.h"
#include "Matrix.h"

#define MOVIE_HEADER_BOX_TYPE                                         L"mvhd"

#define MOVIE_HEADER_DATA_VERSION_0_SIZE                              96
#define MOVIE_HEADER_DATA_VERSION_1_SIZE                              MOVIE_HEADER_DATA_VERSION_0_SIZE + 12

#define MOVIE_HEADER_BOX_FLAG_NONE                                    FULL_BOX_FLAG_NONE

#define MOVIE_HEADER_BOX_FLAG_LAST                                    (FULL_BOX_FLAG_LAST + 0)

class CMovieHeaderBox :
  public CFullBox
{
public:
  // initializes a new instance of CMovieHeaderBox class
  CMovieHeaderBox(HRESULT *result);

  // destructor
  virtual ~CMovieHeaderBox(void);

  /* get methods */

  // gets the creation time of the presentation (in seconds since midnight, Jan. 1, 1904, in UTC time)
  // @return : the creation time
  virtual uint64_t GetCreationTime(void);

  // gets the most recent time the presentation was modified (in seconds since midnight, Jan. 1, 1904, in UTC time)
  // @return : the most recent time the presentation was modified 
  virtual uint64_t GetModificationTime(void);

  // gets the time-scale for the entire presentation
  // @return : the time-scale for the entire presentation
  virtual uint32_t GetTimeScale(void);

  // gets length of the presentation (in the indicated timescale)
  // @return : length of the presentation (in the indicated timescale)
  virtual uint64_t GetDuration(void);

  // gets the fixed point 16.16 number that indicates the preferred rate to play the presentation
  // @return : the fixed point 16.16 number that indicates the preferred rate to play the presentation
  virtual CFixedPointNumber *GetRate(void);

  // gets the fixed point 8.8 number that indicates the preferred playback volume
  // @return : the fixed point 8.8 number that indicates the preferred playback volume
  virtual CFixedPointNumber *GetVolume(void);

  // gets the transformation matrix for the video
  // @return : the transformation matrix for the video
  virtual CMatrix *GetMatrix(void);

  // gets next track ID
  // @return : next track ID
  virtual uint32_t GetNextTrackId(void);

  /* set methods */

  // sets the creation time of the presentation (in seconds since midnight, Jan. 1, 1904, in UTC time)
  // @param creationTime : the creation time to set
  virtual void SetCreationTime(uint64_t creationTime);

  // sets the most recent time the presentation was modified (in seconds since midnight, Jan. 1, 1904, in UTC time)
  // @param modificationTime : the most recent time the presentation was modified to set
  virtual void SetModificationTime(uint64_t modificationTime);

  // sets the time-scale for the entire presentation
  // @param timeScale : the time-scale for the entire presentation to set
  virtual void SetTimeScale(uint32_t timeScale);

  // sets length of the presentation (in the indicated timescale)
  // @param duration : length of the presentation (in the indicated timescale) to set
  virtual void SetDuration(uint64_t duration);

  // sets next track ID
  // @param nextTrackId : next track ID to set
  virtual void SetNextTrackId(uint32_t nextTrackId);

  /* other methods */

  // gets box data in human readable format
  // @param indent : string to insert before each line
  // @return : box data in human readable format or NULL if error
  virtual wchar_t *GetParsedHumanReadable(const wchar_t *indent);

protected:

  // declares the creation time of the presentation (in seconds since midnight, Jan. 1, 1904, in UTC time)
  uint64_t creationTime;
  // declares the most recent time the presentation was modified (in seconds since midnight, Jan. 1, 1904, in UTC time)
  uint64_t modificationTime;
  // specifies the time-scale for the entire presentation; this is the number of time units that pass in one second
  // for example, a time coordinate system that measures time in sixtieths of a second has a time scale of 60
  uint32_t timeScale;
  // declares length of the presentation (in the indicated timescale)
  // this property is derived from the presentation’s tracks: the value of this field corresponds to
  // the duration of the longest track in the presentation
  uint64_t duration;
  // fixed point 16.16 number that indicates the preferred rate to play the presentation
  // 1.0 (0x00010000) is normal forward playback
  CFixedPointNumber *rate;
  // fixed point 8.8 number that indicates the preferred playback volume
  // 1.0 (0x0100) is full volume
  CFixedPointNumber *volume;
  // transformation matrix for the video; (u,v,w) are restricted here to (0,0,1)
  CMatrix *matrix;
  // non-zero integer that indicates a value to use for the track ID of the next track to be added to this presentation
  // zero is not a valid track ID value
  // the value of next_track_ID shall be larger than the largest track-ID in use
  // if this value is equal to all 1s (32-bit maxint), and a new media track is to be added,
  // then a search must be made in the file for an unused track identifier
  uint32_t nextTrackId;

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
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

#ifndef __MSHS_MANIFEST_STREAM_BOX_DEFINED
#define __MSHS_MANIFEST_STREAM_BOX_DEFINED

#include "MshsManifestTrackBoxCollection.h"
#include "MshsManifestStreamFragmentBoxCollection.h"
#include "Box.h"

#define MSHS_MANIFEST_STREAM_BOX_TYPE                                 L"mssr"

#define MSHS_MANIFEST_STREAM_BOX_FLAG_NONE                            BOX_FLAG_NONE

#define MSHS_MANIFEST_STREAM_BOX_FLAG_LAST                            (BOX_FLAG_LAST + 0)

#define STREAM_TYPE_VIDEO                                             L"video"
#define STREAM_TYPE_AUDIO                                             L"audio"
#define STREAM_TYPE_TEXT                                              L"text"

class CMshsManifestStreamBox : public CBox
{
public:
  // creats new instance of CMshsManifestStreamBox class
  CMshsManifestStreamBox(HRESULT *result);
  // desctructor
  virtual ~CMshsManifestStreamBox(void);

  /* get methods */

  // gets the type of the Stream: video, audio, or text
  // if the type specified is text, the following field is required and MUST appear in StreamAttributes: SubtypeAttribute
  // unless the type specified is video, the following fields MUST NOT appear in StreamAttributes:
  // StreamMaxWidthAttribute, StreamMaxHeightAttribute, DisplayWidthAttribute, DisplayHeightAttribute
  // @return : the type of the Stream: video, audio, or text
  const wchar_t *GetStreamBoxType(void);

  // gets a four-character code that identifies the intended use category for each Sample in a text Track
  // however, the FourCC field, specified in section 2.2.2.5 , is used to identify the Media Format for each Sample
  // the following range of values is reserved, with the following semantic meanings:
  // "SCMD": Triggers for actions by the higher-layer implementation on the Client
  // "CHAP": Chapter markers
  // "SUBT": Subtitles used for foreign-language audio
  // "CAPT": Closed captions for the hearing-impaired
  // "DESC": Media descriptions for the hearing-impaired
  // "CTRL": Events the control application business logic
  // "DATA": Application data that does not fall into any of the above categories
  // @return : a four-character code that identifies the intended use category for each Sample in a text Track
  const wchar_t *GetSubType(void);

  // gets a pattern used by the Client to generate Fragment Request messages
  // @return : a pattern used by the Client to generate Fragment Request messages
  const wchar_t *GetUrl(void);

  // gets the time scale for duration and time values in this Stream, specified as the number of increments in one second
  // @return : the time scale for duration and time values in this Stream, specified as the number of increments in one second
  uint64_t GetTimeScale(void);

  // gets the name of the Stream
  // @return : the name of the Stream
  const wchar_t *GetName(void);

  // gets the maximum width of a video Sample, in pixels
  // @return : the maximum width of a video Sample, in pixels
  uint32_t GetMaxWidth(void);

  // gets the maximum height of a video Sample, in pixels
  // @return : the maximum height of a video Sample, in pixels
  uint32_t GetMaxHeight(void);

  // gets the suggested display width of a video Sample, in pixels
  // @return : the suggested display width of a video Sample, in pixels
  uint32_t GetDisplayWidth(void);

  // gets the suggested display height of a video Sample, in pixels
  // @return : the suggested display height of a video Sample, in pixels
  uint32_t GetDisplayHeight(void);

  // gets tracks associated with stream
  // @return : stream tracks
  CMshsManifestTrackBoxCollection *GetTracks(void);

  // gets stream fragments
  // @return : stream fragments
  CMshsManifestStreamFragmentBoxCollection *GetStreamFragments(void);

  /* set methods */

  // sets the type of the Stream: video, audio, or text
  // @param streamBoxType : the type of the Stream: video, audio, or text to set
  // @return : true if successful, false otherwise
  bool SetStreamBoxType(const wchar_t *streamBoxType);

  // sets a four-character code that identifies the intended use category for each Sample in a text Track
  // @param subType : a four-character code that identifies the intended use category for each Sample in a text Track to set
  // @return : true if successful, false otherwise
  bool SetSubType(const wchar_t *subType);

  // sets a pattern used by the Client to generate Fragment Request messages
  // @param url: a pattern used by the Client to generate Fragment Request messages to set
  // @return : true if successful, false otherwise
  bool SetUrl(const wchar_t *url);

  // sets the time scale for duration and time values in this Stream, specified as the number of increments in one second
  // @param timeScale : the time scale for duration and time values in this Stream to set, specified as the number of increments in one second
  void SetTimeScale(uint64_t timeScale);

  // sets the name of the Stream
  // @param name : the name of the Stream set
  // @return : true if successful, false otherwise
  bool SetName(const wchar_t *name);

  // sets the maximum width of a video Sample, in pixels
  // @param maxWidth: the maximum width of a video Sample to set
  void SetMaxWidth(uint32_t maxWidth);

  // sets the maximum height of a video Sample, in pixels
  // @param maxHeight : the maximum height of a video Sample to set
  void SetMaxHeight(uint32_t maxHeight);

  // sets the suggested display width of a video Sample, in pixels
  // @param displayWidth : the suggested display width of a video Sample to set
  void SetDisplayWidth(uint32_t displayWidth);

  // sets the suggested display height of a video Sample, in pixels
  // @param displayHeight: the suggested display height of a video Sample to set
  void SetDisplayHeight(uint32_t displayHeight);

  /* other methods */

  // tests if stream is video type stream
  // @return : true if stream is video type stream, false otherwise
  bool IsVideo(void);

  // tests if stream is audio type stream
  // @return : true if stream is audio type stream, false otherwise
  bool IsAudio(void);

  // tests if stream is text type stream
  // @return : true if stream is text type stream, false otherwise
  bool IsText(void);

  // gets box data in human readable format
  // @param indent : string to insert before each line
  // @return : box data in human readable format or NULL if error
  virtual wchar_t *GetParsedHumanReadable(const wchar_t *indent);

private:

  // the type of the Stream: video, audio, or text
  // if the type specified is text, the following field is required and MUST appear in StreamAttributes: SubtypeAttribute
  // unless the type specified is video, the following fields MUST NOT appear in StreamAttributes:
  // StreamMaxWidthAttribute, StreamMaxHeightAttribute, DisplayWidthAttribute, DisplayHeightAttribute
  wchar_t *streamBoxType;

  // a four-character code that identifies the intended use category for each Sample in a text Track
  // however, the FourCC field, specified in section 2.2.2.5 , is used to identify the Media Format for each Sample
  // the following range of values is reserved, with the following semantic meanings:
  // "SCMD": Triggers for actions by the higher-layer implementation on the Client
  // "CHAP": Chapter markers
  // "SUBT": Subtitles used for foreign-language audio
  // "CAPT": Closed captions for the hearing-impaired
  // "DESC": Media descriptions for the hearing-impaired
  // "CTRL": Events the control application business logic
  // "DATA": Application data that does not fall into any of the above categories
  wchar_t *subType;

  // a pattern used by the Client to generate Fragment Request messages
  wchar_t *url;

  // the time scale for duration and time values in this Stream, specified as the number of increments in one second
  uint64_t timeScale;

  // the name of the Stream
  wchar_t *name;

  // the maximum width of a video Sample, in pixels
  uint32_t maxWidth;

  // the maximum height of a video Sample, in pixels
  uint32_t maxHeight;

  // the suggested display width of a video Sample, in pixels
  uint32_t displayWidth;

  // the suggested display height of a video Sample, in pixels
  uint32_t displayHeight;

  CMshsManifestTrackBoxCollection *tracks;

  CMshsManifestStreamFragmentBoxCollection *streamFragments;

  /* methods */

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
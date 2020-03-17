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

#ifndef __M3U8_DECRYPTION_CONTEXT_DEFINED
#define __M3U8_DECRYPTION_CONTEXT_DEFINED

#include "M3u8StreamFragmentCollection.h"
#include "M3u8CurlInstance.h"
#include "Flags.h"
#include "ParameterCollection.h"

#define M3U8_DECRYPTION_CONTEXT_FLAG_NONE                             FLAGS_NONE

#define M3U8_DECRYPTION_CONTEXT_FLAG_LAST                             (FLAGS_LAST + 0)

class CM3u8DecryptionContext : public CFlags
{
public:
  CM3u8DecryptionContext(HRESULT *result);
  ~CM3u8DecryptionContext();

  /* get methods */

  // gets M3U8 CURL instance (it's only reference to M3U8 CURL instance in M3U8 protocol)
  // @return : M3U8 CURL instance
  CM3u8CurlInstance *GetCurlInstance(void);

  // gets stream fragments collection (it's only reference to stream fragments in M3U8 protocol)
  // @return : stream fragments collection
  CM3u8StreamFragmentCollection *GetStreamFragments(void);

  // gets which stream fragment is currently downloading (UINT_MAX means none)
  // @return : which stream fragment is currently downloading (UINT_MAX means none)
  unsigned int GetStreamFragmentDownloading(void);

  // gets which stream fragment is currently processed
  // @return : which stream fragment is currently processed
  unsigned int GetStreamFragmentProcessing(void);

  // gets which stream fragment have to be downloaded
  // @return : which stream fragment have to be downloaded (UINT_MAX means next stream fragment, always reset after started download of stream fragment)
  unsigned int GetStreamFragmentToDownload(void);

  // gets playlist url
  // @return : playlist url
  const wchar_t *GetPlaylistUrl(void);

  // gets actual configuration (only reference)
  // @return : actual configuration
  CParameterCollection *GetConfiguration(void);

  /* set methods */

  // sets M3U8 CURL instance (it's only reference to M3U8 CURL instance in M3U8 protocol)
  // @param curlInstance : M3U8 CURL instance to set
  void SetCurlInstance(CM3u8CurlInstance *curlInstance);

  // sets stream fragments collection (it's only reference to stream fragments in M3U8 protocol)
  // @param streamFragments : stream fragments collection to set
  void SetStreamFragments(CM3u8StreamFragmentCollection *streamFragments);

  // sets which stream fragment is currently downloading (UINT_MAX means none)
  // @param streamFragmentDownloading : which stream fragment is currently downloading (UINT_MAX means none)
  void SetStreamFragmentDownloading(unsigned int streamFragmentDownloading);

  // sets which stream fragment is currently processed
  // @param streamFragmentProcessing : which stream fragment is currently processed
  void SetStreamFragmentProcessing(unsigned int streamFragmentProcessing);

  // sets which stream fragment have to be downloaded
  // @param streamFragmentToDownload : which stream fragment have to be downloaded (UINT_MAX means next stream fragment, always reset after started download of stream fragment)
  void SetStreamFragmentToDownload(unsigned int streamFragmentToDownload);

  // sets playlist url
  // @param playlistUrl : playlist url to set (only reference, not deep clone)
  void SetPlaylistUrl(const wchar_t *playlistUrl);

  // sets actual configuration (only reference)
  // @param configuration : actual configuration
  void SetConfiguration(CParameterCollection *configuration);

  /* other methods */

protected:
  // holds M3U8 CURL instance (it's only reference to M3U8 CURL instance in M3U8 protocol)
  CM3u8CurlInstance *curlInstance;
  // holds stream fragments collection (it's only reference to stream fragments in M3U8 protocol)
  CM3u8StreamFragmentCollection *streamFragments;
  // holds which stream fragment is currently downloading (UINT_MAX means none)
  unsigned int streamFragmentDownloading;
  // holds which stream fragment is currently processed
  unsigned int streamFragmentProcessing;
  // holds which stream fragment have to be downloaded
  unsigned int streamFragmentToDownload;
  // holds playlist url (only reference, not deep clone)
  const wchar_t *playlistUrl;
  // holds actual configuration (only reference)
  CParameterCollection *configuration;

  /* methods */
};

#endif
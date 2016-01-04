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

#include "stdafx.h"

#pragma warning(push)
// disable warning: 'INT8_MIN' : macro redefinition
// warning is caused by stdint.h and intsafe.h, which both define same macro
#pragma warning(disable:4005)

#include "M3u8DecryptionContext.h"

#pragma warning(pop)

CM3u8DecryptionContext::CM3u8DecryptionContext(HRESULT *result)
  : CFlags()
{
  this->curlInstance = NULL;
  this->streamFragments = NULL;
  this->streamFragmentDownloading = UINT_MAX;
  this->streamFragmentProcessing = UINT_MAX;
  this->streamFragmentToDownload = UINT_MAX;
  this->playlistUrl = NULL;
  this->configuration = NULL;
}

CM3u8DecryptionContext::~CM3u8DecryptionContext()
{
}

/* get methods */

CM3u8CurlInstance *CM3u8DecryptionContext::GetCurlInstance(void)
{
  return this->curlInstance;
}

CM3u8StreamFragmentCollection *CM3u8DecryptionContext::GetStreamFragments(void)
{
  return this->streamFragments;
}

unsigned int CM3u8DecryptionContext::GetStreamFragmentDownloading(void)
{
  return this->streamFragmentDownloading;
}

unsigned int CM3u8DecryptionContext::GetStreamFragmentProcessing(void)
{
  return this->streamFragmentProcessing;
}

unsigned int CM3u8DecryptionContext::GetStreamFragmentToDownload(void)
{
  return this->streamFragmentToDownload;
}

const wchar_t *CM3u8DecryptionContext::GetPlaylistUrl(void)
{
  return this->playlistUrl;
}

CParameterCollection *CM3u8DecryptionContext::GetConfiguration(void)
{
  return this->configuration;
}

/* set methods */

void CM3u8DecryptionContext::SetCurlInstance(CM3u8CurlInstance *curlInstance)
{
  this->curlInstance = curlInstance;
}

void CM3u8DecryptionContext::SetStreamFragments(CM3u8StreamFragmentCollection *streamFragments)
{
  this->streamFragments = streamFragments;
}

void CM3u8DecryptionContext::SetStreamFragmentDownloading(unsigned int streamFragmentDownloading)
{
  this->streamFragmentDownloading = streamFragmentDownloading;
}

void CM3u8DecryptionContext::SetStreamFragmentProcessing(unsigned int streamFragmentProcessing)
{
  this->streamFragmentProcessing = streamFragmentProcessing;
}

void CM3u8DecryptionContext::SetStreamFragmentToDownload(unsigned int streamFragmentToDownload)
{
  this->streamFragmentToDownload = streamFragmentToDownload;
}

void CM3u8DecryptionContext::SetPlaylistUrl(const wchar_t *playlistUrl)
{
  this->playlistUrl = playlistUrl;
}

void CM3u8DecryptionContext::SetConfiguration(CParameterCollection *configuration)
{
  this->configuration = configuration;
}

/* other methods */

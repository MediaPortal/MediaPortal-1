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

#ifndef __M3U8_CURL_INSTANCE_DEFINED
#define __M3U8_CURL_INSTANCE_DEFINED

#include "IProtocol.h"
#include "HttpCurlInstance.h"
#include "M3u8DownloadRequest.h"
#include "M3u8DownloadResponse.h"

class CM3u8CurlInstance : public CHttpCurlInstance
{
public:
  CM3u8CurlInstance(HRESULT *result, CLogger *logger, HANDLE mutex, const wchar_t *protocolName, const wchar_t *instanceName);
  virtual ~CM3u8CurlInstance(void);

  /* get methods */

  // gets M3U8 download request
  // @return : M3U8 download request
  CM3u8DownloadRequest *GetM3u8DownloadRequest(void);

  // gets M3U8 download response
  // @return : M3U8 download response
  CM3u8DownloadResponse *GetM3u8DownloadResponse(void);

  // gets owner of M3U8 CURL instance
  // @return : reference to owner of M3U8 CURL instance (if locked), NULL otherwise
  void *GetOwner(void);

  // gets owner lock count of M3U8 CURL instance
  // @return : owner lock count of M3U8 CURL instance, zero if not locked
  unsigned int GetOwnerLockCount(void);

  // gets M3U8 CURL instance connection state
  // @return : connection state
  ProtocolConnectionState GetConnectionState(void);

  /* set methods */

  // sets M3U8 CURL instance connection state
  // @param connectionState : the connection state to set
  void SetConnectionState(ProtocolConnectionState connectionState);

  /* other methods */

  // initializes CURL instance
  // @param downloadRequest : download request
  // @return : true if successful, false otherwise
  virtual HRESULT Initialize(CDownloadRequest *downloadRequest);

  // lock M3U8 CURL instance for specific owner
  // only owner can unlock instance for other use
  // @param owner : the requested owner to lock instance
  // @return : S_OK if locked, S_FALSE if already locked by same owner, E_FAIL if locked by another owner
  HRESULT LockCurlInstance(void *owner);

  // unlocks M3U8 CURL instance for specific owner
  // only owner can unlock instance for other use
  // @param owner : the requested owner to unlock instance
  // @return : S_OK if unlocked, S_FALSE if still locked by same owner, E_FAIL if locked by another owner or instance not locked
  HRESULT UnlockCurlInstance(void *owner);

  // tests if M3U8 CURL instance is locked
  // @return : true if instance is locked, false otherwise
  bool IsLockedCurlInstance(void);

  // tests if M3U8 CURL instance is locked by specified owner
  // @param owner : the owner to test lock
  // @return : true if instance is locked by owner, false otherwise
  bool IsLockedCurlInstanceByOwner(void *owner);

protected:
  // holds M3U8 download request
  // never created and never destroyed
  // initialized in constructor by deep cloning
  CM3u8DownloadRequest *m3u8DownloadRequest;

  // holds M3U8 download response
  CM3u8DownloadResponse *m3u8DownloadResponse;

  // holds owner of M3U8 CURL instance (if locked)
  void *owner;
  // holds owner lock count (zero if not locked, more than zero if locked)
  unsigned int ownerLockCount;

  // holds connection state
  ProtocolConnectionState connectionState;

  /* methods */

  // gets new instance of download response
  // @return : new download response or NULL if error
  virtual CDownloadResponse *CreateDownloadResponse(void);

  // creates dump box for dump file
  // @return : dump box or NULL if error
  virtual CDumpBox *CreateDumpBox(void);
};

#endif
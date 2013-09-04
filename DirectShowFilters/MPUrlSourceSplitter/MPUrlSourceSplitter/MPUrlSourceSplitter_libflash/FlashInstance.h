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

#ifndef __FLASH_INSTANCE_DEFINED
#define __FLASH_INSTANCE_DEFINED

#define METHOD_CREATE_FLASH_WORKER_NAME                                       L"CreateFlashWorker()"
#define METHOD_DESTROY_FLASH_WORKER_NAME                                      L"DestroyFlashWorker()"
#define METHOD_FLASH_WORKER_NAME                                              L"FlashWorker()"

class CFlashInstance
{
public:
  CFlashInstance(CLogger *logger, const wchar_t *instanceName, const wchar_t *swfFilePath);
  virtual ~CFlashInstance(void);

  /* get methods */

  /* set methods */

  /* other methods */

  // initializes flash instance
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT Initialize(void);

  // gets result from query to flash (make query shortest as possible, same for result)
  // @param query : query in flash format
  // @return : result or NULL if error
  virtual wchar_t *GetResult(const wchar_t *query);

  // clear session to default state
  virtual void ClearSession(void);

protected:
  // logger for logging purposes
  CLogger *logger;
  // the name of flash instance
  wchar_t *instanceName;

  // holds swf file path
  wchar_t *swfFilePath;

  // worker thread 
  HANDLE hFlashWorkerThread;
  bool flashWorkerShouldExit;
  static unsigned int WINAPI FlashWorker(LPVOID lpParam);

  // creates flash worker
  // @return : S_OK if successful
  HRESULT CreateFlashWorker(void);

  // destroys flash worker
  // @return : S_OK if successful
  HRESULT DestroyFlashWorker(void);

  // initalize related properties
  bool initializeRequest;
  bool initializeRequestFinished;
  HRESULT initializeResult;

  // request for result properties
  bool resultRequested;
  bool resultRequestFinished;

  // holds query for worker (only reference to query - do not free memory)
  wchar_t *query;
  // holds query result from worker (only reference to query result - do not free memory)
  wchar_t *queryResult;
};

#endif
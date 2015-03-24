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

#include "FlashInstance.h"
#include "FlashWindow.h"

#include <process.h>

CFlashInstance::CFlashInstance(HRESULT *result, CLogger *logger, const wchar_t *instanceName, const wchar_t *swfFilePath)
{
  this->logger = NULL;
  this->instanceName = NULL;
  this->swfFilePath = NULL;

  this->flashWorkerShouldExit = false;
  this->flashWorkerThread = NULL;

  this->initializeRequest = false;
  this->initializeRequestFinished = false;
  this->initializeResult = S_OK;

  this->query = NULL;
  this->queryResult = NULL;
  this->resultRequested = false;
  this->resultRequestFinished = false;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    CHECK_POINTER_DEFAULT_HRESULT(*result, logger);
    CHECK_POINTER_DEFAULT_HRESULT(*result, instanceName);
    CHECK_POINTER_DEFAULT_HRESULT(*result, swfFilePath);

    if (SUCCEEDED(*result))
    {
      CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_CONSTRUCTOR_START_FORMAT, this->instanceName, METHOD_CONSTRUCTOR_NAME, this));

      this->logger = logger;
      this->instanceName = Duplicate(instanceName);
      this->swfFilePath = Duplicate(swfFilePath);

      CHECK_CONDITION_HRESULT(*result, this->instanceName, *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->swfFilePath, *result, E_OUTOFMEMORY);

      CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, this->instanceName, METHOD_CONSTRUCTOR_NAME));
    }
  }
}

CFlashInstance::~CFlashInstance(void)
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, this->instanceName, METHOD_DESTRUCTOR_NAME));

  this->DestroyFlashWorker();

  FREE_MEM(this->swfFilePath);
  FREE_MEM(this->instanceName);

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, this->instanceName, METHOD_DESTRUCTOR_NAME))
}

/* get methods */

const wchar_t *CFlashInstance::GetResult(const wchar_t *query)
{
  this->query = query;
  this->resultRequested = true;

  while (!this->resultRequestFinished)
  {
    Sleep(10);
  }

  this->resultRequestFinished = false;
  return this->queryResult;
}

/* set methods */

/* other methods */

HRESULT CFlashInstance::Initialize(void)
{
  HRESULT result = S_OK;
  if (this->flashWorkerThread == NULL)
  {
    result = this->CreateFlashWorker();
  }
  
  if (SUCCEEDED(result))
  {
    // create initialize request
    this->initializeRequest = true;
    while (!this->initializeRequestFinished)
    {
      Sleep(10);
    }
    result = this->initializeResult;
    this->initializeRequestFinished = false;
  }

  return result;
}

void CFlashInstance::ClearSession(void)
{
  this->DestroyFlashWorker();

  this->flashWorkerShouldExit = false;
  this->flashWorkerThread = NULL;

  this->initializeRequest = false;
  this->initializeRequestFinished = false;
  this->initializeResult = S_OK;
  this->query = NULL;
  this->queryResult = NULL;

  this->resultRequested = false;
  this->resultRequestFinished = false;
}

/* flash methods */

HRESULT CFlashInstance::CreateFlashWorker(void)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, this->instanceName, METHOD_CREATE_FLASH_WORKER_NAME);

  this->flashWorkerThread = (HANDLE)_beginthreadex(NULL, 0, &CFlashInstance::FlashWorker, this, 0, NULL);

  if (this->flashWorkerThread == NULL)
  {
    // thread not created
    result = HRESULT_FROM_WIN32(GetLastError());
    this->logger->Log(LOGGER_ERROR, L"%s: %s: _beginthreadex() error: 0x%08X", this->instanceName, METHOD_CREATE_FLASH_WORKER_NAME, result);
  }

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, this->instanceName, METHOD_CREATE_FLASH_WORKER_NAME, result);
  return result;
}

HRESULT CFlashInstance::DestroyFlashWorker(void)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, this->instanceName, METHOD_DESTROY_FLASH_WORKER_NAME);

  this->flashWorkerShouldExit = true;

  // wait for the flash worker thread to exit      
  if (this->flashWorkerThread != NULL)
  {
    if (WaitForSingleObject(this->flashWorkerThread, INFINITE) == WAIT_TIMEOUT)
    {
      // thread didn't exit, kill it now
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, this->instanceName, METHOD_DESTROY_FLASH_WORKER_NAME, L"thread didn't exit, terminating thread");
      TerminateThread(this->flashWorkerThread, 0);
    }
    CloseHandle(this->flashWorkerThread);
  }

  this->flashWorkerThread = NULL;
  this->flashWorkerShouldExit = false;

  this->logger->Log(LOGGER_INFO, (SUCCEEDED(result)) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, this->instanceName, METHOD_DESTROY_FLASH_WORKER_NAME, result);
  return result;
}

unsigned int WINAPI CFlashInstance::FlashWorker(LPVOID lpParam)
{
  CFlashInstance *caller = (CFlashInstance *)lpParam;
  caller->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, caller->instanceName, METHOD_FLASH_WORKER_NAME);

  // process messages in loop
  MSG msg;
  CFlashWindow *flashWindow = NULL;
  wchar_t *className = NULL;
  bool canExit = false;

  OleInitialize(NULL);

  while (!canExit)
  {
    if (PeekMessage(&msg, NULL, 0, 0, PM_NOREMOVE))
    {
      if (GetMessage(&msg, NULL, 0, 0))
      {
        TranslateMessage(&msg);
        DispatchMessage(&msg);
      }
    }

    if ((caller->flashWorkerShouldExit) || (caller->initializeRequest))
    {
      if (flashWindow != NULL)
      {
        HWND flashWindowWnd = flashWindow->GetHWND();

        // clean up flash window
        FREE_MEM_CLASS(flashWindow);

        // destroy window
        DestroyWindow(flashWindowWnd);
      }

      if (className != NULL)
      {
        // unregister flash window class
        UnregisterClass(className, NULL);
        FREE_MEM(className);
        canExit = caller->flashWorkerShouldExit;
      }
    }

    // process internal request for initialize
    if ((!canExit) && (caller->initializeRequest))
    {
      // register flash window class
      GUID classNameGuid = GUID_NULL;
      caller->initializeResult = CoCreateGuid(&classNameGuid);
      if (SUCCEEDED(caller->initializeResult) && (classNameGuid != GUID_NULL))
      {
        className = ConvertGuidToString(classNameGuid);
        CHECK_POINTER_HRESULT(caller->initializeResult, className, caller->initializeResult, E_OUTOFMEMORY);

        if (SUCCEEDED(caller->initializeResult))
        {
          WNDCLASSEX wcs = {0};
          wcs.cbSize = sizeof(WNDCLASSEX);
          wcs.lpfnWndProc = CFlashWindow::WndProcStatic;
          wcs.hInstance = NULL;
          wcs.lpszClassName = className;
          ATOM classAtom = RegisterClassEx(&wcs);
          caller->initializeResult = (classAtom != 0) ? S_OK : HRESULT_FROM_WIN32(GetLastError());

          if (FAILED(caller->initializeResult))
          {
            FREE_MEM(className);
          }
        }
      }

      if (SUCCEEDED(caller->initializeResult) && (classNameGuid != GUID_NULL))
      {
        flashWindow = new CFlashWindow(&caller->initializeResult, caller->swfFilePath);
        CHECK_CONDITION_HRESULT(caller->initializeResult, flashWindow, caller->initializeResult, E_OUTOFMEMORY);

        if (SUCCEEDED(caller->initializeResult))
        {
          caller->initializeResult = flashWindow->Create(ShockwaveFlashObjects::CLSID_ShockwaveFlash,
            WS_EX_LAYERED | WS_EX_NOACTIVATE, WS_POPUP | WS_CLIPSIBLINGS,
            NULL, NULL, className);
        }
      }
      caller->initializeRequest = false;
      caller->initializeRequestFinished = true;
    }

    // process internal requests for results
    if ((!canExit) && (caller->resultRequested))
    {
      caller->queryResult = SUCCEEDED(caller->initializeResult) ? flashWindow->GetResult(caller->query) : NULL;
      caller->resultRequested = false;
      caller->resultRequestFinished = true;
    }

    Sleep(1);
  }

  OleUninitialize();

  caller->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, caller->instanceName, METHOD_FLASH_WORKER_NAME);

  // _endthreadex should be called automatically, but for sure
  _endthreadex(0);

  return S_OK;
}
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

#include "MPUrlSourceSplitterInputDownloadPin.h"
#include "MPUrlSourceSplitterOutputDownloadPin.h"

#define METHOD_PIN_MESSAGE_FORMAT                                     L"%s: %s: pin '%s', %s"
#define METHOD_PIN_START_FORMAT                                       L"%s: %s: pin '%s', Start"
#define METHOD_PIN_END_FORMAT                                         L"%s: %s: pin '%s', End"
#define METHOD_PIN_END_FAIL_RESULT_FORMAT                             L"%s: %s: pin '%s', End, Fail, result: 0x%08X"

#define BUFFER_SIZE                                                   (10 * 1024 * 1024)    // 10 MB buffer

#ifdef _DEBUG
#define MODULE_NAME                                                   L"MPUrlSourceSplitterInputDownloadPind"
#else
#define MODULE_NAME                                                   L"MPUrlSourceSplitterInputDownloadPin"
#endif

#define PIN_NAME                                                      (this->name != NULL) ? this->name : L"MediaPortal Url Source Splitter Input Download Pin"

CMPUrlSourceSplitterInputDownloadPin::CMPUrlSourceSplitterInputDownloadPin(CLogger *logger, HRESULT *phr, const wchar_t *name, const wchar_t *downloadFileName, CMPUrlSourceSplitterOutputDownloadPin *outputPin)
  : CUnknown(NAME("MediaPortal Url Source Splitter Input Download Pin"), NULL), CFlags()
{
  this->downloadFileName = NULL;
  this->name = NULL;
  this->buffer = NULL;
  this->bufferPosition = 0;
  
  if (phr != NULL)
  {
    if (SUCCEEDED(*phr))
    {
      this->logger = logger;

      CHECK_POINTER_HRESULT(*phr, logger, *phr, E_INVALIDARG);
      CHECK_POINTER_HRESULT(*phr, name, *phr, E_INVALIDARG);
      CHECK_POINTER_HRESULT(*phr, downloadFileName, *phr, E_INVALIDARG);
      CHECK_POINTER_HRESULT(*phr, outputPin, *phr, E_INVALIDARG);

      this->name = Duplicate(name);
      this->downloadFileName = Duplicate(downloadFileName);
      this->outputPin = outputPin;
      this->buffer = ALLOC_MEM_SET(this->buffer, unsigned char, BUFFER_SIZE, 0);

      CHECK_POINTER_HRESULT(*phr, this->name, *phr, E_OUTOFMEMORY);
      CHECK_POINTER_HRESULT(*phr, this->downloadFileName, *phr, E_OUTOFMEMORY);
      CHECK_POINTER_HRESULT(*phr, this->buffer, *phr, E_OUTOFMEMORY);
    }
  }

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_PIN_END_FORMAT, MODULE_NAME, METHOD_CONSTRUCTOR_NAME, PIN_NAME));
}

CMPUrlSourceSplitterInputDownloadPin::~CMPUrlSourceSplitterInputDownloadPin(void)
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_PIN_START_FORMAT, MODULE_NAME, METHOD_DESTRUCTOR_NAME, PIN_NAME));

  FREE_MEM(this->downloadFileName);
  FREE_MEM(this->buffer);

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_PIN_END_FORMAT, MODULE_NAME, METHOD_DESTRUCTOR_NAME, PIN_NAME));
}

// IUnknown interface implementation

STDMETHODIMP CMPUrlSourceSplitterInputDownloadPin::NonDelegatingQueryInterface(REFIID riid, void** ppv)
{
  CheckPointer(ppv, E_POINTER);

  *ppv = NULL;

  return
    QI(IPin)
    QI(IMemInputPin)
    __super::NonDelegatingQueryInterface(riid, ppv);
}

// IPin interface implementation

HRESULT STDMETHODCALLTYPE CMPUrlSourceSplitterInputDownloadPin::Connect(IPin *pReceivePin, const AM_MEDIA_TYPE *pmt)
{
  return E_NOTIMPL;
}

HRESULT STDMETHODCALLTYPE CMPUrlSourceSplitterInputDownloadPin::ReceiveConnection(IPin *pConnector, const AM_MEDIA_TYPE *pmt)
{
  return S_OK;
}

HRESULT STDMETHODCALLTYPE CMPUrlSourceSplitterInputDownloadPin::Disconnect(void)
{
  return S_OK;
}

HRESULT STDMETHODCALLTYPE CMPUrlSourceSplitterInputDownloadPin::ConnectedTo(IPin **pPin)
{
  return E_NOTIMPL;
}

HRESULT STDMETHODCALLTYPE CMPUrlSourceSplitterInputDownloadPin::ConnectionMediaType(AM_MEDIA_TYPE *pmt)
{
  return E_NOTIMPL;
}

HRESULT STDMETHODCALLTYPE CMPUrlSourceSplitterInputDownloadPin::QueryPinInfo(PIN_INFO *pInfo)
{
  return E_NOTIMPL;
}

HRESULT STDMETHODCALLTYPE CMPUrlSourceSplitterInputDownloadPin::QueryDirection(PIN_DIRECTION *pPinDir)
{
  HRESULT result = (pPinDir != NULL) ? S_OK : E_POINTER;

  CHECK_CONDITION_EXECUTE(SUCCEEDED(result), *pPinDir = PINDIR_INPUT);

  return result;
}

HRESULT STDMETHODCALLTYPE CMPUrlSourceSplitterInputDownloadPin::QueryId(LPWSTR *Id)
{
  return E_NOTIMPL;
}

HRESULT STDMETHODCALLTYPE CMPUrlSourceSplitterInputDownloadPin::QueryAccept(const AM_MEDIA_TYPE *pmt)
{
  return E_NOTIMPL;
}

HRESULT STDMETHODCALLTYPE CMPUrlSourceSplitterInputDownloadPin::EnumMediaTypes(IEnumMediaTypes **ppEnum)
{
  return E_NOTIMPL;
}

HRESULT STDMETHODCALLTYPE CMPUrlSourceSplitterInputDownloadPin::QueryInternalConnections(IPin **apPin, ULONG *nPin)
{
  return E_NOTIMPL;
}

HRESULT STDMETHODCALLTYPE CMPUrlSourceSplitterInputDownloadPin::EndOfStream(void)
{
  HRESULT result = S_OK;

  if (this->bufferPosition > 0)
  {
    result = this->DumpDataToFile(this->buffer, this->bufferPosition);

    this->bufferPosition = 0;
  }

  this->outputPin->FinishDownload(result);

  return S_OK;
}

HRESULT STDMETHODCALLTYPE CMPUrlSourceSplitterInputDownloadPin::BeginFlush(void)
{
  return S_OK;
}

HRESULT STDMETHODCALLTYPE CMPUrlSourceSplitterInputDownloadPin::EndFlush(void)
{
  return S_OK;
}

HRESULT STDMETHODCALLTYPE CMPUrlSourceSplitterInputDownloadPin::NewSegment(REFERENCE_TIME tStart, REFERENCE_TIME tStop, double dRate)
{
  return E_NOTIMPL;
}

// IMemInputPin interface implementation

HRESULT STDMETHODCALLTYPE CMPUrlSourceSplitterInputDownloadPin::GetAllocator(IMemAllocator **ppAllocator)
{
  return VFW_E_NO_ALLOCATOR;
}

HRESULT STDMETHODCALLTYPE CMPUrlSourceSplitterInputDownloadPin::NotifyAllocator(IMemAllocator *pAllocator, BOOL bReadOnly)
{
  return S_OK;
}

HRESULT STDMETHODCALLTYPE CMPUrlSourceSplitterInputDownloadPin::GetAllocatorRequirements(ALLOCATOR_PROPERTIES *pProps)
{
  return E_NOTIMPL;
}

HRESULT STDMETHODCALLTYPE CMPUrlSourceSplitterInputDownloadPin::Receive(IMediaSample *pSample)
{
  HRESULT result = S_OK;

  if (pSample != NULL)
  {
    unsigned int dataLength = (unsigned int)max(0, pSample->GetActualDataLength());

    if (dataLength > 0)
    {
      BYTE *data = NULL;
      pSample->GetPointer(&data);

      if (data != NULL)
      {
        if ((bufferPosition + dataLength) >= BUFFER_SIZE)
        {
          result = this->DumpDataToFile(buffer, bufferPosition);

          CHECK_CONDITION_EXECUTE(SUCCEEDED(result), bufferPosition = 0);
          CHECK_CONDITION_EXECUTE(FAILED(result), this->outputPin->FinishDownload(result));
        }

        if (SUCCEEDED(result))
        {
          if ((bufferPosition + dataLength) >= BUFFER_SIZE)
          {
            // too big to store it to internal buffer, store it directly to file
            result = this->DumpDataToFile(data, dataLength);
          }
          else
          {
            memcpy(buffer + bufferPosition, data, dataLength);
            bufferPosition += dataLength;
          }
        }
      }
    }
  }

  return result;
}

HRESULT STDMETHODCALLTYPE CMPUrlSourceSplitterInputDownloadPin::ReceiveMultiple(IMediaSample **pSamples, long nSamples, long *nSamplesProcessed)
{
  return S_OK;
}

HRESULT STDMETHODCALLTYPE CMPUrlSourceSplitterInputDownloadPin::ReceiveCanBlock(void)
{
  return S_FALSE;
}

/* get methods */

/* set methods */

/* other methods */

/* protected methods */

HRESULT CMPUrlSourceSplitterInputDownloadPin::DumpDataToFile(unsigned char *buffer, unsigned int length)
{
  HRESULT result = ((buffer != NULL) && (length != 0)) ? S_OK : E_INVALIDARG;

  if (SUCCEEDED(result))
  {
    HANDLE file = CreateFile(this->downloadFileName, FILE_APPEND_DATA, FILE_SHARE_READ, NULL, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
    CHECK_CONDITION_HRESULT(result, file != INVALID_HANDLE_VALUE, result, HRESULT_FROM_WIN32(GetLastError()));

    if (SUCCEEDED(result))
    {
      DWORD written = 0;

      CHECK_CONDITION_HRESULT(result, WriteFile(file, buffer, length, &written, NULL) != 0, result, HRESULT_FROM_WIN32(GetLastError()));
      CHECK_CONDITION_HRESULT(result, bufferPosition == written, result, E_OUTOFMEMORY);
    }

    if (file != INVALID_HANDLE_VALUE)
    {
      CloseHandle(file);
      file = NULL;
    }
  }

  return result;
}
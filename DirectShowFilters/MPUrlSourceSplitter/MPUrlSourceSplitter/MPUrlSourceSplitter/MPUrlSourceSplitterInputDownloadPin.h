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

#ifndef __MP_URL_SOURCE_SPLITTER_INPUT_DOWNLOAD_PIN_DEFINED
#define __MP_URL_SOURCE_SPLITTER_INPUT_DOWNLOAD_PIN_DEFINED

#include "Logger.h"
#include "Flags.h"

#include <streams.h>

#define MP_URL_SOURCE_SPLITTER_INPUT_DOWNLOAD_PIN_FLAG_NONE           FLAGS_NONE

#define MP_URL_SOURCE_SPLITTER_INPUT_DOWNLOAD_PIN_FLAG_LAST           (FLAGS_LAST + 0)

class CMPUrlSourceSplitterOutputDownloadPin;

class CMPUrlSourceSplitterInputDownloadPin : public CUnknown, public CFlags, public IPin, public IMemInputPin
{
public:
  CMPUrlSourceSplitterInputDownloadPin(CLogger *logger, HRESULT *phr, const wchar_t *name, const wchar_t *downloadFileName, CMPUrlSourceSplitterOutputDownloadPin *outputPin);
  virtual ~CMPUrlSourceSplitterInputDownloadPin(void);

  DECLARE_IUNKNOWN;
  STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void** ppv);

  // IPin interface

  virtual HRESULT STDMETHODCALLTYPE Connect( 
    /* [in] */ IPin *pReceivePin,
    /* [annotation][in] */ 
    __in_opt  const AM_MEDIA_TYPE *pmt);

  virtual HRESULT STDMETHODCALLTYPE ReceiveConnection( 
    /* [in] */ IPin *pConnector,
    /* [in] */ const AM_MEDIA_TYPE *pmt);

  virtual HRESULT STDMETHODCALLTYPE Disconnect(void);

  virtual HRESULT STDMETHODCALLTYPE ConnectedTo( 
    /* [annotation][out] */ 
    __out  IPin **pPin);

  virtual HRESULT STDMETHODCALLTYPE ConnectionMediaType( 
    /* [annotation][out] */ 
    __out  AM_MEDIA_TYPE *pmt);

  virtual HRESULT STDMETHODCALLTYPE QueryPinInfo( 
    /* [annotation][out] */ 
    __out  PIN_INFO *pInfo);

  virtual HRESULT STDMETHODCALLTYPE QueryDirection( 
    /* [annotation][out] */ 
    __out  PIN_DIRECTION *pPinDir);

  virtual HRESULT STDMETHODCALLTYPE QueryId( 
    /* [annotation][out] */ 
    __out  LPWSTR *Id);

  virtual HRESULT STDMETHODCALLTYPE QueryAccept( 
    /* [in] */ const AM_MEDIA_TYPE *pmt);

  virtual HRESULT STDMETHODCALLTYPE EnumMediaTypes( 
    /* [annotation][out] */ 
    __out  IEnumMediaTypes **ppEnum);

  virtual HRESULT STDMETHODCALLTYPE QueryInternalConnections( 
    /* [annotation][out] */ 
    __out_ecount_part_opt(*nPin, *nPin)  IPin **apPin,
    /* [out][in] */ ULONG *nPin);

  virtual HRESULT STDMETHODCALLTYPE EndOfStream(void);

  virtual HRESULT STDMETHODCALLTYPE BeginFlush(void);

  virtual HRESULT STDMETHODCALLTYPE EndFlush(void);

  virtual HRESULT STDMETHODCALLTYPE NewSegment( 
    /* [in] */ REFERENCE_TIME tStart,
    /* [in] */ REFERENCE_TIME tStop,
    /* [in] */ double dRate);

  // IMemInputPin interface

  virtual HRESULT STDMETHODCALLTYPE GetAllocator( 
    /* [annotation][out] */ 
    __out  IMemAllocator **ppAllocator);

  virtual HRESULT STDMETHODCALLTYPE NotifyAllocator( 
    /* [in] */ IMemAllocator *pAllocator,
    /* [in] */ BOOL bReadOnly);

  virtual HRESULT STDMETHODCALLTYPE GetAllocatorRequirements( 
    /* [annotation][out] */ 
    __out  ALLOCATOR_PROPERTIES *pProps);

  virtual HRESULT STDMETHODCALLTYPE Receive( 
    /* [in] */ IMediaSample *pSample);

  virtual HRESULT STDMETHODCALLTYPE ReceiveMultiple( 
    /* [annotation][size_is][in] */ 
    __in_ecount(nSamples)  IMediaSample **pSamples,
    /* [in] */ long nSamples,
    /* [annotation][out] */ 
    __out  long *nSamplesProcessed);

  virtual HRESULT STDMETHODCALLTYPE ReceiveCanBlock(void);

protected:
  // holds logger instance
  CLogger *logger;

  // holds name
  wchar_t *name;

  // holds download file name
  wchar_t *downloadFileName;

  CMPUrlSourceSplitterOutputDownloadPin *outputPin;

  // holds data buffer
  unsigned char *buffer;
  // holds data buffer position
  unsigned int bufferPosition;

  /* methods */

  // dumps data to file
  // @param buffer : the buffer with data
  // @param length : the length of data to store to file
  // @return : S_OK if successful, error code otherwise
  HRESULT DumpDataToFile(unsigned char *buffer, unsigned int length);
};

#endif
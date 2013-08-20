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

#ifndef __IDOWNLOAD_DEFINED
#define __IDOWNLOAD_DEFINED

#include <streams.h>

// {B7FDAB2F-9870-4DFC-8CC7-8BBC68B1A3BF}
DEFINE_GUID(IID_IDownload, 0xB7FDAB2F, 0x9870, 0x4DFC, 0x8C, 0xC7, 0x8B, 0xBC, 0x68, 0xB1, 0xA3, 0xBF);

// {51D2A240-A172-4FA8-AFD7-CC576EC5CA66}
DEFINE_GUID(IID_IDownloadCallback, 0x51D2A240, 0xA172, 0x4FA8, 0xAF, 0xD7, 0xCC, 0x57, 0x6E, 0xC5, 0xCA, 0x66);

MIDL_INTERFACE("51D2A240-A172-4FA8-AFD7-CC576EC5CA66")
IDownloadCallback : public IUnknown
{
public:
  // this method is called when download finished
  // @param downloadResult : result of download process
  virtual void STDMETHODCALLTYPE OnDownloadCallback(HRESULT downloadResult) = 0;
};

// provides interface for downloading files
MIDL_INTERFACE("B7FDAB2F-9870-4DFC-8CC7-8BBC68B1A3BF")
IDownload : public IAMOpenProgress
{
public:
  // this starts downloading file asynchronously and saves output
  // @param uri : uniform resource identifier of source file
  // @param fileName : the full path containing file name to save received file
  // @param downloadCallback : the callback method called after downloading finished
  // @return : S_OK if successful, E_POINTER if uri, fileName or downloadCallback is NULL
  virtual STDMETHODIMP DownloadAsync(LPCOLESTR uri, LPCOLESTR fileName, IDownloadCallback *downloadCallback) = 0;

  // this starts downloading file synchronously and saves output
  // @param uri : uniform resource identifier of source file
  // @param fileName : the full path containing file name to save received file
  // @return : S_OK if successful, E_POINTER if uri or fileName is NULL
  virtual STDMETHODIMP Download(LPCOLESTR uri, LPCOLESTR fileName) = 0;
};

#endif

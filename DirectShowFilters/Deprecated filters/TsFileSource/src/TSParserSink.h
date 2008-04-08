/**
*  TSParserSink.h
*  Copyright (C) 2005      nate
*  Copyright (C) 2006      bear
*
*  This file is part of TSFileSource, a directshow push source filter that
*  provides an MPEG transport stream output.
*
*  TSFileSource is free software; you can redistribute it and/or modify
*  it under the terms of the GNU General Public License as published by
*  the Free Software Foundation; either version 2 of the License, or
*  (at your option) any later version.
*
*  TSFileSource is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*  GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License
*  along with TSFileSource; if not, write to the Free Software
*  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*
*  authors can be reached on the forums at
*    http://forums.dvbowners.com/
*/

#ifndef TSParserSink_H
#define TSParserSink_H

#include <objbase.h>
#include "ITSParserSink.h"

class CTSParserSink;
#include "TSParserSinkGuids.h"
#include "TSParserSinkFilter.h"
#include "TSParserSinkPin.h"

#include "MultiMemWriter.h"

#include "RegSinkStore.h"
#include "SettingsSinkStore.h"

/**********************************************
 *
 *  CTSParserSinkFilter Class
 *
 **********************************************/

class CTSParserSink : public CUnknown,
					public ITSParserSink,
					public IFileSinkFilter,
					public ISpecifyPropertyPages
{
	friend class CTSParserSinkFilter;
	friend class CTSParserSinkPin;
public:
	DECLARE_IUNKNOWN
	static CUnknown * WINAPI CreateInstance(LPUNKNOWN punk, HRESULT *phr);

private:
	//private contructor because CreateInstance creates this object
	CTSParserSink(IUnknown *pUnk, HRESULT *phr);
	~CTSParserSink();

	// Overriden to say what interfaces we support where
	STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void ** ppv);

protected:
    HRESULT OpenFile();
    HRESULT CloseFile();
    HRESULT Write(PBYTE pbData, LONG lDataLength);

	// ITSParserSink
	STDMETHODIMP GetBufferSize(long *size);

    // IFileSinkFilter
    STDMETHODIMP SetFileName(LPCWSTR pszFileName,const AM_MEDIA_TYPE *pmt);
    STDMETHODIMP GetCurFile(LPWSTR * ppszFileName,AM_MEDIA_TYPE *pmt);

	// ISpecifyPropertyPages
	STDMETHODIMP GetPages(CAUUID *pPages);

	//ITSParserSink
	STDMETHODIMP GetRegSettings();
	STDMETHODIMP SetRegSettings();
	STDMETHODIMP SetRegStore(LPTSTR nameReg);
	STDMETHODIMP GetRegStore(LPTSTR nameReg);
	STDMETHODIMP GetRegFileName(LPTSTR fileName);
	STDMETHODIMP SetRegFileName(LPTSTR fileName);
	STDMETHODIMP GetBufferFileName(LPWSTR fileName);
	STDMETHODIMP SetBufferFileName(LPWSTR fileName);
	STDMETHODIMP GetCurrentTSFile(MemWriter** fileWriter);
	STDMETHODIMP GetNumbFilesAdded(WORD *numbAdd);
	STDMETHODIMP GetNumbFilesRemoved(WORD *numbRem);
	STDMETHODIMP GetCurrentFileId(WORD *fileID);
	STDMETHODIMP GetMinTSFiles(WORD *minFiles);
	STDMETHODIMP SetMinTSFiles(WORD minFiles);
	STDMETHODIMP GetMaxTSFiles(WORD *maxFiles);
	STDMETHODIMP SetMaxTSFiles(WORD maxFiles);
	STDMETHODIMP GetMaxTSFileSize(__int64 *maxSize);
	STDMETHODIMP SetMaxTSFileSize(__int64 maxSize);
	STDMETHODIMP GetChunkReserve(__int64 *chunkSize);
	STDMETHODIMP SetChunkReserve(__int64 chunkSize);
	STDMETHODIMP GetFileBufferSize(__int64 *lpllsize);
	STDMETHODIMP GetNumbErrorPackets(__int64 *lpllerrors);
	STDMETHODIMP SetNumbErrorPackets(__int64 lpllerrors);



protected:

    CTSParserSinkFilter *m_pFilter;
    CTSParserSinkPin *m_pPin;

    CCritSec m_Lock;
    CCritSec m_ReceiveLock;

    CPosPassThru *m_pPosition;

    LPOLESTR m_pFileName;
	LPTSTR m_pRegFileName;

	MultiMemWriter *m_pFileWriter;

	CRegSinkStore *m_pRegStore;
	CSettingsSinkStore *m_pSettingsStore;
	SharedMemory *m_pSharedMemory;
};

#endif

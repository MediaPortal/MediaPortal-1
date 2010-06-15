/**
*  TSFileSink.cpp
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

#include "stdafx.h"
#include "TSFileSink.h"

CUnknown * WINAPI CTSFileSink::CreateInstance(LPUNKNOWN punk, HRESULT *phr)
{
	ASSERT(phr);
	CTSFileSink *pNewObject = new CTSFileSink(punk, phr);

	if (pNewObject == NULL) {
		if (phr)
			*phr = E_OUTOFMEMORY;
	}

	return pNewObject;
}

//////////////////////////////////////////////////////////////////////
// CTSFileSink
//////////////////////////////////////////////////////////////////////
CTSFileSink::CTSFileSink(IUnknown *pUnk, HRESULT *phr) :
	CUnknown(NAME("CTSFileSink"), NULL),
	m_pFilter(NULL),
	m_pPin(NULL),
	m_pPosition(NULL),
	m_pRegStore(NULL),
	m_pFileName(0),
	m_pRegFileName(NULL),
	m_pFileWriter(NULL)
{
	ASSERT(phr);

	m_pFilter = new CTSFileSinkFilter(this, GetOwner(), &m_Lock, phr);
    if (m_pFilter == NULL)
	{
        if (phr)
            *phr = E_OUTOFMEMORY;
        return;
    }

	m_pPin = new CTSFileSinkPin(this, GetOwner(), m_pFilter, &m_Lock, phr);
	if (m_pPin == NULL) {
		if (phr)
			*phr = E_OUTOFMEMORY;
		return;
	}

	SinkStoreParam params;
	params.fileName = TEXT("MyBufferFile");
	params.minFiles = 6;
	params.maxFiles = 60;
	params.maxSize = (__int64)((__int64)1048576 *(__int64)250); //250MB
	params.chunkSize = (__int64)((__int64)1048576 *(__int64)250); //250MB;

	m_pSettingsStore = new CSettingsSinkStore(&params);
	m_pRegStore = new CRegSinkStore("SOFTWARE\\TSFileSink");

	MultiFileWriterParam writerParams;
	writerParams.chunkSize = m_pSettingsStore->getChunkReserveReg();
	writerParams.maxFiles = m_pSettingsStore->getMaxTSFilesReg();
	writerParams.maxSize = m_pSettingsStore->getMaxTSFileSizeReg();
	writerParams.minFiles = m_pSettingsStore->getMinTSFilesReg();

	m_pFileWriter = new MultiFileWriter(&writerParams);

	// Load Registry Settings data
	GetRegStore("default");

	m_pRegFileName = new char[MAX_PATH];
	if (m_pRegFileName != 0)
		sprintf(m_pRegFileName, "MyBufferFile");
	

	if(m_pRegFileName && strlen(m_pRegFileName) > 0 && strlen(m_pRegFileName) <= MAX_PATH)
	{
		if (m_pFileName)
			delete[] m_pFileName;

		long length = lstrlen(m_pRegFileName);

		// Check that the filename ends with .tsbuffer. If it doesn't we'll add it
		if ((length < 9) || (stricmp(m_pRegFileName+length-9, ".tsbuffer") != 0))
		{
			m_pFileName = new wchar_t[1+length+9];
			if (m_pFileName != 0)
				swprintf(m_pFileName, L"%S.tsbuffer", m_pRegFileName);
		}
		else
		{
			m_pFileName = new WCHAR[1+length];
			if (m_pFileName != 0)
				swprintf(m_pFileName, L"%S", m_pRegFileName);
		}
	}
}

CTSFileSink::~CTSFileSink()
{
	CloseFile();

	if (m_pFileWriter) delete m_pFileWriter;
	if (m_pPin) delete m_pPin;
	if (m_pFilter) delete m_pFilter;
	if (m_pPosition) delete m_pPosition;
	if (m_pFileName) delete[] m_pFileName;
	if (m_pRegStore) delete m_pRegStore;
	if (m_pSettingsStore) delete m_pSettingsStore;
	if (m_pRegFileName) delete[] m_pRegFileName;
}

STDMETHODIMP CTSFileSink::NonDelegatingQueryInterface(REFIID riid, void ** ppv)
{
    CheckPointer(ppv,E_POINTER);
    CAutoLock lock(&m_Lock);

	if (riid == IID_ITSFileSink)
	{
		return GetInterface((ITSFileSink*)this, ppv);
	}
    if (riid == IID_IFileSinkFilter)
	{
        return GetInterface((IFileSinkFilter *) this, ppv);
    } 
	else if (riid == IID_ISpecifyPropertyPages)
	{
		return GetInterface((ISpecifyPropertyPages*)this, ppv);
	}
    else if (riid == IID_IBaseFilter || riid == IID_IMediaFilter || riid == IID_IPersist)
	{
        return m_pFilter->NonDelegatingQueryInterface(riid, ppv);
    } 
    else if (riid == IID_IMediaPosition || riid == IID_IMediaSeeking)
	{
        if (m_pPosition == NULL) 
        {
            HRESULT hr = S_OK;
            m_pPosition = new CPosPassThru(NAME("TSFileSink Pass Through"), (IUnknown *) GetOwner(), (HRESULT *) &hr, m_pPin);
            if (m_pPosition == NULL) 
                return E_OUTOFMEMORY;

            if (FAILED(hr)) 
            {
				if (m_pPosition)
					delete m_pPosition;

                m_pPosition = NULL;
                return hr;
            }
        }

        return m_pPosition->NonDelegatingQueryInterface(riid, ppv);
    } 

    return CUnknown::NonDelegatingQueryInterface(riid, ppv);
}


HRESULT CTSFileSink::OpenFile()
{
    CAutoLock lock(&m_Lock);

    // Has a filename been set yet
    if (m_pFileName == NULL)
	{
        return ERROR_INVALID_NAME;
    }

	HRESULT hr = m_pFileWriter->OpenFile(m_pFileName);

    return S_OK;
}

HRESULT CTSFileSink::CloseFile()
{
    // Must lock this section to prevent problems related to
    // closing the file while still receiving data in Receive()
    CAutoLock lock(&m_Lock);

	m_pFileWriter->CloseFile();

    return NOERROR;
}

HRESULT CTSFileSink::Write(PBYTE pbData, LONG lDataLength)
{
    CAutoLock lock(&m_Lock);

	HRESULT hr = m_pFileWriter->Write(pbData, lDataLength);

    return S_OK;
}

STDMETHODIMP CTSFileSink::GetBufferSize(long *size)
{
	CheckPointer(size, E_POINTER);
	*size = 0;
	return S_OK;
}

STDMETHODIMP CTSFileSink::SetFileName(LPCWSTR pszFileName, const AM_MEDIA_TYPE *pmt)
{
    CheckPointer(pszFileName,E_POINTER);

    if(wcslen(pszFileName) == 0 && strlen(m_pRegFileName) > 0)
	{
		if(strlen(m_pRegFileName) > MAX_PATH)
			return ERROR_FILENAME_EXCED_RANGE;

		if (m_pFileName)
			delete[] m_pFileName;

		long length = lstrlen(m_pRegFileName);

		// Check that the filename ends with .tsbuffer. If it doesn't we'll add it
		if ((length < 9) || (stricmp(m_pRegFileName+length-9, ".tsbuffer") != 0))
		{
			m_pFileName = new wchar_t[1+length+9];
			if (m_pFileName == 0)
				return E_OUTOFMEMORY;

			swprintf(m_pFileName, L"%S.tsbuffer", m_pRegFileName);
		}
		else
		{
			m_pFileName = new WCHAR[1+length];
			if (m_pFileName == 0)
				return E_OUTOFMEMORY;

			swprintf(m_pFileName, L"%S", m_pRegFileName);
		}
	}
	else
	{

		if(wcslen(pszFileName) > MAX_PATH)
			return ERROR_FILENAME_EXCED_RANGE;

		if (m_pFileName)
			delete[] m_pFileName;

		long length = wcslen(pszFileName);

		// Check that the filename ends with .tsbuffer. If it doesn't we'll add it
		if ((length < 9) || (_wcsicmp(pszFileName+length-9, L".tsbuffer") != 0))
		{
			m_pFileName = new wchar_t[1+length+9];
			if (m_pFileName == 0)
				return E_OUTOFMEMORY;

			swprintf(m_pFileName, L"%s.tsbuffer", pszFileName);
		}
		else
		{
			m_pFileName = new WCHAR[1+length];
			if (m_pFileName == 0)
				return E_OUTOFMEMORY;

			wcscpy(m_pFileName, pszFileName);
		}
	}
	HRESULT hr = S_OK;

    return hr;
}

STDMETHODIMP CTSFileSink::GetCurFile(LPOLESTR * ppszFileName, AM_MEDIA_TYPE *pmt)
{
    CheckPointer(ppszFileName, E_POINTER);
    *ppszFileName = NULL;

    if (m_pFileName != NULL) 
    {
		//QzTask = CoTask
        *ppszFileName = (LPOLESTR)CoTaskMemAlloc(sizeof(WCHAR) * (1+wcslen(m_pFileName)));

        if (*ppszFileName != NULL)
        {
            wcscpy(*ppszFileName, m_pFileName);
        }
    }

    if(pmt)
    {
        ZeroMemory(pmt, sizeof(*pmt));
        pmt->majortype = MEDIATYPE_NULL;
        pmt->subtype = MEDIASUBTYPE_NULL;
    }

    return S_OK;

}

STDMETHODIMP CTSFileSink::GetPages(CAUUID *pPages)
{
	if (pPages == NULL) return E_POINTER;
	pPages->cElems = 1;
	pPages->pElems = (GUID*)CoTaskMemAlloc(sizeof(GUID));
	if (pPages->pElems == NULL)
	{
		return E_OUTOFMEMORY;
	}
	pPages->pElems[0] = CLSID_TSFileSinkProp;
	return S_OK;
}


STDMETHODIMP CTSFileSink::SetRegStore(LPTSTR nameReg)
{
	char name[128] = "";
	sprintf(name, "%s", nameReg);

	char filename[MAX_PATH] = "";
//	sprintf(filename, "%s", m_pFileWriter->getRegFileName());
	sprintf(filename, "%s", m_pRegFileName);
	m_pSettingsStore->setRegFileNameReg(filename);
	m_pSettingsStore->setMinTSFilesReg(m_pFileWriter->getMinTSFiles());
	m_pSettingsStore->setMaxTSFilesReg(m_pFileWriter->getMaxTSFiles());
	m_pSettingsStore->setMaxTSFileSizeReg(m_pFileWriter->getMaxTSFileSize());
	m_pSettingsStore->setChunkReserveReg(m_pFileWriter->getChunkReserve());

	m_pRegStore->setSettingsInfo(m_pSettingsStore);
    return NOERROR;
}

STDMETHODIMP CTSFileSink::GetRegStore(LPTSTR nameReg)
{
    CheckPointer(nameReg, E_POINTER);
	char name[128] = "";
	sprintf(name, "%s", nameReg);

	std::string saveName = m_pSettingsStore->getName();

	// Load Registry Settings data
	m_pSettingsStore->setName(nameReg);

	if(m_pRegStore->getSettingsInfo(m_pSettingsStore))
	{
//		m_pFileWriter->setRegFileName((char*)m_pSettingsStore->getRegFileNameReg().c_str());
		SetRegFileName((char*)m_pSettingsStore->getRegFileNameReg().c_str());
		m_pFileWriter->setMinTSFiles(m_pSettingsStore->getMinTSFilesReg());
		m_pFileWriter->setMaxTSFiles(m_pSettingsStore->getMaxTSFilesReg());
		m_pFileWriter->setMaxTSFileSize(m_pSettingsStore->getMaxTSFileSizeReg());
		m_pFileWriter->setChunkReserve(m_pSettingsStore->getChunkReserveReg());
	}

    return NOERROR;
}


STDMETHODIMP CTSFileSink::SetRegSettings()
{
	CAutoLock lock(&m_Lock);
	SetRegStore("user");
    return NOERROR;
}

STDMETHODIMP CTSFileSink::GetRegSettings()
{
	CAutoLock lock(&m_Lock);
	GetRegStore("user");
    return NOERROR;
}

STDMETHODIMP CTSFileSink::GetRegFileName(LPTSTR fileName)
{
    CheckPointer(fileName, E_POINTER);
	CAutoLock lock(&m_Lock);

	sprintf((char *)fileName, "%s", m_pRegFileName);
//	sprintf((char *)fileName, "%s", m_pFileWriter->getRegFileName());
    return NOERROR;
}

STDMETHODIMP CTSFileSink::SetRegFileName(LPTSTR fileName)
{
	CAutoLock lock(&m_Lock);
//	m_pFileWriter->setRegFileName(fileName);

	if(strlen(fileName) > MAX_PATH)
		return ERROR_FILENAME_EXCED_RANGE;

	// Take a copy of the filename
	if (m_pRegFileName)
	{
		delete[] m_pRegFileName;
		m_pRegFileName = NULL;
	}
	m_pRegFileName = new CHAR[1+strlen(fileName)];
	if (m_pRegFileName == NULL)
		return E_OUTOFMEMORY;

	strcpy(m_pRegFileName, fileName);

    return NOERROR;
}

STDMETHODIMP CTSFileSink::GetBufferFileName(LPWSTR fileName)
{
    CheckPointer(fileName, E_POINTER);
	CAutoLock lock(&m_Lock);

	// Take a copy of the filename
	if (m_pFileName)
	{
		swprintf(fileName, L"%S", m_pFileName);
		return NOERROR;
//		sprintf((char *)fileName, "%S", m_pFileName);
//		return NOERROR;
	}
/*
	fileName = new WCHAR[1+wcslen(m_pFileName)];
	if (fileName == NULL)
		return E_OUTOFMEMORY;
	
	wcscpy(fileName, m_pFileName);
//	sprintf((char *)fileName, "%S", m_pFileName);
//	sprintf((char *)fileName, "%S", m_pFileWriter->getBufferFileName());
*/
   return E_FAIL;
}

STDMETHODIMP CTSFileSink::SetBufferFileName(LPWSTR fileName)
{
//	m_pFileWriter->setBufferFileName(fileName);
	CAutoLock lock(&m_Lock);

	if(wcslen(fileName) > MAX_PATH)
		return ERROR_FILENAME_EXCED_RANGE;

	// Take a copy of the filename
	if (m_pFileName)
	{
		delete[] m_pFileName;
		m_pFileName = NULL;
	}
	m_pFileName = new WCHAR[1+wcslen(fileName)];
	if (m_pFileName == NULL)
		return E_OUTOFMEMORY;

	wcscpy(m_pFileName, fileName);

    return NOERROR;
}

STDMETHODIMP CTSFileSink::GetCurrentTSFile(FileWriter** fileWriter)
{
    CheckPointer(fileWriter, E_POINTER);
	CAutoLock lock(&m_Lock);
	*fileWriter = m_pFileWriter->getCurrentTSFile();
    return NOERROR;
}

STDMETHODIMP CTSFileSink::GetNumbFilesAdded(WORD *numbAdd)
{
    CheckPointer(numbAdd, E_POINTER);
	CAutoLock lock(&m_Lock);
	*numbAdd = (WORD)m_pFileWriter->getNumbFilesAdded();
    return NOERROR;
}

STDMETHODIMP CTSFileSink::GetNumbFilesRemoved(WORD *numbRem)
{
    CheckPointer(numbRem, E_POINTER);
	CAutoLock lock(&m_Lock);
	*numbRem = (WORD)m_pFileWriter->getNumbFilesRemoved();
    return NOERROR;
}

STDMETHODIMP CTSFileSink::GetCurrentFileId(WORD *fileID)
{
    CheckPointer(fileID, E_POINTER);
	CAutoLock lock(&m_Lock);
	*fileID = (WORD)m_pFileWriter->getCurrentFileId();
    return NOERROR;
}

STDMETHODIMP CTSFileSink::GetMinTSFiles(WORD *minFiles)
{
    CheckPointer(minFiles, E_POINTER);
	CAutoLock lock(&m_Lock);
	*minFiles = (WORD) m_pFileWriter->getMinTSFiles();
    return NOERROR;
}

STDMETHODIMP CTSFileSink::SetMinTSFiles(WORD minFiles)
{
	CAutoLock lock(&m_Lock);
	m_pFileWriter->setMinTSFiles((long)minFiles);
    return NOERROR;
}

STDMETHODIMP CTSFileSink::GetMaxTSFiles(WORD *maxFiles)
{
    CheckPointer(maxFiles, E_POINTER);
	CAutoLock lock(&m_Lock);
	*maxFiles = (WORD) m_pFileWriter->getMaxTSFiles();
	return NOERROR;
}

STDMETHODIMP CTSFileSink::SetMaxTSFiles(WORD maxFiles)
{
	CAutoLock lock(&m_Lock);
	m_pFileWriter->setMaxTSFiles((long)maxFiles);
    return NOERROR;
}

STDMETHODIMP CTSFileSink::GetMaxTSFileSize(__int64 *maxSize)
{
    CheckPointer(maxSize, E_POINTER);
	CAutoLock lock(&m_Lock);
	*maxSize = m_pFileWriter->getMaxTSFileSize();
	return NOERROR;
}

STDMETHODIMP CTSFileSink::SetMaxTSFileSize(__int64 maxSize)
{
	CAutoLock lock(&m_Lock);
	m_pFileWriter->setMaxTSFileSize(maxSize);
    return NOERROR;
}

STDMETHODIMP CTSFileSink::GetChunkReserve(__int64 *chunkSize)
{
    CheckPointer(chunkSize, E_POINTER);
	CAutoLock lock(&m_Lock);
	*chunkSize = m_pFileWriter->getChunkReserve();
	return NOERROR;
}

STDMETHODIMP CTSFileSink::SetChunkReserve(__int64 chunkSize)
{
	CAutoLock lock(&m_Lock);

	m_pFileWriter->setChunkReserve(chunkSize);
    return NOERROR;
}

STDMETHODIMP CTSFileSink::GetFileBufferSize(__int64 *lpllsize)
{
    CheckPointer(lpllsize, E_POINTER);
	CAutoLock lock(&m_Lock);
	m_pFileWriter->GetFileSize(lpllsize);
	return NOERROR;
}

STDMETHODIMP CTSFileSink::GetNumbErrorPackets(__int64 *lpllErrors)
{
    CheckPointer(lpllErrors, E_POINTER);
	CAutoLock lock(&m_Lock);
	*lpllErrors = m_pPin->getNumbErrorPackets();
	return NOERROR;
}

STDMETHODIMP CTSFileSink::SetNumbErrorPackets(__int64 lpllErrors)
{
	CAutoLock lock(&m_Lock);
	m_pPin->setNumbErrorPackets(lpllErrors);
	return NOERROR;
}


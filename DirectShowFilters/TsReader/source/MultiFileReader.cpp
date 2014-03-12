/**
*  MultiFileReader.cpp
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
*  nate can be reached on the forums at
*    http://forums.dvbowners.com/
*/
#include "StdAfx.h"

#include "MultiFileReader.h"
#include <atlbase.h>
#include <mmsystem.h>

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

//Maximum time in msec to wait for the buffer file to become available - Needed for DVB radio (this sometimes takes some time)
#define MAX_BUFFER_TIMEOUT	1500

//block read sizes for SMB2 data cache workaround
#define NEXT_READ_SIZE	8192
#define NEXT_READ_ROLLOVER (NEXT_READ_SIZE*64)

#define INFO_BUFF_SIZE (131072)

extern void LogDebug(const char *fmt, ...) ;
MultiFileReader::MultiFileReader(BOOL useFileNext, BOOL useDummyWrites):
	m_TSBufferFile(),
	m_TSFile(),
	m_TSFileGetLength(),
	m_TSFileNext()
{
	m_startPosition = 0;
	m_endPosition = 0;
	m_currentPosition = 0;
	m_filesAdded = 0;
	m_filesRemoved = 0;
	m_TSFileId = -1;
	m_TSFileIdNext = -1;
  m_lastFileNextRead = timeGetTime();
  m_currPosnFileNext = 0;
  m_bUseFileNext = useFileNext;
  
  m_TSBufferFile.SetDummyWrites(useDummyWrites);
  m_TSFile.SetDummyWrites(useDummyWrites);
  m_TSFileNext.SetDummyWrites(false);
  m_TSFileGetLength.SetDummyWrites(false);

  m_pFileReadNextBuffer = new (std::nothrow) byte[NEXT_READ_SIZE];
  m_pInfoFileBuffer1 = new (std::nothrow) byte[INFO_BUFF_SIZE];
  m_pInfoFileBuffer2 = new (std::nothrow) byte[INFO_BUFF_SIZE];
  
  LogDebug("MultiFileReader::ctor, useFileNext = %d, useDummyWrites %d", m_bUseFileNext, useDummyWrites);
}

MultiFileReader::~MultiFileReader()
{
  if (m_pFileReadNextBuffer)
  {
    delete [] m_pFileReadNextBuffer;
    m_pFileReadNextBuffer = NULL;
  }
  else
  {
    LogDebug("MultiFileReader::dtor - ERROR m_pFileReadBuffer is NULL !!");
  }
  
  if (m_pInfoFileBuffer1)
  {
    delete [] m_pInfoFileBuffer1;
    m_pInfoFileBuffer1 = NULL;
  }
  else
  {
    LogDebug("MultiFileReader::dtor - ERROR m_pInfoFileBuffer1 is NULL !!");
  }

  if (m_pInfoFileBuffer2)
  {
    delete [] m_pInfoFileBuffer2;
    m_pInfoFileBuffer2 = NULL;
  }
  else
  {
    LogDebug("MultiFileReader::dtor - ERROR m_pInfoFileBuffer2 is NULL !!");
  }

  LogDebug("MultiFileReader::dtor");
	//CloseFile called by ~FileReader
}


HRESULT MultiFileReader::GetFileName(LPOLESTR *lpszFileName)
{
//	CheckPointer(lpszFileName,E_POINTER);
  CAutoLock rLock (&m_accessLock);
	return m_TSBufferFile.GetFileName(lpszFileName);
}

HRESULT MultiFileReader::SetFileName(LPCOLESTR pszFileName)
{
//	CheckPointer(pszFileName,E_POINTER);
  CAutoLock rLock (&m_accessLock);
	return m_TSBufferFile.SetFileName(pszFileName);
}

//
// OpenFile
//
HRESULT MultiFileReader::OpenFile()
{
  CAutoLock rLock (&m_accessLock);
  
	HRESULT hr = m_TSBufferFile.OpenFile();

	//For radio the buffer sometimes needs some time to become available, so wait try it more than once
	DWORD tc=GetTickCount();
	while (RefreshTSBufferFile()==S_FALSE)
	{
		if (GetTickCount()-tc>MAX_BUFFER_TIMEOUT)
		{
			LogDebug("MultiFileReader: timedout while waiting for buffer file to become available");
			return S_FALSE;
		}
		Sleep(1);
	}
			
	m_currentPosition = 0;
  //LogDebug("MultiFileReader::OpenFile()");

	return hr;
}

//
// CloseFile
//
HRESULT MultiFileReader::CloseFile()
{
  CAutoLock rLock (&m_accessLock);
	HRESULT hr;
	hr = m_TSBufferFile.CloseFile();
	hr = m_TSFile.CloseFile();
	m_TSFileId = -1;
	hr = m_TSFileNext.CloseFile();
	m_TSFileIdNext = -1;
	return hr;
}

BOOL MultiFileReader::IsFileInvalid()
{
  CAutoLock rLock (&m_accessLock);
	return m_TSBufferFile.IsFileInvalid();
}

DWORD MultiFileReader::SetFilePointer(__int64 llDistanceToMove, DWORD dwMoveMethod)
{
  CAutoLock rLock (&m_accessLock);
  
	RefreshTSBufferFile();

	if (dwMoveMethod == FILE_END)
	{
		m_currentPosition = m_endPosition + llDistanceToMove;
	}
	else if (dwMoveMethod == FILE_CURRENT)
	{
		m_currentPosition += llDistanceToMove;
	}
	else // if (dwMoveMethod == FILE_BEGIN)
	{
		m_currentPosition = m_startPosition + llDistanceToMove;
	}

	if (m_currentPosition < m_startPosition)
		m_currentPosition = m_startPosition;

	if (m_currentPosition > m_endPosition) {
		LogDebug("Seeking beyond the end position: %I64d > %I64d", m_currentPosition, m_endPosition);
		m_currentPosition = m_endPosition;
	}

	//RefreshTSBufferFile();
	return S_OK;
}

__int64 MultiFileReader::GetFilePointer()
{
//	RefreshTSBufferFile();
  CAutoLock rLock (&m_accessLock);
	return m_currentPosition;
}


HRESULT MultiFileReader::Read(PBYTE pbData, ULONG lDataLength, ULONG *dwReadBytes)
{
  CAutoLock rLock (&m_accessLock);
  return ReadNoLock(pbData, lDataLength, dwReadBytes);
}

HRESULT MultiFileReader::ReadNoLock(PBYTE pbData, ULONG lDataLength, ULONG *dwReadBytes)
{
	HRESULT hr;
	HRESULT retval = S_OK;

	// If the file has already been closed, don't continue
	if (m_TSBufferFile.IsFileInvalid())
	{
		*dwReadBytes = 0;
		return E_FAIL;
	}

	if (RefreshTSBufferFile() != S_OK)
	{
		*dwReadBytes = 0;
		return E_FAIL;
	}

	if (m_currentPosition < m_startPosition)
		m_currentPosition = m_startPosition;
		
	__int64 oldCurrentPosn = m_currentPosition;

	// Find out which file the currentPosition is in (and the next file, if it exists)
	MultiFileReaderFile *file = NULL;
	MultiFileReaderFile *fileNext = NULL;
	bool fileFound = false;
	std::vector<MultiFileReaderFile *>::iterator it = m_tsFiles.begin();
	for ( ; it < m_tsFiles.end() ; it++ )
	{
	  if (fileFound)
	  {
	    fileNext = *it; //This is the next file after the file we need to read
	    break;
	  }	 
	  file = *it; 
		if (m_currentPosition < (file->startPosition + file->length))
		{
		  fileFound = true;
	  }
	};

	if(!file)
  {
    LogDebug("MultiFileReader::no file");
		*dwReadBytes = 0;
		return E_FAIL;
  }
  
  if(!fileFound)
  {
		// The current position is past the end of the last file
		*dwReadBytes = 0;
		return retval;
  }
  
	if (m_TSFileId != file->filePositionId)
	{
  	if (!m_TSFile.IsFileInvalid())
  	{
  		m_TSFile.CloseFile();
  	}
		m_TSFile.SetFileName(file->filename);
		m_TSFile.OpenFile();
		m_TSFileId = file->filePositionId;		
	}

  //Start of 'file next' SMB data cache workaround processing
	if(!fileNext && !m_TSFileNext.IsFileInvalid())
  {
  	m_TSFileNext.CloseFile();
    m_TSFileIdNext = -1;
  }
  
  if (fileNext && m_bUseFileNext && m_pFileReadNextBuffer)
  {
  	if (m_TSFileIdNext != fileNext->filePositionId)
  	{
    	if (!m_TSFileNext.IsFileInvalid())
    	{
    		m_TSFileNext.CloseFile();
    	}
  		m_TSFileNext.SetFileName(fileNext->filename);
  		m_TSFileNext.OpenFile();
  		m_TSFileIdNext = fileNext->filePositionId;
  		m_currPosnFileNext = 0;
  		
      //char url[MAX_PATH];
      //WideCharToMultiByte(CP_ACP, 0 ,fileNext->filename, -1, url, MAX_PATH, 0, 0);
      //LogDebug("MultiFileReader::FileNext Changed to %s", url);
      
      if (fileNext->length >= NEXT_READ_SIZE)
      {
        //Do a dummy read to try and refresh the SMB cache
        ULONG bytesNextRead = 0;
    		m_TSFileNext.SetFilePointer(m_currPosnFileNext, FILE_BEGIN);
    		m_TSFileNext.Read(m_pFileReadNextBuffer, NEXT_READ_SIZE, &bytesNextRead);
        m_lastFileNextRead = timeGetTime();
        if (bytesNextRead != NEXT_READ_SIZE)
        {
          char url[MAX_PATH];
          WideCharToMultiByte(CP_ACP, 0 ,fileNext->filename, -1, url, MAX_PATH, 0, 0);
          LogDebug("MultiFileReader::FileNext read 1 failed, bytes %d, posn %I64d, file %s", bytesNextRead, m_currPosnFileNext, url);
        }
    		m_currPosnFileNext += NEXT_READ_SIZE;
    		m_currPosnFileNext %= NEXT_READ_ROLLOVER;
      }
  	}  	
    else if ((fileNext->length >= (m_currPosnFileNext+NEXT_READ_SIZE)) && (timeGetTime() > (m_lastFileNextRead+1100)))
    {
      //Do a dummy read to try and refresh the SMB data cache
      ULONG bytesNextRead = 0;
  		m_TSFileNext.SetFilePointer(m_currPosnFileNext, FILE_BEGIN);
  		m_TSFileNext.Read(m_pFileReadNextBuffer, NEXT_READ_SIZE, &bytesNextRead);
      m_lastFileNextRead = timeGetTime();
      if (bytesNextRead != NEXT_READ_SIZE)
      {
        char url[MAX_PATH];
        WideCharToMultiByte(CP_ACP, 0 ,fileNext->filename, -1, url, MAX_PATH, 0, 0);
        LogDebug("MultiFileReader::FileNext read 2 failed, bytes %d, posn %I64d, file %s", bytesNextRead, m_currPosnFileNext, url);
      }
  		m_currPosnFileNext += NEXT_READ_SIZE;
  		m_currPosnFileNext %= NEXT_READ_ROLLOVER;
    }
  }  
  //End of 'file next' SMB data cache workaround processing

	__int64 seekPosition = m_currentPosition - file->startPosition;

	m_TSFile.SetFilePointer(seekPosition, FILE_BEGIN);
  __int64 posSeeked=m_TSFile.GetFilePointer();
  if (posSeeked!=seekPosition)
  {
    LogDebug("MultiFileReader::SEEK FAILED");
		*dwReadBytes = 0;
		return retval;
  }

	ULONG bytesRead = 0;

	__int64 bytesToRead = file->length - seekPosition;
	
	if (lDataLength > bytesToRead)
	{
    //LogDebug("MultiFileReader::multi-read 0, pbData=%d, lDataLength=%d, bytesToRead=%I64d, bytesRead=%d, m_currentPosition=%I64d, fileLength=%I64d", pbData, lDataLength, bytesToRead, bytesRead, m_currentPosition, file->length);
		hr = m_TSFile.Read(pbData, (ULONG)bytesToRead, &bytesRead);
    if (FAILED(hr))
    {
      LogDebug("MultiFileReader::READ FAILED1");
      retval = E_FAIL;
	    *dwReadBytes = 0;
		  m_currentPosition = oldCurrentPosn;
      return retval;
    }
		m_currentPosition += (__int64)bytesRead;
    //LogDebug("MultiFileReader::multi-read 1, pbData=%d, lDataLength=%d, bytesToRead=%I64d, bytesRead=%d, m_currentPosition=%I64d, fileLength=%I64d", pbData, lDataLength, bytesToRead, bytesRead, m_currentPosition, file->length);
    
    if ((bytesRead < bytesToRead) || !fileNext)
    {
      //We haven't got all of the current file segment (so we can't read the next segment), 
      //or there is no 'next file' to read so just return the data we have...
      //LogDebug("MultiFileReader::multi-read 1A, pbData=%d, lDataLength=%d, bytesToRead=%I64d, bytesRead=%d, m_currentPosition=%I64d", pbData, lDataLength, bytesToRead, bytesRead, m_currentPosition);
	    *dwReadBytes = bytesRead;
      return retval;
    }

		hr = this->ReadNoLock(pbData + (ULONG)bytesToRead, lDataLength - (ULONG)bytesToRead, dwReadBytes);
    if (FAILED(hr))
    {
      LogDebug("MultiFileReader::READ FAILED2");
      retval = E_FAIL;
	    *dwReadBytes = 0;
		  m_currentPosition = oldCurrentPosn;
      return retval;
    }
    //LogDebug("MultiFileReader::multi-read 2, pbData=%d, lDataLength=%d, bytesRead=%d, m_currentPosition=%I64d", (pbData + (ULONG)bytesToRead), (lDataLength - (ULONG)bytesToRead), *dwReadBytes, m_currentPosition);
		*dwReadBytes += bytesRead;
	}
	else
	{  	
		hr = m_TSFile.Read(pbData, lDataLength, dwReadBytes);
    if (FAILED(hr))
    {
      LogDebug("MultiFileReader::READ FAILED3");
      retval = E_FAIL;
	    *dwReadBytes = 0;
		  m_currentPosition = oldCurrentPosn;
      return retval;
    }
		m_currentPosition += (__int64)*dwReadBytes;
    //LogDebug("MultiFileReader::multi-read 3, pbData=%d, lDataLength=%d, dwReadBytes=%d, m_currentPosition=%I64d, fileLength=%I64d", pbData, lDataLength, *dwReadBytes, m_currentPosition, file->length);
	}

	return retval;
}

HRESULT MultiFileReader::Read(PBYTE pbData, ULONG lDataLength, ULONG *dwReadBytes, __int64 llDistanceToMove, DWORD dwMoveMethod)
{
	//If end method then we want llDistanceToMove to be the end of the buffer that we read.
	if (dwMoveMethod == FILE_END)
		llDistanceToMove = 0 - llDistanceToMove - lDataLength;

	SetFilePointer(llDistanceToMove, dwMoveMethod);

	return Read(pbData, lDataLength, dwReadBytes);
}

HRESULT MultiFileReader::RefreshTSBufferFile()
{
	if (m_TSBufferFile.IsFileInvalid())
		return S_FALSE;

	ULONG bytesRead;
	MultiFileReaderFile *file;

  HRESULT result;
	__int64 currentPosition;
  long filesAdded, filesRemoved;
  long filesAdded2, filesRemoved2;
  long Error=0;
  long Loop=10 ;
  __int64 fileLength = 0;
  	
  do
  {
   	if (Error) //Handle errors from a previous loop iteration
   	{
  	  LogDebug("MultiFileReader has error 0x%x in Loop %d. Try to clear SMB Cache.", Error, 10-Loop);  	  
      LogDebug("MultiFileReader m_filesAdded : %d, m_filesRemoved : %d, m_startPosition : %I64d, m_endPosition : %I64d, currentPosition = %I64d", m_filesAdded, m_filesRemoved, m_startPosition, m_endPosition, currentPosition) ;
  	  // try to clear local / remote SMB file cache. This should happen when we close the filehandle
      m_TSBufferFile.CloseFile();
  	  m_TSBufferFile.OpenFile();
  	  Sleep(5);
    }  

    Error=0;
  	currentPosition = -1;
  	filesAdded = -1;
  	filesRemoved = -1;
  	filesAdded2 = -2;
  	filesRemoved2 = -2;
    Loop-- ;

	  fileLength = m_TSBufferFile.GetFileSize();

    // Min file length is Header ( __int64 + long + long ) + filelist ( > 0 ) + Footer ( long + long ) 
    if (fileLength <= (sizeof(__int64) + sizeof(long) + sizeof(long) + sizeof(wchar_t) + sizeof(long) + sizeof(long)))
		  return S_FALSE;

    if (fileLength%2) //Must be an even number of bytes in length
		  return S_FALSE;
		
		if (fileLength > INFO_BUFF_SIZE)
      return E_FAIL ;
  
  	int readLength = sizeof(currentPosition) + sizeof(filesAdded) + sizeof(filesRemoved);
    
  	m_TSBufferFile.SetFilePointer(0, FILE_BEGIN);
  	result = m_TSBufferFile.Read(m_pInfoFileBuffer1, readLength, &bytesRead);

    if (!SUCCEEDED(result) || bytesRead != readLength) 
		  Error |= 0x02;

  	if(Error == 0)
  	{
  		currentPosition = *((__int64*)(m_pInfoFileBuffer1 + 0));
  		filesAdded = *((long*)(m_pInfoFileBuffer1 + sizeof(__int64)));
  		filesRemoved = *((long*)(m_pInfoFileBuffer1 + sizeof(__int64) + sizeof(long)));
  	}

  	m_TSBufferFile.SetFilePointer(0, FILE_BEGIN);
  	result = m_TSBufferFile.Read(m_pInfoFileBuffer2, readLength, &bytesRead);

    if (!SUCCEEDED(result) || bytesRead != readLength) 
		  Error |= 0x04;

  	if(Error == 0)
  	{
  		currentPosition = *((__int64*)(m_pInfoFileBuffer2 + 0));
  		filesAdded2 = *((long*)(m_pInfoFileBuffer2 + sizeof(__int64)));
  		filesRemoved2 = *((long*)(m_pInfoFileBuffer2 + sizeof(__int64) + sizeof(long)));
  	}

    if ((filesAdded2 != filesAdded) || (filesRemoved2 != filesRemoved))
    {
		  Error |= 0x08;
      continue;
    } 

    // If no files added or removed, break the loop !
    if ((m_filesAdded == filesAdded) && (m_filesRemoved == filesRemoved)) 
			break ;

    //Now read the full file for processing and comparison
  	m_TSBufferFile.SetFilePointer(0, FILE_BEGIN);
  	result=m_TSBufferFile.Read(m_pInfoFileBuffer1, (ULONG)fileLength, &bytesRead);
  	
    if (!SUCCEEDED(result) || bytesRead != fileLength) 
      Error |= 0x20 ;
      
    Sleep(1);
	  
	  //read it again to a different buffer  
  	m_TSBufferFile.SetFilePointer(0, FILE_BEGIN);
  	result = m_TSBufferFile.Read(m_pInfoFileBuffer2, (ULONG)fileLength, &bytesRead);

    if (!SUCCEEDED(result) || bytesRead != fileLength) 
		  Error |= 0x40;

    //Compare the two buffers (except the 'currentPosition' values), and compare the filesAdded/filesRemoved values 
    //at the beginning and end of the second buffer for integrity checking
  	if (
  	    (Error == 0) 
  	    && (memcmp(m_pInfoFileBuffer1 + sizeof(__int64), m_pInfoFileBuffer2 + sizeof(__int64), (ULONG)(fileLength - sizeof(__int64))) == 0)
  	    && (memcmp(m_pInfoFileBuffer2 + sizeof(__int64), m_pInfoFileBuffer2 + fileLength - (2*sizeof(long)), 2*sizeof(long)) == 0)
  	    )
  	{
  		currentPosition = *((__int64*)(m_pInfoFileBuffer2 + 0)); //use the most recent value
  		filesAdded = *((long*)(m_pInfoFileBuffer2 + sizeof(__int64)));
  		filesRemoved = *((long*)(m_pInfoFileBuffer2 + sizeof(__int64) + sizeof(long)));
   	}
   	else
   	{
		  Error |= 0x80;
      continue;
    }  

    //Rebuild the file list if files have been added or removed
  	if ((m_filesAdded != filesAdded) || (m_filesRemoved != filesRemoved))
  	{
  		long filesToRemove = filesRemoved - m_filesRemoved;
  		long filesToAdd = filesAdded - m_filesAdded;
  		long fileID = filesRemoved;
  		__int64 nextStartPosition = 0;
  
  		// Removed files that aren't present anymore.
  		while ((filesToRemove > 0) && (m_tsFiles.size() > 0))
  		{
  			MultiFileReaderFile *file = m_tsFiles.at(0);
  			
  			delete file;
  			m_tsFiles.erase(m_tsFiles.begin());
  
  			filesToRemove--;
  		}  
  
  		// Figure out what the start position of the next new file will be
  		if (m_tsFiles.size() > 0)
  		{
  			file = m_tsFiles.back();
  
  			if (filesToAdd > 0)
  			{
  				// If we're adding files the chances are the one at the back has a partial length
  				// so we need update it.
  				result = GetFileLength(file->filename, file->length, true);
  				if (!SUCCEEDED(result)) 
		        Error |= 0x10;
  			}
  
  			nextStartPosition = file->startPosition + file->length;
  		} 
  
  		//Get the real path of the buffer file
  		LPWSTR wfilename;
  		m_TSBufferFile.GetFileName(&wfilename);
  		LPWSTR path = NULL;
  		LPWSTR name = wcsrchr(wfilename, 92);
  		if (name)
  		{
  			name++;
  			long len = name - wfilename;
  			path = new wchar_t[len+1];
  			lstrcpynW(path, wfilename, len+1);
  		}
  
  		// Create a list of files in the .tsbuffer file.
  		std::vector<LPWSTR> filenames;
  
  		LPWSTR pCurr = (LPWSTR)(m_pInfoFileBuffer2 + sizeof(__int64) + sizeof(long) + sizeof(long)); //pointer to start of filename section
  		LPWSTR pEndOfList = (LPWSTR)(m_pInfoFileBuffer2 + fileLength - (2*sizeof(long)));
  		long length = wcslen(pCurr);
  		while ((length > 0) && (pCurr < pEndOfList))
  		{
  			//modify filename path here to include the real path
  			LPWSTR pFilename;
  			LPWSTR temp = wcsrchr(pCurr, 92);
  			if (path && temp)
  			{
  				temp++;
  				pFilename = new wchar_t[wcslen(path)+wcslen(temp)+1];
  				wcscpy(pFilename, path);
  				wcscat(pFilename, temp);
  			}
  			else
  			{
  				pFilename = new wchar_t[length+1];
  				wcscpy(pFilename, pCurr);
  			}
  
  			filenames.push_back(pFilename);
  
  			pCurr += (length + 1);
  			length = wcslen(pCurr);
  		}
  
  		if (path)
  			delete[] path;

  	  if ((filesAdded - filesRemoved) != filenames.size())
  	  {
        LogDebug("MultiFileReader: expected file count incorrect") ;
  		  Error |= 0x200;
        continue;
  	  }  	   
  
  		// Go through files
  		std::vector<MultiFileReaderFile *>::iterator itFiles = m_tsFiles.begin();
  		std::vector<LPWSTR>::iterator itFilenames = filenames.begin();
  
  		while (itFiles < m_tsFiles.end())
  		{
  			file = *itFiles;
  
  			itFiles++;
  			fileID++;
  
  			if (itFilenames < filenames.end())
  			{
  				// TODO: Check that the filenames match. ( Ambass : With buffer integrity check, probably no need to do this !)
  				itFilenames++;
  			}
  			else
  			{
          LogDebug("MultiFileReader has missing files!!") ;
    		  Error |= 0x400;
          continue;
  			}
  		}
  
  		while (itFilenames < filenames.end())
  		{
  			LPWSTR pFilename = *itFilenames;
  
  			file = new MultiFileReaderFile();
  			file->filename = pFilename;
  			file->startPosition = nextStartPosition;
  
  			fileID++;
  			file->filePositionId = fileID;
  
  			result = GetFileLength(pFilename, file->length, false);
  		  if (!SUCCEEDED(result)) 
		      Error |= 0x100;
  
  			m_tsFiles.push_back(file);
  
  			nextStartPosition = file->startPosition + file->length;
  
  			itFilenames++;
  		}
  		
  	  if (m_tsFiles.size() != filenames.size())
  	  {
        LogDebug("MultiFileReader: files to filenames mismatch") ;
  		  Error |= 0x800;
        continue;
  	  }
  	    
  		m_filesAdded = filesAdded;
  		m_filesRemoved = filesRemoved;
  
      //LogDebug("MultiFileReader m_filesAdded : %d, m_filesRemoved : %d, m_startPosition : %I64d, m_endPosition : %I64d, currentPosition = %I64d, LatestFileID = %d", m_filesAdded, m_filesRemoved, m_startPosition, m_endPosition, currentPosition, fileID) ;
  	}

  } while ( Error && Loop ) ; // If Error is set, try again...until Loop reaches 0.
 
  if (Loop < 8)
  {
    LogDebug("MultiFileReader has waited %d times for TSbuffer integrity.", 10-Loop) ;

    if(Error)
    {
      LogDebug("MultiFileReader has failed for TSbuffer integrity. Error : %x", Error) ;
      return E_FAIL ;
    }
  }


	if (m_tsFiles.size() > 0)
	{
		file = m_tsFiles.front();
		m_startPosition = file->startPosition;

		file = m_tsFiles.back();	
    file->length = currentPosition;
		m_endPosition = file->startPosition + currentPosition;	
	}
	else
	{
		m_startPosition = 0;
		m_endPosition = 0;
	}

	return S_OK;
}


__int64 MultiFileReader::GetFileSize()
{
  CAutoLock rLock (&m_accessLock);
  RefreshTSBufferFile();
  return m_endPosition - m_startPosition;
}

HRESULT MultiFileReader::GetFileLength(LPWSTR pFilename, __int64 &length, bool doubleCheck)
{
	HRESULT hr = S_OK;

  long Error=0;
  long Loop=10 ;
  	
  do
  {
   	if (Error) //Handle errors from a previous loop iteration
   	{
   	  if (Loop < 3)
   	  {
  	    LogDebug("MultiFileReader::GetFileLength() has error 0x%x in Loop %d. Trying again", Error, 10-Loop);  
  	  }	  
  	  Sleep(5);
    }  

    Error=0;
    Loop-- ;

  	m_TSFileGetLength.SetFileName(pFilename);
  	hr = m_TSFileGetLength.OpenFile();
  	if (!SUCCEEDED(hr))
  	{
  	  Error |= 0x2;
  	}
  	length = m_TSFileGetLength.GetFileSize();
  	if (doubleCheck)
  	{
    	Sleep(5);
    	if (length != m_TSFileGetLength.GetFileSize())
    	{
    	  Error |= 0x4;
    	}
    }
    m_TSFileGetLength.CloseFile();

  } while ( Error && Loop ) ; // If Error is set, try again...until Loop reaches 0.
 
  if (Loop < 2)
  {
    LogDebug("MultiFileReader::GetFileLength() has waited %d times for stable length.", 10-Loop) ;

    if(Error)
    {
      LogDebug("MultiFileReader::GetFileLength() has failed. Error : %x", Error) ;
      length = 0;
      return E_FAIL ;
    }
  }

	return hr;
}

//Enable 'FileNext' file reads to workaround SMB2/SM3 possible 'data cache' problems
void MultiFileReader::SetFileNext(BOOL useFileNext)
{
  CAutoLock rLock (&m_accessLock);
	m_bUseFileNext = useFileNext;
	//LogDebug("FileReader::SetFileNext, useFileNext = %d", useFileNext);
}

BOOL MultiFileReader::GetFileNext()
{
  CAutoLock rLock (&m_accessLock);
	return m_bUseFileNext;
}


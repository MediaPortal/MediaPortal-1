/**
*  MultiFileReader.cpp
*  Copyright (C) 2005      nate
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

//#include <streams.h>
#include "MultiFileReader.h"
#include <atlbase.h>
extern void Log(const char *fmt, ...) ;
MultiFileReader::MultiFileReader()
{
	m_startPosition = 0;
	m_endPosition = 0;
	m_currentPosition = 0;
	m_filesAdded = 0;
	m_filesRemoved = 0;
	m_TSFileId = 0;
	m_bReadOnly = 1;
	m_bDelay = 0;
  m_bDebugOutput=1;
  m_cachedFileSize=0;
}

MultiFileReader::~MultiFileReader()
{
	//CloseFile called by ~FileReader
/*	USES_CONVERSION;

	std::vector<MultiFileReaderFile *>::iterator it = m_tsFiles.begin();
	for ( ; it < m_tsFiles.end() ; it++ )
	{
		if((*it)->filename)
		{
			DeleteFile(W2T((*it)->filename));
			delete[] (*it)->filename;
		}

		delete *it;
	};
*/
}

FileReader* MultiFileReader::CreateFileReader()
{
	return (FileReader *)new MultiFileReader();
}

int MultiFileReader::GetFileName(char *lpszFileName)
{
	return m_TSBufferFile.GetFileName(lpszFileName);
}

int MultiFileReader::SetFileName(char* pszFileName)
{
  strcpy(m_fileName,pszFileName);
	return m_TSBufferFile.SetFileName(pszFileName);
}

//
// OpenFile
//
int MultiFileReader::OpenFile()
{
  //printf("MultiFileReader::OpenFile()");
	int hr = m_TSBufferFile.OpenFile();

	RefreshTSBufferFile();

	m_currentPosition = 0;

	return hr;
}

//
// CloseFile
//
int MultiFileReader::CloseFile()
{
  //printf("MultiFileReader::CloseFile()");
	int hr;
	hr = m_TSBufferFile.CloseFile();
	hr = m_TSFile.CloseFile();
	m_TSFileId = 0;
	return hr;
}

BOOL MultiFileReader::IsFileInvalid()
{
	return m_TSBufferFile.IsFileInvalid();
}

int MultiFileReader::GetFileSize(__int64 *pStartPosition, __int64 *pLength)
{
//	RefreshTSBufferFile();
	//CheckPointer(pStartPosition,E_POINTER);
	//CheckPointer(pLength,E_POINTER);
	*pStartPosition = m_startPosition;
	*pLength = (__int64)(m_endPosition - m_startPosition);
	return S_OK;
}

DWORD MultiFileReader::SetFilePointer(__int64 llDistanceToMove, DWORD dwMoveMethod)
{
//	RefreshTSBufferFile();

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
		m_currentPosition = llDistanceToMove;
	}

	if (m_currentPosition < m_startPosition)
		m_currentPosition = m_startPosition;

	if (m_currentPosition > m_endPosition)
		m_currentPosition = m_endPosition;

	//RefreshTSBufferFile();
	return S_OK;
}

__int64 MultiFileReader::GetFilePointer()
{
//	RefreshTSBufferFile();
	return m_currentPosition;
}

int MultiFileReader::Read(BYTE* pbData, ULONG lDataLength, ULONG *dwReadBytes)
{
	int hr;
  //printf("MultiFileReader::Read:%d",(int)lDataLength);
	// If the file has already been closed, don't continue
	if (m_TSBufferFile.IsFileInvalid())
  {
    Log("MultiFileReader::Read() failed invalid file()");
		return S_FALSE;
  }
	RefreshTSBufferFile();
  RefreshFileSize();

	if (m_currentPosition < m_startPosition)
		m_currentPosition = m_startPosition;

	// Find out which file the currentPosition is in.
	MultiFileReaderFile *file = NULL;
	std::vector<MultiFileReaderFile *>::iterator it = m_tsFiles.begin();
	for ( ; it < m_tsFiles.end() ; it++ )
	{
		file = *it;
		if (m_currentPosition < (file->startPosition + file->length))
			break;
	};

	if(!file)
  {
    Log("MultiFileReader::Read() failed() no file");
		return S_FALSE;
  }
	if (m_currentPosition < (file->startPosition + file->length))
	{
		if (m_TSFileId != file->filePositionId)
		{
			m_TSFile.CloseFile();
			m_TSFile.SetFileName(file->filename);
			m_TSFile.OpenFile();

			m_TSFileId = file->filePositionId;

			if (m_bDebugOutput)
			{
				char sz[MAX_PATH+128];
				sprintf(sz, "Current File Changed to %s", file->filename);
        Log(sz);
			}
		}

		__int64 seekPosition = m_currentPosition - file->startPosition;
		m_TSFile.SetFilePointer(seekPosition, FILE_BEGIN);

		ULONG bytesRead = 0;

		__int64 bytesToRead = file->length - seekPosition;
    if (bytesToRead > lDataLength)
      bytesToRead = lDataLength;
		if (lDataLength > bytesToRead)
		{
			hr = m_TSFile.Read(pbData, bytesToRead, &bytesRead);
			m_currentPosition += bytesToRead;

			hr = this->Read(pbData + bytesToRead, lDataLength - bytesToRead, dwReadBytes);
			*dwReadBytes += bytesRead;
		}
		else
		{
			hr = m_TSFile.Read(pbData, lDataLength, dwReadBytes);
			m_currentPosition += lDataLength;
		}
	}
	else
	{
		// The current position is past the end of the last file
		*dwReadBytes = 0;
	}

	return S_OK;
}

int MultiFileReader::Read(BYTE* pbData, ULONG lDataLength, ULONG *dwReadBytes, __int64 llDistanceToMove, DWORD dwMoveMethod)
{
	//If end method then we want llDistanceToMove to be the end of the buffer that we read.
	if (dwMoveMethod == FILE_END)
		llDistanceToMove = 0 - llDistanceToMove - lDataLength;

	SetFilePointer(llDistanceToMove, dwMoveMethod);

	return Read(pbData, lDataLength, dwReadBytes);
}

int MultiFileReader::get_ReadOnly(WORD *ReadOnly)
{
	//CheckPointer(ReadOnly, E_POINTER);

	if (!m_TSBufferFile.IsFileInvalid())
		return m_TSBufferFile.get_ReadOnly(ReadOnly);

	*ReadOnly = m_bReadOnly;
	return S_OK;
}

int MultiFileReader::RefreshTSBufferFile()
{
    //printf("MultiFileReader::RefreshTSBufferFile");
	if (m_TSBufferFile.IsFileInvalid())
  {
    Log("MultiFileReader::RefreshTSBufferFile->IsFileInvalid");
		return S_FALSE;
  }
	ULONG bytesRead;
	MultiFileReaderFile *file; 

   HRESULT result;
	__int64 currentPosition;
  long filesAdded, filesRemoved;
  long filesAdded2, filesRemoved2;
  long Error;
  long Loop=10 ;           

  __int64 remainingLength ;
  char *pBuffer ;
  	
  do
  {
    Error=0  ;
  	m_TSBufferFile.SetFilePointer(0, FILE_END);
	  __int64 fileLength = m_TSBufferFile.GetFilePointer();
	  if (fileLength <= (sizeof(__int64) + sizeof(long) + sizeof(long) + sizeof(wchar_t) + sizeof(long) + sizeof(long))) Error|=0x01;

	  m_TSBufferFile.SetFilePointer(0, FILE_BEGIN);
	
    //LAYOUT:
    // 64bit    : current position
    // long     : files added
    // long     : files removed 
    // ... File list
    // long     : files added
    // long     : files removed 

	  result=m_TSBufferFile.Read((BYTE*)&currentPosition, sizeof(currentPosition), &bytesRead);
    if (!SUCCEEDED(result)|| bytesRead!=sizeof(currentPosition)) Error|=0x02;

	  result=m_TSBufferFile.Read((BYTE*)&filesAdded, sizeof(filesAdded), &bytesRead);
    if (!SUCCEEDED(result)|| bytesRead!=sizeof(filesAdded)) Error=0x04;

	  result=m_TSBufferFile.Read((BYTE*)&filesRemoved, sizeof(filesRemoved), &bytesRead);
    if (!SUCCEEDED(result)||  bytesRead!=sizeof(filesRemoved)) Error=0x08;

    // If no files added or removed, break the loop !
    if ((m_filesAdded == filesAdded) && (m_filesRemoved == filesRemoved)) break ;

    remainingLength = fileLength - sizeof(__int64) - sizeof(long) - sizeof(long) - sizeof(long) - sizeof(long) ;

    // Above 100kb or below 0 seems stupid and figure out a problem !!!
		if ((remainingLength > 100000) || (remainingLength < 0)) Error=0x10;;

    pBuffer = (char*)new BYTE[remainingLength+1];
		
		result=m_TSBufferFile.Read((BYTE*)pBuffer, (ULONG)remainingLength, &bytesRead);
    if (!SUCCEEDED(result)||  bytesRead != remainingLength) Error=0x20 ;

   	result=m_TSBufferFile.Read((BYTE*)&filesAdded2, sizeof(filesAdded2), &bytesRead);
    if (!SUCCEEDED(result)|| bytesRead!=sizeof(filesAdded2)) Error=0x40 ;

   	result=m_TSBufferFile.Read((BYTE*)&filesRemoved2, sizeof(filesRemoved2), &bytesRead);
    if (!SUCCEEDED(result)|| bytesRead!=sizeof(filesRemoved2)) Error=0x40 ;

    if ((filesAdded2!=filesAdded) || (filesRemoved2!=filesRemoved))
    {
      Sleep(10) ;
      Error=0x80 ;
    }

    if (Error) delete[] pBuffer;

    Loop-- ;
  } while ( Error && Loop ) ; // If Error is set, try again...until Loop reaches 0.
 
  if (Loop < 9)
  {
    Log("MultiFileReader has waited %d times for TSbuffer integrity.", 10-Loop) ;

    if(Error)
    {
      Log("MultiFileReader has failed for TSbuffer integrity. Error : %x", Error) ;
      return E_FAIL ;
    }
  }
  
  //printf("MultiFileReader::RefreshTSBufferFile files added:%d removed:%d", filesAdded,filesRemoved);
	if ((m_filesAdded != filesAdded) || (m_filesRemoved != filesRemoved))
	{
		long filesToRemove = filesRemoved - m_filesRemoved;
		long filesToAdd = filesAdded - m_filesAdded;
		long fileID = filesRemoved;
		__int64 nextStartPosition = 0;

		if (m_bDebugOutput)
		{
			char sz[512];
			sprintf(sz, "Files Added %i, Removed %i", filesToAdd, filesToRemove);
        Log(sz);
		}

		// Removed files that aren't present anymore.
		while ((filesToRemove > 0) && (m_tsFiles.size() > 0))
		{
			MultiFileReaderFile *file = m_tsFiles.at(0);

			if (m_bDebugOutput)
			{
				char sz[MAX_PATH+128];
				sprintf(sz, "Removing file %s", file->filename);
        Log(sz);
			}
			
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
				// If we're adding files the changes are the one at the back has a partial length
				// so we need update it.
				if (m_bDebugOutput)
					GetFileLength(file->filename, file->length);
				else
					GetFileLength(file->filename, file->length);
			}

			nextStartPosition = file->startPosition + file->length;
		}

    int len=remainingLength;
    //printf("buf len:%d",len);
    for (int i=0; i < len/2;i++)
    {
      pBuffer[i]=pBuffer[i*2];
    }

    char path[1024];
    strcpy(path,"");
    int posSlash=-1;
    for (int i=0; i < strlen(m_fileName);++i)
    {
      if (m_fileName[i]=='\\') posSlash=i;
    }
    if (posSlash>=0)
    {
      strncpy(path,m_fileName,posSlash);
      path[posSlash]=0;
    }


		// Create a list of files in the .tsbuffer file.
		std::vector<char*> filenames;

    //printf("buf:%s", pBuffer);
		char* pCurr = pBuffer;
		long length = strlen(pCurr);
		while (length > 0)
		{
			char* pFilename = new char[1024];
      if (strlen(path)>0)
      {
				//if (pFilename[1]!=':')
				//{
					//strcpy(pFilename,path);
					//strcat(pFilename,"\\");
				//}
        strcpy(pFilename,"");
      }
      else
      {
        strcpy(pFilename,"");
      }
			strcat(pFilename, pCurr);
			filenames.push_back(pFilename);

			pCurr += (length + 1);
			length = strlen(pCurr);
		}

		// Go through files
		std::vector<MultiFileReaderFile *>::iterator itFiles = m_tsFiles.begin();
		std::vector<char*>::iterator itFilenames = filenames.begin();

		while (itFiles < m_tsFiles.end())
		{
			file = *itFiles;

			itFiles++;
			fileID++;

			if (itFilenames < filenames.end())
			{
				// TODO: Check that the filenames match
				itFilenames++;
			}
			else
			{
        Log("Missing files!!");
			}
		}

		while (itFilenames < filenames.end())
		{
			char* pFilename = *itFilenames;

			if (m_bDebugOutput)
			{
				char sz[MAX_PATH+128];
				int nextStPos = nextStartPosition;
				sprintf(sz, "Adding file %s (%i)", pFilename, nextStPos);
				Log(sz);
			}

			file = new MultiFileReaderFile();
			strcpy(file->filename , pFilename);
			file->startPosition = nextStartPosition;

			fileID++;
			file->filePositionId = fileID;

			GetFileLength(pFilename, file->length);

      //printf("new MultiFile:%s %d", file->filename, file->filePositionId);
			m_tsFiles.push_back(file);

			nextStartPosition = file->startPosition + file->length;

			itFilenames++;
		}

		m_filesAdded = filesAdded;
		m_filesRemoved = filesRemoved;

    delete[] pBuffer;
	}

	if (m_tsFiles.size() > 0)
	{
		file = m_tsFiles.front();
		m_startPosition = file->startPosition;

		file = m_tsFiles.back();
		file->length = currentPosition;
		m_endPosition = file->startPosition + currentPosition;

	
		/*if (m_bDebugOutput)
		{
			char sz[128];
			int stPos = m_startPosition;
			int endPos = m_endPosition;
			int curPos = m_currentPosition;
			sprintf(sz, TEXT("StartPosition %i, EndPosition %i, CurrentPosition %i"), stPos, endPos, curPos);
			::OutputDebugString(sz);
		}*/
	}
	else
	{
		m_startPosition = 0;
		m_endPosition = 0;
	}

	return S_OK;
}

int MultiFileReader::GetFileLength(char* pFilename, __int64 &length)
{
	USES_CONVERSION;

	length = 0;

	// Try to open the file
	HANDLE hFile = CreateFileA((LPCSTR)pFilename,   // The filename
						 GENERIC_READ,          // File access
						 FILE_SHARE_READ |
						 FILE_SHARE_WRITE,       // Share access
						 NULL,                  // Security
						 OPEN_EXISTING,         // Open flags
						 (DWORD) 0,             // More flags
						 NULL);                 // Template
	if (hFile != INVALID_HANDLE_VALUE)
	{
		LARGE_INTEGER li;
		li.QuadPart = 0;
		li.LowPart = ::SetFilePointer(hFile, 0, &li.HighPart, FILE_END);
		CloseHandle(hFile);
		
		length = li.QuadPart;
	}
	else
	{
		char msg[MAX_PATH];
		DWORD dwErr = GetLastError();
		sprintf(msg, "Failed to open file %s : %d", pFilename, dwErr);
		Log(msg);
		return (int)(dwErr);
	}
	return S_OK;
}

int MultiFileReader::get_DelayMode(WORD *DelayMode)
{
	*DelayMode = m_bDelay;
	return S_OK;
}

int MultiFileReader::set_DelayMode(WORD DelayMode)
{
	m_bDelay = DelayMode;
	return S_OK;
}

int MultiFileReader::get_ReaderMode(WORD *ReaderMode)
{
	*ReaderMode = TRUE;
	return S_OK;
}

DWORD MultiFileReader::setFilePointer(__int64 llDistanceToMove, DWORD dwMoveMethod)
{
	//Get the file information
	__int64 fileStart, fileEnd, fileLength;
	GetFileSize(&fileStart, &fileLength);
	fileEnd = (__int64)(fileLength + fileStart);
	if (dwMoveMethod == FILE_BEGIN)
		return SetFilePointer((__int64)min(fileEnd,(__int64)(llDistanceToMove + fileStart)), FILE_BEGIN);
	else
		return SetFilePointer((__int64)max((__int64)-fileLength, llDistanceToMove), FILE_END);
}

__int64 MultiFileReader::getFilePointer()
{
	__int64 fileStart, fileEnd, fileLength;
	GetFileSize(&fileStart, &fileLength);
	fileEnd = fileLength + fileStart;
	return (__int64)(GetFilePointer() - fileStart);
		
}

__int64 MultiFileReader::GetFileSize()
{
  if (m_cachedFileSize==0)
  {
    RefreshTSBufferFile();
    RefreshFileSize();
  }
  return m_cachedFileSize;
}

void MultiFileReader::RefreshFileSize()
{
	__int64 fileLength=0;
	std::vector<MultiFileReaderFile *>::iterator it = m_tsFiles.begin();
	for ( ; it < m_tsFiles.end() ; it++ )
	{
		MultiFileReaderFile *file =*it;
		fileLength+=file->length;
	}
	m_cachedFileSize= fileLength;
}
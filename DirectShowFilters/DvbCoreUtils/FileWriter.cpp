/**
*  FileWriter.cpp
*  Copyright (C) 2006-2007      nate
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
#include "..\shared\FileWriter.h"
#include <cstddef>    // NULL
#include <cstring>    // memcpy()
#include <cwchar>     // wcscpy(), wcslen()
#include <sstream>
#include <Windows.h>  // CloseHandle(), CreateFileW(), MAX_PATH

using namespace std;


extern void LogDebug(const wchar_t* fmt, ...);

FileWriter::FileWriter()
{
  m_fileHandle = INVALID_HANDLE_VALUE;
  m_fileName = NULL;
  m_filePart = 2;
  m_useReservations = false;
  m_reservedFileSize = 0;
  m_reservationChunkSize = 2000000;
  m_useAsync = true;
  m_asyncDataOffset = 0;
}

FileWriter::~FileWriter()
{
  CloseFile();
}

HRESULT FileWriter::GetFileName(wchar_t** fileName)
{
  *fileName = m_fileName;
  return S_OK;
}

HRESULT FileWriter::OpenFile(const wchar_t* fileName)
{
  return OpenFile(fileName, false);
}

HRESULT FileWriter::OpenFile(const wchar_t* fileName, bool disableLogging)
{
  if (!disableLogging)
  {
    LogDebug(L"file writer: open file, name = %s",
              fileName == NULL ? L"" : fileName);
  }

  // Is the file already open?
  if (m_fileHandle != INVALID_HANDLE_VALUE)
  {
    if (!disableLogging)
    {
      LogDebug(L"file writer: file already open, current = %s, requested = %s",
                m_fileName == NULL ? L"" : m_fileName,
                fileName == NULL ? L"" : fileName);
    }
    return S_FALSE;
  }

  // Is the file name valid?
  if (fileName == NULL)
  {
    if (!disableLogging)
    {
      LogDebug(L"file writer: failed to open file, name not supplied");
    }
    return E_INVALIDARG;
  }
  unsigned long fileNameLength = wcslen(fileName);
  if (fileNameLength > MAX_PATH)
  {
    if (!disableLogging)
    {
      LogDebug(L"file writer: failed to open file, name length = %lu, maximum name length = %lu, name = %s",
                fileNameLength, MAX_PATH, fileName);
    }
    return CO_E_PATHTOOLONG;
  }

  // See if the file is being read.
  m_fileHandle = CreateFileW(fileName,                // file name
                              GENERIC_WRITE,          // file access
                              0,                      // share access
                              NULL,                   // security
                              OPEN_ALWAYS,            // open flags
                              FILE_ATTRIBUTE_NORMAL,  // more flags
                              NULL);                  // template
  if (m_fileHandle == INVALID_HANDLE_VALUE)
  {
    DWORD errorCode = GetLastError();
    HRESULT hr = HRESULT_FROM_WIN32(errorCode);
    if (!disableLogging)
    {
      LogDebug(L"file writer: failed to open file exclusively, error = %lu, hr = 0x%x, name = %s",
                errorCode, hr, fileName);
    }
    return hr;
  }
  CloseHandle(m_fileHandle);

  // Try to open the file.
  DWORD flagsAndAttributes = FILE_ATTRIBUTE_NORMAL;
  if (m_useAsync)
  {
    flagsAndAttributes |= FILE_FLAG_OVERLAPPED;
  }
  m_fileHandle = CreateFileW(fileName,                // file name
                              GENERIC_WRITE,          // file access
                              FILE_SHARE_READ,        // share access
                              NULL,                   // security
                              OPEN_ALWAYS,            // open flags
                              flagsAndAttributes,     // more flags
                              NULL);                  // template
  if (m_fileHandle == INVALID_HANDLE_VALUE)
  {
    DWORD errorCode = GetLastError();
    HRESULT hr = HRESULT_FROM_WIN32(errorCode);
    if (!disableLogging)
    {
      LogDebug(L"file writer: failed to open file, error = %lu, hr = 0x%x, use async = %d, name = %s",
                errorCode, hr, m_useAsync, fileName);
    }
    return hr;
  }

  // Take a copy of the file name.
  if (m_fileName != NULL)
  {
    delete[] m_fileName;
  }
  m_fileName = new wchar_t[fileNameLength + 1];
  if (m_fileName == NULL)
  {
    if (!disableLogging)
    {
      LogDebug(L"file writer: failed to allocate %lu bytes for a file name copy, name = %s",
                fileNameLength, fileName);
    }
    CloseHandle(m_fileHandle);
    m_fileHandle = INVALID_HANDLE_VALUE;
    return E_OUTOFMEMORY;
  }
  wcscpy(m_fileName, fileName);

  m_filePart = 2;
  m_asyncDataOffset = 0;
  SetFilePointer(0, FILE_END);
  GetFilePointer(m_reservedFileSize);
  SetFilePointer(0, FILE_BEGIN);
  return S_OK;
}

HRESULT FileWriter::CloseFile()
{
  if (m_fileHandle == INVALID_HANDLE_VALUE)
  {
    return S_FALSE;
  }

  LogDebug(L"file writer: close file, name = %s",
            m_fileName == NULL ? L"" : m_fileName);
  if (m_useReservations)
  {
    unsigned long long currentPosition;
    if (SUCCEEDED(GetFilePointer(currentPosition)))
    {
      SetFilePointer(currentPosition, FILE_BEGIN);
    }
    SetEndOfFile(m_fileHandle);
  }

  CloseHandle(m_fileHandle);
  m_fileHandle = INVALID_HANDLE_VALUE;

  if (m_fileName != NULL)
  {
    delete[] m_fileName;
    m_fileName = NULL;
  }
  return S_OK;
}

HRESULT FileWriter::Write(unsigned char* data, unsigned long dataLength)
{
  return WriteInternal(data, dataLength, false, false);
}

HRESULT FileWriter::Write(unsigned char* data, unsigned long dataLength, bool disableLogging)
{
  return WriteInternal(data, dataLength, disableLogging, false);
}

HRESULT FileWriter::Write(unsigned char* data,
                          unsigned long dataLength,
                          bool disableLogging,
                          unsigned long long offset)
{
  m_asyncDataOffset = offset;
  SetFilePointer(offset, FILE_BEGIN);
  return WriteInternal(data, dataLength, disableLogging, false);
}

bool FileWriter::IsFileInvalid()
{
  return m_fileHandle == INVALID_HANDLE_VALUE;
}

HRESULT FileWriter::SetFilePointer(long long distanceToMove, DWORD moveMethod)
{
  //LogDebug(L"file writer: set file pointer, distance to move = %lld, move method = %lu, name = %s",
  //          distanceToMove, moveMethod, m_fileName == NULL ? L"" : m_fileName);
  LARGE_INTEGER temp;
  temp.QuadPart = distanceToMove;
  DWORD result = ::SetFilePointer(m_fileHandle, temp.LowPart, &temp.HighPart, moveMethod);
  if (result == INVALID_SET_FILE_POINTER)
  {
    DWORD errorCode = GetLastError();
    if (errorCode != NO_ERROR)
    {
      HRESULT hr = HRESULT_FROM_WIN32(errorCode);
      LogDebug(L"file writer: failed to set file pointer, distance to move = %lld, move method = %lu, error = %lu, hr = 0x%x, name = %s",
                distanceToMove, moveMethod, errorCode, hr,
                m_fileName == NULL ? L"" : m_fileName);
      return hr;
    }
  }
  return S_OK;
}

HRESULT FileWriter::GetFilePointer(unsigned long long& pointer)
{
  pointer = 0;
  LARGE_INTEGER temp;
  temp.QuadPart = 0;
  temp.LowPart = ::SetFilePointer(m_fileHandle, 0, &temp.HighPart, FILE_CURRENT);
  if (temp.LowPart == INVALID_SET_FILE_POINTER)
  {
    DWORD errorCode = GetLastError();
    if (errorCode != NO_ERROR)
    {
      HRESULT hr = HRESULT_FROM_WIN32(errorCode);
      LogDebug(L"file writer: failed to get file pointer, error = %lu, hr = 0x%x, name = %s",
                errorCode, hr, m_fileName == NULL ? L"" : m_fileName);
      return hr;
    }
  }
  pointer = temp.QuadPart;
  //LogDebug(L"file writer: get file pointer, pointer = %llu, name = %s", pointer, m_fileName == NULL ? L"" : m_fileName);
  return S_OK;
}

void FileWriter::SetReservationConfiguration(unsigned long long reservationChunkSize)
{
  LogDebug(L"file writer: set reservation configuration, chunk size = %llu",
            reservationChunkSize);
  m_useReservations = reservationChunkSize != 0;
  if (m_useReservations)
  {
    m_reservedFileSize = 0;
    m_reservationChunkSize = reservationChunkSize;
  }
}

HRESULT FileWriter::WriteInternal(unsigned char* data,
                                  unsigned long dataLength,
                                  bool disableLogging,
                                  bool isRecursive)
{
  //LogDebug(L"file writer: write, data length = %lu, is recursive = %d, use async = %d, use reservations = %d, name = %s",
  //          dataLength, isRecursive, m_useAsync, m_useReservations,
  //          m_fileName == NULL ? L"" : m_fileName);

  // If there's no data or the file is not open, don't continue.
  if (m_fileHandle == INVALID_HANDLE_VALUE)
  {
    if (!disableLogging)
    {
      LogDebug(L"file writer: failed to write to file, file not open");
    }
    return S_FALSE;
  }
  if (data == NULL || dataLength == 0)
  {
    if (!disableLogging)
    {
      LogDebug(L"file writer: failed to write to file, data length = %lu, name = %s",
                dataLength, m_fileName == NULL ? L"" : m_fileName);
    }
    return S_FALSE;
  }

  if (m_useReservations)
  {
    // Reserve more space if necessary.
    unsigned long long currentPosition;
    if (m_useAsync)
    {
      currentPosition = m_asyncDataOffset;
    }
    else
    {
      GetFilePointer(currentPosition);
    }
    if (currentPosition + dataLength > m_reservedFileSize)
    {
      //LogDebug(L"file writer: extend reservation, current position = %llu, data length = %lu, reserved file size = %llu, reservation chunk size = %llu, name = %s",
      //          currentPosition, dataLength, m_reservedFileSize,
      //          m_reservationChunkSize,
      //          m_fileName == NULL ? L"" : m_fileName);
      while (currentPosition + dataLength > m_reservedFileSize)
      {
        m_reservedFileSize += m_reservationChunkSize;
      }

      SetFilePointer(m_reservedFileSize, FILE_BEGIN);
      SetEndOfFile(m_fileHandle);
      SetFilePointer(currentPosition, FILE_BEGIN);
    }
  }

  DWORD written = 0;
  BOOL result = FALSE;
  OVERLAPPEDEX* overlapped = NULL;
  if (!m_useAsync)
  {
    result = WriteFile(m_fileHandle, data, dataLength, &written, NULL);
  }
  else
  {
    overlapped = new OVERLAPPEDEX();
    if (overlapped == NULL)
    {
      if (!disableLogging)
      {
        LogDebug(L"file writer: failed to allocate overlapped structure, name = %s",
                  m_fileName == NULL ? L"" : m_fileName);
      }
      return E_OUTOFMEMORY;
    }
    ZeroMemory(overlapped, sizeof(OVERLAPPEDEX));

    overlapped->Data = new unsigned char[dataLength];
    if (overlapped->Data == NULL)
    {
      if (!disableLogging)
      {
        LogDebug(L"file writer: failed to allocate %lu bytes for an async data buffer, name = %s",
                  dataLength, m_fileName == NULL ? L"" : m_fileName);
      }
      delete overlapped;
      return E_OUTOFMEMORY;
    }
    memcpy(overlapped->Data, data, dataLength);

    ULARGE_INTEGER offset;
    offset.QuadPart = m_asyncDataOffset;
    overlapped->Offset = offset.LowPart;
    overlapped->OffsetHigh = offset.HighPart;
    overlapped->DataLength = dataLength;
    
    result = WriteFileEx(m_fileHandle,
                          overlapped->Data,
                          overlapped->DataLength,
                          overlapped,
                          AsyncWriteCompleted);
  }

  if (result == FALSE)
  {
    DWORD errorCode = GetLastError();
    if (errorCode == ERROR_IO_PENDING)
    {
      return S_OK;
    }
    if (overlapped != NULL)
    {
      if (overlapped->Data != NULL)
      {
        delete[] overlapped->Data;
      }
      delete overlapped;
    }

    HRESULT hr;
    if (isRecursive || errorCode != ERROR_FILE_TOO_LARGE)
    {
      hr = HRESULT_FROM_WIN32(errorCode);
      if (!disableLogging)
      {
        LogDebug(L"file writer: failed to write to file, error = %lu, hr = 0x%x, data length = %lu, name = %s",
                  errorCode, hr, dataLength, m_fileName);
      }
      return hr;
    }

    // If the file system is formatted with FAT16 or FAT32 then we have file
    // size limits (2 GB and 4 GB respectively). Close the current file and
    // create a new one, then try to write again. Note that if this fails, we
    // won't be able to recover due to loss of the file name.
    LogDebug(L"file writer: reached file system maximum file size, next part = %hhu, name = %s",
              m_filePart, m_fileName);
    std::wstring tempFileName(m_fileName);
    size_t extensionOffset = tempFileName.find_last_of(L".");
    std::wstringstream newFileName;
    newFileName << tempFileName.substr(0, extensionOffset) << L"_p" << m_filePart;
    if (extensionOffset != std::wstring::npos)
    {
      newFileName << tempFileName.substr(extensionOffset);
    }

    unsigned char tempfilePart = m_filePart;  // OpenFile() resets m_filePart.
    CloseFile();
    hr = OpenFile(newFileName.str().c_str(), disableLogging);
    if (FAILED(hr))
    {
      return hr;
    }
    m_filePart = tempfilePart + 1;

    return WriteInternal(data, dataLength, disableLogging, true);
  }

  if (m_useAsync)
  {
    m_asyncDataOffset += dataLength;
    SleepEx(1, TRUE);   // Necessary to allow the async write completion call-back to be invoked.
  }
  else if (written < dataLength)
  {
    if (!disableLogging)
    {
      LogDebug(L"file writer: failed to complete write to file, data length = %lu, written = %lu, name = %s",
                dataLength, written, m_fileName);
    }
    return S_FALSE;
  }

  return S_OK;
}

void WINAPI FileWriter::AsyncWriteCompleted(DWORD errorCode,
                                            DWORD transferredByteCount,
                                            OVERLAPPED* overlapped)
{
  OVERLAPPEDEX* overlappedEx = static_cast<OVERLAPPEDEX*>(overlapped);
  if (overlappedEx == NULL)
  {
    LogDebug(L"file writer: async write completed, error = %lu, byte count = %lu, overlapped = NULL",
              errorCode, transferredByteCount);
    return;
  }

  if (errorCode != ERROR_SUCCESS)
  {
    LogDebug(L"file writer: async write completed, error = %lu, byte count = %lu, data length = %lu",
              errorCode, transferredByteCount, overlappedEx->DataLength);
  }
  else
  {
    //LogDebug(L"file writer: async write completed, byte count = %lu", transferredByteCount);
  }

  if (overlappedEx->Data != NULL)
  {
    delete[] overlappedEx->Data;
  }
  delete overlappedEx;
}
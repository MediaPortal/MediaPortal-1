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
#include <cwchar>     // wcslen(), wcsrchr(), wcstoul()
#include <sstream>
#include <string>
#include <Windows.h>  // CloseHandle(), CreateFileW(), SetFilePointer(), SetEndOfFile(), WriteFile()
#include "..\shared\EnterCriticalSection.h"


#define ASYNC_DATA_QUEUE_LENGTH_LIMIT 200


extern void LogDebug(const wchar_t* fmt, ...);

FileWriter::FileWriter()
{
  m_isFileOpen = false;
  m_fileHandle = INVALID_HANDLE_VALUE;
  m_fileName = NULL;
  m_filePart = 1;

  m_useAsyncAccess = true;
  m_asyncAccessResult = S_OK;
  m_asyncDataOffset = 0;
  m_asyncDataQueueLengthMaximum = 0;
  m_isAsyncDataQueueFull = false;

  m_useReservations = false;
  m_reservedFileSize = 0;
  m_reservationChunkSize = 2000000;
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
  return OpenFile(fileName, true, false);
}

HRESULT FileWriter::OpenFile(const wchar_t* fileName, bool isLoggingEnabled)
{
  return OpenFile(fileName, isLoggingEnabled, false);
}

HRESULT FileWriter::CloseFile()
{
  return CloseFile(false);
}

HRESULT FileWriter::Write(const unsigned char* data, unsigned long dataLength)
{
  return Write(data, dataLength, true);
}

HRESULT FileWriter::Write(const unsigned char* data,
                          unsigned long dataLength,
                          bool isErrorLoggingEnabled)
{
  if (!m_isFileOpen)
  {
    if (isErrorLoggingEnabled)
    {
      LogDebug(L"file writer: failed to write to file, file is not open");
    }
    return S_FALSE;
  }

  if (!m_useAsyncAccess)
  {
    return WriteInternal(data, dataLength, isErrorLoggingEnabled, false);
  }

  HRESULT hr;
  CWriteBuffer* buffer = CWriteBuffer::CreateBuffer(data, dataLength, hr);
  if (buffer == NULL)
  {
    if (isErrorLoggingEnabled)
    {
      LogDebug(L"file writer: failed to write to file, failed to create data buffer, hr = 0x%x, is data valid = %d, data length = %lu",
                hr, data != NULL, dataLength);
    }
    return E_OUTOFMEMORY;
  }
  return EnqueueBuffer(buffer, isErrorLoggingEnabled);
}

bool FileWriter::IsFileOpen() const
{
  return m_isFileOpen;
}

HRESULT FileWriter::SetFilePointer(unsigned long long pointer, bool isErrorLoggingEnabled)
{
  if (!m_isFileOpen)
  {
    if (isErrorLoggingEnabled)
    {
      LogDebug(L"file writer: failed to set file pointer, file is not open");
    }
    return S_FALSE;
  }

  if (!m_useAsyncAccess)
  {
    return SetFilePointer((long long)pointer, FILE_BEGIN, isErrorLoggingEnabled);
  }

  HRESULT hr;
  CWriteBuffer* buffer = CWriteBuffer::CreateBuffer((long long)pointer, hr);
  if (buffer == NULL)
  {
    if (isErrorLoggingEnabled)
    {
      LogDebug(L"file writer: failed to set file pointer, failed to create position buffer, hr = 0x%x, pointer = %llu",
                hr, pointer);
    }
    return E_OUTOFMEMORY;
  }
  return EnqueueBuffer(buffer, isErrorLoggingEnabled);
}

HRESULT FileWriter::GetFilePointer(unsigned long long& pointer, bool isErrorLoggingEnabled) const
{
  if (m_useAsyncAccess)
  {
    pointer = m_asyncDataOffset;
    return S_OK;
  }
  return GetFilePointerInternal(pointer, isErrorLoggingEnabled);
}

void FileWriter::SetReservationConfiguration(unsigned long long reservationChunkSize)
{
  LogDebug(L"file writer: set reservation configuration, chunk size = %llu",
            reservationChunkSize);
  m_useReservations = reservationChunkSize != 0;
  if (m_useReservations)
  {
    m_reservationChunkSize = reservationChunkSize;
  }
}

HRESULT FileWriter::OpenFile(const wchar_t* fileName, bool isLoggingEnabled, bool isPartFile)
{
  if (isLoggingEnabled)
  {
    LogDebug(L"file writer: open file, name = %s, part number = %lu",
              fileName == NULL ? L"" : fileName, m_filePart);
  }

  // Is the file already open?
  if (
    (!isPartFile && m_isFileOpen) ||
    (isPartFile && m_fileHandle != INVALID_HANDLE_VALUE)
  )
  {
    if (isLoggingEnabled)
    {
      LogDebug(L"file writer: file already open, current = %s, requested = %s",
                m_fileName == NULL ? L"" : m_fileName,
                fileName == NULL ? L"" : fileName);
    }
    return S_FALSE;
  }

  // Is the file name valid?
  wstring actualFileName(L"\\\\?\\");
  if (isPartFile)
  {
    actualFileName += m_fileName;
    wstringstream tempFileName;
    string::size_type position = actualFileName.find_last_of(L".");
    if (position == string::npos)
    {
      tempFileName << actualFileName << L"p" << m_filePart;
    }
    else
    {
      tempFileName << actualFileName.substr(0, position) << L"p" <<
                    m_filePart << actualFileName.substr(position);
    }
    actualFileName = tempFileName.str();
  }
  else
  {
    if (fileName == NULL)
    {
      if (isLoggingEnabled)
      {
        LogDebug(L"file writer: failed to open file, name not supplied");
      }
      return E_INVALIDARG;
    }
    actualFileName += fileName;
  }

  // Check if the file is being read.
  m_fileHandle = CreateFileW(actualFileName.c_str(),  // file name
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
    if (isLoggingEnabled)
    {
      LogDebug(L"file writer: failed to open file exclusively, error = %lu, hr = 0x%x, name = %s",
                errorCode, hr, actualFileName.c_str());
    }
    return hr;
  }
  CloseHandle(m_fileHandle);

  // Try to open the file.
  m_fileHandle = CreateFileW(actualFileName.c_str(),  // file name
                              GENERIC_WRITE,          // file access
                              FILE_SHARE_READ,        // share access
                              NULL,                   // security
                              OPEN_ALWAYS,            // open flags
                              FILE_ATTRIBUTE_NORMAL,  // more flags
                              NULL);                  // template
  if (m_fileHandle == INVALID_HANDLE_VALUE)
  {
    DWORD errorCode = GetLastError();
    HRESULT hr = HRESULT_FROM_WIN32(errorCode);
    if (isLoggingEnabled)
    {
      LogDebug(L"file writer: failed to open file, error = %lu, hr = 0x%x, name = %s",
                errorCode, hr, actualFileName.c_str());
    }
    return hr;
  }

  if (!isPartFile)
  {
    // Take a copy of the file name.
    size_t fileNameLength = wcslen(fileName) + 1; 
    wchar_t* newFileName = new wchar_t[fileNameLength];
    if (newFileName == NULL)
    {
      if (isLoggingEnabled)
      {
        LogDebug(L"file writer: failed to allocate %llu bytes for a file name copy, name = %s",
                  (unsigned long long)fileNameLength, fileName);
      }
      CloseHandle(m_fileHandle);
      m_fileHandle = INVALID_HANDLE_VALUE;
      return E_OUTOFMEMORY;
    }
    wcsncpy(newFileName, fileName, fileNameLength);

    if (m_fileName != NULL)
    {
      delete[] m_fileName;
    }
    m_fileName = newFileName;

    m_asyncAccessThreadPointlessLoopCount = 0;
    if (m_useAsyncAccess && !m_asyncAccessThread.Start(INFINITE, &FileWriter::AsyncThreadFunction, this))
    {
      LogDebug(L"file writer: failed to start async access thread, falling back to synchronous access");
      m_useAsyncAccess = false;
    }

    // Parse the file part number from the file name. This code assumes that
    // the file extension doesn't contain a 'p' character.
    m_filePart = 1;
    const wchar_t* partNumber = wcsrchr(fileName, L'p');
    if (partNumber != NULL)
    {
      wchar_t* afterPartNumber = NULL;
      m_filePart = wcstoul(partNumber + 1, &afterPartNumber, 10);
      if (
        m_filePart == 0 ||
        (afterPartNumber != NULL && afterPartNumber[0] != L'.')   // The name happens to contain "...p[number]...", but it's not actually the part number.
      )
      {
        m_filePart = 1;
      }
    }
  }

  m_isFileOpen = true;
  m_reservedFileSize = 0;
  m_asyncDataOffset = 0;
  m_asyncDataQueueLengthMaximum = 0;
  m_isAsyncDataQueueFull = false;
  return S_OK;
}

HRESULT FileWriter::CloseFile(bool isPartFile)
{
  if (!isPartFile && !m_isFileOpen)
  {
    return S_FALSE;
  }

  LogDebug(L"file writer: close file, name = %s, part number = %lu",
            m_fileName == NULL ? L"" : m_fileName, m_filePart);

  if (m_useAsyncAccess && !isPartFile)
  {
    if (!m_isAsyncDataQueueFull)
    {
      m_asyncAccessThread.Wake();   // flush
    }
    m_asyncAccessThread.Stop();
    for (vector<CWriteBuffer*>::iterator it = m_asyncDataQueue.begin(); it != m_asyncDataQueue.end(); it++)
    {
      CWriteBuffer* buffer = *it;
      if (buffer != NULL)
      {
        delete buffer;
        *it = NULL;
      }
    }
    m_asyncDataQueue.clear();
    LogDebug(L"file writer: async access information, max queue length = %lu, pointless loop count = %lu",
              m_asyncDataQueueLengthMaximum,
              m_asyncAccessThreadPointlessLoopCount);
  }

  HRESULT hr = S_OK;
  if (m_fileHandle != INVALID_HANDLE_VALUE)
  {
    // Reservation config may have changed mid-file, so don't rely on
    // m_useReservations.
    unsigned long long currentPosition;
    hr = GetFilePointerInternal(currentPosition, true);
    if (FAILED(hr) || m_reservedFileSize > currentPosition)
    {
      BOOL setEndOfFileResult = SetEndOfFile(m_fileHandle);
      if (setEndOfFileResult != TRUE)
      {
        DWORD errorCode = GetLastError();
        hr = HRESULT_FROM_WIN32(errorCode);
        LogDebug(L"file writer: failed to set end of file on close, error = %lu, hr = 0x%x, current position = %llu, reserved file size = %llu, name = %s, part number = %lu",
                  errorCode, hr, currentPosition, m_reservedFileSize,
                  m_fileName == NULL ? L"" : m_fileName, m_filePart);
      }
    }

    CloseHandle(m_fileHandle);
    m_fileHandle = INVALID_HANDLE_VALUE;
  }

  if (!isPartFile)
  {
    if (m_fileName != NULL)
    {
      delete[] m_fileName;
      m_fileName = NULL;
    }
    m_filePart = 1;

    m_isFileOpen = false;
  }
  return hr;
}

HRESULT FileWriter::WriteInternal(const unsigned char* data,
                                  unsigned long dataLength,
                                  bool isErrorLoggingEnabled,
                                  bool isRecursive)
{
  //LogDebug(L"file writer: write, data length = %lu, is recursive = %d, use async access = %d, use reservations = %d, name = %s, part number = %lu",
  //          dataLength, isRecursive, m_useAsyncAccess, m_useReservations,
  //          m_fileName == NULL ? L"" : m_fileName, m_filePart);

  // If there's no data or the file is not open, don't continue.
  HRESULT hr;
  BOOL result;
  if (m_fileHandle == INVALID_HANDLE_VALUE)
  {
    hr = OpenFile(m_fileName, isErrorLoggingEnabled, true);
    if (FAILED(hr))
    {
      return hr;
    }
  }
  if (data == NULL || dataLength == 0)
  {
    if (isErrorLoggingEnabled)
    {
      LogDebug(L"file writer: failed to write to file, data length = %lu, name = %s, part number = %lu",
                dataLength, m_fileName == NULL ? L"" : m_fileName, m_filePart);
    }
    return S_FALSE;
  }

  if (m_useReservations)
  {
    // Reserve more space if necessary.
    unsigned long long currentPosition;
    GetFilePointerInternal(currentPosition, isErrorLoggingEnabled);
    if (currentPosition + dataLength > m_reservedFileSize)
    {
      //LogDebug(L"file writer: extend reservation, current position = %llu, data length = %lu, reserved file size = %llu, reservation chunk size = %llu, name = %s, part number = %lu",
      //          currentPosition, dataLength, m_reservedFileSize,
      //          m_reservationChunkSize,
      //          m_fileName == NULL ? L"" : m_fileName, m_filePart);
      while (currentPosition + dataLength > m_reservedFileSize)
      {
        m_reservedFileSize += m_reservationChunkSize;
      }

      bool extendReservationSuccess = false;
      hr = SetFilePointer(m_reservedFileSize, FILE_BEGIN, isErrorLoggingEnabled);
      if (SUCCEEDED(hr))
      {
        result = SetEndOfFile(m_fileHandle);
        if (result != TRUE)
        {
          hr = HRESULT_FROM_WIN32(GetLastError());
        }
        else
        {
          hr = SetFilePointer(currentPosition, FILE_BEGIN, isErrorLoggingEnabled);
          if (SUCCEEDED(hr))
          {
            extendReservationSuccess = true;
          }
        }
      }
      if (!extendReservationSuccess)
      {
        DWORD errorCode = GetLastError();
        LogDebug(L"file writer: failed to extend reservation, error = %lu, hr = 0x%x, current position = %llu, target reservation = %llu, name = %s, part number = %lu",
                  errorCode, hr, currentPosition, m_reservedFileSize,
                  m_fileName, m_filePart);
      }
    }
  }

  DWORD written = 0;
  result = WriteFile(m_fileHandle, data, dataLength, &written, NULL);
  if (result == FALSE)
  {
    DWORD errorCode = GetLastError();
    if (isRecursive || errorCode != ERROR_FILE_TOO_LARGE)
    {
      hr = HRESULT_FROM_WIN32(errorCode);
      if (isErrorLoggingEnabled)
      {
        LogDebug(L"file writer: failed to write to file, error = %lu, hr = 0x%x, data length = %lu, name = %s, part number = %lu",
                  errorCode, hr, dataLength, m_fileName, m_filePart);
      }
      return hr;
    }

    // If the file system is formatted with FAT16 or FAT32 then we have file
    // size limits (2 GB and 4 GB respectively). Close the current file and
    // create a new one, then try to write again.
    LogDebug(L"file writer: reached file system maximum file size, name = %s, part number = %lu",
              m_fileName, m_filePart);
    m_filePart++;
    CloseFile(true);
    hr = OpenFile(m_fileName, isErrorLoggingEnabled, true);
    if (FAILED(hr))
    {
      return hr;
    }
    return WriteInternal(data, dataLength, isErrorLoggingEnabled, true);
  }

  m_asyncDataOffset += written;
  if (written < dataLength)
  {
    if (isErrorLoggingEnabled)
    {
      LogDebug(L"file writer: failed to complete write to file, data length = %lu, written = %lu, name = %s, part number = %lu",
                dataLength, written, m_fileName, m_filePart);
    }
    return S_FALSE;
  }

  return S_OK;
}

HRESULT FileWriter::GetFilePointerInternal(unsigned long long& pointer, bool isErrorLoggingEnabled) const
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
      if (isErrorLoggingEnabled)
      {
        LogDebug(L"file writer: failed to get file pointer, error = %lu, hr = 0x%x, name = %s, part number = %lu",
                  errorCode, hr, m_fileName == NULL ? L"" : m_fileName,
                  m_filePart);
      }
      return hr;
    }
  }
  pointer = temp.QuadPart;
  //LogDebug(L"file writer: get file pointer, pointer = %llu, name = %s, part number = %lu",
  //          pointer, m_fileName == NULL ? L"" : m_fileName, m_filePart);
  return S_OK;
}

HRESULT FileWriter::SetFilePointer(long long distanceToMove,
                                    DWORD moveMethod,
                                    bool isErrorLoggingEnabled)
{
  //LogDebug(L"file writer: set file pointer, distance to move = %lld, move method = %lu, name = %s, part number = %lu",
  //          distanceToMove, moveMethod, m_fileName == NULL ? L"" : m_fileName,
  //          m_filePart);
  if (m_fileHandle == INVALID_HANDLE_VALUE)
  {
    return E_HANDLE;
  }

  LARGE_INTEGER temp;
  temp.QuadPart = distanceToMove;
  DWORD result = ::SetFilePointer(m_fileHandle, temp.LowPart, &temp.HighPart, moveMethod);
  if (result == INVALID_SET_FILE_POINTER)
  {
    DWORD errorCode = GetLastError();
    if (errorCode != NO_ERROR)
    {
      HRESULT hr = HRESULT_FROM_WIN32(errorCode);
      if (isErrorLoggingEnabled)
      {
        LogDebug(L"file writer: failed to set file pointer, distance to move = %lld, move method = %lu, error = %lu, hr = 0x%x, name = %s, part number = %lu",
                  distanceToMove, moveMethod, errorCode, hr,
                  m_fileName == NULL ? L"" : m_fileName, m_filePart);
      }
      return hr;
    }
  }

  if (moveMethod == FILE_BEGIN)
  {
    m_asyncDataOffset = distanceToMove;
  }
  else if (m_useAsyncAccess)
  {
    return GetFilePointerInternal(m_asyncDataOffset, isErrorLoggingEnabled);
  }
  return S_OK;
}

bool __cdecl FileWriter::AsyncThreadFunction(void* arg)
{
  FileWriter* writer = (FileWriter*)arg;
  if (writer == NULL)
  {
    LogDebug(L"file writer: async thread writer not provided");
    return false;
  }

  writer->WriteNextAsyncBuffer();
  return true;
}

HRESULT FileWriter::EnqueueBuffer(CWriteBuffer* buffer, bool isErrorLoggingEnabled)
{
  size_t queueSize = 0;
  {
    CEnterCriticalSection lock(m_asyncDataQueueSection);
    if (m_isAsyncDataQueueFull || m_asyncDataQueue.size() >= ASYNC_DATA_QUEUE_LENGTH_LIMIT)
    {
      m_isAsyncDataQueueFull = true;
      if (isErrorLoggingEnabled)
      {
        LogDebug(L"file writer: failed to set file pointer, async data queue is full, name = %s, part number = %lu",
                  m_fileName, m_filePart);
      }
      delete buffer;
      return E_FAIL;
    }

    m_asyncDataQueue.push_back(buffer);
    queueSize = m_asyncDataQueue.size();
    if (m_asyncDataQueue.size() > m_asyncDataQueueLengthMaximum)
    {
      m_asyncDataQueueLengthMaximum = m_asyncDataQueue.size();
    }
  }

  if (queueSize == 1)
  {
    m_asyncAccessThread.Wake();
  }
  return S_OK;
}

void FileWriter::WriteNextAsyncBuffer()
{
  CWriteBuffer* buffer;
  {
    CEnterCriticalSection lock(m_asyncDataQueueSection);
    while (true)
    {
      if (m_asyncDataQueue.size() == 0)
      {
        m_isAsyncDataQueueFull = false;
        m_asyncAccessThreadPointlessLoopCount++;
        return;
      }
      buffer = m_asyncDataQueue[0];
      if (buffer != NULL)
      {
        break;
      }
      m_asyncDataQueue.erase(m_asyncDataQueue.begin());
    }
  }

  HRESULT hr = S_OK;
  bool isPositionValid = false;
  long long position = buffer->Position(isPositionValid);
  if (isPositionValid)
  {
    hr = SetFilePointer(position,
                        FILE_BEGIN,
                        SUCCEEDED(m_asyncAccessResult) && !buffer->IsRetry());
  }

  const unsigned char* data = buffer->Buffer();
  if (SUCCEEDED(hr) && data != NULL)
  {
    hr = WriteInternal(data,
                        buffer->Size(),
                        SUCCEEDED(m_asyncAccessResult) && !buffer->IsRetry(),
                        false);
  }

  if (FAILED(hr))
  {
    buffer->IncrementRetryCount();
    if (buffer->CanRetry())
    {
      // A delay may help to avoid the error condition on the next retry, so
      // don't wake the thread here.
      return;
    }
  }
  m_asyncAccessResult = hr;
  delete buffer;
  CEnterCriticalSection lock(m_asyncDataQueueSection);
  m_asyncDataQueue.erase(m_asyncDataQueue.begin());
  if (m_asyncDataQueue.size() == 0)
  {
    m_isAsyncDataQueueFull = false;
  }
  else
  {
    m_asyncAccessThread.Wake();
  }
}
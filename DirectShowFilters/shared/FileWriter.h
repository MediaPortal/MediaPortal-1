/**
*  FileWriter.h
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

#pragma once
#include <cstddef>    // NULL
#include <vector>
#include <WinError.h> // HRESULT
#include "..\shared\CriticalSection.h"
#include "..\shared\Thread.h"

using namespace MediaPortal;
using namespace std;


class FileWriter
{
  public:
    FileWriter();
    virtual ~FileWriter();

    HRESULT GetFileName(wchar_t** fileName);
    HRESULT OpenFile(const wchar_t* fileName);
    HRESULT OpenFile(const wchar_t* fileName, bool isLoggingEnabled);
    HRESULT CloseFile();

    HRESULT Write(const unsigned char* data, unsigned long dataLength);
    HRESULT Write(const unsigned char* data, unsigned long dataLength, bool isErrorLoggingEnabled);

    bool IsFileOpen() const;

    HRESULT SetFilePointer(unsigned long long pointer, bool isErrorLoggingEnabled);
    HRESULT GetFilePointer(unsigned long long& pointer, bool isErrorLoggingEnabled) const;

    void SetReservationConfiguration(unsigned long long reservationChunkSize);

  protected:
    class CWriteBuffer
    {
      public:
        ~CWriteBuffer()
        {
          if (m_buffer != NULL)
          {
            delete[] m_buffer;
            m_buffer = NULL;
          }
        }

        static CWriteBuffer* CreateBuffer(const unsigned char* data,
                                          unsigned long dataSize,
                                          HRESULT& hr)
        {
          return CreateBuffer(data, dataSize, -1, hr);
        }

        static CWriteBuffer* CreateBuffer(long long position, HRESULT& hr)
        {
          return CreateBuffer(NULL, 0, position, hr);
        }

        const unsigned char* Buffer() const
        {
          return m_buffer;
        }

        unsigned long Size() const
        {
          return m_size;
        }

        long long Position(bool& isValid) const
        {
          isValid = m_position != -1;
          return m_position;
        }

        void IncrementRetryCount()
        {
          m_retryCount++;
        }

        bool IsRetry() const
        {
          return m_retryCount != 0;
        }

        bool CanRetry() const
        {
          return m_retryCount < 20;
        }

      private:
        CWriteBuffer()
        {
          m_buffer = NULL;
          m_size = 0;
          m_position = -1;
          m_retryCount = 0;
        }

        static CWriteBuffer* CreateBuffer(const unsigned char* data,
                                          unsigned long dataSize,
                                          long long position,
                                          HRESULT& hr)
        {
          if (
            (dataSize != 0 && data == NULL) ||
            (dataSize == 0 && data != NULL) ||
            (position == -1 && data == NULL && dataSize == 0)
          )
          {
            hr = E_INVALIDARG;
            return NULL;
          }

          CWriteBuffer* buffer = new CWriteBuffer();
          if (buffer == NULL)
          {
            hr = E_OUTOFMEMORY;
            return NULL;
          }

          if (data != NULL)
          {
            buffer->m_buffer = new unsigned char[dataSize];
            if (buffer->m_buffer == NULL)
            {
              hr = E_OUTOFMEMORY;
              delete buffer;
              return NULL;
            }
            memcpy(buffer->m_buffer, data, dataSize);
            buffer->m_size = dataSize;
          }
          buffer->m_position = position;

          hr = S_OK;
          return buffer;
        }

        unsigned char* m_buffer;
        unsigned long m_size;     // unit = bytes
        long long m_position;
        unsigned char m_retryCount;
    };

    HRESULT OpenFile(const wchar_t* fileName, bool isLoggingEnabled, bool isPartFile);
    HRESULT CloseFile(bool isPartFile);

    HRESULT WriteInternal(const unsigned char* data,
                          unsigned long dataLength,
                          bool isErrorLoggingEnabled,
                          bool isRecursive);

    HRESULT GetFilePointerInternal(unsigned long long& pointer, bool isErrorLoggingEnabled) const;
    HRESULT SetFilePointer(long long distanceToMove, DWORD moveMethod, bool isErrorLoggingEnabled);

    static bool __cdecl AsyncThreadFunction(void* arg);
    HRESULT EnqueueBuffer(CWriteBuffer* buffer, bool isErrorLoggingEnabled);
    void WriteNextAsyncBuffer();

    bool m_isFileOpen;
    HANDLE m_fileHandle;
    wchar_t* m_fileName;
    unsigned long m_filePart;                   // For when file size exceeds file system limits (eg. FAT16, FAT32), and the file must be split.

    bool m_useAsyncAccess;
    CThread m_asyncAccessThread;
    HRESULT m_asyncAccessResult;
    unsigned long m_asyncAccessThreadPointlessLoopCount;

    unsigned long long m_asyncDataOffset;
    vector<CWriteBuffer*> m_asyncDataQueue;
    CCriticalSection m_asyncDataQueueSection;
    unsigned long m_asyncDataQueueLengthMaximum;
    bool m_isAsyncDataQueueFull;

    bool m_useReservations;
    unsigned long long m_reservedFileSize;      // unit = bytes
    unsigned long long m_reservationChunkSize;  // unit = bytes
};
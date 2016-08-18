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
#include <Windows.h>  // OVERLAPPED, CloseHandle(), CreateFileW(), SetFilePointer(), SetEndOfFile(), SleepEx(), WriteFile(), WriteFileEx()
#include <WinError.h> // HRESULT


class FileWriter
{
  public:
    FileWriter();
    virtual ~FileWriter();

    HRESULT GetFileName(wchar_t** fileName);
    HRESULT OpenFile(const wchar_t* fileName);
    HRESULT OpenFile(const wchar_t* fileName, bool disableLogging);
    HRESULT CloseFile();

    HRESULT Write(unsigned char* data, unsigned long dataLength);
    HRESULT Write(unsigned char* data, unsigned long dataLength, bool disableLogging);
    HRESULT Write(unsigned char* data,
                  unsigned long dataLength,
                  bool disableLogging,
                  unsigned long long offset);

    bool IsFileInvalid();

    HRESULT SetFilePointer(long long distanceToMove, DWORD moveMethod);
    HRESULT GetFilePointer(unsigned long long& pointer);

    void SetReservationConfiguration(unsigned long long reservationChunkSize);

  protected:
    HRESULT WriteInternal(unsigned char* data,
                          unsigned long dataLength,
                          bool disableLogging,
                          bool isRecursive);
    static void WINAPI AsyncWriteCompleted(DWORD errorCode,
                                            DWORD transferredByteCount,
                                            OVERLAPPED* overlapped);

    HANDLE m_fileHandle;
    wchar_t* m_fileName;
    unsigned char m_filePart;                   // For when file size exceeds file system limits (eg. FAT16, FAT32), and the file must be split.
    bool m_useAsync;
    unsigned long long m_asyncDataOffset;

    bool m_useReservations;
    unsigned long long m_reservedFileSize;      // unit = bytes
    unsigned long long m_reservationChunkSize;  // unit = bytes

    typedef struct OVERLAPPEDEX : public OVERLAPPED
    {
      unsigned long DataLength;
      unsigned char* Data;
    } OVERLAPPEDEX;
};
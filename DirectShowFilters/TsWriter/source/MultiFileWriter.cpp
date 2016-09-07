/**
*  MultiFileWriter.cpp
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
#include "MultiFileWriter.h"
#include <cstddef>      // NULL
#include <cstring>      // memcpy()
#include <cwchar>       // wcscpy(), wcslen(), wcstol()
#include <Windows.h>    // MAX_PATH
#include "FileReader.h"
#include "FileUtils.h"

using namespace std;


#define RESERVED_DISK_SPACE 100000000   // 100 MB


extern void LogDebug(const wchar_t* fmt, ...);
extern bool TsWriterDisableTsBufferReservation();

MultiFileWriter::MultiFileWriter()
{
  m_fileRegister = NULL;
  m_registerFileName = NULL;

  m_fileData = NULL;
  ResetDataFileProperties();

  // Default configuration.
  m_dataFileSizeMaximum = 256000000;   // 256 MB
  m_dataFileReservationChunkSize = m_dataFileSizeMaximum;
  m_dataFileCountMinimum = 6;
  m_dataFileCountMaximum = 20;
}

MultiFileWriter::~MultiFileWriter()
{
  CloseFile();
}

HRESULT MultiFileWriter::GetFileName(wchar_t** fileName)
{
  *fileName = m_registerFileName;
  return S_OK;
}

HRESULT MultiFileWriter::OpenFile(const wchar_t* fileName, bool& resume)
{
  LogDebug(L"multi file writer: open file, name = %s, resume = %d",
            fileName == NULL ? L"" : fileName, resume);

  // Is the file already open?
  if (m_fileRegister != NULL || m_fileData != NULL)
  {
    LogDebug(L"multi file writer: file already open, current = %s, requested = %s",
              m_registerFileName == NULL ? L"" : m_registerFileName,
              fileName == NULL ? L"" : fileName);
    return S_FALSE;
  }

  // If resuming, read the register file which contains the current state.
  HRESULT hr;
  unsigned long dataFileCount = 2;
  if (resume)
  {
    hr = ReadRegisterFile(fileName);

    resume = hr == S_OK;
    if (m_dataFileNames.size() > dataFileCount)
    {
      dataFileCount = 0;
    }
    else if (m_dataFileNames.size() != 0)
    {
      dataFileCount = dataFileCount - (m_dataFileNames.size() - 1);
    }
  }

  // Check disk space. We need to have or be able to create at least 2 data
  // files.
  unsigned long long availableDiskSpace = 0;
  if (
    SUCCEEDED(CFileUtils::GetAvailableDiskSpace(fileName, availableDiskSpace)) &&
    availableDiskSpace < ((m_dataFileSizeMaximum * dataFileCount) + RESERVED_DISK_SPACE)
  )
  {
    LogDebug(L"multi file writer: failed to open file, available disk space = %llu bytes, maximum data file size = %llu bytes, name = %s",
              availableDiskSpace, m_dataFileSizeMaximum,
              fileName == NULL ? L"" : fileName);
    return E_FAIL;
  }

  // Open the register and data files.
  m_fileRegister = new FileWriter();
  if (m_fileRegister == NULL)
  {
    LogDebug(L"multi file writer: failed to allocate register file writer, name = %s",
              fileName == NULL ? L"" : fileName);
    return E_OUTOFMEMORY;
  }
  hr = m_fileRegister->OpenFile(fileName);
  if (FAILED(hr))
  {
    LogDebug(L"multi file writer: failed to open register file, hr = 0x%x, name = %s",
              hr, fileName == NULL ? L"" : fileName);
    delete m_fileRegister;
    return hr;
  }

  m_fileData = new FileWriter();
  if (m_fileData == NULL)
  {
    LogDebug(L"multi file writer: failed to allocate data file writer, name = %s",
              fileName == NULL ? L"" : fileName);
    m_fileRegister->CloseFile();
    delete m_fileRegister;
    m_fileRegister = NULL;
    return E_OUTOFMEMORY;
  }
  if (resume)
  {
    hr = m_fileData->OpenFile(m_dataFileNames[m_dataFileNames.size() - 1]);
    if (FAILED(hr))
    {
      LogDebug(L"multi file writer: failed to reopen last data file, hr = 0x%x, name = %s",
                hr, m_dataFileNames[m_dataFileNames.size() - 1]);
      return hr;
    }

    hr = m_fileData->SetFilePointer(m_dataFileSize, FILE_BEGIN);
    if (FAILED(hr))
    {
      LogDebug(L"multi file writer: failed to set the pointer for the last data file, hr = 0x%x, name = %s",
                hr, m_dataFileNames[m_dataFileNames.size() - 1]);
      return hr;
    }
  }

  unsigned long long reservationChunkSize = m_dataFileReservationChunkSize;
  if (TsWriterDisableTsBufferReservation() || fileName[0] == L'\\')
  {
    reservationChunkSize = 0;
  }
  m_fileData->SetReservationConfiguration(reservationChunkSize);

  // Take a copy of the file name.
  unsigned long fileNameLength = wcslen(fileName);
  if (m_registerFileName != NULL)
  {
    delete[] m_registerFileName;
  }
  m_registerFileName = new wchar_t[fileNameLength + 1];
  if (m_registerFileName == NULL)
  {
    LogDebug(L"multi file writer: failed to allocate %lu bytes for the register file name, name = %s",
              fileNameLength, fileName);
    CloseFile();
    return E_OUTOFMEMORY;
  }
  wcscpy(m_registerFileName, fileName);

  return S_OK;
}

HRESULT MultiFileWriter::CloseFile()
{
  LogDebug(L"multi file writer: close file, name = %s",
            m_registerFileName == NULL ? L"" : m_registerFileName);
  if (m_fileRegister != NULL)
  {
    m_fileRegister->CloseFile();
    delete m_fileRegister;
    m_fileRegister = NULL;
  }
  if (m_registerFileName != NULL)
  {
    delete[] m_registerFileName;
    m_registerFileName = NULL;
  }

  if (m_fileData != NULL)
  {
    m_fileData->CloseFile();
    delete m_fileData;
    m_fileData = NULL;
  }
  ResetDataFileProperties();
  return S_OK;
}

HRESULT MultiFileWriter::Write(unsigned char* data, unsigned long dataLength, bool disableLogging)
{
  //LogDebug(L"multi file writer: write, data length = %lu, name = %s",
  //          dataLength, m_registerFileName == NULL ? L"" : m_registerFileName);

  if (m_fileRegister == NULL || m_fileData == NULL)
  {
    if (!disableLogging)
    {
      LogDebug(L"multi file writer: failed to write to file, file not open");
    }
    return S_FALSE;
  }

  bool isDataFileChanged = false;
  HRESULT hr;
  if (m_fileData->IsFileInvalid())
  {
    hr = OpenDataFile(disableLogging);
    if (FAILED(hr))
    {
      return hr;
    }
    isDataFileChanged = true;
  }

  // Do we need to move to the next data file?
  unsigned long long fileSpace = m_dataFileSizeMaximum - m_dataFileSize;
  if (dataLength > fileSpace)
  {
    // Complete the current file.
    unsigned long dataToWrite = (unsigned long)fileSpace - dataLength;
    if (dataToWrite > 0)
    {
      hr = m_fileData->Write(data, dataToWrite, disableLogging);
      if (FAILED(hr))
      {
        return hr;
      }
      m_dataFileSize += dataToWrite;
    }

    // Start a new file.
    hr = OpenDataFile(disableLogging);
    if (FAILED(hr))
    {
      return hr;
    }

    isDataFileChanged = true;
    data += dataToWrite;
    dataLength -= dataToWrite;
  }

  hr = m_fileData->Write(data, dataLength, disableLogging);
  if (FAILED(hr))
  {
    return hr;
  }

  m_dataFileSize += dataLength;
  return WriteRegisterFile(isDataFileChanged);
}

void MultiFileWriter::GetCurrentFilePosition(unsigned long& currentFileId,
                                              unsigned long long& currentFilePointer)
{
  currentFileId = m_dataFileIdCurrent;
  currentFilePointer = m_dataFileSize;
}

void MultiFileWriter::SetConfiguration(MultiFileWriterParams& parameters)
{
  LogDebug(L"multi file writer: set configuration, maximum file size = %llu (%llu) bytes, reservation chunk size = %llu (%llu) bytes, file count minimum = %lu (%lu), file count maximum = %lu (%lu), name = %s",
            parameters.MaximumFileSize, m_dataFileSizeMaximum,
            parameters.ReservationChunkSize, m_dataFileReservationChunkSize,
            parameters.FileCountMinimum, m_dataFileCountMinimum,
            parameters.FileCountMaximum, m_dataFileCountMaximum,
            m_registerFileName == NULL ? L"" : m_registerFileName);

  m_dataFileSizeMaximum = parameters.MaximumFileSize;
  m_dataFileCountMinimum = parameters.FileCountMinimum;
  m_dataFileCountMaximum = parameters.FileCountMaximum;

  m_dataFileReservationChunkSize = 0;
  if (parameters.ReservationChunkSize != 0)
  {
    if (m_dataFileSizeMaximum % parameters.ReservationChunkSize == 0)
    {
      m_dataFileReservationChunkSize = parameters.ReservationChunkSize;
    }
    else
    {
      LogDebug(L"multi file writer: reservation disabled because chunk size is not a file size factor");
    }
  }
  if (m_fileData != NULL)
  {
    unsigned long long reservationChunkSize = m_dataFileReservationChunkSize;
    if (TsWriterDisableTsBufferReservation() || m_registerFileName[0] == L'\\')
    {
      reservationChunkSize = 0;
    }
    m_fileData->SetReservationConfiguration(reservationChunkSize);
  }
}

HRESULT MultiFileWriter::OpenDataFile(bool disableLogging)
{
  m_fileData->CloseFile();
  m_dataFileSize = 0;

  HRESULT hr;
  unsigned long long availableDiskSpace = 0;
  if (
    SUCCEEDED(CFileUtils::GetAvailableDiskSpace(m_registerFileName, availableDiskSpace)) &&
    availableDiskSpace < (m_dataFileSizeMaximum + RESERVED_DISK_SPACE)
  )
  {
    if (!disableLogging)
    {
      LogDebug(L"multi file writer: not enough available disk space to create new data file, available disk space = %llu bytes, maximum data file size = %llu bytes, name = %s",
                availableDiskSpace, m_dataFileSizeMaximum, m_registerFileName);
    }
    return ReuseDataFile(disableLogging);
  }

  // Build up to the minimum data file count.
  if (m_dataFileNames.size() < m_dataFileCountMinimum) 
  {
    return CreateDataFile(disableLogging);
  }

  // If we already have the desired number of data files, prefer to reuse an
  // existing file. That may not be possible if time-shifting is paused. In
  // that case we try to create a new data file if configuration allows.
  hr = ReuseDataFile(disableLogging);
  if (FAILED(hr) && m_dataFileNames.size() < m_dataFileCountMaximum)
  {
    hr = CreateDataFile(disableLogging);
  }
  return hr;
}

HRESULT MultiFileWriter::CreateDataFile(bool disableLogging)
{
  // Determine the name of the next data file.
  wchar_t* fileName = new wchar_t[MAX_PATH];
  if (fileName == NULL)
  {
    if (!disableLogging)
    {
      LogDebug(L"multi file writer: failed to allocate %lu bytes for a data file name",
                MAX_PATH);
    }
    return E_OUTOFMEMORY;
  }

  do
  {
    swprintf(fileName, L"%s%lu.ts", MAX_PATH, m_registerFileName, m_dataFileIdNext++);
  }
  while (CFileUtils::Exists(fileName));

  // Create the file and update the list of data file names.
  HRESULT hr = m_fileData->OpenFile(fileName, disableLogging);
  if (FAILED(hr))
  {
    if (!disableLogging)
    {
      LogDebug(L"multi file writer: failed to create new data file, hr = 0x%x, name = %s",
                hr, fileName);
    }
    delete[] fileName;
    return hr;
  }

  m_dataFileNames.push_back(fileName);
  m_dataFileCountUsed++;
  m_dataFileIdCurrent = m_dataFileIdNext - 1;
  LogDebug(L"multi file writer: created data file, file count = %llu, name = %s",
            (unsigned long long)m_dataFileNames.size(), fileName);
  return S_OK;
}

HRESULT MultiFileWriter::ReuseDataFile(bool disableLogging)
{
  // Try to reuse the oldest data file. The file may be locked:
  // - temporarily, if TsReader is doing a duration calculation
  // - indefinitely, if time-shifting is paused
  wchar_t* fileName = m_dataFileNames.at(0);
  HRESULT hr;
  unsigned char retryCount = 0;
  for (retryCount = 0; retryCount < 5; retryCount++)
  {
    CFileUtils::DeleteFile(fileName);   // The file may only be marked for deletion.
    hr = m_fileData->OpenFile(fileName, disableLogging);
    if (SUCCEEDED(hr))
    {
      break;
    }
    Sleep(20);
  }

  if (retryCount == 5)
  {
    if (!disableLogging)
    {
      LogDebug(L"multi file writer: failed to reuse data file (is time-shifting paused?), hr = 0x%x, name = %s",
                hr, fileName);
    }
    return hr;
  }

  // Update the list of data file names.
  m_dataFileNames.erase(m_dataFileNames.begin());
  m_dataFileCountRemoved++;

  m_dataFileNames.push_back(fileName);
  m_dataFileCountUsed++;

  m_dataFileIdCurrent = wcstol(fileName + wcslen(m_registerFileName), NULL, 10);
  LogDebug(L"multi file writer: reusing data file, retry count = %hhu, file count = %llu, name = %s",
            retryCount, (unsigned long long)m_dataFileNames.size(), fileName);
  return S_OK;
}

HRESULT MultiFileWriter::ReadRegisterFile(const wchar_t* fileName)
{
  unsigned long readByteCount = REGISTER_FILE_WRITE_BUFFER_SIZE;
  HRESULT hr = FileReader::Read(fileName, m_registerFileWriteBuffer, readByteCount);
  if (hr != S_OK)
  {
    return hr;
  }
  if (
    readByteCount < sizeof(unsigned long long) + (sizeof(unsigned long) * 4) ||
    readByteCount == REGISTER_FILE_WRITE_BUFFER_SIZE
  )
  {
    LogDebug(L"multi file writer: unexpected register file size, size = %lu, name = %s",
              readByteCount, fileName);
    return E_UNEXPECTED;
  }

  unsigned char* readPointer = m_registerFileWriteBuffer;

  m_dataFileSize = *((unsigned long long*)readPointer);
  readPointer += sizeof(unsigned long long);

  m_dataFileCountUsed = *((unsigned long*)readPointer);
  readPointer += sizeof(unsigned long);

  m_dataFileCountRemoved = *((unsigned long*)readPointer);
  readPointer += sizeof(unsigned long);

  m_dataFileIdNext = 1;
  while (true)
  {
    unsigned short fileNameLength = wcslen((wchar_t*)readPointer) + 1;
    if (fileNameLength == 1)
    {
      m_dataFileIdNext++;
      return S_OK;
    }

    wchar_t* dataFileName = new wchar_t[fileNameLength];
    if (dataFileName == NULL)
    {
      LogDebug(L"multi file writer: failed to allocate %hu bytes for a data file name",
                fileNameLength);
      ResetDataFileProperties();
      return E_OUTOFMEMORY;
    }

    wcscpy(dataFileName, (wchar_t*)readPointer);
    m_dataFileNames.push_back(dataFileName);

    if (!CFileUtils::Exists(fileName))
    {
      LogDebug(L"multi file writer: data file missing, name = %s",
                dataFileName);
      ResetDataFileProperties();
      return HRESULT_FROM_WIN32(ERROR_FILE_NOT_FOUND);
    }

    m_dataFileIdCurrent = wcstol(dataFileName + wcslen(fileName), NULL, 10);
    if (m_dataFileIdCurrent > m_dataFileIdNext)
    {
      m_dataFileIdNext = m_dataFileIdCurrent;
    }
    readPointer += (fileNameLength * sizeof(wchar_t));
  }

  // Unreachable.
  return S_FALSE;
}

HRESULT MultiFileWriter::WriteRegisterFile(bool updateFileInfo)
{
  unsigned char* writePointer = m_registerFileWriteBuffer;

  // Current data file content length...
  *((unsigned long long*)writePointer) = m_dataFileSize;
  writePointer += sizeof(unsigned long long);

  if (updateFileInfo)
  {
    // Data file counts...
    *((unsigned long*)writePointer) = m_dataFileCountUsed;
    writePointer += sizeof(unsigned long);

    *((unsigned long*)writePointer) = m_dataFileCountRemoved;
    writePointer += sizeof(unsigned long);

    // Data file names...
    vector<wchar_t*>::const_iterator it = m_dataFileNames.begin();
    for ( ; it < m_dataFileNames.end(); it++)
    {
      wchar_t* fileName = *it;
      if (fileName == NULL)
      {
        continue;
      }

      unsigned long length = wcslen(fileName) + 1;
      length *= sizeof(wchar_t);
      memcpy(writePointer, fileName, length);
      writePointer += length;

      if (writePointer - m_registerFileWriteBuffer > REGISTER_FILE_WRITE_BUFFER_SIZE - MAX_PATH)
      {
        LogDebug(L"multi file writer: failed to write register file, data file count = %llu",
                  (unsigned long long)m_dataFileNames.size());
        return S_FALSE;
      }
    }

    // Mark the end of the content with a Unicode null character.
    *((wchar_t*)writePointer) = NULL;
    writePointer += sizeof(wchar_t);

    // For backwards compatibility with TsReader's multi file reader...
    *((unsigned long*)writePointer) = m_dataFileCountUsed;
    writePointer += sizeof(unsigned long);

    *((unsigned long*)writePointer) = m_dataFileCountRemoved;
    writePointer += sizeof(unsigned long);
  }

  return m_fileRegister->Write(m_registerFileWriteBuffer,
                                writePointer - m_registerFileWriteBuffer,
                                false,
                                0);
}

void MultiFileWriter::ResetDataFileProperties()
{
  m_dataFileSize = 0;

  m_dataFileIdCurrent = 0;
  m_dataFileIdNext = 1;

  m_dataFileCountUsed = 0;
  m_dataFileCountRemoved = 0;

  for (vector<wchar_t*>::iterator it = m_dataFileNames.begin(); it < m_dataFileNames.end(); it++)
  {
    wchar_t* fileName = *it;
    if (fileName != NULL)
    {
      delete[] fileName;
      *it = NULL;
    }
  }
  m_dataFileNames.clear();
}
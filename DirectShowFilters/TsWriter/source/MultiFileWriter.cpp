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
#include <cwchar>       // wcslen(), wcsncpy(), wcsrchr(), wcstoul()
#include <sstream>
#include <string>
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
    hr = m_fileData->OpenFile(m_dataFileNames.back());
    if (FAILED(hr))
    {
      LogDebug(L"multi file writer: failed to reopen previous data file, hr = 0x%x, name = %s",
                hr, m_dataFileNames.back());
      CloseFile();
      return hr;
    }

    hr = m_fileData->SetFilePointer(m_dataFileSize, true);
    if (FAILED(hr))
    {
      LogDebug(L"multi file writer: failed to resume previous data file, hr = 0x%x, size = %llu, name = %s",
                hr, m_dataFileSize, m_dataFileNames.back());
      CloseFile();
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
  wcsncpy(m_registerFileName, fileName, fileNameLength + 1);

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

HRESULT MultiFileWriter::Write(unsigned char* data,
                                unsigned long dataLength,
                                bool isErrorLoggingEnabled)
{
  //LogDebug(L"multi file writer: write, data length = %lu, name = %s",
  //          dataLength, m_registerFileName == NULL ? L"" : m_registerFileName);

  if (m_fileRegister == NULL || m_fileData == NULL)
  {
    if (isErrorLoggingEnabled)
    {
      LogDebug(L"multi file writer: failed to write to file, file not open");
    }
    return S_FALSE;
  }

  bool isDataFileChanged = false;
  HRESULT hr;
  if (!m_fileData->IsFileOpen())
  {
    hr = OpenDataFile(isErrorLoggingEnabled);
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
      hr = m_fileData->Write(data, dataToWrite, isErrorLoggingEnabled);
      if (FAILED(hr))
      {
        return hr;
      }
      m_dataFileSize += dataToWrite;
    }

    // Start a new file.
    hr = OpenDataFile(isErrorLoggingEnabled);
    if (FAILED(hr))
    {
      return hr;
    }

    isDataFileChanged = true;
    data += dataToWrite;
    dataLength -= dataToWrite;
  }

  hr = m_fileData->Write(data, dataLength, isErrorLoggingEnabled);
  if (FAILED(hr))
  {
    return hr;
  }

  m_dataFileSize += dataLength;
  hr = WriteRegisterFile(isDataFileChanged);
  if (hr != S_OK)
  {
    // If updating the register file fails (which it shouldn't, unless
    // something is seriously ***seriously*** wrong!), indicate partial success
    // and don't retry.
    return S_FALSE;
  }
  return S_OK;
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
  m_dataFileReservationChunkSize = parameters.ReservationChunkSize;
  m_dataFileCountMinimum = parameters.FileCountMinimum;
  m_dataFileCountMaximum = parameters.FileCountMaximum;

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

HRESULT MultiFileWriter::OpenDataFile(bool isErrorLoggingEnabled)
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
    if (isErrorLoggingEnabled)
    {
      LogDebug(L"multi file writer: not enough available disk space to create new data file, available disk space = %llu bytes, maximum data file size = %llu bytes, name = %s",
                availableDiskSpace, m_dataFileSizeMaximum, m_registerFileName);
    }
    return ReuseDataFile(isErrorLoggingEnabled);
  }

  // Build up to the minimum data file count.
  if (m_dataFileNames.size() < m_dataFileCountMinimum) 
  {
    return CreateDataFile(isErrorLoggingEnabled);
  }

  // If we already have the desired number of data files, prefer to reuse an
  // existing file. That may not be possible if time-shifting is paused. In
  // that case we try to create a new data file if configuration allows.
  hr = ReuseDataFile(isErrorLoggingEnabled);
  if (FAILED(hr) && m_dataFileNames.size() < m_dataFileCountMaximum)
  {
    hr = CreateDataFile(isErrorLoggingEnabled);
  }
  return hr;
}

HRESULT MultiFileWriter::CreateDataFile(bool isErrorLoggingEnabled)
{
  // Determine the name of the next data file.
  wstring baseFileName(m_registerFileName);
  // TODO uncomment when TsReader doesn't recognise data files by .ts.tsbuffer
  //baseFileName = baseFileName.substr(0, baseFileName.find_last_of(L".")).append(L".");
  wstring fileNameString;
  do
  {
    wstringstream fileNameStream;
    fileNameStream << baseFileName << m_dataFileIdNext++ << L".ts";
    fileNameString = fileNameStream.str();
  }
  while (CFileUtils::Exists(fileNameString.c_str()));
  wchar_t* fileName = new wchar_t[fileNameString.size() + 1];
  if (fileName == NULL)
  {
    if (isErrorLoggingEnabled)
    {
      LogDebug(L"multi file writer: failed to allocate %llu bytes for a data file name",
                (unsigned long long)fileNameString.size() + 1);
    }
    return E_OUTOFMEMORY;
  }
  wcsncpy(fileName, fileNameString.c_str(), fileNameString.size() + 1);

  // Create the file and update the list of data file names.
  HRESULT hr = m_fileData->OpenFile(fileName, isErrorLoggingEnabled);
  if (FAILED(hr))
  {
    if (isErrorLoggingEnabled)
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

HRESULT MultiFileWriter::ReuseDataFile(bool isErrorLoggingEnabled)
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
    hr = m_fileData->OpenFile(fileName, isErrorLoggingEnabled);
    if (SUCCEEDED(hr))
    {
      break;
    }
    Sleep(20);
  }

  if (retryCount == 5)
  {
    if (isErrorLoggingEnabled)
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

  // Determine the file's ID from its name.
  unsigned long fileIdPosition = wcslen(m_registerFileName) + 1;
  wchar_t* fileExtension = wcsrchr(m_registerFileName, L'.');
  if (fileExtension != NULL)
  {
    fileIdPosition = (fileExtension - m_registerFileName) + 1;
  }
  // TODO remove the next line when TsReader doesn't recognise data files by .ts.tsbuffer
  fileIdPosition = wcslen(m_registerFileName);
  m_dataFileIdCurrent = wcstoul(fileName + fileIdPosition, NULL, 10);

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

  unsigned long fileIdPosition = wcslen(fileName) + 1;
  const wchar_t* fileExtension = wcsrchr(fileName, L'.');
  if (fileExtension != NULL)
  {
    fileIdPosition = (fileExtension - fileName) + 1;
  }
  // TODO remove the next line when TsReader doesn't recognise data files by .ts.tsbuffer
  fileIdPosition = wcslen(fileName);

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

    wcsncpy(dataFileName, (wchar_t*)readPointer, fileNameLength);
    m_dataFileNames.push_back(dataFileName);

    if (!CFileUtils::Exists(fileName))
    {
      LogDebug(L"multi file writer: data file missing, name = %s",
                dataFileName);
      ResetDataFileProperties();
      return HRESULT_FROM_WIN32(ERROR_FILE_NOT_FOUND);
    }

    m_dataFileIdCurrent = wcstoul(dataFileName + fileIdPosition, NULL, 10);
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

      if (writePointer - m_registerFileWriteBuffer > (unsigned long long)REGISTER_FILE_WRITE_BUFFER_SIZE - length)
      {
        LogDebug(L"multi file writer: failed to write register file, data file count = %llu",
                  (unsigned long long)m_dataFileNames.size());
        return E_OUTOFMEMORY;
      }

      memcpy(writePointer, fileName, length);
      writePointer += length;
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

  HRESULT hr = m_fileRegister->SetFilePointer(0, true);
  if (SUCCEEDED(hr))
  {
    hr = m_fileRegister->Write(m_registerFileWriteBuffer,
                                writePointer - m_registerFileWriteBuffer,
                                true);
  }
  return hr;
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
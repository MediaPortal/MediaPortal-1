/**
*  MultiFileWriter.h
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
#include <vector>
#include <WinError.h> // HRESULT
#include "..\..\shared\FileWriter.h"


#define REGISTER_FILE_WRITE_BUFFER_SIZE 65536


typedef struct MultiFileWriterParams
{
  unsigned long long MaximumFileSize;       // unit = bytes
  unsigned long long ReservationChunkSize;  // unit = bytes; must be a factor for MaximumFileSize
  unsigned long FileCountMinimum;
  unsigned long FileCountMaximum;
} MultiFileWriterParams;

class MultiFileWriter
{
  public:
    MultiFileWriter();
    virtual ~MultiFileWriter();

    HRESULT GetFileName(wchar_t** fileName);
    HRESULT OpenFile(const wchar_t* fileName, bool& resume);
    HRESULT CloseFile();
  
    HRESULT Write(unsigned char* data, unsigned long dataLength, bool disableLogging);

    void GetCurrentFilePosition(unsigned long& currentFileId,
                                unsigned long long& currentFilePointer);
    void SetConfiguration(MultiFileWriterParams& parameters);

  protected:
    HRESULT OpenDataFile(bool disableLogging);
    HRESULT CreateDataFile(bool disableLogging);
    HRESULT ReuseDataFile(bool disableLogging);

    HRESULT ReadRegisterFile(const wchar_t* fileName);
    HRESULT WriteRegisterFile(bool updateFileInfo);
    void ResetDataFileProperties();

    FileWriter* m_fileRegister;
    wchar_t* m_registerFileName;
    unsigned char m_registerFileWriteBuffer[REGISTER_FILE_WRITE_BUFFER_SIZE];

    FileWriter* m_fileData;
    std::vector<wchar_t*> m_dataFileNames;

    unsigned long long m_dataFileSize;                  // unit = bytes
    unsigned long long m_dataFileSizeMaximum;           // unit = bytes
    unsigned long long m_dataFileReservationChunkSize;  // unit = bytes

    unsigned long m_dataFileIdCurrent;
    unsigned long m_dataFileIdNext;

    unsigned long m_dataFileCountMinimum;
    unsigned long m_dataFileCountMaximum;

    unsigned long m_dataFileCountUsed;
    unsigned long m_dataFileCountRemoved;
};
/*
    Copyright (C) 2007-2010 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2.  If not, see <http://www.gnu.org/licenses/>.
*/

#ifndef __MPIPTV_FILE_DEFINE_DEFINED
#define __MPIPTV_FILE_DEFINE_DEFINED

#include "MPIPTV_FILE_Exports.h"
#include "Logger.h"
#include "ProtocolInterface.h"
#include "LinearBuffer.h"

// we should get data in two seconds
#define FILE_RECEIVE_DATA_TIMEOUT_DEFAULT                   2000
// repeat reading file forever
#define REPEAT_FOREVER                                      0
#define REPEAT_LIMIT_DEFAULT                                REPEAT_FOREVER
#define FILE_INTERNAL_BUFFER_MULTIPLIER_DEFAULT             8
#define FILE_OPEN_CONNECTION_MAXIMUM_ATTEMPTS_DEFAULT       3

#define CONFIGURATION_SECTION_FILE                          _T("FILE")

#define CONFIGURATION_FILE_RECEIVE_DATA_TIMEOUT             _T("FileReceiveDataTimeout")
#define CONFIGURATION_FILE_REPEAT_LIMIT                     _T("FileRepeatLimit")
#define CONFIGURATION_FILE_INTERNAL_BUFFER_MULTIPLIER       _T("FileInternalBufferMultiplier")
#define CONFIGURATION_FILE_OPEN_CONNECTION_MAXIMUM_ATTEMPTS _T("FileOpenConnectionMaximumAttempts")

// returns protocol class instance
PIProtocol CreateProtocolInstance(void);

// destroys protocol class instance
void DestroyProtocolInstance(PIProtocol pProtocol);

// This class is exported from the MPIPTV_FILE.dll
class MPIPTV_FILE_API CMPIPTV_FILE : public IProtocol
{
public:
  // constructor
  // create instance of CMPITV_FILE class
  CMPIPTV_FILE(void);

  // destructor
  ~CMPIPTV_FILE(void);

  /* IProtocol interface */
  TCHAR *GetProtocolName(void);
  int Initialize(HANDLE lockMutex, CParameterCollection *configuration);
  int ClearSession(void);
  int ParseUrl(const TCHAR *url, const CParameterCollection *parameters);
  int OpenConnection(void);
  int IsConnected(void);
  void CloseConnection(void);
  void GetSafeBufferSizes(HANDLE lockMutex, unsigned int *freeSpace, unsigned int *occupiedSpace, unsigned int *bufferSize);
  void ReceiveData(bool *shouldExit);
  unsigned int FillBuffer(IMediaSample *pSamp, char *pData, long cbData);
  unsigned int GetReceiveDataTimeout(void);
  GUID GetInstanceId(void);
  unsigned int GetOpenConnectionMaximumAttempts(void);

protected:
  CLogger logger;

  // holds file path
  TCHAR *filePath;
  // holds file
  FILE *fileStream;

  HANDLE lockMutex;

  char *receiveBuffer;    // internal receive buffer - must be long enough to not lost data (especially for UDP)
  LinearBuffer buffer;    // internal buffer which is used for storing data and sending to MediaPortal

  // holds various parameters supplied by TvService
  CParameterCollection *configurationParameters;
  // holds various parameters supplied by TvService when loading file
  CParameterCollection *loadParameters;

  // holds default buffer size
  unsigned int defaultBufferSize;

  // holds receive data timeout
  unsigned int receiveDataTimeout;

  // holds number of readings of file
  unsigned int repeatCount;

  // holds repeat limit of reading file
  unsigned int repeatLimit;

  // holds open connection maximum attempts
  unsigned int openConnetionMaximumAttempts;

  // holds end of file log message
  unsigned int endOfFileReachedLogged;

  // specifies if input packets have to be dumped to file
  bool dumpInputPackets;
};

#endif

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

#pragma once

#ifndef __UTILITIES_DEFINED
#define __UTILITIES_DEFINED

#include "MPIPTVSourceExports.h"
#include "ParameterCollection.h"
#include "Logger.h"
#include "LinearBuffer.h"

#include <streams.h>

// get Tv Server folder
// @return : reference to null terminated string with path of Tv Server ended with '\' or NULL if error occured
MPIPTVSOURCE_API TCHAR *GetTvServerFolder(void);

// get path to file in Tv Server folder
// Tv Server folder always ended with '\'
// @param filePath : the file path in Tv Server folder
// @return : reference to null terminated string with path of file or NULL if error occured
MPIPTVSOURCE_API TCHAR *GetTvServerFilePath(const TCHAR *filePath);

// get parameters from configuration file
// @param logger : logger for logging purposes or NULL
// @param moduleName : the name of module calling GetConfiguration(), ignored if logger is NULL
// @param functionName : the name of function calling GetConfiguration(), ignored if logger is NULL
// @param section : the section name from configuration file
// @return : the collection of configuration parameters
MPIPTVSOURCE_API CParameterCollection *GetConfiguration(CLogger *logger, const TCHAR *moduleName, const TCHAR *functionName, const TCHAR *section);

// common implementation of FillBuffer() method
// @param logger : logger for logging purposes
// @param protocolName : name of protocol calling FillBufferStandard()
// @param functionName : name of function calling FillBufferStandard()
// @param lockMutex : mutex to lock access to buffer
// @param buffer : linear buffer
// @param pSamp :
// @param pData :
// @param cbData :
// @return: count of bytes written to media sample
MPIPTVSOURCE_API unsigned int FillBufferStandard(CLogger *logger, const TCHAR *protocolName, const TCHAR *functionName, HANDLE lockMutex, LinearBuffer *buffer, IMediaSample *pSamp, char *pData, long cbData);

#endif

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

#include "StdAfx.h"

#include "Utilities.h"
#include "LinearBuffer.h"
#include "ProtocolInterface.h"

#include <tchar.h>
#include <ObjBase.h>
#include <ShlObj.h>
#include <stdio.h>

MPIPTVSOURCE_API TCHAR *GetTvServerFolder(void)
{
  return GetTvServerFilePath(NULL);
}

MPIPTVSOURCE_API TCHAR *GetTvServerFilePath(const TCHAR *filePath)
{
  TCHAR *result = NULL;
  ALLOC_MEM_DEFINE_SET(folder, TCHAR, MAX_PATH, 0);
  if (folder != NULL)
  {
    // get common application data folder
    SHGetSpecialFolderPath(NULL, folder, CSIDL_COMMON_APPDATA, FALSE);

    if (filePath == NULL)
    {
      result = FormatString(_T("%s\\Team MediaPortal\\MediaPortal TV Server\\"), folder);
    }
    else
    {
      result = FormatString(_T("%s\\Team MediaPortal\\MediaPortal TV Server\\%s"), folder, filePath);
    }
  }

  FREE_MEM(folder);

  return result;
}

TCHAR *GetString(const TCHAR *string, DWORD length, DWORD part)
{
  TCHAR *result = NULL;
  DWORD processed = 0;
  DWORD count = 0;
  TCHAR *buffer = (TCHAR *)string;
  
  while (processed < length)
  {
    if (count == part)
    {
      result = buffer;
      break;
    }

    processed += _tcslen(buffer) + 1;
    buffer = (TCHAR *)(string + processed);
    count++;
  }

  return result;
}

DWORD GetStringCount(const TCHAR *string, DWORD length)
{
  DWORD result = 0;
  DWORD processed = 0;
  TCHAR *buffer = (TCHAR *)string;
  
  while (processed < length)
  {
    processed += _tcslen(buffer) + 1;
    buffer = (TCHAR *)(string + processed);
    result++;
  }

  return result;
}

CParameterCollection *GetConfiguration(CLogger *logger, const TCHAR *moduleName, const TCHAR *functionName, const TCHAR *section)
{
  CParameterCollection *collection = new CParameterCollection();
  TCHAR *configurationFile = GetTvServerFilePath(_T("MPIPTVSource.ini"));
  if (configurationFile != NULL)
  {
    if (logger != NULL)
    {
      logger->Log(LOGGER_INFO, _T("%s: %s: loading configuration file: %s, section: %s"), moduleName, functionName, configurationFile, section);
    }

    LARGE_INTEGER size;
    size.QuadPart = 0;
    HANDLE hConfigFile = CreateFile(configurationFile, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);

    if (hConfigFile != INVALID_HANDLE_VALUE)
    {
      if (GetFileSizeEx(hConfigFile, &size))
      {
        ALLOC_MEM_DEFINE_SET(keys, TCHAR, size.LowPart, 0);

        if (keys != NULL)
        {
          DWORD keysLength = GetPrivateProfileString(section, NULL, NULL, keys, size.LowPart, configurationFile);
          DWORD keysCount = GetStringCount(keys, keysLength);

          for (DWORD i = 0; i < keysCount; i++)
          {
            TCHAR *keyName = GetString(keys, keysLength, i);

            ALLOC_MEM_DEFINE_SET(keyValue, TCHAR, size.LowPart, 0);
            if (keyValue != NULL)
            {
              DWORD keyValueLength = GetPrivateProfileString(section, keyName, NULL, keyValue, size.LowPart, configurationFile);

              CParameter *parameter = new CParameter(keyName, keyValue);
              collection->Add(parameter);

              FREE_MEM(keyValue);
            }
          }

          FREE_MEM(keys);
        }
      }

      CloseHandle(hConfigFile);
      hConfigFile = INVALID_HANDLE_VALUE;
    }
    else
    {
      if (logger != NULL)
      {
        logger->Log(LOGGER_WARNING, METHOD_MESSAGE_FORMAT, moduleName, functionName, _T("configuration file not loaded, using default values"));
      }
    }

    FREE_MEM(configurationFile);
  }

  return collection;
}

MPIPTVSOURCE_API unsigned int FillBufferStandard(CLogger *logger, const TCHAR *protocolName, const TCHAR *functionName, HANDLE lockMutex, LinearBuffer *buffer, IMediaSample *pSamp, char *pData, long cbData)
{
  unsigned int lengthOfPostableData = 0;

  WaitForSingleObject(lockMutex, INFINITE);

  // by specification (http://msdn.microsoft.com/en-us/library/s3f49ktz%28v=VS.100%29.aspx) is int and long same data types
  lengthOfPostableData = min(buffer->GetBufferOccupiedSpace(), (unsigned int)cbData);
  logger->Log(LOGGER_DATA, _T("%s: %s: posting %u from %u (occupied buffer size) / %u (sample size) bytes"), protocolName, functionName, lengthOfPostableData, buffer->GetBufferOccupiedSpace(), pSamp->GetSize());
  logger->Log(LOGGER_DATA, _T("%s: %s: length of postable data: %u, buffer occupied space: %u"), protocolName, functionName, lengthOfPostableData, buffer->GetBufferOccupiedSpace());

  int postedData = 0;
  if (lengthOfPostableData > 0)
  {
    // copy data from buffer
    postedData = buffer->CopyFromBuffer(pData, lengthOfPostableData, DVB_PACKET_SIZE);
    logger->Log(LOGGER_DATA, _T("%s: %s: posted data: %u"), protocolName, functionName, postedData);

    // remove data from buffer
    if (buffer->GetBufferFreeSpace() <= (buffer->GetBufferSize() / 2))
    {
      logger->Log(LOGGER_DATA, _T("%s: %s: after copy move remaining data"), protocolName, functionName);

      // if free space is lower than half of buffer
      // remove data from buffer and move remaining data
      buffer->RemoveFromBufferAndMove(postedData);
    }
    else
    {
      // only remove from buffer
      buffer->RemoveFromBuffer(postedData);
    }

    pSamp->SetActualDataLength(postedData);
  }

  // On an udp stream we cannot see if there was a packet lost, nor if this is a synchron point nor the timestamps. 
  // So, just leave it like it is and let the other filter decide
  logger->Log(LOGGER_DATA, _T("%s: %s: buffer occupied space: %u"), protocolName, functionName, buffer->GetBufferOccupiedSpace());
  ReleaseMutex(lockMutex);

  return postedData;
}

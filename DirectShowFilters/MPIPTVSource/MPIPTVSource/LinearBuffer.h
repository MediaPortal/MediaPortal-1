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

#ifndef __LINEARBUFFER_DEFINED
#define __LINEARBUFFER_DEFINED

#include "Logger.h"
#include "MPIPTVSourceExports.h"

// represents linear buffer
// this class is not thread safe !!!
class MPIPTVSOURCE_API LinearBuffer
{
public:
  // create linear buffer
  LinearBuffer(void);

  // create linear buffer with specified size
  LinearBuffer(unsigned int size);

  // destructor
  ~LinearBuffer(void);

  // get free space in buffer
  // @return : free space in buffer
  unsigned int GetBufferFreeSpace(void);

  // get occupied space in buffer
  // @return : occupied space in buffer
  unsigned int GetBufferOccupiedSpace(void);

  // get total size of buffer
  // @return : buffer size
  unsigned int GetBufferSize(void);

  // add data from source to buffer
  // @param source : the reference to source to copy data
  // @param length : the length of data to copy
  // @return : count of added data
  unsigned int AddToBuffer(char* source, unsigned int length);

  // remove from buffer
  // @param length : the length of data to remove from buffer
  void RemoveFromBuffer(unsigned int length);

  // remove from buffer and moves remaining data to start of buffer
  // @param length : the length of data to remove from buffer
  void RemoveFromBufferAndMove(unsigned int length);

  // copy data from internal buffer to destination, data are copied by packets if specified packetSize
  // @param destination : the reference to destination buffer to copy data
  // @param length : the length of data to copy
  // @param packetSize : the packet size (if specified, data are copied by blocks)
  // @return : length of copied data
  unsigned int CopyFromBuffer(char* destination, unsigned int length, unsigned int packetSize);

  // copy data from internal buffer to destination starting from start, data are copied by packets if specified packetSize
  // @param destination : the reference to destination buffer to copy data
  // @param length : the length of data to copy
  // @param packetSize : the packet size (if specified, data are copied by blocks)
  // @param start : the position from where copying start
  // @return : length of copied data
  unsigned int CopyFromBuffer(char* destination, unsigned int length, unsigned int packetSize, unsigned int start);

  // initialize buffer to specified size
  // @param size : required size of buffer
  // @return : true if successful, false otherwise
  bool InitializeBuffer(unsigned int size);

  // clear buffer content
  void ClearBuffer(void);

  // delete buffer
  void DeleteBuffer(void);

  // get the first position of character 'c' in buffer searching from 'start'
  // @param start : the position from where searching start
  // @param c : the character to find
  // @return : the first position of character or UINT_MAX if not found
  unsigned int GetFirstPosition(unsigned int start, char c);

  // resize buffer to new size
  // @param size : new size of buffer
  // @return : true if successful, false otherwise
  bool ResizeBuffer(unsigned int size);

  // safely resize buffer to new size
  // @param lockMutex : mutex to lock access to buffer
  // @param size : new size of buffer
  // @param logMessage : specifies if log message have to be generated to log file
  // @param logger : logger for logging purposes
  // @param protocolName : name of protocol calling SafeResizeBuffer()
  // @param functionName : name of function calling SafeResizeBuffer()
  // @return : true if successful, false otherwise
  bool SafeResizeBuffer(HANDLE lockMutex, unsigned int size, bool logMessage, CLogger *logger, const TCHAR *protocolName, const TCHAR *functionName);

private:
  // internal buffer
  char* buffer;

  // specifies where data starts
  char* dataStart;

  // specifies where data ends
  char* dataEnd;

  // internal buffer size
  unsigned int bufferSize;
};

#endif

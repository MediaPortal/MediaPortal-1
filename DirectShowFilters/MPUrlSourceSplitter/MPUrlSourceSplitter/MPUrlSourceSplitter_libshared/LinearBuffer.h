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

#ifndef __LINEAR_BUFFER_DEFINED
#define __LINEAR_BUFFER_DEFINED

// represents linear buffer
// this class is not thread safe !!!
class CLinearBuffer
{
public:
  // create linear buffer
  CLinearBuffer(HRESULT *result);

  // create linear buffer with specified size
  CLinearBuffer(HRESULT *result, unsigned int size);

  // destructor
  ~CLinearBuffer(void);

  // gets reference to internal buffer
  // can be used for direct buffer manipulation
  // @return : reference to internal buffer or NULL if error
  unsigned char *GetInternalBuffer(void);

  // deeply clones current instance of linear buffer
  // @return : deep clone of current instance or NULL if error
  CLinearBuffer *Clone(void);

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
  unsigned int AddToBuffer(const unsigned char *source, unsigned int length);

  // add data from source to buffer and resize buffer if necessary to fit added data
  // @param source : the reference to source to copy data
  // @param length : the length of data to copy
  // @return : count of added data
  unsigned int AddToBufferWithResize(const unsigned char *source, unsigned int length);

  // add data from source to buffer and resize buffer if necessary to fit added data
  // @param source : the reference to source to copy data
  // @param length : the length of data to copy
  // @param minBufferSize : the minimum buffer size after resizing (if necessary)
  // @return : count of added data
  unsigned int AddToBufferWithResize(const unsigned char *source, unsigned int length, unsigned int minBufferSize);

  // add content of linear buffer to current buffer
  // @param buffer : the linear buffer to add content
  // @return : count of added data
  unsigned int AddToBufferWithResize(CLinearBuffer *buffer);

  // add content of linear buffer to current buffer
  // @param buffer : the linear buffer to add content
  // @param minBufferSize : the minimum buffer size after resizing (if necessary)
  // @return : count of added data
  unsigned int AddToBufferWithResize(CLinearBuffer *buffer, unsigned int minBufferSize);

  // add content of linear buffer to current buffer
  // @param buffer : the linear buffer to add content
  // @param start : the start position in buffer parameter to add data
  // @param length : the count of data to add
  // @return : count of added data
  unsigned int AddToBufferWithResize(CLinearBuffer *buffer, unsigned int start, unsigned int length);

  // add content of linear buffer to current buffer
  // @param buffer : the linear buffer to add content
  // @param start : the start position in buffer parameter to add data
  // @param length : the count of data to add
  // @param minBufferSize : the minimum buffer size after resizing (if necessary)
  // @return : count of added data
  unsigned int AddToBufferWithResize(CLinearBuffer *buffer, unsigned int start, unsigned int length, unsigned int minBufferSize);

  // remove from buffer
  // @param length : the length of data to remove from buffer
  void RemoveFromBuffer(unsigned int length);

  // remove from buffer and moves remaining data to start of buffer
  // @param length : the length of data to remove from buffer
  void RemoveFromBufferAndMove(unsigned int length);

  // remove from buffer counted from end
  // @param length : the length of data to remove from buffer
  void RemoveFromBufferEnd(unsigned int length);

  // copy data from internal buffer to destination, data are copied by packets if specified packetSize
  // @param destination : the reference to destination buffer to copy data
  // @param length : the length of data to copy
  // @return : length of copied data
  unsigned int CopyFromBuffer(unsigned char *destination, unsigned int length);

  // copy data from internal buffer to destination, data are copied by packets if specified packetSize
  // @param destination : the reference to destination buffer to copy data
  // @param length : the length of data to copy
  // @param start : the position from where copying start
  // @return : length of copied data
  unsigned int CopyFromBuffer(unsigned char *destination, unsigned int length, unsigned int start);

  // initialize buffer to specified size
  // @param size : required size of buffer
  // @return : true if successful, false otherwise
  bool InitializeBuffer(unsigned int size);

  // initialize buffer to specified size and set whole buffer with specified value
  // @param size : required size of buffer
  // @param value : value to set in buffer
  // @return : true if successful, false otherwise
  bool InitializeBuffer(unsigned int size, char value);

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

  // compares data in linear buffer with specified buffer
  // @param buffer : the buffer to compare with linear buffer
  // @param length : the length of buffer to compare
  // @return : true if data are same, false otherwise
  bool CompareBuffer(const unsigned char *buffer, unsigned int length);

  // compares data in linear buffer with specified buffer
  // @param buffer : the buffer to compare with linear buffer
  // @param length : the length of buffer to compare
  // @param start : the position in linear buffer to start comparing
  // @return : true if data are same, false otherwise
  bool CompareBuffer(const unsigned char *buffer, unsigned int length, unsigned int start);

  // compares data in linear buffer with specified linear buffer
  // @param buffer : the linear buffer to compare with linear buffer
  // @return : true if data are same, false otherwise
  bool CompareBuffer(CLinearBuffer *buffer);

  // compares data in linear buffer with specified linear buffer
  // @param buffer : the buffer to compare with linear buffer
  // @param start : the position in specified linear buffer to start comparing
  // @return : true if data are same, false otherwise
  bool CompareBuffer(CLinearBuffer *buffer, unsigned int start);

private:
  // internal buffer
  unsigned char *buffer;

  // specifies where data starts
  unsigned char *dataStart;

  // specifies where data ends
  unsigned char *dataEnd;

  // internal buffer size
  unsigned int bufferSize;
};

#endif

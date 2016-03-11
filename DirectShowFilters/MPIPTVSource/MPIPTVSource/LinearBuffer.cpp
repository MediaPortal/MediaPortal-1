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

#include "stdafx.h"

#include "LinearBuffer.h"

LinearBuffer::LinearBuffer(void)
{
  this->buffer = NULL;
  this->DeleteBuffer();
}

LinearBuffer::LinearBuffer(unsigned int size)
{
  this->buffer = NULL;
  this->DeleteBuffer();

  // create internal buffer
  this->InitializeBuffer(size);
}

LinearBuffer::~LinearBuffer(void)
{
  this->DeleteBuffer();
}

bool LinearBuffer::InitializeBuffer(unsigned int size)
{
  // remove current buffer (if any)
  this->DeleteBuffer();

  this->buffer = ALLOC_MEM(char, size);

  if (this->buffer != NULL)
  {
    this->bufferSize = size;
    this->ClearBuffer();
  }

  return (this->buffer != NULL);
}

void LinearBuffer::ClearBuffer(void)
{
  this->dataStart = this->buffer;
  this->dataEnd = this->buffer;
}

void LinearBuffer::DeleteBuffer(void)
{
  FREE_MEM(this->buffer);
  this->bufferSize = 0;
  this->ClearBuffer();
}

unsigned int LinearBuffer::GetBufferSize()
{
  return this->bufferSize;
}

unsigned int LinearBuffer::GetBufferFreeSpace()
{
  return ((long)this->bufferSize - (long)this->dataEnd + (long)this->buffer);
}

unsigned int LinearBuffer::GetBufferOccupiedSpace(void)
{
  return ((long)this->dataEnd - (long)this->dataStart);
}

void LinearBuffer::RemoveFromBuffer(unsigned int length)
{
  // the length to remove from buffer cannot be greater than occupied space
  length = min(length, this->GetBufferOccupiedSpace());

  if (length > 0)
  {
    if (length == this->GetBufferOccupiedSpace())
    {
      // removing all data from buffer
      this->ClearBuffer();
    }
    else
    {
      this->dataStart += length;
    }
  }
}

void LinearBuffer::RemoveFromBufferAndMove(unsigned int length)
{
  // the length to remove from buffer cannot be greater than occupied space
  length = min(length, this->GetBufferOccupiedSpace());

  if (length > 0)
  {
    if (length == this->GetBufferOccupiedSpace())
    {
      // removing all data from buffer
      this->ClearBuffer();
    }
    else
    {
      this->dataStart += length;

      int remainingDataLength = this->GetBufferOccupiedSpace();
      memcpy(this->buffer, this->dataStart, remainingDataLength);
      this->dataStart = this->buffer;
      this->dataEnd = this->buffer + remainingDataLength;
    }
  }
}

unsigned int LinearBuffer::AddToBuffer(char* source, unsigned int length)
{
  unsigned int returnValue = 0;
  if ((length > 0) && (length <= this->GetBufferFreeSpace()))
  {
    memcpy(this->dataEnd, source, length);
    this->dataEnd += length;
    returnValue = length;
  }

  return returnValue;
}

unsigned int LinearBuffer::CopyFromBuffer(char* destination, unsigned int length, unsigned int packetSize)
{
  return this->CopyFromBuffer(destination, length, packetSize, 0);
}

unsigned int LinearBuffer::CopyFromBuffer(char* destination, unsigned int length, unsigned int packetSize, unsigned int start)
{
  int copiedBytes = 0;

  // length cannot be greater than buffer occupied space
  length = min(length, this->GetBufferOccupiedSpace() - start);
  if (length > 0)
  {
    if (packetSize > 0)
    {
      copiedBytes = (length / packetSize) * packetSize;
    }
    else
    {
      copiedBytes = length;
    }

    // copy to destination only if length is greater than zero
    if (copiedBytes > 0)
    {
      memcpy(destination, this->dataStart + start, copiedBytes);
    }
  }

  return copiedBytes;
}

unsigned int LinearBuffer::GetFirstPosition(unsigned int start, char c)
{
  unsigned int result = UINT_MAX;

  for(unsigned int i = start; i < this->GetBufferOccupiedSpace(); i++)
  {
    if (this->buffer[i] == c)
    {
      result = i;
      break;
    }
  }

  return result;
}

bool LinearBuffer::ResizeBuffer(unsigned int size)
{
  unsigned int occupiedSize = this->GetBufferOccupiedSpace();
  bool result = (size >= occupiedSize);

  if (result)
  {
    // requested buffer size is bigger than current occupied space
    // create new buffer
    char *tempBuffer = ALLOC_MEM(char, size);
    result = (tempBuffer != NULL);

    if (result)
    {
      // copy content from current buffer to new buffer
      this->CopyFromBuffer(tempBuffer, occupiedSize, 0);
      // delete current buffer
      this->DeleteBuffer();
      // set new buffer pointer
      this->buffer = tempBuffer;
      // set new start, end of buffer
      this->dataStart = tempBuffer;
      this->dataEnd = tempBuffer + occupiedSize;
      this->bufferSize = size;
    }
  }

  return result;
}

bool LinearBuffer::SafeResizeBuffer(HANDLE lockMutex, unsigned int size, bool logMessage, CLogger *logger, const TCHAR *protocolName, const TCHAR *functionName)
{
  bool result = false;
  if ((lockMutex != NULL) && (size != this->bufferSize) &&
      ((logMessage && (logger != NULL)) || (!logMessage)))
  {
    WaitForSingleObject(lockMutex, INFINITE);
    result = this->ResizeBuffer(size);

    if (result && logMessage)
    {
      logger->Log(LOGGER_WARNING, _T("%s: %s: resized buffer, buffer size: %u, free buffer size: %u, occupied buffer size: %u"), protocolName, functionName, this->bufferSize, this->GetBufferFreeSpace(), this->GetBufferOccupiedSpace());
    }
    ReleaseMutex(lockMutex);
  }

  return result;
}

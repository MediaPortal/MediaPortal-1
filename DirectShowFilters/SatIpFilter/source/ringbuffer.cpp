#include "ringbuffer.h"
#include <stdio.h>
#include <string.h>

RingBuffer::RingBuffer(int sizeBytes)
{
	_data = new unsigned char[sizeBytes];
	memset(_data, 0, sizeBytes);
	_size = sizeBytes;
	_readPtr = 0;
	_writePtr = 0;
	_writeBytesAvail = sizeBytes;
}

RingBuffer::~RingBuffer()
{
	delete[] _data;
}

// Set all data to 0 and flag buffer as empty.
bool RingBuffer::Empty(void)
{
	memset(_data, 0, _size);
	_readPtr = 0;
	_writePtr = 0;
	_writeBytesAvail = _size;
	return true;
}

int RingBuffer::Read(unsigned char *dataPtr, int numBytes)
{
	// If there's nothing to read or no data available, then we can't read anything.
	if (dataPtr == 0 || numBytes <= 0 || _writeBytesAvail == _size)
	{
		return 0;
	}

	int readBytesAvail = _size - _writeBytesAvail;

	// Cap our read at the number of bytes available to be read.
	if (numBytes > readBytesAvail)
	{
		numBytes = readBytesAvail;
	}

	// Simultaneously keep track of how many bytes we've read and our position in the outgoing buffer
	if (numBytes > _size - _readPtr)
	{
		int len = _size - _readPtr;
		memcpy(dataPtr, _data + _readPtr, len);
		memcpy(dataPtr + len, _data, numBytes - len);
	}
	else
	{
		memcpy(dataPtr, _data + _readPtr, numBytes);
	}

	_readPtr = (_readPtr + numBytes) % _size;
	_writeBytesAvail += numBytes;

	return numBytes;
}

unsigned char RingBuffer::ReadOneByte()
{
	int readBytesAvail = _size - _writeBytesAvail;
	unsigned char out;
	unsigned char *tmp = new unsigned char[1];

	// Simultaneously keep track of how many bytes we've read and our position in the outgoing buffer
	if (1 > _size - _readPtr)
	{
		int len = _size - _readPtr;
		memcpy(tmp, _data + _readPtr, len);
		memcpy(tmp + len, _data, 1 - len);
	}
	else
	{
		memcpy(tmp, _data + _readPtr, 1);
	}

	_readPtr = (_readPtr + 1) % _size;
	_writeBytesAvail += 1;

	out = *tmp;
	return out;
}

unsigned char RingBuffer::MovePointerAndReadOneByte(int numBytes)
{
	int readBytesAvail = _size - _writeBytesAvail;
	unsigned char out;
	unsigned char *tmp = new unsigned char[1];

	// move pointer
	_readPtr = (_readPtr + numBytes - 1) % _size;

	// Simultaneously keep track of how many bytes we've read and our position in the outgoing buffer
	if (1 > _size - _readPtr)
	{
		int len = _size - _readPtr;
		memcpy(tmp, _data + _readPtr, len);
		memcpy(tmp + len, _data, 1 - len);
	}
	else
	{
		memcpy(tmp, _data + _readPtr, 1);
	}

	_readPtr = (_readPtr + 1) % _size;
	_writeBytesAvail += numBytes;

	out = *tmp;
	return out;
}

// Write to the ring buffer. Do not overwrite data that has not yet
// been read.
int RingBuffer::Write(unsigned char *dataPtr, int numBytes)
{
	// If there's nothing to write or no room available, we can't write anything.
	if (dataPtr == 0 || numBytes <= 0 || _writeBytesAvail == 0)
	{
		return 0;
	}

	// Cap our write at the number of bytes available to be written.
	if (numBytes > _writeBytesAvail)
	{
		numBytes = _writeBytesAvail;
	}

	// Simultaneously keep track of how many bytes we've written and our position in the incoming buffer
	if (numBytes > _size - _writePtr)
	{
		int len = _size - _writePtr;
		memcpy(_data + _writePtr, dataPtr, len);
		memcpy(_data, dataPtr + len, numBytes - len);
	}
	else
	{
		memcpy(_data + _writePtr, dataPtr, numBytes);
	}

	_writePtr = (_writePtr + numBytes) % _size;
	_writeBytesAvail -= numBytes;

	return numBytes;
}

int RingBuffer::GetSize(void)
{
	return _size;
}

int RingBuffer::GetWriteAvail(void)
{
	return _writeBytesAvail;
}

int RingBuffer::GetReadAvail(void)
{
	return _size - _writeBytesAvail;
}
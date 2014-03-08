/*
 *  ringbuffer.cpp
 *
 *    RingBuffer class implementation. Simple ring buffer functionality
 *    expressed as a c++ class.
 *
 *
 *  Copyright (c) 2005,2007  Arek Korbik
 *
 *  This file is part of XiphQT, the Xiph QuickTime Components.
 *
 *  XiphQT is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU Lesser General Public
 *  License as published by the Free Software Foundation; either
 *  version 2.1 of the License, or (at your option) any later version.
 *
 *  XiphQT is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public
 *  License along with XiphQT; if not, write to the Free Software
 *  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 *
 *
 *  Last modified: $Id$
 *
 */


#include "ringbuffer.h"


RingBuffer::RingBuffer() :
mBuffer(NULL),
mBStart(0),
mBEnd(0),
mBSize(0),
mNeedsWrapping(false)
{
}

RingBuffer::~RingBuffer() {
    delete[] mBuffer;
}


void RingBuffer::Initialize(LONG inBufferByteSize) {
    mBSize = inBufferByteSize;

    if (mBuffer)
        delete[] mBuffer;

    mBuffer = new Byte[mBSize * 2];

    mBStart = 0;
    mBEnd = 0;
    mNeedsWrapping = false;
}

void RingBuffer::Uninitialize() {
    mBSize = 0;

    if (mBuffer) {
        delete[] mBuffer;
        mBuffer = NULL;
    }

    Reset();

}

void RingBuffer::Reset() {
    mBStart = 0;
    mBEnd = 0;
    mNeedsWrapping = false;
}

LONG RingBuffer::Reallocate(LONG inBufferByteSize) {
    Byte *bptr = NULL;
	LONG data_size = 0;

    // can't decrease the size at the moment
    if (inBufferByteSize > mBSize) {
        bptr = new Byte[inBufferByteSize * 2];
        data_size = GetDataAvailable();
        if (mNeedsWrapping) {
			LONG headBytes = mBSize - mBStart;
			memmove(mBuffer + mBStart, bptr, headBytes);
			memmove(mBuffer, bptr + headBytes, mBEnd);
            mNeedsWrapping = false;
        } else {
			memmove(mBuffer + mBStart, bptr, data_size);
        }
        mBEnd = data_size;
        mBStart = 0;

        delete[] mBuffer;
        mBuffer = bptr;
        mBSize = inBufferByteSize;
    }

    return mBSize;
}

LONG RingBuffer::GetBufferByteSize() const {
    return mBSize;
}

LONG RingBuffer::GetDataAvailable() const {
	LONG ret = 0;

    if (mBStart < mBEnd)
        ret = mBEnd - mBStart;
    else if (mBEnd < mBStart)
        ret = mBSize + mBEnd - mBStart;

    return ret;
}

LONG RingBuffer::GetSpaceAvailable() const {
	LONG ret = mBSize;

    if (mBStart > mBEnd)
        ret =  mBStart - mBEnd;
    else if (mBEnd > mBStart)
        ret = mBSize - mBEnd + mBStart;

    return ret;

}


void RingBuffer::In(void* data, LONG& ioBytes) {
	LONG copiedBytes = GetSpaceAvailable();
    if (copiedBytes > ioBytes)
        copiedBytes = ioBytes;

    if (mBEnd + copiedBytes <= mBSize) {
		memmove(data, mBuffer + mBEnd, copiedBytes);
        mBEnd += copiedBytes;
        if (mBEnd < mBStart)
            mNeedsWrapping = true;
    } else {
		LONG wrappedBytes = mBSize - mBEnd;
        Byte* dataSplit = static_cast<Byte*>(data) + wrappedBytes;
		memmove(data, mBuffer + mBEnd, wrappedBytes);

        mBEnd = copiedBytes - wrappedBytes;
		memmove(dataSplit, mBuffer, mBEnd);

        mNeedsWrapping = true;
    }

    ioBytes -= copiedBytes;
}

void RingBuffer::Zap(LONG inBytes) {
    if (inBytes >= GetDataAvailable()) {
        mBStart = 0;
        mBEnd = 0;
        mNeedsWrapping = false;
    } else if (mBStart < mBEnd || mBStart + inBytes < mBSize) {
        mBStart += inBytes;
    } else {
        mBStart += inBytes - mBSize;
        mNeedsWrapping = false;
    }
}

Byte* RingBuffer::GetData() {
    if (GetDataAvailable() == 0)
        return mBuffer;
    else {
        if (mNeedsWrapping) {
			memmove(mBuffer, mBuffer + mBSize, mBEnd);
            mNeedsWrapping = false;
        }
        return mBuffer + mBStart;
    }
}

Byte* RingBuffer::GetDataEnd() {
	LONG available = GetDataAvailable();
    if (available == 0)
        return mBuffer;
    else {
        if (mNeedsWrapping) {
			memmove(mBuffer, mBuffer + mBSize, mBEnd);
            mNeedsWrapping = false;
        }
        return mBuffer + mBStart + available;
    }
}

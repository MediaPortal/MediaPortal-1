/*
 *  ringbuffer.h
 *
 *    RingBuffer class definition. Simple ring buffer implementation.
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


#if !defined(__ringbuffer_h__)
#define __ringbuffer_h__

#include "stdint.h"
#include "WinDef.h"
#include "typeDef.h"

class RingBuffer
{
 public:
    RingBuffer();
    virtual ~RingBuffer();

 public:
	 virtual void  Initialize(LONG inBufferByteSize);
    virtual void   Uninitialize();
    virtual void   Reset();
	virtual LONG Reallocate(LONG inBufferByteSize);

	virtual LONG GetBufferByteSize() const;
	virtual LONG GetDataAvailable() const;
	virtual LONG GetSpaceAvailable() const;

	virtual void   In(void* data, LONG& ioBytes);
	virtual void   Zap(LONG inBytes);

    virtual Byte * GetData();
    virtual Byte * GetDataEnd();

 protected:
    Byte *  mBuffer;

	LONG  mBStart;
	LONG  mBEnd;

	LONG  mBSize;

    bool mNeedsWrapping;
};


#endif /* __ringbuffer_h__ */

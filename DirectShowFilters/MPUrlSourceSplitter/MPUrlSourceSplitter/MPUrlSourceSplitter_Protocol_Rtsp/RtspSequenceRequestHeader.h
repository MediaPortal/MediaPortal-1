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

#ifndef __RTSP_SEQUENCE_REQUEST_HEADER_DEFINED
#define __RTSP_SEQUENCE_REQUEST_HEADER_DEFINED

#include "RtspRequestHeader.h"

#define RTSP_SEQUENCE_REQUEST_HEADER_NAME                             L"CSeq"

#define RTSP_SEQUENCE_NUMBER_UNSPECIFIED                              UINT_MAX

class CRtspSequenceRequestHeader : public CRtspRequestHeader
{
public:
  CRtspSequenceRequestHeader(void);
  virtual ~CRtspSequenceRequestHeader(void);

  /* get methods */

  // gets RTSP header name
  // @return : RTSP header name
  virtual const wchar_t *GetName(void);

  // gets RTSP sequence number
  // @return : RTSP sequence number
  virtual unsigned int GetSequenceNumber(void);

  /* set methods */

  // sets RTSP header name
  // @param name : RTSP header name to set
  // @return : true if successful, false otherwise
  virtual bool SetName(const wchar_t *name);

  // sets RTSP sequence number
  // @param : RTSP sequence number to set
  virtual void SetSequenceNumber(unsigned int sequenceNumber);

  /* other methods */

  // deep clones of current instance
  // @return : deep clone of current instance or NULL if error
  virtual CRtspSequenceRequestHeader *Clone(void);

protected:

  unsigned int sequenceNumber;

  // deeply clones current instance to cloned header
  // @param  clonedHeader : cloned header to hold clone of current instance
  // @return : true if successful, false otherwise
  virtual bool CloneInternal(CHttpHeader *clonedHeader);

  // returns new RTSP request header object to be used in cloning
  // @return : RTSP request header object or NULL if error
  virtual CHttpHeader *GetNewHeader(void);
};

#endif
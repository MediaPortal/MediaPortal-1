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

#ifndef __UDP_SOCKET_CONTEXT_DEFINED
#define __UDP_SOCKET_CONTEXT_DEFINED

#include "SocketContext.h"

class CUdpSocketContext : public CSocketContext
{
public:
  CUdpSocketContext(void);
  CUdpSocketContext(SOCKET socket);
  virtual ~CUdpSocketContext(void);

  /* get methods */

  // gets last sender IP address
  // last sender IP address is set when received some data
  // @return : last sender IP address or NULL if not specified
  virtual CIpAddress *GetLastSenderIpAddress(void);

  /* set methods */

  // sets last sender IP address
  // @param sender : the IP address of last sender
  // @return : true if successful, false oterwise
  virtual bool SetLastSenderIpAddress(CIpAddress *sender);

  /* other methods */

  // sends data on a connected socket
  // @param buffer : pointer to a buffer containing the data to be transmitted
  // @param length : the length, in bytes, of the data in buffer pointed to by the buffer parameter
  // @param sentLength : reference to total number of bytes sent, which can be less than the number requested to be sent
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT Send(const char *buffer, unsigned int length, unsigned int *sentLength);

  // sends data on a connected socket
  // @param buffer : pointer to a buffer containing the data to be transmitted
  // @param length : the length, in bytes, of the data in buffer pointed to by the buffer parameter
  // @param flags : set of flags that specify the way in which the call is made, see remarks of send() method
  // @param sentLength : reference to total number of bytes sent, which can be less than the number requested to be sent
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT Send(const char *buffer, unsigned int length, int flags, unsigned int *sentLength);

  // sends data on a connected socket
  // @param buffer : pointer to a buffer containing the data to be transmitted
  // @param length : the length, in bytes, of the data in buffer pointed to by the buffer parameter
  // @param sentLength : reference to total number of bytes sent, which can be less than the number requested to be sent
  // @param client : the IP address and port of client to send data
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT Send(const char *buffer, unsigned int length, unsigned int *sentLength, CIpAddress *client);

  // sends data on a connected socket
  // @param buffer : pointer to a buffer containing the data to be transmitted
  // @param length : the length, in bytes, of the data in buffer pointed to by the buffer parameter
  // @param flags : set of flags that specify the way in which the call is made, see remarks of send() method
  // @param sentLength : reference to total number of bytes sent, which can be less than the number requested to be sent
  // @param client : the IP address and port of client to send data
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT Send(const char *buffer, unsigned int length, int flags, unsigned int *sentLength, CIpAddress *client);

  // receives data from a connected socket
  // @param buffer : pointer to the buffer to receive the incoming data
  // @param length : the length, in bytes, of the buffer pointed to by the buffer parameter
  // @param receivedLength : reference to the number of bytes received, if the connection has been gracefully closed, value is zero
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT Receive(char *buffer, unsigned int length, unsigned int *receivedLength);

  // receives data from a connected socket
  // @param buffer : pointer to the buffer to receive the incoming data
  // @param length : the length, in bytes, of the buffer pointed to by the buffer parameter
  // @param flasg : set of flags that influences the behavior of this function, see remarks of recv() method
  // @param receivedLength : reference to the number of bytes received, if the connection has been gracefully closed, value is zero
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT Receive(char *buffer, unsigned int length, int flags, unsigned int *receivedLength);

  // receives data from a connected socket
  // @param buffer : pointer to the buffer to receive the incoming data
  // @param length : the length, in bytes, of the buffer pointed to by the buffer parameter
  // @param receivedLength : reference to the number of bytes received, if the connection has been gracefully closed, value is zero
  // @param sender : reference to variable to hold sender who sends data
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT Receive(char *buffer, unsigned int length, unsigned int *receivedLength, CIpAddress **sender);

  // receives data from a connected socket
  // @param buffer : pointer to the buffer to receive the incoming data
  // @param length : the length, in bytes, of the buffer pointed to by the buffer parameter
  // @param flasg : set of flags that influences the behavior of this function, see remarks of recv() method
  // @param receivedLength : reference to the number of bytes received, if the connection has been gracefully closed, value is zero
  // @param sender : reference to variable to hold sender who sends data
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT Receive(char *buffer, unsigned int length, int flags, unsigned int *receivedLength, CIpAddress **sender);

protected:

  // holds last sender IP address (set when received some data)
  CIpAddress *lastSenderIpAddress;
};

#endif
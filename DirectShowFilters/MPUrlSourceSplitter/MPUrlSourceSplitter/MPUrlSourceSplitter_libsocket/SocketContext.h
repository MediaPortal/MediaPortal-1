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

#ifndef __SOCKET_CONTEXT_DEFINED
#define __SOCKET_CONTEXT_DEFINED

#include "IpAddress.h"

#include <stdint.h>

#define BUFFER_REQUEST_SIZE_DEFAULT                                           32768

#define BUFFER_LENGTH_DEFAULT                                                 262144

#define SOCKET_STATE_UNDEFINED                                                0
#define SOCKET_STATE_READABLE                                                 1
#define SOCKET_STATE_WRITABLE                                                 2

class CSocketContext
{
public:
  CSocketContext(void);
  CSocketContext(SOCKET socket);
  virtual ~CSocketContext(void);

  /* get methods */

  // gets IP address associated with socket
  // @return : IP address or NULL if error
  virtual CIpAddress *GetIpAddress(void);

  // gets socket option
  // @param level : lhe level at which the option is defined (SOL_SOCKET, ...)
  // @param optionName : the socket option for which the value is to be retrieved(SO_ACCEPTCONN, ...)
  // @param optionValue : a pointer to the buffer in which the value for the requested option is to be returned
  // @param optionLength : a pointer to the size, in bytes, of the optionValue buffer
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT GetOption(int level, int optionName, char *optionValue, int *optionLength);

  // gets pending incoming data length
  // @param incomingDataLength : the reference to variable to hold pending incoming data length
  // @return : S_OK if successful, E_NOT_VALID_STATE if socket is not readable, error code otherwise
  virtual HRESULT GetPendingIncomingDataLength(unsigned int *incomingDataLength);

  // gets received data length
  // @return : received data length
  virtual uint64_t GetReceivedDataLength(void);

  // gets sent data length
  // @return : sent data length
  virtual uint64_t GetSentDataLength(void);

  /* set methods */

  // sets IP address to be associated with socket
  // @param address : IP address to set (IP address is cloned)
  // @return : S_OK if successful, false otherwise
  virtual HRESULT SetIpAddress(CIpAddress *address);

  // sets socket option
  // @param level : lhe level at which the option is defined (SOL_SOCKET, ...)
  // @param optionName : the socket option for which the value is to be set (SO_BROADCAST, ...)
  // @param optionValue : a pointer to the buffer in which the value for the requested option is specified
  // @param optionLength : the size, in bytes, of the buffer pointed to by the optionValue parameter
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT SetOption(int level, int optionName, const char *optionValue, int optionLength);

  // sets socket blocking mode
  // @param blocking : true if blocking socket, false otherwise
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT SetBlockingMode(bool blocking);

  /* other methods */

  // creates socket with specified family, type and protocol
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT CreateSocket(void);

  // creates socket with specified family, type and protocol
  // @param address : address to create socket (can be NULL if already set)
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT CreateSocket(CIpAddress *address);

  // closes socket
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT CloseSocket(void);

  // tests if connection is closed 
  // @return : S_OK if connection is closed, S_FALSE if connection is opened, E_NOT_VALID_STATE if socket is not readable, error code otherwise
  virtual HRESULT IsClosed(void);

  // binds set IP address with a socket
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT Bind(void);
  
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

  // determines the status of socket
  // @param read : determine if socket is in readable state (incoming connection or unread data)
  // @param write : determine if socket is in writable state (connect is successful or can send data)
  // @param timeout : the maximum time for select to wait (in ms)
  // @param state : reference to socket state variable
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT Select(bool read, bool write, unsigned int timeout, unsigned int *state);

protected:

  // holds internal socket
  SOCKET internalSocket;

  // specifies if WSA was correctly initialized
  bool wsaInitialized;

  // holds IP address associated with socket
  CIpAddress *ipAddress;

  /* methods */

  uint64_t receivedDataLength;
  uint64_t sentDataLength;
};

#endif
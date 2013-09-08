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

#ifndef __NETWORK_DEFINED
#define __NETWORK_DEFINED

#include "MPIPTVSourceExports.h"
#include "ParameterCollection.h"
#include "Logger.h"

#include <Ws2tcpip.h>
#include <WinInet.h>

// resolve a host name to a list of socket addresses (like getaddrinfo())
// @param serverName : host name to resolve
// @param port : port number for the socket addresses
// @param address : pointer set to the resulting chained list
// @param p_hints : parameters (see getaddrinfo() manual page)
// @return : 0 on success, a getaddrinfo() error otherwise
MPIPTVSOURCE_API int GetIpAddress(const TCHAR *serverName, WORD port, ADDRINFOT **address, const ADDRINFOT *pHints);

// test if address is in multicast range
// @param addr : the address to test
// @return : true if address is in multicast range, false otherwise
MPIPTVSOURCE_API bool SockAddrIsMulticast(const ADDRINFOT *addr);

// get network interface IP address (IPv4 or IPv6)
// @param logger : logger for logging purposes
// @param protocolName : name of protocol calling GetInterfaceAddress()
// @param interfaceName : name of network interface (e.g. "Local Area Connection")
// @param family : internet family (AF_INET or AF_INET6)
// @return : reference to null-terminated string if success, NULL otherwise
MPIPTVSOURCE_API TCHAR *GetInterfaceAddress(CLogger *logger, const TCHAR *protocolName, const TCHAR *interfaceName, int family);

// get network interface ID (IPv4 or IPv6)
// @param logger : logger for logging purposes
// @param protocolName : name of protocol calling GetInterfaceId()
// @param interfaceName : name of network interface (e.g. "Local Area Connection")
// @param family : internet family (AF_INET or AF_INET6)
// @return: network interface ID if success, 0 otherwise
MPIPTVSOURCE_API ULONG GetInterfaceId(CLogger *logger, const TCHAR *protocolName, const TCHAR *interfaceName, int family);

// join to multicast group using IPv4
// @param logger : logger for logging purposes
// @param protocolName : name of protocol calling JoinMulticastGroupIPv4()
// @param m_socket : descriptor identifying an bound socket
// @param source : source server
// @param local : local multicast
// @param parameters : various parameters for connection
// @return: 0 if success
MPIPTVSOURCE_API int JoinMulticastGroupIPv4(CLogger *logger, const TCHAR *protocolName, SOCKET m_socket, const struct sockaddr_in *source, const struct sockaddr_in *local, CParameterCollection *parameters);

// join to multicast group using IPv6
// @param logger : logger for logging purposes
// @param protocolName : name of protocol calling JoinMulticastGroupIPv6()
// @param m_socket : descriptor identifying an bound socket
// @param local : local mutlicast
// @param parameters : various parameters for connection
// @return: 0 if success
MPIPTVSOURCE_API int JoinMulticastGroupIPv6(CLogger *logger, const TCHAR *protocolName, SOCKET m_socket, const struct sockaddr_in6 *local, CParameterCollection *parameters);

// subscribe to multicast group
// @param logger : logger for logging purposes
// @param protocolName : name of protocol calling SubscribeToMulticastGroup()
// @param m_socket : descriptor identifying an bound socket
// @param source : source server
// @param sourceLen : length of source server sockaddr structure (can be sockaddr_in for IPv4 or sockaddr_in6 for IPv6)
// @param local : local mutlicast
// @param localLen : length of local multicast sockaddr structure (can be sockaddr_in for IPv4 or sockaddr_in6 for IPv6)
// @param parameters : various parameters for connection
// @return: 0 if success
MPIPTVSOURCE_API int SubscribeToMulticastGroup(CLogger *logger, const TCHAR *protocolName, SOCKET m_socket, const struct sockaddr *source, socklen_t sourceLen, const struct sockaddr *local, socklen_t localLen, CParameterCollection *parameters);

// leave multicast group using IPv4
// @param logger : logger for logging purposes
// @param protocolName : name of protocol calling LeaveMulticastGroupIPv4()
// @param m_socket : descriptor identifying an bound socket
// @param source : source server
// @param local : local mutlicast
// @param parameters : various parameters for connection
// @return: 0 if success
MPIPTVSOURCE_API int LeaveMulticastGroupIPv4(CLogger *logger, const TCHAR *protocolName, SOCKET m_socket, const struct sockaddr_in *source, const struct sockaddr_in *local, CParameterCollection *parameters);

// leave multicast group using IPv6
// @param logger : logger for logging purposes
// @param protocolName : name of protocol calling LeaveMulticastGroupIPv6()
// @param m_socket : descriptor identifying an bound socket
// @param local : local mutlicast
// @param parameters : various parameters for connection
// @return: 0 if success
MPIPTVSOURCE_API int LeaveMulticastGroupIPv6(CLogger *logger, const TCHAR *protocolName, SOCKET m_socket, const struct sockaddr_in6 *local, CParameterCollection *parameters);

// unsubscribe from multicast group
// @param logger : logger for logging purposes
// @param protocolName : name of protocol calling UnsubscribeFromMulticastGroup()
// @param m_socket : descriptor identifying an bound socket
// @param source : source server
// @param sourceLen : length of source server sockaddr structure (can be sockaddr_in for IPv4 or sockaddr_in6 for IPv6)
// @param local : local mutlicast
// @param localLen : length of local multicast sockaddr structure (can be sockaddr_in for IPv4 or sockaddr_in6 for IPv6)
// @param parameters : various parameters for connection
// @return: 0 if success
MPIPTVSOURCE_API int UnsubscribeFromMulticastGroup(CLogger *logger, const TCHAR *protocolName, SOCKET m_socket, const struct sockaddr *source, socklen_t sourceLen, const struct sockaddr *local, socklen_t localLen, CParameterCollection *parameters);

// clear url components
// @param url : url components to clear
MPIPTVSOURCE_API void ZeroURL(URL_COMPONENTS *url);

// socket operations

// create socket with specified family, type and protocol
// @param logger : logger for logging purposes
// @param functionName : name of function calling CreateSocket()
// @param family : the address family specification
// @param type : the type specification for the new socket
// @param protocol : the protocol to be used
// @return : if no error occurs, CreateSocket() returns a descriptor referencing the new socket. Otherwise, a value of INVALID_SOCKET is returned, and a specific error code can be retrieved by calling WSAGetLastError.
MPIPTVSOURCE_API SOCKET CreateSocket(CLogger *logger, const TCHAR *protocolName, const TCHAR *functionName, int family, int type, int protocol);

// set a socket option
// @param logger : logger for logging purposes
// @param functionName : name of function calling SetSocketOption()
// @param optionName : the name of socket option for which the value is to be set
// @param s : a descriptor that identifies a socket
// @param level : the level at which the option is defined (for example, SOL_SOCKET)
// @param optname : the socket option for which the value is to be set (for example, SO_BROADCAST)
// @param optval : a pointer to the buffer in which the value for the requested option is specified
// @param optnlen : the size, in bytes, of the buffer pointed to by the optval parameter
// @return : if no error occurs return zero, otherwise a value of WSAGetLastError()
MPIPTVSOURCE_API int SetSocketOption(CLogger *logger, const TCHAR *protocolName, const TCHAR *functionName, const TCHAR *optionName, SOCKET s, int level, int optname, const char *optval, int optlen);

// set a socket option
// @param logger : logger for logging purposes
// @param functionName : name of function calling GetSocketOption()
// @param optionName : the name of socket option for which the value is to be retrieved
// @param s : a descriptor that identifies a socket
// @param level : the level at which the option is defined (for example, SOL_SOCKET)
// @param optname : the socket option for which the value is to be retrieved
// @param optval : a pointer to the buffer in which the value for the requested option is to be returned
// @param optnlen : a pointer to the size, in bytes, of the optval buffer
// @return : if no error occurs return zero, otherwise a value of WSAGetLastError()
MPIPTVSOURCE_API int GetSocketOption(CLogger *logger, const TCHAR *protocolName, const TCHAR *functionName, const TCHAR *optionName, SOCKET s, int level, int optname, char *optval, int *optlen);

// set a socket option
// @param logger : logger for logging purposes
// @param functionName : name of function calling SetAndCheckSocketOption()
// @param optionName : the name of socket option for which the value is to be set
// @param s : a descriptor that identifies a socket
// @param level : the level at which the option is defined (for example, SOL_SOCKET)
// @param optname : the socket option for which the value is to be set (for example, SO_BROADCAST)
// @param optval : a pointer to the buffer in which the value for the requested option is specified
// @param optnlen : the size, in bytes, of the buffer pointed to by the optval parameter
// @return : if no error occurs return zero, otherwise a value of WSAGetLastError() or E_OUTOFMEMORY if not enough memory or -1 if set and check are not same
MPIPTVSOURCE_API int SetAndCheckSocketOption(CLogger *logger, const TCHAR *protocolName, const TCHAR *functionName, const TCHAR *optionName, SOCKET s, int level, int optname, const char *optval, int optlen);

// set a socket option
// @param logger : logger for logging purposes
// @param functionName : name of function calling SetBlockingMode()
// @param s : a descriptor that identifies a socket
// @param blocking : false if non-blocking, true if blocking
// @return : if no error occurs return zero, otherwise a value of WSAGetLastError()
MPIPTVSOURCE_API int SetBlockingMode(CLogger *logger, const TCHAR *protocolName, const TCHAR *functionName, SOCKET s, bool blocking);

// associate a local address with a socket
// @param logger : logger for logging purposes
// @param functionName : name of function calling Bind()
// @param s : a descriptor that identifies a socket
// @param name : a pointer to a sockaddr structure of the local address to assign to the bound socket
// @param namelen : the length, in bytes, of the value pointed to by the name parameter
// @return : if no error occurs return zero, otherwise a value of WSAGetLastError()
MPIPTVSOURCE_API int Bind(CLogger *logger, const TCHAR *protocolName, const TCHAR *functionName, SOCKET s, const sockaddr *name, int namelen);

// process error
// @param logger : logger for logging purposes
// @param functionName : name of function calling ProcessError()
// @param networkFunctionName : name of network function
// @param s : a descriptor that identifies a socket
// @param timeout : the maximum time to wait, NULL if not specified
// @param read : specifies if socket have to be readable
// @param write : specifies if socket have to be writable 
// @return : if no error occurs return zero, otherwise a value of WSAGetLastError(), -1 when socket is not readable, -2 when socket is not writable, -3 when socket is not readable and writable
MPIPTVSOURCE_API int ProcessError(CLogger *logger, const TCHAR *protocolName, const TCHAR *functionName, const TCHAR *networkFunctionName, SOCKET s, unsigned int timeout, bool read, bool write);

// establish a connection to a specified socket
// @param logger : logger for logging purposes
// @param functionName : name of function calling Connect()
// @param s : a descriptor that identifies a socket
// @param name : a pointer to the sockaddr structure to which the connection should be established
// @param namelen : the length, in bytes, of the sockaddr structure pointed to by the name parameter
// @param timeout : the maximum time to wait, NULL if not specified
// @param read : specifies if socket have to be readable
// @param write : specifies if socket have to be writable 
// @return : if no error occurs return zero, otherwise a value of WSAGetLastError(), -1 when socket is not readable, -2 when socket is not writable, -3 when socket is not readable and writable
MPIPTVSOURCE_API int Connect(CLogger *logger, const TCHAR *protocolName, const TCHAR *functionName, SOCKET s, const sockaddr *name, int namelen, unsigned int timeout, bool read, bool write);

// send data on a connected socket
// @param logger : logger for logging purposes
// @param functionName : name of function calling Send()
// @param s : a descriptor that identifies a socket
// @param buffer : a pointer to a buffer containing the data to be transmitted
// @param length : the length, in bytes, of the data in buffer pointed to by the buffer parameter
// @param flags : a set of flags that specify the way in which the call is made
// @param timeout : the maximum time to wait, NULL if not specified
// @return : if no error occurs return zero, otherwise a value of WSAGetLastError() or E_OUTOFMEMORY if not enough memory
MPIPTVSOURCE_API int Send(CLogger *logger, const TCHAR *protocolName, const TCHAR *functionName, SOCKET s, const char *buffer, int length, int flags, unsigned int timeout);

// send string on a connected socket
// @param logger : logger for logging purposes
// @param functionName : name of function calling Send()
// @param s : a descriptor that identifies a socket
// @param buffer : a pointer to a buffer containing the data to be transmitted terminated with null character
// @param flags : a set of flags that specify the way in which the call is made
// @param timeout : the maximum time to wait, NULL if not specified
// @return : if no error occurs return zero, otherwise a value of WSAGetLastError() or E_OUTOFMEMORY if not enough memory
MPIPTVSOURCE_API int SendString(CLogger *logger, const TCHAR *protocolName, const TCHAR *functionName, SOCKET s, const TCHAR *buffer, int flags, unsigned int timeout);

// get data from connected socket
// @param logger : logger for logging purposes
// @param protocolName : name of protocol calling GetDataFromSocket()
// @param functionName : name of function calling GetDataFromSocket()
// @param s : a descriptor that identifies a socket
// @param buffer : a pointer to a buffer to store the data
// @param length : the length of buffer
// @return : returns zero if no data, the length of data if successful, SOCKET_ERROR if error
MPIPTVSOURCE_API int GetDataFromSocket(CLogger *logger, const TCHAR *protocolName, const TCHAR *functionName, SOCKET s, char *buffer, unsigned int length);

// write input packet to dump file
// @param protocolInstance : instance identifier of protocol
// @param length : the length of packet
// @param packet : a pointer to packet data to dump
// @return : return zero if no error
MPIPTVSOURCE_API int DumpInputPacket(GUID protocolInstance, unsigned int length, char *packet);

#endif

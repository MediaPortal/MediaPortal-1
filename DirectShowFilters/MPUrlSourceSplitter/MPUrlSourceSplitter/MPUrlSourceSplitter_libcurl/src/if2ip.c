/***************************************************************************
 *                                  _   _ ____  _
 *  Project                     ___| | | |  _ \| |
 *                             / __| | | | |_) | |
 *                            | (__| |_| |  _ <| |___
 *                             \___|\___/|_| \_\_____|
 *
 * Copyright (C) 1998 - 2013, Daniel Stenberg, <daniel@haxx.se>, et al.
 *
 * This software is licensed as described in the file COPYING, which
 * you should have received as part of this distribution. The terms
 * are also available at http://curl.haxx.se/docs/copyright.html.
 *
 * You may opt to use, copy, modify, merge, publish, distribute and/or sell
 * copies of the Software, and permit persons to whom the Software is
 * furnished to do so, under the terms of the COPYING file.
 *
 * This software is distributed on an "AS IS" basis, WITHOUT WARRANTY OF ANY
 * KIND, either express or implied.
 *
 ***************************************************************************/

#include "curl_setup.h"

#ifdef HAVE_NETINET_IN_H
#  include <netinet/in.h>
#endif
#ifdef HAVE_ARPA_INET_H
#  include <arpa/inet.h>
#endif
#ifdef HAVE_NET_IF_H
#  include <net/if.h>
#endif
#ifdef HAVE_SYS_IOCTL_H
#  include <sys/ioctl.h>
#endif
#ifdef HAVE_NETDB_H
#  include <netdb.h>
#endif
#ifdef HAVE_SYS_SOCKIO_H
#  include <sys/sockio.h>
#endif
#ifdef HAVE_IFADDRS_H
#  include <ifaddrs.h>
#endif
#ifdef HAVE_STROPTS_H
#  include <stropts.h>
#endif
#ifdef __VMS
#  include <inet.h>
#endif

#include "inet_ntop.h"
#include "strequal.h"
#include "if2ip.h"

#define _MPRINTF_REPLACE /* use our functions only */
#include <curl/mprintf.h>

#include "curl_memory.h"
/* The last #include file should be: */
#include "memdebug.h"

/* ------------------------------------------------------------------ */

#if defined(HAVE_GETIFADDRS)

bool Curl_if_is_interface_name(const char *interf)
{
  bool result = FALSE;

  struct ifaddrs *iface, *head;

  if(getifaddrs(&head) >= 0) {
    for(iface=head; iface != NULL; iface=iface->ifa_next) {
      if(curl_strequal(iface->ifa_name, interf)) {
        result = TRUE;
        break;
      }
    }
    freeifaddrs(head);
  }
  return result;
}

if2ip_result_t Curl_if2ip(int af, unsigned int remote_scope,
                          const char *interf, char *buf, int buf_size)
{
  struct ifaddrs *iface, *head;
  if2ip_result_t res = IF2IP_NOT_FOUND;

#ifndef ENABLE_IPV6
  (void) remote_scope;
#endif

  if(getifaddrs(&head) >= 0) {
    for(iface=head; iface != NULL; iface=iface->ifa_next) {
      if(iface->ifa_addr != NULL) {
        if(iface->ifa_addr->sa_family == af) {
          if(curl_strequal(iface->ifa_name, interf)) {
            void *addr;
            char *ip;
            char scope[12]="";
            char ipstr[64];
#ifdef ENABLE_IPV6
            if(af == AF_INET6) {
              unsigned int scopeid = 0;
              addr = &((struct sockaddr_in6 *)iface->ifa_addr)->sin6_addr;
#ifdef HAVE_SOCKADDR_IN6_SIN6_SCOPE_ID
              /* Include the scope of this interface as part of the address */
              scopeid =
                ((struct sockaddr_in6 *)iface->ifa_addr)->sin6_scope_id;
#endif
              if(scopeid != remote_scope) {
                /* We are interested only in interface addresses whose
                   scope ID matches the remote address we want to
                   connect to: global (0) for global, link-local for
                   link-local, etc... */
                if(res == IF2IP_NOT_FOUND) res = IF2IP_AF_NOT_SUPPORTED;
                continue;
              }
              if(scopeid)
                snprintf(scope, sizeof(scope), "%%%u", scopeid);
            }
            else
#endif
              addr = &((struct sockaddr_in *)iface->ifa_addr)->sin_addr;
            res = IF2IP_FOUND;
            ip = (char *) Curl_inet_ntop(af, addr, ipstr, sizeof(ipstr));
            snprintf(buf, buf_size, "%s%s", ip, scope);
            break;
          }
        }
        else if((res == IF2IP_NOT_FOUND) &&
                curl_strequal(iface->ifa_name, interf)) {
          res = IF2IP_AF_NOT_SUPPORTED;
        }
      }
    }
    freeifaddrs(head);
  }
  return res;
}

#elif defined(HAVE_IOCTL_SIOCGIFADDR)

bool Curl_if_is_interface_name(const char *interf)
{
  /* This is here just to support the old interfaces */
  char buf[256];

  return (Curl_if2ip(AF_INET, 0, interf, buf, sizeof(buf)) ==
          IF2IP_NOT_FOUND) ? FALSE : TRUE;
}

if2ip_result_t Curl_if2ip(int af, unsigned int remote_scope,
                          const char *interf, char *buf, int buf_size)
{
  struct ifreq req;
  struct in_addr in;
  struct sockaddr_in *s;
  curl_socket_t dummy;
  size_t len;

  (void)remote_scope;

  if(!interf || (af != AF_INET))
    return IF2IP_NOT_FOUND;

  len = strlen(interf);
  if(len >= sizeof(req.ifr_name))
    return IF2IP_NOT_FOUND;

  dummy = socket(AF_INET, SOCK_STREAM, 0);
  if(CURL_SOCKET_BAD == dummy)
    return IF2IP_NOT_FOUND;

  memset(&req, 0, sizeof(req));
  memcpy(req.ifr_name, interf, len+1);
  req.ifr_addr.sa_family = AF_INET;

  if(ioctl(dummy, SIOCGIFADDR, &req) < 0) {
    sclose(dummy);
    /* With SIOCGIFADDR, we cannot tell the difference between an interface
       that does not exist and an interface that has no address of the
       correct family. Assume the interface does not exist */
    return IF2IP_NOT_FOUND;
  }

  s = (struct sockaddr_in *)&req.ifr_addr;
  memcpy(&in, &s->sin_addr, sizeof(in));
  Curl_inet_ntop(s->sin_family, &in, buf, buf_size);

  sclose(dummy);
  return IF2IP_FOUND;
}

#else

#include <winsock2.h>
#include <IPHlpApi.h>
#include <ObjBase.h>

/* copied from Memory.h */

#define ALLOC_MEM(type, length)                               (type *)CoTaskMemAlloc(length * sizeof(type))
#define REALLOC_MEM(formerVariable, type, length)             (type *)CoTaskMemRealloc(formerVariable, length * sizeof(type))
#define ALLOC_MEM_DEFINE(variable, type, length)              type *variable = ALLOC_MEM(type, length)
#define ALLOC_MEM_SET(variable, type, length, value)          ALLOC_MEM(type, length); \
                                                              if (variable != NULL) \
                                                              { \
                                                                memset(variable, value, length * sizeof(type)); \
                                                              }
#define ALLOC_MEM_DEFINE_SET(variable, type, length, value)   type *variable = ALLOC_MEM_SET(variable, type, length, value)
#define FREE_MEM(variable)                                    if (variable != NULL) \
                                                              { \
                                                                CoTaskMemFree(variable); \
                                                                variable = NULL; \
                                                              }

#define FREE_MEM_CLASS(variable)                              if (variable != NULL) \
                                                              { \
                                                                delete variable; \
                                                                variable = NULL; \
                                                              }


#define COM_SAFE_RELEASE(instance)                            if (instance != NULL) \
                                                              { \
                                                                instance->Release(); \
                                                                instance = NULL; \
                                                              }

#define CHECK_CONDITION(result, condition, case_true, case_false)                 if (result == 0) { result = (condition) ? case_true : case_false; }

#define CHECK_CONDITION_HRESULT(result, condition, case_true, case_false)         if (SUCCEEDED(result)) { result = (condition) ? case_true : case_false; }

#define CHECK_POINTER_HRESULT(result, pointer, case_true, case_false)             CHECK_CONDITION_HRESULT(result, pointer != NULL, case_true, case_false)
#define CHECK_POINTER_DEFAULT_HRESULT(result, pointer)                            CHECK_POINTER_HRESULT(result, pointer, S_OK, E_POINTER)

#define CHECK_POINTER(result, pointer, case_true, case_false)                     CHECK_CONDITION(result, pointer != NULL, case_true, case_false)

#define CHECK_CONDITION_EXECUTE(condition, command)                               if (condition) { command; }

#define CHECK_CONDITION_EXECUTE_RESULT(condition, command, result)                CHECK_CONDITION_EXECUTE(condition, result = command)

#define CHECK_CONDITION_NOT_NULL_EXECUTE(condition, command)                      CHECK_CONDITION_EXECUTE(condition != NULL, command)

#define CHECK_HRESULT_EXECUTE(result, command)                                    CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), command, result)

/* copied from Strings.cpp */

wchar_t *ConvertToUnicodeA(const char *string)
{
  wchar_t *result = NULL;

  if (string != NULL)
  {
    size_t length = 0;

    if (mbstowcs_s(&length, NULL, 0, string, strlen(string)) == 0)
    {
      result = ALLOC_MEM_SET(result, wchar_t, length, 0);
      if (result != NULL)
      {
        if (mbstowcs_s(&length, result, length, string, strlen(string)) != 0)
        {
          // error occurred but buffer is created
          FREE_MEM(result);
        }
      }
    }
  }

  return result;
}

int CompareWithNullW(const wchar_t *str1, const wchar_t *str2)
{
  int result = 0;

  if ((str1 != NULL) && (str2 != NULL))
  {
    result = wcscmp(str1, str2);
  }
  else if (str1 != NULL)
  {
    result = -1;
  }
  else if (str2 != NULL)
  {
    result = 1;
  }

  return result;
}

bool Curl_if_is_interface_name(const char *interf)
{
  /* This is here just to support the old interfaces */
  char buf[256];

  return (Curl_if2ip(AF_INET, 0, interf, buf, sizeof(buf)) ==
          IF2IP_NOT_FOUND) ? FALSE : TRUE;
}

if2ip_result_t Curl_if2ip(int af, unsigned int remote_scope,
                          const char *interf, char *buf, int buf_size)
{
  if2ip_result_t result = IF2IP_NOT_FOUND;
  HRESULT res = S_OK;

  ULONG bufferLen = 0;
  ULONG flags = GAA_FLAG_INCLUDE_PREFIX;
  DWORD retval = 0;

  PIP_ADAPTER_ADDRESSES addresses = NULL;
  PIP_ADAPTER_ADDRESSES pCurrAddresses = NULL;
  PIP_ADAPTER_UNICAST_ADDRESS pUnicast = NULL;
  PIP_ADAPTER_ADDRESSES address = NULL;
  PIP_ADAPTER_UNICAST_ADDRESS unicastAddress = NULL;
  DWORD length = 256;

  wchar_t *networkInterface = ConvertToUnicodeA(interf);
  CHECK_POINTER_HRESULT(res, networkInterface, res, E_OUTOFMEMORY);

  if (SUCCEEDED(res))
  {
    if (GetAdaptersAddresses(af, flags, NULL, addresses, &bufferLen) == ERROR_BUFFER_OVERFLOW)
    {
      addresses = (PIP_ADAPTER_ADDRESSES)CoTaskMemAlloc(bufferLen);

      CHECK_POINTER_HRESULT(res, addresses, res, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(res), HRESULT_FROM_WIN32(GetAdaptersAddresses(af, flags, NULL, addresses, &bufferLen)), res);

      if (SUCCEEDED(res))
      {
        for (address = addresses; ((result != IF2IP_FOUND) && SUCCEEDED(res) && (address != NULL)); address = address->Next)
        {
          if (SUCCEEDED(res))
          {
            if (CompareWithNullW(address->FriendlyName, networkInterface) == 0)
            {
              CHECK_CONDITION_EXECUTE(address->FirstUnicastAddress == NULL, res = E_OUTOFMEMORY);

              for (unicastAddress = address->FirstUnicastAddress; (SUCCEEDED(res) && (unicastAddress != NULL)); unicastAddress = unicastAddress->Next)
              {
                res = WSAAddressToString(unicastAddress->Address.lpSockaddr, unicastAddress->Address.iSockaddrLength, NULL, buf, &length);

                if (SUCCEEDED(res))
                {
                  result = IF2IP_FOUND;
                  break;
                }

                if (FAILED(res))
                {
                  res = S_OK;
                }
              }
            }
          }
        }
      }

      FREE_MEM(addresses);
    }
  }

  if (FAILED(res))
  {
    result = IF2IP_NOT_FOUND;
  }

  FREE_MEM(networkInterface);
  return result;
}

#endif


The purpose of this document is to remark all changes made in libcurl by any developer. These same changes
(or at least their meaning) must be made in next releases of libcurl.
 
Changes in libcurl 7.37.0:

--------------------------------------------
File: \src\http.c
Comment: in Curl_http_readwrite_headers() method add on end of block of code started with 'if(conn->handler->protocol & CURLPROTO_RTSP)'
Code:

if (k->httpcode >= 400)
{
  // error occured, we can assume that there will not be content (body)
  data->set.opt_no_body = TRUE;
}

--------------------------------------------
File: \src\connect.c
Comment: in singleipconnect() method add WSAEISCONN error in switch before default
Code:

case WSAEISCONN:
  // socket is already connected
  *sockp = sockfd;
  break;

--------------------------------------------
Comment: in Curl_getconnectinfo() method replace block of code starting with 'if(recv((RECV_TYPE_ARG1)c->sock[FIRSTSOCKET], (RECV_TYPE_ARG2)&buf,' (to end of condition block)
Code:

int ret = recv((RECV_TYPE_ARG1)c->sock[FIRSTSOCKET], (RECV_TYPE_ARG2)&buf, (RECV_TYPE_ARG3)1, (RECV_TYPE_ARG4)MSG_PEEK);

if ((ret == SOCKET_ERROR) && (SOCKERRNO == EWOULDBLOCK))
{
  // correct, socket is not blocking
}
else if ((ret == SOCKET_ERROR) || (ret == 0))
{
  // in case of SOCKET_ERROR, there is some error
  // in case of zero, socket is gracefully closed /* FIN received */
  return CURL_SOCKET_BAD;   
}

--------------------------------------------
File: \src\if2ip.c
Comment: in conditional section between '#else' and '#endif' replace all with
Code:

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

--------------------------------------------
File: \src\curl_rtmp.c
Comment: before #endif of #ifdef USE_LIBRTMP insert
Code:

#else

#include "urldata.h"
#include "nonblock.h" /* for curlx_nonblock */
#include "progress.h" /* for Curl_pgrsSetUploadSize */
#include "transfer.h"
#include "warnless.h"
#include <curl/curl.h>

const struct Curl_handler Curl_handler_rtmp = {
  "RTMP",                               /* scheme */
  ZERO_NULL,                           /* setup_connection */
  ZERO_NULL,                              /* do_it */
  ZERO_NULL,                            /* done */
  ZERO_NULL,                            /* do_more */
  ZERO_NULL,                         /* connect_it */
  ZERO_NULL,                            /* connecting */
  ZERO_NULL,                            /* doing */
  ZERO_NULL,                            /* proto_getsock */
  ZERO_NULL,                            /* doing_getsock */
  ZERO_NULL,                            /* domore_getsock */
  ZERO_NULL,                            /* perform_getsock */
  ZERO_NULL,                      /* disconnect */
  ZERO_NULL,                            /* readwrite */
  PORT_RTMP,                            /* defport */
  CURLPROTO_RTMP,                       /* protocol */
  PROTOPT_NONE                          /* flags*/
};

const struct Curl_handler Curl_handler_rtmpt = {
  "RTMPT",                              /* scheme */
  ZERO_NULL,                           /* setup_connection */
  ZERO_NULL,                              /* do_it */
  ZERO_NULL,                            /* done */
  ZERO_NULL,                            /* do_more */
  ZERO_NULL,                         /* connect_it */
  ZERO_NULL,                            /* connecting */
  ZERO_NULL,                            /* doing */
  ZERO_NULL,                            /* proto_getsock */
  ZERO_NULL,                            /* doing_getsock */
  ZERO_NULL,                            /* domore_getsock */
  ZERO_NULL,                            /* perform_getsock */
  ZERO_NULL,                      /* disconnect */
  ZERO_NULL,                            /* readwrite */
  PORT_RTMPT,                           /* defport */
  CURLPROTO_RTMPT,                      /* protocol */
  PROTOPT_NONE                          /* flags*/
};

const struct Curl_handler Curl_handler_rtmpe = {
  "RTMPE",                              /* scheme */
  ZERO_NULL,                           /* setup_connection */
  ZERO_NULL,                              /* do_it */
  ZERO_NULL,                            /* done */
  ZERO_NULL,                            /* do_more */
  ZERO_NULL,                         /* connect_it */
  ZERO_NULL,                            /* connecting */
  ZERO_NULL,                            /* doing */
  ZERO_NULL,                            /* proto_getsock */
  ZERO_NULL,                            /* doing_getsock */
  ZERO_NULL,                            /* domore_getsock */
  ZERO_NULL,                            /* perform_getsock */
  ZERO_NULL,                      /* disconnect */
  ZERO_NULL,                            /* readwrite */
  PORT_RTMP,                            /* defport */
  CURLPROTO_RTMPE,                      /* protocol */
  PROTOPT_NONE                          /* flags*/
};

const struct Curl_handler Curl_handler_rtmpte = {
  "RTMPTE",                             /* scheme */
  ZERO_NULL,                           /* setup_connection */
  ZERO_NULL,                              /* do_it */
  ZERO_NULL,                            /* done */
  ZERO_NULL,                            /* do_more */
  ZERO_NULL,                         /* connect_it */
  ZERO_NULL,                            /* connecting */
  ZERO_NULL,                            /* doing */
  ZERO_NULL,                            /* proto_getsock */
  ZERO_NULL,                            /* doing_getsock */
  ZERO_NULL,                            /* domore_getsock */
  ZERO_NULL,                            /* perform_getsock */
  ZERO_NULL,                      /* disconnect */
  ZERO_NULL,                            /* readwrite */
  PORT_RTMPT,                           /* defport */
  CURLPROTO_RTMPTE,                     /* protocol */
  PROTOPT_NONE                          /* flags*/
};

const struct Curl_handler Curl_handler_rtmps = {
  "RTMPS",                              /* scheme */
  ZERO_NULL,                           /* setup_connection */
  ZERO_NULL,                              /* do_it */
  ZERO_NULL,                            /* done */
  ZERO_NULL,                            /* do_more */
  ZERO_NULL,                         /* connect_it */
  ZERO_NULL,                            /* connecting */
  ZERO_NULL,                            /* doing */
  ZERO_NULL,                            /* proto_getsock */
  ZERO_NULL,                            /* doing_getsock */
  ZERO_NULL,                            /* domore_getsock */
  ZERO_NULL,                            /* perform_getsock */
  ZERO_NULL,                      /* disconnect */
  ZERO_NULL,                            /* readwrite */
  PORT_RTMPS,                           /* defport */
  CURLPROTO_RTMPS,                      /* protocol */
  PROTOPT_NONE                          /* flags*/
};

const struct Curl_handler Curl_handler_rtmpts = {
  "RTMPTS",                             /* scheme */
  ZERO_NULL,                           /* setup_connection */
  ZERO_NULL,                              /* do_it */
  ZERO_NULL,                            /* done */
  ZERO_NULL,                            /* do_more */
  ZERO_NULL,                         /* connect_it */
  ZERO_NULL,                            /* connecting */
  ZERO_NULL,                            /* doing */
  ZERO_NULL,                            /* proto_getsock */
  ZERO_NULL,                            /* doing_getsock */
  ZERO_NULL,                            /* domore_getsock */
  ZERO_NULL,                            /* perform_getsock */
  ZERO_NULL,                      /* disconnect */
  ZERO_NULL,                            /* readwrite */
  PORT_RTMPS,                           /* defport */
  CURLPROTO_RTMPTS,                     /* protocol */
  PROTOPT_NONE                          /* flags*/
};

--------------------------------------------
File: \src\curl_rtmp.h
Comment: comment conditional compilation #ifdef USE_LIBRTMP and #endif
Code:

//#ifdef USE_LIBRTMP
extern const struct Curl_handler Curl_handler_rtmp;
extern const struct Curl_handler Curl_handler_rtmpt;
extern const struct Curl_handler Curl_handler_rtmpe;
extern const struct Curl_handler Curl_handler_rtmpte;
extern const struct Curl_handler Curl_handler_rtmps;
extern const struct Curl_handler Curl_handler_rtmpts;
//#endif

--------------------------------------------
File: \src\url.c
Comment: comment conditional compilation #ifdef USE_LIBRTMP and #endif
Code:

//#ifdef USE_LIBRTMP
  &Curl_handler_rtmp,
  &Curl_handler_rtmpt,
  &Curl_handler_rtmpe,
  &Curl_handler_rtmpte,
  &Curl_handler_rtmps,
  &Curl_handler_rtmpts,
//#endif

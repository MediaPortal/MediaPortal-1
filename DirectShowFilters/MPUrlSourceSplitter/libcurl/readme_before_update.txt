
The purpose of this document is to remark all changes made in libcurl by any developer. These same changes
(or at least their meaning) must be made in next releases of libcurl.
 
Changes in libcurl 7.31.0:

File: \include\curl\curl.h
Comment: define RTMP callback function
Code:

#ifdef USE_LIBRTMP

typedef void (*curl_rtmp_log_callback) (struct RTMP *r, int level, const char *fmt, va_list);

#endif

--------------------------------------------
Comment: in CURLoption enum define RTMP callback function and RTMP user data for curl_easy_setopt() method
Code:

/* callback log function for RTMP protocol */
CINIT(RTMP_LOG_CALLBACK, FUNCTIONPOINT, 219),

/* user data (reference) for RTMP log function */
CINIT(RTMP_LOG_USERDATA, OBJECTPOINT, 220),

--------------------------------------------
Comment: in CURLINFO enum add RTMP total duration and RTMP current time for curl_easy_getinfo() method
Code:

CURLINFO_RTMP_TOTAL_DURATION    = CURLINFO_DOUBLE + 43,
CURLINFO_RTMP_CURRENT_TIME      = CURLINFO_DOUBLE + 44,

CURLINFO_LASTONE          = 45

--------------------------------------------
File: \include\urldata.h
Comment: in struct UserDefined add RTMP callack, user data, total duration and current time
Code:

#ifdef USE_LIBRTMP
  curl_rtmp_log_callback frtmp_log_func;   /* function that write RTMP protocol log data  */
  void *curl_rtmp_log_user_data; /* user data for RTMP log callback */
  double rtmp_total_duration;   /* total duration of stream */
  double rtmp_current_time;   /* current stream time */
#endif

--------------------------------------------
File: \src\curl_rtmp.c
Comment: include log.h from librtmp

#include <librtmp/log.h>

--------------------------------------------
Comment: in rtmp_setup() method add after line 'RTMP_Init(r);'
Code:

if (conn->data->set.frtmp_log_func != NULL)
{
  RTMP_LogSetCallback(r, conn->data->set.frtmp_log_func);
}
r->m_logUserData = conn->data->set.curl_rtmp_log_user_data;

--------------------------------------------
Comment: in rtmp_connect() method add at the beginning
Code:

int on = 1;

--------------------------------------------
Comment: in rtmp_connect() method replace after 'curlx_nonblock(r->m_sb.sb_socket, FALSE);' and before 'if(!RTMP_Connect1(r, NULL))'
Code:

if (conn->data->set.connecttimeout != 0)
{
  // connect timeout set
  // timeout is divided by three to make at least two connection attempts
  tv = max((int)conn->data->set.connecttimeout / 3, 1000);
}

RTMP_Log(r, RTMP_LOGDEBUG, "socket timeout: %d (ms)", tv);

if (setsockopt(r->m_sb.sb_socket, SOL_SOCKET, SO_RCVTIMEO, (char *)&tv, sizeof(tv)) == SOCKET_ERROR)
{
  RTMP_Log(r, RTMP_LOGWARNING, "error while setting timeout on socket: %d", WSAGetLastError());
}

if (setsockopt(r->m_sb.sb_socket, IPPROTO_TCP, TCP_NODELAY, (char *) &on, sizeof(on)) == SOCKET_ERROR)
{
  RTMP_Log(r, RTMP_LOGWARNING, "error while setting TCP no delay on socket: %d", WSAGetLastError());
}

--------------------------------------------
File: \src\getinfo.c
Comment: in getinfo_double() method add before 'default' statement in 'switch(info)' switch
Code:

#ifdef USE_LIBRTMP
  case CURLINFO_RTMP_TOTAL_DURATION:
    *param_doublep = data->set.rtmp_total_duration;
    break;
  case CURLINFO_RTMP_CURRENT_TIME:
    *param_doublep = data->set.rtmp_current_time;
    break;
#endif

--------------------------------------------
File: \src\transfer.c
Comment: include rtmp.h
Code:

#ifdef USE_LIBRTMP
#include <librtmp/rtmp.h>
#endif

--------------------------------------------
Comment: in Curl_readwrite() method add at the beginning
Code:

#ifdef USE_LIBRTMP
  bool rtmpProtocolActive = false;
  bool rtmpProtocolFinished = false;
#endif

--------------------------------------------
Comment: in Curl_readwrite() method replace
Code to replace:

  if((k->keepon & KEEP_RECV) &&
     ((select_res & CURL_CSELECT_IN) || conn->bits.stream_was_rewound
     )) {

         result = readwrite_data(data, conn, k, &didwhat, done);

         if(result || *done)
           return result;
  }

Code: 

#ifdef USE_LIBRTMP
  rtmpProtocolActive = (conn->handler->protocol & (CURLPROTO_RTMP | CURLPROTO_RTMPE | CURLPROTO_RTMPS | CURLPROTO_RTMPT | CURLPROTO_RTMPTE | CURLPROTO_RTMPTS));

  if (rtmpProtocolActive)
  {
    // check if RTMP protocol has finished transmission
    RTMP *r = (RTMP *)conn->proto.generic;
    if ((r->m_read.status == RTMP_READ_COMPLETE))
    {
      // RTMP protocol finished transmission
      rtmpProtocolFinished = true;
    }
  }

#endif

  if((k->keepon & KEEP_RECV) &&
     ((select_res & CURL_CSELECT_IN) || conn->bits.stream_was_rewound
#ifdef USE_LIBRTMP
     // in case of RTMP protocol read always data independently of socket state
     // in another case can be some data unprocessed and we will be waiting for no reason
     || rtmpProtocolActive
#endif
     )) {
#ifdef USE_LIBRTMP
       if ((rtmpProtocolActive && (!rtmpProtocolFinished)) || (!rtmpProtocolActive))
       {
#endif
         result = readwrite_data(data, conn, k, &didwhat, done);

#ifdef USE_LIBRTMP
         if (rtmpProtocolActive)
         {
           RTMP *r = (RTMP *)conn->proto.generic;
           if ((r->m_read.status == RTMP_READ_COMPLETE))
           {
             // set done flag
             *done = TRUE;
           }
         }
#endif

         if(result || *done)
           return result;
#ifdef USE_LIBRTMP
       }
#endif
  }

--------------------------------------------
File: \src\url.c
Comment: in Curl_setopt() method add before 'default' statement in 'switch(option)' switch
Code:

#ifdef USE_LIBRTMP
  case CURLOPT_RTMP_LOG_CALLBACK:
    /*
     * Set RTMP protocol log callback
     */
    data->set.frtmp_log_func = va_arg(param, curl_rtmp_log_callback);
    break;
  case CURLOPT_RTMP_LOG_USERDATA:
    /*
    * Set RTMP protocol log callback user data
    */
    data->set.curl_rtmp_log_user_data = va_arg(param, void *);
    break;
#endif

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
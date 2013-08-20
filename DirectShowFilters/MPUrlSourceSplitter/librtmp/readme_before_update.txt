
The purpose of this document is to remark all changes made in librtmp by any developer. These same changes
(or at least their meaning) must be made in next releases of librtmp.
 
Changes in Commit: 7340f6dbc6b3c8e552baab2e5a891c2de75cddcc:

File: \amf.h, \dh.h, \handshake.h, \http.h
Comment: add to each method as first parameter 'struct RTMP *'
Code:

e.g. from:
  char *AMF_EncodeString(char *output, char *outend, const AVal * str);
to:
  char *AMF_EncodeString(struct RTMP *r, char *output, char *outend, const AVal * str);

--------------------------------------------

File: \log.h
Comment: include 'rtmp.h'
Code:

#include "rtmp.h"

--------------------------------------------

Comment: replace code
Code to replace:

extern RTMP_LogLevel RTMP_debuglevel;

typedef void (RTMP_LogCallback)(int level, const char *fmt, va_list);
void RTMP_LogSetCallback(RTMP_LogCallback *cb);
void RTMP_LogSetOutput(FILE *file);
void RTMP_LogPrintf(const char *format, ...);
void RTMP_LogStatus(const char *format, ...);
void RTMP_Log(int level, const char *format, ...);
void RTMP_LogHex(int level, const uint8_t *data, unsigned long len);
void RTMP_LogHexString(int level, const uint8_t *data, unsigned long len);
void RTMP_LogSetLevel(RTMP_LogLevel lvl);
RTMP_LogLevel RTMP_LogGetLevel(void);

Code:

//extern RTMP_LogLevel RTMP_debuglevel;

typedef void (RTMP_LogCallback)(struct RTMP *r, int level, const char *fmt, va_list);
void RTMP_LogSetCallback(struct RTMP *r, RTMP_LogCallback *cb);
//void RTMP_LogSetOutput(FILE *file);
//void RTMP_LogPrintf(const char *format, ...);
//void RTMP_LogStatus(const char *format, ...);
void RTMP_Log(struct RTMP *r, int level, const char *format, ...);
void RTMP_LogHex(struct RTMP *r, int level, const uint8_t *data, unsigned long len);
void RTMP_LogHexString(struct RTMP *r, int level, const uint8_t *data, unsigned long len);
//void RTMP_LogSetLevel(struct RTMP *r, RTMP_LogLevel lvl);
//RTMP_LogLevel RTMP_LogGetLevel(struct RTMP *r);


--------------------------------------------

File: \rtmp_sys.h
Comment: comment code
Code:

//#define EWOULDBLOCK	WSAETIMEDOUT	/* we don't use nonblocking, but we do use timeouts */

--------------------------------------------

File: \rtmp.h
Comment: include 'log.h'
Code:

#include "log.h"

--------------------------------------------

Comment:
Code to replace:

  void RTMPPacket_Reset(RTMPPacket *p);
  void RTMPPacket_Dump(RTMPPacket *p);
  int RTMPPacket_Alloc(RTMPPacket *p, int nSize);
  void RTMPPacket_Free(RTMPPacket *p);

Code:

  void RTMPPacket_Reset(struct RTMP *r, RTMPPacket *p);
  void RTMPPacket_Dump(struct RTMP *r, RTMPPacket *p);
  int RTMPPacket_Alloc(struct RTMP *r, RTMPPacket *p, int nSize);
  void RTMPPacket_Free(struct RTMP *r, RTMPPacket *p);

--------------------------------------------

Comment: add to struct RTMP
Code:

    void *m_logUserData;
    RTMP_LogCallback *m_logCallback;

--------------------------------------------

Comment: add 'RTMP *r' as first parameter to these methods
Code:

  int RTMP_ParseURL(RTMP *r, const char *url, int *protocol, AVal *host, unsigned int *port, AVal *playpath, AVal *app);
  void RTMP_ParsePlaypath(RTMP *r, AVal *in, AVal *out);
  int RTMP_FindFirstMatchingProperty(RTMP *r, AMFObject *obj, const AVal *name, AMFObjectProperty * p);
  int RTMPSockBuf_Fill(RTMP *r, RTMPSockBuf *sb);
  int RTMPSockBuf_Send(RTMP *r, RTMPSockBuf *sb, const char *buf, int len);
  int RTMPSockBuf_Close(RTMP *r, RTMPSockBuf *sb);
  int RTMP_HashSWF(RTMP *r, const char *url, unsigned int *size, unsigned char *hash, int age);

--------------------------------------------

File: \amf.c, \hashswf.c, \parseurl.c
Comment: to all methods add 'RTMP *r' as first parameter also add 'r' as first parameter to calling methods (best method is compiling)
Code:

--------------------------------------------

File: \log.c
Comment: comment lines
Code:

//RTMP_LogLevel RTMP_debuglevel = RTMP_LOGERROR;

//static RTMP_LogCallback rtmp_log_default, *cb = rtmp_log_default;

--------------------------------------------

Comment: comment methods
Code:

//static void rtmp_log_default(int level, const char *format, va_list vl)

//void RTMP_LogSetOutput(FILE *file)

//void RTMP_LogSetLevel(RTMP *r, RTMP_LogLevel level)

//RTMP_LogLevel RTMP_LogGetLevel(RTMP *r)

//void RTMP_LogPrintf(const char *format, ...)

//void RTMP_LogStatus(const char *format, ...)

--------------------------------------------

Comment: change methods
Code:

void RTMP_LogSetCallback(RTMP *r, RTMP_LogCallback *cbp)
{
  if (r != NULL)
  {
    r->m_logCallback = cbp;
  }
}

void RTMP_Log(r, RTMP *r, int level, const char *format, ...)
{
  va_list args;
  va_start(args, format);

  if (r != NULL)
  {
    if (r->m_logCallback != NULL)
    {
      r->m_logCallback(r, level, format, args);
    }
  }

  va_end(args);
}

--------------------------------------------

Comment: change parameters of methods
Code:

void RTMP_LogHex(RTMP *r, int level, const uint8_t *data, unsigned long len)

void RTMP_LogHexString(RTMP *r, int level, const uint8_t *data, unsigned long len)

--------------------------------------------

Comment: remove this code in RTMP_LogHex() method
Code:

if ( level > RTMP_debuglevel )
    return;

--------------------------------------------

Comment: change this code 'if ( !data || level > RTMP_debuglevel )' in RTMP_LogHexString() method
Code:

if ( !data )

--------------------------------------------

File: \rtmp.c
Comment: all methods must have first parameter 'RTMP *r'
Code:

--------------------------------------------

Comment: add after '#include "log.h"'
Code: 

#undef EWOULDBLOCK
#define EWOULDBLOCK	WSAETIMEDOUT	/* we don't use nonblocking, but we do use timeouts */

--------------------------------------------

Comment: in RTMP_ConnectStream() method add to beginning
Code: 

int tempSeekTime = 0;

--------------------------------------------

Comment: in RTMP_ConnectStream() method comment
Code: 

/*if (seekTime > 0)
    r->Link.seekTime = seekTime;*/

--------------------------------------------

Comment: in RTMP_ConnectStream() method add before 'while (!r->m_bPlaying && RTMP_IsConnected(r) && RTMP_ReadPacket(r, &packet))'
Code: 

RTMP_Log(r, RTMP_LOGDEBUG, "ConnectStream(): Start");
RTMP_Log(r, RTMP_LOGDEBUG, "ConnectStream(): link seek time: %d, seek time: %d", r->Link.seekTime, seekTime);

tempSeekTime = r->Link.seekTime;
r->Link.seekTime = 0;

--------------------------------------------

Comment: in RTMP_ConnectStream() method add after 'while (!r->m_bPlaying && RTMP_IsConnected(r) && RTMP_ReadPacket(r, &packet))' cycle
Code: 

r->Link.seekTime = tempSeekTime;

if ((r->Link.seekTime > 0) && (r->m_bPlaying))
{
  RTMP_Log(r, RTMP_LOGDEBUG, "ConnectStream(): playing, trying to seek: %d", r->Link.seekTime);
  if (!RTMP_SendSeek(r, r->Link.seekTime))
  {
    RTMP_Log(r, RTMP_LOGDEBUG, "ConnectStream(): playing, seeking failed");
    RTMP_Close(r);
  }
}

RTMP_Log(r, RTMP_LOGDEBUG, "ConnectStream(): End");

--------------------------------------------

Comment: in options[] array add auth option before { {NULL,0}, 0, 0}
Code:

{ AVC("auth"),      OFF(Link.auth),          OPT_STR, 0,
"Authentication string to be appended to the connect string" },

--------------------------------------------

Comment: in RTMP_SetupURL() method add changing flags when auth string is appended (before if (!r->Link.tcUrl.av_len) { ... } )
Code:

if (r->Link.auth.av_len)
{
  r->Link.lFlags |= RTMP_LF_AUTH;
}

--------------------------------------------

Comment: in HandleCtrl() method replace 'else if (r->Link.SWFSize)'
Code:

/*else*/ if (r->Link.SWFSize)

--------------------------------------------

Comment: in Read_1_Packet() method replace 'int rtnGetNextMediaPacket = 0, ret = RTMP_READ_EOF;'
Code:

int rtnGetNextMediaPacket = 0, ret = RTMP_READ_ERROR;

--------------------------------------------

Comment: in RTMP_Read() method replace 'return total;'
Code:

 return (r->m_read.status == RTMP_READ_ERROR) ? RTMP_READ_ERROR : total;
#pragma once

#include "liveMedia.hh"
#include "BasicUsageEnvironment.hh"
#include "MPTaskScheduler.h"
#include "GroupsockHelper.hh"

#include "MemoryBuffer.h"
#include "TSThread.h"
#include "MPRTSPClient.h"

#define MAX_DURATION_DESCRIBE_RESPONSE_BYTE_COUNT 4096

//RTSP response - default timeouts in milliseconds
//Actual values are set via registry keya (in TsReader.cpp)
#define TIMEOUT_GENERIC_RTSP_RESPONSE 500
#define TIMEOUT_FILE_ACTION_RTSP_RESPONSE 2000

class CRTSPClient: public TSThread
{
  public:
    CRTSPClient(CMemoryBuffer& buffer);
    virtual ~CRTSPClient(void);

    long Duration();

    bool OpenStream(char* url);
    bool Play(double start,double duration);
    void Continue();
    bool IsPaused();
    bool Pause();
    bool UpdateDuration();
    void Stop();
    DWORD  m_regRtspGenericTimeout;
    DWORD  m_regRtspFileTimeout;

  private:
    bool SetupStreams();
    bool InternalPlay(double startPoint);
    void Shutdown();

    // Synchronous commands only!
    HANDLE m_genericResponseEvent;
    int m_genericResponseResultCode;
    static void OnGenericResponseReceived(RTSPClient* client, int resultCode, char* resultString);

    // Asynchronous duration updates.
    HANDLE m_durationDescribeResponseEvent;
    int m_durationDescribeResponseResultCode;
    char m_durationDescribeResponseResultString[MAX_DURATION_DESCRIBE_RESPONSE_BYTE_COUNT];
    static void OnDurationDescribeResponseReceived(RTSPClient* client, int resultCode, char* resultString);

    void StartBufferThread();
    void StopBufferThread();
    virtual void ThreadProc();

    CMemoryBuffer& m_buffer;
    UsageEnvironment* m_env;
    MPRTSPClient* m_client;
    MediaSession* m_session;
    double m_duration;
    char m_url[2048];
    bool m_isSetup;
    volatile bool m_isBufferThreadActive;
    bool m_isPaused;
    volatile bool m_updateDuration;
    volatile bool m_recreateUpdateDurationClient;
};

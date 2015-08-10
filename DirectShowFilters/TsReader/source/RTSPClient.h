#pragma once

#include "liveMedia.hh"
#include "BasicUsageEnvironment.hh"
#include "MPTaskScheduler.h"
#include "GroupsockHelper.hh"

#include "MemoryBuffer.h"
#include "TSThread.h"
#include "MPRTSPClient.h"

#define MAX_DURATION_DESCRIBE_RESPONSE_BYTE_COUNT 4096

class CRTSPClient: public TSThread
{
  public:
    CRTSPClient(CMemoryBuffer& buffer);
    virtual ~CRTSPClient(void);

	  bool IsRunning();
	  long Duration();

    bool OpenStream(char* url);
    bool Play(double fStart,double fDuration);
	  void Continue();
    bool IsPaused();
    bool Pause();
    bool UpdateDuration();
    void Stop();

	  void FillBuffer(DWORD byteCount);

  protected:
    bool SetupStreams();
    bool InternalPlay(double startPoint);
    void Shutdown();

    CMemoryBuffer& m_buffer;
	  UsageEnvironment* m_env;
	  MPRTSPClient* m_ourClient;
	  MediaSession* m_session;

    // Synchronous commands only!
    HANDLE m_genericResponseEvent;
    int m_genericResponseResultCode;
    static void OnGenericResponseReceived(RTSPClient* client, int resultCode, char* resultString);

    // Asynchronous duration updates.
    HANDLE m_durationDescribeResponseEvent;
    int m_durationDescribeResponseResultCode;
    char m_durationDescribeResponseResultString[MAX_DURATION_DESCRIBE_RESPONSE_BYTE_COUNT];
    static void OnDurationDescribeResponseReceived(RTSPClient* client, int resultCode, char* resultString);
    bool InternalUpdateDuration(MPRTSPClient* client);
	
  private:
    //thread
	  void StartBufferThread();
	  void StopBufferThread();
	  virtual void ThreadProc();
    bool m_isBufferThreadActive;
	  long m_duration;
	  char m_url[2048];
	  bool m_isRunning;
    bool m_isPaused;
};

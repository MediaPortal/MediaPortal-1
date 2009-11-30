#pragma once

#include "liveMedia.hh"
#include "BasicUsageEnvironment.hh"
#include "MPTaskScheduler.h"
#include "GroupsockHelper.hh"

#include "MemoryBuffer.h"
#include "TSThread.h"

class CRTSPClient: public TSThread
{
public:
  CRTSPClient(CMemoryBuffer& buffer);
  virtual ~CRTSPClient(void);
  bool Initialize();
  bool OpenStream(char* url);
  bool Play(double fStart);
  void Stop();
	bool IsRunning();
	long Duration();
	bool Run();
	bool Pause();
  bool IsPaused();
	void Continue();
	void FillBuffer(DWORD byteCount);

protected:
  CMemoryBuffer& m_buffer;
  Medium* createClient(UsageEnvironment& env,int verbosityLevel, char const* applicationName) ;
  char* getOptionsResponse(Medium* client, char const* url,char* username, char* password) ;
  char* getSDPDescriptionFromURL(Medium* client, char const* url,
			         char const* username, char const* password,
			         char const* /*proxyServerName*/,
			         unsigned short /*proxyServerPortNum*/,
			         unsigned short /*clientStartPort*/) ;
  Boolean clientSetupSubsession(Medium* client, MediaSubsession* subsession,Boolean streamUsingTCP) ;
  Boolean clientStartPlayingSession(Medium* client,MediaSession* session) ;
  Boolean clientTearDownSession(Medium* client,MediaSession* session) ;
  void closeMediaSinks();
  void tearDownStreams();
  void shutdown();
  bool setupStreams();
  bool startPlayingStreams() ;
  void checkForPacketArrival(void* /*clientData*/) ;
	UsageEnvironment* m_env;
	Medium* m_ourClient ;
	MediaSession* m_session ;

  Boolean allowProxyServers ;
  Boolean controlConnectionUsesTCP ;
  Boolean supportCodecSelection ;
  char const* clientProtocolName ;
  portNumBits tunnelOverHTTPPortNum ;
  unsigned statusCode ;
  char const* singleMedium ;
  unsigned short desiredPortNum   ;
  Boolean createReceivers ;
  int simpleRTPoffsetArg ;
  unsigned socketInputBufferSize ;
  Boolean streamUsingTCP ;
  unsigned fileSinkBufferSize ;
  Boolean oneFilePerFrame ;
	
public:
  //thread
	void StartBufferThread();
	void StopBufferThread();
	virtual void ThreadProc();
  bool m_BufferThreadActive;
	long m_duration;
	double m_fStart;
	char m_url[2048];
	bool m_bRunning;
  bool m_bPaused;
};

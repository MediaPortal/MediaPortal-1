#include <afx.h>
#include <afxwin.h>

#include <winsock2.h>
#include <ws2tcpip.h>
#include <streams.h>
#include "tsreader.h"
#include "rtspclient.h"
#include "MemorySink.h"

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"


extern void LogDebug(const char* fmt, ...);
extern DWORD m_tGTStartTime;

// unit = milliseconds
#define TIMEOUT_GENERIC_RTSP_RESPONSE 500

//Size in bytes of the CMemorySink buffer (TRANSPORT_PACKET_SIZE * TRANSPORT_PACKETS_PER_NETWORK_PACKET * 15)
#define MEM_SINK_BUF_SIZE (188*7*15)


CRTSPClient::CRTSPClient(CMemoryBuffer& buffer)
  : m_buffer(buffer)
{
  LogDebug("CRTSPClient::CRTSPClient()");
  m_duration = 7200 * 1000;
  m_session = NULL;
  m_client = NULL;
  m_isSetup = false;
  m_isBufferThreadActive = false;
  m_isPaused = false;
  m_updateDuration = false;

  m_genericResponseEvent = CreateEvent(NULL, TRUE, FALSE, NULL);
  m_durationDescribeResponseEvent = CreateEvent(NULL, TRUE, FALSE, NULL);
  if (m_genericResponseEvent == NULL || m_durationDescribeResponseEvent == NULL) 
  {
    LogDebug("CRTSPClient::CRTSPClient(): failed to create events");
    return;
  }

  TaskScheduler* scheduler = MPTaskScheduler::createNew();
  if (scheduler == NULL) 
  {
    LogDebug("CRTSPClient::CRTSPClient(): failed to create task scheduler");
    return;
  }

  m_env = BasicUsageEnvironment::createNew(*scheduler);
  if (m_env == NULL) 
  {
    LogDebug("CRTSPClient::CRTSPClient(): failed to create usage environment");
    delete scheduler;
    return;
  }
}

CRTSPClient::~CRTSPClient()
{
  LogDebug("CRTSPClient::~CRTSPClient()");
  Shutdown();
  if (m_env)
  {
    TaskScheduler* scheduler = &m_env->taskScheduler();
    m_env->reclaim();
    m_env = NULL;
    delete scheduler;
  }
  if (m_genericResponseEvent != NULL)
  {
    CloseHandle(m_genericResponseEvent);
    m_genericResponseEvent = NULL;
  }
  if (m_durationDescribeResponseEvent != NULL)
  {
    CloseHandle(m_durationDescribeResponseEvent);
    m_durationDescribeResponseEvent = NULL;
  }
}

void SubsessionAfterPlaying(void* clientData) 
{
  LogDebug("CRTSPClient::SubsessionAfterPlaying()");
}

void SubsessionByeHandler(void* clientData) 
{
  LogDebug("CRTSPClient::SubsessionByeHandler()");
}

bool CRTSPClient::SetupStreams()
{
  LogDebug("CRTSPClient::SetupStreams()");
  bool result = false;
  MediaSubsessionIterator iter(*m_session);
  MediaSubsession* subsession;
  while ((subsession = iter.next()) != NULL) 
  {
    if (subsession->clientPortNum() == 0) continue; // port # was not set

    LogDebug("CRTSPClient::SetupStreams(): send RTSP SETUP");
    ResetEvent(m_genericResponseEvent);
    m_client->sendSetupCommand(*subsession, &CRTSPClient::OnGenericResponseReceived);
    if (WaitForSingleObject(m_genericResponseEvent, TIMEOUT_GENERIC_RTSP_RESPONSE) == WAIT_TIMEOUT)
    {
      LogDebug("CRTSPClient::SetupStreams(): RTSP SETUP timed out");
    }
    else if (m_genericResponseResultCode != 0)
	  {
      LogDebug("CRTSPClient::SetupStreams(): RTSP SETUP failed, result code = %d, message = %s", m_genericResponseResultCode, m_env->getResultMsg());
    }
    else
    {
      LogDebug("CRTSPClient::SetupStreams(): RTSP SETUP succeeded, port = %d", subsession->clientPortNum());
      result = true;
    }
  }
  return result;
}

void CRTSPClient::Shutdown()
{
  LogDebug("CRTSPClient::Shutdown()");

  // Teardown, then shutdown, any outstanding RTP/RTCP subsessions
  if (m_session != NULL && m_client != NULL && m_isSetup)
  {
    LogDebug("CRTSPClient::Shutdown(): send RTSP TEARDOWN");
    ResetEvent(m_genericResponseEvent);
    m_client->sendTeardownCommand(*m_session, &CRTSPClient::OnGenericResponseReceived);
    if (WaitForSingleObject(m_genericResponseEvent, TIMEOUT_GENERIC_RTSP_RESPONSE) == WAIT_TIMEOUT)
    {
      LogDebug("CRTSPClient::Shutdown(): RTSP TEARDOWN timed out");
    }
    else if (m_genericResponseResultCode != 0)
	  {
      LogDebug("CRTSPClient::Shutdown(): RTSP TEARDOWN failed, result code = %d, message = %s", m_genericResponseResultCode, m_env->getResultMsg());
    }
  }

  // LIVE555 will abort() the process if we cause it to try to read a socket
  // after the socket is closed. Since we're about to close all our sockets, we
  // therefore need to stop the LIVE555 thread now.
  StopBufferThread();

  if (m_session != NULL)
  {
    MediaSubsessionIterator iter(*m_session);
    MediaSubsession* subsession;
    while ((subsession = iter.next()) != NULL) 
    {
      Medium::close(subsession->sink);
      subsession->sink = NULL;
    }

    Medium::close(m_session);
    m_session = NULL;
  }

  m_isSetup = false;

  // Finally, shut down our client.
  if (m_client != NULL)
  {
    Medium::close(m_client);
    m_client = NULL;
  }
}

bool CRTSPClient::OpenStream(char* url)
{
  LogDebug("CRTSPClient::OpenStream()");
  if (m_env == NULL)
  {
    LogDebug("CRTSPClient::OpenStream(): environment is NULL");
    return false;
  }
	
  strcpy(m_url, url);
  if (m_client != NULL)
  {
    Shutdown();
  }
  LogDebug("CRTSPClient::OpenStream(): create RTSP client, url = %s", m_url);
  m_client = MPRTSPClient::createNew(this, *m_env, m_url, 0/*verbosity level*/, "TSFileSource");
  if (m_client == NULL) 
  {
    LogDebug("CRTSPClient::OpenStream(): failed to create RTSP client, message = %s", m_env->getResultMsg());
    return false;
  }

  // Thread has to be running so that LIVE555 can handle RTSP command
  // responses and duration updates in the background.
  StartBufferThread();

  if (!UpdateDuration())
  {
    Shutdown();
    return false;
  }
  LogDebug("CRTSPClient::OpenStream(): duration = %d", m_duration);

  // Create a media session object from the SDP description:
  m_session = MediaSession::createNew(*m_env, m_durationDescribeResponseResultString);
  if (m_session == NULL) 
  {
    LogDebug("CRTSPClient::OpenStream(): failed to create media session from RTSP DESCRIBE response, message = %s", m_env->getResultMsg());
    Shutdown();
    return false;
  } 
  else if (!m_session->hasSubsessions()) 
  {
    LogDebug("CRTSPClient::OpenStream(): media session has no sub-sessions");
    Shutdown();
    return false;
  }

  // Then, setup the RTP sources for the session:
  MediaSubsessionIterator iter(*m_session);
  MediaSubsession* subsession;
  Boolean madeProgress = False;
  int desiredPortNum = 0;
  while ((subsession = iter.next()) != NULL) 
  {
    if (desiredPortNum != 0) 
    {
      subsession->setClientPortNum(desiredPortNum);
      desiredPortNum += 2;
    }

    if (!subsession->initiate(-1)) 
    {
      LogDebug("CRTSPClient::OpenStream(): failed to create receiver for sub-session, medium name = %s, codec name = %s, message = %s", subsession->mediumName(), subsession->codecName(), m_env->getResultMsg());
      continue;
    }

    LogDebug("CRTSPClient::OpenStream(): created receiver for sub-session, medium name = %s, codec name = %s, port = %d", subsession->mediumName(), subsession->codecName(), subsession->clientPortNum());
    madeProgress = True;

    if (subsession->rtpSource() != NULL) 
    {
      // Because we're saving the incoming data, rather than playing
      // it in real time, allow an especially large time threshold
      // for reordering misordered incoming packets:
      increaseReceiveBufferTo(*m_env, subsession->rtpSource()->RTPgs()->socketNum(), 2000000);
      subsession->rtpSource()->setPacketReorderingThresholdTime(500000);  // 500 milliseconds
    }
  }

  if (!madeProgress || !SetupStreams()) 
  {
    Shutdown();
    return false;
  }
  m_isSetup = true;
	
  // Create output files (file sinks) for each sub-session:
  iter.reset();
  while ((subsession = iter.next()) != NULL) 
  {
    if (subsession->readSource() == NULL)
    {
      continue;
    }
    		
    CMemorySink* fileSink = CMemorySink::createNew(*m_env, m_buffer, MEM_SINK_BUF_SIZE);
    subsession->sink = fileSink;
    if (subsession->sink == NULL) 
    {
      LogDebug("CRTSPClient::OpenStream(): failed to create file sink, message = %s", m_env->getResultMsg());
      Shutdown();
      return false;
    }

    LogDebug("CRTSPClient::OpenStream(): created file sink");

    // Set handlers and start playing.
    subsession->sink->startPlaying(*(subsession->readSource()), SubsessionAfterPlaying, subsession);
    if (subsession->rtcpInstance() != NULL) 
    {
      subsession->rtcpInstance()->setByeHandler(SubsessionByeHandler,subsession);
    }
  }
  return true;
}

void CRTSPClient::Stop()
{
  LogDebug("CRTSPClient:Stop()");
  Shutdown();
  m_buffer.Clear();
  LogDebug("CRTSPClient:Stop(): done");
}

void CRTSPClient::StartBufferThread()
{
  if (!m_isBufferThreadActive)
  {
    LogDebug("CRTSPClient::StartBufferThread()");
    StartThread();
    LogDebug("CRTSPClient::StartBufferThread(): done");
  }
}

void CRTSPClient::StopBufferThread()
{
  if (!m_isBufferThreadActive)
  {
    return;
  }

  LogDebug("CRTSPClient::StopBufferThread()");
  m_isBufferThreadActive = false;
  StopThread(20000);
  LogDebug("CRTSPClient::StopBufferThread(): done");
}

long CRTSPClient::Duration()
{
  return m_duration;
}

void CRTSPClient::ThreadProc()
{
  HRESULT hr = S_OK;
  m_isBufferThreadActive = true;
  ::SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_ABOVE_NORMAL);
  LogDebug("CRTSPClient::ThreadProc(): thread started, thread ID = %d", GetCurrentThreadId());
  MPRTSPClient* client = MPRTSPClient::createNew(this, *m_env, m_url, 0/*verbosity level*/, "TSFileSource");
  if (client == NULL)
  {
    LogDebug("CRTSPClient::UpdateDuration(): failed to create RTSP client");
    return;
  }

  while (m_env != NULL && !ThreadIsStopping(1))
  {
    if (m_updateDuration)
    {
      // Unfortunately we must reset the URL before each request. LIVE55
      // updates the URL from the DESCRIBE response Content-Base header. That
      // header contains an undesirable trailing slash.
      client->SetUrl(m_url);

      client->sendDescribeCommand(&CRTSPClient::OnDurationDescribeResponseReceived);
      m_updateDuration = false;
    }
    for (int i = 0; i < 10; ++i)
    {
      if (!m_isBufferThreadActive)
      {
        break;
      }
      m_env->taskScheduler().doEventLoop(); 
    }
    if (!m_isBufferThreadActive)
    {
      break;
    }
  }

  LogDebug("CRTSPClient::ThreadProc(): thread stopping, thread ID = %d", GetCurrentThreadId());
  Medium::close(client);
  m_isBufferThreadActive = false;
  return;
}

bool CRTSPClient::Play(double start, double duration)
{
  // This clause is here to handle the Stop() -> Play() sequence. Currently
  // TsReader never uses Stop(), so it shouldn't be needed.
  if (m_client == NULL || m_session == NULL)
  {
    char* url = new char[strlen(m_url) + 1];
    if (url != NULL)
    {
      strcpy(url, m_url);
      bool openResult = OpenStream(url);
      delete[] url;
      if (!openResult)
      {
        Shutdown();
        return false;
      }
    }
  }

  //Sanity check the start value
  double dur = ((double)m_duration) / 1000.0;
  double maxDur = fmax(dur, duration); //Allow for m_duration being too low when timeshifting (value only updated every 4 seconds)
  if (maxDur > 0.0)
  {
    start = fmin(start, maxDur);
  }
  else
  {
    start = 0.0;
  }
  if (start < 0.0)
  {
    start = 0.0;
  }

  LogDebug("CRTSPClient::Play(): start = %f, duration = %f, m_duration = %f", (float)start, (float)duration, (float)dur);
  StartBufferThread();  // Note: thread expected to be running already. This is for "safety".
  if (!InternalPlay(start)) 
  {
    Shutdown();
    return false;
  }
  return true;
}

void CRTSPClient::Continue()
{
  if (m_client != NULL && m_session != NULL && m_isPaused)
  {
    InternalPlay(-1.0);
  }
}

bool CRTSPClient::InternalPlay(double startPoint)
{
  LogDebug("CRTSPClient::InternalPlay() - start = %f", (float)startPoint);
  if (m_client != NULL && m_session != NULL)
  {
    ResetEvent(m_genericResponseEvent);
    m_client->sendPlayCommand(*m_session, &CRTSPClient::OnGenericResponseReceived, startPoint);
    if (WaitForSingleObject(m_genericResponseEvent, TIMEOUT_GENERIC_RTSP_RESPONSE) == WAIT_TIMEOUT)
    {
      LogDebug("CRTSPClient::InternalPlay(): RTSP PLAY timed out");
      return false;
    }
    if (m_genericResponseResultCode != 0)
	  {
      LogDebug("CRTSPClient::InternalPlay(): RTSP PLAY failed, result code = %d, message = %s", m_genericResponseResultCode, m_env->getResultMsg());
      return false;
    }

    m_isPaused = false;
  }
  LogDebug("CRTSPClient::InternalPlay(): done");
  return true;
}

bool CRTSPClient::IsPaused()
{
  return m_isPaused;
}

bool CRTSPClient::Pause()
{
  LogDebug("CRTSPClient::Pause()");
  if (m_client != NULL && m_session != NULL)
  {
    ResetEvent(m_genericResponseEvent);
    m_client->sendPauseCommand(*m_session, &CRTSPClient::OnGenericResponseReceived);
    if (WaitForSingleObject(m_genericResponseEvent, TIMEOUT_GENERIC_RTSP_RESPONSE) == WAIT_TIMEOUT)
    {
      LogDebug("CRTSPClient::Pause(): RTSP PAUSE timed out");
      return false;
    }
    if (m_genericResponseResultCode != 0)
	  {
      LogDebug("CRTSPClient::Pause(): RTSP PAUSE failed, result code = %d, message = %s", m_genericResponseResultCode, m_env->getResultMsg());
      return false;
    }

    m_isPaused = true;
  }
  LogDebug("CRTSPClient::Pause(): done");
  return true;
}

void CRTSPClient::OnGenericResponseReceived(RTSPClient* client, int resultCode, char* resultString)
{
  CRTSPClient* rtspClient = (CRTSPClient*)((MPRTSPClient*)client)->Context();
  if (rtspClient != NULL)
  {
    rtspClient->m_genericResponseResultCode = resultCode;
    SetEvent(rtspClient->m_genericResponseEvent);
  }
  else
  {
    LogDebug("CRTSPClient::OnGenericResponseReceived(): unhandled response, code = %d, response = %s", resultCode, resultString == NULL ? "" : resultString);
  }
  if (resultString != NULL)
  {
    delete[] resultString;
  }
}

bool CRTSPClient::UpdateDuration()
{
  //LogDebug("CRTSPClient::UpdateDuration()");
  ResetEvent(m_durationDescribeResponseEvent);

  // Setting this variable triggers the "buffer" thread to send a DESCRIBE
  // command.
  m_updateDuration = true;

  // Wait for a response. Don't wait longer than the calling period (currently ~5000 ms).
  if (WaitForSingleObject(m_durationDescribeResponseEvent, 500) == WAIT_TIMEOUT)
  {
    LogDebug("CRTSPClient::UpdateDuration(): RTSP DESCRIBE timed out, message = %s", m_env->getResultMsg());
    return false;
  }

  //LogDebug("CRTSPClient::UpdateDuration(): result code = %d", m_durationDescribeResponseResultCode);
  if (m_durationDescribeResponseResultCode != 0)
	{
    LogDebug("CRTSPClient::UpdateDuration(): RTSP DESCRIBE failed, result code = %d, message = %s", m_durationDescribeResponseResultCode, m_env->getResultMsg());
    return false;
  }
  //LogDebug("CRTSPClient::UpdateDuration(): RTSP DESCRIBE response = %s", m_durationDescribeResponseResultString);

  char* rangeSection = strstr(m_durationDescribeResponseResultString, "a=range:npt=");
  if (rangeSection != NULL)
  {
    char* startString = rangeSection + strlen("a=range:npt=");
    char* endString = strstr(rangeSection, "-");
    if (endString != NULL)
    {
      endString++;
      double start = atof(startString);
      double end = atof(endString);

      //LogDebug("CRTSPClient::UpdateDuration(): range start = %f, range end = %f", start, end);
      m_duration = (end - start) * 1000.0;
      return true;
    }
  }
  return false;
}

void CRTSPClient::OnDurationDescribeResponseReceived(RTSPClient* client, int resultCode, char* resultString)
{
  //LogDebug("CRTSPClient::OnDurationDescribeResponseReceived(): code = %d, response = %s", resultCode, resultString == NULL ? "" : resultString);
  CRTSPClient* rtspClient = (CRTSPClient*)((MPRTSPClient*)client)->Context();
  if (rtspClient == NULL)
  {
    // This can happen if the client attempted to send a command, the command
    // was assumed to have timed out after some time, and the client was
    // subsequently destroyed as a result. This is the response to the command.
    // We can't do anything more than clean up.
    LogDebug("CRTSPClient::OnDurationDescribeResponseReceived(): RTSP client is NULL, code = %d, response = %s", resultCode, resultString == NULL ? "" : resultString);
    if (resultString != NULL)
    {
      delete[] resultString;
    }
    return;
  }

  rtspClient->m_durationDescribeResponseResultCode = resultCode;
  if (resultString == NULL)
  {
    rtspClient->m_durationDescribeResponseResultString[0] = NULL;
  }
  else
  {
    strncpy(rtspClient->m_durationDescribeResponseResultString, resultString, MAX_DURATION_DESCRIBE_RESPONSE_BYTE_COUNT);
    rtspClient->m_durationDescribeResponseResultString[MAX_DURATION_DESCRIBE_RESPONSE_BYTE_COUNT - 1] = NULL;
    delete[] resultString;
  }
  SetEvent(rtspClient->m_durationDescribeResponseEvent);
}
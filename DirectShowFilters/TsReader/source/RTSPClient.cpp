#include <afx.h>
#include <afxwin.h>

#include <winsock2.h>
#include <ws2tcpip.h>
#include <streams.h>
#include "rtspclient.h"
#include "MemorySink.h"

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

extern void LogDebug(const char *fmt, ...) ;

CRTSPClient::CRTSPClient(CMemoryBuffer& buffer)
:m_buffer(buffer)
{
  LogDebug("CRTSPClient::CRTSPClient()");
  allowProxyServers = False;
  controlConnectionUsesTCP = True;
  supportCodecSelection = False;
  clientProtocolName = "RTSP";
  tunnelOverHTTPPortNum = 0;
  statusCode = 0;
  singleMedium = NULL;
  desiredPortNum = 0;
  createReceivers = True;
  simpleRTPoffsetArg = -1;
  socketInputBufferSize = 0;
  streamUsingTCP = False;
  fileSinkBufferSize = 20000;
  oneFilePerFrame = False;
  m_BufferThreadActive=false;
  m_duration=7200*1000;
  m_fStart=0.0f;
  m_session=NULL;
  m_ourClient=NULL;
  m_bPaused=false;
}

CRTSPClient::~CRTSPClient()
{
  LogDebug("CRTSPClient::~CRTSPClient()");
  Medium::close(m_ourClient);
  m_ourClient = NULL;
}


Medium* CRTSPClient::createClient(UsageEnvironment& env,int verbosityLevel, char const* applicationName) 
{
  LogDebug("CRTSPClient::createClient()");
  return RTSPClient::createNew(env, verbosityLevel, applicationName,tunnelOverHTTPPortNum);
}

char* CRTSPClient::getOptionsResponse(Medium* client, char const* url,char* username, char* password) 
{
  LogDebug("CRTSPClient::getOptionsResponse()");
  RTSPClient* rtspClient = (RTSPClient*)client;
  return rtspClient->sendOptionsCmd(url, username, password);
}

char* CRTSPClient::getSDPDescriptionFromURL(Medium* client, char const* url,
			       char const* username, char const* password,
			       char const* /*proxyServerName*/,
			       unsigned short /*proxyServerPortNum*/,
			       unsigned short /*clientStartPort*/) 
{
  LogDebug("CRTSPClient::getSDPDescriptionFromURL()");
  RTSPClient* rtspClient = (RTSPClient*)client;
  char* result;
  if (username != NULL && password != NULL) 
  {
    result = rtspClient->describeWithPassword(url, username, password);
  } 
  else 
  {
    result = rtspClient->describeURL(url);
  }

  statusCode = rtspClient->describeStatus();
  return result;
}

char* CRTSPClient::getSDPDescription() 
{
//	LogDebug("CRTSPClient::getSDPDescription()");
  RTSPClient *client = (RTSPClient*)m_ourClient;
  RTSPClient *rtspClient = RTSPClient::createNew(client->envir(), 0, "TSFileSource", tunnelOverHTTPPortNum);
  char* result;
  result = rtspClient->describeURL(m_url);

//  LogDebug("CRTSPClient::getSDPDescription() statusCode = %d", rtspClient->describeStatus());
  Medium::close(rtspClient);

  return result;
}

Boolean CRTSPClient::clientSetupSubsession(Medium* client, MediaSubsession* subsession,Boolean streamUsingTCP) 
{
  LogDebug("CRTSPClient::clientSetupSubsession()");
  if (client == NULL || subsession == NULL) return False;
  RTSPClient* rtspClient = (RTSPClient*)client;
  return rtspClient->setupMediaSubsession(*subsession,False, streamUsingTCP);
}

Boolean CRTSPClient::clientStartPlayingSession(Medium* client,MediaSession* session) 
{
  LogDebug("CRTSPClient::clientStartPlayingSession()");
  if (client == NULL || session == NULL) return False;
  RTSPClient* rtspClient = (RTSPClient*)client;

  long dur=m_duration/1000;
  double fStart = m_fStart ;
  double fStartToEnd ;
  if (m_fDuration > 0.0)
  {
    fStartToEnd = m_fDuration-m_fStart ;
    if (fStartToEnd<0) fStartToEnd=0 ;
    fStart = dur - fStartToEnd ;
    if (fStart<0) fStart=0 ;
  }

  long diff=abs(dur-m_fStart);
//  if (diff <20 && m_fStart>1 )
//  {
//    m_fStart=dur+5;
//  }
  LogDebug("CRTSPClient::clientStartPlayingSession() play from %.3f / %.3f",fStart,(float)m_duration/1000);
  return rtspClient->playMediaSession(*session,fStart);
}

Boolean CRTSPClient::clientTearDownSession(Medium* client,MediaSession* session) 
{
  LogDebug("CRTSPClient::clientTearDownSession()");
  if (client == NULL || session == NULL) return False;
  RTSPClient* rtspClient = (RTSPClient*)client;
  return rtspClient->teardownMediaSession(*session);
}

void subsessionAfterPlaying(void* clientData) 
{
  LogDebug("CRTSPClient::subsessionAfterPlaying()");
}
void subsessionByeHandler(void* clientData) 
{
  LogDebug("CRTSPClient::subsessionByeHandler()");
}

void CRTSPClient::closeMediaSinks() 
{
  if (m_session == NULL) return;
  LogDebug("CRTSPClient::closeMediaSinks()");
  MediaSubsessionIterator iter(*m_session);
  MediaSubsession* subsession;
  while ((subsession = iter.next()) != NULL) 
  {
    Medium::close(subsession->sink);
    subsession->sink = NULL;
  }
}

void CRTSPClient::tearDownStreams() 
{
  if (m_session == NULL) return;
  LogDebug("CRTSPClient::tearDownStreams()");

  clientTearDownSession(m_ourClient, m_session);
}
bool CRTSPClient::setupStreams()
{
  //setup streams
  LogDebug("CRTSPClient::setupStreams()");
  Boolean madeProgress=False;
  MediaSubsessionIterator iter(*m_session);
  MediaSubsession *subsession;

  while ((subsession = iter.next()) != NULL) 
  {
    if (subsession->clientPortNum() == 0) continue; // port # was not set

    if (!clientSetupSubsession(m_ourClient, subsession, streamUsingTCP)) 
    {
      LogDebug( "Failed to setup %s %s %s" ,subsession->mediumName(),subsession->codecName(),m_env->getResultMsg() );;
    } 
    else 
    {
      LogDebug( "Setup %s %s %d %d" ,subsession->mediumName(),subsession->codecName(),subsession->clientPortNum(),subsession->clientPortNum()+1);;
      madeProgress = True;
    }
  }
  if (!madeProgress) 
  {
    shutdown();
    return false;
  }
  return true;
}

bool CRTSPClient::startPlayingStreams() 
{
  LogDebug("CRTSPClient::startPlayingStreams()");
  if (!clientStartPlayingSession(m_ourClient, m_session)) 
  {
    LogDebug("Failed to start playing session:%s " ,m_env ->getResultMsg() );
    shutdown();
    return false;
  } 
  else 
  {
    LogDebug("Started playing session");
  }
  return true;
}

void CRTSPClient::shutdown()
{
  LogDebug("CRTSPClient::shutdown()");
  if (m_env != NULL) 
  {
    //m_env->taskScheduler().unscheduleDelayedTask(sessionTimerTask);
    //m_env->taskScheduler().unscheduleDelayedTask(arrivalCheckTimerTask);
    //m_env->taskScheduler().unscheduleDelayedTask(interPacketGapCheckTimerTask);
    //m_env->taskScheduler().unscheduleDelayedTask(qosMeasurementTimerTask);
  }
  // Close our output files:
  closeMediaSinks();

  // Teardown, then shutdown, any outstanding RTP/RTCP subsessions
  tearDownStreams();
  Medium::close(m_session);

  // Finally, shut down our client:
  Medium::close(m_ourClient);;
  m_session=NULL;;
  m_ourClient=NULL;
}


bool CRTSPClient::Initialize()
{
  LogDebug("CRTSPClient::Initialize()");
  m_duration=7200*1000;
  TaskScheduler* scheduler = MPTaskScheduler::createNew();
  m_env = BasicUsageEnvironment::createNew(*scheduler);
	
  m_ourClient = createClient(*m_env, 0/*verbosityLevel*/, "TSFileSource");
  if (m_ourClient == NULL) 
  {
    LogDebug("Failed to create %s %s" ,clientProtocolName,m_env->getResultMsg() );
    shutdown();
    return false;
  }
  return true;
}

bool CRTSPClient::OpenStream(char* url)
{
  LogDebug("CRTSPClient::OpenStream()");
  m_session=NULL;
	
  strcpy(m_url,url);
  // Open the URL, to get a SDP description: 
  char* sdpDescription= getSDPDescriptionFromURL(m_ourClient, url, ""/*username*/, ""/*password*/,""/*proxyServerName*/, 0/*proxyServerPortNum*/,1234/*desiredPortNum*/);
  if (sdpDescription == NULL) 
  {
    LogDebug("Failed to get a SDP description from URL %s %s",url ,m_env->getResultMsg() );
    shutdown();
    return false;
  }
  //LogDebug("Opened URL %s %s",url,sdpDescription);

  char* range=strstr(sdpDescription,"a=range:npt=");
  if (range!=NULL)
  {
    char *pStart = range+strlen("a=range:npt=");
    char *pEnd = strstr(range,"-") ;
    if (pEnd!=NULL)
    {
      pEnd++ ;
      double Start=atof(pStart) ;
      double End=atof(pEnd) ;

      LogDebug("rangestart:%f rangeend:%f", Start,End);
      m_duration=((End-Start)*1000.0);
    }
  }
  // Create a media session object from this SDP description:
  m_session = MediaSession::createNew(*m_env, sdpDescription);
  delete[] sdpDescription;
  if (m_session == NULL) 
  {
    LogDebug("Failed to create a MediaSession object from the SDP description:%s ",m_env->getResultMsg());
    shutdown();
    return false;
  } 
  else if (!m_session->hasSubsessions()) 
  {
    LogDebug("This session has no media subsessions");
    shutdown();
    return false;
  }

  // Then, setup the "RTPSource"s for the session:
  MediaSubsessionIterator iter(*m_session);
  MediaSubsession *subsession;
  Boolean madeProgress = False;
  char const* singleMediumToTest = singleMedium;
  while ((subsession = iter.next()) != NULL) 
  {
    // If we've asked to receive only a single medium, then check this now:
    if (singleMediumToTest != NULL) 
    {
      if (strcmp(subsession->mediumName(), singleMediumToTest) != 0) 
      {
        LogDebug("Ignoring %s %s %s" , subsession->mediumName(),subsession->codecName(),singleMedium);
        continue;
      } 
      else 
      {
        // Receive this subsession only
        singleMediumToTest = "xxxxx";
        // this hack ensures that we get only 1 subsession of this type
      }
    }
    if (desiredPortNum != 0) 
    {
      subsession->setClientPortNum(desiredPortNum);
      desiredPortNum += 2;
    }

    if (createReceivers) 
    {
      if (!subsession->initiate(simpleRTPoffsetArg)) 
      {
        LogDebug("Unable to create receiver for %s %s %s" ,subsession->mediumName(),subsession->codecName(),m_env->getResultMsg());
      } 
      else 
      {
        LogDebug("Created receiver for %s %s %d %d " ,subsession->mediumName(),subsession->codecName(),subsession->clientPortNum(),subsession->clientPortNum()+1 );
        madeProgress = True;

        if (subsession->rtpSource() != NULL) 
        {
          // Because we're saving the incoming data, rather than playing
          // it in real time, allow an especially large time threshold
          // (1 second) for reordering misordered incoming packets:
          
          int socketNum= subsession->rtpSource()->RTPgs()->socketNum();
          LogDebug("rtsp:increaseReceiveBufferTo to 2000000 for s:%d",socketNum);
          increaseReceiveBufferTo( *m_env, socketNum, 2000000 );

          unsigned const thresh = 1000000; // 1 second 
          subsession->rtpSource()->setPacketReorderingThresholdTime(thresh);

          if (socketInputBufferSize > 0) 
          {
            // Set the RTP source's input buffer size as specified:
            int socketNum= subsession->rtpSource()->RTPgs()->socketNum();
            unsigned curBufferSize= getReceiveBufferSize(*m_env, socketNum);
            unsigned newBufferSize= setReceiveBufferTo(*m_env, socketNum, socketInputBufferSize);
            LogDebug( "Changed socket receive buffer size for the %s %s %d %d",
            subsession->mediumName(),subsession->codecName(),curBufferSize,newBufferSize);
          }
        }
      }
    } 
    else 
    {
      if (subsession->clientPortNum() == 0) 
      {
        LogDebug("No client port was specified for the %s %s",subsession->mediumName(),subsession->codecName());
      } 
      else 
      {	
        madeProgress = True;
      }
    }
  }
  if (!madeProgress) 
  {
    shutdown();
    return false;
  }
	
  // Perform additional 'setup' on each subsession, before playing them:
  if (!setupStreams())
  {
    return false;
  }

  // Create output files:
  // Create and start "FileSink"s for each subsession:
  madeProgress = False;
  iter.reset();
  while ((subsession = iter.next()) != NULL) 
  {
    if (subsession->readSource() == NULL) continue; // was not initiated
		
    CMemorySink* fileSink= CMemorySink::createNew(*m_env,m_buffer,fileSinkBufferSize);
    subsession->sink = fileSink;
    if (subsession->sink == NULL) 
    {
      LogDebug("Failed to create FileSink %s",m_env->getResultMsg());
      shutdown();
      return false;
    } 
    LogDebug("Created output sink:");;
    subsession->sink->startPlaying(*(subsession->readSource()),subsessionAfterPlaying,subsession);
				  
    // Also set a handler to be called if a RTCP "BYE" arrives
    // for this subsession:
    if (subsession->rtcpInstance() != NULL) 
    {
      subsession->rtcpInstance()->setByeHandler(subsessionByeHandler,subsession);
    }
    madeProgress = True;
  }

  return true;
}


void CRTSPClient::Stop()
{
  LogDebug("CRTSPClient:Stop");
  
  if (m_BufferThreadActive) 
  {
    StopBufferThread();
  }
  shutdown();
  m_buffer.Clear();
  LogDebug("CRTSPClient:Stop done");
}

void CRTSPClient::StartBufferThread()
{
  LogDebug("CRTSPClient::StartBufferThread");
  if (!m_BufferThreadActive)
  {
    StartThread();
    m_BufferThreadActive = true;
  }
  LogDebug("CRTSPClient::StartBufferThread done");
}

void CRTSPClient::StopBufferThread()
{
  LogDebug("CRTSPClient::StopBufferThread");
  m_bRunning=false;
  if (!m_BufferThreadActive)
    return;

  StopThread(20000);

  m_BufferThreadActive = false;
  LogDebug("CRTSPClient::StopBufferThread done");
}

bool CRTSPClient::IsRunning()
{
  return m_BufferThreadActive;
}

long CRTSPClient::Duration()
{
  return m_duration;
}

void CRTSPClient::FillBuffer(DWORD byteCount)
{	
  LogDebug("CRTSPClient::Fillbuffer...%d\n",byteCount);
  DWORD tickCount=GetTickCount();
  while ( IsRunning() && m_buffer.Size() < byteCount)
  {
    Sleep(5);
    if (GetTickCount()-tickCount > 3000) break;
  }
  LogDebug("CRTSPClient::Fillbuffer...%d/%d\n",byteCount,m_buffer.Size() );
}

void CRTSPClient::ThreadProc()
{
  HRESULT hr = S_OK;
  m_BufferThreadActive = TRUE;
  m_bRunning=true;
  ::SetThreadPriority(GetCurrentThread(),THREAD_PRIORITY_ABOVE_NORMAL);
  LogDebug("CRTSPClient:: thread started:%d", GetCurrentThreadId());
  while (m_env!=NULL && !ThreadIsStopping(0))
  {
    for (int i=0; i < 10;++i)
    {
      if (m_bRunning==false) break;
      m_env->taskScheduler().doEventLoop(); 
    }
    if (m_bRunning==false) break;
  }
  LogDebug("CRTSPClient:: thread stopped:%d", GetCurrentThreadId());
  m_BufferThreadActive = false;
  return;
}

bool CRTSPClient::Run()
{
  LogDebug("CRTSPClient::Run()");

  return true;
}
void CRTSPClient::Continue()
{
  if (m_ourClient!=NULL && m_session!=NULL)
  {
    RTSPClient* rtspClient=(RTSPClient*)m_ourClient;
    rtspClient->playMediaSession(*m_session,-1.0);
    StartBufferThread();
    m_bPaused=false;
    int x=1;
  }
}

bool CRTSPClient::IsPaused()
{
  return m_bPaused;
}
bool CRTSPClient::Pause()
{
  LogDebug("CRTSPClient::Pause()");
  if (m_ourClient!=NULL && m_session!=NULL)
  {
    LogDebug("CRTSPClient::Pause() stopthread");
    StopThread(10000);                           // Ambass : sometimes 100mS ( prev value ) is not enough and thread is not stopped.  
	                                               //          now stopping takes around 5 secs ?!?! why ????         
    LogDebug("CRTSPClient::Pause() thread stopped");
    RTSPClient* rtspClient=(RTSPClient*)m_ourClient;
    rtspClient->pauseMediaSession(*m_session);
    m_bPaused=true;
    int x=1;
  }
  LogDebug("CRTSPClient::Pause() done");
  return true;
}
bool CRTSPClient::Play(double fStart,double fDuration)
{
  LogDebug("CRTSPClient::Play from %f / %f", (float)fStart,(float)fDuration);
  m_bPaused=false;
  m_fStart=fStart;
  m_fDuration=fDuration;
  if (m_BufferThreadActive)
  {
    Stop();
    m_buffer.Clear();
    if (Initialize()==false) 
    {
      shutdown();
      return false;
    }
    if (OpenStream(m_url)==false) 
    {
      shutdown();
      return false;
    }
  }
  if (m_ourClient==NULL||m_session==NULL)
  {
    m_buffer.Clear();
    if (Initialize()==false) 
    {
      shutdown();
      return false;
    }
    if (OpenStream(m_url)==false) 
    {
      shutdown();
      return false;
    }
  }
  if (!startPlayingStreams()) 
  {			
    shutdown();
    return false;
  }
  StartBufferThread();
  return true;
}

bool CRTSPClient::UpdateDuration()
{
  char* sdpDescription= getSDPDescription();
  if (sdpDescription == NULL) 
	{
    LogDebug("UpdateStreamDuration: Failed to get a SDP description from URL %s %s", m_url ,m_env->getResultMsg() );
    return false;
  }
  //LogDebug("Opened URL %s %s",url,sdpDescription);

  char* range=strstr(sdpDescription,"a=range:npt=");
  if (range!=NULL)
  {
    char *pStart = range+strlen("a=range:npt=");
    char *pEnd = strstr(range,"-") ;
    if (pEnd!=NULL)
    {
      pEnd++ ;
      double Start=atof(pStart) ;
      double End=atof(pEnd) ;

      //LogDebug("rangestart:%f rangeend:%f", Start,End);
      m_duration=((End-Start)*1000.0);
    }
  }
}
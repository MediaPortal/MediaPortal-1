#include <streams.h>
#include "rtspclient.h"
#include "MemorySink.h"

extern void Log(const char *fmt, ...) ;
CRTSPClient::CRTSPClient(CMemoryBuffer& buffer)
:m_buffer(buffer)
{
	Log("CRTSPClient::CRTSPClient()");
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
}

CRTSPClient::~CRTSPClient()
{
	Log("CRTSPClient::~CRTSPClient()");
}


Medium* CRTSPClient::createClient(UsageEnvironment& env,int verbosityLevel, char const* applicationName) 
{
	Log("CRTSPClient::createClient()");
  return RTSPClient::createNew(env, verbosityLevel, applicationName,tunnelOverHTTPPortNum);
}

char* CRTSPClient::getOptionsResponse(Medium* client, char const* url,char* username, char* password) 
{
	Log("CRTSPClient::getOptionsResponse()");
  RTSPClient* rtspClient = (RTSPClient*)client;
  return rtspClient->sendOptionsCmd(url, username, password);
}

char* CRTSPClient::getSDPDescriptionFromURL(Medium* client, char const* url,
			       char const* username, char const* password,
			       char const* /*proxyServerName*/,
			       unsigned short /*proxyServerPortNum*/,
			       unsigned short /*clientStartPort*/) 
{
	Log("CRTSPClient::getSDPDescriptionFromURL()");
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

Boolean CRTSPClient::clientSetupSubsession(Medium* client, MediaSubsession* subsession,Boolean streamUsingTCP) 
{
	Log("CRTSPClient::clientSetupSubsession()");
  if (client == NULL || subsession == NULL) return False;
  RTSPClient* rtspClient = (RTSPClient*)client;
  return rtspClient->setupMediaSubsession(*subsession,False, streamUsingTCP);
}

Boolean CRTSPClient::clientStartPlayingSession(Medium* client,MediaSession* session) 
{
	Log("CRTSPClient::clientStartPlayingSession()");
  if (client == NULL || session == NULL) return False;
  RTSPClient* rtspClient = (RTSPClient*)client;

  long dur=m_duration/1000;
  long diff=abs(dur-m_fStart);
  if (diff <20)
  {
    m_fStart=dur+5;
  }
	Log("CRTSPClient::clientStartPlayingSession() play from %d / %d",(int)m_fStart,(int)m_duration);
  return rtspClient->playMediaSession(*session,m_fStart);

}

Boolean CRTSPClient::clientTearDownSession(Medium* client,MediaSession* session) 
{
	Log("CRTSPClient::clientTearDownSession()");
  if (client == NULL || session == NULL) return False;
  RTSPClient* rtspClient = (RTSPClient*)client;
  return rtspClient->teardownMediaSession(*session);
}

void subsessionAfterPlaying(void* clientData) 
{
	Log("CRTSPClient::subsessionAfterPlaying()");
}
void subsessionByeHandler(void* clientData) 
{
	Log("CRTSPClient::subsessionByeHandler()");
}

void CRTSPClient::closeMediaSinks() 
{
  if (m_session == NULL) return;
	Log("CRTSPClient::closeMediaSinks()");
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
	Log("CRTSPClient::tearDownStreams()");

  clientTearDownSession(m_ourClient, m_session);
}
bool CRTSPClient::setupStreams()
{
	//setup streams
	Log("CRTSPClient::setupStreams()");
	Boolean madeProgress=False;
	MediaSubsessionIterator iter(*m_session);
	MediaSubsession *subsession;

	while ((subsession = iter.next()) != NULL) 
	{
		if (subsession->clientPortNum() == 0) continue; // port # was not set

		if (!clientSetupSubsession(m_ourClient, subsession, streamUsingTCP)) 
		{
			Log( "Failed to setup %s %s %s" ,subsession->mediumName(),subsession->codecName(),m_env->getResultMsg() );;
		} 
		else 
		{
			Log( "Setup %s %s %d %d" ,subsession->mediumName(),subsession->codecName(),subsession->clientPortNum(),subsession->clientPortNum()+1);;
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
	Log("CRTSPClient::startPlayingStreams()");
  if (!clientStartPlayingSession(m_ourClient, m_session)) 
  {
    Log("Failed to start playing session:%s " ,m_env ->getResultMsg() );
    shutdown();
    return false;
  } 
  else 
  {
    Log("Started playing session");
  }
  return true;
}

void CRTSPClient::shutdown()
{
	Log("CRTSPClient::shutdown()");
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
	Log("CRTSPClient::Initialize()");
	m_duration=7200*1000;
	TaskScheduler* scheduler = BasicTaskScheduler::createNew();
  m_env = BasicUsageEnvironment::createNew(*scheduler);
	
  m_ourClient = createClient(*m_env, 0/*verbosityLevel*/, "TSFileSource");
  if (m_ourClient == NULL) 
	{
    Log("Failed to create %s %s" ,clientProtocolName,m_env->getResultMsg() );
    shutdown();
    return false;
  }
    return true;
}

bool CRTSPClient::OpenStream(char* url)
{
	Log("CRTSPClient::OpenStream()");
	m_session=NULL;
	
	strcpy(m_url,url);
	// Open the URL, to get a SDP description: 
  char* sdpDescription= getSDPDescriptionFromURL(m_ourClient, url, ""/*username*/, ""/*password*/,""/*proxyServerName*/, 0/*proxyServerPortNum*/,1234/*desiredPortNum*/);
  if (sdpDescription == NULL) 
	{
    Log("Failed to get a SDP description from URL %s %s",url ,m_env->getResultMsg() );
    shutdown();
    return false;
  }
  //Log("Opened URL %s %s",url,sdpDescription);

	char* range=strstr(sdpDescription,"a=range:npt=");
	if (range!=NULL)
	{
		range+=strlen("a=range:npt=");
		char rangeEnd[128];
		char rangeStart[128];
		strcpy(rangeEnd,"");
		strcpy(rangeStart,"");
		int pos=0;
		int setPos=0;
		while (isdigit(range[pos]) )
		{
			rangeStart[setPos]=range[pos];
			rangeStart[setPos+1]=0;
			pos++;
			setPos++;
		}
		pos++;
		setPos=0;
		while (isdigit(range[pos]) )
		{
			rangeEnd[setPos]=range[pos];
			rangeEnd[setPos+1]=0;
			pos++;
			setPos++;
		}
	
    Log("rangestart:%s rangeend:%s", rangeStart,rangeEnd);
		if (strlen(rangeStart)>0)
		{
			long startOfFile =0;//atol(rangeStart);
			long endOfFile =7200*1000;
			if (strlen(rangeEnd)>0)
			{
				endOfFile=atol(rangeEnd);
			}
      Log("start:%d end:%d", (int)startOfFile,(int)endOfFile);
			m_duration=(endOfFile-startOfFile)*1000;
		}
	}
  // Create a media session object from this SDP description:
  m_session = MediaSession::createNew(*m_env, sdpDescription);
  delete[] sdpDescription;
  if (m_session == NULL) 
	{
    Log("Failed to create a MediaSession object from the SDP description:%s ",m_env->getResultMsg());
    shutdown();
    return false;
  } 
	else if (!m_session->hasSubsessions()) 
	{
    Log("This session has no media subsessions");
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
				Log("Ignoring %s %s %s" , subsession->mediumName(),subsession->codecName(),singleMedium);
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
				Log("Unable to create receiver for %s %s %s" ,subsession->mediumName(),subsession->codecName(),m_env->getResultMsg());
			} 
			else 
			{
				Log("Created receiver for %s %s %d %d " ,subsession->mediumName(),subsession->codecName(),subsession->clientPortNum(),subsession->clientPortNum()+1 );
				madeProgress = True;

				if (subsession->rtpSource() != NULL) 
				{
					// Because we're saving the incoming data, rather than playing
					// it in real time, allow an especially large time threshold
					// (1 second) for reordering misordered incoming packets:
					unsigned const thresh = 1000000; // 1 second 
					subsession->rtpSource()->setPacketReorderingThresholdTime(thresh);

					if (socketInputBufferSize > 0) 
					{
						// Set the RTP source's input buffer size as specified:
						int socketNum= subsession->rtpSource()->RTPgs()->socketNum();
						unsigned curBufferSize= getReceiveBufferSize(*m_env, socketNum);
						unsigned newBufferSize= setReceiveBufferTo(*m_env, socketNum, socketInputBufferSize);
						Log( "Changed socket receive buffer size for the %s %s %d %d",
							subsession->mediumName(),subsession->codecName(),curBufferSize,newBufferSize);
					}
				}
      }
    } 
		else 
		{
      if (subsession->clientPortNum() == 0) 
			{
				Log("No client port was specified for the %s %s",subsession->mediumName(),subsession->codecName());
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
			Log("Failed to create FileSink %s",m_env->getResultMsg());
      shutdown();
      return false;
		} 
		Log("Created output sink:");;
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
	Log("CRTSPClient:Stop");
  
  if (m_BufferThreadActive) 
  {
    StopBufferThread();
  }
  shutdown();
  m_buffer.Clear();
	Log("CRTSPClient:Stop done");
}

void CRTSPClient::StartBufferThread()
{
	Log("CRTSPClient::StartBufferThread");
  if (!m_BufferThreadActive)
	{
		StartThread();
		m_BufferThreadActive = true;
	}
	Log("CRTSPClient::StartBufferThread done");
}

void CRTSPClient::StopBufferThread()
{
	Log("CRTSPClient::StopBufferThread");
	m_bRunning=false;
	if (!m_BufferThreadActive)
		return;

	StopThread(20000);

	m_BufferThreadActive = false;
	Log("CRTSPClient::StopBufferThread done");
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
	Log("CRTSPClient::Fillbuffer...%d\n",byteCount);
  DWORD tickCount=GetTickCount();
	while ( IsRunning() && m_buffer.Size() < byteCount)
	{
		Sleep(5);
    if (GetTickCount()-tickCount > 3000) break;
	}
	Log("CRTSPClient::Fillbuffer...%d/%d\n",byteCount,m_buffer.Size() );
	
}
void CRTSPClient::ThreadProc()
{
	HRESULT hr = S_OK;
	m_BufferThreadActive = TRUE;
	m_bRunning=true;
	::SetThreadPriority(GetCurrentThread(),THREAD_PRIORITY_ABOVE_NORMAL);
	Log("CRTSPClient:: thread started:%d", GetCurrentThreadId());
	while (m_env!=NULL && !ThreadIsStopping(0))
	{
		for (int i=0; i < 10;++i)
		{
			if (m_bRunning==false) break;
      m_env->taskScheduler().doEventLoop(); 
		}
		if (m_bRunning==false) break;
	}
  Log("CRTSPClient:: thread stopped:%d", GetCurrentThreadId());
	m_BufferThreadActive = false;
	return;
}

bool CRTSPClient::Run()
{
	Log("CRTSPClient::Run()");

	return true;
}
void CRTSPClient::Continue()
{
	if (m_ourClient!=NULL && m_session!=NULL)
	{
		RTSPClient* rtspClient=(RTSPClient*)m_ourClient;
		rtspClient->continueMediaSession(*m_session);
		StartBufferThread();
		int x=1;
	}
}
bool CRTSPClient::Pause()
{
	Log("CRTSPClient::Pause()");
	if (m_ourClient!=NULL && m_session!=NULL)
	{
	  Log("CRTSPClient::Pause() stopthread");
		StopThread(100);
	  Log("CRTSPClient::Pause() thread stopped");
		RTSPClient* rtspClient=(RTSPClient*)m_ourClient;
		rtspClient->pauseMediaSession(*m_session);
		int x=1;
	}
	Log("CRTSPClient::Pause() done");
	return true;
}
bool CRTSPClient::Play(float fStart)
{
	Log("CRTSPClient::Play from %f", fStart);
	m_fStart=fStart;
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
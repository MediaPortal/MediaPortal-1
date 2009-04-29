#include <streams.h>
#include "rtspclient.h"
#include "MemorySink.h"

CRTSPClient::CRTSPClient(CMemoryBuffer& buffer)
:m_buffer(buffer)
{
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
}

CRTSPClient::~CRTSPClient()
{
}


Medium* CRTSPClient::createClient(UsageEnvironment& env,int verbosityLevel, char const* applicationName) 
{
  return RTSPClient::createNew(env, verbosityLevel, applicationName,tunnelOverHTTPPortNum);
}

char* CRTSPClient::getOptionsResponse(Medium* client, char const* url,char* username, char* password) 
{
  RTSPClient* rtspClient = (RTSPClient*)client;
  return rtspClient->sendOptionsCmd(url, username, password);
}

char* CRTSPClient::getSDPDescriptionFromURL(Medium* client, char const* url,
			       char const* username, char const* password,
			       char const* /*proxyServerName*/,
			       unsigned short /*proxyServerPortNum*/,
			       unsigned short /*clientStartPort*/) 
{
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
  if (client == NULL || subsession == NULL) return False;
  RTSPClient* rtspClient = (RTSPClient*)client;
  return rtspClient->setupMediaSubsession(*subsession,False, streamUsingTCP);
}

Boolean CRTSPClient::clientStartPlayingSession(Medium* client,MediaSession* session) 
{
  if (client == NULL || session == NULL) return False;
  RTSPClient* rtspClient = (RTSPClient*)client;
  return rtspClient->playMediaSession(*session);
}

Boolean CRTSPClient::clientTearDownSession(Medium* client,MediaSession* session) 
{
  if (client == NULL || session == NULL) return False;
  RTSPClient* rtspClient = (RTSPClient*)client;
  return rtspClient->teardownMediaSession(*session);
}

void subsessionAfterPlaying(void* clientData) 
{
}
void subsessionByeHandler(void* clientData) 
{
}

void CRTSPClient::closeMediaSinks() 
{
  if (m_session == NULL) return;
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

  clientTearDownSession(m_ourClient, m_session);
}
bool CRTSPClient::setupStreams()
{
	//setup streams
	Boolean madeProgress=False;
	MediaSubsessionIterator iter(*m_session);
	MediaSubsession *subsession;

	while ((subsession = iter.next()) != NULL) 
	{
		if (subsession->clientPortNum() == 0) continue; // port # was not set

		if (!clientSetupSubsession(m_ourClient, subsession, streamUsingTCP)) 
		{
			*m_env << "Failed to setup \"" << subsession->mediumName()
				<< "/" << subsession->codecName()
				<< "\" subsession: " << m_env->getResultMsg() << "\n";
		} 
		else 
		{
			*m_env << "Setup \"" << subsession->mediumName()
				<< "/" << subsession->codecName()
				<< "\" subsession (client ports " << subsession->clientPortNum()
				<< "-" << subsession->clientPortNum()+1 << ")\n";
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
  if (!clientStartPlayingSession(m_ourClient, m_session)) 
  {
    *m_env << "Failed to start playing session: " << m_env ->getResultMsg() << "\n";
    shutdown();
    return false;
  } 
  else 
  {
    *m_env  << "Started playing session\n";
  }
  return true;
}

void CRTSPClient::shutdown()
{
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
  Medium::close(m_ourClient);
}


bool CRTSPClient::Initialize()
{
	TaskScheduler* scheduler = BasicTaskScheduler::createNew();
  m_env = BasicUsageEnvironment::createNew(*scheduler);
	
  m_ourClient = createClient(*m_env, 0/*verbosityLevel*/, "TSFileSource");
  if (m_ourClient == NULL) 
	{
    *m_env << "Failed to create " << clientProtocolName
				<< " client: " << m_env->getResultMsg() << "\n";
    shutdown();
    return false;
  }
    return true;
}

bool CRTSPClient::OpenStream(char* url)
{
	// Open the URL, to get a SDP description: 
  char* sdpDescription= getSDPDescriptionFromURL(m_ourClient, url, ""/*username*/, ""/*password*/,""/*proxyServerName*/, 0/*proxyServerPortNum*/,1234/*desiredPortNum*/);
  if (sdpDescription == NULL) 
	{
    *m_env << "Failed to get a SDP description from URL \"" << url << "\": " << m_env->getResultMsg() << "\n";
    shutdown();
    return false;
  }
  *m_env << "Opened URL \"" << url<< "\", returning a SDP description:\n" << sdpDescription << "\n";

  // Create a media session object from this SDP description:
  m_session = MediaSession::createNew(*m_env, sdpDescription);
  delete[] sdpDescription;
  if (m_session == NULL) 
	{
    *m_env << "Failed to create a MediaSession object from the SDP description: " << m_env->getResultMsg() << "\n";
    shutdown();
    return false;
  } 
	else if (!m_session->hasSubsessions()) 
	{
    *m_env << "This session has no media subsessions (i.e., \"m=\" lines)\n";
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
				*m_env << "Ignoring \"" << subsession->mediumName()
				  << "/" << subsession->codecName()
				  << "\" subsession, because we've asked to receive a single " << singleMedium
				  << " session only\n";
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
				*m_env  << "Unable to create receiver for \"" << subsession->mediumName()
					<< "/" << subsession->codecName()
					<< "\" subsession: " << m_env->getResultMsg() << "\n";
			} 
			else 
			{
				*m_env  << "Created receiver for \"" << subsession->mediumName()
					<< "/" << subsession->codecName()
					<< "\" subsession (client ports " << subsession->clientPortNum()
					<< "-" << subsession->clientPortNum()+1 << ")\n";
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
						*m_env  << "Changed socket receive buffer size for the \""
							 << subsession->mediumName()
							 << "/" << subsession->codecName()
							 << "\" subsession from "
							 << curBufferSize << " to "
							 << newBufferSize << " bytes\n";
					}
				}
      }
    } 
		else 
		{
      if (subsession->clientPortNum() == 0) 
			{
				*m_env  << "No client port was specified for the \""
						 << subsession->mediumName()
						 << "/" << subsession->codecName()
						 << "\" subsession.  (Try adding the \"-p <portNum>\" option.)\n";
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
			*m_env << "Failed to create FileSink\"" << m_env->getResultMsg() << "\n";
      shutdown();
      return false;
		} 
		*m_env << "Created output sink:" << "\"\n";
    subsession->sink->startPlaying(*(subsession->readSource()),subsessionAfterPlaying,subsession);
				  
		// Also set a handler to be called if a RTCP "BYE" arrives
		// for this subsession:
		if (subsession->rtcpInstance() != NULL) 
		{
			subsession->rtcpInstance()->setByeHandler(subsessionByeHandler,subsession);
		}
		madeProgress = True;
	}

  if (!startPlayingStreams()) return false;
  return true;
}


void CRTSPClient::StartBufferThread()
{
  if (!m_BufferThreadActive)
	{
		StartThread();
		m_BufferThreadActive = true;
	}
}

void CRTSPClient::StopBufferThread()
{
	if (!m_BufferThreadActive)
		return;

	StopThread(10);

	m_BufferThreadActive = false;
}

void CRTSPClient::ThreadProc()
{
	HRESULT hr = S_OK;
	m_BufferThreadActive = TRUE;

	BoostThread Boost;

  *m_env << "rtsp thread started:" << "\"\n";
	while (m_env!=NULL && !ThreadIsStopping(0))
	{
		m_env->taskScheduler().doEventLoop(); 
		Sleep(1);
	};
  *m_env << "rtsp thread stopped:" << "\"\n";
	m_BufferThreadActive = false;
	return;
}

void CRTSPClient::Play()
{
		StartBufferThread();
}
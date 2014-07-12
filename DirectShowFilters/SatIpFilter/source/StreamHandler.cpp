#include "StreamHandler.h"

#include <cstdint>

extern void LogDebug(const char *fmt, ...);

//
//  CStreamHandler class
//
CStreamHandler::CStreamHandler()
{
	// configure streaming
	_LoadMe = LoadLibrary("RtpStreamer.dll");
	if (_LoadMe != 0)
		LogDebug("RtpStreamer.dll library loaded!");
	else
		LogDebug("RtpStreamer.dll library failed to load!");
	_MPrtpStreamEntryPoint = (pvFunctv)GetProcAddress(_LoadMe, "CreateClassInstance");
	if (!_MPrtpStreamEntryPoint) LogDebug("Couldn't find entry point in RtpStreamer.dll!");
	_MPrtpStream.reset((IMPrtpStream*)(_MPrtpStreamEntryPoint()));
	_streamRunning = false;
	_startStreaming = false;
	_streamConfigured = false;
	_stop = false;
	_pmtSet = false;

	_clientIp = "192.168.178.26";
	_bytesWritten = 0;

	_pmtParser = new CPmtParser();
	_sectionDecoder = new CSectionDecoder();
}

// Destructor

CStreamHandler::~CStreamHandler()
{
	delete _sectionDecoder;
	delete _pmtParser;
}

void CStreamHandler::configure() {
	_streamConfigured = true;
	_startStreaming = true;
	_stop = false;
}

void CStreamHandler::setPmt(int pmt) {
	LogDebug("Set PMT with pid=%d", pmt);
	_pidfilter.Reset();
	_pidfilter.Add(pmt);
	_sectionDecoder->Reset();
	_pmtParser->Reset();
	_pmtParser->SetCallBack(this);
	_sectionDecoder->SetPid(pmt);
	_sectionDecoder->SetCallBack(this);
	_pmtSet = true;
}

void CStreamHandler::write(unsigned char *dataPtr, int numBytes)
{
	// Check if a multiple of 188 Bytes are Written
	if (numBytes % TS_PACKET_LEN != 0) {
		LogDebug("");
	}

	// section decoder
	_sectionDecoder->OnTsPacket(dataPtr);

	// PID filter
	uint16_t pid = PidFilter::getPidFromPackage(dataPtr);
	//LogDebug("Pid 0x%04x", Pid);
	// the Stream must be configured before we write packages to the buffer
	if (_pidfilter.PidRequested(pid) && _streamConfigured && _MPrtpStream.get() != nullptr) {
		_MPrtpStream->write(dataPtr, TS_PACKET_LEN);
		if (!_streamRunning)
			_bytesWritten += TS_PACKET_LEN;
	}

	//LogDebug("Stream Running: %d, Stop: %d, _startStreaming: %d, _bytesWritten: %d", (_streamRunning ? 1 : 0), (_stop ? 1 : 0), (_startStreaming ? 1 : 0), _bytesWritten);
	if (!_streamRunning && !_stop && _startStreaming && _streamConfigured && (!_pmtSet || _bytesWritten > (TS_PACKET_LEN * 90))) {
		_streamRunning = true;
		LogDebug("streamHandler: startStreaming");
		_test2 = "test";
		_streamingThread = (HANDLE)_beginthread(&CStreamHandler::CreateStream, 0, (void*)this);
		LogDebug("streamHandler: streaming thread started");
	}
}

void __cdecl CStreamHandler::CreateStream(void* arg)
{
	LogDebug("streamHandler: Streaming thread CreateStream");
	CStreamHandler* filter = (CStreamHandler*)arg;
	filter->_MPrtpStream->MPrtpStreamCreate(filter->_clientIp.c_str(), filter->_clientPort, filter->_test2);
}

void CStreamHandler::start()
{
	LogDebug("streamHandler: start()");
	if (_MPrtpStream.get() != nullptr)
		_MPrtpStream->RtpStart();
	_stop = false;
	_bytesWritten = 0;
	_startStreaming = true;
}

void CStreamHandler::stop()
{
	LogDebug("streamHandler: begin stop()");
	_stop = true;
	if (_MPrtpStream.get() != nullptr) {
		_MPrtpStream->RtpStop();
		if (_streamingThread != INVALID_HANDLE_VALUE) {
			WaitForSingleObject(_streamingThread, INFINITE);
			_streamingThread = INVALID_HANDLE_VALUE;
			LogDebug("streamHandler: streaming thread stopped");
		}
	}
	_streamRunning = false;
	_startStreaming = false;
	_streamConfigured = false;
	_pmtSet = false;
	LogDebug("streamHandler: finished stop()");
}

void CStreamHandler::OnPmtReceived(const CPidTable& pidTable) {
	LogDebug("Got PMT with pid=%hu", pidTable.PmtPid);

	vector<VideoPid*>::const_iterator vPidIt = pidTable.VideoPids.begin();
	while (vPidIt != pidTable.VideoPids.end())
	{
		_pidfilter.Add((*vPidIt)->Pid);
		LogDebug("Added video pid %d", (*vPidIt)->Pid);
		vPidIt++;
	}
	vector<AudioPid*>::const_iterator aPidIt = pidTable.AudioPids.begin();
	while (aPidIt != pidTable.AudioPids.end())
	{
		_pidfilter.Add((*aPidIt)->Pid);
		LogDebug("Added audio pid %d", (*aPidIt)->Pid);
		aPidIt++;
	}

	vector<SubtitlePid*>::const_iterator sPidIt = pidTable.SubtitlePids.begin();
	while (sPidIt != pidTable.SubtitlePids.end())
	{
		_pidfilter.Add((*sPidIt)->Pid);
		LogDebug("Added subtitle pid %d", (*sPidIt)->Pid);
		sPidIt++;
	}
}

void CStreamHandler::OnNewSection(int pid, int tableId, CSection& section) {
	_pmtParser->OnNewSection(section);
}
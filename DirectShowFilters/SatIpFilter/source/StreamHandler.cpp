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
		LogDebug("LoadMe library loaded!\n");
	else
		LogDebug("LoadMe library failed to load!\n");
	_MPrtpStreamEntryPoint = (pvFunctv)GetProcAddress(_LoadMe, "CreateClassInstance");
	if (!_MPrtpStreamEntryPoint) LogDebug("shit!!");
	_MPrtpStream.reset((IMPrtpStream*)(_MPrtpStreamEntryPoint()));
	_streamRunning = false;
	_startStreaming = false;
	_stop = false;

	_clientIp = "192.168.178.26";
	_bytesWritten = 0;
	// TODO remove this later if we can configure the stream
	_streamConfigured = false;
}

// Destructor

CStreamHandler::~CStreamHandler()
{
	
}

void CStreamHandler::configure() {
	_streamConfigured = true;
	_startStreaming = true;
	_stop = false;
}

void CStreamHandler::write(unsigned char *dataPtr, int numBytes)
{
	// Check if a multiple of 188 Bytes are Written
	if (numBytes % TS_PACKET_LEN != 0) {
		LogDebug("");
	}

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
	if (!_streamRunning && !_stop && _startStreaming && _streamConfigured && _bytesWritten > (TS_PACKET_LEN * 900)) {
		_streamRunning = true;
		LogDebug("startStreaming");
		_test2 = "test";
		_streamingThread = std::thread(&IMPrtpStream::MPrtpStreamCreate, this->_MPrtpStream.get(), _clientIp.c_str(), _clientPort, _test2);
		_streamingThread.detach(); // fire & forget, maybe not the best option so have a look here later: http://stackoverflow.com/questions/16296284/workaround-for-blocking-async
	}
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
		if (_streamRunning) {
			//TerminateThread(_streamingThread.native_handle(), 0);
			//CloseHandle(_streamingThread.native_handle());
			//_streamingThread.join();
		}
	}
	_streamRunning = false;
	_startStreaming = false;
	LogDebug("streamHandler: finished stop()");
}
#ifndef  CONFIG_H  
	#include "config.h"
#endif
#include "PidFilter.h"
#include "RtpStreamInterface.h"
#include <thread>
#include <string>
#include <memory>
#include <process.h>

class CStreamHandler
{

	typedef void*(*pvFunctv)();

public:

	CStreamHandler();
	~CStreamHandler();

	void write(unsigned char *dataPtr, int numBytes);
	void start();
	void stop();
	void configure();

	PidFilter	_pidfilter;
	std::string _clientIp;
	int _clientPort;

private:

	HINSTANCE _LoadMe;
	pvFunctv  _MPrtpStreamEntryPoint;
	std::unique_ptr<IMPrtpStream> _MPrtpStream;

	bool _streamRunning;
	bool _streamConfigured;
	int _bytesWritten;
	char* _test2;
	bool _startStreaming;
	bool _stop;
	std::thread _streamingThread;

};
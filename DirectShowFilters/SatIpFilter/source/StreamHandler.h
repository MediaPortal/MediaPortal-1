#ifndef  CONFIG_H  
	#include "config.h"
#endif
#include "PidFilter.h"
#include "RtpStreamInterface.h"
#include "SectionDecoder.h"
#include "PmtParser.h"
#include <string>
#include <memory>
#include <process.h>
#include <sstream>

class CStreamHandler : public IPmtCallBack2, public ISectionCallback
{

	typedef void*(*pvFunctv)();

public:

	CStreamHandler();
	~CStreamHandler();

	void write(unsigned char *dataPtr, int numBytes);
	void start();
	void stop();
	void configure();
	void OnPmtReceived(const CPidTable& pidInfo);
	void OnNewSection(int pid, int tableId, CSection& section);
	void setPmt(int pmt);

	PidFilter	_pidfilter;
	std::string _clientIp;
	int _clientPort;

private:
	static void __cdecl CreateStream(void* arg);

	HINSTANCE _LoadMe;
	pvFunctv  _MPrtpStreamEntryPoint;
	std::unique_ptr<IMPrtpStream> _MPrtpStream;

	bool _streamRunning;
	bool _streamConfigured;
	int _bytesWritten;
	char* _test2;
	bool _startStreaming;
	bool _stop;
	bool _pmtSet;
	HANDLE _streamingThread;
	CSectionDecoder* _sectionDecoder;
	CPmtParser* _pmtParser;

};
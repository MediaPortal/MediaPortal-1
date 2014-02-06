// RtpStreamer.h
#include "RtpStream.h"

#pragma managed
#using <mscorlib.dll>

//#pragma once

using namespace System;
using namespace System::Runtime::InteropServices;

namespace RtpStreamer {

	public ref class RtpStreamer
	{
		private:
		  MPrtpStream *_MPrtpStream;
		public:
		  RtpStreamer()
		  {
			_MPrtpStream = new MPrtpStream();
		  }
		  ~RtpStreamer()
		  {
			delete _MPrtpStream;
			_MPrtpStream = 0;
		  }
		  void RtpStreamCreate(/*char* stopLoop, */String ^destinationAddressStr, int _rtpPort, String ^fileName)
		  {
			  _MPrtpStream->MPrtpStreamCreate(/*stopLoop, */(char *)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(destinationAddressStr).ToPointer(), _rtpPort, (char *)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(fileName).ToPointer());
		  }
		  void RtpStreamStop()
		  {
			  _MPrtpStream->RtpStop();
		  }
	};
}
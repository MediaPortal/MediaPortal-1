#pragma once

// structure used to communicate subtitles to MediaPortal's managed code
struct SUBTITLE
{
  // Subtitle bitmap
  LONG        bmType;
  LONG        bmWidth;
  LONG        bmHeight;
  LONG        bmWidthBytes;
  WORD        bmPlanes;
  WORD        bmBitsPixel;
  LPVOID      bmBits;

  unsigned    __int64 timestamp;
  unsigned    __int64 timeOut;
  int         firstScanLine;
};

struct TEXT_SUBTITLE{

	int firstLine;  // can be 0 to (totalLines - 1)
	int totalLines; // for teletext this is 25 lines

	unsigned    __int64 timestamp;
	unsigned    __int64 timeOut;

	LPCSTR text;
	
	/*~TEXT_SUBTITLE(){
		delete text;
	}*/
};
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

  LONG        screenWidth;
  LONG        screenHeight;

  unsigned    __int64 timestamp;
  unsigned    __int64 timeOut;
  int         firstScanLine;
};

struct TEXT_SUBTITLE
{
	int character_table;
	LPCSTR language;
	int page;
	LPCSTR text;
	int firstLine;  // can be 0 to (totalLines - 1)
	int totalLines; // for teletext this is 25 lines

	unsigned    __int64 timestamp;
	unsigned    __int64 timeOut;

	
};

struct DVBLANG
{
	DVBLANG()
  {
	}
	DVBLANG(byte b1, byte b2, byte b3)
  {
		lang[0] = b1;	
		lang[1] = b2;
		lang[2] = b3;
	}

	byte lang[3];
};

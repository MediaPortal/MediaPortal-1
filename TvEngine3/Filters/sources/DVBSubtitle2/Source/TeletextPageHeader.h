#ifndef __TELETEXT_PAGE_HEADER_H__
#define __TELETEXT_PAGE_HEADER_H__

#pragma once

#include <windows.h>
#include "DVBSub.h"
#include <cassert>
#include <hash_set>
#include "Hamming.h"

class TeletextPageHeader{
public:
	TeletextPageHeader(int mag,byte* data);

	bool eraseBit();

	bool isSubtitle();

	bool isSerial();

	bool isTimeFiller();

	int PageNumber();

	int Magazine();

	short Language();

private:
	short pageNum;
	short language;
	short magazine;
	bool timefiller;
	bool erasePage;
	bool newsflash;
	bool subtitle;
	bool supressHeader;
	bool updateIndicator;
	bool interruptedSequence;
	bool inhibitDisplay;
	bool magazineSerial; 

};

#endif
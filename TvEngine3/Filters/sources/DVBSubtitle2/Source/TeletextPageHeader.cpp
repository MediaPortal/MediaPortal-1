#include "TeletextPageHeader.h"

// data recieved is with the 4 first bytes cut-off (magazine etc
TeletextPageHeader::TeletextPageHeader(int mag,byte* data){
	int offset = 0;	
	magazine = mag;
	BYTE pageByte = unham(data[offset], data[offset+1]); // The lower two (hex) numbers of page
	timefiller = (pageByte == 0xFF);

	if(!timefiller){
		pageNum = (mag * 100 + 10*(pageByte >> 4) + (pageByte & 0x0F));
		assert(pageNum >= 100 && pageNum <= 966);
	}
	else pageNum = -1;

	//int subpage = ((unham(data[offset + 4], data[offset+5]) << 8) | unham(data[offset+2], data[offset+3])) & 0x3F7F;

	language = ((unham(data[offset + 6], data[offset + 7]) >> 5) & 0x07);

	erasePage =                     (data[offset + 3] & 0x80) == 0x80; // Byte 9,  bit 8
	newsflash =                     (data[offset + 5] & 0x20) == 0x20; // Byte 11, bit 6
	subtitle =                      (data[offset + 5] & 0x80) == 0x80; // Byte 11, bit 8

	supressHeader =                 (data[offset + 6] & 0x02) == 0x02; // Byte 12, bit 2
	updateIndicator =               (data[offset + 6] & 0x08) == 0x08; // Byte 12, bit 4

	interruptedSequence =           (data[offset + 6] & 0x20) == 0x20; // Byte 12, bit 6
	inhibitDisplay =                (data[offset + 6] & 0x80) == 0x80; // Byte 12, bit 8
	magazineSerial =                (data[offset + 7] & 0x02) == 0x02; // Byte 13, bit 2

	if(magazineSerial){
		LogDebug("Magazine %i reported as in serial mode!",mag);
	}
}

bool TeletextPageHeader::eraseBit(){
	return erasePage;
}

bool TeletextPageHeader::isSubtitle(){
	return subtitle;
}

bool TeletextPageHeader::isSerial(){
	return magazineSerial;
}

bool TeletextPageHeader::isTimeFiller(){
	return timefiller;
}

int TeletextPageHeader::PageNumber(){
	assert(!timefiller);
	return pageNum;
}

int TeletextPageHeader::Magazine(){
	return magazine;
}

short TeletextPageHeader::Language(){
	assert(!timefiller);
	return language;
}
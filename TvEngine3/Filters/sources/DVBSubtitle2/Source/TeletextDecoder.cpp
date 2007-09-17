#include "TeletextDecoder.h"
#include "Hamming.h"
#include "TeletextConversion.h"
#include <iomanip>
#include <sstream>
#include <cassert>

using namespace std;

extern void LogDebug( const char *fmt, ... );



#define MSB3_NP( x ) (x & 0x70) // 3 most significant bits, removing parity bit
#define LSB4( x ) (x & 0x0F) // 4 less significant bits (no parity)


// table to invert bit ordering of a byte
unsigned char invtab[256] =
{
  0x00, 0x80, 0x40, 0xc0, 0x20, 0xa0, 0x60, 0xe0, 
  0x10, 0x90, 0x50, 0xd0, 0x30, 0xb0, 0x70, 0xf0, 
  0x08, 0x88, 0x48, 0xc8, 0x28, 0xa8, 0x68, 0xe8, 
  0x18, 0x98, 0x58, 0xd8, 0x38, 0xb8, 0x78, 0xf8, 
  0x04, 0x84, 0x44, 0xc4, 0x24, 0xa4, 0x64, 0xe4, 
  0x14, 0x94, 0x54, 0xd4, 0x34, 0xb4, 0x74, 0xf4, 
  0x0c, 0x8c, 0x4c, 0xcc, 0x2c, 0xac, 0x6c, 0xec, 
  0x1c, 0x9c, 0x5c, 0xdc, 0x3c, 0xbc, 0x7c, 0xfc, 
  0x02, 0x82, 0x42, 0xc2, 0x22, 0xa2, 0x62, 0xe2, 
  0x12, 0x92, 0x52, 0xd2, 0x32, 0xb2, 0x72, 0xf2, 
  0x0a, 0x8a, 0x4a, 0xca, 0x2a, 0xaa, 0x6a, 0xea, 
  0x1a, 0x9a, 0x5a, 0xda, 0x3a, 0xba, 0x7a, 0xfa, 
  0x06, 0x86, 0x46, 0xc6, 0x26, 0xa6, 0x66, 0xe6, 
  0x16, 0x96, 0x56, 0xd6, 0x36, 0xb6, 0x76, 0xf6, 
  0x0e, 0x8e, 0x4e, 0xce, 0x2e, 0xae, 0x6e, 0xee, 
  0x1e, 0x9e, 0x5e, 0xde, 0x3e, 0xbe, 0x7e, 0xfe, 
  0x01, 0x81, 0x41, 0xc1, 0x21, 0xa1, 0x61, 0xe1, 
  0x11, 0x91, 0x51, 0xd1, 0x31, 0xb1, 0x71, 0xf1, 
  0x09, 0x89, 0x49, 0xc9, 0x29, 0xa9, 0x69, 0xe9, 
  0x19, 0x99, 0x59, 0xd9, 0x39, 0xb9, 0x79, 0xf9, 
  0x05, 0x85, 0x45, 0xc5, 0x25, 0xa5, 0x65, 0xe5, 
  0x15, 0x95, 0x55, 0xd5, 0x35, 0xb5, 0x75, 0xf5, 
  0x0d, 0x8d, 0x4d, 0xcd, 0x2d, 0xad, 0x6d, 0xed, 
  0x1d, 0x9d, 0x5d, 0xdd, 0x3d, 0xbd, 0x7d, 0xfd, 
  0x03, 0x83, 0x43, 0xc3, 0x23, 0xa3, 0x63, 0xe3, 
  0x13, 0x93, 0x53, 0xd3, 0x33, 0xb3, 0x73, 0xf3, 
  0x0b, 0x8b, 0x4b, 0xcb, 0x2b, 0xab, 0x6b, 0xeb, 
  0x1b, 0x9b, 0x5b, 0xdb, 0x3b, 0xbb, 0x7b, 0xfb, 
  0x07, 0x87, 0x47, 0xc7, 0x27, 0xa7, 0x67, 0xe7, 
  0x17, 0x97, 0x57, 0xd7, 0x37, 0xb7, 0x77, 0xf7, 
  0x0f, 0x8f, 0x4f, 0xcf, 0x2f, 0xaf, 0x6f, 0xef, 
  0x1f, 0x9f, 0x5f, 0xdf, 0x3f, 0xbf, 0x7f, 0xff, 
};


void Magazine::SetLine(int line, byte* data){
	assert(pageNumInProgress == -1 ||  (pageNumInProgress >= 100 && pageNumInProgress <= 966));
	memcpy(GetLine(line),data,40);
}

string ToBinary(byte b){
	stringstream ss;

	for (int i=7; i>=0; i--) {
		int bit = ((b >> i) & 1);
		ss << bit;	
	}
	return ss.str();
}

void Magazine::EndPage(){

	if(pageNumInProgress != -1 && (pageNumInProgress < 0 || pageNumInProgress >= 966)){
		LogDebug("DANGER DANGER!, endpage with pageNumInProgress = %i", pageNumInProgress);
		return;
	}
	
	if(pageNumInProgress != pageToDecode) return;

	LogDebug("Finished Page %i: (pageToDecode is %i)",pageNumInProgress, pageToDecode);
	bool hasContent = true;

	for(int i = 0; i < 25; i++){
		stringstream s;
		bool boxed = false;
		byte* lineContent = GetLine(i);

		for(int j = 0; j < 40; j++){
			//s << setbase(16) << (int)pageContent[i][j] << " ";
			//s << ToBinary(pageContent[i][j]) << " ";

			// Remove spacing attributes ( see 12.2 of the draft)
			// FIXME: Some subtitles will have the attributed 'double height'
			// and therefore have an empty line between subs.

			
			// ís this content a space attribute?
			if(MSB3_NP(lineContent[j]) == 0){
				
				if(LSB4(lineContent[j]) == SPACE_ATTRIB_BOX_START){
					//LogDebug("BS - boxed is true");
					boxed = true;
					hasContent = true;
				}
				else if(LSB4(lineContent[j]) == SPACE_ATTRIB_BOX_END){
					//LogDebug("BE - boxed is false");
					boxed = false;
				}
				// remove spacing attribute
				lineContent[j] = TELETEXT_BLANK;
			}
			else if(!boxed){
				// if we are not in boxed mode,
				// we dont want to keep the content
				lineContent[j] = TELETEXT_BLANK;
				assert(!boxed);
				//LogDebug("BOXED FALSE: blanking %i %i", i,j);
			}
		}
		//LogDebug(s.str().c_str());
	}

	
	if(!hasContent) {
		LogDebug("BLANK PAGE");
	}
	
	char text[TELETEXT_WIDTH+1];
	text[TELETEXT_WIDTH] = '\0';
	stringstream s;

	for(int i = 0; i < TELETEXT_LINES; i++){
		memset(text,0,TELETEXT_WIDTH);

		int offset = i*(TELETEXT_WIDTH+1);

		memcpy(text,GetLine(i),TELETEXT_WIDTH);
		ConvertLine(language,text);
		// then do language correction!
		assert(text[TELETEXT_WIDTH] == '\0');
		assert(strlen(text) == TELETEXT_WIDTH);
		
		if(hasContent)LogDebug(text);
		s << text << "\n";
		
	}

	TEXT_SUBTITLE sub;

	string lines = s.str();

	sub.firstLine = 0;
	sub.totalLines = TELETEXT_LINES;
	sub.text = lines.c_str();
	sub.timestamp = 0;
	sub.timeOut = 3000000; // teletext pages will be actively overwritten if they need to hide

	filter->NotifyTeletextSubtitle(sub);
	
}

void Magazine::StartPage(TeletextPageHeader& header){
	int offset = 0;
	int mag = header.Magazine();
	if(mag != magID){
		LogDebug("Magazine magid mag: %i, %i", magID,mag);
	}
	assert(mag == magID);
	assert(pageNumInProgress == -1 ||  (pageNumInProgress >= 100 && pageNumInProgress <= 966));

	if(header.isTimeFiller()){ // time filling header to indicate end of page
		if(pageNumInProgress != -1){ // if we were working on a previous page its finished now
			EndPage();
		}
		//LogDebug("Mag %i FILLER ends page %i", magID, pageNumInProgress);
		Clear();
		pageNumInProgress = -1;
		return;
	}

	int new_page_num = header.PageNumber();
	language = header.Language();

	if(pageNumInProgress != new_page_num){
		//LogDebug("Mag %i, Page %i finished by new page %i", magID, pageNumInProgress, new_page_num);
		if(pageNumInProgress != -1){ // if we were working on a previous page its finished now
			EndPage();
		}
		Clear();
		pageNumInProgress = new_page_num;
	}
	
	if(header.eraseBit()) {
		Clear();
	}
	
	if(!header.isSubtitle()){
		if(nonSubPages.find(pageNumInProgress) == nonSubPages.end()){
			nonSubPages.insert(pageNumInProgress);
			//LogDebug("Not subtitle (total %i): Page %i", nonSubPages.size(), pageNumInProgress);
		}
	}
	else{
		if(subPages.find(pageNumInProgress) == subPages.end()){
			subPages.insert(pageNumInProgress);
			//LogDebug("New subtitle page encountered (total %i): Page %i",subPages.size(), pageNumInProgress);
		}
		subPages.insert(pageNumInProgress);	
	}
	assert(pageNumInProgress >= 100 && pageNumInProgress <= 966);
}

void TeletextDecoder::NotifySubPageInfo(int page, char lang[3]){
	LogDebug("Page %i is lang %c%c%c",page, lang[0],lang[1],lang[2]);

	// temp, just choose the latest advertised page
	for(int i = 0; i < 8; i++){
		magazines[i].UsePage(page, lang);
	}
}

void TeletextDecoder::OnTeletextPacket(byte* data){
		//LogDebug("OnTeletextPacket : Begin");
		// data_field
		byte reserved_parity_offset = data[0]; // parity/offset etc

		//LogDebug("first data_field byte: %s", ToBinary(reserved_parity_offset).c_str()); 
		byte reserved_future_use = data[0] & 0xC0; // first two bits
		assert(reserved_future_use == 0xC0);

		byte field_parity = (data[0] & 0x20) >> 5; // 3rd bit
		//LogDebug("field parity %i", field_parity);

		byte line_offset = data[0] & 0x1F; // last 5 bits
		assert( line_offset == 0x00 || (line_offset >= 0x07 && line_offset <= 0x16));

		byte framing_code = data[1]; 
		if(framing_code != 0xE4){
			LogDebug("FRAMING CODE WRONG! %s",ToBinary(framing_code).c_str());
		}
		assert(framing_code == 0xE4);

		// what is this for? (A: reverse bit ordering)
		for (int j = 2; j < DATA_FIELD_SIZE; j++)
		{ 
			data[j] = invtab[data[j]];
		}

		byte magazine_and_packet_address1 = data[2];
		byte magazine_and_packet_address2 = data[3];
		byte magazine_and_packet_address = unham(magazine_and_packet_address1, magazine_and_packet_address2);

		byte mag = magazine_and_packet_address & 7;

		// mag == 0 means page is 8nn
		if (mag == 0) mag = 8;

		int magIndex = mag -1;

		assert(magIndex >= 0 && magIndex <= 7);

		byte Y = (magazine_and_packet_address >> 3) & 0x1f; // Y is the packet number

		int offset = 4; // start of data differs between packet types

		if(Y == 0){ // teletext packet header
			TeletextPageHeader header(mag,&data[offset]);
			if(header.isSerial()){ // to support serial mode, just end all pages in progress (there should be only one)
				int inProgress = 0;
				for(int i = 0; i < 8; i++){
					if(magazines[i].PageInProgress()){					
						inProgress++;
					}
					magazines[i].EndPage();
				}
				assert(inProgress <= 1); // at most one page should be in progress
			}
			this->magazines[magIndex].StartPage(header);
		}
		else if(Y >= 1 && Y <= 25){ // display content
			this->magazines[magIndex].SetLine(Y,&data[offset]);
		}
		else{
			//LogDebug("Packet %i for magazine %i (discarded)", Y, mag);
		}
}
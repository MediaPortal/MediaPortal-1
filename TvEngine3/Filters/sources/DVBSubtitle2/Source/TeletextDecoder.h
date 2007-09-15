#ifndef __TELETEXT_DECODER_H__
#define __TELETEXT_DECODER_H__

#pragma once

#include <windows.h>
#include "DVBSub.h"
#include <cassert>
#include <hash_set>
#include "TeletextPageHeader.h"

using namespace stdext;

const int TELETEXT_LINES = 25;
const int TELETEXT_WIDTH = 40;
const int DATA_FIELD_SIZE = 44;
const unsigned char TELETEXT_BLANK  = 0x20;
const unsigned char SPACE_ATTRIB_BOX_START = 0x0B;
const unsigned char SPACE_ATTRIB_BOX_END = 0x0A;



class Magazine{

public:
	Magazine(){
		pageContent = new byte[TELETEXT_LINES * TELETEXT_WIDTH];
		LogDebug("Magazine ctor");
		pageNumInProgress = -1;
		language = -1;
		magID = -1;
		pageToDecode = -1;

	}

	~Magazine(){
		LogDebug("Magazine dtor");
		//assert(_CrtIsValidPointer(pageContent));
		delete[] pageContent;
	}

	void StartPage(TeletextPageHeader& header);
	void SetLine(int line, byte* data);
	void EndPage();

	byte* GetLine(int l){
		SanityCheck();
		return (byte*)(pageContent + max(l-1,0)*TELETEXT_WIDTH);
	}

	void Clear(){
		SanityCheck();
		memset(this->pageContent,TELETEXT_BLANK,TELETEXT_LINES*TELETEXT_WIDTH);
	}

	void SetFilter(CDVBSub* filter){
		this->filter = filter;
	}

	void UsePage(int page, char lang[3]){
		pageToDecode = page;
	}

	void SanityCheck(){
		//assert(_CrtIsValidPointer(pageContent));
		//assert(_CrtIsValidPointer(pageContent,TELETEXT_LINES*TELETEXT_WIDTH,TRUE));
		assert(magID == -1 || (magID <= 8 && magID >= 0));
	}

	bool PageInProgress(){
		return pageNumInProgress != -1;
	}

	void SetMag(int mag){
		assert(pageNumInProgress == -1 && language == -1);
		assert(mag >= 1 && mag <= 8);
		/*if(!//_CrtIsValidPointer(&pageContent)){
			LogDebug("PC in mag %i invalid!", mag);
		}*/
		magID = mag;
		SanityCheck();
	}
private:
	CDVBSub* filter;
	int pageNumInProgress;
	int language; // encoding language
	int real_language; // DVB SI language info
	int pageToDecode;
	byte* pageContent; // indexed by line and character (col)
	hash_set<int> nonSubPages;
	hash_set<int> subPages;
	int magID;
};



class TeletextDecoder{

public:
	TeletextDecoder(CDVBSub* filter){
		
		LogDebug("Teletext decoder ctor ..");
		this->filter = filter;
		
		magazines = new Magazine[8];
		
		//assert(_CrtIsValidPointer(this));
		LogDebug("TxtDec this %i", this);
		
//		LogDebug("tcs is valid just after init? : %i", //_CrtIsValidPointer(tcs));
		//LogDebug("magazines is valid just after init? : %i", //_CrtIsValidPointer(magazines));
		//LogDebug("otherMags is valid just after init? : %i", //_CrtIsValidPointer(otherMags));		

		//assert(_CrtIsValidPointer(magazines));

		for(int i = 0; i < 8; i++){
			magazines[i].SetMag(i+1);
			magazines[i].SetFilter(filter);
			magazines[i].Clear();
		}
		
	}

	~TeletextDecoder(){
		delete[] magazines;
	}

	void OnTeletextPacket(byte* data);

	void NotifySubPageInfo(int page, char lang[3]);

	
	Magazine* otherMags;
	Magazine* magazines;

private:
	CDVBSub* filter;

};

#endif
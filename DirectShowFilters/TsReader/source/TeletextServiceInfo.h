#pragma once

struct TeletextServiceInfo{

	TeletextServiceInfo(){
		type = -1;
		page = -1;
		lang[0] = lang[1] = lang[2] = '0';
	}
	
	TeletextServiceInfo(const TeletextServiceInfo& toCopy){
		type = toCopy.type;
		page = toCopy.page;
		lang[0] = toCopy.lang[0];
		lang[1] = toCopy.lang[1];
		lang[2] = toCopy.lang[2];
	}

	bool IsSubtitleInfo(){
		return (type == 0x02 || type == 0x03);
	}

	short type;
	short page;
	char lang[3];
};
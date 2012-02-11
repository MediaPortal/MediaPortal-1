/* 
 *	Copyright (C) 2006-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#include "SkyManager.h"


//	Cstr
CSkyManager::CSkyManager()
{
	try
	{
		Mediaportal::CEnterCriticalSection enter(criticalSection);

		LogDebug("CSkyManager::cstr()");

		isInitialized = false;
		epgFirmwareHuffmanTreesPopulated = false;
		epgGrabbingAbortedThroughTooManyErrors = false;
		
		numberErrorsLogged = 0;
		numberBouquetsPopulated = 0;
		numberFailedHuffmanDecodes = 0;

		//	Initialize all firmware huffman trees
		if(!InitializeEpgFirmwareHuffmanTrees())
			return;

		//	Initialize child objects
		skyEpgParser = new CSkyEpgParser(this);
		skyChannelParser = new CSkyChannelParser(this);

	}
	catch(...)
	{
		LogDebug("CSkyManager::cstr() - Exception whilst initializing");
		return;
	}
	
	isInitialized = true;
	LogDebug("CSkyManager::cstr() - Sky manager initialized successfully");

}

//	Flzr
CSkyManager::~CSkyManager()
{
	Mediaportal::CEnterCriticalSection enter(criticalSection);
	
	LogDebug("CSkyManager::flzr()");

	Reset();

	//	Cleanup
	delete skyEpgParser;
	delete skyChannelParser;

	//	Cleanup huffman tables
	map<unsigned short, CHuffmanTree*>::iterator huffmanIterator;

	for(huffmanIterator = countryFirmwareHuffmanTrees.begin(); huffmanIterator != countryFirmwareHuffmanTrees.end(); huffmanIterator++)
	{
		CHuffmanTree* currentHuffmanTree = huffmanIterator->second;

		delete currentHuffmanTree;
	}
}

//	Logs an error a maximum number of times
void CSkyManager::LogError(const char* message)
{
	if(numberErrorsLogged >= MAX_LOGGED_ERRORS)
		return;

	LogDebug(message);

	numberErrorsLogged++;

	//	If number of errors is exceeded, abort active operations
	if(numberErrorsLogged == MAX_LOGGED_ERRORS)
	{
		LogDebug("CSkyManager - Aborting all current operations, too many errors");
		epgGrabbingAbortedThroughTooManyErrors = true;
	}
}

//	Initializes all the epg firmware huffman trees
bool CSkyManager::InitializeEpgFirmwareHuffmanTrees()
{
	try
	{
		Mediaportal::CEnterCriticalSection enter(criticalSection);


		LogDebug("CSkyManager::InitializeEpgFirmwareHuffmanTrees()");

		//	Create Sky UK firmware huffman tree
		CHuffmanTree* ukFirmwareHuffmanTree = new CHuffmanTree();

		//	Add to the country lookup
		countryFirmwareHuffmanTrees.insert(pair<unsigned char, CHuffmanTree*>(SKY_COUNTRY_UK, ukFirmwareHuffmanTree));


		//	Add all leaf nodes
		ukFirmwareHuffmanTree->AddLeaf(0x055C614C, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C614D, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C614E, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C614F, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6150, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6151, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6152, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6153, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6154, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x00000008, 7, "");
		ukFirmwareHuffmanTree->AddLeaf(0x00000077, 7, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6155, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6156, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6157, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6158, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6159, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C615A, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C615B, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C615C, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C615D, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C615E, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C615F, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6160, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6161, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6162, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6163, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6164, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6165, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6166, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6167, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6168, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6169, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x00000006, 3, "\x20");
		ukFirmwareHuffmanTree->AddLeaf(0x00000218, 11, "\x21");
		ukFirmwareHuffmanTree->AddLeaf(0x055C616A, 27, "\x22");
		ukFirmwareHuffmanTree->AddLeaf(0x055C616B, 27, "\x23");
		ukFirmwareHuffmanTree->AddLeaf(0x0000AB8D, 16, "\x24");
		ukFirmwareHuffmanTree->AddLeaf(0x055C616C, 27, "\x25");
		ukFirmwareHuffmanTree->AddLeaf(0x0000041D, 11, "\x26");
		ukFirmwareHuffmanTree->AddLeaf(0x00000082, 8, "\x27");
		ukFirmwareHuffmanTree->AddLeaf(0x00000745, 11, "\x28");
		ukFirmwareHuffmanTree->AddLeaf(0x000002AC, 10, "\x29");
		ukFirmwareHuffmanTree->AddLeaf(0x000008AE, 12, "\x2A");
		ukFirmwareHuffmanTree->AddLeaf(0x055C616D, 27, "\x2B");
		ukFirmwareHuffmanTree->AddLeaf(0x00000058, 7, "\x2C");
		ukFirmwareHuffmanTree->AddLeaf(0x0000008B, 8, "\x2D");
		ukFirmwareHuffmanTree->AddLeaf(0x00000076, 7, "\x2E");
		ukFirmwareHuffmanTree->AddLeaf(0x0000051E, 14, "\x2F");
		ukFirmwareHuffmanTree->AddLeaf(0x000001D2, 9, "\x30");
		ukFirmwareHuffmanTree->AddLeaf(0x00000167, 9, "\x31");
		ukFirmwareHuffmanTree->AddLeaf(0x00000228, 10, "\x32");
		ukFirmwareHuffmanTree->AddLeaf(0x0000020F, 10, "\x33");
		ukFirmwareHuffmanTree->AddLeaf(0x00000050, 10, "\x34");
		ukFirmwareHuffmanTree->AddLeaf(0x000003A8, 10, "\x35");
		ukFirmwareHuffmanTree->AddLeaf(0x00000229, 10, "\x36");
		ukFirmwareHuffmanTree->AddLeaf(0x00000222, 10, "\x37");
		ukFirmwareHuffmanTree->AddLeaf(0x00000456, 11, "\x38");
		ukFirmwareHuffmanTree->AddLeaf(0x0000010D, 10, "\x39");
		ukFirmwareHuffmanTree->AddLeaf(0x00000746, 11, "\x3A");
		ukFirmwareHuffmanTree->AddLeaf(0x000000A2, 11, "\x3B");
		ukFirmwareHuffmanTree->AddLeaf(0x00001D1F, 13, "\x3C");
		ukFirmwareHuffmanTree->AddLeaf(0x055C616E, 27, "\x3D");
		ukFirmwareHuffmanTree->AddLeaf(0x00001D4C, 13, "\x3E");
		ukFirmwareHuffmanTree->AddLeaf(0x00000EA7, 12, "\x3F");
		ukFirmwareHuffmanTree->AddLeaf(0x0002AE31, 18, "\x40");
		ukFirmwareHuffmanTree->AddLeaf(0x000000E2, 8, "\x41");
		ukFirmwareHuffmanTree->AddLeaf(0x00000040, 8, "\x42");
		ukFirmwareHuffmanTree->AddLeaf(0x00000042, 8, "\x43");
		ukFirmwareHuffmanTree->AddLeaf(0x000001C7, 9, "\x44");
		ukFirmwareHuffmanTree->AddLeaf(0x000003A0, 10, "\x45");
		ukFirmwareHuffmanTree->AddLeaf(0x00000154, 9, "\x46");
		ukFirmwareHuffmanTree->AddLeaf(0x00000110, 9, "\x47");
		ukFirmwareHuffmanTree->AddLeaf(0x00000155, 9, "\x48");
		ukFirmwareHuffmanTree->AddLeaf(0x000003A1, 10, "\x49");
		ukFirmwareHuffmanTree->AddLeaf(0x00000029, 9, "\x4A");
		ukFirmwareHuffmanTree->AddLeaf(0x000003A7, 10, "\x4B");
		ukFirmwareHuffmanTree->AddLeaf(0x00000106, 9, "\x4C");
		ukFirmwareHuffmanTree->AddLeaf(0x00000089, 8, "\x4D");
		ukFirmwareHuffmanTree->AddLeaf(0x000001D7, 9, "\x4E");
		ukFirmwareHuffmanTree->AddLeaf(0x00000082, 9, "\x4F");
		ukFirmwareHuffmanTree->AddLeaf(0x00000015, 8, "\x50");
		ukFirmwareHuffmanTree->AddLeaf(0x00001157, 13, "\x51");
		ukFirmwareHuffmanTree->AddLeaf(0x000001D6, 9, "\x52");
		ukFirmwareHuffmanTree->AddLeaf(0x00000009, 7, "\x53");
		ukFirmwareHuffmanTree->AddLeaf(0x0000000B, 7, "\x54");
		ukFirmwareHuffmanTree->AddLeaf(0x0000055D, 11, "\x55");
		ukFirmwareHuffmanTree->AddLeaf(0x00000755, 11, "\x56");
		ukFirmwareHuffmanTree->AddLeaf(0x000000B2, 8, "\x57");
		ukFirmwareHuffmanTree->AddLeaf(0x0000E36F, 16, "\x58");
		ukFirmwareHuffmanTree->AddLeaf(0x0000055E, 11, "\x59");
		ukFirmwareHuffmanTree->AddLeaf(0x00001D50, 13, "\x5A");
		ukFirmwareHuffmanTree->AddLeaf(0x00015719, 17, "\x5B");
		ukFirmwareHuffmanTree->AddLeaf(0x055C616F, 27, "\x5C");
		ukFirmwareHuffmanTree->AddLeaf(0x0001C6DC, 17, "\x5D");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6170, 27, "\x5E");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6171, 27, "\x5F");
		ukFirmwareHuffmanTree->AddLeaf(0x00000752, 11, "\x60");
		ukFirmwareHuffmanTree->AddLeaf(0x00000009, 4, "\x61");
		ukFirmwareHuffmanTree->AddLeaf(0x00000070, 7, "\x62");
		ukFirmwareHuffmanTree->AddLeaf(0x00000039, 6, "\x63");
		ukFirmwareHuffmanTree->AddLeaf(0x00000009, 5, "\x64");
		ukFirmwareHuffmanTree->AddLeaf(0x0000000F, 4, "\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x00000021, 6, "\x66");
		ukFirmwareHuffmanTree->AddLeaf(0x00000023, 6, "\x67");
		ukFirmwareHuffmanTree->AddLeaf(0x00000017, 5, "\x68");
		ukFirmwareHuffmanTree->AddLeaf(0x00000005, 4, "\x69");
		ukFirmwareHuffmanTree->AddLeaf(0x0000071A, 11, "\x6A");
		ukFirmwareHuffmanTree->AddLeaf(0x00000040, 7, "\x6B");
		ukFirmwareHuffmanTree->AddLeaf(0x00000014, 5, "\x6C");
		ukFirmwareHuffmanTree->AddLeaf(0x0000002B, 6, "\x6D");
		ukFirmwareHuffmanTree->AddLeaf(0x00000007, 4, "\x6E");
		ukFirmwareHuffmanTree->AddLeaf(0x00000003, 4, "\x6F");
		ukFirmwareHuffmanTree->AddLeaf(0x00000007, 6, "\x70");
		ukFirmwareHuffmanTree->AddLeaf(0x0000055B, 11, "\x71");
		ukFirmwareHuffmanTree->AddLeaf(0x00000002, 4, "\x72");
		ukFirmwareHuffmanTree->AddLeaf(0x00000000, 4, "\x73");
		ukFirmwareHuffmanTree->AddLeaf(0x00000006, 4, "\x74");
		ukFirmwareHuffmanTree->AddLeaf(0x0000002D, 6, "\x75");
		ukFirmwareHuffmanTree->AddLeaf(0x00000054, 7, "\x76");
		ukFirmwareHuffmanTree->AddLeaf(0x00000006, 6, "\x77");
		ukFirmwareHuffmanTree->AddLeaf(0x000003AB, 10, "\x78");
		ukFirmwareHuffmanTree->AddLeaf(0x00000011, 6, "\x79");
		ukFirmwareHuffmanTree->AddLeaf(0x000002CC, 10, "\x7A");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6172, 27, "\x7B");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6173, 27, "\x7C");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6174, 27, "\x7D");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6175, 27, "\x7E");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6176, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6177, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6178, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6179, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C617A, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C617B, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C617C, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C617D, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C617E, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C617F, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6180, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6181, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6182, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6183, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6184, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6185, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6186, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6187, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6188, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6189, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C618A, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C618B, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C618C, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C618D, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C618E, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C618F, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6190, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6191, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6192, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x0001C6DD, 17, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6193, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6194, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6195, 27, "");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6196, 27, "\xA0");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6197, 27, "\xA1");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6198, 27, "\xA2");
		ukFirmwareHuffmanTree->AddLeaf(0x055C6199, 27, "\xA3");
		ukFirmwareHuffmanTree->AddLeaf(0x055C619A, 27, "\xA4");
		ukFirmwareHuffmanTree->AddLeaf(0x055C619B, 27, "\xA5");
		ukFirmwareHuffmanTree->AddLeaf(0x055C619C, 27, "\xA6");
		ukFirmwareHuffmanTree->AddLeaf(0x055C619D, 27, "\xA7");
		ukFirmwareHuffmanTree->AddLeaf(0x055C619E, 27, "\xA8");
		ukFirmwareHuffmanTree->AddLeaf(0x055C619F, 27, "\xA9");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61A0, 27, "\xAA");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61A1, 27, "\xAB");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61A2, 27, "\xAC");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61A3, 27, "\xAD");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61A4, 27, "\xAE");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61A5, 27, "\xAF");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61A6, 27, "\xB0");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61A7, 27, "\xB1");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61A8, 27, "\xB2");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61A9, 27, "\xB3");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61AA, 27, "\xB4");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61AB, 27, "\xB5");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61AC, 27, "\xB6");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61AD, 27, "\xB7");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61AE, 27, "\xB8");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61AF, 27, "\xB9");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61B0, 27, "\xBA");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61B1, 27, "\xBB");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61B2, 27, "\xBC");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61B3, 27, "\xBD");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61B4, 27, "\xBE");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61B5, 27, "\xBF");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61B6, 27, "\xC0");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61B7, 27, "\xC1");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61B8, 27, "\xC2");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61B9, 27, "\xC3");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61BA, 27, "\xC4");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61BB, 27, "\xC5");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61BC, 27, "\xC6");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61BD, 27, "\xC7");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61BE, 27, "\xC8");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61BF, 27, "\xC9");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61C0, 27, "\xCA");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61C1, 27, "\xCB");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61C2, 27, "\xCC");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61C3, 27, "\xCD");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61C4, 27, "\xCE");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61C5, 27, "\xCF");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61C6, 27, "\xD0");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61C7, 27, "\xD1");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61C8, 27, "\xD2");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61C9, 27, "\xD3");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61CA, 27, "\xD4");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61CB, 27, "\xD5");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61CC, 27, "\xD6");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61CD, 27, "\xD7");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61CE, 27, "\xD8");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61CF, 27, "\xD9");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61D0, 27, "\xDA");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61D1, 27, "\xDB");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61D2, 27, "\xDC");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61D3, 27, "\xDD");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61D4, 27, "\xDE");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61D5, 27, "\xDF");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61D6, 27, "\xE0");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61D7, 27, "\xE1");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61D8, 27, "\xE2");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61D9, 27, "\xE3");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61DA, 27, "\xE4");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61DB, 27, "\xE5");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61DC, 27, "\xE6");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61DD, 27, "\xE7");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61DE, 27, "\xE8");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61DF, 27, "\xE9");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61E0, 27, "\xEA");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61E1, 27, "\xEB");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61E2, 27, "\xEC");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61E3, 27, "\xED");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61E4, 27, "\xEE");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61E5, 27, "\xEF");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61E6, 27, "\xF0");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61E7, 27, "\xF1");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61E8, 27, "\xF2");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61E9, 27, "\xF3");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61EA, 27, "\xF4");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61EB, 27, "\xF5");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61EC, 27, "\xF6");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61ED, 27, "\xF7");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61EE, 27, "\xF8");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61EF, 27, "\xF9");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61F0, 27, "\xFA");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61F1, 27, "\xFB");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61F2, 27, "\xFC");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61F3, 27, "\xFD");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61F4, 27, "\xFE");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61F5, 27, "\xFF");
		ukFirmwareHuffmanTree->AddLeaf(0x00002AFE, 14, "\x28\x49\x6E\x63\x6C\x75\x64\x69\x6E\x67\x20");
		ukFirmwareHuffmanTree->AddLeaf(0x00003A22, 14, "\x28\x4E\x65\x77\x20\x53\x65\x72\x69\x65\x73\x29");
		ukFirmwareHuffmanTree->AddLeaf(0x00001D1D, 13, "\x28\x50\x61\x72\x74\x20");
		ukFirmwareHuffmanTree->AddLeaf(0x000003A6, 10, "\x28\x52\x65\x70\x65\x61\x74\x29");
		ukFirmwareHuffmanTree->AddLeaf(0x00000087, 9, "\x28\x53\x74\x65\x72\x65\x6F\x29");
		ukFirmwareHuffmanTree->AddLeaf(0x00000083, 9, "\x28\x53\x74\x65\x72\x65\x6F\x29\x20\x28\x54\x65\x6C\x65\x74\x65\x78\x74\x29");
		ukFirmwareHuffmanTree->AddLeaf(0x0000038C, 10, "\x28\x54\x65\x6C\x65\x74\x65\x78\x74\x29");
		ukFirmwareHuffmanTree->AddLeaf(0x000041CE, 15, "\x28\x57\x69\x64\x65\x73\x63\x72\x65\x65\x6E\x29");
		ukFirmwareHuffmanTree->AddLeaf(0x000055C7, 15, "\x41\x63\x74\x69\x6F\x6E");
		ukFirmwareHuffmanTree->AddLeaf(0x00002CDF, 14, "\x41\x64\x76\x65\x6E\x74\x75\x72\x65\x73");
		ukFirmwareHuffmanTree->AddLeaf(0x00000864, 13, "\x41\x6D\x65\x72\x69\x63\x61");
		ukFirmwareHuffmanTree->AddLeaf(0x00007537, 15, "\x41\x6E\x69\x6D\x61\x74\x65\x64");
		ukFirmwareHuffmanTree->AddLeaf(0x00000865, 13, "\x41\x75\x73\x74\x72\x61\x6C\x69\x61");
		ukFirmwareHuffmanTree->AddLeaf(0x00003AA2, 14, "\x41\x77\x61\x79");
		ukFirmwareHuffmanTree->AddLeaf(0x00002AFF, 14, "\x42\x42\x43");
		ukFirmwareHuffmanTree->AddLeaf(0x000038D8, 14, "\x42\x61\x62\x79");
		ukFirmwareHuffmanTree->AddLeaf(0x00003AA3, 14, "\x42\x65\x73\x74");
		ukFirmwareHuffmanTree->AddLeaf(0x00002CD8, 14, "\x42\x69\x67");
		ukFirmwareHuffmanTree->AddLeaf(0x0000115F, 13, "\x42\x69\x6C\x6C");
		ukFirmwareHuffmanTree->AddLeaf(0x00001150, 13, "\x42\x6C\x61\x63\x6B");
		ukFirmwareHuffmanTree->AddLeaf(0x0000166E, 13, "\x42\x6C\x75\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x00000146, 12, "\x42\x72\x65\x61\x6B\x66\x61\x73\x74");
		ukFirmwareHuffmanTree->AddLeaf(0x0000157C, 13, "\x42\x72\x69\x74\x61\x69\x6E");
		ukFirmwareHuffmanTree->AddLeaf(0x00001D1C, 13, "\x42\x72\x69\x74\x69\x73\x68");
		ukFirmwareHuffmanTree->AddLeaf(0x00000866, 13, "\x42\x75\x73\x69\x6E\x65\x73\x73");
		ukFirmwareHuffmanTree->AddLeaf(0x0000157D, 13, "\x43\x61\x6C\x6C");
		ukFirmwareHuffmanTree->AddLeaf(0x00002AE0, 14, "\x43\x61\x72\x74\x6F\x6F\x6E");
		ukFirmwareHuffmanTree->AddLeaf(0x00002AE1, 14, "\x43\x68\x61\x6E\x6E\x65\x6C");
		ukFirmwareHuffmanTree->AddLeaf(0x00003AA7, 14, "\x43\x68\x69\x6C\x64\x72\x65\x6E");
		ukFirmwareHuffmanTree->AddLeaf(0x000038D9, 14, "\x43\x6C\x6F\x63\x6B");
		ukFirmwareHuffmanTree->AddLeaf(0x00003A23, 14, "\x43\x6F\x6D\x65\x64\x79");
		ukFirmwareHuffmanTree->AddLeaf(0x0000754A, 15, "\x43\x6F\x6F\x6B");
		ukFirmwareHuffmanTree->AddLeaf(0x00007536, 15, "\x43\x6F\x75\x6E\x74\x72\x79");
		ukFirmwareHuffmanTree->AddLeaf(0x00000AB4, 12, "\x44\x69\x72\x65\x63\x74\x65\x64\x20\x62\x79\x20");
		ukFirmwareHuffmanTree->AddLeaf(0x00000867, 13, "\x44\x72\x61\x6D\x61");
		ukFirmwareHuffmanTree->AddLeaf(0x00001151, 13, "\x45\x61\x73\x74");
		ukFirmwareHuffmanTree->AddLeaf(0x000041CF, 15, "\x45\x64\x75\x63\x61\x74\x69\x6F\x6E");
		ukFirmwareHuffmanTree->AddLeaf(0x0000051F, 14, "\x45\x6E\x67\x6C\x69\x73\x68");
		ukFirmwareHuffmanTree->AddLeaf(0x0000028E, 13, "\x45\x75\x72\x6F\x70\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x00002CD9, 14, "\x45\x78\x74\x72\x61");
		ukFirmwareHuffmanTree->AddLeaf(0x00002AE2, 14, "\x46\x69\x6E\x61\x6C");
		ukFirmwareHuffmanTree->AddLeaf(0x000071B4, 15, "\x46\x69\x6E\x61\x6E\x63\x69\x61\x6C");
		ukFirmwareHuffmanTree->AddLeaf(0x00000E37, 12, "\x46\x6F\x72");
		ukFirmwareHuffmanTree->AddLeaf(0x00003A3D, 14, "\x46\x72\x65\x6E\x63\x68");
		ukFirmwareHuffmanTree->AddLeaf(0x00001152, 13, "\x46\x72\x6F\x6D");
		ukFirmwareHuffmanTree->AddLeaf(0x0000157E, 13, "\x47\x65\x6F\x72\x67\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x0000111A, 13, "\x47\x65\x74");
		ukFirmwareHuffmanTree->AddLeaf(0x00002236, 14, "\x47\x69\x72\x6C\x73");
		ukFirmwareHuffmanTree->AddLeaf(0x00002237, 14, "\x47\x6F\x6C\x64\x65\x6E");
		ukFirmwareHuffmanTree->AddLeaf(0x0000754B, 15, "\x47\x6F\x6C\x66");
		ukFirmwareHuffmanTree->AddLeaf(0x0000156A, 13, "\x47\x6F\x6F\x64");
		ukFirmwareHuffmanTree->AddLeaf(0x00003A24, 14, "\x47\x72\x65\x61\x74");
		ukFirmwareHuffmanTree->AddLeaf(0x0000754C, 15, "\x48\x61\x6D\x70\x73\x68\x69\x72\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x00001153, 13, "\x48\x65\x61\x64\x6C\x69\x6E\x65\x73");
		ukFirmwareHuffmanTree->AddLeaf(0x00003A9A, 14, "\x48\x65\x61\x72");
		ukFirmwareHuffmanTree->AddLeaf(0x00001070, 13, "\x48\x69\x6C\x6C");
		ukFirmwareHuffmanTree->AddLeaf(0x000071B5, 15, "\x48\x6F\x6C\x6C\x79\x77\x6F\x6F\x64");
		ukFirmwareHuffmanTree->AddLeaf(0x00001154, 13, "\x48\x6F\x6D\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x00003A25, 14, "\x48\x6F\x75\x72");
		ukFirmwareHuffmanTree->AddLeaf(0x00001072, 13, "\x48\x6F\x75\x73\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x0000156B, 13, "\x48\x6F\x77");
		ukFirmwareHuffmanTree->AddLeaf(0x00003AA4, 14, "\x49\x54\x4E");
		ukFirmwareHuffmanTree->AddLeaf(0x0000754D, 15, "\x49\x6D\x70\x6F\x72\x74\x61\x6E\x74");
		ukFirmwareHuffmanTree->AddLeaf(0x0000115E, 13, "\x49\x6E\x63\x6C\x75\x64\x69\x6E\x67");
		ukFirmwareHuffmanTree->AddLeaf(0x00003A26, 14, "\x49\x6E\x74\x65\x72\x6E\x61\x74\x69\x6F\x6E\x61\x6C");
		ukFirmwareHuffmanTree->AddLeaf(0x00000447, 11, "\x4A\x6F\x68\x6E");
		ukFirmwareHuffmanTree->AddLeaf(0x00003A27, 14, "\x4C\x61\x73\x74");
		ukFirmwareHuffmanTree->AddLeaf(0x000020E6, 14, "\x4C\x61\x74\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x000022AC, 14, "\x4C\x65\x61\x72\x6E");
		ukFirmwareHuffmanTree->AddLeaf(0x000022AD, 14, "\x4C\x69\x74\x74\x6C\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x00001D10, 13, "\x4C\x69\x76\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x00003A3C, 14, "\x4C\x6F\x6E\x64\x6F\x6E");
		ukFirmwareHuffmanTree->AddLeaf(0x00002CDE, 14, "\x4C\x6F\x6F\x6B");
		ukFirmwareHuffmanTree->AddLeaf(0x000071B6, 15, "\x4C\x75\x6E\x63\x68");
		ukFirmwareHuffmanTree->AddLeaf(0x00001155, 13, "\x4D\x61\x6E");
		ukFirmwareHuffmanTree->AddLeaf(0x00001071, 13, "\x4D\x61\x72\x6B");
		ukFirmwareHuffmanTree->AddLeaf(0x00000AB9, 12, "\x4D\x65\x72\x69\x64\x69\x61\x6E");
		ukFirmwareHuffmanTree->AddLeaf(0x0000166D, 13, "\x4D\x69\x63\x68\x61\x65\x6C");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61F6, 27, "\x4D\x69\x6E\x75\x74\x65\x73");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61F7, 27, "\x4D\x6F\x72\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61F8, 27, "\x4D\x6F\x72\x6E\x69\x6E\x67");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61F9, 27, "\x4D\x75\x72\x64\x65\x72");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61FA, 27, "\x4E\x61\x74\x69\x6F\x6E");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61FB, 27, "\x4E\x65\x69\x67\x68\x62\x6F\x75\x72\x73");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61FC, 27, "\x4E\x65\x77");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61FD, 27, "\x4E\x65\x77\x73\x20\x26\x20\x57\x65\x61\x74\x68\x65\x72");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61FE, 27, "\x4E\x65\x77\x73\x20\x41\x6E\x64\x20\x57\x65\x61\x74\x68\x65\x72");
		ukFirmwareHuffmanTree->AddLeaf(0x055C61FF, 27, "\x50\x61\x75\x6C");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3000, 26, "\x50\x6C\x75\x73");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3001, 26, "\x50\x72\x61\x79\x65\x72");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3002, 26, "\x50\x72\x65\x73\x65\x6E\x74");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3003, 26, "\x50\x72\x65\x73\x65\x6E\x74\x65\x64\x20\x62\x79");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3004, 26, "\x51\x75\x69\x7A");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3005, 26, "\x52\x65\x67\x69\x6F\x6E\x61\x6C");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3006, 26, "\x52\x65\x70\x72\x65\x73\x65\x6E\x74");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3007, 26, "\x52\x65\x73\x6F\x75\x72\x63\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3008, 26, "\x52\x65\x76\x69\x65\x77");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3009, 26, "\x52\x69\x63\x68\x61\x72\x64");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE300A, 26, "\x53\x63\x68\x6F\x6F\x6C");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE300B, 26, "\x53\x65\x72\x69\x65\x73");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE300C, 26, "\x53\x65\x72\x76\x69\x63\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE300D, 26, "\x53\x68\x6F\x77");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE300E, 26, "\x53\x6D\x69\x74\x68");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE300F, 26, "\x53\x6F\x75\x74\x68");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3010, 26, "\x53\x70\x6F\x72\x74");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3011, 26, "\x53\x74\x61\x72");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3012, 26, "\x53\x74\x72\x65\x65\x74");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3013, 26, "\x54\x56");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3014, 26, "\x54\x65\x61\x63\x68\x69\x6E\x67");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3015, 26, "\x54\x68\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3016, 26, "\x54\x6F\x64\x61\x79");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3017, 26, "\x54\x6F\x6E\x69\x67\x68\x74");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3018, 26, "\x57\x65\x61\x74\x68\x65\x72");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3019, 26, "\x57\x65\x73\x74\x65\x72\x6E");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE301A, 26, "\x57\x65\x73\x74\x6D\x69\x6E\x73\x74\x65\x72");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE301B, 26, "\x57\x69\x6C\x6C\x69\x61\x6D");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE301C, 26, "\x57\x69\x74\x68");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE301D, 26, "\x57\x6F\x72\x6C\x64");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE301E, 26, "\x61\x62\x6F\x75\x74");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE301F, 26, "\x61\x63\x74\x69\x6F\x6E\x2D\x70\x61\x63\x6B\x65\x64");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3020, 26, "\x61\x64\x76\x65\x6E\x74\x75\x72\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3021, 26, "\x61\x66\x74\x65\x72\x6E\x6F\x6F\x6E");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3022, 26, "\x61\x6C\x65\x72\x74");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3023, 26, "\x61\x6C\x6C\x2D\x73\x74\x61\x72\x20\x63\x61\x73\x74");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3024, 26, "\x61\x6E\x64");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3025, 26, "\x61\x6E\x79\x77\x68\x65\x72\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3026, 26, "\x61\x75\x64\x69\x65\x6E\x63\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3027, 26, "\x62\x61\x73\x65\x64");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3028, 26, "\x62\x6F\x6F\x6B");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3029, 26, "\x62\x75\x73\x69\x6E\x65\x73\x73");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE302A, 26, "\x62\x75\x74");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE302B, 26, "\x63\x65\x6C\x65\x62\x72\x69\x74\x79");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE302C, 26, "\x63\x68\x61\x6E\x63\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE302D, 26, "\x63\x68\x61\x74");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE302E, 26, "\x63\x68\x69\x6C\x64");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE302F, 26, "\x63\x6C\x61\x73\x73\x69\x63");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3030, 26, "\x63\x6F\x6E\x73\x75\x6D\x65\x72");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3031, 26, "\x63\x6F\x6E\x74\x65\x73\x74\x61\x6E\x74\x73");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3032, 26, "\x63\x6F\x6E\x74\x69\x6E\x75\x65\x73");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3033, 26, "\x63\x6F\x6E\x74\x72\x6F\x76\x65\x72\x73\x69\x61\x6C");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3034, 26, "\x64\x65\x61\x6C\x65\x72");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3035, 26, "\x64\x65\x6C\x69\x76\x65\x72");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3036, 26, "\x64\x69\x73\x63\x75\x73\x73");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3037, 26, "\x64\x6F\x63\x75\x6D\x65\x6E\x74");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3038, 26, "\x64\x72\x61\x6D\x61");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3039, 26, "\x65\x64\x69\x74\x69\x6F\x6E");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE303A, 26, "\x65\x64\x75\x63\x61\x74\x69\x6F\x6E");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE303B, 26, "\x65\x76\x65\x6E\x74\x73");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE303C, 26, "\x65\x76\x65\x72\x79");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE303D, 26, "\x65\x78\x63\x65\x6C\x6C\x65\x6E\x74");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE303E, 26, "\x65\x79\x65\x64");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE303F, 26, "\x66\x61\x6D\x69\x6C\x79");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3040, 26, "\x66\x61\x6D\x6F\x75\x73");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3041, 26, "\x66\x65\x61\x74\x75\x72");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3042, 26, "\x66\x69\x6C\x6D");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3043, 26, "\x66\x6F\x6F\x74\x62\x61\x6C\x6C");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3044, 26, "\x66\x6F\x72");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3045, 26, "\x66\x72\x6F\x6D");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3046, 26, "\x67\x65\x6E\x65\x72\x61\x6C\x20\x6B\x6E\x6F\x77\x6C\x65\x64\x67\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3047, 26, "\x67\x65\x74");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3048, 26, "\x67\x75\x65\x73\x74");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3049, 26, "\x67\x75\x65\x73\x74\x73");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE304A, 26, "\x68\x61\x73");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE304B, 26, "\x68\x61\x76\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE304C, 26, "\x68\x65\x61\x64\x6C\x69\x6E\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE304D, 26, "\x68\x65\x72");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE304E, 26, "\x68\x69\x73");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE304F, 26, "\x68\x6F\x6D\x65\x20\x61\x6E\x64\x20\x61\x62\x72\x6F\x61\x64");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3050, 26, "\x68\x6F\x73\x74");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3051, 26, "\x68\x6F\x77");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3052, 26, "\x69\x6E");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3053, 26, "\x69\x6E\x63\x6C\x75\x64\x69\x6E\x67");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3054, 26, "\x69\x6E\x74\x65\x72\x6E\x61\x74\x69\x6F\x6E\x61\x6C");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3055, 26, "\x69\x6E\x74\x65\x72\x76\x69\x65\x77");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3056, 26, "\x69\x6E\x74\x72\x6F\x64\x75\x63\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3057, 26, "\x69\x6E\x76\x65\x73\x74\x69\x67\x61\x74");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3058, 26, "\x69\x6E\x76\x69\x74\x65\x73");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3059, 26, "\x69\x73\x73\x75\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE305A, 26, "\x6B\x6E\x6F\x77\x6C\x65\x64\x67\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE305B, 26, "\x6C\x69\x66\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE305C, 26, "\x6C\x69\x76\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE305D, 26, "\x6C\x6F\x6F\x6B");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE305E, 26, "\x6D\x61\x67\x61\x7A\x69\x6E\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE305F, 26, "\x6D\x65\x65\x74\x73\x20");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3060, 26, "\x6D\x6F\x72\x6E\x69\x6E\x67");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3061, 26, "\x6D\x6F\x72\x6E\x69\x6E\x67\x20\x6D\x61\x67\x61\x7A\x69\x6E\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3062, 26, "\x6D\x75\x73\x69\x63");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3063, 26, "\x6E\x65\x61\x72");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3064, 26, "\x6E\x65\x74\x77\x6F\x72\x6B");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3065, 26, "\x6E\x65\x77");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3066, 26, "\x6E\x65\x77\x20\x73\x65\x72\x69\x65\x73");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3067, 26, "\x6E\x69\x67\x68\x74");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3068, 26, "\x6F\x66");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3069, 26, "\x6F\x6E");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE306A, 26, "\x6F\x6E\x69\x67\x68\x74");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE306B, 26, "\x6F\x75\x74");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE306C, 26, "\x6F\x76\x65\x72");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE306D, 26, "\x70\x61\x72\x74");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE306E, 26, "\x70\x65\x6F\x70\x6C\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE306F, 26, "\x70\x68\x6F\x6E\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3070, 26, "\x70\x6F\x6C\x69");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3071, 26, "\x70\x6F\x6C\x69\x63\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3072, 26, "\x70\x6F\x6C\x69\x74\x69\x63\x61\x6C\x20\x63\x68\x61\x74\x20\x73\x68\x6F\x77");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3073, 26, "\x70\x6F\x70\x75\x6C\x61\x72");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3074, 26, "\x70\x72\x65\x73\x65\x6E\x74\x65\x64\x20\x62\x79\x20");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3075, 26, "\x70\x72\x6F\x67\x72\x61\x6D\x6D");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3076, 26, "\x71\x75\x69\x7A");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3077, 26, "\x72\x65\x63\x6F\x6E\x73\x74\x72\x75\x63\x74\x69\x6F\x6E");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3078, 26, "\x72\x65\x70\x6F\x72\x74");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3079, 26, "\x72\x65\x76\x69\x65\x77");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE307A, 26, "\x73\x63\x68\x6F\x6F\x6C");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE307B, 26, "\x73\x65\x72\x69\x65\x73");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE307C, 26, "\x73\x68\x6F\x72\x74\x20");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE307D, 26, "\x73\x68\x6F\x77");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE307E, 26, "\x73\x6F\x6D\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE307F, 26, "\x73\x74\x61\x72\x72\x69\x6E\x67");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3080, 26, "\x73\x74\x61\x72\x73");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3081, 26, "\x73\x74\x6F\x72\x69\x65\x73");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3082, 26, "\x73\x74\x6F\x72\x79");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3083, 26, "\x73\x74\x75\x64\x69\x6F");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3084, 26, "\x73\x75\x72\x70\x72\x69\x73\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3085, 26, "\x74\x65\x6C\x6C\x65\x72");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3086, 26, "\x74\x68\x61\x74");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3087, 26, "\x74\x68\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3088, 26, "\x74\x68\x65\x69\x72");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3089, 26, "\x74\x68\x65\x6D");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE308A, 26, "\x74\x68\x65\x79");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE308B, 26, "\x74\x68\x69\x73");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE308C, 26, "\x74\x68\x72\x6F\x75\x67\x68");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE308D, 26, "\x74\x6F");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE308E, 26, "\x74\x6F\x70");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE308F, 26, "\x74\x72\x61\x6E\x73");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3090, 26, "\x75\x6E\x64\x65\x72");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3091, 26, "\x75\x70");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3092, 26, "\x76\x65\x72\x79");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3093, 26, "\x76\x69\x64\x65\x6F");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3094, 26, "\x76\x69\x65\x77");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3095, 26, "\x76\x69\x6E\x74\x61\x67\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3096, 26, "\x76\x69\x73\x69\x74");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3097, 26, "\x77\x61\x73");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3098, 26, "\x77\x61\x79");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE3099, 26, "\x77\x65\x65\x6B");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE309A, 26, "\x77\x65\x6C\x6C");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE309B, 26, "\x77\x68\x61\x74");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE309C, 26, "\x77\x68\x65\x6E");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE309D, 26, "\x77\x68\x69\x63\x68");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE309E, 26, "\x77\x68\x69\x6C\x65");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE309F, 26, "\x77\x68\x6F");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE30A0, 26, "\x77\x69\x6C\x6C");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE30A1, 26, "\x77\x69\x6E");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE30A2, 26, "\x77\x69\x74\x68");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE30A3, 26, "\x77\x6F\x72\x64\x73");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE30A4, 26, "\x77\x6F\x72\x6C\x64");
		ukFirmwareHuffmanTree->AddLeaf(0x02AE30A5, 26, "\x77\x72\x69\x74\x74\x65\x6E");
		ukFirmwareHuffmanTree->AddLeaf(0x0000088C, 12, "\x79\x65\x61\x72");
		ukFirmwareHuffmanTree->AddLeaf(0x0000059A, 11, "\x79\x6F\x75");



		
		//	Create Sky Italy firmware huffman tree
		CHuffmanTree* italyFirmwareHuffmanTree = new CHuffmanTree();

		//	Add to the country lookup
		countryFirmwareHuffmanTrees.insert(pair<unsigned char, CHuffmanTree*>(SKY_COUNTRY_IT, italyFirmwareHuffmanTree));


		//	Add all leaf nodes
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EAA, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EAB, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EAC, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EAD, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EAE, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EAF, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EB0, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EB1, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EB2, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EB3, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EB4, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EB5, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EB6, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x0000004E, 7, "\x09");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EB7, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EB8, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EB9, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EBA, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EBB, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EBC, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EBD, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EBE, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EBF, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EC0, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EC1, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EC2, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EC3, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EC4, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EC5, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EC6, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EC7, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EC8, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x00000001, 2, "\x20");
		italyFirmwareHuffmanTree->AddLeaf(0x00001C1D, 13, "\x21");
		italyFirmwareHuffmanTree->AddLeaf(0x0001C423, 17, "\x22");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EC9, 29, "\x23");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1ECA, 29, "\x24");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1ECB, 29, "\x25");
		italyFirmwareHuffmanTree->AddLeaf(0x00003885, 14, "\x26");
		italyFirmwareHuffmanTree->AddLeaf(0x00000004, 7, "\x27");
		italyFirmwareHuffmanTree->AddLeaf(0x0000016A, 13, "\x28");
		italyFirmwareHuffmanTree->AddLeaf(0x00000D3F, 12, "\x29");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1ECC, 29, "\x2A");
		italyFirmwareHuffmanTree->AddLeaf(0x00016B1F, 21, "\x2B");
		italyFirmwareHuffmanTree->AddLeaf(0x00000030, 6, "\x2C");
		italyFirmwareHuffmanTree->AddLeaf(0x000000B3, 8, "\x2D");
		italyFirmwareHuffmanTree->AddLeaf(0x00000026, 6, "\x2E");
		italyFirmwareHuffmanTree->AddLeaf(0x00007109, 15, "\x2F");
		italyFirmwareHuffmanTree->AddLeaf(0x00000093, 8, "\x30");
		italyFirmwareHuffmanTree->AddLeaf(0x00000120, 9, "\x31");
		italyFirmwareHuffmanTree->AddLeaf(0x000003D9, 10, "\x32");
		italyFirmwareHuffmanTree->AddLeaf(0x0000038E, 10, "\x33");
		italyFirmwareHuffmanTree->AddLeaf(0x00000625, 11, "\x34");
		italyFirmwareHuffmanTree->AddLeaf(0x00000F74, 12, "\x35");
		italyFirmwareHuffmanTree->AddLeaf(0x00000B2F, 12, "\x36");
		italyFirmwareHuffmanTree->AddLeaf(0x00000D1E, 12, "\x37");
		italyFirmwareHuffmanTree->AddLeaf(0x0000068E, 11, "\x38");
		italyFirmwareHuffmanTree->AddLeaf(0x000003D7, 10, "\x39");
		italyFirmwareHuffmanTree->AddLeaf(0x00000123, 9, "\x3A");
		italyFirmwareHuffmanTree->AddLeaf(0x0000043D, 11, "\x3B");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1ECD, 29, "\x3C");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1ECE, 29, "\x3D");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1ECF, 29, "\x3E");
		italyFirmwareHuffmanTree->AddLeaf(0x000019B6, 13, "\x3F");
		italyFirmwareHuffmanTree->AddLeaf(0x0000E210, 16, "\x40");
		italyFirmwareHuffmanTree->AddLeaf(0x0000000A, 8, "\x41");
		italyFirmwareHuffmanTree->AddLeaf(0x000001E9, 9, "\x42");
		italyFirmwareHuffmanTree->AddLeaf(0x00000094, 8, "\x43");
		italyFirmwareHuffmanTree->AddLeaf(0x0000010A, 9, "\x44");
		italyFirmwareHuffmanTree->AddLeaf(0x000001C0, 9, "\x45");
		italyFirmwareHuffmanTree->AddLeaf(0x0000038C, 10, "\x46");
		italyFirmwareHuffmanTree->AddLeaf(0x0000012C, 9, "\x47");
		italyFirmwareHuffmanTree->AddLeaf(0x0000021F, 10, "\x48");
		italyFirmwareHuffmanTree->AddLeaf(0x000003DF, 10, "\x49");
		italyFirmwareHuffmanTree->AddLeaf(0x00000316, 10, "\x4A");
		italyFirmwareHuffmanTree->AddLeaf(0x000007A3, 11, "\x4B");
		italyFirmwareHuffmanTree->AddLeaf(0x0000007D, 9, "\x4C");
		italyFirmwareHuffmanTree->AddLeaf(0x0000010D, 9, "\x4D");
		italyFirmwareHuffmanTree->AddLeaf(0x0000010B, 9, "\x4E");
		italyFirmwareHuffmanTree->AddLeaf(0x0000032B, 10, "\x4F");
		italyFirmwareHuffmanTree->AddLeaf(0x000001A0, 9, "\x50");
		italyFirmwareHuffmanTree->AddLeaf(0x00000058, 11, "\x51");
		italyFirmwareHuffmanTree->AddLeaf(0x00000194, 9, "\x52");
		italyFirmwareHuffmanTree->AddLeaf(0x000000B4, 8, "\x53");
		italyFirmwareHuffmanTree->AddLeaf(0x000001AC, 9, "\x54");
		italyFirmwareHuffmanTree->AddLeaf(0x00000D13, 12, "\x55");
		italyFirmwareHuffmanTree->AddLeaf(0x00000243, 10, "\x56");
		italyFirmwareHuffmanTree->AddLeaf(0x00000346, 10, "\x57");
		italyFirmwareHuffmanTree->AddLeaf(0x00001C59, 13, "\x58");
		italyFirmwareHuffmanTree->AddLeaf(0x000018F3, 13, "\x59");
		italyFirmwareHuffmanTree->AddLeaf(0x000018A6, 13, "\x5A");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1ED0, 29, "\x5B");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1ED1, 29, "\x5C");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1ED2, 29, "\x5D");
		italyFirmwareHuffmanTree->AddLeaf(0x00005AC4, 19, "\x5E");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1ED3, 29, "\x5F");
		italyFirmwareHuffmanTree->AddLeaf(0x000002D7, 14, "\x60");
		italyFirmwareHuffmanTree->AddLeaf(0x0000001F, 5, "\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x000000E1, 8, "\x62");
		italyFirmwareHuffmanTree->AddLeaf(0x00000039, 6, "\x63");
		italyFirmwareHuffmanTree->AddLeaf(0x00000058, 7, "\x64");
		italyFirmwareHuffmanTree->AddLeaf(0x0000000A, 4, "\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x0000001B, 7, "\x66");
		italyFirmwareHuffmanTree->AddLeaf(0x00000003, 6, "\x67");
		italyFirmwareHuffmanTree->AddLeaf(0x000000B6, 8, "\x68");
		italyFirmwareHuffmanTree->AddLeaf(0x00000001, 4, "\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x00000D10, 12, "\x6A");
		italyFirmwareHuffmanTree->AddLeaf(0x0000018C, 9, "\x6B");
		italyFirmwareHuffmanTree->AddLeaf(0x00000079, 7, "\x6C");
		italyFirmwareHuffmanTree->AddLeaf(0x00000022, 6, "\x6D");
		italyFirmwareHuffmanTree->AddLeaf(0x00000017, 5, "\x6E");
		italyFirmwareHuffmanTree->AddLeaf(0x00000002, 4, "\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x00000023, 6, "\x70");
		italyFirmwareHuffmanTree->AddLeaf(0x00000389, 10, "\x71");
		italyFirmwareHuffmanTree->AddLeaf(0x0000001B, 5, "\x72");
		italyFirmwareHuffmanTree->AddLeaf(0x00000000, 5, "\x73");
		italyFirmwareHuffmanTree->AddLeaf(0x0000001D, 5, "\x74");
		italyFirmwareHuffmanTree->AddLeaf(0x0000000E, 6, "\x75");
		italyFirmwareHuffmanTree->AddLeaf(0x00000067, 7, "\x76");
		italyFirmwareHuffmanTree->AddLeaf(0x00000066, 9, "\x77");
		italyFirmwareHuffmanTree->AddLeaf(0x0000071E, 11, "\x78");
		italyFirmwareHuffmanTree->AddLeaf(0x000001ED, 9, "\x79");
		italyFirmwareHuffmanTree->AddLeaf(0x0000001A, 7, "\x7A");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1ED4, 29, "\x7B");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1ED5, 29, "\x7C");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1ED6, 29, "\x7D");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1ED7, 29, "\x7E");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1ED8, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x00002D60, 18, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1ED9, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EDA, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EDB, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EDC, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EDD, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EDE, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EDF, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x0001C422, 17, "");
		italyFirmwareHuffmanTree->AddLeaf(0x00002D61, 18, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EE0, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EE1, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EE2, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EE3, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EE4, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EE5, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EE6, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EE7, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x0000B58E, 20, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EE8, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EE9, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EEA, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EEB, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x00005AC5, 19, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EEC, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EED, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EEE, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EEF, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EF0, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EF1, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EF2, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EF3, 29, "");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EF4, 29, "\xA0");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EF5, 29, "\xA1");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EF6, 29, "\xA2");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EF7, 29, "\xA3");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EF8, 29, "\xA4");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EF9, 29, "\xA5");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EFA, 29, "\xA6");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EFB, 29, "\xA7");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EFC, 29, "\xA8");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EFD, 29, "\xA9");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EFE, 29, "\xAA");
		italyFirmwareHuffmanTree->AddLeaf(0x016B1EFF, 29, "\xAB");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F00, 28, "\xAC");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F01, 28, "\xAD");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F02, 28, "\xAE");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F03, 28, "\xAF");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F04, 28, "\xB0");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F05, 28, "\xB1");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F06, 28, "\xB2");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F07, 28, "\xB3");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F08, 28, "\xB4");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F09, 28, "\xB5");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F0A, 28, "\xB6");
		italyFirmwareHuffmanTree->AddLeaf(0x00000B59, 16, "\xB7");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F0B, 28, "\xB8");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F0C, 28, "\xB9");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F0D, 28, "\xBA");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F0E, 28, "\xBB");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F0F, 28, "\xBC");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F10, 28, "\xBD");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F11, 28, "\xBE");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F12, 28, "\xBF");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F13, 28, "\xC0");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F14, 28, "\xC1");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F15, 28, "\xC2");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F16, 28, "\xC3");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F17, 28, "\xC4");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F18, 28, "\xC5");
		italyFirmwareHuffmanTree->AddLeaf(0x000005AD, 15, "\xC6");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F19, 28, "\xC7");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F1A, 28, "\xC8");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F1B, 28, "\xC9");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F1C, 28, "\xCA");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F1D, 28, "\xCB");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F1E, 28, "\xCC");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F1F, 28, "\xCD");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F20, 28, "\xCE");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F21, 28, "\xCF");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F22, 28, "\xD0");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F23, 28, "\xD1");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F24, 28, "\xD2");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F25, 28, "\xD3");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F26, 28, "\xD4");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F27, 28, "\xD5");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F28, 28, "\xD6");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F29, 28, "\xD7");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F2A, 28, "\xD8");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F2B, 28, "\xD9");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F2C, 28, "\xDA");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F2D, 28, "\xDB");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F2E, 28, "\xDC");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F2F, 28, "\xDD");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F30, 28, "\xDE");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F31, 28, "\xDF");
		italyFirmwareHuffmanTree->AddLeaf(0x00005AC6, 19, "\xE0");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F32, 28, "\xE1");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F33, 28, "\xE2");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F34, 28, "\xE3");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F35, 28, "\xE4");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F36, 28, "\xE5");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F37, 28, "\xE6");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F38, 28, "\xE7");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F39, 28, "\xE8");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F3A, 28, "\xE9");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F3B, 28, "\xEA");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F3C, 28, "\xEB");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F3D, 28, "\xEC");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F3E, 28, "\xED");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F3F, 28, "\xEE");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F40, 28, "\xEF");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F41, 28, "\xF0");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F42, 28, "\xF1");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F43, 28, "\xF2");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F44, 28, "\xF3");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F45, 28, "\xF4");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F46, 28, "\xF5");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F47, 28, "\xF6");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F48, 28, "\xF7");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F49, 28, "\xF8");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F4A, 28, "\xF9");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F4B, 28, "\xFA");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F4C, 28, "\xFB");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F4D, 28, "\xFC");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F4E, 28, "\xFD");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F4F, 28, "\xFE");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F50, 28, "\xFF");
		italyFirmwareHuffmanTree->AddLeaf(0x00000878, 12, "\x28\x64\x75\x72");
		italyFirmwareHuffmanTree->AddLeaf(0x0000007C, 9, "\x32\x30\x30\x31");
		italyFirmwareHuffmanTree->AddLeaf(0x00000E35, 12, "\x32\x30\x30\x32");
		italyFirmwareHuffmanTree->AddLeaf(0x00000879, 12, "\x41\x64\x75\x6C\x74\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x00000337, 10, "\x41\x6C\x6C\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x000005AC, 11, "\x41\x6D\x65\x72\x69\x63\x61\x6E");
		italyFirmwareHuffmanTree->AddLeaf(0x00000CAB, 12, "\x41\x72\x67\x65\x6E\x74\x69\x6E\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x00000C41, 12, "\x41\x74\x74\x75\x61\x6C\x69\x74\x61\x27");
		italyFirmwareHuffmanTree->AddLeaf(0x000004BF, 11, "\x42\x61\x74\x65\x6D\x61\x6E");
		italyFirmwareHuffmanTree->AddLeaf(0x000018BB, 13, "\x42\x65\x63\x68\x69\x73");
		italyFirmwareHuffmanTree->AddLeaf(0x00001A78, 13, "\x43\x61\x6D\x70\x69\x6F\x6E\x61\x74\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x00001A1D, 13, "\x43\x61\x72\x6C\x6F\x73");
		italyFirmwareHuffmanTree->AddLeaf(0x000007BB, 11, "\x43\x61\x72\x74\x6F\x6F\x6E");
		italyFirmwareHuffmanTree->AddLeaf(0x00000432, 11, "\x43\x6C\x75\x62");
		italyFirmwareHuffmanTree->AddLeaf(0x00000861, 12, "\x43\x6F\x6D\x6D\x65\x64\x69\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x000005AD, 11, "\x43\x6F\x6E");
		italyFirmwareHuffmanTree->AddLeaf(0x0000191F, 13, "\x44\x27\x41\x6C\x6F\x27");
		italyFirmwareHuffmanTree->AddLeaf(0x00000257, 10, "\x44\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x00001EA3, 13, "\x44\x61\x6C");
		italyFirmwareHuffmanTree->AddLeaf(0x000004BE, 11, "\x44\x61\x6C\x6C\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x00000F79, 12, "\x44\x72\x61\x6D\x6D\x61\x74\x69\x63\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x00001A3E, 13, "\x44\x75\x72\x61\x6E\x74\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x000018E6, 13, "\x45\x63\x68\x65\x76\x65\x72\x72\x69\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x000018E7, 13, "\x45\x6D\x6D\x61\x6E\x75\x65\x6C\x6C\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x00001A22, 13, "\x45\x6E\x7A\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x0000005B, 11, "\x46\x61\x72\x65\x73");
		italyFirmwareHuffmanTree->AddLeaf(0x00001918, 13, "\x46\x69\x67\x6C\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x0000035A, 10, "\x46\x69\x6C\x6D");
		italyFirmwareHuffmanTree->AddLeaf(0x0000084C, 12, "\x46\x69\x6E\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x0000005C, 11, "\x46\x69\x75\x6D\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x00000431, 11, "\x46\x72\x61\x6E\x63\x69\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x00000C48, 12, "\x47\x69\x61\x6C\x6C\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x000018E8, 13, "\x47\x69\x6F\x76\x61\x6E\x6E\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x000004F0, 11, "\x48\x61\x72\x72\x6F\x6E");
		italyFirmwareHuffmanTree->AddLeaf(0x00001A24, 13, "\x49\x54\x56");
		italyFirmwareHuffmanTree->AddLeaf(0x000000CE, 10, "\x49\x6C");
		italyFirmwareHuffmanTree->AddLeaf(0x0000025B, 10, "\x49\x6E");
		italyFirmwareHuffmanTree->AddLeaf(0x000007A1, 11, "\x49\x6E\x66\x6F\x72\x6D\x61\x7A\x69\x6F\x6E\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x00000210, 10, "\x49\x6E\x74\x72\x61\x74\x74\x65\x6E\x69\x6D\x65\x6E\x74\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x00000711, 11, "\x49\x74\x61\x6C\x69\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x000018E9, 13, "\x4A\x61\x76\x69\x65\x72");
		italyFirmwareHuffmanTree->AddLeaf(0x000004AB, 11, "\x4A\x65\x61\x6E");
		italyFirmwareHuffmanTree->AddLeaf(0x00001EE5, 13, "\x4A\x6F\x68\x6E");
		italyFirmwareHuffmanTree->AddLeaf(0x00000185, 11, "\x4B\x61\x73\x73\x6F\x76\x69\x74\x7A");
		italyFirmwareHuffmanTree->AddLeaf(0x0000005D, 11, "\x4C\x27\x61\x70\x70\x61\x73\x73\x69\x6F\x6E\x61\x6E\x74\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x00000256, 10, "\x4C\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x00000627, 11, "\x4C\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x0000034E, 10, "\x4D\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x00000872, 12, "\x4D\x61\x67\x61\x7A\x69\x6E\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x000018EA, 13, "\x4D\x61\x6D\x6D\x75\x63\x63\x61\x72\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x00000592, 11, "\x4D\x61\x6E\x68\x61\x74\x74\x61\x6E");
		italyFirmwareHuffmanTree->AddLeaf(0x00001C5E, 13, "\x4D\x61\x72\x63\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x000004F6, 11, "\x4D\x61\x72\x79");
		italyFirmwareHuffmanTree->AddLeaf(0x00000186, 11, "\x4D\x61\x74\x68\x69\x65\x75");
		italyFirmwareHuffmanTree->AddLeaf(0x00001A7B, 13, "\x4D\x69\x63\x68\x61\x65\x6C");
		italyFirmwareHuffmanTree->AddLeaf(0x00000C8B, 12, "\x4D\x6F\x6D\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x0000005E, 11, "\x4E\x61\x64\x69\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x00000D3E, 12, "\x4E\x65\x77\x73");
		italyFirmwareHuffmanTree->AddLeaf(0x00000B2E, 12, "\x4E\x6F\x74\x69\x7A\x69\x61\x72\x69\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x00000244, 10, "\x4F\x72\x61\x72\x69\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x00000596, 11, "\x50\x61\x74\x72\x69\x63\x6B");
		italyFirmwareHuffmanTree->AddLeaf(0x000000B3, 12, "\x50\x61\x75\x6C");
		italyFirmwareHuffmanTree->AddLeaf(0x00000626, 11, "\x50\x65\x72");
		italyFirmwareHuffmanTree->AddLeaf(0x00001955, 13, "\x50\x65\x74\x65\x72");
		italyFirmwareHuffmanTree->AddLeaf(0x00001C69, 13, "\x50\x72\x6F\x67\x72\x61\x6D\x6D\x61\x7A\x69\x6F\x6E\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x000004F7, 11, "\x50\x73\x79\x63\x68\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x00000190, 9, "\x52\x65\x67\x69\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x00000187, 11, "\x52\x65\x6E\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x000018F5, 13, "\x52\x6F\x73\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x00001EE4, 13, "\x52\x75\x62\x72\x69\x63\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x00001911, 13, "\x53\x61\x6E\x64\x72\x65\x6C\x6C\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x000018EB, 13, "\x53\x65\x69\x67\x6E\x65\x72");
		italyFirmwareHuffmanTree->AddLeaf(0x00000306, 12, "\x53\x65\x72\x76\x69\x7A\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x00000908, 12, "\x53\x6E\x6F\x77");
		italyFirmwareHuffmanTree->AddLeaf(0x000018F6, 13, "\x53\x6F\x72\x76\x69\x6E\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x00001ADA, 13, "\x53\x74\x65\x66\x61\x6E\x69\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x00000F44, 12, "\x53\x74\x72\x65\x61\x6D");
		italyFirmwareHuffmanTree->AddLeaf(0x000005AF, 11, "\x53\x74\x72\x65\x65\x74");
		italyFirmwareHuffmanTree->AddLeaf(0x000018EC, 13, "\x53\x74\x72\x65\x67\x68\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x000018BA, 13, "\x54\x52\x41\x4D\x41\x3A");
		italyFirmwareHuffmanTree->AddLeaf(0x00000C72, 12, "\x54\x65\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x00000345, 10, "\x55\x53\x41");
		italyFirmwareHuffmanTree->AddLeaf(0x000007BD, 11, "\x55\x6E");
		italyFirmwareHuffmanTree->AddLeaf(0x00000B22, 12, "\x55\x6E\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x000018ED, 13, "\x56\x65\x72\x6F\x6E\x65\x73\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x00000189, 11, "\x56\x69\x6E\x63\x65\x6E\x74");
		italyFirmwareHuffmanTree->AddLeaf(0x00000621, 11, "\x57\x61\x6C\x6C");
		italyFirmwareHuffmanTree->AddLeaf(0x00000316, 12, "\x57\x6F\x72\x6C\x64");
		italyFirmwareHuffmanTree->AddLeaf(0x00000433, 11, "\x58\x58\x58");
		italyFirmwareHuffmanTree->AddLeaf(0x000003D6, 10, "\x61\x64");
		italyFirmwareHuffmanTree->AddLeaf(0x00000184, 11, "\x61\x66\x66\x69\x64\x61\x72\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x0000025E, 10, "\x61\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x0000001E, 7, "\x61\x6C");
		italyFirmwareHuffmanTree->AddLeaf(0x000006B7, 11, "\x61\x6C\x6C\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x00000F75, 12, "\x61\x6C\x6C\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x00000E2E, 12, "\x61\x6D\x69\x63\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x000004F1, 11, "\x61\x6D\x6D\x61\x7A\x7A\x61\x72\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x000003D5, 10, "\x61\x6E\x64");
		italyFirmwareHuffmanTree->AddLeaf(0x00001919, 13, "\x61\x6E\x66\x69\x74\x65\x61\x74\x72\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x000004AA, 11, "\x61\x6E\x6E\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x0000019A, 9, "\x61\x73");
		italyFirmwareHuffmanTree->AddLeaf(0x00001AD9, 13, "\x61\x74\x74\x72\x61\x76\x65\x72\x73\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x000000B4, 12, "\x61\x76\x65\x72");
		italyFirmwareHuffmanTree->AddLeaf(0x00001A23, 13, "\x62\x61\x6D\x62\x69\x6E\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x00001C58, 13, "\x62\x61\x6E\x64\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x000005A8, 11, "\x62\x65\x6C\x6C\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x00001912, 13, "\x62\x72\x61\x6E\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x000004F2, 11, "\x62\x72\x6F\x6B\x65\x72");
		italyFirmwareHuffmanTree->AddLeaf(0x000000B2, 12, "\x62\x75\x73\x69\x6E\x65\x73\x73");
		italyFirmwareHuffmanTree->AddLeaf(0x00000B23, 12, "\x63\x61\x73\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x0000043A, 11, "\x63\x61\x73\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x0000005F, 11, "\x63\x61\x73\x73\x65\x6C");
		italyFirmwareHuffmanTree->AddLeaf(0x00000714, 11, "\x63\x65\x72\x63\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x00000092, 8, "\x63\x68\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x0000018F, 11, "\x63\x68\x69\x6C\x6F\x6D\x65\x74\x72\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x00000C45, 12, "\x63\x69\x74\x74\x61\x27");
		italyFirmwareHuffmanTree->AddLeaf(0x00000C5C, 12, "\x63\x6F\x6D\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x000019B1, 13, "\x63\x6F\x6D\x70\x61\x67\x6E\x69\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x000000D2, 8, "\x63\x6F\x6E");
		italyFirmwareHuffmanTree->AddLeaf(0x00001A3F, 13, "\x63\x6F\x6E\x71\x75\x69\x73\x74\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x00000C44, 12, "\x63\x6F\x6E\x74\x72\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x00001C7E, 13, "\x63\x72\x65\x64\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x0000033E, 12, "\x63\x75\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x00000871, 12, "\x63\x75\x6C\x74\x75\x72\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x00000317, 12, "\x63\x75\x72\x69\x6F\x73\x69\x74\x61\x27");
		italyFirmwareHuffmanTree->AddLeaf(0x00000032, 8, "\x64\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x00001A25, 13, "\x64\x61\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x000007B0, 11, "\x64\x61\x6C");
		italyFirmwareHuffmanTree->AddLeaf(0x00000969, 12, "\x64\x61\x6C\x6C\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F51, 28, "\x64\x65\x63\x69\x64\x47\x32\x36\x36\x20\x20\x3A\x20\x20\x64\x65\x64\x69\x63\x61\x74\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x00000C40, 12, "\x64\x65\x67\x6C\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x000000C6, 10, "\x64\x65\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x000001E2, 9, "\x64\x65\x6C");
		italyFirmwareHuffmanTree->AddLeaf(0x000002CA, 10, "\x64\x65\x6C\x6C\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x00000638, 11, "\x64\x65\x6C\x6C\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x00001951, 13, "\x64\x65\x73\x74\x72\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x00001E8B, 13, "\x64\x65\x76\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x00000020, 6, "\x64\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x0000191A, 13, "\x64\x69\x61\x62\x6F\x6C\x69\x63\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x000019B0, 13, "\x64\x69\x63\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x0000090B, 12, "\x64\x69\x72\x65\x74\x74\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x00000180, 11, "\x64\x69\x73\x74\x61\x6E\x7A\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x00000593, 11, "\x64\x69\x76\x69\x64\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x000018EE, 13, "\x64\x69\x76\x69\x73\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x00001A79, 13, "\x64\x6F\x6C\x63\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x00001EA2, 13, "\x64\x6F\x6D\x61\x6E\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x0000031F, 10, "\x64\x75\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x0000018D, 9, "\x65\x27");
		italyFirmwareHuffmanTree->AddLeaf(0x0000016F, 9, "\x65\x64");
		italyFirmwareHuffmanTree->AddLeaf(0x00001C40, 13, "\x65\x73\x73\x65\x72\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x00001C7D, 13, "\x65\x73\x73\x65\x72\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x00B58F52, 28, "\x65\x76\x65\x6E\x74\x69\x47");
		italyFirmwareHuffmanTree->AddLeaf(0x0000033F, 12, "\x66\x61\x74\x74\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x000005A9, 11, "\x66\x65\x73\x74\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x00000423, 11, "\x66\x69\x6C\x6D");
		italyFirmwareHuffmanTree->AddLeaf(0x00001C43, 13, "\x67\x65\x6D\x65\x6C\x6C\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x000007A0, 11, "\x67\x69\x6F\x72\x6E\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x00000706, 11, "\x67\x69\x6F\x76\x61\x6E\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x00001EF1, 13, "\x67\x69\x6F\x76\x61\x6E\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x0000012E, 9, "\x67\x6C\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x00001C5F, 13, "\x67\x72\x61\x6E\x64\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x00001E8A, 13, "\x67\x72\x61\x6E\x64\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x0000191B, 13, "\x67\x72\x69\x67\x69\x27");
		italyFirmwareHuffmanTree->AddLeaf(0x00000315, 10, "\x68\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x000000CB, 8, "\x69\x6C");
		italyFirmwareHuffmanTree->AddLeaf(0x0000191C, 13, "\x69\x6D\x62\x61\x74\x74\x65\x72\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x0000006A, 7, "\x69\x6E");
		italyFirmwareHuffmanTree->AddLeaf(0x00001950, 13, "\x69\x6E\x63\x69\x6E\x74\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x00000C49, 12, "\x69\x6E\x66\x6F\x72\x6D\x61\x7A\x69\x6F\x6E\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x00000254, 10, "\x69\x6E\x69\x7A\x69\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x00000877, 12, "\x69\x6E\x74\x65\x72\x6E\x61\x7A\x69\x6F\x6E\x61\x6C\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x00000909, 12, "\x69\x6E\x74\x65\x72\x76\x69\x73\x74\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x000000CC, 8, "\x6C\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x00000686, 11, "\x6C\x61\x76\x6F\x72\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x000000D7, 8, "\x6C\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x000018EF, 13, "\x6C\x65\x76\x61\x74\x72\x69\x63\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x000018F4, 13, "\x6C\x69\x62\x72\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x000001E3, 9, "\x6C\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x00000628, 11, "\x6C\x6F\x72\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x0000009F, 8, "\x6D\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x00000876, 12, "\x6D\x61\x67\x67\x69\x6F\x72\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x00000590, 11, "\x6D\x61\x6C\x63\x61\x70\x69\x74\x61\x74\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x000018F0, 13, "\x6D\x61\x73\x63\x68\x69\x65\x74\x74\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x00000715, 11, "\x6D\x6F\x6E\x64\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x00000E0F, 12, "\x6D\x75\x73\x69\x63\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x00000F73, 12, "\x6E\x61\x74\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x000007B8, 11, "\x6E\x65\x6C");
		italyFirmwareHuffmanTree->AddLeaf(0x00001EA0, 13, "\x6E\x65\x6C\x6C\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x00000424, 11, "\x6E\x65\x6C\x6C\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x000004B5, 11, "\x6E\x65\x77\x73");
		italyFirmwareHuffmanTree->AddLeaf(0x0000019E, 11, "\x6E\x6F\x6E");
		italyFirmwareHuffmanTree->AddLeaf(0x0000191D, 13, "\x6E\x6F\x72\x64");
		italyFirmwareHuffmanTree->AddLeaf(0x00000307, 12, "\x6E\x75\x6F\x76\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x000002D5, 10, "\x6F\x66");
		italyFirmwareHuffmanTree->AddLeaf(0x00000873, 12, "\x6F\x67\x6E\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x00000342, 10, "\x6F\x72\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x00001ADB, 13, "\x70\x61\x72\x74\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x0000003F, 8, "\x70\x65\x72");
		italyFirmwareHuffmanTree->AddLeaf(0x00001A7A, 13, "\x70\x69\x63\x63\x6F\x6C\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x0000027A, 10, "\x70\x69\x75\x27");
		italyFirmwareHuffmanTree->AddLeaf(0x0000018E, 11, "\x70\x6F\x6C\x69\x7A\x69\x6F\x74\x74\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x00000181, 11, "\x70\x6F\x72\x70\x6F\x72\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x00001C7C, 13, "\x70\x72\x69\x6D\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x000018F7, 13, "\x70\x72\x6F\x64\x75\x74\x74\x6F\x72\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x00001C68, 13, "\x70\x72\x6F\x67\x72\x61\x6D\x6D\x61\x7A\x69\x6F\x6E\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x00001AD8, 13, "\x70\x72\x6F\x70\x72\x69\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x00000245, 10, "\x70\x72\x6F\x73\x73\x69\x6D\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x00001EC6, 13, "\x71\x75\x61\x74\x74\x72\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x000018F1, 13, "\x72\x65\x67\x69\x6D\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x000005AE, 11, "\x72\x69\x63\x63\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x00001954, 13, "\x72\x69\x63\x6F\x72\x64\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x00001C41, 13, "\x72\x6F\x76\x69\x6E\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x00001C1C, 13, "\x73\x61\x6C\x76\x61\x72\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x000019B7, 13, "\x73\x63\x72\x69\x74\x74\x6F\x72\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x00001A1E, 13, "\x73\x63\x72\x69\x76\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x00001EC7, 13, "\x73\x65\x72\x69\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x00000870, 12, "\x73\x65\x72\x76\x69\x7A\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x000018A7, 13, "\x73\x65\x74\x74\x69\x6D\x61\x6E\x61\x6C\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x000000F0, 8, "\x73\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x00000188, 11, "\x73\x69\x6E\x67\x6F\x6C\x61\x72\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x00000C8A, 12, "\x73\x6F\x6C\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x00000623, 11, "\x73\x6F\x6E\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x00000422, 11, "\x73\x74\x65\x73\x73\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x0000071B, 11, "\x73\x74\x6F\x72\x69\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x00001910, 13, "\x73\x74\x72\x65\x67\x68\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x00000382, 10, "\x73\x75");
		italyFirmwareHuffmanTree->AddLeaf(0x00000425, 11, "\x73\x75\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x00000968, 12, "\x73\x75\x63\x63\x65\x73\x73\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x00000CD9, 12, "\x73\x75\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x0000090A, 12, "\x73\x75\x6C\x6C\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x00000427, 11, "\x73\x75\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x00001A1C, 13, "\x73\x75\x6F\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x00000CA9, 12, "\x74\x61\x6C\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x00000F62, 12, "\x74\x65\x6D\x70\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x000007A9, 11, "\x74\x68\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x0000191E, 13, "\x74\x69\x6D\x69\x64\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x000004F3, 11, "\x74\x6F\x72\x74\x75\x72\x61\x72\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x000001A6, 9, "\x74\x72\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x000018F2, 13, "\x74\x72\x61\x73\x66\x6F\x72\x6D\x61\x72\x6C\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x00000182, 11, "\x74\x72\x65\x63\x65\x6E\x74\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x00001A1F, 13, "\x74\x72\x6F\x76\x61\x74\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x00000CDA, 12, "\x74\x75\x74\x74\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x00001913, 13, "\x75\x6C\x74\x69\x6D\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x000000C9, 8, "\x75\x6E");
		italyFirmwareHuffmanTree->AddLeaf(0x0000016E, 9, "\x75\x6E\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x00000E2D, 12, "\x75\x6E\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x00001EF0, 13, "\x75\x6F\x6D\x69\x6E\x69");
		italyFirmwareHuffmanTree->AddLeaf(0x0000018A, 11, "\x76\x65\x64\x6F\x6E\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x0000084D, 12, "\x76\x65\x6E\x67\x6F\x6E\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x00000860, 12, "\x76\x65\x72\x73\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x00000C52, 12, "\x76\x69\x61\x67\x67\x69\x6F");
		italyFirmwareHuffmanTree->AddLeaf(0x00001EA1, 13, "\x76\x69\x65\x6E\x65");
		italyFirmwareHuffmanTree->AddLeaf(0x0000062F, 11, "\x76\x69\x74\x61");
		italyFirmwareHuffmanTree->AddLeaf(0x00001C7F, 13, "\x76\x75\x6F\x6C\x65");

	}
	catch(...)
	{
		LogDebug("CSkyManager::InitializeEpgFirmwareHuffmanTrees() - Exception whilst initializing firmware huffman trees");
		return false;
	}

	LogDebug("CSkyManager::InitializeEpgFirmwareHuffmanTrees() - Firmware huffman trees populated successfully");

	return true;
}

//	Resets the manager
void CSkyManager::Reset()
{
	Mediaportal::CEnterCriticalSection enter(criticalSection);

	//	TODO - free previous grab

	//	We need to free the memory used by the previous grab
	map<unsigned short, SkyChannel*>::iterator channelsIt;

	//	Loop through all channels
	for(channelsIt = channels.begin(); channelsIt != channels.end(); channelsIt++)
	{
		SkyChannel* currentChannel = channelsIt->second;

		//	If this channel has events
		if(currentChannel->events != NULL)
		{
			map<unsigned short, SkyEpgEvent*>::iterator epgEventsIt;

			//	Loop through all events to clear them
			for(epgEventsIt = currentChannel->events->begin(); epgEventsIt != currentChannel->events->end(); epgEventsIt++)
			{
				SkyEpgEvent* skyEpgEvent = epgEventsIt->second;

				//	Clear titles and summaries if used
				if(skyEpgEvent->title != NULL)
					free(skyEpgEvent->title);

				if(skyEpgEvent->summary != NULL)
					free(skyEpgEvent->summary);

				delete skyEpgEvent;
			}

			//	Clear the events map
			currentChannel->events->clear();

			delete currentChannel->events;
		}

		//	Delete the channel
		delete currentChannel;
	}

	channels.clear();

	//	Free all bouquets
	map<unsigned short, SkyBouquet*>::iterator bouquetsIt;

	for(bouquetsIt = bouquets.begin(); bouquetsIt != bouquets.end(); bouquetsIt++)
	{
		delete bouquetsIt->second;
	}

	bouquets.clear();

	//	Reset the number of titles/summaries decoded
	titlesDecoded = 0;
	summariesDecoded = 0;

	//	Clear carousel lookups
	titleDataCarouselStartLookup.clear();
	completedTitleDataCarousels.clear();
	summaryDataCarouselStartLookup.clear();
	completedSummaryDataCarousels.clear();

	numberErrorsLogged = 0;
	numberBouquetsPopulated = 0;
	numberFailedHuffmanDecodes = 0;

	isEpgGrabbingActive = false;
	epgGrabbingAbortedThroughTooManyErrors = false;
	
	LogDebug("CSkyManager::Reset() - Reset successful");
}

//	Activates the EPG grabber for the specified country code
void CSkyManager::ActivateEpgGrabber(unsigned short activateCountryId)
{
	Mediaportal::CEnterCriticalSection enter(criticalSection);

	currentCountryId = activateCountryId;

	if(currentCountryId == SKY_COUNTRY_UK)
	{
		LogDebug("CSkyManager::ActivateEpgGrabber() - Activated Sky EPG grabber for country: UK");
		isEpgGrabbingActive = true;
	}

	else if(currentCountryId == SKY_COUNTRY_IT)
	{
		LogDebug("CSkyManager::ActivateEpgGrabber() - Activated Sky EPG grabber for country: Italy");
		isEpgGrabbingActive = true;
	}

	else
	{
		LogDebug("CSkyManager::ActivateEpgGrabber() - Error, invalid country id specified");
		isEpgGrabbingActive = false;
	}
	
	numberBouquetsPopulated = 0;
}

//	De-activates the epg grabber
void CSkyManager::DeActivateEpgGrabber()
{
	Mediaportal::CEnterCriticalSection enter(criticalSection);
	
	LogDebug("CSkyManager::DeActivateEpgGrabber()");

	isEpgGrabbingActive = false;
}

//	Is the epg grabber active
bool CSkyManager::IsEpgGrabbingActive()
{
	return isEpgGrabbingActive;
}

//	Has the epg grabber finished
bool CSkyManager::IsEpgGrabbingFinished()
{
	return AreAllBouquetsPopulated() && AreAllTitlesPopulated() && AreAllSummariesPopulated();
}
//	Has the EPG grabbing been aborted through too many errors?
bool CSkyManager::HasEpgGrabbingAborted()
{
	return epgGrabbingAbortedThroughTooManyErrors;
}
//	Fired when a new ts packet is received
void CSkyManager::OnTsPacket(CTsHeader& header, byte* tsPacket)
{
	unsigned short pid = header.Pid;

	//	If we are grabbing epg, pass the packet to the relavent parser
	if(isEpgGrabbingActive)
	{
		if(DoesPidCarryChannelNetworkData(pid))
			skyChannelParser->OnTsPacket(header, tsPacket);

		else if(DoesPidCarryEpgData(pid))
			skyEpgParser->OnTsPacket(header, tsPacket);
	}
}

//	Does this pid carry channel network data?
bool CSkyManager::DoesPidCarryChannelNetworkData(int pid)
{
	return pid == PID_SKYUKIT_EPG_CHANNEL_INFORMATION;
}

//	Does this tid carry epg summary data
bool CSkyManager::DoesTidCarryChannelNetworkData(int tid)
{
	return tid == PID_SKYUKIT_EPG_CHANNEL_TABLEID;
}

//	Does this pid carry epg data?
bool CSkyManager::DoesPidCarryEpgData(int pid)
{
	return DoesPidCarryEpgTitleData(pid) || DoesPidCarryEpgSummaryData(pid);
}

//	Does this tid carry epg title data
bool CSkyManager::DoesTidCarryEpgTitleData(int tid)
{
	return tid >= PID_SKYUKIT_EPG_TITLE_TABLEID_F && tid <= PID_SKYUKIT_EPG_TITLE_TABLEID_T;
}

//	Does this tid carry epg summary data
bool CSkyManager::DoesTidCarryEpgSummaryData(int tid)
{
	return tid >= PID_SKYUKIT_EPG_SUMMARY_TABLEID_F && tid <= PID_SKYUKIT_EPG_SUMMARY_TABLEID_T;
}

//	Does the pid carry epg title data?
bool CSkyManager::DoesPidCarryEpgTitleData(int pid)
{
	return (pid >= PID_SKYUKIT_EPG_TITLES_F && pid <= PID_SKYUKIT_EPG_TITLES_T);
}

//	Does the pid carry epg summary data?
bool CSkyManager::DoesPidCarryEpgSummaryData(int pid)
{
	return (pid >= PID_SKYUKIT_EPG_SUMMARIES_F && pid <= PID_SKYUKIT_EPG_SUMMARIES_T);
}

//	Gets the bouquet with the specified id
SkyBouquet* CSkyManager::GetBouquet(unsigned short bouquetId)
{
	Mediaportal::CEnterCriticalSection enter(criticalSection);

	SkyBouquet* returnBouquet = NULL;

	//	Try to find a lookup collection for this bouquet id
	map<unsigned short, SkyBouquet*>::iterator bouquetIterator = bouquets.find(bouquetId);
	
	//	Create a new lookup for this channel if one does not exist yet, else get existing from iterator
	if(bouquetIterator == bouquets.end())
	{
		returnBouquet = new SkyBouquet(bouquetId);
		bouquets.insert(pair<unsigned short, SkyBouquet*>(bouquetId, returnBouquet));
	}
	else
		returnBouquet = bouquetIterator->second;

	return returnBouquet;
}

//	Are all bouquets now populated?
bool CSkyManager::AreAllBouquetsPopulated()
{
	Mediaportal::CEnterCriticalSection enter(criticalSection);

	return (bouquets.size() > 0) && (bouquets.size() == numberBouquetsPopulated);
}

//	Are all epg titles populated?
bool CSkyManager::AreAllTitlesPopulated()
{
	return completedTitleDataCarousels.size() == (PID_SKYUKIT_EPG_TITLES_T - PID_SKYUKIT_EPG_TITLES_F + 1);
}

//	Are all epg summaries populated?
bool CSkyManager::AreAllSummariesPopulated()
{
	return completedSummaryDataCarousels.size() == (PID_SKYUKIT_EPG_SUMMARIES_T - PID_SKYUKIT_EPG_SUMMARIES_F + 1);
}

//	Notifys the manager that a bouquet is now fully populated
void CSkyManager::NotifyBouquetPopulated()
{
	Mediaportal::CEnterCriticalSection enter(criticalSection);

	numberBouquetsPopulated++;
	
	int bouquetsFound = bouquets.size();

	//	If fully populated
	if(bouquetsFound == numberBouquetsPopulated)
		LogDebug("CSkyManager - Channel & bouquet scan complete.  Found a total of %d channels in %d bouquets", GetChannelCount(), bouquetsFound);
}

//	Gets the number of channels found
unsigned int CSkyManager::GetChannelCount()
{
	Mediaportal::CEnterCriticalSection enter(criticalSection);

	return channels.size();
}

//	Gets the existing, or creates a new channel with the specified id
SkyChannel* CSkyManager::GetChannel(unsigned short channelId)
{
	Mediaportal::CEnterCriticalSection enter(criticalSection);
	
	SkyChannel* returnChannel = NULL;

	//	Try to find a lookup collection for this channel id
	map<unsigned short, SkyChannel*>::iterator channelIterator = channels.find(channelId);
	
	//	Create a new lookup for this channel if one does not exist yet, else get existing from iterator
	if(channelIterator == channels.end())
	{
		returnChannel = new SkyChannel(channelId);
		channels.insert(pair<unsigned short, SkyChannel*>(channelId, returnChannel));
	}
	else
		returnChannel = channelIterator->second;

	return returnChannel;
}

//	Gets the epg event count
unsigned int CSkyManager::GetEpgEventCount()
{
	Mediaportal::CEnterCriticalSection enter(criticalSection);

	map<unsigned short, SkyChannel*>::iterator channelIterator;

	int eventCount = 0;

	//	Loop through all channels
	for(channelIterator = channels.begin(); channelIterator != channels.end(); channelIterator++)
	{
		if(channelIterator->second->events != NULL)
			eventCount += channelIterator->second->events->size();
	}

	return eventCount;
}

//	Gets the existing, or creates a new epg event with the specified ids
SkyEpgEvent* CSkyManager::GetEpgEvent(unsigned short channelId, unsigned short eventId)
{
	Mediaportal::CEnterCriticalSection enter(criticalSection);

	//	Get the channel that this epg event applies to
	SkyChannel* channel = GetChannel(channelId);

	//	Initialize the events collection if null
	if(channel->events == NULL)
		channel->events = new map<unsigned short, SkyEpgEvent*>();

	SkyEpgEvent* returnEvent = NULL;

	//	Try to find a lookup collection for this channel id
	map<unsigned short, SkyEpgEvent*>::iterator eventIterator = channel->events->find(eventId);
	
	//	Create a new lookup for this channel if one does not exist yet, else get existing from iterator
	if(eventIterator == channel->events->end())
	{
		returnEvent = new SkyEpgEvent(eventId);
		channel->events->insert(pair<unsigned short, SkyEpgEvent*>(eventId, returnEvent));
	}
	else
		returnEvent = eventIterator->second;

	return returnEvent;

}

//	Gets the current country id
unsigned int CSkyManager::GetCurrentCountryId()
{
	return currentCountryId;
}

//	Decodes a huffman buffer
byte* CSkyManager::DecodeHuffmanData(byte* buffer, int length)
{
	Mediaportal::CEnterCriticalSection enter(criticalSection);
	
	//	First 2 bits are the huffman table id
	byte huffmanTableId = (buffer[0] & 0xC0) >> 6;

	//	Work out the appropriate huffman table
	//	0 = firmware table
	//	1 = dynamic broadcast table 1 (from Pid 0x85)
	//	2 = dynamic broadcast table 2 (from Pid 0x85)
	CHuffmanTree* decodeHuffmanTree = NULL;

	if(huffmanTableId == 0)
	{
		//	Using firmware trees
		map<unsigned short, CHuffmanTree*>::iterator huffmanTreeIterator = countryFirmwareHuffmanTrees.find(currentCountryId);

		if(huffmanTreeIterator == countryFirmwareHuffmanTrees.end())
		{
			LogError("CSkyManager::DecodeHuffmanData() - Failed to find firmware huffman tree");
			return NULL;			
		}

		decodeHuffmanTree = huffmanTreeIterator->second;
	}
	else
	{
		numberFailedHuffmanDecodes++;
		return NULL;
	}

	if(decodeHuffmanTree == NULL)
	{
		LogError("CSkyManager::DecodeHuffmanData() - Failed to locate huffman tree to decode data");
		return NULL;
	}

	int decodedLength = decodeHuffmanTree->Decode(buffer, length, 2, &huffmanDecodeBuffer[0], HUFFMAN_DECODE_BUFFER_SIZE);

	//	Allocate a buffer large enough for the decoded buffer
	byte* copiedDecode = (byte*) malloc((decodedLength + 1) * sizeof(byte));

	memcpy(copiedDecode, huffmanDecodeBuffer, decodedLength);
	copiedDecode[decodedLength] = '\0';

	return copiedDecode;
}

//	Notifys the manager that a title event has been received
void CSkyManager::OnTitleReceived(unsigned short pid, unsigned int titleChannelEventUnionId)
{
	Mediaportal::CEnterCriticalSection enter(criticalSection);

	//	Have we added the start of this pid's carousel yet?
	map<unsigned short, unsigned int>::iterator it = titleDataCarouselStartLookup.find(pid);

	//	This pid is just starting
	if(it == titleDataCarouselStartLookup.end())
	{
		titleDataCarouselStartLookup.insert(pair<unsigned short, unsigned int>(pid, titleChannelEventUnionId));
	}

	//	Else test if we are now back round to the start of the title carousel
	else
	{
		if(it->second == titleChannelEventUnionId)
		{
			completedTitleDataCarousels.push_back(pid);

			//	If all title pids have now completed, log a message
			if(AreAllTitlesPopulated())
				LogDebug("CSkyManager - Title decoding complete, found %u", titlesDecoded);
					
		}
	}

}

//	Gets if the title data carousel on the specified pid is complete
bool CSkyManager::IsTitleDataCarouselOnPidComplete(unsigned short pid)
{
	Mediaportal::CEnterCriticalSection enter(criticalSection);

	vector<unsigned short>::iterator it = completedTitleDataCarousels.begin();

	//	Loop through to see if this pid is complete
	for(it = completedTitleDataCarousels.begin(); it != completedTitleDataCarousels.end(); it++)
	{
		if(*it == pid)
			return true;
	}

	return false;
}



//	Notifys the manager that a summary event has been received
void CSkyManager::OnSummaryReceived(unsigned short pid, unsigned int summaryChannelEventUnionId)
{
	Mediaportal::CEnterCriticalSection enter(criticalSection);

	//	Have we added the start of this pid's carousel yet?
	map<unsigned short, unsigned int>::iterator it = summaryDataCarouselStartLookup.find(pid);

	//	This pid is just starting
	if(it == summaryDataCarouselStartLookup.end())
	{
		summaryDataCarouselStartLookup.insert(pair<unsigned short, unsigned int>(pid, summaryChannelEventUnionId));
	}

	//	Else test if we are now back round to the start of the title carousel
	else
	{
		if(it->second == summaryChannelEventUnionId)
		{
			completedSummaryDataCarousels.push_back(pid);

			//	If all title pids have now completed, log a message
			if(AreAllSummariesPopulated())
				LogDebug("CSkyManager - Summary decoding complete, found %u", summariesDecoded);
		}
	}

}

//	Gets if the title data carousel on the specified pid is complete
bool CSkyManager::IsSummaryDataCarouselOnPidComplete(unsigned short pid)
{
	Mediaportal::CEnterCriticalSection enter(criticalSection);

	vector<unsigned short>::iterator it = completedSummaryDataCarousels.begin();

	//	Loop through to see if this pid is complete
	for(it = completedSummaryDataCarousels.begin(); it != completedSummaryDataCarousels.end(); it++)
	{
		if(*it == pid)
			return true;
	}

	return false;
}

//	Fired when a title is decoded
void CSkyManager::OnTitleDecoded()
{
	titlesDecoded++;
}

//	Fired when a summary is decoded
void CSkyManager::OnSummaryDecoded()
{
	summariesDecoded++;
}


//	Fired when the epg callback is triggered
void CSkyManager::OnEpgCallback()
{
	//	Report any failed items
	if(numberFailedHuffmanDecodes > 0)
		LogDebug("CSkyManager::OnEpgCallback() - %d huffman encoded items could not be processed as they require an unknown table", numberFailedHuffmanDecodes);
}
//	Resets the epg retrieval iterators
void CSkyManager::ResetEpgRetrieval()
{
	Mediaportal::CEnterCriticalSection enter(criticalSection);

	retrievalReset = true;
}

//	Gets the next sky epg channel information
void CSkyManager::GetNextSkyEpgChannel(unsigned char *atEnd, unsigned short *channelId, unsigned short *networkId, unsigned short *transportId, unsigned short *serviceId)
{
	Mediaportal::CEnterCriticalSection enter(criticalSection);

	//	If the retrieval was reset, we want to read the first channel
	if(retrievalReset)
	{
		epgRetrievalChannelsIt = channels.begin();
		retrievalReset = false;
	}
	else
		epgRetrievalChannelsIt++;

	//	Skip this channel if it has no events defined
	while(epgRetrievalChannelsIt != channels.end() && (epgRetrievalChannelsIt->second->events == NULL || (epgRetrievalChannelsIt->second->events != NULL && epgRetrievalChannelsIt->second->events->empty())))
		epgRetrievalChannelsIt++;

	//	If we are now at the end, send back the is last channel flag
	if(epgRetrievalChannelsIt == channels.end())
	{
		*atEnd = 1;
		return;
	}


	SkyChannel* channel = epgRetrievalChannelsIt->second;

	//	If the channel is null, log error
	if(channel == NULL)
	{
		LogDebug("CSkyManager::GetNextSkyEpgChannel() - Channel is null unexpectedly");
		*atEnd = 2;
		return;
	}

	*channelId = channel->channelId;
	*networkId = channel->networkId;
	*transportId = channel->transportId;
	*serviceId = channel->serviceId;

	//	Start the events iterator at the first event for this channel
	epgRetrievalChannelEventsIt = channel->events->begin();
}

//	Gets the next sky epg channel event information
void CSkyManager::GetNextSkyEpgChannelEvent(unsigned char* atEnd, unsigned short* eventId, unsigned short* mjdStart, unsigned int* startTime, unsigned int* duration, unsigned char** title, unsigned char** summary, unsigned char** theme, unsigned short* seriesId, byte* seriesTermination)
{
	Mediaportal::CEnterCriticalSection enter(criticalSection);

	//	If there are no events for this channel, or we are already at the end
	if(epgRetrievalChannelsIt->second->events == NULL || epgRetrievalChannelsIt->second->events->empty() || epgRetrievalChannelEventsIt == epgRetrievalChannelsIt->second->events->end())
	{
		*atEnd = 1;
		return;
	}

	SkyEpgEvent* epgEvent = epgRetrievalChannelEventsIt->second;

	//	If the epg event is null, log error
	if(epgEvent == NULL)
	{
		LogDebug("CSkyManager::GetNextSkyEpgChannelEvent() - Epg event is null unexpectedly");
		*atEnd = 2;
		return;
	}

	//	Set the return fields
	*eventId = epgEvent->eventId;
	*mjdStart = epgEvent->mjdStart;
	*startTime = epgEvent->startTime;
	*duration = epgEvent->duration;
	*title = epgEvent->title;
	*summary = epgEvent->summary;
	*theme = epgEvent->theme;
	*seriesId = epgEvent->seriesId;

	//	Series termination is only applicable if series id is available
	if(epgEvent->seriesId > NULL)
		*seriesTermination = epgEvent->seriesTermination;
	else
		*seriesTermination = 0;

	//	Move to next epg event for this channel
	epgRetrievalChannelEventsIt++;

}

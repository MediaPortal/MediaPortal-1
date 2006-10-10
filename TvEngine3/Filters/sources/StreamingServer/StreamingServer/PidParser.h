/**
*  PidParser.h
*  Copyright (C) 2003      bisswanger
*  Copyright (C) 2004-2006 bear
*  Copyright (C) 2005      nate
*
*  This file is part of TSFileSource, a directshow push source filter that
*  provides an MPEG transport stream output.
*
*  TSFileSource is free software; you can redistribute it and/or modify
*  it under the terms of the GNU General Public License as published by
*  the Free Software Foundation; either version 2 of the License, or
*  (at your option) any later version.
*
*  TSFileSource is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*  GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License
*  along with TSFileSource; if not, write to the Free Software
*  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*
*  bisswanger can be reached at WinSTB@hotmail.com
*    Homepage: http://www.winstb.de
*
*  bear and nate can be reached on the forums at
*    http://forums.dvbowners.com/
*/

#ifndef PIDPARSER_H
#define PIDPARSER_H

#include "PidInfo.h"
#include "FileReader.h"
#include "SampleBuffer.h"

class PidParser
{
public:
PidParser(CSampleBuffer *pSampleBuffer, FileReader *pFileReader);
	virtual ~PidParser();


	HRESULT ParsePinMode(__int64 fileStartPointer = 0);
	HRESULT ParseFromFile(__int64 fileStartPointer);
	HRESULT RefreshPids();
	HRESULT RefreshDuration(BOOL bStoreInArray, FileReader *pFileReader);
	HRESULT set_ProgramSID();
	HRESULT FindSyncByte(PBYTE pbData, ULONG ulDataLength, ULONG* a, int step);
	HRESULT FindFirstPCR(PBYTE pData, ULONG ulDataLength, PidInfo *pPids, REFERENCE_TIME* pcrtime, ULONG* pulPos);
	HRESULT FindLastPCR(PBYTE pData, ULONG ulDataLength, PidInfo *pPids, REFERENCE_TIME* pcrtime, ULONG* pulPos);
	HRESULT FindNextPCR(PBYTE pData, ULONG ulDataLength, PidInfo *pPids, REFERENCE_TIME* pcrtime, ULONG* pulPos, int step);
	HRESULT FindNextOPCR(PBYTE pData, ULONG ulDataLength, PidInfo *pPids, REFERENCE_TIME* pcrtime, ULONG* pulPos, int step);
	void get_ChannelNumber(BYTE *pointer);
	void get_NetworkName(BYTE *pointer);
	void get_ONetworkName(BYTE *pointer);
	void get_ChannelName(BYTE *pointer);
	void get_ShortNextDescr(BYTE *pointer);
	void get_ExtendedNextDescr(BYTE *pointer);
	void get_ShortDescr(BYTE *pointer);
	void get_ExtendedDescr(BYTE *pointer);
	void get_CurrentTSArray(ULONG *pPidArray);
	void set_ProgramNumber(WORD programNumber);
	void set_SIDPid(BOOL bProgramSID);
	WORD get_ProgramNumber();
	BOOL get_ProgPinMode();
	void set_ProgPinMode(BOOL mode);
	__int64 get_StartOffset(void);
	REFERENCE_TIME get_StartTimeOffset(void);
	BOOL get_AsyncMode();
	void set_AsyncMode(BOOL mode);
	ULONG get_PacketSize();
	void PrintTime(LPCTSTR lstring, __int64 value, __int64 divider);

	int m_NitPid;
	int m_NetworkID;
	int m_ONetworkID;
	int m_TStreamID;
	int m_ProgramSID;
	BOOL m_ProgPinMode;
	BOOL m_AsyncMode;
	ULONG m_PacketSize;
	PidInfo pids;
	PidInfoArray pidArray;	//Currently selected pids
	BOOL m_ATSCFlag;
	unsigned char m_NetworkName[128];
	BOOL m_ParsingLock;

protected:

	HRESULT ParsePAT(PBYTE pData, ULONG ulDataLength, long pos);
	HRESULT ParsePMT(PBYTE pData, ULONG ulDataLength, long pos);
	HRESULT IsValidPMT(PBYTE pData, ULONG ulDataLength);
	HRESULT ACheckVAPids(PBYTE pData, ULONG ulDataLength);
	HRESULT CheckVAStreams(PBYTE pData, ULONG ulDataLength);
	HRESULT CheckNIDInFile(FileReader *pFileReader);
	HRESULT CheckONIDInFile(FileReader *pFileReader);
	HRESULT ParseEISection (ULONG ulDataLength);
	HRESULT ParseShortEvent(int start, ULONG ulDataLength);
	HRESULT ParseExtendedEvent(int start, ULONG ulDataLength);
	HRESULT GetPCRduration(
		PBYTE pData,
		long lDataLength,
		PidInfo *pPids,
		__int64 filelength,
		__int64* pStartFilePos,
		__int64* pEndFilePos,
		FileReader *pFileReader);

	HRESULT CheckForPCR(PBYTE pData, ULONG ulDataLength, PidInfo *pPids, int pos, REFERENCE_TIME* pcrtime);
	HRESULT CheckForOPCR(PBYTE pData, ULONG ulDataLength, PidInfo *pPids, int pos, REFERENCE_TIME* pcrtime);

	void AddPidArray();
	void SetPidArray(int n);
	void AddTsPid(PidInfo *pidInfo, WORD pid);

	BOOL CheckEsDescriptorForAC3(PBYTE pData, ULONG ulDataLength, int pos, int lastpos);
	BOOL CheckEsDescriptorForTeletext(PBYTE pData, ULONG ulDataLength, int pos, int lastpos);
	BOOL CheckEsDescriptorForDTS(PBYTE pData, ULONG ulDataLength, int pos, int lastpos);
	BOOL CheckEsDescriptorForSubtitle(PBYTE pData, ULONG ulDataLength, int pos, int lastpos);
	bool CheckForEPG(PBYTE pData, int pos, bool *extpacket, int *sectlen, int *sidcount, int *event);
	bool CheckForNID(PBYTE pData, int pos, bool *extPacket, int *sectlen);
	bool CheckForONID(PBYTE pData, int pos, bool *extpacket, int *sectlen);

	REFERENCE_TIME GetPCRFromFile(FileReader *pFileReader, int step);
	REFERENCE_TIME GetFileDuration(PidInfo *pPids, FileReader *pFileReader);

	int		m_buflen;
	BYTE	m_pDummy[0x4000];
	BYTE	m_shortdescr[128];
	BYTE	m_extenddescr[600];
	WORD	m_pgmnumb;
	__int64	filepos;
	__int64 m_fileLenOffset;
	__int64	m_fileEndOffset;
	__int64	m_fileStartOffset;
	__int64 m_FileStartPointer;

	FileReader *m_pFileReader;
	CSampleBuffer *m_pSampleBuffer;
	CCritSec m_ParserLock;
};

#endif

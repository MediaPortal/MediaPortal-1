/**
*  ITSParserSource.h
*  Copyright (C) 2003      bisswanger
*  Copyright (C) 2004-2006 bear
*  Copyright (C) 2005      nate
*
*  This file is part of TSParserSource, a directshow push source filter that
*  provides an MPEG transport stream output.
*
*  TSParserSource is free software; you can redistribute it and/or modify
*  it under the terms of the GNU General Public License as published by
*  the Free Software Foundation; either version 2 of the License, or
*  (at your option) any later version.
*
*  TSParserSource is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*  GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License
*  along with TSParserSource; if not, write to the Free Software
*  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*
*  bisswanger can be reached at WinSTB@hotmail.com
*    Homepage: http://www.winstb.de
*
*  bear and nate can be reached on the forums at
*    http://forums.dvbowners.com/
*/
// {7A6B3AC9-4337-4da1-A805-382E569097B1}
DEFINE_GUID(IID_ITSParserSource, 
0x7a6b3ac9, 0x4337, 0x4da1, 0xa8, 0x5, 0x38, 0x2e, 0x56, 0x90, 0x97, 0xb1);
DECLARE_INTERFACE_(ITSParserSource, IUnknown) //compatable to 2.0.1.7 official release
{
	STDMETHOD(GetVideoPid) (THIS_ WORD * vpid) PURE;
	STDMETHOD(GetAudioPid) (THIS_ WORD * apid) PURE;
	STDMETHOD(GetAudio2Pid) (THIS_ WORD * a2pid) PURE;
	STDMETHOD(GetAC3Pid) (THIS_ WORD * ac3pid) PURE;
	STDMETHOD(GetTelexPid) (THIS_ WORD * telexpid) PURE;
	STDMETHOD(GetPMTPid) (THIS_ WORD * pmtpid) PURE;
	STDMETHOD(GetSIDPid) (THIS_ WORD * sidpid) PURE;
	STDMETHOD(GetPCRPid) (THIS_ WORD * pcrpid) PURE;
	STDMETHOD(GetDuration) (THIS_ REFERENCE_TIME * dur) PURE;
	STDMETHOD(GetShortDescr) (THIS_ BYTE * shortdesc) PURE;
	STDMETHOD(GetExtendedDescr) (THIS_ BYTE * extdesc) PURE;

	STDMETHOD(GetPgmNumb) (THIS_ WORD * pPgmNumb) PURE;
	STDMETHOD(GetPgmCount) (THIS_ WORD * pPgmCount) PURE;
	STDMETHOD(SetPgmNumb) (THIS_ WORD pPgmNumb) PURE;
	STDMETHOD(NextPgmNumb) (void) PURE;
	STDMETHOD(GetTsArray) (THIS_ ULONG * pPidArray) PURE;

	STDMETHOD(GetAC3Mode) (THIS_ WORD * pAC3Mode) PURE;
	STDMETHOD(SetAC3Mode) (THIS_ WORD AC3Mode) PURE;

	STDMETHOD(GetMP2Mode) (THIS_ WORD * pMP2Mode) PURE;
	STDMETHOD(SetMP2Mode) (THIS_ WORD MP2Mode) PURE;

	STDMETHOD(GetAutoMode) (THIS_ WORD * pAutoMode) PURE;
	STDMETHOD(SetAutoMode) (THIS_ WORD AutoMode) PURE;

	STDMETHOD(GetDelayMode) (THIS_ WORD * pDelayMode) PURE;
	STDMETHOD(SetDelayMode) (THIS_ WORD DelayMode) PURE;

	STDMETHOD(GetRateControlMode) (THIS_ WORD * pRateControl) PURE;
	STDMETHOD(SetRateControlMode) (THIS_ WORD RateControl) PURE;

	STDMETHOD(GetCreateTSPinOnDemux) (THIS_ WORD * pbCreatePin) PURE;
	STDMETHOD(SetCreateTSPinOnDemux) (THIS_ WORD bCreatePin) PURE;

	STDMETHOD(GetReadOnly) (THIS_ WORD * pFileMode) PURE;

	STDMETHOD(GetBitRate) (THIS_ long *pRate) PURE;
	STDMETHOD(SetBitRate) (THIS_ long Rate) PURE;

//New interfaces added after 2.0.1.7 official release
	STDMETHOD(GetAC3_2Pid) (THIS_ WORD * ac3_2pid) PURE;

	STDMETHOD(GetNIDPid) (THIS_ WORD * nidpid) PURE;
	STDMETHOD(GetChannelNumber) (THIS_ BYTE * pointer) PURE;
	STDMETHOD(GetNetworkName) (THIS_ BYTE * pointer) PURE;

	STDMETHOD(GetONIDPid) (THIS_ WORD * onidpid) PURE;
	STDMETHOD(GetONetworkName) (THIS_ BYTE * pointer) PURE;
	STDMETHOD(GetChannelName) (THIS_ BYTE * pointer) PURE;

	STDMETHOD(GetTSIDPid) (THIS_ WORD * tsidpid) PURE;

	STDMETHOD(GetEPGFromFile) (void) PURE;
	STDMETHOD(GetShortNextDescr) (THIS_ BYTE * shortnextdesc) PURE;
	STDMETHOD(GetExtendedNextDescr) (THIS_ BYTE * extnextdesc) PURE;

	STDMETHOD(PrevPgmNumb) (void) PURE;

	STDMETHOD(GetAudio2Mode) (THIS_ WORD * pAudio2Mode) PURE;
	STDMETHOD(SetAudio2Mode) (THIS_ WORD Audio2Mode) PURE;

	STDMETHOD(GetNPControl) (THIS_ WORD *pNPControl) PURE;
	STDMETHOD(SetNPControl) (THIS_ WORD pNPControl) PURE;

	STDMETHOD(GetNPSlave) (THIS_ WORD *pNPSlave) PURE;
	STDMETHOD(SetNPSlave) (THIS_ WORD pNPSlave) PURE;

	STDMETHOD(SetTunerEvent) (void) PURE;

	STDMETHOD(SetRegSettings) (void) PURE;
	STDMETHOD(GetRegSettings) (void) PURE;
	STDMETHOD(SetRegProgram) (void) PURE;

	STDMETHOD(ShowFilterProperties)(void) PURE;
	STDMETHOD(Refresh)(void) PURE;

	STDMETHOD(GetROTMode) (THIS_ WORD *ROTMode) PURE;
	STDMETHOD(SetROTMode) (THIS_ WORD ROTMode) PURE;
	STDMETHOD(GetClockMode) (THIS_ WORD *ClockMode) PURE;
	STDMETHOD(SetClockMode) (THIS_ WORD ClockMode) PURE;

//New method added after 2.0.1.8
	STDMETHOD(GetVideoPidType) (THIS_ BYTE * pointer) PURE;
	STDMETHOD(ShowEPGInfo)(void) PURE;
	STDMETHOD(GetAACPid) (THIS_ WORD * pAacPid) PURE;
	STDMETHOD(GetAAC2Pid) (THIS_ WORD * pAac2Pid) PURE;
	STDMETHOD(GetCreateTxtPinOnDemux) (THIS_ WORD * pbCreatePin) PURE;
	STDMETHOD(SetCreateTxtPinOnDemux) (THIS_ WORD bCreatePin) PURE;
	STDMETHOD(Load) (THIS_ LPCOLESTR pszFileName,const AM_MEDIA_TYPE *pmt) PURE;
	STDMETHOD(GetPCRPosition) (THIS_ REFERENCE_TIME * pos) PURE;
	STDMETHOD(ShowStreamMenu)(THIS_ HWND hwnd) PURE;

//New method added after 2.2.0.3
	STDMETHOD(GetFixedAspectRatio) (THIS_ WORD * pbFixedAR) PURE;
	STDMETHOD(SetFixedAspectRatio) (THIS_ WORD pbFixedAR) PURE;

//New method added after 2.2.0.6
	STDMETHOD(GetCurFile)(THIS_ LPOLESTR * ppszFileName,AM_MEDIA_TYPE *pmt) PURE;
	STDMETHOD(GetDTSPid) (THIS_ WORD * pDtsPid) PURE;
	STDMETHOD(GetDTS2Pid) (THIS_ WORD * pDts2Pid) PURE;
	STDMETHOD(GetCreateSubPinOnDemux) (THIS_ WORD * pbCreatePin) PURE;
	STDMETHOD(SetCreateSubPinOnDemux) (THIS_ WORD bCreatePin) PURE;
	STDMETHOD(GetSubtitlePid) (THIS_ WORD * subpid) PURE;

//New method added after 2.2.0.8
	STDMETHOD(GetSharedMode) (THIS_ WORD * pSharedMode) PURE;
	STDMETHOD(SetSharedMode) (THIS_ WORD SharedMode) PURE;
	STDMETHOD(GetInjectMode) (THIS_ WORD * pInjectMode) PURE;
	STDMETHOD(SetInjectMode) (THIS_ WORD InjectMode) PURE;


};


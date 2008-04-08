/**
*  SettingsStore.ccp
*  Copyright (C) 2004-2006 bear
*  Copyright (C) 2003  Shaun Faulds
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
*  bear can be reached on the forums at
*    http://forums.dvbowners.com/
*/


#include "SettingsStore.h"

CSettingsStore::CSettingsStore(void)
{
	lastUsed = time(NULL);
}

CSettingsStore::~CSettingsStore(void)
{

}

void CSettingsStore::setName(std::string newName)
{
	int index = newName.find("\\");

	while(index > 0)
	{
		newName.replace(index, 1, "/");
		index = newName.find("\\");
	}

	name = newName;
}

std::string CSettingsStore::getName()
{
	return name;
}

BOOL CSettingsStore::getAutoModeReg()
{
	return autoMode;
}

BOOL CSettingsStore::getNPControlReg()
{
	return nPControl;
}

BOOL CSettingsStore::getNPSlaveReg()
{
	return nPSlave;
}

BOOL CSettingsStore::getMP2ModeReg()
{
	return mp2Mode;
}

BOOL CSettingsStore::getAC3ModeReg()
{
	return ac3Mode;
}

BOOL CSettingsStore::getFixedAspectRatioReg()
{
	return fixedAR;
}

BOOL CSettingsStore::getCreateTSPinOnDemuxReg()
{
	return tsPinMode;
}

BOOL CSettingsStore::getCreateTxtPinOnDemuxReg()
{
	return txtPinMode;
}

BOOL CSettingsStore::getCreateSubPinOnDemuxReg()
{
	return subPinMode;
}

BOOL CSettingsStore::getSharedModeReg()
{
	return sharedMode;
}

BOOL CSettingsStore::getInjectModeReg()
{
	return injectMode;
}

BOOL CSettingsStore::getDelayModeReg()
{
	return delayMode;
}

BOOL CSettingsStore::getRateControlModeReg()
{
	return rateMode;
}

BOOL CSettingsStore::getAudio2ModeReg()
{
	return audio2Mode;
}

int CSettingsStore::getProgramSIDReg()
{
	return programSID;
}

BOOL CSettingsStore::getROTModeReg()
{
	return rotMode;
}

BOOL CSettingsStore::getClockModeReg()
{
	return clockMode;
}

void CSettingsStore::setAutoModeReg(BOOL bAuto)
{
	autoMode = bAuto;
	return;
}

void CSettingsStore::setNPControlReg(BOOL bNPControl)
{
	nPControl = bNPControl;
	return;
}

void CSettingsStore::setNPSlaveReg(BOOL bNPSlave)
{
	nPSlave = bNPSlave;
	return;
}

void CSettingsStore::setMP2ModeReg(BOOL bMP2)
{
	mp2Mode = bMP2;
	return;
}

void CSettingsStore::setAC3ModeReg(BOOL bAC3)
{
	ac3Mode = bAC3;
	return;
}

void CSettingsStore::setFixedAspectRatioReg(BOOL bFixedAR)
{
	fixedAR = bFixedAR;
	return;
}

void CSettingsStore::setCreateTSPinOnDemuxReg(BOOL bTSPin)
{
	tsPinMode = bTSPin;
	return;
}

void CSettingsStore::setCreateTxtPinOnDemuxReg(BOOL bTxtPin)
{
	txtPinMode = bTxtPin;
	return;
}

void CSettingsStore::setCreateSubPinOnDemuxReg(BOOL bSubPin)
{
	subPinMode = bSubPin;
	return;
}

void CSettingsStore::setSharedModeReg(BOOL bSharedMode)
{
	sharedMode = bSharedMode;
	return;
}

void CSettingsStore::setInjectModeReg(BOOL bInjectMode)
{
	injectMode = bInjectMode;
	return;
}

void CSettingsStore::setDelayModeReg(BOOL bDelay)
{
	delayMode = bDelay;
	return;
}

void CSettingsStore::setRateControlModeReg(BOOL bRate)
{
	rateMode = bRate;
	return;
}

void CSettingsStore::setAudio2ModeReg(BOOL bAudio2)
{
	audio2Mode = bAudio2;
	return;
}

void CSettingsStore::setProgramSIDReg(int bSID)
{
	programSID = bSID;
	return;
}

void CSettingsStore::setROTModeReg(BOOL bROTMode)
{
	rotMode = bROTMode;
	return;
}

void CSettingsStore::setClockModeReg(BOOL bClockMode)
{
	clockMode = bClockMode;
	return;
}

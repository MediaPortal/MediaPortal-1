/**
*  SettingsSinkStore.ccp
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


#include "SettingsSinkStore.h"

CSettingsSinkStore::CSettingsSinkStore(SinkStoreParam *params)
{
	lastUsed = time(NULL);
	fileName = (params->fileName); //"MyBufferFile";
	minFiles = params->minFiles; //6;
	maxFiles = params->maxFiles; //60;
	maxSize = params->maxSize; //(__int64)((__int64)1048576 *(__int64)250); //250MB
	chunkSize = params->chunkSize; //(__int64)((__int64)1048576 *(__int64)250); //250MB
}

CSettingsSinkStore::~CSettingsSinkStore(void)
{

}

void CSettingsSinkStore::setName(std::string newName)
{
	int index = newName.find("\\");

	while(index > 0)
	{
		newName.replace(index, 1, "/");
		index = newName.find("\\");
	}

	name = newName;
}

std::string CSettingsSinkStore::getName()
{
	return name;
}

std::string CSettingsSinkStore::getRegFileNameReg()
{
	return fileName;
}

void CSettingsSinkStore::setRegFileNameReg(LPTSTR szFileName)
{
	fileName = (szFileName);

	int index = fileName.find("\\");

	while(index > 0)
	{
		fileName.replace(index, 1, "/");
		index = fileName.find("\\");
	}
}

long CSettingsSinkStore::getMinTSFilesReg()
{
	return minFiles;
}

void CSettingsSinkStore::setMinTSFilesReg(long lMinFiles)
{
	minFiles = lMinFiles;
}

long CSettingsSinkStore::getMaxTSFilesReg()
{
	return maxFiles;
}

void CSettingsSinkStore::setMaxTSFilesReg(long lMaxFiles)
{
	maxFiles = lMaxFiles;
}

__int64 CSettingsSinkStore::getMaxTSFileSizeReg()
{
	return maxSize;
}

void CSettingsSinkStore::setMaxTSFileSizeReg(__int64 llMaxSize)
{
	maxSize = llMaxSize;
}

__int64 CSettingsSinkStore::getChunkReserveReg()
{
	return chunkSize;
}

void CSettingsSinkStore::setChunkReserveReg(__int64 llChunkSize)
{
	chunkSize = llChunkSize;
}



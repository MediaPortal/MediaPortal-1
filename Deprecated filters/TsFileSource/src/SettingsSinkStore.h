/**
*  SettingsSinkStore.h
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

//#pragma once
#ifndef SETTINGSINKSTORE_H
#define SETTINGSINKSTORE_H


#include <windows.h>
#include <time.h>
#include <string>

typedef struct 
{
	LPTSTR fileName;
	long 	minFiles;
	long 	maxFiles;
	__int64	maxSize;
	__int64	chunkSize;
} SinkStoreParam;

class CSettingsSinkStore
{
public:
	CSettingsSinkStore(SinkStoreParam *params);
	~CSettingsSinkStore(void);

	__int64 getLastUsed();
	__int64 getStartPosition();
	std::string getName();

	void setLastUsed(__int64 time);
	void setStartPosition(__int64 start);
	void setName(std::string newName);

	std::string getRegFileNameReg();
	void setRegFileNameReg(LPTSTR szFileName);
	long getMinTSFilesReg();
	void setMinTSFilesReg(long lMinFiles);
	long getMaxTSFilesReg();
	void setMaxTSFilesReg(long lMaxFiles);
	__int64 getMaxTSFileSizeReg();
	void setMaxTSFileSizeReg(__int64 llMaxSize);
	__int64 getChunkReserveReg();
	void setChunkReserveReg(__int64 llChunkSize);

private:
	__int64 lastUsed;
	__int64 startAT;
	std::string name;
	
	std::string fileName;
	long 	minFiles;
	long 	maxFiles;
	__int64	maxSize;
	__int64	chunkSize;

};
#endif

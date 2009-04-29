/*
   DVRview
   Copyright (C) 2003  Shaun Faulds

   This program is free software; you can redistribute it and/or modify
   it under the terms of the GNU General Public License as published by
   the Free Software Foundation; either version 2 of the License, or
   (at your option) any later version.

   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU General Public License for more details.

   You should have received a copy of the GNU General Public License
   along with this program; if not, write to the Free Software
   Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.

*/

#include "SettingsStore.h"

CSettingsStore::CSettingsStore(void)
{
	lastUsed = time(NULL);
}

CSettingsStore::~CSettingsStore(void)
{

}

void CSettingsStore::setLastUsed(__int64 time)
{
	lastUsed = time;
}

void CSettingsStore::setStartPosition(__int64 start)
{
	startAT = start;
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

__int64 CSettingsStore::getLastUsed()
{
	return lastUsed;
}

__int64 CSettingsStore::getStartPosition()
{
	return startAT;
}

std::string CSettingsStore::getName()
{
	return name;
}


/*
    Copyright (C) 2007-2010 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2.  If not, see <http://www.gnu.org/licenses/>.
*/

#pragma once

LPCTSTR folder = TEXT("Software\\Team MediaPortal\\IPTV");

// Registry setting names
LPCTSTR IPTVPort = TEXT("IPTVPort");

// Default values for the settings in registry
DWORD RTSP_CLIENT_PORT_MIN_DEFAULT_REG = 50000;
DWORD RTSP_CLIENT_PORT_MIN_DEFAULT;

#define MAX_REG_LENGTH 256

// For accessing the registry
void LoadSettingsFromRegistry();
void ReadRegistryKeyDword(HKEY hKey, LPCTSTR& lpSubKey, DWORD& data);  
void WriteRegistryKeyDword(HKEY hKey, LPCTSTR& lpSubKey, DWORD& data);

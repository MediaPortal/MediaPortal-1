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

#ifndef __VERSION
#define __VERSION

#include "VersionInfo.h"

#define UNIX_TIMESTAMP_2000_01_01                                     946684800
#define SECONDS_IN_DAY                                                86400

#define MP_URL_SOURCE_SPLITTER_VERSION_MAJOR                          2
#define MP_URL_SOURCE_SPLITTER_VERSION_MINOR                          2
#define MP_URL_SOURCE_SPLITTER_VERSION_REVISION                       21
#define MP_URL_SOURCE_SPLITTER_VERSION_BUILD                          ((BUILD_INFO_MP_URL_SOURCE_SPLITTER - UNIX_TIMESTAMP_2000_01_01) / SECONDS_IN_DAY)

#endif
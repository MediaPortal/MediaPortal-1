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

#ifndef __PARAMETERS_DEFINED
#define __PARAMETERS_DEFINED

#define PARAMETER_NAME_INTERFACE                                              L"Interface"
#define PARAMETER_NAME_URL                                                    L"Url"
#define PARAMETER_NAME_MAX_PLUGINS                                            L"MaxPlugins"
#define PARAMETER_NAME_DOWNLOAD_FILE_NAME                                     L"DownloadFileName"
#define PARAMETER_NAME_CACHE_FOLDER                                           L"CacheFolder"
#define PARAMETER_NAME_LIVE_STREAM                                            L"LiveStream"
#define PARAMETER_NAME_DUMP_INPUT_RAW_DATA                                    L"DumpInputRawData"
#define PARAMETER_NAME_DUMP_OUTPUT_PIN_RAW_DATA                               L"DumpOutputPinRawData"

#define PARAMETER_NAME_LOG_GLOBAL_MUTEX_NAME                                  L"LogGlobalMutexName"
#define PARAMETER_NAME_LOG_BACKUP_FILE_NAME                                   L"LogBackupFileName"
#define PARAMETER_NAME_LOG_FILE_NAME                                          L"LogFileName"
#define PARAMETER_NAME_LOG_MAX_SIZE                                           L"LogMaxSize"
#define PARAMETER_NAME_LOG_VERBOSITY                                          L"LogVerbosity"

#define PARAMETER_NAME_FINISH_TIME                                            L"FinishTime"

#define PARAMETER_NAME_LIVE_STREAM_DEFAULT                                    false
#define PARAMETER_NAME_DUMP_INPUT_RAW_DATA_DEFAULT                            false
#define PARAMETER_NAME_DUMP_OUTPUT_PIN_RAW_DATA_DEFAULT                       false

#define LOG_MAX_SIZE_DEFAULT                                                  10485760
#define LOG_VERBOSITY_DEFAULT                                                 LOGGER_VERBOSE

#endif
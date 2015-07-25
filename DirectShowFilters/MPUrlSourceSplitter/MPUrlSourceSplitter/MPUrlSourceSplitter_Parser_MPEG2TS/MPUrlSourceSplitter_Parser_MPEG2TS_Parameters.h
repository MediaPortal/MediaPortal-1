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

#ifndef __MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_PARAMETERS_DEFINED
#define __MP_URL_SOURCE_SPLITTER_PARSER_MPEG2TS_PARAMETERS_DEFINED

#define PARAMETER_NAME_MPEG2TS_DETECT_DISCONTINUITY                   L"Mpeg2TsDetectDiscontinuity"
#define PARAMETER_NAME_MPEG2TS_ALIGN_TO_MPEG2TS_PACKET                L"Mpeg2TsAlignToMpeg2TSPacket"

#define PARAMETER_NAME_MPEG2TS_TRANSPORT_STREAM_ID                    L"Mpeg2TsTransportStreamID"
#define PARAMETER_NAME_MPEG2TS_PROGRAM_NUMBER                         L"Mpeg2TsProgramNumber"
#define PARAMETER_NAME_MPEG2TS_PROGRAM_MAP_PID                        L"Mpeg2TsProgramMapPID"

#define PARAMETER_NAME_MPEG2TS_SET_NOT_SCRAMBLED                      L"Mpeg2TsSetNotScrambled"

#define PARAMETER_NAME_MPEG2TS_FILTER_PROGRAM_NUMBER_COUNT            L"Mpeg2TsFilterProgramNumberCount"

#define PARAMETER_NAME_FORMAT_MPEG2TS_FILTER_PROGRAM_NUMBER           L"Mpeg2TsFilterProgramNumber%08u"

#define PARAMETER_NAME_FORMAT_MPEG2TS_LEAVE_PROGRAM_ELEMENT_COUNT     L"Mpeg2TsFilterProgramNumber%08uLeaveProgramElementCount"

#define PARAMETER_NAME_FORMAT_MPEG2TS_LEAVE_PROGRAM_ELEMENT           L"Mpeg2TsFilterProgramNumber%08uLeaveProgramElement%08u"

#define PARAMETER_NAME_MPEG2TS_STREAM_ANALYSIS                        L"Mpeg2TsStreamAnalysis"

#define MPEG2TS_DETECT_DISCONTINUITY_DEFAULT                          true
#define MPEG2TS_ALIGN_TO_MPEG2TS_PACKET                               true

#define MPEG2TS_TRANSPORT_STREAM_ID_DEFAULT                           UINT_MAX
#define MPEG2TS_PROGRAM_NUMBER_DEFAULT                                UINT_MAX
#define MPEG2TS_PROGRAM_MAP_PID_DEFAULT                               UINT_MAX

#define MPEG2TS_SET_NOT_SCRAMBLED_DEFAULT                             false

#define MPEG2TS_FILTER_PROGRAM_NUMBER_COUNT_DEFAULT                   0
#define MPEG2TS_LEAVE_PROGRAM_ELEMENT_COUNT_DEFAULT                   0

#define MPEG2TS_STREAM_ANALYSIS_DEFAULT                               false

#endif

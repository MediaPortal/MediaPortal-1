/* 
*	Copyright (C) 2006-2009 Team MediaPortal
*	http://www.team-mediaportal.com
*
*  This Program is free software; you can redistribute it and/or modify
*  it under the terms of the GNU General Public License as published by
*  the Free Software Foundation; either version 2, or (at your option)
*  any later version.
*   
*  This Program is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
*  GNU General Public License for more details.
*   
*  You should have received a copy of the GNU General Public License
*  along with GNU Make; see the file COPYING.  If not, write to
*  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
*  http://www.gnu.org/copyleft/gpl.html
*
*/

extern void LogDebug(const char *fmt, ...) ;
#include "MPMPEG2TransportStreamFromPESSource.h"


MPMPEG2TransportStreamFromPESSource* MPMPEG2TransportStreamFromPESSource
::createNew(UsageEnvironment& env, MPEG1or2DemuxedElementaryStream* inputSource) {
	return new MPMPEG2TransportStreamFromPESSource(env, inputSource);
}

MPMPEG2TransportStreamFromPESSource
::MPMPEG2TransportStreamFromPESSource(UsageEnvironment& env,
									  MPEG1or2DemuxedElementaryStream* inputSource)
									  : MPEG2TransportStreamFromPESSource(env,inputSource),
									  fMPInputSource(inputSource) {
										  LogDebug("MPEG2TransportStreamFromPESSource::ctor:%x",this);
}

MPMPEG2TransportStreamFromPESSource::~MPMPEG2TransportStreamFromPESSource() 
{
	LogDebug("MPEG2TransportStreamFromPESSource::dtor:%x",this);
	fMPInputSource=NULL;
}


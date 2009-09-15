/**********
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

#ifndef _MPMPEG2_TRANSPORT_STREAM_FROM_PES_SOURCE_H
#define _MPMPEG2_TRANSPORT_STREAM_FROM_PES_SOURCE_H

#include "MPEG2TransportStreamFromPESSource.hh"

class MPMPEG2TransportStreamFromPESSource: public MPEG2TransportStreamFromPESSource {
public:
  static MPMPEG2TransportStreamFromPESSource*
  createNew(UsageEnvironment& env, MPEG1or2DemuxedElementaryStream* inputSource);


  MPEG1or2DemuxedElementaryStream* InputSource() { return fMPInputSource;}
protected:
  MPMPEG2TransportStreamFromPESSource(UsageEnvironment& env,
				    MPEG1or2DemuxedElementaryStream* inputSource);
      // called only by createNew()
  virtual ~MPMPEG2TransportStreamFromPESSource();

private:
  MPEG1or2DemuxedElementaryStream* fMPInputSource;
};

#endif

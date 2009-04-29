/* 
 *	Copyright (C) 2006-2008 Team MediaPortal
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
#pragma warning( disable: 4995 4996 )

#include "ChannelInfo.h"

CChannelInfo::CChannelInfo(void)
{
  Reset();
}

CChannelInfo::~CChannelInfo(void)
{
}

void CChannelInfo::Reset()
{
	LCN=10000;
  NetworkId=0;
  TransportId=0;
  ServiceId=0;
  EIT_schedule_flag=0;
  EIT_present_following_flag=0;
  RunningStatus=0;
  FreeCAMode=0;
  ServiceType=0;
  MajorChannel=0;
  MinorChannel=0;
  Frequency=0;
  Modulation=0;
  strcpy(ProviderName,"");
  strcpy(ServiceName,"");
}

/* 
 *	Copyright (C) 2006 Team MediaPortal
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
#pragma once
#include "pidtable.h" 

class CChannelInfo
{
public:
  CChannelInfo(void);
  virtual ~CChannelInfo(void);
  void Reset();
  int NetworkId;
  int TransportId;
  int ServiceId;
  int MajorChannel;
  int MinorChannel;
  int Frequency;
  int EIT_schedule_flag;
  int EIT_present_following_flag;
  int RunningStatus;
  int FreeCAMode;
  int ServiceType;
  int Modulation;
	int LCN;
  char ProviderName[255];
  char ServiceName[255];

  CPidTable PidTable;
  int PatVersion;
};

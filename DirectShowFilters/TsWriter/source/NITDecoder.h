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
#pragma once
#include "..\..\shared\sectiondecoder.h"
#include "..\..\shared\section.h"
#include <vector>
using namespace std;

#define PID_NIT 0x10

typedef  struct stNITLCN
{
	int network_id;
	int transport_id;
	int service_id;
	int LCN;
}NITLCN;
typedef  struct stNITSatDescriptor
{
	int Frequency;
	float OrbitalPosition;
	int WestEastFlag;
	int Polarisation;
	int Modulation;
	int Symbolrate;
	int FECInner;
  int RollOff;
  int isS2;
	string NetworkName;
}NITSatDescriptor;
//
typedef  struct stNITCableDescriptor
{
	int Frequency;
	int FECOuter;
	int Modulation;
	int Symbolrate;
	int FECInner;
	string NetworkName;
}NITCableDescriptor;

//
typedef struct stNITTerrestrialDescriptor
{
	int CentreFrequency;
	int Bandwidth;
	int Constellation;
	int HierarchyInformation;
	int CoderateHPStream;
	int CoderateLPStream;
	int GuardInterval;
	int TransmissionMode; 
	int OtherFrequencyFlag;
	string NetworkName;
}NITTerrestrialDescriptor;

typedef struct stDVBNetworkInfo
{
	vector<NITSatDescriptor>		  satteliteNIT;
	vector<NITCableDescriptor>		  cableNIT;
	vector<NITTerrestrialDescriptor>  terrestialNIT;
	vector<NITLCN>					  lcnNIT;
	string							  NetworkName;

}DVBNetworkInfo;

class CNITDecoder: public  CSectionDecoder
{
public:
	CNITDecoder(void);
	virtual ~CNITDecoder(void);
	void  OnNewSection(CSection& sections);
	void  Reset();
	bool  Ready();
	int		GetLogicialChannelNumber(int networkId, int transportId, int serviceId);
	DVBNetworkInfo m_nit;
private:
	void decodeNITTable(byte* buf);
	void DVB_GetLogicalChannelNumber(int original_network_id,int transport_stream_id,byte* buf);
  void DVB_GetSatDelivSys(byte* b,int maxLen);
  void DVB_GetTerrestrialDelivSys(byte*b , int maxLen);
  void DVB_GetCableDelivSys(byte* b, int maxLen);
	typedef  struct stNITLCN
	{
		int network_id;
		int transport_id;
		int service_id;
		int LCN;
	}NITLCN;

	vector<NITLCN> m_vecLCN;
	typedef vector<NITLCN>::iterator ivecLCN;
	DWORD m_timer;
};

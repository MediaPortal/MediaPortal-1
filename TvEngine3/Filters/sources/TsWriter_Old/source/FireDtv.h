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
#include <vector>
using namespace std;
typedef enum 
{
    KSPROPERTY_FIRESAT_SELECT_MULTIPLEX_DVB_S,
	KSPROPERTY_FIRESAT_SELECT_SERVICE_DVB_S,
	KSPROPERTY_FIRESAT_SELECT_PIDS_DVB_S,
    KSPROPERTY_FIRESAT_SIGNAL_STRENGTH_TUNER,
	KSPROPERTY_FIRESAT_DRIVER_VERSION,
	KSPROPERTY_FIRESAT_SELECT_MULTIPLEX_DVB_T,
	KSPROPERTY_FIRESAT_SELECT_PIDS_DVB_T,
	KSPROPERTY_FIRESAT_SELECT_MULTIPLEX_DVB_C,
	KSPROPERTY_FIRESAT_SELECT_PIDS_DVB_C,
    KSPROPERTY_FIRESAT_GET_FRONTEND_STATUS,	
	KSPROPERTY_FIRESAT_GET_SYSTEM_INFO,     
	KSPROPERTY_FIRESAT_GET_FIRMWARE_VERSION,
	KSPROPERTY_FIRESAT_LNB_CONTROL,	
	KSPROPERTY_FIRESAT_GET_LNB_PARAM,
	KSPROPERTY_FIRESAT_SET_LNB_PARAM,
	KSPROPERTY_FIRESAT_SET_POWER_STATUS,
	KSPROPERTY_FIRESAT_SET_AUTO_TUNE_STATUS,
	KSPROPERTY_FIRESAT_FIRMWARE_UPDATE,
	KSPROPERTY_FIRESAT_FIRMWARE_UPDATE_STATUS,
	KSPROPERTY_FIRESAT_CI_RESET,
	KSPROPERTY_FIRESAT_CI_WRITE_TPDU,
	KSPROPERTY_FIRESAT_CI_READ_TPDU,
	KSPROPERTY_FIRESAT_HOST2CA,
	KSPROPERTY_FIRESAT_CA2HOST,
	KSPROPERTY_FIRESAT_GET_BOARD_TEMP,
	KSPROPERTY_FIRESAT_TUNE_QPSK,

	// Remote controlling
	KSPROPERTY_FIRESAT_REMOTE_CONTROL_REGISTER,
	KSPROPERTY_FIRESAT_REMOTE_CONTROL_CANCEL,

	KSPROPERTY_FIRESAT_GET_CI_STATUS,
	KSPROPERTY_FIRESAT_TEST_INTERFACE,

} KSPROPERTY_FIRESAT;


// Use also for DVB-C FireDTV
typedef struct _FIRESAT_SELECT_PIDS_S
{
	BOOL				bCurrentTransponder;//FRODO : Set TRUE
	BOOL				bFullTransponder;   //FRODO : Set FALSE when selecting PIDs
	UCHAR				uLnb;		        //FRODO : Don´t care
	struct
	{
		ULONG			uFrequency;    // 9.750.000 - 12.750.000 kHz
		ULONG			uSymbolRate;   // kBaud 1.000 - 40.000
		UCHAR			uFecInner;	   // FEC_12,FEC_23, FEC_34, FEC_56, FEC_78
		UCHAR			uPolarization; // POLARIZATION_HORIZONTAL, POLARIZATION_VERTICAL, POLARIZATION_NONE
	}QpskParameter; //FRODO : Don´t care this structure
	UCHAR				uNumberOfValidPids; // 1-16
	WORD				uPids[16];
}FIRESAT_SELECT_PIDS_DVBS, *PFIRESAT_SELECT_PIDS_DVBS;

typedef struct _FIRESAT_SELECT_PIDS_DVBT
{
	BOOL				bCurrentTransponder;//FRODO : Set TRUE
	BOOL				bFullTransponder;   //FRODO : Set FALSE when selecting PIDs
	struct
	{
		ULONG			uFrequency;    // kHz 47.000-860.000
		UCHAR			uBandwidth;    // BANDWIDTH_8_MHZ, BANDWIDTH_7_MHZ, BANDWIDTH_6_MHZ
		UCHAR			uConstellation;// CONSTELLATION_DVB_T_QPSK,CONSTELLATION_QAM_16,CONSTELLATION_QAM_64,OFDM_AUTO
		UCHAR			uCodeRateHP;   // CR_12,CR_23,CR_34,CR_56,CR_78,OFDM_AUTO
		UCHAR			uCodeRateLP;   // CR_12,CR_23,CR_34,CR_56,CR_78,OFDM_AUTO
		UCHAR			uGuardInterval;// GUARD_INTERVAL_1_32,GUARD_INTERVAL_1_16,GUARD_INTERVAL_1_8,GUARD_INTERVAL_1_4,OFDM_AUTO
		UCHAR			uTransmissionMode;// TRANSMISSION_MODE_2K, TRANSMISSION_MODE_8K, OFDM_AUTO
		UCHAR			uHierarchyInfo;// HIERARCHY_NONE,HIERARCHY_1,HIERARCHY_2,HIERARCHY_4,OFDM_AUTO
	}OFDMParameter;//FRODO : Don´t care this structure
	UCHAR				uNumberOfValidPids; // 1-16
	WORD				uPids[16];
}FIRESAT_SELECT_PIDS_DVBT, *PFIRESAT_SELECT_PIDS_DVBT;


class CFireDtv
{
public:
	CFireDtv(IFilterGraph *graph);
	virtual ~CFireDtv(void);
	bool		IsFireDtv();
	bool		SetPids(vector<int> pids);
	void		DisablePidFiltering();
private:
	bool m_isDvbt;
	HRESULT SelectPids_DVBT(PFIRESAT_SELECT_PIDS_DVBT pPids);
	HRESULT SelectPids_DVBS(PFIRESAT_SELECT_PIDS_DVBS		pPids);
	CComQIPtr<IKsPropertySet> m_pPropertySet;
};

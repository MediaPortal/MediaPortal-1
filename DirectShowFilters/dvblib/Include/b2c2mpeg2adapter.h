/*
// Copyright (c) 1998-2002 B2C2, Incorporated.  All Rights Reserved.
//
// THIS IS UNPUBLISHED PROPRIETARY SOURCE CODE OF B2C2, INCORPORATED.
// The copyright notice above does not evidence any
// actual or intended publication of such source code.
//
// This file is proprietary source code of B2C2, Incorporated. and is released pursuant to and
// subject to the restrictions of the non-disclosure agreement and license contract entered
// into by the parties.
*/
//
// File: b2c2mpeg2adapter.h
//
//
// Note: (Windows) In order to use the device notification handling support functions 
//       of the adapter class, 
//			
//           _B2C2_USE_DEVICE_NOTIFICATION and WINVER=0x0500
//
//       must be defined at the Project Settings
//

#ifndef _b2c2mpeg2adapter_h_
#define _b2c2mpeg2adapter_h_

#if defined _B2C2_USE_DEVICE_NOTIFICATION
	#include <dbt.h>
	#include <cfgmgr32.h>	// for MAX_DEVICE_ID_LEN and CM_Get_Device_ID ()
							// part of NTDDK
#endif //defined _B2C2_USE_DEVICE_NOTIFICATION

#if defined __linux__
	class IB2C2MPEG2TunerCtrl2;
	class IB2C2MPEG2DataCtrl3;
	class IB2C2MPEG2AVCtrl2;
	
	// Forward declaration, since we are using a pointer only.
    class CAVSrcFilter;
#endif //defined __linux__

#if defined WIN32
	typedef interface IGraphBuilder			IGraphBuilder;
	typedef interface IBaseFilter			IBaseFilter;
	typedef interface IPin					IPin;
	typedef interface IMediaControl			IMediaControl;
	typedef interface IMediaEvent			IMediaEvent;
	typedef interface IB2C2MPEG2DataCtrl3	IB2C2MPEG2DataCtrl3;
	typedef interface IB2C2MPEG2TunerCtrl2	IB2C2MPEG2TunerCtrl2;
	typedef interface IB2C2MPEG2AVCtrl2		IB2C2MPEG2AVCtrl2;

	#define B2C2_USB_DEVICE_ID				TEXT("vid_0af7")
	#define B2C2_PCI_DEVICE_ID				TEXT("13d0")
											
	#define B2C2_FILTER_MAX_TS_PINS			4
#endif //defined WIN32

//
//  B2C2MPEG2Adapter
// 
class B2C2MPEG2Adapter
{
public: // Construction
	B2C2MPEG2Adapter (const TCHAR *pszAdapterName);
	~B2C2MPEG2Adapter ();
	
public: // Interface

#if defined WIN32
	inline IGraphBuilder*			GetFilterGraph()	{ return m_pFilterGraph; }
#endif //defined WIN32

	inline IB2C2MPEG2TunerCtrl2*	GetTunerControl()	{ return m_pIB2C2MPEG2TunerCtrl; }
	inline IB2C2MPEG2DataCtrl3*		GetDataControl()	{ return m_pIB2C2MPEG2DataCtrl;	}
	inline IB2C2MPEG2AVCtrl2*		GetAvControl()		{ return m_pIB2C2MPEG2AvCtrl; }

	inline BOOL IsInitialized()	{ return (m_pFilter != NULL); };

public: // Methods
	HRESULT Initialize();
	void Release();

	// Error information handling methods

	inline DWORD GetLastError()
	{
		DWORD dwRet = m_dwLastErrorCode;
		m_dwLastErrorCode = 0;
		return dwRet;
	}
	inline const TCHAR* GetLastErrorText() 	{ return (const TCHAR*) m_szLastErrorText; }

	inline void SetLastError(const TCHAR *szErrText, DWORD dwErrCode)
	{
		m_dwLastErrorCode = dwErrCode;
		sprintf( m_szLastErrorText, TEXT("%.*s"), B2C2_MAX_ERROR_TEXT-1, szErrText);
	}

#if defined WIN32
	HRESULT EnumerateFilterPins (BOOL bAudoPin = TRUE, BOOL bVideoPin = TRUE, BOOL bTsPins = TRUE);
	HRESULT GetAudioVideoOutPins (IPin **ppPinOutAudio, IPin **ppPinOutVideo);

	HRESULT GetMediaControl (IMediaControl **ppMediaControl);
	HRESULT GetMediaEvent (IMediaEvent **ppMediaEvent);

	HRESULT CreateTsFilter (int nPin, REFCLSID refCLSID, IBaseFilter **ppCustomFilter = NULL);
	HRESULT GetTsInterfaceFilter (int nPin, const IID& iid, IUnknown ** ppInterfaceFilter);
	HRESULT GetTsOutPin (int nPin, IPin **ppTsOutPin);
	HRESULT ConnectTsFilterInToTsOutPin (int nPin, const TCHAR * szInPinName = NULL);
#endif //defined WIN32

protected: // Instantiated member classes (Linux) or Interface pointer (Windows)

	IB2C2MPEG2TunerCtrl2	*m_pIB2C2MPEG2TunerCtrl;	
	IB2C2MPEG2DataCtrl3		*m_pIB2C2MPEG2DataCtrl;	
	IB2C2MPEG2AVCtrl2		*m_pIB2C2MPEG2AvCtrl;

#if defined __linux__
	CAVSrcFilter			*m_pFilter;
#endif //defined __linux__

#if defined WIN32
	IGraphBuilder			*m_pFilterGraph;
	IBaseFilter				*m_pFilter;
	IPin					*m_pPinOutAudio;
	IPin					*m_pPinOutVideo;
	IMediaControl			*m_pMediaControl;
	IMediaEvent				*m_pMediaEvent;
	IBaseFilter				*m_pTsPinFilter[B2C2_FILTER_MAX_TS_PINS];
	IUnknown				*m_pTsPinInterfaceFilter[B2C2_FILTER_MAX_TS_PINS];
	IPin					*m_pTsOutPin[B2C2_FILTER_MAX_TS_PINS];
	IPin					*m_pTsFilterInPin[B2C2_FILTER_MAX_TS_PINS];

#endif //defined WIN32

private: // Member variables
	enum 
	{
		B2C2_MAX_ERROR_TEXT	= 50,
		B2C2_MAX_DEVICE_NAME_LEN = 256,
	};

	TCHAR	m_szLastErrorText[B2C2_MAX_ERROR_TEXT];
	DWORD	m_dwLastErrorCode;

//
// Device Notification Handling
//

#if defined _B2C2_USE_DEVICE_NOTIFICATION

public: //Device Notification Methods

	BOOL RegisterDeviceNotification(HANDLE hRecipient);
	BOOL UnregisterDeviceNotification();

	int IsDeviceArrival(WPARAM wChangeEvent, LPARAM lData);
	int IsDeviceRemoveComplete(WPARAM wChangeEvent, LPARAM lData);

public: // Definitions / member variables

	enum E_B2C2_DEVICE
	{
		EDEV_NON_B2C2 = 0,
		EDEV_B2C2_USB,
		EDEV_B2C2_PCI,
	};

protected: // Methods

	E_B2C2_DEVICE IsDeviceBroadcastEvent(UINT uiEvent, WPARAM wChangeEvent, LPARAM lData);
	E_B2C2_DEVICE GetB2C2DeviceType (PDEV_BROADCAST_HDR pDevBcHdr);

private: // member variables

	HDEVNOTIFY	m_hDevNotify;
	HANDLE		m_hUser32Dll;

private: // Temporary Variables

	TCHAR m_szDeviceId[MAX_DEVICE_ID_LEN];
	TCHAR m_szTmpStr[B2C2_MAX_DEVICE_NAME_LEN];

#endif //defined _B2C2_USE_DEVICE_NOTIFICATION

};

#endif // _b2c2mpeg2adapter_h_


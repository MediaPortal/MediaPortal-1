#include <windows.h>
#include <xprtdefs.h>
#include <initguid.h>

#pragma once

const int TELETEXT_EVENT_SEEK_START = 0;
const int TELETEXT_EVENT_SEEK_END = 1;
const int TELETEXT_EVENT_RESET = 2;
//const int TELETEXT_EVENT_BUFFER_IN_UPDATE = 3;
//const int TELETEXT_EVENT_BUFFER_OUT_UPDATE = 4;
const int TELETEXT_EVENT_PACKET_PCR_UPDATE = 5;
const int TELETEXT_EVENT_CURRENT_PCR_UPDATE = 6;
//const int TELETEXT_EVENT_COMPENSATION_UPDATE = 7;
const int TELETEXT_EVENTVALUE_NONE = 0;
        
// {3AB7E208-7962-11DC-9F76-850456D89593}
DEFINE_GUID(IID_ITeletextSource, 
  0x3AB7E208, 0x7962, 0x11DC, 0x9F, 0x76, 0x85, 0x04, 0x56, 0xD8, 0x95, 0x93);


DECLARE_INTERFACE_( ITeletextSource, IUnknown )
{
  STDMETHOD(SetTeletextTSPacketCallBack) ( int (CALLBACK *pTeletextCallback)(byte*, int) ) PURE;
  STDMETHOD(SetTeletextEventCallback( int (CALLBACK *pResetCallback)(int eventcode, DWORD64 eval))) PURE; 
  STDMETHOD(SetTeletextServiceInfoCallback( int (CALLBACK *pServiceInfoCallback)(int page, byte type, byte lb1, byte lb2, byte lb3))) PURE; 
};
// dvblib.h : Hauptheaderdatei für die dvblib DLL
//
#include "stdafx.h"
//

#pragma once

#ifndef __AFXWIN_H__
	#error 'stdafx.h' muss vor dieser Datei in PCH eingeschlossen werden.
#endif

#include "resource.h"		// Hauptsymbole

//
// CdvblibApp
// Siehe dvblib.cpp für die Implementierung dieser Klasse
//
typedef struct
{
    WORD SectionLength          : 12;
    WORD Reserved               :  2;
    WORD PrivateIndicator       :  1;
    WORD SectionSyntaxIndicator :  1;
} MPEG_HEADER_BITS, *PMPEG_HEADER_BITS;

typedef HRESULT (__stdcall *DeliverSectionData)(BYTE *dataPointer,long sectionLength,WORD header);

class CdvblibApp : public CWinApp
{
public:
	CdvblibApp();

// Überschreibungen
public:
	virtual BOOL InitInstance();

	DECLARE_MESSAGE_MAP()
};

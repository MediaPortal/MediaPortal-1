//////////////////////////////////////////////////////////////////////////////
//
//                          (C) TechnoTrend AG 2005
//  All rights are reserved. Reproduction in whole or in part is prohibited
//  without the written consent of the copyright owner.
//
//  TechnoTrend reserves the right to make changes without notice at any time.
//  TechnoTrend makes no warranty, expressed, implied or statutory, including
//  but not limited to any implied warranty of merchantability or fitness for
//  any particular purpose, or that the use will not infringe any third party
//  patent, copyright or trademark. TechnoTrend must not be liable for any
//  loss or damage arising from its use.
//
//////////////////////////////////////////////////////////////////////////////

#ifndef TYP_SLOTINFO_H
#define TYP_SLOTINFO_H

/////////////////////////////////////////////////////////////////////////////
/// \brief
///     CI slot informations.
/////////////////////////////////////////////////////////////////////////////
typedef struct _TYP_SLOT_INFO
{
    /// CI status
	BYTE	nStatus;
    /// menu title string
	char*	pMenuTitleString;
    /// cam system ID's
	WORD*	pCaSystemIDs;
    /// number of cam system ID's
	WORD	wNoOfCaSystemIDs;
} TYP_SLOT_INFO;

#endif // #ifndef TYP_SLOTINFO_H

// eof

//------------------------------------------------------------------------------
// File: GargUIDs.h
//
// Desc: DirectShow sample code - definition of CLSIDs.
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------


#ifndef __GARGUIDS__
#define __GARGUIDS__

#ifdef __cplusplus
extern "C" {
#endif


//
// CcParser filter object
// {6F0B7D9C-7548-49a9-AC4C-1DA1927E6C15}
DEFINE_GUID(CLSID_CcParser,
0x6f0b7d9c, 0x7548, 0x49a9, 0xac, 0x4c, 0x1d, 0xa1, 0x92, 0x7e, 0x6c, 0x15);


//
// CcParser filter property page
// {1FE316FF-461C-4ac6-BEF8-4230D2744047}
DEFINE_GUID(CLSID_CcPProp,
0x1fe316ff, 0x461c, 0x4ac6, 0xbe, 0xf8, 0x42, 0x30, 0xd2, 0x74, 0x40, 0x47);

//
//  Note: ICcParser's uuid is defined with the interface (see igargle.h)
//  ICcParser is a private interface created by the filter.
//  The filter object and the property page defined here are public interfaces
//  that can be called by an application or another filter.


#ifdef __cplusplus
}
#endif

#endif // __GARGUIDS__

/**
*  NetworkGuids.h
*  Copyright (C) 2006      bear
*
*  This file is part of TSFileSource, a directshow push source filter that
*  provides an MPEG transport stream output.
*
*  TSFileSource is free software; you can redistribute it and/or modify
*  it under the terms of the GNU General Public License as published by
*  the Free Software Foundation; either version 2 of the License, or
*  (at your option) any later version.
*
*  TSFileSource is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*  GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License
*  along with TSFileSource; if not, write to the Free Software
*  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*
*  nate can be reached on the forums at
*    http://forums.dvbowners.com/
*/
#ifndef NETWORKGUIDS_H
#define NETWORKGUIDS_H

// {a07e6137-6c07-45d9-a00c-7de7a7e6319b}
extern GUID CLSID_BDA_DSNetSend =
{0xa07e6137, 0x6c07, 0x45d9, 0xa0, 0x0c, 0x7d, 0xe7, 0xa7, 0xe6, 0x31, 0x9b};

// {4ce1b653-cb0d-4eba-a723-01026bb5ff13}
extern GUID CLSID_BDA_DSNetReceive =
{0x4ce1b653, 0xcb0d, 0x4eba, 0xa7, 0x23, 0x01, 0x02, 0x6b, 0xb5, 0xff, 0x13};

// {CE3B76CB-9540-48fa-9974-69A625D478E3}
extern GUID CLSID_DSNetSend = 
{0xce3b76cb, 0x9540, 0x48fa, 0x99, 0x74, 0x69, 0xa6, 0x25, 0xd4, 0x78, 0xe3};

// {319F0815-ACEF-45fe-B497-A2E5D90A69D7}
extern GUID CLSID_DSNetReceive = 
{0x319f0815, 0xacef, 0x45fe, 0xb4, 0x97, 0xa2, 0xe5, 0xd9, 0xa, 0x69, 0xd7};

//
//  property page CLSIDs
//

// {fa966959-7498-4f16-8585-cc1b688501cd}
extern GUID CLSID_BDA_IPMulticastSendProppage =
{0xfa966959, 0x7498, 0x4f16, 0x85, 0x85, 0xcc, 0x1b, 0x68, 0x85, 0x01, 0xcd};

// {b8753014-fd31-4a3e-929f-982dc29d6f4a}
extern GUID CLSID_BDA_IPMulticastRecvProppage =
{0xb8753014, 0xfd31, 0x4a3e, 0x92, 0x9f, 0x98, 0x2d, 0xc2, 0x9d, 0x6f, 0x4a};

// {03EC9C19-13C4-43d7-B183-895CB89E761C}
extern GUID CLSID_IPMulticastSendProppage =
{0x3ec9c19, 0x13c4, 0x43d7, 0xb1, 0x83, 0x89, 0x5c, 0xb8, 0x9e, 0x76, 0x1c};

// {DC01D8AD-2BF8-4914-A15E-231A96C04B0A}
extern GUID CLSID_IPMulticastRecvProppage =
{0xdc01d8ad, 0x2bf8, 0x4914, 0xa1, 0x5e, 0x23, 0x1a, 0x96, 0xc0, 0x4b, 0xa};


//**********************************************************


//
//  interfaces
//
// {1CB42CC8-D32C-4f73-9267-C114DA470378}
extern GUID IID_IMulticastConfig =
{0x1CB42CC8, 0xD32C, 0x4f73, 0x92, 0x67, 0xC1, 0x14, 0xDA, 0x47, 0x03, 0x78};

DECLARE_INTERFACE_(IMulticastConfig, IUnknown) 
{
	STDMETHOD(SetNetworkInterface) (THIS_ ULONG ulNIC) PURE;
	STDMETHOD(GetNetworkInterface) (THIS_ ULONG *pNIC) PURE;
	STDMETHOD(SetMulticastGroup) (THIS_ ULONG ulIP, USHORT usPort) PURE;
	STDMETHOD(GetMulticastGroup) (THIS_ ULONG *pIP, USHORT *pPort) PURE;
};
#endif
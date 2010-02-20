/**
*  NetRender.h
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

#ifndef NETRENDER_H
#define NETRENDER_H

class CNetRender 
{
	friend class NetInfo;
	friend class NetInfoArray;
public:
	CNetRender();
	virtual ~CNetRender();

	static HRESULT CreateNetworkGraph(NetInfo *netAddr);
	static HRESULT RestartNetworkGraph(NetInfo *netAddr);
	static void DeleteNetworkGraph(NetInfo *netAddr);
	static BOOL UpdateNetFlow(NetInfoArray *netArray);
	static BOOL IsMulticastActive(NetInfo *netAddr, NetInfoArray *netArray, int *pos);
	static BOOL IsMulticastAddress(LPOLESTR lpszFileName, NetInfo *netAddr);
	static BOOL IsMulticastingIP(DWORD dwIP);
	static BOOL IsUnicastingIP(DWORD dwIP);
	static HRESULT AddGraphToRot(
					IUnknown *pUnkGraph, 
					DWORD *pdwRegister
					);
	static void RemoveGraphFromRot(DWORD pdwRegister);
	static HRESULT GetObjectFromROT(WCHAR* wsFullName, IUnknown **ppUnk);

private:

};

#endif

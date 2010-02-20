/*
 *	Copyright (C) 2006 Team MediaPortal
 *  Author: tourettes
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

#include "DemuxPinMapper.h"
#include <windows.h>
#include <streams.h>
#include <bdaiface.h>

extern void Log( const char *fmt, ... );

HRESULT CDemuxPinMapper::MapPidToDemuxer( LONG pid, IPin *pDemuxerPin, MEDIA_SAMPLE_CONTENT sampleContent )
{
	IMPEG2PIDMap	*pMap=NULL;
	IEnumPIDMap		*pPidEnum=NULL;
	PID_MAP			  pm;
	ULONG			    count;

	HRESULT hr = pDemuxerPin->QueryInterface( IID_IMPEG2PIDMap,(void**)&pMap );
	if( SUCCEEDED(hr) && pMap != NULL )
	{
		hr = pMap->EnumPIDMap( &pPidEnum );
		if( SUCCEEDED(hr) && pPidEnum!=NULL )
		{
			while( pPidEnum->Next( 1, &pm, &count ) == S_OK )
			{
				if ( count != 1 )
				{
					break;
				}

				/*umPid = pm.ulPID;
				hr = pMap->UnmapPID( 1, &umPid );
				if( FAILED(hr) )
				{
					break;
				}*/
			}
			hr = pMap->MapPID( 1, (ULONG*)&pid, sampleContent );

			pPidEnum->Release();
		}
		pMap->Release();
	}

  if( hr != S_OK )
    Log( "CDemuxPinMapper::MapDemuxerPid failed! - %d", hr );

  return hr;
}
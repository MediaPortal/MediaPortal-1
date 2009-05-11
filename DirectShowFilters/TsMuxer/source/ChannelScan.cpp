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
#include "ChannelScan.h"
#include "TeletextGrabber.h"
#include "Hamming.h"

extern void LogDebug(const char *fmt, ...) ;

CChannelScan::CChannelScan(LPUNKNOWN pUnk, HRESULT *phr ) 
:CUnknown( NAME ("TsMuxerChannelScan"), pUnk)
{
	LogDebug("CChannelScan::ctor()");
	strcpy(m_sServiceName,"                    ");
	m_iBufferPos=0;
	m_pBuffer = new byte[20000];
	m_pBufferTemp = new byte[20000];
	m_bIsScanning = false;
}
CChannelScan::~CChannelScan(void)
{
	LogDebug("CChannelScan::dtor()");
	delete[] m_pBuffer;
	delete[] m_pBufferTemp;
}

STDMETHODIMP CChannelScan::SetCallBack(IAnalogChannelScanCallback* callback)
{
	LogDebug("CChannelScan::SetCallBack - %x", callback);
	m_pCallback=callback;
	return S_OK;
}
STDMETHODIMP CChannelScan::Start()
{
	LogDebug("CChannelScan::Start()");
	m_bIsScanning = true;
	m_bChannelFound = FALSE;
	strcpy(m_sServiceName,"                    ");
	return S_OK;
}
STDMETHODIMP CChannelScan::Stop()
{
	LogDebug("CChannelScan::Stop()");
	m_bIsScanning = false;
	m_bChannelFound = FALSE;
	return S_OK;
}

STDMETHODIMP CChannelScan::IsReady( BOOL* yesNo) 
{
	if(m_bChannelFound){
		LogDebug("CChannelScan::IsReady() - true");
	}else{
		LogDebug("CChannelScan::IsReady() - false");
	}
	*yesNo = m_bChannelFound;
	return S_OK;
}
STDMETHODIMP CChannelScan::GetChannel(char** serviceName)
{
	LogDebug("CChannelScan::GetChannel() -  '%s'",m_sServiceName);
	*serviceName=m_sServiceName;
	return S_OK;
}

void CChannelScan::OnTeletextData(byte* sampleData, int sampleLen)
{
	try{
		if(m_bIsScanning){
			byte magazine_and_packet_address;
			byte magazine_and_packet_address1;
			byte magazine_and_packet_address2;
			byte mag;
			byte packetNumber;
			memcpy(&m_pBuffer[m_iBufferPos], sampleData,sampleLen);
			m_iBufferPos += sampleLen;
			// Is at least one line in the buffer?
			while(m_iBufferPos>VBI_LINE_LENGTH){

				if(!(m_pBuffer[0] == 0 && m_pBuffer[1] == 0 && m_pBuffer[2] ==0 && m_pBuffer[3] ==0 && m_pBuffer[4] ==0)){
					magazine_and_packet_address1 = m_pBuffer[0];
					magazine_and_packet_address2 = m_pBuffer[1];
					magazine_and_packet_address = unham(magazine_and_packet_address1, magazine_and_packet_address2);

					mag = magazine_and_packet_address & 7;

					// mag == 0 mean magazin number 8. This is the one we're looking for.
					if (mag == 0){ 

						packetNumber = (magazine_and_packet_address >> 3) & 0x1f;

						// packetNumber 30 means that we have found the line
						if(packetNumber == 30){

							LogDebug("CChannelScan::OnTeletextData - Packet Number 30 of magazin 8 found");
							for (int i = 0; i < 20; i++)
							{
								m_sServiceName[i] = (char)(m_pBuffer[22 + i] & 127);
							}
							LogDebug("CChannelScan::OnTeletextData - Name found: '%s'",m_sServiceName);
							m_bChannelFound = TRUE;
							if(m_pCallback!=NULL){
								m_pCallback->OnScannerDone();
							}
						}
					}
				}
				// Now clean up the buffer
				m_iBufferPos -=VBI_LINE_LENGTH;
				memcpy(m_pBufferTemp,&m_pBuffer[VBI_LINE_LENGTH],m_iBufferPos);
				memcpy(m_pBuffer,m_pBufferTemp,m_iBufferPos);
			}
		}
	}catch (...) {
		LogDebug("CChannelScan:: ERROR");
	}
}

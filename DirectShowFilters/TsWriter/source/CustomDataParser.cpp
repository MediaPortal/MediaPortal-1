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

#include <windows.h>
#include <commdlg.h>
#include <bdatypes.h>
#include <time.h>
#include <streams.h>
#include <initguid.h>

#include "CustomDataParser.h"
#pragma warning(disable : 4995)

extern void LogDebug(const char *fmt, ...) ;
extern bool DisableCRCCheck();

CCustomDataParser::CCustomDataParser(LPUNKNOWN pUnk, HRESULT *phr) 
:CUnknown( NAME ("MpTsCustomDataGrabber"), pUnk)
{
  Reset();
}

CCustomDataParser::~CCustomDataParser(void)
{
}

void CCustomDataParser::Reset()
{
  m_CustomPacketWriter = new FileWriter;
  CEnterCriticalSection enter(m_section);

  for (int i=0; i < (int)m_vecDecoders.size();++i)
  {
    CSectionDecoder* pDecoder = m_vecDecoders[i];
    delete pDecoder;
  }

  m_vecDecoders.clear();
	
  for (int i=0; i < (int)m_vecDecoders.size();++i)
  {
    CSectionDecoder* pDecoder = m_vecDecoders[i];
    pDecoder->Reset();
  }

  m_bGrabbing=false;
}

bool CCustomDataParser::isGrabbing()
{
  CEnterCriticalSection enter(m_section);
  return m_bGrabbing;
}

void CCustomDataParser::AbortGrabbing()
{
  CEnterCriticalSection enter(m_section);
  m_bGrabbing=false;
}

void CCustomDataParser::OnTsPacket(byte* tsPacket)
{
  try
  {
    if (m_bGrabbing==false) return;	
    int pid=((tsPacket[1] & 0x1F) <<8)+tsPacket[2];
		
    if(IsPidWanted(pid))
    {
      for (int i=0; i < (int)m_vecDecoders.size();++i)
      {
        CSectionDecoder* pDecoder = m_vecDecoders[i];
		pDecoder->OnTsPacket(tsPacket);
      }
    }
  }
  catch(...)
  {
    LogDebug("On TSPacket Error");
  }
}

bool CCustomDataParser::IsPidWanted(int pid)
{
  for (int i=0; i < (int)m_WantedPids.size();++i)
  {
    if(pid==m_WantedPids[i])
    {
      return true;
    }
  }
  return false;
}

void CCustomDataParser::OnNewSection(int pid,int tableId,CSection& section)
{
  try
  {
    if (section.section_length>0)
    {
      byte sync[8]; // Set the Sync Bytes FF AA FF
      sync[0]=0xff;
      sync[1]=0xaa;
      sync[2]=0xff;
      byte first;
      byte last;
      last=pid & 0xff;
      first=pid>>8;
      sync[3]=first;
      sync[4]=last;
      byte first1;
      byte last1;
      last1=section.section_length & 0xff;
      first1=section.section_length>>8;
      sync[5]=first1;
      sync[6]=last1;
      sync[7]=section.section_number;
      m_CustomPacketWriter->Write(sync,8);
      m_CustomPacketWriter->Write(section.Data,section.section_length+5);
    }
  }
  catch(...)
  {
    LogDebug("exception in CCustomDataParser::OnNewSection");
  }
}

void CCustomDataParser::AddSectionDecoder(int pid)
{
  CSectionDecoder* pDecoder= new CSectionDecoder();
  try
  {
    pDecoder->SetPid(pid);
    pDecoder->EnableCrcCheck(false);
    pDecoder->SetCallBack(this);
    m_vecDecoders.push_back(pDecoder);
    m_WantedPids.push_back(pid);
    LogDebug("Add pid 0x%x to Custom Grabber",pid);
  }
  catch(...)
  {
    LogDebug("Error adding pid 0x%x to Custom Grabber");
  }
}

void CCustomDataParser::SetFileName(wchar_t* pwszFileName)
{
  try
  {
    m_CustomPacketWriter->SetFileName(pwszFileName);
	m_CustomPacketWriter->OpenFile();
	m_bGrabbing=true;
  }
  catch(...)
  {
    LogDebug("Error Setting Custom file name");
  }
}

void CCustomDataParser::OpenFile()
{
  try
  {
    m_CustomPacketWriter->OpenFileWithShare();
  }
  catch(...)
  {
    LogDebug("Error Opening Custom file");
  }
}

void CCustomDataParser::Stop()
{
  try
  {
    m_CustomPacketWriter->CloseFile();
	m_WantedPids.clear();
  }
  catch(...)
  {
    LogDebug("Error Closing Custom file");
  }
}
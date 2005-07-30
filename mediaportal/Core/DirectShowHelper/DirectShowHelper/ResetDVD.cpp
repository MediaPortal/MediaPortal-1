/* 
 *	Copyright (C) 2005 Media Portal
 *  Author: Frodo
 *	http://mediaportal.sourceforge.net
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
#include "StdAfx.h"
#include <io.h>
#include ".\resetdvd.h"
#include "CSSauth.h"
#include "CSSscramble.h"
#include "udf.h"

CResetDVD::CResetDVD(LPCTSTR path)
: m_session(DVD_END_ALL_SESSIONS)
, m_hDrive(INVALID_HANDLE_VALUE)
{
	if(Open(path))
	{
		if(BeginSession())
		{
			Authenticate();
			// GetDiscKey();
			EndSession();
		}

		Close();
	}
}

CResetDVD::~CResetDVD(void)
{
	EndSession();
}


bool CResetDVD::Open(LPCTSTR path)
{
	Close();

	CString fn = path;
	CString drive = _T("\\\\.\\") + fn.Left(fn.Find(':')+1);

	m_hDrive = CreateFile(drive, GENERIC_READ, FILE_SHARE_READ, NULL, 
		OPEN_EXISTING, FILE_ATTRIBUTE_READONLY|FILE_FLAG_SEQUENTIAL_SCAN, (HANDLE)NULL);
	if(m_hDrive == INVALID_HANDLE_VALUE)
		return(false);

	return(true);
}

void CResetDVD::Close()
{
	if(m_hDrive != INVALID_HANDLE_VALUE)
	{
		CloseHandle(m_hDrive);
		m_hDrive = INVALID_HANDLE_VALUE;
	}
}

bool CResetDVD::BeginSession()
{
	EndSession();

	if(m_hDrive == INVALID_HANDLE_VALUE)
		return(false);

	DWORD BytesReturned;
	if(!DeviceIoControl(m_hDrive, IOCTL_DVD_START_SESSION, NULL, 0, &m_session, sizeof(m_session), &BytesReturned, NULL))
	{
		m_session = DVD_END_ALL_SESSIONS;
		if(!DeviceIoControl(m_hDrive, IOCTL_DVD_END_SESSION, &m_session, sizeof(m_session), NULL, 0, &BytesReturned, NULL)
		|| !DeviceIoControl(m_hDrive, IOCTL_DVD_START_SESSION, NULL, 0, &m_session, sizeof(m_session), &BytesReturned, NULL))
		{
			CloseHandle(m_hDrive);
			DWORD err = GetLastError();
			return(false);
		}
	}

	return(true);
}

void CResetDVD::EndSession()
{
	if(m_session != DVD_END_ALL_SESSIONS)
	{
		DWORD BytesReturned;
		DeviceIoControl(m_hDrive, IOCTL_DVD_END_SESSION, &m_session, sizeof(m_session), NULL, 0, &BytesReturned, NULL);
        m_session = DVD_END_ALL_SESSIONS;
	}
}

bool CResetDVD::Authenticate()
{
	if(m_session == DVD_END_ALL_SESSIONS) 
		return(false);

	BYTE Challenge[10], Key[10];

	for(int i = 0; i < 10; i++) Challenge[i] = i;

	if(!SendKey(DvdChallengeKey, Challenge))
		return(false);

	if(!ReadKey(DvdBusKey1, Key))
		return(false);

	int varient = -1;

	for(int i = 31; i >= 0; i--)
	{
		BYTE KeyCheck[5];
		CSSkey1(i, Challenge, KeyCheck);
		if(!memcmp(KeyCheck, Key, 5))
			varient = i;
	}

	if(!ReadKey(DvdChallengeKey, Challenge))
		return(false);

	CSSkey2(varient, Challenge, &Key[5]);

	if(!SendKey(DvdBusKey2, &Key[5]))
		return(false);

	CSSbuskey(varient, Key, m_SessionKey);

	return(true);
}

bool CResetDVD::GetDiscKey()
{
	if(m_session == DVD_END_ALL_SESSIONS) 
		return(false);

	BYTE DiscKeys[2048];
	if(!ReadKey(DvdDiskKey, DiscKeys))
		return(false);

	for(int i = 0; i < g_nPlayerKeys; i++)
	{
		for(int j = 1; j < 409; j++)
		{
			BYTE DiscKey[6];
			memcpy(DiscKey, &DiscKeys[j*5], 5);
			DiscKey[5] = 0;

			CSSdisckey(DiscKey, g_PlayerKeys[i]);

			BYTE Hash[6];
			memcpy(Hash, &DiscKeys[0], 5);
			Hash[5] = 0;

			CSSdisckey(Hash, DiscKey);

			if(!memcmp(Hash, DiscKey, 6))
			{
				memcpy(m_DiscKey, DiscKey, 6);
				return(true);
			}
		}
	}

	return(false);
}

bool CResetDVD::GetTitleKey(int lba, BYTE* pKey)
{
	if(m_session == DVD_END_ALL_SESSIONS) 
		return(false);

	if(!ReadKey(DvdTitleKey, pKey, lba))
		return(false);

	if(!(pKey[0]|pKey[1]|pKey[2]|pKey[3]|pKey[4]))
		return(false);

	pKey[5] = 0;

	CSStitlekey(pKey, m_DiscKey);

	return(true);
}

static void Reverse(BYTE* d, BYTE* s, int len)
{
	if(d == s)
	{
		for(s += len-1; d < s; d++, s--)
			*d ^= *s, *s ^= *d, *d ^= *s;
	}
	else
	{
		for(int i = 0; i < len; i++)
			d[i] = s[len-1 - i];
	}
}

bool CResetDVD::SendKey(DVD_KEY_TYPE KeyType, BYTE* pKeyData)
{
	CAutoPtr<DVD_COPY_PROTECT_KEY> key;

	switch(KeyType)
	{
		case DvdChallengeKey: 
			key.Attach((DVD_COPY_PROTECT_KEY*)new BYTE[DVD_CHALLENGE_KEY_LENGTH]);
			key->KeyLength = DVD_CHALLENGE_KEY_LENGTH;
			Reverse(key->KeyData, pKeyData, 10);
			break;
		case DvdBusKey2:
			key.Attach((DVD_COPY_PROTECT_KEY*)new BYTE[DVD_BUS_KEY_LENGTH]);
			key->KeyLength = DVD_BUS_KEY_LENGTH;
			Reverse(key->KeyData, pKeyData, 5);
			break;
		default: 
			break;
	}

	if(!key)
		return(false);

	key->SessionId = m_session;
	key->KeyType = KeyType;
	key->KeyFlags = 0;

	DWORD BytesReturned;
	return(!!DeviceIoControl(m_hDrive, IOCTL_DVD_SEND_KEY, key, key->KeyLength, NULL, 0, &BytesReturned, NULL));
}

bool CResetDVD::ReadKey(DVD_KEY_TYPE KeyType, BYTE* pKeyData, int lba)
{
	CAutoPtr<DVD_COPY_PROTECT_KEY> key;

	switch(KeyType)
	{
		case DvdChallengeKey: 
			key.Attach((DVD_COPY_PROTECT_KEY*)new BYTE[DVD_CHALLENGE_KEY_LENGTH]);
			key->KeyLength = DVD_CHALLENGE_KEY_LENGTH;
			key->Parameters.TitleOffset.QuadPart = 0;
			break;
		case DvdBusKey1:
			key.Attach((DVD_COPY_PROTECT_KEY*)new BYTE[DVD_BUS_KEY_LENGTH]);
			key->KeyLength = DVD_BUS_KEY_LENGTH;
			key->Parameters.TitleOffset.QuadPart = 0;
			break;
		case DvdDiskKey:
			key.Attach((DVD_COPY_PROTECT_KEY*)new BYTE[DVD_DISK_KEY_LENGTH]);
			key->KeyLength = DVD_DISK_KEY_LENGTH;
			key->Parameters.TitleOffset.QuadPart = 0;
			break;
		case DvdTitleKey:
			key.Attach((DVD_COPY_PROTECT_KEY*)new BYTE[DVD_TITLE_KEY_LENGTH]);
			key->KeyLength = DVD_TITLE_KEY_LENGTH;
			key->Parameters.TitleOffset.QuadPart = 2048i64*lba;
			break;
		default: 
			break;
	}

	if(!key)
		return(false);

	key->SessionId = m_session;
	key->KeyType = KeyType;
	key->KeyFlags = 0;

	DWORD BytesReturned;
	if(!DeviceIoControl(m_hDrive, IOCTL_DVD_READ_KEY, key, key->KeyLength, key, key->KeyLength, &BytesReturned, NULL))
	{
		DWORD err = GetLastError();
		return(false);
	}

	switch(KeyType)
	{
		case DvdChallengeKey:
			Reverse(pKeyData, key->KeyData, 10);
			break;
		case DvdBusKey1:
			Reverse(pKeyData, key->KeyData, 5);
			break;
		case DvdDiskKey:
			memcpy(pKeyData, key->KeyData, 2048);
			for(int i = 0; i < 2048/5; i++)
				pKeyData[i] ^= m_SessionKey[4-(i%5)];
			break;
		case DvdTitleKey:
			memcpy(pKeyData, key->KeyData, 5);
			for(int i = 0; i < 5; i++)
				pKeyData[i] ^= m_SessionKey[4-(i%5)];
			break;
		default: 
			break;
	}

	return(true);
}

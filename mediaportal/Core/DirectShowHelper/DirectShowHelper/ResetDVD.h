#pragma once

#include "winddk\ntddcdvd.h"

class CResetDVD
{
public:
	CResetDVD(LPCTSTR path);
	virtual ~CResetDVD(void);

protected:
	HANDLE m_hDrive;

	DVD_SESSION_ID m_session;
	bool BeginSession();
	void EndSession();

	BYTE m_SessionKey[5];
	bool Authenticate();

	BYTE m_DiscKey[6], m_TitleKey[6];
	bool GetDiscKey();
	bool GetTitleKey(int lba, BYTE* pKey);


	bool Open(LPCTSTR path);
	void Close();

	operator HANDLE() {return m_hDrive;}
	operator DVD_SESSION_ID() {return m_session;}

	bool SendKey(DVD_KEY_TYPE KeyType, BYTE* pKeyData);
	bool ReadKey(DVD_KEY_TYPE KeyType, BYTE* pKeyData, int lba = 0);

};

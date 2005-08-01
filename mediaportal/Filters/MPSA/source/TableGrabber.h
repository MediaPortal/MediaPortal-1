#pragma once
#pragma warning(disable: 4511 4512 4995)
#include "section.h"

class TableGrabber
{
public:
	TableGrabber(void);
	~TableGrabber(void);
	void OnPacket(byte* pbData,long lDataLen);
	bool IsSectionGrabbed();
	void Reset();
	void SetTableId(int pid,int tableId);
	byte* GetTable();
private:
	Sections m_sectionUtils;
	int      m_bufferPosition;
	byte     m_tableBuffer[70000];
	bool	 m_bSectionGrabbed;
	int      m_lastContinuityCounter;
	int      m_sectionTableID;
	int      m_pid;
};

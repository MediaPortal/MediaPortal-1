#pragma once
#pragma warning(disable: 4511 4512 4995)
#include "section.h"

class TableGrabber
{
private:
	struct DVBSectionHeader
	{
		 int TableID;
		 int SectionSyntaxIndicator;
		 int SectionLength;
		 int TableIDExtension;
		 int VersionNumber;
		 int CurrentNextIndicator;
		 int SectionNumber;
		 int LastSectionNumber;
		 int HeaderExtB8B9;
		 int HeaderExtB10B11;
		 int HeaderExtB12;
		 int HeaderExtB13;
	};
public:
	TableGrabber(void);
	~TableGrabber(void);
	void	OnPacket(byte* pbData,long lDataLen);
	bool	IsSectionGrabbed();
	void	Reset();
	void	SetTableId(int pid,int tableId);
	int		GetTableLen();
	byte*	GetTable();
private:
	void		GetSectionHeader(byte* data,int offset, DVBSectionHeader& header);
	void		ParseSection();
	Sections	m_sectionUtils;
	int			m_bufferPosition;
	byte		m_tableBuffer[70000];
	bool		m_bSectionGrabbed;
	int			m_lastContinuityCounter;
	int			m_sectionTableID;
	int			m_pid;
};

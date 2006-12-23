#pragma once
#pragma warning(disable: 4511 4512 4995)
#include "section.h"
#include <map>
#include <vector>
using namespace std;

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
	struct TableSection
	{
		int  iSize;
		byte byData[5000];
	};

public:
	TableGrabber(void);
	~TableGrabber(void);
	void	OnPacket(byte* pbData,long lDataLen);
	bool	IsSectionGrabbed();
	void	Reset();
	void	SetTableId(int pid,int tableId);
	int		GetTableLen(int section);
	byte*	GetTable(int section);
	int     Count();

private:
    
	void		GetSectionHeader(byte* data,int offset, DVBSectionHeader& header);
	void		ParseSection(byte* pData, long lDataLen);
	bool		m_bSectionGrabbed;
	int			m_sectionTableID;
	int			m_pid;
	time_t      timeoutTimer;
	map<ULONG, int> m_mapSections;
	typedef map<ULONG, int>::iterator imapSections;
	vector<TableSection> m_vecSections;
	typedef vector<TableSection> ::iterator ivecSections;
};

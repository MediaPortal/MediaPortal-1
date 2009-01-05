#pragma once

#include "PidParser.h"
#include "FileReader.h"
#include "MultiFileReader.h"
#include "SampleBuffer.h"
class CTsFileDuration
{
public:
  CTsFileDuration(void);
  virtual ~CTsFileDuration(void);
  HRESULT UpdateDuration();
	int SetFileName(char* pszFileName);
	int OpenFile();
	int CloseFile();
  float Duration();
  __int64 GetFileSize ();
private:
  __int64 ConvertPCRtoRT(__int64 pcrtime);
  BOOL IsTimeShifting(FileReader *pFileReader, BOOL *timeMode);
  HRESULT GetPositions( LONGLONG * pCurrent, LONGLONG * pStop );

  PidParser* m_pPidParser;
  char       m_pFileName[MAX_PATH];
  FileReader* m_pFileReader;
  CSampleBuffer m_sampleBuffer;
	long m_BitRateCycle;
	__int64 m_DataRateSave;
	long m_DataRate;
  CRefTime m_rtDuration;
  CRefTime m_rtStop;
  CRefTime m_rtStart;
  bool m_bSeeking;
	REFERENCE_TIME m_rtLastCurrentTime;
	REFERENCE_TIME m_rtTimeShiftPosition;
	__int64 m_IntBaseTimePCR;
	__int64 m_IntStartTimePCR;
	__int64 m_IntCurrentTimePCR;
	__int64 m_IntEndTimePCR;
	__int64 m_LastMultiFileStart;
	__int64 m_LastMultiFileEnd;
	long m_lTSPacketDeliverySize;
	REFERENCE_TIME m_rtLastSeekStart;
	BOOL m_bGetAvailableMode;
	__int64 m_LastFileSize;
	__int64 m_LastStartSize;
};

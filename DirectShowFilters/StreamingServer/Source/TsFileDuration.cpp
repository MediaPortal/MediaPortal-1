#include <streams.h>
#include "TsFileDuration.h"

CTsFileDuration::CTsFileDuration(void)
{
}

CTsFileDuration::~CTsFileDuration(void)
{
}

int CTsFileDuration::SetFileName(char* pszFileName)
{
  strcpy(m_pFileName, pszFileName);
  return S_OK;
}
int CTsFileDuration::OpenFile()
{
  m_bSeeking=false;
  m_BitRateCycle=0;
	m_DataRate = 10000000;
  m_rtLastCurrentTime = (REFERENCE_TIME)((REFERENCE_TIME)timeGetTime() * (REFERENCE_TIME)10000);
	m_IntBaseTimePCR = 0;
	m_IntStartTimePCR = 0;
	m_IntCurrentTimePCR = 0;
	m_IntEndTimePCR = 0;
	m_LastMultiFileStart = 0;
	m_LastMultiFileEnd = 0;
	m_rtLastSeekStart = 0;
	m_rtTimeShiftPosition = 0;
  m_lTSPacketDeliverySize = 65536/4;//188000;
  m_bGetAvailableMode = FALSE;
  m_LastFileSize = 0;
  m_LastStartSize = 0;

  if (strstr(m_pFileName,".tsbuffer")!=NULL)
  {
    m_pFileReader = new MultiFileReader();
  }
  else
  {
    m_pFileReader = new FileReader();
  }
  m_pFileReader->SetFileName(m_pFileName);
  m_pFileReader->OpenFile();
  m_pPidParser = new PidParser(&m_sampleBuffer,m_pFileReader);
  m_pPidParser->set_ProgPinMode(0);
  m_pPidParser->ParseFromFile(0);

    
  m_rtDuration = m_pPidParser->pids.dur;
  m_rtStop = m_rtDuration;
  m_DataRate = m_pPidParser->pids.bitrate;
  m_IntBaseTimePCR = m_pPidParser->pids.start;
  m_IntStartTimePCR = m_pPidParser->pids.start;
  m_IntCurrentTimePCR = m_pPidParser->pids.start;
  m_IntEndTimePCR = m_pPidParser->pids.end;
  return S_OK;

}
int CTsFileDuration::CloseFile()
{
  if (m_pFileReader!=NULL)
  {
    m_pFileReader->CloseFile();
    delete m_pFileReader;
    m_pFileReader=NULL;
  }
  
  if (m_pPidParser!=NULL)
  {
    delete m_pPidParser;
    m_pPidParser=NULL;
  }
  return S_OK;
}

float CTsFileDuration::Duration()
{
  float msec=m_rtDuration.Millisecs();
  msec/=1000.0f;
  return msec;
}
HRESULT CTsFileDuration::UpdateDuration()
{
	HRESULT hr = E_FAIL;

//***********************************************************************************************
//Old Capture format Additions

	if(!m_pPidParser->pids.pcr)
	{
		hr = S_FALSE;

		if (m_bSeeking)
			return hr;

		REFERENCE_TIME rtCurrentTime = (REFERENCE_TIME)((REFERENCE_TIME)timeGetTime() * (REFERENCE_TIME)10000);
		REFERENCE_TIME rtStop = 0;

		if (m_BitRateCycle < 50)
			m_DataRateSave = m_DataRate;

		//Calculate our time increase
		__int64 fileStart;
		__int64	fileSize = 0;
		m_pFileReader->GetFileSize(&fileStart, &fileSize);

		__int64 calcDuration = 0;
		if ((__int64)((__int64)m_DataRateSave / (__int64)8000) > 0)
		{
			calcDuration = (__int64)(fileSize / (__int64)((__int64)m_DataRateSave / (__int64)8000));
			calcDuration = (__int64)(calcDuration * (__int64)10000);
		}

		if ((__int64)m_pPidParser->pids.dur)
		{
			if (!m_bSeeking)
			{
				m_pPidParser->pids.dur = (REFERENCE_TIME)calcDuration;
				for (int i = 0; i < m_pPidParser->pidArray.Count(); i++)
				{
					m_pPidParser->pidArray[i].dur = m_pPidParser->pids.dur;
				}
				m_rtDuration = m_pPidParser->pids.dur;
				m_rtStop = m_pPidParser->pids.dur;
			}

			if ((REFERENCE_TIME)(m_rtLastCurrentTime + (REFERENCE_TIME)RT_SECOND) < rtCurrentTime) {
				//Get CSourceSeeking current time.
				GetPositions(&rtCurrentTime, &rtStop);
				//Test if we had been seeking recently and wait 2sec if so.
				if ((REFERENCE_TIME)(m_rtLastSeekStart + (REFERENCE_TIME)RT_2_SECOND) < rtCurrentTime) {

					//Send event to update filtergraph clock.
					if (!m_bSeeking)
					{
						m_rtLastCurrentTime = (REFERENCE_TIME)((REFERENCE_TIME)timeGetTime() * (REFERENCE_TIME)10000);
						//@NotifyEvent(EC_LENGTH_CHANGED, NULL, NULL);
						hr = S_OK;
					}
				}
			}

		}
//PrintTime(TEXT("UpdateDuration1"), (__int64) m_rtDuration, 10000);
		return hr;
	}

//***********************************************************************************************

	hr = S_FALSE;

	if (m_bSeeking)
		return hr;

	WORD readonly = 0;
	m_pFileReader->get_ReadOnly(&readonly);

	REFERENCE_TIME rtCurrentTime = (REFERENCE_TIME)((REFERENCE_TIME)timeGetTime() * (REFERENCE_TIME)10000);
	REFERENCE_TIME rtStop = 0;

	//check for duration every second of size change
	if (readonly)
	{
		//Get the FileReader Type
		WORD bMultiMode;
		m_pFileReader->get_ReaderMode(&bMultiMode);
		if(bMultiMode
			&& (REFERENCE_TIME)(m_rtLastCurrentTime + (REFERENCE_TIME)RT_SECOND) < rtCurrentTime)	//Do MultiFile timeshifting mode
		{
			//Get CSourceSeeking current time.
			GetPositions(&rtCurrentTime, &rtStop);
			//Test if we had been seeking recently and wait 2sec if so.
//			if ((REFERENCE_TIME)(m_rtLastSeekStart + (REFERENCE_TIME)RT_2_SECOND) > rtCurrentTime
//				&& m_rtLastSeekStart < (REFERENCE_TIME)RT_2_SECOND)
//			{
//				if(m_rtLastSeekStart)// || rtCurrentTime)
//				{
////////				if (m_IntEndTimePCR != -1) //cold start
//PrintTime(TEXT("UpdateDuration2"), (__int64) m_rtDuration, 10000);
//					return hr;
//				}
//			}

			//Check if Cold Start
			if(!m_IntBaseTimePCR && !m_IntStartTimePCR && !m_IntEndTimePCR)
			{
				m_LastMultiFileStart = -1;
				m_LastMultiFileEnd = -1;
			}

			BOOL bLengthChanged = FALSE;
			BOOL bStartChanged = FALSE;
			ULONG ulBytesRead = 0;
			__int64 pcrPos;
			ULONG pos = 0;

			// We'll use fileEnd instead of fileLength since fileLength /could/ be the same
			// even though fileStart and fileEnd have moved.
			__int64 fileStart, fileEnd, filelength;
			m_pFileReader->GetFileSize(&fileStart, &fileEnd);
			filelength = fileEnd;
			fileEnd += fileStart;
//			LONG lDataLength = m_lTSPacketDeliverySize;
			LONG lDataLength = (LONG)min(filelength/4, 2000000);
			lDataLength = max(m_lTSPacketDeliverySize, lDataLength);
			if (fileStart != m_LastMultiFileStart)
			{
				ulBytesRead = 0;
				pcrPos = -1;

				//Set Pointer to start of file to get end pcr
				m_pFileReader->setFilePointer(m_pPidParser->get_StartOffset(), FILE_BEGIN);
				PBYTE pData = new BYTE[lDataLength];
//				m_pFileReader->Read(pData, lDataLength, &ulBytesRead);
				if FAILED(hr = m_pFileReader->Read(pData, lDataLength, &ulBytesRead))
				{
					//Debug(TEXT("Failed to read from start of file"));
				}

				if (ulBytesRead < (ULONG)lDataLength)
				{
					//Debug(TEXT("Didn't read as much as it should have"));
				}

				hr = S_OK;
				pos = 0;
				hr = m_pPidParser->FindNextPCR(pData, ulBytesRead, &m_pPidParser->pids, &pcrPos, &pos, 1); //Get the PCR
				delete[] pData;
				//park the Pointer to end of file 
//				m_pFileReader->setFilePointer( (__int64)max(0, (__int64)(m_pFileReader->getFilePointer() -(__int64)100000)), FILE_BEGIN);

				__int64	pcrDeltaTime = (__int64)(pcrPos - m_IntStartTimePCR);
				//Test if we have a pcr or if the pcr is less than rollover time
				if (FAILED(hr) || pcrDeltaTime < 0)
				{
					//Debug(TEXT("Negative PCR Delta. This should only happen if there's a pcr rollover.\n"));
					//PrintTime(TEXT("Prev Start PCR"), m_IntStartTimePCR, 90);
					//PrintTime(TEXT("Start PCR"), pcrPos, 90);
					if(pcrPos)
					{
						m_IntStartTimePCR = pcrPos;
						m_IntBaseTimePCR = m_IntStartTimePCR;
					}

//PrintTime(TEXT("UpdateDuration3"), (__int64) m_rtDuration, 10000);
					return hr;
				}
				else
				{
					//Cold Start
					if (m_LastMultiFileStart == -1)
					{
						m_IntBaseTimePCR = pcrPos;
					}

					m_IntStartTimePCR = pcrPos;

					//update the times in the array
					for (int i = 0; i < m_pPidParser->pidArray.Count(); i++)
						m_pPidParser->pidArray[i].start += pcrDeltaTime;

					m_pPidParser->pids.start += pcrDeltaTime;

					m_LastMultiFileStart = fileStart;
					bStartChanged = TRUE;
					m_rtLastSeekStart = (__int64)max(0, (__int64)ConvertPCRtoRT(m_IntCurrentTimePCR - m_IntStartTimePCR));
					m_rtStart = m_rtLastSeekStart;
					//@ResetStreamTime();
				}
			};

			if (fileEnd != m_LastMultiFileEnd)
			{
				ulBytesRead = 0;
				pcrPos = -1;

				//Set Pointer to end of file to get end pcr
				m_pFileReader->setFilePointer((__int64)-lDataLength, FILE_END);
				PBYTE pData = new BYTE[lDataLength];
				if FAILED(hr = m_pFileReader->Read(pData, lDataLength, &ulBytesRead))
				{
					//Debug(TEXT("Failed to read from end of file"));
				}

				if (ulBytesRead < (ULONG)lDataLength)
				{
					//Debug(TEXT("Didn't read as much as it should have"));
				}

				pos = ulBytesRead - m_pPidParser->get_PacketSize();

				hr = S_OK;
				hr = m_pPidParser->FindNextPCR(pData, ulBytesRead, &m_pPidParser->pids, &pcrPos, &pos, -1); //Get the PCR
				delete[] pData;
				//park the Pointer to end of file 
//				m_pFileReader->setFilePointer( (__int64)max(0, (__int64)(m_pFileReader->getFilePointer() -(__int64)100000)), FILE_BEGIN);

				__int64	pcrDeltaTime = (__int64)(pcrPos - m_IntEndTimePCR);
				//Test if we have a pcr or if the pcr is less than rollover time
				if (FAILED(hr) || pcrDeltaTime < 0)
				{
					//Debug(TEXT("Negative PCR Delta. This should only happen if there's a pcr rollover.\n"));
					//PrintTime(TEXT("Prev End PCR"), m_IntEndTimePCR, 90);
					//PrintTime(TEXT("End PCR"), pcrPos, 90);
					if(pcrPos)
						m_IntEndTimePCR = pcrPos;
//PrintTime(TEXT("UpdateDuration5"), (__int64) m_rtDuration, 10000);
					return hr;
				}
				else
				{
					//Cold Start
					if (m_LastMultiFileEnd == -1 && pcrPos)
					{
						m_IntEndTimePCR = pcrPos;
						m_pPidParser->pids.dur = (__int64)ConvertPCRtoRT(m_IntEndTimePCR - m_IntStartTimePCR); 
						m_LastMultiFileEnd = fileEnd;
//PrintTime(TEXT("UpdateDuration4"), (__int64) m_pPidParser->pids.dur, 10000);
						return hr;
					}
					else
						m_IntEndTimePCR = pcrPos;

					bLengthChanged = TRUE;
				}

				//update the times in the array
				for (int i = 0; i < m_pPidParser->pidArray.Count(); i++)
				{
					m_pPidParser->pidArray[i].end += pcrDeltaTime;
				}

				m_pPidParser->pids.end += pcrDeltaTime;
				m_LastMultiFileEnd = fileEnd;
			}

			if ((bLengthChanged | bStartChanged) && !m_bSeeking)
			{	
				__int64	pcrDeltaTime;
				if(m_bGetAvailableMode)
					//Use this code to cause the end time to be relative to the base time.
					pcrDeltaTime = (__int64)(m_IntEndTimePCR - m_IntBaseTimePCR);
				else
					//Use this code to cause the end time to be relative to the start time.
					pcrDeltaTime = (__int64)(m_IntEndTimePCR - m_IntStartTimePCR);

				m_pPidParser->pids.dur = (__int64)ConvertPCRtoRT(pcrDeltaTime);

				// update pid arrays
				for (int i = 0; i < m_pPidParser->pidArray.Count(); i++)
					m_pPidParser->pidArray[i].dur = m_pPidParser->pids.dur;

				m_rtDuration = m_pPidParser->pids.dur;

				if(m_bGetAvailableMode)
					//Use this code to cause the end time to be relative to the base time.
					m_rtStop = m_pPidParser->pids.dur;
				else
				{
					//Use this code to cause the end time to be relative to the start time.
					__int64 offset = max(0, (__int64)((__int64)m_rtStart - (__int64)m_rtDuration));
					if (offset)
						m_rtStop = ConvertPCRtoRT(m_IntCurrentTimePCR - m_IntStartTimePCR);
					else
						m_rtStop = m_pPidParser->pids.dur;
				}

//PrintTime(TEXT("UpdateDuration: m_IntBaseTimePCR"), (__int64)m_IntBaseTimePCR, 90);
//PrintTime(TEXT("UpdateDuration: m_IntStartTimePCR"), (__int64)m_IntStartTimePCR, 90);
//PrintTime(TEXT("UpdateDuration: m_IntEndTimePCR"), (__int64)m_IntEndTimePCR, 90);
//PrintTime(TEXT("UpdateDuration: pids.start"), (__int64)m_pPidParser->pids.start, 90);
//PrintTime(TEXT("UpdateDuration: pids.end"), (__int64)m_pPidParser->pids.end, 90);
//PrintTime(TEXT("UpdateDuration: pids.dur"), (__int64)m_pPidParser->pids.dur, 10000);

				if (!m_bSeeking)
				{
					m_rtLastCurrentTime = (REFERENCE_TIME)((REFERENCE_TIME)timeGetTime() * (REFERENCE_TIME)10000);
					//@NotifyEvent(EC_LENGTH_CHANGED, NULL, NULL);
//PrintTime(TEXT("UpdateDuration6"), (__int64) m_rtDuration, 10000);
					return S_OK;
				}
			}
			return S_FALSE;
		}
		else // FileReader Mode
		{
			//check for duration every second of size change
			BOOL bTimeMode;
			BOOL bTimeShifting = IsTimeShifting(m_pFileReader, &bTimeMode);

			BOOL bLengthChanged = FALSE;

			//check for valid values
			if ((m_pPidParser->pids.pcr | m_pPidParser->get_ProgPinMode())
				&& m_IntEndTimePCR
				&& TRUE){
				//check for duration every second of size change
				if(((REFERENCE_TIME)(m_rtLastCurrentTime + (REFERENCE_TIME)RT_SECOND) < rtCurrentTime))
				{
					ULONG pos;
					__int64 pcrPos;
					ULONG ulBytesRead = 0;
					__int64 fileStart, fileEnd, filelength;
					m_pFileReader->GetFileSize(&fileStart, &fileEnd);
					filelength = fileEnd;
					fileEnd += fileStart;
		//			LONG lDataLength = m_lTSPacketDeliverySize;
					LONG lDataLength = (long)min(filelength/4, 2000000);
					lDataLength = max(m_lTSPacketDeliverySize, lDataLength);

					//Do a quick parse of duration if not seeking
					if (m_bSeeking)
						return hr;

					//Set Pointer to end of file to get end pcr
					m_pFileReader->setFilePointer((__int64)-lDataLength, FILE_END);
					PBYTE pData = new BYTE[lDataLength];
					m_pFileReader->Read(pData, lDataLength, &ulBytesRead);
					pos = ulBytesRead - m_pPidParser->get_PacketSize();

					hr = S_OK;
					hr = m_pPidParser->FindNextPCR(pData, ulBytesRead, &m_pPidParser->pids, &pcrPos, &pos, -1); //Get the PCR
					delete[] pData;
	
					__int64	pcrDeltaTime = (__int64)(pcrPos - m_IntEndTimePCR);
					//Test if we have a pcr or if the pcr is less than rollover time
					if (FAILED(hr) || pcrDeltaTime < (__int64)0) {
						if(pcrPos)
							m_IntEndTimePCR = pcrPos;
						return hr;
					}

					m_IntEndTimePCR = pcrPos;
					m_IntStartTimePCR += pcrDeltaTime;
					//if not time shifting update the duration
					if (!bTimeMode)
						m_pPidParser->pids.dur += (__int64)ConvertPCRtoRT(pcrDeltaTime);

					//update the times in the array
					for (int i = 0; i < m_pPidParser->pidArray.Count(); i++)
					{
						m_pPidParser->pidArray[i].end += pcrDeltaTime;
						// update the start time if shifting else update the duration
						if (bTimeMode)
							m_pPidParser->pidArray[i].start += pcrDeltaTime;
						else
							m_pPidParser->pidArray[i].dur = m_pPidParser->pids.dur;
					}
					m_pPidParser->pids.end += pcrDeltaTime;
					bLengthChanged = TRUE;
				}
			}
			else if ((REFERENCE_TIME)(m_rtLastCurrentTime + (REFERENCE_TIME)RT_SECOND) < rtCurrentTime
				&& TRUE)
			{
				//update all of the pid array times from a file parse.
				if (!m_bSeeking)
					m_pPidParser->RefreshDuration(TRUE, m_pFileReader);

				bLengthChanged = TRUE;
			}

			if (bLengthChanged)
			{
				if (!m_bSeeking)
				{
					//Set the filtergraph clock time to stationary position
					if (bTimeMode)
					{
//						__int64 current = (__int64)ConvertPCRtoRT(max(0, (__int64)(m_IntEndTimePCR - m_IntCurrentTimePCR)));
//						current = max(0,(__int64)(m_pPidParser->pids.dur - current - (__int64)RT_SECOND)); 
						CRefTime cTime;
						//@StreamTime(cTime);
						REFERENCE_TIME current = (REFERENCE_TIME)(m_rtLastSeekStart + REFERENCE_TIME(cTime));
						//Set the position of the filtergraph clock if first time shift pass
						if (!m_rtTimeShiftPosition)
							m_rtTimeShiftPosition = (REFERENCE_TIME)min(current, m_pPidParser->pids.dur - (__int64)RT_SECOND);
						//  set clock to stop or update time if not first pass
//						m_rtStop = max(0, (__int64)(m_pPidParser->pids.dur - (__int64)ConvertPCRtoRT((__int64)(m_IntEndTimePCR - m_IntCurrentTimePCR))));
						m_rtStop = max(m_rtTimeShiftPosition, m_rtLastSeekStart);
						if (!m_bSeeking)
						{
							m_rtLastCurrentTime = (REFERENCE_TIME)((REFERENCE_TIME)timeGetTime() * (REFERENCE_TIME)10000);
							//@NotifyEvent(EC_LENGTH_CHANGED, NULL, NULL);
							hr = S_OK;
						}
					}
					else
					{
						// if was time shifting but not anymore such as filewriter pause or stop
						if(m_rtTimeShiftPosition){
							//reset the stream clock and last seek save value
							m_rtStart = m_rtTimeShiftPosition;
							m_rtStop = m_rtTimeShiftPosition;
							m_rtLastSeekStart = m_rtTimeShiftPosition;
							//@ResetStreamTime();
							m_rtTimeShiftPosition = 0;
						}
						else
						{
							if(bTimeShifting)
							{
								m_rtTimeShiftPosition = 0;
//								CRefTime cTime;
//								StreamTime(cTime);
//								REFERENCE_TIME rtCurrent = (REFERENCE_TIME)(m_rtLastSeekStart + REFERENCE_TIME(cTime));
								__int64 current = (__int64)max(0, (__int64)ConvertPCRtoRT(max(0,(__int64)(m_IntEndTimePCR - m_IntCurrentTimePCR))));
								REFERENCE_TIME rtCurrent = (REFERENCE_TIME)(m_pPidParser->pids.dur - current); 
								m_rtDuration = m_pPidParser->pids.dur;
								m_rtStop = rtCurrent;
							}
							else
							{
								m_rtTimeShiftPosition = 0;
								m_rtDuration = m_pPidParser->pids.dur;
								m_rtStop = m_pPidParser->pids.dur;
							}

						}
												

					}

					//Send event to update filtergraph clock
					if (!m_bSeeking)
					{
						m_rtLastCurrentTime = (REFERENCE_TIME)((REFERENCE_TIME)timeGetTime() * (REFERENCE_TIME)10000);
						//@NotifyEvent(EC_LENGTH_CHANGED, NULL, NULL);
						return S_OK;;
					}

				}

				if ((REFERENCE_TIME)(m_rtLastCurrentTime + (REFERENCE_TIME)RT_SECOND) < rtCurrentTime) {
					//Get CSourceSeeking current time.
					GetPositions(&rtCurrentTime, &rtStop);
					//Test if we had been seeking recently and wait 2sec if so.
					if ((REFERENCE_TIME)(m_rtLastSeekStart + (REFERENCE_TIME)RT_2_SECOND) < rtCurrentTime) {

						//Send event to update filtergraph clock.
						if (!m_bSeeking)
						{
							m_rtLastCurrentTime = (REFERENCE_TIME)((REFERENCE_TIME)timeGetTime() * (REFERENCE_TIME)10000);
							//@NotifyEvent(EC_LENGTH_CHANGED, NULL, NULL);
							return S_OK;;
						}
					}
				}

			}
			else
				return S_FALSE;
		}
	}
	return S_OK;
}

__int64 CTsFileDuration::ConvertPCRtoRT(__int64 pcrtime)
{
	return (__int64)(pcrtime / 9) * 1000;
}

BOOL CTsFileDuration::IsTimeShifting(FileReader *pFileReader, BOOL *timeMode)
{
	WORD readonly = 0;
	m_pFileReader->get_ReadOnly(&readonly);
	__int64	fileStart, fileSize = 0;
	m_pFileReader->GetFileSize(&fileStart, &fileSize);
	*timeMode = (m_LastFileSize == fileSize) & (fileStart ? TRUE : FALSE) & (readonly ? TRUE : FALSE) & (m_LastStartSize != fileStart);
	m_LastFileSize = fileSize;
	m_LastStartSize = fileStart;
	return (fileStart ? TRUE : FALSE) & (readonly ? TRUE : FALSE);
}
__int64 CTsFileDuration::GetFileSize ()
{  
  __int64 fileStart;
  __int64 fileSize = 0;

  m_pFileReader->GetFileSize(&fileStart, &fileSize);
  return fileSize;
}


HRESULT CTsFileDuration::GetPositions( LONGLONG * pCurrent, LONGLONG * pStop )
{
    if(pCurrent) {
        *pCurrent = m_rtStart;
    }
    if(pStop) {
        *pStop = m_rtStop;
    }

    return S_OK;;
}

#include <streams.h>
#include "RTSPFileReader.h"



CRTSPFileReader::CRTSPFileReader(SharedMemory* pSharedMemory)
:FileReader(pSharedMemory)
,m_client(m_memoryBuffer)
{

}

CRTSPFileReader::~CRTSPFileReader()
{
}

CRTSPFileReader* CRTSPFileReader::CreateFileReader()
{
	return new CRTSPFileReader(NULL);
}

HRESULT CRTSPFileReader::OpenFile()
{  
	// Open the URL, to get a SDP description:
	char url[MAX_PATH];
	WideCharToMultiByte(CP_ACP,0,m_pFileName,-1,url,MAX_PATH,0,0);
  if (m_client.Initialize())
  {
    if (m_client.OpenStream(url))
    {
      m_client.Play();
      return S_OK;
    }
  }
	return E_FAIL;
}

HRESULT CRTSPFileReader::CloseFile()
{
	return NOERROR;
}

HRESULT CRTSPFileReader::Read(PBYTE pbData, ULONG lDataLength, ULONG *dwReadBytes)
{
	*dwReadBytes=  m_memoryBuffer.ReadFromBuffer(pbData,lDataLength,0);
	return NOERROR;
}

HRESULT CRTSPFileReader::Read(PBYTE pbData, ULONG lDataLength, ULONG *dwReadBytes, __int64 llDistanceToMove, DWORD dwMoveMethod)
{
	*dwReadBytes=  m_memoryBuffer.ReadFromBuffer(pbData,lDataLength,0);
	return NOERROR;
}


HRESULT CRTSPFileReader::GetFileSize(__int64 *pStartPosition, __int64 *pLength)
{
	*pStartPosition=0LL;
	*pLength=0LL;
	return S_OK;
}

HRESULT CRTSPFileReader::GetInfoFileSize(__int64 *lpllsize)
{
	*lpllsize=0LL;
	return S_OK;
}

HRESULT CRTSPFileReader::GetStartPosition(__int64 *lpllpos)
{
	*lpllpos=0LL;
	return S_OK;
}

BOOL CRTSPFileReader::IsFileInvalid()
{
	return true;
}

DWORD CRTSPFileReader::SetFilePointer(__int64 llDistanceToMove, DWORD dwMoveMethod)
{
	return 0;
}

__int64 CRTSPFileReader::GetFilePointer()
{
	return 0;
}

DWORD CRTSPFileReader::setFilePointer(__int64 llDistanceToMove, DWORD dwMoveMethod)
{
	return 0;
}

__int64 CRTSPFileReader::getFilePointer()
{
	return 0;
}


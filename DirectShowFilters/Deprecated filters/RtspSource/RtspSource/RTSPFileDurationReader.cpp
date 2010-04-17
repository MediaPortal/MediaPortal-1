#include <streams.h>
#include "RTSPFileDurationReader.h"



CRTSPFileDurationReader::CRTSPFileDurationReader(SharedMemory* pSharedMemory)
:FileReader(pSharedMemory)
{

}

CRTSPFileDurationReader::~CRTSPFileDurationReader()
{
}

CRTSPFileDurationReader* CRTSPFileDurationReader::CreateFileReader()
{
	return new CRTSPFileDurationReader(NULL);
}

HRESULT CRTSPFileDurationReader::OpenFile()
{  
 return S_OK;
}

HRESULT CRTSPFileDurationReader::CloseFile()
{
	return NOERROR;
}

HRESULT CRTSPFileDurationReader::Read(PBYTE pbData, ULONG lDataLength, ULONG *dwReadBytes)
{
	*dwReadBytes= 0;
	return NOERROR;
}

HRESULT CRTSPFileDurationReader::Read(PBYTE pbData, ULONG lDataLength, ULONG *dwReadBytes, __int64 llDistanceToMove, DWORD dwMoveMethod)
{
	*dwReadBytes=  0;
	return NOERROR;
}


HRESULT CRTSPFileDurationReader::GetFileSize(__int64 *pStartPosition, __int64 *pLength)
{
	*pStartPosition=0LL;
	*pLength=(1024LL*1024LL*500LL);
	return S_OK;
}

HRESULT CRTSPFileDurationReader::GetInfoFileSize(__int64 *lpllsize)
{
	*lpllsize=0LL;
	return S_OK;
}

HRESULT CRTSPFileDurationReader::GetStartPosition(__int64 *lpllpos)
{
	*lpllpos=0LL;
	return S_OK;
}

BOOL CRTSPFileDurationReader::IsFileInvalid()
{
	return true;
}

DWORD CRTSPFileDurationReader::SetFilePointer(__int64 llDistanceToMove, DWORD dwMoveMethod)
{
	return 0;
}

__int64 CRTSPFileDurationReader::GetFilePointer()
{
	return 0;
}

DWORD CRTSPFileDurationReader::setFilePointer(__int64 llDistanceToMove, DWORD dwMoveMethod)
{
	return 0;
}

__int64 CRTSPFileDurationReader::getFilePointer()
{
	return 0;
}


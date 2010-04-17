#include <afx.h>
#include <afxwin.h>

#include <winsock2.h>
#include <ws2tcpip.h>
#include <streams.h>
#include "MemoryReader.h"

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

CMemoryReader::CMemoryReader(CMemoryBuffer& buffer)
:m_buffer(buffer)
{
}

CMemoryReader::~CMemoryReader(void)
{
}

HRESULT CMemoryReader::Read(BYTE* pbData, ULONG lDataLength, ULONG *dwReadBytes)
{
  *dwReadBytes =m_buffer.ReadFromBuffer(pbData,lDataLength);
  if ((*dwReadBytes) <=0) return S_FALSE;
  return S_OK;
}
HRESULT CMemoryReader::Read(BYTE* pbData, ULONG lDataLength, ULONG *dwReadBytes, __int64 llDistanceToMove, DWORD dwMoveMethod)
{
  *dwReadBytes =m_buffer.ReadFromBuffer(pbData,lDataLength);
  if ((*dwReadBytes) <=0) return S_FALSE;
  return S_OK;
}
DWORD CMemoryReader::setFilePointer(__int64 llDistanceToMove, DWORD dwMoveMethod)
{
  return 0;
}
bool CMemoryReader::HasMoreData(int bytes)
{
  return (m_buffer.Size()>=bytes);
}
int CMemoryReader::HasData()
{
  return (m_buffer.Size());
}
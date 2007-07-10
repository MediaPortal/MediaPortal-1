
#include <streams.h>
#include "MemoryReader.h"

CMemoryReader::CMemoryReader(CMemoryBuffer& buffer)
:m_buffer(buffer)
{
}

CMemoryReader::~CMemoryReader(void)
{
}

int CMemoryReader::Read(BYTE* pbData, ULONG lDataLength, ULONG *dwReadBytes)
{
  *dwReadBytes =m_buffer.ReadFromBuffer(pbData,lDataLength);
  return *dwReadBytes;
}
int CMemoryReader::Read(BYTE* pbData, ULONG lDataLength, ULONG *dwReadBytes, __int64 llDistanceToMove, DWORD dwMoveMethod)
{
  *dwReadBytes =m_buffer.ReadFromBuffer(pbData,lDataLength);
  return *dwReadBytes;
}
DWORD CMemoryReader::setFilePointer(__int64 llDistanceToMove, DWORD dwMoveMethod)
{
  return 0;
}
bool CMemoryReader::HasMoreData(int bytes)
{
  return (m_buffer.Size()>=bytes);
}
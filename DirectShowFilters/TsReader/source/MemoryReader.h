#pragma once
#include "filereader.h"
#include "memorybuffer.h"
class CMemoryReader :
  public FileReader
{
public:
  CMemoryReader(CMemoryBuffer& buffer);
  virtual ~CMemoryReader(void);
  HRESULT Read(BYTE* pbData, ULONG lDataLength, ULONG *dwReadBytes);
  HRESULT Read(BYTE* pbData, ULONG lDataLength, ULONG *dwReadBytes, __int64 llDistanceToMove, DWORD dwMoveMethod);
  DWORD setFilePointer(__int64 llDistanceToMove, DWORD dwMoveMethod);
  bool IsBuffer(){return true;};
  bool HasMoreData(int bytes);
  int HasData();
private:
  CMemoryBuffer& m_buffer;
};

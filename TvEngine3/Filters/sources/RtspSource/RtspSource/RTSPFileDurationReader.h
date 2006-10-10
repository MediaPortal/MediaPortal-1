#pragma once
#include "SharedMemory.h"
#include "FileReader.h"

class CRTSPFileDurationReader: public FileReader
{
public:
	CRTSPFileDurationReader(SharedMemory* pSharedMemory);
	virtual ~CRTSPFileDurationReader();
	virtual CRTSPFileDurationReader* CreateFileReader();

	// Open and write to the file
	virtual HRESULT OpenFile();
	virtual HRESULT CloseFile();
	virtual HRESULT Read(PBYTE pbData, ULONG lDataLength, ULONG *dwReadBytes);
	virtual HRESULT Read(PBYTE pbData, ULONG lDataLength, ULONG *dwReadBytes, __int64 llDistanceToMove, DWORD dwMoveMethod);
	virtual HRESULT GetFileSize(__int64 *pStartPosition, __int64 *pLength);
	HRESULT					GetInfoFileSize(__int64 *lpllsize);
	HRESULT					GetStartPosition(__int64 *lpllpos);
	virtual BOOL		IsFileInvalid();
	virtual DWORD		SetFilePointer(__int64 llDistanceToMove, DWORD dwMoveMethod);
	virtual __int64 GetFilePointer();
	virtual DWORD		setFilePointer(__int64 llDistanceToMove, DWORD dwMoveMethod);
	virtual __int64 getFilePointer();
};
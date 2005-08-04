/*
	MediaPortal TS-SourceFilter by Agree
	Parts taken from TSSourceFilter.ax by nate, bear and bisswanger
	
*/

#ifndef __FileReader
#define __FileReader

#include "StreamPids.h"

class FileReader
{
public:

	FileReader();
	virtual ~FileReader();
	HRESULT GetFileName(LPOLESTR *lpszFileName);
	HRESULT SetFileName(LPCOLESTR pszFileName);
	HRESULT OpenFile();
	HRESULT CloseFile();
	BOOL IsFileInvalid();
	HRESULT GetFileSize(__int64 *lpllsize);
	DWORD SetFilePointer(__int64 llDistanceToMove, DWORD dwMoveMethod);
	__int64 GetFilePointer();
	__int64 get_FileSize(void);
	HRESULT Read(PBYTE pbData, ULONG lDataLength, ULONG *dwReadBytes);
	HRESULT Read(PBYTE pbData, ULONG lDataLength, ULONG *dwReadBytes, __int64 llDistanceToMove, DWORD dwMoveMethod);

protected:
	HANDLE   m_hFile;               // Handle to file for streaming
	LPOLESTR m_pFileName;           // The filename where we stream
	BOOL     m_bReadOnly;
	BOOL     m_bDelay;
	__int64 m_fileSize;
	HANDLE   m_hInfoFile;               // Handle to Infofile for filesize
};

#endif

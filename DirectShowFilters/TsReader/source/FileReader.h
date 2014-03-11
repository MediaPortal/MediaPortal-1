/**
*  FileReader.h
*  Copyright (C) 2005      nate
*  Copyright (C) 2006      bear
*
*  This file is part of TSFileSource, a directshow push source filter that
*  provides an MPEG transport stream output.
*
*  TSFileSource is free software; you can redistribute it and/or modify
*  it under the terms of the GNU General Public License as published by
*  the Free Software Foundation; either version 2 of the License, or
*  (at your option) any later version.
*
*  TSFileSource is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*  GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License
*  along with TSFileSource; if not, write to the Free Software
*  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*
*  nate can be reached on the forums at
*    http://forums.dvbowners.com/
*/

#ifndef FILEREADER
#define FILEREADER

#include <Streams.h>

class FileReader
{
public:

	FileReader();
	virtual ~FileReader();

	// Open and write to the file
	virtual HRESULT GetFileName(LPOLESTR *lpszFileName);
	virtual HRESULT SetFileName(LPCOLESTR pszFileName);
	virtual HRESULT OpenFile();
	virtual HRESULT CloseFile();
	virtual HRESULT Read(PBYTE pbData, ULONG lDataLength, ULONG *dwReadBytes);
	virtual HRESULT Read(PBYTE pbData, ULONG lDataLength, ULONG *dwReadBytes, __int64 llDistanceToMove, DWORD dwMoveMethod);
	virtual HRESULT GetFileSize(__int64 *pStartPosition, __int64 *pLength);
	virtual BOOL IsFileInvalid();
	virtual DWORD SetFilePointer(__int64 llDistanceToMove, DWORD dwMoveMethod);
	virtual __int64 GetFilePointer();

	virtual void SetDummyWrites(BOOL useDummyWrites);

	virtual __int64 GetFileSize();
	
	//The three methods below are for MemoryBuffer() reader compatibility
	virtual bool IsBuffer(){return false;};
	virtual bool HasMoreData(){return false;};
	virtual int HasData(){return 0; } ;

	//The two methods below are for MultiFileReader() compatibility
	virtual BOOL GetFileNext(){return false;};
	virtual void SetFileNext(BOOL useFileNext);

protected:
  
  CString randomStrGen(int length); 
	
	HANDLE   m_hFile; 				// Handle to file for streaming
	LPOLESTR m_pFileName;           // The filename where we stream

	BOOL     m_bUseDummyWrites;
  CCritSec     m_accessLock;
};

#endif

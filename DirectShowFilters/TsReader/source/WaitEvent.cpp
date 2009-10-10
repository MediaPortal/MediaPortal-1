#include <winsock2.h>
#include <ws2tcpip.h>
#include <streams.h>
#include "WaitEvent.h"

CWaitEvent::CWaitEvent(LPSECURITY_ATTRIBUTES lpEventAttributes,BOOL bManualReset,BOOL bInitialState,LPCTSTR lpName)
{
  m_hObject = ::CreateEvent(lpEventAttributes, bManualReset, bInitialState,lpName);
}

CWaitEvent::~CWaitEvent(void)
{
  ::CloseHandle(m_hObject);
}

void CWaitEvent::SetEvent()
{
  ::SetEvent(m_hObject);
}
void CWaitEvent::ResetEvent()
{
   ::ResetEvent(m_hObject);
}
BOOL CWaitEvent::Wait()
{
  DWORD dwResult=::WaitForSingleObject(m_hObject,500);
  if (dwResult==WAIT_OBJECT_0) return true;

  return false;
}
#pragma once

class CWaitEvent
{
public:
  CWaitEvent(LPSECURITY_ATTRIBUTES lpEventAttributes,BOOL bManualReset,BOOL bInitialState,LPCTSTR lpName);
  virtual ~CWaitEvent(void);
  BOOL Wait();
  void SetEvent();
  void ResetEvent();

protected:
  HANDLE m_hObject;
};

#pragma once

#include "DVBTSFilter.h"
#include "Lock.h"
#include <vector>
#include <map>
#include <queue>

class CTTPremiumOutputPin;

class TSDataFilter : public CDVBTSFilter
{
public:
	TSDataFilter(int bufferSize);
	virtual ~TSDataFilter();

	void Start();
	void Stop();
	bool IsRunning() { return m_isRunning; }
	int RefCount() { return m_watchers.size(); }
	
	int Copy(BYTE *buff, int maxLen);

	void AddRef(CTTPremiumOutputPin *pPin);
	void RemoveRef(CTTPremiumOutputPin *pPin);

protected:
	friend class CTTPremiumOutputPin;

	virtual void OnDataArrival(BYTE* Buff, int len);
  HRESULT SendData(CTTPremiumOutputPin *outputPin, BYTE* buff, int len);

	bool					m_isRunning;
	int						m_bufferSize;

	Lock					    *m_lock;
  std::vector<CTTPremiumOutputPin *> m_watchers;
};
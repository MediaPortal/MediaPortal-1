#include <shlobj.h>
#define Number_of_Pids 50

#include <set>
#include <vector>
#include <cstdint>
#include <string>

class PidFilter
{
public:
	PidFilter();
	~PidFilter();
	void Add(uint16_t pid);
	void Del(uint16_t pid);
	void Reset();
	void SyncPids(std::string newPids);
	void SyncPids(std::vector<uint16_t> newPids);
	bool PidRequested(uint16_t pid);

	// When given a pointer to a package start, e.g. the pointer points to the sync byte, it returns the PID
	static uint16_t getPidFromPackage(unsigned char* packageStart);
private:
	std::set<uint16_t> pids;
	int pidCounter;
	int index;
	CRITICAL_SECTION csPidsInAccess;
};
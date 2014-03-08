#define Number_of_Pids 50

class PidFilter
{
public:
	PidFilter();
	~PidFilter();
	void Add(int pid);
	void Del(int pid);
	bool PidRequested(unsigned short pid);
private:
	int* _pids;
	int pidCounter;
	int index;
};
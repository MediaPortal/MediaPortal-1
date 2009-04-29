#pragma once
#include "Section.h"

class ISectionCallback
{
public:
	virtual void OnNewSection(int pid, int tableId, CSection& section)=0;
};

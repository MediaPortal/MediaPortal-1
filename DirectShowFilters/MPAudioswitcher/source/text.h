#pragma once


// extern tstring ExplodeMin(tstring str, std::list<tstring>& sl, TCHAR sep, int limit = 0);
// extern tstring Explode(tstring str, std::list<tstring>& sl, TCHAR sep, int limit = 0);
// extern tstring Implode(std::list<tstring>& sl, TCHAR sep);

template<class T, typename SEP>
T Explode(T str, std::list<T>& sl, SEP sep, int limit = 0)
{
	sl.RemoveAll();

	for(int i = 0, j = 0; ; i = j+1)
	{
		j = str.Find(sep, i);

		if(j < 0 || sl.GetCount() == limit-1)
		{
			sl.AddTail(str.Mid(i).Trim());
			break;
		}
		else
		{
			sl.AddTail(str.Mid(i, j-i).Trim());
		}		
	}

	return sl.GetHead();
}

template<class T, typename SEP>
T ExplodeMin(T str, std::list<T>& sl, SEP sep, int limit = 0)
{
	Explode(str, sl, sep, limit);
	POSITION pos = sl.GetHeadPosition();
	while(pos) 
	{
		POSITION tmp = pos;
		if(sl.GetNext(pos).IsEmpty())
			sl.RemoveAt(tmp);
	}
	if(sl.IsEmpty()) sl.AddTail(T()); // eh

	return sl.GetHead();
}

template<class T, typename SEP>
T Implode(std::list<T>& sl, SEP sep)
{
	T ret;
	POSITION pos = sl.GetHeadPosition();
	while(pos)
	{
		ret += sl.GetNext(pos);
		if(pos) ret += sep;
	}
	return(ret);
}

extern std::string ConvertMBCS(std::string str, DWORD SrcCharSet, DWORD DstCharSet);
extern std::string UrlEncode(std::string str, bool fRaw = false);
extern std::string UrlDecode(std::string str, bool fRaw = false);
extern DWORD CharSetToCodePage(DWORD dwCharSet);
extern std::list<tstring>& MakeLower(std::list<tstring>& sl);
extern std::list<tstring>& MakeUpper(std::list<tstring>& sl);
extern std::list<tstring>& RemoveStrings(std::list<tstring>& sl, int minlen, int maxlen);


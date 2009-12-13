/* 
 *      Copyright (C) 2005-2009 Team MediaPortal
 *      http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

template <class T> class CMyQueue : public CCritSec {
	T* m_elements;
	int m_size;
	int m_pos;
	int m_insertPos;
	int m_count;

	inline int NextIdx(int idx)
	{
		return (idx+1)%m_size;
	}
public:
	CMyQueue(int size)
	{
		m_count = 0;
		m_pos = 0;
		m_insertPos = 0;
		m_size = size;
		m_elements = new T[size];
	}

	~CMyQueue()
	{
		delete[] m_elements;
	}

	void Clear()
	{
		m_count = 0;
		m_insertPos = m_pos = 0;
	}

	bool IsFull() 
	{
		return m_count == m_size;
	}

	bool IsEmpty()
	{
		return m_count == 0;
	}

	bool Put(T elem)
	{
		CAutoLock lock(this);
		if ( m_count == m_size ) {
			Log("MyQueue: No more space");
			return false;
		}
		m_count++;
		m_elements[m_insertPos] = elem;
		m_insertPos = NextIdx(m_insertPos);
		return true;
	}

	T Get()
	{
		CAutoLock lock(this);
		if ( m_count == 0 ) return NULL;
		m_count--;
		T ret;
		ret = m_elements[m_pos];
		m_pos = NextIdx(m_pos);
		return ret;
	}

	T Peek()
	{
		CAutoLock lock(this);
		if ( m_count == 0 ) return NULL;
		T ret;
		ret = m_elements[m_pos];
		return ret;
	}

	int Count()
	{
		return m_count;
	}
};
